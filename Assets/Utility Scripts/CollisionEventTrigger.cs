using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Utility.CommonUtils
{
    public class CollisionEventTrigger : MonoBehaviour
    {
        public string triggerTag = "Player";

        public float collisionThresold = 0f;

        public UnityEventGameObject onTriggerEnter;

        public UnityEventGameObject onTriggerExit;

        public UnityEventGameObject onCollisionEnter;

        public UnityEventGameObject onCollisionExit;

        public string messageToCollider;

        public void OnTriggerEnter(Collider other)
        {
            if (string.IsNullOrEmpty(triggerTag) || other.CompareTag(triggerTag))
            {
                onTriggerEnter.Invoke(other.gameObject);

                if (!string.IsNullOrEmpty(messageToCollider))
                {
                    Debug.Log(messageToCollider + "Enter to " + other.gameObject);
                    other.SendMessage(messageToCollider + "Enter", gameObject, SendMessageOptions.DontRequireReceiver);
                }
            }
        }

        public void OnTriggerExit(Collider other)
        {
            if (string.IsNullOrEmpty(triggerTag) || other.CompareTag(triggerTag))
            {
                onTriggerExit.Invoke(other.gameObject);

                if (!string.IsNullOrEmpty(messageToCollider))
                {
                    other.SendMessage(messageToCollider + "Exit", gameObject, SendMessageOptions.DontRequireReceiver);
                }
            }
        }

        public void OnCollisionEnter(Collision collision)
        {
            if (collision.impulse.magnitude < collisionThresold)
                return;

            if (string.IsNullOrEmpty(triggerTag) || collision.rigidbody.CompareTag(triggerTag))
            {
                onCollisionEnter.Invoke(collision.rigidbody.gameObject);

                if (!string.IsNullOrEmpty(messageToCollider))
                {
                    collision.rigidbody.gameObject.SendMessage(messageToCollider + "Enter", gameObject, SendMessageOptions.DontRequireReceiver);
                }
            }
        }

        public void OnCollisionExit(Collision collision)
        {
            if (string.IsNullOrEmpty(triggerTag) || collision.rigidbody.CompareTag(triggerTag))
            {
                onCollisionExit.Invoke(collision.rigidbody.gameObject);

                if (!string.IsNullOrEmpty(messageToCollider))
                {
                    collision.rigidbody.gameObject.SendMessage(messageToCollider + "Exit", gameObject, SendMessageOptions.DontRequireReceiver);
                }
            }
        }
    }
}