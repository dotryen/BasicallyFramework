#if BASICALLY_CLIENT
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Basically.Client {
    using Utility;
    using Entities;
    using Networking;

    public static class Interpolation {
        internal struct InterpState {
            public float localTimestamp;
            public float RemoteTimestamp => world.TickMS;
            public WorldSnapshot world;

            public InterpState(float time, WorldSnapshot snap) {
                this.localTimestamp = time;
                world = snap;
                for (int i = 0; i < snap.ids.Length; i++) {
                    world.states[i].Tick = snap.tick;
                }
            }
        }

        static SortedList<uint, InterpState> buffer;
        static SortedList<ushort, PredictableEntity> predictSort;
        static float interpTime;

        internal static SortedList<uint, InterpState> Buffer => buffer;

        internal static void Initialize() {
            buffer = new SortedList<uint, InterpState>();
            interpTime = 0;
        }

        internal static void AddState(WorldSnapshot snap) {
            if (buffer.Count > 64) return; // piss off
            if (buffer.Count == 1 && snap.TickMS <= buffer.Values[0].RemoteTimestamp) return;
            if (buffer.Count >= 2 && snap.TickMS <= buffer.Values[1].RemoteTimestamp) return;
            if (buffer.ContainsKey(snap.tick)) return;

            buffer.Add(snap.tick, new InterpState(Time.time, snap));
        }

        public static void Update(float deltaTime) {
            float threshold = Time.time - BGlobals.TICK;
            if (!HasAmountOlderThan(threshold, 2)) return;

            float catchup = CalculateCatchup(4, 0.10f);
            deltaTime *= 1 + catchup;

            interpTime += deltaTime;

            GetFirstSecondAndDelta(out var from, out var to, out float alpha);

            while (interpTime >= alpha && HasAmountOlderThan(threshold, 3)) {
                interpTime -= alpha;
                buffer.RemoveAt(0);
                GetFirstSecondAndDelta(out from, out to, out alpha);
            }

            float t = Mathf.InverseLerp(from.RemoteTimestamp, to.RemoteTimestamp, from.RemoteTimestamp + interpTime);

            foreach (var ent in EntityManager.Entities.Values) {
                ent.Interpolate(from.world.states[ent.ID], to.world.states[ent.ID], t);
            }

            if (!HasAmountOlderThan(threshold, 3)) {
                interpTime = Mathf.Min(interpTime, alpha);
            }
        }

        #region Helpers

        static bool HasAmountOlderThan(float threshold, int amount) {
            return buffer.Count >= amount && buffer.Values[amount - 1].localTimestamp <= threshold;
        }

        static float CalculateCatchup(int catchupThreshold, float catchupMultiplier)  {
            // NOTE: we count ALL buffer entires > threshold as excess.
            //       not just the 'old enough' ones.
            //       if buffer keeps growing, we have to catch up no matter what.
            int excess = buffer.Count - catchupThreshold;
            return excess > 0 ? excess * catchupMultiplier : 0;
        }

        static void GetFirstSecondAndDelta(out InterpState first, out InterpState second, out float delta) {
            // get first & second
            first = buffer.Values[0];
            second = buffer.Values[1];

            // delta between first & second is needed a lot
            // USES REMOTE SO TICKS
            delta = second.world.TickMS - first.world.TickMS;
        }

        #endregion
    }
}

#endif