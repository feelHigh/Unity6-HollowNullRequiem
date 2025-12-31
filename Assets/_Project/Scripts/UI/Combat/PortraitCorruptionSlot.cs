// ============================================
// PortraitCorruptionSlot.cs
// Combined portrait with corruption bar display
// ============================================

using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using HNR.Core.Events;
using HNR.Characters;

namespace HNR.UI.Combat
{
    /// <summary>
    /// Displays a Requiem portrait with a horizontal corruption bar below.
    /// Used in SharedVitalityBar to show per-character corruption levels.
    /// </summary>
    public class PortraitCorruptionSlot : MonoBehaviour
    {
        [Header("Portrait")]
        [SerializeField] private Image _portrait;
        [SerializeField] private Image _portraitFrame;

        [Header("Corruption Bar")]
        [SerializeField] private Image _corruptionFill;
        [SerializeField] private Image _corruptionBackground;

        [Header("Animation")]
        [SerializeField] private float _fillSpeed = 5f;
        [SerializeField] private bool _smoothTransition = true;

        private RequiemInstance _requiem;
        private Gradient _corruptionGradient;
        private float _targetFill;
        private float _currentFill;
        private bool _isInNullState;

        private void Awake()
        {
            _corruptionGradient = CreateCorruptionGradient();
        }

        private void OnDestroy()
        {
            if (_requiem != null)
            {
                EventBus.Unsubscribe<CorruptionChangedEvent>(OnCorruptionChanged);
                EventBus.Unsubscribe<NullStateEnteredEvent>(OnNullStateEntered);
                EventBus.Unsubscribe<NullStateExitedEvent>(OnNullStateExited);
            }
        }

        /// <summary>
        /// Initialize the slot with a Requiem instance.
        /// </summary>
        /// <param name="requiem">The Requiem to track.</param>
        public void Initialize(RequiemInstance requiem)
        {
            _requiem = requiem;

            // Set portrait sprite
            if (_portrait != null && requiem.Data?.Portrait != null)
            {
                _portrait.sprite = requiem.Data.Portrait;
            }

            // Set frame color by aspect
            if (_portraitFrame != null && requiem.Data != null)
            {
                _portraitFrame.color = UIColors.GetAspectColor(requiem.Data.SoulAspect);
            }

            // Initialize corruption display
            _currentFill = requiem.Corruption / 100f;
            _targetFill = _currentFill;
            _isInNullState = requiem.InNullState;
            UpdateCorruptionVisual(_currentFill);

            // Subscribe to events
            EventBus.Subscribe<CorruptionChangedEvent>(OnCorruptionChanged);
            EventBus.Subscribe<NullStateEnteredEvent>(OnNullStateEntered);
            EventBus.Subscribe<NullStateExitedEvent>(OnNullStateExited);

            Debug.Log($"[PortraitCorruptionSlot] Initialized for {requiem.Name}, corruption: {requiem.Corruption}");
        }

        private void Update()
        {
            // Smooth fill transition
            if (_smoothTransition && Mathf.Abs(_currentFill - _targetFill) > 0.001f)
            {
                _currentFill = Mathf.Lerp(_currentFill, _targetFill, Time.deltaTime * _fillSpeed);
                UpdateCorruptionVisual(_currentFill);
            }

            // Null State pulse animation
            if (_isInNullState && _corruptionFill != null)
            {
                float pulse = (Mathf.Sin(Time.time * 3f) + 1f) * 0.5f;
                _corruptionFill.color = Color.Lerp(
                    new Color(0.6f, 0f, 0.6f),  // Base purple
                    new Color(1f, 0f, 1f),      // Bright purple pulse
                    pulse
                );
            }
        }

        private void OnCorruptionChanged(CorruptionChangedEvent evt)
        {
            if (evt.Requiem != _requiem) return;

            _targetFill = evt.NewValue / 100f;

            if (!_smoothTransition)
            {
                _currentFill = _targetFill;
                UpdateCorruptionVisual(_currentFill);
            }

            // Trigger visual feedback on corruption gain
            if (evt.Delta > 0)
            {
                TriggerCorruptionGainFeedback();
            }
        }

