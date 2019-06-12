using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

//[System.Serializable]
//public class UnityEventPose : UnityEvent<Pose>{
//}

[RequireComponent(typeof(ARPlaneManager), typeof(ARReferencePointManager), typeof(ARRaycastManager))]
public class DetectedPlaneRaycaster : MonoBehaviour
{
    public UnityPositionRotationEvent onHitPlane;
    public UnityEvent onMiss;

    private ARPlaneManager planeManager;
    private ARSessionOrigin sessionOrigin;
    private ARRaycastManager raycastManager;
    private ARReferencePointManager referencePointManager;

    // Start is called before the first frame update
    private void Start()
    {
        planeManager = GetComponent<ARPlaneManager>();
        sessionOrigin = GetComponent<ARSessionOrigin>();
        raycastManager = GetComponent<ARRaycastManager>();
        referencePointManager = GetComponent<ARReferencePointManager>();
    }

    public void AnchorObjectToHitPosition(Transform objectTransform)
    {
        if (RaycastFromCameraCenter(out List<ARRaycastHit> hitList))
        {
            var hit = hitList[0];

            if (referencePointManager != null)
            {
                var hitPlane = planeManager.GetPlane(hit.trackableId);
                if (hitPlane != null)
                {
                    var referencePoint = objectTransform.GetComponentInParent<ARReferencePoint>();
                    if (referencePoint == null || referencePoint.trackableId != hit.trackableId) {
                        objectTransform.SetParent(null);
                        if (referencePoint != null && referencePoint.transform.childCount == 0)
                        {
                            Debug.Log("Destroy old empty reference point");
                            Destroy(referencePoint);
                        }
                        referencePoint = referencePointManager.AttachReferencePoint(hitPlane, hit.pose);
                    }                        
                    objectTransform.SetParent(referencePoint.transform);
                }
                else
                {
                    Debug.LogWarning("hit.trackableId "+ hit.trackableId+ " couldn't be found in "+planeManager);
                }
            }
            
            objectTransform.SetPositionAndRotation(hit.pose.position, hit.pose.rotation);
            objectTransform.gameObject.SetActive(true);
        }
        else
        {
            Debug.LogWarning("Couldn't place object : no hit detected");
        }
    }

    // Update is called once per frame
    private void Update()
    {
        if (RaycastFromCameraCenter(out List<ARRaycastHit> hitList))
        {
            var hit = hitList[0];
            onHitPlane.Invoke(hit.pose.position, hit.pose.rotation);
        }
        else
        {
            if (!Application.isEditor)
                onMiss.Invoke();
        }
    }

    private bool RaycastFromCameraCenter(out List<ARRaycastHit> hitList, TrackableType trackableTypeFilter = TrackableType.PlaneWithinBounds)
    {
        hitList = null;
        var camera = sessionOrigin.camera;
        Ray cameraRay = new Ray(camera.transform.position, camera.transform.forward);
        List<ARRaycastHit> hitResults = new List<ARRaycastHit>();

        if (raycastManager.Raycast(new Vector2(Screen.width / 2f, Screen.height / 2f), hitResults, trackableTypeFilter))
        {
            hitList = hitResults;
            return true;
        }
        return false;
    }
}