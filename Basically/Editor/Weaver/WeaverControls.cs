using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Basically.Editor.Weaver {
    internal static class WeaverControls {
        public const string MENU_BUTTON = "Basically/Weaver/Pause Weaver";
        public const string WEAVER_READY = "BASIC_WEAVER_READY";
        public const string WEAVER_PAUSE = "BASIC_WEAVER_PAUSE";
        public const string WEAVER_TRY = "BASIC_WEAVER_TRY";
        public const string WEAVER_FAILED = "BASIC_WEAVER_FAIL";

        /// <summary>
        /// Used to prevent reloading of values.
        /// </summary>
        public static bool Ready {
            get {
                return SessionState.GetBool(WEAVER_READY, false);
            }

            set {
                SessionState.SetBool(WEAVER_READY, value);
            }
        }

        public static bool Paused {
            get {
                return SessionState.GetBool(WEAVER_PAUSE, false);
            }

            set {
                SessionState.SetBool(WEAVER_PAUSE, value);
            }
        }

        public static bool TriedWeave {
            get {
                return SessionState.GetBool(WEAVER_TRY, false);
            }

            set {
                SessionState.SetBool(WEAVER_TRY, value);
            }
        }

        public static bool WeaveFailed {
            get {
                return SessionState.GetBool(WEAVER_FAILED, false);
            }

            set {
                SessionState.SetBool(WEAVER_FAILED, value);
            }
        }

        public static void Initialize() {
            if (Ready) return;

            Paused = false;
            TriedWeave = false;
            WeaveFailed = false;
            Ready = true;
        }

        [MenuItem(MENU_BUTTON)]
        static void PauseButton() {
            if (!Paused) {
                if (!EditorUtility.DisplayDialog("Are you sure?", "Pausing will break most functions and buttons if recompiled", "Do Not Pause", "Pause")) {
                    Paused = true;
                }
            } else {
                Paused = false;

                if (TriedWeave) {
                    WeaverHook.WeaveExistingAssemblies();
                    TriedWeave = false;
                }
            }
        }

        [MenuItem(MENU_BUTTON, true)]
        static bool PauseValidate() {
            Menu.SetChecked(MENU_BUTTON, Paused);
            return true;
        }

        [MenuItem("Basically/Weaver/Weave")]
        static void DoWeave() {
            WeaverHook.WeaveExistingAssemblies();
        }
    }
}
