using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class CollectableObject : MonoBehaviour
{
    public bool canBePickedUp;
   
    public string socketName = "Inventory";

    public bool deactivateOnAcquire = true;

    public UnityEvent onCollected;

    public void setCanBePickedUp(bool b)
    {
        canBePickedUp = b;
    }

    void OnCollected()
    {
        Acquire();
        onCollected.Invoke();
    }

    public void Acquire()
    {
        Acquire(null);
    }

    public void Acquire(Camera cam)
    {
        if (!canBePickedUp)
        {
            return;
        }

        Debug.Log("Acquire "+name, gameObject);

        if (cam == null)
            cam = Camera.main;

        var itemSocket = cam.transform.Find(socketName);
        if (itemSocket != null)
        {
            var otherItemsCount = itemSocket.childCount;
            if (otherItemsCount > 0)
            {
                foreach (Transform t in itemSocket)
                {
                    t.gameObject.SetActive(false);
                }
            }
            transform.parent = itemSocket.transform;
            transform.SetPositionAndRotation(itemSocket.transform.position, itemSocket.transform.rotation);

            
        }
        else
        {
            Debug.LogWarning("No socket "+ itemSocket+" in "+cam);
        }

        if (deactivateOnAcquire)
        {
            gameObject.SetActive(false);
        }
        else
        {
            var rb = GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = true;
            }
        }
    }
}
