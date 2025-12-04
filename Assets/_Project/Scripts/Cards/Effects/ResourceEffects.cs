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

            // TODO: Implement full Corruption system in Week 6
            // Will need to:
            // 1. Track corruption per RequiemInstance
            // 2. Trigger Null State at 100 corruption
            // 3. Apply Null State effects based on Requiem type

            Debug.Log($"[CorruptionEffect] {action} {amount} Corruption");

            // When corruption system is implemented:
            // var requiem = GetSourceRequiem(context);
            // int previousValue = requiem.Corruption;
            // requiem.Corruption = _isGain
            //     ? Mathf.Min(100, requiem.Corruption + amount)
            //     : Mathf.Max(0, requiem.Corruption - amount);
            // EventBus.Publish(new CorruptionChangedEvent(requiem, previousValue, requiem.Corruption));
            //
            // if (requiem.Corruption >= 100 && previousValue < 100)
            //     EventBus.Publish(new NullStateEnteredEvent(requiem));
        }
    }
}
