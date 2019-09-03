using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(TextMeshProUGUI))]
public class TextMeshProUGUIManager : MonoBehaviour
{
    private TextMeshProUGUI TextMeshProUGUI;
    public string Mask;
    public string Text;
    // Start is called before the first frame update
    public void Awake()
    {
        TextMeshProUGUI = GetComponent<TextMeshProUGUI>();
        Mask = TextMeshProUGUI.text;
    }

    public void SetTextWithMask(string text)
    {
        TextMeshProUGUI.text = string.Format(Mask, text);
    }

}
