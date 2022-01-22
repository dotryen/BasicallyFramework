#if BASICALLY_CLIENT

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Basically.Client {
    using Networking;
    using Frameworks;

    internal class ClientCallbacks : HostCallbacks {
        public override void OnConnect(Connection conn) {
            conn.Status = ConnectionStatus.WaitingForHandshake;
            NetworkClient.connection = conn;
            Debug.Log("Succesfully connected to server, awaiting ID.");
        }

        public override void OnDisconnect(Connection conn, uint data) {
            NetworkClient.originalCallbacks?.OnDisconnect(conn, data);
            ClientFramework.Instance.StopFramework();
            Debug.Log("Server ended our connection.");
        }

        public override void OnTimeout(Connection conn) {
            NetworkClient.originalCallbacks?.OnTimeout(conn);
            ClientFramework.Instance.StopFramework();
            Debug.Log("Server timed out.");
        }

        public override void OnReceive(Connection conn) {
            // Debug.Log("Message received.");
            NetworkClient.originalCallbacks?.OnReceive(conn);
        }

        public override void OnSend(Connection conn) {
            NetworkClient.originalCallbacks?.OnSend(conn);
        }
    }
}

#endif