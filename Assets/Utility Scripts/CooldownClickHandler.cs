using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class CooldownClickHandler : MonoBehaviour, IPointerClickHandler
{
    public float cooldownTime = 0.5f;

    float timer;

    public UnityEvent onClick;

    public Animator animator;

    public string animatorParameterName = "Cooldown";

    public void OnPointerClick(PointerEventData eventData)
    {
        if (timer <= 0f)
        {
            timer = cooldownTime;

            onClick.Invoke();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (timer > 0f)
        {
            timer = timer - Time.deltaTime;
            if (animator != null)
            {
                animator.SetFloat(animatorParameterName, timer / cooldownTime );
            }
        }
    }
}
