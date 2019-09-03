using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MonoBehaviourExtension
{

    public static IEnumerator Delay(this MonoBehaviour mb, float delay, System.Action action)
    {
        yield return new WaitForSeconds(delay);
        action.Invoke();
    }

    public static void ReplaceLayerRecursive(this GameObject mb, int layerToReplace, int newLayer)
    {
        if (mb.layer == layerToReplace)
            mb.layer = newLayer;

        foreach (Transform t in mb.transform)
        {
            ReplaceLayerRecursive(t.gameObject, layerToReplace, newLayer);
        }
    }

    public static bool IsVisibleFrom(this Renderer renderer, Camera camera)
    {
        Plane[] planes = GeometryUtility.CalculateFrustumPlanes(camera);
        return GeometryUtility.TestPlanesAABB(planes, renderer.bounds);
    }
}
