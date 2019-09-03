using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class InventoryContainer : MonoBehaviour
{
    public Transform containerForUsableItems;

    public Transform containerForNonUsableItems;

    public UnityEvent onChildrenChange;

    //private int _previouslySelectedItemIndex = -1;
    //public int selectedItemIndex = -1;

    // Start is called before the first frame update
    void Start()
    {
        //selectedItemIndex = -1;
        OnTransformChildrenChanged();
    }

    public Transform GetContainerTransformForNonUsable()
    {
        return containerForNonUsableItems;
    }

    HashSet<Transform> children = new HashSet<Transform>();

    //Dictionary<Transform, ItemDragHandler> transformDragHandlers = new Dictionary<Transform, ItemDragHandler>();

    Dictionary<ItemDragHandler, UnityAction> handlerActionsSwipeDown = new Dictionary<ItemDragHandler, UnityAction>();

    Dictionary<ItemDragHandler, UnityAction> handlerActionsSwipeUP = new Dictionary<ItemDragHandler, UnityAction>();

    public UnityEvent onHasActiveItem;

    public UnityEvent onHasNoActiveItems;

    private void OnTransformChildrenChanged()
    {
        onChildrenChange.Invoke();

        //Debug.Log(name+ " children.Count: " + children.Count);
        List<Transform> addedChildren = new List<Transform>();
        foreach(Transform t in transform)
        {
            if (!children.Contains(t))
            {
                children.Add(t);
                addedChildren.Add(t);
                Debug.Log("[Inventory] Added " + t.name + ";", t);
            }
        }

        List<Transform> removedChildren = new List<Transform>();
        foreach (Transform t in children)
        {
            if (t.parent != transform)
            {
                
                removedChildren.Add(t);
                var itemDrag = t.GetComponent<ItemDragHandler>();
                if (itemDrag != null)
                {
                    if (handlerActionsSwipeDown.TryGetValue(itemDrag, out var swipeDownAction))
                    {
                        //Debug.Log("RemoveListener onSwipeDown for " + t);
                        itemDrag.onSwipeDown.RemoveListener(swipeDownAction);
                        handlerActionsSwipeDown.Remove(itemDrag);
                    }
                    if (handlerActionsSwipeUP.TryGetValue(itemDrag, out var swipeUpAction))
                    {
                        //Debug.Log("RemoveListener onSwipeUp for " + t);
                        itemDrag.onSwipeUp.RemoveListener(swipeUpAction);
                        handlerActionsSwipeUP.Remove(itemDrag);
                    }
                }
            }
        }

        foreach (var c in removedChildren) {
            children.Remove(c);
        }

        /*var keys = new List<ItemDragHandler>(handlerActionsSwipeDown.Keys);
        foreach (ItemDragHandler registeredDragHandler in keys)
        {
            if (registeredDragHandler.transform.parent != transform)
            {
                registeredDragHandler.onSwipeDown.RemoveListener(handlerActionsSwipeDown[registeredDragHandler]);
                handlerActionsSwipeDown.Remove(registeredDragHandler);
            }
        }

        keys = new List<ItemDragHandler>(handlerActionsSwipeUP.Keys);
        foreach (ItemDragHandler registeredDragHandler in keys)
        {
            if (registeredDragHandler.transform.parent != transform)
            {
                registeredDragHandler.onSwipeUp.RemoveListener(handlerActionsSwipeUP[registeredDragHandler]);
                handlerActionsSwipeUP.Remove(registeredDragHandler);
            }
        }*/
        
        foreach (Transform child in transform)
        {
            //if (!transformDragHandlers.ContainsKey(child))
            //{
            var dragHandler = child.GetComponent<ItemDragHandler>();
            if (dragHandler != null)
            {

                var collectableObject = child.GetComponent<CollectableObject>();

                var itemContainer = containerForUsableItems;

                if (collectableObject != null)
                {
                    if (!collectableObject.useable)
                    {
                        itemContainer = containerForNonUsableItems;
                    }
                }

                if (!handlerActionsSwipeUP.ContainsKey(dragHandler))
                {
                    UnityAction newSwipeUpAction = () =>
                    {
                        //var thisAction = handlerActionsSwipeUP[dragHandler];
                        //dragHandler.onSwipeUp.RemoveListener(thisAction);
                        //handlerActionsSwipeUP.Remove(dragHandler);
                        if (dragHandler.selectNextItemAfterThrow)
                            Invoke("SelectCurrentItem", 0.1f);
                    };
                    handlerActionsSwipeUP.Add(dragHandler, newSwipeUpAction);
                    dragHandler.onSwipeUp.AddListener(newSwipeUpAction);
                }

                if (!handlerActionsSwipeDown.ContainsKey(dragHandler))
                {
                    if (!child.gameObject.activeSelf || !collectableObject.useable ) //|| child.GetSiblingIndex() != 0
                    {
                        //Stash all inactive children into the exhibition box 
                        collectableObject.StashItem(itemContainer);
                    }
                    else
                    {
                        Debug.Log("[Inventory] New SwipeDown action delegate added to " + dragHandler, dragHandler);
                        UnityAction newSwipeDownAction = () =>
                        {
                            if (collectableObject.StashItem(itemContainer))
                            {
                                //var thisAction = handlerActionsSwipeDown[dragHandler];
                                //dragHandler.onSwipeDown.RemoveListener(thisAction);
                                //handlerActionsSwipeDown.Remove(dragHandler);
                            }
                        };
                        handlerActionsSwipeDown.Add(dragHandler, newSwipeDownAction);
                        dragHandler.onSwipeDown.AddListener(newSwipeDownAction);
                    }
                }
                else
                {
                    if (!child.gameObject.activeSelf || !collectableObject.useable)
                    {
                        collectableObject.StashItem(itemContainer);
                    }
                }
                    //transformDragHandlers.Add(child, dragHandler);
            }
            //}
        }

        if (transform.childCount > 0){
            onHasActiveItem.Invoke();
        }
        else{
            onHasNoActiveItems.Invoke();
        }
    }
    
    /*
    void OnItemSwipeDown(ItemDragHandler dragHandler)
    {
        var swipedItem = dragHandler.transform;
        Debug.Log("OnItemSwipeDown: "+swipedItem.name+" "+ swipedItem.GetSiblingIndex());
        selectedItemIndex = swipedItem.GetSiblingIndex();

        //dragHandler.onSwipeUp.AddListener(() => { OnItemSwipeUp(dragHandler); });
        dragHandler.onClick.AddListener(() => { OnItemClick(dragHandler); });
    }*/

    // Update is called once per frame
    void Update()
    {
        /*
        if (exhibitionTransform != null)
        {
            if (_previouslySelectedItemIndex != selectedItemIndex && exhibitionTransform.childCount > selectedItemIndex)
            {
                Debug.Log("Selected item "+ selectedItemIndex+" insted of "+ _previouslySelectedItemIndex);
                
                //Stash all exhibited items:     
                while (exhibitionTransform.childCount > 0)
                {
                    var exhibitedItem = exhibitionTransform.GetChild(0);
                    exhibitedItem.gameObject.SetActive(false);
                    //exhibitedItem.SetParent(transform);
                    //exhibitedItem.SetPositionAndRotation(transform.position, transform.rotation);
                    //exhibitedItem.SetSiblingIndex(_previouslySelectedItemIndex);
                    Debug.Log("Stashed " + exhibitedItem);
                }
                _previouslySelectedItemIndex = selectedItemIndex;
                //Show selected item or nothing:
                if (selectedItemIndex > -1)
                {
                    SelectCurrentItem();
                }
            }
        }
        */
    }

    public Transform AcquireItemFromInventory(string requiredItemName)
    {
        var itemTransform = FindItemInInventory(requiredItemName);
        if (itemTransform != null){
            var collectableComponent = itemTransform.GetComponent<CollectableObject>();

            bool canBeHeldInHands = collectableComponent == null || collectableComponent.useable;

            if (itemTransform.gameObject.activeSelf && canBeHeldInHands)
            {
                collectableComponent.Acquire();
            }
        }
        else{
            Debug.LogWarning("[Inventory] "+requiredItemName +" not found ");
        }
        
        return itemTransform;
    }

    public Transform FindItemInInventory(string requiredItemName)
    {
        foreach(Transform child in containerForUsableItems)
        {
            if (child.name.StartsWith(requiredItemName))
            {
                return child;
            }
        }
        foreach(Transform child in containerForNonUsableItems)
        {
            if (child.name.StartsWith(requiredItemName))
            {
                return child;
            }
        }
        
        return null;
    }

    public void SelectCurrentItem()
    {
        //Debug.Log("SelectCurrentItem");

        foreach(Transform itemTransform in containerForUsableItems)
        {
            var collectableComponent = itemTransform.GetComponent<CollectableObject>();

            bool canBeHeldInHands = collectableComponent == null || collectableComponent.useable;

            if (itemTransform.gameObject.activeSelf && canBeHeldInHands)
            {
                //itemTransform.SetParent(transform);
                //itemTransform.SetPositionAndRotation(transform.position, transform.rotation);
                //itemTransform.gameObject.SetActive(true);
                collectableComponent.Acquire();
                //Debug.Log(itemTransform + " parented to " + transform);
                break;
            }
        }


    }

    public void StashAllItems()
    {
        Debug.Log("[InventoryContainer] StashAllItems in "+name, transform);
        var otherItemsCount = transform.childCount;
        if (otherItemsCount > 0)
        {
            foreach (Transform t in transform)
            {
                var collectable = t.GetComponent<CollectableObject>();
                if (collectable != null)
                {
                    var dragHandler = t.GetComponent<ItemDragHandler>();
                    if (dragHandler == null)
                    {
                        Debug.LogWarning(t.gameObject.name+ " have no ItemDragHandler!");
                    }
                    if (dragHandler.canBeSwipedDown)
                        dragHandler.onSwipeDown.Invoke();
                    else
                        collectable.Drop();
                }
                else
                    t.gameObject.SetActive(false);
            }
        }
    }
}
