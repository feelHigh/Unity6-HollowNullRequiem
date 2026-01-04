// ============================================
// CombatAudioController.cs
// Plays audio in response to combat events
// ============================================

using UnityEngine;
using HNR.Core;
using HNR.Core.Interfaces;
using HNR.Core.Events;
using HNR.Combat;

namespace HNR.Audio
{
    /// <summary>
    /// Handles audio playback for combat events.
    /// Subscribes to EventBus and triggers appropriate sounds.
    /// </summary>
    public class CombatAudioController : MonoBehaviour
    {
        // ============================================
        // SFX IDs (must match AudioConfigSO entries)
        // ============================================

        [Header("Card SFX")]
        [SerializeField] private string _cardDrawSFX = "card_draw";
        [SerializeField] private string _cardPlaySFX = "card_play";
        [SerializeField] private string _cardDiscardSFX = "card_discard";

        [Header("Combat SFX")]
        [SerializeField] private string _damageHitSFX = "damage_hit";
        [SerializeField] private string _criticalHitSFX = "critical_hit";
        [SerializeField] private string _blockSFX = "block";
        [SerializeField] private string _healSFX = "heal";

        [Header("Turn SFX")]
        [SerializeField] private string _turnStartSFX = "turn_start";
        [SerializeField] private string _turnEndSFX = "turn_end";

        [Header("Status SFX")]
        [SerializeField] private string _corruptionGainSFX = "corruption_gain";
        [SerializeField] private string _nullStateSFX = "null_state";
        [SerializeField] private string _statusAppliedSFX = "status_applied";

        [Header("Enemy SFX")]
        [SerializeField] private string _enemyDefeatedSFX = "enemy_defeated";
        [SerializeField] private string _enemyAttackSFX = "enemy_attack";

        [Header("Combat Resolution SFX")]
        [SerializeField] private string _victorySFX = "victory";
        [SerializeField] private string _defeatSFX = "defeat";

        [Header("Music")]
        [SerializeField] private string _combatMusic = "combat_theme";
        [SerializeField] private string _bossMusic = "boss_theme";
        [SerializeField] private float _musicFadeTime = 2f;

        // ============================================
        // Runtime State
        // ============================================

        private IAudioManager _audioManager;
        private bool _isBossFight;

        // ============================================
        // Lifecycle
        // ============================================

        private void Awake()
        {
            // Use TryGet to avoid errors if AudioManager isn't available yet
            ServiceLocator.TryGet(out _audioManager);
        }

        private void Start()
        {
            // Retry in Start if not found in Awake (timing issues)
            EnsureAudioManager();
        }

        /// <summary>
        /// Ensures the audio manager reference is valid, attempting to fetch if null.
        /// </summary>
        private bool EnsureAudioManager()
        {
            if (_audioManager != null) return true;

            ServiceLocator.TryGet(out _audioManager);

            if (_audioManager == null)
            {
                Debug.LogWarning("[CombatAudioController] IAudioManager not found in ServiceLocator. Audio will be disabled.");
                return false;
            }

            return true;
        }

        private void OnEnable()
        {
            // Combat lifecycle
            EventBus.Subscribe<CombatStartedEvent>(OnCombatStarted);
            EventBus.Subscribe<CombatEndedEvent>(OnCombatEnded);

            // Turn events
            EventBus.Subscribe<TurnStartedEvent>(OnTurnStarted);
            EventBus.Subscribe<TurnEndedEvent>(OnTurnEnded);

            // Card events
            EventBus.Subscribe<CardDrawnEvent>(OnCardDrawn);
            EventBus.Subscribe<CardPlayedEvent>(OnCardPlayed);
            EventBus.Subscribe<CardDiscardedEvent>(OnCardDiscarded);

            // Damage/healing events
            EventBus.Subscribe<DamageDealtEvent>(OnDamageDealt);
            EventBus.Subscribe<BlockGainedEvent>(OnBlockGained);
            EventBus.Subscribe<HealingReceivedEvent>(OnHealingReceived);

            // Status events
            EventBus.Subscribe<CorruptionChangedEvent>(OnCorruptionChanged);
            EventBus.Subscribe<NullStateEnteredEvent>(OnNullStateEntered);
            EventBus.Subscribe<StatusAppliedEvent>(OnStatusApplied);

            // Enemy events
            EventBus.Subscribe<EnemyDefeatedEvent>(OnEnemyDefeated);
            EventBus.Subscribe<EnemyIntentExecutedEvent>(OnEnemyIntentExecuted);
        }

        private void OnDisable()
        {
            // Combat lifecycle
            EventBus.Unsubscribe<CombatStartedEvent>(OnCombatStarted);
            EventBus.Unsubscribe<CombatEndedEvent>(OnCombatEnded);

            // Turn events
            EventBus.Unsubscribe<TurnStartedEvent>(OnTurnStarted);
            EventBus.Unsubscribe<TurnEndedEvent>(OnTurnEnded);

            // Card events
            EventBus.Unsubscribe<CardDrawnEvent>(OnCardDrawn);
            EventBus.Unsubscribe<CardPlayedEvent>(OnCardPlayed);
            EventBus.Unsubscribe<CardDiscardedEvent>(OnCardDiscarded);

            // Damage/healing events
            EventBus.Unsubscribe<DamageDealtEvent>(OnDamageDealt);
            EventBus.Unsubscribe<BlockGainedEvent>(OnBlockGained);
            EventBus.Unsubscribe<HealingReceivedEvent>(OnHealingReceived);

            // Status events
            EventBus.Unsubscribe<CorruptionChangedEvent>(OnCorruptionChanged);
            EventBus.Unsubscribe<NullStateEnteredEvent>(OnNullStateEntered);
            EventBus.Unsubscribe<StatusAppliedEvent>(OnStatusApplied);

            // Enemy events
            EventBus.Unsubscribe<EnemyDefeatedEvent>(OnEnemyDefeated);
            EventBus.Unsubscribe<EnemyIntentExecutedEvent>(OnEnemyIntentExecuted);
        }

