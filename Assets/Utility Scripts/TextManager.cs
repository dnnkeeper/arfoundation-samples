using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TextManager : MonoBehaviour
{
    private string text;

    public string Text {
        get {
            var textComponent1 = GetComponent<TextMeshProUGUI>();
            if (textComponent1 != null) {
                return textComponent1.text;
            }
            var textComponent2 = GetComponent<TextMeshPro>();
            if (textComponent2 != null)
            {
                return textComponent2.text;
            }
            var textComponent3 = GetComponent<Text>();
            if (textComponent3 != null)
            {
                return textComponent3.text;
            }
            throw new Exception("Не найдена компоненте с текстом.");
        }
        set {
            var textComponent1 = GetComponent<TextMeshProUGUI>();
            if (textComponent1 != null)
            {
                textComponent1.text = value;
                return;
            }
            var textComponent2 = GetComponent<TextMeshPro>();
            if (textComponent2 != null)
            {
                textComponent2.text = value;
                return;
            }
            var textComponent3 = GetComponent<Text>();
            if (textComponent3 != null)
            {
                textComponent3.text = value;
                return;
            }
            throw new Exception("Не найдена компоненте с текстом.");
        }
    }

    public void SetText(string text) {
        this.Text = text;
    }

    public void SetText(Location location) {
        this.Text = location.ToString();
    }

}
