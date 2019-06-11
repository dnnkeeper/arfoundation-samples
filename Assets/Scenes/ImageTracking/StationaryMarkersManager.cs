using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.XR.ARFoundation;
using UnityEngine.SpatialTracking;
using UnityEngine.Events;
using System.Collections;

/// This component listens for images detected by the <c>XRImageTrackingSubsystem</c>
/// and overlays some information as well as the source Texture2D on top of the
/// detected image.
/// </summary>
[RequireComponent(typeof(ARTrackedImageManager))]
public class StationaryMarkersManager : MonoBehaviour
{
    public UnityEvent onHasMarkers;
    public UnityEvent onZeroMarkers;

    int _markersCount = -1;
    public int markersCount
    {
        get { return _markersCount; }
        set
        {
            if (_markersCount != value)
            {
                if (_markersCount != 0 && value == 0)
                {
                    SetVirtualSceneActive(false);
                    onZeroMarkers.Invoke();
                }

                if (value > 0)
                {
                    SetVirtualSceneActive(true);
                    onHasMarkers.Invoke();
                }

                _markersCount = value;
                Debug.Log("markersCount: " + _markersCount);
            }
        }
    }

    GameObject[] virtualScenes;

    public void SetVirtualSceneActive(bool b)
    {
        foreach (var virtualScene in virtualScenes)
        {
            virtualScene.SetActive(b);
        }
    }


    ARTrackedImageManager m_TrackedImageManager;

    public Dictionary<Guid, StationaryMarker> virtualMarkersDict
            = new Dictionary<Guid, StationaryMarker>();

    IEnumerator waitForEnvManager()
    {
        AREnvironmentProbeManager EnvironmentProbeManager = GetComponent<AREnvironmentProbeManager>();

        Debug.Log("Waiting for EnvironmentProbeManager ... ");

        while (EnvironmentProbeManager == null)
        {
            yield return null;
        }

        Debug.Log("EnvironmentProbeManager found! ");

        while ( EnvironmentProbeManager.subsystem == null)
        {
            yield return null;
        }

        Debug.Log("EnvironmentProbeManager.subsystem.supported : " + EnvironmentProbeManager.subsystem.supported);
    }

    void Awake()
    {
        StartCoroutine(waitForEnvManager());

        var cam = Camera.main;
        cam.transform.localPosition = Vector3.zero;
        cam.transform.localRotation = Quaternion.identity;

        virtualScenes = GameObject.FindGameObjectsWithTag("VirtualScene");

        if (!Application.isEditor)
        {
            SetVirtualSceneActive(false);
            onZeroMarkers.Invoke();
        }

        m_TrackedImageManager = GetComponent<ARTrackedImageManager>();

        Screen.sleepTimeout = SleepTimeout.NeverSleep;

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
    
    StationaryMarker lastTrackedMarker;

    ARTrackedImage lastTrackedImage;

    void OnTrackedImagesChanged(ARTrackedImagesChangedEventArgs eventArgs)
    {
        markersCount = Mathf.Max(0, markersCount) + eventArgs.added.Count - eventArgs.removed.Count;

        foreach (var trackedImage in eventArgs.added)
        {
            // Give the initial image a reasonable default scale
            trackedImage.transform.localScale = new Vector3(0.01f, 1f, 0.01f);
            
            Debug.Log("virtualMarker found! " + trackedImage.referenceImage.name + " guid: " + trackedImage.referenceImage.guid);

            MatchImageWithMarker(trackedImage);
        }

        foreach (var trackedImage in eventArgs.updated)
        {
            MatchImageWithMarker(trackedImage);
        }
    }

    private void Update()
    {
        if (lastTrackedImage != null && lastTrackedImage.trackingState != TrackingState.None)
            MatchImageWithMarker(lastTrackedImage);
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
                    //Debug.LogWarning("trackedImage scale "+ trackedImage.transform.lossyScale.ToString("F2")+" != "+ virtualMarker.transform.lossyScale.ToString("F2") + " of virtual marker! Positioning might become incorrect!");
                    trackedImage.transform.localScale = virtualMarker.transform.lossyScale;
                }

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
            var centerPoseRotationEuler = lastTrackedImage.transform.eulerAngles;
            var virtualMarkerRotation = lastTrackedMarker.transform.rotation;
            var virtualMarkerRotationEuler = lastTrackedMarker.transform.rotation.eulerAngles;

            
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
            }

            transform.SetPositionAndRotation(
                                lastTrackedMarker.transform.TransformPoint(lastTrackedImage.transform.InverseTransformPoint(transform.position)),
                                lastTrackedMarker.transform.rotation * (Quaternion.Inverse(lastTrackedImage.transform.rotation) * transform.rotation )
                                );
        }
    }
}