        private void OnNullStateEntered(NullStateEnteredEvent evt)
        {
            if (evt.Requiem != _requiem) return;

            _isInNullState = true;
            TriggerNullStateFeedback();
        }

        private void OnNullStateExited(NullStateExitedEvent evt)
        {
            if (evt.Requiem != _requiem) return;

            _isInNullState = false;
            UpdateCorruptionVisual(_currentFill);
        }

        private void UpdateCorruptionVisual(float normalizedValue)
        {
            if (_corruptionFill != null)
            {
                _corruptionFill.fillAmount = normalizedValue;

                // Only update color if not in Null State (Null State has pulse animation)
                if (!_isInNullState)
                {
                    _corruptionFill.color = _corruptionGradient.Evaluate(normalizedValue);
                }
            }
        }

        private void TriggerCorruptionGainFeedback()
        {
            // Flash the corruption bar
            if (_corruptionFill != null)
            {
                _corruptionFill.transform.DOPunchScale(Vector3.one * 0.1f, 0.2f, 2).SetLink(gameObject);
            }

            // Shake portrait slightly
            if (_portrait != null)
            {
                _portrait.transform.DOShakePosition(0.2f, 2f).SetLink(gameObject);
            }
        }

        private void TriggerNullStateFeedback()
        {
            // Flash bright purple
            if (_corruptionFill != null)
            {
                var seq = DOTween.Sequence();
                seq.Append(_corruptionFill.DOColor(new Color(1f, 0f, 1f), 0.1f));
                seq.Append(_corruptionFill.DOColor(new Color(0.6f, 0f, 0.6f), 0.1f));
                seq.SetLoops(3);
                seq.SetLink(gameObject);
            }

            // Pulse portrait frame
            if (_portraitFrame != null)
            {
                var originalColor = _portraitFrame.color;
                var seq = DOTween.Sequence();
                seq.Append(_portraitFrame.DOColor(new Color(1f, 0f, 1f), 0.15f));
                seq.Append(_portraitFrame.DOColor(originalColor, 0.15f));
                seq.SetLoops(2);
                seq.SetLink(gameObject);
            }
        }

        /// <summary>
        /// Creates the corruption gradient from safe (green) to null state (purple).
        /// </summary>
        private Gradient CreateCorruptionGradient()
        {
            var gradient = new Gradient();

            var colorKeys = new GradientColorKey[]
            {
                new GradientColorKey(new Color(0.2f, 0.8f, 0.2f), 0f),     // Safe (0%) - Green
                new GradientColorKey(new Color(0.8f, 0.8f, 0.2f), 0.25f),  // Uneasy (25%) - Yellow
                new GradientColorKey(new Color(0.8f, 0.5f, 0.2f), 0.5f),   // Strained (50%) - Orange
                new GradientColorKey(new Color(0.8f, 0.2f, 0.2f), 0.75f),  // Critical (75%) - Red
                new GradientColorKey(new Color(0.6f, 0f, 0.6f), 1f)        // Null State (100%) - Purple
            };

            var alphaKeys = new GradientAlphaKey[]
            {
                new GradientAlphaKey(1f, 0f),
                new GradientAlphaKey(1f, 1f)
            };

            gradient.SetKeys(colorKeys, alphaKeys);
            return gradient;
        }

        /// <summary>
        /// Gets the tracked Requiem instance.
        /// </summary>
        public RequiemInstance TrackedRequiem => _requiem;

        /// <summary>
        /// Force refresh the display from current Requiem state.
        /// </summary>
        public void ForceRefresh()
        {
            if (_requiem != null)
            {
                _currentFill = _requiem.Corruption / 100f;
                _targetFill = _currentFill;
                _isInNullState = _requiem.InNullState;
                UpdateCorruptionVisual(_currentFill);
            }
        }
    }
}
