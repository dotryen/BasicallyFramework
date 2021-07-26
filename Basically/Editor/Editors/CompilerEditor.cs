using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Basically.Editor;

[CustomEditor(typeof(CompilerStorage))]
public class CompilerEditor : Editor {
    public override void OnInspectorGUI() {
        EditorGUILayout.HelpBox("Used for compiler defines.", MessageType.Warning);
    }
}
