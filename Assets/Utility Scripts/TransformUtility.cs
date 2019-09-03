using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TransformUtility : MonoBehaviour
{
    public void Toggle()
    {
        gameObject.SetActive(!gameObject.activeSelf);
    }
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
        GameObject.Destroy(target);
    }

    public void DestroySelf()
    {
        GameObject.Destroy(gameObject);
    }
    public void SetParentNull()
    {
        transform.SetParent(null);
    }

    public void SetParentToTargetParentPreservingPos(Transform target)
    {
        SetParentPreservingPos(target.parent);
    }

    public void SetParentPreservingPos(Transform target)
    {
        var pos = transform.position;

        var rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            pos = rb.position;
        }
        
        transform.SetParent(target, true);

        transform.position = pos;

        if (rb != null)
        {
            rb.position = pos;
        }
    }

    public void SetLocalScale(float scale)
    {
        transform.localScale = Vector3.one * scale;
    }
}
