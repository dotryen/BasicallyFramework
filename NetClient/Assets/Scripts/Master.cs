using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.AddressableAssets;
using Basically.Entities;
using Basically.Networking;
using Basically.Client;

public class Master : Client {
    public float start = 0;

    public GameObject shell;
    public GameObject game;

    private string ip = "localhost";

    private void Start() {
        SceneManager.sceneLoaded += (arg1, arg2) => {
            var type = FindObjectOfType<MapType>();

            if (!type.twoDimensional) {
                Instantiate(shell, Vector3.back * 3, Quaternion.identity);
            } else {
                Instantiate(game);
            }

            EntityManager.OnLoad();
            EntityManager.ClientStart();
        };

        StartCoroutine(Load());
    }

    protected override void OnGUI() {
        base.OnGUI();

        GUILayout.Label($"Connected: {NetworkClient.Connected}");
        if (!NetworkClient.Connected) {
            ip = GUILayout.TextField(ip);

            if (GUILayout.Button("Connect")) {
                Connect(ip, 27020);
                start = Time.time;
            }
        } else {
            GUILayout.Label($"Ping: {NetworkClient.Connection.Ping}");
            Interpolation.InterpolationGUI();

            if (GUILayout.Button("Disconnect")) {
                Disconnect(0);
            }
        }
    }

    private IEnumerator Load() {
        var shell = Addressables.LoadAssetAsync<GameObject>("PlayerShell");
        var game = Addressables.LoadAssetAsync<GameObject>("2DGame");
        if (!shell.IsDone || !game.IsDone) yield return null;

        this.shell = shell.Result;
        this.game = game.Result;

        SceneManager.LoadScene(1);
    }
}
