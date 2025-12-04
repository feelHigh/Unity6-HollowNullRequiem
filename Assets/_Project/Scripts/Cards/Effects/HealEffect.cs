// ============================================
// HealEffect.cs
// Healing effect implementations
// ============================================

using UnityEngine;
using HNR.Core.Events;
using HNR.Combat;

namespace HNR.Cards
{
    /// <summary>
    /// Heal flat HP amount to the team.
    /// </summary>
    public class HealEffect : ICardEffect
    {
        public void Execute(CardEffectData data, EffectContext context)
        {
            int healAmount = data.Value;

            // Apply heal multiplier from context (buffs like Regeneration boost)
            healAmount = context.CalculateHeal(healAmount);

            // Heal through TurnManager if available
            if (context.TurnManager != null)
            {
                context.TurnManager.HealTeam(healAmount);
            }
            else if (context.CombatContext != null)
            {
                // Direct heal if no TurnManager
                int previousHP = context.CombatContext.TeamHP;
                context.CombatContext.TeamHP = Mathf.Min(
                    context.CombatContext.TeamHP + healAmount,
                    context.CombatContext.TeamMaxHP
                );
                int actualHeal = context.CombatContext.TeamHP - previousHP;

                EventBus.Publish(new TeamHPChangedEvent(
                    context.CombatContext.TeamHP,
                    context.CombatContext.TeamMaxHP,
                    actualHeal
                ));
            }

            // Publish healing event (null target for team-wide heal)
            EventBus.Publish(new HealingReceivedEvent(null, healAmount));

            Debug.Log($"[HealEffect] Healed {healAmount} HP");
        }
    }

    /// <summary>
    /// Heal percentage of max HP to the team.
    /// Value represents percentage (e.g., 25 = 25% of max HP).
    /// </summary>
    public class HealPercentEffect : ICardEffect
    {
        public void Execute(CardEffectData data, EffectContext context)
        {
            if (context.CombatContext == null && context.TurnManager == null)
            {
                Debug.LogWarning("[HealPercentEffect] No combat context available");
                return;
            }

            // Get max HP from context
            int maxHP = context.CombatContext?.TeamMaxHP ??
                        context.TurnManager?.Context?.TeamMaxHP ?? 0;

            if (maxHP <= 0)
            {
                Debug.LogWarning("[HealPercentEffect] Invalid max HP");
                return;
            }

            // Calculate heal amount from percentage
            float percent = data.Value / 100f;
            int healAmount = Mathf.RoundToInt(maxHP * percent);

            // Apply heal multiplier from context
            healAmount = context.CalculateHeal(healAmount);

            // Heal through TurnManager if available
            if (context.TurnManager != null)
            {
                context.TurnManager.HealTeam(healAmount);
            }
            else if (context.CombatContext != null)
            {
                int previousHP = context.CombatContext.TeamHP;
                context.CombatContext.TeamHP = Mathf.Min(
                    context.CombatContext.TeamHP + healAmount,
                    context.CombatContext.TeamMaxHP
                );
                int actualHeal = context.CombatContext.TeamHP - previousHP;

                EventBus.Publish(new TeamHPChangedEvent(
                    context.CombatContext.TeamHP,
                    context.CombatContext.TeamMaxHP,
                    actualHeal
                ));
            }

            // Publish healing event
            EventBus.Publish(new HealingReceivedEvent(null, healAmount));

            Debug.Log($"[HealPercentEffect] Healed {data.Value}% ({healAmount} HP)");
        }
    }
}
