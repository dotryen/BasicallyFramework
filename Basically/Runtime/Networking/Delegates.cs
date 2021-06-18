using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Basically.Serialization;

namespace Basically.Networking {
    /// <summary>
    /// Delegate for recieving packets
    /// </summary>
    /// <param name="player">Connection the packet originated from.</param>
    /// <param name="message">Message from connection.</param>
    public delegate void Receiver(Connection player, NetworkMessage message);
}
