using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleSystemCollisionBroadcaster : MonoBehaviour
{
    public ParticleSystem part;
    public List<ParticleCollisionEvent> collisionEvents;

    public string message = "OnParticleHit";

    void Start()
    {
        part = GetComponent<ParticleSystem>();
        collisionEvents = new List<ParticleCollisionEvent>();
    }

    void OnParticleCollision(GameObject other)
    {
        int numCollisionEvents = part.GetCollisionEvents(other, collisionEvents);

        
        Rigidbody rb = other.GetComponent<Rigidbody>();

        if (rb == null)
            rb = other.GetComponentInParent<Rigidbody>();

        GameObject messageReciever = other;
        if (rb != null)
            messageReciever = rb.gameObject;

        int i = 0;

        while (i < numCollisionEvents && i < 10 )
        {
            /*if (rb)
            {
                Vector3 pos = collisionEvents[i].intersection;
                Vector3 force = collisionEvents[i].velocity * 10;
                rb.AddForce(force);
            }
            else
            {
                other.SendMessage(message, SendMessageOptions.DontRequireReceiver);
            }*/

            //Debug.Log("Collision with "+other);
            messageReciever.SendMessage(message, SendMessageOptions.DontRequireReceiver);
            i++;
        }
    }
}
