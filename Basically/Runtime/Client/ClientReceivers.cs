﻿#if BASICALLY_CLIENT

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Basically.Networking;

namespace Basically.Client {
    internal static class ClientReceivers {
        public static void Welcome(Connection conn, WelcomeMessage message) {
            NetworkClient.ID = message.id;
            conn.Send(new WelcomeConfirmation() {
                id = message.id
            }, 0, PacketType.Reliable);

            Debug.Log("WELCOME MESSAGE RECEIVED");
        }

        public static void EntityUpdate(Connection conn, WorldSnapshot message) {
            if (!Client.Instance.advance) {
                Client.Instance.tick = message.tick;
                Client.Instance.advance = true;
            }

            Interpolation.AddState(message);
        }
    }
}

#endif