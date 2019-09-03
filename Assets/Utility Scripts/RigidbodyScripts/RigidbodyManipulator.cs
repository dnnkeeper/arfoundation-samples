using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Camera))]
public class RigidbodyManipulator : MonoBehaviour
{
    public Transform manipulatorSocket;

    public LayerMask raycastCollisionLayers = 1;
    public float raycastRadius = 0.1f;
    public float maxRayDistance = 2f;

    [Header("Grabbed Object Properties")]

    public Vector3 cameraLocalTargetPosition = new Vector3(0, 0, 1);
    public float grabPointOffset = 0.1f;
    public float dragAmount = 5f;
    public float angularDragAmount = 4f;
    public float breakHeight = 0.5f;
    public float forceMultiplier = 1f;
    public float maxForce = 100f;
    public float maxVelocity = 4f;

    public bool grabByHitPoint = true;
    public bool useForce = true;
    public bool compensateGravity = false;

    public float attractionSpeed = 2f;

    [Header("XR Line Settings")]
    public Material XRLineMaterial;
    public Gradient colorGradient;
    public float defaultLineWidthMultiplier = 2f;
    public float minimumWidthMultiplier = 0.5f;
    public int lineVertexCount = 20;
    public float grabSphereRadius = 0.025f;
    public float lineCurvePower = 2f;

    [ColorUsageAttribute(true, true)]
    public Color startHDRColor = new Color32(0, 54, 191, 255);
    [ColorUsageAttribute(true, true)]
    public Color endHDRColor = new Color32(191, 0, 0, 255);

    public UnityEvent onBreak;
    public UnityEvent onDrop;
    public UnityEvent onGrab;
    public UnityEvent onHasTarget;
    public UnityEvent onLostTarget;
    public UnityEvent onZeroTarget;

    private Vector3 grabDirection;

    private Transform oldObjectParent;
    private Camera _cam;
    private Rigidbody _targetedRigidbody;
    private Rigidbody _hitRigidbody;
    private XRLineRenderer _pathLineRenderer;
    private GameObject _pointRenderer;
    private GameObject _targetPointRenderer;

    private Rigidbody _grabbedRigidbody;
    private bool active;
    private Coroutine attractionCoroutine;
    private Vector3 cameraWorldSpaceTargetPosition;
    public Vector3 hitPoint, rigidbodyLocalHitPoint;
    private Vector3[] linePositions;
    private float originalAngularDragAmount;
    private float originalDragAmount;
    private RigidbodyInterpolation origObjectInterpolation;
    private float upMagnitude;

    bool targetsChanged;

    public Rigidbody hitRigidbody
    {
        get
        {
            return _hitRigidbody;
        }
        set
        {
            //Inactive object is not valid target
            if (value != null && !value.gameObject.activeSelf)
                value = null;

            if (value == null && _hitRigidbody != null)
            {
                //Debug.Log("Manipulator hit Nothing", gameObject);
                onZeroTarget.Invoke();

                //if (!active)
                //{
                //    //Debug.Log("Manipulator Lost Target", gameObject);
                //    onLostTarget.Invoke();
                //}
            }

            if (_hitRigidbody != value)
            {
                targetsChanged = true;

                _hitRigidbody = value;
            }
        }
    }

    public Camera mainCamera
    {
        get
        {
            if (_cam == null)
            {
                _cam = GetComponent<Camera>();
            }
            return _cam;
        }
    }

    public XRLineRenderer pathLineRenderer
    {
        get
        {
            if (_pathLineRenderer == null)
            {
                _pathLineRenderer = GetComponent<XRLineRenderer>();
                if (_pathLineRenderer == null)
                {
                    _pathLineRenderer = gameObject.AddComponent<XRLineRenderer>();
                    _pathLineRenderer.colorGradient = colorGradient;
                    _pathLineRenderer.SetVertexCount(lineVertexCount);
                    _pathLineRenderer.widthMultiplier = defaultLineWidthMultiplier;
                    _pathLineRenderer.material = XRLineMaterial;
                }

                Debug.Log("[Manipulator] Created LineRenderer for manipulator: " + _pathLineRenderer, gameObject);
            }
            return _pathLineRenderer;
        }
    }

