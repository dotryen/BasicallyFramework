/*
 *  Managed C# wrapper for an extended version of ENet
 *  Copyright (c) 2013 James Bellinger
 *  Copyright (c) 2016 Nate Shoffner
 *  Copyright (c) 2018 Stanislav Denisov
 *
 *  Permission is hereby granted, free of charge, to any person obtaining a copy
 *  of this software and associated documentation files (the "Software"), to deal
 *  in the Software without restriction, including without limitation the rights
 *  to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 *  copies of the Software, and to permit persons to whom the Software is
 *  furnished to do so, subject to the following conditions:
 *
 *  The above copyright notice and this permission notice shall be included in all
 *  copies or substantial portions of the Software.
 *
 *  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 *  IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 *  FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 *  AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 *  LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 *  OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 *  SOFTWARE.
 */

// CHANGES:
// Changed namespace and accessibility to hide from users. For simplicity sake.

using System;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;

namespace Basically.Networking.ENet {
	[Flags]
	internal enum PacketFlags {
		None = 0,
		Reliable = 1 << 0,
		Unsequenced = 1 << 1,
		NoAllocate = 1 << 2,
		UnreliableFragmented = 1 << 3,
		Instant = 1 << 4,
		Unthrottled = 1 << 5,
		Sent =  1 << 8
	}

	internal enum EventType {
		None = 0,
		Connect = 1,
		Disconnect = 2,
		Receive = 3,
		Timeout = 4
	}

	internal enum PeerState {
		Uninitialized = -1,
		Disconnected = 0,
		Connecting = 1,
		AcknowledgingConnect = 2,
		ConnectionPending = 3,
		ConnectionSucceeded = 4,
		Connected = 5,
		DisconnectLater = 6,
		Disconnecting = 7,
		AcknowledgingDisconnect = 8,
		Zombie = 9
	}

	[StructLayout(LayoutKind.Explicit, Size = 18)]
	internal struct ENetAddress {
		[FieldOffset(16)]
		internal ushort port;
	}

	[StructLayout(LayoutKind.Sequential)]
	internal struct ENetEvent {
		internal EventType type;
		internal IntPtr peer;
		internal byte channelID;
		internal uint data;
		internal IntPtr packet;
	}

	[StructLayout(LayoutKind.Sequential)]
	internal struct ENetCallbacks {
		internal AllocCallback malloc;
		internal FreeCallback free;
		internal NoMemoryCallback noMemory;
	}

	internal delegate IntPtr AllocCallback(IntPtr size);
	internal delegate void FreeCallback(IntPtr memory);
	internal delegate void NoMemoryCallback();
	internal delegate void PacketFreeCallback(Packet packet);
	internal delegate int InterceptCallback(ref Event @event, ref Address address, IntPtr receivedData, int receivedDataLength);
	internal delegate ulong ChecksumCallback(IntPtr buffers, int bufferCount);

	internal static class ArrayPool {
		[ThreadStatic]
		private static byte[] byteBuffer;
		[ThreadStatic]
		private static IntPtr[] pointerBuffer;

		internal static byte[] GetByteBuffer() {
			if (byteBuffer == null)
				byteBuffer = new byte[64];

			return byteBuffer;
		}

		internal static IntPtr[] GetPointerBuffer() {
			if (pointerBuffer == null)
				pointerBuffer = new IntPtr[Library.maxPeers];

			return pointerBuffer;
		}
	}

	internal struct Address {
		private ENetAddress nativeAddress;

		internal ENetAddress NativeData {
			get {
				return nativeAddress;
			}

			set {
				nativeAddress = value;
			}
		}

		internal Address(ENetAddress address) {
			nativeAddress = address;
		}

		internal ushort Port {
			get {
				return nativeAddress.port;
			}

			set {
				nativeAddress.port = value;
			}
		}

		internal string GetIP() {
			StringBuilder ip = new StringBuilder(1025);

			if (Native.enet_address_get_ip(ref nativeAddress, ip, (IntPtr)ip.Capacity) != 0)
				return String.Empty;

			return ip.ToString();
		}

		internal bool SetIP(string ip) {
			if (ip == null)
				throw new ArgumentNullException("ip");

			return Native.enet_address_set_ip(ref nativeAddress, ip) == 0;
		}

		internal string GetHost() {
			StringBuilder hostName = new StringBuilder(1025);

			if (Native.enet_address_get_hostname(ref nativeAddress, hostName, (IntPtr)hostName.Capacity) != 0)
				return String.Empty;

			return hostName.ToString();
		}

