using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Basically.Modding {
    using Mapping;

    [CreateAssetMenu(fileName = "PackData.asset", menuName = "Basically/Pack Data")]
    public class PackDataContainer : ScriptableObject {
        public PackData data;
    }
}
