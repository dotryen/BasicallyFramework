using System;
using System.Collections;
using System.Collections.Generic;
using Basically.Serialization;
using Buffer = Basically.Serialization.Buffer;

namespace Basically.Networking {
    using ENet;

    public sealed class Connection {
        internal Peer peer;

        public bool Connected { get; internal set; }
        public byte ID => (byte)peer.ID;
        public uint Ping => peer.RoundTripTime / 2;

        internal Connection() {

        }

        internal void Setup(Peer peer) {
            this.peer = peer;
            Connected = true;
        }

        internal void Reset() {
            peer = default;
            Connected = false;
        }

        public void Send<T>(T message, byte channel, PacketType type) where T : NetworkMessage {
            if (!Connected) return;

            SendInternal(MessagePacker.SerializeMessage(message), channel, (PacketFlags)type);
        }

        internal void SendInternal(byte[] payload, byte channel, PacketFlags type) {
            Packet packet = default;
            packet.Create(payload, type);
            peer.Send(channel, ref packet);
        }

        public void Disconnect(byte data = 0) {
            if (!Connected) return;
            peer.Disconnect(data);
        }
    }
}
