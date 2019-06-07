using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RectTransformSetter : MonoBehaviour
{
    RectTransform rt;

    // Start is called before the first frame update
    void Start()
    {
        rt = GetComponent<RectTransform>();
        
    }
    public void SetAnchorXMin(float anchorMin)
    {
        rt.anchorMin = new Vector2(anchorMin, rt.anchorMin.y);
    }

    public void SetAnchorXMax(float anchorMax)
    {
        rt.anchorMax = new Vector2(anchorMax, rt.anchorMax.y);
    }

    public void SetAnchorYMin(float anchorMin)
    {
        rt.anchorMin = new Vector2(rt.anchorMin.x, anchorMin);
    }

    public void SetAnchorYMax(float anchorMax)
    {
        rt.anchorMax = new Vector2(rt.anchorMax.x, anchorMax);
    }

    public void SetPivotX(float X)
    {
        rt.pivot = new Vector2(X, rt.pivot.y);
    }

    public void SetPivotY(float Y)
    {
        rt.pivot = new Vector2(rt.pivot.x, Y);
    }

    public void ResetPos()
    {
        rt.anchoredPosition = Vector2.zero;
    }
}
