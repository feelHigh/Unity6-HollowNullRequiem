// ============================================
// SettingsOverlay.cs
// Modal overlay for quick volume settings access
// ============================================

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using HNR.Core;
using HNR.Core.Events;
using HNR.Core.Interfaces;
using HNR.Combat;

namespace HNR.UI.Screens
{
    /// <summary>
    /// Modal overlay for volume settings, accessible from any scene.
    /// Does not pause the game - meant for quick volume adjustments.
    /// </summary>
    public class SettingsOverlay : MonoBehaviour
    {
        // ============================================
        // Singleton
        // ============================================

        private static SettingsOverlay _instance;
        public static SettingsOverlay Instance => _instance;

        // ============================================
        // UI References
        // ============================================

        [Header("Container")]
        [SerializeField, Tooltip("Main overlay canvas group")]
        private CanvasGroup _overlay;

        [SerializeField, Tooltip("Dark background panel")]
        private Image _backgroundPanel;

        [SerializeField, Tooltip("Settings content panel")]
        private RectTransform _settingsPanel;

        [Header("Title")]
        [SerializeField, Tooltip("Settings title text")]
        private TMP_Text _titleText;

        [Header("Volume Sliders")]
        [SerializeField, Tooltip("Master volume slider")]
        private Slider _masterVolumeSlider;

        [SerializeField, Tooltip("Master volume label")]
        private TMP_Text _masterVolumeLabel;

        [SerializeField, Tooltip("Music volume slider")]
        private Slider _musicVolumeSlider;

        [SerializeField, Tooltip("Music volume label")]
        private TMP_Text _musicVolumeLabel;

        [SerializeField, Tooltip("SFX volume slider")]
        private Slider _sfxVolumeSlider;

        [SerializeField, Tooltip("SFX volume label")]
        private TMP_Text _sfxVolumeLabel;

        [Header("Buttons")]
        [SerializeField, Tooltip("Close button")]
        private Button _closeButton;

        // ============================================
        // Configuration
        // ============================================

        [Header("Animation")]
        [SerializeField] private float _fadeInDuration = 0.25f;
        [SerializeField] private float _fadeOutDuration = 0.2f;
        [SerializeField] private float _scaleFromValue = 0.85f;

        // ============================================
        // State
        // ============================================

        private bool _isShowing;
        private Tween _currentTween;
        private IAudioManager _audioManager;

        // ============================================
        // Unity Lifecycle
        // ============================================

        private void Awake()
        {
            // Setup singleton
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;

            // Setup initial state
            if (_overlay != null)
            {
                _overlay.alpha = 0f;
                _overlay.interactable = false;
                _overlay.blocksRaycasts = false;
            }

            // Note: Don't call SetActive(false) here - the generator handles that
            // and calling it here would prevent proper initialization
        }

        private void OnEnable()
        {
            // Setup references
            SetupReferences();

            // Setup listeners
            if (_closeButton != null)
                _closeButton.onClick.AddListener(OnCloseClicked);

            if (_masterVolumeSlider != null)
                _masterVolumeSlider.onValueChanged.AddListener(OnMasterVolumeChanged);

            if (_musicVolumeSlider != null)
                _musicVolumeSlider.onValueChanged.AddListener(OnMusicVolumeChanged);

            if (_sfxVolumeSlider != null)
                _sfxVolumeSlider.onValueChanged.AddListener(OnSFXVolumeChanged);

            // Load current settings
            LoadCurrentSettings();
        }

        private void OnDisable()
        {
            // Cleanup listeners
            if (_closeButton != null)
                _closeButton.onClick.RemoveListener(OnCloseClicked);

            if (_masterVolumeSlider != null)
                _masterVolumeSlider.onValueChanged.RemoveListener(OnMasterVolumeChanged);

            if (_musicVolumeSlider != null)
                _musicVolumeSlider.onValueChanged.RemoveListener(OnMusicVolumeChanged);

            if (_sfxVolumeSlider != null)
                _sfxVolumeSlider.onValueChanged.RemoveListener(OnSFXVolumeChanged);
        }

        private void OnDestroy()
        {
            _currentTween?.Kill();

            if (_instance == this)
                _instance = null;
        }

        private void Update()
        {
            // Close on Escape key
            if (_isShowing && Input.GetKeyDown(KeyCode.Escape))
            {
                Hide();
            }
        }

        // ============================================
        // Setup
        // ============================================

        private void SetupReferences()
        {
            ServiceLocator.TryGet(out _audioManager);
        }

        private void LoadCurrentSettings()
        {
            if (_audioManager == null) return;

            // Load values without triggering callbacks
            if (_masterVolumeSlider != null)
            {
                _masterVolumeSlider.SetValueWithoutNotify(_audioManager.MasterVolume);
                UpdateVolumeLabel(_masterVolumeLabel, _audioManager.MasterVolume);
            }

            if (_musicVolumeSlider != null)
            {
                _musicVolumeSlider.SetValueWithoutNotify(_audioManager.MusicVolume);
                UpdateVolumeLabel(_musicVolumeLabel, _audioManager.MusicVolume);
            }

            if (_sfxVolumeSlider != null)
            {
                _sfxVolumeSlider.SetValueWithoutNotify(_audioManager.SFXVolume);
                UpdateVolumeLabel(_sfxVolumeLabel, _audioManager.SFXVolume);
            }
        }

