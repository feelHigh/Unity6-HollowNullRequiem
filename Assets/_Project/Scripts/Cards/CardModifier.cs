// ============================================
// CardModifier.cs
// Temporary modifier applied to card instances
// ============================================

namespace HNR.Cards
{
    /// <summary>
    /// Temporary modifier applied to a card instance.
    /// Examples: cost reduction from relics, damage bonus from buffs.
    /// </summary>
    public class CardModifier
    {
        // ============================================
        // Properties
        // ============================================

        /// <summary>Type of modification (Cost, DamageBonus, BlockBonus).</summary>
        public ModifierType Type { get; }

        /// <summary>Modifier value (positive or negative).</summary>
        public int Value { get; }

        /// <summary>Turns remaining. 0 or less = permanent.</summary>
        public int RemainingTurns { get; private set; }

        /// <summary>Source of modifier for UI/debugging.</summary>
        public string Source { get; }

        /// <summary>True if modifier never expires.</summary>
        public bool IsPermanent => RemainingTurns <= 0;

        // ============================================
        // Constructor
        // ============================================

        /// <summary>
        /// Create a new card modifier.
        /// </summary>
        /// <param name="type">Type of modification</param>
        /// <param name="value">Modifier value (positive or negative)</param>
        /// <param name="duration">Duration in turns (0 or less = permanent)</param>
        /// <param name="source">Source identifier for debugging</param>
        public CardModifier(ModifierType type, int value, int duration, string source = "")
        {
            Type = type;
            Value = value;
            RemainingTurns = duration;
            Source = source ?? "";
        }

        // ============================================
        // Public Methods
        // ============================================

        /// <summary>
        /// Reduce duration by one turn.
        /// </summary>
        /// <returns>True if modifier has expired and should be removed</returns>
        public bool Tick()
        {
            // Permanent modifiers never expire
            if (IsPermanent) return false;

            RemainingTurns--;
            return RemainingTurns <= 0;
        }

        /// <summary>
        /// Get formatted string for debugging.
        /// </summary>
        public override string ToString()
        {
            string duration = IsPermanent ? "permanent" : $"{RemainingTurns} turns";
            string sourceInfo = string.IsNullOrEmpty(Source) ? "" : $" [{Source}]";
            return $"{Type}: {Value:+#;-#;0} ({duration}){sourceInfo}";
        }
    }
}
