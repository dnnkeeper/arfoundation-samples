using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public struct UnityEventExposed
{
    public string name;
    public UnityEvent action;
}

namespace Utility.CommonUtils
{
    public class CustomEventsListener : MonoBehaviour
    {
        public UnityEventExposed[] customEvents;

        public void invokeCustomEventN(int n)
        {
            if (n >= 0 && customEvents.Length > n)
            {
                var customEvent = customEvents[n];

                if (customEvent.action != null)
                {
                    Debug.Log(gameObject.name + " Invoke custom event " + customEvent.name);

                    customEvent.action.Invoke();
                }
            }
        }

        public void invokeCustomEvent(string sName)
        {
            foreach (var customEvent in customEvents)
            {
                if (customEvent.name.Equals(sName))
                {
                    Debug.Log(gameObject.name + " Invoke custom event " + customEvent.name);

                    customEvent.action.Invoke();

                    break;
                }
            }
        }
    }
}