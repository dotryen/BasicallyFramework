using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.Compilation;

namespace Basically.Editor.Weaver {
    internal static class WeaverHook {
        static bool currentWeaveFailed;

        [InitializeOnLoadMethod]
        static void OnLoad() {
            // return;

            WeaverControls.Initialize();

            CompilationPipeline.compilationStarted += (obj) => {
                if (!WeaverControls.Paused) {
                    currentWeaveFailed = false;
                    WeaverMaster.Start();
                }
            };

            CompilationPipeline.assemblyCompilationFinished += (ass, mes) => {
                if (WeaverControls.Paused || currentWeaveFailed) {
                    WeaverControls.TriedWeave = true;
                } else {
                    OnAssemblyCompile(ass, mes);
                }
            };

            CompilationPipeline.compilationFinished += (objElectricBoogaloo) => {
                if (!WeaverControls.Paused) {
                    WeaverMaster.End();
                    EditorUtility.RequestScriptReload();
                }
            };

            if (!WeaverControls.Ready) WeaveExistingAssemblies();
            Debug.Log("Weaver callbacks successfully added.");
        }

        public static void WeaveExistingAssemblies() {
            currentWeaveFailed = false;
            WeaverMaster.Start();

            foreach (var asm in CompilationPipeline.GetAssemblies()) {
                if (currentWeaveFailed) break;
                OnAssemblyCompile(asm.outputPath, new CompilerMessage[0]);
            }

            WeaverMaster.End();
        }

        #region Hook

        public const string RUNTIME_DLL = "Basically";
        public const string EDITOR_DLL = RUNTIME_DLL + ".Editor";

        internal static void OnAssemblyCompile(string assemblyPath, CompilerMessage[] messages) {
            if (!File.Exists(AsmUtil.GetBasicallyAssembly(Platform.Player).outputPath)) return;

            bool isEditor = false;

            void Work(string include) {
                HashSet<string> depend = GetDependencies(assemblyPath);
                depend.Add(Path.GetDirectoryName(UnityCoreModule()));
                if (include != null) depend.Add(Path.GetDirectoryName(include));

                bool status = true;
                try {
                    status = WeaverMaster.Weave(assemblyPath, depend.ToArray(), isEditor);
                } catch (System.Exception) {
                    status = false;
                }

                WeaverControls.WeaveFailed = !status;
                currentWeaveFailed = !status;
            }

            void Check(string asm) {
                if (Path.GetFileNameWithoutExtension(assemblyPath) == asm) {
                    Work(null);
                } else {
                    if (!File.Exists(NameToPath(asm))) return;
                    Work(NameToPath(asm));
                }
            }

            if (messages.Any(x => x.type == CompilerMessageType.Error)) return;

            if (assemblyPath.Contains(".Editor") || assemblyPath.Contains("-Editor")) {
                // Editor branch
                isEditor = true;
                Check(EDITOR_DLL);
            } else {
                // Player branch
                Check(RUNTIME_DLL);
            }
        }

        static string UnityCoreModule() {
            return UnityEditorInternal.InternalEditorUtility.GetEngineCoreModuleAssemblyPath();
        }

        static HashSet<string> GetDependencies(string assemblyPath) {
            HashSet<string> depend = new HashSet<string> { Path.GetDirectoryName(assemblyPath) };

            foreach (Assembly asm in CompilationPipeline.GetAssemblies()) {
                if (asm.outputPath == assemblyPath) {
                    foreach (string refer in asm.compiledAssemblyReferences) {
                        depend.Add(Path.GetDirectoryName(refer));
                    }
                }
            }

            return depend;
        }

        static string NameToPath(string name) {
            return $"Library/ScriptAssemblies/{name}.dll";
        }

        #endregion
    }
}
