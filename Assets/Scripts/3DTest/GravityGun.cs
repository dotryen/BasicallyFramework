﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GravityGun : MonoBehaviour {
    public Transform origin;
    [Space]
    public float holdDistance;
    public float grabbingDistance;
    public float pullingDistance;
    [Header("Force")]
    public float acceleration;
    public float pullForce;
    public float throwingForce;
    [Header("Damping")]
    public float dampingThreshold;
    public float dampingAmount = 0.8f;
    [Space]
    public Rigidbody heldObject;

    public Vector3 HoldPosition => (origin.forward * holdDistance) + origin.position;

    private bool rightClickHeld;
    private float totalMass;

    public void Update() {
        if (Input.GetKeyDown(KeyCode.R)) {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        if (Input.GetMouseButtonDown(1)) {
            if (!heldObject) {
                if (CanGrab(out RaycastHit hit)) {
                    Grab(hit.rigidbody);
                } else {
                    rightClickHeld = true;
                }
            } else {
                LetGo();
            }
        }

        if (Input.GetMouseButtonUp(1)) {
            rightClickHeld = false;
        }

        if (Input.GetMouseButtonDown(0)) {
            Throw();
        }

        holdDistance += Input.mouseScrollDelta.y;
        holdDistance = Mathf.Max(3, holdDistance);
    }

    public void FixedUpdate() {
        if (rightClickHeld) {
            Pull();
        }

        if (heldObject) {
            var direction = HoldPosition - heldObject.worldCenterOfMass;

            // damp
            if (direction.magnitude < dampingThreshold + holdDistance) {
                heldObject.velocity *= dampingAmount;
            }

            // add force
            var addAmount = Mathf.InverseLerp(0, dampingThreshold, direction.magnitude);
            var speed = acceleration * totalMass;
            heldObject.AddForce((direction.normalized * speed) * addAmount);
        }
    }

    public void Pull() {
        if (!heldObject) {
            if (Physics.Raycast(origin.position, origin.forward, out RaycastHit hit, pullingDistance)) {
                if (hit.rigidbody) {
                    if (hit.distance < grabbingDistance) {
                        Grab(hit.rigidbody);
                    } else {
                        GetTotalMass(hit.rigidbody);
                        var speed = pullForce * totalMass;

                        if (hit.rigidbody.velocity.magnitude < speed) {
                            hit.rigidbody.AddForce(-origin.forward * speed);
                        }
                    }
                }
            }
        }
    }

    public void Grab(Rigidbody original) {
        if (!heldObject && original) {
            GetTotalMass(original);

            original.constraints = RigidbodyConstraints.FreezeRotation;
            original.useGravity = false;
            heldObject = original;
        }
    }

    public void LetGo() {
        heldObject.constraints = RigidbodyConstraints.None;
        heldObject.useGravity = true;
        heldObject = null;
    }

    public void Throw() {
        if (heldObject) {
            ThrowInternal(heldObject);

            LetGo();
        } else {
            if (Physics.Raycast(origin.position, origin.forward, out RaycastHit hit, grabbingDistance)) {
                if (hit.rigidbody) {
                    ThrowInternal(hit.rigidbody);
                    return;
                }
            }
        }
    }

    private void ThrowInternal(Rigidbody rb) {
        GetTotalMass(rb);
        rb.AddForce(origin.forward * throwingForce * totalMass, ForceMode.Impulse);
    }

    public bool CanGrab(out RaycastHit hit) {
        if (Physics.Raycast(origin.position, origin.forward, out hit, grabbingDistance)) {
            return hit.rigidbody;
        }
        return false;
    }

    public void GetTotalMass(Rigidbody original) {
        List<Rigidbody> rigidbodies = new List<Rigidbody>();
        rigidbodies.Add(original);
        rigidbodies.AddRange(original.GetComponentsInChildren<Rigidbody>());
        rigidbodies.AddRange(original.GetComponentsInParent<Rigidbody>());

        totalMass = 0f;
        foreach (Rigidbody rb in rigidbodies) {
            totalMass += rb.mass;
        }
        
    }
}
