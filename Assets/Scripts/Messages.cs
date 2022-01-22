using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Basically.Networking;

public struct SimplePlayerInput : NetworkMessage {
    public Vector3 direction;
}
