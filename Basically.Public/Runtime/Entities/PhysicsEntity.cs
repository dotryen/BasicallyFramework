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
        private new Rigidbody rigidbody;

        public override Vector3 Position => rigidbody.position;
        public override Quaternion Rotation => rigidbody.rotation;
        public Rigidbody Rigidbody => rigidbody;

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

        protected internal override void Serialize(ref IParameters parameters) {
            parameters.Add("vel", rigidbody.velocity);
            parameters.Add("angVel", rigidbody.angularVelocity);
        }

        protected internal override void Interpolate(EntityState from, EntityState to, float interpAmount) {
            Vector3 fromVel = from.parameters.Get<Vector3>("vel") * Time.deltaTime;
            Vector3 toVel = to.parameters.Get<Vector3>("vel") * Time.deltaTime;

            transform.position = Curves.HermiteUnclamped(from.position, fromVel, to.position, toVel, interpAmount);
            transform.rotation = Quaternion.SlerpUnclamped(from.rotation, to.rotation, interpAmount);
        }
    }
}

#endif