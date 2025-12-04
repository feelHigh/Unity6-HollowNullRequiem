// ============================================
// CombatContext.cs
// Shared context for all combat phases
// ============================================

using System.Collections.Generic;

namespace HNR.Combat
{
    /// <summary>
    /// Shared context for all combat phases.
    /// Contains all state needed during combat.
    /// </summary>
    public class CombatContext
    {
        // ============================================
        // Team State
        // ============================================

        /// <summary>Current team HP (shared pool).</summary>
        public int TeamHP { get; set; }

        /// <summary>Maximum team HP.</summary>
        public int TeamMaxHP { get; set; }

        /// <summary>Current team Block (absorbs damage).</summary>
        public int TeamBlock { get; set; }

        // ============================================
        // Resources
        // ============================================

        /// <summary>Current Action Points available this turn.</summary>
        public int CurrentAP { get; set; }

        /// <summary>Maximum AP per turn.</summary>
        public int MaxAP { get; set; }

        /// <summary>Soul Essence resource for special abilities.</summary>
        public int SoulEssence { get; set; }

        // ============================================
        // Turn Tracking
        // ============================================

        /// <summary>Current turn number (starts at 1).</summary>
        public int TurnNumber { get; set; }

        /// <summary>True during player phase, false during enemy phase.</summary>
        public bool IsPlayerTurn { get; set; }

        // ============================================
        // Combat State Flags
        // ============================================

        /// <summary>True when combat has ended (victory or defeat).</summary>
        public bool CombatEnded { get; set; }

        /// <summary>True if player won, false if defeated.</summary>
        public bool PlayerVictory { get; set; }
    }
}
