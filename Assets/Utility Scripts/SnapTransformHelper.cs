using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Utility.CommonUtils
{

    public class SnapTransformHelper : MonoBehaviour
    {
        public Vector3 rayStart = Vector3.up;

        public Vector3 rayDir = -Vector3.up;

        public Vector3 multipliers = Vector3.one;

        public float maxDistance = 10f;

        public LayerMask collisionMask = 1;

        public UnityEvent onHit;

        public UnityEvent onNotHit;

        bool wasHit;

        Vector3 localRayStart;

        // Start is called before the first frame update
        void Start()
        {
            if (transform.parent != null)
                localRayStart = transform.parent.InverseTransformPoint(transform.position + rayStart);
            else
                localRayStart = transform.position + rayStart;

            Snap();
        }

        // Update is called once per frame
        void Update()
        {
            Snap();
        }

        void Snap()
        {
            RaycastHit hit;

            var r = new Ray(GetRayStartWorldPos(), rayDir);
            if (Physics.Raycast(r, out hit, maxDistance, collisionMask))
            {
                //Debug.DrawLine(r.origin, hit.point, Color.magenta);

                var hitPosition = hit.point;

                hitPosition.x = Mathf.Lerp(transform.position.x, hitPosition.x, multipliers.x);
                hitPosition.y = Mathf.Lerp(transform.position.y, hitPosition.y, multipliers.y);
                hitPosition.z = Mathf.Lerp(transform.position.z, hitPosition.z, multipliers.z);

                transform.position = hitPosition;

                if (!wasHit)
                {
                    wasHit = true;
                    onHit.Invoke();
                }
            }
            else
            {
                if (wasHit)
                {
                    wasHit = false;
                    onNotHit.Invoke();
                }
            }
        }

        Vector3 GetRayStartWorldPos()
        {
            Vector3 rayStartWorldPos = localRayStart;
            if (transform.parent != null)
            {
                rayStartWorldPos = transform.parent.TransformPoint(localRayStart);
            }
            return rayStartWorldPos;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawRay(GetRayStartWorldPos(), rayDir.normalized * maxDistance);
        }
    }
}