#if BASICALLY_SERVER

using System;   
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Basically.Server {
    using Networking;
    using Serialization;
    using Utility;

    public static class NetworkServer {
        private static Transport host;
        internal static HostCallbacks originalCallbacks;
        internal static MethodHandler handler;

        public static MethodHandler Handler => handler;
        public static Connection[] Connections => host.Connections;
        public static byte ConnectedPlayers {
            get {
                if (host != null) return (byte)Connections.Where(x => x != null).Count();
                return 0;
            }
        }

        /// <summary>
        /// Initializes the Basically server.
        /// </summary>
        /// <param name="callbacks">Host callbacks to use, can be null.</param>
        public static void Initialize(HostCallbacks callbacks = null) {
            if (host != null) return;

            handler = new MethodHandler(new ServerCallbacks());
            handler.AddReceiverClass(typeof(ServerReceivers));
            originalCallbacks = callbacks;

            host = new Transport(handler);
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
            ThreadData.ExecuteUnity();
        }

        /// <summary>
        /// Broadcasts the message to every player.
        /// </summary>
        /// <typeparam name="T">Message type.</typeparam>
        /// <param name="message">The message.</param>
        /// <param name="channel">Which channel to send the message through.</param>
        /// <param name="type">What type of message is this.</param>
        public static void Broadcast<T>(T message, byte channel, MessageType type) where T : struct, NetworkMessage {
            host.Broadcast(message, channel, type);
        }

        /// <summary>
        /// Broadcasts the message to every player, excluding one.
        /// </summary>
        /// <typeparam name="T">Message type.</typeparam>
        /// <param name="message">The message.</param>
        /// <param name="channel">Which channel to send the message through.</param>
        /// <param name="type">What type of message is this.</param>
        /// <param name="excluding">Which player/connection to exclude.</param>
        public static void Broadcast<T>(T message, byte channel, MessageType type, Connection excluding) where T : struct, NetworkMessage {
            host.Broadcast(message, channel, type, excluding.ID);
        }

        /// <summary>
        /// Broadcasts the message to every player, excluding one.
        /// </summary>
        /// <typeparam name="T">Message type.</typeparam>
        /// <param name="message">The message.</param>
        /// <param name="channel">Which channel to send the message through.</param>
        /// <param name="type">What type of message is this.</param>
        /// <param name="excluding">ID of player to exclude.</param>
        public static void Broadcast<T>(T message, byte channel, MessageType type, byte excluding) where T : struct, NetworkMessage {
            host.Broadcast(message, channel, type, excluding);
        }
    }
}

#endif