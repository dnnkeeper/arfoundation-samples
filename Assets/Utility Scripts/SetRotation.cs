using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetRotation : MonoBehaviour
{
    public bool debug;

    public bool localRotation;

    public Vector3 targetRotation;

    public Vector3 multiplier = new Vector3(1,1,1);

    public bool useLerp;

    public float lerpSpeed = 1f;

    private void OnEnable()
    {
        var mainCam = Camera.main;
        if (mainCam != null)
        {
            var camEvents = mainCam.GetComponent<CameraOnRenderEvents>();
            if (camEvents != null)
                camEvents.onPreRender += Update;
        }
    }

    void OnDisable()
    {
        var mainCam = Camera.main;
        if (mainCam != null)
        {
            var camEvents = mainCam.GetComponent<CameraOnRenderEvents>();
            if (camEvents != null)
                camEvents.onPreRender -= Update;
        }
    }

    // Update is called once per frame
    void Update()
    { 
        var myRot = localRotation? transform.localRotation.eulerAngles : transform.rotation.eulerAngles;
        var rot = new Vector3( Mathf.Lerp(myRot.x, targetRotation.x, multiplier.x), Mathf.Lerp(myRot.y, targetRotation.y, multiplier.y), Mathf.Lerp(myRot.z, targetRotation.z, multiplier.z));
        if (localRotation)
            transform.localRotation = useLerp? Quaternion.Lerp(transform.localRotation, Quaternion.Euler(rot), Time.deltaTime * lerpSpeed) : Quaternion.Euler(rot);
        else
            transform.rotation = useLerp ? Quaternion.Lerp(transform.rotation, Quaternion.Euler(rot), Time.deltaTime * lerpSpeed) : Quaternion.Euler(rot); 
    }

    private void OnGUI()
    {
        if (debug)
            GUILayout.Label(transform.localEulerAngles.ToString());
    }
}
