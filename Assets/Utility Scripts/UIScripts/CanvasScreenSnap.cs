using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using Unity.Collections;

namespace Utility.UI
{
    /// <summary>
    /// This component snaps Canvas to the Camera viewport, matching greater size of the canvas with the minimal size of the viewport (envelop mode)
    /// </summary>
    [RequireComponent(typeof(Canvas))]
    public class CanvasScreenSnap : MonoBehaviour
    {
        public Camera cam;

        public UnityEvent onStartSnapping;
        public UnityEvent onStartReturning;
        public UnityEvent onSnapped;
        public UnityEvent onReturned;

        public float nearClipDistance;

        public enum SnapState
        {
            None,
            Snapping,
            Returning
        }

        public SnapState state;

        Transform originalParent;

        //public bool isSnapping, isReturning;

        public void SnapToCurrentCamera()
        {
            if (cam == null)
                cam = Camera.main;
            SnapToCamera(cam);
        }

        public void SnapToCamera(Camera camera)
        {
            if (transform.parent.name != "CanvasLerpParent")
            {
                originalParent = transform.parent;

                var parent = new GameObject("CanvasLerpParent").transform;
                parent.position = originalParent.position;
                parent.rotation = originalParent.rotation;
                transform.SetParent(parent);
            }

            enabled = true;
            if (cam == null)
                cam = Camera.main;
            cam = camera;
            state = SnapState.Snapping;
            onStartSnapping.Invoke();
            //StartCoroutine(snapToScreenRoutine(camera, 1f));
        }

        public void SnapToOrigin()
        {
            var parent = transform.parent;
            if (parent != originalParent && originalParent != null)
            {
                transform.SetParent(originalParent);
                GameObject.Destroy(parent.gameObject);
            }

            enabled = true;
            state = SnapState.Returning;
            onStartReturning.Invoke();
            //StartCoroutine(returnRoutine(1f));
        }

        public void SnapToggle()
        {
            //cam = camera;
            if (state != SnapState.Snapping)
            {
                SnapToCamera(cam);
            }
            else
            {
                SnapToOrigin();
            }
        }

        Vector3 initSize;
        Vector3 initScale;
        Vector3 initLocalPos;
        Quaternion initLocalRot;

        //Transform parent;

        RectTransform rt;

        // Use this for initialization
        void Start()
        {
            if (cam == null)
                cam = Camera.main;

            //canvas = GetComponent<Canvas>();
            rt = transform as RectTransform;
            initSize = rt.sizeDelta * rt.localScale;

            initScale = rt.localScale;
            initLocalPos = rt.localPosition;
            initLocalRot = rt.localRotation;

            Debug.Log("initialScale " + initSize);
        }

        [ReadOnly]
        public float progressTime;

        bool snapped;

        public bool IsSnapped()
        {
            return snapped;
        }

        private void LateUpdate()
        {
            if (state != SnapState.None)
            {
                if (state == SnapState.Snapping)
                {
                    progressTime += Time.deltaTime ;
                }
                else
                {
                    progressTime -= Time.deltaTime;
                }

                if (progressTime <= 0f)
                {
                   
                    onReturned.Invoke();
                    snapped = false;
                    //enabled = false;
                }
                else if (progressTime >= 1f)
                {
                    onSnapped.Invoke();
                    snapped = true;
                    //enabled = false;
                }

                progressTime = Mathf.Clamp01(progressTime);

                nearClipDistance = Mathf.Max(cam.nearClipPlane, nearClipDistance);

                Vector3[] frustumCorners = new Vector3[4];
                Vector3[] worldSpaceCorner = new Vector3[4];
                cam.CalculateFrustumCorners(new Rect(0, 0, 1, 1), nearClipDistance, Camera.MonoOrStereoscopicEye.Mono, frustumCorners);
                for (int i = 0; i < 4; i++)
                {
                    worldSpaceCorner[i] = cam.transform.position + cam.transform.TransformVector(frustumCorners[i]);
                }

                screenSize = new Vector3((worldSpaceCorner[1] - worldSpaceCorner[2]).magnitude, (worldSpaceCorner[0] - worldSpaceCorner[1]).magnitude, 1f);

                //worldSpaceCorner[0]
                Vector3 targetPos = cam.ViewportToWorldPoint(new Vector3(rt.pivot.x, rt.pivot.y, nearClipDistance)) + cam.transform.forward * 0.001f;

                //float screenSizeMin = Mathf.Min(screenSize.x, screenSize.y);

                //float guiSizeMax = Mathf.Max(initSize.x, initSize.y);

                float scale = Mathf.Min(screenSize.x / initSize.x, screenSize.y / initSize.y);
                    //screenSizeMin / guiSizeMax;

                Quaternion targetRot = Quaternion.LookRotation(cam.transform.forward, cam.transform.up);

                    //parent = transform.parent;

                //else
                //{
                //    parent = new GameObject("CanvasLerpParent").transform;
                //    parent.transform.position = transform.position;
                //    parent.transform.rotation = transform.rotation;
                //    transform.parent = parent;
                //}

                if (transform.parent != null)
                {
                    Vector3 targetScale = new Vector3(initScale.x * scale / transform.parent.lossyScale.x, initScale.y / transform.parent.lossyScale.y * scale, 1f);

                    LerpParams(transform.parent.TransformPoint(initLocalPos), targetPos, initScale, targetScale, transform.parent.rotation * initLocalRot, targetRot, progressTime);
                }
            }
        }

        Vector3 screenSize;

        void LerpParams(Vector3 initPos, Vector3 targetPos, Vector3 initScale, Vector3 targetScale, Quaternion initRotation, Quaternion targetRotation, float progress)
        {
            transform.position = Vector3.Lerp(initPos, targetPos, progress * progress);

            transform.rotation = Quaternion.Lerp(initRotation, targetRotation, progress);

            transform.localScale = Vector3.Lerp(initScale, targetScale, progress );
        }
    }
}
