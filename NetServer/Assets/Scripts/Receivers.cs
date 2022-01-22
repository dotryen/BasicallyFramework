using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Basically.Networking;
using Basically.Entities;

[ReceiverClass]
public static class Receivers {
    public static void OnInputs(Connection conn, Inputs inputs) {
        var player = (PlayerShellEntity)EntityManager.PredictableEntities.ElementAt(0).Value;
        player.CurrentInput = inputs;
    }

    public static void SimpleInput(Connection conn, SimplePlayerInput input) {
        var ent = (SimplePlayerEntity)EntityManager.PredictableEntities.ElementAt(0).Value;
        ent.moveVector = input.direction;
    }
}
