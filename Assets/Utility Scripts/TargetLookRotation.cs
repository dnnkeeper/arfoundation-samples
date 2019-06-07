using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Utility.CommonUtils
{
    public class TargetLookRotation : MonoBehaviour
    {
        public bool findMainCamera;

        public Transform target;

        public bool lookAway;

        private void Start()
        {
            if (findMainCamera && target == null)
            {
                target = Camera.main.transform;
            }
        }

        // Update is called once per frame
        void Update()
        {
            transform.rotation = Quaternion.LookRotation((lookAway ? -1f : 1f) * (target.position - transform.position).normalized, target.up);
        }
    }
}