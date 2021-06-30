using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Basically.Server {
    using Networking;

    internal class ServerCallbacks : HostCallbacks {
        public override void OnConnect(Connection conn) {
            var message = new WelcomeMessage() {
                id = conn.ID
            };
            conn.Send(message, 0, MessageType.Reliable);

            ServerTimers.Add(conn.ID, 10, () => {
                conn.Disconnect();
            });
        }
    }
}
