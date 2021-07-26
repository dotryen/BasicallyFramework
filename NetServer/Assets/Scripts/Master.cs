using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Basically.Server;
using Basically.Entities;

public class Master : Server {
    public GameObject player;
    public GameObject game;

    private void Start() {
        SceneManager.sceneLoaded += (arg1, arg2) => {
            EntityManager.OnLoad();
            EntityManager.ServerStart();

            if (FindObjectOfType<Rigidbody>()) {
                Instantiate(player, Vector3.back * 3, Quaternion.identity);
            } else {
                Instantiate(game);
            }
            
            StartServer(1, 27020);
        };

        SceneManager.LoadScene(1);
    }
}
