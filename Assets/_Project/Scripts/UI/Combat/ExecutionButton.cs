// ============================================
// ExecutionButton.cs
// Large circular End Turn button with glow ring
// Positioned bottom-right of combat screen
// ============================================

using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using HNR.Core.Events;
using HNR.Combat;

namespace HNR.UI.Combat
{
    /// <summary>
    /// Large circular End Turn button with glowing ring effect.
    /// Positioned bottom-right of combat screen.
    /// </summary>
    public class ExecutionButton : MonoBehaviour
    {
        [Header("Visual")]
        [SerializeField] private Image _buttonBackground;
        [SerializeField] private Image _checkmarkIcon;
        [SerializeField] private Image _glowRing;
        [SerializeField] private ParticleSystem _readyParticles;
        [SerializeField] private Button _button;

        [Header("Animation")]
        [SerializeField] private float _glowPulseSpeed = 1f;
        [SerializeField] private float _pressScale = 0.9f;

        private Color _readyColor;
        private Color _processingColor;
        private Color _disabledColor;
        private Sequence _glowSequence;
        private ExecutionButtonState _currentState;

        private void Start()
        {
            _readyColor = UIColors.SoulCyan;
            _processingColor = UIColors.SoulGold;
            _disabledColor = UIColors.PanelGray;

            if (_button != null)
            {
                _button.onClick.AddListener(OnClick);
            }

            EventBus.Subscribe<TurnStartedEvent>(OnTurnStarted);
            EventBus.Subscribe<TurnEndedEvent>(OnTurnEnded);
        }

        private void OnDestroy()
        {
            if (_button != null)
            {
                _button.onClick.RemoveListener(OnClick);
            }

            EventBus.Unsubscribe<TurnStartedEvent>(OnTurnStarted);
            EventBus.Unsubscribe<TurnEndedEvent>(OnTurnEnded);
            _glowSequence?.Kill();
        }

        private void OnTurnStarted(TurnStartedEvent evt)
        {
            SetState(evt.IsPlayerTurn ? ExecutionButtonState.Ready : ExecutionButtonState.Disabled);
        }

        private void OnTurnEnded(TurnEndedEvent evt)
        {
            SetState(ExecutionButtonState.Processing);
        }

        /// <summary>
        /// Sets the button state and updates visuals accordingly.
        /// </summary>
        /// <param name="state">The new button state.</param>
        public void SetState(ExecutionButtonState state)
        {
            _currentState = state;
            _glowSequence?.Kill();

            switch (state)
            {
                case ExecutionButtonState.Ready:
                    SetVisuals(_readyColor, Color.white, true);
                    StartGlowPulse();
                    if (_readyParticles != null) _readyParticles.Play();
                    break;

                case ExecutionButtonState.Processing:
                    SetVisuals(_processingColor, Color.white, false);
                    if (_glowRing != null) _glowRing.DOFade(0.5f, 0.1f);
                    if (_readyParticles != null) _readyParticles.Stop();
                    break;

                case ExecutionButtonState.Disabled:
                    SetVisuals(_disabledColor, new Color(1, 1, 1, 0.5f), false);
                    if (_glowRing != null) _glowRing.DOFade(0.2f, 0.1f);
                    if (_readyParticles != null) _readyParticles.Stop();
                    break;
            }
        }

        private void SetVisuals(Color baseColor, Color iconColor, bool interactable)
        {
            if (_buttonBackground != null) _buttonBackground.color = baseColor;
            if (_glowRing != null) _glowRing.color = baseColor;
            if (_checkmarkIcon != null) _checkmarkIcon.color = iconColor;
            if (_button != null) _button.interactable = interactable;
        }

        private void StartGlowPulse()
        {
            if (_glowRing == null) return;

            _glowSequence = DOTween.Sequence();
            _glowSequence.Append(_glowRing.DOFade(0.3f, _glowPulseSpeed));
            _glowSequence.Append(_glowRing.DOFade(0.8f, _glowPulseSpeed));
            _glowSequence.SetLoops(-1);
        }

        private void OnClick()
        {
            if (_currentState != ExecutionButtonState.Ready) return;

            if (_button != null)
            {
                _button.transform.DOScale(_pressScale, 0.1f)
                    .OnComplete(() => _button.transform.DOScale(1f, 0.1f));
            }

            SetState(ExecutionButtonState.Processing);
            EventBus.Publish(new EndTurnRequestedEvent());
        }
    }

    /// <summary>
    /// States for the ExecutionButton.
    /// </summary>
    public enum ExecutionButtonState
    {
        /// <summary>Player turn active, button clickable with cyan glow.</summary>
        Ready,
        /// <summary>Turn ending in progress, gold color, not clickable.</summary>
        Processing,
        /// <summary>Not player turn, gray color, not clickable.</summary>
        Disabled
    }
}
