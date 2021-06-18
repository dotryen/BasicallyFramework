#if BASICALLY_SERVER

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Basically.Server {
    using Networking;
    using Entities;
    using Utility;

    internal static class SnapshotBuilder {
        internal static WorldSnapshot CreateSnapshot() {
            var count = EntityManager.entities.Length;

            WorldSnapshot snap = new WorldSnapshot() {
                tick = Server.Instance.tick,
                ids = new int[count],
                positions = new Vector3[count],
                quaternions = new Quaternion[count],
                parameters = new Parameters[count]
            };

            for (int i = 0; i < count; i++) {
                var ent = EntityManager.entities[i];
                snap.ids[i] = ent.ID;
                snap.positions[i] = ent.tPosition;
                snap.quaternions[i] = ent.tRotation;
                snap.parameters[i] = ent.WriteData();
            }

            return snap;
        }
    }
}

#endif