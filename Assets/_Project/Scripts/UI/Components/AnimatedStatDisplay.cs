// ============================================
// AnimatedStatDisplay.cs
// Animated stat display with number lerp and dual value support
// ============================================

using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace HNR.UI.Components
{
    /// <summary>
    /// Animated stat display that lerps between values.
    /// Supports single value mode (e.g., "123") or dual value mode (e.g., "45/60").
    /// Used in MapScreen for HP and currency displays.
    /// </summary>
    public class AnimatedStatDisplay : MonoBehaviour
    {
        // ============================================
        // Display Mode
        // ============================================

        public enum DisplayMode
        {
            SingleValue,    // "123"
            DualValue       // "45/60" (current/max)
        }

        [Header("Mode")]
        [SerializeField] private DisplayMode _displayMode = DisplayMode.SingleValue;

        [Header("Display")]
        [SerializeField] private Image _icon;
        [SerializeField] private TMP_Text _valueText;

        [Header("Animation")]
        [SerializeField] private float _animationSpeed = 5f;
        [SerializeField] private float _punchScale = 1.1f;
        [SerializeField] private float _punchDuration = 0.2f;

        [Header("Colors")]
        [SerializeField] private Color _normalColor = Color.white;
        [SerializeField] private Color _increaseColor = new Color(0.18f, 0.8f, 0.44f); // Green
        [SerializeField] private Color _decreaseColor = new Color(1f, 0.25f, 0.21f);   // Red

        // ============================================
        // State
        // ============================================

        private int _displayedCurrent;
        private int _targetCurrent;
        private int _displayedMax;
        private int _targetMax;

        private Coroutine _lerpCoroutine;
        private Coroutine _colorCoroutine;

        // ============================================
        // Unity Lifecycle
        // ============================================

        private void Awake()
        {
            // Initialize display to show 0 instead of placeholder text
            UpdateDisplay();
        }

        // ============================================
        // Public API
        // ============================================

        /// <summary>
        /// Sets the value immediately without animation (single value mode).
        /// </summary>
        public void SetValueImmediate(int value)
        {
            _displayedCurrent = value;
            _targetCurrent = value;
            UpdateDisplay();
        }

        /// <summary>
        /// Sets values immediately without animation (dual value mode).
        /// </summary>
        public void SetValueImmediate(int current, int max)
        {
            _displayedCurrent = current;
            _targetCurrent = current;
            _displayedMax = max;
            _targetMax = max;
            UpdateDisplay();
        }

        /// <summary>
        /// Animates to a new value with lerp effect (single value mode).
        /// </summary>
        public void AnimateToValue(int newValue)
        {
            bool isIncrease = newValue > _targetCurrent;
            _targetCurrent = newValue;

            StartAnimation(isIncrease);
        }

        /// <summary>
        /// Animates to new values with lerp effect (dual value mode).
        /// </summary>
        public void AnimateToValue(int newCurrent, int newMax)
        {
            bool isIncrease = newCurrent > _targetCurrent;
            _targetCurrent = newCurrent;
            _targetMax = newMax;

            StartAnimation(isIncrease);
        }

        /// <summary>
        /// Gets the current displayed value.
        /// </summary>
        public int DisplayedValue => _displayedCurrent;

        /// <summary>
        /// Gets the current displayed max value.
        /// </summary>
        public int DisplayedMax => _displayedMax;

        /// <summary>
        /// Gets the target value being animated to.
        /// </summary>
        public int TargetValue => _targetCurrent;

        /// <summary>
        /// Sets the icon sprite.
        /// </summary>
        public void SetIcon(Sprite sprite)
        {
            if (_icon != null)
            {
                _icon.sprite = sprite;
            }
        }

        /// <summary>
        /// Sets the display mode.
        /// </summary>
        public void SetDisplayMode(DisplayMode mode)
        {
            _displayMode = mode;
            UpdateDisplay();
        }

        // ============================================
        // Animation
        // ============================================

        private void StartAnimation(bool isIncrease)
        {
            if (_lerpCoroutine != null)
            {
                StopCoroutine(_lerpCoroutine);
            }
            _lerpCoroutine = StartCoroutine(LerpValue());

            // Show color feedback
            if (_colorCoroutine != null)
            {
                StopCoroutine(_colorCoroutine);
            }
            _colorCoroutine = StartCoroutine(FlashColor(isIncrease ? _increaseColor : _decreaseColor));

            // Punch scale on change
            if (_valueText != null)
            {
                StartCoroutine(PunchScale());
            }
        }

        private IEnumerator LerpValue()
        {
            while (_displayedCurrent != _targetCurrent || _displayedMax != _targetMax)
            {
                // Lerp current value
                if (_displayedCurrent != _targetCurrent)
                {
                    float delta = (_targetCurrent - _displayedCurrent) * _animationSpeed * Time.unscaledDeltaTime;

                    // Ensure we make progress even with small differences
                    if (Mathf.Abs(delta) < 1f)
                    {
                        delta = Mathf.Sign(_targetCurrent - _displayedCurrent);
                    }

                    _displayedCurrent += Mathf.RoundToInt(delta);

                    // Final snap when close
                    if (Mathf.Abs(_targetCurrent - _displayedCurrent) <= 1)
                    {
                        _displayedCurrent = _targetCurrent;
                    }
                }

                // Lerp max value (for dual mode)
                if (_displayedMax != _targetMax)
                {
                    float delta = (_targetMax - _displayedMax) * _animationSpeed * Time.unscaledDeltaTime;

                    if (Mathf.Abs(delta) < 1f)
                    {
                        delta = Mathf.Sign(_targetMax - _displayedMax);
                    }

                    _displayedMax += Mathf.RoundToInt(delta);

                    if (Mathf.Abs(_targetMax - _displayedMax) <= 1)
                    {
                        _displayedMax = _targetMax;
                    }
                }

                UpdateDisplay();
                yield return null;
            }

            _lerpCoroutine = null;
        }

        private IEnumerator FlashColor(Color flashColor)
        {
            if (_valueText == null) yield break;

            _valueText.color = flashColor;
            float elapsed = 0f;
            float duration = 0.5f;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = elapsed / duration;
                _valueText.color = Color.Lerp(flashColor, _normalColor, t);
                yield return null;
            }

            _valueText.color = _normalColor;
            _colorCoroutine = null;
        }

        private IEnumerator PunchScale()
        {
            if (_valueText == null) yield break;

            Transform t = _valueText.transform;
            Vector3 originalScale = Vector3.one;
            Vector3 punchScale = originalScale * _punchScale;

            // Scale up
            float elapsed = 0f;
            float halfDuration = _punchDuration / 2f;

            while (elapsed < halfDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float progress = elapsed / halfDuration;
                t.localScale = Vector3.Lerp(originalScale, punchScale, progress);
                yield return null;
            }

            // Scale down
            elapsed = 0f;
            while (elapsed < halfDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float progress = elapsed / halfDuration;
                t.localScale = Vector3.Lerp(punchScale, originalScale, progress);
                yield return null;
            }

            t.localScale = originalScale;
        }

        // ============================================
        // Display Update
        // ============================================

        private void UpdateDisplay()
        {
            if (_valueText != null)
            {
                _valueText.text = _displayMode == DisplayMode.DualValue
                    ? FormatDualValue(_displayedCurrent, _displayedMax)
                    : FormatSingleValue(_displayedCurrent);
            }
        }

        private string FormatSingleValue(int value)
        {
            // Format large numbers with K/M suffixes
            if (value >= 1000000)
            {
                return $"{value / 1000000f:F1}M";
            }
            if (value >= 10000)
            {
                return $"{value / 1000f:F1}K";
            }
            return value.ToString("N0");
        }

        private string FormatDualValue(int current, int max)
        {
            return $"{current}/{max}";
        }
    }
}
