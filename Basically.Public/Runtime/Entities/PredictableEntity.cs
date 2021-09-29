using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Basically.Entities {
    /// <summary>
    /// A special kind of entity that adds prediction functionality on the client.
    /// </summary>
    public class PredictableEntity : Entity {
        public bool DoPrediction { get; set; } = false;
        public Vector3 PositionError { get; internal set; }
        public Quaternion RotationError { get; internal set; }

        protected internal virtual void Predict() {

        }
    }
}
