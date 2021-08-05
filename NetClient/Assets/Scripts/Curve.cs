using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Basically.Utility;

[ExecuteAlways]
public class Curve : MonoBehaviour {
    [System.Serializable]
    public class Point {
        public Transform position;
        public Transform tangent;

        public Vector3 Position => position.position;
        public Vector3 Tangent => tangent.position - position.position;
    }

    public Point start;
    public Point end;

    public void Update() {
        HermiteCurve.DrawCurve(start.Position, start.Tangent, end.Position, end.Tangent, Time.deltaTime);
    }
}
