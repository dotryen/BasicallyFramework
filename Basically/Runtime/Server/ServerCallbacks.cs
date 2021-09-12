#if BASICALLY_SERVER

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Basically.Server {
    using Networking;

    internal class ServerCallbacks : HostCallbacks {
        public override void OnConnect(Connection conn) {
            NetworkUtility.Log($"Connection {conn.ID} successfully connected, sending ID");
            ServerTimers.Add(conn.ID, 10, () => {
                conn.Disconnect();
                NetworkUtility.Log("Handshake time limit reached.");
            });

            var message = new WelcomeMessage() {
                id = conn.ID
            };
            conn.Send(message, 0, MessageType.Reliable);
        }

        public override void OnDisconnect(Connection conn, uint data) {
            NetworkServer.originalCallbacks?.OnDisconnect(conn, data);
            Debug.Log($"Player {conn.ID} left the server.");
        }

        public override void OnTimeout(Connection conn) {
            NetworkServer.originalCallbacks?.OnTimeout(conn);
            Debug.Log($"Player {conn.ID} timed out.");
        }

        public override void OnReceive(Connection conn) {
            NetworkServer.originalCallbacks?.OnReceive(conn);
        }

        public override void OnSend(Connection conn) {
            NetworkUtility.Log($"Message sent to connection {conn.ID}");
            NetworkServer.originalCallbacks?.OnSend(conn);
        }
    }
}

#endif