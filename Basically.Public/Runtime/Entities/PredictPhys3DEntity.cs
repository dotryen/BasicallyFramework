using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Basically.Entities {
    using Utility;

    /// <summary>
    /// Predictable 3D physics entity.
    /// </summary>
    [AddComponentMenu("Basically/Entities/Predictable/Predictable Phys3D Entity")]
    public class PredictPhys3DEntity : PredictableEntity {
        private new Rigidbody rigidbody;

        public override Vector3 Position => rigidbody.position;
        public override Quaternion Rotation => rigidbody.rotation;
        public Rigidbody Rigidbody => rigidbody;

        public Vector3 Velocity { get; set; }

        protected internal override void OnServerStart() {
            base.OnServerStart();
            rigidbody = GetComponent<Rigidbody>();
        }

        protected internal override void OnClientStart() {
            base.OnClientStart();
            rigidbody = GetComponent<Rigidbody>();
        }

        protected internal override void OnServerTick() {
            base.OnServerTick();
            Velocity = rigidbody.velocity;
        }

        protected internal override void Serialize(ref IParameters parameters) {
            parameters.Add("vel", rigidbody.velocity);
            parameters.Add("angVel", rigidbody.angularVelocity);
        }

        protected internal override void Record(ref IParameters parameters) {
            Serialize(ref parameters);
        }

        protected internal override void InterpFunc(EntityState from, EntityState to, float interpAmount) {
            Vector3 fromVel = from.parameters.Get<Vector3>("vel") * Time.deltaTime;
            Vector3 toVel = to.parameters.Get<Vector3>("vel") * Time.deltaTime;

            transform.position = Curves.HermiteUnclamped(from.position, fromVel, to.position, toVel, interpAmount);
            transform.rotation = Quaternion.SlerpUnclamped(from.rotation, to.rotation, interpAmount);
        }

        protected internal override uint NeedsCorrection(EntityState client, EntityState server) {
            var posErr = server.position - client.position;
            var rotErr = 1f - Quaternion.Dot(server.rotation, client.rotation);

            if (posErr.sqrMagnitude > positionErrorThreshold || rotErr > rotationErrorThreshold) {
                prevPosCache = rigidbody.position + PositionError;
                prevRotCache = rigidbody.rotation * RotationError;

                rigidbody.position = server.position;
                rigidbody.rotation = server.rotation;
                rigidbody.velocity = server.parameters.Get<Vector3>("vel");
                rigidbody.angularVelocity = server.parameters.Get<Vector3>("angVel");

                return BGlobals.Tick - server.Tick;
            }

            return 0;
        }
    }
}
