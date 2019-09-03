using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class ItemDragHandler : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler, IPointerClickHandler
{
    public bool active = true;

    public bool canBeSwipedUp = true;

    public bool canBeSwipedDown = true;

    public float maxThrowVelocity = 10f;
    /// <summary>
    /// Minimal drag distance to perform action
    /// </summary>
    [Range(0f, 1f)]
    public float minDragDistance = 0.2f;

    public float throwPowerMultiplier = 1.0f;

    public UnityEvent onSwipeUp;
    public UnityEvent onSwipeDown;
    public UnityEvent onClick;

    public bool selectNextItemAfterThrow = true;

    /// <summary>
    /// How fast does start drag position lerps to current drag position
    /// </summary>
    //public float dragReductionSpeed = 5.0f;

    Vector2 startDragPos, lastDragPos;

    public void OnBeginDrag(PointerEventData eventData)
    {
        startDragPos = eventData.position;
    }

    float dragSpeed;

    float timestampDrag;

    public void OnDrag(PointerEventData eventData)
    {
        float deltaTime = Time.time - timestampDrag;

        var deltaVector = new Vector2(eventData.delta.x / eventData.pressEventCamera.pixelWidth, eventData.delta.y / eventData.pressEventCamera.pixelHeight);

        dragSpeed = deltaVector.y / deltaTime;//Mathf.Lerp(dragSpeed, deltaVector.y / deltaTime, deltaTime * 30f); 

        lastDragPos = eventData.position;

        timestampDrag = Time.time;
    }

    private void Update()
    {
        //startDragPos = Vector2.Lerp(startDragPos, lastDragPos, dragReductionSpeed * Time.deltaTime);
    }

    void OnAcquire()
    {
        Debug.Log("OnAcquire "+name);
        active = true;
    }

    void OnDropItem()
    {
        Debug.Log("OnDropItem "+name);
        active = false;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!active)
        {
            Debug.LogWarning("ItemDragHandler inactive", this);
            return;
        }

        if (Application.isEditor)
        {
            if (!Input.GetMouseButtonUp(0))
                return;
        }

        var delta = eventData.pressEventCamera.ScreenToViewportPoint(eventData.position - startDragPos);

        if (canBeSwipedUp && delta.y >= minDragDistance)
        {
            Debug.Log(name+" onSwipeUp delta: " + delta.y);

            onSwipeUp.Invoke();

            Rigidbody rb = GetComponent<Rigidbody>();
            if (rb != null)
            {
                var collectable = GetComponent<CollectableObject>();
                if (collectable != null)
                    collectable.Drop();
                rb.isKinematic = false;
                Debug.Log("dragSpeed: "+ dragSpeed);
                rb.AddForce(transform.forward * Mathf.Clamp(throwPowerMultiplier>0f?(dragSpeed * throwPowerMultiplier):maxThrowVelocity, 0f, maxThrowVelocity), ForceMode.VelocityChange);
            }
        }
        else if (canBeSwipedDown && delta.y < -0.05f)
        {
            Debug.Log(name+" onSwipeDown delta: " + delta.y);

            onSwipeDown.Invoke();
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        //var mainCamera = Camera.main;
        //if (mainCamera != null)
        //{
        //    var manipulator = mainCamera.GetComponent<RigidbodyManipulator>();
        //    if (manipulator != null)
        //    {
        //        var screenClickPosition = eventData.pointerCurrentRaycast.screenPosition;
        //        if (screenClickPosition == Vector2.zero)
        //            screenClickPosition = Input.mousePosition;

        //        Vector3 rayOrigin = eventData.pressEventCamera.ScreenToWorldPoint(new Vector3(screenClickPosition.x, screenClickPosition.y, eventData.pressEventCamera.nearClipPlane));

        //        Vector3 rayDir = (rayOrigin - eventData.pressEventCamera.transform.position).normalized;

        //        manipulator.PerformRaycast(mainCamera.transform.position, rayDir);
                
        //        //manipulator.hitRigidbody = GetComponent<Rigidbody>();
        //        //manipulator.hitPoint = eventData.pointerCurrentRaycast.worldPosition;

        //        manipulator.Toggle();
        //    }
        //}

        onClick.Invoke();
    }
}
