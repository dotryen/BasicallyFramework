#if PHYS_3D

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Basically.Entities {
    using Utility;
    using Networking;

    [RequireComponent(typeof(Rigidbody))]
    public class PhysicsEntity : Entity {
        public new Rigidbody rigidbody;

        public override Vector3 Position => rigidbody.position;
        public override Quaternion Rotation => rigidbody.rotation;

        [NetVariable]
        public Vector3 Velocity { get; set; }

        protected internal override void OnServerStart() {
            rigidbody = GetComponent<Rigidbody>();
        }

        protected internal override void OnClientStart() {
            rigidbody = GetComponent<Rigidbody>();
            rigidbody.isKinematic = true;
        }

        protected internal override void OnServerTick() {
            Velocity = rigidbody.velocity;
            // Debug.DrawRay(rigidbody.position, rigidbody.velocity);
        }

        protected internal override void Interpolate(EntityState from, EntityState to, float interpAmount) {
            base.Interpolate(from, to, interpAmount);
            return;

            Vector3 fromVel = Vector3.ClampMagnitude((Vector3)from.parameters["vel"], 1);
            Vector3 toVel = Vector3.ClampMagnitude((Vector3)to.parameters["vel"], 1);

            transform.position = HermiteCurve.Sample(from.position, fromVel, to.position, toVel, interpAmount);
            // transform.position = Vector3.Lerp(from.position, to.position, interpAmount);
            transform.rotation = Quaternion.Slerp(from.rotation, to.rotation, interpAmount);
            HermiteCurve.DrawCurve(from.position, fromVel, to.position, toVel);
        }
    }
}

#endif