		internal bool SetHost(string hostName) {
			if (hostName == null)
				throw new ArgumentNullException("hostName");

			return Native.enet_address_set_hostname(ref nativeAddress, hostName) == 0;
		}
	}

	internal struct Event {
		private ENetEvent nativeEvent;

		internal ENetEvent NativeData {
			get {
				return nativeEvent;
			}

			set {
				nativeEvent = value;
			}
		}

		internal Event(ENetEvent @event) {
			nativeEvent = @event;
		}

		internal EventType Type {
			get {
				return nativeEvent.type;
			}
		}

		internal Peer Peer {
			get {
				return new Peer(nativeEvent.peer);
			}
		}

		internal byte ChannelID {
			get {
				return nativeEvent.channelID;
			}
		}

		internal uint Data {
			get {
				return nativeEvent.data;
			}
		}

		internal Packet Packet {
			get {
				return new Packet(nativeEvent.packet);
			}
		}
	}

	internal class Callbacks {
		private ENetCallbacks nativeCallbacks;

		internal ENetCallbacks NativeData {
			get {
				return nativeCallbacks;
			}

			set {
				nativeCallbacks = value;
			}
		}

		internal Callbacks(AllocCallback allocCallback, FreeCallback freeCallback, NoMemoryCallback noMemoryCallback) {
			nativeCallbacks.malloc = allocCallback;
			nativeCallbacks.free = freeCallback;
			nativeCallbacks.noMemory = noMemoryCallback;
		}
	}

	public struct Packet : IDisposable {
		private IntPtr nativePacket;

		internal IntPtr NativeData {
			get {
				return nativePacket;
			}

			set {
				nativePacket = value;
			}
		}

		internal Packet(IntPtr packet) {
			nativePacket = packet;
		}

		public void Dispose() {
			if (nativePacket != IntPtr.Zero) {
				Native.enet_packet_dispose(nativePacket);
				nativePacket = IntPtr.Zero;
			}
		}

		internal bool IsSet {
			get {
				return nativePacket != IntPtr.Zero;
			}
		}

		internal IntPtr Data {
			get {
				ThrowIfNotCreated();

				return Native.enet_packet_get_data(nativePacket);
			}
		}

		internal IntPtr UserData {
			get {
				ThrowIfNotCreated();

				return Native.enet_packet_get_user_data(nativePacket);
			}

			set {
				ThrowIfNotCreated();

				Native.enet_packet_set_user_data(nativePacket, value);
			}
		}

		internal int Length {
			get {
				ThrowIfNotCreated();

				return Native.enet_packet_get_length(nativePacket);
			}
		}

		internal bool HasReferences {
			get {
				ThrowIfNotCreated();

				return Native.enet_packet_check_references(nativePacket) != 0;
			}
		}

		internal void ThrowIfNotCreated() {
			if (nativePacket == IntPtr.Zero)
				throw new InvalidOperationException("Packet not created");
		}

		internal void SetFreeCallback(IntPtr callback) {
			ThrowIfNotCreated();

			Native.enet_packet_set_free_callback(nativePacket, callback);
		}

		internal void SetFreeCallback(PacketFreeCallback callback) {
			ThrowIfNotCreated();

			Native.enet_packet_set_free_callback(nativePacket, Marshal.GetFunctionPointerForDelegate(callback));
		}

		internal void Create(byte[] data) {
			if (data == null)
				throw new ArgumentNullException("data");

			Create(data, data.Length);
		}

		internal void Create(byte[] data, int length) {
			Create(data, length, PacketFlags.None);
		}

		internal void Create(byte[] data, PacketFlags flags) {
			Create(data, data.Length, flags);
		}

		internal void Create(byte[] data, int length, PacketFlags flags) {
			if (data == null)
				throw new ArgumentNullException("data");

			if (length < 0 || length > data.Length)
				throw new ArgumentOutOfRangeException("length");

			nativePacket = Native.enet_packet_create(data, (IntPtr)length, flags);
		}

		internal void Create(IntPtr data, int length, PacketFlags flags) {
			if (data == IntPtr.Zero)
				throw new ArgumentNullException("data");

			if (length < 0)
				throw new ArgumentOutOfRangeException("length");

			nativePacket = Native.enet_packet_create(data, (IntPtr)length, flags);
		}

		internal void Create(byte[] data, int offset, int length, PacketFlags flags) {
			if (data == null)
				throw new ArgumentNullException("data");

			if (offset < 0)
				throw new ArgumentOutOfRangeException("offset");

			if (length < 0 || length > data.Length)
				throw new ArgumentOutOfRangeException("length");

			nativePacket = Native.enet_packet_create_offset(data, (IntPtr)length, (IntPtr)offset, flags);
		}

