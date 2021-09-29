using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Basically.Entities {
    using Utility;
    using Serialization;
    using Networking;

    public static class EntityManager {
        public static Entity[] entities;

        public static void OnLoad() {
            entities = Object.FindObjectsOfType<Entity>();

            for (ushort i = 0; i < entities.Length; i++) {
                entities[i].ID = i;
            }
        }

        public static void RegisterEntity(Entity entity) {
            entity.ID = (ushort)entities.Length;
            Array.Resize(ref entities, entities.Length + 1);
            entities[entity.ID] = entity;
        }

        public static void CreateEntity(string name) {

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
}
