#if BASICALLY_SERVER

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Basically.Frameworks {
    using Entities;
    using Networking;
    using Server;
    using Utility;

    [AddComponentMenu("Basically/Frameworks/Server Framework")]
    public class DedicatedFramework : Framework {
        public static new DedicatedFramework Instance => (DedicatedFramework)Framework.Instance;

        public byte maxPlayers = 16;
        public ushort port = 27020;

        private uint skipCounter = BGlobals.STATE_TICKS_SKIPPED;

        protected virtual HostCallbacks ServerCallbacks => null;

        internal override void OnStart() {
            NetworkServer.Initialize(ServerCallbacks);

            NetworkServer.StartServer(maxPlayers, port);
            BGlobals.IsServer = true;
        }

        internal override void OnStop() {
            NetworkServer.StopServer();
            NetworkServer.Deinitialize();
            BGlobals.IsServer = false;
        }

        internal override void SimulatePrePhys() {
            NetworkServer.Update();
            EntityManager.ServerTick();
            // GameHistory.OnTick();
        }

        internal override void SimulatePostPhys() {
            if (skipCounter == BGlobals.STATE_TICKS_SKIPPED) {
                // var record = GameHistory.LatestRecord;
                // if (record.tick != tick) print($"tick mismatch {tick - record.tick}");
            
                NetworkServer.Broadcast(EntityManager.LatestRecord, 0, MessageType.Unreliable);
                skipCounter = 1;
            } else {
                skipCounter++;
            }
        }
    }
}

#endif