		internal void Create(IntPtr data, int offset, int length, PacketFlags flags) {
			if (data == IntPtr.Zero)
				throw new ArgumentNullException("data");

			if (offset < 0)
				throw new ArgumentOutOfRangeException("offset");

			if (length < 0)
				throw new ArgumentOutOfRangeException("length");

			nativePacket = Native.enet_packet_create_offset(data, (IntPtr)length, (IntPtr)offset, flags);
		}

		internal void CopyTo(byte[] destination) {
			if (destination == null)
				throw new ArgumentNullException("destination");

			Marshal.Copy(Data, destination, 0, Length);
		}
	}

	internal class Host : IDisposable {
		private IntPtr nativeHost;

		internal IntPtr NativeData {
			get {
				return nativeHost;
			}

			set {
				nativeHost = value;
			}
		}

		public void Dispose() {
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing) {
			if (nativeHost != IntPtr.Zero) {
				Native.enet_host_destroy(nativeHost);
				nativeHost = IntPtr.Zero;
			}
		}

		~Host() {
			Dispose(false);
		}

		internal bool IsSet {
			get {
				return nativeHost != IntPtr.Zero;
			}
		}

		internal uint PeersCount {
			get {
				ThrowIfNotCreated();

				return Native.enet_host_get_peers_count(nativeHost);
			}
		}

		internal uint PacketsSent {
			get {
				ThrowIfNotCreated();

				return Native.enet_host_get_packets_sent(nativeHost);
			}
		}

		internal uint PacketsReceived {
			get {
				ThrowIfNotCreated();

				return Native.enet_host_get_packets_received(nativeHost);
			}
		}

		internal uint BytesSent {
			get {
				ThrowIfNotCreated();

				return Native.enet_host_get_bytes_sent(nativeHost);
			}
		}

		internal uint BytesReceived {
			get {
				ThrowIfNotCreated();

				return Native.enet_host_get_bytes_received(nativeHost);
			}
		}

		internal void ThrowIfNotCreated() {
			if (nativeHost == IntPtr.Zero)
				throw new InvalidOperationException("Host not created");
		}

		private static void ThrowIfChannelsExceeded(int channelLimit) {
			if (channelLimit < 0 || channelLimit > Library.maxChannelCount)
				throw new ArgumentOutOfRangeException("channelLimit");
		}

		internal void Create() {
			Create(null, 1, 0);
		}

		internal void Create(int bufferSize) {
			Create(null, 1, 0, 0, 0, bufferSize);
		}

		internal void Create(Address? address, int peerLimit) {
			Create(address, peerLimit, 0);
		}

		internal void Create(Address? address, int peerLimit, int channelLimit) {
			Create(address, peerLimit, channelLimit, 0, 0, 0);
		}

		internal void Create(int peerLimit, int channelLimit) {
			Create(null, peerLimit, channelLimit, 0, 0, 0);
		}

		internal void Create(int peerLimit, int channelLimit, uint incomingBandwidth, uint outgoingBandwidth) {
			Create(null, peerLimit, channelLimit, incomingBandwidth, outgoingBandwidth, 0);
		}

		internal void Create(Address? address, int peerLimit, int channelLimit, uint incomingBandwidth, uint outgoingBandwidth) {
			Create(address, peerLimit, channelLimit, incomingBandwidth, outgoingBandwidth, 0);
		}

		internal void Create(Address? address, int peerLimit, int channelLimit, uint incomingBandwidth, uint outgoingBandwidth, int bufferSize) {
			if (nativeHost != IntPtr.Zero)
				throw new InvalidOperationException("Host already created");

			if (peerLimit < 0 || peerLimit > Library.maxPeers)
				throw new ArgumentOutOfRangeException("peerLimit");

			ThrowIfChannelsExceeded(channelLimit);

			if (address != null) {
				var nativeAddress = address.Value.NativeData;

				nativeHost = Native.enet_host_create(ref nativeAddress, (IntPtr)peerLimit, (IntPtr)channelLimit, incomingBandwidth, outgoingBandwidth, bufferSize);
			} else {
				nativeHost = Native.enet_host_create(IntPtr.Zero, (IntPtr)peerLimit, (IntPtr)channelLimit, incomingBandwidth, outgoingBandwidth, bufferSize);
			}

			if (nativeHost == IntPtr.Zero)
				throw new InvalidOperationException("Host creation call failed");
		}

