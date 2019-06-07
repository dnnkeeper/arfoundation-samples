using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class RotateByDrag : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler
{
    public Vector3 rotationAxis = Vector3.forward;

    Vector3 startHitPos;

    int pointerID;

    bool isDragging;

    void Start()
    {
        targetRotation = transform.rotation;
    }

    void IBeginDragHandler.OnBeginDrag(PointerEventData eventData)
    {
        if (!isDragging)
        {
            isDragging = true;
            pointerID = eventData.pointerId;
            targetRotation = transform.rotation;
            targetRotationAngles = transform.rotation.eulerAngles;
            startHitPos = eventData.pointerCurrentRaycast.worldPosition;
        }
    }


    public float speed = 1f;

    Quaternion targetRotation;

    Vector3 targetRotationAngles;

    void IDragHandler.OnDrag(PointerEventData eventData)
    {
        if (eventData.pointerId == pointerID && eventData.pointerCurrentRaycast.isValid)
        {
            Vector3 posToStartHit = startHitPos - transform.position;

            var hitPosition = eventData.pointerCurrentRaycast.worldPosition;

            Vector3 posToNewHit = hitPosition - transform.position;

            float angleDelta = Vector3.SignedAngle(posToStartHit, posToNewHit, Vector3.up);

            Debug.DrawRay(transform.position, posToStartHit, Color.red);

            Debug.DrawRay(transform.position, posToNewHit, Color.magenta);

            startHitPos = hitPosition;

            targetRotation = Quaternion.Euler(rotationAxis * angleDelta) * targetRotation;

            targetRotationAngles += Quaternion.Euler(rotationAxis * angleDelta).eulerAngles;
        }
    }

    void Update()
    {
        if (speed >= 0)
        {
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * speed);
                        
            // Interpolate to get the current angle/axis between the source and target.
            //Vector3 currentAngle = Vector3.Lerp(transform.rotation.eulerAngles, targetRotationAngles, Time.deltaTime * speed);

            // Assign the current rotation
            //transform.rotation = Quaternion.Euler(currentAngle);
        }
        else
        {
            transform.rotation = targetRotation;
        }
    }

    void IEndDragHandler.OnEndDrag(PointerEventData eventData)
    {
        isDragging = false;
    }
}
