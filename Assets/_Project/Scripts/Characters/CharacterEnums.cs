// ============================================
// CharacterEnums.cs
// Character-related enumerations for the character system
// ============================================

namespace HNR.Characters
{
    /// <summary>
    /// Requiem class defining combat role and playstyle.
    /// </summary>
    public enum RequiemClass
    {
        /// <summary>
        /// High damage, lower survivability. Kira archetype.
        /// Focuses on burst damage and burn effects.
        /// </summary>
        Striker,

        /// <summary>
        /// Debuffs, utility, manipulation. Mordren archetype.
        /// Weakens enemies and drains their strength.
        /// </summary>
        Controller,

        /// <summary>
        /// Healing, buffs, corruption management. Elara archetype.
        /// Provides shields, heals, and reduces Corruption.
        /// </summary>
        Support,

        /// <summary>
        /// High HP, block generation. Thornwick archetype.
        /// Absorbs damage and provides sustained defense.
        /// </summary>
        Tank
    }

    /// <summary>
    /// Special effect triggered when Requiem enters Null State (100 Corruption).
    /// Each Requiem has a unique Null State effect.
    /// </summary>
    public enum NullStateEffect
    {
        /// <summary>
        /// Kira: All Burn effects deal double damage.
        /// Cards gain "Apply 2 Burn" if they didn't already apply Burn.
        /// </summary>
        DamageBoost,

        /// <summary>
        /// Mordren: HP drain effects heal for double.
        /// Enemies take 50% of damage they deal to themselves.
        /// </summary>
        LifestealBoost,

        /// <summary>
        /// Elara: Healing effects also deal equal damage to a random enemy.
        /// Light damage ignores enemy Block.
        /// </summary>
        HealingDamage,

        /// <summary>
        /// Thornwick: Regenerate 5 HP at turn start.
        /// All Block effects increased by 50%.
        /// </summary>
        DefenseRegen
    }

    /// <summary>
    /// Status effect types that can be applied to combatants.
    /// </summary>
    public enum StatusType
    {
        // ============================================
        // Damage Over Time
        // ============================================

        /// <summary>Fire damage at end of turn. Stacks decrease by 1.</summary>
        Burn,

        /// <summary>Damage at end of turn. Stacks remain until cleansed.</summary>
        Poison,

        // ============================================
        // Debuffs
        // ============================================

        /// <summary>Deal 25% less damage.</summary>
        Weakness,

        /// <summary>Take 25% more damage.</summary>
        Vulnerability,

        /// <summary>Skip next action.</summary>
        Stun,

        // ============================================
        // Buffs
        // ============================================

        /// <summary>Deal bonus damage per stack.</summary>
        Strength,

        /// <summary>Gain bonus block per stack.</summary>
        Dexterity,

        /// <summary>Heal HP at turn start.</summary>
        Regeneration,

        // ============================================
        // Special
        // ============================================

        /// <summary>Immune to debuffs for duration.</summary>
        Shielded,

        /// <summary>Thorns damage to attackers.</summary>
        Thorns,

        /// <summary>Draw extra cards at turn start.</summary>
        DrawBonus,

        // ============================================
        // Defense Modifiers
        // ============================================

        /// <summary>Take 25% less damage (opposite of Vulnerability).</summary>
        Protected,

        // ============================================
        // Card Cost Modifiers
        // ============================================

        /// <summary>Next card costs +1 AP.</summary>
        Dazed,

        /// <summary>Next card costs -1 AP.</summary>
        Energized,

        // ============================================
        // Combo/Special Effects
        // ============================================

        /// <summary>Takes bonus damage from next attack.</summary>
        Marked,

        /// <summary>Stacks build to trigger effects at threshold.</summary>
        Ritual
    }

    /// <summary>
    /// Soul Aspects for characters (mirrors HNR.Cards.SoulAspect).
    /// Defined here for character system independence.
    /// </summary>
    public enum CharacterAspect
    {
        /// <summary>Neutral, no elemental affinity.</summary>
        None,

        /// <summary>Flame aspect. Strong vs Nature, weak vs Shadow/Light.</summary>
        Flame,

        /// <summary>Shadow aspect. Strong vs Flame/Light, weak vs Nature/Arcane.</summary>
        Shadow,

        /// <summary>Nature aspect. Strong vs Shadow/Arcane, weak vs Flame.</summary>
        Nature,

        /// <summary>Arcane aspect. Strong vs Shadow, weak vs Nature/Light.</summary>
        Arcane,

        /// <summary>Light aspect. Strong vs Flame/Arcane, weak vs Shadow.</summary>
        Light
    }
}
