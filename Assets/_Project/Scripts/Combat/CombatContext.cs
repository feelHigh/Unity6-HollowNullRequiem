// ============================================
// CombatContext.cs
// Shared context for all combat phases
// ============================================

using System.Collections.Generic;
using HNR.Core.Events;

namespace HNR.Combat
{
    // ============================================
    // FORWARD DECLARATIONS - Placeholder Types
    // TODO: Move to proper locations when implemented
    // ============================================

    /// <summary>
    /// Placeholder: Manages status effects on combatants.
    /// TODO: Implement in Scripts/Combat/StatusEffects/StatusEffectManager.cs
    /// </summary>
    public class StatusEffectManager
    {
        public void TickEffects() { }
        public void ApplyEffect(object target, string effectId, int stacks) { }
        public void ClearEffects(object target) { }
    }

    // ============================================
    // COMBAT CONTEXT
    // ============================================

    /// <summary>
    /// Shared context passed to all combat phases.
    /// Contains complete state of the current combat encounter.
    /// </summary>
    public class CombatContext
    {
        // ============================================
        // Team State
        // ============================================

        /// <summary>The active Requiem team in this combat.</summary>
        public List<RequiemInstance> Team { get; set; } = new();

        /// <summary>Current team HP (shared pool).</summary>
        public int TeamHP { get; set; }

        /// <summary>Maximum team HP.</summary>
        public int TeamMaxHP { get; set; }

        /// <summary>Current team Block (absorbs damage, resets each turn).</summary>
        public int TeamBlock { get; set; }

        // ============================================
        // Resources
        // ============================================

        /// <summary>Current Action Points available this turn.</summary>
        public int CurrentAP { get; set; }

        /// <summary>Maximum AP per turn (default 3).</summary>
        public int MaxAP { get; set; } = 3;

        /// <summary>Soul Essence resource for special abilities.</summary>
        public int SoulEssence { get; set; }

        // ============================================
        // Enemies
        // ============================================

        /// <summary>Active enemies in this combat encounter.</summary>
        public List<EnemyInstance> Enemies { get; set; } = new();

        // ============================================
        // Turn Tracking
        // ============================================

        /// <summary>Current turn number (starts at 1).</summary>
        public int TurnNumber { get; set; }

        /// <summary>True during player phase, false during enemy phase.</summary>
        public bool IsPlayerTurn { get; set; }

        // ============================================
        // Manager References
        // ============================================

        /// <summary>Reference to deck management system.</summary>
        public DeckManager DeckManager { get; set; }

        /// <summary>Reference to hand management system.</summary>
        public HandManager HandManager { get; set; }

        /// <summary>Reference to status effect management system.</summary>
        public StatusEffectManager StatusManager { get; set; }

        // ============================================
        // Combat State Flags
        // ============================================

        /// <summary>True when combat has ended (victory or defeat).</summary>
        public bool CombatEnded { get; set; }

        /// <summary>True if player won, false if defeated.</summary>
        public bool PlayerVictory { get; set; }

        // ============================================
        // Methods
        // ============================================

        /// <summary>
        /// Reset all combat state to default values.
        /// Called when initializing a new combat encounter.
        /// </summary>
        public void Reset()
        {
            Team.Clear();
            Enemies.Clear();
            TeamHP = 0;
            TeamMaxHP = 0;
            TeamBlock = 0;
            CurrentAP = 0;
            MaxAP = 3;
            SoulEssence = 0;
            TurnNumber = 0;
            IsPlayerTurn = false;
            DeckManager = null;
            HandManager = null;
            StatusManager = null;
            CombatEnded = false;
            PlayerVictory = false;
        }
    }
}
