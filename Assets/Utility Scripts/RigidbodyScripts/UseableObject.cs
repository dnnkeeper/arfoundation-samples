using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class UseableObject : MonoBehaviour, IUseableObjectHandler
{
    public bool canBeUsed = true;

    public UnityEvent onUsed;

    public bool CanBeUsed()
    {
        return canBeUsed;
    }

    public void Use(Transform inventoryTransformParent)
    {
        onUsed.Invoke();
    }

}
