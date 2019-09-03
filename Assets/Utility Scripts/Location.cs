using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Location
{
    public float latitude;
    public float longitude;

    public Location(float latitude, float longitude)
    {
        this.latitude = latitude;
        this.longitude = longitude;
    }

    public override string ToString()
    {
        return "latitude: " + latitude + "\n" + "longitude: " + longitude;
    }
}


[System.Serializable]
public class UnityEventLocation : UnityEvent<Location>
{

}