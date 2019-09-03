using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class DestroyTimer : MonoBehaviour {

    public UnityEvent onDestroy;

    public float timeToDestroy = 1f;

	// Use this for initialization
	void Start () {
        if (timeToDestroy > 0f)
            Destroy(gameObject, timeToDestroy);
	}

    public void DestroyNow()
    {
        onDestroy.Invoke();
        GameObject.Destroy(gameObject);
    }

    private void OnDestroy()
    {
        onDestroy.Invoke();
    }
}
