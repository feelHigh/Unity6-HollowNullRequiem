// ============================================
// NullStateModal.cs
// Corruption overlay modal when Requiem enters Null State
// ============================================

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using HNR.Core;
using HNR.Core.Events;
using HNR.Characters;

namespace HNR.UI
{
    /// <summary>
    /// Modal overlay displayed when a Requiem reaches 100% corruption.
    /// Shows Null State effects and requires player confirmation to continue.
    /// </summary>
    public class NullStateModal : MonoBehaviour
    {
        // ============================================
        // Visual Effects
        // ============================================

        [Header("Visual Effects")]
        [SerializeField, Tooltip("Main overlay canvas group")]
        private CanvasGroup _overlay;

        [SerializeField, Tooltip("Dark background panel")]
        private Image _backgroundPanel;

        [SerializeField, Tooltip("Corruption vignette effect around edges")]
        private Image _corruptionVignette;

        [SerializeField, Tooltip("Corruption particles")]
        private ParticleSystem _corruptionParticles;

        [SerializeField, Tooltip("Pulsing glow effect")]
        private Image _pulseGlow;

        // ============================================
        // Requiem Display
        // ============================================

        [Header("Requiem Display")]
        [SerializeField, Tooltip("Requiem portrait image")]
        private Image _requiemPortrait;

        [SerializeField, Tooltip("Portrait frame/border")]
        private Image _portraitFrame;

        [SerializeField, Tooltip("Requiem name text")]
        private TMP_Text _requiemNameText;

        [SerializeField, Tooltip("Corruption percentage text")]
        private TMP_Text _corruptionText;

        [SerializeField, Tooltip("Soul Aspect icon")]
        private Image _aspectIcon;

        // ============================================
        // Info Display
        // ============================================

        [Header("Info Display")]
        [SerializeField, Tooltip("NULL STATE title")]
        private TMP_Text _titleText;

        [SerializeField, Tooltip("Effects description text")]
        private TMP_Text _effectsText;

        [SerializeField, Tooltip("Requiem Art name text")]
        private TMP_Text _artNameText;

        [SerializeField, Tooltip("Disclaimer/reset info text")]
        private TMP_Text _disclaimerText;

        // ============================================
        // Action Button
        // ============================================

        [Header("Action")]
        [SerializeField, Tooltip("Unleash button to confirm and continue")]
        private Button _unleashButton;

        [SerializeField, Tooltip("Unleash button text")]
        private TMP_Text _unleashButtonText;

        [SerializeField, Tooltip("Glow effect around button")]
        private Image _buttonGlow;

        // ============================================
        // Configuration
        // ============================================

        [Header("Configuration")]
        [SerializeField, Tooltip("HP penalty percentage when entering Null State")]
        private float _hpPenalty = 0.33f;

        [SerializeField, Tooltip("Damage bonus percentage in Null State")]
        private float _damageBonus = 0.50f;

        [SerializeField, Tooltip("Corruption reset value after combat")]
        private int _corruptionResetValue = 50;

        // ============================================
        // Animation
        // ============================================

        [Header("Animation")]
        [SerializeField] private float _fadeInDuration = 0.5f;
        [SerializeField] private float _pulseSpeed = 1f;
        [SerializeField] private Color _nullStateColor = new Color(0.77f, 0.12f, 0.23f);
        [SerializeField] private Color _vignetteColor = new Color(0.55f, 0f, 0.1f, 0.8f);

        // ============================================
        // State
        // ============================================

        private RequiemInstance _corruptedRequiem;
        private bool _isShowing;
        private Sequence _pulseSequence;
        private System.Action _onUnleash;

        // ============================================
        // Unity Lifecycle
        // ============================================

        private void Awake()
        {
            // Start hidden
            if (_overlay != null)
            {
                _overlay.alpha = 0f;
                _overlay.interactable = false;
                _overlay.blocksRaycasts = false;
            }

            gameObject.SetActive(false);
        }

        private void OnDestroy()
        {
            _pulseSequence?.Kill();
        }

        // ============================================
        // Public API
        // ============================================

