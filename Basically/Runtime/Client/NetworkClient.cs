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

        public static byte ID { get; internal set; }
        public static uint Ping => host.Connections[0].Ping / 2;
        public static bool Connected { get {
                if (host != null) {
                    if (host.Connections != null) {
                        return host.Connections[0].Connected;
                    }
                }
                
                return false;
            } 
        }
        public static ulong BytesReceived { get {
                if (!Connected) return 0;
                else return host.Connections[0].peer.BytesReceived;
            } 
        }

        /// <summary>
        /// Initializes the Basically client.
        /// </summary>
        /// <param name="channels">Channel limit for networking.</param>
        public static void Initialize(int channels) {
            if (host != null) return;
            host = new NetworkHost(channels);
            SerializerStorage.Initialize();
            ActionCache.Initialize();

            AddReceiverClass(typeof(ClientReceivers));
        }

        /// <summary>
        /// Deinitializes the Basically client.
        /// </summary>
        public static void Deinitialize() {
            host = null;
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
        /// Updates the host, must be updated costantly. (The update funtion)
        /// </summary>
        public static void Update() {
            if (host == null) return;
            ActionCache.Execute();
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