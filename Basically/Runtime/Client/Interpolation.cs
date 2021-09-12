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
            public float time;
            public WorldSnapshot world;

            public InterpState(float time, WorldSnapshot snap) {
                this.time = time;
                world = snap;
            }
        }

        static List<InterpState> buffer;

        // STATE
        static bool playing;
        static bool initialized;

        // TIME
        static float time;
        static float mark = 0f;
        
        // RECORDS
        static InterpState lastState;
        static uint lastTick;

        internal static void Initialize() {
            buffer = new List<InterpState>();

            playing = false;
            initialized = false;

            time = 0;
            mark = 0;

            lastState = default;
            lastTick = 0;
        }

        internal static void AddState(WorldSnapshot snap) {
            if (snap.tick <= lastTick) return;
            buffer.Add(new InterpState(time, snap));
            lastTick = snap.tick;
        }

        public static void Update(float delta) {
            if (!playing) {
                if (buffer.Count > 0 && !initialized) {
                    lastState = buffer[0];
                    initialized = true;
                    buffer.RemoveAt(0);
                }

                if (buffer.Count > 0 && initialized) {
                    playing = true;
                }
            } else {
                while (buffer.Count > 0 && mark > buffer[0].time) {
                    lastState = buffer[0];
                    buffer.RemoveAt(0);
                }

                if (buffer.Count > 0) {
                    if (buffer[0].time > 0) {
                        var alpha = (mark - lastState.time) / (buffer[0].time - lastState.time);

                        for (int i = 0; i < lastState.world.ids.Length; i++) {
                            var ent = EntityManager.entities[lastState.world.ids[i]];
                            ent.Interpolate(lastState.world.states[i], buffer[0].world.states[i], alpha);
                        }
                    }
                }

                mark += delta;
            }

            time += delta;
        }

        private static void Snap(InterpState state) {
            for (int i = 0; i < state.world.ids.Length; i++) {
                var id = state.world.ids[i];
                var ent = EntityManager.entities[id];

                ent.transform.position = state.world.states[id].position;
                ent.transform.rotation = state.world.states[id].rotation;
            }
        }

        public static void InterpolationGUI() {
            GUILayout.Box($"Interpolation:\nStates: {buffer.Count}\nMark: {mark}\nTime: {time}");
            // GUILayout.Label($"Interpolation: {buffer.Count} states");
            // GUILayout.Label($"    Mark: {mark}");
            // GUILayout.Label($"    Time: {time}");
        }
    }
}

#endif