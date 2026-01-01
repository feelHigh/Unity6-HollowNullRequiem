// ============================================
// CustomEffects.cs
// Custom effect implementations for cards using EffectType.Custom
// These effects use the _customData field to determine specific behavior
// ============================================

using System.Collections.Generic;
using UnityEngine;
using HNR.Core;
using HNR.Core.Events;
using HNR.Combat;
using HNR.Characters;

namespace HNR.Cards
{
    /// <summary>
    /// Dispatcher for custom effects based on CardEffectData.CustomData value.
    /// Routes to specific implementations based on the custom effect key.
    /// </summary>
    public class CustomEffectHandler : ICardEffect
    {
        private readonly Dictionary<string, ICardEffect> _customEffects;

        public CustomEffectHandler()
        {
            _customEffects = new Dictionary<string, ICardEffect>
            {
                // Kira effects
                { "TriggerAllBurn", new TriggerAllBurnEffect() },
                { "FlameDamageBoost", new AspectDamageBoostEffect(SoulAspect.Flame) },
                { "SurviveLethal", new SurviveLethalEffect() },

                // Thornwick effects
                { "Regeneration", new ApplyRegenerationEffect() },
                { "Thorns", new ApplyThornsEffect() },
                { "DamageEqualsBlock", new DamageEqualsBlockEffect() },
                { "GuardBlockBonus", new GuardBlockBonusEffect() },
                { "StartTurnBlock", new StartTurnBlockEffect() },

                // Elara effects
                { "RemoveDebuffs", new RemoveDebuffsEffect() },
                { "AutoBlockPerTurn", new StartTurnBlockEffect() }, // Alias
                { "Revive", new ReviveEffect() },
                { "PreventDeath", new PreventDeathEffect() },

                // Mordren effects
                { "EnemyMissChance", new EvasionBuffEffect() },
                { "StealStrength", new StealStrengthEffect() },

                // Shared effects
                { "SelfDamage", new SelfDamageEffect() }
            };
        }

        public void Execute(CardEffectData data, EffectContext context)
        {
            string customKey = data.CustomData?.Trim();

            if (string.IsNullOrEmpty(customKey))
            {
                Debug.LogWarning("[CustomEffectHandler] No custom data key specified");
                return;
            }

            if (_customEffects.TryGetValue(customKey, out var handler))
            {
                handler.Execute(data, context);
            }
            else
            {
                Debug.LogWarning($"[CustomEffectHandler] Unknown custom effect: {customKey}");
            }
        }
    }

    // ============================================
    // Kira Custom Effects
    // ============================================

    /// <summary>
    /// Trigger all Burn stacks on all enemies immediately, dealing their full damage at once.
    /// Used by Kira's Combustion card.
    /// </summary>
    public class TriggerAllBurnEffect : ICardEffect
    {
        public void Execute(CardEffectData data, EffectContext context)
        {
            var enemies = context.GetAllEnemies();
            var statusManager = context.CombatContext?.StatusManager;

            if (statusManager == null)
            {
                ServiceLocator.TryGet<StatusEffectManager>(out statusManager);
            }

            int totalDamage = 0;
            foreach (var enemy in enemies)
            {
                if (enemy.IsDead) continue;

                int burnStacks = statusManager?.GetStatusStacks(enemy, StatusType.Burn) ?? 0;
                if (burnStacks > 0)
                {
                    // Deal burn damage immediately
                    enemy.TakeDamage(burnStacks);
                    totalDamage += burnStacks;

                    // Remove all burn stacks
                    statusManager?.RemoveStatus(enemy, StatusType.Burn, 0);

                    Debug.Log($"[TriggerAllBurnEffect] Triggered {burnStacks} burn on {enemy.Name}");
                }
            }

            if (totalDamage > 0)
            {
                EventBus.Publish(new DamageDealtEvent(null, null, totalDamage, 0, false));
            }
        }
    }

