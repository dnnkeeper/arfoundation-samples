using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class FadeInOnEnable : MonoBehaviour {

    public float duration = 1f;

    public float durationStay = 1f;

    public float durationOut = 1f;
    
    //public float currentAlpha;

    CanvasRenderer _rend;
    CanvasRenderer rend {
        get {
            if (_rend == null)
            {
                _rend = GetComponent<Image>().canvasRenderer;
            }
            return _rend;
        }
    }

    void OnEnable()
    {
        if (enabled)
        {
            if (duration > 0f)
            {
                rend.SetAlpha(0f);
                GetComponent<Image>().CrossFadeAlpha(1f, duration, false);
            }
            else
            {
                rend.SetAlpha(1f);
            }
        }
    }

    /*void Update()
    {
        currentAlpha = rend.GetAlpha();
    }*/

    public void FadeInAndOut()
    {
        gameObject.SetActive(true);
        //rend.SetAlpha(1f);
        Invoke("FadeOutAndDisable", duration+durationStay);
    }
    /*
    public void EnableFadeOutAndDisable()
    {
        gameObject.SetActive(true);
        enabled = false;
        rend.SetAlpha(1f);
        Invoke("FadeOutAndDisable", duration + durationStay);
    }*/

    public void SetAlpha(float f)
    {
        rend.SetAlpha(f);
    }

    public void FadeOut()
    {
        rend.SetAlpha(1f);
        GetComponent<Image>().CrossFadeAlpha(0f, durationOut, false);
    }

    public void FadeOutAndDisable()
    {
        rend.SetAlpha(1f);
        GetComponent<Image>().CrossFadeAlpha(0f, durationOut, false);
        Invoke("Disable", durationOut);
    }

    void Disable()
    {
        gameObject.SetActive(false);
        //enabled = true;
    }
}
