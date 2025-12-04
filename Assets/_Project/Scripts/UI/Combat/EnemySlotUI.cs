// ============================================
// EnemySlotUI.cs
// UI component for displaying enemy information
// ============================================

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using HNR.Core;
using HNR.Core.Events;

namespace HNR.Combat
{
    /// <summary>
    /// Displays enemy information in combat: HP bar, block, intent.
    /// Subscribes to combat events for real-time updates.
    /// </summary>
    public class EnemySlotUI : MonoBehaviour
    {
        // ============================================
        // References
        // ============================================

        [Header("Visual References")]
        [SerializeField, Tooltip("Enemy sprite display")]
        private Image _enemySprite;

        [SerializeField, Tooltip("HP bar slider")]
        private Slider _hpBar;

        [SerializeField, Tooltip("HP text display")]
        private TextMeshProUGUI _hpText;

        [SerializeField, Tooltip("Block value display")]
        private TextMeshProUGUI _blockText;

        [SerializeField, Tooltip("Block icon (hidden when 0)")]
        private GameObject _blockIcon;

        [Header("Intent Display")]
        [SerializeField, Tooltip("Intent icon image")]
        private Image _intentIcon;

        [SerializeField, Tooltip("Intent value text")]
        private TextMeshProUGUI _intentValueText;

        [SerializeField, Tooltip("Highlight ring for targeting")]
        private GameObject _highlightRing;

        [Header("Intent Icons")]
        [SerializeField] private Sprite _attackIcon;
        [SerializeField] private Sprite _attackMultipleIcon;
        [SerializeField] private Sprite _defendIcon;
        [SerializeField] private Sprite _buffIcon;
        [SerializeField] private Sprite _debuffIcon;
        [SerializeField] private Sprite _corruptIcon;
        [SerializeField] private Sprite _unknownIcon;

        [Header("Animation")]
        [SerializeField, Tooltip("Duration for HP bar lerp")]
        private float _hpAnimSpeed = 5f;

        // ============================================
        // Runtime State
        // ============================================

        private EnemyInstance _enemy;
        private float _targetHPFill;

        // ============================================
        // Properties
        // ============================================

        /// <summary>The enemy this slot displays.</summary>
        public EnemyInstance Enemy => _enemy;

        // ============================================
        // Initialization
        // ============================================

        /// <summary>
        /// Initialize the slot with an enemy instance.
        /// </summary>
        /// <param name="enemy">Enemy to display</param>
        public void Initialize(EnemyInstance enemy)
        {
            _enemy = enemy;

            // Set sprite
            if (_enemySprite != null && enemy.Data?.Sprite != null)
            {
                _enemySprite.sprite = enemy.Data.Sprite;
            }

            // Initial display update
            UpdateDisplay();
            UpdateIntent();

            // Subscribe to events
            SubscribeToEvents();

            Debug.Log($"[EnemySlotUI] Initialized for {enemy.Name}");
        }

        private void OnDestroy()
        {
            UnsubscribeFromEvents();
        }

        // ============================================
        // Event Handling
        // ============================================

        private void SubscribeToEvents()
        {
            EventBus.Subscribe<EnemyDamagedEvent>(OnEnemyDamaged);
            EventBus.Subscribe<TurnStartedEvent>(OnTurnStarted);
            EventBus.Subscribe<EnemyDefeatedEvent>(OnEnemyDefeated);
        }

        private void UnsubscribeFromEvents()
        {
            EventBus.Unsubscribe<EnemyDamagedEvent>(OnEnemyDamaged);
            EventBus.Unsubscribe<TurnStartedEvent>(OnTurnStarted);
            EventBus.Unsubscribe<EnemyDefeatedEvent>(OnEnemyDefeated);
        }

        private void OnEnemyDamaged(EnemyDamagedEvent evt)
        {
            if (evt.Enemy == _enemy)
            {
                UpdateDisplay();
            }
        }

        private void OnTurnStarted(TurnStartedEvent evt)
        {
            if (evt.IsPlayerTurn)
            {
                UpdateIntent();
            }
        }

        private void OnEnemyDefeated(EnemyDefeatedEvent evt)
        {
            if (evt.Enemy == _enemy)
            {
                // Could play death animation here
                gameObject.SetActive(false);
            }
        }

        // ============================================
        // Update Methods
        // ============================================

