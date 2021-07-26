using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using System.IO;
#endif

namespace Basically.Serialization {
    public class BasicallyCache : ScriptableObject {
        [Serializable]
        internal class SerializerCache {
            public string[] inputs;
            public string[] types;
            public int bits;
        }

        [Serializable]
        internal class MessageCache {
            public string[] types;
            public int bits;
        }

        // serializer stuffs
        public string[] serializerTypes;
        public string[] serializer;
        public int serializerBits;

        // shit that is transferred
        public string[] messages;
        public int messageBits;

        static BasicallyCache instance;
        static Dictionary<Type, Serializer> serializerCache;
        static Dictionary<byte, Type> messageCache;

        public static int MessageBits => instance.messageBits;
        public static int SerializerBits => instance.serializerBits;

        public static void Initialize() {
#if !UNITY_EDITOR
            if (instance) return;

            instance = Resources.Load<BasicallyCache>("Basically/Storage");
            serializerCache = new Dictionary<Type, Serializer>();
            messageCache = new Dictionary<byte, Type>();
#else
            if (Application.isPlaying) {
                if (instance) return;

                instance = Resources.Load<BasicallyCache>("Basically/Storage");
                serializerCache = new Dictionary<Type, Serializer>();
                messageCache = new Dictionary<byte, Type>();
            } else {
                instance = GetAsset();
                serializerCache = new Dictionary<Type, Serializer>();
                messageCache = new Dictionary<byte, Type>();
            }
#endif
        }

        public static Serializer<T> GetSerializer<T>() {
            return (Serializer<T>)GetSerializer(typeof(T));
        }

        public static Serializer GetSerializer(Type type) {
            if (serializerCache != null) {
                if (serializerCache.TryGetValue(type, out Serializer serial)) {
                    return serial;
                }
            }

            for (byte i = 0; i < instance.serializerTypes.Length; i++) {
                if (type.AssemblyQualifiedName == instance.serializerTypes[i]) {
                    var newSerial = (Serializer)Activator.CreateInstance(Type.GetType(instance.serializer[i]));
                    newSerial.Index = i;
                    if (serializerCache != null) serializerCache.Add(type, newSerial);
                    return newSerial;
                }
            }

            return null;
        }

        public static Serializer GetSerializer(byte index) {
            if (serializerCache != null) {
                foreach (var pair in serializerCache) {
                    if (pair.Value.Index == index) return pair.Value;
                }
            }

            var newSerial = (Serializer)Activator.CreateInstance(Type.GetType(instance.serializer[index]));
            newSerial.Index = index;
            if (serializerCache != null) serializerCache.Add(Type.GetType(instance.serializerTypes[index]), newSerial);
            return newSerial;
        }

        public static Type GetMessage(byte index) {
            if (messageCache.TryGetValue(index, out Type type)) {
                return type;
            } else {
                Type newType = Type.GetType(instance.messages[index]);
                messageCache.Add(index, newType);
                return newType;
            }
        }

        public static byte GetMessageIndex(Type type) {
            if (!type.IsValueType) throw new ArgumentException("Type is not a struct!!!!");

            for (byte i = 0; i < instance.messages.Length; i++) {
                if (instance.messages[i] == type.AssemblyQualifiedName) return i;
            }

            throw new Exception("That type is not included in messages, it cannot be used.");
        }

#if UNITY_EDITOR
        public static BasicallyCache GetAsset() {
            var storage = AssetDatabase.LoadAssetAtPath<BasicallyCache>("Assets/Resources/Basically/Storage.asset");
            if (!storage) {
                storage = CreateInstance<BasicallyCache>();
                Directory.CreateDirectory("Assets/Resources/Basically");
                AssetDatabase.CreateAsset(storage, "Assets/Resources/Basically/Storage.asset");
            }

            return storage;
        }
#endif
    }
}
