using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using Mono.Cecil;
using UnityEditor.Compilation;
using UAssembly = UnityEditor.Compilation.Assembly;
using SysAssembly = System.Reflection.Assembly;

namespace Basically.Editor.Weaver {
    internal static class AsmUtil {
        // GETS ASSEMBLIES FROM PIPELINE
        /// <summary>
        /// Tool to get Unity assemblies.
        /// </summary>
        /// <param name="platform">Which platform to search through.</param>
        /// <returns>Assemblies compiled by Unity.</returns>
        public static UAssembly[] GetAssemblies(Platform platform) {
            var assemblies = CompilationPipeline.GetAssemblies();
            return assemblies.Where(x => x.flags == (AssemblyFlags)platform).ToArray();
        }

        public static UAssembly GetAssemblyByName(string name) {
            var assemblies = CompilationPipeline.GetAssemblies();
            return assemblies.FirstOrDefault(x => x.name == name);
        }

        public static UAssembly GetAssemblyByPath(string path) {
            var assemblies = CompilationPipeline.GetAssemblies();
            return assemblies.FirstOrDefault(x => x.outputPath == path);
        }

        /// <summary>
        /// Finds the Basically assembly in Unity form.
        /// </summary>
        /// <param name="platform">Which platform to search through.</param>
        /// <returns>Basically assembly.</returns>
        public static UAssembly GetBasicallyAssembly(Platform platform) {
            return GetBasicallyAssembly(GetAssemblies(platform), platform);
        }

        public static UAssembly GetBasicallyAssembly(UAssembly[] assemblies, Platform platform) {
            if (platform == Platform.Player) return assemblies.First(x => x.name == WeaverHook.RUNTIME_DLL);
            else return assemblies.First(x => x.name == WeaverHook.EDITOR_DLL);
        }

        /// <summary>
        /// Gets assemblies that belong to the user. (Not Unity or Basically)
        /// </summary>
        /// <param name="platform">Which platform to search through.</param>
        /// <returns>All user assemblies.</returns>
        public static UAssembly[] GetUserAssemblies(Platform platform) {
            return GetUserAssemblies(GetAssemblies(platform));
        }

        public static UAssembly[] GetUserAssemblies(UAssembly[] assemblies) {
            return assemblies.Where(x => IsUserAssembly(x.name)).ToArray();
        }

        public static bool IsUserAssembly(string name) {
            return !name.StartsWith("Unity") && name != WeaverHook.RUNTIME_DLL && name != WeaverHook.EDITOR_DLL;
        }

        /// <summary>
        /// Gets assemblies that reference Basically.
        /// </summary>
        /// <param name="platform">Which platform to search through.</param>
        /// <returns>Assemblies that reference Basically.</returns>
        public static UAssembly[] GetAssembliesWithReference(Platform platform) {
            var assemblies = GetAssemblies(platform);
            var user = GetUserAssemblies(assemblies);
            var basicallyAss = GetBasicallyAssembly(assemblies, platform);

            return user.Where(y => y.assemblyReferences.Contains(basicallyAss)).ToArray();
        }

        public static SysAssembly LoadAssembly(this UAssembly assembly, bool inTemp) {
            if (inTemp) return SysAssembly.LoadFile($"Temp/{assembly.name}.dll");
            else return SysAssembly.LoadFile(assembly.outputPath);
        }

        public static AssemblyDefinition LoadDefinition(this UAssembly assembly) {
            return AssemblyDefinition.ReadAssembly(assembly.outputPath);
        }
    }
}
