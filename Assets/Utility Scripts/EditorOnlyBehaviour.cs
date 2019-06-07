using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EditorOnlyBehaviour : MonoBehaviour
{
    public bool DisableInRuntime = true;
    public bool DisableInEditor = false;

    // Use this for initialization
    void Start()
    {
        if (Application.isEditor)
        {   
            if (DisableInEditor && isActiveAndEnabled)
            {
                Debug.Log(gameObject.name + " <color=grey>disabled</color> (DisableInEditor)");
                gameObject.SetActive(false);
            }
            else if (!isActiveAndEnabled)
            {
                Debug.Log(gameObject.name + " <color=white>enabled</color> (EditorOnly)");
                gameObject.SetActive(true);
            }
        }
        else
        {
            if (DisableInRuntime && isActiveAndEnabled)
            {
                Debug.Log(gameObject.name + " <color=grey>disabled</color> (DisableInRuntime)");
                gameObject.SetActive(false);
            }            
        }
    }
}