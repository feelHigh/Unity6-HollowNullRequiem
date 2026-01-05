// ============================================
// AutoBattleController.cs
// Automated card playing during auto-battle mode
// ============================================

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using HNR.Core;
using HNR.Core.Events;
using HNR.Cards;
using HNR.Characters;
using HNR.UI.Combat;

namespace HNR.Combat
{
    /// <summary>
    /// Handles automated card playing during auto-battle mode.
    /// Plays cards by priority: Strike > Guard > Skill > Power.
    /// </summary>
    public class AutoBattleController : MonoBehaviour
    {
        // ============================================
        // Configuration
        // ============================================

        [Header("Timing")]
        [SerializeField, Tooltip("Delay between playing cards (seconds)")]
        private float _cardPlayDelay = 0.4f;

        [SerializeField, Tooltip("Delay before ending turn (seconds)")]
        private float _endTurnDelay = 0.3f;

        // ============================================
        // State
        // ============================================

        private bool _isAutoEnabled;
        private Coroutine _autoPlayCoroutine;
        private TurnManager _turnManager;
        private CardFanLayout _cardFanLayout;

        // ============================================
        // Unity Lifecycle
        // ============================================

        private void Awake()
        {
            ServiceLocator.Register(this);
        }

        private void OnEnable()
        {
            EventBus.Subscribe<AutoBattleToggledEvent>(OnAutoBattleToggled);
            EventBus.Subscribe<TurnStartedEvent>(OnTurnStarted);
            EventBus.Subscribe<CombatEndedEvent>(OnCombatEnded);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<AutoBattleToggledEvent>(OnAutoBattleToggled);
            EventBus.Unsubscribe<TurnStartedEvent>(OnTurnStarted);
            EventBus.Unsubscribe<CombatEndedEvent>(OnCombatEnded);
        }

        private void OnDestroy()
        {
            ServiceLocator.Unregister<AutoBattleController>();
            StopAutoPlay();
        }

        // ============================================
        // Event Handlers
        // ============================================

        private void OnAutoBattleToggled(AutoBattleToggledEvent evt)
        {
            _isAutoEnabled = evt.Enabled;
            Debug.Log($"[AutoBattleController] Auto-battle {(evt.Enabled ? "ENABLED" : "DISABLED")}");

            if (_isAutoEnabled)
            {
                // Cache references
                ServiceLocator.TryGet(out _turnManager);
                ServiceLocator.TryGet(out _cardFanLayout);

                // If already in player phase, start auto-play immediately
                if (_turnManager != null && _turnManager.CurrentPhase == CombatPhase.PlayerPhase)
                {
                    StartAutoPlay();
                }
            }
            else
            {
                StopAutoPlay();
            }
        }

        private void OnTurnStarted(TurnStartedEvent evt)
        {
            Debug.Log($"[AutoBattleController] TurnStarted event received - IsPlayerTurn: {evt.IsPlayerTurn}, AutoEnabled: {_isAutoEnabled}");

            if (!evt.IsPlayerTurn) return;
            if (!_isAutoEnabled) return;

            // Cache references if not already cached
            if (_turnManager == null) ServiceLocator.TryGet(out _turnManager);
            if (_cardFanLayout == null) ServiceLocator.TryGet(out _cardFanLayout);

            Debug.Log("[AutoBattleController] Starting auto-play for new player turn");

            // Small delay to let cards deal before auto-playing
            StartCoroutine(DelayedAutoPlay(0.8f)); // Increased delay to ensure cards are drawn
        }

        private void OnCombatEnded(CombatEndedEvent evt)
        {
            _isAutoEnabled = false;
            StopAutoPlay();
        }

        // ============================================
        // Auto-Play Logic
        // ============================================

        private IEnumerator DelayedAutoPlay(float delay)
        {
            // Use real-time delay to be consistent regardless of Time.timeScale
            yield return new WaitForSecondsRealtime(delay);
            if (_isAutoEnabled)
            {
                StartAutoPlay();
            }
        }

        private void StartAutoPlay()
        {
            if (_autoPlayCoroutine != null)
            {
                StopCoroutine(_autoPlayCoroutine);
            }
            _autoPlayCoroutine = StartCoroutine(AutoPlayCoroutine());
        }

        private void StopAutoPlay()
        {
            if (_autoPlayCoroutine != null)
            {
                StopCoroutine(_autoPlayCoroutine);
                _autoPlayCoroutine = null;
            }
        }

