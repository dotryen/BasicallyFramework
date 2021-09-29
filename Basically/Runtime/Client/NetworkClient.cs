#if BASICALLY_CLIENT

using System;
using UnityEngine;

namespace Basically.Client {
    using Networking;

    public static class NetworkClient {
        private static Transport host;
        internal static HostCallbacks originalCallbacks;
        internal static MethodHandler handler;

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

        public static MethodHandler Handler => handler;

        public static byte ID { get; internal set; }

        /// <summary>
        /// Initializes the Basically client.
        /// </summary>
        /// <param name="callbacks">Host callbacks to use, can be null.</param>
        public static void Initialize(HostCallbacks callbacks = null) {
            if (host != null) return;

            handler = new MethodHandler(new ClientCallbacks());
            handler.AddReceiverClass(typeof(ClientReceivers));
            originalCallbacks = callbacks;

            host = new Transport(handler);
        }

        /// <summary>
        /// Deinitializes the Basically client.
        /// Disconnecting the client automatically deinitializes.
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
    }
}

#endif