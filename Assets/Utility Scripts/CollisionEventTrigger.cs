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

        public UnityEventGameObject onParticlesHit;

        public string messageToCollider;

        public void OnTriggerEnter(Collider other)
        {
            if (string.IsNullOrEmpty(triggerTag) || other.CompareTag(triggerTag))
            {
                onTriggerEnter.Invoke(other.gameObject);

                if (!string.IsNullOrEmpty(messageToCollider))
                {
                    Debug.Log(messageToCollider + "Enter to " + other.gameObject, other.gameObject);
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

            if (string.IsNullOrEmpty(triggerTag) || (collision.rigidbody != null && collision.rigidbody.CompareTag(triggerTag)) )
            {
                onCollisionEnter.Invoke(collision.rigidbody!=null?collision.rigidbody.gameObject:collision.collider.gameObject);

                //Debug.Log("onCollisionEnter "+ collision.rigidbody.name, this);

                if (!string.IsNullOrEmpty(messageToCollider))
                {
                    var messageTarget = collision.rigidbody!=null? collision.rigidbody.gameObject: collision.gameObject;
                    messageTarget.SendMessage(messageToCollider + "Enter", gameObject, SendMessageOptions.DontRequireReceiver);
                }
            }
        }

        public void OnCollisionExit(Collision collision)
        {
            if (string.IsNullOrEmpty(triggerTag) || (collision.rigidbody != null && collision.rigidbody.CompareTag(triggerTag)) )
            {
                onCollisionExit.Invoke(collision.rigidbody != null ? collision.rigidbody.gameObject : collision.collider.gameObject);

                if (!string.IsNullOrEmpty(messageToCollider))
                {
                    var messageTarget = collision.rigidbody != null ? collision.rigidbody.gameObject : collision.gameObject;
                    messageTarget.SendMessage(messageToCollider + "Exit", gameObject, SendMessageOptions.DontRequireReceiver);
                }
            }
        }

        void OnParticleHit(ParticleSystem particlesSystem)
        {
            Debug.Log("Hit by particles from "+particlesSystem.gameObject.name, this);
            onParticlesHit.Invoke(particlesSystem.gameObject);
        }
    }
}