        private IEnumerator AutoPlayCoroutine()
        {
            Debug.Log("[AutoBattleController] Starting auto-play loop");

            // Wait for CardFanLayout to be populated (using real-time to work at any speed)
            float waitTime = 0f;
            while (_cardFanLayout == null || _cardFanLayout.Cards.Count == 0)
            {
                ServiceLocator.TryGet(out _cardFanLayout);
                yield return new WaitForSecondsRealtime(0.1f);
                waitTime += 0.1f;
                if (waitTime > 3f) // Increased timeout for safety
                {
                    Debug.LogWarning("[AutoBattleController] Timeout waiting for cards");
                    break;
                }
            }

            while (_isAutoEnabled && _turnManager != null && _turnManager.CurrentPhase == CombatPhase.PlayerPhase)
            {
                // Wait for any ongoing draw animations
                while (_turnManager.IsDrawingCards)
                {
                    yield return null;
                }

                // Small real-time delay to ensure UI is ready
                yield return new WaitForSecondsRealtime(0.1f);

                // Find the best playable card
                var card = GetBestPlayableCard();

                if (card == null)
                {
                    // No playable cards - end turn
                    Debug.Log("[AutoBattleController] No playable cards, ending turn (auto-battle will continue next turn)");
                    yield return new WaitForSecondsRealtime(_endTurnDelay);

                    if (_isAutoEnabled && _turnManager.CurrentPhase == CombatPhase.PlayerPhase)
                    {
                        _turnManager.EndPlayerTurn();
                    }
                    // Exit this coroutine - OnTurnStarted will start a new one for the next player turn
                    break;
                }

                // Determine target
                var target = GetTargetForCard(card);

                // Play the card
                Debug.Log($"[AutoBattleController] Auto-playing: {card.CardData?.Data?.CardName} -> {target?.Name ?? "self"}");

                if (_turnManager.TryPlayCard(card, target))
                {
                    // Wait between card plays (real-time for consistent pacing)
                    yield return new WaitForSecondsRealtime(_cardPlayDelay);
                }
                else
                {
                    // Card play failed, try next iteration
                    Debug.LogWarning($"[AutoBattleController] Failed to play card: {card.CardData?.Data?.CardName}");
                    yield return new WaitForSecondsRealtime(0.1f);
                }
            }

            _autoPlayCoroutine = null;
            Debug.Log($"[AutoBattleController] Auto-play loop ended (AutoEnabled: {_isAutoEnabled})");
        }

        /// <summary>
        /// Gets the best playable card from hand based on priority.
        /// Priority: Strike (0) > Guard (1) > Skill (2) > Power (3)
        /// </summary>
        private CombatCard GetBestPlayableCard()
        {
            // Ensure we have CardFanLayout reference
            if (_cardFanLayout == null)
            {
                ServiceLocator.TryGet(out _cardFanLayout);
                if (_cardFanLayout == null)
                {
                    Debug.LogWarning("[AutoBattleController] CardFanLayout not found");
                    return null;
                }
            }

            if (_turnManager?.Context == null) return null;

            int currentAP = _turnManager.Context.CurrentAP;
            CombatCard bestCard = null;
            int bestPriority = int.MaxValue;

            foreach (var card in _cardFanLayout.Cards)
            {
                if (card?.CardData == null) continue;
                if (!card.CardData.CanPlay(currentAP)) continue;

                int priority = GetCardPriority(card.CardData.Data.CardType);
                if (priority < bestPriority)
                {
                    bestPriority = priority;
                    bestCard = card;
                }
            }

            return bestCard;
        }

        /// <summary>
        /// Gets priority value for a card type. Lower is higher priority.
        /// </summary>
        private int GetCardPriority(CardType cardType)
        {
            return cardType switch
            {
                CardType.Strike => 0,
                CardType.Guard => 1,
                CardType.Skill => 2,
                CardType.Power => 3,
                _ => 99
            };
        }

        /// <summary>
        /// Determines the appropriate target for a card based on its target type.
        /// </summary>
        private ICombatTarget GetTargetForCard(CombatCard card)
        {
            if (card?.CardData?.Data == null) return null;

            var targetType = card.CardData.Data.TargetType;

            switch (targetType)
            {
                case TargetType.SingleEnemy:
                    return GetFirstAliveEnemy();

                case TargetType.SingleAlly:
                    return GetFirstAliveAlly();

                case TargetType.None:
                case TargetType.Self:
                case TargetType.AllEnemies:
                case TargetType.AllAllies:
                case TargetType.Random:
                default:
                    return null;
            }
        }

        /// <summary>
        /// Gets the first alive enemy as a target.
        /// </summary>
        private EnemyInstance GetFirstAliveEnemy()
        {
            if (_turnManager?.Context?.Enemies == null) return null;

            foreach (var enemy in _turnManager.Context.Enemies)
            {
                if (enemy != null && !enemy.IsDead)
                {
                    return enemy;
                }
            }

            return null;
        }

        /// <summary>
        /// Gets the first alive ally (Requiem) as a target.
        /// </summary>
        private RequiemInstance GetFirstAliveAlly()
        {
            if (_turnManager?.Context?.Team == null) return null;

            foreach (var requiem in _turnManager.Context.Team)
            {
                if (requiem != null && !requiem.IsDead)
                {
                    return requiem;
                }
            }

            return null;
        }

        // ============================================
        // Public API
        // ============================================

        /// <summary>
        /// Whether auto-battle is currently enabled.
        /// </summary>
        public bool IsAutoEnabled => _isAutoEnabled;
    }
}
