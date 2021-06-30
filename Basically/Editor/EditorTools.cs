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
        if (Storage.setup) return;

        foreach (var field in typeof(CompilerStorage).GetFields()) {
            field.SetValue(Storage, true);
        }

        UpdateDefines();
    }

    #region Client

    [MenuItem("Basically/Project Type/Client")]
    static void ClientDefine() {
        Storage.client = !Storage.client;
        UpdateDefines();
    }

    [MenuItem("Basically/Project Type/Client", true)]
    static bool ClientValidate() {
        Menu.SetChecked("Basically/Project Type/Client", Storage.client);
        return true;
    }

    #endregion

    #region Server

    [MenuItem("Basically/Project Type/Server")]
    static void ServerDefine() {
        Storage.server = !Storage.server;
        UpdateDefines();
    }

    [MenuItem("Basically/Project Type/Server", true)]
    static bool ServerValidate() {
        Menu.SetChecked("Basically/Project Type/Server", Storage.server);
        return true;
    }

    #endregion

    #region Entity

    [MenuItem("Basically/Features/Entities")]
    static void EntityDefine() {
        Storage.entities = !Storage.entities;
        UpdateDefines();
    }

    [MenuItem("Basically/Features/Entities", true)]
    static bool EntityValidate() {
        Menu.SetChecked("Basically/Features/Entities", Storage.entities);
        return true;
    }

    #endregion

    #region Mappacks

    [MenuItem("Basically/Features/Mappacks")]
    static void MappackDefine() {
        Storage.mappacks = !Storage.mappacks;
        UpdateDefines();
    }

    [MenuItem("Basically/Features/Mappacks", true)]
    static bool MappackValidate() {
        Menu.SetChecked("Basically/Features/Mappacks", Storage.mappacks);
        return true;
    }

    #endregion

    static void UpdateDefines() {
        BuildTargetGroup group = EditorUserBuildSettings.selectedBuildTargetGroup;
        var currentDefines = PlayerSettings.GetScriptingDefineSymbolsForGroup(group).Split(';').ToList();

        // get basically defines
        Dictionary<string, bool> basicallyDefines = new Dictionary<string, bool>();
        foreach (var field in typeof(CompilerStorage).GetFields()) {
            if (field.Name == "setup") continue;
            basicallyDefines.Add("BASICALLY_" + field.Name.ToUpper(), (bool)field.GetValue(Storage));
        }
        
        foreach (var define in basicallyDefines) {
            if (define.Value) {
                if (!currentDefines.Contains(define.Key)) {
                    currentDefines.Add(define.Key);
                }
            } else {
                if (currentDefines.Contains(define.Key)) {
                    currentDefines.Remove(define.Key);
                }
            }
        }

        PlayerSettings.SetScriptingDefineSymbolsForGroup(group, string.Join(";", currentDefines));

        // update object
        EditorUtility.SetDirty(Storage);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    #endregion
}
