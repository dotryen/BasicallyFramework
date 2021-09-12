#if BASICALLY_CLIENT

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Basically.Client {
    using Entities;
    using Networking;

    public class Client : MonoBehaviour {
        public static Client Instance { get; private set; }

        public byte channelCount = 1;

        internal bool advance;
        internal uint tick = 0;

        /// <summary>
        /// Current client tick. Should match the server's.
        /// </summary>
        public uint Tick => tick;

        /// <summary>
        /// Tick expected on server when a message is sent.
        /// </summary>
        public uint PredictedTick => (uint)(tick + Mathf.FloorToInt(NetworkClient.Connection.Ping / NetworkTiming.TICK));

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

        protected virtual void OnGUI() {
            // if (advance) {
            //     Interpolation.InterpolationGUI();
            // }
        }

        public void Connect(string ip, ushort port) {
            if (NetworkClient.Connected) return;

            Time.fixedDeltaTime = NetworkTiming.TICK;
            Time.maximumDeltaTime = NetworkTiming.TICK;

#if PHYS_3D
            Physics.autoSimulation = false;
#endif
#if PHYS_2D
            Physics2D.autoSimulation = false;
#endif

            Interpolation.Initialize();

            NetworkClient.Initialize(null);
            NetworkClient.ConnectToServer(ip, port);
        }

        public void Disconnect(byte data) {
            NetworkClient.Disconnect();
            advance = false;
        }
    }
}

#endif