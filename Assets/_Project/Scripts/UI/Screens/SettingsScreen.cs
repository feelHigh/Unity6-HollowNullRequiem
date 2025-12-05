// ============================================
// SettingsScreen.cs
// Player settings and options screen
// ============================================

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using HNR.Core;
using HNR.Core.Interfaces;
using HNR.Audio;

namespace HNR.UI
{
    /// <summary>
    /// Settings screen for player options including audio, gameplay, and graphics.
    /// </summary>
    public class SettingsScreen : MonoBehaviour
    {
        // ============================================
        // Serialized Fields
        // ============================================

        [Header("Audio")]
        [SerializeField] private Slider _masterVolumeSlider;
        [SerializeField] private Slider _musicVolumeSlider;
        [SerializeField] private Slider _sfxVolumeSlider;
        [SerializeField] private Toggle _musicMuteToggle;
        [SerializeField] private Toggle _sfxMuteToggle;

        [Header("Gameplay")]
        [SerializeField] private Toggle _hapticsToggle;
        [SerializeField] private Toggle _screenShakeToggle;
        [SerializeField] private Toggle _tutorialsToggle;

        [Header("Graphics")]
        [SerializeField] private TMP_Dropdown _qualityDropdown;

        [Header("Data")]
        [SerializeField] private Button _deleteSaveButton;
        [SerializeField] private Button _resetTutorialsButton;

        [Header("Navigation")]
        [SerializeField] private Button _backButton;
        [SerializeField] private Button _creditsButton;

        [Header("Info")]
        [SerializeField] private VersionDisplay _versionDisplay;

        // ============================================
        // Private State
        // ============================================

        private IAudioManager _audioManager;
        private HapticController _hapticController;
        private QualitySettingsManager _qualityManager;
        private TutorialTooltipManager _tutorialManager;

        // ============================================
        // Unity Lifecycle
        // ============================================

        private void Awake()
        {
            SetupListeners();
        }

        private void OnEnable()
        {
            SetupReferences();
            LoadCurrentSettings();
        }

        // ============================================
        // Setup Methods
        // ============================================

        private void SetupReferences()
        {
            ServiceLocator.TryGet(out _audioManager);
            ServiceLocator.TryGet(out _hapticController);
            ServiceLocator.TryGet(out _qualityManager);
            ServiceLocator.TryGet(out _tutorialManager);
        }

        private void SetupListeners()
        {
            // Audio
            _masterVolumeSlider?.onValueChanged.AddListener(OnMasterVolumeChanged);
            _musicVolumeSlider?.onValueChanged.AddListener(OnMusicVolumeChanged);
            _sfxVolumeSlider?.onValueChanged.AddListener(OnSFXVolumeChanged);
            _musicMuteToggle?.onValueChanged.AddListener(OnMusicMuteChanged);
            _sfxMuteToggle?.onValueChanged.AddListener(OnSFXMuteChanged);

            // Gameplay
            _hapticsToggle?.onValueChanged.AddListener(OnHapticsChanged);
            _screenShakeToggle?.onValueChanged.AddListener(OnScreenShakeChanged);
            _tutorialsToggle?.onValueChanged.AddListener(OnTutorialsChanged);

            // Graphics
            _qualityDropdown?.onValueChanged.AddListener(OnQualityChanged);

            // Buttons
            _deleteSaveButton?.onClick.AddListener(OnDeleteSaveClicked);
            _resetTutorialsButton?.onClick.AddListener(OnResetTutorialsClicked);
            _backButton?.onClick.AddListener(OnBackClicked);
            _creditsButton?.onClick.AddListener(OnCreditsClicked);
        }

        private void LoadCurrentSettings()
        {
            // Audio
            if (_audioManager != null)
            {
                if (_masterVolumeSlider != null)
                    _masterVolumeSlider.SetValueWithoutNotify(_audioManager.MasterVolume);
                if (_musicVolumeSlider != null)
                    _musicVolumeSlider.SetValueWithoutNotify(_audioManager.MusicVolume);
                if (_sfxVolumeSlider != null)
                    _sfxVolumeSlider.SetValueWithoutNotify(_audioManager.SFXVolume);
                if (_musicMuteToggle != null)
                    _musicMuteToggle.SetIsOnWithoutNotify(!_audioManager.IsMusicMuted);
                if (_sfxMuteToggle != null)
                    _sfxMuteToggle.SetIsOnWithoutNotify(!_audioManager.IsSFXMuted);
            }

            // Haptics
            if (_hapticController != null && _hapticsToggle != null)
                _hapticsToggle.SetIsOnWithoutNotify(_hapticController.HapticsEnabled);

            // Screen shake
            if (_screenShakeToggle != null)
                _screenShakeToggle.SetIsOnWithoutNotify(PlayerPrefs.GetInt("ScreenShakeEnabled", 1) == 1);

            // Tutorials
            if (_tutorialManager != null && _tutorialsToggle != null)
                _tutorialsToggle.SetIsOnWithoutNotify(_tutorialManager.TutorialsEnabled);

            // Quality
            if (_qualityManager != null && _qualityDropdown != null)
                _qualityDropdown.SetValueWithoutNotify((int)_qualityManager.CurrentTier);
        }