    public GameObject pointRenderer
    {
        get
        {
            if (_pointRenderer == null)
            {
                var childTransform = transform.Find("ManipulatorPoint");
                if (childTransform == null)
                {
                    _pointRenderer = GameObject.CreatePrimitive(PrimitiveType.Sphere); // new GameObject("ManipulatorPoint");
                    _pointRenderer.name = "ManipulatorPoint";
                    _pointRenderer.GetComponent<Collider>().enabled = false;
                    _pointRenderer.transform.SetParent(transform);
                    _pointRenderer.transform.localScale = new Vector3(grabSphereRadius, grabSphereRadius, grabSphereRadius);
                    _pointRenderer.GetComponent<MeshRenderer>().material = new Material(Shader.Find("Mobile/Particles/Additive"));
                }
                else
                {
                    _pointRenderer = childTransform.gameObject;
                }

                Debug.Log("[Manipulator] Created PointRenderer for manipulator: " + _pointRenderer, _pointRenderer);
            }
            return _pointRenderer;
        }
    }

    public GameObject targetPointRenderer
    {
        get
        {
            if (_targetPointRenderer == null)
            {
                var childTransform = transform.Find("ManipulatorTargetPoint");
                if (childTransform == null)
                {
                    _targetPointRenderer = GameObject.CreatePrimitive(PrimitiveType.Sphere); // new GameObject("ManipulatorPoint");
                    _targetPointRenderer.name = "ManipulatorTargetPoint";
                    _targetPointRenderer.GetComponent<Collider>().enabled = false;
                    _targetPointRenderer.transform.SetParent(transform);
                    _targetPointRenderer.transform.localScale = new Vector3(grabSphereRadius, grabSphereRadius, grabSphereRadius);
                    _targetPointRenderer.GetComponent<MeshRenderer>().material = new Material(Shader.Find("Mobile/Particles/Additive"));
                }
                else
                {
                    _targetPointRenderer = childTransform.gameObject;
                }

                Debug.Log("[Manipulator] Created targetPointRenderer for " + _targetPointRenderer.name, _targetPointRenderer);
            }
            return _targetPointRenderer;
        }
    }

    public Rigidbody grabbedRigidbody
    {
        get
        {
            return _grabbedRigidbody;
        }
    }

    public void AttractSelectedItem()
    {
        if (attractionCoroutine == null)
            attractionCoroutine = StartCoroutine(attractionRoutine());
    }

