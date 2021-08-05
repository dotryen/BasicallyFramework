#if BASICALLY_CLIENT

using System;
using UnityEngine;

namespace Basically.Client {
    using Networking;

    public static class NetworkClient {
        private static NetworkHost host;
        internal static HostCallbacks originalCallbacks;

        public static Connection Connection {
            get {
                if (host == null) return null;
                return host.Connections[0];
            }
        }

        public static bool Connected {
            get {
                if (Connection == null) return false;
                return Connection.Connected;
            }
        }

        public static byte ID { get; internal set; }

        /// <summary>
        /// Initializes the Basically client.
        /// </summary>
        /// <param name="channels">Channel limit for networking.</param>
        public static void Initialize(int channels = 0xFF) {
            if (host != null) return;
            host = new NetworkHost(channels, new ClientCallbacks());
            originalCallbacks = null;

            AddReceiverClass(typeof(ClientReceivers));
        }

        public static void Initialize(HostCallbacks callbacks, int channels = 0xFF) {
            if (host != null) return;
            host = new NetworkHost(channels, new ClientCallbacks());
            originalCallbacks = callbacks;

            AddReceiverClass(typeof(ClientReceivers));
        }

        /// <summary>
        /// Deinitializes the Basically client.
        /// </summary>
        public static void Deinitialize() {
            host = null;
            originalCallbacks = null;
        }

        /// <summary>
        /// Attempt to connect to a server.
        /// </summary>
        /// <param name="ip">The ip of the server.</param>
        /// <param name="port">The port of the server.</param>
        public static void ConnectToServer(string ip, ushort port) {
            if (host == null) return;
            host.ConnectToHost(ip, port);
        }

        /// <summary>
        /// Updates the host, must be updated constantly (The update funtion)
        /// </summary>
        public static void Update() {
            if (host == null) return;
            ThreadData.ExecuteUnity();
        }

        /// <summary>
        /// Disconnect from the server.
        /// </summary>
        public static void Disconnect() {
            if (host == null) return;
            Connection.Disconnect();
        }

        /// <summary>
        /// Sends a packet to the server.
        /// </summary>
        /// <typeparam name="T">Type of packet.</typeparam>
        /// <param name="message">The packet.</param>
        /// <param name="channel">Which channel to send it to.</param>
        /// <param name="type">What kind of packet is it.</param>
        public static void Send<T>(T message, byte channel, MessageType type) where T : struct, NetworkMessage {
            if (Connection == null) return;
            Connection.Send(message, channel, type);
        }

        /// <summary>
        /// Adds receivers from a class. All receivers must be public and static.
        /// </summary>
        /// <param name="type">Class where receivers are contained.</param>
        public static void AddReceiverClass(Type type) {
            if (host == null) return;
            host.AddReceiverClass(type);
        }
    }
}

#endif