using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System.Threading;

namespace Basically.Networking {
    using ENet;
    using Utility;
    using Serialization;

    public class NetworkHost {
        private Host host;
        private Connection[] connections;
        private Dictionary<ushort, MessageDelegate> handlers;
        internal HostCallbacks callbacks;

        private readonly int channelLimit;
        private bool ready;
        private Thread networkThread;

        private object readerKey;
        private object writerKey;

        const int MAXIMUM_SIZE = 4 * 1024;

        public Connection[] Connections => connections;

        /// <summary>
        /// Creates a new host with default settings.
        /// </summary>
        public NetworkHost() {
            SharedConstruct();

            channelLimit = 0xFF;
            callbacks = new HostCallbacks();
        }

        /// <summary>
        /// Creates a host with a custom channel count.
        /// </summary>
        /// <param name="channels">Channel limit. (Max is 0xFF)</param>
        public NetworkHost(int channels) {
            SharedConstruct();

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

            SharedConstruct();

            channelLimit = channels;
            this.callbacks = callbacks;
            this.callbacks.Host = this;
        }

        private void SharedConstruct() {
            NetworkUtility.Initialize();
            ThreadData.Initialize();

            // initialize pools
            readerKey = Pool<Reader>.Create(5, () => {
                return new Reader(MAXIMUM_SIZE);
            }, (reader) => {
                reader.Reset();
            });

            writerKey = Pool<Writer>.Create(5, () => {
                return new Writer(MAXIMUM_SIZE);
            }, (writer) => {
                writer.Reset();
            });

            host = new Host();
            handlers = new Dictionary<ushort, MessageDelegate>();
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
                ThreadData.ExecuteNet();
                Update();
                // NetworkUtility.Log("Host update ran"); // FUCK THIS LINE, IT LITERALLY FREEZES UNITY AND ATTEMPTS TO DESTROY MY COMPUTER
            }

            // disconnect everyone
            foreach (var conn in connections) {
                conn.Disconnect();
            }
            host.Flush();

            // create new host
            host.Dispose();
            host = new Host();

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

                                ThreadData.AddUnity(() => {
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
                        ThreadData.AddUnity(() => {
                            var conn = connections[netEvent.Peer.ID];
                            callbacks.OnDisconnect(conn, netEvent.Data);
                            conn.Reset();
                        });
                        break;
                    }

                    case EventType.Timeout: {
                        // Reset to reuse connection
                        ThreadData.AddUnity(() => {
                            var conn = connections[netEvent.Peer.ID];
                            callbacks.OnTimeout(conn);
                            conn.Reset();
                        });
                        break;
                    }

                    case EventType.Receive: {
                        if (netEvent.Packet.Length > MAXIMUM_SIZE) {
                            NetworkUtility.LogError("Packet received is too large for readers inside pool.");
                            // no backup, for now at least
                            return;
                        }

                        // get data
                        Reader reader = Pool<Reader>.Pull();
                        netEvent.Packet.CopyTo(reader.ToArray());

                        // get message
                        if (!Packer.Unpack(reader, out ushort header)) break;

                        if (handlers.TryGetValue(header, out var del)) {
                            ThreadData.AddUnity(() => {
                                var conn = connections[netEvent.Peer.ID];
                                callbacks.OnReceive(conn);
                                del(conn, reader);
                                Pool<Reader>.Push(reader);
                            });
                        } else {
                            Pool<Reader>.Push(reader);
                        }

                        // dispose
                        netEvent.Packet.Dispose();
                        break;
                    }
                }
            }
        }

        public void Stop() {
            ready = false;
        }

        #region Utility

        public void AddReceiver<T>(Action<Connection, T> handler, bool requireAuth = true) where T : struct, NetworkMessage {
            ushort header = Packer.GetId<T>();
            handlers[header] = Packer.Wrap(handler, requireAuth);
        }

        public void AddReceiverClass(Type type) {
            var attr = (ReceiverClassAttribute)type.GetCustomAttributes(typeof(ReceiverClassAttribute)).FirstOrDefault();
            if (attr == null) return;

            var method = type.GetMethod("_Init", BindingFlags.Public | BindingFlags.Static);
            method.Invoke(null, new object[] { this });
            NetworkUtility.Log($"{type.FullName} initialized.");
        }

        private void PopulateConnections(byte count) {
            connections = new Connection[count];
            for (int i = 0; i < count; i++) {
                connections[i] = new Connection {
                    host = this
                };
            }
        }

        #endregion
    }
}
