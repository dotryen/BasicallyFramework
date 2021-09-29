using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimatorUpdate : MonoBehaviour
{
    public float updateFactor = 1f;
    Animator animator;

    private void Start() {
        animator = GetComponent<Animator>();
    }

    private void Update() {
        animator.Update(Time.deltaTime * updateFactor);
    }
}
