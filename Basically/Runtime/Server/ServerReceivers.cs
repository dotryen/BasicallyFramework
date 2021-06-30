#if BASICALLY_SERVER

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Basically.Server {
    using Networking;

    internal static class ServerReceivers {
        public static void WelcomeConfirmation(Connection conn, WelcomeMessage message) {
            ServerTimers.Stop(conn.ID);

            if (conn.ID != message.id) {
                Debug.Log("Possibly spoofed id. Disconnected player " + conn.ID);
                conn.Disconnect();
            } else {
                NetworkServer.originalCallbacks?.OnConnect(conn);
                Debug.Log($"Player {conn.ID} approved.");
            }
        }
    }
}

#endif