    public void Toggle()
    {
        if (attractionCoroutine != null)
            return;

        if (active)
        {
            active = false;
            Debug.Log("[Manipulator] Manipulator OFF");
            onDrop.Invoke();
            DropCurrentItem();
            //if (hitRigidbody == null)
            //{
            //    Debug.Log("Manipulator Lost Target", gameObject);
            //    onLostTarget.Invoke();
            //}
        }
        else
        {
            if (_targetedRigidbody != null)
            {
                var useableObject = _targetedRigidbody.GetComponent<IUseableObjectHandler>();
                if (useableObject != null) {
                    Debug.Log("[Manipulator] Use " + _targetedRigidbody.name);
                    useableObject.Use(mainCamera.transform);
                }
            }

            if (hitRigidbody != null)
            {
                active = true;
                Debug.Log("[Manipulator] Manipulator ON GRAB " + hitRigidbody);
                _grabbedRigidbody = hitRigidbody;
                onGrab.Invoke();
                cameraWorldSpaceTargetPosition = GetGrabTargetPosition();
                origObjectInterpolation = _grabbedRigidbody.interpolation;
                //Keep object attraction position in the same stop by which it was grabbed + grabPointOffset by its normal
                cameraLocalTargetPosition = mainCamera.transform.InverseTransformPoint(hitPoint) + Vector3.up * grabPointOffset;
                Collider rbCollider = _grabbedRigidbody.GetComponent<Collider>();
                if (rbCollider == null)
                    rbCollider = _grabbedRigidbody.GetComponentInChildren<Collider>();
                var boundsExtentMagnitude = rbCollider.bounds.extents.magnitude;
                float nearPushRadius = mainCamera.nearClipPlane + boundsExtentMagnitude;// + selectedRigidbody.GetComponent<Collider>().bounds.size.magnitude;
                if (cameraLocalTargetPosition.magnitude < nearPushRadius)
                {
                    cameraLocalTargetPosition = cameraLocalTargetPosition.normalized * nearPushRadius;
                }
                pathLineRenderer.enabled = true;
                pointRenderer.SetActive(true);
                originalDragAmount = hitRigidbody.drag;
                originalAngularDragAmount = hitRigidbody.angularDrag;
                rigidbodyLocalHitPoint = hitRigidbody.transform.InverseTransformPoint(hitPoint);
                _grabbedRigidbody.drag = dragAmount;
                _grabbedRigidbody.angularDrag = angularDragAmount;
                _grabbedRigidbody.SendMessage("OnGrab", SendMessageOptions.DontRequireReceiver);
            }
        }
    }

    //public AnimationCurve attractionCurve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 1));

    private IEnumerator attractionRoutine()
    {
        Vector3 initialTargetPos = cameraLocalTargetPosition;
        Vector3 grabPointWS = _grabbedRigidbody.transform.TransformPoint(rigidbodyLocalHitPoint);
        var cameraLocalTargetPositionOriginal = cameraLocalTargetPosition;

        Vector3 localAttractionTarget = Vector3.zero;
        var collectable = _grabbedRigidbody.GetComponent<CollectableObject>();
        if (collectable != null)
        {
            //if (collectable.useable)
            //{
                var itemSocket = mainCamera.transform.Find(collectable.socketName);
                if (itemSocket != null)
                {
                    localAttractionTarget = itemSocket.localPosition;
                    Debug.Log("[Manipulator] "+itemSocket.name + " localAttractionTarget: " + localAttractionTarget.ToString("0.00"));
                }
            //}
            //else
            //{
                //localAttractionTarget = manipulatorSocket.localPosition;
            //}
        }

        bool compensateGravityOriginal = compensateGravity;
        
        compensateGravity = true;

        float t = 0;
        while (_grabbedRigidbody != null && t <= 1f)
        {
            t += Time.deltaTime * attractionSpeed;
            //grabPointWS = _selectedRigidbody.transform.TransformPoint(rigidbodyLocalHitPoint);
            
            cameraLocalTargetPosition = Vector3.Lerp(cameraLocalTargetPositionOriginal, localAttractionTarget, Mathf.Clamp01(t) );

            yield return new WaitForEndOfFrame();
        }

        compensateGravity = compensateGravityOriginal;

        if (_grabbedRigidbody != null)
        {
            Debug.Log("[Manipulator] "+_grabbedRigidbody.name+ " OnCollected");
            //_selectedRigidbody.isKinematic = true;
            var rb = _grabbedRigidbody;
            DropCurrentItem();
            yield return null;
            rb.SendMessage("OnCollected", SendMessageOptions.DontRequireReceiver);
        }

        cameraLocalTargetPosition = initialTargetPos;
        attractionCoroutine = null;
    }

