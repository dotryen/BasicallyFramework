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
        private readonly HostCallbacks callbacks;
        private readonly ThreadTasks tasks;

        public ThreadTasks Tasks => tasks;

        public MethodHandler() {
            handlers = new Dictionary<ushort, MessageDelegate>();
            tasks = new ThreadTasks();
        }

        public MethodHandler(HostCallbacks callbacks) : this() {
            this.callbacks = callbacks;
        }

        internal void OnConnect(Connection connection) {
            tasks.Add(() => {
                callbacks?.OnConnect(connection);
            });
        }

        internal void OnDisconnect(Connection connection, uint data) {
            tasks.Add(() => {
                callbacks?.OnDisconnect(connection, data);
            });
        }

        internal void OnTimeout(Connection connection) {
            tasks.Add(() => { callbacks?.OnTimeout(connection); });
        }

        internal bool OnReceive(Connection connection, Reader reader, ushort header) {
            if (handlers.TryGetValue(header, out var del)) {
                tasks.Add(() => {
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
            // NetworkUtility.Log($"{type.FullName} initialized."); // causes exception
        }
    }
}
