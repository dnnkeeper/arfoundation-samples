using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.XR.ARFoundation;
using UnityEngine.SpatialTracking;

/// This component listens for images detected by the <c>XRImageTrackingSubsystem</c>
/// and overlays some information as well as the source Texture2D on top of the
/// detected image.
/// </summary>
[RequireComponent(typeof(ARTrackedImageManager))]
public class StationaryMarkersManager : MonoBehaviour
{
    ARTrackedImageManager m_TrackedImageManager;

    public Dictionary<Guid, StationaryMarker> virtualMarkersDict
            = new Dictionary<Guid, StationaryMarker>();

    //public Transform offsetOrigin;

    public Transform cameraTransform;

    public TrackedPoseDriver tracker;

    void Awake()
    {
        Screen.sleepTimeout = SleepTimeout.NeverSleep;

        m_TrackedImageManager = GetComponent<ARTrackedImageManager>();

        var virtualMarkers = GameObject.FindObjectsOfType<StationaryMarker>();
        foreach (var virtualMarker in virtualMarkers)
        {
            var referenceImage = virtualMarker.imageLibrary[virtualMarker.imageIndex];

            if (!virtualMarkersDict.ContainsKey( referenceImage.guid ) )
            {
                virtualMarkersDict.Add(referenceImage.guid, virtualMarker);
            }
            else
            {
                Debug.LogError("virtualMarkersDict already contains marker with guid " + referenceImage.guid+ " texture: " + referenceImage.texture);
            }

            virtualMarker.gameObject.SetActive(false);
        }
    }

    void OnEnable()
    {
        m_TrackedImageManager.trackedImagesChanged += OnTrackedImagesChanged;
    }

    void OnDisable()
    {
        m_TrackedImageManager.trackedImagesChanged -= OnTrackedImagesChanged;
    }

    void UpdateInfo(ARTrackedImage trackedImage)
    {
        //var planeParentGo = trackedImage.transform.GetChild(0).gameObject;
        //var planeGo = planeParentGo.transform.GetChild(0).gameObject;

        // Disable the visual plane if it is not being tracked
        if (trackedImage.trackingState != TrackingState.None)
        {
            Debug.Log(trackedImage.referenceImage.name + ".trackingState == " + trackedImage.trackingState);

            //planeGo.SetActive(true);

            // The image extents is only valid when the image is being tracked
            //trackedImage.transform.localScale = new Vector3(trackedImage.size.x, 1f, trackedImage.size.y);
        }
        else
        {
            Debug.Log(trackedImage.referenceImage.name + ".trackingState == TrackingState.None");
            //planeGo.SetActive(false);
        }
    }

    StationaryMarker lastTrackedMarker;

    ARTrackedImage lastTrackedImage;

    void OnTrackedImagesChanged(ARTrackedImagesChangedEventArgs eventArgs)
    {
        foreach (var trackedImage in eventArgs.added)
        {
            // Give the initial image a reasonable default scale
            trackedImage.transform.localScale = new Vector3(0.01f, 1f, 0.01f);
            
            UpdateInfo(trackedImage);

            Debug.Log("virtualMarker found! " + trackedImage.referenceImage.name + " guid: " + trackedImage.referenceImage.guid);

            MatchImageWithMarker(trackedImage);
        }

        foreach (var trackedImage in eventArgs.updated)
        {
            UpdateInfo(trackedImage);

            MatchImageWithMarker(trackedImage);
        }
    }

    private void Update()
    {
        SyncCameraWithTracker();
    }

    void MatchImageWithMarker(ARTrackedImage trackedImage)
    {
        StationaryMarker virtualMarker;

        if (virtualMarkersDict.TryGetValue(trackedImage.referenceImage.guid, out virtualMarker))
        {
            if (trackedImage.trackingState != TrackingState.None)
            {
                lastTrackedMarker = virtualMarker;
                lastTrackedImage = trackedImage;

                if (trackedImage.transform.lossyScale != virtualMarker.transform.lossyScale)
                {
                    Debug.LogWarning("trackedImage scale "+ trackedImage.transform.lossyScale+" != "+ virtualMarker.transform.lossyScale+" of virtual marker! Positioning might become incorrect!");
                }

                //lastTrackedImage.transform.localScale = Vector3.one;
                virtualMarker.gameObject.SetActive(true);
                SyncOffset();
            }
            else
            {
                virtualMarker.gameObject.SetActive(false);
            }
        }
    }

    public Transform debugTrackableParent;

    void SyncOffset()
    {
        if (lastTrackedMarker != null && lastTrackedImage != null)
        {
            //var centerPoseRotationEuler = lastTrackedImage.transform.eulerAngles;
            //var virtualMarkerRotation = lastTrackedMarker.transform.rotation;
            //var virtualMarkerRotationEuler = lastTrackedMarker.transform.rotation.eulerAngles;

            /*
            if (Mathf.Abs(virtualMarkerRotationEuler.x) > 1f || Mathf.Abs(virtualMarkerRotationEuler.z) > 1f)
            {
                Quaternion rotatedTargetOrigin = Quaternion.LookRotation(Vector3.Cross(Vector3.up, Vector3.ProjectOnPlane(lastTrackedMarker.transform.right, Vector3.up)), Vector3.up);
                Quaternion fromOriginToMarker = Quaternion.Inverse(rotatedTargetOrigin) * lastTrackedMarker.transform.rotation;
                Quaternion rotatedOrigin = Quaternion.LookRotation(Vector3.Cross(Vector3.up, lastTrackedImage.transform.right), Vector3.up);
                Quaternion fromOriginToAnchor = Quaternion.Inverse(rotatedOrigin) * lastTrackedImage.transform.rotation;

                lastTrackedImage.transform.rotation = rotatedOrigin * fromOriginToMarker;
            }
            else
            {
                lastTrackedImage.transform.rotation = Quaternion.Euler(virtualMarkerRotationEuler.x, centerPoseRotationEuler.y, virtualMarkerRotationEuler.z);
            }*/

            transform.SetPositionAndRotation(
                                lastTrackedMarker.transform.TransformPoint(lastTrackedImage.transform.InverseTransformPoint(transform.position)),
                                lastTrackedMarker.transform.rotation * (Quaternion.Inverse(lastTrackedImage.transform.rotation) * transform.rotation )
                                );

            Debug.Log("sessionOrigin " + transform.position);



            ARSessionOrigin sessionOrigin = GetComponent<ARSessionOrigin>();
            debugTrackableParent.SetParent(sessionOrigin.trackablesParent);
            debugTrackableParent.localPosition = Vector3.zero;
            debugTrackableParent.localRotation = Quaternion.identity;
            sessionOrigin.camera = null;
            sessionOrigin.trackablesParent.position = transform.position;
            sessionOrigin.trackablesParent.rotation = transform.rotation;

            //sessionOrigin.trackablesParent.SetPositionAndRotation(transform.position, transform.rotation);
            //sessionOrigin.trackablesParent.localPosition = Vector3.zero;
            //sessionOrigin.trackablesParent.localRotation = Quaternion.identity;
        }
    }

    void SyncCameraWithTracker()
    {
        //cameraTransform.localPosition = tracker.transform.localPosition;
        //cameraTransform.localRotation = tracker.transform.localRotation;

        //cameraTransform.position = tracker.transform.position;
        //cameraTransform.rotation = tracker.transform.rotation;
    }
}
