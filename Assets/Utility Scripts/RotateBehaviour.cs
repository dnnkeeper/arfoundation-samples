using UnityEngine;
using System.Collections;

public class RotateBehaviour : MonoBehaviour {

    public Vector3 RotationAmount;

    public bool resetOnDisable = false;

    Quaternion initialRotation;

    bool initialized;

	void OnEnable () 
    {
        if (!initialized)
        {
            initialized = true;
            //Debug.Log(name + " Initial Rotation Saved " + transform.rotation.eulerAngles, transform);
            initialRotation = transform.rotation;
        }
    }

    private void OnDisable()
    {
        if (resetOnDisable)
            ResetRotation();
    }

    // Update is called once per frame
    void Update () 
    {
        transform.Rotate(RotationAmount * Time.deltaTime);
	}

    public void RotateDegrees(Vector3 eulerRotation)
    {
        transform.Rotate(eulerRotation);
    }

    public void RotateY(float deg)
    {
        transform.Rotate(new Vector3(0, deg, 0), Space.World );
    }

    public void ResetRotation()
    {
        if (initialized)
        {
            transform.rotation = initialRotation;
            //Debug.Log(name + " ResetRotation " + transform.rotation.eulerAngles, transform);
        }
    }
}
