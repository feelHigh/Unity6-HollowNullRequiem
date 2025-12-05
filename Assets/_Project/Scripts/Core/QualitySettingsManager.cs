// ============================================
// QualitySettingsManager.cs
// Device-adaptive quality settings with auto-detection
// ============================================

using UnityEngine;

namespace HNR.Core
{
    /// <summary>
    /// Manages quality settings based on device capabilities.
    /// Auto-detects device tier and applies appropriate settings.
    /// </summary>
    /// <remarks>
    /// Device tiers (TDD 10):
    /// - Low: &lt;4GB RAM → 30fps, Quality Level 0
    /// - Mid: 4-6GB RAM → 45fps, Quality Level 1
    /// - High: 6GB+ RAM → 60fps, Quality Level 2
    ///
    /// User can override auto-detection via SetQualityTier().
    /// Preference saved to PlayerPrefs.
    /// </remarks>
    public class QualitySettingsManager : MonoBehaviour
    {
        // ============================================
        // Configuration
        // ============================================

        [Header("Detection")]
        [SerializeField, Tooltip("Automatically detect device tier on startup")]
        private bool _autoDetect = true;

        [Header("Current Settings")]
        [SerializeField]
        private QualityTier _currentTier = QualityTier.Mid;

        [Header("Override")]
        [SerializeField, Tooltip("Force a specific quality tier")]
        private bool _forceQualityTier;

        [SerializeField]
        private QualityTier _forcedTier;

        // ============================================
        // Constants
        // ============================================

        private const string QUALITY_PREF_KEY = "QualityTier";
        private const int RAM_THRESHOLD_HIGH = 6000;  // 6GB+
        private const int RAM_THRESHOLD_MID = 4000;   // 4GB+

        // ============================================
        // Properties
        // ============================================

        /// <summary>Current applied quality tier.</summary>
        public QualityTier CurrentTier => _currentTier;

        /// <summary>True if using user-set quality instead of auto-detect.</summary>
        public bool IsUsingOverride => _forceQualityTier;

        /// <summary>Target frame rate for current tier.</summary>
        public int TargetFrameRate => _currentTier switch
        {
            QualityTier.Low => 30,
            QualityTier.Mid => 45,
            QualityTier.High => 60,
            _ => 60
        };

        // ============================================
        // Unity Lifecycle
        // ============================================

        private void Awake()
        {
            ServiceLocator.Register(this);
            Initialize();
        }

        private void OnDestroy()
        {
            ServiceLocator.Unregister<QualitySettingsManager>();
        }

        // ============================================
        // Initialization
        // ============================================

        private void Initialize()
        {
            // Check for saved user preference
            if (PlayerPrefs.HasKey(QUALITY_PREF_KEY))
            {
                _forceQualityTier = true;
                _forcedTier = (QualityTier)PlayerPrefs.GetInt(QUALITY_PREF_KEY);
                Debug.Log($"[QualitySettings] Loaded saved preference: {_forcedTier}");
            }

            // Apply quality settings
            if (_forceQualityTier)
            {
                ApplyQualityTier(_forcedTier);
            }
            else if (_autoDetect)
            {
                var detected = DetectDeviceTier();
                ApplyQualityTier(detected);
            }
        }

        // ============================================
        // Detection
        // ============================================

        /// <summary>
        /// Detect device tier based on hardware specifications.
        /// </summary>
        /// <returns>Detected quality tier</returns>
        public QualityTier DetectDeviceTier()
        {
            int ramMB = SystemInfo.systemMemorySize;

            QualityTier detected;
            if (ramMB >= RAM_THRESHOLD_HIGH)
            {
                detected = QualityTier.High;
            }
            else if (ramMB >= RAM_THRESHOLD_MID)
            {
                detected = QualityTier.Mid;
            }
            else
            {
                detected = QualityTier.Low;
            }

            Debug.Log($"[QualitySettings] Detected tier: {detected} (RAM: {ramMB}MB)");
            return detected;
        }

