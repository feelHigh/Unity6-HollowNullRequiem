// ============================================
// VersionDisplay.cs
// UI component for showing build version
// ============================================

using UnityEngine;
using TMPro;

namespace HNR.UI
{
    /// <summary>
    /// Displays application version in UI with optional build info.
    /// </summary>
    public class VersionDisplay : MonoBehaviour
    {
        // ============================================
        // Serialized Fields
        // ============================================

        [Header("References")]
        [SerializeField] private TextMeshProUGUI _versionText;

        [Header("Format")]
        [SerializeField] private string _format = "v{0}";
        [SerializeField] private bool _includeBuildNumber = true;

        // ============================================
        // Unity Lifecycle
        // ============================================

        private void Start()
        {
            UpdateDisplay();
        }

        // ============================================
        // Public Methods
        // ============================================

        /// <summary>
        /// Update the version display text.
        /// </summary>
        public void UpdateDisplay()
        {
            if (_versionText == null) return;

            string version = Application.version;

            if (_includeBuildNumber)
            {
#if UNITY_ANDROID && !UNITY_EDITOR
                int versionCode = GetAndroidVersionCode();
                if (versionCode > 0)
                    version += $" ({versionCode})";
#endif
            }

            _versionText.text = string.Format(_format, version);
        }

        /// <summary>
        /// Get full version string with build type suffix.
        /// </summary>
        /// <returns>Version string with build type.</returns>
        public string GetFullVersion()
        {
            string version = Application.version;

#if UNITY_EDITOR
            version += " (Editor)";
#elif DEVELOPMENT_BUILD
            version += " (Dev)";
#else
            version += " (Release)";
#endif

            return version;
        }

        /// <summary>
        /// Get detailed build information.
        /// </summary>
        /// <returns>Multi-line build info string.</returns>
        public string GetBuildInfo()
        {
            return $"Version: {Application.version}\n" +
                   $"Unity: {Application.unityVersion}\n" +
                   $"Platform: {Application.platform}\n" +
                   $"Bundle: {Application.identifier}";
        }

        /// <summary>
        /// Get version for display in logs.
        /// </summary>
        /// <returns>Compact version string.</returns>
        public string GetLogVersion()
        {
            return $"{Application.identifier} v{Application.version}";
        }

        /// <summary>
        /// Set custom format string.
        /// </summary>
        /// <param name="format">Format string with {0} placeholder.</param>
        public void SetFormat(string format)
        {
            _format = format;
            UpdateDisplay();
        }

#if UNITY_ANDROID
        private int GetAndroidVersionCode()
        {
            try
            {
                using var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
                using var context = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
                using var packageManager = context.Call<AndroidJavaObject>("getPackageManager");
                using var packageInfo = packageManager.Call<AndroidJavaObject>(
                    "getPackageInfo", Application.identifier, 0);
                return packageInfo.Get<int>("versionCode");
            }
            catch
            {
                return 0;
            }
        }
#endif
    }
}
