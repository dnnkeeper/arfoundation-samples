using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Optimization for setting main camera at onEnable event instead of searching it every frame
/// </summary>
[RequireComponent(typeof(Canvas))]
public class CanvasMainCameraSetter : MonoBehaviour
{
    void OnEnable()
    {
        var canvas = GetComponent<Canvas>();
        if (canvas.worldCamera == null)
            canvas.worldCamera = Camera.main;
    }
}
