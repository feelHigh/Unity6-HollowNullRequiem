// ============================================
// CombatAudioController.cs
// Plays audio in response to combat events
// ============================================

using System.Collections.Generic;
using UnityEngine;
using HNR.Core;
using HNR.Core.Interfaces;
using HNR.Core.Events;
using HNR.Characters;
using HNR.Combat;
using HNR.Map;

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
        [SerializeField] private string _healSFX = "heal";

        [Header("Status SFX")]
        [SerializeField] private string _corruptionGainSFX = "corruption_gain";

        [Header("Enemy SFX")]
        [SerializeField] private string _enemyDefeatedSFX = "enemy_defeated";
        [SerializeField] private string _enemyAttackSFX = "enemy_attack";

        [Header("Combat Resolution SFX")]
        [SerializeField] private string _victorySFX = "victory";
        [SerializeField] private string _defeatSFX = "defeat";

        [Header("Music")]
        [SerializeField] private string _combatMusic = "combat_theme";
        [SerializeField] private string _bossMusic = "boss_theme";
        [SerializeField] private string _zone1EliteMusic = "zone1_elite_theme";
        [SerializeField] private string _zone2EliteMusic = "zone2_elite_theme";
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

            // Card events
            EventBus.Subscribe<CardDrawnEvent>(OnCardDrawn);
            EventBus.Subscribe<CardPlayedEvent>(OnCardPlayed);
            EventBus.Subscribe<CardDiscardedEvent>(OnCardDiscarded);

            // Damage/healing events
            EventBus.Subscribe<DamageDealtEvent>(OnDamageDealt);
            EventBus.Subscribe<HealingReceivedEvent>(OnHealingReceived);

            // Status events
            EventBus.Subscribe<CorruptionChangedEvent>(OnCorruptionChanged);
            EventBus.Subscribe<NullStateExitedEvent>(OnNullStateExited);

            // Enemy events
            EventBus.Subscribe<EnemyDefeatedEvent>(OnEnemyDefeated);
            EventBus.Subscribe<EnemyIntentExecutedEvent>(OnEnemyIntentExecuted);
        }

        private void OnDisable()
        {
            // Combat lifecycle
            EventBus.Unsubscribe<CombatStartedEvent>(OnCombatStarted);
            EventBus.Unsubscribe<CombatEndedEvent>(OnCombatEnded);

            // Card events
            EventBus.Unsubscribe<CardDrawnEvent>(OnCardDrawn);
            EventBus.Unsubscribe<CardPlayedEvent>(OnCardPlayed);
            EventBus.Unsubscribe<CardDiscardedEvent>(OnCardDiscarded);

            // Damage/healing events
            EventBus.Unsubscribe<DamageDealtEvent>(OnDamageDealt);
            EventBus.Unsubscribe<HealingReceivedEvent>(OnHealingReceived);

            // Status events
            EventBus.Unsubscribe<CorruptionChangedEvent>(OnCorruptionChanged);
            EventBus.Unsubscribe<NullStateExitedEvent>(OnNullStateExited);

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

            // Get node type, zone, and final node flag from CombatBootstrap
            var nodeType = CombatBootstrap.PendingNodeType;
            int zone = CombatBootstrap.PendingZone;
            bool isFinalNode = CombatBootstrap.IsFinalNode;

            // Determine music based on zone and whether this is the final node
            string music = DetermineCombatMusic(zone, isFinalNode);
            _audioManager?.PlayMusic(music, _musicFadeTime);

            Debug.Log($"[CombatAudioController] Combat started - Boss: {_isBossFight}, NodeType: {nodeType}, Zone: {zone}, FinalNode: {isFinalNode}, Music: {music}");
        }

        /// <summary>
        /// Determines which music track to play based on zone and whether this is the final node.
        /// Zone finale themes only play on the final node of each zone.
        /// </summary>
        private string DetermineCombatMusic(int zone, bool isFinalNode)
        {
            // Zone finale themes only play on the final node of each zone
            if (isFinalNode)
            {
                return zone switch
                {
                    1 => _zone1EliteMusic,
                    2 => _zone2EliteMusic,
                    // Zone 3+ uses boss theme
                    _ => _bossMusic
                };
            }

            // All other combat encounters use default combat music
            return _combatMusic;
        }

        private void OnCombatEnded(CombatEndedEvent evt)
        {
            string sfx = evt.Victory ? _victorySFX : _defeatSFX;
            _audioManager?.PlaySFX(sfx);
            _audioManager?.StopMusic(_musicFadeTime);

            Debug.Log($"[CombatAudioController] Combat ended - Victory: {evt.Victory}");
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

        private void OnNullStateExited(NullStateExitedEvent evt)
        {
            Debug.Log($"[CombatAudioController] {evt.Requiem?.Name} exited Null State");
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
