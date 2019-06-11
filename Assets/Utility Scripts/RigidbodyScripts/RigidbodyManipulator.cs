using System.Collections;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Camera))]
public class RigidbodyManipulator : MonoBehaviour
{
    Camera _cam;

    public float maxRayDistance = 2f;

    public LayerMask raycastCollisionLayers = 1;

    private GameObject _pointRenderer;
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
                    _pointRenderer.GetComponent<Collider>().enabled = false;
                    _pointRenderer.transform.SetParent(transform);
                    _pointRenderer.transform.localScale = new Vector3(grabSphereRadius, grabSphereRadius, grabSphereRadius);
                    _pointRenderer.GetComponent<MeshRenderer>().material = new Material(Shader.Find("Particles/Standard Unlit"));
                }
                else
                {
                    _pointRenderer = childTransform.gameObject;
                }

                Debug.Log("Created PointRenderer for manipulator: " + _pointRenderer, _pointRenderer);
            }
            return _pointRenderer;
        }
    }

    public Material XRLineMaterial;

    public float defaultLineWidthMultiplier = 2f;

    public int lineVertexCount = 20;

    private XRLineRenderer _pathLineRenderer;
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
                    //pathLineRenderer.startColor = Color.white;
                    //pathLineRenderer.endColor = Color.white;
                    _pathLineRenderer.colorGradient = colorGradient;
                    //_pathLineRenderer.numCornerVertices = 10;
                    //_pathLineRenderer.numCapVertices = 10;
                    //_pathLineRenderer.startWidth = 0.02f;
                    //_pathLineRenderer.endWidth = 0.01f;
                    //_pathLineRenderer.material = new Material(Shader.Find("Particles/Standard Unlit"));
                    //_pathLineRenderer.positionCount = lineVertexCount;
                    //_pathLineRenderer.widthStart = 0.02f;
                    //_pathLineRenderer.widthEnd = 0.02f;
                    _pathLineRenderer.SetVertexCount(lineVertexCount);
                    _pathLineRenderer.widthMultiplier = defaultLineWidthMultiplier;
                    _pathLineRenderer.material = XRLineMaterial;
                }

                Debug.Log("Created LineRenderer for manipulator: " + _pathLineRenderer, gameObject);
            }
            return _pathLineRenderer;
        }
    }

    public Transform manipulatorSocket;

    // Start is called before the first frame update
    void Start()
    {
        linePositions = new Vector3[lineVertexCount];
        //Cam = GetComponent<Camera>();
        if (manipulatorSocket != null) {
            if (manipulatorSocket.GetComponentInParent<Camera>() != mainCamera)
            {
                Debug.LogWarning("ManipulatorSocket parent is not a MainCamera! Find new socket in MainCamera");
                manipulatorSocket = mainCamera.transform.Find("ManipulatorSocket");
            }
        }
        else
            manipulatorSocket = mainCamera.transform.Find("ManipulatorSocket");

    }

    bool active;

    private Rigidbody _hitRigidbody;
    public Rigidbody hitRigidbody
    {
        get
        {
            return _hitRigidbody;
        }
        set
        {

            if (value != null && !value.gameObject.activeSelf)
                value = null;

            if (value == null && _hitRigidbody != null)
            {
                Debug.Log("Manipulator onZeroTarget", gameObject);
                onZeroTarget.Invoke();

                if (!active)
                {
                    Debug.Log("Manipulator Lost Target", gameObject);
                    onLostTarget.Invoke();
                }
            }

            _hitRigidbody = value;
        }
    }

    public Camera mainCamera { get {
            if (_cam == null)
            {
                _cam = GetComponent<Camera>();
            }
            return _cam;
        }
    }

    private Rigidbody _selectedRigidbody;
    public Rigidbody SelectedRigidbody
    {
        get
        {
            return _selectedRigidbody;
        }
    }

    float originalDragAmount;
    float originalAngularDragAmount;

    public bool grabByHitPoint = true;

    public Vector3 grabPointOffset = Vector3.up * 0.1f;

    public float dragAmount = 5f;
    public float angularDragAmount = 4f;


    void DropCurrentItem()
    {
        if (_selectedRigidbody != null)
        {
            Debug.Log("DropCurrentItem "+_selectedRigidbody);

            //_selectedRigidbody.transform.parent = oldObjectParent;
            //oldObjectParent = null;
            _selectedRigidbody.interpolation = origObjectInterpolation;
            _selectedRigidbody.angularDrag = originalAngularDragAmount;
            _selectedRigidbody.drag = originalDragAmount;
            _selectedRigidbody.SendMessage("OnDrop", SendMessageOptions.DontRequireReceiver);
            _selectedRigidbody = null;
            rigidbodyLocalHitPoint = Vector3.zero;
        }
        pathLineRenderer.enabled = false;
        pointRenderer.SetActive(false);
    }

    Vector3 hitPoint, rigidbodyLocalHitPoint;

    public UnityEvent onHasTarget;

    public UnityEvent onLostTarget;

    public UnityEvent onZeroTarget;

    public UnityEvent onGrab;
    public UnityEvent onDrop;

    protected Transform oldObjectParent;

    RigidbodyInterpolation origObjectInterpolation;

    public void Toggle()
    {
        if (active)
        {
            active = false;

            Debug.Log("Manipulator OFF");

            onDrop.Invoke();

            DropCurrentItem();

            if (hitRigidbody == null)
            {
                Debug.Log("Manipulator Lost Target", gameObject);
                onLostTarget.Invoke();
            }

        }
        else
        {
            if (hitRigidbody != null)
            {
                active = true;

                Debug.Log("Manipulator ON GRAB "+hitRigidbody);

                _selectedRigidbody = hitRigidbody;

                onGrab.Invoke();

                cameraWorldSpaceTargetPosition = GetGrabTargetPosition();

                origObjectInterpolation = _selectedRigidbody.interpolation;

                //oldObjectParent = _selectedRigidbody.transform.parent;

                //_selectedRigidbody.transform.SetParent(transform);

                //Keep object attraction position in the same stop by which it was grabbed + grabPointOffset

                cameraLocalTargetPosition = mainCamera.transform.InverseTransformPoint(hitPoint) + grabPointOffset;

                float nearPushRadius = mainCamera.nearClipPlane + _selectedRigidbody.GetComponent<Collider>().bounds.extents.magnitude;// + selectedRigidbody.GetComponent<Collider>().bounds.size.magnitude;

                if (cameraLocalTargetPosition.magnitude < nearPushRadius)
                {
                    cameraLocalTargetPosition = cameraLocalTargetPosition.normalized * nearPushRadius;
                }

                pathLineRenderer.enabled = true;

                pointRenderer.SetActive(true);

                originalDragAmount = hitRigidbody.drag;

                originalAngularDragAmount = hitRigidbody.angularDrag;

                

                rigidbodyLocalHitPoint = hitRigidbody.transform.InverseTransformPoint(hitPoint);

                _selectedRigidbody.drag = dragAmount;

                _selectedRigidbody.angularDrag = angularDragAmount;

                _selectedRigidbody.SendMessage("OnGrab", SendMessageOptions.DontRequireReceiver);

                //float nearPushRadius = mainCamera.nearClipPlane + selectedRigidbody.GetComponent<Collider>().bounds.size.magnitude;

                //if (cameraLocalTargetPosition.magnitude < nearPushRadius)
                //{
                //    cameraLocalTargetPosition = cameraLocalTargetPosition.normalized * nearPushRadius;
                //}

            }
        }
    }

    public float raycastRadius = 0.1f;

    void Update()
    {
        var rayOrigin = mainCamera.transform.position;
        var rayDir = mainCamera.transform.forward;

        Rigidbody rb = null;

        RaycastHit hit;

        if (Physics.SphereCast(rayOrigin, raycastRadius, rayDir, out hit, maxRayDistance, raycastCollisionLayers))
        {
            Debug.DrawLine(rayOrigin, hit.point, Color.red);
            if (hit.collider.gameObject != transform.gameObject)
            {
                rb = hit.collider.GetComponent<Rigidbody>();
                if (rb == null)
                {
                    rb = hit.collider.GetComponentInParent<Rigidbody>();
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
                            hitPoint = hit.point;
                            
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

        if (_selectedRigidbody != null && !_selectedRigidbody.gameObject.activeSelf)
            _selectedRigidbody = null;

        hitRigidbody = rb;

        if (Input.GetKeyDown(KeyCode.Space))
        {
            Toggle();
        }

        if (_selectedRigidbody != null && !_selectedRigidbody.isKinematic)
        {
            if (active)
            {
                var collectable = _selectedRigidbody.GetComponent<CollectableObject>();
                if (collectable != null && collectable.canBePickedUp)
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
                Debug.Log("Deactivate manipulator due to lost rigidbody");
                Toggle();
            }
        }
    }

    //private void OnPreRender()
    //{
    //    if (_selectedRigidbody != null && !_selectedRigidbody.isKinematic)
    //    {
    //        if (active)
    //        {
    //            DrawManipulator();
    //        }
    //    }
    //}

    //public float trackingForce = 10f;

    public float maxVelocity = 4f;

    public float maxForce = 100f;

    public float forceMultiplier = 1f;

    //public float maxAcceleration = 5f;

    //public float minDiff = 5f;

    public Vector3 cameraLocalTargetPosition = new Vector3(0, 0, 1);

    public float attractionSpeed = 2f;

    Vector3 cameraWorldSpaceTargetPosition;

    Vector3 GetGrabTargetPositionSmooth()
    {
        return cameraWorldSpaceTargetPosition;
    }
    Vector3 GetGrabTargetPosition()
    {
        return mainCamera.transform.TransformPoint(cameraLocalTargetPosition);
    }

    Coroutine attractionCoroutine;

    public void AttractSelectedItem()
    {
        if (attractionCoroutine == null)
            attractionCoroutine = StartCoroutine(attractionRoutine());
    }

    IEnumerator attractionRoutine()
    {
        Vector3 initialTargetPos = cameraLocalTargetPosition;
        Vector3 grabPointWS = _selectedRigidbody.transform.TransformPoint(rigidbodyLocalHitPoint);
        var cameraLocalTargetPositionOriginal = cameraLocalTargetPosition;
        float t = 0;
        while (_selectedRigidbody != null &&  t <= 1f )//(grabPointWS - manipulatorSocket.position).sqrMagnitude > 0.01f)
        {
            t += Time.deltaTime * attractionSpeed;
            grabPointWS = _selectedRigidbody.transform.TransformPoint(rigidbodyLocalHitPoint);
            cameraLocalTargetPosition = Vector3.Lerp(cameraLocalTargetPositionOriginal, Vector3.zero, Mathf.Clamp01(t));
            yield return null;
        }

        if (_selectedRigidbody != null)
        {
            _selectedRigidbody.SendMessage("OnCollected", SendMessageOptions.DontRequireReceiver);
        }

        cameraLocalTargetPosition = initialTargetPos;

        attractionCoroutine = null;
    }

    public bool useForce = true;

    public bool compensateGravity = false;

    private void FixedUpdate()
    {
        cameraWorldSpaceTargetPosition = Vector3.Lerp(cameraWorldSpaceTargetPosition, mainCamera.transform.TransformPoint(cameraLocalTargetPosition), Time.fixedDeltaTime * 30f);

        if (active && _selectedRigidbody != null)
        {
            _selectedRigidbody.interpolation = RigidbodyInterpolation.Interpolate;

            if (useForce)
            {
                Vector3 grabPoint = _selectedRigidbody.transform.TransformPoint(rigidbodyLocalHitPoint);

                Vector3 targetForce = (GetGrabTargetPositionSmooth() - grabPoint) / Time.fixedDeltaTime;

                if (compensateGravity && _selectedRigidbody.useGravity)
                {
                    targetForce -= Physics.gravity;
                }

                targetForce = targetForce * forceMultiplier;
                targetForce = Vector3.ClampMagnitude(targetForce, maxForce);
                _selectedRigidbody.AddForceAtPosition(targetForce, grabPoint, ForceMode.Force); //Vector3.MoveTowards(hitRigidbody.velocity, targetVelocity, Time.fixedDeltaTime * maxAcceleration)
                _selectedRigidbody.velocity = Vector3.ClampMagnitude(_selectedRigidbody.velocity, maxVelocity);

                Vector3 fromCameraToRb = _selectedRigidbody.transform.position - mainCamera.transform.position;
                if (fromCameraToRb.magnitude < mainCamera.nearClipPlane + _selectedRigidbody.GetComponent<Collider>().bounds.size.magnitude)
                {
                    _selectedRigidbody.position = mainCamera.transform.position + fromCameraToRb.normalized * (mainCamera.nearClipPlane + _selectedRigidbody.GetComponent<Collider>().bounds.size.magnitude);
                    _selectedRigidbody.transform.position = _selectedRigidbody.position;
                }
            }
            else
            {
                //_selectedRigidbody.position = GetGrabTargetPosition();
                //_selectedRigidbody.transform.position = _selectedRigidbody.position;
                //_selectedRigidbody.Sleep();
                
                _selectedRigidbody.MovePosition(GetGrabTargetPositionSmooth());
            }
        }
    }

    public Gradient colorGradient;

    public float lineCurvePower = 2f;

    Vector3[] linePositions;

    //Vector3 grabPointToTargetPoint;

    //public float grabLeanRate = 4f;

    public UnityEvent onBreak;

    public float breakHeight = 0.5f;

    float upMagnitude;

    public Vector3 grabDirection;

    //public float grabNormalDistance = 0.3f;

    [ColorUsageAttribute(true, true)]
    public Color startHDRColor = new Color32(0, 54, 191, 255);
    [ColorUsageAttribute(true, true)]
    public Color endHDRColor = new Color32(191, 0, 0, 255);

    void DrawManipulator()
    {
        //List<Vector3> corners = new List<Vector3>();

        if (manipulatorSocket == null)
        {
            var manipulatorSocketGO = new GameObject("ManipulatorSocket");
            manipulatorSocketGO.transform.parent = transform;
            manipulatorSocketGO.transform.localPosition = new Vector3(0f, -0.1f, 0f);
            manipulatorSocket = manipulatorSocketGO.transform;
        }

        Vector3 grabPoint = _selectedRigidbody.transform.TransformPoint(rigidbodyLocalHitPoint);

        float distance = (grabPoint - transform.position).magnitude;

        int c = pathLineRenderer.GetPositions(linePositions);

        Vector3 linePos = manipulatorSocket.position;

        grabDirection = GetGrabTargetPositionSmooth() - _selectedRigidbody.transform.TransformPoint(rigidbodyLocalHitPoint);

        upMagnitude = Vector3.Project(grabDirection, Vector3.up).magnitude;

        float progressToBreak = Mathf.Clamp01(upMagnitude / breakHeight);

        pathLineRenderer.material.color = (Color.Lerp(startHDRColor, endHDRColor, progressToBreak));

        pathLineRenderer.widthStart = Mathf.Lerp(0.002f, 0.001f, progressToBreak);

        pathLineRenderer.widthEnd = Mathf.Lerp(0.01f, 0.001f, progressToBreak);

        if (upMagnitude > breakHeight)
        {
            onBreak.Invoke();
            Toggle();
            return;
        }

        //pathLineRenderer.startColor = 

        for (int i = 0; i < c; i++)
        {
            float t = (float)(i) / (c-1f);
            //linePos = manipulatorSocket.position + Vector3.Project( (grabPoint - manipulatorSocket.position) * t, manipulatorSocket.forward);
            //linePos = manipulatorSocket.position + Vector3.Project((grabPoint - manipulatorSocket.position) * t, ( ( grabPoint+grabPointToTargetPoint.normalized* grabNormalDistance) - manipulatorSocket.position).normalized );
            var grabTargetPosition = Vector3.Lerp( GetGrabTargetPosition(), GetGrabTargetPositionSmooth(), t);
            linePos = manipulatorSocket.position + Vector3.Project((grabPoint - manipulatorSocket.position) * t, (grabTargetPosition - manipulatorSocket.position).normalized);
            linePos = Vector3.Lerp(linePos, grabPoint, Mathf.Pow(t, lineCurvePower));
            if (!pathLineRenderer.useWorldSpace)
                linePos = transform.InverseTransformPoint(linePos);
            linePositions[i] = linePos;
        }

        pathLineRenderer.SetPositions(linePositions);


        //var newPoint = linePositions[linePositions.Length - 1];

        //grabPointToTargetPoint = Vector3.Lerp(grabPointToTargetPoint, GetGrabTargetPosition() - grabPoint, Time.deltaTime * grabLeanRate);

        //Debug.DrawLine(grabPoint, grabPoint + grabPointToTargetPoint, Color.red);

        pointRenderer.transform.position = grabPoint;
    }

    public float grabSphereRadius = 0.025f;

    private void OnDrawGizmosSelected()
    
    {
        if (_selectedRigidbody != null)
        {
            Vector3 grabPoint = _selectedRigidbody.transform.TransformPoint(rigidbodyLocalHitPoint);
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
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(hitPoint, 0.01f);
            }
            else
            {
                //if (mainCamera != null)
                //{
                //    Gizmos.color = Color.red;
                //    Gizmos.DrawRay(mainCamera.transform.position, mainCamera.transform.forward);
                //}
            }
        }
    }
}
