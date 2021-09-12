using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Mono.Cecil;
using UnityEngine;
using UnityEditor;
using UAssembly = UnityEditor.Compilation.Assembly;

namespace Basically.Editor.Weaver {
    public static class WeaverMaster {
        const string GENERATED_NAMESPACE = "Basically.Generated";
        const string GENERATED_CLASS = "GenCode";

        public static TypeDefinition CurrentGeneratedClass {
            get {
                return genClass;
            }
        }

        static TypeDefinition genClass = null;
        static List<Weaver> editorWeavers = new List<Weaver>();
        static List<Weaver> playerWeavers = new List<Weaver>();

        internal static void Start() {
            var assemblies = new List<UAssembly>();
            assemblies.AddRange(AsmUtil.GetAssembliesWithReference(Platform.Editor));
            assemblies.Add(AsmUtil.GetBasicallyAssembly(Platform.Editor));

            foreach (var asm in assemblies) {
                if (!File.Exists(asm.outputPath)) continue;
                var weavers = asm.LoadAssembly(false).GetAllDescendantsOf(typeof(Weaver)).Select(x => (Weaver)Activator.CreateInstance(x));

                foreach (var weav in weavers) {
                    if (weav.IsEditor) editorWeavers.Add(weav);
                    else playerWeavers.Add(weav);
                }
            }
        }

        internal static bool Weave(string assembly, string[] depend, bool isEditor) {
            if (Path.GetFileNameWithoutExtension(assembly).StartsWith("Unity")) return true;

            // create resolver
            var resolver = new DefaultAssemblyResolver();
            resolver.AddSearchDirectory(Path.GetDirectoryName(assembly));
            resolver.AddSearchDirectory(UnityEngineDLLDirectory());

            if (depend != null) {
                foreach (var str in depend) {
                    resolver.AddSearchDirectory(str);
                }
            }

            // get definition
            var definition = AssemblyDefinition.ReadAssembly(assembly, new ReaderParameters { ReadWrite = true, ReadSymbols = true, AssemblyResolver = resolver });

            if (HasGeneratedClass(definition.MainModule)) return true;

            // work
            bool status = true;
            genClass = CreateGeneratedClass(definition.MainModule);

            foreach (var weaver in isEditor ? editorWeavers : playerWeavers) {
                weaver.Module = definition.MainModule;

                try {
                    weaver.Weave();
                } catch (Exception ex) {
                    Debug.LogError(ex);
                    status = false;
                    break;
                }
            }

            if (status) {
                definition.MainModule.Types.Add(genClass);
                definition.Write(new WriterParameters { WriteSymbols = true });
            }

            // dispose
            genClass = null;
            definition.Dispose();
            resolver.Dispose();

            return status;
        }

        internal static void End() {
            foreach (var edit in editorWeavers) {
                edit.Reset();
            }

            foreach (var play in playerWeavers) {
                play.Reset();
            }

            editorWeavers.Clear();
            playerWeavers.Clear();
        }

        private static Weaver[] GetAllWeavers(bool editor) {
            var assemblies = new List<UAssembly>();
            assemblies.AddRange(AsmUtil.GetAssembliesWithReference(Platform.Editor));
            assemblies.Add(AsmUtil.GetBasicallyAssembly(Platform.Editor));

            var weavers = new List<Weaver>();
            foreach (var asm in assemblies) {
                weavers.AddRange(asm.LoadAssembly(false).GetAllDescendantsOf(typeof(Weaver)).Select(x => (Weaver)Activator.CreateInstance(x)));
            }

            return weavers.Where(x => x.IsEditor == editor).OrderBy(y => y.Priority).ToArray();
        }

        private static string UnityEngineDLLDirectory() {
            return Path.GetDirectoryName(EditorApplication.applicationPath) + @"\Data\Managed";
        }

        private static TypeDefinition CreateGeneratedClass(ModuleDefinition module) {            
            return new TypeDefinition(GENERATED_NAMESPACE, GENERATED_CLASS,
                TypeAttributes.BeforeFieldInit | TypeAttributes.Class | TypeAttributes.AnsiClass | TypeAttributes.Public | TypeAttributes.AutoClass | TypeAttributes.Abstract | TypeAttributes.Sealed,
                module.ImportReference(typeof(object)));
        }

        private static bool HasGeneratedClass(ModuleDefinition module) {
            return module.GetTypes().Any(td => td.Namespace == GENERATED_NAMESPACE && td.Name == GENERATED_CLASS);
        }
    }
}
