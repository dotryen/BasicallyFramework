using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Basically.Networking {
    using ENet;

    public enum MessageType {
        /// <summary>
        /// Packets are guarrenteed to arrive and be in order.
        /// </summary>
        Reliable = PacketFlags.Reliable,
        /// <summary>
        /// Packets won't be resent when dropped.
        /// </summary>
        UnreliableSequenced = PacketFlags.None,
        /// <summary>
        /// Packets can arrive out of order and won't be resent when dropped.
        /// </summary>
        Unreliable = PacketFlags.Unsequenced,
        /// <summary>
        /// Packets will skip the transport's queue and be sent instantly.
        /// </summary>
        Instant = PacketFlags.Instant
    }
}
