using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System.IO;
using UnityEditor;

namespace Basically.Editor {
    public static class CompilerDefines {
        const string FILE_PATH = "Library/BasicallyDefine";
        const string DEFINE_PREFIX = "BASICALLY_";

        static bool ready = false;

        public static bool Setup { get; private set; } = false;

        public static bool client;
        // [MenuCheckbox("Basically/DefineTest/ClientFake")]
        public static bool ClientFake {
            get {
                return client;
            }

            internal set {
                client = value;
                if (ready) Update();
            }
        }

        // [InitializeOnLoadMethod]
        static void OnLoad() {
            if (!File.Exists(FILE_PATH)) {
                File.Create(FILE_PATH);

                foreach (var define in GetAllDefines()) {
                    define.SetValue(null, true);
                }

                Update();
            } else {
                Load();
            }

            ready = true;
        }

        static void Update() {
            Dictionary<string, bool> dic = new Dictionary<string, bool>();

            foreach (var prop in GetAllDefines()) {
                dic.Add(prop.Name.ToUpper(), (bool)prop.GetValue(null));
            }

            SetUnityDefines(dic);
            Save(dic);
        }

        static void SetUnityDefines(Dictionary<string, bool> dic) {
            BuildTargetGroup group = EditorUserBuildSettings.selectedBuildTargetGroup;
            var currentDefines = PlayerSettings.GetScriptingDefineSymbolsForGroup(group).Split(';').ToList();

            foreach (var define in dic) {
                var full = DEFINE_PREFIX + define.Key;

                if (define.Value) {
                    if (!currentDefines.Contains(full)) {
                        currentDefines.Add(full);
                    }
                } else {
                    if (currentDefines.Contains(full)) {
                        currentDefines.Remove(full);
                    }
                }
            }

            PlayerSettings.SetScriptingDefineSymbolsForGroup(group, string.Join(";", currentDefines));
        }

        static void Load() {
            var defines = File.ReadAllText(FILE_PATH).Split(new[] { System.Environment.NewLine }, System.StringSplitOptions.None);
            var properties = GetAllDefines();

            foreach (var define in defines) {
                var shit = define.Split(' ');
                if (shit.Length != 2) continue;

                var prop = properties.First(x => x.Name.ToUpper() == shit[0]);
                prop.SetValue(null, shit[0] == "1" ? true : false);
            }
        }

        static void Save(Dictionary<string, bool> dic) {
            var list = dic.Select(x => $"{x.Key} {(x.Value ? "1" : "0")}");
            File.WriteAllText(FILE_PATH, string.Join("\n", list));
        }

        static PropertyInfo[] GetAllDefines() {
            return typeof(CompilerDefines).GetProperties().Where(x => x.Name != "Setup").ToArray();
        }
    }
}
