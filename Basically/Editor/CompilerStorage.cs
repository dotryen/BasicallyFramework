using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using UnityEngine;
using UnityEditor;

namespace Basically.Editor {
    internal class CompilerStorage : ScriptableObject {
        public bool setup = false;

        public bool client;
        public bool server;
        public bool entities;
        public bool mappacks;

        #region Menu Stuff

        static CompilerStorage Storage {
            get {
                var obj = AssetDatabase.LoadAssetAtPath<CompilerStorage>("Assets/Editor/Basically/Compiler.asset");
                if (!obj) {
                    obj = CreateInstance<CompilerStorage>();
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

        [MenuCheckbox("Basically/Project Type/Client")]
        public static bool ClientProp {
            get {
                return Storage.client;
            }

            set {
                Storage.client = value;
                UpdateDefines();
            }
        }

        [MenuCheckbox("Basically/Project Type/Server")]
        public static bool ServerProp {
            get {
                return Storage.server;
            }

            set {
                Storage.server = value;
                UpdateDefines();
            }
        }

        [MenuCheckbox("Basically/Features/Entities")]
        public static bool EntityProp {
            get {
                return Storage.entities;
            }

            set {
                Storage.entities = value;
                UpdateDefines();
            }
        }

        [MenuCheckbox("Basically/Features/Mappacks")]
        public static bool MapProp {
            get {
                return Storage.mappacks;
            }

            set {
                Storage.mappacks = value;
                UpdateDefines();
            }
        }

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
}