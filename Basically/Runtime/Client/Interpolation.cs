#if BASICALLY_CLIENT
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Basically.Client {
    using Utility;
    using Entities;
    using Networking;

    public static class Interpolation {
        internal class WorldState {
            public int tick;
            public int[] ids;
            public EntityState[] states;

            public WorldState(WorldSnapshot snap) {
                tick = snap.tick;
                ids = snap.ids;
                states = new EntityState[ids.Length];

                for (int i = 0; i < snap.ids.Length; i++) {
                    states[i] = new EntityState() {
                        position = snap.positions[i],
                        rotation = snap.quaternions[i],
                        parameters = snap.parameters[i]
                    };
                }
            }
        }

        // extra two frames for jitter and shit
        const int BUFFER_SIZE = 4;
        static WorldState[] buffer;
        static int bufferSpot = 0; // snapshots are never shot out each tick, so this counteracts that

        // TIME
        static float time;
        static int lastTick;

        internal static void Initialize() {
            buffer = new WorldState[BUFFER_SIZE];
            // interpTicks = Mathf.FloorToInt(NetworkTiming.INTERP_TIME_TICK); // moves states ahead to allow for new snapshots (for dial-up era lol)
            // interpTicks = 0; // this is better
            time = 0;
            lastTick = 0;
            bufferSpot = 0;
        }

        internal static void AddState(WorldSnapshot snap) {
            if (snap.tick <= lastTick) return;

            buffer[BufferPos(bufferSpot)] = new WorldState(snap);
            lastTick = snap.tick;
            // bufferSpot++;
            // Debug.Log($"Adding state {snap.tick} to position {bufferSpot}");
        }

        public static void Update(float delta) {
            time += delta;

            WorldState from;
            WorldState to;
            int toPos;
            int fromPos;

            { // Get States
                toPos = Mathf.CeilToInt(time / NetworkTiming.SNAPSHOT_INTERVAL);
                fromPos = Mathf.FloorToInt(time / NetworkTiming.SNAPSHOT_INTERVAL);
                to = buffer[BufferPos(toPos)];
                from = buffer[BufferPos(fromPos)];
            }
            
            // Interpolation Shit

            if (from != null && to != null) { // Interpolate
                var amount = Mathf.InverseLerp(fromPos * NetworkTiming.SNAPSHOT_INTERVAL, toPos * NetworkTiming.SNAPSHOT_INTERVAL, time);

                for (int i = 0; i < EntityManager.entities.Length; i++) {
                    var ent = EntityManager.entities[i];

                    DoEntityRead(ent, from.tick, from.states[i].parameters);
                    ent.Interpolate(from.states[i], to.states[i], amount);
                }
                Debug.Log($"INTERP: From Tick: {from.tick}, To Tick: {to.tick}, Amount: {amount}");
            } else if (from == null && to != null) { // Snap
                Snap(to);
                Debug.Log("Snapping: TO");
            } else if (from != null && to == null) {
                Snap(from);
                Debug.Log("Snapping: FROM");
            } else { // we cant really do anything without data
                Debug.LogError("INTERP: To and From are null!");
            }
        }

        public static void Tick() {
            buffer[BufferPos(bufferSpot + 1)] = null;
            bufferSpot++;
        }

        private static void Snap(WorldState state) {
            for (int i = 0; i < state.ids.Length; i++) {
                var id = state.ids[i];
                var ent = EntityManager.entities[id];

                DoEntityRead(ent, state.tick, state.states[id].parameters);
                ent.transform.position = state.states[id].position;
                ent.transform.rotation = state.states[id].rotation;
            }
        }

        private static void DoEntityRead(Entity ent, int tick, Parameters param) {
            if (ent.lastTickUpdated < tick) {
                ent.ReadData(param);
                ent.lastTickUpdated = tick;
            }
        }

        private static int BufferPos(int tick) => Mathf.Max(0, tick % BUFFER_SIZE);

        internal static void InterpolationGUI() {
            GUILayout.Label("Interpolation:");
            for (int i = 0; i < BUFFER_SIZE; i++) {
                string output = $"{i} ";

                if (buffer[i] == null) output += "= null ";
                else output += "= taken ";

                if (BufferPos(bufferSpot) == i) output += "(Current)";

                GUILayout.Label(output);
            }

            GUILayout.Space(10);
        }
    }
}

#endif