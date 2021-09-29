#if PHYS_3D

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Basically.Entities {
    using Utility;

    /// <summary>
    /// A 3D physics entity
    /// </summary>
    [AddComponentMenu("Basically/Entities/Phys3D Entity")]
    [RequireComponent(typeof(Rigidbody))]
    public class PhysicsEntity : Entity {
        [HideInInspector]
        public new Rigidbody rigidbody;

        public bool prediction;

        // Prediction correction
        Vector3 positionError;
        Quaternion rotationError;

        public override Vector3 Position => rigidbody.position;
        public override Quaternion Rotation => rigidbody.rotation;

        public Vector3 Velocity { get; set; }

        protected internal override void OnServerStart() {
            rigidbody = GetComponent<Rigidbody>();
        }

        protected internal override void OnClientStart() {
            rigidbody = GetComponent<Rigidbody>();
        }

        protected internal override void OnServerTick() {
            Velocity = rigidbody.velocity;
        }

        protected internal override void OnClientTick() {
            rigidbody.isKinematic = !prediction;
        }

        protected internal override void Serialize(ref IParameters parameters) {
            parameters.Add("vel", rigidbody.velocity);
        }

        protected internal override void Interpolate(EntityState from, EntityState to, float interpAmount) {
            Vector3 fromVel = from.parameters.Get<Vector3>("vel") * Time.deltaTime;
            Vector3 toVel = to.parameters.Get<Vector3>("vel") * Time.deltaTime;

            Vector3 pos = Curves.Hermite(from.position, fromVel, to.position, toVel, interpAmount);
            Quaternion rot = Quaternion.Slerp(from.rotation, to.rotation, interpAmount);

            if (prediction) {
                var positionError = pos - rigidbody.position;

                if (positionError.sqrMagnitude >= 0.0001f) {
                    Vector3 oldPos = rigidbody.position;
                    Vector3 oldVel = rigidbody.velocity;

                    Vector3 prevPos = rigidbody.position + positionError;

                    // rigidbody.position = pos;
                    // Set velocity
                }
            } else {
                transform.SetPositionAndRotation(pos, rot);
            }
        }
    }
}

#endif