		internal void PreventConnections(bool state) {
			ThrowIfNotCreated();

			Native.enet_host_prevent_connections(nativeHost, (byte)(state ? 1 : 0));
		}

		internal void Broadcast(byte channelID, ref Packet packet) {
			ThrowIfNotCreated();

			packet.ThrowIfNotCreated();
			Native.enet_host_broadcast(nativeHost, channelID, packet.NativeData);
			packet.NativeData = IntPtr.Zero;
		}

		internal void Broadcast(byte channelID, ref Packet packet, Peer excludedPeer) {
			ThrowIfNotCreated();

			packet.ThrowIfNotCreated();
			Native.enet_host_broadcast_exclude(nativeHost, channelID, packet.NativeData, excludedPeer.NativeData);
			packet.NativeData = IntPtr.Zero;
		}

		internal void Broadcast(byte channelID, ref Packet packet, Peer[] peers) {
			if (peers == null)
				throw new ArgumentNullException("peers");

			ThrowIfNotCreated();

			packet.ThrowIfNotCreated();

			if (peers.Length > 0) {
				IntPtr[] nativePeers = ArrayPool.GetPointerBuffer();
				int nativeCount = 0;

				for (int i = 0; i < peers.Length; i++) {
					if (peers[i].NativeData != IntPtr.Zero) {
						nativePeers[nativeCount] = peers[i].NativeData;
						nativeCount++;
					}
				}

				Native.enet_host_broadcast_selective(nativeHost, channelID, packet.NativeData, nativePeers, (IntPtr)nativeCount);
			}

			packet.NativeData = IntPtr.Zero;
		}

		internal int CheckEvents(out Event @event) {
			ThrowIfNotCreated();

			ENetEvent nativeEvent;

			var result = Native.enet_host_check_events(nativeHost, out nativeEvent);

			if (result <= 0) {
				@event = default(Event);

				return result;
			}

			@event = new Event(nativeEvent);

			return result;
		}

		internal Peer Connect(Address address) {
			return Connect(address, 0, 0);
		}

		internal Peer Connect(Address address, int channelLimit) {
			return Connect(address, channelLimit, 0);
		}

		internal Peer Connect(Address address, int channelLimit, uint data) {
			ThrowIfNotCreated();
			ThrowIfChannelsExceeded(channelLimit);

			var nativeAddress = address.NativeData;
			var peer = new Peer(Native.enet_host_connect(nativeHost, ref nativeAddress, (IntPtr)channelLimit, data));

			if (peer.NativeData == IntPtr.Zero)
				throw new InvalidOperationException("Host connect call failed");

			return peer;
		}

		internal int Service(int timeout, out Event @event) {
			if (timeout < 0)
				throw new ArgumentOutOfRangeException("timeout");

			ThrowIfNotCreated();

			ENetEvent nativeEvent;

			var result = Native.enet_host_service(nativeHost, out nativeEvent, (uint)timeout);

			if (result <= 0) {
				@event = default(Event);

				return result;
			}

			@event = new Event(nativeEvent);

			return result;
		}

		internal void SetBandwidthLimit(uint incomingBandwidth, uint outgoingBandwidth) {
			ThrowIfNotCreated();

			Native.enet_host_bandwidth_limit(nativeHost, incomingBandwidth, outgoingBandwidth);
		}

		internal void SetChannelLimit(int channelLimit) {
			ThrowIfNotCreated();
			ThrowIfChannelsExceeded(channelLimit);

			Native.enet_host_channel_limit(nativeHost, (IntPtr)channelLimit);
		}

		internal void SetMaxDuplicatePeers(ushort number) {
			ThrowIfNotCreated();

			Native.enet_host_set_max_duplicate_peers(nativeHost, number);
		}

		internal void SetInterceptCallback(IntPtr callback) {
			ThrowIfNotCreated();

			Native.enet_host_set_intercept_callback(nativeHost, callback);
		}

		internal void SetInterceptCallback(InterceptCallback callback) {
			ThrowIfNotCreated();

			Native.enet_host_set_intercept_callback(nativeHost, Marshal.GetFunctionPointerForDelegate(callback));
		}

		internal void SetChecksumCallback(IntPtr callback) {
			ThrowIfNotCreated();

			Native.enet_host_set_checksum_callback(nativeHost, callback);
		}

		internal void SetChecksumCallback(ChecksumCallback callback) {
			ThrowIfNotCreated();

			Native.enet_host_set_checksum_callback(nativeHost, Marshal.GetFunctionPointerForDelegate(callback));
		}

		internal void Flush() {
			ThrowIfNotCreated();

			Native.enet_host_flush(nativeHost);
		}
	}

