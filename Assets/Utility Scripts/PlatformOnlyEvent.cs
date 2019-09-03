using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PlatformOnlyEvent : MonoBehaviour
{
    public bool inEditor = true;

    public UnityEvent onAndroid;
    public UnityEvent onIOS;

    void OnEnable()
    {
        if (inEditor || !Application.isEditor)
        {

#if UNITY_IOS
            Debug.Log("OnIOS.Invoke");
            onIOS.Invoke();
#endif

#if UNITY_ANDROID
            Debug.Log("onAndroid.Invoke");
            onAndroid.Invoke();
#endif
        }
    }
}