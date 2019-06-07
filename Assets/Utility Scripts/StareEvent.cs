using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace Utility.CommonUtils
{
    public class StareEvent : MonoBehaviour
    {
        [ReadOnly]
        bool visible;

        public UnityEvent onBecomeVisible;

        public UnityEvent onBecomeInvisible;

        private void OnBecameVisible()
        {
            if (!visible)
            {
                visible = true;

                onBecomeVisible.Invoke();
            }

        }

        private void OnBecameInvisible()
        {
            if (visible)
            {
                visible = false;

                onBecomeInvisible.Invoke();
            }
        }
    }
}