// ============================================
// CardExecutor.cs
// Executes card effects with registered handlers
// ============================================

using System.Collections.Generic;
using UnityEngine;
using HNR.Core;
using HNR.Core.Events;
using HNR.Combat;
using HNR.Characters;

// Resolve ambiguity: use real RequiemDataSO from Characters
using RequiemDataSO = HNR.Characters.RequiemDataSO;

namespace HNR.Cards
{
    /// <summary>
    /// Executes card effects using registered effect handlers.
    /// Central hub for all card effect execution.
    /// </summary>
    public class CardExecutor : MonoBehaviour
    {
        // ============================================
        // Effect Handlers
        // ============================================

        private Dictionary<EffectType, ICardEffect> _effectHandlers;

        // ============================================
        // Unity Lifecycle
        // ============================================

        private void Awake()
        {
            InitializeEffectHandlers();
            ServiceLocator.Register(this);
        }

        private void OnDestroy()
        {
            ServiceLocator.Unregister<CardExecutor>();
        }

        // ============================================
        // Initialization
        // ============================================

        private void InitializeEffectHandlers()
        {
            _effectHandlers = new Dictionary<EffectType, ICardEffect>
            {
                // Damage
                { EffectType.Damage, new DamageEffect() },
                { EffectType.DamageMultiple, new DamageMultipleEffect() },

                // Defense
                { EffectType.Block, new BlockEffect() },

                // Healing
                { EffectType.Heal, new HealEffect() },
                { EffectType.HealPercent, new HealPercentEffect() },

                // Status Effects
                { EffectType.ApplyBurn, new ApplyStatusEffect(StatusType.Burn) },
                { EffectType.ApplyPoison, new ApplyStatusEffect(StatusType.Poison) },
                { EffectType.ApplyWeakness, new ApplyStatusEffect(StatusType.Weakness) },
                { EffectType.ApplyVulnerability, new ApplyStatusEffect(StatusType.Vulnerability) },
                { EffectType.ApplyStun, new ApplyStatusEffect(StatusType.Stun) },

                // Card Manipulation
                { EffectType.DrawCards, new DrawCardsEffect() },
                { EffectType.DiscardRandom, new DiscardRandomEffect() },
                { EffectType.Exhaust, new ExhaustEffect() },

                // Resources
                { EffectType.GainAP, new GainAPEffect() },
                { EffectType.GainSE, new GainSEEffect() },

                // Corruption
                { EffectType.CorruptionGain, new CorruptionEffect(true) },
                { EffectType.CorruptionReduce, new CorruptionEffect(false) }
            };
        }

        // ============================================
        // Execution
        // ============================================

        /// <summary>
        /// Execute all effects of a card on a target.
        /// </summary>
        /// <param name="card">Card instance to execute</param>
        /// <param name="target">Primary target (can be null for untargeted cards)</param>
        public void Execute(CardInstance card, ICombatTarget target)
        {
            if (card?.Data == null)
            {
                Debug.LogWarning("[CardExecutor] Cannot execute null card");
                return;
            }

            // Build execution context
            var context = BuildContext(card, target);

            Debug.Log($"[CardExecutor] Executing card: {card.Data.CardName} with {card.Data.Effects.Count} effects");

            // Execute each effect in sequence
            foreach (var effectData in card.Data.Effects)
            {
                ExecuteEffect(effectData, context);
            }

            // Publish card played event
            EventBus.Publish(new CardPlayedEvent(card, target as ICombatTarget));
        }

        /// <summary>
        /// Execute all effects of a card on multiple targets.
        /// </summary>
        /// <param name="card">Card instance to execute</param>
        /// <param name="targets">List of targets</param>
        public void Execute(CardInstance card, List<ICombatTarget> targets)
        {
            if (card?.Data == null)
            {
                Debug.LogWarning("[CardExecutor] Cannot execute null card");
                return;
            }

            var primaryTarget = targets.Count > 0 ? targets[0] : null;
            var context = BuildContext(card, primaryTarget);
            context.AllTargets = targets;

            Debug.Log($"[CardExecutor] Executing card: {card.Data.CardName} on {targets.Count} targets");

            foreach (var effectData in card.Data.Effects)
            {
                ExecuteEffect(effectData, context);
            }

            EventBus.Publish(new CardPlayedEvent(card, primaryTarget as ICombatTarget));
        }

        /// <summary>
        /// Build execution context from card and target.
        /// </summary>
        private EffectContext BuildContext(CardInstance card, ICombatTarget target)
        {
            var turnManager = ServiceLocator.TryGet<TurnManager>(out var tm) ? tm : null;
            var deckManager = ServiceLocator.TryGet<DeckManager>(out var dm) ? dm : null;

            return new EffectContext
            {
                Card = card,
                Source = card.Data.Owner,
                Target = target,
                TurnManager = turnManager,
                CombatContext = turnManager?.Context,
                DeckManager = deckManager,
                DamageMultiplier = 1f,
                BlockMultiplier = 1f,
                HealMultiplier = 1f
            };
        }

        /// <summary>
        /// Execute a single effect with the given context.
        /// </summary>
        private void ExecuteEffect(CardEffectData effectData, EffectContext context)
        {
            if (_effectHandlers.TryGetValue(effectData.EffectType, out var handler))
            {
                handler.Execute(effectData, context);
            }
            else
            {
                Debug.LogWarning($"[CardExecutor] No handler registered for effect type: {effectData.EffectType}");
            }
        }

        // ============================================
        // Handler Registration
        // ============================================

        /// <summary>
        /// Register a custom effect handler.
        /// </summary>
        /// <param name="type">Effect type to handle</param>
        /// <param name="handler">Handler implementation</param>
        public void RegisterHandler(EffectType type, ICardEffect handler)
        {
            _effectHandlers[type] = handler;
            Debug.Log($"[CardExecutor] Registered handler for: {type}");
        }

        /// <summary>
        /// Check if a handler is registered for an effect type.
        /// </summary>
        /// <param name="type">Effect type to check</param>
        /// <returns>True if handler is registered</returns>
        public bool HasHandler(EffectType type)
        {
            return _effectHandlers.ContainsKey(type);
        }
    }
}
