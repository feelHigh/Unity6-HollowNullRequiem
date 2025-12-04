// ============================================
// EnemyCorruptionAttack.cs
// Utility for enemy corruption attacks
// ============================================

using UnityEngine;
using HNR.Core;
using HNR.Characters;

namespace HNR.Combat
{
    /// <summary>
    /// Utility class for applying corruption from enemy attacks.
    /// Used by enemies like Corrupted Wisp, Null Herald, and The Hollow King.
    /// </summary>
    public static class EnemyCorruptionAttack
    {
        // ============================================
        // Team-Wide Corruption
        // ============================================

        /// <summary>
        /// Apply corruption to the entire team.
        /// Used for mass corruption attacks (Null Herald, Boss specials).
        /// </summary>
        /// <param name="amount">Amount of corruption to apply to each Requiem</param>
        /// <param name="context">Current combat context</param>
        public static void ApplyCorruptionToTeam(int amount, CombatContext context)
        {
            if (context?.Team == null || amount <= 0) return;

            foreach (var requiem in context.Team)
            {
                if (requiem != null && !requiem.IsDead)
                {
                    requiem.AddCorruption(amount);
                    Debug.Log($"[EnemyCorruptionAttack] {requiem.Name} gained {amount} corruption from enemy attack");
                }
            }
        }

        /// <summary>
        /// Apply corruption to the entire team using CorruptionManager.
        /// </summary>
        /// <param name="amount">Amount of corruption to apply</param>
        public static void ApplyCorruptionToTeam(int amount)
        {
            if (amount <= 0) return;

            if (ServiceLocator.TryGet<CorruptionManager>(out var manager))
            {
                manager.AddCorruptionToTeam(amount);
            }
            else
            {
                Debug.LogWarning("[EnemyCorruptionAttack] CorruptionManager not found");
            }
        }

        // ============================================
        // Single Target Corruption
        // ============================================

        /// <summary>
        /// Apply corruption to a single Requiem.
        /// Used for targeted corruption attacks.
        /// </summary>
        /// <param name="requiem">Target Requiem</param>
        /// <param name="amount">Amount of corruption to apply</param>
        public static void ApplyCorruptionToTarget(RequiemInstance requiem, int amount)
        {
            if (requiem == null || requiem.IsDead || amount <= 0) return;

            requiem.AddCorruption(amount);
            Debug.Log($"[EnemyCorruptionAttack] {requiem.Name} gained {amount} corruption from targeted attack");
        }

        /// <summary>
        /// Apply corruption to a random living Requiem.
        /// </summary>
        /// <param name="amount">Amount of corruption to apply</param>
        /// <param name="context">Current combat context</param>
        public static void ApplyCorruptionToRandom(int amount, CombatContext context)
        {
            if (context?.Team == null || amount <= 0) return;

            // Build list of valid targets
            var validTargets = new System.Collections.Generic.List<RequiemInstance>();
            foreach (var requiem in context.Team)
            {
                if (requiem != null && !requiem.IsDead)
                {
                    validTargets.Add(requiem);
                }
            }

            if (validTargets.Count == 0) return;

            // Select random target
            var target = validTargets[Random.Range(0, validTargets.Count)];
            ApplyCorruptionToTarget(target, amount);
        }

        // ============================================
        // On-Hit Corruption
        // ============================================

        /// <summary>
        /// Apply corruption when an enemy deals damage.
        /// Called by TurnManager when processing enemy attacks.
        /// </summary>
        /// <param name="enemy">The attacking enemy</param>
        /// <param name="context">Current combat context</param>
        public static void ApplyOnHitCorruption(EnemyInstance enemy, CombatContext context)
        {
            if (enemy?.Data == null || context?.Team == null) return;

            int corruptionOnHit = enemy.Data.CorruptionOnHit;
            if (corruptionOnHit <= 0) return;

            // Apply to random team member (simulates hitting one character)
            ApplyCorruptionToRandom(corruptionOnHit, context);
        }

        /// <summary>
        /// Apply corruption to all team members when damage is dealt.
        /// Used for AoE corruption attacks.
        /// </summary>
        /// <param name="enemy">The attacking enemy</param>
        /// <param name="context">Current combat context</param>
        public static void ApplyOnHitCorruptionToAll(EnemyInstance enemy, CombatContext context)
        {
            if (enemy?.Data == null || context?.Team == null) return;

            int corruptionOnHit = enemy.Data.CorruptionOnHit;
            if (corruptionOnHit <= 0) return;

            ApplyCorruptionToTeam(corruptionOnHit, context);
        }

        // ============================================
        // Damage-Based Corruption (Optional Mechanic)
        // ============================================

        /// <summary>
        /// Apply corruption based on unblocked damage taken.
        /// Optional mechanic: +1 corruption per 10 unblocked damage.
        /// </summary>
        /// <param name="unblockedDamage">Amount of damage that wasn't blocked</param>
        /// <param name="context">Current combat context</param>
        public static void ApplyDamageBasedCorruption(int unblockedDamage, CombatContext context)
        {
            if (unblockedDamage <= 0 || context?.Team == null) return;

            // +1 corruption per 10 damage (rounded down)
            int corruptionGain = unblockedDamage / 10;
            if (corruptionGain <= 0) return;

            ApplyCorruptionToRandom(corruptionGain, context);
            Debug.Log($"[EnemyCorruptionAttack] Team gained {corruptionGain} corruption from {unblockedDamage} unblocked damage");
        }
    }
}
