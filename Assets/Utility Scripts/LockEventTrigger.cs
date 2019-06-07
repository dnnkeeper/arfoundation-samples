using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

using Utility.PhysicsUtils;

public class LockEventTrigger : MonoBehaviour, IPointerClickHandler
{
    public string keyName = "Key";

    public bool locked = true;

    public UnityEvent onUnlocked;

    public UnityEvent onLocked;

    public GameObject keyObject;

    UnityAction onKeyDragAction;

    //public UnityGameObjectEvent onKeyEnter;

    //public UnityGameObjectEvent onKeyExit;

    //public UnityGameObjectEvent onKeyCollisionEnter;

    //public UnityGameObjectEvent onKeyCollisionExit;

    bool _wasLocked;

    private void Start()
    {
        onKeyDragAction = new UnityAction(OnKeyDrag);
        _wasLocked = locked;
    }

    private void Update()
    {
        if (_wasLocked != locked)
        {
            _wasLocked = locked;
            if (locked)
            {
                Debug.Log("LOCK");
                RigidbodyDragHandler rbHandler = keyObject.GetComponent<RigidbodyDragHandler>();
                if (rbHandler != null)
                {
                    rbHandler.onBeginDrag.RemoveListener(onKeyDragAction);
                }
                
                onLocked.Invoke();
            }
            else
            {
                Debug.Log("UNLOCK");
                RigidbodyDragHandler rbHandler = keyObject.GetComponent<RigidbodyDragHandler>();
                if (rbHandler != null)
                {
                    rbHandler.onBeginDrag.AddListener(onKeyDragAction);
                }

                onUnlocked.Invoke();
            }
        }
    }

    public void OnKeyDrag()
    {
        Debug.Log("OnKeyDrag");
        if (!locked)
        {
            locked = true;
        }
    }

    public void OnTriggerEnter(Collider other)
    {
        if (string.IsNullOrEmpty(keyName) || other.gameObject.name == keyName)
        {
            Debug.Log("KEY IN");
            locked = false;
            keyObject = other.gameObject;

            Rigidbody rb = other.GetComponent<Rigidbody>();
            if(rb != null)
                rb.isKinematic = true;
            other.isTrigger = true;
            other.gameObject.transform.parent = transform;
            onUnlocked.Invoke();
            //onKeyEnter.Invoke(other.gameObject);
        }
    }

    public void OnTriggerExit(Collider other)
    {
        if (string.IsNullOrEmpty(keyName) || other.gameObject.name == keyName)
        {
            Debug.Log("KEY OUT");
            locked = true;
            onLocked.Invoke();
            //onKeyExit.Invoke(other.gameObject);
        }
    }

    public void OnCollisionEnter(Collision collision)
    {
        if (string.IsNullOrEmpty(keyName) || collision.gameObject.name == keyName)
        {
            locked = false;
            keyObject = collision.gameObject;
            collision.rigidbody.isKinematic = true;
            collision.collider.isTrigger = true;
            collision.gameObject.transform.parent = transform;
            onUnlocked.Invoke();
            //onKeyCollisionEnter.Invoke(collision.rigidbody.gameObject);
        }
    }

    public void OnCollisionExit(Collision collision)
    {
        if (string.IsNullOrEmpty(keyName) || collision.gameObject.name == keyName)
        {
            locked = true;
            onLocked.Invoke();
            //onKeyCollisionExit.Invoke(collision.rigidbody.gameObject);
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log("LockEventTrigger OnPointerClick");

        var cam = eventData.pressEventCamera;

        if (cam == null)
            cam = Camera.main;

        var itemSocket = cam.transform.Find("ItemSocket");
        if (itemSocket != null)
        {
            var key = itemSocket.Find(keyName);
            if (key != null)
            {
                key.transform.parent = transform;
                var rb = key.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.isKinematic = true;
                }
                var collider = key.GetComponent<Collider>();
                if (collider != null)
                {
                    collider.isTrigger = true;
                }
                onUnlocked.Invoke();
            }
        }
    }
}
