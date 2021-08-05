#if BASICALLY_CLIENT

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Basically.Client {
    using Networking;

    internal class ClientCallbacks : HostCallbacks {
        public override void OnConnect(Connection conn) {
            NetworkClient.Connection.Status = ConnectionStatus.WaitingForHandshake;
            Debug.Log("Succesfully connected to server, awaiting ID.");
        }

        public override void OnDisconnect(Connection conn, uint data) {
            Reset();
            NetworkClient.originalCallbacks?.OnDisconnect(conn, data);
            Debug.Log("Server ended our connection.");
        }

        public override void OnTimeout(Connection conn) {
            Reset();
            NetworkClient.originalCallbacks?.OnTimeout(conn);
            Debug.Log("Server timed out.");
        }

        public override void OnReceive(Connection conn) {
            Debug.Log("Message received.");
            NetworkClient.originalCallbacks?.OnReceive(conn);
        }

        public override void OnSend(Connection conn) {
            NetworkClient.originalCallbacks?.OnSend(conn);
        }

        void Reset() {
            NetworkClient.Connection.Status = ConnectionStatus.NotConnected;
            Host.Stop();
            Debug.Log("Reset called");
        }
    }
}

#endif