using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnableChildren : MonoBehaviour {

    public float interval = 1f;

	public void EnableChild(float n)
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            if (i != (int)n)
                transform.GetChild(i).gameObject.SetActive(false);
            else
                transform.GetChild(i).gameObject.SetActive(true);
        }
        
    }

    public void EnableChildByName(string childName)
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            var child = transform.GetChild(i);
            if (child.name != childName)
                child.gameObject.SetActive(false);
            else
                child.gameObject.SetActive(true);
        }

    }

    public void ToggleChildByName(string childName)
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            var child = transform.GetChild(i);
            if (child.name == childName)
            {
                child.gameObject.SetActive(!child.gameObject.activeSelf);
            }
        }

    }

    public int selectedN;

    void Awake()
    {
        
    }

    void OnEnable()
    {

    }

    public void SelectNext()
    {
        selectedN += 1;
        if (selectedN >= transform.childCount)
        {
            selectedN = 0;
        }
        EnableChild(selectedN);
    }

    public void SelectPrev()
    {
        selectedN -= 1;
        if (selectedN < 0)
        {
            selectedN = transform.childCount-1;
        }
        EnableChild(selectedN);
    }

    public void EnableChildWithFade(float n)
    {
        selectedN = (int)n;

        foreach (Transform t in transform)
        {
            if (t.GetSiblingIndex() != selectedN) { 
                t.SendMessage("OnDeactivation", SendMessageOptions.DontRequireReceiver);
            }
            else
            {
                t.SendMessage("OnActivation", SendMessageOptions.DontRequireReceiver);
            }
        }
    }

    IEnumerator fadingActivate(CanvasRenderer targetRenderer, float time, bool activate)
    {
        float t = 0f;

        while (t < time)
        {
            t += Time.deltaTime;

            float d = t / time;

            targetRenderer.SetAlpha( Mathf.Clamp01( activate? d : (1f - d) ) );

            yield return null;
        }

        targetRenderer.gameObject.SetActive(activate);

        t = 0f;
    }

    public void DisableAll()
    {
        foreach (Transform child in transform)
        {
            child.gameObject.SetActive(false);
        }
    }

    public void StartEnablingAll (float interval)
    {
        StopAllCoroutines();

        foreach (Animator child in transform.GetComponentsInChildren<Animator>())
        {
            Debug.Log(child+" stop animator");
            child.enabled = false;
        }

        StartCoroutine(enableInCircle(interval, false, false, false));
    }

    public void StartDisablingAll(float interval)
    {
        StopAllCoroutines();
        StartCoroutine(disableRoutine(interval));
    }

    public void StartEnablingInCircle(float interval)
    {
        StopAllCoroutines();

        foreach (Animator child in transform.GetComponentsInChildren<Animator>())
        {
            Debug.Log(child + " stop animator");
            child.enabled = false;
        }

        StartCoroutine(enableInCircle(interval, false));
    }

    public void StartEnablingIterative(float interval)
    {
        StartCoroutine(enableInCircle(interval, true));
    }

    public void StartEnablingIterativeWithMessage(float interval)
    {
        StartCoroutine(enableInCircle(interval, true, true));
    }


    public void StopEnabling()
    {
        StopAllCoroutines();
    }

    IEnumerator disableRoutine(float interval)
    {
        int n = 0;

        float t = 0f;

        while (n < transform.childCount)
        {
            while (t < interval)
            {
                t += Time.deltaTime;
                yield return null;
            }

            t = 0f;

            GameObject child = transform.GetChild(n).gameObject;

            var animator = child.GetComponentInChildren<Animator>();

            if (animator != null)
            {
                Debug.Log("disableRoutine Disappear " + child +" "+n);
                animator.enabled = true;
                animator.Play("Disappear");
            }else
                child.SetActive(false);

            n++;
        }

        Debug.Log("disableRoutine complete " + n);
    }

    IEnumerator enableInCircle(float interval, bool deactivate = false, bool sendMessage = false, bool loop = true)
    {
        int n = 0;

        foreach (Transform child in transform)
        {
            if (child.gameObject.activeSelf)
            {
                if (deactivate)
                    n = Mathf.Max(n, child.GetSiblingIndex()+1);
                else
                    n++;
            }
        }


        //Debug.Log("enableInCircle "+deactivate+" n = "+n);

        GameObject previousChild = (n-1>=0)? transform.GetChild(n-1).gameObject : null;

        float t = (interval - 0.01f);//(n == 0)? (interval - 0.01f) : 0f; //first activation start time 0.01s

        while ( loop || n < transform.childCount)
        {
            
            while (t < interval)
            {
                t += Time.deltaTime;
                yield return null;
            }

            t = 0f;

            if (n >= transform.childCount)
            {
                foreach (Transform c in transform)
                {
                    c.gameObject.SetActive(false);
                }
                n = 0;
            }

            GameObject child = transform.GetChild(n).gameObject;
            
            child.SetActive(true);
            //var animator = child.GetComponent<Animator>();
            //if (animator != null)
            //    animator.enabled = true;

            if (deactivate && previousChild != null)
            {
                if (sendMessage)
                    previousChild.SendMessage("OnScreenDisappeared");
                else
                    previousChild.SetActive(false);
            }

            previousChild = child;
            n++;
        }

        Debug.Log("enableInCircle complete enabling at " + n);
    }
}
