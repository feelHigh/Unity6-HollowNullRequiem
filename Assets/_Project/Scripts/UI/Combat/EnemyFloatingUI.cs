// ============================================
// EnemyFloatingUI.cs
// World-space UI for enemies with HP and intent
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
    /// World-space UI floating above enemies with HP bar and diamond intent indicator.
    /// Billboards to camera for consistent visibility.
    /// </summary>
    public class EnemyFloatingUI : MonoBehaviour
    {
        [Header("Anchoring")]
        [SerializeField] private Vector3 _offset = new(0, 2f, 0);

        [Header("Health Bar")]
        [SerializeField] private Image _hpBarFill;
        [SerializeField] private Image _hpBarBackground;
        [SerializeField] private TMP_Text _hpText;

        [Header("Intent Indicator")]
        [SerializeField] private RectTransform _intentContainer;
        [SerializeField] private Image _intentDiamond;
        [SerializeField] private Image _intentIcon;
        [SerializeField] private TMP_Text _intentCountdown;
        [SerializeField] private TMP_Text _intentValue;

        [Header("Intent Sprites")]
        [SerializeField] private Sprite _attackSprite;
        [SerializeField] private Sprite _defendSprite;
        [SerializeField] private Sprite _buffSprite;
        [SerializeField] private Sprite _debuffSprite;
        [SerializeField] private Sprite _ultimateSprite;
        [SerializeField] private Sprite _unknownSprite;

        [Header("Animation")]
        [SerializeField] private float _intentAppearDuration = 0.3f;
        [SerializeField] private float _pulseDuration = 0.5f;

        private EnemyInstance _enemy;
        private Transform _worldAnchor;
        private Sequence _pulseSequence;
        private Camera _mainCamera;

        /// <summary>
        /// Initializes the floating UI for an enemy.
        /// </summary>
        /// <param name="enemy">The enemy instance to track.</param>
        public void Initialize(EnemyInstance enemy)
        {
            _enemy = enemy;
            _worldAnchor = enemy.transform;
            _mainCamera = Camera.main;

            UpdateHealth(enemy.CurrentHP, enemy.MaxHP);

            // Show initial intent
            var intent = enemy.GetCurrentIntent();
            if (intent != null)
            {
                UpdateIntent(intent);
            }

            EventBus.Subscribe<EnemyDamagedEvent>(OnEnemyDamaged);
            EventBus.Subscribe<EnemyIntentChangedEvent>(OnIntentChanged);
            EventBus.Subscribe<EnemyDefeatedEvent>(OnEnemyDefeated);
        }

        private void OnDestroy()
        {
            EventBus.Unsubscribe<EnemyDamagedEvent>(OnEnemyDamaged);
            EventBus.Unsubscribe<EnemyIntentChangedEvent>(OnIntentChanged);
            EventBus.Unsubscribe<EnemyDefeatedEvent>(OnEnemyDefeated);
            _pulseSequence?.Kill();
        }

        private void LateUpdate()
        {
            // Follow enemy position
            if (_worldAnchor != null)
            {
                transform.position = _worldAnchor.position + _offset;
            }

            // Billboard to camera
            if (_mainCamera != null)
            {
                transform.LookAt(_mainCamera.transform);
                transform.Rotate(0, 180, 0);
            }
        }

        private void OnEnemyDamaged(EnemyDamagedEvent evt)
        {
            if (evt.Enemy != _enemy) return;
            UpdateHealth(_enemy.CurrentHP, _enemy.MaxHP);
        }

        private void OnIntentChanged(EnemyIntentChangedEvent evt)
        {
            if (evt.Enemy != _enemy) return;
            UpdateIntent(evt.Intent);
        }

        private void OnEnemyDefeated(EnemyDefeatedEvent evt)
        {
            if (evt.Enemy != _enemy) return;
            gameObject.SetActive(false);
        }

        private void UpdateHealth(int current, int max)
        {
            if (_hpBarFill != null)
            {
                float ratio = max > 0 ? (float)current / max : 0;
                _hpBarFill.fillAmount = ratio;

                // Color based on health percentage
                _hpBarFill.color = ratio switch
                {
                    < 0.25f => UIColors.CorruptionGlow,
                    < 0.5f => UIColors.SoulGold,
                    _ => UIColors.CorruptionRed
                };
            }

            if (_hpText != null)
            {
                _hpText.text = current.ToString();
            }
        }

        /// <summary>
        /// Updates the intent display from an IntentStep.
        /// </summary>
        /// <param name="intent">The intent to display.</param>
        public void UpdateIntent(IntentStep intent)
        {
            if (intent == null) return;

            _pulseSequence?.Kill();

            UpdateIntentIcon(intent.IntentType);
            UpdateIntentDiamondColor(intent.IntentType);

            if (_intentValue != null)
            {
                _intentValue.text = intent.Value > 0 ? intent.Value.ToString() : "";
            }

            // Countdown not directly available from IntentStep, hide by default
            if (_intentCountdown != null)
            {
                _intentCountdown.gameObject.SetActive(false);
            }

            AnimateIntentAppear();

            // Pulse for attack intents
            if (IsAttackIntent(intent.IntentType))
            {
                StartImminentPulse();
            }
        }

        /// <summary>
        /// Updates intent with explicit values for countdown support.
        /// </summary>
        /// <param name="type">Intent type.</param>
        /// <param name="value">Damage/block value.</param>
        /// <param name="turnsUntil">Turns until special attack (0 = imminent).</param>
        public void UpdateIntent(IntentType type, int value, int turnsUntil = 0)
        {
            _pulseSequence?.Kill();

            UpdateIntentIcon(type);
            UpdateIntentDiamondColor(type);

            if (_intentValue != null)
            {
                _intentValue.text = value > 0 ? value.ToString() : "";
            }

            if (_intentCountdown != null)
            {
                _intentCountdown.text = turnsUntil > 0 ? turnsUntil.ToString() : "";
                _intentCountdown.gameObject.SetActive(turnsUntil > 0);
            }

            AnimateIntentAppear();

            if (IsAttackIntent(type) && turnsUntil == 0)
            {
                StartImminentPulse();
            }
        }

        private void UpdateIntentIcon(IntentType type)
        {
            if (_intentIcon == null) return;

            _intentIcon.sprite = type switch
            {
                IntentType.Attack => _attackSprite,
                IntentType.AttackMultiple => _attackSprite,
                IntentType.AttackAll => _attackSprite,
                IntentType.Defend => _defendSprite,
                IntentType.Buff => _buffSprite,
                IntentType.Debuff => _debuffSprite,
                IntentType.Heal => _buffSprite,
                IntentType.Special => _ultimateSprite,
                IntentType.Unknown => _unknownSprite,
                _ => _attackSprite
            };
        }

        private void UpdateIntentDiamondColor(IntentType type)
        {
            if (_intentDiamond == null) return;

            _intentDiamond.color = type switch
            {
                IntentType.Attack => UIColors.CorruptionGlow,
                IntentType.AttackMultiple => UIColors.CorruptionGlow,
                IntentType.AttackAll => UIColors.CorruptionGlow,
                IntentType.Defend => UIColors.SoulCyan,
                IntentType.Buff => UIColors.SoulGold,
                IntentType.Heal => UIColors.NatureAspect,
                IntentType.Debuff => UIColors.HollowViolet,
                IntentType.Special => UIColors.HollowViolet,
                IntentType.Corrupt => UIColors.CorruptionRed,
                _ => UIColors.PanelGray
            };
        }

        private bool IsAttackIntent(IntentType type)
        {
            return type == IntentType.Attack ||
                   type == IntentType.AttackMultiple ||
                   type == IntentType.AttackAll;
        }

        private void AnimateIntentAppear()
        {
            if (_intentContainer == null) return;

            _intentContainer.localScale = Vector3.zero;
            _intentContainer.DOScale(1f, _intentAppearDuration).SetEase(Ease.OutBack);
        }

        private void StartImminentPulse()
        {
            if (_intentDiamond == null) return;

            _pulseSequence = DOTween.Sequence();
            _pulseSequence.Append(_intentDiamond.DOFade(0.5f, _pulseDuration));
            _pulseSequence.Append(_intentDiamond.DOFade(1f, _pulseDuration));
            _pulseSequence.SetLoops(-1);
        }

        /// <summary>
        /// Hides the intent indicator.
        /// </summary>
        public void HideIntent()
        {
            if (_intentContainer != null)
            {
                _intentContainer.gameObject.SetActive(false);
            }
            _pulseSequence?.Kill();
        }

        /// <summary>
        /// Shows the intent indicator.
        /// </summary>
        public void ShowIntent()
        {
            if (_intentContainer != null)
            {
                _intentContainer.gameObject.SetActive(true);
            }
        }
    }
}
