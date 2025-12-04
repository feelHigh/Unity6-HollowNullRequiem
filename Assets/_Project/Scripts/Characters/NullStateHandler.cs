// ============================================
// NullStateHandler.cs
// Handles Null State effects for each Requiem
// ============================================

using UnityEngine;
using HNR.Core;
using HNR.Core.Events;

namespace HNR.Characters
{
    /// <summary>
    /// Subscribes to Null State events and applies character-specific effects.
    /// Each Requiem has a unique Null State effect per GDD Section 5.
    /// </summary>
    /// <remarks>
    /// Null State Effects:
    /// - Kira (DamageBoost): Burn deals 2x damage, cards apply +2 Burn
    /// - Mordren (LifestealBoost): Drain effects heal 2x, enemies damage themselves
    /// - Elara (HealingDamage): Healing effects also damage random enemy
    /// - Thornwick (DefenseRegen): +5 HP regen, +50% Block at turn start
    /// </remarks>
    public class NullStateHandler : MonoBehaviour
    {
        // ============================================
        // Constants
        // ============================================

        private const float KIRA_BURN_MULTIPLIER = 2.0f;
        private const float MORDREN_LIFESTEAL_MULTIPLIER = 2.0f;
        private const int THORNWICK_BLOCK_REGEN = 5;
        private const int THORNWICK_HEAL_REGEN = 5;

        // ============================================
        // Lifecycle
        // ============================================

        private void OnEnable()
        {
            EventBus.Subscribe<NullStateEnteredEvent>(OnNullStateEntered);
            EventBus.Subscribe<NullStateExitedEvent>(OnNullStateExited);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<NullStateEnteredEvent>(OnNullStateEntered);
            EventBus.Unsubscribe<NullStateExitedEvent>(OnNullStateExited);
        }

        // ============================================
        // Event Handlers
        // ============================================

        private void OnNullStateEntered(NullStateEnteredEvent evt)
        {
            if (evt.Requiem == null || evt.Requiem.Data == null) return;

            ApplyNullStateEffect(evt.Requiem, true);
            Debug.Log($"[NullStateHandler] {evt.Requiem.Name} entered Null State - applying {evt.Requiem.Data.NullStateEffect} effect");
        }

        private void OnNullStateExited(NullStateExitedEvent evt)
        {
            if (evt.Requiem == null || evt.Requiem.Data == null) return;

            ApplyNullStateEffect(evt.Requiem, false);
            Debug.Log($"[NullStateHandler] {evt.Requiem.Name} exited Null State - removing effects");
        }

        // ============================================
        // Effect Application
        // ============================================

        /// <summary>
        /// Apply or remove Null State effect based on Requiem's configured effect type.
        /// </summary>
        /// <param name="requiem">The Requiem instance</param>
        /// <param name="entering">True when entering Null State, false when exiting</param>
        private void ApplyNullStateEffect(RequiemInstance requiem, bool entering)
        {
            switch (requiem.Data.NullStateEffect)
            {
                case NullStateEffect.DamageBoost:
                    ApplyDamageBoost(requiem, entering);
                    break;

                case NullStateEffect.LifestealBoost:
                    ApplyLifestealBoost(requiem, entering);
                    break;

                case NullStateEffect.HealingDamage:
                    ApplyHealingDamage(requiem, entering);
                    break;

                case NullStateEffect.DefenseRegen:
                    ApplyDefenseRegen(requiem, entering);
                    break;

                default:
                    Debug.LogWarning($"[NullStateHandler] Unknown NullStateEffect for {requiem.Name}");
                    break;
            }
        }

        /// <summary>
        /// Kira's Null State: Burn effects deal double damage.
        /// Cards gain "Apply 2 Burn" if they didn't already apply Burn.
        /// </summary>
        private void ApplyDamageBoost(RequiemInstance requiem, bool active)
        {
            float multiplier = active ? KIRA_BURN_MULTIPLIER : 1.0f;
            requiem.SetBurnDamageMultiplier(multiplier);

            Debug.Log($"[NullState] {requiem.Name}: Burn damage multiplier = {multiplier}x");
        }

        /// <summary>
        /// Mordren's Null State: HP drain effects heal for double.
        /// Enemies take 50% of damage they deal to themselves.
        /// </summary>
        private void ApplyLifestealBoost(RequiemInstance requiem, bool active)
        {
            float multiplier = active ? MORDREN_LIFESTEAL_MULTIPLIER : 1.0f;
            requiem.SetLifestealMultiplier(multiplier);

            Debug.Log($"[NullState] {requiem.Name}: Lifesteal multiplier = {multiplier}x");
        }

        /// <summary>
        /// Elara's Null State: Healing effects also deal equal damage to a random enemy.
        /// Light damage ignores enemy Block.
        /// </summary>
        private void ApplyHealingDamage(RequiemInstance requiem, bool active)
        {
            requiem.SetHealingDamagesEnemies(active);

            Debug.Log($"[NullState] {requiem.Name}: Healing damages enemies = {active}");
        }

        /// <summary>
        /// Thornwick's Null State: Regenerates 5 HP at the start of each turn.
        /// All Block effects are increased by 50%.
        /// </summary>
        private void ApplyDefenseRegen(RequiemInstance requiem, bool active)
        {
            int blockRegen = active ? THORNWICK_BLOCK_REGEN : 0;
            int healRegen = active ? THORNWICK_HEAL_REGEN : 0;
            requiem.SetNullStateRegen(blockRegen, healRegen);

            Debug.Log($"[NullState] {requiem.Name}: Turn regen = +{blockRegen} Block, +{healRegen} HP");
        }

        // ============================================
        // Utility
        // ============================================

        /// <summary>
        /// Get description of Null State effect for UI display.
        /// </summary>
        public static string GetEffectDescription(NullStateEffect effect)
        {
            return effect switch
            {
                NullStateEffect.DamageBoost => "Burn deals 2x damage. Cards apply +2 Burn.",
                NullStateEffect.LifestealBoost => "Drain heals 2x. Enemies damage themselves.",
                NullStateEffect.HealingDamage => "Healing also damages a random enemy.",
                NullStateEffect.DefenseRegen => "+5 HP, +5 Block at turn start.",
                _ => "Unknown effect"
            };
        }
    }
}
