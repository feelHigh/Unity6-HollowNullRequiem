// ============================================
// ResourceEffects.cs
// Resource manipulation effect implementations
// ============================================

using UnityEngine;
using HNR.Core;
using HNR.Core.Events;
using HNR.Combat;

namespace HNR.Cards
{
    /// <summary>
    /// Gain Action Points this turn.
    /// </summary>
    public class GainAPEffect : ICardEffect
    {
        public void Execute(CardEffectData data, EffectContext context)
        {
            int apGain = data.Value;

            if (context.CombatContext != null)
            {
                context.CombatContext.CurrentAP += apGain;
                EventBus.Publish(new APChangedEvent(
                    context.CombatContext.CurrentAP,
                    context.CombatContext.MaxAP
                ));
            }
            else if (context.TurnManager?.Context != null)
            {
                context.TurnManager.Context.CurrentAP += apGain;
                EventBus.Publish(new APChangedEvent(
                    context.TurnManager.Context.CurrentAP,
                    context.TurnManager.Context.MaxAP
                ));
            }

            Debug.Log($"[GainAPEffect] Gained {apGain} AP");
        }
    }

    /// <summary>
    /// Gain Soul Essence resource.
    /// Soul Essence is used for powerful abilities.
    /// </summary>
    public class GainSEEffect : ICardEffect
    {
        public void Execute(CardEffectData data, EffectContext context)
        {
            int seGain = data.Value;

            if (context.CombatContext != null)
            {
                int previousSE = context.CombatContext.SoulEssence;
                context.CombatContext.SoulEssence += seGain;
                EventBus.Publish(new SoulEssenceChangedEvent(
                    context.CombatContext.SoulEssence,
                    seGain
                ));
            }
            else if (context.TurnManager?.Context != null)
            {
                int previousSE = context.TurnManager.Context.SoulEssence;
                context.TurnManager.Context.SoulEssence += seGain;
                EventBus.Publish(new SoulEssenceChangedEvent(
                    context.TurnManager.Context.SoulEssence,
                    seGain
                ));
            }

            Debug.Log($"[GainSEEffect] Gained {seGain} Soul Essence");
        }
    }

    /// <summary>
    /// Modify Corruption on the source Requiem.
    /// Corruption builds toward Null State (100 = triggers Null State).
    /// </summary>
    public class CorruptionEffect : ICardEffect
    {
        private readonly bool _isGain;

        /// <summary>
        /// Create a corruption effect.
        /// </summary>
        /// <param name="isGain">True to gain corruption, false to reduce</param>
        public CorruptionEffect(bool isGain)
        {
            _isGain = isGain;
        }

        public void Execute(CardEffectData data, EffectContext context)
        {
            int amount = data.Value;
            string action = _isGain ? "Gained" : "Reduced";

            // Find the source RequiemInstance from the team
            var requiem = GetSourceRequiem(context);
            if (requiem == null)
            {
                Debug.LogWarning("[CorruptionEffect] Could not find source Requiem for corruption effect");
                return;
            }

            // Apply corruption change via RequiemInstance methods (which publish events)
            if (_isGain)
            {
                requiem.AddCorruption(amount);
            }
            else
            {
                requiem.RemoveCorruption(amount);
            }

            Debug.Log($"[CorruptionEffect] {requiem.Name} {action} {amount} Corruption. Total: {requiem.Corruption}");
        }

        /// <summary>
        /// Find the RequiemInstance that owns the card from the combat team.
        /// </summary>
        private HNR.Characters.RequiemInstance GetSourceRequiem(EffectContext context)
        {
            // If no source data, can't find instance
            if (context.Source == null) return null;

            // Search the team for matching RequiemDataSO
            if (context.CombatContext?.Team != null)
            {
                foreach (var requiem in context.CombatContext.Team)
                {
                    if (requiem.Data == context.Source)
                    {
                        return requiem;
                    }
                }
            }

            // Fallback: Return first living team member for neutral cards
            if (context.CombatContext?.Team != null && context.CombatContext.Team.Count > 0)
            {
                foreach (var requiem in context.CombatContext.Team)
                {
                    if (!requiem.IsDead)
                    {
                        return requiem;
                    }
                }
            }

            return null;
        }
    }
}
