using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Basically.Entities;
using Basically.Utility;

public class PlayerShellEntity : PhysicsEntity {
    public Transform orientation;

    public override Quaternion Rotation => orientation.rotation;
}
