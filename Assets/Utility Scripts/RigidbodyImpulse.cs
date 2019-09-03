using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class RigidbodyImpulse : MonoBehaviour {

    public bool applyOnEnable = true;

    public bool applyOnEnableRelative = false;

    public Vector3 initialVelocity = Vector3.up;

    public Vector3 randomVectorModifier = Vector3.zero;

    private void OnEnable()
    {
        if (applyOnEnable)
        {
            if (applyOnEnableRelative)
                ApplyRandomImpulseRelative();
            else
                ApplyRandomImpulse();
        }
    }

    public void ApplyRandomImpulse()
    {
        Vector3 randomForce = randomVectorModifier;
        randomForce.Scale(Random.onUnitSphere);
        GetComponent<Rigidbody>().AddForce(initialVelocity + randomForce, ForceMode.VelocityChange);
    }

    public void ApplyImpulse(Vector3 force)
    {
        GetComponent<Rigidbody>().AddForce( force, ForceMode.VelocityChange);
    }

    public void ApplyRandomImpulseRelative()
    {
        Vector3 randomForce = randomVectorModifier;
        randomForce.Scale(Random.onUnitSphere);
        GetComponent<Rigidbody>().AddRelativeForce(initialVelocity + randomForce, ForceMode.VelocityChange);
    }

    public void ApplyRelativeImpulse(Vector3 force)
    {
        GetComponent<Rigidbody>().AddRelativeForce( force, ForceMode.VelocityChange);
    }
}
