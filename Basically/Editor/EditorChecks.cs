using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Basically.Editor {
    using Weaver;

    internal static class EditorChecks {
        [InitializeOnLoadMethod]
        static void OnLoad() {
            EditorApplication.playModeStateChanged += PlayModeCheck;
        }

        static void PlayModeCheck(PlayModeStateChange change) {
            if (change == PlayModeStateChange.ExitingEditMode) {
                if (WeaverControls.WeaveFailed) {
                    WeaverHook.WeaveExistingAssemblies();

                    if (WeaverControls.WeaveFailed) {
                        Debug.LogError("Attempts at weaving have failed. Cannot enter play mode until issues are resolved.");
                        EditorApplication.isPlaying = false;
                    }
                }
            }

#if UNITY_2019_3_OR_NEWER
            if (EditorSettings.enterPlayModeOptionsEnabled) {
                Debug.LogError("Enter Play mode options are not supported.");
                EditorApplication.isPlaying = false;
            }
#endif
        }
    }
}

