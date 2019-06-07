using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetRotation : MonoBehaviour
{
    public Vector3 targetRotation;

    public Vector3 multiplier = new Vector3(1,1,1);

    // Update is called once per frame
    void Update()
    { 
        var myRot = transform.rotation.eulerAngles;
        var rot = new Vector3( Mathf.Lerp(myRot.x, targetRotation.x, multiplier.x), Mathf.Lerp(myRot.y, targetRotation.y, multiplier.y), Mathf.Lerp(myRot.z, targetRotation.z, multiplier.z));
        transform.rotation = Quaternion.Euler(rot);
    }
}