        private void Update()
        {
            // Smooth HP bar animation
            if (_hpBar != null && Mathf.Abs(_hpBar.value - _targetHPFill) > 0.001f)
            {
                _hpBar.value = Mathf.MoveTowards(_hpBar.value, _targetHPFill, _hpAnimSpeed * Time.deltaTime);
            }
        }

        /// <summary>
        /// Update HP and block display.
        /// </summary>
        public void UpdateDisplay()
        {
            if (_enemy == null) return;

            // Update HP bar target (animated in Update)
            _targetHPFill = _enemy.MaxHP > 0 ? (float)_enemy.CurrentHP / _enemy.MaxHP : 0f;

            // Update HP text
            if (_hpText != null)
            {
                _hpText.text = $"{_enemy.CurrentHP}/{_enemy.MaxHP}";
            }

            // Update block display
            if (_blockText != null)
            {
                _blockText.text = _enemy.Block > 0 ? _enemy.Block.ToString() : "";
            }

            if (_blockIcon != null)
            {
                _blockIcon.SetActive(_enemy.Block > 0);
            }
        }

        /// <summary>
        /// Update intent icon and value.
        /// </summary>
        public void UpdateIntent()
        {
            if (_enemy == null) return;

            var intent = _enemy.GetCurrentIntent();
            if (intent == null)
            {
                if (_intentIcon != null) _intentIcon.gameObject.SetActive(false);
                if (_intentValueText != null) _intentValueText.gameObject.SetActive(false);
                return;
            }

            // Show intent
            if (_intentIcon != null)
            {
                _intentIcon.gameObject.SetActive(true);
                _intentIcon.sprite = GetIntentIcon(intent.IntentType);
                _intentIcon.color = GetIntentColor(intent.IntentType);
            }

            // Show value
            if (_intentValueText != null)
            {
                _intentValueText.gameObject.SetActive(true);

                string valueText = intent.IntentType switch
                {
                    IntentType.AttackMultiple => $"{intent.Value}x{intent.SecondaryValue}",
                    IntentType.AttackAll => $"{intent.Value} ALL",
                    _ when intent.Value > 0 => intent.Value.ToString(),
                    _ => ""
                };

                _intentValueText.text = valueText;
            }
        }

        private Sprite GetIntentIcon(IntentType type)
        {
            return type switch
            {
                IntentType.Attack => _attackIcon,
                IntentType.AttackMultiple => _attackMultipleIcon ?? _attackIcon,
                IntentType.AttackAll => _attackIcon,
                IntentType.Defend => _defendIcon,
                IntentType.Buff => _buffIcon,
                IntentType.Heal => _buffIcon,
                IntentType.Debuff => _debuffIcon,
                IntentType.Corrupt => _corruptIcon ?? _debuffIcon,
                IntentType.Unknown => _unknownIcon,
                _ => _unknownIcon
            };
        }

        private Color GetIntentColor(IntentType type)
        {
            return type switch
            {
                IntentType.Attack => new Color(0.8f, 0.2f, 0.2f),      // Red
                IntentType.AttackMultiple => new Color(0.9f, 0.3f, 0.1f), // Orange-red
                IntentType.AttackAll => new Color(1f, 0.4f, 0.2f),     // Orange
                IntentType.Defend => new Color(0.2f, 0.6f, 0.8f),      // Blue
                IntentType.Buff => new Color(0.8f, 0.7f, 0.2f),        // Gold
                IntentType.Heal => new Color(0.2f, 0.8f, 0.2f),        // Green
                IntentType.Debuff => new Color(0.6f, 0.2f, 0.6f),      // Purple
                IntentType.Corrupt => new Color(0.4f, 0.1f, 0.5f),     // Dark purple
                _ => Color.white
            };
        }

        // ============================================
        // Targeting Highlight
        // ============================================

        /// <summary>
        /// Show or hide the targeting highlight ring.
        /// </summary>
        /// <param name="show">True to show, false to hide</param>
        public void ShowHighlight(bool show)
        {
            if (_highlightRing != null)
            {
                _highlightRing.SetActive(show);
            }
        }

        // ============================================
        // Public API
        // ============================================

        /// <summary>
        /// Force refresh all display elements.
        /// </summary>
        public void Refresh()
        {
            UpdateDisplay();
            UpdateIntent();
        }
    }
}
