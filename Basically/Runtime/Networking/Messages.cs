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

    [DeltaEncoded]
    internal struct WorldSnapshot : NetworkMessage {
        public uint tick;
        public ushort[] ids;
        public EntityState[] states;

        public float TickMS => tick * NetworkTiming.TICK;
    }

    internal struct DeltaConfirm : NetworkMessage {
        // TODO: Add delta confirmations
    }

    public struct PlayerEnter : NetworkMessage {
        public byte id;
    }
}
