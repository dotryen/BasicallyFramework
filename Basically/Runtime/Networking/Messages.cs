using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Basically.Networking {
    using Utility;

    internal struct WelcomeMessage : NetworkMessage {
        public byte id;
    }

    internal struct WelcomeConfirmation : NetworkMessage {
        public byte id;
    }

    internal struct WorldSnapshot : NetworkMessage {
        public int tick;
        public int[] ids;
        public Vector3[] positions;
        public Quaternion[] quaternions;
        public Parameters[] parameters;
    }
}
