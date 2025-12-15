// ============================================
// ICharacterVisual.cs
// Interface for character visual/animation systems
// ============================================

using UnityEngine;

namespace HNR.Characters.Visuals
{
    /// <summary>
    /// Interface for character visual components.
    /// Abstracts animation system to support both HeroEditor and simple sprites.
    /// </summary>
    public interface ICharacterVisual
    {
        /// <summary>
        /// Play an attack animation.
        /// </summary>
        /// <param name="type">Type of attack animation to play.</param>
        void PlayAttack(AttackType type = AttackType.Slash);

        /// <summary>
        /// Play hit/damage reaction animation.
        /// </summary>
        void PlayHit();

        /// <summary>
        /// Play death animation.
        /// </summary>
        /// <param name="forward">True for forward fall, false for backward fall.</param>
        void PlayDeath(bool forward = false);

        /// <summary>
        /// Return to idle state.
        /// </summary>
        void SetIdle();

        /// <summary>
        /// Set character expression (if supported).
        /// </summary>
        /// <param name="expression">Expression name (e.g., "Default", "Interrupted", "Smile").</param>
        void SetExpression(string expression);

        /// <summary>
        /// Flash the character with a color (damage feedback).
        /// </summary>
        /// <param name="color">Color to flash.</param>
        /// <param name="duration">Duration of the flash.</param>
        void FlashColor(Color color, float duration);

        /// <summary>
        /// Play a skill/ability animation.
        /// </summary>
        void PlaySkill();

        /// <summary>
        /// Play block/defend animation.
        /// </summary>
        void PlayBlock();

        /// <summary>
        /// Set facing direction.
        /// </summary>
        /// <param name="faceRight">True to face right, false to face left.</param>
        void SetFacing(bool faceRight);

        /// <summary>
        /// Whether the visual is currently playing an animation.
        /// </summary>
        bool IsAnimating { get; }
    }

    /// <summary>
    /// Types of attack animations available.
    /// </summary>
    public enum AttackType
    {
        /// <summary>Melee slash attack.</summary>
        Slash,
        /// <summary>Quick jab/thrust attack.</summary>
        Jab,
        /// <summary>Ranged shot attack.</summary>
        Shoot
    }
}
