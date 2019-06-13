﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

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

        while (EnvironmentProbeManager.subsystem == null)
        {
            yield return null;
        }

        Debug.Log("EnvironmentProbeManager.subsystem.supported : " + EnvironmentProbeManager.subsystem.supported);
    }

    void Awake()
    {
        Debug.Log("SessionOrigin rotation = " + transform.rotation.eulerAngles.ToString("F2"));

        StartCoroutine(waitForEnvManager());

        var cam = Camera.main;
        cam.transform.localPosition = Vector3.zero;
        cam.transform.localRotation = Quaternion.identity;

        

        m_TrackedImageManager = GetComponent<ARTrackedImageManager>();

        Screen.sleepTimeout = SleepTimeout.NeverSleep;

        var virtualMarkers = GameObject.FindObjectsOfType<StationaryMarker>();
        foreach (var virtualMarker in virtualMarkers)
        {
            var referenceImage = virtualMarker.imageLibrary[virtualMarker.imageIndex];

            if (!virtualMarkersDict.ContainsKey(referenceImage.guid))
            {
                virtualMarkersDict.Add(referenceImage.guid, virtualMarker);
            }
            else
            {
                Debug.LogError("virtualMarkersDict already contains marker with guid " + referenceImage.guid + " texture: " + referenceImage.texture);
            }

            virtualMarker.gameObject.SetActive(false);
        }

        virtualScenes = GameObject.FindGameObjectsWithTag("VirtualScene");

        if (!Application.isEditor)
        {
            SetVirtualSceneActive(false);
            onZeroMarkers.Invoke();
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

    List<ARTrackedImage> trackedImages = new List<ARTrackedImage>();

    void OnTrackedImagesChanged(ARTrackedImagesChangedEventArgs eventArgs)
    {


        foreach (var trackedImage in eventArgs.added)
        {
            if (!trackedImages.Contains(trackedImage))
                trackedImages.Add(trackedImage);

            // Give the initial image a reasonable default scale
            trackedImage.transform.localScale = new Vector3(0.01f, 1f, 0.01f);

            Debug.Log("virtualMarker found! " + trackedImage.referenceImage.name + " guid: " + trackedImage.referenceImage.guid);

            MatchImageWithMarker(trackedImage);
        }

        foreach (var trackedImage in eventArgs.updated)
        {
            MatchImageWithMarker(trackedImage);
        }

        foreach (var trackedImage in eventArgs.removed)
        {
            if (trackedImages.Contains(trackedImage))
                trackedImages.Remove(trackedImage);
        }

        //markersCount = Mathf.Max(0, markersCount) + eventArgs.added.Count - eventArgs.removed.Count;
        int activeTrackedImagesCount = 0;
        foreach (var trackedImage in trackedImages)
        {
            if (trackedImage.trackingState == TrackingState.Tracking)
            {
                activeTrackedImagesCount++;
            }
        }
        markersCount = activeTrackedImagesCount;
    }

    public Transform compassTransform;

    float headingSmooth;

    private void Update()
    {
        if (!Input.compass.enabled)
        {
            Input.location.Start();
            Input.compass.enabled = true;
        }

        headingSmooth = Mathf.Lerp(headingSmooth, Input.compass.trueHeading, Time.deltaTime);
        if (!Application.isEditor)
        {
            var sessionOrigin = GetComponent<ARSessionOrigin>();
            var camera = sessionOrigin.camera;
            var prjMat = camera.projectionMatrix;

            var fov_y = Mathf.Atan(1 / prjMat[5]) * 2f * Mathf.Rad2Deg;

            camera.fieldOfView = fov_y;

            if (markersCount <= 0)
            {
                if (compassTransform != null)
                {

                    var camPlanarRotation = Quaternion.LookRotation(Vector3.Cross(camera.transform.right, Vector3.up), Vector3.up);

                    var rotationY = camPlanarRotation.eulerAngles.y - headingSmooth;

                    compassTransform.rotation = Quaternion.Euler(0f, rotationY, 0f);//Quaternion.Lerp(compassTransform.rotation, Quaternion.Euler(0f, rotationY, 0f), Time.deltaTime);

                    if (Input.compass.headingAccuracy > 0 && Input.compass.headingAccuracy < 5f)
                    {
                        var correctedRotation = transform.rotation * Quaternion.Inverse(compassTransform.rotation);

                        //if (Quaternion.Angle(transform.rotation, correctedRotation) > 30f)
                        //{
                        //    transform.rotation = correctedRotation;
                        //}
                        //else
                        transform.rotation = correctedRotation;//Quaternion.Lerp(transform.rotation, correctedRotation, Time.deltaTime * 2f);
                    }
                }
            }
        }

        if (lastTrackedImage != null && lastTrackedImage.trackingState == TrackingState.Tracking)
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
                                lastTrackedMarker.transform.rotation * (Quaternion.Inverse(lastTrackedImage.transform.rotation) * transform.rotation)
                                );
        }
    }

    void OnGUI()
    {
        GUILayout.Label("compass accuracy: "+Input.compass.headingAccuracy.ToString("F2"));
        GUILayout.Label("markersCount: " + markersCount);
        foreach (var trackedImage in trackedImages)
        {
            GUILayout.Label(trackedImage.referenceImage.name + " " + trackedImage.trackingState);
        }
    }
}
