using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CopyRotation : MonoBehaviour
{
    public Transform source;

    // Update is called once per frame
    void Update()
    {
        if (source != null)
            transform.rotation = source.rotation;
    }
}
