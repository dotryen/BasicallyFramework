﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.AddressableAssets;
using Basically.Server;
using Basically.Networking;
using Basically.Entities;
using Basically.Frameworks;

public class FrameworkMaster : MonoBehaviour {
    GameObject player;
    GameObject game;

    DedicatedServerFramework framework;

    private void Start() {
        framework = GetComponent<DedicatedServerFramework>();

        SceneManager.sceneLoaded += (arg1, arg2) => {
            var type = FindObjectOfType<MapType>();
            if (!type.twoDimensional) {
                Instantiate(player, Vector3.back * 3, Quaternion.identity);
            } else {
                Instantiate(game);
            }

            EntityManager.OnLoad();
            EntityManager.ServerStart();

            framework.StartFramework();
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
