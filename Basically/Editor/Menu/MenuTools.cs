using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public static class EditorTools {
    [MenuItem("Basically/Recompile")]
    static void Recompile() {
        UnityEditor.Compilation.CompilationPipeline.RequestScriptCompilation();
    }

    [MenuItem("Basically/Reload")]
    static void Reload() {
        EditorUtility.RequestScriptReload();
    }
}
