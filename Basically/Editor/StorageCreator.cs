using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditor.Compilation;
using Basically.Serialization;
using Basically.Networking;
using Basically.Utility;

using SysAssembly = System.Reflection.Assembly;
using UAssembly = UnityEditor.Compilation.Assembly;

internal static class StorageCreator {
    const bool DEBUG = false;

    [InitializeOnLoadMethod]
    static void OnLoad() {
        CompilationPipeline.compilationFinished += (obj) => {
            CreateSerializerStorage();
        };
    }

    public static void CreateSerializerStorage() {
        SerializerStorage storage = SerializerStorage.GetAsset();
        SysAssembly[] assemblies = null;

        { // assemblies
            var unityAss = CompilationPipeline.GetAssemblies(AssembliesType.PlayerWithoutTestAssemblies).Where(x => !x.name.StartsWith("Unity")).ToArray();
            assemblies = new SysAssembly[unityAss.Length];
            for (int i = 0; i < unityAss.Length; i++) {
                assemblies[i] = SysAssembly.LoadFile(unityAss[i].outputPath);
            }
        }

        { // serializers
            Dictionary<Type, Type> dic = new Dictionary<Type, Type>();

            foreach (SysAssembly ass in assemblies) {
                foreach (var type in GetAllGenericDescendantsOf(ass, typeof(Serializer<>))) {
                    var arguments = type.BaseType.GetGenericArguments();
                    dic.Add(arguments[0], type);
                    if (DEBUG) Debug.Log($"SERIALIZER PROCESS: Type: {arguments[0].FullName}, Serializer: {type.FullName}");
                }
            }

            storage.SetupSerializers(dic);
        }

        { // messages
            List<Type> messages = new List<Type>();
            foreach (SysAssembly ass in assemblies) {
                foreach (var type in GetAllDescendantsOf(ass, typeof(NetworkMessage))) {
                    messages.Add(type);
                    if (DEBUG) Debug.Log($"MESSAGES PROCESS: Type: {type.FullName}");
                }
            }

            storage.SetupMessages(messages.ToArray());
        }

        // unity saving stuffs
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    private static IEnumerable<Type> GetAllDescendantsOf(SysAssembly assembly, Type type) {
        return assembly.GetTypes().Where(x => type.IsAssignableFrom(x) && x != type && !x.IsDefined(typeof(IgnoreAttribute)));
    }

    private static IEnumerable<Type> GetAllGenericDescendantsOf(SysAssembly assembly, Type genericTypeDefinition) {
        return from x in assembly.GetTypes()
               let y = x.BaseType
               where !x.IsAbstract && !x.IsInterface && y != null && y.IsGenericType && y.GetGenericTypeDefinition() == genericTypeDefinition && !x.IsDefined(typeof(IgnoreAttribute))
               select x;
    }
}
