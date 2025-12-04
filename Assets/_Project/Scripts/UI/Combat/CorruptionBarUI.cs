// ============================================
// CorruptionBarUI.cs
// UI component for displaying Requiem corruption
// ============================================

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using HNR.Core;
using HNR.Core.Events;
using HNR.Characters;

namespace HNR.UI
{
    /// <summary>
    /// Displays corruption level for a single Requiem.
    /// Shows gradient fill 0-100 and Null State indicator.
    /// </summary>
    public class CorruptionBarUI : MonoBehaviour
    {
        // ============================================
        // References
        // ============================================

        [Header("UI References")]
        [SerializeField, Tooltip("Slider component for corruption bar")]
        private Slider _corruptionSlider;

        [SerializeField, Tooltip("Fill image for color gradient")]
        private Image _fillImage;

        [SerializeField, Tooltip("Text showing corruption value")]
        private TextMeshProUGUI _valueText;

        [SerializeField, Tooltip("Indicator shown when in Null State")]
        private GameObject _nullStateIndicator;

        [SerializeField, Tooltip("Requiem portrait image")]
        private Image _requiemPortrait;

        [SerializeField, Tooltip("Corruption state tier label")]
        private TextMeshProUGUI _stateLabel;

        // ============================================
        // Colors
        // ============================================

        [Header("Colors")]
        [SerializeField, Tooltip("Gradient from safe (green) to critical (red)")]
        private Gradient _corruptionGradient;

        [SerializeField, Tooltip("Color when in Null State")]
        private Color _nullStateColor = new Color(0.8f, 0f, 0.8f);

        [SerializeField, Tooltip("Pulse color for Null State")]
        private Color _nullStatePulseColor = new Color(1f, 0f, 1f);

        // ============================================
        // Animation
        // ============================================

        [Header("Animation")]
        [SerializeField, Tooltip("Speed of Null State pulse animation")]
        private float _pulseSpeed = 2f;

        [SerializeField, Tooltip("Enable smooth value transitions")]
        private bool _smoothTransition = true;

        [SerializeField, Tooltip("Speed of smooth value change")]
        private float _transitionSpeed = 5f;

        // ============================================
        // State
        // ============================================

        private RequiemInstance _requiem;
        private float _targetValue;
        private float _currentDisplayValue;
        private bool _isInNullState;

        // ============================================
        // Initialization
        // ============================================

        private void Awake()
        {
            // Set up default gradient if not configured
            if (_corruptionGradient == null || _corruptionGradient.colorKeys.Length == 0)
            {
                _corruptionGradient = CreateDefaultGradient();
            }
        }

        /// <summary>
        /// Initialize the corruption bar for a specific Requiem.
        /// </summary>
        /// <param name="requiem">The RequiemInstance to track</param>
        public void Initialize(RequiemInstance requiem)
        {
            _requiem = requiem;

            // Set portrait
            if (_requiemPortrait != null && requiem.Data?.Portrait != null)
            {
                _requiemPortrait.sprite = requiem.Data.Portrait;
            }

            // Subscribe to events
            EventBus.Subscribe<CorruptionChangedEvent>(OnCorruptionChanged);
            EventBus.Subscribe<NullStateEnteredEvent>(OnNullStateEntered);
            EventBus.Subscribe<NullStateExitedEvent>(OnNullStateExited);

            // Set initial state
            _currentDisplayValue = requiem.Corruption;
            _targetValue = requiem.Corruption;
            _isInNullState = requiem.InNullState;

            UpdateDisplay(requiem.Corruption);
            SetNullStateVisual(_isInNullState);

            Debug.Log($"[CorruptionBarUI] Initialized for {requiem.Name}");
        }

        private void OnDestroy()
        {
            EventBus.Unsubscribe<CorruptionChangedEvent>(OnCorruptionChanged);
            EventBus.Unsubscribe<NullStateEnteredEvent>(OnNullStateEntered);
            EventBus.Unsubscribe<NullStateExitedEvent>(OnNullStateExited);
        }

        // ============================================
        // Update Loop
        // ============================================

        private void Update()
        {
            // Smooth value transition
            if (_smoothTransition && Mathf.Abs(_currentDisplayValue - _targetValue) > 0.01f)
            {
                _currentDisplayValue = Mathf.Lerp(_currentDisplayValue, _targetValue, Time.deltaTime * _transitionSpeed);
                UpdateSliderValue(_currentDisplayValue);
            }

            // Null State pulse animation
            if (_isInNullState && _fillImage != null)
            {
                float pulse = (Mathf.Sin(Time.time * _pulseSpeed) + 1f) * 0.5f;
                _fillImage.color = Color.Lerp(_nullStateColor, _nullStatePulseColor, pulse);
            }
        }

