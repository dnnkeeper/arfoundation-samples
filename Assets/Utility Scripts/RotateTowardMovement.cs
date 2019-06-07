using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateTowardMovement : MonoBehaviour
{
    Vector3 lastPos;

    private void Awake()
    {
        lastPos = transform.position;
    }

    private void LateUpdate()
    {
        transform.rotation = Quaternion.LookRotation(transform.position - lastPos, transform.up);
        lastPos = transform.position;
    }
}