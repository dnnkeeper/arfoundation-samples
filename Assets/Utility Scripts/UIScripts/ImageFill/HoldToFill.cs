using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;

[RequireComponent(typeof(Image))]
public class HoldToFill : MonoBehaviour, IPointerDownHandler, IPointerUpHandler  {

    Image image;

    Coroutine holdRoutine;

    public float timeToActivate = 2f;

    public UnityEvent onStart;

    public UnityEvent onBreak;

    public UnityEvent onComplete;

    float fillAmount;

    public void OnPointerDown(PointerEventData eventData)
    {
        if (holdRoutine != null)
        {
            //StopCoroutine(holdRoutine);
            return;
        }
        onStart.Invoke();
        holdRoutine = StartCoroutine(OnHold(timeToActivate, image.fillAmount * timeToActivate, progress => {
            fillAmount = progress;
            if (progress >= 1f)
                onComplete.Invoke();
        }));
    }


    public void OnPointerUp(PointerEventData eventData)
    {
        if (holdRoutine != null)
        {
            StopCoroutine(holdRoutine);
            holdRoutine = null;
            onBreak.Invoke();
            fillAmount = 0f;
        }
    }

    IEnumerator OnHold(float holdTime = 1f, float offset = 0f, System.Action<float> onProgress = null)
    {
        float elapsedTime = offset;
        while (elapsedTime <= holdTime)
        {
            yield return null;

            elapsedTime += Time.deltaTime;

            float progress = elapsedTime / holdTime;
            
            if (onProgress != null)
                onProgress.Invoke(progress);
        }
    }

    // Use this for initialization
    void Start () {
        image = GetComponent<Image>();
    }
	
	// Update is called once per frame
	void Update () {

        image.fillAmount = (fillAmount==0f)?Mathf.Lerp(image.fillAmount, 0f, Time.deltaTime) : fillAmount;
    }

}