    private void DrawManipulator()
    {
        if (manipulatorSocket == null)
        {
            var manipulatorSocketGO = new GameObject("ManipulatorSocket");
            manipulatorSocketGO.transform.parent = transform;
            manipulatorSocketGO.transform.localPosition = new Vector3(0f, -0.1f, 0f);
            manipulatorSocket = manipulatorSocketGO.transform;
        }

        Vector3 grabPoint = _grabbedRigidbody.transform.TransformPoint(rigidbodyLocalHitPoint);
        float distance = (grabPoint - transform.position).magnitude;
        int c = pathLineRenderer.GetPositions(linePositions);
        Vector3 linePos = manipulatorSocket.position;
        grabDirection = GetGrabTargetPositionSmooth() - _grabbedRigidbody.transform.TransformPoint(rigidbodyLocalHitPoint);
        upMagnitude = Vector3.Project(grabDirection, Vector3.up).magnitude;
        float progressToBreak = Mathf.Clamp01(upMagnitude / breakHeight);
        pathLineRenderer.material.color = (Color.Lerp(startHDRColor, endHDRColor, progressToBreak));
        pathLineRenderer.widthMultiplier = Mathf.Lerp(defaultLineWidthMultiplier, defaultLineWidthMultiplier * minimumWidthMultiplier, progressToBreak);
        //pathLineRenderer.widthEnd = Mathf.Lerp(defaultLineWidthMultiplier, defaultLineWidthMultiplier * 0.5f, progressToBreak);

        if (upMagnitude > breakHeight)
        {
            onBreak.Invoke();
            Toggle();
            return;
        }

        for (int i = 0; i < c; i++)
        {
            float t = (float)(i) / (c - 1f);
            var grabTargetPosition = Vector3.Lerp(GetGrabTargetPosition(), GetGrabTargetPositionSmooth(), t);
            linePos = manipulatorSocket.position + Vector3.Project((grabPoint - manipulatorSocket.position) * t, (grabTargetPosition - manipulatorSocket.position).normalized);
            linePos = Vector3.Lerp(linePos, grabPoint, Mathf.Pow(t, lineCurvePower));
            if (!pathLineRenderer.useWorldSpace)
                linePos = transform.InverseTransformPoint(linePos);
            linePositions[i] = linePos;
        }

        pathLineRenderer.SetPositions(linePositions);
        pointRenderer.transform.position = grabPoint;
    }

    private void DropCurrentItem()
    {
        if (_grabbedRigidbody != null)
        {
            Debug.Log("[Manipulator] DropCurrentItem " + _grabbedRigidbody);
            _grabbedRigidbody.interpolation = origObjectInterpolation;
            _grabbedRigidbody.angularDrag = originalAngularDragAmount;
            _grabbedRigidbody.drag = originalDragAmount;
            _grabbedRigidbody.SendMessage("OnDropItem", SendMessageOptions.DontRequireReceiver);
            _grabbedRigidbody = null;
            targetsChanged = true;
            rigidbodyLocalHitPoint = Vector3.zero;
        }

        pathLineRenderer.enabled = false;
        pointRenderer.SetActive(false);
    }

    

    private Vector3 GetGrabTargetPosition()
    {
        return mainCamera.transform.TransformPoint(cameraLocalTargetPosition);
    }

    private Vector3 GetGrabTargetPositionSmooth()
    {
        return cameraWorldSpaceTargetPosition;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawRay(mainCamera.transform.position, mainCamera.transform.forward*100f);

        if (_grabbedRigidbody != null)
        {
            Vector3 grabPoint = _grabbedRigidbody.transform.TransformPoint(rigidbodyLocalHitPoint);
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(grabPoint, grabSphereRadius);
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(grabPoint, GetGrabTargetPositionSmooth());
            Gizmos.DrawWireSphere(GetGrabTargetPositionSmooth(), 0.05f);
        }
        else
        {
            if (hitRigidbody != null)
            {
                Gizmos.DrawWireSphere(hitPoint, 0.01f);
            }
        }
    }

    void OnEnable()
    {
        //var camEvents = GetComponent<CameraOnRenderEvents>();
        //if (camEvents != null)
        //    camEvents.onPreRender += onPreRender;

        //canvas = GetComponent<Canvas>();
        //canvas.enabled = true;
    }

