using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollidersActivator : MonoBehaviour
{

    Dictionary<Collider, bool> collidersOriginalStates = new Dictionary<Collider, bool>();

    private void Start()
    {
        SaveCollidersStates();
    }

    public void SaveCollidersStates()
    {
        foreach(Collider c in GetComponentsInChildren<Collider>())
        {
            if (collidersOriginalStates.ContainsKey(c))
            {
                collidersOriginalStates[c] = c.enabled;
            }
            else
                collidersOriginalStates.Add(c, c.enabled);
        }
    }

    public void RestoreCollidersStates()
    {
        foreach (var kvp in collidersOriginalStates)
        {
            kvp.Key.enabled = kvp.Value;
        }
    }

    public void AllCollidersEnabled(bool value)
    {
        foreach (var kvp in collidersOriginalStates)
        {
            kvp.Key.enabled = value;
        }
    }
}
