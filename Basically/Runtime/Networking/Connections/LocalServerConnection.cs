#if BASICALLY_SERVER

using System;
using System.Collections;
using System.Collections.Generic;

namespace Basically.Networking {
    using Server;
    using Utility;
    using Serialization;

    /// <summary>
    /// Connection to local server
    /// </summary>
    public class LocalServerConnection : Connection {
        public override string IP => "127.0.0.1";

        public override uint Ping => 0;

        internal override void DoDisconnect(ushort data) {
            NetworkServer.handler.OnDisconnect(NetworkServer.Connections[ID], data);
        }

        internal override void SendInternal(byte[] payload, byte channel, MessageType type) {
            var reader = Pool<Reader>.Pull();
            Array.Copy(payload, reader.ToArray(), payload.Length);

            if (Packer.Unpack(reader, out var header)) {
                if (!NetworkServer.handler.OnReceive(NetworkServer.Connections[ID], reader, header)) {
                    Pool<Reader>.Push(reader);
                }
            }
        }
    }
}

#endif