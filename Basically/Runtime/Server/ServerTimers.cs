#if BASICALLY_SERVER

using System;
using System.Collections;
using System.Collections.Generic;
using System.Timers;

namespace Basically.Server {
    using Networking;

    /// <summary>
    /// Used for handshake.
    /// </summary>
    internal static class ServerTimers {
        static Dictionary<int, Timer> timers = new Dictionary<int, Timer>();

        public static void Add(int key, int seconds, Action act) {
            Timer timer = new Timer();
            timer.Interval = seconds * 1000;
            timer.AutoReset = false;
            timer.Elapsed += (src, e) => {
                act();
                timers.Remove(key);
            };

            timer.Start();
            timers.Add(key, timer);
        }

        public static void Stop(int key) {
            timers[key].Stop();
            timers.Remove(key);
        }
    }
}

#endif