// ============================================
// AspectEffectiveness.cs
// Soul Aspect damage multiplier calculations
// ============================================

namespace HNR.Cards
{
    /// <summary>
    /// Calculates damage multipliers based on Soul Aspect matchups.
    /// Based on GDD effectiveness chart:
    /// - Strong vs: 1.25x damage
    /// - Neutral: 1.0x damage
    /// - Weak vs: 0.75x damage
    /// </summary>
    public static class AspectEffectiveness
    {
        /// <summary>Damage multiplier when attacker has advantage.</summary>
        public const float STRONG_MULTIPLIER = 1.25f;

        /// <summary>Damage multiplier when neutral matchup.</summary>
        public const float NEUTRAL_MULTIPLIER = 1.0f;

        /// <summary>Damage multiplier when attacker has disadvantage.</summary>
        public const float WEAK_MULTIPLIER = 0.75f;

        /// <summary>
        /// Get damage multiplier based on attacker and defender aspects.
        /// </summary>
        /// <param name="attackerAspect">Soul Aspect of the attacker</param>
        /// <param name="defenderAspect">Soul Aspect of the defender</param>
        /// <returns>Damage multiplier (0.75, 1.0, or 1.25)</returns>
        public static float GetMultiplier(SoulAspect attackerAspect, SoulAspect defenderAspect)
        {
            // None aspect is always neutral
            if (attackerAspect == SoulAspect.None || defenderAspect == SoulAspect.None)
            {
                return NEUTRAL_MULTIPLIER;
            }

            // Check effectiveness based on GDD chart
            return attackerAspect switch
            {
                // Flame: Strong vs Nature, Weak vs Shadow/Light
                SoulAspect.Flame => defenderAspect switch
                {
                    SoulAspect.Nature => STRONG_MULTIPLIER,
                    SoulAspect.Shadow => WEAK_MULTIPLIER,
                    SoulAspect.Light => WEAK_MULTIPLIER,
                    _ => NEUTRAL_MULTIPLIER
                },

                // Shadow: Strong vs Flame/Light, Weak vs Nature/Arcane
                SoulAspect.Shadow => defenderAspect switch
                {
                    SoulAspect.Flame => STRONG_MULTIPLIER,
                    SoulAspect.Light => STRONG_MULTIPLIER,
                    SoulAspect.Nature => WEAK_MULTIPLIER,
                    SoulAspect.Arcane => WEAK_MULTIPLIER,
                    _ => NEUTRAL_MULTIPLIER
                },

                // Nature: Strong vs Shadow/Arcane, Weak vs Flame
                SoulAspect.Nature => defenderAspect switch
                {
                    SoulAspect.Shadow => STRONG_MULTIPLIER,
                    SoulAspect.Arcane => STRONG_MULTIPLIER,
                    SoulAspect.Flame => WEAK_MULTIPLIER,
                    _ => NEUTRAL_MULTIPLIER
                },

                // Arcane: Strong vs Shadow, Weak vs Nature/Light
                SoulAspect.Arcane => defenderAspect switch
                {
                    SoulAspect.Shadow => STRONG_MULTIPLIER,
                    SoulAspect.Nature => WEAK_MULTIPLIER,
                    SoulAspect.Light => WEAK_MULTIPLIER,
                    _ => NEUTRAL_MULTIPLIER
                },

                // Light: Strong vs Flame/Arcane, Weak vs Shadow
                SoulAspect.Light => defenderAspect switch
                {
                    SoulAspect.Flame => STRONG_MULTIPLIER,
                    SoulAspect.Arcane => STRONG_MULTIPLIER,
                    SoulAspect.Shadow => WEAK_MULTIPLIER,
                    _ => NEUTRAL_MULTIPLIER
                },

                _ => NEUTRAL_MULTIPLIER
            };
        }

        /// <summary>
        /// Check if attacker has advantage over defender.
        /// </summary>
        public static bool IsStrong(SoulAspect attacker, SoulAspect defender)
        {
            return GetMultiplier(attacker, defender) > NEUTRAL_MULTIPLIER;
        }

        /// <summary>
        /// Check if attacker has disadvantage against defender.
        /// </summary>
        public static bool IsWeak(SoulAspect attacker, SoulAspect defender)
        {
            return GetMultiplier(attacker, defender) < NEUTRAL_MULTIPLIER;
        }
    }
}
