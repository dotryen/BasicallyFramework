using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Concurrent;

namespace Basically.Utility {
    public class ThreadTasks {
        private ConcurrentQueue<Action> tasks;

        public int TaskCount => tasks.Count;

        public ThreadTasks() {
            tasks = new ConcurrentQueue<Action>();
        }

        public void Add(Action action) {
            tasks.Enqueue(action);
        }

        public void Execute() {
            while (tasks.TryDequeue(out Action act)) {
                act();
            }
        }
    }
}