        // ============================================
        // Event Handlers
        // ============================================

        private void OnCorruptionChanged(CorruptionChangedEvent evt)
        {
            if (evt.Requiem != _requiem) return;

            _targetValue = evt.NewValue;

            if (!_smoothTransition)
            {
                _currentDisplayValue = evt.NewValue;
            }

            UpdateDisplay(evt.NewValue);
        }

        private void OnNullStateEntered(NullStateEnteredEvent evt)
        {
            if (evt.Requiem != _requiem) return;

            _isInNullState = true;
            SetNullStateVisual(true);
        }

        private void OnNullStateExited(NullStateExitedEvent evt)
        {
            if (evt.Requiem != _requiem) return;

            _isInNullState = false;
            SetNullStateVisual(false);
        }

        // ============================================
        // Display Updates
        // ============================================

        private void UpdateDisplay(int corruption)
        {
            float normalized = corruption / 100f;

            UpdateSliderValue(normalized * 100f);

            // Update value text
            if (_valueText != null)
            {
                _valueText.text = $"{corruption}/100";
            }

            // Update color if not in Null State
            if (!_isInNullState && _fillImage != null)
            {
                _fillImage.color = _corruptionGradient.Evaluate(normalized);
            }

            // Update state label
            if (_stateLabel != null)
            {
                _stateLabel.text = GetStateLabel(corruption);
            }
        }

        private void UpdateSliderValue(float value)
        {
            if (_corruptionSlider != null)
            {
                _corruptionSlider.value = value / 100f;
            }
        }

        private void SetNullStateVisual(bool active)
        {
            _isInNullState = active;

            // Show/hide Null State indicator
            if (_nullStateIndicator != null)
            {
                _nullStateIndicator.SetActive(active);
            }

            // Set Null State color
            if (_fillImage != null)
            {
                _fillImage.color = active ? _nullStateColor : _corruptionGradient.Evaluate(_currentDisplayValue / 100f);
            }

            // Update state label
            if (_stateLabel != null)
            {
                _stateLabel.text = active ? "NULL STATE" : GetStateLabel((int)_currentDisplayValue);
            }
        }

        // ============================================
        // Utility
        // ============================================

        /// <summary>
        /// Get the corruption state label based on value.
        /// </summary>
        private string GetStateLabel(int corruption)
        {
            if (corruption >= 100) return "NULL STATE";
            if (corruption >= 75) return "CRITICAL";
            if (corruption >= 50) return "STRAINED";
            if (corruption >= 25) return "UNEASY";
            return "SAFE";
        }

        /// <summary>
        /// Create default gradient (green -> yellow -> red -> purple).
        /// </summary>
        private Gradient CreateDefaultGradient()
        {
            var gradient = new Gradient();

            var colorKeys = new GradientColorKey[]
            {
                new GradientColorKey(new Color(0.2f, 0.8f, 0.2f), 0f),     // Safe - Green
                new GradientColorKey(new Color(0.8f, 0.8f, 0.2f), 0.25f),  // Uneasy - Yellow
                new GradientColorKey(new Color(0.8f, 0.5f, 0.2f), 0.5f),   // Strained - Orange
                new GradientColorKey(new Color(0.8f, 0.2f, 0.2f), 0.75f),  // Critical - Red
                new GradientColorKey(new Color(0.6f, 0f, 0.6f), 1f)        // Null State - Purple
            };

            var alphaKeys = new GradientAlphaKey[]
            {
                new GradientAlphaKey(1f, 0f),
                new GradientAlphaKey(1f, 1f)
            };

            gradient.SetKeys(colorKeys, alphaKeys);
            return gradient;
        }

        // ============================================
        // Public Methods
        // ============================================

        /// <summary>
        /// Force update the display (useful for testing).
        /// </summary>
        public void ForceUpdate()
        {
            if (_requiem != null)
            {
                _targetValue = _requiem.Corruption;
                _currentDisplayValue = _requiem.Corruption;
                UpdateDisplay(_requiem.Corruption);
                SetNullStateVisual(_requiem.InNullState);
            }
        }

        /// <summary>
        /// Get the currently tracked Requiem.
        /// </summary>
        public RequiemInstance TrackedRequiem => _requiem;
    }
}
