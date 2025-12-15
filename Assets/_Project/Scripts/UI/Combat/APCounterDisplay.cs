// ============================================
// APCounterDisplay.cs
// Large glowing AP number display above card fan
// Replaces 3-orb AP indicator with CZN-style counter
// ============================================

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using HNR.Core.Events;
using HNR.Combat;

namespace HNR.UI.Combat
{
    /// <summary>
    /// Displays AP as a single glowing number above the card fan.
    /// Replaces the 3-orb AP indicator with CZN-style counter.
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
        }

        private void OnDestroy()
        {
            EventBus.Unsubscribe<APChangedEvent>(OnAPChanged);
            _glowSequence?.Kill();
        }

        private void StartGlowAnimation()
        {
            if (_glowBackground == null) return;

            _glowSequence = DOTween.Sequence();
            _glowSequence.Append(_glowBackground.DOFade(0.3f, 1f));
            _glowSequence.Append(_glowBackground.DOFade(0.6f, 1f));
            _glowSequence.SetLoops(-1);
        }

        private void OnAPChanged(APChangedEvent evt)
        {
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
                    _apText.transform.DOScale(1f, _pulseDuration / 2);
                });

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
            _apText.transform.DOShakePosition(0.3f, 5f);

            DOVirtual.DelayedCall(0.5f, () => {
                if (_apText != null)
                {
                    _apText.color = _currentAP > 0 ? _fullColor : _emptyColor;
                }
            });
        }

        private void AnimatePulse()
        {
            if (_apText == null) return;

            _apText.transform.DOScale(_pulseScale, _pulseDuration / 2)
                .OnComplete(() => _apText.transform.DOScale(1f, _pulseDuration / 2));
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
