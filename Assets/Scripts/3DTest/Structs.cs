using UnityEngine;
using Basically.Networking;

public struct Inputs : NetworkMessage {
    public bool up;
    public bool down;
    public bool left;
    public bool right;
    public bool sprint;
    public bool jump;

    public Vector2 MovementVector => Vector3.ClampMagnitude(new Vector2((left ? -1 : 0) + (right ? 1 : 0), (down ? -1 : 0) + (up ? 1 : 0)), 1);
    public float orientation;
    public float viewAngle;
}
