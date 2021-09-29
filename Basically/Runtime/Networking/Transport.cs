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

    public class Transport {
        private Host host;
        private Connection[] connections;
        private Dictionary<uint, byte> peerToConn;
        private MethodHandler handler;

        // THREADING
        private bool ready;
        private Thread networkThread;

        // POOL KEYS
        private object readerKey;
        private object writerKey;

        const int MAXIMUM_SIZE = 4 * 1024;

        public Connection[] Connections => connections;
        public byte ConnectedPlayers { get {
                return (byte)connections.Where((x) => {
                    if (x != null) {
                        if (x.Authenticated) return true;
                    }
                    return false;
                }).Count();
            } 
        }
        public MethodHandler MethodHandler => handler;

        /// <summary>
        /// Creates a new host.
        /// </summary>
        public Transport() {
            NetworkUtility.Initialize();
            ThreadData.Initialize();
            InitializePools();
            host = new Host();
            handler = new MethodHandler(new HostCallbacks());
            peerToConn = new Dictionary<uint, byte>();
        }

        /// <summary>
        /// Creates a host with custom settings.
        /// </summary>
        /// <param name="handler">Handler to use, MUST NOT BE NULL.</param>
        public Transport(MethodHandler handler) {
            NetworkUtility.Initialize();
            ThreadData.Initialize();
            InitializePools();
            host = new Host();
            this.handler = handler;
            peerToConn = new Dictionary<uint, byte>();
        }

        ~Transport() {
            Pool<Reader>.Dispose(readerKey);
            Pool<Writer>.Dispose(writerKey);
            NetworkUtility.Deinitialize();
        }

        /// <summary>
        /// Connects to other host.
        /// </summary>
        /// <param name="ip">IP to connect to.</param>
        /// <param name="port">Port to use.</param>
        public void ConnectToHost(string ip, ushort port) {
            if (ip == "localhost") ip = "127.0.0.1";

            Address add = new Address {
                Port = port
            };
            add.SetHost(ip);

            connections = new Connection[1];

            host.Create(1, 0xFF);
            host.Connect(add);
            StartThread();
        }

        /// <summary>
        /// Starts a host.
        /// </summary>
        /// <param name="port">Port to use.</param>
        /// <param name="maxPlayers">Max players/connections.</param>
        public void StartHost(ushort port, byte maxPlayers) {
            Address add = new Address {
                Port = port
            };

            connections = new Connection[maxPlayers];

            host.Create(add, maxPlayers);
            StartThread();
        }

        /// <summary>
        /// Requests the networking thread to end.
        /// </summary>
        public void Stop() {
            ready = false;
        }

        /// <summary>
        /// Broadcasts the message to every player.
        /// </summary>
        /// <typeparam name="T">Message type.</typeparam>
        /// <param name="message">The message.</param>
        /// <param name="channel">Which channel to send the message through.</param>
        /// <param name="type">What type of message is this.</param>
        public void Broadcast<T>(T message, byte channel, MessageType type) where T : struct, NetworkMessage {
            if (ConnectedPlayers == 0) return;

            ThreadData.AddNet(() => {
                var writer = Pool<Writer>.Pull();
                Packer.Pack(message, writer);
                var payload = writer.ToArray();
                Pool<Writer>.Push(writer);

                foreach (var conn in connections) {
                    if (conn == null) continue;
                    if (!conn.Connected) continue;

                    conn.SendInternal(payload, channel, type);
                }
            });
        }

        /// <summary>
        /// Broadcasts the message to every player.
        /// </summary>
        /// <typeparam name="T">Message type.</typeparam>
        /// <param name="message">The message.</param>
        /// <param name="channel">Which channel to send the message through.</param>
        /// <param name="type">What type of message is this.</param>
        /// <param name="excluding">ID of connection to exclude.</param>
        public void Broadcast<T>(T message, byte channel, MessageType type, byte excluding) where T : struct, NetworkMessage {
            if (ConnectedPlayers == 0) return;

            ThreadData.AddNet(() => {
                var writer = Pool<Writer>.Pull();
                Packer.Pack(message, writer);
                var payload = writer.ToArray();
                Pool<Writer>.Push(writer);

                foreach (var conn in connections) {
                    if (conn == null) continue;
                    if (!conn.Connected) continue;
                    if (conn.ID == excluding) continue;

                    conn.SendInternal(payload, channel, type);
                }
            });
        }

        /// <summary>
        /// Broadcasts the message to every player.
        /// </summary>
        /// <typeparam name="T">Message type.</typeparam>
        /// <param name="message">The message.</param>
        /// <param name="channel">Which channel to send the message through.</param>
        /// <param name="type">What type of message is this.</param>
        /// <param name="excluding">Connection to exclude.</param>
        public void Broadcast<T>(T message, byte channel, MessageType type, Connection excluding) where T : struct, NetworkMessage {
            Broadcast(message, channel, type, excluding.ID);
        }

        #region Loop and stuff

        private void StartThread() {
            ready = true;
            networkThread = new Thread(ThreadFunc) {
                Name = "Basically.Net"
            };
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
                if (conn == null) continue;
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
                    if (host.Service(0, out netEvent) <= 0) break;
                    polled = true;
                }

                switch (netEvent.Type) {
                    case EventType.None:
                        // do nothing lol why is this here
                        break;

                    case EventType.Connect: {
                        if (ConnectedPlayers == connections.Length || connections.Any(x => {
                            if (x == null) return false;
                            return x.IP == netEvent.Peer.IP;
                        })) {
                            netEvent.Peer.Disconnect(0);
                            break;
                        }

                        var connection = new ENetConnection(netEvent.Peer);
                        AddConnection(connection);
                        peerToConn.Add(netEvent.Peer.ID, connection.ID);
                        handler.OnConnect(connection);
                        break;
                    }

                    case EventType.Disconnect: {
                        var conn = connections[peerToConn[netEvent.Peer.ID]];
                        RemoveConnection(conn.ID);
                        peerToConn.Remove(netEvent.Peer.ID);

                        handler.OnDisconnect(conn, netEvent.Data);
                        break;
                    }

                    case EventType.Timeout: {
                        var conn = connections[peerToConn[netEvent.Peer.ID]];
                        RemoveConnection(conn.ID);
                        peerToConn.Remove(netEvent.Peer.ID);

                        handler.OnTimeout(conn);
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
                        if (Packer.Unpack(reader, out var header)) {
                            if (!handler.OnReceive(GetConnection(netEvent.Peer), reader, header)) {
                                Pool<Reader>.Push(reader);
                            }
                        }

                        // dispose
                        netEvent.Packet.Dispose();
                        break;
                    }
                }
            }
        }

        #endregion

        #region Utility

        private Connection GetConnection(Peer peer) {
            return connections[peerToConn[peer.ID]];
        }

        private void AddConnection(Connection connect) {
            for (byte i = 0; i < connections.Length; i++) {
                if (connections[i] == null) {
                    connections[i] = connect;
                    connect.ID = i;
                    return;
                }
            }
        }

        private void RemoveConnection(byte id) {
            if (id > connections.Length - 1) throw new ArgumentOutOfRangeException();
            connections[id] = null;
        }

        private void InitializePools() {
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
        }

        #endregion
    }
}
