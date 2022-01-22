using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Basically.Utility;

namespace Basically.Entities {
    /// <summary>
    /// In-game objects that synced from the server and interpolated.
    /// </summary>
    [AddComponentMenu("Basically/Entities/Entity")]
    public class Entity : MonoBehaviour {
        public ushort ID { get; internal set; }

        public virtual Vector3 Position {
            get {
                return transform.position;
            }

            set {
                transform.position = value;
            }
        }
        public virtual Quaternion Rotation {
            get {
                return transform.rotation;
            }

            set {
                transform.rotation = value;
            }
        }
        
        #region Server

        /// <summary>
        /// Called when a map is loaded on the server.
        /// </summary>
        protected internal virtual void OnServerStart() {

        }

        /// <summary>
        /// Called for each tick on the server.
        /// </summary>
        protected internal virtual void OnServerTick() {

        }

        #endregion

        #region Client

        /// <summary>
        /// Called when a map is loaded on the client.
        /// </summary>
        protected internal virtual void OnClientStart() {

        }

        /// <summary>
        /// Called for each frame on the client.
        /// </summary>
        protected internal virtual void OnClientUpdate() {

        }

        /// <summary>
        /// Called after update.
        /// </summary>
        protected internal virtual void OnClientLateUpdate() {

        }

        /// <summary>
        /// Called for each tick on the client.
        /// </summary>
        protected internal virtual void OnClientTick() {

        }

        #endregion

        /// <summary>
        /// Serialize the entity for transfer.
        /// </summary>
        /// <param name="parameters">Parameters to add values to.</param>
        protected internal virtual void Serialize(ref IParameters parameters) {
            
        }

        /// <summary>
        /// The opposite of serializing.
        /// </summary>
        /// <param name="parameters">Parameters with the entities data.</param>
        protected internal virtual void Deserialize(IParameters parameters) {

        }

        /// <summary>
        /// Interpolate between entity states.
        /// </summary>
        /// <param name="from">State to interpolate from.</param>
        /// <param name="to">State to interpolate to.</param>
        /// <param name="interpAmount">Interpolation amount.</param>
        protected internal virtual void Interpolate(EntityState from, EntityState to, float interpAmount) {
            transform.SetPositionAndRotation(Vector3.LerpUnclamped(from.position, to.position, interpAmount), Quaternion.SlerpUnclamped(from.rotation, to.rotation, interpAmount));
            // Debug.DrawLine(from.position, to.position, Color.yellow);
        }

        #region Weaver Methods

        /// <summary>
        /// DO NOT EDIT, THESE ARE GENERATED AT COMPILE TIME
        /// </summary>
        protected internal virtual void Weaver_GetNetVars(ref IParameters parameters) {

        }

        /// <summary>
        /// DO NOT EDIT, THESE ARE GENERATED AT COMPILE TIME
        /// </summary>
        protected internal virtual void Weaver_SetNetVars(IParameters parameters) {

        }

        #endregion
    }
}
