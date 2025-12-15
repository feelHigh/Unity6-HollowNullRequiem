// ============================================
// CurrencyTicker.cs
// Animated currency display with number lerp
// ============================================

using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace HNR.UI.Components
{
    /// <summary>
    /// Animated currency display that lerps between values.
    /// Used in GlobalHeader for Soul Crystals, Void Dust, Aether Stamina.
    /// </summary>
    public class CurrencyTicker : MonoBehaviour
    {
        [Header("Display")]
        [SerializeField] private Image _icon;
        [SerializeField] private TMP_Text _valueText;

        [Header("Animation")]
        [SerializeField] private float _animationSpeed = 5f;
        [SerializeField] private float _punchScale = 1.1f;
        [SerializeField] private float _punchDuration = 0.2f;

        [Header("Colors")]
        [SerializeField] private Color _normalColor = Color.white;
        [SerializeField] private Color _increaseColor = Color.green;
        [SerializeField] private Color _decreaseColor = Color.red;

        private int _displayedValue;
        private int _targetValue;
        private Coroutine _lerpCoroutine;
        private Coroutine _colorCoroutine;

        // ============================================
        // Public API
        // ============================================

        /// <summary>
        /// Sets the value immediately without animation.
        /// </summary>
        /// <param name="value">Value to display.</param>
        public void SetValueImmediate(int value)
        {
            _displayedValue = value;
            _targetValue = value;
            UpdateDisplay();
        }

        /// <summary>
        /// Animates to a new value with lerp effect.
        /// </summary>
        /// <param name="newValue">Target value to animate to.</param>
        public void AnimateToValue(int newValue)
        {
            bool isIncrease = newValue > _targetValue;
            _targetValue = newValue;

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

        /// <summary>
        /// Gets the current displayed value.
        /// </summary>
        public int DisplayedValue => _displayedValue;

        /// <summary>
        /// Gets the target value being animated to.
        /// </summary>
        public int TargetValue => _targetValue;

        /// <summary>
        /// Sets the icon sprite.
        /// </summary>
        /// <param name="sprite">Icon sprite.</param>
        public void SetIcon(Sprite sprite)
        {
            if (_icon != null)
            {
                _icon.sprite = sprite;
            }
        }

        // ============================================
        // Animation Coroutines
        // ============================================

        private IEnumerator LerpValue()
        {
            while (_displayedValue != _targetValue)
            {
                float delta = (_targetValue - _displayedValue) * _animationSpeed * Time.unscaledDeltaTime;

                // Ensure we make progress even with small differences
                if (Mathf.Abs(delta) < 1f)
                {
                    delta = Mathf.Sign(_targetValue - _displayedValue);
                }

                _displayedValue += Mathf.RoundToInt(delta);

                // Clamp to target to avoid overshooting
                if ((_targetValue > _displayedValue && _displayedValue > _targetValue) ||
                    (_targetValue < _displayedValue && _displayedValue < _targetValue))
                {
                    _displayedValue = _targetValue;
                }

                // Final snap when close
                if (Mathf.Abs(_targetValue - _displayedValue) <= 1)
                {
                    _displayedValue = _targetValue;
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
                _valueText.text = FormatValue(_displayedValue);
            }
        }

        private string FormatValue(int value)
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
    }
}
