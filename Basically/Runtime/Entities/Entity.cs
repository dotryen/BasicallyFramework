using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Basically.Utility;

namespace Basically.Entities {
    public class Entity : MonoBehaviour {
        public int ID { get; internal set; }

        internal virtual Vector3 tPosition => transform.position;
        internal virtual Quaternion tRotation => transform.rotation;

#if BASICALLY_CLIENT
        internal int lastTickUpdated = 0;
        public int LastTickReceived => lastTickUpdated;
#endif
        
        #region Server

        protected internal virtual void OnServerStart() {

        }

        protected internal virtual void OnServerTick() {

        }

        #endregion

        #region Client

        protected internal virtual void OnClientStart() {

        }

        protected internal virtual void OnClientUpdate() {

        }

        protected internal virtual void OnClientLateUpdate() {

        }

        protected internal virtual void OnClientTick() {

        }

        #endregion

        protected internal virtual Parameters WriteData() {
            return null;
        }

        protected internal virtual void ReadData(Parameters param) {

        }

        protected internal virtual void Interpolate(EntityState from, EntityState to, float interpAmount) {
            transform.position = Vector3.Lerp(from.position, to.position, interpAmount);
            transform.rotation = Quaternion.Slerp(from.rotation, to.rotation, interpAmount);
            Debug.DrawLine(from.position, to.position, Color.yellow);
        }
    }
}
