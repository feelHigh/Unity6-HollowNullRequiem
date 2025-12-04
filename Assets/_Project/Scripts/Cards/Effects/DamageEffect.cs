// ============================================
// DamageEffect.cs
// Damage effect implementations with soul aspect multipliers
// ============================================

using UnityEngine;
using HNR.Core.Events;
using HNR.Combat;

namespace HNR.Cards
{
    /// <summary>
    /// Deal damage to a single target.
    /// Applies card modifiers and soul aspect effectiveness.
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

            // Get base damage and apply card modifiers
            int damage = data.Value;
            if (context.Card != null)
            {
                damage = context.Card.GetModifiedDamage(damage);
            }

            // Apply soul aspect effectiveness multiplier
            if (context.Source != null && context.Target is EnemyInstance enemy && enemy.Data != null)
            {
                float multiplier = AspectEffectiveness.GetMultiplier(
                    context.Source.SoulAspect,
                    enemy.Data.SoulAspect
                );
                damage = Mathf.RoundToInt(damage * multiplier);

                if (multiplier > 1f)
                {
                    Debug.Log($"[DamageEffect] Soul Aspect advantage! {multiplier}x damage");
                }
                else if (multiplier < 1f)
                {
                    Debug.Log($"[DamageEffect] Soul Aspect disadvantage. {multiplier}x damage");
                }
            }

            // Apply context multiplier (from buffs/debuffs)
            damage = context.CalculateDamage(damage);

            // Deal damage
            context.Target.TakeDamage(damage);

            // Publish event (using null for source since we don't have ICombatant yet)
            // TODO: Properly implement ICombatant on RequiemInstance
            EventBus.Publish(new DamageDealtEvent(null, null, damage, 0, false));

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

            int baseDamagePerHit = data.Value;
            int hitCount = data.Duration > 0 ? data.Duration : 1;

            // Get base damage with card modifiers
            int damagePerHit = baseDamagePerHit;
            if (context.Card != null)
            {
                damagePerHit = context.Card.GetModifiedDamage(baseDamagePerHit);
            }

            // Apply soul aspect multiplier once (affects all hits)
            float aspectMultiplier = 1f;
            if (context.Source != null && context.Target is EnemyInstance enemy && enemy.Data != null)
            {
                aspectMultiplier = AspectEffectiveness.GetMultiplier(
                    context.Source.SoulAspect,
                    enemy.Data.SoulAspect
                );
                damagePerHit = Mathf.RoundToInt(damagePerHit * aspectMultiplier);
            }

            // Apply context multiplier
            damagePerHit = context.CalculateDamage(damagePerHit);

            int totalDamage = 0;

            // Deal damage for each hit
            for (int i = 0; i < hitCount; i++)
            {
                context.Target.TakeDamage(damagePerHit);
                totalDamage += damagePerHit;

                // Publish event for each hit
                EventBus.Publish(new DamageDealtEvent(null, null, damagePerHit, 0, false));
            }

            Debug.Log($"[DamageMultipleEffect] Dealt {damagePerHit}x{hitCount} = {totalDamage} total damage to {context.Target.Name}");
        }
    }
}
