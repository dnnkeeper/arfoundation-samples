using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class OnDistanceEvent : MonoBehaviour
{
    public Transform target;

    public float distance = 1f;

    public bool isInRadius;

    public UnityEvent onApproach;
    public UnityEvent onLeave;
    // Update is called once per frame
    void Update()
    {
        if (target != null)
        {
            if (isInRadius && (transform.position - target.position).sqrMagnitude >= distance)
            {
                Debug.Log("onLeave"+ target);
                isInRadius = false;
                onLeave.Invoke();
            }
            else if (!isInRadius && (transform.position - target.position).sqrMagnitude < distance)
            {
                Debug.Log("onApproach" + target);
                isInRadius = true;
                onApproach.Invoke();
            }
        }
    }
}
