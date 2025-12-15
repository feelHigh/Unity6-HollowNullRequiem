// ============================================
// CombatManager.cs
// High-level facade for combat operations
// ============================================

using System.Collections.Generic;
using UnityEngine;
using HNR.Core;
using HNR.Core.Events;
using HNR.Cards;
using HNR.Characters;

namespace HNR.Combat
{
    /// <summary>
    /// High-level facade for combat operations.
    /// Provides simplified API for common combat actions.
    /// </summary>
    public class CombatManager : MonoBehaviour
    {
        // ============================================
        // Singleton
        // ============================================

        private static CombatManager _instance;
        public static CombatManager Instance => _instance;

        // ============================================
        // Manager References
        // ============================================

        private TurnManager _turnManager;
        private DeckManager _deckManager;
        private HandManager _handManager;
        private StatusEffectManager _statusManager;
        private SoulEssenceManager _seManager;
        private EncounterManager _encounterManager;
        private CardExecutor _cardExecutor;
        private RequiemArtExecutor _artExecutor;

        // ============================================
        // Properties
        // ============================================

        /// <summary>Current combat context.</summary>
        public CombatContext Context => _turnManager?.Context;

        /// <summary>Whether combat is currently active.</summary>
        public bool IsCombatActive => Context != null && !Context.CombatEnded;

        /// <summary>Whether it's currently the player's turn.</summary>
        public bool IsPlayerTurn => Context?.IsPlayerTurn ?? false;

        /// <summary>Current combat phase.</summary>
        public CombatPhase CurrentPhase => _turnManager?.CurrentPhase ?? CombatPhase.Setup;

        /// <summary>Current turn number.</summary>
        public int TurnNumber => Context?.TurnNumber ?? 0;

        /// <summary>Current team HP.</summary>
        public int TeamHP => Context?.TeamHP ?? 0;

        /// <summary>Maximum team HP.</summary>
        public int TeamMaxHP => Context?.TeamMaxHP ?? 0;

        /// <summary>Current team block.</summary>
        public int TeamBlock => Context?.TeamBlock ?? 0;

        /// <summary>Current AP.</summary>
        public int CurrentAP => Context?.CurrentAP ?? 0;

        /// <summary>Maximum AP.</summary>
        public int MaxAP => Context?.MaxAP ?? 3;

        /// <summary>Current Soul Essence.</summary>
        public int SoulEssence => _seManager?.CurrentSE ?? 0;

        // ============================================
        // Unity Lifecycle
        // ============================================

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            ServiceLocator.Register(this);
        }

        private void Start()
        {
            CacheManagerReferences();
        }

        private void OnDestroy()
        {
            if (_instance == this)
            {
                ServiceLocator.Unregister<CombatManager>();
                _instance = null;
            }
        }

        private void CacheManagerReferences()
        {
            ServiceLocator.TryGet(out _turnManager);
            ServiceLocator.TryGet(out _deckManager);
            ServiceLocator.TryGet(out _handManager);
            ServiceLocator.TryGet(out _statusManager);
            ServiceLocator.TryGet(out _seManager);
            ServiceLocator.TryGet(out _encounterManager);
            ServiceLocator.TryGet(out _cardExecutor);
            ServiceLocator.TryGet(out _artExecutor);
        }

        // ============================================
        // Combat Lifecycle
        // ============================================

        /// <summary>
        /// Start a new combat encounter.
        /// </summary>
        /// <param name="team">Player's team of Requiems.</param>
        /// <param name="enemies">Enemy instances to fight.</param>
        public void StartCombat(List<RequiemInstance> team, List<EnemyInstance> enemies)
        {
            if (_turnManager == null)
            {
                Debug.LogError("[CombatManager] TurnManager not found");
                return;
            }

            _turnManager.StartCombat(team, enemies);
            Debug.Log($"[CombatManager] Combat started with {team.Count} allies vs {enemies.Count} enemies");
        }

        /// <summary>
        /// Start combat using Team wrapper.
        /// </summary>
        /// <param name="team">Player's team.</param>
        /// <param name="enemies">Enemy instances.</param>
        public void StartCombat(Team team, List<EnemyInstance> enemies)
        {
            StartCombat(team.ToList(), enemies);
        }

        /// <summary>
        /// End the player's turn.
        /// </summary>
        public void EndPlayerTurn()
        {
            if (!IsPlayerTurn)
            {
                Debug.LogWarning("[CombatManager] Cannot end turn - not player's turn");
                return;
            }

            _turnManager?.EndPlayerTurn();
        }

