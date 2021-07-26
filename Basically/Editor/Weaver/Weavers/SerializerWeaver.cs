using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;
using UnityEngine;
using UnityEditor;

namespace Basically.Editor.Weaver {
    using Networking;
    using Serialization;

    internal class SerializerWeaver : Weaver {
        public override bool IsEditor => false;
        public override int Priority => 0;

        BasicallyCache storage;

        // TODO: Finish weaver
        public override void Weave() {
            return; // disable for now

            var messageRef = Module.ImportReference(typeof(NetworkMessage));
            var serialRef = Module.ImportReference(typeof(Serializer<>));
            if (messageRef == null || serialRef == null) return;

            storage = BasicallyCache.GetAsset();
            AddMessages();
            AddSerializers();
            Save();
        }

        void AddSerializers() {
            var serializers = GetDescendants(typeof(Serializer<>)).Where(x => !x.HasGenericParameters);
            if (serializers.Count() == 0) return;

            storage.serializer = storage.messages.Concat(serializers.Select(x => GetQualifiedName(x))).ToArray();
            storage.serializerTypes = storage.serializerTypes.Concat(serializers.Select(x => GetQualifiedName(x.BaseType.GenericParameters[0]))).ToArray();
            storage.serializerBits = GetBits(storage.serializerTypes.Length);
        }

        void AddMessages() {
            var messages = GetDescendants<NetworkMessage>().Where(x => !x.HasGenericParameters);
            if (messages.Count() == 0) ;

            storage.messages = storage.messages.Concat(messages.Select(x => x.GetType().AssemblyQualifiedName)).ToArray();
            storage.messageBits = GetBits(storage.messages.Length);
        }

        int GetBits(int count) {
            for (int i = 1; i < 8; i++) {
                if (Mathf.Pow(2, i) >= count) {
                    return i;
                }
            }
            return 0;
        }

        string GetQualifiedName(TypeDefinition type) {
            return type.GetType().AssemblyQualifiedName;
        }

        string GetQualifiedName(GenericParameter par) {
            return par.GetType().AssemblyQualifiedName;
        }

        void Save() {
            EditorUtility.SetDirty(storage);
            AssetDatabase.SaveAssets();
        }
    }
}