    /// <summary>
    /// Boost damage for all cards of a specific soul aspect this combat.
    /// Used by Kira's Fire Within (Flame +2 damage).
    /// </summary>
    public class AspectDamageBoostEffect : ICardEffect
    {
        private readonly SoulAspect _aspect;

        public AspectDamageBoostEffect(SoulAspect aspect)
        {
            _aspect = aspect;
        }

        public void Execute(CardEffectData data, EffectContext context)
        {
            int bonusDamage = data.Value;

            // Apply card modifiers to all cards in deck with matching aspect
            var deckManager = context.DeckManager;
            if (deckManager == null)
            {
                ServiceLocator.TryGet<DeckManager>(out deckManager);
            }

            if (deckManager != null)
            {
                int modifiedCount = 0;

                string sourceName = context.Card?.Data?.CardName ?? "AspectBoost";

                // Modify cards in draw pile
                foreach (var card in deckManager.DrawPile)
                {
                    if (card.Data.SoulAspect == _aspect)
                    {
                        card.AddModifier(new CardModifier(ModifierType.DamageBonus, bonusDamage, 0, sourceName));
                        modifiedCount++;
                    }
                }

                // Modify cards in discard pile
                foreach (var card in deckManager.DiscardPile)
                {
                    if (card.Data.SoulAspect == _aspect)
                    {
                        card.AddModifier(new CardModifier(ModifierType.DamageBonus, bonusDamage, 0, sourceName));
                        modifiedCount++;
                    }
                }

                // Modify cards in hand
                var handManager = context.CombatContext?.HandManager;
                if (handManager != null)
                {
                    foreach (var card in handManager.Hand)
                    {
                        if (card.CardInstance?.Data.SoulAspect == _aspect)
                        {
                            card.CardInstance.AddModifier(new CardModifier(ModifierType.DamageBonus, bonusDamage, 0, sourceName));
                            modifiedCount++;
                        }
                    }
                }

                Debug.Log($"[AspectDamageBoostEffect] Applied +{bonusDamage} damage to {modifiedCount} {_aspect} cards");
            }
        }
    }

    /// <summary>
    /// If team would take lethal damage this turn, survive with 1 HP instead.
    /// Used by Kira's Phoenix Feather.
    /// </summary>
    public class SurviveLethalEffect : ICardEffect
    {
        public void Execute(CardEffectData data, EffectContext context)
        {
            // Apply Protected status to prevent lethal (handled by damage system)
            var statusManager = context.CombatContext?.StatusManager;
            if (statusManager == null)
            {
                ServiceLocator.TryGet<StatusEffectManager>(out statusManager);
            }

            // Use Shielded status as proxy for survive lethal
            // The damage system should check for this and prevent death
            var team = context.CombatContext?.Team;
            if (team != null && team.Count > 0)
            {
                // Apply to all team members
                foreach (var requiem in team)
                {
                    statusManager?.ApplyStatus(requiem, StatusType.Shielded, data.Value, data.Duration);
                }
                Debug.Log($"[SurviveLethalEffect] Team protected from lethal damage for {data.Duration} turn(s)");
            }
        }
    }

    // ============================================
    // Thornwick Custom Effects
    // ============================================

    /// <summary>
    /// Apply Regeneration status effect.
    /// Used by Thornwick's Regenerate card.
    /// </summary>
    public class ApplyRegenerationEffect : ICardEffect
    {
        public void Execute(CardEffectData data, EffectContext context)
        {
            var statusManager = context.CombatContext?.StatusManager;
            if (statusManager == null)
            {
                ServiceLocator.TryGet<StatusEffectManager>(out statusManager);
            }

            var team = context.CombatContext?.Team;
            if (team != null)
            {
                foreach (var requiem in team)
                {
                    statusManager?.ApplyStatus(requiem, StatusType.Regeneration, data.Value, data.Duration);
                }
                Debug.Log($"[ApplyRegenerationEffect] Applied {data.Value} Regeneration for {data.Duration} turns");
            }

            EventBus.Publish(new StatusAppliedEvent(null, StatusType.Regeneration, data.Value, data.Duration));
        }
    }