        /// <summary>
        /// Show the Null State modal for a corrupted Requiem.
        /// </summary>
        /// <param name="requiem">The Requiem that entered Null State.</param>
        /// <param name="onUnleash">Callback when player confirms.</param>
        public void Show(RequiemInstance requiem, System.Action onUnleash = null)
        {
            if (_isShowing) return;

            _corruptedRequiem = requiem;
            _onUnleash = onUnleash;
            _isShowing = true;

            gameObject.SetActive(true);
            UpdateDisplay();
            SetupButton();
            PlayShowAnimation();

            // Pause game
            Time.timeScale = 0f;

            Debug.Log($"[NullStateModal] Showing Null State for {requiem?.Data?.RequiemName ?? "Unknown"}");
        }

        /// <summary>
        /// Hide the modal.
        /// </summary>
        public void Hide()
        {
            if (!_isShowing) return;

            _isShowing = false;
            PlayHideAnimation();

            // Resume game
            Time.timeScale = 1f;

            Debug.Log("[NullStateModal] Null State modal hidden");
        }

        /// <summary>
        /// Check if the modal is currently showing.
        /// </summary>
        public bool IsShowing => _isShowing;

        // ============================================
        // Display Updates
        // ============================================

        private void UpdateDisplay()
        {
            var data = _corruptedRequiem?.Data;

            // Title
            if (_titleText != null)
            {
                _titleText.text = "NULL STATE";
                _titleText.color = _nullStateColor;
            }

            // Requiem name
            if (_requiemNameText != null)
            {
                _requiemNameText.text = data?.RequiemName ?? "Unknown Requiem";
            }

            // Portrait
            if (_requiemPortrait != null)
            {
                if (data?.Portrait != null)
                {
                    _requiemPortrait.sprite = data.Portrait;
                    _requiemPortrait.color = Color.white;
                }
                else
                {
                    _requiemPortrait.sprite = null;
                    _requiemPortrait.color = data?.AspectColor ?? _nullStateColor;
                }
            }

            // Portrait frame color
            if (_portraitFrame != null)
            {
                _portraitFrame.color = _nullStateColor;
            }

            // Corruption text
            if (_corruptionText != null)
            {
                _corruptionText.text = "100%";
                _corruptionText.color = _nullStateColor;
            }

            // Aspect icon
            if (_aspectIcon != null && data != null)
            {
                _aspectIcon.color = data.AspectColor;
            }

            // Effects description
            if (_effectsText != null)
            {
                int hpPenaltyPercent = Mathf.RoundToInt(_hpPenalty * 100);
                int damageBonusPercent = Mathf.RoundToInt(_damageBonus * 100);

                _effectsText.text = $"-{hpPenaltyPercent}% Current HP\n+{damageBonusPercent}% Damage";
            }

            // Requiem Art name
            if (_artNameText != null)
            {
                string artName = data?.RequiemArt?.ArtName ?? "Null Awakening";
                _artNameText.text = $"REQUIEM ART: {artName}";
            }

            // Disclaimer
            if (_disclaimerText != null)
            {
                _disclaimerText.text = $"Corruption resets to {_corruptionResetValue}% after combat";
            }

            // Vignette color
            if (_corruptionVignette != null)
            {
                _corruptionVignette.color = _vignetteColor;
            }

            // Button text
            if (_unleashButtonText != null)
            {
                _unleashButtonText.text = "UNLEASH";
            }

            // Button glow
            if (_buttonGlow != null)
            {
                _buttonGlow.color = _nullStateColor;
            }
        }

        private void SetupButton()
        {
            if (_unleashButton != null)
            {
                _unleashButton.onClick.RemoveAllListeners();
                _unleashButton.onClick.AddListener(OnUnleashClicked);
            }
        }

        // ============================================
        // Button Handler
        // ============================================

        private void OnUnleashClicked()
        {
            Debug.Log($"[NullStateModal] Unleash clicked for {_corruptedRequiem?.Data?.RequiemName}");

            // Apply Null State effects
            ApplyNullStateEffects();

            // Invoke callback
            _onUnleash?.Invoke();

            // Publish event
            EventBus.Publish(new NullStateActivatedEvent(_corruptedRequiem));

            // Hide modal
            Hide();
        }

        private void ApplyNullStateEffects()
        {
            if (_corruptedRequiem == null) return;

            // Apply HP penalty
            int currentHP = _corruptedRequiem.CurrentHP;
            int hpLoss = Mathf.RoundToInt(currentHP * _hpPenalty);
            _corruptedRequiem.TakeDamage(hpLoss);

            Debug.Log($"[NullStateModal] Applied HP penalty: -{hpLoss} HP (was {currentHP})");

            // Null State is now active on the RequiemInstance
            // The damage bonus is handled by the combat system when calculating damage
        }

