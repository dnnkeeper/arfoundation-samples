using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

[RequireComponent(typeof(RawImage))]
public class RenderCameraToRawImage : MonoBehaviour {

    public Color origColor;
    RenderTexture targetRenderTexture;

    public Camera sourceCamera;

    Vector2 targetPixelRectSize;

    //Resolution res;

    RawImage _targetRawImage;
    RawImage targetRawImage{
        get
        { 
            if (_targetRawImage == null)
                _targetRawImage = GetComponent<RawImage>();
            return _targetRawImage;
        }
    }

    // Use this for initialization
    void Start()
    {
        //res = Screen.currentResolution;
        
    }

    Texture originalTexture;

    // Update is called once per frame
    void Update()
    {
        if (sourceCamera != null && targetRawImage.rectTransform.rect.size * transform.lossyScale != targetPixelRectSize)
        {
            //Debug.Log("pixelRect changed. Create new renderTexture!");

            targetPixelRectSize = targetRawImage.rectTransform.rect.size * transform.lossyScale;

            CreateRenderTexture();

            sourceCamera.targetTexture = targetRenderTexture;

            targetRawImage.texture = targetRenderTexture;

            //res = Screen.currentResolution;
        }
    }

    public FilterMode filterMode = FilterMode.Bilinear;

    void CreateRenderTexture()
    {
        if (targetRenderTexture != null)
        {
            targetRenderTexture.Release();
        }

        var sizeDelta = targetPixelRectSize;//targetRawImage.rectTransform.rect.size;
        if (sizeDelta.x > 0 && sizeDelta.y > 0)
        {
            targetRenderTexture = new RenderTexture((int)sizeDelta.x, (int)sizeDelta.y, 24, RenderTextureFormat.Default);
            targetRenderTexture.filterMode = filterMode;
            targetRenderTexture.useMipMap = false;
            //targetRenderTexture.anisoLevel = 0;
            targetRenderTexture.name = name + "_RenderTargetTexture";
            targetRenderTexture.Create();
            Debug.Log("[RenderToRawImage] Created new Render Texture: "+sizeDelta+" for "+name, transform);
        }
    }

    void OnEnable()
    {
        targetPixelRectSize = targetRawImage.rectTransform.rect.size * transform.lossyScale;

        if (targetRenderTexture == null)
        {
            CreateRenderTexture();
        }

        sourceCamera.targetTexture = targetRenderTexture;
        
        originalTexture = targetRawImage.texture;
        origColor = targetRawImage.color;

        targetRawImage.texture = targetRenderTexture;
        targetRawImage.color = Color.white;

        sourceCamera.enabled = true;
    }

    void OnDisable()
    {
        if (targetRenderTexture != null)
        {
            if (sourceCamera != null)
                sourceCamera.targetTexture = null;
            if (targetRawImage != null)
            {
                targetRawImage.texture = originalTexture;
                targetRawImage.color = origColor;
            }
            targetRenderTexture.Release();
			Destroy (targetRenderTexture);
            targetRenderTexture = null;
        }

        if (sourceCamera != null)
            sourceCamera.enabled = false;

		//GC.Collect ();
    }

}
