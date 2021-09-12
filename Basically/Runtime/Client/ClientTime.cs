#if BASICALLY_CLIENT

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Basically.Client {
    using Networking;
    using Entities;

    public static class ClientTime {
        internal static bool ready;
        internal static uint tick;

        /// <summary>
        /// Current tick.
        /// </summary>
        public static uint Tick => tick;

        /// <summary>
        /// Tick the server is expected to be at.
        /// </summary>
        public static uint PredictedTick { get {
                if (NetworkClient.Connected) {
                    return (uint)(tick + Mathf.FloorToInt(NetworkClient.Connection.Ping / NetworkTiming.TICK));
                }
                return tick;
            }
        }

        internal static void Initialize(WorldSnapshot snapshot) {
            if (ready) return;
            tick = snapshot.tick;
            ready = true;
        }

        public static void Simulate() {
            EntityManager.ClientTick();
#if PHYS_3D
            Physics.Simulate(Time.fixedDeltaTime);
#endif
#if PHYS_2D
            Physics2D.Simulate(Time.fixedDeltaTime);
#endif

            tick++;
        }
    }
}

#endif