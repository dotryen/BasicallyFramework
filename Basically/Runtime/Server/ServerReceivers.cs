#if BASICALLY_SERVER

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Basically.Server {
    using Networking;
    using Utility;

    [ReceiverClass]
    internal static class ServerReceivers {
        [NoAuth]
        public static void WelcomeConfirmation(Connection conn, WelcomeMessage message) {
            ServerTimers.Stop(conn.ID);

            var auth = new AuthMessage();

            if (conn.ID != message.id) {
                auth.success = false;

                conn.Send(auth, 0, MessageType.Reliable);
                conn.Disconnect();

                Debug.Log("Possibly spoofed response. Disconnected player " + conn.ID);
            } else {
                conn.Status = ConnectionStatus.Connected;
                auth.success = true;

                var time = new TimeRequest() {
                    serverTime = BGlobals.Tick
                };

                conn.Send(auth, 0, MessageType.Reliable);
                // conn.Send(time, 0, MessageType.Reliable);
                NetworkServer.originalCallbacks?.OnConnect(conn);

                Debug.Log($"Player {conn.ID} approved.");
            }
        }

        public static void TimeRequest(Connection conn, TimeRequest message) {
            message.serverTime = BGlobals.Tick;

            conn.Send(message, 0, MessageType.Reliable);
        }
    }
}

#endif