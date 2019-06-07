using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Utility.CommonUtils
{
    public class Cloner : MonoBehaviour
    {

        public void Clone()
        {
            GameObject clone = GameObject.Instantiate(gameObject, transform.position, transform.rotation, transform.parent); ;
            clone.SetActive(true);
        }
    }
}
