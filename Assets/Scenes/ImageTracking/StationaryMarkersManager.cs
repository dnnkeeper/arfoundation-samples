using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
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

    public UnityEvent onDriftCorrected;

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
                    StartCoroutine(Delay(1.0f, () => { if (_markersCount == 0) SetVirtualSceneActive(false); }));
                    //SetVirtualSceneActive(false);
                    onZeroMarkers.Invoke();
                }

                if (value > 0)
                {
                    SetVirtualSceneActive(true);
                    onHasMarkers.Invoke();
                }

                _markersCount = value;
                Debug.Log("[MarkersManager] " + "markersCount: " + _markersCount);
            }
        }
    }
    [ContextMenu("OnHasMarkersTest")]
    public void OnHasMarkersTest()
    {
        onDriftCorrected.Invoke();
        onHasMarkers.Invoke();
    }

    public static IEnumerator Delay(float delay, System.Action action)
    {
        yield return new WaitForSeconds(delay);
        action.Invoke();
    }


    GameObject[] virtualScenes;

    public void SetVirtualSceneActive(bool b)
    {
        Debug.Log("[MarkersManager] " + "SetVirtualSceneActive " + b);

        foreach (var virtualScene in virtualScenes)
        {
            if (virtualScene != null)
                virtualScene.SetActive(b);
        }
    }


    ARTrackedImageManager m_TrackedImageManager;

    public Dictionary<Guid, StationaryMarker> virtualMarkersDict
            = new Dictionary<Guid, StationaryMarker>();

    IEnumerator waitForEnvManager()
    {
        AREnvironmentProbeManager EnvironmentProbeManager = GetComponent<AREnvironmentProbeManager>();

        Debug.Log("[MarkersManager] " + "Waiting for EnvironmentProbeManager ... ");

        while (EnvironmentProbeManager == null)
        {
            yield return null;
        }

        Debug.Log("[MarkersManager] " + "EnvironmentProbeManager found! ");

        while (EnvironmentProbeManager.subsystem == null)
        {
            yield return null;
        }

        Debug.Log("[MarkersManager] " + "EnvironmentProbeManager.subsystem.supported : " + EnvironmentProbeManager.subsystem.supported);
    }

    void Awake()
    {
        m_TrackedImageManager = GetComponent<ARTrackedImageManager>();

        Screen.sleepTimeout = SleepTimeout.NeverSleep;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log("[MarkersManager] SessionOrigin rotation = " + transform.rotation.eulerAngles.ToString("F2"));

        //StartCoroutine(waitForEnvManager());

        if (!Application.isEditor)
        {
            var cam = Camera.main;
            cam.transform.localPosition = Vector3.zero;
            cam.transform.localRotation = Quaternion.identity;
        }

        var virtualMarkers = GameObject.FindObjectsOfType<StationaryMarker>();

        Debug.Log("[MarkersManager] " + virtualMarkers.Length + " virtual markers found on scene " + SceneManager.GetActiveScene().name);

        foreach (var virtualMarker in virtualMarkers)
        {
            if (virtualMarker.imageLibrary != null)
            {
                if (virtualMarker.imageIndex < virtualMarker.imageLibrary.count)
                {

                    var referenceImage = virtualMarker.imageLibrary[virtualMarker.imageIndex];

                    if (!virtualMarkersDict.ContainsKey(referenceImage.guid))
                    {
                        Debug.Log("[MarkersManager] " + virtualMarker.name + " added for reference image: " + referenceImage.name);
                        virtualMarkersDict.Add(referenceImage.guid, virtualMarker);
                    }
                    else
                    {
                        Debug.LogError("[MarkersManager] virtualMarkersDict already contains marker with guid " + referenceImage.guid + " texture: " + referenceImage.texture, virtualMarker);
                    }
                }
                else
                {
                    Debug.LogError(virtualMarker.imageIndex + " index not present in imageLibrary.count: " + virtualMarker.imageLibrary.count);
                }
            }
            else
            {
                Debug.LogError("[MarkersManager] " + virtualMarker.name + " imageLibrary is Null");
            }
            virtualMarker.gameObject.SetActive(false);
        }

        virtualScenes = GameObject.FindGameObjectsWithTag("VirtualScene");

        Debug.Log("[MarkersManager] virtualScenes found " + virtualScenes.Length);

        if (!Application.isEditor)
        {
            SetVirtualSceneActive(false);
            onZeroMarkers.Invoke();
        }
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;

        transform.rotation = Quaternion.identity;

        m_TrackedImageManager.trackedImagesChanged += OnTrackedImagesChanged;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;

        m_TrackedImageManager.trackedImagesChanged -= OnTrackedImagesChanged;

    }

    public StationaryMarker lastTrackedMarker;
    public StationaryMarker LastTrackedMarker { get => lastTrackedMarker; }

    ARTrackedImage lastTrackedImage;
    public ARTrackedImage LastTrackedImage { get => lastTrackedImage; }

    List<ARTrackedImage> trackedImages = new List<ARTrackedImage>();

    private Vector3 driftVector;
    public Vector3 DriftVector
    {
        get
        {
            return driftVector;
        }
    }
    private float driftMagnitude;
    public float DriftMagnitude
    {
        get
        {
            return driftMagnitude;
        }
    }

    void OnTrackedImagesChanged(ARTrackedImagesChangedEventArgs eventArgs)
    {
        foreach (var trackedImage in eventArgs.added)
        {
            if (!trackedImages.Contains(trackedImage))
                trackedImages.Add(trackedImage);

            // Give the initial image a reasonable default scale
            trackedImage.transform.localScale = new Vector3(0.01f, 1f, 0.01f);

            Debug.Log("[MarkersManager] " + "trackedImage Add: " + trackedImage.referenceImage.name + " guid: " + trackedImage.referenceImage.guid);

            MatchImageWithMarker(trackedImage);

        }

        foreach (var trackedImage in eventArgs.updated)
        {
            MatchImageWithMarker(trackedImage);
        }

        foreach (var trackedImage in eventArgs.removed)
        {
            if (trackedImages.Contains(trackedImage))
            {
                Debug.Log("[MarkersManager] trackedImage Remove: " + trackedImage.name);
                trackedImages.Remove(trackedImage);
            }
        }

        //markersCount = Mathf.Max(0, markersCount) + eventArgs.added.Count - eventArgs.removed.Count;
        int activeTrackedImagesCount = 0;
        foreach (var trackedImage in trackedImages)
        {
#if UNITY_IOS
            activeTrackedImagesCount++;
#else
            if (trackedImage.trackingState != TrackingState.None)
            {
                activeTrackedImagesCount++;
            }
#endif
        }
        markersCount = activeTrackedImagesCount;
    }

    public Transform compassTransform;

    float headingSmooth;

    public Transform debugLastImageTransform;

    private void Update()
    {
        if (Input.GetKey(KeyCode.Escape))
        {
            if (SceneManager.GetActiveScene().buildIndex != 0)
            {
                SceneManager.LoadScene(0);
            }
            else
            {
                Application.Quit();
            }
        }

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
                var camPlanarRotation = Quaternion.LookRotation(Vector3.Cross(camera.transform.right, Vector3.up), Vector3.up);

                var rotationY = camPlanarRotation.eulerAngles.y - headingSmooth;

                var compassRotation = Quaternion.Euler(0f, rotationY, 0f);

                if (compassTransform != null)
                    compassTransform.rotation = compassRotation;//Quaternion.Lerp(compassTransform.rotation, Quaternion.Euler(0f, rotationY, 0f), Time.deltaTime);

                if (Input.compass.headingAccuracy > 0 && Input.compass.headingAccuracy < 5f)
                {
                    var correctedRotation = transform.rotation * Quaternion.Inverse(compassRotation);

                    transform.rotation = correctedRotation;
                }
            }
        }

        if (lastTrackedImage != null && lastTrackedImage.trackingState == TrackingState.Tracking)
            MatchImageWithMarker(lastTrackedImage);

        SyncOffset();
    }

    StationaryMarker MatchImageWithMarker(ARTrackedImage trackedImage)
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
                //SyncOffset();
            }
            else
            {
                virtualMarker.gameObject.SetActive(false);
            }
        }
        else
        {
            Debug.LogWarning("[MarkersManager] " + "Image " + trackedImage.referenceImage.name + " has no corresponding StationaryMarker!");
        }

        return virtualMarker;
    }

    void SyncOffset()
    {
        if (lastTrackedMarker != null && lastTrackedImage != null)
        {
            Transform sessionOrigin = transform;

            Vector3 sceneSpaceUp = Vector3.up;

            //Calculate how marker is rotated relative to the plane with markerSpaceUp normal and marker.right vector belonging to the plane
            Quaternion markerRotationOnPlane = Quaternion.LookRotation(Vector3.Cross(Vector3.ProjectOnPlane(lastTrackedMarker.transform.right, sceneSpaceUp), sceneSpaceUp), sceneSpaceUp);
            Quaternion fromMarkerRotationOnPlaneToMarker = Quaternion.Inverse(markerRotationOnPlane) * lastTrackedMarker.transform.rotation;

            //Calculate how detected image is rotated relative to the plane with world up normal and image.right vector belonging to the plane
            Quaternion imageRotationOnPlane = Quaternion.LookRotation(Vector3.Cross(Vector3.ProjectOnPlane(lastTrackedImage.transform.localRotation * Vector3.right, Vector3.up), Vector3.up), Vector3.up);

            Quaternion fromImageRotationOnPlaneToImage = Quaternion.Inverse(imageRotationOnPlane) * lastTrackedImage.transform.localRotation;

            //Correct image rotation with its known marker rotation
            lastTrackedImage.transform.localRotation = imageRotationOnPlane * fromMarkerRotationOnPlaneToMarker;

            //Find world position offset for ARSession: get lastTrackedMarker position (in unity scene) and interpret session position in detected image local space as marker local position
            var correctedWorldOriginPos = lastTrackedMarker.transform.TransformPoint(lastTrackedImage.transform.InverseTransformPoint(sessionOrigin.position));

            //Find world rotation offset for ARSession: get lastTrackedMarker rotation (in unity scene) and rotate it as session origin is rotated to detected image
            var correctedWorldOriginRot = lastTrackedMarker.transform.rotation * (Quaternion.Inverse(lastTrackedImage.transform.rotation) * sessionOrigin.rotation);

            //Drift measurement
            driftVector = correctedWorldOriginPos - sessionOrigin.position;
            var driftRotation = Quaternion.Angle(sessionOrigin.rotation, correctedWorldOriginRot);
            driftMagnitude = driftVector.magnitude;
            if (driftMagnitude >= 0.01f)
            {
                Debug.Log("[MarkersManager] " + "Drift: " + driftMagnitude.ToString("0.000m") + " " + driftVector + " " + driftRotation + " deg");
                if (!supressDriftEventOnce)
                {
                    onDriftCorrected.Invoke();
                }
                else
                {
                    supressDriftEventOnce = false;
                    Debug.Log("Listener ignor drift on this Frame!");
                }

                Vector3 markerDrift = lastTrackedImage.transform.position - lastTrackedMarker.transform.position;
                Debug.Log("[MarkersManager] " + "MarkerDrift: " + markerDrift.magnitude.ToString("0.000m") + " " + markerDrift + " " + Quaternion.Angle(lastTrackedImage.transform.rotation, lastTrackedMarker.transform.rotation) + " deg");

                if (debugLastImageTransform != null)
                {
                    debugLastImageTransform.parent = sessionOrigin;
                    debugLastImageTransform.localPosition = sessionOrigin.InverseTransformPoint(lastTrackedMarker.transform.position);
                    debugLastImageTransform.localRotation = Quaternion.Inverse(sessionOrigin.rotation) * lastTrackedMarker.transform.rotation;
                }

                var lineRenderer = lastTrackedMarker.GetComponentInChildren<LineRenderer>();
                if (lineRenderer != null)
                {
                    lineRenderer.useWorldSpace = true;
                    var index = lineRenderer.positionCount % 10;
                    lineRenderer.positionCount = Mathf.Min(10, (index + 1));
                    lineRenderer.SetPosition(0, LastTrackedMarker.transform.position + Vector3.up * 0.1f);
                    lineRenderer.SetPosition(lineRenderer.positionCount - 1, (LastTrackedMarker.transform.position - markerDrift) + Vector3.up * 0.1f);

                }

                var driftInfo = lastTrackedMarker.transform.Find("DriftInfo");
                if (driftInfo != null)
                {
                    driftInfo.GetComponentInChildren<Text>().text = "Drift: " + markerDrift.magnitude.ToString("0.00m");
                }
            }

            sessionOrigin.SetPositionAndRotation(
                                correctedWorldOriginPos,
                                correctedWorldOriginRot
                                );
        }
    }

    bool supressDriftEventOnce = false;
    public void SupressDriftEventOnce() {
        supressDriftEventOnce = true;
    }

    //void OnGUI()
    //{
    //    GUILayout.Label("markersCount: " + markersCount);
    //    foreach (var trackedImage in trackedImages)
    //    {
    //        if (lastTrackedImage == trackedImage && trackedImage.trackingState == TrackingState.Tracking)
    //        {
    //            var style = new GUIStyle(GUI.skin.label);
    //            style.normal.textColor = Color.green;

    //            GUILayout.Label(trackedImage.referenceImage.name + " trackingState " + trackedImage.trackingState, style);
    //        }
    //        else
    //            GUILayout.Label(trackedImage.referenceImage.name + " trackingState " + trackedImage.trackingState);
    //    }
    //    //GUILayout.Label("compass accuracy: " + Input.compass.headingAccuracy.ToString("F2"));
    //}
}
