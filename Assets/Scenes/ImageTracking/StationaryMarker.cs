using UnityEngine;
using UnityEngine.XR.ARSubsystems;

#if UNITY_EDITOR

using UnityEditor;
using System.IO;

#endif

[ExecuteInEditMode, SelectionBaseAttribute]
public class StationaryMarker : MonoBehaviour
{
    public XRReferenceImageLibrary imageLibrary;

    private int _imageIndex;
    public int imageIndex;

#if UNITY_EDITOR

    [ContextMenu("UpdateImageByIndex")]
    public void UpdateImage()
    {
        _imageMaterial = null;
        UpdateImage(imageIndex);
    }

    private Transform imageGameObject;

    private Material _imageMaterial;

    private Material imageMaterial
    {
        get
        {
            if (_imageMaterial == null)
            {
                imageGameObject = transform.Find("MarkerRenderer");
                if (imageGameObject != null)
                {
                    var renderer = imageGameObject.GetComponent<Renderer>();
#if UNITY_EDITOR
                    _imageMaterial = AssetDatabase.LoadAssetAtPath<Material>(Path.GetDirectoryName(AssetDatabase.GetAssetPath(imageLibrary)) + "/" + name + ".mat");
#else
                    _imageMaterial = renderer.material;
#endif
                }

                if (_imageMaterial == null)
                {
                    _imageMaterial = new Material(Shader.Find("Unlit/Texture"));
#if UNITY_EDITOR
                    Debug.Log("New material for marker " + name);
                    AssetDatabase.CreateAsset(_imageMaterial, Path.GetDirectoryName(AssetDatabase.GetAssetPath(imageLibrary)) + "/" + name + ".mat");
#endif
                }
            }
            return _imageMaterial;
        }
    }

    public void UpdateImage(int idx)
    {
        imageGameObject = transform.Find("MarkerRenderer");
        if (imageGameObject == null)
        {
            var newGameObject = GameObject.CreatePrimitive(PrimitiveType.Quad);
            newGameObject.name = "MarkerRenderer";
            imageGameObject = newGameObject.transform;
        }

        imageGameObject.parent = transform;
        imageGameObject.localPosition = Vector3.zero;
        imageGameObject.localRotation = Quaternion.Euler(90f, 0f, 0f);
        //imageGameObject.localScale = Vector3.one * 0.3f;

        if (imageLibrary != null && idx >= 0 && imageLibrary.count > idx)
        {
            var imgInfo = imageLibrary[idx];
            if (imgInfo.texture != null)
            {
                imageMaterial.mainTexture = imgInfo.texture;
                imageGameObject.GetComponent<Renderer>().material = imageMaterial;
                if (imgInfo.width != 0f)
                {
                    //imageGameObject.localScale = new Vector3(imgInfo.width, imgInfo.width * imgInfo.texture.height / imgInfo.texture.width, 1f);
                    transform.localScale = new Vector3(imgInfo.width, 1f, imgInfo.width * imgInfo.texture.height / imgInfo.texture.width);
                }
                Debug.Log("imgInfo.width " + imgInfo.width);
            }
            else
            {
                Debug.LogWarning("imageDatabase[idx].Texture == null");
            }
        }
        else
        {
            imageMaterial.mainTexture = null;
            imageGameObject.GetComponent<Renderer>().material = imageMaterial;
        }
    }

    private void Update()
    {
        if (_imageIndex != imageIndex)
        {
            _imageIndex = imageIndex;
            UpdateImage(_imageIndex);
        }

        imageGameObject = transform.Find("MarkerRenderer");
        if (imageGameObject != null && imageGameObject.transform.localPosition != Vector3.zero)
        {
            var localPos = imageGameObject.localPosition;
            imageGameObject.localPosition = Vector3.zero;
            transform.position = imageGameObject.TransformPoint(localPos);
        }
    }

#endif
}