using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhysicsGrabber : MonoBehaviour {
    [Header("Position")]
    public float acceleration = 350f;
    public float dampingThreshold = 5;
    public float dampingAmount = 0.8f;

    [Header("Rotation")]
    public float rotationAccel;
    public float rotationMax;

    private float rotationTarget;
    private Rigidbody2D selected;

    private Vector2 target;
    private bool touching;

    private void Start() {
        Input.simulateMouseWithTouches = false;
    }

    public void Update() {
#if UNITY_STANDALONE
        if (Input.GetMouseButtonDown(0)) {
            DoThing(Input.mousePosition);
        }

        target = Input.mousePosition;

        rotationTarget = -Input.GetAxisRaw("Horizontal") * rotationMax;
#else
        if (Input.touchCount >= 1) {
            if (!touching) {
                DoThing(Input.GetTouch(0).position);
            }

            touching = true;

            if (Input.touchCount >= 2) {
                target = Vector2.Lerp(Input.GetTouch(0).position, Input.GetTouch(1).position, 0.5f);
            } else {
                target = Input.GetTouch(0).position;
            }
        } else {
            touching = false;
            if (selected) {
                DoThing(Vector3.zero);
            }
        }
#endif
    }

    public void FixedUpdate() {
        if (selected) {
            // position
            {
                var direction = (Vector2)Camera.main.ScreenToWorldPoint(target) - selected.worldCenterOfMass;

                // damp
                if (direction.magnitude < dampingThreshold) {
                    selected.velocity *= dampingAmount;
                }

                // add force
                var addAmount = Mathf.InverseLerp(0, dampingThreshold, direction.magnitude);
                var speed = acceleration * selected.mass;
                selected.AddForce((direction.normalized * speed) * addAmount);
            }

            // rotation
            {
                var delta = (rotationTarget - selected.angularVelocity) * rotationAccel;
                selected.AddTorque(delta * Mathf.Deg2Rad * selected.mass);
            }
        }
    }

    public void DoThing(Vector3 screenPoint) {
        if (!selected) {
            var info = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(screenPoint), Vector2.zero);
            if (info.rigidbody != null) {
                selected = info.rigidbody;
            }
        } else {
            selected = null;
        }
    }
}
