using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleSystemsController : MonoBehaviour
{
    
    public void EnableEmissionForAll(bool value)
    {
        var systems = GetComponentsInChildren<ParticleSystem>();

        // find out the maximum lifetime of any particles in this effect
        foreach (var ps in systems)
        {
            var em = ps.emission;
            em.enabled = value;
        }

    }

}
