// ============================================
// TurnManager.cs
// Orchestrates combat phases and turn flow
// ============================================

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HNR.Core;
using HNR.Core.Events;
using HNR.Cards;
using HNR.Characters;
using HNR.Characters.Visuals;

namespace HNR.Combat
{
    /// <summary>
    /// Orchestrates combat phases and manages turn flow.
    /// Central controller for the combat system.
    /// </summary>
    public class TurnManager : MonoBehaviour
    {
        // ============================================
        // Configuration
        // ============================================

        [Header("Configuration")]
        [SerializeField, Tooltip("AP granted at start of each turn")]
        private int _startingAP = 3;

        [SerializeField, Tooltip("Cards drawn at start of each turn")]
        private int _cardsPerTurn = 5;

        [Header("Card Draw Animation")]
        [SerializeField, Tooltip("Delay between drawing each card")]
        private float _cardDrawDelay = 0.15f;

        [SerializeField, Tooltip("Additional delay when deck reshuffles mid-draw")]
        private float _reshuffleDelay = 0.4f;

        // ============================================
        // Runtime State
        // ============================================

        private Dictionary<CombatPhase, ICombatPhase> _phases = new();
        private ICombatPhase _currentPhase;
        private CombatContext _context;
        private bool _combatActive;
        private bool _reshuffleOccurred;
        private Coroutine _drawCoroutine;

        // ============================================
        // Properties
        // ============================================

        /// <summary>Current combat phase.</summary>
        public CombatPhase CurrentPhase => _currentPhase?.PhaseType ?? CombatPhase.Setup;

        /// <summary>Combat context with all state.</summary>
        public CombatContext Context => _context;

        /// <summary>Whether it's currently the player's turn.</summary>
        public bool IsPlayerTurn => _context?.IsPlayerTurn ?? false;

        /// <summary>Whether combat is currently active.</summary>
        public bool IsCombatActive => _combatActive;

        // ============================================
        // Unity Lifecycle
        // ============================================

        private void Awake()
        {
            ServiceLocator.Register(this);
            _context = new CombatContext();
            InitializePhases();
        }

        private void OnEnable()
        {
            EventBus.Subscribe<EndTurnRequestedEvent>(OnEndTurnRequested);
            EventBus.Subscribe<EnemyDefeatedEvent>(OnEnemyDefeated);
            EventBus.Subscribe<DeckReshuffledEvent>(OnDeckReshuffled);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<EndTurnRequestedEvent>(OnEndTurnRequested);
            EventBus.Unsubscribe<EnemyDefeatedEvent>(OnEnemyDefeated);
            EventBus.Unsubscribe<DeckReshuffledEvent>(OnDeckReshuffled);
        }

        private void OnDeckReshuffled(DeckReshuffledEvent evt)
        {
            _reshuffleOccurred = true;
            Debug.Log($"[TurnManager] Deck reshuffled - {evt.CardCount} cards");
        }

        private void OnDestroy()
        {
            ServiceLocator.Unregister<TurnManager>();
        }

        private void OnEndTurnRequested(EndTurnRequestedEvent evt)
        {
            Debug.Log("[TurnManager] EndTurnRequestedEvent received");
            EndPlayerTurn();
        }

        private void OnEnemyDefeated(EnemyDefeatedEvent evt)
        {
            // Check if all enemies are defeated for immediate victory
            if (!_combatActive || _context.CombatEnded) return;

            bool allDefeated = true;
            foreach (var enemy in _context.Enemies)
            {
                if (!enemy.IsDead)
                {
                    allDefeated = false;
                    break;
                }
            }

            if (allDefeated)
            {
                Debug.Log("[TurnManager] All enemies defeated - triggering immediate victory!");
                _context.PlayerVictory = true;
                TransitionToPhase(CombatPhase.Victory);
            }
        }

        private void Update()
        {
            if (_combatActive && _currentPhase != null)
            {
                _currentPhase.Update(_context);
            }
        }

