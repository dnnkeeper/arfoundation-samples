using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChildrenInfoUI : MonoBehaviour
{
    public GameObject InfoUIPrefab;

    HashSet<Transform> children = new HashSet<Transform>();

    /// <summary>
    /// Start is called on the frame when a script is enabled just before
    /// any of the Update methods is called the first time.
    /// </summary>
    void Start()
    {
        OnTransformChildrenChanged();
    }

    private void OnTransformChildrenChanged()
    {
        List<Transform> addedChildren = new List<Transform>();
        foreach (Transform t in transform)
        {
            if (!children.Contains(t))
            {
                children.Add(t);
                addedChildren.Add(t);
            }
        }

        List<Transform> removedChildren = new List<Transform>();
        foreach (Transform t in children)
        {
            if (t.parent != transform)
            {
                removedChildren.Add(t);
            }
        }

        foreach (var c in removedChildren)
        {
            children.Remove(c);
        }

        foreach (Transform removedItem in removedChildren)
        {
            var infoUI = removedItem.Find(InfoUIPrefab.name);
            if (infoUI != null)
            {
                GameObject.Destroy(infoUI.gameObject);
            }
        }

        foreach (Transform item in addedChildren)
        {
            var infoUI = item.Find(InfoUIPrefab.name);
            if (infoUI == null)
            {
                infoUI = GameObject.Instantiate(InfoUIPrefab, item).transform;
                infoUI.name = InfoUIPrefab.name;
            }

            var collectable = item.GetComponent<CollectableObject>();
            if (collectable != null)
            {
                if (collectable.stackable)
                {
                    var stackContainer = item.Find("StackContainer");
                    if (stackContainer != null)
                    {
                        var stackCount = 1 + stackContainer.childCount;
                        var CountInfoText = infoUI.Find("CountInfoText");
                        if (CountInfoText != null)
                            CountInfoText.GetComponent<Text>().text = stackCount.ToString();
                    }
                }
            }

            var NameInfoText = infoUI.Find("NameInfoText");
            if (NameInfoText != null)
                NameInfoText.GetComponent<Text>().text = collectable != null? collectable.itemName : item.name;

            
        }

    }
}
