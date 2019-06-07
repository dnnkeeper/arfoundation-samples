using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RigidbodyMagnet : MonoBehaviour
{
    new Collider collider;

    // Start is called before the first frame update
    void Start()
    {
        collider = GetComponent<Collider>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        var rb = collision.rigidbody;
        if (rb != null)
        {
            rb.AddForce(transform.position - rb.position, ForceMode.Force);
        }
    }

}