        // ============================================
        // Initialization
        // ============================================

        private void InitializePhases()
        {
            _phases[CombatPhase.Setup] = new SetupPhase();
            _phases[CombatPhase.DrawPhase] = new DrawPhase(_cardsPerTurn);
            _phases[CombatPhase.PlayerPhase] = new PlayerPhase();
            _phases[CombatPhase.EndPhase] = new EndPhase();
            _phases[CombatPhase.EnemyPhase] = new EnemyPhase();
            _phases[CombatPhase.Victory] = new VictoryPhase();
            _phases[CombatPhase.Defeat] = new DefeatPhase();
        }

        // ============================================
        // Combat Control
        // ============================================

        /// <summary>
        /// Start a new combat encounter.
        /// </summary>
        /// <param name="team">Player's Requiem team</param>
        /// <param name="enemies">Enemies to fight</param>
        public void StartCombat(List<RequiemInstance> team, List<EnemyInstance> enemies)
        {
            _context.Reset();
            _context.Team = team;
            _context.Enemies = enemies;
            _context.MaxAP = _startingAP;
            _context.DeckManager = ServiceLocator.TryGet<DeckManager>(out var dm) ? dm : null;
            _context.HandManager = ServiceLocator.TryGet<HandManager>(out var hm) ? hm : null;

            // Calculate team HP from all Requiems
            int totalHP = 0;
            foreach (var requiem in team)
            {
                totalHP += requiem.MaxHP;
            }
            _context.TeamHP = totalHP;
            _context.TeamMaxHP = totalHP;

            _combatActive = true;
            TransitionToPhase(CombatPhase.Setup);

            EventBus.Publish(new CombatStartedEvent(enemies));
            Debug.Log($"[TurnManager] Combat started: {team.Count} Requiems vs {enemies.Count} Enemies");
        }

        /// <summary>
        /// Transition to a specific combat phase.
        /// </summary>
        /// <param name="phase">Phase to transition to</param>
        public void TransitionToPhase(CombatPhase phase)
        {
            if (_currentPhase != null)
            {
                _currentPhase.Exit(_context);
                var previousPhase = _currentPhase.PhaseType;
                Debug.Log($"[TurnManager] Exiting phase: {previousPhase}");
                EventBus.Publish(new CombatPhaseChangedEvent(previousPhase, phase));
            }

            if (!_phases.TryGetValue(phase, out var newPhase))
            {
                Debug.LogError($"[TurnManager] Phase not found: {phase}");
                return;
            }

            _currentPhase = newPhase;
            Debug.Log($"[TurnManager] Entering phase: {phase}");
            _currentPhase.Enter(_context);
        }

        /// <summary>
        /// Advance to the next phase based on current phase logic.
        /// </summary>
        public void AdvancePhase()
        {
            if (_currentPhase == null) return;

            var nextPhase = _currentPhase.GetNextPhase(_context);
            TransitionToPhase(nextPhase);
        }

        /// <summary>
        /// End the player's turn (triggers end phase).
        /// </summary>
        public void EndPlayerTurn()
        {
            if (CurrentPhase != CombatPhase.PlayerPhase)
            {
                Debug.LogWarning("[TurnManager] Cannot end turn - not in player phase");
                return;
            }

            TransitionToPhase(CombatPhase.EndPhase);
        }

        /// <summary>
        /// End the combat encounter.
        /// </summary>
        /// <param name="victory">True if player won</param>
        public void EndCombat(bool victory)
        {
            _combatActive = false;
            _context.CombatEnded = true;
            _context.PlayerVictory = victory;

            int voidShards = 0;
            if (victory)
            {
                foreach (var enemy in _context.Enemies)
                {
                    if (enemy.Data != null)
                    {
                        voidShards += enemy.Data.VoidShardReward;
                    }
                }
            }

            EventBus.Publish(new CombatEndedEvent(victory));
            Debug.Log($"[TurnManager] Combat ended: {(victory ? "Victory" : "Defeat")}, Shards: {voidShards}");
        }