        // ============================================
        // Quality Application
        // ============================================

        /// <summary>
        /// Apply quality tier settings.
        /// </summary>
        /// <param name="tier">Quality tier to apply</param>
        public void ApplyQualityTier(QualityTier tier)
        {
            _currentTier = tier;

            switch (tier)
            {
                case QualityTier.Low:
                    ApplyLowQuality();
                    break;
                case QualityTier.Mid:
                    ApplyMidQuality();
                    break;
                case QualityTier.High:
                    ApplyHighQuality();
                    break;
            }

            Debug.Log($"[QualitySettings] Applied tier: {tier}");
        }

        private void ApplyLowQuality()
        {
            Application.targetFrameRate = 30;
            QualitySettings.SetQualityLevel(0, applyExpensiveChanges: true);
            QualitySettings.vSyncCount = 0;

            // Reduce VFX intensity
            ApplyVFXQuality(0.5f);
        }

        private void ApplyMidQuality()
        {
            Application.targetFrameRate = 45;
            QualitySettings.SetQualityLevel(1, applyExpensiveChanges: true);
            QualitySettings.vSyncCount = 0;

            // Moderate VFX
            ApplyVFXQuality(0.75f);
        }

        private void ApplyHighQuality()
        {
            Application.targetFrameRate = 60;
            QualitySettings.SetQualityLevel(2, applyExpensiveChanges: true);
            QualitySettings.vSyncCount = 0;

            // Full VFX quality
            ApplyVFXQuality(1f);
        }

        private void ApplyVFXQuality(float multiplier)
        {
            // Scale particle system emission rates based on quality
            var particleSystems = FindObjectsByType<ParticleSystem>(FindObjectsSortMode.None);
            foreach (var ps in particleSystems)
            {
                var emission = ps.emission;
                // Note: For persistent scaling, you would store original values
                // and apply multiplier. This is a simplified example.
                emission.rateOverTimeMultiplier = multiplier;
            }
        }

        // ============================================
        // User Preference API
        // ============================================

        /// <summary>
        /// Set quality tier (user preference override).
        /// Saves to PlayerPrefs for persistence.
        /// </summary>
        /// <param name="tier">Quality tier to set</param>
        public void SetQualityTier(QualityTier tier)
        {
            _forceQualityTier = true;
            _forcedTier = tier;

            // Save preference
            PlayerPrefs.SetInt(QUALITY_PREF_KEY, (int)tier);
            PlayerPrefs.Save();

            ApplyQualityTier(tier);
            Debug.Log($"[QualitySettings] User set tier: {tier}");
        }

        /// <summary>
        /// Reset to auto-detection mode.
        /// Clears saved preference.
        /// </summary>
        public void ResetToAuto()
        {
            _forceQualityTier = false;
            PlayerPrefs.DeleteKey(QUALITY_PREF_KEY);
            PlayerPrefs.Save();

            var detected = DetectDeviceTier();
            ApplyQualityTier(detected);
            Debug.Log("[QualitySettings] Reset to auto-detect");
        }

        // ============================================
        // Debug Info
        // ============================================

        /// <summary>
        /// Get device and quality info string for debugging/display.
        /// </summary>
        public string GetDeviceInfoString()
        {
            return $"Device: {SystemInfo.deviceModel}\n" +
                   $"RAM: {SystemInfo.systemMemorySize}MB\n" +
                   $"GPU: {SystemInfo.graphicsDeviceName}\n" +
                   $"Quality Tier: {_currentTier}\n" +
                   $"Target FPS: {TargetFrameRate}\n" +
                   $"Override: {(_forceQualityTier ? "Yes" : "Auto")}";
        }

        /// <summary>
        /// Log current device and quality info to console.
        /// </summary>
        public void LogDeviceInfo()
        {
            Debug.Log($"[QualitySettings] {GetDeviceInfoString()}");
        }
    }
}
