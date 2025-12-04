// ============================================
// PlayerPhase.cs
// Player plays cards and uses abilities
// ============================================

using UnityEngine;
using HNR.Core.Events;

namespace HNR.Combat
{
    /// <summary>
    /// Player plays cards and uses abilities.
    /// Waits for player input - does not auto-advance.
    /// </summary>
    public class PlayerPhase : ICombatPhase
    {
        public CombatPhase PhaseType => CombatPhase.PlayerPhase;

        public void Enter(CombatContext context)
        {
            Debug.Log("[PlayerPhase] Player turn started - waiting for input");
        }

        public void Update(CombatContext context)
        {
            // Check victory condition
            if (CheckVictory(context))
            {
                context.CombatEnded = true;
                context.PlayerVictory = true;
                return;
            }

            // Check defeat condition
            if (CheckDefeat(context))
            {
                context.CombatEnded = true;
                context.PlayerVictory = false;
                return;
            }

            // Wait for player input
            // TurnManager.EndPlayerTurn() or card plays advance the game
        }

        public void Exit(CombatContext context)
        {
            EventBus.Publish(new TurnEndedEvent(true));
            Debug.Log("[PlayerPhase] Player turn ended");
        }

        public CombatPhase GetNextPhase(CombatContext context)
        {
            if (context.PlayerVictory)
            {
                return CombatPhase.Victory;
            }

            if (context.CombatEnded)
            {
                return CombatPhase.Defeat;
            }

            return CombatPhase.EndPhase;
        }

        /// <summary>
        /// Check if all enemies are defeated.
        /// </summary>
        private bool CheckVictory(CombatContext context)
        {
            if (context.Enemies == null || context.Enemies.Count == 0)
            {
                return false;
            }

            foreach (var enemy in context.Enemies)
            {
                if (!enemy.IsDead)
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Check if team HP has reached zero.
        /// </summary>
        private bool CheckDefeat(CombatContext context)
        {
            return context.TeamHP <= 0;
        }
    }
}
