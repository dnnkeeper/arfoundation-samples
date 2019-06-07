using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TransformUtility : MonoBehaviour
{
    public void Adopt(GameObject target)
    {
        target.transform.SetParent(transform);
    }

    public void DestroyTarget(GameObject target)
    {
        GameObject.Destroy(target);
    }

    public void Destroy(GameObject target)
    {
        Destroy(target);
    }
}
