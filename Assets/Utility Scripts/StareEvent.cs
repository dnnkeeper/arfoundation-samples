using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace Utility.CommonUtils
{
    public class StareEvent : MonoBehaviour
    {
        public bool visible;

        public UnityEvent onBecomeVisible;

        public UnityEvent onBecomeInvisible;

        public float delay = 1.0f;

        Coroutine delayRoutine;

        private void OnBecameVisible()
        {
            if (isActiveAndEnabled)
            {
                if (!visible)
                {
                    if (delayRoutine != null)
                    {
                        StopCoroutine(delayRoutine);
                    }
                    delayRoutine = StartCoroutine(DelayedRoutine(() => {

                        visible = true;

                        onBecomeVisible.Invoke();

                    }));
                }
            }
        }

        IEnumerator DelayedRoutine(System.Action callback)
        {
            float timer = 0f;
            while (timer < delay)
            {
                timer += Time.deltaTime;
                yield return null;
            }
            callback.Invoke();
        }

        private void OnBecameInvisible()
        {
            if (delayRoutine != null)
            {
                StopCoroutine(delayRoutine);
            }

            if (isActiveAndEnabled)
            {
                if (visible)
                {
                    visible = false;

                    onBecomeInvisible.Invoke();
                }
            }
        }
        void Update()
        {

        }
    }
}