        // ============================================
        // Animation
        // ============================================

        private void PlayShowAnimation()
        {
            if (_overlay == null) return;

            // Enable interaction
            _overlay.interactable = true;
            _overlay.blocksRaycasts = true;

            // Fade in overlay
            _overlay.alpha = 0f;
            _overlay.DOFade(1f, _fadeInDuration).SetUpdate(true);

            // Title scale animation
            if (_titleText != null)
            {
                _titleText.transform.localScale = Vector3.one * 2f;
                _titleText.transform.DOScale(1f, _fadeInDuration * 1.5f)
                    .SetEase(Ease.OutBack)
                    .SetUpdate(true);
            }

            // Portrait shake
            if (_requiemPortrait != null)
            {
                _requiemPortrait.transform.DOShakePosition(_fadeInDuration, 10f, 20, 90f)
                    .SetUpdate(true);
            }

            // Start vignette pulse
            StartVignettePulse();

            // Start particles
            if (_corruptionParticles != null)
            {
                _corruptionParticles.Play();
            }

            // Button glow pulse
            StartButtonGlowPulse();
        }

        private void PlayHideAnimation()
        {
            if (_overlay == null)
            {
                gameObject.SetActive(false);
                return;
            }

            // Stop pulses
            _pulseSequence?.Kill();

            // Stop particles
            if (_corruptionParticles != null)
            {
                _corruptionParticles.Stop();
            }

            // Fade out
            _overlay.DOFade(0f, _fadeInDuration * 0.5f)
                .SetUpdate(true)
                .OnComplete(() =>
                {
                    _overlay.interactable = false;
                    _overlay.blocksRaycasts = false;
                    gameObject.SetActive(false);
                });
        }

        private void StartVignettePulse()
        {
            if (_corruptionVignette == null) return;

            _pulseSequence?.Kill();
            _pulseSequence = DOTween.Sequence();

            Color startColor = _vignetteColor;
            Color pulseColor = new Color(startColor.r, startColor.g, startColor.b, startColor.a * 1.5f);

            _pulseSequence.Append(_corruptionVignette.DOColor(pulseColor, _pulseSpeed));
            _pulseSequence.Append(_corruptionVignette.DOColor(startColor, _pulseSpeed));
            _pulseSequence.SetLoops(-1);
            _pulseSequence.SetUpdate(true);
        }

        private void StartButtonGlowPulse()
        {
            if (_buttonGlow == null) return;

            Color startColor = _nullStateColor;
            startColor.a = 0.5f;
            Color endColor = _nullStateColor;
            endColor.a = 1f;

            _buttonGlow.color = startColor;
            _buttonGlow.DOColor(endColor, _pulseSpeed * 0.5f)
                .SetLoops(-1, LoopType.Yoyo)
                .SetUpdate(true);
        }

        // ============================================
        // Static Helper
        // ============================================

        private static NullStateModal _instance;

        /// <summary>
        /// Gets or creates the singleton instance.
        /// </summary>
        public static NullStateModal Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindFirstObjectByType<NullStateModal>();

                    if (_instance == null)
                    {
                        Debug.LogWarning("[NullStateModal] No NullStateModal found in scene");
                    }
                }
                return _instance;
            }
        }

        /// <summary>
        /// Static method to show the Null State modal.
        /// </summary>
        /// <param name="requiem">The corrupted Requiem.</param>
        /// <param name="onUnleash">Callback when confirmed.</param>
        public static void ShowNullState(RequiemInstance requiem, System.Action onUnleash = null)
        {
            var modal = Instance;
            if (modal != null)
            {
                modal.Show(requiem, onUnleash);
            }
            else
            {
                Debug.LogError("[NullStateModal] Cannot show - no instance found");
                // Still invoke callback to not block combat
                onUnleash?.Invoke();
            }
        }
    }

    // ============================================
    // Supporting Events
    // ============================================

    /// <summary>
    /// Event fired when Null State is activated for a Requiem.
    /// </summary>
    public class NullStateActivatedEvent : GameEvent
    {
        public RequiemInstance Requiem { get; }

        public NullStateActivatedEvent(RequiemInstance requiem)
        {
            Requiem = requiem;
        }
    }
}
