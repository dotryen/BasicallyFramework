#if BASICALLY_CLIENT

using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Basically.Networking;
using Basically.Serialization;

namespace Basically.Client {
    public static class NetworkClient {
        private static NetworkHost host;
        internal static HostCallbacks originalCallbacks;

        public static byte ID { get; internal set; }
        public static uint Ping { get {
                if (host == null) return 0;
                else return host.Connections[0].Ping;
            }
        }
        public static ulong BytesReceived { get {
                if (!Connected) return 0;
                else return host.Connections[0].peer.BytesReceived;
            } 
        }

        public static ConnectionStatus ConnectionStatus { get; internal set; } = ConnectionStatus.NotConnected;
        public static bool Connected => ConnectionStatus == ConnectionStatus.Connected;

        /// <summary>
        /// Initializes the Basically client.
        /// </summary>
        /// <param name="channels">Channel limit for networking.</param>
        public static void Initialize(int channels = 0xFF) {
            if (host != null) return;
            host = new NetworkHost(channels, new ClientCallbacks());
            originalCallbacks = null;

            BasicallyCache.Initialize();

            AddReceiverClass(typeof(ClientReceivers));
        }

        public static void Initialize(HostCallbacks callbacks, int channels = 0xFF) {
            if (host != null) return;
            host = new NetworkHost(channels, new ClientCallbacks());
            originalCallbacks = callbacks;

            BasicallyCache.Initialize();

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
            ThreadData.Execute();
        }

        /// <summary>
        /// Disconnect from the server.
        /// </summary>
        public static void Disconnect() {
            if (host == null) return;
            host.Stop();
        }

        /// <summary>
        /// Sends a packet to the server.
        /// </summary>
        /// <typeparam name="T">Type of packet.</typeparam>
        /// <param name="message">The packet.</param>
        /// <param name="channel">Which channel to send it to.</param>
        /// <param name="type">What kind of packet is it.</param>
        public static void Send<T>(T message, byte channel, MessageType type) where T : struct, NetworkMessage {
            if (host == null) return;
            if (Connected) host.Connections[0].Send(message, channel, type);
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
    }
}

#endif