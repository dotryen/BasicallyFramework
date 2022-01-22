using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Basically.Entities;
using Basically.Utility;
using Basically.Client;
using Basically.Networking;

public class SimplePlayerEntity : PredictableEntity {
    public float speed = 1f;
    public Vector3 moveVector;

    protected override void OnClientTick() {
        moveVector.x = Input.GetAxisRaw("Horizontal");
        moveVector.y = Input.GetAxisRaw("Vertical");

        var message = new SimplePlayerInput() {
            direction = moveVector
        };
        NetworkClient.Send(message, 0, MessageType.Unreliable);

        base.OnClientTick();
    }

    protected override void Predict() {
        transform.position += moveVector * speed * Time.deltaTime;
    }

    protected override void Record(ref IParameters parameters) {
        base.Record(ref parameters);
        parameters.Add("dir", moveVector);
    }
}