    void OnDisable()
    {
        //var camEvents = GetComponent<CameraOnRenderEvents>();
        //if (camEvents != null)
        //    camEvents.onPreRender -= onPreRender;
    }

    // Start is called before the first frame update
    private void Start()
    {
        linePositions = new Vector3[lineVertexCount];

        if (manipulatorSocket != null)
        {
            if (manipulatorSocket.GetComponentInParent<Camera>() != mainCamera)
            {
                Debug.LogWarning("[Manipulator] ManipulatorSocket parent is not a MainCamera! Find new socket in MainCamera");
                manipulatorSocket = mainCamera.transform.Find("ManipulatorSocket");
            }
        }
        else
            manipulatorSocket = mainCamera.transform.Find("ManipulatorSocket");
    }

    void onPreRender()
    {
        if (isActiveAndEnabled)
            Update();
    }

    bool collisionHit;
    public Rigidbody closestHitRb;
    RaycastHit closestHit;
    public void PerformRaycast(Vector3 rayOrigin, Vector3 rayDir)
    {
        closestHit = new RaycastHit();

        //var rayOrigin = mainCamera.transform.position + mainCamera.transform.forward * (mainCamera.nearClipPlane + raycastRadius);
        //var rayDir = mainCamera.transform.forward;
        Rigidbody rb = null;

        //RaycastHit hit;
        //var collisionHit = (Physics.SphereCast(rayOrigin, raycastRadius, rayDir, out hit, maxRayDistance, raycastCollisionLayers));
        
        RaycastHit[] hits = Physics.SphereCastAll(rayOrigin, raycastRadius, rayDir, maxRayDistance, raycastCollisionLayers);
        collisionHit = false;
        if (hits.Length > 0)
        {
            Physics.Raycast(rayOrigin, rayDir, out RaycastHit preciseHit, maxRayDistance, raycastCollisionLayers);
            //closestHit = hits[0];
            float closestDistanceToScreenCenter = Mathf.Infinity;
            if (preciseHit.rigidbody != null)
            {
                closestHit = preciseHit;
                closestDistanceToScreenCenter = (Vector3.ProjectOnPlane(preciseHit.point - rayOrigin, rayDir)).sqrMagnitude;
            }

            foreach (RaycastHit hit in hits)
            {
                if (hit.rigidbody != null)
                {
                    float radius = (Vector3.ProjectOnPlane(hit.point-rayOrigin, rayDir)).sqrMagnitude;
                    if (radius < closestDistanceToScreenCenter-0.001f )
                    {
                        closestDistanceToScreenCenter = radius;
                        closestHit = hit;
                    }
                }
            }

            collisionHit = closestHit.rigidbody != null;

            if (collisionHit)
            {

                Debug.DrawRay(rayOrigin, rayDir, Color.red);
                Debug.DrawLine(rayOrigin + rayDir, closestHit.point, Color.red);
                if (closestHit.collider.gameObject != transform.gameObject && closestHit.point.sqrMagnitude > 0f)
                {
                    rb = closestHit.collider.GetComponent<Rigidbody>();
                    if (rb == null)
                    {
                        rb = closestHit.collider.GetComponentInParent<Rigidbody>();
                    }
                    
                    
                    if (rb != null)
                    {
                        if (rb.isKinematic)
                        {
                            rb = null;
                        }
                        else
                        {
                            if (grabByHitPoint)
                            {
                                hitPoint = closestHit.point;
                            }
                            else
                                hitPoint = rb.transform.position;

                            if (rb != hitRigidbody)
                            {
                                onHasTarget.Invoke();

                                //Debug.Log("hitRigidbody = " + rb);
                            }
                        }
                    }
                }
            }
        }
        else
        {
            
            //closestHit = new RaycastHit();
        }

        
        if (_targetedRigidbody != closestHit.rigidbody)
        {
            
            if (_targetedRigidbody != null)
            {
                _targetedRigidbody.SendMessage("OnTargeted", false, SendMessageOptions.DontRequireReceiver);
            }

            var newTargetRigidbody = closestHit.rigidbody;
            //Debug.Log("_targetedRigidbody was  " + _targetedRigidbody + " become "+ newTargetRigidbody);
            targetsChanged = true;

            if (newTargetRigidbody != null)
            {
                CollectableObject collectableObject = newTargetRigidbody.GetComponent<CollectableObject>();

                if (collectableObject != null && collectableObject.IsAcquired())
                {
                    newTargetRigidbody = null;
                }
                else
                {
                    var useableObject = newTargetRigidbody.GetComponent<IUseableObjectHandler>();
                    if (useableObject != null )
                    {
                        if (useableObject.CanBeUsed())
                        {
                            Debug.Log("[Manipulator] "+newTargetRigidbody.name + " targeted");
                            newTargetRigidbody.SendMessage("OnTargeted", true, SendMessageOptions.DontRequireReceiver);
                        }
                        else
                        {
                            newTargetRigidbody = null;
                        }
                    }
                    else
                    {
                        newTargetRigidbody = null;
                    }
                }
            }
            _targetedRigidbody = newTargetRigidbody;
        }


        if (_grabbedRigidbody != null && !_grabbedRigidbody.gameObject.activeSelf)
            _grabbedRigidbody = null;

        if (attractionCoroutine == null)
            hitRigidbody = rb;
    }

