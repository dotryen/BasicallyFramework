using System.Collections;
using System.Collections.Generic;
using System.Collections.Concurrent;

namespace Basically.Serialization {
    public static class BufferPool {
        // can be changed to fit the game
        public const int BUFFER_SIZE = 4 * 1024;
        private static ConcurrentBag<Buffer> pool;

        /// <summary>
        /// Creates the Pool.
        /// </summary>
        public static void Initialize() {
            pool = new ConcurrentBag<Buffer>() {
                new Buffer(BUFFER_SIZE),
                new Buffer(BUFFER_SIZE)
            };
        }

        /// <summary>
        /// Get a buffer from the pool. Buffer must be returned to minimize allocations.
        /// </summary>
        /// <returns>A buffer.</returns>
        public static Buffer Get() {
            if (pool.TryTake(out Buffer buf)) {
                return buf;
            } else {
                return new Buffer(BUFFER_SIZE);
            }
        }

        /// <summary>
        /// Adds a buffer to the pool.
        /// </summary>
        /// <param name="buffer">Buffer to add.</param>
        public static void Return(Buffer buffer) {
            buffer.Reset();
            pool.Add(buffer);
        }
    }
}
