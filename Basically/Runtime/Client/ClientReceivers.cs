#if BASICALLY_CLIENT

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Basically.Client {
    using Frameworks;
    using Networking;
    using Utility;
    using Entities;

    [ReceiverClass]
    internal static class ClientReceivers {
        [NoAuth]
        public static void Welcome(Connection conn, WelcomeMessage message) {
            NetworkClient.ID = message.id;

            conn.Send(message, 0, MessageType.Reliable);

            Debug.Log($"Welcome received. ID: {message.id}");
            Debug.Log($"Awaiting AuthMessage.");
        }

        [NoAuth]
        public static void Autheticate(Connection conn, AuthMessage message) {
            if (message.success) {
                conn.Status = ConnectionStatus.Connected;
                NetworkClient.originalCallbacks?.OnConnect(conn);
                Debug.Log("Auth Success");
            } else {
                Debug.Log("Auth Failed");
            }
        }

        public static void TimeRequest(Connection conn, TimeRequest message) {
            ClientFramework.Instance.tick = message.serverTime;
            // ClientFramework.Instance.tick = (uint)(message.serverTime + Mathf.FloorToInt(conn.Ping / BTime.TICK));
            ClientFramework.Instance.advance = true;
            ClientFramework.Instance.AfterTickUpdate(); // force update
        }

        public static void EntityUpdate(Connection conn, WorldSnapshot message) {
            EntityManager.AddSnapshot(message);

            if (!ClientFramework.Instance.advance) {
                ClientFramework.Instance.tick = message.tick; // MUST BE CHANGED
                ClientFramework.Instance.advance = true;
                ClientFramework.Instance.AfterTickUpdate(); // force update
            }
        }
    }
}

#endif