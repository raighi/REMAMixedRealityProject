using UnityEngine;
#if UNITY_ANDROID && !UNITY_EDITOR
using UnityEngine.Android;
#endif
using UnityEngine.XR;
using System.Collections.Generic;

/// <summary>
/// Service haptique multi-plateforme.
/// Abstrait la vibration téléphone (AR) et les haptics Quest (VR).
/// Singleton accessible globalement via HapticService.Instance.
///
/// Setup :
/// - Créer un GameObject "HapticService" dans la scène
/// - Attacher ce script
/// - Assigner un HapticConfig
/// - Appeler HapticService.Instance.Play(HapticType.Grab) depuis n'importe quel script
/// </summary>
public class HapticService : MonoBehaviour
{
    public static HapticService Instance { get; private set; }

    [Header("Config")]
    [SerializeField] private HapticConfig config;

    // Cache pour les devices XR (Quest controllers)
    private InputDevice? _rightController;
    private InputDevice? _leftController;
    private bool _isVR;

    // Android vibrator (pour haptics plus fins que Handheld.Vibrate)
#if UNITY_ANDROID && !UNITY_EDITOR
    private AndroidJavaObject _vibrator;
    private bool _hasVibrator;
#endif

    // ─────────────────────────────────────────────
    // Lifecycle
    // ─────────────────────────────────────────────

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        DetectPlatform();
    }

    private void DetectPlatform()
    {
        // Détecter si on est en VR (Quest)
        var xrDisplaySubsystems = new List<XRDisplaySubsystem>();
        SubsystemManager.GetSubsystems(xrDisplaySubsystems);
        _isVR = xrDisplaySubsystems.Count > 0 && xrDisplaySubsystems[0].running;

        if (_isVR)
        {
            RefreshControllers();
        }
        else
        {
            InitAndroidVibrator();
        }
    }

    private void RefreshControllers()
    {
        var devices = new List<InputDevice>();

        InputDevices.GetDevicesWithCharacteristics(
            InputDeviceCharacteristics.Right | InputDeviceCharacteristics.Controller, devices);
        if (devices.Count > 0) _rightController = devices[0];

        devices.Clear();
        InputDevices.GetDevicesWithCharacteristics(
            InputDeviceCharacteristics.Left | InputDeviceCharacteristics.Controller, devices);
        if (devices.Count > 0) _leftController = devices[0];
    }

    private void InitAndroidVibrator()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        try
        {
            using (var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            {
                var activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
                _vibrator = activity.Call<AndroidJavaObject>("getSystemService", "vibrator");
                _hasVibrator = _vibrator != null && _vibrator.Call<bool>("hasVibrator");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"[HapticService] Impossible d'initialiser le vibrator Android : {e.Message}");
            _hasVibrator = false;
        }
#endif
    }

    // ─────────────────────────────────────────────
    // API publique
    // ─────────────────────────────────────────────

    /// <summary>
    /// Joue un retour haptique du type spécifié.
    /// </summary>
    public void Play(HapticType type)
    {
        if (config == null) return;

        float duration, amplitude, frequency;

        switch (type)
        {
            case HapticType.Grab:
                duration = config.GrabDuration;
                amplitude = config.GrabAmplitude;
                frequency = config.GrabFrequency;
                break;

            case HapticType.Drop:
                duration = config.DropDuration;
                amplitude = config.DropAmplitude;
                frequency = config.DropFrequency;
                break;

            case HapticType.CoopFullGrab:
                duration = config.CoopFullGrabDuration;
                amplitude = config.CoopFullGrabAmplitude;
                frequency = config.CoopFullGrabFrequency;
                break;

            default:
                return;
        }

        if (_isVR)
            PlayVRHaptics(amplitude, duration);
        else
            PlayMobileHaptics(duration, amplitude);
    }

    // ─────────────────────────────────────────────
    // Implémentations plateformes
    // ─────────────────────────────────────────────

    private void PlayVRHaptics(float amplitude, float duration)
    {
        // Refresh si les controllers n'étaient pas prêts au démarrage
        if (_rightController == null || !_rightController.Value.isValid)
            RefreshControllers();

        // Vibrer les deux controllers
        if (_rightController.HasValue && _rightController.Value.isValid)
            _rightController.Value.SendHapticImpulse(0, amplitude, duration);

        if (_leftController.HasValue && _leftController.Value.isValid)
            _leftController.Value.SendHapticImpulse(0, amplitude, duration);
    }

    private void PlayMobileHaptics(float duration, float amplitude)
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        if (!_hasVibrator || _vibrator == null) return;

        try
        {
            long milliseconds = (long)(duration * 1000f);
            int androidAmplitude = Mathf.Clamp((int)(amplitude * 255f), 1, 255);

            // API 26+ (Android 8+) : VibrationEffect avec amplitude
            if (GetAndroidSDKVersion() >= 26)
            {
                using (var vibrationEffectClass = new AndroidJavaClass("android.os.VibrationEffect"))
                {
                    var effect = vibrationEffectClass.CallStatic<AndroidJavaObject>(
                        "createOneShot", milliseconds, androidAmplitude);
                    _vibrator.Call("vibrate", effect);
                }
            }
            else
            {
                // Fallback : vibration simple sans contrôle d'amplitude
                _vibrator.Call("vibrate", milliseconds);
            }
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"[HapticService] Erreur vibration Android : {e.Message}");
            // Fallback ultime
            Handheld.Vibrate();
        }
#else
        // Fallback éditeur / iOS
        Handheld.Vibrate();
#endif
    }

#if UNITY_ANDROID && !UNITY_EDITOR
    private int GetAndroidSDKVersion()
    {
        using (var version = new AndroidJavaClass("android.os.Build$VERSION"))
        {
            return version.GetStatic<int>("SDK_INT");
        }
    }
#endif
}