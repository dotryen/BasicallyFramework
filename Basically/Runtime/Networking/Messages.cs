using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Basically.Networking {
    using Serialization;
    using Utility;
    using Entities;

    internal struct WelcomeMessage : NetworkMessage {
        public byte id;
    }

    internal struct AuthMessage : NetworkMessage {
        public bool success;
    }

    internal struct TimeRequest : NetworkMessage {
        public uint clientTime;
        public uint serverTime;
    }

    [DeltaEncoded]
    internal struct WorldSnapshot : NetworkMessage {
        public uint tick;
        public ushort[] ids;
        public EntityState[] states;

        public float TickMS => tick * BGlobals.TICK;
    }

    internal struct DeltaConfirm : NetworkMessage {
        // TODO: Add delta confirmations
    }

    public struct PlayerEnter : NetworkMessage {
        public byte id;
    }
}
