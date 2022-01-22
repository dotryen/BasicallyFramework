#if BASICALLY_SERVER

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Basically.Server {
    using Entities;
    using Networking;
    using Utility;

    public class Server : MonoBehaviour {
        public static Server Instance { get; private set; }

        public byte channelCount = 1;
        private bool ready = false;

        private void Awake() {
            if (Instance != null) {
                Destroy(this);
            } else {
                DontDestroyOnLoad(gameObject);
                Instance = this;
            }
        }

        private void OnDestroy() {
            StopServer();
            NetworkServer.Deinitialize();
        }

        private void Update() {
            if (!ready) return;
            NetworkServer.Update();
        }

        private void FixedUpdate() {
            if (!ready) return;

            GameHistory.OnTick();

            if (GameHistory.SnapshotReady) {
                NetworkServer.Broadcast(GameHistory.LatestRecord, 0, MessageType.Unreliable);
            }
        }

        public void StartServer(byte maxPlayers, ushort port) {
            Time.fixedDeltaTime = BGlobals.TICK;
#if PHYS_3D
            Physics.autoSimulation = false;
#endif
#if PHYS_2D
            Physics2D.autoSimulation = false;
#endif
            GameHistory.Initialize();
            NetworkServer.Initialize();
            NetworkServer.StartServer(maxPlayers, port);
            ready = true;
        }

        public void StopServer() {
            NetworkServer.StopServer();
            ready = false;
        }
    }
}

#endif