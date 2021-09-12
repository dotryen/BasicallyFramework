using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Basically.Networking {
    using ENet;

    public class ENetConnection : Connection {
        private Peer peer;

        public override string IP => peer.IP;
        public override uint Ping => peer.RoundTripTime / 2;

        internal ENetConnection(Peer peer) {
            this.peer = peer;
        }

        internal override void DoDisconnect(ushort data) {
            peer.Disconnect(data);
        }

        internal override void SendInternal(byte[] payload, byte channel, MessageType type) {
            Packet packet = default;
            packet.Create(payload, (PacketFlags)type);
            peer.Send(channel, ref packet);
        }
    }
}
