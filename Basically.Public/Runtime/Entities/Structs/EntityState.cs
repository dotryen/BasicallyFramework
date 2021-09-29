using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Basically.Entities {
    using Utility;

    public struct EntityState {
        public Vector3 position;
        public Quaternion rotation;
        public IParameters parameters;
    }
}
