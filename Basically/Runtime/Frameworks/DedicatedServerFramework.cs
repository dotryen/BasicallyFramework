#if BASICALLY_SERVER

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Basically.Frameworks {
    using Networking;
    using Server;

    [AddComponentMenu("Basically/Frameworks/Server Framework")]
    public class DedicatedServerFramework : Framework {
        public static new DedicatedServerFramework Instance => (DedicatedServerFramework)Framework.Instance;

        public byte maxPlayers = 16;
        public ushort port = 27020;

        internal override void OnStart() {
            GameHistory.Initialize();
            NetworkServer.Initialize();

            NetworkServer.StartServer(maxPlayers, port);
        }

        internal override void OnStop() {
            NetworkServer.StopServer();
            NetworkServer.Deinitialize();
        }

        internal override void SimulatePostPhys() {
            if (GameHistory.SnapshotReady) NetworkServer.Broadcast(GameHistory.LatestRecord, 0, MessageType.Unreliable);
        }

        internal override void SimulatePrePhys() {
            NetworkServer.Update();
            GameHistory.OnTick();
        }
    }
}

#endif