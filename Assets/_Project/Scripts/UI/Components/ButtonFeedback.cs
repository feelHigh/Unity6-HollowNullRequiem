// ============================================
// ButtonFeedback.cs
// Consistent button interaction feedback
// ============================================

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DG.Tweening;
using HNR.Core;
using HNR.Core.Interfaces;

namespace HNR.UI
{
    /// <summary>
    /// Haptic feedback intensity levels.
    /// </summary>
    public enum HapticIntensity
    {
        Light,
        Medium,
        Heavy
    }

    /// <summary>
    /// Provides visual, audio, and haptic feedback for button interactions.
    /// Attach to any UI button for consistent interaction feel.
    /// </summary>
    public class ButtonFeedback : MonoBehaviour,
        IPointerDownHandler, IPointerUpHandler,
        IPointerEnterHandler, IPointerExitHandler,
        IPointerClickHandler
    {
        // ============================================
        // Scale Settings
        // ============================================

        [Header("Scale Feedback")]
        [SerializeField, Tooltip("Scale when pressed")]
        private float _pressScale = 0.95f;

        [SerializeField, Tooltip("Scale when hovered")]
        private float _hoverScale = 1.05f;

        [SerializeField, Tooltip("Animation duration")]
        private float _duration = 0.1f;

        [SerializeField, Tooltip("Punch scale on click")]
        private float _clickPunchScale = 0.1f;

        // ============================================
        // Color Settings
        // ============================================

        [Header("Color Feedback")]
        [SerializeField, Tooltip("Apply color feedback")]
        private bool _useColorFeedback = true;

        [SerializeField, Tooltip("Target graphic for color feedback")]
        private Graphic _targetGraphic;

        [SerializeField, Tooltip("Brightness multiplier when pressed")]
        private float _pressBrightness = 0.8f;

        [SerializeField, Tooltip("Brightness multiplier when hovered")]
        private float _hoverBrightness = 1.1f;

        // ============================================
        // Audio Settings
        // ============================================

        [Header("Audio Feedback")]
        [SerializeField, Tooltip("Play sound effects")]
        private bool _playSound = true;

        [SerializeField, Tooltip("Sound ID for click")]
        private string _clickSoundId = "ui_click";

        [SerializeField, Tooltip("Sound ID for hover")]
        private string _hoverSoundId = "ui_hover";

        // ============================================
        // Haptic Settings
        // ============================================

        [Header("Haptic Feedback")]
        [SerializeField, Tooltip("Enable haptic feedback")]
        private bool _playHaptic = true;

        [SerializeField, Tooltip("Haptic intensity on click")]
        private HapticIntensity _hapticIntensity = HapticIntensity.Light;

        // ============================================
        // Runtime State
        // ============================================

        private Vector3 _originalScale;
        private Color _originalColor;
        private Button _button;
        private bool _isPressed;
        private bool _isHovered;
        private Tweener _scaleTween;
        private Tweener _colorTween;

        // ============================================
        // Unity Lifecycle
        // ============================================

        private void Awake()
        {
            _originalScale = transform.localScale;
            _button = GetComponent<Button>();

            if (_targetGraphic == null)
                _targetGraphic = GetComponent<Graphic>();

            if (_targetGraphic != null)
                _originalColor = _targetGraphic.color;
        }

        private void OnDisable()
        {
            // Reset to original state
            _scaleTween?.Kill();
            _colorTween?.Kill();
            transform.localScale = _originalScale;

            if (_targetGraphic != null)
                _targetGraphic.color = _originalColor;

            _isPressed = false;
            _isHovered = false;
        }

        // ============================================
        // Pointer Events
        // ============================================

        public void OnPointerDown(PointerEventData eventData)
        {
            if (!IsInteractable()) return;

            _isPressed = true;
            AnimateScale(_pressScale);

            if (_useColorFeedback)
                AnimateColor(_pressBrightness);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (!_isPressed) return;
            _isPressed = false;

            // Return to hover or normal state
            float targetScale = _isHovered ? _hoverScale : 1f;
            AnimateScale(targetScale);

            if (_useColorFeedback)
            {
                float targetBrightness = _isHovered ? _hoverBrightness : 1f;
                AnimateColor(targetBrightness);
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (!IsInteractable()) return;

            // Punch scale effect
            _scaleTween?.Kill();
            transform.DOPunchScale(Vector3.one * _clickPunchScale, _duration * 2f, 1, 0.5f);

            // Audio feedback
            if (_playSound)
            {
                if (ServiceLocator.TryGet<IAudioManager>(out var audioManager))
                {
                    audioManager.PlaySFX(_clickSoundId);
                }
            }

            // Haptic feedback
            if (_playHaptic)
            {
                TriggerHaptic(_hapticIntensity);
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (!IsInteractable()) return;

            _isHovered = true;

            if (!_isPressed)
            {
                AnimateScale(_hoverScale);

                if (_useColorFeedback)
                    AnimateColor(_hoverBrightness);
            }

            // Hover sound (subtle)
            if (_playSound && !string.IsNullOrEmpty(_hoverSoundId))
            {
                if (ServiceLocator.TryGet<IAudioManager>(out var audioManager))
                {
                    audioManager.PlaySFX(_hoverSoundId);
                }
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            _isHovered = false;

            if (!_isPressed)
            {
                AnimateScale(1f);

                if (_useColorFeedback)
                    AnimateColor(1f);
            }
        }

        // ============================================
        // Animation Helpers
        // ============================================

        private void AnimateScale(float targetMultiplier)
        {
            _scaleTween?.Kill();
            _scaleTween = transform
                .DOScale(_originalScale * targetMultiplier, _duration)
                .SetEase(Ease.OutQuad)
                .SetUpdate(true); // Ignore time scale
        }

        private void AnimateColor(float brightnessMultiplier)
        {
            if (_targetGraphic == null) return;

            _colorTween?.Kill();

            Color targetColor = new Color(
                _originalColor.r * brightnessMultiplier,
                _originalColor.g * brightnessMultiplier,
                _originalColor.b * brightnessMultiplier,
                _originalColor.a
            );

            _colorTween = _targetGraphic
                .DOColor(targetColor, _duration)
                .SetEase(Ease.OutQuad)
                .SetUpdate(true);
        }

        private void TriggerHaptic(HapticIntensity intensity)
        {
#if UNITY_ANDROID || UNITY_IOS
            // Use Unity's Handheld vibration as fallback
            switch (intensity)
            {
                case HapticIntensity.Light:
                    Handheld.Vibrate(); // Basic vibration
                    break;
                case HapticIntensity.Medium:
                    Handheld.Vibrate();
                    break;
                case HapticIntensity.Heavy:
                    Handheld.Vibrate();
                    break;
            }
#endif
        }

        // ============================================
        // Utility
        // ============================================

        private bool IsInteractable()
        {
            return _button == null || _button.interactable;
        }

        /// <summary>
        /// Manually trigger click feedback (for programmatic clicks).
        /// </summary>
        public void TriggerClickFeedback()
        {
            if (!IsInteractable()) return;

            transform.DOPunchScale(Vector3.one * _clickPunchScale, _duration * 2f, 1, 0.5f);

            if (_playSound && ServiceLocator.TryGet<IAudioManager>(out var audioManager))
                audioManager.PlaySFX(_clickSoundId);

            if (_playHaptic)
                TriggerHaptic(_hapticIntensity);
        }

        /// <summary>
        /// Set whether this button should play sounds.
        /// </summary>
        public void SetSoundEnabled(bool enabled)
        {
            _playSound = enabled;
        }

        /// <summary>
        /// Set whether this button should play haptics.
        /// </summary>
        public void SetHapticEnabled(bool enabled)
        {
            _playHaptic = enabled;
        }
    }
}
