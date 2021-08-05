using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Basically.Networking {
    public class HostCallbacks {
        public NetworkHost Host { get; internal set; }

        /// <summary>
        /// When a user connects to the host.
        /// </summary>
        /// <param name="conn">Connection of the new user.</param>
        public virtual void OnConnect(Connection conn) {

        }

        /// <summary>
        /// When a user disconnects, the passing connection is not valid and should only be used to get data before reset, such as ID.
        /// </summary>
        /// <param name="conn">Connection of the user.</param>
        public virtual void OnDisconnect(Connection conn, uint data) {

        }

        /// <summary>
        /// When a user times out, the passing connection is not valid and should only be used to get data before reset, such as ID.
        /// </summary>
        /// <param name="conn">Connection of the user.</param>
        public virtual void OnTimeout(Connection conn) {

        }

        /// <summary>
        /// When a message is received from a user.
        /// Note: This is not used for handling received messages. For handling messages, use Receivers.
        /// </summary>
        /// <param name="conn">Connection of the user.</param>
        public virtual void OnReceive(Connection conn) {

        }

        /// <summary>
        /// When a message is sent to a user.
        /// </summary>
        /// <param name="conn">Connection of the user.</param>
        public virtual void OnSend(Connection conn) {

        }
    }
}
