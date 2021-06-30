#if BASICALLY_CLIENT
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Basically.Entities;
using Basically.Networking;

namespace Basically.Client {
    public static class Interpolation {
        class WorldState {
            public int tick;
            public int[] ids;
            public EntityState[] states;
        }

        // extra two frames for jitter and shit
        const int BUFFER_SIZE = NetworkTiming.SNAPSHOTS_PER_SECOND + 2;

        static WorldState[] buffer;
        static float currentTime;
        static int bufferSpot;
        static int lastTick;
        static int interpTicks;

        internal static void Initialize() {
            buffer = new WorldState[BUFFER_SIZE];
            // interpTicks = Mathf.FloorToInt(NetworkTiming.INTERP_TIME_TICK); // WIth interpolation
            interpTicks = 0; // Cancel interpolation
            currentTime = 0;
            bufferSpot = 0;
        }

        internal static void AddState(WorldSnapshot snap) {
            if (snap.tick <= lastTick) return;

            var state = new WorldState() {
                tick = snap.tick,
                ids = snap.ids,
                states = new EntityState[snap.ids.Length]
            };
            
            for (int i = 0; i < snap.ids.Length; i++) {
                state.states[i] = new EntityState() {
                    position = snap.positions[i],
                    rotation = snap.quaternions[i],
                    parameters = snap.parameters[i]
                };
            }

            buffer[BufferPos(bufferSpot + interpTicks)] = state;
            lastTick = snap.tick;
            bufferSpot++;
        }

        public static void Update(float deltaTime) {
            currentTime += deltaTime;
            int toTick = Mathf.CeilToInt(currentTime / NetworkTiming.STATE_INTERVAL);
            var toSnap = buffer[BufferPos(toTick)];

            if (toSnap == null) {
                // get one at next state
                toTick++;
                toSnap = buffer[BufferPos(toTick)];
            }

            int fromTick = Mathf.FloorToInt(currentTime / NetworkTiming.STATE_INTERVAL);
            var fromSnap = buffer[BufferPos(fromTick)];

            if (fromSnap == null) {
                // get one at before state
                fromTick--;
                fromSnap = buffer[BufferPos(toTick)];
            }

            if (toSnap != null) {
                if (fromSnap != null) {
                    // interp
                    var amount = Mathf.InverseLerp(fromTick * NetworkTiming.STATE_INTERVAL, toTick * NetworkTiming.STATE_INTERVAL, currentTime);
                    for (int i = 0; i < EntityManager.entities.Length; i++) {
                        var ent = EntityManager.entities[i];
                        if (ent.lastTickUpdated != fromSnap.tick) {
                            ent.ReadData(fromSnap.states[i].parameters);
                            ent.lastTickUpdated = fromSnap.tick;
                        }

                        ent.Interpolate(fromSnap.states[i], toSnap.states[i], amount);
                    }
                    // Debug.Log($"INTERP: From Tick: {fromSnap.tick}, To Tick: {toSnap.tick}, Amount: {amount}");
                    Client.Instance.currentInterpTime = amount;
                } else {
                    // snap lol
                    for (int i = 0; i < EntityManager.entities.Length; i++) {
                        var ent = EntityManager.entities[i];
                        ent.transform.position = toSnap.states[i].position;
                        ent.transform.rotation = toSnap.states[i].rotation;
                    }
                    // Debug.Log("Snapping");
                }
            }
        }

        private static int BufferPos(int tick) => Mathf.Max(0, tick % BUFFER_SIZE);
    }
}

#endif