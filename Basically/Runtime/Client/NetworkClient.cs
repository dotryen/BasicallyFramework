#if BASICALLY_CLIENT

using System;
using UnityEngine;

namespace Basically.Client {
    using Networking;

    public static class NetworkClient {
        internal static Transport host;
        internal static HostCallbacks originalCallbacks;
        internal static MethodHandler handler;
        internal static Connection connection;

        public static Connection Connection => connection;

        public static bool Connected {
            get {
                if (Connection == null) return false;
                return Connection.Connected;
            }
        }

        public static bool Authenticated {
            get {
                if (Connection == null) return false;
                return Connection.Authenticated;
            }
        }

        public static MethodHandler Handler => handler;

        public static byte ID { get; internal set; }

        public static bool Initialized { get; private set; }

        /// <summary>
        /// Initializes the Basically client.
        /// </summary>
        /// <param name="callbacks">Host callbacks to use, can be null.</param>
        public static void Initialize(HostCallbacks callbacks = null) {
            if (Initialized) return;

            handler = new MethodHandler(new ClientCallbacks());
            handler.AddReceiverClass(typeof(ClientReceivers));
            originalCallbacks = callbacks;

            host = new Transport(handler);
            Initialized = true;
        }

        #if BASICALLY_SERVER

        internal static void LocalInitialize(HostCallbacks callbacks = null) {
            if (Initialized) return;

            handler = new MethodHandler(new ClientCallbacks());
            handler.AddReceiverClass(typeof(ClientReceivers));
            originalCallbacks = callbacks;

            connection = new LocalServerConnection();

            Initialized = true;
        }

        #endif

        /// <summary>
        /// Deinitializes the Basically client.
        /// Disconnecting the client automatically deinitializes.
        /// </summary>
        public static void Deinitialize() {
            host = null;
            originalCallbacks = null;
            connection = null;
            Initialized = false;
        }

        /// <summary>
        /// Attempt to connect to a server.
        /// </summary>
        /// <param name="ip">The ip of the server.</param>
        /// <param name="port">The port of the server.</param>
        public static void ConnectToServer(string ip, ushort port) {
            if (host == null) return;
            host.ConnectToHost(ip, port);
            connection = host.Connections[0];
        }

        /// <summary>
        /// Updates the host, must be updated constantly (The update funtion)
        /// </summary>
        public static void Update() {
            if (host == null) return;
            handler.Tasks.Execute();
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