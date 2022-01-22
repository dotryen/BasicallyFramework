using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Basically.Frameworks {
    using Networking;
    using Utility;

    /// <summary>
    /// What Basically's runtime is based (get it) on.
    /// </summary>
    public abstract class Framework : MonoBehaviour {
        static Framework instance;

        internal uint tick;
        internal bool running;

        public static Framework Instance => instance;
        public bool Running => running;

        private void Awake() {
            if (instance != null) {
                Destroy(this);
                return;
            } else {
                instance = this;
            }
            DontDestroyOnLoad(gameObject);
        }

        private void OnDestroy() {
            if (instance == this) {
                instance = null;
            }
            StopFramework();
        }

        private void FixedUpdate() {
            if (!running) return;

            SimulatePrePhys();
#if PHYS_3D
            Physics.Simulate(Time.fixedDeltaTime);
#endif
#if PHYS_2D
            Physics2D.Simulate(Time.fixedDeltaTime);
#endif
            SimulatePostPhys();
            tick++;
            AfterTickUpdate();
        }

        public void StartFramework() {
            InitializeUnitySettings();
            OnStart();
            running = true;
        }

        public void StopFramework() {
            if (!running) return;
            OnStop();
            running = false;
        }

        internal abstract void OnStart();

        internal abstract void OnStop();

        internal abstract void SimulatePrePhys();

        internal abstract void SimulatePostPhys();

        internal virtual void AfterTickUpdate() {
            BGlobals.Tick = tick;
            BGlobals.PredictedTick = tick;
        }

        private void InitializeUnitySettings() {
#if PHYS_3D
            Physics.autoSimulation = false;
#endif
#if PHYS_2D
            Physics2D.autoSimulation = false;
#endif
            Time.fixedDeltaTime = BGlobals.TICK;
            Time.maximumDeltaTime = BGlobals.TICK;
        }
    }

    public static class FrameExt {
        /// <summary>
        /// Run Coroutine on current framework.
        /// </summary>
        /// <param name="rout">Routine to run.</param>
        /// <returns>A coroutine.</returns>
        public static Coroutine Run(this IEnumerator rout) {
            if (Framework.Instance == null) return null;
            return Framework.Instance.StartCoroutine(rout);
        }

        /// <summary>
        /// Stops the Coroutine on current framework.
        /// </summary>
        /// <param name="rout">Routine to stop.</param>
        public static void Stop(this IEnumerator rout) {
            if (Framework.Instance == null) return;
            Framework.Instance.StopCoroutine(rout);
        }

        /// <summary>
        /// Stops the Coroutine on current framework.
        /// </summary>
        /// <param name="coroutine">Coroutine to stop. Should belong to framework.</param>
        public static void Stop(this Coroutine coroutine) {
            if (Framework.Instance == null) return;
            Framework.Instance.StopCoroutine(coroutine);
        }
    }
}