	internal struct Peer {
		private IntPtr nativePeer;
		private uint nativeID;

		internal IntPtr NativeData {
			get {
				return nativePeer;
			}

			set {
				nativePeer = value;
			}
		}

		internal Peer(IntPtr peer) {
			nativePeer = peer;
			nativeID = nativePeer != IntPtr.Zero ? Native.enet_peer_get_id(nativePeer) : 0;
		}

		internal bool IsSet {
			get {
				return nativePeer != IntPtr.Zero;
			}
		}

		internal uint ID {
			get {
				return nativeID;
			}
		}

		internal string IP {
			get {
				ThrowIfNotCreated();

				byte[] ip = ArrayPool.GetByteBuffer();

				if (Native.enet_peer_get_ip(nativePeer, ip, (IntPtr)ip.Length) == 0)
					return Encoding.ASCII.GetString(ip, 0, ip.StringLength());
				else
					return String.Empty;
			}
		}

		internal ushort Port {
			get {
				ThrowIfNotCreated();

				return Native.enet_peer_get_port(nativePeer);
			}
		}

		internal uint MTU {
			get {
				ThrowIfNotCreated();

				return Native.enet_peer_get_mtu(nativePeer);
			}
		}

		internal PeerState State {
			get {
				return nativePeer == IntPtr.Zero ? PeerState.Uninitialized : Native.enet_peer_get_state(nativePeer);
			}
		}

		internal uint RoundTripTime {
			get {
				ThrowIfNotCreated();

				return Native.enet_peer_get_rtt(nativePeer);
			}
		}

		internal uint LastRoundTripTime {
			get {
				ThrowIfNotCreated();

				return Native.enet_peer_get_last_rtt(nativePeer);
			}
		}

		internal uint LastSendTime {
			get {
				ThrowIfNotCreated();

				return Native.enet_peer_get_lastsendtime(nativePeer);
			}
		}

		internal uint LastReceiveTime {
			get {
				ThrowIfNotCreated();

				return Native.enet_peer_get_lastreceivetime(nativePeer);
			}
		}

		internal ulong PacketsSent {
			get {
				ThrowIfNotCreated();

				return Native.enet_peer_get_packets_sent(nativePeer);
			}
		}

		internal ulong PacketsLost {
			get {
				ThrowIfNotCreated();

				return Native.enet_peer_get_packets_lost(nativePeer);
			}
		}

		internal float PacketsThrottle {
			get {
				ThrowIfNotCreated();

				return Native.enet_peer_get_packets_throttle(nativePeer);
			}
		}

		internal ulong BytesSent {
			get {
				ThrowIfNotCreated();

				return Native.enet_peer_get_bytes_sent(nativePeer);
			}
		}

		internal ulong BytesReceived {
			get {
				ThrowIfNotCreated();

				return Native.enet_peer_get_bytes_received(nativePeer);
			}
		}

		internal IntPtr Data {
			get {
				ThrowIfNotCreated();

				return Native.enet_peer_get_data(nativePeer);
			}

			set {
				ThrowIfNotCreated();

				Native.enet_peer_set_data(nativePeer, value);
			}
		}

		internal void ThrowIfNotCreated() {
			if (nativePeer == IntPtr.Zero)
				throw new InvalidOperationException("Peer not created");
		}

		internal void ConfigureThrottle(uint interval, uint acceleration, uint deceleration, uint threshold) {
			ThrowIfNotCreated();

			Native.enet_peer_throttle_configure(nativePeer, interval, acceleration, deceleration, threshold);
		}

		internal bool Send(byte channelID, ref Packet packet) {
			ThrowIfNotCreated();

			packet.ThrowIfNotCreated();

			return Native.enet_peer_send(nativePeer, channelID, packet.NativeData) == 0;
		}

		internal bool Receive(out byte channelID, out Packet packet) {
			ThrowIfNotCreated();

			IntPtr nativePacket = Native.enet_peer_receive(nativePeer, out channelID);

			if (nativePacket != IntPtr.Zero) {
				packet = new Packet(nativePacket);

				return true;
			}

			packet = default(Packet);

			return false;
		}

		internal void Ping() {
			ThrowIfNotCreated();

			Native.enet_peer_ping(nativePeer);
		}

		internal void PingInterval(uint interval) {
			ThrowIfNotCreated();

			Native.enet_peer_ping_interval(nativePeer, interval);
		}

		internal void Timeout(uint timeoutLimit, uint timeoutMinimum, uint timeoutMaximum) {
			ThrowIfNotCreated();

			Native.enet_peer_timeout(nativePeer, timeoutLimit, timeoutMinimum, timeoutMaximum);
		}