        /// <summary>
        /// Force end combat with specified result.
        /// </summary>
        /// <param name="victory">Whether player won.</param>
        public void ForceCombatEnd(bool victory)
        {
            if (Context != null)
            {
                Context.CombatEnded = true;
                Context.PlayerVictory = victory;
                EventBus.Publish(new CombatEndedEvent(victory));
            }
        }

        // ============================================
        // Card Operations
        // ============================================

        /// <summary>
        /// Attempt to play a card on a target.
        /// </summary>
        /// <param name="card">Card visual component to play.</param>
        /// <param name="target">Target for the card.</param>
        /// <returns>True if card was played successfully.</returns>
        public bool TryPlayCard(Card card, ICombatTarget target = null)
        {
            if (!IsPlayerTurn)
            {
                Debug.LogWarning("[CombatManager] Cannot play card - not player's turn");
                return false;
            }

            if (_turnManager == null) return false;

            return _turnManager.TryPlayCard(card, target);
        }

        /// <summary>
        /// Draw cards from the deck.
        /// </summary>
        /// <param name="count">Number of cards to draw.</param>
        /// <returns>List of drawn cards.</returns>
        public List<CardInstance> DrawCards(int count)
        {
            if (_deckManager == null) return new List<CardInstance>();

            var drawnCards = new List<CardInstance>();
            for (int i = 0; i < count; i++)
            {
                var card = _deckManager.Draw();
                if (card != null)
                {
                    drawnCards.Add(card);
                }
            }
            return drawnCards;
        }

        /// <summary>
        /// Discard a card from hand.
        /// </summary>
        /// <param name="card">Card to discard.</param>
        public void DiscardCard(CardInstance card)
        {
            _deckManager?.Discard(card);
        }

        /// <summary>
        /// Check if a card can be played.
        /// </summary>
        /// <param name="card">Card to check.</param>
        /// <returns>True if card can be played.</returns>
        public bool CanPlayCard(CardInstance card)
        {
            if (card == null || !IsPlayerTurn) return false;
            return CurrentAP >= card.CurrentCost;
        }

        // ============================================
        // Damage & Healing
        // ============================================

        /// <summary>
        /// Deal damage to a target.
        /// </summary>
        /// <param name="target">Target to damage.</param>
        /// <param name="amount">Base damage amount.</param>
        /// <param name="source">Source of damage (optional).</param>
        public void DealDamage(ICombatTarget target, int amount, ICombatTarget source = null)
        {
            if (target == null) return;
            target.TakeDamage(amount);
        }

        /// <summary>
        /// Deal damage to all enemies.
        /// </summary>
        /// <param name="amount">Base damage amount.</param>
        /// <param name="source">Source of damage.</param>
        public void DealDamageToAllEnemies(int amount, ICombatTarget source = null)
        {
            if (Context?.Enemies == null) return;

            foreach (var enemy in Context.Enemies)
            {
                if (enemy != null && !enemy.IsDead)
                {
                    DealDamage(enemy, amount, source);
                }
            }
        }

        /// <summary>
        /// Deal damage to the team.
        /// </summary>
        /// <param name="amount">Damage amount.</param>
        public void DealDamageToTeam(int amount)
        {
            if (Context == null) return;

            // Apply block first
            int remainingDamage = amount;
            if (Context.TeamBlock > 0)
            {
                int blocked = Mathf.Min(Context.TeamBlock, remainingDamage);
                Context.TeamBlock -= blocked;
                remainingDamage -= blocked;
            }

            if (remainingDamage > 0)
            {
                Context.TeamHP = Mathf.Max(0, Context.TeamHP - remainingDamage);
                EventBus.Publish(new TeamHPChangedEvent(Context.TeamHP, Context.TeamMaxHP));
            }
        }

        /// <summary>
        /// Heal the team.
        /// </summary>
        /// <param name="amount">Heal amount.</param>
        public void HealTeam(int amount)
        {
            if (Context == null) return;

            Context.TeamHP = Mathf.Min(Context.TeamMaxHP, Context.TeamHP + amount);
            EventBus.Publish(new TeamHPChangedEvent(Context.TeamHP, Context.TeamMaxHP));
        }

        /// <summary>
        /// Add block to the team.
        /// </summary>
        /// <param name="amount">Block amount.</param>
        public void AddBlock(int amount)
        {
            if (Context == null) return;

            Context.TeamBlock += amount;
            EventBus.Publish(new BlockChangedEvent(Context.TeamBlock));
        }

        // ============================================
        // Status Effects
        // ============================================

