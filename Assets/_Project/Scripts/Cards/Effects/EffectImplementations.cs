// ============================================
// EffectImplementations.cs
// Concrete implementations of card effects
// ============================================

using UnityEngine;
using HNR.Core;
using HNR.Core.Events;
using HNR.Combat;
using HNR.Characters;

// Resolve ambiguity: use real RequiemDataSO from Characters
using RequiemDataSO = HNR.Characters.RequiemDataSO;

namespace HNR.Cards
{
    // ============================================
    // NOTE: Core effects are now in separate files:
    // - DamageEffect.cs (DamageEffect, DamageMultipleEffect)
    // - BlockEffect.cs (BlockEffect)
    // - HealEffect.cs (HealEffect, HealPercentEffect)
    // - ApplyStatusEffect.cs (ApplyStatusEffect, RemoveStatusEffect)
    // ============================================

    // ============================================
    // CARD MANIPULATION EFFECTS
    // ============================================

    /// <summary>
    /// Draw cards from deck.
    /// </summary>
    public class DrawCardsEffect : ICardEffect
    {
        public void Execute(CardEffectData data, EffectContext context)
        {
            int cardsToDraw = data.Value;

            if (context.DeckManager != null && context.CombatContext?.HandManager != null)
            {
                // DrawCards handles reshuffling and publishes CardDrawnEvent internally
                var drawnCards = context.DeckManager.DrawCards(cardsToDraw);
                foreach (var card in drawnCards)
                {
                    context.CombatContext.HandManager.AddCard(card);
                }
            }

            Debug.Log($"[DrawCardsEffect] Drew {cardsToDraw} cards");
        }
    }

    /// <summary>
    /// Discard random cards from hand.
    /// </summary>
    public class DiscardRandomEffect : ICardEffect
    {
        public void Execute(CardEffectData data, EffectContext context)
        {
            int cardsToDiscard = data.Value;

            // TODO: Implement random discard from hand
            Debug.Log($"[DiscardRandomEffect] Discarding {cardsToDiscard} random cards");
        }
    }

    /// <summary>
    /// Exhaust the played card (remove from combat).
    /// </summary>
    public class ExhaustEffect : ICardEffect
    {
        public void Execute(CardEffectData data, EffectContext context)
        {
            if (context.Card != null && context.DeckManager != null)
            {
                context.DeckManager.Exhaust(context.Card);
                EventBus.Publish(new CardDiscardedEvent(context.Card, exhausted: true));
            }

            Debug.Log($"[ExhaustEffect] Card exhausted");
        }
    }

    // ============================================
    // RESOURCE EFFECTS
    // ============================================

    /// <summary>
    /// Gain Action Points this turn.
    /// </summary>
    public class GainAPEffect : ICardEffect
    {
        public void Execute(CardEffectData data, EffectContext context)
        {
            if (context.CombatContext != null)
            {
                context.CombatContext.CurrentAP += data.Value;
                EventBus.Publish(new APChangedEvent(
                    context.CombatContext.CurrentAP,
                    context.CombatContext.MaxAP
                ));
            }

            Debug.Log($"[GainAPEffect] Gained {data.Value} AP");
        }
    }

    /// <summary>
    /// Gain Soul Essence resource.
    /// </summary>
    public class GainSEEffect : ICardEffect
    {
        public void Execute(CardEffectData data, EffectContext context)
        {
            if (context.CombatContext != null)
            {
                context.CombatContext.SoulEssence += data.Value;
                EventBus.Publish(new SoulEssenceChangedEvent(
                    context.CombatContext.SoulEssence - data.Value,
                    context.CombatContext.SoulEssence
                ));
            }

            Debug.Log($"[GainSEEffect] Gained {data.Value} Soul Essence");
        }
    }

    // ============================================
    // CORRUPTION EFFECTS
    // ============================================

    /// <summary>
    /// Modify Corruption on the source Requiem.
    /// </summary>
    public class CorruptionEffect : ICardEffect
    {
        private readonly bool _isGain;

        public CorruptionEffect(bool isGain)
        {
            _isGain = isGain;
        }

        public void Execute(CardEffectData data, EffectContext context)
        {
            // TODO: Implement Corruption system on RequiemInstance
            string action = _isGain ? "Gained" : "Reduced";
            Debug.Log($"[CorruptionEffect] {action} {data.Value} Corruption");

            // When corruption system is implemented:
            // EventBus.Publish(new CorruptionChangedEvent(requiem, previousValue, newValue));
        }
    }
}
