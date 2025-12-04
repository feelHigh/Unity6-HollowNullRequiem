// ============================================
// DamageNumberUI.cs
// Poolable floating damage/heal number display
// ============================================

using System;
using UnityEngine;
using TMPro;
using DG.Tweening;
using HNR.Core.Interfaces;

namespace HNR.UI
{
    /// <summary>
    /// Type of damage number to display.
    /// </summary>
    public enum DamageNumberType
    {
        Damage,
        Heal,
        Block,
        Corruption
    }

    /// <summary>
    /// Poolable floating damage number component.
    /// Animates pop-in, float up, and fade out.
    /// </summary>
    [RequireComponent(typeof(RectTransform), typeof(CanvasGroup))]
    public class DamageNumberUI : MonoBehaviour, IPoolable
    {
        // ============================================
        // References
        // ============================================

        [Header("References")]
        [SerializeField, Tooltip("Text component for number display")]
        private TMP_Text _numberText;

        [SerializeField, Tooltip("Canvas group for fading")]
        private CanvasGroup _canvasGroup;

        // ============================================
        // Animation Settings
        // ============================================

        [Header("Animation")]
        [SerializeField, Tooltip("Total animation lifetime")]
        private float _lifetime = 0.8f;

        [SerializeField, Tooltip("Distance to float upward")]
        private float _floatDistance = 50f;

        [SerializeField, Tooltip("Delay before starting fade")]
        private float _fadeDelay = 0.5f;

        // ============================================
        // Scale Settings
        // ============================================

        [Header("Scaling")]
        [SerializeField, Tooltip("Peak scale during pop animation")]
        private float _popScale = 1.3f;

        [SerializeField, Tooltip("Scale multiplier for critical hits")]
        private float _criticalScale = 1.5f;

        [SerializeField, Tooltip("Duration of pop-in animation")]
        private float _popInDuration = 0.1f;

        [SerializeField, Tooltip("Duration of pop-out animation")]
        private float _popOutDuration = 0.1f;

        // ============================================
        // Runtime State
        // ============================================

        private RectTransform _rectTransform;
        private Sequence _animSequence;
        private Action<DamageNumberUI> _onComplete;
        private Vector2 _startPosition;

        // ============================================
        // Unity Lifecycle
        // ============================================

        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
            if (_canvasGroup == null)
                _canvasGroup = GetComponent<CanvasGroup>();
        }

        private void OnDestroy()
        {
            _animSequence?.Kill();
        }

        // ============================================
        // IPoolable Implementation
        // ============================================

        /// <summary>
        /// Called when spawned from pool. Reset to initial state.
        /// </summary>
        public void OnSpawnFromPool()
        {
            _canvasGroup.alpha = 1f;
            transform.localScale = Vector3.zero;
            gameObject.SetActive(true);
        }

        /// <summary>
        /// Called when returned to pool. Clean up animations.
        /// </summary>
        public void OnReturnToPool()
        {
            _animSequence?.Kill();
            _animSequence = null;
            _onComplete = null;
            gameObject.SetActive(false);
        }

        // ============================================
        // Public Methods
        // ============================================

        /// <summary>
        /// Display a damage number with animation.
        /// </summary>
        /// <param name="value">Numeric value to display.</param>
        /// <param name="type">Type of number (damage, heal, etc.).</param>
        /// <param name="isCritical">Whether this is a critical hit.</param>
        /// <param name="onComplete">Callback when animation completes (for pool return).</param>
        public void Show(int value, DamageNumberType type, bool isCritical, Action<DamageNumberUI> onComplete)
        {
            _onComplete = onComplete;
            _startPosition = _rectTransform.anchoredPosition;

            // Format text
            string prefix = isCritical ? "CRIT!\n" : "";
            string sign = GetSignForType(type);
            _numberText.text = $"{prefix}{sign}{Mathf.Abs(value)}";
            _numberText.color = GetColorForType(type);

            // Calculate scale
            float scale = isCritical ? _criticalScale : 1f;
            Animate(scale);
        }

        /// <summary>
        /// Display a damage number at a specific position.
        /// </summary>
        /// <param name="position">World position to display at.</param>
        /// <param name="value">Numeric value to display.</param>
        /// <param name="type">Type of number.</param>
        /// <param name="isCritical">Whether this is a critical hit.</param>
        /// <param name="onComplete">Callback when animation completes.</param>
        public void ShowAtPosition(Vector3 position, int value, DamageNumberType type, bool isCritical, Action<DamageNumberUI> onComplete)
        {
            _rectTransform.position = position;
            Show(value, type, isCritical, onComplete);
        }

        /// <summary>
        /// Display blocked damage with shield icon prefix.
        /// </summary>
        /// <param name="blockedAmount">Amount of damage blocked.</param>
        /// <param name="onComplete">Callback when animation completes.</param>
        public void ShowBlocked(int blockedAmount, Action<DamageNumberUI> onComplete)
        {
            _onComplete = onComplete;
            _startPosition = _rectTransform.anchoredPosition;

            _numberText.text = $"<sprite=0> {blockedAmount}"; // Assumes shield sprite in TMP
            _numberText.color = GetColorForType(DamageNumberType.Block);

            Animate(1f);
        }

        // ============================================
        // Animation
        // ============================================

        private void Animate(float targetScale)
        {
            _animSequence?.Kill();
            _animSequence = DOTween.Sequence();

            // Pop in: scale from 0 to popScale
            _animSequence.Append(transform.DOScale(_popScale * targetScale, _popInDuration)
                .SetEase(Ease.OutBack));

            // Pop out: scale down to target
            _animSequence.Append(transform.DOScale(targetScale, _popOutDuration)
                .SetEase(Ease.InOutQuad));

            // Float up: move upward over lifetime
            _animSequence.Join(_rectTransform.DOAnchorPosY(_startPosition.y + _floatDistance, _lifetime)
                .SetEase(Ease.OutQuad));

            // Fade out: start after delay, complete by end of lifetime
            float fadeDuration = _lifetime - _fadeDelay;
            _animSequence.Insert(_fadeDelay, _canvasGroup.DOFade(0f, fadeDuration)
                .SetEase(Ease.InQuad));

            // Complete callback for pool return
            _animSequence.OnComplete(() => _onComplete?.Invoke(this));
        }

        // ============================================
        // Helpers
        // ============================================

        private string GetSignForType(DamageNumberType type) => type switch
        {
            DamageNumberType.Damage => "-",
            DamageNumberType.Heal => "+",
            DamageNumberType.Block => "",
            DamageNumberType.Corruption => "+",
            _ => ""
        };

        private Color GetColorForType(DamageNumberType type) => type switch
        {
            DamageNumberType.Damage => UIColors.CorruptionGlow,
            DamageNumberType.Heal => new Color32(0, 255, 100, 255), // Bright green
            DamageNumberType.Block => UIColors.SoulCyan,
            DamageNumberType.Corruption => UIColors.HollowViolet,
            _ => Color.white
        };
    }
}
