// ============================================
// SettingsData.cs
// Player settings and preferences
// ============================================

using System;

namespace HNR.Data
{
    /// <summary>
    /// Contains all player settings and preferences.
    /// Serialized by Easy Save 3 for persistence.
    /// </summary>
    [Serializable]
    public class SettingsData
    {
        /// <summary>
        /// Master volume (0.0 - 1.0).
        /// </summary>
        public float MasterVolume = 1.0f;

        /// <summary>
        /// Music volume (0.0 - 1.0).
        /// </summary>
        public float MusicVolume = 0.8f;

        /// <summary>
        /// Sound effects volume (0.0 - 1.0).
        /// </summary>
        public float SFXVolume = 1.0f;

        /// <summary>
        /// Whether vibration/haptics are enabled.
        /// </summary>
        public bool VibrationEnabled = true;

        /// <summary>
        /// Screen shake intensity (0.0 - 1.0).
        /// </summary>
        public float ScreenShakeIntensity = 1.0f;

        /// <summary>
        /// Whether to show damage numbers.
        /// </summary>
        public bool ShowDamageNumbers = true;

        /// <summary>
        /// Whether to show card tooltips.
        /// </summary>
        public bool ShowTooltips = true;

        /// <summary>
        /// Combat animation speed multiplier.
        /// </summary>
        public float AnimationSpeed = 1.0f;

        /// <summary>
        /// Whether to auto-end turn when no playable cards.
        /// </summary>
        public bool AutoEndTurn = false;

        /// <summary>
        /// Language/localization code.
        /// </summary>
        public string LanguageCode = "en";

        /// <summary>
        /// Whether this is the first time launching the game.
        /// </summary>
        public bool FirstLaunch = true;

        /// <summary>
        /// Creates default settings.
        /// </summary>
        public static SettingsData CreateDefault()
        {
            return new SettingsData();
        }
    }
}
