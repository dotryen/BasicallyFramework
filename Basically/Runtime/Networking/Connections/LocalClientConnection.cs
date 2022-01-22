#if BASICALLY_CLIENT

using System;
using System.Collections;
using System.Collections.Generic;

namespace Basically.Networking {
    using Client;
    using Utility;
    using Serialization;

    /// <summary>
    /// Connection to local client
    /// </summary>
    public class LocalClientConnection : Connection {
        public override string IP => "127.0.0.1";

        public override uint Ping => 0;

        internal override void DoDisconnect(ushort data) {
            NetworkClient.handler.OnDisconnect(NetworkClient.connection, data);
        }

        internal override void SendInternal(byte[] payload, byte channel, MessageType type) {
            var reader = Pool<Reader>.Pull();
            Array.Copy(payload, reader.ToArray(), payload.Length);

            if (Packer.Unpack(reader, out var header)) {
                if (!NetworkClient.handler.OnReceive(NetworkClient.connection, reader, header)) {
                    Pool<Reader>.Push(reader);
                }
            }
        }
    }
}

#endif