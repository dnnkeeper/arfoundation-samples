using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FirstChildOnlyActive : MonoBehaviour
{
    private void OnTransformChildrenChanged()
    {
        foreach (Transform child in transform)
        {
            if (child.GetSiblingIndex() != 0)
            {
                child.gameObject.SetActive(false);
            }
            else
            {
                child.gameObject.SetActive(true);
            }
        }
    }
}
