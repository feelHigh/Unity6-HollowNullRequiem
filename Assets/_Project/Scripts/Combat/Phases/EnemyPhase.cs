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
    /// Processes all enemy actions then checks for defeat.
    /// </summary>
    public class EnemyPhase : ICombatPhase
    {
        private int _currentEnemyIndex;
        private bool _phaseComplete;

        public CombatPhase PhaseType => CombatPhase.EnemyPhase;

        public void Enter(CombatContext context)
        {
            Debug.Log("[EnemyPhase] Enemy turn started");
            context.IsPlayerTurn = false;

            EventBus.Publish(new TurnStartedEvent(false, context.TurnNumber));

            _currentEnemyIndex = 0;
            _phaseComplete = false;
        }

        public void Update(CombatContext context)
        {
            if (_phaseComplete) return;

            // Process each enemy's action
            while (_currentEnemyIndex < context.Enemies.Count)
            {
                var enemy = context.Enemies[_currentEnemyIndex];
                if (!enemy.IsDead)
                {
                    ExecuteEnemyAction(enemy, context);
                }
                _currentEnemyIndex++;
            }

            _phaseComplete = true;

            // Check for defeat before transitioning
            if (context.TeamHP <= 0)
            {
                ServiceLocator.Get<TurnManager>()?.TransitionToPhase(CombatPhase.Defeat);
            }
            else
            {
                ServiceLocator.Get<TurnManager>()?.TransitionToPhase(GetNextPhase(context));
            }
        }

        /// <summary>
        /// Execute a single enemy's current intent.
        /// </summary>
        private void ExecuteEnemyAction(EnemyInstance enemy, CombatContext context)
        {
            var intent = enemy.GetCurrentIntent();
            if (intent == null)
            {
                Debug.LogWarning($"[EnemyPhase] {enemy.Name} has no intent");
                return;
            }

            Debug.Log($"[EnemyPhase] {enemy.Name} executes: {intent.IntentType}");

            switch (intent.IntentType)
            {
                case IntentType.Attack:
                    int damage = intent.Value > 0 ? intent.Value : (enemy.Data?.GetScaledDamage(1) ?? 5);
                    DealDamageToTeam(damage, context);
                    EventBus.Publish(new EnemyIntentExecutedEvent(enemy, intent));
                    break;

                case IntentType.AttackMultiple:
                    int hitDamage = intent.Value > 0 ? intent.Value : (enemy.Data?.GetScaledDamage(1) ?? 5);
                    int hits = intent.SecondaryValue > 0 ? intent.SecondaryValue : 1;
                    for (int i = 0; i < hits; i++)
                    {
                        DealDamageToTeam(hitDamage, context);
                    }
                    EventBus.Publish(new EnemyIntentExecutedEvent(enemy, intent));
                    break;

                case IntentType.Defend:
                    int block = intent.Value > 0 ? intent.Value : (enemy.Data?.BaseBlock ?? 5);
                    enemy.GainBlock(block);
                    Debug.Log($"[EnemyPhase] {enemy.Name} gains {block} block");
                    EventBus.Publish(new EnemyIntentExecutedEvent(enemy, intent));
                    break;

                case IntentType.Buff:
                    // TODO: Apply buff status effect
                    Debug.Log($"[EnemyPhase] {enemy.Name} buffs self");
                    EventBus.Publish(new EnemyIntentExecutedEvent(enemy, intent));
                    break;

                case IntentType.Debuff:
                    // TODO: Apply debuff to team
                    Debug.Log($"[EnemyPhase] {enemy.Name} debuffs team");
                    EventBus.Publish(new EnemyIntentExecutedEvent(enemy, intent));
                    break;

                case IntentType.Corrupt:
                    // TODO: Apply corruption
                    Debug.Log($"[EnemyPhase] {enemy.Name} corrupts team by {intent.Value}");
                    EventBus.Publish(new EnemyIntentExecutedEvent(enemy, intent));
                    break;

                default:
                    Debug.Log($"[EnemyPhase] {enemy.Name} does something unknown");
                    break;
            }

            enemy.AdvanceIntent();
        }

        /// <summary>
        /// Deal damage to the player's team, accounting for block.
        /// </summary>
        private void DealDamageToTeam(int damage, CombatContext context)
        {
            int blocked = Mathf.Min(damage, context.TeamBlock);
            context.TeamBlock -= blocked;
            int remaining = damage - blocked;

            if (blocked > 0)
            {
                EventBus.Publish(new BlockChangedEvent(context.TeamBlock, context.TeamBlock + blocked));
            }

            if (remaining > 0)
            {
                context.TeamHP -= remaining;
                EventBus.Publish(new TeamHPChangedEvent(context.TeamHP, context.TeamMaxHP, -remaining));
                Debug.Log($"[EnemyPhase] Team takes {remaining} damage ({blocked} blocked). HP: {context.TeamHP}/{context.TeamMaxHP}");
            }
            else
            {
                Debug.Log($"[EnemyPhase] Attack fully blocked ({blocked} damage blocked)");
            }
        }

        public void Exit(CombatContext context)
        {
            EventBus.Publish(new TurnEndedEvent(false));
        }

        public CombatPhase GetNextPhase(CombatContext context)
        {
            if (context.TeamHP <= 0)
            {
                return CombatPhase.Defeat;
            }

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

            return CombatPhase.DrawPhase;
        }
    }
}
