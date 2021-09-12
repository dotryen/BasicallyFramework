using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Basically.Entities {
    /// <summary>
    /// A special kind of entity that adds prediction functionality on the client.
    /// </summary>
    public class PredictableEntity : Entity {
        protected internal virtual void Predict() {

        }
    }
}
