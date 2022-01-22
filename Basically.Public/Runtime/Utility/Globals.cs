using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Basically.Utility {
    public static class BGlobals {
        /// <summary>
        /// Is Basically running as a host? (client and server)
        /// </summary>
        public static bool IsHost { get; internal set; }

        /// <summary>
        /// Is Basically running as a server?
        /// </summary>
        public static bool IsServer { get; internal set; }

        /// <summary>
        /// Is Basically running as a client?
        /// </summary>
        public static bool IsClient { get; internal set; }

        /// <summary>
        /// Current tick.
        /// </summary>
        public static uint Tick { get; internal set; }

        /// <summary>
        /// Predicted server tick.
        /// </summary>
        public static uint PredictedTick { get; internal set; }

        /// <summary>
        /// Currect tick in milliseconds.
        /// </summary>
        public static float TickMS => Tick * TICK;

        #region Constants

        /// <summary>
        /// Tick rate the framework will work on.
        /// </summary>
        public const int TICK_RATE = 64;

        /// <summary>
        /// The amount of snapshots being sent/received per second.
        /// </summary>
        public const int SNAPSHOTS_PER_SECOND = TICK_RATE / 4;

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

