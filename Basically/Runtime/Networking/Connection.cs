using System.Collections;
using System.Collections.Generic;
using Basically.Serialization;

namespace Basically.Networking {
    using Utility;
    using ENet;

    public sealed class Connection {
        internal Peer peer;
        internal NetworkHost host;

        public bool Connected => Status == ConnectionStatus.WaitingForHandshake || Authenticated;
        public bool Authenticated => Status == ConnectionStatus.Connected;
        public ConnectionStatus Status { get; internal set; } = ConnectionStatus.NotConnected;
        public byte ID => (byte)peer.ID;
        public string IP => peer.IP;
        public uint Ping => peer.RoundTripTime / 2;
        public ulong BytesReceived => peer.BytesReceived;

        internal Connection() {

        }

        internal void Setup(Peer peer) {
            this.peer = peer;
            Status = ConnectionStatus.WaitingForHandshake;
        }

        internal void Reset() {
            peer = default;
            Status = ConnectionStatus.NotConnected;
        }

        /// <summary>
        /// Sends the user the message.
        /// </summary>
        /// <typeparam name="T">Type of message.</typeparam>
        /// <param name="message">The message to send.</param>
        /// <param name="channel">Which channel to send through.</param>
        /// <param name="type">How to send it.</param>
        public void Send<T>(T message, byte channel, MessageType type) where T : struct, NetworkMessage {
            if (!Connected) return;

            var writer = Pool<Writer>.Pull();
            Packer.Pack(message, writer);
            SendInternal(writer.ToArray(), channel, (PacketFlags)type);
            Pool<Writer>.Push(writer);
        }

        internal void SendInternal(byte[] payload, byte channel, PacketFlags type) {
            ThreadData.AddNet(() => {
                Packet packet = default;
                packet.Create(payload, type);
                peer.Send(channel, ref packet);
            });
        }

        public void Disconnect(byte data = 0) {
            if (!Connected) return;
            peer.Disconnect(data);
            // host.callbacks.OnDisconnect(this, data);
            Reset();
        }
    }
}
