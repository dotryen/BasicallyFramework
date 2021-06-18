using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Basically.Utility;

namespace Basically.Entities {
    [RequireComponent(typeof(Rigidbody))]
    public class PhysicsEntity : Entity {
        public Rigidbody rb;

        internal override Vector3 tPosition => rb.position;
        internal override Quaternion tRotation => rb.rotation;

        public Vector3 Velocity { get; private set; }
        public bool debug = false;

        protected internal override void OnServerStart() {
            rb = GetComponent<Rigidbody>();
        }

        protected internal override void OnClientStart() {
            rb = GetComponent<Rigidbody>();
            rb.isKinematic = true;
        }

        protected internal override void OnServerTick() {
            Debug.DrawRay(rb.position, rb.velocity);
        }

        protected internal override Parameters WriteData() {
            Parameters param = new Parameters();
            param.Add("vel", rb.velocity);
            return param;
        }

        protected internal override void ReadData(Parameters param) {
            Velocity = (Vector3)param["vel"];
            // Debug.DrawRay(transform.position, Velocity, Color.white);
        }

        protected internal override void Interpolate(EntityState from, EntityState to, float interpAmount) {
            Vector3 fromVel = (Vector3)from.parameters["vel"];
            Vector3 toVel = (Vector3)to.parameters["vel"];

            transform.position = HermiteCurve.Sample(from.position, fromVel, to.position, toVel, interpAmount);
            transform.rotation = Quaternion.Slerp(from.rotation, to.rotation, interpAmount);

            if (debug) Debug.Log($"From Velocity: {fromVel}, To Velocity: {toVel}, Difference: {toVel - fromVel}");
            HermiteCurve.DrawCurve(from.position, fromVel, to.position, toVel, Color.yellow);
        }
    }
}

