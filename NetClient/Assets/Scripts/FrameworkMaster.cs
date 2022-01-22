using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.AddressableAssets;
using Basically.Entities;
using Basically.Networking;
using Basically.Client;
using Basically.Frameworks;
using Basically.Utility;

public class FrameworkMaster : MonoBehaviour {
    public float start = 0;
    public GameObject shell;
    public GameObject game;
    private ClientFramework framework;

    private void Start() {
        framework = GetComponent<ClientFramework>();

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

    protected void OnGUI() {
        GUILayout.Label($"Connected: {NetworkClient.Connected}");
        if (!NetworkClient.Connected) {
            framework.ipAddress = GUILayout.TextField(framework.ipAddress);

            if (GUILayout.Button("Connect")) {
                framework.StartFramework();
                start = Time.time;
            }
        } else {
            GUILayout.Label($"Tick: {BGlobals.Tick}");
            GUILayout.Label($"Predicted Tick: {BGlobals.PredictedTick}");
            GUILayout.Label($"Ping: {NetworkClient.Connection.Ping}");

            if (GUILayout.Button("Disconnect")) {
                framework.StopFramework();
            }
        }
    }

    private IEnumerator Load() {
        var shell = Addressables.LoadAssetAsync<GameObject>("PredPlayer");
        var game = Addressables.LoadAssetAsync<GameObject>("2DGame");
        if (!shell.IsDone || !game.IsDone) yield return null;

        this.shell = shell.Result;
        this.game = game.Result;

        SceneManager.LoadScene(1);
    }
}
