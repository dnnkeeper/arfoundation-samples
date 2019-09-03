using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public interface IUseableObjectHandler
{
    bool CanBeUsed();
    void Use(Transform inventoryTransformParent);
}

public class ItemSlot : MonoBehaviour, IUseableObjectHandler, IPointerClickHandler
{
    public string inventoryContainerName = "Inventory";

    public string requiredItemName = "Key";

    public int requiredItemsCount = 1;

    public bool activated;

    int usedItems;

    public UnityEventInt onUsedItem;

    public UnityEvent onActivated;

    public bool CanBeUsed()
    {
        return !activated;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        var cam = (eventData.pressEventCamera??eventData.enterEventCamera)??Camera.main;
        Use(cam.transform);
    }

    public void Use(Transform playerTransform)
    {
        if (activated)
        {
            Debug.Log(name+" already activated");
            return;
        }

        var inventoryTransform = playerTransform.Find(inventoryContainerName);

        if (inventoryTransform != null)
        {
            InventoryContainer inventoryComponent = inventoryTransform.GetComponent<InventoryContainer>();
            if (inventoryComponent != null)
            {
                inventoryTransform = inventoryComponent.GetContainerTransformForNonUsable();
            }

            var foundItems = new List<Transform>();
            int count = FindItemsInInventory(inventoryTransform, foundItems);
            
            usedItems = 0;

            for (int i = 0; i < count; i++)
            {
                var item = foundItems[i];
                var stack = item.Find("StackContainer");
                if (stack != null)
                {
                    var foundStackedItems = new List<Transform>();
                    int stackCount = FindItemsInInventory(stack, foundStackedItems);
                    Debug.Log(requiredItemName+ " Stacked items found: " + stackCount);
                    foundItems.AddRange(foundStackedItems);
                }
            }

            Debug.Log("Found " + foundItems.Count + " items with name " + requiredItemName + " in " + inventoryTransform, inventoryTransform);

            foreach (var item in foundItems)
            {
                var consumedItem = ConsumeItem(item);
                
                if (usedItems >= requiredItemsCount)
                {
                    Debug.Log(name + " activated! ");
                    
                    activated = true;
                    onActivated.Invoke();

                    consumedItem.gameObject.SetActive(true);

                    // if (isActiveAndEnabled){
                    //     StartCoroutine(AtractToSlotRoutine(0.3f, item));
                    // }
                    // else{
                        consumedItem.SetPositionAndRotation(transform.position, transform.rotation);
                    //}

                    break;
                }
                else{
                    consumedItem.SetPositionAndRotation(transform.position, transform.rotation);
                }
            }
            //Debug.Log("deletedItems " + usedItems + " from " + inventoryContainerName);
        }
        else
        {
            Debug.LogWarning("Player has no " + inventoryContainerName + " in camera children");
        }
    }

    IEnumerator AtractToSlotRoutine(float timer, Transform objectToAttract)
    {
        float t = 0f;
        while (t <= timer)
        {
            t += Time.deltaTime;

            float progress = Mathf.Clamp01(t / timer);
            objectToAttract.position = Vector3.Lerp(objectToAttract.position, transform.position, progress);
            objectToAttract.rotation = Quaternion.Lerp(objectToAttract.rotation, transform.rotation, progress);

            yield return null;
        }
        transform.position = transform.position;
        transform.rotation = transform.rotation;

    }

    public Transform ConsumeItem(Transform item)
    {
        var stack = item.Find("StackContainer");
        if (stack != null)
        {
            if (stack.childCount > 0)
            {
                item = stack.GetChild(0);
            }
        }

        usedItems++;

        //GameObject.Destroy(keyTransform.gameObject);
        item.parent = transform;

       
        Debug.Log("Consumed " + item.gameObject, this);
        onUsedItem.Invoke(usedItems);
        return item;
    }

    private void OnValidate()
    {
        requiredItemsCount = Mathf.Max(1, Mathf.Abs(requiredItemsCount));
    }

    public int FindItemsInInventory(Transform inventoryTransform, List<Transform> items)
    {
        int count = 0;
        
        foreach(Transform child in inventoryTransform)
        {
            if (child.name.StartsWith(requiredItemName))
            {
                items.Add(child);
                count++;
            }
        }
        return count;
    }

}
