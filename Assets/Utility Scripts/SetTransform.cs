using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetTransform : MonoBehaviour
{
    public void SetTransformPosition(Vector3 pos)
    {
        transform.position = pos;
    }

    public void SetTransformRotation(Quaternion q)
    {
        transform.rotation = q;
    }

    public void SetTransformValues(Transform refTransform)
    {
        transform.position = refTransform.position;
        transform.rotation = refTransform.rotation;
    }

}
