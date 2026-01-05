// ============================================
// SoulEssenceManager.cs
// Manages Soul Essence accumulation and Requiem Art activation
// ============================================

using UnityEngine;
using HNR.Core;
using HNR.Core.Events;
using HNR.Characters;

namespace HNR.Combat
{
    /// <summary>
    /// Manages Soul Essence accumulation during combat.
    /// Subscribes to combat events and grants SE based on actions.
    /// </summary>
    public class SoulEssenceManager : MonoBehaviour
    {
        // ============================================
        // SE Generation Constants
        // ============================================

        private const int SE_PER_CARD_PLAYED = 2;
        private const int SE_PER_ENEMY_KILLED = 5;
        private const int SE_PER_NULL_STATE_TURN = 3;
        private const int DAMAGE_THRESHOLD_FOR_SE = 10;
        private const int SE_PER_DAMAGE_DEALT = 1;
        private const int SE_PER_DAMAGE_TAKEN = 2;
        private const int MAX_SE = 100;

        // ============================================
        // References
        // ============================================

        private TurnManager _turnManager;

        // ============================================
        // Properties
        // ============================================

        /// <summary>Current Soul Essence in the combat context.</summary>
        public int CurrentSE => _turnManager?.Context?.SoulEssence ?? 0;

        // ============================================
        // Unity Lifecycle
        // ============================================

        private void Awake()
        {
            ServiceLocator.Register(this);
        }

        private void OnEnable()
        {
            EventBus.Subscribe<CardPlayedEvent>(OnCardPlayed);
            EventBus.Subscribe<DamageDealtEvent>(OnDamageDealt);
            EventBus.Subscribe<EnemyDefeatedEvent>(OnEnemyDefeated);
            EventBus.Subscribe<TurnStartedEvent>(OnTurnStarted);
            EventBus.Subscribe<CombatStartedEvent>(OnCombatStarted);
            EventBus.Subscribe<CombatEndedEvent>(OnCombatEnded);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<CardPlayedEvent>(OnCardPlayed);
            EventBus.Unsubscribe<DamageDealtEvent>(OnDamageDealt);
            EventBus.Unsubscribe<EnemyDefeatedEvent>(OnEnemyDefeated);
            EventBus.Unsubscribe<TurnStartedEvent>(OnTurnStarted);
            EventBus.Unsubscribe<CombatStartedEvent>(OnCombatStarted);
            EventBus.Unsubscribe<CombatEndedEvent>(OnCombatEnded);
        }

        private void OnDestroy()
        {
            ServiceLocator.Unregister<SoulEssenceManager>();
        }

        // ============================================
        // Event Handlers
        // ============================================

        private void OnCombatStarted(CombatStartedEvent evt)
        {
            // Cache TurnManager reference
            ServiceLocator.TryGet<TurnManager>(out _turnManager);
        }

        private void OnCombatEnded(CombatEndedEvent evt)
        {
            // SE resets at end of combat (handled by CombatContext.Reset())
            _turnManager = null;
        }

        /// <summary>
        /// Grant SE when a card is played.
        /// </summary>
        private void OnCardPlayed(CardPlayedEvent evt)
        {
            AddSoulEssence(SE_PER_CARD_PLAYED, "Card Played");
        }

        /// <summary>
        /// Grant SE based on damage dealt or taken.
        /// </summary>
        private void OnDamageDealt(DamageDealtEvent evt)
        {
            if (evt.Amount <= 0) return;

            // Check if player dealt damage (source is RequiemInstance or null for card effects)
            bool playerDealtDamage = evt.Source is RequiemInstance ||
                                     (evt.Source == null && evt.Target is EnemyInstance);

            // Check if player took damage (target is RequiemInstance or team damage)
            bool playerTookDamage = evt.Target is RequiemInstance ||
                                    (evt.Target == null && evt.Source is EnemyInstance);

            if (playerDealtDamage)
            {
                // +1 SE per 10 damage dealt
                int seGain = evt.Amount / DAMAGE_THRESHOLD_FOR_SE;
                if (seGain > 0)
                {
                    AddSoulEssence(seGain * SE_PER_DAMAGE_DEALT, "Damage Dealt");
                }
            }
            else if (playerTookDamage)
            {
                // +2 SE per 10 damage taken (incentivizes risk)
                int seGain = evt.Amount / DAMAGE_THRESHOLD_FOR_SE;
                if (seGain > 0)
                {
                    AddSoulEssence(seGain * SE_PER_DAMAGE_TAKEN, "Damage Taken");
                }
            }
        }

