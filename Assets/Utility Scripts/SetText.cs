using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

[RequireComponent(typeof(Text))]
public class SetText : MonoBehaviour {

    public string textFormat = "{0}";

    public UnityEventString onTextSet;

    Text textComponent
    {
        get { return GetComponent<Text>(); }
    }
    
    public void SetTextValue(string text)
    {
        textComponent.text = string.Format(textFormat, text);
        onTextSet.Invoke(textComponent.text);
    }

    public void SetTextValue(float f)
    {
        SetTextValue(f.ToString());
    }

    public void SetTextValue(int v)
    {
        SetTextValue(v.ToString());
    }
}