    /// <summary>
    /// Apply Thorns status effect.
    /// Used by Thornwick's Thorny Embrace card.
    /// </summary>
    public class ApplyThornsEffect : ICardEffect
    {
        public void Execute(CardEffectData data, EffectContext context)
        {
            var statusManager = context.CombatContext?.StatusManager;
            if (statusManager == null)
            {
                ServiceLocator.TryGet<StatusEffectManager>(out statusManager);
            }

            var team = context.CombatContext?.Team;
            if (team != null)
            {
                foreach (var requiem in team)
                {
                    statusManager?.ApplyStatus(requiem, StatusType.Thorns, data.Value, data.Duration);
                }
                Debug.Log($"[ApplyThornsEffect] Applied {data.Value} Thorns for {data.Duration} turns");
            }

            EventBus.Publish(new StatusAppliedEvent(null, StatusType.Thorns, data.Value, data.Duration));
        }
    }

    /// <summary>
    /// Deal damage equal to current team block.
    /// Used by Thornwick's Nature's Wrath.
    /// </summary>
    public class DamageEqualsBlockEffect : ICardEffect
    {
        public void Execute(CardEffectData data, EffectContext context)
        {
            int currentBlock = context.CombatContext?.TeamBlock ?? 0;

            if (currentBlock <= 0)
            {
                Debug.Log("[DamageEqualsBlockEffect] No block to convert to damage");
                return;
            }

            if (context.Target == null)
            {
                Debug.LogWarning("[DamageEqualsBlockEffect] No target specified");
                return;
            }

            // Deal damage equal to block
            context.Target.TakeDamage(currentBlock);

            Debug.Log($"[DamageEqualsBlockEffect] Dealt {currentBlock} damage (equal to block) to {context.Target.Name}");
            EventBus.Publish(new DamageDealtEvent(context.SourceInstance, context.Target, currentBlock, 0, false));
        }
    }

    /// <summary>
    /// Guard cards gain bonus block this combat.
    /// Used by Thornwick's Fortress (Power card).
    /// </summary>
    public class GuardBlockBonusEffect : ICardEffect
    {
        public void Execute(CardEffectData data, EffectContext context)
        {
            int bonusBlock = data.Value;

            var deckManager = context.DeckManager;
            if (deckManager == null)
            {
                ServiceLocator.TryGet<DeckManager>(out deckManager);
            }

            if (deckManager != null)
            {
                int modifiedCount = 0;
                string sourceName = context.Card?.Data?.CardName ?? "GuardBonus";

                // Modify Guard cards in draw pile
                foreach (var card in deckManager.DrawPile)
                {
                    if (card.Data.CardType == CardType.Guard)
                    {
                        card.AddModifier(new CardModifier(ModifierType.BlockBonus, bonusBlock, 0, sourceName));
                        modifiedCount++;
                    }
                }

                // Modify Guard cards in discard pile
                foreach (var card in deckManager.DiscardPile)
                {
                    if (card.Data.CardType == CardType.Guard)
                    {
                        card.AddModifier(new CardModifier(ModifierType.BlockBonus, bonusBlock, 0, sourceName));
                        modifiedCount++;
                    }
                }

                // Modify Guard cards in hand
                var handManager = context.CombatContext?.HandManager;
                if (handManager != null)
                {
                    foreach (var card in handManager.Hand)
                    {
                        if (card.CardInstance?.Data.CardType == CardType.Guard)
                        {
                            card.CardInstance.AddModifier(new CardModifier(ModifierType.BlockBonus, bonusBlock, 0, sourceName));
                            modifiedCount++;
                        }
                    }
                }

                Debug.Log($"[GuardBlockBonusEffect] Applied +{bonusBlock} block to {modifiedCount} Guard cards");
            }
        }
    }

