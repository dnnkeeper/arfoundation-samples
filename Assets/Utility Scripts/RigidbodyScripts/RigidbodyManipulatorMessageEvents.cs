using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class RigidbodyManipulatorMessageEvents : MonoBehaviour
{
    public UnityEvent onGrab;
    public UnityEvent onDrop;
    public UnityEvent onTargeted;
    public UnityEvent onLostTarget;
    void OnGrab()
    {
        onGrab.Invoke();
    }

    void OnDropItem()
    {
        onDrop.Invoke();
    }

    void OnTargeted(bool isTargeted)
    {
        if (isTargeted)
        {
            //Debug.Log("OnTargeted " + name);
            onTargeted.Invoke();
        }
        else
        {
            //Debug.Log("OnLostTarget " + name);
            onLostTarget.Invoke();
        }
    }

}
