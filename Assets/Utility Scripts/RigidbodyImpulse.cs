using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class RigidbodyImpulse : MonoBehaviour {

    public Vector3 initialVelocity = Vector3.up;

    public Vector3 randomVectorModifier = Vector3.zero;

    private void OnEnable()
    {
        Vector3 randomForce = randomVectorModifier;
        randomForce.Scale(Random.onUnitSphere);
        GetComponent<Rigidbody>().AddForce(initialVelocity + randomForce, ForceMode.VelocityChange);
    }
}
