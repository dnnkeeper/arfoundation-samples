using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent (typeof(RectTransform))]
public class SwipeControls : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
	public float sensivity = 1.0f;

	public UnityEventVector3 onSwipe;

	public UnityEventVector3 onFastSwipe;

	public UnityEventFloat onPinchZoom;

	//public UnityEventQuaternion onRotate;

	public Vector2 startPos, prevPos, lastPos;

	RectTransform rectTransform;

	public float fastStep = 10f;

	public float xThreshold = 100f;
	public float yThreshold = 100f;

	void Awake ()
	{
		rectTransform = GetComponent<RectTransform> ();
	}

	public void OnBeginDrag (PointerEventData eventData)
	{
		startPos = eventData.position;
		lastPos = eventData.position;
		prevPos = eventData.position;
	}

	public void OnDrag (PointerEventData eventData)
	{
		if (pinch)
			return;

		lastPos = eventData.position;
		Vector3 delta = (lastPos - prevPos);

		delta.x *= (1920f / Screen.currentResolution.width);
		delta.y *= (1080f / Screen.currentResolution.height);

		prevPos = lastPos;

		//delta.x /= rectTransform.rect.width;
		//delta.y /= rectTransform.rect.height;

		onSwipe.Invoke (delta * sensivity);


        if (Input.touchCount == 2)
        {
            pinch = true;
            // Store both touches.
            Touch touchZero = Input.GetTouch(0);
            Touch touchOne = Input.GetTouch(1);

            // Find the position in the previous frame of each touch.
            Vector2 touchZeroPrevPos = touchZero.position - touchZero.deltaPosition;
            Vector2 touchOnePrevPos = touchOne.position - touchOne.deltaPosition;

            // Find the magnitude of the vector (the distance) between the touches in each frame.
            float prevTouchDeltaMag = (touchZeroPrevPos - touchOnePrevPos).magnitude;
            float touchDeltaMag = (touchZero.position - touchOne.position).magnitude;

            // Find the difference in the distances between each frame.
            float deltaMagnitudeDiff = prevTouchDeltaMag - touchDeltaMag;

            onPinchZoom.Invoke(deltaMagnitudeDiff);

        }
        else
            pinch = false;

    }

	public void OnEndDrag (PointerEventData eventData)
	{
		float deltaX = (startPos.x - eventData.position.x);
		float signX = Mathf.Sign (deltaX);
        
		if (Mathf.Abs (deltaX) > xThreshold) {
			//Debug.Log("FAST SWIPE X INVOKE");
			onFastSwipe.Invoke (new Vector3 (signX * fastStep, 0f, 0f));
		}

		float deltaY = (startPos.y - eventData.position.y);
		float signY = Mathf.Sign (deltaY);
        
		if (Mathf.Abs (deltaY) > xThreshold) {
			//Debug.Log("FAST SWIPE Y INVOKE");
			onFastSwipe.Invoke (new Vector3 (0f, signY * fastStep, 0f));
		}

		//throw new NotImplementedException();
	}

	bool pinch;

}
