using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Compass : MonoBehaviour
{
    public Text compassText;

    UnityEngine.Compass compass;

    void Start()
    {
        compass = Input.compass;
        if (!compass.enabled) {
            compass.enabled = true;
            Debug.Log("compass enabled");
        }

    }
    // Update is called once per frame
    void Update()
    {
        var euler = transform.localRotation.eulerAngles;
        transform.localRotation = Quaternion.Slerp(transform.localRotation, Quaternion.Euler(euler.x, euler.y, compass.trueHeading), Time.deltaTime);
        if (compassText != null)
            compassText.text = (Input.compass.trueHeading).ToString("F1");
    }
}
