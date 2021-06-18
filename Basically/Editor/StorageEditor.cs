using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Basically.Serialization;

[CustomEditor(typeof(SerializerStorage))]
public class StorageEditor : Editor {
    bool showSerializers = false;
    bool showMessages = false;

    SerializerStorage asset;
    KeyValuePair<string, string>[] serializers;
    KeyValuePair<byte, string>[] messages;

    public void OnEnable() {
        asset = SerializerStorage.GetAsset();
        serializers = new KeyValuePair<string, string>[asset.serializerTypes.Length];
        messages = new KeyValuePair<byte, string>[asset.messages.Length];

        for (byte i = 0; i < asset.serializerTypes.Length; i++) {
            serializers[i] = new KeyValuePair<string, string>(Type.GetType(asset.serializerTypes[i]).FullName, Type.GetType(asset.serializer[i]).FullName);
        }

        for (byte i = 0; i < asset.messages.Length; i++) {
            messages[i] = new KeyValuePair<byte, string>(i, Type.GetType(asset.messages[i]).FullName);
        }
    }

    public override void OnInspectorGUI() {
        EditorGUILayout.HelpBox("Do not delete or edit this file. It is essential for the networking in Basically.", MessageType.Warning);
        EditorGUILayout.LabelField($"{serializers.Length} serializers. ({asset.serializerBits} bits) [0-{Mathf.Pow(2, asset.serializerBits) - 1}]");
        EditorGUILayout.LabelField($"{messages.Length} messages. ({asset.messageBits} bits) [0-{Mathf.Pow(2, asset.messageBits) - 1}]");
        showSerializers = EditorGUILayout.Foldout(showSerializers, "Serializers", EditorStyles.foldoutHeader);
        if (showSerializers) {
            for (byte i = 0; i < serializers.Length; i++) {
                DrawBox("Type: " + serializers[i].Key, "Serializer: " + serializers[i].Value);
            }
        }

        showMessages = EditorGUILayout.Foldout(showMessages, "Messages", EditorStyles.foldoutHeader);
        if (showMessages) {
            for (byte i = 0; i < messages.Length; i++) {
                DrawBox("Index: " + messages[i].Key, "Message: " + messages[i].Value);
            }
        }
    }

    public void DrawBox(string top, string bottom) {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.LabelField(top);
        EditorGUILayout.LabelField(bottom);
        EditorGUILayout.EndVertical();
    }
}
