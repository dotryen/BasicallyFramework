using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.AddressableAssets;
using Basically.Server;
using Basically.Networking;
using Basically.Entities;

public class Master : Server {
    GameObject player;
    GameObject game;

    private void Start() {
        SceneManager.sceneLoaded += (arg1, arg2) => {
            var type = FindObjectOfType<MapType>();
            if (!type.twoDimensional) {
                Instantiate(player, Vector3.back * 3, Quaternion.identity);
            } else {
                Instantiate(game);
            }

            EntityManager.OnLoad();
            EntityManager.ServerStart();

            StartServer(1, 27020);
        };

        StartCoroutine(Load());
    }

    private IEnumerator Load() {
        var player = Addressables.LoadAssetAsync<GameObject>("Player");
        var game = Addressables.LoadAssetAsync<GameObject>("2DGame");
        if (!player.IsDone || !game.IsDone) yield return null;

        this.player = player.Result;
        this.game = game.Result;

        SceneManager.LoadScene(1);
    }
}
