#if BASICALLY_CLIENT

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Basically.Frameworks {
    using Networking;
    using Client;
    using Entities;

    [AddComponentMenu("Basically/Frameworks/Client Framework")]
    public class ClientFramework : Framework {
        public new static ClientFramework Instance => (ClientFramework)Framework.Instance;

        public string ipAddress = "localhost";
        public ushort port = 27020;

        internal bool advance = false;

        public uint PredictedTick => (uint)(Tick + Mathf.FloorToInt(NetworkClient.Connection.Ping / NetworkTiming.TICK));

        private void Update() {
            NetworkClient.Update();
            if (advance) Interpolation.Update(Time.deltaTime);
        }

        public void UpdateDestination(string ip, ushort port) {
            ipAddress = ip;
            this.port = port;
        }

        internal override void OnStart() {
            Interpolation.Initialize();
            NetworkClient.Initialize();

            NetworkClient.ConnectToServer(ipAddress, port);
        }

        internal override void OnStop() {
            advance = false;

            NetworkClient.Disconnect();
            NetworkClient.Deinitialize();
        }

        internal override void SimulatePostPhys() {
            
        }

        internal override void SimulatePrePhys() {
            EntityManager.ClientTick();
        }
    }
}

#endif