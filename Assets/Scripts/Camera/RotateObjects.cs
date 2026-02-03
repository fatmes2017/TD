using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateObjects : MonoBehaviour
{
    [SerializeField] Vector3 rotationVector;
    [SerializeField] float rotationSpeed;

    // Update is called once per frame
    void Update()
    {
        float newRotationSpeed = rotationSpeed * 100;
        transform.Rotate(rotationVector, newRotationSpeed);
    }
}
