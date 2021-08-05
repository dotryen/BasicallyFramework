using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Basically.Networking {
    using Serialization;
    using Utility;

    internal struct WelcomeMessage : NetworkMessage {
        public byte id;
    }

    internal struct AuthMessage : NetworkMessage {
        public bool success;
    }

    [DeltaEncoded]
    internal struct WorldSnapshot : NetworkMessage {
        public int tick;
        public int[] ids;
        public Vector3[] positions;
        public Quaternion[] quaternions;
    }

    internal struct DeltaConfirm : NetworkMessage {
        // TODO: Add delta confirmations
    }
}
