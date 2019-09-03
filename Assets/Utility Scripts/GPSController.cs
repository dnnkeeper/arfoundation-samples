using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Android;
using UnityEngine.Events;

public class GPSController : MonoBehaviour {

    public UnityEventLocation OnFindLocation;
    public UnityEventString OnFailed;
    public UnityEvent OnStartRequestLocationPermission;
    public UnityEvent OnFinishRequestLocationPermission;
    public UnityEvent OnStartEnableLocationService;
    public UnityEvent OnFinishEnableLocationService;
    public int WaitTimeForInitialization = 10;
    public int WaitTimeForHorizontalConvergence = 10;
    public float HorizontalAccuracy = 10000;

    private Coroutine coroutine;
    public void FindLocation() {
        if (coroutine == null) {
#if UNITY_EDITOR
            Location DefaultLocation = new Location(56.2f, 43.8f);
            OnFindLocation.Invoke(DefaultLocation);
#else
#if UNITY_ANDROID
        this.StartThrowingCoroutine(FindLocationOnAndroidCoroutine(), ProcessException);
#else
#if UNITY_IOS
        this.StartThrowingCoroutine(FindLocationOnIOSCoroutine(), ProcessException);
#else
        ProcessException(new Exception("GPS сервис работает только на мобильных платформах. https://docs.unity3d.com/ScriptReference/Input-location.html"));
#endif
#endif
#endif
        }
    }

    public void ProcessException(Exception exception) {
        if (exception != null)
        {
            OnFailed.Invoke(exception.Message);
            if (Input.location.status != LocationServiceStatus.Stopped)
            {
                Input.location.Stop();
                coroutine = null;
            }
        }
    }

#if UNITY_IOS
    public IEnumerator FindLocationOnIOSCoroutine() {
        Start:
        yield return WaitEnableLocationService();
        Input.location.Start();
        var timer1 = WaitTimeForInitialization;
        while (Input.location.status == LocationServiceStatus.Initializing && timer1 > 0)
        {
            timer1--;
            yield return new WaitForSeconds(1);
        }
        if (timer1 <= 0)
        {
            throw new Exception("Превышено время ожидания для инициализации GPS сервиса. Перезагрузите приложение.");
        }
        if (Input.location.status == LocationServiceStatus.Failed || Input.location.status == LocationServiceStatus.Stopped)
        {
            OnStartRequestLocationPermission.Invoke();
            goto Start;
        }
        OnFinishRequestLocationPermission.Invoke();

        if (Input.location.status == LocationServiceStatus.Running)
        {
            var timer2 = WaitTimeForHorizontalConvergence;
            while (Input.location.lastData.horizontalAccuracy > HorizontalAccuracy && timer2 > 0)
            {
                timer2--;
                yield return new WaitForSeconds(1);
            }
            if (timer2 <= 0)
            {
                throw new Exception("Превышено время ожидания для сходимости радиуса точности.");
            }
            OnFindLocation.Invoke(new Location(Input.location.lastData.latitude, Input.location.lastData.longitude));
        }
        Input.location.Stop();
    }
#endif

#if UNITY_ANDROID
    public IEnumerator FindLocationOnAndroidCoroutine() {
        yield return GetPermissionOnAndroid();
        yield return WaitEnableLocationService();
        Input.location.Start();
        var timer1 = WaitTimeForInitialization;
        while (Input.location.status == LocationServiceStatus.Initializing && timer1 > 0)
        {
            timer1--;
            yield return new WaitForSeconds(1);
        }
        if (timer1 <= 0)
        {
            throw new Exception("Превышено время ожидания для инициализации GPS сервиса.");
        }
        if (Input.location.status == LocationServiceStatus.Failed)
        {
            throw new Exception("Произошла ошибка у службы определения местоположения.");
        }
        if (Input.location.status == LocationServiceStatus.Stopped)
        {
            throw new Exception("Cлужба определения местоположения остановлена.");
        }
        if (Input.location.status == LocationServiceStatus.Running)
        {
            var timer2 = WaitTimeForHorizontalConvergence;
            while (Input.location.lastData.horizontalAccuracy > HorizontalAccuracy && timer2 > 0)
            {
                timer2--;
                yield return new WaitForSeconds(1);
            }
            if (timer2 <= 0)
            {
                throw new Exception("Превышено время ожидания для сходимости радиуса точности.");
            }
            OnFindLocation.Invoke(new Location(Input.location.lastData.latitude, Input.location.lastData.longitude));
        }
        Input.location.Stop();
    }
#endif

#if UNITY_ANDROID
    public IEnumerator GetPermissionOnAndroidWithError()
    {
        if (!Permission.HasUserAuthorizedPermission(Permission.FineLocation))
        {
            Permission.RequestUserPermission(Permission.FineLocation);
            yield return new WaitForSeconds(0.1f);
        }

        if (!Permission.HasUserAuthorizedPermission(Permission.FineLocation))
        {
            throw new Exception("Вы отклонили разрешение на GPS данных. Перезапустите приложение или зайдите в настройки и включите разрешение.");
        }
    }
#endif

#if UNITY_ANDROID
    public IEnumerator GetPermissionOnAndroid() {
        if (!Permission.HasUserAuthorizedPermission(Permission.FineLocation)) {
            OnStartRequestLocationPermission.Invoke();
            while (!Permission.HasUserAuthorizedPermission(Permission.FineLocation))
            {
                Permission.RequestUserPermission(Permission.FineLocation);
                yield return new WaitForSeconds(0.5f);
            }
            OnFinishRequestLocationPermission.Invoke();
        }
    }
#endif

    public IEnumerator WaitEnableLocationService() {
        if (!Input.location.isEnabledByUser) {
            OnStartEnableLocationService.Invoke();
            while (!Input.location.isEnabledByUser)
            {
                yield return new WaitForSeconds(0.5f);
            }
            OnFinishEnableLocationService.Invoke();
        }
    }

}

public static class LocationServiceUtils
{
    public static void StartWithRequestPermission(this LocationService service) {
#if UNITY_ANDROID
        if (!Permission.HasUserAuthorizedPermission(Permission.FineLocation)) {
            Permission.RequestUserPermission(Permission.FineLocation);
        }
#endif
        service.Start();
    }
}