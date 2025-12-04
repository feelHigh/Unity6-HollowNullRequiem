// ============================================
// CorruptionPulseEffect.cs
// Vignette pulse effect for corruption feedback
// ============================================

using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using HNR.Core.Events;

namespace HNR.UI
{
    /// <summary>
    /// Creates a pulsing vignette overlay that intensifies with corruption level.
    /// Provides visual feedback for corruption danger.
    /// </summary>
    public class CorruptionPulseEffect : MonoBehaviour
    {
        // ============================================
        // Overlay Configuration
        // ============================================

        [Header("Overlay")]
        [SerializeField, Tooltip("Vignette image overlay")]
        private Image _vignetteOverlay;

        [SerializeField, Tooltip("Color gradient from low to high corruption")]
        private Gradient _corruptionGradient;

        // ============================================
        // Threshold Settings
        // ============================================

        [Header("Thresholds")]
        [SerializeField, Tooltip("Corruption % to start pulse effect (0-1)")]
        private float _lowThreshold = 0.5f;

        [SerializeField, Tooltip("Corruption % for rapid pulse (0-1)")]
        private float _highThreshold = 0.8f;

        [SerializeField, Tooltip("Corruption % for constant danger state (0-1)")]
        private float _criticalThreshold = 0.95f;

        // ============================================
        // Pulse Settings
        // ============================================

        [Header("Pulse Settings")]
        [SerializeField, Tooltip("Slow pulse duration (seconds per cycle)")]
        private float _slowPulseSpeed = 2f;

        [SerializeField, Tooltip("Fast pulse duration (seconds per cycle)")]
        private float _fastPulseSpeed = 0.5f;

        [SerializeField, Tooltip("Maximum overlay alpha")]
        private float _maxAlpha = 0.3f;

        [SerializeField, Tooltip("Minimum pulse alpha multiplier")]
        private float _minPulseAlpha = 0.5f;

        // ============================================
        // Burst Settings
        // ============================================

        [Header("Burst Settings")]
        [SerializeField, Tooltip("Burst fade in duration")]
        private float _burstInDuration = 0.1f;

        [SerializeField, Tooltip("Burst fade out duration")]
        private float _burstOutDuration = 0.3f;

        // ============================================
        // Runtime State
        // ============================================

        private float _currentCorruptionPercent;
        private Sequence _pulseSequence;
        private bool _isEnabled = true;

        // ============================================
        // Unity Lifecycle
        // ============================================

        private void Awake()
        {
            // Initialize default gradient if not set
            if (_corruptionGradient == null)
            {
                _corruptionGradient = CreateDefaultGradient();
            }

            // Ensure overlay starts invisible
            if (_vignetteOverlay != null)
            {
                var color = _vignetteOverlay.color;
                color.a = 0f;
                _vignetteOverlay.color = color;
            }
        }

        private void OnEnable()
        {
            EventBus.Subscribe<CorruptionChangedEvent>(OnCorruptionChanged);
            EventBus.Subscribe<NullStateEnteredEvent>(OnNullStateEntered);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<CorruptionChangedEvent>(OnCorruptionChanged);
            EventBus.Unsubscribe<NullStateEnteredEvent>(OnNullStateEntered);
            _pulseSequence?.Kill();
        }

        private void OnDestroy()
        {
            _pulseSequence?.Kill();
        }

        // ============================================
        // Event Handlers
        // ============================================

        private void OnCorruptionChanged(CorruptionChangedEvent evt)
        {
            // Convert 0-100 to 0-1
            _currentCorruptionPercent = evt.NewValue / 100f;
            UpdateEffect();

            // Trigger burst on significant gain
            if (evt.Delta >= 10)
            {
                TriggerBurst();
            }
        }

        private void OnNullStateEntered(NullStateEnteredEvent evt)
        {
            // Intense burst when entering Null State
            TriggerNullStateBurst();
        }

        // ============================================
        // Effect Control
        // ============================================

        /// <summary>
        /// Enable or disable the corruption pulse effect.
        /// </summary>
        public void SetEnabled(bool enabled)
        {
            _isEnabled = enabled;

            if (!enabled)
            {
                _pulseSequence?.Kill();
                if (_vignetteOverlay != null)
                    _vignetteOverlay.DOFade(0f, 0.3f);
            }
            else
            {
                UpdateEffect();
            }
        }

