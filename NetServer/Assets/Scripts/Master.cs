using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Basically.Server;
using Basically.Entities;

public class Master : Server {
    public GameObject player;

    private void Start() {
        SceneManager.sceneLoaded += (arg1, arg2) => {
            EntityManager.OnLoad();
            EntityManager.ServerStart();

            Instantiate(player, Vector3.back * 3, Quaternion.identity);
            StartServer(1, 27020);
        };

        SceneManager.LoadScene(1);
    }
}
