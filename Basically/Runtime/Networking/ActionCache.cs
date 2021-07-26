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
        static ConcurrentQueue<Action> toExecute;

        public static void Initialize() {
            toExecute = new ConcurrentQueue<Action>();
        }

        public static void Add(Action action) {
            if (action == null) throw new ArgumentNullException("Action");

            toExecute.Enqueue(action);
        }

        public static void Execute() {
            while (toExecute.TryDequeue(out Action act)) {
                act();
            }
        }
    }
}
