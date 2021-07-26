using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Basically.Entities;
using Basically.Networking;
using Basically.Client;

public class Master : Client {
    float start = 0;

    private void Start() {
        SceneManager.sceneLoaded += (arg1, arg2) => {
            EntityManager.OnLoad();
            EntityManager.ClientStart();
        };

        SceneManager.LoadScene(1);
    }

    protected override void OnGUI() {
        base.OnGUI();

        GUILayout.Label($"Connected: {NetworkClient.Connected}");
        GUILayout.Label($"Bytes: {NetworkClient.BytesReceived / (Time.time - start)}");
        GUILayout.Label($"Kilobytes: {Mathf.RoundToInt((NetworkClient.BytesReceived / (Time.time - start)) / 1000f)}");
        if (GUILayout.Button("Connect")) {
            Connect("localhost", 27020);
            start = Time.time;
        }
    }
}
