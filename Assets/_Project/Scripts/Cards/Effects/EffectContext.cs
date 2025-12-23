// ============================================
// EffectContext.cs
// Runtime context for card effect execution
// ============================================

using System.Collections.Generic;
using HNR.Combat;
using HNR.Core.Events;

// Resolve ambiguity: use real implementations from proper namespaces
using RequiemDataSO = HNR.Characters.RequiemDataSO;
using RequiemInstance = HNR.Characters.RequiemInstance;
using EnemyInstance = HNR.Combat.EnemyInstance;

namespace HNR.Cards
{
    /// <summary>
    /// Runtime context passed to card effect handlers.
    /// Contains all information needed to execute effects.
    /// </summary>
    public class EffectContext
    {
        // ============================================
        // Card Information
        // ============================================

        /// <summary>The card instance being played.</summary>
        public CardInstance Card { get; set; }

        /// <summary>The Requiem data that owns this card (can be null for neutral cards).</summary>
        public RequiemDataSO Source { get; set; }

        /// <summary>The Requiem instance that played this card (for event publishing).</summary>
        public RequiemInstance SourceInstance { get; set; }

        // ============================================
        // Targeting
        // ============================================

        /// <summary>Primary target of the card effect.</summary>
        public ICombatTarget Target { get; set; }

        /// <summary>All targets when card affects multiple entities.</summary>
        public List<ICombatTarget> AllTargets { get; set; } = new();

        // ============================================
        // Manager References
        // ============================================

        /// <summary>Reference to TurnManager for combat operations.</summary>
        public TurnManager TurnManager { get; set; }

        /// <summary>Reference to the current combat context.</summary>
        public CombatContext CombatContext { get; set; }

        /// <summary>Reference to DeckManager for card operations.</summary>
        public DeckManager DeckManager { get; set; }

        // ============================================
        // Runtime Modifiers
        // ============================================

        /// <summary>Damage multiplier from buffs/debuffs.</summary>
        public float DamageMultiplier { get; set; } = 1f;

        /// <summary>Block multiplier from buffs/debuffs.</summary>
        public float BlockMultiplier { get; set; } = 1f;

        /// <summary>Heal multiplier from buffs/debuffs.</summary>
        public float HealMultiplier { get; set; } = 1f;

        // ============================================
        // Helper Methods
        // ============================================

        /// <summary>
        /// Get all enemies from combat context.
        /// </summary>
        public List<EnemyInstance> GetAllEnemies()
        {
            return CombatContext?.Enemies ?? new List<EnemyInstance>();
        }

        /// <summary>
        /// Get all Requiems from combat context.
        /// </summary>
        public List<RequiemInstance> GetAllRequiems()
        {
            return CombatContext?.Team ?? new List<RequiemInstance>();
        }

        /// <summary>
        /// Calculate final damage with multipliers.
        /// </summary>
        /// <param name="baseDamage">Raw damage value</param>
        /// <returns>Modified damage value</returns>
        public int CalculateDamage(int baseDamage)
        {
            return UnityEngine.Mathf.RoundToInt(baseDamage * DamageMultiplier);
        }

        /// <summary>
        /// Calculate final block with multipliers.
        /// </summary>
        /// <param name="baseBlock">Raw block value</param>
        /// <returns>Modified block value</returns>
        public int CalculateBlock(int baseBlock)
        {
            return UnityEngine.Mathf.RoundToInt(baseBlock * BlockMultiplier);
        }

        /// <summary>
        /// Calculate final heal with multipliers.
        /// </summary>
        /// <param name="baseHeal">Raw heal value</param>
        /// <returns>Modified heal value</returns>
        public int CalculateHeal(int baseHeal)
        {
            return UnityEngine.Mathf.RoundToInt(baseHeal * HealMultiplier);
        }
    }
}