        /// <summary>
        /// Apply a status effect to a target.
        /// </summary>
        /// <param name="target">Target to apply status to.</param>
        /// <param name="statusType">Type of status.</param>
        /// <param name="stacks">Number of stacks.</param>
        /// <param name="duration">Duration in turns (0 for permanent).</param>
        public void ApplyStatus(ICombatTarget target, StatusType statusType, int stacks, int duration = 0)
        {
            _statusManager?.ApplyStatus(target, statusType, stacks, duration);
        }

        /// <summary>
        /// Remove a status effect from a target.
        /// </summary>
        /// <param name="target">Target to remove status from.</param>
        /// <param name="statusType">Type of status to remove.</param>
        public void RemoveStatus(ICombatTarget target, StatusType statusType)
        {
            _statusManager?.RemoveStatus(target, statusType);
        }

        /// <summary>
        /// Check if target has a status effect.
        /// </summary>
        /// <param name="target">Target to check.</param>
        /// <param name="statusType">Status type to check for.</param>
        /// <returns>True if target has the status.</returns>
        public bool HasStatus(ICombatTarget target, StatusType statusType)
        {
            return _statusManager?.HasStatus(target, statusType) ?? false;
        }

        /// <summary>
        /// Get stack count for a status on a target.
        /// </summary>
        /// <param name="target">Target to check.</param>
        /// <param name="statusType">Status type to check.</param>
        /// <returns>Number of stacks (0 if none).</returns>
        public int GetStatusStacks(ICombatTarget target, StatusType statusType)
        {
            return _statusManager?.GetStatusStacks(target, statusType) ?? 0;
        }

        // ============================================
        // Soul Essence & Requiem Arts
        // ============================================

        /// <summary>
        /// Add Soul Essence.
        /// </summary>
        /// <param name="amount">Amount to add.</param>
        public void AddSoulEssence(int amount)
        {
            _seManager?.AddSoulEssence(amount);
        }

        /// <summary>
        /// Spend Soul Essence.
        /// </summary>
        /// <param name="amount">Amount to spend.</param>
        /// <returns>True if had enough to spend.</returns>
        public bool SpendSoulEssence(int amount)
        {
            return _seManager?.SpendSoulEssence(amount) ?? false;
        }

        /// <summary>
        /// Check if a Requiem can activate their Art.
        /// </summary>
        /// <param name="requiem">Requiem to check.</param>
        /// <returns>True if Art can be activated.</returns>
        public bool CanActivateArt(RequiemInstance requiem)
        {
            return _seManager?.CanActivateArt(requiem) ?? false;
        }

        /// <summary>
        /// Attempt to activate a Requiem's Art.
        /// </summary>
        /// <param name="requiem">Requiem to activate Art for.</param>
        /// <returns>True if Art was activated.</returns>
        public bool TryActivateArt(RequiemInstance requiem)
        {
            return _seManager?.TryActivateArt(requiem) ?? false;
        }

        // ============================================
        // Queries
        // ============================================

        /// <summary>
        /// Get all alive enemies.
        /// </summary>
        /// <returns>List of alive enemies.</returns>
        public List<EnemyInstance> GetAliveEnemies()
        {
            if (Context?.Enemies == null) return new List<EnemyInstance>();
            return Context.Enemies.FindAll(e => e != null && !e.IsDead);
        }

        /// <summary>
        /// Get the team as a Team wrapper.
        /// </summary>
        /// <returns>Team wrapper or null.</returns>
        public Team GetTeam()
        {
            if (Context?.Team == null) return null;
            return Team.FromList(Context.Team);
        }

        /// <summary>
        /// Get cards in hand.
        /// </summary>
        /// <returns>Read-only list of Card visuals in hand.</returns>
        public IReadOnlyList<Card> GetHand()
        {
            return _handManager?.Hand ?? (IReadOnlyList<Card>)new List<Card>();
        }

        /// <summary>
        /// Get draw pile count.
        /// </summary>
        public int DrawPileCount => _deckManager?.DrawPileCount ?? 0;

        /// <summary>
        /// Get discard pile count.
        /// </summary>
        public int DiscardPileCount => _deckManager?.DiscardPileCount ?? 0;

        /// <summary>
        /// Get exhaust pile count.
        /// </summary>
        public int ExhaustPileCount => _deckManager?.ExhaustPileCount ?? 0;

        // ============================================
        // Utility
        // ============================================

        /// <summary>
        /// Check victory condition (all enemies dead).
        /// </summary>
        /// <returns>True if all enemies are dead.</returns>
        public bool CheckVictory()
        {
            return GetAliveEnemies().Count == 0;
        }

        /// <summary>
        /// Check defeat condition (team HP <= 0).
        /// </summary>
        /// <returns>True if team is defeated.</returns>
        public bool CheckDefeat()
        {
            return TeamHP <= 0;
        }
    }
}