        /// <summary>
        /// Grant SE when an enemy is defeated.
        /// </summary>
        private void OnEnemyDefeated(EnemyDefeatedEvent evt)
        {
            AddSoulEssence(SE_PER_ENEMY_KILLED, "Enemy Defeated");
        }

        /// <summary>
        /// Grant SE for each Requiem in Null State at turn start.
        /// </summary>
        private void OnTurnStarted(TurnStartedEvent evt)
        {
            if (!evt.IsPlayerTurn) return;
            if (_turnManager?.Context?.Team == null) return;

            // +3 SE per Requiem in Null State
            foreach (var requiem in _turnManager.Context.Team)
            {
                if (requiem != null && requiem.InNullState)
                {
                    AddSoulEssence(SE_PER_NULL_STATE_TURN, $"{requiem.Name} Null State");
                }
            }
        }

        // ============================================
        // SE Management
        // ============================================

        /// <summary>
        /// Add Soul Essence to the shared combat pool.
        /// </summary>
        /// <param name="amount">Amount to add</param>
        /// <param name="source">Source description for debugging</param>
        public void AddSoulEssence(int amount, string source = "")
        {
            if (amount <= 0) return;
            if (_turnManager?.Context == null) return;

            int previousSE = _turnManager.Context.SoulEssence;
            int newSE = Mathf.Min(previousSE + amount, MAX_SE);
            _turnManager.Context.SoulEssence = newSE;

            int delta = newSE - previousSE;
            if (delta > 0)
            {
                EventBus.Publish(new SoulEssenceChangedEvent(newSE, delta));
                Debug.Log($"[SoulEssenceManager] +{delta} SE ({source}). Total: {newSE}/{MAX_SE}");
            }
        }

        /// <summary>
        /// Spend Soul Essence for Requiem Art activation.
        /// </summary>
        /// <param name="amount">Amount to spend</param>
        /// <returns>True if successful</returns>
        public bool SpendSoulEssence(int amount)
        {
            if (_turnManager?.Context == null) return false;
            if (_turnManager.Context.SoulEssence < amount) return false;

            int previousSE = _turnManager.Context.SoulEssence;
            _turnManager.Context.SoulEssence -= amount;
            int delta = _turnManager.Context.SoulEssence - previousSE;

            EventBus.Publish(new SoulEssenceChangedEvent(
                _turnManager.Context.SoulEssence,
                delta  // Negative delta for SE spent
            ));

            Debug.Log($"[SoulEssenceManager] Spent {amount} SE. Remaining: {_turnManager.Context.SoulEssence}");
            return true;
        }

        // ============================================
        // Requiem Art Activation
        // ============================================

        /// <summary>
        /// Check if a Requiem can activate their Art.
        /// </summary>
        /// <param name="requiem">Requiem to check</param>
        /// <returns>True if Art can be activated</returns>
        public bool CanActivateArt(RequiemInstance requiem)
        {
            if (requiem == null || requiem.Data?.RequiemArt == null) return false;
            if (requiem.HasUsedArtThisCombat) return false;

            var art = requiem.Data.RequiemArt;
            int effectiveCost = art.GetEffectiveCost(requiem.InNullState);

            return CurrentSE >= effectiveCost;
        }

        /// <summary>
        /// Attempt to activate a Requiem's Art.
        /// </summary>
        /// <param name="requiem">Requiem to activate Art for</param>
        /// <returns>True if Art was activated successfully</returns>
        public bool TryActivateArt(RequiemInstance requiem)
        {
            if (!CanActivateArt(requiem))
            {
                Debug.LogWarning($"[SoulEssenceManager] Cannot activate Art for {requiem?.Name}");
                return false;
            }

            var art = requiem.Data.RequiemArt;
            int effectiveCost = art.GetEffectiveCost(requiem.InNullState);

            // Spend SE (free in Null State)
            if (effectiveCost > 0 && !SpendSoulEssence(effectiveCost))
            {
                return false;
            }

            // Mark as used this combat
            requiem.HasUsedArtThisCombat = true;

            // Execute Art effects via RequiemArtExecutor
            if (ServiceLocator.TryGet<RequiemArtExecutor>(out var artExecutor))
            {
                artExecutor.ExecuteArt(requiem, art);
            }
            else
            {
                Debug.LogWarning($"[SoulEssenceManager] RequiemArtExecutor not found, Art effects not executed");
            }

            Debug.Log($"[SoulEssenceManager] {requiem.Name} activated {art.ArtName}!");

            // Publish event
            EventBus.Publish(new RequiemArtActivatedEvent(requiem, art));

            return true;
        }
    }
}
