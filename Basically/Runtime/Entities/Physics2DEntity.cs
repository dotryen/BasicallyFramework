#if PHYS_2D

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Basically.Entities {
    using Utility;
    using Networking;

    /// <summary>
    /// A 2D physics entity
    /// </summary>
    [AddComponentMenu("Basically/Entities/Phys2D Entity")]
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

        protected internal override Parameters Serialize() {
            var param = new Parameters(1);
            param.Add("vel", rigidbody.velocity);
            return param;
        }

        protected internal override void Interpolate(EntityState from, EntityState to, float interpAmount) {
            Vector2 fromVel = from.parameters.Get<Vector2>("vel") * Time.deltaTime;
            Vector2 toVel = to.parameters.Get<Vector2>("vel") * Time.deltaTime;

            transform.position = Curves.Hermite(from.position, fromVel, to.position, toVel, interpAmount);
            transform.rotation = Quaternion.Slerp(from.rotation, to.rotation, interpAmount);
            // HermiteCurve.DrawCurve(from.position, fromVel, to.position, toVel);
        }
    }
}

#endif