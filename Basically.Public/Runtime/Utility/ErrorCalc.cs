using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ErrorCalc {
    [Tooltip("Compared to Square Magnitude, not regular.")]
    [SerializeField]
    private float positionThreshold = 0.0000001f;
    
    [Tooltip("Compared to the dot product of the two rotations")]
    [Range(-1f, 1f)]
    [SerializeField]
    private float rotationThreshold = 0.00001f;

    public float PositionThreshold { get; set; }
}
