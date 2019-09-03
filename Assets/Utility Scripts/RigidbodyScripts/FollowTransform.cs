using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowTransform : MonoBehaviour
{
    public Transform targetTransform;

    public float positionLerp = 30f;

    public float rotationLerp = 30f;

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

    void Update()
    {
        if (targetTransform != null)
        {
            transform.position = Vector3.Lerp(transform.position, targetTransform.position, positionLerp * Time.deltaTime);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetTransform.rotation, rotationLerp * Time.deltaTime);
        }
    }
}
