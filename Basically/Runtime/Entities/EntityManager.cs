using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Basically.Entities {
    using Utility;
    using Serialization;
    using Networking;

    public static class EntityManager {
        public static Entity[] entities;

        public static void OnLoad() {
            entities = Object.FindObjectsOfType<Entity>();

            for (int i = 0; i < entities.Length; i++) {
                entities[i].ID = i;
            }
        }

#if BASICALLY_SERVER

        public static void ServerStart() {
            foreach (var ent in entities) {
                ent.OnServerStart();
            }
        }

        public static void ServerTick() {
            foreach (var ent in entities) {
                ent.OnServerTick();
            }
        }

#endif

#if BASICALLY_CLIENT

        public static void ClientStart() {
            foreach (var ent in entities) {
                ent.OnClientStart();
            }
        }

        public static void ClientTick() {
            foreach (var ent in entities) {
                ent.OnClientTick();
            }
        }

#endif
    }

    public class EntityState {
        public Vector3 position;
        public Quaternion rotation;
        public Parameters parameters;
    }
}
