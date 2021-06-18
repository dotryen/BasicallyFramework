using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using UnityEngine;
using UnityEditor;
using Basically.Serialization;

public static class EditorTools {
    [MenuItem("Basically/Show Storage")]
    static void ShowStorage() {
        Selection.activeObject = SerializerStorage.GetAsset();
    }

    [MenuItem("Basically/Refresh Storage")]
    static void RecreateStorage() {
        StorageCreator.CreateSerializerStorage();
    }

    #region Defines

    const string CLIENT_DEFINE = "BASICALLY_CLIENT";
    const string SERVER_DEFINE = "BASICALLY_SERVER";

    static CompilerStorage Storage {
        get {
            var obj = AssetDatabase.LoadAssetAtPath<CompilerStorage>("Assets/Editor/Basically/Compiler.asset");
            if (!obj) {
                obj = ScriptableObject.CreateInstance<CompilerStorage>();
                Directory.CreateDirectory("Assets/Editor/Basically");
                AssetDatabase.CreateAsset(obj, "Assets/Editor/Basically/Compiler.asset");
            }

            return obj;
        }
    }

    [InitializeOnLoadMethod]
    static void Setup() {
        if (!Storage.setup) {
            Storage.client = true;
            Storage.server = true;
            Storage.setup = true;

            UpdateDefines();
        }
    }

    [MenuItem("Basically/Defines/Client")]
    static void ToggleClientDefine() {
        Storage.client = !Storage.client;
        UpdateDefines();
    }

    [MenuItem("Basically/Defines/Client", true)]
    static bool ClientValidate() {
        Menu.SetChecked("Basically/Defines/Client", Storage.client);
        return true;
    }

    [MenuItem("Basically/Defines/Server")]
    static void ToggleServerDefine() {
        Storage.server = !Storage.server;
        UpdateDefines();
    }

    [MenuItem("Basically/Defines/Server", true)]
    static bool ServerValidate() {
        Menu.SetChecked("Basically/Defines/Server", Storage.server);
        return true;
    }

    static void UpdateDefines() {
        BuildTargetGroup group = EditorUserBuildSettings.selectedBuildTargetGroup;
        var defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(group).Split(';').ToList();

        if (defines.Contains(CLIENT_DEFINE)) {
            if (!Storage.client) {
                defines.Remove(CLIENT_DEFINE);
            }
        } else if (Storage.client) {
            defines.Add(CLIENT_DEFINE);
        }

        if (defines.Contains(SERVER_DEFINE)) {
            if (!Storage.server) {
                defines.Remove(SERVER_DEFINE);
            }
        } else if (Storage.server) {
            defines.Add(SERVER_DEFINE);
        }

        PlayerSettings.SetScriptingDefineSymbolsForGroup(group, string.Join(";", defines));

        // update object
        EditorUtility.SetDirty(Storage);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    #endregion
}
