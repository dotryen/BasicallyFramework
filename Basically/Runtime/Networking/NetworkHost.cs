using System;
using System.Collections;
using System.Collections.Generic;
using Basically.Serialization;
using Basically.Utility;
using Buffer = Basically.Serialization.Buffer;

namespace Basically.Networking {
    using ENet;

    public class NetworkHost {
        private Host host;
        private Connection[] connections;
        private Dictionary<byte, Receiver> receivers;

        private readonly int channelLimit = 0xFF;
        private bool ready;

        public event Action<Connection> OnConnect;
        public event Action<Connection> OnDisconnect;

        public Connection[] Connections => connections;

        public NetworkHost() {
            NetworkUtility.Initialize();
            host = new Host();
            receivers = new Dictionary<byte, Receiver>();
        }

        public NetworkHost(int channels) : this() {
            channelLimit = channels;
        }

        ~NetworkHost() {
            
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
            ready = true;
        }

        public void StartHost(ushort port, byte maxPlayers) {
            Address add = new Address {
                Port = port
            };

            PopulateConnections(maxPlayers);

            host.Create(add, connections.Length);
            ready = true;
        }

        public void Update() {
            if (!ready) return;

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

                    case EventType.Connect:
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
                                OnConnect?.Invoke(connections[netEvent.Peer.ID]);
                                break;
                            }
                        }

                        // disconnect if failed
                        netEvent.Peer.Disconnect(1);
                        break;

                    case EventType.Disconnect:
                        // Reset to reuse connection
                        connections[netEvent.Peer.ID].Reset();
                        break;

                    case EventType.Timeout:
                        // Reset to reuse connection
                        connections[netEvent.Peer.ID].Reset();
                        break;

                    case EventType.Receive:
                        Buffer buffer = new Buffer(netEvent.Packet.Length);
                        netEvent.Packet.CopyTo(buffer);

                        NetworkMessage message = MessagePacker.DeserializeMessage(buffer, out byte index);
                        receivers[index](connections[netEvent.Peer.ID], message);
                        netEvent.Packet.Dispose();
                        break;
                }
            }
        }

        public void Stop() {
            if (connections == null) return;
            foreach (var conn in connections) {
                conn.Disconnect();
            }
            ready = false;
        }

        public void AddReceiver(Type message, Receiver receiver) {
            if (!TypeUtility.IsBaseType(typeof(NetworkMessage), message)) throw new ArgumentException("Message type is not a NetworkMessage.");
            AddReceiverInternal(message, receiver);
        }

        private void AddReceiverInternal(Type type, Receiver receiver) {
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
    }
}
