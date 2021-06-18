using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Basically.Entities;
using Basically.Networking;
using Basically.Client;

public class Master : Client {
    private void Start() {
        SceneManager.sceneLoaded += (arg1, arg2) => {
            EntityManager.OnLoad();
            EntityManager.ClientStart();
        };

        SceneManager.LoadScene(1);
    }

    private void OnGUI() {
        GUILayout.Label($"Connected: {NetworkClient.Connected}");
        if (GUILayout.Button("Connect")) {
            Connect("localhost", 27020);
        }
    }
}
