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

            // Perform post-combat cleanup (even on defeat for state consistency)
            PostCombatCleanup(context);

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

        /// <summary>
        /// Perform post-combat cleanup for all team members.
        /// Even on defeat, we clean up state for potential retry/meta progression.
        /// </summary>
        private void PostCombatCleanup(CombatContext context)
        {
            if (context.Team == null) return;

            foreach (var requiem in context.Team)
            {
                if (requiem == null) continue;

                // Reset corruption for Null State Requiems to 50
                if (requiem.InNullState)
                {
                    requiem.SetCorruption(50);
                    Debug.Log($"[DefeatPhase] {requiem.Name} corruption reset to 50 (was in Null State)");
                }

                // Reset Null State modifiers
                requiem.ResetNullStateModifiers();

                // Reset Art usage flag
                requiem.HasUsedArtThisCombat = false;
            }

            Debug.Log("[DefeatPhase] Post-combat cleanup complete");
        }
    }
}