        // ============================================
        // Card Playing
        // ============================================

        /// <summary>
        /// Attempt to play a card on a target.
        /// </summary>
        /// <param name="card">Card to play</param>
        /// <param name="target">Target for the card (can be null)</param>
        /// <returns>True if card was played successfully</returns>
        public bool TryPlayCard(Card card, ICombatTarget target)
        {
            if (CurrentPhase != CombatPhase.PlayerPhase)
            {
                Debug.LogWarning("[TurnManager] Cannot play card - not in player phase");
                return false;
            }

            if (card?.CardInstance == null)
            {
                Debug.LogWarning("[TurnManager] Cannot play card - null card or instance");
                return false;
            }

            var instance = card.CardInstance;

            if (!instance.CanPlay(_context.CurrentAP))
            {
                Debug.LogWarning($"[TurnManager] Cannot play card - not enough AP ({_context.CurrentAP}/{instance.CurrentCost})");
                return false;
            }

            // Spend AP
            _context.CurrentAP -= instance.CurrentCost;
            EventBus.Publish(new APChangedEvent(_context.CurrentAP, _context.MaxAP));

            // Trigger animation on the card owner's visual
            PlayCardAnimation(instance);

            // Execute card effects via CardExecutor
            if (ServiceLocator.TryGet<CardExecutor>(out var executor))
            {
                // For cards targeting all enemies/allies, get the full target list
                var targetType = instance.Data.TargetType;
                if (targetType == TargetType.AllEnemies || targetType == TargetType.AllAllies)
                {
                    var targetingSystem = ServiceLocator.TryGet<TargetingSystem>(out var ts) ? ts : null;
                    var allTargets = targetingSystem?.GetAllTargets(targetType) ?? new System.Collections.Generic.List<ICombatTarget>();
                    executor.Execute(instance, allTargets);
                }
                else
                {
                    executor.Execute(instance, target);
                }
            }
            else
            {
                Debug.LogWarning("[TurnManager] CardExecutor not found - effects not executed");
            }

            // Remove from hand
            _context.HandManager?.RemoveCard(card);

            // Handle card after play: Power cards exhaust, others discard
            if (instance.Data.CardType == CardType.Power)
            {
                _context.DeckManager?.Exhaust(instance);
                Debug.Log($"[TurnManager] Power card exhausted: {instance.Data.CardName}");
            }
            else
            {
                _context.DeckManager?.Discard(instance);
            }

            // Publish CardPlayedEvent for SE generation and other subscribers
            EventBus.Publish(new CardPlayedEvent(instance, target));

            Debug.Log($"[TurnManager] Played card: {instance.Data.CardName} (Cost: {instance.CurrentCost}, AP remaining: {_context.CurrentAP})");
            return true;
        }

        // ============================================
        // Damage/Healing
        // ============================================

        /// <summary>
        /// Apply damage to the team HP pool.
        /// </summary>
        /// <param name="amount">Damage amount</param>
        public void DamageTeam(int amount)
        {
            int blocked = Mathf.Min(amount, _context.TeamBlock);
            int actualDamage = amount - blocked;

            _context.TeamBlock -= blocked;
            _context.TeamHP = Mathf.Max(0, _context.TeamHP - actualDamage);

            if (blocked > 0)
            {
                EventBus.Publish(new BlockChangedEvent(_context.TeamBlock, _context.TeamBlock + blocked));
            }

            EventBus.Publish(new TeamHPChangedEvent(_context.TeamHP, _context.TeamMaxHP, -actualDamage));

            Debug.Log($"[TurnManager] Team took {actualDamage} damage ({blocked} blocked). HP: {_context.TeamHP}/{_context.TeamMaxHP}");

            if (_context.TeamHP <= 0)
            {
                TransitionToPhase(CombatPhase.Defeat);
            }
        }

