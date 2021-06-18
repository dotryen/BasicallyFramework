using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Basically.Networking {
    public static class NetworkTiming {
        public const int TICK_RATE = 64;
        public const float TICK = 1f / TICK_RATE;
        public const int SNAPSHOTS_PER_SECOND = TICK_RATE / 4;
        public const float STATE_INTERVAL = TICK * (TICK_RATE / SNAPSHOTS_PER_SECOND);
        public const float INTERP_TIME = STATE_INTERVAL * 2;
        /// <summary>
        /// MUST BE FLOORED AT RUNTIME BECAUSE C# IS A BITCH
        /// </summary>
        public const int INTERP_TIME_TICK = (int)(INTERP_TIME / TICK);
    }
}
