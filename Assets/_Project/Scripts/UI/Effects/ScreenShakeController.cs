// ============================================
// ScreenShakeController.cs
// UI screen shake effects for combat feedback
// ============================================

using UnityEngine;
using DG.Tweening;
using HNR.Core;
using HNR.Core.Events;

namespace HNR.UI
{
    /// <summary>
    /// Screen shake intensity levels.
    /// </summary>
    public enum ShakeIntensity
    {
        Light,
        Medium,
        Heavy
    }

    /// <summary>
    /// Controls screen shake effects for combat feedback.
    /// Registered with ServiceLocator for global access.
    /// </summary>
    public class ScreenShakeController : MonoBehaviour
    {
        // ============================================
        // Target Configuration
        // ============================================

        [Header("Target")]
        [SerializeField, Tooltip("RectTransform to shake (usually main canvas content)")]
        private RectTransform _shakeTarget;

        // ============================================
        // Light Shake Settings
        // ============================================

        [Header("Light Shake")]
        [SerializeField, Tooltip("Magnitude in pixels")]
        private float _lightMagnitude = 3f;

        [SerializeField, Tooltip("Duration in seconds")]
        private float _lightDuration = 0.15f;

        // ============================================
        // Medium Shake Settings
        // ============================================

        [Header("Medium Shake")]
        [SerializeField, Tooltip("Magnitude in pixels")]
        private float _mediumMagnitude = 6f;

        [SerializeField, Tooltip("Duration in seconds")]
        private float _mediumDuration = 0.25f;

        // ============================================
        // Heavy Shake Settings
        // ============================================

        [Header("Heavy Shake")]
        [SerializeField, Tooltip("Magnitude in pixels")]
        private float _heavyMagnitude = 12f;

        [SerializeField, Tooltip("Duration in seconds")]
        private float _heavyDuration = 0.4f;

        // ============================================
        // Shake Settings
        // ============================================

        [Header("Shake Settings")]
        [SerializeField, Tooltip("Number of shake vibrations")]
        private int _vibrato = 20;

        [SerializeField, Tooltip("Randomness of shake direction (0-180)")]
        private float _randomness = 90f;

        [SerializeField, Tooltip("If true, shake snaps to integer positions")]
        private bool _snapping = false;

        [SerializeField, Tooltip("If true, shake fades out over duration")]
        private bool _fadeOut = true;

        // ============================================
        // Runtime State
        // ============================================

        private Vector2 _originalPosition;
        private Tweener _currentShake;
        private bool _isEnabled = true;

        // ============================================
        // Unity Lifecycle
        // ============================================

        private void Awake()
        {
            if (_shakeTarget == null)
                _shakeTarget = GetComponent<RectTransform>();

            if (_shakeTarget != null)
                _originalPosition = _shakeTarget.anchoredPosition;

            ServiceLocator.Register(this);
        }

        private void OnDestroy()
        {
            _currentShake?.Kill();
            ServiceLocator.Unregister<ScreenShakeController>();
        }

        private void OnEnable()
        {
            // Subscribe to combat events for automatic shake
            EventBus.Subscribe<DamageDealtEvent>(OnDamageDealt);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<DamageDealtEvent>(OnDamageDealt);
        }

        // ============================================
        // Event Handlers
        // ============================================

        private void OnDamageDealt(DamageDealtEvent evt)
        {
            // Shake based on damage amount
            if (evt.Amount >= 20)
                Shake(ShakeIntensity.Heavy);
            else if (evt.Amount >= 10)
                Shake(ShakeIntensity.Medium);
            else if (evt.Amount >= 5)
                Shake(ShakeIntensity.Light);
        }

        // ============================================
        // Public API
        // ============================================

        /// <summary>
        /// Enable or disable screen shake (for settings menu).
        /// </summary>
        /// <param name="enabled">Whether shake is enabled.</param>
        public void SetEnabled(bool enabled)
        {
            _isEnabled = enabled;

            if (!enabled)
                StopShake();
        }

        /// <summary>
        /// Whether screen shake is currently enabled.
        /// </summary>
        public bool IsEnabled => _isEnabled;

        /// <summary>
        /// Trigger a screen shake with preset intensity.
        /// </summary>
        /// <param name="intensity">Shake intensity level.</param>
        public void Shake(ShakeIntensity intensity)
        {
            if (!_isEnabled || _shakeTarget == null) return;

            var (magnitude, duration) = intensity switch
            {
                ShakeIntensity.Light => (_lightMagnitude, _lightDuration),
                ShakeIntensity.Medium => (_mediumMagnitude, _mediumDuration),
                ShakeIntensity.Heavy => (_heavyMagnitude, _heavyDuration),
                _ => (_lightMagnitude, _lightDuration)
            };

            DoShake(magnitude, duration);
        }

        /// <summary>
        /// Trigger a screen shake with custom parameters.
        /// </summary>
        /// <param name="magnitude">Shake magnitude in pixels.</param>
        /// <param name="duration">Shake duration in seconds.</param>
        public void ShakeCustom(float magnitude, float duration)
        {
            if (!_isEnabled || _shakeTarget == null) return;
            DoShake(magnitude, duration);
        }

        /// <summary>
        /// Immediately stop any active shake.
        /// </summary>
        public void StopShake()
        {
            _currentShake?.Kill();
            if (_shakeTarget != null)
                _shakeTarget.anchoredPosition = _originalPosition;
        }

        // ============================================
        // Internal
        // ============================================

        private void DoShake(float magnitude, float duration)
        {
            // Kill existing shake
            _currentShake?.Kill();

            // Reset to original position before starting new shake
            _shakeTarget.anchoredPosition = _originalPosition;

            // Create shake tween
            _currentShake = _shakeTarget.DOShakeAnchorPos(
                duration,
                magnitude,
                _vibrato,
                _randomness,
                _snapping,
                _fadeOut
            ).OnComplete(() =>
            {
                // Ensure we return to exact original position
                _shakeTarget.anchoredPosition = _originalPosition;
            });
        }
    }
}