    /// <summary>
    /// Gain block at the start of each turn.
    /// Used by Thornwick's Ancient Grove and Elara's Light Barrier.
    /// </summary>
    public class StartTurnBlockEffect : ICardEffect
    {
        public void Execute(CardEffectData data, EffectContext context)
        {
            int blockPerTurn = data.Value;

            // Apply Dexterity status as a proxy for block gain per turn
            var statusManager = context.CombatContext?.StatusManager;
            if (statusManager == null)
            {
                ServiceLocator.TryGet<StatusEffectManager>(out statusManager);
            }

            var team = context.CombatContext?.Team;
            if (team != null)
            {
                foreach (var requiem in team)
                {
                    statusManager?.ApplyStatus(requiem, StatusType.Dexterity, blockPerTurn, 0); // Permanent for combat
                }
                Debug.Log($"[StartTurnBlockEffect] Team gains +{blockPerTurn} block per turn");
            }
        }
    }

    // ============================================
    // Elara Custom Effects
    // ============================================

    /// <summary>
    /// Remove all debuffs from the team.
    /// Used by Elara's Cleansing Wave.
    /// </summary>
    public class RemoveDebuffsEffect : ICardEffect
    {
        private static readonly StatusType[] Debuffs = new[]
        {
            StatusType.Burn,
            StatusType.Poison,
            StatusType.Weakness,
            StatusType.Vulnerability,
            StatusType.Stun,
            StatusType.Dazed,
            StatusType.Marked
        };

        public void Execute(CardEffectData data, EffectContext context)
        {
            var statusManager = context.CombatContext?.StatusManager;
            if (statusManager == null)
            {
                ServiceLocator.TryGet<StatusEffectManager>(out statusManager);
            }

            var team = context.CombatContext?.Team;
            if (team == null || statusManager == null)
            {
                Debug.LogWarning("[RemoveDebuffsEffect] No team or status manager available");
                return;
            }

            int removedCount = 0;
            foreach (var requiem in team)
            {
                foreach (var debuff in Debuffs)
                {
                    if (statusManager.HasStatus(requiem, debuff))
                    {
                        statusManager.RemoveStatus(requiem, debuff, 0);
                        removedCount++;
                    }
                }
            }

            Debug.Log($"[RemoveDebuffsEffect] Removed {removedCount} debuffs from team");
        }
    }

    /// <summary>
    /// Heal target to a percentage of max HP.
    /// Used by Elara's Resurrection.
    /// </summary>
    public class ReviveEffect : ICardEffect
    {
        public void Execute(CardEffectData data, EffectContext context)
        {
            int healPercent = data.Value; // e.g., 50 = 50% of max HP

            var combatContext = context.CombatContext;
            if (combatContext == null)
            {
                Debug.LogWarning("[ReviveEffect] No combat context available");
                return;
            }

            int maxHP = combatContext.TeamMaxHP;
            int healAmount = Mathf.RoundToInt(maxHP * (healPercent / 100f));

            // Heal the team
            var turnManager = context.TurnManager;
            if (turnManager != null)
            {
                turnManager.HealTeam(healAmount);
            }
            else
            {
                combatContext.TeamHP = Mathf.Min(combatContext.TeamHP + healAmount, maxHP);
                EventBus.Publish(new TeamHPChangedEvent(combatContext.TeamHP, maxHP, -healAmount));
            }

            Debug.Log($"[ReviveEffect] Healed team for {healAmount} HP ({healPercent}% of max)");
            EventBus.Publish(new HealingReceivedEvent(null, healAmount));
        }
    }