        // ============================================
        // Audio Callbacks
        // ============================================

        private void OnMasterVolumeChanged(float value)
        {
            if (_audioManager != null)
                _audioManager.MasterVolume = value;
        }

        private void OnMusicVolumeChanged(float value)
        {
            if (_audioManager != null)
                _audioManager.MusicVolume = value;
        }

        private void OnSFXVolumeChanged(float value)
        {
            if (_audioManager != null)
                _audioManager.SFXVolume = value;
        }

        private void OnMusicMuteChanged(bool enabled)
        {
            _audioManager?.MuteMusic(!enabled);
        }

        private void OnSFXMuteChanged(bool enabled)
        {
            _audioManager?.MuteSFX(!enabled);
        }

        // ============================================
        // Gameplay Callbacks
        // ============================================

        private void OnHapticsChanged(bool enabled)
        {
            if (_hapticController != null)
                _hapticController.HapticsEnabled = enabled;
        }

        private void OnScreenShakeChanged(bool enabled)
        {
            PlayerPrefs.SetInt("ScreenShakeEnabled", enabled ? 1 : 0);

            var feedbackIntegrator = FindAnyObjectByType<CombatFeedbackIntegrator>();
            feedbackIntegrator?.SetScreenShakeEnabled(enabled);
        }

        private void OnTutorialsChanged(bool enabled)
        {
            if (_tutorialManager != null)
                _tutorialManager.TutorialsEnabled = enabled;
        }

        // ============================================
        // Graphics Callbacks
        // ============================================

        private void OnQualityChanged(int index)
        {
            if (_qualityManager != null)
                _qualityManager.SetQualityTier((QualityTier)index);
        }

        // ============================================
        // Button Callbacks
        // ============================================

        private void OnDeleteSaveClicked()
        {
            Debug.Log("[SettingsScreen] Delete save requested");

            if (ServiceLocator.TryGet<ISaveManager>(out var saveManager))
            {
                saveManager.DeleteRun();
            }

            _audioManager?.PlaySFX("ui_confirm");
        }

        private void OnResetTutorialsClicked()
        {
            _tutorialManager?.ResetAllTooltips();
            _audioManager?.PlaySFX("ui_confirm");
            Debug.Log("[SettingsScreen] Tutorials reset");
        }

        private void OnBackClicked()
        {
            SaveSettings();
            gameObject.SetActive(false);
        }

        private void OnCreditsClicked()
        {
            var credits = FindAnyObjectByType<CreditsScreen>(FindObjectsInactive.Include);
            credits?.Show();
        }

        // ============================================
        // Public Methods
        // ============================================

        /// <summary>
        /// Show settings screen.
        /// </summary>
        public void Show()
        {
            gameObject.SetActive(true);
        }

        /// <summary>
        /// Hide settings screen.
        /// </summary>
        public void Hide()
        {
            SaveSettings();
            gameObject.SetActive(false);
        }

        /// <summary>
        /// Save all settings to PlayerPrefs.
        /// </summary>
        public void SaveSettings()
        {
            PlayerPrefs.Save();
            Debug.Log("[SettingsScreen] Settings saved");
        }

        /// <summary>
        /// Reset all settings to defaults.
        /// </summary>
        public void ResetToDefaults()
        {
            // Audio
            if (_audioManager != null)
            {
                _audioManager.MasterVolume = 1f;
                _audioManager.MusicVolume = 0.7f;
                _audioManager.SFXVolume = 1f;
                _audioManager.MuteMusic(false);
                _audioManager.MuteSFX(false);
            }

            // Gameplay
            if (_hapticController != null)
                _hapticController.HapticsEnabled = true;

            PlayerPrefs.SetInt("ScreenShakeEnabled", 1);

            if (_tutorialManager != null)
                _tutorialManager.TutorialsEnabled = true;

            // Quality
            _qualityManager?.ResetToAuto();

            LoadCurrentSettings();
            SaveSettings();

            Debug.Log("[SettingsScreen] Reset to defaults");
        }
    }
}
