using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class OnRenderEvent : MonoBehaviour
{
    public UnityEvent onReflectionBake;

    public UnityEvent onPostRender;

    public List<Camera> currentCameras;

    private void OnRenderObject()
    {
        var cam = Camera.current;
        //Debug.Log("OnRenderObject " + Camera.current);
        if (!currentCameras.Contains(cam))
            currentCameras.Add(cam);

        if (cam.name == "Reflection Probes Camera")
        {
            //Debug.Log("onReflectionBake");
            onReflectionBake.Invoke();
        }
    }

    public void LateUpdate()
    {
        //Debug.Log("OnPostRender");
        onPostRender.Invoke();
    }
}
