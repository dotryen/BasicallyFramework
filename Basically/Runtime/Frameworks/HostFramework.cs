#if BASICALLY_CLIENT && BASICALLY_SERVER

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Basically.Frameworks {
    using Networking;
    using Client;
    using Server;
    using Utility;
    using Entities;

    [AddComponentMenu("Basically/Frameworks/Host Framework")]
    public class HostFramework : Framework {
        public byte maxPlayers = 16;
        public ushort port = 27020;

        private uint skipCounter = BGlobals.STATE_TICKS_SKIPPED;

        protected virtual HostCallbacks ClientCallbacks => null;
        protected virtual HostCallbacks ServerCallbacks => null;

        private void Update() {
            if (NetworkClient.Initialized) {
                NetworkClient.Update();
            }
        }

        internal override void OnStart() {
            // GameHistory.Initialize();

            NetworkClient.LocalInitialize(ClientCallbacks);
            NetworkServer.Initialize(ServerCallbacks);

            NetworkServer.StartServer(maxPlayers, port);
            NetworkServer.host.AddConnection(new LocalClientConnection());

            BGlobals.IsClient = true;
            BGlobals.IsServer = true;
        }

        internal override void OnStop() {
            NetworkServer.StopServer();

            NetworkServer.Deinitialize();
            NetworkClient.Deinitialize();

            BGlobals.IsClient = false;
            BGlobals.IsServer = false;
        }

        internal override void SimulatePrePhys() {
            NetworkServer.Update();
            EntityManager.ServerTick();
        }

        internal override void SimulatePostPhys() {
            if (skipCounter == BGlobals.STATE_TICKS_SKIPPED) {
                // var record = GameHistory.LatestRecord;
                // if (record.tick != tick) print($"tick mismatch {tick - record.tick}");

                NetworkServer.Broadcast(EntityManager.LatestRecord, 0, MessageType.Unreliable, NetworkClient.ID);
                skipCounter = 1;
            } else {
                skipCounter++;
            }
        }
    }
}

#endif