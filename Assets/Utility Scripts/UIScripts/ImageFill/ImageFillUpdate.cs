using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class ImageFillUpdate : MonoBehaviour
{

    public float speed = 1f;

    Image image;

    // Use this for initialization
    void Start()
    {
        image = GetComponent<Image>();
    }

    float progress;

    // Update is called once per frame
    void Update()
    {
        image.fillAmount += speed * Time.deltaTime;
    }

    public void AddAmount(float f)
    {
        GetComponent<Image>().fillAmount += f;
    }
}
