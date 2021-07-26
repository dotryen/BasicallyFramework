using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using UnityEngine;
using UnityEditor;
using Basically.Serialization;
using Basically.Editor.Weaver;

public static class EditorTools {
    [MenuItem("Basically/Show Storage")]
    static void ShowStorage() {
        Selection.activeObject = BasicallyCache.GetAsset();
    }

    [MenuItem("Basically/Recompile")]
    static void Recompile() {
        UnityEditor.Compilation.CompilationPipeline.RequestScriptCompilation();
    }
}
