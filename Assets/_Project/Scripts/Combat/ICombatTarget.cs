// ============================================
// ICombatTarget.cs
// Interface for targetable combat entities
// ============================================

using UnityEngine;

namespace HNR.Combat
{
    /// <summary>
    /// Interface for entities that can be targeted in combat.
    /// Implemented by RequiemInstance and EnemyInstance.
    /// </summary>
    public interface ICombatTarget
    {
        /// <summary>Display name of the target.</summary>
        string Name { get; }

        /// <summary>World position for targeting and effects.</summary>
        Vector3 Position { get; }

        /// <summary>True if this target has been defeated.</summary>
        bool IsDead { get; }

        /// <summary>
        /// Apply damage to this target.
        /// </summary>
        /// <param name="amount">Raw damage amount before mitigation</param>
        void TakeDamage(int amount);

        /// <summary>
        /// Heal this target.
        /// </summary>
        /// <param name="amount">Amount to heal</param>
        void Heal(int amount);

        /// <summary>
        /// Show or hide targeting highlight effect.
        /// </summary>
        /// <param name="show">True to show, false to hide</param>
        void ShowTargetHighlight(bool show);
    }
}
