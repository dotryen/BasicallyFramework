#if PHYS_2D

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Basically.Entities {
    using Utility;

    [RequireComponent(typeof(Rigidbody2D))]
    public class Physics2DEntity : Entity {
        public new Rigidbody2D rigidbody;

        internal override Vector3 tPosition => rigidbody.position;

        public Vector2 Velocity { get; private set; }

        protected internal override void OnServerStart() {
            rigidbody = GetComponent<Rigidbody2D>();
        }

        protected internal override void OnClientStart() {
            rigidbody = GetComponent<Rigidbody2D>();
            rigidbody.bodyType = RigidbodyType2D.Kinematic;
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
            Velocity = (Vector2)param["vel"];
            // Debug.DrawRay(transform.position, Velocity, Color.white);
        }

        protected internal override void Interpolate(EntityState from, EntityState to, float interpAmount) {
            Vector2 fromVel = (Vector2)from.parameters["vel"];
            Vector2 toVel = (Vector2)to.parameters["vel"];

            // transform.position = HermiteCurve.Sample(from.position, fromVel, to.position, toVel, interpAmount);
            transform.position = Vector2.Lerp(from.position, to.position, interpAmount);
            transform.rotation = Quaternion.Slerp(from.rotation, to.rotation, interpAmount);
            HermiteCurve.DrawCurve(from.position, fromVel, to.position, toVel);
        }
    }
}

#endif