		internal void Disconnect(uint data) {
			ThrowIfNotCreated();

			Native.enet_peer_disconnect(nativePeer, data);
		}

		internal void DisconnectNow(uint data) {
			ThrowIfNotCreated();

			Native.enet_peer_disconnect_now(nativePeer, data);
		}

		internal void DisconnectLater(uint data) {
			ThrowIfNotCreated();

			Native.enet_peer_disconnect_later(nativePeer, data);
		}

		internal void Reset() {
			ThrowIfNotCreated();

			Native.enet_peer_reset(nativePeer);
		}
	}

	internal static class Extensions {
		internal static int StringLength(this byte[] data) {
			if (data == null)
				throw new ArgumentNullException("data");

			int i;

			for (i = 0; i < data.Length && data[i] != 0; i++);

			return i;
		}
	}

	internal static class Library {
		internal const uint maxChannelCount = 0xFF;
		internal const uint maxPeers = 0xFFF;
		internal const uint maxPacketSize = 32 * 1024 * 1024;
		internal const uint throttleThreshold = 40;
		internal const uint throttleScale = 32;
		internal const uint throttleAcceleration = 2;
		internal const uint throttleDeceleration = 2;
		internal const uint throttleInterval = 5000;
		internal const uint timeoutLimit = 32;
		internal const uint timeoutMinimum = 5000;
		internal const uint timeoutMaximum = 30000;
		internal const uint version = (2 << 16) | (4 << 8) | (5);

		internal static uint Time {
			get {
				return Native.enet_time_get();
			}
		}

		internal static bool Initialize() {
			if (Native.enet_linked_version() != version)
				throw new InvalidOperationException("Incompatatible version");

			return Native.enet_initialize() == 0;
		}

		internal static bool Initialize(Callbacks callbacks) {
			if (callbacks == null)
				throw new ArgumentNullException("callbacks");

			if (Native.enet_linked_version() != version)
				throw new InvalidOperationException("Incompatatible version");

			ENetCallbacks nativeCallbacks = callbacks.NativeData;

			return Native.enet_initialize_with_callbacks(version, ref nativeCallbacks) == 0;
		}

		internal static void Deinitialize() {
			Native.enet_deinitialize();
		}

		internal static ulong CRC64(IntPtr buffers, int bufferCount) {
			return Native.enet_crc64(buffers, bufferCount);
		}
	}

	[SuppressUnmanagedCodeSecurity]
	internal static class Native {
		#if __IOS__ || UNITY_IOS && !UNITY_EDITOR
			private const string nativeLibrary = "__Internal";
		#else
			private const string nativeLibrary = "enet";
		#endif

		[DllImport(nativeLibrary, CallingConvention = CallingConvention.Cdecl)]
		internal static extern int enet_initialize();

		[DllImport(nativeLibrary, CallingConvention = CallingConvention.Cdecl)]
		internal static extern int enet_initialize_with_callbacks(uint version, ref ENetCallbacks inits);