    private void LateUpdate()
    {
        if (targetsChanged)
        {
            targetsChanged = false;

            if ( grabbedRigidbody != null || _targetedRigidbody!= null || hitRigidbody != null)
            {
                //Debug.Log("targetsChanged! grabbedRigidbody: " + grabbedRigidbody+ " _targetedRigidbody: "+ _targetedRigidbody+ " hitRigidbody: "+ hitRigidbody);
                onHasTarget.Invoke();
            }
            else
            {
                onLostTarget.Invoke();
            }
        }
    }

    float raycastTimer;
    float raycastPeriod = 0.05f;

    private void FixedUpdate()
    {
        if ( raycastTimer > raycastPeriod )
        {
            var rayOrigin = mainCamera.transform.position + mainCamera.transform.forward * (mainCamera.nearClipPlane + raycastRadius);
            var rayDir = mainCamera.transform.forward;
            PerformRaycast(rayOrigin, rayDir);
            raycastTimer = 0f;
        }
        raycastTimer += Time.fixedDeltaTime;

        cameraWorldSpaceTargetPosition = Vector3.Lerp(cameraWorldSpaceTargetPosition, mainCamera.transform.TransformPoint(cameraLocalTargetPosition), Time.fixedDeltaTime * 30f);

        if (active && _grabbedRigidbody != null)
        {
            _grabbedRigidbody.interpolation = RigidbodyInterpolation.Interpolate;

            if (useForce)
            {
                Vector3 grabPoint = _grabbedRigidbody.transform.TransformPoint(rigidbodyLocalHitPoint);
                Vector3 targetForce = (GetGrabTargetPositionSmooth() - grabPoint) / Time.fixedDeltaTime;

                if (compensateGravity && _grabbedRigidbody.useGravity)
                {
                    targetForce -= Physics.gravity;
                }

                targetForce = targetForce * forceMultiplier;
                targetForce = Vector3.ClampMagnitude(targetForce, maxForce);
                _grabbedRigidbody.AddForceAtPosition(targetForce, grabPoint, ForceMode.Force); //Vector3.MoveTowards(hitRigidbody.velocity, targetVelocity, Time.fixedDeltaTime * maxAcceleration)
                _grabbedRigidbody.velocity = Vector3.ClampMagnitude(_grabbedRigidbody.velocity, maxVelocity);

                Vector3 fromCameraToRb = _grabbedRigidbody.transform.position - mainCamera.transform.position;

                Collider rbCollider = _grabbedRigidbody.GetComponent<Collider>();
                if (rbCollider == null)
                    rbCollider = _grabbedRigidbody.GetComponentInChildren<Collider>();
                var boundsSizeMagnitude = rbCollider? rbCollider.bounds.size.magnitude : 0f;

                //if (fromCameraToRb.magnitude < mainCamera.nearClipPlane + boundsSizeMagnitude)
                //{
                //    _grabbedRigidbody.position = mainCamera.transform.position + fromCameraToRb.normalized * (mainCamera.nearClipPlane + _grabbedRigidbody.GetComponent<Collider>().bounds.size.magnitude);
                //    _grabbedRigidbody.transform.position = _grabbedRigidbody.position;
                //}
            }
            else
            {
                _grabbedRigidbody.MovePosition(GetGrabTargetPositionSmooth());
            }
        }
    }

