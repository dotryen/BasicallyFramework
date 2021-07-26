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

        public static Receiver GetDelegate(MethodInfo info) {
            var connectionArg = Expression.Parameter(typeof(Connection));
            var messageArg = Expression.Parameter(typeof(NetworkMessage));
            var body = Expression.Call(null, info, connectionArg, Expression.Convert(messageArg, info.GetParameters()[1].ParameterType));
            var lambda = Expression.Lambda<Receiver>(body, connectionArg, messageArg);
            return lambda.Compile();
        }

        public static bool VerifyMethod(MethodInfo info) {
            var param = info.GetParameters();
            if (param.Length != 2) return false;
            if (param[0].ParameterType != typeof(Connection)) return false;
            if (!TypeUtility.IsBaseType(typeof(NetworkMessage), param[1].ParameterType)) return false;
            return true;
        }

        internal static void Log(object message) {
            Debug.Log("NETWORKING LOG: " + message);
        }

        internal static void LogWarning(object message) {
            Debug.LogWarning("NETWORKING WARNING: " + message);
        }

        internal static void LogError(object message) {
            Debug.LogError("NETWORKING ERROR: " + message);
        }
    }
}
