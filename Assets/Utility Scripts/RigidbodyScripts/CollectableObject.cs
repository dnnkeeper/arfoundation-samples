using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class CollectableObject : MonoBehaviour, IPointerClickHandler
{
    public bool canBeAcquired;

    //[System.Serializable]
    //public class SocketInfo
    //{
    //    public string socketName = "Inventory";
    //    public Vector3 localPosition = -Vector3.up * 0.05f;
    //    public Quaternion localRotation = Quaternion.Euler(-30f, 0f, 0f);

    //    protected Transform socketTransform;

    //    public Transform GetOrCreateSocketOnCamera(Camera cam, Collider objectCollider = null)
    //    {
    //        if (socketTransform == null)
    //        {
    //            var newSocket = cam.transform.Find(socketName);

    //            if (newSocket == null)
    //            {
    //                newSocket = new GameObject(socketName).transform;
    //                newSocket.SetParent(cam.transform);

    //                float myColliderRadius = 0.1f;

    //                if (objectCollider != null)
    //                {
    //                    myColliderRadius = objectCollider.bounds.extents.magnitude;
    //                    Debug.Log(objectCollider.name + " collider radius: " + myColliderRadius);
    //                }

    //                newSocket.SetPositionAndRotation(cam.transform.position + localPosition + cam.transform.forward * (cam.nearClipPlane + myColliderRadius), cam.transform.rotation * localRotation);
    //            }
    //            socketTransform = newSocket;
    //        }
    //        return socketTransform;
    //    }
    //}

    //public SocketInfo socketInfo;

    public string socketName = "Inventory";

    public string itemName;

    /// <summary>
    /// Deactivates on acquire if false
    /// </summary>
    public bool useable = false;

    public bool stackable = true;

    public UnityEvent onCollected;

    public UnityEvent onDropped;

    public UnityEvent OnGrabbed;

    public UnityEvent OnUsed;

    Transform originalItemParent;

    RigidbodyInterpolation originalInterpolationMode;

    //public UnityEvent onDropped;

    [SerializeField]
    bool _collected;

    public bool IsAcquired()
    {
        return _collected;
    }

    Vector3 startPosition;
    Quaternion startRotation;

    private void Start()
    {
        startPosition = transform.position;
        startRotation = transform.rotation;
    }

    Coroutine returnCoroutine;

    public void ReturnToStartPosition(float timer)
    {
        if (returnCoroutine != null)
        {
            StopCoroutine(returnCoroutine);
        }
        returnCoroutine = StartCoroutine(ReturnToStartPositionRoutine(timer));
    }

    public IEnumerator ReturnToStartPositionRoutine(float delay)
    {
        yield return new WaitForSeconds(delay);

        transform.position = startPosition;
        transform.rotation = startRotation;
        Debug.Log("[CollectableObject] " + name + " returned to start position", transform);
    }

    private void OnEnable()
    {
        if (!string.IsNullOrEmpty(itemName))
            name = itemName;
    }

    private void Reset()
    {
        itemName = name;
    }

    public void setCanBePickedUp(bool b)
    {
        canBeAcquired = b;
    }

    void OnCollected()
    {
        Acquire();           
    }

    public void AcquireNow()
    {
        Acquire(null);
    }

    public bool Acquire()
    {
        return Acquire(null);
    }

    void OnGrab()
    {
        if (returnCoroutine != null)
        {
            StopCoroutine(returnCoroutine);
        }
        OnGrabbed.Invoke();
    }

    Transform _itemTransform;
    Transform itemTransform
    {
        get
        {
            if (_itemTransform == null)
            {
                _itemTransform = transform;
                var rb = GetComponent<Rigidbody>();
                if (rb != null)
                {
                    _itemTransform = rb.transform;
                }
            }
            return _itemTransform;
        }
    }

    Coroutine stashCoroutine;

    public bool StashItem(Transform itemContainer, bool active = true)
    {
        //Transform itemTransform = transform;

        if (itemContainer != null)
        {
            if (stackable)
            {
                foreach (Transform child in itemContainer)
                {
                    if (child.name.StartsWith(itemName))
                    {
                        active = false;
                        var stackContainer = child.Find("StackContainer");
                        if (stackContainer == null)
                        {
                            stackContainer = new GameObject("StackContainer").transform;
                            stackContainer.SetParent(child);
                            stackContainer.SetPositionAndRotation(child.position, child.rotation);
                            Debug.Log("StackContainer for " + name + " has been created in " + child, stackContainer);
                        }
                        //temporarily parent this item to container to invoke parenting event
                        itemTransform.SetParent(itemContainer);
                        itemContainer = stackContainer;
                        break;
                    }
                }
            }

            //itemTransform.gameObject.SetActive(active);
            
            //itemTransform.SetPositionAndRotation(itemContainer.position, itemContainer.rotation);
            if (isActiveAndEnabled)
            {
                stashCoroutine = StartCoroutine( StashToContainerRoutine(0.5f, itemContainer) );
            }
            else
            {
                itemTransform.SetParent(itemContainer);
                itemTransform.SetPositionAndRotation(itemContainer.position, itemContainer.rotation);
            }

            itemTransform.SetAsFirstSibling();

            //ItemDragHandler itemDragHandler = itemTransform.GetComponent<ItemDragHandler>();
            //if (itemDragHandler != null)
            //{
            //    itemDragHandler.enabled = false;
            //}
            Debug.Log("Stashed " + itemTransform.name + " into " + itemContainer.name);
            return true;
        }
        else
        {
            Debug.LogWarning("Cannot stash " + itemTransform.name + " into NULL");
            return false;
        }
    }

    public bool Acquire(Camera cam)
    {
        
        if (!canBeAcquired)
        {
            return false;
        }

        if (IsAcquired())
        {
            Drop();
        }

        if (stashCoroutine != null)
        {
            StopCoroutine(stashCoroutine);
        }

        if (returnCoroutine != null)
        {
            StopCoroutine(returnCoroutine);
        }

        Debug.Log("Acquire "+name, gameObject);

        if (cam == null)
            cam = Camera.main;
       
        var socketTransform = cam.transform.Find(socketName);

        if (socketTransform != null)
        {
            if (useable)
            {
                var otherItemsCount = socketTransform.childCount;
                if (otherItemsCount > 0)
                {
                    foreach (Transform t in socketTransform)
                    {
                        var collectable = t.GetComponent<CollectableObject>();
                        if (collectable != null)
                        {
                            var dragHandler = t.GetComponent<ItemDragHandler>();
                            if (dragHandler == null)
                            {
                                Debug.LogWarning(t.gameObject.name+ " have no ItemDragHandler! Adding component");
                                dragHandler = t.gameObject.AddComponent<ItemDragHandler>();
                            }
                            if (dragHandler.canBeSwipedDown)
                                dragHandler.onSwipeDown.Invoke();
                            else
                                collectable.Drop();
                        }
                        else
                            t.gameObject.SetActive(false);
                    }
                }
            }
            else
            {
                gameObject.SetActive(false);
            }

            originalItemParent = itemTransform.parent;

            itemTransform.parent = socketTransform.transform;

            if (isActiveAndEnabled)
            { 
                StartCoroutine(AttatchToSocketRoutine(0.3f));
            }
            else
            {
                itemTransform.localPosition = Vector3.zero;
                itemTransform.localRotation = Quaternion.identity;
            }

            itemTransform.SendMessage("OnAcquire", SendMessageOptions.DontRequireReceiver);

            //transform.SetPositionAndRotation(socketTransform.transform.position, socketTransform.transform.rotation);
        }
        else
        {
            Debug.LogWarning("No socket "+ socketName + " in "+cam);
            return false;
        }

        
        var rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
            originalInterpolationMode = rb.interpolation;
            rb.interpolation = RigidbodyInterpolation.None;
            Debug.Log("rigidbody set kinematic after collection", rb);
        }

        
        _collected = true;

        onCollected.Invoke();

        return true;
    }

    Vector3 socketLocalPosition = Vector3.zero;
    Quaternion socketLocalRotation = Quaternion.identity;

    IEnumerator AttatchToSocketRoutine(float timer)
    {
        Transform grabHandle = itemTransform.Find("GrabHandle");
        if (grabHandle != null)
        {
            var itemPositionFromHandle = grabHandle.InverseTransformPoint(itemTransform.position);
            var itemRotationFromHandle = Quaternion.Inverse(grabHandle.rotation) * itemTransform.rotation;

            socketLocalRotation = itemRotationFromHandle;
            socketLocalPosition = itemPositionFromHandle;
        }

        float t = 0f;
        while (t <= timer)
        {
            t += Time.deltaTime;

            float progress = Mathf.Clamp01(t / timer);
            itemTransform.localPosition = Vector3.Lerp(itemTransform.localPosition, socketLocalPosition, progress);
            itemTransform.localRotation = Quaternion.Lerp(itemTransform.localRotation, socketLocalRotation, progress);

            yield return null;
        }
        itemTransform.localPosition = socketLocalPosition;
        itemTransform.localRotation = socketLocalRotation;

    }

    IEnumerator StashToContainerRoutine(float timer, Transform containerTransform)
    {
        float t = 0f;
        while (t <= timer)
        {
            t += Time.deltaTime;

            float progress = Mathf.Clamp01(t / timer);
            itemTransform.position = Vector3.Lerp(itemTransform.position, containerTransform.position, progress);
            itemTransform.rotation = Quaternion.Lerp(itemTransform.rotation, containerTransform.rotation, progress);

            yield return null;
        }
        itemTransform.position = containerTransform.position;
        itemTransform.rotation = containerTransform.rotation;
        itemTransform.SetParent(containerTransform);

    }

    public void Drop()
    {
        var socketParent = itemTransform.parent;

        itemTransform.parent = originalItemParent;

        //if (socketParent != null && socketParent.childCount > 0)
        //{
        //    var nextItemInSocket = socketParent.GetChild(0);

        //    if (nextItemInSocket != null)
        //    {
        //        StartCoroutine(this.Delay(0.1f, () => { Debug.Log("nextItemInSocket "+ nextItemInSocket.name); nextItemInSocket.gameObject.SetActive(true); }));
        //    }
        //}

        var rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = false;
            rb.interpolation = originalInterpolationMode;
            //Debug.Log(rb.name+" dropped from socket", rb);
        }

        itemTransform.SendMessage("OnDropItem", SendMessageOptions.DontRequireReceiver);

        if (_collected)
        {
            _collected = false;
            onDropped.Invoke();
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (IsAcquired())
            OnUsed.Invoke();
    }
}
