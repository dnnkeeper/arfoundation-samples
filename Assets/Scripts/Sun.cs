using Reflector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Entropedia
{
    [RequireComponent(typeof(Light))]
    public class Sun : MonoBehaviour
    {
        public ReflectorProbe reflectionProbe;

        public UnityEvent onUpdateSunRotation;

        [SerializeField]
        bool controlIntensity = true;

        [SerializeField]
        float intensityMultiplier = 1f;

        [SerializeField]
        float longitude;

        [SerializeField]
        float latitude;

        [SerializeField]
        [Range(0, 24)]
        int hour;

        [SerializeField]
        [Range(0, 60)]
        int minutes;

        DateTime time;
        Light lightSource;

        [SerializeField]
        float timeSpeed = 1;

        [SerializeField]
        int frameSteps = 1;
        int frameStep;

        public Light LightSource
        {
            get
            {
                if (lightSource == null)
                    lightSource = GetComponent<Light>();
                return lightSource;
            }
        }

        public void SetTime(DateTime time) {
            this.time = time;
            OnValidate();
        }

        public void SetTime(int hour, int minutes) {
            this.hour = hour;
            this.minutes = minutes;
            OnValidate();
        }

        public void SetUpdateSteps(int i) {
            frameSteps = i;
        }

        public void SetTimeSpeed(float speed) {
            timeSpeed = speed;
        }

        private void Awake()
        {
            
            time = DateTime.Now;
            hour = time.Hour;
            minutes = time.Minute;
        }

        IEnumerator Start()
        {
            yield return new WaitForSecondsRealtime(1f);
            var webLocation = GetComponent<GeoCountry>();
            if (webLocation != null)
            {
                while (webLocation.isInProgress)
                {
                    print("webLocation.isInProgress");
                    yield return new WaitForSecondsRealtime(1f);
                }

                latitude = webLocation.lat;
                longitude = webLocation.lon;
                print("webLocation location " + latitude + "/" + longitude);
            }


            // First, check if user has location service enabled
            if (!Input.location.isEnabledByUser)
                yield break;

            // Start service before querying location
            Input.location.Start();

            // Wait until service initializes
            int maxWait = 20;
            while (Input.location.status == LocationServiceStatus.Initializing && maxWait > 0)
            {
                yield return new WaitForSeconds(1);
                maxWait--;
            }

            // Service didn't initialize in 20 seconds
            if (maxWait < 1)
            {
                print("Timed out");
                yield break;
            }

            // Connection has failed
            if (Input.location.status == LocationServiceStatus.Failed)
            {
                print("Unable to determine device location");
                yield break;
            }
            else
            {
                // Access granted and location value could be retrieved
                print("Location: " + Input.location.lastData.latitude + " " + Input.location.lastData.longitude + " " + Input.location.lastData.altitude + " " + Input.location.lastData.horizontalAccuracy + " " + Input.location.lastData.timestamp);

                latitude = Input.location.lastData.latitude;
                longitude = Input.location.lastData.longitude;
            }

            // Stop service if there is no need to query location updates continuously
            Input.location.Stop();
        }

        private void OnValidate()
        {
            time = DateTime.Now.Date + new TimeSpan(hour, minutes, 0);
        }

        private void Update()
        {
            time = time.AddSeconds(timeSpeed * Time.deltaTime);
            if (frameStep==0) {
                SetPosition();
                onUpdateSunRotation.Invoke();
                RenderProbe();
            }
            frameStep = (frameStep + 1) % frameSteps;


        }

        [ContextMenu("RenderProbe")]
        void RenderProbe()
        {
            if (reflectionProbe != null)
                reflectionProbe.RefreshReflection();
        }

        void SetPosition()
        {
            Vector3 angles = new Vector3();
            double alt;
            double azi;
            SunPosition.CalculateSunPosition(time, (double)latitude, (double)longitude, out azi, out alt);
            angles.x = (float)alt * Mathf.Rad2Deg;
            angles.y = 180f + (float)azi * Mathf.Rad2Deg;
            //UnityEngine.Debug.Log(" sun angles:"+angles);
            transform.localRotation = Quaternion.Euler(angles);
            if (controlIntensity)
                LightSource.intensity = Mathf.InverseLerp(-10f, 0f, angles.x) * intensityMultiplier;
            //UnityEngine.Debug.Log(" LightSource.intensity: "+LightSource.intensity);
        }

    }

    /*
     * The following source came from this blog:
     * http://guideving.blogspot.co.uk/2010/08/sun-position-in-c.html
     */
    public static class SunPosition
    {
        private const double Deg2Rad = Math.PI / 180.0;
        private const double Rad2Deg = 180.0 / Math.PI;

        /*! 
         * \brief Calculates the sun light. 
         * 
         * CalcSunPosition calculates the suns "position" based on a 
         * given date and time in local time, latitude and longitude 
         * expressed in decimal degrees. It is based on the method 
         * found here: 
         * http://www.astro.uio.no/~bgranslo/aares/calculate.html 
         * The calculation is only satisfiably correct for dates in 
         * the range March 1 1900 to February 28 2100. 
         * \param dateTime Time and date in local time. 
         * \param latitude Latitude expressed in decimal degrees. 
         * \param longitude Longitude expressed in decimal degrees. 
         */
        public static void CalculateSunPosition(
            DateTime dateTime, double latitude, double longitude, out double outAzimuth, out double outAltitude)
        {
            // Convert to UTC  
            dateTime = dateTime.ToUniversalTime();

            // Number of days from J2000.0.  
            double julianDate = 367 * dateTime.Year -
                (int)((7.0 / 4.0) * (dateTime.Year +
                (int)((dateTime.Month + 9.0) / 12.0))) +
                (int)((275.0 * dateTime.Month) / 9.0) +
                dateTime.Day - 730531.5;

            double julianCenturies = julianDate / 36525.0;

            // Sidereal Time  
            double siderealTimeHours = 6.6974 + 2400.0513 * julianCenturies;

            double siderealTimeUT = siderealTimeHours +
                (366.2422 / 365.2422) * (double)dateTime.TimeOfDay.TotalHours;

            double siderealTime = siderealTimeUT * 15 + longitude;

            // Refine to number of days (fractional) to specific time.  
            julianDate += (double)dateTime.TimeOfDay.TotalHours / 24.0;
            julianCenturies = julianDate / 36525.0;

            // Solar Coordinates  
            double meanLongitude = CorrectAngle(Deg2Rad *
                (280.466 + 36000.77 * julianCenturies));

            double meanAnomaly = CorrectAngle(Deg2Rad *
                (357.529 + 35999.05 * julianCenturies));

            double equationOfCenter = Deg2Rad * ((1.915 - 0.005 * julianCenturies) *
                Math.Sin(meanAnomaly) + 0.02 * Math.Sin(2 * meanAnomaly));

            double elipticalLongitude =
                CorrectAngle(meanLongitude + equationOfCenter);

            double obliquity = (23.439 - 0.013 * julianCenturies) * Deg2Rad;

            // Right Ascension  
            double rightAscension = Math.Atan2(
                Math.Cos(obliquity) * Math.Sin(elipticalLongitude),
                Math.Cos(elipticalLongitude));

            double declination = Math.Asin(
                Math.Sin(rightAscension) * Math.Sin(obliquity));

            // Horizontal Coordinates  
            double hourAngle = CorrectAngle(siderealTime * Deg2Rad) - rightAscension;

            if (hourAngle > Math.PI)
            {
                hourAngle -= 2 * Math.PI;
            }

            double altitude = Math.Asin(Math.Sin(latitude * Deg2Rad) *
                Math.Sin(declination) + Math.Cos(latitude * Deg2Rad) *
                Math.Cos(declination) * Math.Cos(hourAngle));

            // Nominator and denominator for calculating Azimuth  
            // angle. Needed to test which quadrant the angle is in.  
            double aziNom = -Math.Sin(hourAngle);
            double aziDenom =
                Math.Tan(declination) * Math.Cos(latitude * Deg2Rad) -
                Math.Sin(latitude * Deg2Rad) * Math.Cos(hourAngle);

            double azimuth = Math.Atan(aziNom / aziDenom);

            if (aziDenom < 0) // In 2nd or 3rd quadrant  
            {
                azimuth += Math.PI;
            }
            else if (aziNom < 0) // In 4th quadrant  
            {
                azimuth += 2 * Math.PI;
            }

            outAltitude = altitude;
            outAzimuth = azimuth;
        }

        /*! 
        * \brief Corrects an angle. 
        * 
        * \param angleInRadians An angle expressed in radians. 
        * \return An angle in the range 0 to 2*PI. 
        */
        private static double CorrectAngle(double angleInRadians)
        {
            if (angleInRadians < 0)
            {
                return 2 * Math.PI - (Math.Abs(angleInRadians) % (2 * Math.PI));
            }
            else if (angleInRadians > 2 * Math.PI)
            {
                return angleInRadians % (2 * Math.PI);
            }
            else
            {
                return angleInRadians;
            }
        }
    }

}