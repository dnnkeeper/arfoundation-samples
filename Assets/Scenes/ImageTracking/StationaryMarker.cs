using UnityEngine;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.SceneManagement;
using System;
#if UNITY_EDITOR

using UnityEditor;
using System.IO;

#endif

[ExecuteInEditMode, SelectionBaseAttribute]
public class StationaryMarker : MonoBehaviour
{
    public XRReferenceImageLibrary imageLibrary;

    [HideInInspector, SerializeField]
    private int _imageIndex;
    public int imageIndex;

    public event Action onMarkerEnabled;

    private void OnEnable()
    {
        if (onMarkerEnabled != null)
        {
            Debug.Log("OnMarkerEnabled "+name, transform);
            onMarkerEnabled.Invoke();
        }
    }

#if UNITY_EDITOR

    [ContextMenu("UpdateImageByIndex")]
    public void UpdateImage()
    {
        //_imageMaterial = null;
        UpdateImage(imageIndex);
    }

    private Transform imageGameObject;

    //private Material _imageMaterial;

    private Material imageMaterial
    {
        get
        {
            Material _imageMaterial = null;

#if UNITY_EDITOR
            if (imageLibrary[imageIndex] != null && imageLibrary[imageIndex].texture != null)
            {
                _imageMaterial = GetOrCreateNewMaterialAsset(imageLibrary[imageIndex].texture.name);
            }
#else
            imageGameObject = transform.Find("MarkerRenderer");
            if (imageGameObject != null)
            {
                var renderer = imageGameObject.GetComponent<Renderer>();

                _imageMaterial = renderer.material;
            }
            if (_imageMaterial == null)
            {
                _imageMaterial = new Material(Shader.Find("Unlit/Texture"));

                _imageMaterial.name = name;
            }
#endif
            return _imageMaterial;
        }
    }

    Material GetOrCreateNewMaterialAsset(string MaterialName)
    {
#if UNITY_EDITOR

        Material _imageMaterial = AssetDatabase.LoadAssetAtPath<Material>(Path.GetDirectoryName(AssetDatabase.GetAssetPath(imageLibrary)) + "/" + MaterialName + ".mat");

        if (_imageMaterial == null)
        {
            _imageMaterial = new Material(Shader.Find("Unlit/Texture"));

            _imageMaterial.name = name;

            Debug.Log("New material for marker " + name);
            string materialName = imageIndex.ToString();

            AssetDatabase.CreateAsset(_imageMaterial, Path.GetDirectoryName(AssetDatabase.GetAssetPath(imageLibrary)) + "/" + _imageMaterial.name + ".mat");
        }
        return _imageMaterial;
#endif
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
                var material = imageMaterial;
                material.mainTexture = imgInfo.texture;
                imageGameObject.GetComponent<Renderer>().material = material;
                if (imgInfo.width != 0f)
                {
                    //imageGameObject.localScale = new Vector3(imgInfo.width, imgInfo.width * imgInfo.texture.height / imgInfo.texture.width, 1f);
                    transform.localScale = new Vector3(imgInfo.width, imgInfo.width, imgInfo.width * imgInfo.texture.height / imgInfo.texture.width);
                }
                Debug.Log("imgInfo.width " + imgInfo.width+" "+ imgInfo.texture.name);
            }
            else
            {
                Debug.LogWarning("imageDatabase[idx].Texture == null");
            }
        }
        else
        {
            var material = imageMaterial;
            material.mainTexture = null;
            imageGameObject.GetComponent<Renderer>().material = material;
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape)){
            if (SceneManager.GetActiveScene().buildIndex != 0)
                SceneManager.LoadScene(0);
            else
                Application.Quit();
        }

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