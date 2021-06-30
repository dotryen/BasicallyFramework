using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

namespace Basically.Networking {
    using ENet;
    using Utility;
    using Serialization;

    public class NetworkHost {
        private Host host;
        private Connection[] connections;
        private Dictionary<byte, Receiver> receivers;

        private readonly int channelLimit;
        private bool ready;
        private HostCallbacks callbacks;
        private Thread networkThread;

        public Connection[] Connections => connections;

        /// <summary>
        /// Creates a new host with default settings.
        /// </summary>
        public NetworkHost() {
            NetworkUtility.Initialize();
            host = new Host();
            receivers = new Dictionary<byte, Receiver>();

            channelLimit = 0xFF;
            callbacks = new HostCallbacks();
        }

        /// <summary>
        /// Creates a host with a custom channel count.
        /// </summary>
        /// <param name="channels">Channel limit. (Max is 0xFF)</param>
        public NetworkHost(int channels) {
            NetworkUtility.Initialize();
            host = new Host();
            receivers = new Dictionary<byte, Receiver>();

            channelLimit = channels;
            callbacks = new HostCallbacks();
        }

        /// <summary>
        /// Creates a host with custom settings.
        /// </summary>
        /// <param name="channels">Channel limit. (Max is 0xFF)</param>
        /// <param name="callbacks">Callbacks to use, MUST NOT BE NULL.</param>
        public NetworkHost(int channels, HostCallbacks callbacks) {
            if (callbacks == null) throw new ArgumentNullException("Callbacks");

            NetworkUtility.Initialize();
            host = new Host();
            receivers = new Dictionary<byte, Receiver>();

            channelLimit = channels;
            this.callbacks = callbacks;
        }

        public void ConnectToHost(string ip, ushort port) {
            if (ip == "localhost") ip = "127.0.0.1";

            Address add = new Address {
                Port = port
            };
            add.SetHost(ip);

            PopulateConnections(1);

            host.Create(1, channelLimit);
            host.Connect(add);
            StartThread();
        }

        public void StartHost(ushort port, byte maxPlayers) {
            Address add = new Address {
                Port = port
            };

            PopulateConnections(maxPlayers);

            host.Create(add, connections.Length);
            StartThread();
        }

        private void StartThread() {
            ready = true;
            networkThread = new Thread(ThreadFunc);
            networkThread.Name = "Basically.Net";
            networkThread.Start();
        }

        private void ThreadFunc() {
            UnityEngine.Profiling.Profiler.BeginThreadProfiling("Basically", "Network");

            while (ready) {
                Update();
            }

            // disconnect everyone
            foreach (var conn in connections) {
                conn.Disconnect();
            }

            UnityEngine.Profiling.Profiler.EndThreadProfiling();
        }

        private void Update() {
            bool polled = false;
            while (!polled) {
                if (host.CheckEvents(out Event netEvent) <= 0) {
                    if (host.Service(0, out netEvent) <= 0) return;

                    polled = true;
                }
                switch (netEvent.Type) {
                    case EventType.None:
                        // do nothing lol why is this here
                        break;

                    case EventType.Connect: {
                        bool canAccept = !connections[netEvent.Peer.ID].Connected;

                        if (canAccept) {
                            foreach (var conn in connections) {
                                if (conn.Connected) {
                                    if (conn.peer.IP == netEvent.Peer.IP) {
                                        canAccept = false;
                                    }
                                }
                            }

                            if (canAccept) {
                                connections[netEvent.Peer.ID].Setup(netEvent.Peer);

                                ActionCache.Add(() => {
                                    callbacks.OnConnect(connections[netEvent.Peer.ID]);
                                });
                                break;
                            }
                        }

                        // disconnect if failed
                        netEvent.Peer.Disconnect(1);
                        break;
                    }

                    case EventType.Disconnect: {
                        // Reset to reuse connection
                        ActionCache.Add(() => {
                            var conn = connections[netEvent.Peer.ID];
                            callbacks.OnDisconnect(conn);
                            conn.Reset();
                        });
                        break;
                    }

                    case EventType.Timeout: {
                        // Reset to reuse connection
                        ActionCache.Add(() => {
                            var conn = connections[netEvent.Peer.ID];
                            callbacks.OnDisconnect(conn);
                            conn.Reset();
                        });
                        break;
                    }

                    case EventType.Receive: {
                        // get data
                        Buffer buffer = new Buffer(netEvent.Packet.Length);
                        netEvent.Packet.CopyTo(buffer);

                        // get message
                        NetworkMessage message = MessagePacker.DeserializeMessage(buffer, out byte index);

                        // add to cache
                        ActionCache.Add(() => {
                            var conn = connections[netEvent.Peer.ID];
                            callbacks.OnReceive(conn);
                            receivers[index](conn, message);
                        });

                        // dispose
                        netEvent.Packet.Dispose();
                        break;
                    }
                }
            }
        }

        public void Stop() {
            if (!ready) return;
            ready = false;
        }

        #region Utility

        public void AddReceiver<T>(Receiver receiver) where T : struct, NetworkMessage {
            AddReceiverInternal(typeof(T), receiver);
        }

        internal void AddReceiverInternal(Type type, Receiver receiver) {
            if (receiver == null) throw new ArgumentNullException("Receiver");

            byte index = SerializerStorage.GetMessageIndex(type);
            if (receivers.ContainsKey(index)) throw new Exception($"Receiver {index} already registered.");
            receivers.Add(index, receiver);
        }

        private void PopulateConnections(byte count) {
            connections = new Connection[count];
            for (int i = 0; i < count; i++) {
                connections[i] = new Connection();
            }
        }

        #endregion
    }
}
