using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Concurrent;
using UnityEngine;

namespace Basically.Networking {

    // TODO: Add message passing through ThreadData
    /// <summary>
    /// Carries actions over from different threads
    /// </summary>
    internal static class ThreadData {
        internal static ConcurrentQueue<Action> toExecuteUnity;
        internal static ConcurrentQueue<Action> toExecuteClient;
        internal static ConcurrentQueue<Action> toExecuteNet;

        public static void Initialize() {
            toExecuteUnity = new ConcurrentQueue<Action>();
            toExecuteNet = new ConcurrentQueue<Action>();
        }

        public static void AddUnity(Action action) {
            if (action == null) throw new ArgumentNullException("Action");
            toExecuteUnity.Enqueue(action);
        }

        public static void AddNet(Action action) {
            if (action == null) throw new ArgumentNullException("Action");
            toExecuteNet.Enqueue(action);
        }

        public static void ExecuteUnity() {
            while (toExecuteUnity.TryDequeue(out Action act)) {
                act();
            }
        }

        public static void ExecuteNet() {
            while (toExecuteNet.TryDequeue(out Action act)) {
                act();
            }
        }
    }
}
