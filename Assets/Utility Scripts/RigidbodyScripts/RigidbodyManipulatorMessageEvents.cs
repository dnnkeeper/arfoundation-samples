using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class RigidbodyManipulatorMessageEvents : MonoBehaviour
{
    public UnityEvent onGrab;
    public UnityEvent onDrop;

    void OnGrab()
    {
        onGrab.Invoke();
    }

    void OnDrop()
    {
        onDrop.Invoke();
    }

}
