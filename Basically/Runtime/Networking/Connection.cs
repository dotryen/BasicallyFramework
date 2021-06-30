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
        public string IP => peer.IP;
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

        /// <summary>
        /// Sends the user the message.
        /// </summary>
        /// <typeparam name="T">Type of message.</typeparam>
        /// <param name="message">The message to send.</param>
        /// <param name="channel">Which channel to send through.</param>
        /// <param name="type">How to send it.</param>
        public void Send<T>(T message, byte channel, MessageType type) where T : struct, NetworkMessage {
            if (!Connected) return;
            SendInternal(MessagePacker.SerializeMessage(message), channel, (PacketFlags)type);
        }

        /// <summary>
        /// Faster than Send<> but bypasses checks, should only be used by Basically.
        /// </summary>
        /// <param name="payload"></param>
        /// <param name="channel"></param>
        /// <param name="type"></param>
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
