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
    // DAMAGE EFFECTS
    // ============================================

    /// <summary>
    /// Deal damage to a single target.
    /// </summary>
    public class DamageEffect : ICardEffect
    {
        public void Execute(CardEffectData data, EffectContext context)
        {
            if (context.Target == null)
            {
                Debug.LogWarning("[DamageEffect] No target specified");
                return;
            }

            int damage = context.CalculateDamage(data.Value);
            context.Target.TakeDamage(damage);

            Debug.Log($"[DamageEffect] Dealt {damage} damage to {context.Target.Name}");
        }
    }

    /// <summary>
    /// Deal damage multiple times to a target.
    /// Value = damage per hit, Duration = number of hits.
    /// </summary>
    public class DamageMultipleEffect : ICardEffect
    {
        public void Execute(CardEffectData data, EffectContext context)
        {
            if (context.Target == null)
            {
                Debug.LogWarning("[DamageMultipleEffect] No target specified");
                return;
            }

            int damagePerHit = context.CalculateDamage(data.Value);
            int hits = data.Duration > 0 ? data.Duration : 1;

            for (int i = 0; i < hits; i++)
            {
                context.Target.TakeDamage(damagePerHit);
            }

            Debug.Log($"[DamageMultipleEffect] Dealt {damagePerHit}x{hits} damage to {context.Target.Name}");
        }
    }

    // ============================================
    // DEFENSE EFFECTS
    // ============================================

    /// <summary>
    /// Gain Block for the team.
    /// </summary>
    public class BlockEffect : ICardEffect
    {
        public void Execute(CardEffectData data, EffectContext context)
        {
            int block = context.CalculateBlock(data.Value);

            if (context.TurnManager != null)
            {
                context.TurnManager.AddTeamBlock(block);
            }
            else if (context.CombatContext != null)
            {
                int previousBlock = context.CombatContext.TeamBlock;
                context.CombatContext.TeamBlock += block;
                EventBus.Publish(new BlockChangedEvent(context.CombatContext.TeamBlock, previousBlock));
            }

            Debug.Log($"[BlockEffect] Gained {block} block");
        }
    }

    // ============================================
    // HEALING EFFECTS
    // ============================================

    /// <summary>
    /// Heal flat HP amount.
    /// </summary>
    public class HealEffect : ICardEffect
    {
        public void Execute(CardEffectData data, EffectContext context)
        {
            int heal = context.CalculateHeal(data.Value);

            if (context.TurnManager != null)
            {
                context.TurnManager.HealTeam(heal);
            }
            else if (context.CombatContext != null)
            {
                int previousHP = context.CombatContext.TeamHP;
                context.CombatContext.TeamHP = Mathf.Min(
                    context.CombatContext.TeamHP + heal,
                    context.CombatContext.TeamMaxHP
                );
                int actualHeal = context.CombatContext.TeamHP - previousHP;
                EventBus.Publish(new TeamHPChangedEvent(
                    context.CombatContext.TeamHP,
                    context.CombatContext.TeamMaxHP,
                    actualHeal
                ));
            }

            Debug.Log($"[HealEffect] Healed {heal} HP");
        }
    }

    /// <summary>
    /// Heal percentage of max HP.
    /// </summary>
    public class HealPercentEffect : ICardEffect
    {
        public void Execute(CardEffectData data, EffectContext context)
        {
            if (context.CombatContext == null)
            {
                Debug.LogWarning("[HealPercentEffect] No combat context");
                return;
            }

            int maxHP = context.CombatContext.TeamMaxHP;
            int healAmount = Mathf.RoundToInt(maxHP * (data.Value / 100f));
            healAmount = context.CalculateHeal(healAmount);

            if (context.TurnManager != null)
            {
                context.TurnManager.HealTeam(healAmount);
            }
            else
            {
                int previousHP = context.CombatContext.TeamHP;
                context.CombatContext.TeamHP = Mathf.Min(
                    context.CombatContext.TeamHP + healAmount,
                    maxHP
                );
                int actualHeal = context.CombatContext.TeamHP - previousHP;
                EventBus.Publish(new TeamHPChangedEvent(
                    context.CombatContext.TeamHP,
                    maxHP,
                    actualHeal
                ));
            }

            Debug.Log($"[HealPercentEffect] Healed {data.Value}% ({healAmount} HP)");
        }
    }

    // ============================================
    // STATUS EFFECT APPLICATION
    // ============================================

    /// <summary>
    /// Apply a status effect to target.
    /// </summary>
    public class ApplyStatusEffect : ICardEffect
    {
        private readonly StatusType _statusType;

        public ApplyStatusEffect(StatusType statusType)
        {
            _statusType = statusType;
        }

        public void Execute(CardEffectData data, EffectContext context)
        {
            if (context.Target == null)
            {
                Debug.LogWarning($"[ApplyStatusEffect] No target for {_statusType}");
                return;
            }

            // TODO: Implement StatusEffectManager integration
            // For now, log the intended effect
            Debug.Log($"[ApplyStatusEffect] Applied {data.Value} stacks of {_statusType} to {context.Target.Name} for {data.Duration} turns");

            // Publish event for status application
            // EventBus.Publish(new StatusAppliedEvent(context.Target, _statusType, data.Value, data.Duration));
        }
    }

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
