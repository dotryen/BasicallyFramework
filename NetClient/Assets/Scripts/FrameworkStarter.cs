using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Basically.Frameworks;
using Basically.Entities;

public class FrameworkStarter : MonoBehaviour {
    public Framework framework;

    private void Start() {
        SceneManager.sceneLoaded += (arg1, arg2) => {
            EntityManager.OnLoad();
            EntityManager.ClientStart();
            framework.StartFramework();
        };

        SceneManager.LoadScene(1);
    }
}
