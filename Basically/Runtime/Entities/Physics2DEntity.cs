#if PHYS_2D

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Basically.Entities {
    using Utility;
    using Networking;

    [RequireComponent(typeof(Rigidbody2D))]
    public class Physics2DEntity : Entity {
        public new Rigidbody2D rigidbody;

        public override Vector3 Position => rigidbody.position;

        [NetVariable]
        public Vector2 Velocity { get; set; }

        protected internal override void OnServerStart() {
            rigidbody = GetComponent<Rigidbody2D>();
        }

        protected internal override void OnClientStart() {
            rigidbody = GetComponent<Rigidbody2D>();
            rigidbody.bodyType = RigidbodyType2D.Kinematic;
        }

        protected internal override void OnServerTick() {
            Velocity = rigidbody.velocity;
            // Debug.DrawRay(rigidbody.position, rigidbody.velocity);
        }

        protected internal override void Interpolate(EntityState from, EntityState to, float interpAmount) {
            base.Interpolate(from, to, interpAmount);
            return;

            Vector2 fromVel = (Vector2)from.parameters["vel"];
            Vector2 toVel = (Vector2)to.parameters["vel"];

            transform.position = HermiteCurve.Sample(from.position, fromVel, to.position, toVel, interpAmount);
            // transform.position = Vector2.Lerp(from.position, to.position, interpAmount);
            transform.rotation = Quaternion.Slerp(from.rotation, to.rotation, interpAmount);
            HermiteCurve.DrawCurve(from.position, fromVel, to.position, toVel);
        }
    }
}

#endif