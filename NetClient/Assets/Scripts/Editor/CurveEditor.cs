using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Curve))]
public class CurveEditor : Editor {
    Curve curve => (Curve)target;

    public override void OnInspectorGUI() {
        base.OnInspectorGUI();

        EditorGUILayout.LabelField($"Start tangent distance: {curve.start.Tangent.magnitude}");
        EditorGUILayout.LabelField($"End tangent distance: {curve.end.Tangent.magnitude}");
    }
}