        /// <summary>
        /// Add block to the team.
        /// </summary>
        /// <param name="amount">Block amount</param>
        public void AddTeamBlock(int amount)
        {
            int previousBlock = _context.TeamBlock;
            _context.TeamBlock += amount;

            EventBus.Publish(new BlockChangedEvent(_context.TeamBlock, previousBlock));
            Debug.Log($"[TurnManager] Team gained {amount} block. Total: {_context.TeamBlock}");
        }

        /// <summary>
        /// Heal the team.
        /// </summary>
        /// <param name="amount">Heal amount</param>
        public void HealTeam(int amount)
        {
            int previousHP = _context.TeamHP;
            _context.TeamHP = Mathf.Min(_context.TeamHP + amount, _context.TeamMaxHP);
            int actualHeal = _context.TeamHP - previousHP;

            EventBus.Publish(new TeamHPChangedEvent(_context.TeamHP, _context.TeamMaxHP, actualHeal));
            Debug.Log($"[TurnManager] Team healed {actualHeal}. HP: {_context.TeamHP}/{_context.TeamMaxHP}");
        }

        // ============================================
        // AP Management
        // ============================================

        /// <summary>
        /// Add AP to the current turn.
        /// </summary>
        /// <param name="amount">AP amount to add</param>
        public void AddAP(int amount)
        {
            if (amount <= 0) return;

            _context.CurrentAP += amount;
            EventBus.Publish(new APChangedEvent(_context.CurrentAP, _context.MaxAP));
            Debug.Log($"[TurnManager] Gained {amount} AP. Current: {_context.CurrentAP}/{_context.MaxAP}");
        }

        // ============================================
        // Card Draw
        // ============================================

        /// <summary>
        /// Draw cards from the deck (immediate, no animation delay).
        /// Use DrawCardsSequential for animated drawing.
        /// </summary>
        /// <param name="count">Number of cards to draw</param>
        public void DrawCards(int count)
        {
            if (count <= 0) return;

            if (_context.DeckManager == null)
            {
                Debug.LogWarning("[TurnManager] DeckManager not available for card draw");
                return;
            }

            for (int i = 0; i < count; i++)
            {
                var card = _context.DeckManager.Draw();
                if (card != null && _context.HandManager != null)
                {
                    _context.HandManager.AddCard(card);
                }
            }

            Debug.Log($"[TurnManager] Drew {count} cards");
        }

        /// <summary>
        /// Draw cards sequentially with animation delays.
        /// Handles reshuffle mid-draw gracefully with additional delay.
        /// </summary>
        /// <param name="count">Number of cards to draw</param>
        /// <param name="onComplete">Callback when drawing is complete</param>
        public void DrawCardsSequential(int count, Action onComplete = null)
        {
            if (_drawCoroutine != null)
            {
                StopCoroutine(_drawCoroutine);
            }
            _drawCoroutine = StartCoroutine(DrawCardsSequentialCoroutine(count, onComplete));
        }

        /// <summary>
        /// Coroutine that draws cards one at a time with delays.
        /// </summary>
        private IEnumerator DrawCardsSequentialCoroutine(int count, Action onComplete)
        {
            if (count <= 0 || _context.DeckManager == null)
            {
                onComplete?.Invoke();
                yield break;
            }

            _reshuffleOccurred = false;
            int cardsDrawn = 0;

            for (int i = 0; i < count; i++)
            {
                // Check if reshuffle occurred before this draw
                bool wasReshuffled = _reshuffleOccurred;
                _reshuffleOccurred = false;

                // Draw the card
                var card = _context.DeckManager.Draw();

                // If reshuffle happened during Draw(), add extra delay
                if (_reshuffleOccurred)
                {
                    Debug.Log("[TurnManager] Reshuffle detected - adding delay before continuing draw");
                    yield return new WaitForSeconds(_reshuffleDelay);
                }

                if (card != null && _context.HandManager != null)
                {
                    _context.HandManager.AddCard(card);
                    cardsDrawn++;

                    // Wait between card draws (except after last card)
                    if (i < count - 1)
                    {
                        yield return new WaitForSeconds(_cardDrawDelay);
                    }
                }
                else
                {
                    // No more cards available
                    Debug.Log($"[TurnManager] No more cards to draw after {cardsDrawn} cards");
                    break;
                }
            }

            Debug.Log($"[TurnManager] Sequential draw complete: {cardsDrawn} cards");
            _drawCoroutine = null;
            onComplete?.Invoke();
        }

