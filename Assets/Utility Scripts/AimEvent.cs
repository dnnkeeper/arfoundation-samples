using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class AimEvent : MonoBehaviour
{
    //public Vector3 centerViewportPos = new Vector3(0.5f, 0.5f, 0f);

    public UnityEvent onAimed;
    public UnityEvent onNotAimed;

    public Transform[] aimableTransforms;

    static Transform closest = null;
    //static float closestDistance = Mathf.Infinity;

    Camera _cam;

    protected void Start()
    {
        _cam = GetComponent<Camera>();
        foreach (var t in aimableTransforms)
        {
            t.SendMessage("NotAimed", SendMessageOptions.DontRequireReceiver);
        }
    }

    Vector3 getProjectedPosFromCenter(Transform t)
    {
        return (_cam.WorldToViewportPoint(t.position) - new Vector3(0.5f, 0.5f, 0f) );
    }

    // Update is called once per frame
    void Update()
    {
        if (_cam != null)
        {
            Transform newClosest = null;

            float newClosestDistance = Mathf.Infinity;

            foreach (var t in aimableTransforms)
            {
                if (Vector3.Dot(t.position - _cam.transform.position, _cam.transform.forward) > 0)
                {
                    var viewportPos = getProjectedPosFromCenter(t);
                    viewportPos.z = 0f;
                    float distance = viewportPos.sqrMagnitude;
                    if (distance < newClosestDistance)
                    {
                        newClosest = t;
                        newClosestDistance = distance;
                    }
                }
            }

            if (newClosest != closest)
            {
                if (closest != null)
                    closest.SendMessage("NotAimed", SendMessageOptions.DontRequireReceiver);

                closest = newClosest;
                if (newClosest != null)
                    newClosest.SendMessage("Aimed", SendMessageOptions.DontRequireReceiver);
            }
        }
    }

    private void LateUpdate()
    {
    }

    public void Aimed()
    {
        onAimed.Invoke();
    }

    public void NotAimed()
    {
        onNotAimed.Invoke();
    }
}
