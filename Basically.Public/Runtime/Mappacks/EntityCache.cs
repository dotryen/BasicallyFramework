using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Basically.Mapping {
    using Entities;

    [DisallowMultipleComponent]
    public class EntityCache : MonoBehaviour {
        // Ensures that the entity list will be the same on each client
        internal Entity[] entities;
    }
}
