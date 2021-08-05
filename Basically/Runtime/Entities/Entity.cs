using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Basically.Utility;

namespace Basically.Entities {
    /// <summary>
    /// In-Game objects that synced from the server and interpolated.
    /// </summary>
    public class Entity : MonoBehaviour {
        public int ID { get; internal set; }

        public virtual Vector3 Position => transform.position;
        public virtual Quaternion Rotation => transform.rotation;
        
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

        protected internal virtual void Interpolate(EntityState from, EntityState to, float interpAmount) {
            transform.position = Vector3.Lerp(from.position, to.position, interpAmount);
            transform.rotation = Quaternion.Slerp(from.rotation, to.rotation, interpAmount);
            Debug.DrawLine(from.position, to.position, Color.yellow);
        }
    }
}
