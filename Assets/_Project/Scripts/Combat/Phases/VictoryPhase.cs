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

            // Perform post-combat cleanup
            PostCombatCleanup(context);

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

        /// <summary>
        /// Perform post-combat cleanup for all team members.
        /// - Resets Null State corruption to 50 (not 0)
        /// - Clears temporary modifiers
        /// - Resets HasUsedArtThisCombat flags
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
                    Debug.Log($"[VictoryPhase] {requiem.Name} corruption reset to 50 (was in Null State)");
                }

                // Reset Null State modifiers
                requiem.ResetNullStateModifiers();

                // Reset Art usage flag
                requiem.HasUsedArtThisCombat = false;
            }

            // SE resets to 0 (handled by CombatContext.Reset() when new combat starts)
            Debug.Log("[VictoryPhase] Post-combat cleanup complete");
        }
    }
}
