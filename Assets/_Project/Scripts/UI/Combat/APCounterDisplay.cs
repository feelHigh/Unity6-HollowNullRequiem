// ============================================
// APCounterDisplay.cs
// Large glowing AP number display above card fan
// ============================================

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using HNR.Core;
using HNR.Core.Events;
using HNR.Combat;

namespace HNR.UI.Combat
{
    /// <summary>
    /// Displays AP as a single glowing number above the card fan.
    /// </summary>
    public class APCounterDisplay : MonoBehaviour
    {
        [Header("Display")]
        [SerializeField] private TMP_Text _apText;
        [SerializeField] private Image _glowBackground;
        [SerializeField] private ParticleSystem _energyParticles;

        [Header("Animation")]
        [SerializeField] private float _pulseScale = 1.15f;
        [SerializeField] private float _pulseDuration = 0.3f;

        [Header("Colors")]
        [SerializeField] private Color _fullColor;
        [SerializeField] private Color _emptyColor;
        [SerializeField] private Color _insufficientColor;

        private int _currentAP;
        private Sequence _glowSequence;

        private void Start()
        {
            _fullColor = UIColors.SoulCyan;
            _emptyColor = UIColors.PanelGray;
            _insufficientColor = UIColors.CorruptionGlow;

            StartGlowAnimation();
            EventBus.Subscribe<APChangedEvent>(OnAPChanged);
            EventBus.Subscribe<CombatStartedEvent>(OnCombatStarted);

            // Initialize with current AP if combat already active
            InitializeFromContext();
        }

        private void OnCombatStarted(CombatStartedEvent evt)
        {
            // Initialize AP display when combat starts
            InitializeFromContext();
        }

        private void InitializeFromContext()
        {
            if (ServiceLocator.TryGet<TurnManager>(out var turnManager) && turnManager.Context != null)
            {
                SetAP(turnManager.Context.CurrentAP, turnManager.Context.MaxAP);
            }
            else
            {
                // Default display before combat starts
                SetAP(3, 3);
            }
        }

        private void OnDestroy()
        {
            EventBus.Unsubscribe<APChangedEvent>(OnAPChanged);
            EventBus.Unsubscribe<CombatStartedEvent>(OnCombatStarted);
            _glowSequence?.Kill();
        }

        private void StartGlowAnimation()
        {
            if (_glowBackground == null) return;

            // Subtle glow pulse - high base alpha with small variation
            _glowSequence = DOTween.Sequence();
            _glowSequence.Append(_glowBackground.DOFade(0.7f, 1.5f).SetEase(Ease.InOutSine));
            _glowSequence.Append(_glowBackground.DOFade(0.9f, 1.5f).SetEase(Ease.InOutSine));
            _glowSequence.SetLoops(-1);
            _glowSequence.SetLink(gameObject);
        }

        private void OnAPChanged(APChangedEvent evt)
        {
            Debug.Log($"[APCounterDisplay] Received APChangedEvent: {evt.CurrentAP}/{evt.MaxAP}");
            SetAP(evt.CurrentAP, evt.MaxAP);
        }

        /// <summary>
        /// Sets the AP display value and updates visuals.
        /// </summary>
        /// <param name="current">Current AP available.</param>
        /// <param name="max">Maximum AP for this turn.</param>
        public void SetAP(int current, int max)
        {
            _currentAP = current;

            if (_apText != null)
            {
                _apText.text = current.ToString();
                _apText.color = current > 0 ? _fullColor : _emptyColor;
                Debug.Log($"[APCounterDisplay] Updated text to: {current}");
            }
            else
            {
                Debug.LogWarning("[APCounterDisplay] _apText is NULL - cannot update display!");
            }

            if (_glowBackground != null)
            {
                _glowBackground.color = current > 0 ? _fullColor : _emptyColor;
            }

            if (current > 0)
            {
                AnimatePulse();
            }

            UpdateParticles(current);
        }

        /// <summary>
        /// Animates AP being spent with scale-down effect.
        /// </summary>
        /// <param name="newValue">New AP value after spending.</param>
        public void AnimateAPSpent(int newValue)
        {
            if (_apText == null) return;

            _apText.transform.DOScale(0.8f, _pulseDuration / 2)
                .OnComplete(() => {
                    _apText.text = newValue.ToString();
                    _apText.transform.DOScale(1f, _pulseDuration / 2).SetLink(gameObject);
                })
                .SetLink(gameObject);

            _currentAP = newValue;
            _apText.color = newValue > 0 ? _fullColor : _emptyColor;
        }

        /// <summary>
        /// Animates insufficient AP feedback with shake and red flash.
        /// </summary>
        public void AnimateInsufficientAP()
        {
            if (_apText == null) return;

            _apText.color = _insufficientColor;
            _apText.transform.DOShakePosition(0.3f, 5f).SetLink(gameObject);

            DOVirtual.DelayedCall(0.5f, () => {
                if (_apText != null)
                {
                    _apText.color = _currentAP > 0 ? _fullColor : _emptyColor;
                }
            }).SetLink(gameObject);
        }

        private void AnimatePulse()
        {
            if (_apText == null) return;

            _apText.transform.DOScale(_pulseScale, _pulseDuration / 2)
                .OnComplete(() => _apText.transform.DOScale(1f, _pulseDuration / 2).SetLink(gameObject))
                .SetLink(gameObject);
        }

        private void UpdateParticles(int current)
        {
            if (_energyParticles == null) return;

            if (current > 0 && !_energyParticles.isPlaying)
            {
                _energyParticles.Play();
            }
            else if (current == 0 && _energyParticles.isPlaying)
            {
                _energyParticles.Stop();
            }
        }
    }
}
