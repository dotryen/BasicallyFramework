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
        if (NetworkClient.Connected) {
            var bytes = NetworkClient.Connection.BytesReceived;
            GUILayout.Label($"Bytes: {bytes / (Time.time - start)}");
            GUILayout.Label($"Kilobytes: {Mathf.RoundToInt((bytes / (Time.time - start)) / 1000f)}");
        }
        if (!NetworkClient.Connected) {
            if (GUILayout.Button("Connect")) {
                Connect("localhost", 27020);
                start = Time.time;
            }
        } else {
            if (GUILayout.Button("Disconnect")) {
                Disconnect(0);
            }
        }
    }
}
