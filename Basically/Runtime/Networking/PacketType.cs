﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Basically.Networking {
    using ENet;

    public enum PacketType {
        Reliable = PacketFlags.Reliable,
        UnreliableSequenced = PacketFlags.None,
        Unreliable = PacketFlags.Unsequenced,
        Instant = PacketFlags.Instant
    }
}