        /// <summary>
        /// Check if card drawing is in progress.
        /// </summary>
        public bool IsDrawingCards => _drawCoroutine != null;

        // ============================================
        // Soul Essence
        // ============================================

        /// <summary>
        /// Add Soul Essence to the active Requiem.
        /// </summary>
        /// <param name="amount">SE amount to add</param>
        public void AddSoulEssence(int amount)
        {
            if (amount <= 0) return;

            _context.SoulEssence += amount;
            EventBus.Publish(new SoulEssenceChangedEvent(_context.SoulEssence, _context.SoulEssence - amount));
            Debug.Log($"[TurnManager] Gained {amount} Soul Essence. Current: {_context.SoulEssence}");
        }

        // ============================================
        // Animation
        // ============================================

        /// <summary>
        /// Play animation on the card owner's visual based on card type.
        /// </summary>
        /// <param name="cardInstance">The card being played</param>
        private void PlayCardAnimation(CardInstance cardInstance)
        {
            if (cardInstance?.Data == null)
            {
                Debug.LogWarning("[TurnManager] PlayCardAnimation - cardInstance or Data is null");
                return;
            }

            Debug.Log($"[TurnManager] PlayCardAnimation called for: {cardInstance.Data.CardName}, Type: {cardInstance.Data.CardType}");

            // Find the RequiemInstance that owns this card
            RequiemInstance owner = null;
            var ownerData = cardInstance.Data.Owner;

            Debug.Log($"[TurnManager] Card owner data: {(ownerData != null ? ownerData.RequiemName : "NULL (neutral card)")}");

            if (ownerData != null)
            {
                // Find matching RequiemInstance in team
                foreach (var requiem in _context.Team)
                {
                    Debug.Log($"[TurnManager] Checking team member: {requiem.Name}, Data match: {requiem.Data == ownerData}");
                    if (requiem.Data == ownerData)
                    {
                        owner = requiem;
                        break;
                    }
                }
            }

            // If no owner found (neutral card), use first team member
            if (owner == null && _context.Team.Count > 0)
            {
                owner = _context.Team[0];
                Debug.Log($"[TurnManager] Using first team member as fallback: {owner.Name}");
            }

            if (owner == null)
            {
                Debug.LogWarning($"[TurnManager] No owner found for card: {cardInstance.Data.CardName}");
                return;
            }

            Debug.Log($"[TurnManager] Owner found: {owner.Name}, Visual: {(owner.Visual != null ? "EXISTS" : "NULL")}");

            if (owner.Visual == null)
            {
                Debug.LogWarning($"[TurnManager] No visual found for card animation: {cardInstance.Data.CardName}, Owner: {owner.Name}");
                return;
            }

            // Play animation based on card type
            switch (cardInstance.Data.CardType)
            {
                case CardType.Strike:
                    owner.Visual.PlayAttack(owner.Data?.PreferredAttackType ?? AttackType.Slash);
                    Debug.Log($"[TurnManager] {owner.Name} plays attack animation for {cardInstance.Data.CardName}");
                    break;

                case CardType.Guard:
                    owner.Visual.PlayBlock();
                    Debug.Log($"[TurnManager] {owner.Name} plays block animation for {cardInstance.Data.CardName}");
                    break;

                case CardType.Skill:
                case CardType.Power:
                    owner.Visual.PlaySkill();
                    Debug.Log($"[TurnManager] {owner.Name} plays skill animation for {cardInstance.Data.CardName}");
                    break;
            }
        }
    }
}
