using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class FillProgressEvents : MonoBehaviour {

    public UnityEvent onEmpty;
    public UnityEvent onFull;

    Image image;

    // Use this for initialization
    void Start () {
        image = GetComponent<Image>();
    }

    float progress;

	// Update is called once per frame
	void Update () {
		if (image.fillAmount != progress)
        {
            progress = image.fillAmount;
            if (progress == 0f)
            {
                onEmpty.Invoke();
            }
            else if (progress == 1f)
            {
                onFull.Invoke();
            }
        }
	}
}
