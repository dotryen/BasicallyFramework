#if BASICALLY_SERVER

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Basically.Networking;

namespace Basically.Server {
    internal static class ServerReceivers {
        public static void WelcomeConfirmation(Connection conn, WelcomeConfirmation message) {
            if (conn.ID != message.id) {
                Debug.Log("Possibly spoofed id. Disconnected player " + conn.ID);
                conn.Disconnect();
            } else {
                Debug.Log($"Player {conn.ID} approved.");
            }
        }
    }
}

#endif