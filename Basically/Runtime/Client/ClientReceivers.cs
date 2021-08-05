#if BASICALLY_CLIENT

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Basically.Networking;

namespace Basically.Client {
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

        public static void EntityUpdate(Connection conn, WorldSnapshot message) {
            Interpolation.AddState(message);

            if (!Client.Instance.advance) {
                Client.Instance.tick = message.tick;
                Client.Instance.advance = true;
            }
        }
    }
}

#endif