// ============================================
// EnemyPhase.cs
// Enemies execute their telegraphed intents
// ============================================

using UnityEngine;
using HNR.Core;
using HNR.Core.Events;

namespace HNR.Combat
{
    /// <summary>
    /// Enemies execute their telegraphed intents.
    /// Processes all enemy actions then checks for victory/defeat.
    /// </summary>
    public class EnemyPhase : ICombatPhase
    {
        private bool _actionsComplete;

        public CombatPhase PhaseType => CombatPhase.EnemyPhase;

        public void Enter(CombatContext context)
        {
            _actionsComplete = false;
            context.IsPlayerTurn = false;

            EventBus.Publish(new TurnStartedEvent(false, context.TurnNumber));

            Debug.Log($"[EnemyPhase] {context.Enemies.Count} enemies acting");

            // Execute each enemy's intent
            foreach (var enemy in context.Enemies)
            {
                if (enemy.IsDead) continue;

                ExecuteEnemyIntent(enemy, context);
            }

            _actionsComplete = true;
            Debug.Log("[EnemyPhase] All enemies have acted");
        }

        public void Update(CombatContext context)
        {
            // Check for defeat after enemy actions
            if (context.TeamHP <= 0)
            {
                context.CombatEnded = true;
                context.PlayerVictory = false;
                ServiceLocator.Get<TurnManager>()?.TransitionToPhase(CombatPhase.Defeat);
                return;
            }

            if (_actionsComplete)
            {
                _actionsComplete = false;
                ServiceLocator.Get<TurnManager>()?.TransitionToPhase(GetNextPhase(context));
            }
        }

        public void Exit(CombatContext context)
        {
            EventBus.Publish(new TurnEndedEvent(false));
        }

        public CombatPhase GetNextPhase(CombatContext context)
        {
            // Check if all enemies defeated
            bool allDefeated = true;
            foreach (var enemy in context.Enemies)
            {
                if (!enemy.IsDead)
                {
                    allDefeated = false;
                    break;
                }
            }

            if (allDefeated)
            {
                return CombatPhase.Victory;
            }

            // Continue to next player turn
            return CombatPhase.DrawPhase;
        }

        /// <summary>
        /// Execute a single enemy's current intent.
        /// </summary>
        private void ExecuteEnemyIntent(EnemyInstance enemy, CombatContext context)
        {
            // TODO: Get intent from enemy's pattern
            // TODO: Execute intent based on type (Attack, Defend, Buff, etc.)
            // TODO: Advance enemy's intent pattern

            Debug.Log($"[EnemyPhase] Enemy executes intent");

            // Placeholder: Basic attack for now
            // var turnManager = ServiceLocator.Get<TurnManager>();
            // turnManager?.DamageTeam(enemy.Data?.BaseAttack ?? 5);
        }
    }
}