		[DllImport(nativeLibrary, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void enet_deinitialize();

		[DllImport(nativeLibrary, CallingConvention = CallingConvention.Cdecl)]
		internal static extern uint enet_linked_version();

		[DllImport(nativeLibrary, CallingConvention = CallingConvention.Cdecl)]
		internal static extern uint enet_time_get();

		[DllImport(nativeLibrary, CallingConvention = CallingConvention.Cdecl)]
		internal static extern ulong enet_crc64(IntPtr buffers, int bufferCount);

		[DllImport(nativeLibrary, CallingConvention = CallingConvention.Cdecl)]
		internal static extern int enet_address_set_ip(ref ENetAddress address, string ip);

		[DllImport(nativeLibrary, CallingConvention = CallingConvention.Cdecl)]
		internal static extern int enet_address_set_hostname(ref ENetAddress address, string hostName);

		[DllImport(nativeLibrary, CallingConvention = CallingConvention.Cdecl)]
		internal static extern int enet_address_get_ip(ref ENetAddress address, StringBuilder ip, IntPtr ipLength);

		[DllImport(nativeLibrary, CallingConvention = CallingConvention.Cdecl)]
		internal static extern int enet_address_get_hostname(ref ENetAddress address, StringBuilder hostName, IntPtr nameLength);

		[DllImport(nativeLibrary, CallingConvention = CallingConvention.Cdecl)]
		internal static extern IntPtr enet_packet_create(byte[] data, IntPtr dataLength, PacketFlags flags);

		[DllImport(nativeLibrary, CallingConvention = CallingConvention.Cdecl)]
		internal static extern IntPtr enet_packet_create(IntPtr data, IntPtr dataLength, PacketFlags flags);

		[DllImport(nativeLibrary, CallingConvention = CallingConvention.Cdecl)]
		internal static extern IntPtr enet_packet_create_offset(byte[] data, IntPtr dataLength, IntPtr dataOffset, PacketFlags flags);

		[DllImport(nativeLibrary, CallingConvention = CallingConvention.Cdecl)]
		internal static extern IntPtr enet_packet_create_offset(IntPtr data, IntPtr dataLength, IntPtr dataOffset, PacketFlags flags);

		[DllImport(nativeLibrary, CallingConvention = CallingConvention.Cdecl)]
		internal static extern int enet_packet_check_references(IntPtr packet);

		[DllImport(nativeLibrary, CallingConvention = CallingConvention.Cdecl)]
		internal static extern IntPtr enet_packet_get_data(IntPtr packet);

		[DllImport(nativeLibrary, CallingConvention = CallingConvention.Cdecl)]
		internal static extern IntPtr enet_packet_get_user_data(IntPtr packet);

		[DllImport(nativeLibrary, CallingConvention = CallingConvention.Cdecl)]
		internal static extern IntPtr enet_packet_set_user_data(IntPtr packet, IntPtr userData);

		[DllImport(nativeLibrary, CallingConvention = CallingConvention.Cdecl)]
		internal static extern int enet_packet_get_length(IntPtr packet);

		[DllImport(nativeLibrary, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void enet_packet_set_free_callback(IntPtr packet, IntPtr callback);

		[DllImport(nativeLibrary, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void enet_packet_dispose(IntPtr packet);

		[DllImport(nativeLibrary, CallingConvention = CallingConvention.Cdecl)]
		internal static extern IntPtr enet_host_create(ref ENetAddress address, IntPtr peerLimit, IntPtr channelLimit, uint incomingBandwidth, uint outgoingBandwidth, int bufferSize);

		[DllImport(nativeLibrary, CallingConvention = CallingConvention.Cdecl)]
		internal static extern IntPtr enet_host_create(IntPtr address, IntPtr peerLimit, IntPtr channelLimit, uint incomingBandwidth, uint outgoingBandwidth, int bufferSize);

		[DllImport(nativeLibrary, CallingConvention = CallingConvention.Cdecl)]
		internal static extern IntPtr enet_host_connect(IntPtr host, ref ENetAddress address, IntPtr channelCount, uint data);

		[DllImport(nativeLibrary, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void enet_host_broadcast(IntPtr host, byte channelID, IntPtr packet);

		[DllImport(nativeLibrary, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void enet_host_broadcast_exclude(IntPtr host, byte channelID, IntPtr packet, IntPtr excludedPeer);

		[DllImport(nativeLibrary, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void enet_host_broadcast_selective(IntPtr host, byte channelID, IntPtr packet, IntPtr[] peers, IntPtr peersLength);

		[DllImport(nativeLibrary, CallingConvention = CallingConvention.Cdecl)]
		internal static extern int enet_host_service(IntPtr host, out ENetEvent @event, uint timeout);

		[DllImport(nativeLibrary, CallingConvention = CallingConvention.Cdecl)]
		internal static extern int enet_host_check_events(IntPtr host, out ENetEvent @event);

		[DllImport(nativeLibrary, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void enet_host_channel_limit(IntPtr host, IntPtr channelLimit);

		[DllImport(nativeLibrary, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void enet_host_bandwidth_limit(IntPtr host, uint incomingBandwidth, uint outgoingBandwidth);

		[DllImport(nativeLibrary, CallingConvention = CallingConvention.Cdecl)]
		internal static extern uint enet_host_get_peers_count(IntPtr host);

		[DllImport(nativeLibrary, CallingConvention = CallingConvention.Cdecl)]
		internal static extern uint enet_host_get_packets_sent(IntPtr host);

		[DllImport(nativeLibrary, CallingConvention = CallingConvention.Cdecl)]
		internal static extern uint enet_host_get_packets_received(IntPtr host);

		[DllImport(nativeLibrary, CallingConvention = CallingConvention.Cdecl)]
		internal static extern uint enet_host_get_bytes_sent(IntPtr host);

		[DllImport(nativeLibrary, CallingConvention = CallingConvention.Cdecl)]
		internal static extern uint enet_host_get_bytes_received(IntPtr host);

		[DllImport(nativeLibrary, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void enet_host_set_max_duplicate_peers(IntPtr host, ushort number);

		[DllImport(nativeLibrary, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void enet_host_set_intercept_callback(IntPtr host, IntPtr callback);

		[DllImport(nativeLibrary, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void enet_host_set_checksum_callback(IntPtr host, IntPtr callback);

		[DllImport(nativeLibrary, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void enet_host_flush(IntPtr host);

		[DllImport(nativeLibrary, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void enet_host_destroy(IntPtr host);

		[DllImport(nativeLibrary, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void enet_host_prevent_connections(IntPtr host, byte state);

		[DllImport(nativeLibrary, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void enet_peer_throttle_configure(IntPtr peer, uint interval, uint acceleration, uint deceleration, uint threshold);

		[DllImport(nativeLibrary, CallingConvention = CallingConvention.Cdecl)]
		internal static extern uint enet_peer_get_id(IntPtr peer);

		[DllImport(nativeLibrary, CallingConvention = CallingConvention.Cdecl)]
		internal static extern int enet_peer_get_ip(IntPtr peer, byte[] ip, IntPtr ipLength);

		[DllImport(nativeLibrary, CallingConvention = CallingConvention.Cdecl)]
		internal static extern ushort enet_peer_get_port(IntPtr peer);

		[DllImport(nativeLibrary, CallingConvention = CallingConvention.Cdecl)]
		internal static extern uint enet_peer_get_mtu(IntPtr peer);

		[DllImport(nativeLibrary, CallingConvention = CallingConvention.Cdecl)]
		internal static extern PeerState enet_peer_get_state(IntPtr peer);

		[DllImport(nativeLibrary, CallingConvention = CallingConvention.Cdecl)]
		internal static extern uint enet_peer_get_rtt(IntPtr peer);

		[DllImport(nativeLibrary, CallingConvention = CallingConvention.Cdecl)]
		internal static extern uint enet_peer_get_last_rtt(IntPtr peer);

		[DllImport(nativeLibrary, CallingConvention = CallingConvention.Cdecl)]
		internal static extern uint enet_peer_get_lastsendtime(IntPtr peer);

		[DllImport(nativeLibrary, CallingConvention = CallingConvention.Cdecl)]
		internal static extern uint enet_peer_get_lastreceivetime(IntPtr peer);

		[DllImport(nativeLibrary, CallingConvention = CallingConvention.Cdecl)]
		internal static extern ulong enet_peer_get_packets_sent(IntPtr peer);

		[DllImport(nativeLibrary, CallingConvention = CallingConvention.Cdecl)]
		internal static extern ulong enet_peer_get_packets_lost(IntPtr peer);

		[DllImport(nativeLibrary, CallingConvention = CallingConvention.Cdecl)]
		internal static extern float enet_peer_get_packets_throttle(IntPtr peer);

		[DllImport(nativeLibrary, CallingConvention = CallingConvention.Cdecl)]
		internal static extern ulong enet_peer_get_bytes_sent(IntPtr peer);

		[DllImport(nativeLibrary, CallingConvention = CallingConvention.Cdecl)]
		internal static extern ulong enet_peer_get_bytes_received(IntPtr peer);

		[DllImport(nativeLibrary, CallingConvention = CallingConvention.Cdecl)]
		internal static extern IntPtr enet_peer_get_data(IntPtr peer);

		[DllImport(nativeLibrary, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void enet_peer_set_data(IntPtr peer, IntPtr data);

		[DllImport(nativeLibrary, CallingConvention = CallingConvention.Cdecl)]
		internal static extern int enet_peer_send(IntPtr peer, byte channelID, IntPtr packet);

		[DllImport(nativeLibrary, CallingConvention = CallingConvention.Cdecl)]
		internal static extern IntPtr enet_peer_receive(IntPtr peer, out byte channelID);

		[DllImport(nativeLibrary, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void enet_peer_ping(IntPtr peer);

		[DllImport(nativeLibrary, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void enet_peer_ping_interval(IntPtr peer, uint pingInterval);

		[DllImport(nativeLibrary, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void enet_peer_timeout(IntPtr peer, uint timeoutLimit, uint timeoutMinimum, uint timeoutMaximum);

		[DllImport(nativeLibrary, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void enet_peer_disconnect(IntPtr peer, uint data);

		[DllImport(nativeLibrary, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void enet_peer_disconnect_now(IntPtr peer, uint data);

		[DllImport(nativeLibrary, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void enet_peer_disconnect_later(IntPtr peer, uint data);

		[DllImport(nativeLibrary, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void enet_peer_reset(IntPtr peer);
	}
}