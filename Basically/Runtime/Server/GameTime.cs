#if BASICALLY_SERVER

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Basically.Server {
    using Frameworks;
    using Networking;
    using Entities;
    using Utility;

    internal static class GameHistory {
        const int SIZE = BGlobals.SNAPSHOTS_PER_SECOND;

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
        static uint Tick => BGlobals.Tick;

        public static void Initialize() {
            buffer = new WorldSnapshot[SIZE];
            rewindCache = default;
            slot = 0;
            statesSkipped = 1;
        }

        public static void OnTick() {
            if (statesSkipped == BGlobals.STATE_TICKS_SKIPPED) {
                Add(Record());
                statesSkipped = 1;
            } else {
                statesSkipped++;
            }
        }

        public static WorldSnapshot Record() {
            var count = EntityManager.Entities.Count;

            WorldSnapshot snap = new WorldSnapshot() {
                tick = Tick,
                ids = new ushort[count],
                states = new EntityState[count]
            };

            for (ushort i = 0; i < count; i++) {
                var pair = EntityManager.Entities.ElementAt(i);
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

        /// <summary>
        /// Rewinds time and interpolates to be as close the the client as possible.
        /// </summary>
        /// <param name="ms">Amount of ms to rewind</param>
        public static void Rewind(uint ms) {
            rewindCache = Record();

            int lag = Mathf.FloorToInt(ms / BGlobals.SNAPSHOT_INTERVAL);
            float interp = Mathf.InverseLerp(0, BGlobals.TICK, ms - (lag * BGlobals.SNAPSHOT_INTERVAL));

            uint pos = slot - (uint)lag;

            var from = buffer[Wrap(pos)];
            var to = pos == slot ? rewindCache : buffer[Wrap(pos + 1)];

            foreach (var ent in EntityManager.Entities.Values) {
                ent.Interpolate(from.states[ent.ID], to.states[ent.ID], interp);
            }

            rewinding = true;
        }

        /// <summary>
        /// Returns to present-time.
        /// </summary>
        public static void Reset() {
            if (!rewinding) return;

            // TODO: Add resimulation
            foreach (var ent in EntityManager.Entities.Values) {
                var state = rewindCache.states[ent.ID];
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