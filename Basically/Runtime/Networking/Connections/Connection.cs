using System.Collections;
using System.Collections.Generic;
using Basically.Serialization;

namespace Basically.Networking {
    using Utility;

    public abstract class Connection {
        public bool Connected => Status == ConnectionStatus.WaitingForHandshake || Authenticated;
        public bool Authenticated => Status == ConnectionStatus.Connected;
        public ConnectionStatus Status { get; internal set; } = ConnectionStatus.NotConnected;
        public byte ID { get; internal set; }
        public abstract string IP { get; }
        public abstract uint Ping { get; }

        internal ThreadTasks threadToQueue;

        internal Connection() {
            Status = ConnectionStatus.WaitingForHandshake;
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

            threadToQueue.Add(() => {
                var writer = Pool<Writer>.Pull();
                Packer.Pack(message, writer);
                SendInternal(writer.ToArray(), channel, type);
                Pool<Writer>.Push(writer);
            });
        }

        internal void SendWithoutQueue<T>(T message, byte channel, MessageType type) where T : struct, NetworkMessage {
            if (!Connected) return;

            var writer = Pool<Writer>.Pull();
            Packer.Pack(message, writer);
            SendInternal(writer.ToArray(), channel, type);
            Pool<Writer>.Push(writer);
        }

        /// <summary>
        /// Disconnects the user.
        /// </summary>
        /// <param name="data">Additional data to be sent.</param>
        public void Disconnect(ushort data = 0) {
            if (!Connected) return;
            DoDisconnect(data);
            Status = ConnectionStatus.NotConnected;
        }

        internal abstract void SendInternal(byte[] payload, byte channel, MessageType type);

        internal abstract void DoDisconnect(ushort data);
    }
}
