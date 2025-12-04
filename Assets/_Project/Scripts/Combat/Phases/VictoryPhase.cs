// ============================================
// VictoryPhase.cs
// All enemies defeated - show rewards
// ============================================

using UnityEngine;
using HNR.Core;

namespace HNR.Combat
{
    /// <summary>
    /// All enemies defeated - show rewards.
    /// Terminal phase that ends combat.
    /// </summary>
    public class VictoryPhase : ICombatPhase
    {
        public CombatPhase PhaseType => CombatPhase.Victory;

        public void Enter(CombatContext context)
        {
            context.CombatEnded = true;
            context.PlayerVictory = true;

            Debug.Log("[VictoryPhase] Victory! All enemies defeated.");

            // End combat with victory
            ServiceLocator.Get<TurnManager>()?.EndCombat(true);
        }

        public void Update(CombatContext context)
        {
            // Terminal phase - no updates
        }

        public void Exit(CombatContext context)
        {
            // No cleanup needed
        }

        public CombatPhase GetNextPhase(CombatContext context)
        {
            // Terminal phase - stays in victory
            return CombatPhase.Victory;
        }
    }
}