        // ============================================
        // Combat Lifecycle Handlers
        // ============================================

        private void OnCombatStarted(CombatStartedEvent evt)
        {
            // Try to get audio manager if not available yet (late initialization)
            EnsureAudioManager();

            // Determine if boss fight by checking enemy types
            _isBossFight = false;
            if (evt.Enemies != null)
            {
                foreach (var enemy in evt.Enemies)
                {
                    if (enemy != null && enemy.Data != null && enemy.Data.IsBoss)
                    {
                        _isBossFight = true;
                        break;
                    }
                }
            }

            string music = _isBossFight ? _bossMusic : _combatMusic;
            _audioManager?.PlayMusic(music, _musicFadeTime);

            Debug.Log($"[CombatAudioController] Combat started - Boss: {_isBossFight}");
        }

        private void OnCombatEnded(CombatEndedEvent evt)
        {
            string sfx = evt.Victory ? _victorySFX : _defeatSFX;
            _audioManager?.PlaySFX(sfx);
            _audioManager?.StopMusic(_musicFadeTime);

            Debug.Log($"[CombatAudioController] Combat ended - Victory: {evt.Victory}");
        }

        // ============================================
        // Turn Handlers
        // ============================================

        private void OnTurnStarted(TurnStartedEvent evt)
        {
            if (evt.IsPlayerTurn)
            {
                _audioManager?.PlaySFX(_turnStartSFX);
            }
        }

        private void OnTurnEnded(TurnEndedEvent evt)
        {
            if (evt.WasPlayerTurn)
            {
                _audioManager?.PlaySFX(_turnEndSFX);
            }
        }

        // ============================================
        // Card Handlers
        // ============================================

        private void OnCardDrawn(CardDrawnEvent evt)
        {
            _audioManager?.PlaySFX(_cardDrawSFX);
        }

        private void OnCardPlayed(CardPlayedEvent evt)
        {
            _audioManager?.PlaySFX(_cardPlaySFX);
        }

        private void OnCardDiscarded(CardDiscardedEvent evt)
        {
            _audioManager?.PlaySFX(_cardDiscardSFX);
        }

        // ============================================
        // Combat Action Handlers
        // ============================================

        private void OnDamageDealt(DamageDealtEvent evt)
        {
            string sfx = evt.IsCritical ? _criticalHitSFX : _damageHitSFX;

            // Use positional audio if target has valid position
            if (evt.Target != null)
            {
                _audioManager?.PlaySFXAtPosition(sfx, evt.Target.Position);
            }
            else
            {
                _audioManager?.PlaySFX(sfx);
            }
        }

        private void OnBlockGained(BlockGainedEvent evt)
        {
            if (evt.Amount > 0)
            {
                _audioManager?.PlaySFX(_blockSFX);
            }
        }

        private void OnHealingReceived(HealingReceivedEvent evt)
        {
            if (evt.Amount > 0)
            {
                _audioManager?.PlaySFX(_healSFX);
            }
        }

        // ============================================
        // Status Handlers
        // ============================================

        private void OnCorruptionChanged(CorruptionChangedEvent evt)
        {
            // Only play sound when corruption increases
            if (evt.Delta > 0)
            {
                _audioManager?.PlaySFX(_corruptionGainSFX);
            }
        }

        private void OnNullStateEntered(NullStateEnteredEvent evt)
        {
            _audioManager?.PlaySFX(_nullStateSFX);
            Debug.Log($"[CombatAudioController] {evt.Requiem?.Name} entered Null State!");
        }

        private void OnStatusApplied(StatusAppliedEvent evt)
        {
            _audioManager?.PlaySFX(_statusAppliedSFX);
        }

        // ============================================
        // Enemy Handlers
        // ============================================

        private void OnEnemyDefeated(EnemyDefeatedEvent evt)
        {
            _audioManager?.PlaySFX(_enemyDefeatedSFX);
        }

        private void OnEnemyIntentExecuted(EnemyIntentExecutedEvent evt)
        {
            if (evt.Intent == null) return;

            // Play attack sound for all damaging intent types
            bool isAttackIntent = evt.Intent.IntentType == IntentType.Attack ||
                                  evt.Intent.IntentType == IntentType.AttackMultiple ||
                                  evt.Intent.IntentType == IntentType.AttackAll;

            if (isAttackIntent)
            {
                if (evt.Enemy != null)
                {
                    _audioManager?.PlaySFXAtPosition(_enemyAttackSFX, evt.Enemy.Position);
                }
                else
                {
                    _audioManager?.PlaySFX(_enemyAttackSFX);
                }
            }
        }
    }
}
