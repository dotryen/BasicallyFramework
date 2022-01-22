#if BASICALLY_CLIENT

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Basically.Frameworks {
    using Networking;
    using Client;
    using Entities;
    using Utility;

    [AddComponentMenu("Basically/Frameworks/Client Framework")]
    public class ClientFramework : Framework {
        public new static ClientFramework Instance => (ClientFramework)Framework.Instance;

        public string ipAddress = "localhost";
        public ushort port = 27020;

        internal bool advance = false;
        internal SortedList<uint, PredictableEntity> predictList;

        private uint tickCache = 0;

        private void Update() {
            NetworkClient.Update();
            if (advance) EntityManager.UpdateInterpolation(Time.deltaTime);
        }

        public void UpdateDestination(string ip, ushort port) {
            ipAddress = ip;
            this.port = port;
        }

        internal override void OnStart() {
            predictList = new SortedList<uint, PredictableEntity>();

            NetworkClient.Initialize();

            NetworkClient.ConnectToServer(ipAddress, port);
            BGlobals.IsClient = true;
        }

        internal override void OnStop() {
            advance = false;

            if (NetworkClient.Connected) NetworkClient.Disconnect();
            NetworkClient.Deinitialize();
            BGlobals.IsClient = false;
        }

        internal override void SimulatePrePhys() {
            if (!advance) return;
            EntityManager.ClientTick();
        }

        internal override void SimulatePostPhys() {
            if (!advance) return;
            // This runs the entire prediction system.
            // The system that makes shit feel good.

            predictList.Clear();
            if (EntityManager.TimeBuffer.Count >= 1) {
                WorldSnapshot snapshot;
                if (EntityManager.TimeBuffer.Count >= 2) snapshot = EntityManager.TimeBuffer.Values[1].world;
                else snapshot = EntityManager.TimeBuffer.Values[0].world;

                foreach (var ent in EntityManager.PredictableEntities.Values) {
                    if (!ent.doPrediction) continue;

                    var rewind = ent.NeedsCorrectionInternal(snapshot.states[ent.ID]);
                    if (rewind > 0) {
                        predictList.Add(rewind, ent);
                    }
                }

                if (predictList.Count != 0) {
                    tickCache = tick;
                    var reversed = predictList.Reverse().ToArray();
                    var rewindTick = tickCache - reversed[0].Key;
                    var maxIndex = 1;

                    Debug.Log($"Correcting for {reversed[0].Key} ticks");

                    tick = rewindTick;
                    AfterTickUpdate();

                    while (tick < tickCache) {
                        while (true) {
                            if (reversed.Length == maxIndex) break;
                            if (tickCache - reversed[0].Key == tick) maxIndex++;
                            else break;
                        }

                        for (int i = 0; i < maxIndex; i++) {
                            var current = reversed[0];
                            current.Value.CorrectInternal(snapshot.states[current.Value.ID]);
                        }

                        Physics.Simulate(Time.fixedDeltaTime);
                        tick++;
                        AfterTickUpdate();
                    }

                    foreach (var ent in predictList.Values) {
                        ent.CorrectFinalize();
                    }
                }
            }
        }

        internal override void AfterTickUpdate() {
            if (!advance) return;
            BGlobals.Tick = tick;
            BGlobals.PredictedTick = (uint)(tick + Mathf.FloorToInt(NetworkClient.Connection.Ping / BGlobals.TICK));
        }
    }
}

#endif