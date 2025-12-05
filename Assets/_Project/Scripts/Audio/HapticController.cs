// ============================================
// HapticController.cs
// Mobile haptic feedback with intensity levels
// ============================================

using UnityEngine;
using HNR.Core;

namespace HNR.Audio
{
    /// <summary>
    /// Haptic feedback intensity levels.
    /// </summary>
    public enum HapticIntensity
    {
        /// <summary>Light tap for UI interactions (buttons, toggles).</summary>
        Light,

        /// <summary>Medium impact for gameplay events (card plays, hits).</summary>
        Medium,

        /// <summary>Heavy impact for significant events (critical hits, Null State).</summary>
        Heavy
    }

    /// <summary>
    /// Manages haptic feedback for mobile devices.
    /// Uses Android native vibration API for precise control.
    /// </summary>
    /// <remarks>
    /// Register with ServiceLocator at startup.
    /// Persists enabled state via PlayerPrefs.
    /// </remarks>
    public class HapticController : MonoBehaviour
    {
        // ============================================
        // Configuration
        // ============================================

        [Header("Settings")]
        [SerializeField] private bool _hapticsEnabled = true;

        [Header("Durations (milliseconds)")]
        [Tooltip("Duration for light UI feedback")]
        [SerializeField] private long _lightDuration = 10;

        [Tooltip("Duration for medium gameplay feedback")]
        [SerializeField] private long _mediumDuration = 25;

        [Tooltip("Duration for heavy impact feedback")]
        [SerializeField] private long _heavyDuration = 50;

        // ============================================
        // Constants
        // ============================================

        private const string PREFS_KEY_HAPTICS = "HapticsEnabled";

        // ============================================
        // Properties
        // ============================================

        /// <summary>
        /// Whether haptic feedback is enabled.
        /// Persists to PlayerPrefs when set.
        /// </summary>
        public bool HapticsEnabled
        {
            get => _hapticsEnabled;
            set
            {
                _hapticsEnabled = value;
                PlayerPrefs.SetInt(PREFS_KEY_HAPTICS, value ? 1 : 0);
                PlayerPrefs.Save();
                Debug.Log($"[HapticController] Haptics {(value ? "enabled" : "disabled")}");
            }
        }

        // ============================================
        // Lifecycle
        // ============================================

        private void Awake()
        {
            ServiceLocator.Register(this);
            LoadSettings();
        }

        private void OnDestroy()
        {
            ServiceLocator.Unregister<HapticController>();
        }

        private void LoadSettings()
        {
            _hapticsEnabled = PlayerPrefs.GetInt(PREFS_KEY_HAPTICS, 1) == 1;
            Debug.Log($"[HapticController] Initialized - Haptics: {(_hapticsEnabled ? "ON" : "OFF")}");
        }

        // ============================================
        // Public API
        // ============================================

        /// <summary>
        /// Trigger haptic feedback with specified intensity.
        /// </summary>
        /// <param name="intensity">Vibration intensity level</param>
        public void Vibrate(HapticIntensity intensity)
        {
            if (!_hapticsEnabled) return;

#if UNITY_ANDROID && !UNITY_EDITOR
            long duration = GetDuration(intensity);
            VibrateAndroid(duration);
#elif UNITY_EDITOR
            // Log for testing in editor
            Debug.Log($"[HapticController] Vibrate: {intensity}");
#endif
        }

        /// <summary>
        /// Light haptic for UI interactions (buttons, toggles).
        /// </summary>
        public void LightTap() => Vibrate(HapticIntensity.Light);

        /// <summary>
        /// Medium haptic for gameplay events (card plays, damage).
        /// </summary>
        public void MediumImpact() => Vibrate(HapticIntensity.Medium);

        /// <summary>
        /// Heavy haptic for significant events (critical hits, Null State).
        /// </summary>
        public void HeavyImpact() => Vibrate(HapticIntensity.Heavy);

        /// <summary>
        /// Custom duration vibration (Android only).
        /// </summary>
        /// <param name="milliseconds">Duration in milliseconds</param>
        public void VibrateCustom(long milliseconds)
        {
            if (!_hapticsEnabled) return;

#if UNITY_ANDROID && !UNITY_EDITOR
            VibrateAndroid(milliseconds);
#elif UNITY_EDITOR
            Debug.Log($"[HapticController] Custom vibrate: {milliseconds}ms");
#endif
        }

        // ============================================
        // Private Methods
        // ============================================

        private long GetDuration(HapticIntensity intensity)
        {
            return intensity switch
            {
                HapticIntensity.Light => _lightDuration,
                HapticIntensity.Medium => _mediumDuration,
                HapticIntensity.Heavy => _heavyDuration,
                _ => _mediumDuration
            };
        }

#if UNITY_ANDROID && !UNITY_EDITOR
        /// <summary>
        /// Android native vibration using VibrationEffect (API 26+).
        /// Falls back to legacy vibrate for older devices.
        /// </summary>
        private void VibrateAndroid(long milliseconds)
        {
            try
            {
                using var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
                using var currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
                using var vibrator = currentActivity.Call<AndroidJavaObject>("getSystemService", "vibrator");

                if (vibrator == null)
                {
                    Debug.LogWarning("[HapticController] Vibrator service not available");
                    return;
                }

                // Check if device has vibrator
                bool hasVibrator = vibrator.Call<bool>("hasVibrator");
                if (!hasVibrator)
                {
                    Debug.LogWarning("[HapticController] Device does not have vibrator");
                    return;
                }

                // Android 8.0+ (API 26+) uses VibrationEffect
                // Project targets API 24+, so check SDK version
                int sdkInt = GetAndroidSDKVersion();

                if (sdkInt >= 26)
                {
                    // Use VibrationEffect for API 26+
                    using var vibrationEffectClass = new AndroidJavaClass("android.os.VibrationEffect");
                    // -1 is DEFAULT_AMPLITUDE
                    using var effect = vibrationEffectClass.CallStatic<AndroidJavaObject>(
                        "createOneShot", milliseconds, -1);
                    vibrator.Call("vibrate", effect);
                }
                else
                {
                    // Legacy vibrate for API 24-25
                    vibrator.Call("vibrate", milliseconds);
                }
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"[HapticController] Vibration failed: {e.Message}");
            }
        }

        private int GetAndroidSDKVersion()
        {
            try
            {
                using var buildVersion = new AndroidJavaClass("android.os.Build$VERSION");
                return buildVersion.GetStatic<int>("SDK_INT");
            }
            catch
            {
                return 24; // Minimum supported
            }
        }
#endif
    }
}
