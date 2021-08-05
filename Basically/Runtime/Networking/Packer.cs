using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Basically.Networking {
    using Serialization;
    using Utility;
    using ENet;

    internal static class Packer {
        public static ushort GetId<T>() {
            return (ushort)(typeof(T).FullName.GetStableHashCode() & 0xFFFF);
        }

        public static void Pack<T>(T message, Writer writer) where T : struct , NetworkMessage {
            // Debug.Log($"Packing {typeof(T).FullName}");
            ushort header = GetId<T>();
            writer.WriteUShort(header);
            writer.Write(message);
        }

        public static bool Unpack(Reader reader, out ushort header) {
            try {
                header = reader.ReadUShort();
                return true;
            } catch (Exception ex) {
                header = 0;
                Debug.LogError(ex);
                return false;
            }
        }

        public static MessageDelegate Wrap<T>(Action<Connection, T> handler, bool requireAuth) where T : struct, NetworkMessage {
            return (conn, reader) => {
                T message = default;
                try {
                    if (requireAuth && !conn.Authenticated) {
                        conn.Disconnect();
                    }

                    message = reader.Read<T>();
                } catch (Exception ex) {
                    Debug.LogError(ex);
                    conn.Disconnect();
                    return;
                }

                try {
                    handler(conn, message);
                } catch (Exception ex) {
                    Debug.LogError(ex);
                    return;
                }
            };
        }
    }

    public class DeltaKey {
        readonly ushort key;
        public ushort Key => key;

        internal DeltaKey(ushort key) {
            this.key = key;
        }
    }
}
