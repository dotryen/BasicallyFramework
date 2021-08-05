using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using UnityEngine;

namespace Basically.Utility {
    public static class Pool<T> {
        static ConcurrentBag<T> bag;
        static object key;
        static Func<T> ctor;
        static Action<T> reset;

        public static object Create(int count, Func<T> constructor, Action<T> res) {
            if (key != null) return null;

            bag = new ConcurrentBag<T>();
            key = new object();
            ctor = constructor;
            reset = res;

            for (int i = 0; i < count; i++) {
                bag.Add(ctor());
            }
            
            return key;
        }

        public static void Dispose(object testKey) {
            if (!ReferenceEquals(key, testKey)) return;
            bag = null;
            key = null;
            ctor = null;
            reset = null;
        }

        public static T Pull() {
            if (bag.TryTake(out T result)) {
                return result;
            } else {
                return ctor();
            }
        }

        public static void Push(T item) {
            reset(item);
            bag.Add(item);
        }
    }
}