    /// <summary>
    /// Prevent death once this combat.
    /// Used by Elara's Divine Intervention.
    /// </summary>
    public class PreventDeathEffect : ICardEffect
    {
        public void Execute(CardEffectData data, EffectContext context)
        {
            // Apply Shielded status as proxy for death prevention
            var statusManager = context.CombatContext?.StatusManager;
            if (statusManager == null)
            {
                ServiceLocator.TryGet<StatusEffectManager>(out statusManager);
            }

            var team = context.CombatContext?.Team;
            if (team != null)
            {
                foreach (var requiem in team)
                {
                    statusManager?.ApplyStatus(requiem, StatusType.Shielded, data.Value, data.Duration);
                }
                Debug.Log($"[PreventDeathEffect] Team protected from death for {data.Duration} instance(s)");
            }
        }
    }

    // ============================================
    // Mordren Custom Effects
    // ============================================

    /// <summary>
    /// Apply evasion buff (chance for enemies to miss).
    /// Used by Mordren's Veil of Night.
    /// </summary>
    public class EvasionBuffEffect : ICardEffect
    {
        public void Execute(CardEffectData data, EffectContext context)
        {
            int evasionChance = data.Value; // Percentage chance

            // Apply Protected status as a proxy for evasion
            // The damage system can interpret this as damage reduction
            var statusManager = context.CombatContext?.StatusManager;
            if (statusManager == null)
            {
                ServiceLocator.TryGet<StatusEffectManager>(out statusManager);
            }

            var team = context.CombatContext?.Team;
            if (team != null)
            {
                foreach (var requiem in team)
                {
                    // Use Protected status (25% damage reduction per stack)
                    int stacks = Mathf.CeilToInt(evasionChance / 25f);
                    statusManager?.ApplyStatus(requiem, StatusType.Protected, stacks, data.Duration);
                }
                Debug.Log($"[EvasionBuffEffect] Applied {evasionChance}% evasion to team");
            }
        }
    }

    /// <summary>
    /// Steal Strength from target enemy.
    /// Used by Mordren's Siphon Power.
    /// </summary>
    public class StealStrengthEffect : ICardEffect
    {
        public void Execute(CardEffectData data, EffectContext context)
        {
            int strengthToSteal = data.Value;

            if (context.Target == null)
            {
                Debug.LogWarning("[StealStrengthEffect] No target specified");
                return;
            }

            var statusManager = context.CombatContext?.StatusManager;
            if (statusManager == null)
            {
                ServiceLocator.TryGet<StatusEffectManager>(out statusManager);
            }

            if (statusManager != null)
            {
                // Remove Strength from enemy (apply Weakness as alternative)
                statusManager.ApplyStatus(context.Target, StatusType.Weakness, strengthToSteal, 0);

                // Add Strength to team
                var team = context.CombatContext?.Team;
                if (team != null)
                {
                    foreach (var requiem in team)
                    {
                        statusManager.ApplyStatus(requiem, StatusType.Strength, strengthToSteal, 0);
                    }
                }

                Debug.Log($"[StealStrengthEffect] Stole {strengthToSteal} strength from {context.Target.Name}");
            }
        }
    }

    // ============================================
    // Shared Custom Effects
    // ============================================

    /// <summary>
    /// Deal damage to the team (self-damage).
    /// Used by Shared Sacrifice card.
    /// </summary>
    public class SelfDamageEffect : ICardEffect
    {
        public void Execute(CardEffectData data, EffectContext context)
        {
            int damage = data.Value;

            var combatContext = context.CombatContext;
            if (combatContext == null)
            {
                Debug.LogWarning("[SelfDamageEffect] No combat context available");
                return;
            }

            // Deal damage to team (bypasses block)
            int previousHP = combatContext.TeamHP;
            combatContext.TeamHP = Mathf.Max(1, combatContext.TeamHP - damage); // Don't allow self-kill

            Debug.Log($"[SelfDamageEffect] Team took {damage} self-damage (HP: {previousHP} -> {combatContext.TeamHP})");
            EventBus.Publish(new TeamHPChangedEvent(combatContext.TeamHP, combatContext.TeamMaxHP, damage));
        }
    }
}