    bool touched;

    private void Update()
    {
        /*if (!touched && Input.touchCount > 0)
        {
            Debug.Log("Touched");

            touched = true;

            var screenClickPosition = Input.touches[0].position;
            
            Vector3 rayOrigin = mainCamera.ScreenToWorldPoint(new Vector3(screenClickPosition.x, screenClickPosition.y, mainCamera.nearClipPlane));

            Vector3 rayDir = (rayOrigin - mainCamera.transform.position).normalized;

            PerformRaycast(mainCamera.transform.position, rayDir);

            Toggle();

            if (hitRigidbody != null && hitRigidbody.GetComponent<CollectableObject>() == null)
                cameraLocalTargetPosition = Vector3.forward;
        }
        else if (touched && Input.touchCount == 0)
        {
            Debug.Log("Touched False");

            touched = false;

            return;
        }*/

        if (Input.GetMouseButtonDown(0))
        {
            //Debug.Log("Clicked");

            var screenClickPosition = Input.mousePosition;

            Vector3 rayOrigin = mainCamera.ScreenToWorldPoint(new Vector3(screenClickPosition.x, screenClickPosition.y, mainCamera.nearClipPlane));

            Vector3 rayDir = (rayOrigin - mainCamera.transform.position).normalized;

            PerformRaycast(mainCamera.transform.position, rayDir);

            Toggle();

            if (hitRigidbody != null && hitRigidbody.GetComponent<CollectableObject>() == null)
                cameraLocalTargetPosition = Vector3.forward;
        }


        if (Input.GetKeyDown(KeyCode.Space))
        {
            Toggle();
        }

        if (_grabbedRigidbody != null && !_grabbedRigidbody.isKinematic)
        {
            if (active)
            {
                var collectable = _grabbedRigidbody.GetComponent<CollectableObject>();
                if (collectable != null && collectable.canBeAcquired)
                    AttractSelectedItem();

                DrawManipulator();
            }
            else
                DropCurrentItem();
        }
        else
        {
            if (active)
            {
                Debug.Log("[Manipulator] Deactivate manipulator due to lost rigidbody");
                Toggle();
            }
        }

        closestHitRb = closestHit.rigidbody;

        if (hitRigidbody != null && attractionCoroutine == null)// && closestHit.point.sqrMagnitude > 0f)
        {
            if (targetPointRenderer != null)
            {
                
                targetPointRenderer.SetActive(true);
                targetPointRenderer.transform.parent = null;

                Vector3 targetPointPosition = closestHit.point;

                if ( (targetPointRenderer.transform.position - targetPointPosition).sqrMagnitude < 0.1f)
                    targetPointRenderer.transform.position = Vector3.Lerp(targetPointRenderer.transform.position, targetPointPosition, 30f*Time.deltaTime);
                else
                    targetPointRenderer.transform.position = targetPointPosition;
            }
        }
        else
        {
            if (targetPointRenderer != null)
                targetPointRenderer.SetActive(false);
        }
    }
}