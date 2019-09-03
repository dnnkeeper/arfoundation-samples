using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class TransposeLocalTransform : MonoBehaviour
{
    public Vector3 localPosition = Vector3.zero;

    public Vector3 localScale = Vector3.one;

    Transform _otherParentOld;
    public Transform otherParent;

    void Reset()
    {
        localPosition = Vector3.zero;
        localScale = Vector3.one;
    }

    // Update is called once per frame
    void LateUpdate()
    {
        if (otherParent != null)
        {
            if (_otherParentOld != otherParent)
            {
                _otherParentOld = otherParent;
                localPosition = otherParent.InverseTransformPoint(transform.position);
                localScale = otherParent.InverseTransformVector(transform.lossyScale);
            }

//#if UNITY_EDITOR
//            string inspectorWindowTypeName = "UnityEditor.InspectorWindow";
//            string sceneWindowTypeName = "UnityEditor.SceneView";
//            string editorFocusedWindowTypeName = UnityEditor.EditorWindow.focusedWindow.GetType().ToString();

//            if (UnityEditor.Selection.activeGameObject == gameObject && (editorFocusedWindowTypeName==inspectorWindowTypeName || editorFocusedWindowTypeName==sceneWindowTypeName) )
//            {
//                localPosition = otherParent.InverseTransformPoint(transform.position);
//                localScale = otherParent.InverseTransformVector(transform.lossyScale);
//            }
//#endif

            Vector3 worldPosition = otherParent.TransformPoint(localPosition);
            transform.localPosition = transform.parent.InverseTransformPoint(worldPosition);

            Vector3 worldScale = otherParent.TransformVector(localScale);
            transform.localScale = transform.parent.InverseTransformVector(worldScale);
        }
    }
}
