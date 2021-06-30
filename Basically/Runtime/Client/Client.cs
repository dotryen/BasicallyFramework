#if BASICALLY_CLIENT

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Basically.Client {
    using Entities;

    public class Client : MonoBehaviour {
        public static Client Instance { get; private set; }

        public byte channelCount = 1;
        [Range(0, 1)]
        public float currentInterpTime = 0f;

        internal bool advance;
        internal int tick = 0;

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
            NetworkClient.Disconnect();
            NetworkClient.Deinitialize();
        }

        private void Update() {
            NetworkClient.Update();

            if (advance) {
                Interpolation.Update(Time.deltaTime);
            }
        }

        private void FixedUpdate() {
            if (!advance) return;

            EntityManager.ClientTick();
#if PHYS_3D
            Physics.Simulate(Time.fixedDeltaTime);
#endif
#if PHYS_2D
            Physics2D.Simulate(Time.fixedDeltaTime);
#endif
            tick++;
        }

        public void Connect(string ip, ushort port) {
            Time.fixedDeltaTime = Networking.NetworkTiming.TICK;
#if PHYS_3D
            Physics.autoSimulation = false;
#endif
#if PHYS_2D
            Physics2D.autoSimulation = false;
#endif

            Interpolation.Initialize();
            NetworkClient.Initialize(channelCount);
            NetworkClient.ConnectToServer(ip, port);
        }

        public void Disconnect(byte data) {
            NetworkClient.Disconnect();
            advance = false;
        }
    }
}

#endif