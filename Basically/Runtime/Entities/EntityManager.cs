using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Basically.Entities {
    using Utility;
    using Serialization;
    using Networking;

    public static class EntityManager {
        internal struct InterpState {
            public float LocalTimestamp;
            public float RemoteTimestamp => world.TickMS;
            public WorldSnapshot world;

            public InterpState(float time, WorldSnapshot snap) {
                LocalTimestamp = time;
                world = snap;

                for (int i = 0; i < snap.ids.Length; i++) {
                    world.states[i].Tick = snap.tick;
                }
            }
        }

        private static Dictionary<ushort, Entity> entities;
        private static Dictionary<ushort, PredictableEntity> predictiableEntities;

        private static SortedList<uint, InterpState> timeBuffer;
        private static float interpolationTime;
        private static bool rewinding;
        private static WorldSnapshot worldBeforeRewind;

        public static Dictionary<ushort, Entity> Entities => entities;
        public static Dictionary<ushort, PredictableEntity> PredictableEntities => predictiableEntities;
        internal static SortedList<uint, InterpState> TimeBuffer => timeBuffer;
        internal static WorldSnapshot LatestRecord => timeBuffer.Values[timeBuffer.Count - 1].world;

        #region Gathering Entities

        public static void OnLoad() {
            entities = new Dictionary<ushort, Entity>();
            predictiableEntities = new Dictionary<ushort, PredictableEntity>();

            timeBuffer = new SortedList<uint, InterpState>();
            interpolationTime = 0;
            rewinding = false;
            worldBeforeRewind = default;

            foreach (var ent in Object.FindObjectsOfType<Entity>()) {
                if (ent is PredictableEntity) RegisterEntity((PredictableEntity)ent);
                else RegisterEntity(ent);
            }
        }

        public static void RegisterEntity(PredictableEntity entity) {
            RegisterEntity((Entity)entity);
            predictiableEntities[entity.ID] = entity;
        }

        public static void RegisterEntity(Entity entity) {
            entity.ID = NextAvailableID();
            entities[entity.ID] = entity;
        }

        #endregion

        #region Interpolation

        internal static void AddSnapshot(WorldSnapshot snap) {
            if (timeBuffer.Count > BGlobals.TICK_RATE) return; // piss off
            if (timeBuffer.Count == 1 && snap.TickMS <= timeBuffer.Values[0].RemoteTimestamp) return;
            if (timeBuffer.Count >= 2 && snap.TickMS <= timeBuffer.Values[1].RemoteTimestamp) return;
            if (timeBuffer.ContainsKey(snap.tick)) return;

            timeBuffer.Add(snap.tick, new InterpState(Time.time, snap));
        }

        public static void UpdateInterpolation(float deltaTime) {
            if (rewinding) return;

            float threshold = Time.time - BGlobals.TICK;
            if (!HasAmountOlderThan(threshold, 2)) return;

            float catchup = CalculateCatchup(4, 0.10f);
            deltaTime *= 1 + catchup;

            interpolationTime += deltaTime;

            GetFirstSecondAndDelta(out var from, out var to, out float alpha);

            while (interpolationTime >= alpha && HasAmountOlderThan(threshold, 3)) {
                interpolationTime -= alpha;
                timeBuffer.RemoveAt(0);
                GetFirstSecondAndDelta(out from, out to, out alpha);
            }

            float t = Mathf.InverseLerp(from.RemoteTimestamp, to.RemoteTimestamp, from.RemoteTimestamp + interpolationTime);

            foreach (var ent in Entities.Values) {
                ent.Interpolate(from.world.states[ent.ID], to.world.states[ent.ID], t);
            }

            if (!HasAmountOlderThan(threshold, 3)) {
                interpolationTime = Mathf.Min(interpolationTime, alpha);
            }
        }

        public static void Rewind(float ms) {
            var ticks = (uint)Mathf.FloorToInt(ms / BGlobals.TICK);
            var interp = (ms % BGlobals.TICK) / BGlobals.TICK;

            if (ticks > BGlobals.TICK_RATE) return;

            worldBeforeRewind = Record();

            var fromWorld = timeBuffer[BGlobals.Tick - ticks].world;
            var toWorld = timeBuffer[BGlobals.Tick - ticks + 1].world;

            ApplySnapshot(fromWorld, toWorld, interp);

            rewinding = true;
        }

        public static void Return() {
            if (!rewinding) return;
            ApplySnapshot(worldBeforeRewind);
        }

        internal static WorldSnapshot Record() {
            var count = Entities.Count;

            WorldSnapshot snap = new WorldSnapshot() {
                tick = BGlobals.Tick,
                ids = new ushort[count],
                states = new EntityState[count]
            };

            for (ushort i = 0; i < count; i++) {
                var pair = Entities.ElementAt(i);
                IParameters param = new SerParameters(0);
                pair.Value.Serialize(ref param);

                snap.ids[i] = pair.Key;
                snap.states[i] = new EntityState {
                    position = pair.Value.Position,
                    rotation = pair.Value.Rotation,
                    parameters = param
                };
            }

            return snap;
        }

        private static void ApplySnapshot(WorldSnapshot snapshot) {
            for (ushort i = 0; i < snapshot.states.Length; i++) {
                var pair = Entities.ElementAt(i);
                var id = pair.Key;

                var state = snapshot.states[id];
                
                pair.Value.Position = state.position;
                pair.Value.Rotation = state.rotation;
                pair.Value.Deserialize(state.parameters);
            }
        }

        private static void ApplySnapshot(WorldSnapshot fromWorld, WorldSnapshot toWorld, float interp) {
            for (ushort i = 0; i < Entities.Count; i++) {
                var pair = entities.ElementAt(i);

                var from = fromWorld.states[pair.Key];
                var to = toWorld.states[pair.Key];

                pair.Value.Interpolate(from, to, interp);
                pair.Value.Deserialize(from.parameters);
            }
        }

        #region Helpers

        private static bool HasAmountOlderThan(float threshold, int amount) {
            return timeBuffer.Count >= amount && timeBuffer.Values[amount - 1].LocalTimestamp <= threshold;
        }

        private static float CalculateCatchup(int catchupThreshold, float catchupMultiplier) {
            // NOTE: we count ALL timeBuffer entires > threshold as excess.
            //       not just the 'old enough' ones.
            //       if timeBuffer keeps growing, we have to catch up no matter what.
            int excess = timeBuffer.Count - catchupThreshold;
            return excess > 0 ? excess * catchupMultiplier : 0;
        }

        private static void GetFirstSecondAndDelta(out InterpState first, out InterpState second, out float delta) {
            // get first & second
            first = timeBuffer.Values[0];
            second = timeBuffer.Values[1];

            // delta between first & second is needed a lot
            // USES REMOTE SO TICKS
            delta = second.world.TickMS - first.world.TickMS;
        }

        #endregion

        #endregion

        #region Loop Methods

#if BASICALLY_SERVER

        public static void ServerStart() {
            foreach (var ent in entities.Values) {
                ent.OnServerStart();
            }
        }

        public static void ServerTick() {
            foreach (var ent in entities.Values) {
                ent.OnServerTick();
            }

            timeBuffer.Add(BGlobals.Tick, new InterpState(Time.time, Record()));
            if (timeBuffer.Count > BGlobals.TICK_RATE) {
                timeBuffer.RemoveAt(0);
            }
        }

        #endif

        #if BASICALLY_CLIENT

        public static void ClientStart() {
            foreach (var ent in entities.Values) {
                ent.OnClientStart();
            }
        }

        public static void ClientTick() {
            foreach (var ent in entities.Values) {
                ent.OnClientTick();
            }
        }

#endif

        #endregion

        #region Helpers

        private static ushort NextAvailableID() {
            for (ushort i = 0; i < ushort.MaxValue; i++) {
                if (!entities.ContainsKey(i)) return i;
            }
            return 0;
        }

        #endregion
    }
}
