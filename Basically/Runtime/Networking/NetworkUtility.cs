using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Linq.Expressions;
using UnityEngine;
using Basically.Utility;

namespace Basically.Networking {
    using ENet;

    public static class NetworkUtility {
        static bool initialized;

        internal static void Initialize() {
            if (initialized) return;
            Library.Initialize();
            initialized = true;
        }

        internal static void Deinitialize() {
            if (!initialized) return;
            Library.Deinitialize();
            initialized = false;
        }

        public static int MSToTicks(float ms) => Mathf.FloorToInt(ms / (NetworkTiming.TICK * 1000f));

        internal static void Log(object message) {
            ThreadData.AddUnity(() => {
                Debug.Log("NETWORKING LOG: " + message);
            });
        }

        internal static void LogWarning(object message) {
            ThreadData.AddUnity(() => {
                Debug.LogWarning("NETWORKING WARNING: " + message);
            });
        }

        internal static void LogError(object message) {
            ThreadData.AddUnity(() => {
                Debug.LogError("NETWORKING ERROR: " + message);
            });
        }
    }
}
