using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraOnRenderEvents : MonoBehaviour
{
    public event Action onPreRender;
    private void OnPreRender()
    {
        if (onPreRender != null)
        {
            onPreRender.Invoke();
        }
    }
}
