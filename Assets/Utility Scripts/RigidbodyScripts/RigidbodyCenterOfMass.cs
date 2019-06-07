using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RigidbodyCenterOfMass : MonoBehaviour
{
    public Vector3 centerOfMass;
    Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.centerOfMass = centerOfMass;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.TransformPoint(centerOfMass), 0.01f);
    }
}
