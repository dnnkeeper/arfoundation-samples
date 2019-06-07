using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeactivateOnDisable : MonoBehaviour {

	void OnDisable()
    {
        //Debug.Log("OnDisable");
        gameObject.SetActive(false);
    }
}
