using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;

namespace Basically.Networking {
    using Serialization;
    using Utility;

    public class MethodHandler {
        private readonly Dictionary<ushort, MessageDelegate> handlers;
        private HostCallbacks callbacks;

        public MethodHandler() {
            handlers = new Dictionary<ushort, MessageDelegate>();
        }

        public MethodHandler(HostCallbacks callbacks) : this() {
            this.callbacks = callbacks;
        }

        internal void OnConnect(Connection connection) {
            ThreadData.AddUnity(() => {
                callbacks?.OnConnect(connection);
            });
        }

        internal void OnDisconnect(Connection connection, uint data) {
            ThreadData.AddUnity(() => {
                callbacks?.OnDisconnect(connection, data);
            });
        }

        internal void OnTimeout(Connection connection) {
            ThreadData.AddUnity(() => { callbacks?.OnTimeout(connection); });
        }

        internal bool OnReceive(Connection connection, Reader reader, ushort header) {
            if (handlers.TryGetValue(header, out var del)) {
                ThreadData.AddUnity(() => {
                    callbacks?.OnReceive(connection);
                    del(connection, reader);
                    Pool<Reader>.Push(reader);
                });
                return true; 
            }
            return false;
        }

        public void AddReceiver<T>(Action<Connection, T> handler, bool requireAuth = true) where T : struct, NetworkMessage {
            ushort header = Packer.GetId<T>();
            handlers[header] = Packer.Wrap(handler, requireAuth);
        }

        public void AddReceiverClass(Type type) {
            var attr = (ReceiverClassAttribute)type.GetCustomAttributes(typeof(ReceiverClassAttribute)).FirstOrDefault();
            if (attr == null) return;

            var method = type.GetMethod("_Init", BindingFlags.Public | BindingFlags.Static);
            method.Invoke(null, new object[] { this });
            // NetworkUtility.Log($"{type.FullName} initialized.");
        }
    }
}
