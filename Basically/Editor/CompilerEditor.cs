using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(CompilerStorage))]
public class CompilerEditor : Editor {
    public override void OnInspectorGUI() {
        EditorGUILayout.HelpBox("Used for compiler defines.", MessageType.Warning);
    }
}
