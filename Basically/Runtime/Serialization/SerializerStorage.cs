using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using System.IO;
#endif

namespace Basically.Serialization {
    public class SerializerStorage : ScriptableObject {

        // serializer stuffs
        public string[] serializerTypes;
        public string[] serializer;
        public int serializerBits;

        // shit that is transferred
        public string[] messages;
        public int messageBits;

        static SerializerStorage instance;
        static Dictionary<Type, Serializer> serializerCache;
        static Dictionary<byte, Type> messageCache;

        public static int MessageBits => instance.messageBits;
        public static int SerializerBits => instance.serializerBits;

        public static void Initialize() {
#if !UNITY_EDITOR
            if (instance) return;

            instance = Resources.Load<SerializerStorage>("Basically/Storage");
            serializerCache = new Dictionary<Type, Serializer>();
            messageCache = new Dictionary<byte, Type>();
#else
            if (Application.isPlaying) {
                if (instance) return;

                instance = Resources.Load<SerializerStorage>("Basically/Storage");
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
        public static SerializerStorage GetAsset() {
            var storage = AssetDatabase.LoadAssetAtPath<SerializerStorage>("Assets/Resources/Basically/Storage.asset");
            if (!storage) {
                storage = CreateInstance<SerializerStorage>();
                Directory.CreateDirectory("Assets/Resources/Basically");
                AssetDatabase.CreateAsset(storage, "Assets/Resources/Basically/Storage.asset");
            }

            return storage;
        }

        public void SetupSerializers(Dictionary<Type, Type> dic) {
            if (dic.Count > (byte.MaxValue + 1)) throw new ArgumentException("Too large!!!!!");
            serializerTypes = new string[dic.Count];
            serializer = new string[dic.Count];

            byte index = 0;
            foreach (var pair in dic) {
                serializerTypes[index] = pair.Key.AssemblyQualifiedName;
                serializer[index] = pair.Value.AssemblyQualifiedName;
                index++;
            }

            for (int i = 1; i < 8; i++) {
                if (Mathf.Pow(2, i) >= dic.Count) {
                    serializerBits = i;
                    break;
                }
            }

            EditorUtility.SetDirty(this);
        }

        public void SetupMessages(Type[] list) {
            if (list.Length > (byte.MaxValue + 1)) throw new ArgumentException("Too large!!!!!!");
            messages = new string[list.Length];

            for (byte i = 0; i < list.Length; i++) {
                messages[i] = list[i].AssemblyQualifiedName;
            }

            for (int i = 1; i < 8; i++) {
                if (Mathf.Pow(2, i) >= list.Length) {
                    messageBits = i;
                    break;
                }
            }

            EditorUtility.SetDirty(this);
        }
#endif
    }
}