        private void UpdateVolumeLabel(TMP_Text label, float value)
        {
            if (label != null)
            {
                label.text = $"{Mathf.RoundToInt(value * 100)}%";
            }
        }

        // ============================================
        // Public API
        // ============================================

        /// <summary>
        /// Shows the settings overlay.
        /// </summary>
        public void Show()
        {
            if (_isShowing) return;

            // Ensure singleton is set
            if (_instance == null)
            {
                _instance = this;
            }

            _isShowing = true;
            gameObject.SetActive(true);

            // Refresh settings
            SetupReferences();
            LoadCurrentSettings();

            // Animate in
            _currentTween?.Kill();

            if (_overlay != null)
            {
                _overlay.alpha = 0f;
                _overlay.interactable = true;
                _overlay.blocksRaycasts = true;
            }

            if (_settingsPanel != null)
            {
                _settingsPanel.localScale = Vector3.one * _scaleFromValue;
            }

            var sequence = DOTween.Sequence();
            sequence.Append(_overlay.DOFade(1f, _fadeInDuration).SetEase(Ease.OutQuad));

            if (_settingsPanel != null)
            {
                sequence.Join(_settingsPanel.DOScale(1f, _fadeInDuration).SetEase(Ease.OutBack));
            }

            // Use SetUpdate(true) to animate even when Time.timeScale = 0
            sequence.SetUpdate(true);
            sequence.SetLink(gameObject);
            _currentTween = sequence;

            Debug.Log("[SettingsOverlay] Shown");
        }

        /// <summary>
        /// Hides the settings overlay.
        /// </summary>
        public void Hide()
        {
            if (!_isShowing) return;

            // Save settings
            PlayerPrefs.Save();

            _currentTween?.Kill();

            var sequence = DOTween.Sequence();

            if (_settingsPanel != null)
            {
                sequence.Append(_settingsPanel.DOScale(_scaleFromValue, _fadeOutDuration).SetEase(Ease.InQuad));
            }

            sequence.Join(_overlay.DOFade(0f, _fadeOutDuration).SetEase(Ease.InQuad));
            sequence.OnComplete(() =>
            {
                if (_overlay != null)
                {
                    _overlay.interactable = false;
                    _overlay.blocksRaycasts = false;
                }
                gameObject.SetActive(false);
                _isShowing = false;

                // Publish close event for combat pause restoration
                EventBus.Publish(new SettingsClosedEvent());
            });

            // Use SetUpdate(true) to animate even when Time.timeScale = 0
            sequence.SetUpdate(true);
            sequence.SetLink(gameObject);
            _currentTween = sequence;

            Debug.Log("[SettingsOverlay] Hidden");
        }

        /// <summary>
        /// Whether the settings overlay is currently visible.
        /// </summary>
        public bool IsShowing => _isShowing;

        // ============================================
        // Volume Callbacks
        // ============================================

        private void OnMasterVolumeChanged(float value)
        {
            if (_audioManager != null)
            {
                _audioManager.MasterVolume = value;
            }
            else
            {
                Debug.LogWarning("[SettingsOverlay] AudioManager not found - using AudioListener fallback");
            }

            // Fallback: Always set AudioListener.volume for master
            AudioListener.volume = value;
            UpdateVolumeLabel(_masterVolumeLabel, value);
        }

        private void OnMusicVolumeChanged(float value)
        {
            if (_audioManager != null)
            {
                _audioManager.MusicVolume = value;
            }
            else
            {
                Debug.LogWarning("[SettingsOverlay] AudioManager not found for music volume");
            }
            UpdateVolumeLabel(_musicVolumeLabel, value);
        }

        private void OnSFXVolumeChanged(float value)
        {
            if (_audioManager != null)
            {
                _audioManager.SFXVolume = value;
            }
            else
            {
                Debug.LogWarning("[SettingsOverlay] AudioManager not found for SFX volume");
            }
            UpdateVolumeLabel(_sfxVolumeLabel, value);
        }

        // ============================================
        // Button Callbacks
        // ============================================

        private void OnCloseClicked()
        {
            Hide();
        }

        // ============================================
        // Static Helpers
        // ============================================

        /// <summary>
        /// Shows the settings overlay (static helper).
        /// </summary>
        public static void ShowSettings()
        {
            // Try to find instance if not set (e.g., if object was inactive)
            if (_instance == null)
            {
                _instance = FindAnyObjectByType<SettingsOverlay>(FindObjectsInactive.Include);
            }

            if (_instance != null)
            {
                _instance.Show();
            }
            else
            {
                Debug.LogWarning("[SettingsOverlay] Instance not found. Ensure SettingsOverlay exists in scene.");
            }
        }

        /// <summary>
        /// Hides the settings overlay (static helper).
        /// </summary>
        public static void HideSettings()
        {
            if (_instance != null)
            {
                _instance.Hide();
            }
        }

        /// <summary>
        /// Toggles the settings overlay.
        /// </summary>
        public static void Toggle()
        {
            if (_instance == null) return;

            if (_instance.IsShowing)
                _instance.Hide();
            else
                _instance.Show();
        }
    }
}
