using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.ARFoundation;

//[System.Serializable]
//public class UnityEventPose : UnityEvent<Pose>{
//}

[RequireComponent(typeof(ARPlaneManager), typeof(ARSessionOrigin))]
public class DetectedPlaneRaycaster : MonoBehaviour
{
    public UnityPositionRotationEvent onHitPlane;
    public UnityEvent onMiss;

    ARSessionOrigin sessionOrigin;
    ARRaycastManager raycastManager;

    // Start is called before the first frame update
    void Start()
    {
        sessionOrigin = GetComponent<ARSessionOrigin>();
        raycastManager = GetComponent<ARRaycastManager>();
    }

    // Update is called once per frame
    void Update()
    {
        var camera = sessionOrigin.camera;
        Ray cameraRay = new Ray(camera.transform.position, camera.transform.forward);
        List<ARRaycastHit> hitResults = new List<ARRaycastHit>();

        if ( raycastManager.Raycast( new Vector2(Screen.width / 2f, Screen.height / 2f), hitResults, UnityEngine.XR.ARSubsystems.TrackableType.PlaneWithinBounds))
        {
            var hit = hitResults[0];
            onHitPlane.Invoke(hit.pose.position, hit.pose.rotation);
        }
        else
        {
            if (!Application.isEditor)
                onMiss.Invoke();
        }
    }
}
