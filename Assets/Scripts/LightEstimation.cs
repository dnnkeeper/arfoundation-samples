using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.XR.ARFoundation;

/// <summary>
/// A component that can be used to access the most
/// recently received light estimation information
/// for the physical environment as observed by an
/// AR device.
/// </summary>
[RequireComponent(typeof(Light))]
public class LightEstimation : MonoBehaviour
{
    [SerializeField]
    [Tooltip("The ARCameraManager which will produce frame events containing light estimation information.")]
    ARCameraManager m_CameraManager;


    public float intensityMultiplier = 1f;
    public float ambientIntensityMultiplier = 1f;

    /// <summary>
    /// Affect only ambient color if False
    /// </summary>
    public bool affectLightSource;

    public AmbientMode ambientMode = AmbientMode.Trilight;

    [SerializeField]
    Vector3 ambientGradientMultiplier = new Vector3(1.0f, 0.5f, 0.1f);

    /// <summary>
    /// Get or set the <c>ARCameraManager</c>.
    /// </summary>
    public ARCameraManager cameraManager
    {
        get { return m_CameraManager; }
        set
        {
            if (m_CameraManager == value)
                return;

            if (m_CameraManager != null)
                m_CameraManager.frameReceived -= FrameChanged;

            m_CameraManager = value;

            if (m_CameraManager != null & enabled)
                m_CameraManager.frameReceived += FrameChanged;
        }
    }

    /// <summary>
    /// The estimated brightness of the physical environment, if available.
    /// </summary>
    public float? brightness { get; private set; }

    /// <summary>
    /// The estimated color temperature of the physical environment, if available.
    /// </summary>
    public float? colorTemperature { get; private set; }

    /// <summary>
    /// The estimated color correction value of the physical environment, if available.
    /// </summary>
    public Color? colorCorrection { get; private set; }

    void Awake ()
    {
        m_Light = GetComponent<Light>();
    }

    void OnEnable()
    {
        RenderSettings.ambientMode = ambientMode;
        if (m_CameraManager != null)
            m_CameraManager.frameReceived += FrameChanged;
    }

    void OnDisable()
    {
        if (m_CameraManager != null)
            m_CameraManager.frameReceived -= FrameChanged;
    }

    void FrameChanged(ARCameraFrameEventArgs args)
    {
        if (args.lightEstimation.averageBrightness.HasValue)
        {
            brightness = args.lightEstimation.averageBrightness.Value;

            if (affectLightSource)
                m_Light.intensity = brightness.Value * intensityMultiplier;

            RenderSettings.ambientIntensity = brightness.Value * ambientIntensityMultiplier;
        }

        if (args.lightEstimation.averageColorTemperature.HasValue)
        {
            colorTemperature = args.lightEstimation.averageColorTemperature.Value;

            if (affectLightSource)
                m_Light.colorTemperature = colorTemperature.Value;
        }

        if (args.lightEstimation.colorCorrection.HasValue)
        {
            colorCorrection = args.lightEstimation.colorCorrection.Value;

            if (affectLightSource)
                m_Light.color = colorCorrection.Value;

            if (args.lightEstimation.averageBrightness.HasValue)
            {
                var brightness = args.lightEstimation.averageBrightness.Value * ambientIntensityMultiplier;
                colorCorrection *= brightness;
            }

            RenderSettings.ambientLight = colorCorrection.Value * ambientGradientMultiplier.x;
            RenderSettings.ambientSkyColor = colorCorrection.Value * ambientGradientMultiplier.x;
            RenderSettings.ambientEquatorColor = colorCorrection.Value * ambientGradientMultiplier.y;
            RenderSettings.ambientGroundColor = colorCorrection.Value * ambientGradientMultiplier.z;

        }
    }

    Light m_Light;
}
