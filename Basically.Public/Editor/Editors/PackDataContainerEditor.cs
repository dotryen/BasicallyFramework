using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Basically.Modding {
    [CustomEditor(typeof(PackDataContainer))]
    public class PackDataEditor : Editor {
        public override void OnInspectorGUI() {
            var data = ((PackDataContainer)target).data;
            data.name = EditorGUILayout.TextField("Name", data.name);
            data.description = EditorGUILayout.TextField("Description", data.description);
            data.author = EditorGUILayout.TextField("Author", data.author);
        }
    }
}
