using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Utility.UI
{

    public class TabbedMenu : MonoBehaviour
    {

        public int selectedBtn;

        int activatedBtn = -1;

        Button[] buttons = new Button[0];

        //public UnityEvent onClick;

        public UnityEvent[] buttonEvents;

        void Awake()
        {
            //Debug.LogWarning("be aware Input.multiTouchEnabled = false");
            //Input.multiTouchEnabled = false;

            buttons = GetComponentsInChildren<Button>();
            //buttonEvents = new UnityEvent[buttons.Length];
            //Debug.Log("buttons " + buttons.Length);

            for (int i = 0; i < buttons.Length; i++)
            {
                int n = i;
                buttons[i].onClick.AddListener(delegate { OnClickEvent(n); });
            }

        }

        void OnClickEvent(int n)
        {
            //Debug.Log("OnClickEvent " + n);
            //onClick.Invoke();
            //selectedBtn = n;
            //if (n < buttons.Length)
            //    buttonEvents[n].Invoke();
            if (n != selectedBtn)
                SelectAndInvoke(n);
        }

        public void InvokeSelected()
        {
            buttonEvents[selectedBtn].Invoke();
        }

        public void SelectAndInvoke(int n)
        {
            Select(n);
            buttonEvents[n].Invoke();
        }

        public void Select(int n)
        {
            if (selectedBtn != n)
            {

                selectedBtn = n;

                Debug.Log("Select " + selectedBtn + " " + useHighlatedSprite);

                for (int i = 0; i < buttons.Length; i++)
                {
                    if (i != n)
                        buttons[i].SendMessage("OnTabSelect", false, SendMessageOptions.DontRequireReceiver);
                    else
                        buttons[i].SendMessage("OnTabSelect", true, SendMessageOptions.DontRequireReceiver);
                }
            }
        }

        public bool useHighlatedSprite = true;

        // Update is called once per frame
        void Update()
        {
            if (selectedBtn >= 0 && buttons.Length > selectedBtn)
            {
                if (activatedBtn != selectedBtn)
                {
                    //Debug.Log("Update button");
                    if (activatedBtn >= 0)
                    {
                        buttons[activatedBtn].image.sprite = buttons[activatedBtn].spriteState.disabledSprite;
                    }

                    activatedBtn = selectedBtn;
                    buttons[selectedBtn].image.sprite = useHighlatedSprite ? buttons[selectedBtn].spriteState.highlightedSprite : buttons[selectedBtn].spriteState.pressedSprite;
                }
            }
        }
    }
}
