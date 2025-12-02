// ============================================
// CardEnums.cs
// Card-related enumerations for the card system
// ============================================

namespace HNR.Cards
{
    /// <summary>
    /// Primary card classification affecting frame color and behavior.
    /// </summary>
    public enum CardType
    {
        /// <summary>Damage dealing cards - Red frame.</summary>
        Strike,

        /// <summary>Block/defense cards - Blue frame.</summary>
        Guard,

        /// <summary>Utility effects (buffs, debuffs, draw) - Green frame.</summary>
        Skill,

        /// <summary>Persistent buffs that exhaust when played - Purple frame.</summary>
        Power
    }

    /// <summary>
    /// Card rarity affecting drop rates and shop prices.
    /// </summary>
    public enum CardRarity
    {
        /// <summary>60% drop rate, 30-50 Void Shards.</summary>
        Common,

        /// <summary>25% drop rate, 60-90 Void Shards.</summary>
        Uncommon,

        /// <summary>12% drop rate, 100-150 Void Shards.</summary>
        Rare,

        /// <summary>3% drop rate, 200-300 Void Shards.</summary>
        Legendary
    }

    /// <summary>
    /// Soul Aspect for elemental effectiveness calculations.
    /// </summary>
    public enum SoulAspect
    {
        /// <summary>Neutral cards, no effectiveness bonus.</summary>
        None,

        /// <summary>Strong vs Nature, weak vs Shadow/Light.</summary>
        Flame,

        /// <summary>Strong vs Flame/Light, weak vs Nature/Arcane.</summary>
        Shadow,

        /// <summary>Strong vs Shadow/Arcane, weak vs Flame.</summary>
        Nature,

        /// <summary>Strong vs Shadow, weak vs Nature/Light.</summary>
        Arcane,

        /// <summary>Strong vs Flame/Arcane, weak vs Shadow.</summary>
        Light
    }

    /// <summary>
    /// Targeting mode for card effects.
    /// </summary>
    public enum TargetType
    {
        /// <summary>No targeting required (self-buffs, draw cards).</summary>
        None,

        /// <summary>Requires enemy selection.</summary>
        SingleEnemy,

        /// <summary>Hits all enemies automatically.</summary>
        AllEnemies,

        /// <summary>Select one Requiem ally.</summary>
        SingleAlly,

        /// <summary>Affects entire team.</summary>
        AllAllies,

        /// <summary>Caster only.</summary>
        Self,

        /// <summary>Random valid target.</summary>
        Random
    }

    /// <summary>
    /// Effect types that cards can apply.
    /// </summary>
    public enum EffectType
    {
        // ============================================
        // Damage
        // ============================================

        /// <summary>Deal direct damage to target.</summary>
        Damage,

        /// <summary>Deal multiple hits. Value = damage per hit, Duration = hit count.</summary>
        DamageMultiple,

        // ============================================
        // Defense
        // ============================================

        /// <summary>Gain Block that absorbs damage until next turn.</summary>
        Block,

        // ============================================
        // Healing
        // ============================================

        /// <summary>Restore flat HP amount.</summary>
        Heal,

        /// <summary>Restore HP as percentage of max HP.</summary>
        HealPercent,

        // ============================================
        // Status Effects
        // ============================================

        /// <summary>Apply Burn: damage over time.</summary>
        ApplyBurn,

        /// <summary>Apply Poison: stacking damage over time.</summary>
        ApplyPoison,

        /// <summary>Apply Weakness: reduced damage dealt.</summary>
        ApplyWeakness,

        /// <summary>Apply Vulnerability: increased damage taken.</summary>
        ApplyVulnerability,

        /// <summary>Apply Stun: skip next action.</summary>
        ApplyStun,

        // ============================================
        // Corruption
        // ============================================

        /// <summary>Gain Corruption (toward Null State).</summary>
        CorruptionGain,

        /// <summary>Reduce Corruption.</summary>
        CorruptionReduce,

        // ============================================
        // Card Manipulation
        // ============================================

        /// <summary>Draw cards from deck.</summary>
        DrawCards,

        /// <summary>Discard random cards from hand.</summary>
        DiscardRandom,

        /// <summary>Remove card from play for this combat.</summary>
        Exhaust,

        // ============================================
        // Resources
        // ============================================

        /// <summary>Gain Action Points this turn.</summary>
        GainAP,

        /// <summary>Gain Soul Energy.</summary>
        GainSE,

        // ============================================
        // Special
        // ============================================

        /// <summary>Summon a temporary combat entity.</summary>
        SummonEntity,

        /// <summary>Create a copy of a card.</summary>
        CopyCard,

        /// <summary>Custom effect with behavior defined in CustomData.</summary>
        Custom
    }

    /// <summary>
    /// Modifier types for runtime card modifications.
    /// </summary>
    public enum ModifierType
    {
        /// <summary>Modify AP cost of card.</summary>
        Cost,

        /// <summary>Modify damage output.</summary>
        DamageBonus,

        /// <summary>Modify block gained.</summary>
        BlockBonus
    }
}
