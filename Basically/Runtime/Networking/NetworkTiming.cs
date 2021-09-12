using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Basically.Networking {
    public static class NetworkTiming {
        #region Input

        /// <summary>
        /// Tick rate the framework will work on.
        /// </summary>
        public const int TICK_RATE = 64;

        /// <summary>
        /// The amount of snapshots being sent/received per second.
        /// </summary>
        public const int SNAPSHOTS_PER_SECOND = TICK_RATE / 4;

        #endregion

        #region Output

        /// <summary>
        /// Time of a single tick.
        /// </summary>
        public const float TICK = 1f / TICK_RATE;

        /// <summary>
        /// Time between snapshots. sent/received.
        /// </summary>
        public const float SNAPSHOT_INTERVAL = TICK * STATE_TICKS_SKIPPED;

        /// <summary>
        /// Amount of ticks skipped between snapshots. (Should be a nice even number)
        /// </summary>
        public const int STATE_TICKS_SKIPPED = TICK_RATE / SNAPSHOTS_PER_SECOND;

        #endregion
    }
}
