using UnityEngine;
using System.Collections;
using Newtonsoft.Json;
using UnityEngine.Networking;


/// <summary>
/// The Geo data for a user.
/// 
/// http://ip-api.com/docs/api:json
/// 
/// <code>
/// {
/// 	"status": "success",
/// 	"country": "COUNTRY",
/// 	"countryCode": "COUNTRY CODE",
/// 	"region": "REGION CODE",
/// 	"regionName": "REGION NAME",
/// 	"city": "CITY",
/// 	"zip": "ZIP CODE",
/// 	"lat": LATITUDE,
/// 	"lon": LONGITUDE,
/// 	"timezone": "TIME ZONE",
/// 	"isp": "ISP NAME",
/// 	"org": "ORGANIZATION NAME",
/// 	"as": "AS NUMBER / NAME",
/// 	"query": "IP ADDRESS USED FOR QUERY"
/// }
/// </code>
/// 
/// </summary>
public class GeoData 
{
	/// <summary>
	/// The status that is returned if the response was successful.
	/// </summary>
	public const string SuccessResult = "success";

	[JsonProperty("status")]
	public string Status { get; set; }

	[JsonProperty("country")]
	public string Country { get; set; }

    [JsonProperty("lat")]
    public float lat { get; set; }

    [JsonProperty("lon")]
    public float lon { get; set; }

    [JsonProperty("query")]
	public string IpAddress { get; set; }
}

public class GeoCountry : MonoBehaviour {

    public float lat, lon;

    public bool isInProgress = true;

    IEnumerator GetGeoData()
    {
        isInProgress = true;
        UnityWebRequest www = UnityWebRequest.Get("http://ip-api.com/json");
        yield return www.SendWebRequest();

        if (www.isNetworkError || www.isHttpError)
        {
            Debug.Log(www.error);
        }
        else
        {
            // Show results as text
            //Debug.Log(www.downloadHandler.text);

            string results = www.downloadHandler.text;

            var data = JsonConvert.DeserializeObject<GeoData>(results);

            // Ensure successful
            if (data.Status != GeoData.SuccessResult)
            {

                // TODO: Hook into an auto retry case

                Debug.LogError("Unsuccessful geo data request: " + results);

                isInProgress = false;

                yield break;
            }

            Debug.Log("User's Country: \"" + data.Country + "\"; Query: \"" + data.IpAddress + "\"");
            lat = data.lat;
            lon = data.lon;
            isInProgress = false;
        }
    }

    // Use this for initialization
    void Start () {
        StartCoroutine(GetGeoData());
	}

	// Update is called once per frame
	void Update () {
	
	}
}