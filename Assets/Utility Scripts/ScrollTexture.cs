using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScrollTexture : MonoBehaviour
{
    public Material selectedMaterial;

    public string textureName = "_MainTex";

    public Vector2 speed = new Vector2(1, 0);

    Vector2 originalOffset;

    private void Start()
    {
        if (selectedMaterial == null)
        {
            selectedMaterial = GetComponent<Renderer>().material;
            originalOffset = selectedMaterial.GetTextureOffset(textureName);
        }
    }
    void Update()
    {
        if (selectedMaterial != null)
        {
            selectedMaterial.SetTextureOffset(textureName, originalOffset + speed * Time.time );
        }
    }
}
