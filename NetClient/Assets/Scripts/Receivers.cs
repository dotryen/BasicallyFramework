using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Basically.Client;
using Basically.Networking;
using Basically.Entities;
using Basically.Serialization;

[ReceiverClass]
public static class Receivers {
    public static void CreatePlayer(Connection conn, PlayerEnter message) {
        var master = (Master)Client.Instance;

        var go = Object.Instantiate(master.shell, Vector3.back * 3, Quaternion.identity);
        EntityManager.RegisterEntity(go.GetComponent<PlayerShellEntity>());
    }
}
