using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class EditorOnlyEvent : MonoBehaviour
{
    public UnityEvent onEditor;
    public UnityEvent onRuntime;

    //public bool DisableInRuntime = true;
    //public bool DisableInEditor = false;

    void OnEnable()
    {
        if (Application.isEditor)
        {
            onEditor.Invoke();
        }
        else
        {
            onRuntime.Invoke();
        }
    }
}