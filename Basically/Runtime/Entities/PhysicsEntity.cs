#if PHYS_3D

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Basically.Entities {
    using Utility;

    [RequireComponent(typeof(Rigidbody))]
    public class PhysicsEntity : Entity {
        public new Rigidbody rigidbody;

        internal override Vector3 tPosition => rigidbody.position;
        internal override Quaternion tRotation => rigidbody.rotation;

        public Vector3 Velocity { get; private set; }

        protected internal override void OnServerStart() {
            rigidbody = GetComponent<Rigidbody>();
        }

        protected internal override void OnClientStart() {
            rigidbody = GetComponent<Rigidbody>();
            rigidbody.isKinematic = true;
        }

        protected internal override void OnServerTick() {
            // Debug.DrawRay(rigidbody.position, rigidbody.velocity);
        }

        protected internal override Parameters WriteData() {
            Parameters param = new Parameters();
            param.Add("vel", rigidbody.velocity);
            return param;
        }

        protected internal override void ReadData(Parameters param) {
            Velocity = (Vector3)param["vel"];
            // Debug.DrawRay(transform.position, Velocity, Color.white);
        }

        protected internal override void Interpolate(EntityState from, EntityState to, float interpAmount) {
            Vector3 fromVel = Vector3.ClampMagnitude((Vector3)from.parameters["vel"], 1);
            Vector3 toVel = Vector3.ClampMagnitude((Vector3)to.parameters["vel"], 1);

            // position = HermiteCurve.Sample(from.position, fromVel, to.position, toVel, interpAmount);
            transform.position = Vector3.Lerp(from.position, to.position, interpAmount);
            transform.rotation = Quaternion.Slerp(from.rotation, to.rotation, interpAmount);
            HermiteCurve.DrawCurve(from.position, fromVel, to.position, toVel);
        }
    }
}

#endif