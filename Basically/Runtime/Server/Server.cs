#if BASICALLY_SERVER

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Basically.Server {
    using Entities;
    using Networking;

    public class Server : MonoBehaviour {
        public static Server Instance { get; private set; }

        public byte channelCount = 1;
        internal int tick = 0;
        private bool ready = false;

        public int Tick => tick;

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

            EntityManager.ServerTick();
#if PHYS_3D
            Physics.Simulate(Time.fixedDeltaTime);
#endif
#if PHYS_2D
            Physics2D.Simulate(Time.fixedDeltaTime);
#endif
            // TODO: UNCOMMENT THIS AFTER FIXING OTHER SHIT
            if (tick % NetworkTiming.STATE_TICKS_SKIPPED == 0) {
                NetworkServer.Broadcast(SnapshotBuilder.CreateSnapshot(), 0, MessageType.Unreliable);
            }

            tick++;
        }

        public void StartServer(byte maxPlayers, ushort port) {
            Time.fixedDeltaTime = NetworkTiming.TICK;
#if PHYS_3D
            Physics.autoSimulation = false;
#endif
#if PHYS_2D
            Physics2D.autoSimulation = false;
#endif
            NetworkServer.Initialize(channelCount, null);
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