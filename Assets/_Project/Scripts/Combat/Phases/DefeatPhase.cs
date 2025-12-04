// ============================================
// DefeatPhase.cs
// Team HP reached 0 - run ends
// ============================================

using UnityEngine;
using HNR.Core;

namespace HNR.Combat
{
    /// <summary>
    /// Team HP reached 0 - run ends.
    /// Terminal phase that ends combat.
    /// </summary>
    public class DefeatPhase : ICombatPhase
    {
        public CombatPhase PhaseType => CombatPhase.Defeat;

        public void Enter(CombatContext context)
        {
            context.CombatEnded = true;
            context.PlayerVictory = false;

            Debug.Log("[DefeatPhase] Defeat! Team HP reached 0.");

            // End combat with defeat
            ServiceLocator.Get<TurnManager>()?.EndCombat(false);
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
            // Terminal phase - stays in defeat
            return CombatPhase.Defeat;
        }
    }
}
