#if BASICALLY_SERVER

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Basically.Server {
    using Frameworks;
    using Networking;
    using Entities;
    using Utility;

    internal static class GameHistory {
        const int SIZE = NetworkTiming.SNAPSHOTS_PER_SECOND;

        static WorldSnapshot[] buffer; // ring buffer
        static WorldSnapshot rewindCache;
        static uint slot;

        // Time
        static int statesSkipped;
        static bool rewinding;

        // Public Properties
        public static WorldSnapshot LatestRecord => buffer[Wrap(slot - 1)];
        public static bool SnapshotReady => statesSkipped == 1;
        public static bool Rewinding => rewinding;

        // Utility
        static uint BufferSlot => Wrap(slot);
        static uint Tick => Framework.Instance.Tick;

        public static void Initialize() {
            buffer = new WorldSnapshot[SIZE];
            rewindCache = default;
            slot = 0;
            statesSkipped = 1;
        }

        public static void OnTick() {
            if (statesSkipped == NetworkTiming.STATE_TICKS_SKIPPED) {
                Add(Record());
                statesSkipped = 1;
            } else {
                statesSkipped++;
            }
        }

        public static WorldSnapshot Record() {
            var count = EntityManager.entities.Length;

            WorldSnapshot snap = new WorldSnapshot() {
                tick = Tick,
                ids = new ushort[count],
                states = new EntityState[count]
            };

            for (int i = 0; i < count; i++) {
                var ent = EntityManager.entities[i];
                IParameters param = new SerParameters(0);
                ent.Serialize(ref param);

                snap.ids[i] = ent.ID;
                snap.states[i] = new EntityState {
                    position = ent.Position,
                    rotation = ent.Rotation,
                    parameters = param
                };
            }

            return snap;
        }

        /// <summary>
        /// Rewinds time and interpolates to be as close the the client as possible.
        /// </summary>
        /// <param name="ms">Amount of ms to rewind</param>
        public static void Rewind(uint ms) {
            rewindCache = Record();

            int lag = Mathf.FloorToInt(ms / NetworkTiming.SNAPSHOT_INTERVAL);
            float interp = Mathf.InverseLerp(0, NetworkTiming.TICK, ms - (lag * NetworkTiming.SNAPSHOT_INTERVAL));

            uint pos = slot - (uint)lag;

            var from = buffer[Wrap(pos)];
            var to = pos == slot ? rewindCache : buffer[Wrap(pos + 1)];

            for (ushort i = 0; i < to.ids.Length; i++) {
                EntityManager.entities[to.ids[i]].Interpolate(from.states[i], to.states[i], interp);
            }

            rewinding = true;
        }

        /// <summary>
        /// Returns to present-time.
        /// </summary>
        public static void Reset() {
            if (!rewinding) return;

            // TODO: Add resimulation
            for (ushort i = 0; i < EntityManager.entities.Length; i++) {
                var ent = EntityManager.entities[i];
                var state = rewindCache.states[i];
                ent.transform.SetPositionAndRotation(state.position, state.rotation);
            }

            rewindCache = default;
            rewinding = false;
        }

        public static void Add(WorldSnapshot snapshot) {
            buffer[BufferSlot] = snapshot;
            slot++;
        }

        static uint Wrap(uint input) {
            return input % SIZE;
        }
    }
}

#endif