        /// <summary>
        /// Manually set corruption level (0-1).
        /// </summary>
        public void SetCorruptionLevel(float percent)
        {
            _currentCorruptionPercent = Mathf.Clamp01(percent);
            UpdateEffect();
        }

        /// <summary>
        /// Trigger a burst flash effect.
        /// </summary>
        public void TriggerBurst()
        {
            if (!_isEnabled || _vignetteOverlay == null) return;

            _pulseSequence?.Pause();

            var seq = DOTween.Sequence();
            seq.Append(_vignetteOverlay.DOFade(_maxAlpha, _burstInDuration));
            seq.Append(_vignetteOverlay.DOFade(GetCurrentTargetAlpha() * _minPulseAlpha, _burstOutDuration));
            seq.OnComplete(() => _pulseSequence?.Play());
        }

        /// <summary>
        /// Trigger intense burst for Null State entry.
        /// </summary>
        public void TriggerNullStateBurst()
        {
            if (!_isEnabled || _vignetteOverlay == null) return;

            _pulseSequence?.Kill();

            // Set to critical color
            _vignetteOverlay.color = _corruptionGradient.Evaluate(1f);

            var seq = DOTween.Sequence();
            seq.Append(_vignetteOverlay.DOFade(0.6f, 0.05f));
            seq.Append(_vignetteOverlay.DOFade(0.2f, 0.1f));
            seq.Append(_vignetteOverlay.DOFade(0.5f, 0.05f));
            seq.Append(_vignetteOverlay.DOFade(_maxAlpha, 0.2f));
            seq.OnComplete(() => UpdateEffect());
        }

        // ============================================
        // Internal
        // ============================================

        private void UpdateEffect()
        {
            if (!_isEnabled || _vignetteOverlay == null) return;

            _pulseSequence?.Kill();

            if (_currentCorruptionPercent < _lowThreshold)
            {
                // Below threshold - fade out
                _vignetteOverlay.DOFade(0f, 0.3f);
                return;
            }

            // Set color based on corruption level
            var color = _corruptionGradient.Evaluate(_currentCorruptionPercent);
            _vignetteOverlay.color = new Color(color.r, color.g, color.b, _vignetteOverlay.color.a);

            // Calculate pulse parameters
            float intensity = Mathf.InverseLerp(_lowThreshold, 1f, _currentCorruptionPercent);
            float targetAlpha = _maxAlpha * intensity;

            // Determine pulse speed based on corruption level
            float pulseSpeed;
            if (_currentCorruptionPercent >= _criticalThreshold)
            {
                pulseSpeed = _fastPulseSpeed * 0.5f; // Even faster at critical
            }
            else if (_currentCorruptionPercent >= _highThreshold)
            {
                pulseSpeed = _fastPulseSpeed;
            }
            else
            {
                pulseSpeed = _slowPulseSpeed;
            }

            // Start pulsing
            _pulseSequence = DOTween.Sequence();
            _pulseSequence.Append(_vignetteOverlay.DOFade(targetAlpha, pulseSpeed / 2f).SetEase(Ease.InOutSine));
            _pulseSequence.Append(_vignetteOverlay.DOFade(targetAlpha * _minPulseAlpha, pulseSpeed / 2f).SetEase(Ease.InOutSine));
            _pulseSequence.SetLoops(-1);
        }

        private float GetCurrentTargetAlpha()
        {
            if (_currentCorruptionPercent < _lowThreshold) return 0f;
            float intensity = Mathf.InverseLerp(_lowThreshold, 1f, _currentCorruptionPercent);
            return _maxAlpha * intensity;
        }

        private Gradient CreateDefaultGradient()
        {
            var gradient = new Gradient();

            // Red to purple corruption gradient
            var colorKeys = new GradientColorKey[]
            {
                new GradientColorKey(new Color(0.55f, 0f, 0f), 0f),      // Dark red at 0%
                new GradientColorKey(new Color(0.7f, 0.1f, 0.2f), 0.5f), // Red at 50%
                new GradientColorKey(new Color(0.5f, 0.1f, 0.5f), 0.8f), // Purple at 80%
                new GradientColorKey(new Color(0.4f, 0f, 0.6f), 1f)      // Deep purple at 100%
            };

            var alphaKeys = new GradientAlphaKey[]
            {
                new GradientAlphaKey(1f, 0f),
                new GradientAlphaKey(1f, 1f)
            };

            gradient.SetKeys(colorKeys, alphaKeys);
            return gradient;
        }
    }
}
