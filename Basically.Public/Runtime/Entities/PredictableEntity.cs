using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Basically.Entities {
    using Utility;

    /// <summary>
    /// A special kind of entity that adds prediction functionality on the client.
    /// </summary>
    public class PredictableEntity : Entity {
        const int BUFFER_SIZE = BGlobals.TICK_RATE;

        public bool doPrediction = false;
        public Vector3 PositionError { get; protected set; }
        public Quaternion RotationError { get; protected set; }

        public float positionErrorThreshold = 0.0001f;
        public float rotationErrorThreshold = 0.0001f;

        [Tooltip("All things visual related should be contained in this object as it is corrected for smoothness.")]
        public Transform visualTransform;

        private EntityState[] clientStates = new EntityState[BUFFER_SIZE];
        private bool interpolateErrors; // false if correction errors

        protected Vector3 prevPosCache;
        protected Quaternion prevRotCache;

        protected internal override void OnServerTick() {
            Predict();
        }

        protected internal override void OnClientTick() {
            var slot = BGlobals.Tick % BUFFER_SIZE;
            clientStates[slot] = RecordSelf();

            if (doPrediction) Predict();

            // Gradually correct error
            PositionError *= 0.9f;
            RotationError = Quaternion.Slerp(RotationError, Quaternion.identity, 0.1f);

            // Correct visually not physically (Deceiving your eyes in a literal sense)
            if (visualTransform) UpdateError();
        }

        /// <summary>
        /// Prediction method
        /// </summary>
        protected internal virtual void Predict() {

        }

        /// <summary>
        /// Method to smooth error
        /// </summary>
        protected internal virtual void UpdateError() {
            visualTransform.transform.position = transform.position + PositionError;
            visualTransform.transform.rotation = transform.rotation * RotationError;
        }

        /// <summary>
        /// Used to save data for prediction
        /// </summary>
        /// <param name="parameters">Supplied parameters</param>
        protected internal virtual void Record(ref IParameters parameters) {

        }

        protected internal override sealed void Interpolate(EntityState from, EntityState to, float interpAmount) {
            if (doPrediction) {
                interpolateErrors = false;
                return; // Prediction and interpolation don't mix.
            }

            if (!interpolateErrors) {
                var prevPos = transform.position + PositionError;
                var prevRot = transform.rotation * RotationError;

                InterpFunc(from, to, interpAmount);

                PositionError = transform.position - prevPos;
                RotationError = Quaternion.Inverse(transform.rotation) * prevRot;

                interpolateErrors = true;
            } else {
                InterpFunc(from, to, interpAmount);
            }
        }

        /// <summary>
        /// Replaces interpolate for extra features.
        /// </summary>
        /// <param name="from">State to interpolate from.</param>
        /// <param name="to">State to interpolate to.</param>
        /// <param name="interpAmount">Interpolation amount.</param>
        protected internal virtual void InterpFunc(EntityState from, EntityState to, float interpAmount) {
            base.Interpolate(from, to, interpAmount);
        }

        #region Prediction Correction

        #region Internal Callbacks

        internal uint NeedsCorrectionInternal(EntityState server) {
            return NeedsCorrection(GetClientState(server.Tick), server);
        }

        internal void CorrectInternal(EntityState server) {
            CorrectSimulation(GetClientState(server.Tick), server);
        }

        #endregion

        /// <summary>
        /// Decides if entity will be corrected. If needed, do preparations here.
        /// </summary>
        /// <param name="client">Client state.</param>
        /// <param name="server">Server state.</param>
        /// <returns>Number of ticks to rewind.</returns>
        protected internal virtual uint NeedsCorrection(EntityState client, EntityState server) {
            var posErr = server.position - client.position;
            var rotErr = 1f - Quaternion.Dot(server.rotation, client.rotation);

            if (posErr.sqrMagnitude > 0.0000001f || rotErr > 0.00001f) {
                prevPosCache = transform.position + PositionError;
                prevRotCache = transform.rotation * RotationError;

                transform.SetPositionAndRotation(server.position, server.rotation);

                return BGlobals.Tick - server.Tick;
            }

            return 0;
        }

        /// <summary>
        /// How to correct prediction errors.
        /// </summary>
        /// <param name="client">Client state.</param>
        /// <param name="server">Server state.</param>
        protected internal void CorrectSimulation(EntityState client, EntityState server) {
            Predict();
            RecordSelf();
        }

        /// <summary>
        /// Finalize prediction and set variables.
        /// </summary>
        protected internal virtual void CorrectFinalize() {
            if ((prevPosCache - transform.position).sqrMagnitude >= 4f) {
                PositionError = Vector3.zero;
                RotationError = Quaternion.identity;
            } else {
                PositionError = prevPosCache - transform.position;
                RotationError = Quaternion.Inverse(transform.rotation) * prevRotCache;
            }
        }

        #endregion

        private EntityState GetClientState(uint tick) {
            return clientStates[tick % BUFFER_SIZE];
        }

        private EntityState RecordSelf() {
            IParameters parameters = new Parameters(0);
            Record(ref parameters);

            return new EntityState() {
                Tick = BGlobals.Tick,
                position = Position,
                rotation = Rotation,
                parameters = parameters
            };
        }

        // private T GetInBufferByTick<T>(T[] buffer, uint tick) {
        //     return buffer[tick % buffer.Length];
        // }

        // I want this to be as speedy as possible, so we implement weaver methods and leave them empty
        #region Weaver Methods

        /// <summary>
        /// DO NOT EDIT, THESE ARE GENERATED AT COMPILE TIME
        /// </summary>
        protected internal virtual void Weaver_GetPredictableVars(ref IParameters parameters) {

        }

        /// <summary>
        /// DO NOT EDIT, THESE ARE GENERATED AT COMPILE TIME
        /// </summary>
        protected internal virtual void Weaver_SetPredictableVars(IParameters parameters) {

        }

        #endregion
    }
}
