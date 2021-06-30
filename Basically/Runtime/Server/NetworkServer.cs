#if BASICALLY_SERVER

using System;   
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Basically.Networking;
using Basically.Serialization;
using Basically.Utility;

namespace Basically.Server {
    public static class NetworkServer {
        private static NetworkHost host;
        internal static HostCallbacks originalCallbacks;

        public static Connection[] Connections => host.Connections;
        public static byte ConnectedPlayers => (byte)Connections.Where(x => x.Connected).Count();

        /// <summary>
        /// Initializes the Basically server.
        /// </summary>
        /// <param name="channels">Channel limit for networking.</param>
        public static void Initialize(int channels, HostCallbacks callbacks) {
            if (host != null) return;

            host = new NetworkHost(channels, new ServerCallbacks());
            SerializerStorage.Initialize();
            ActionCache.Initialize();

            AddReceiverClass(typeof(ServerReceivers));
            originalCallbacks = callbacks;
        }

        /// <summary>
        /// Deinitializes the Basically server.
        /// </summary>
        public static void Deinitialize() {
            host = null;
        }

        /// <summary>
        /// Starts hosting the server.
        /// </summary>
        /// <param name="maxPlayers">Maximum amount of connections/players.</param>
        /// <param name="port">Port the server will run on.</param>
        public static void StartServer(byte maxPlayers, ushort port) {
            if (host == null) return;
            host.StartHost(port, maxPlayers);
        }

        /// <summary>
        /// Stops the server, disconnecting everyone.
        /// </summary>
        public static void StopServer() {
            if (host == null) return;
            host.Stop();
        }

        /// <summary>
        /// Updates the server, should be updated every frame.
        /// </summary>
        public static void Update() {
            if (host == null) return;
            ActionCache.Execute();
        }

        /// <summary>
        /// Adds receivers from a class. All receivers must be public and static.
        /// </summary>
        /// <param name="type">Class where receivers are contained.</param>
        public static void AddReceiverClass(Type type) {
            if (host == null) return;

            foreach (var method in type.GetMethods(BindingFlags.Public | BindingFlags.Static)) {
                if (!NetworkUtility.VerifyMethod(method)) continue;
                host.AddReceiverInternal(method.GetParameters()[1].ParameterType, NetworkUtility.GetDelegate(method));
            }
        }

        /// <summary>
        /// Broadcasts the message to every player.
        /// </summary>
        /// <typeparam name="T">Message type.</typeparam>
        /// <param name="message">The message.</param>
        /// <param name="channel">Which channel to send the message through.</param>
        /// <param name="type">What type of message is this.</param>
        public static void Broadcast<T>(T message, byte channel, MessageType type) where T : NetworkMessage {
            var payload = MessagePacker.SerializeMessage(message);
            var flags = (Networking.ENet.PacketFlags)type;

            foreach (var conn in host.Connections) {
                if (conn.Connected) conn.SendInternal(payload, channel, flags);
            }
        }

        /// <summary>
        /// Broadcasts the message to every player, excluding one.
        /// </summary>
        /// <typeparam name="T">Message type.</typeparam>
        /// <param name="message">The message.</param>
        /// <param name="channel">Which channel to send the message through.</param>
        /// <param name="type">What type of message is this.</param>
        /// <param name="excluding">Which player/connection to exclude.</param>
        public static void Broadcast<T>(T message, byte channel, MessageType type, Connection excluding) where T : NetworkMessage {
            var payload = MessagePacker.SerializeMessage(message);
            var flags = (Networking.ENet.PacketFlags)type;

            foreach (var conn in host.Connections) {
                if (conn.Connected) {
                    if (conn != excluding) {
                        conn.SendInternal(payload, channel, flags);
                    }
                }
            }
        }

        /// <summary>
        /// Broadcasts the message to every player, excluding one.
        /// </summary>
        /// <typeparam name="T">Message type.</typeparam>
        /// <param name="message">The message.</param>
        /// <param name="channel">Which channel to send the message through.</param>
        /// <param name="type">What type of message is this.</param>
        /// <param name="excluding">ID of player to exclude.</param>
        public static void Broadcast<T>(T message, byte channel, MessageType type, byte excluding) where T : NetworkMessage {
            Broadcast(message, channel, type, host.Connections[excluding]);
        }
    }
}

#endif