// ============================================
// EnemyPhase.cs
// Enemies execute their telegraphed intents
// ============================================

using UnityEngine;
using HNR.Core;
using HNR.Core.Events;
using HNR.Characters;
using HNR.Characters.Visuals;

namespace HNR.Combat
{
    /// <summary>
    /// Enemies execute their telegraphed intents.
    /// Processes enemy actions with animation delays so player can see them.
    /// </summary>
    public class EnemyPhase : ICombatPhase
    {
        private int _currentEnemyIndex;
        private bool _phaseComplete;
        private bool _waitingForAnimation;
        private float _animationTimer;

        // Timing constants
        private const float ATTACK_ANIMATION_DELAY = 0.6f;
        private const float POST_ATTACK_DELAY = 0.3f;

        public CombatPhase PhaseType => CombatPhase.EnemyPhase;

        public void Enter(CombatContext context)
        {
            Debug.Log("[EnemyPhase] Enemy turn started");
            context.IsPlayerTurn = false;

            EventBus.Publish(new TurnStartedEvent(false, context.TurnNumber));

            _currentEnemyIndex = 0;
            _phaseComplete = false;
            _waitingForAnimation = false;
            _animationTimer = 0f;
        }

        public void Update(CombatContext context)
        {
            if (_phaseComplete) return;

            // Wait for animation to complete
            if (_waitingForAnimation)
            {
                _animationTimer -= Time.deltaTime;
                if (_animationTimer > 0f)
                {
                    return;
                }
                _waitingForAnimation = false;
                _currentEnemyIndex++;
            }

            // Process next enemy action
            while (_currentEnemyIndex < context.Enemies.Count)
            {
                var enemy = context.Enemies[_currentEnemyIndex];
                if (!enemy.IsDead)
                {
                    ExecuteEnemyAction(enemy, context);

                    // Wait for animation before processing next enemy
                    _waitingForAnimation = true;
                    _animationTimer = ATTACK_ANIMATION_DELAY + POST_ATTACK_DELAY;
                    return;
                }
                _currentEnemyIndex++;
            }

            // All enemies processed
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

            // Get enemy's preferred attack type for animations
            var attackType = enemy.Data?.PreferredAttackType ?? AttackType.Slash;

            switch (intent.IntentType)
            {
                case IntentType.Attack:
                    // Play attack animation
                    enemy.Visual?.PlayAttack(attackType);

                    int damage = intent.Value > 0 ? intent.Value : (enemy.Data?.GetScaledDamage(1) ?? 5);
                    DealDamageToTeam(damage, context, enemy);
                    EventBus.Publish(new EnemyIntentExecutedEvent(enemy, intent));
                    break;

                case IntentType.AttackMultiple:
                    // Play attack animation
                    enemy.Visual?.PlayAttack(attackType);

                    int hitDamage = intent.Value > 0 ? intent.Value : (enemy.Data?.GetScaledDamage(1) ?? 5);
                    int hits = intent.SecondaryValue > 0 ? intent.SecondaryValue : 1;
                    for (int i = 0; i < hits; i++)
                    {
                        DealDamageToTeam(hitDamage, context, enemy);
                    }
                    EventBus.Publish(new EnemyIntentExecutedEvent(enemy, intent));
                    break;

                case IntentType.Defend:
                    // Play block/defend animation
                    enemy.Visual?.PlayBlock();

                    int block = intent.Value > 0 ? intent.Value : (enemy.Data?.BaseBlock ?? 5);
                    enemy.GainBlock(block);
                    Debug.Log($"[EnemyPhase] {enemy.Name} gains {block} block");
                    EventBus.Publish(new EnemyIntentExecutedEvent(enemy, intent));
                    break;

                case IntentType.Buff:
                    // Play skill animation for buff
                    enemy.Visual?.PlaySkill();

                    // Apply Strength buff to self
                    var buffStatusMgr = ServiceLocator.Get<StatusEffectManager>();
                    int buffStacks = intent.Value > 0 ? intent.Value : 2;
                    buffStatusMgr?.ApplyStatus(enemy, StatusType.Strength, buffStacks);
                    Debug.Log($"[EnemyPhase] {enemy.Name} gains {buffStacks} Strength");
                    EventBus.Publish(new EnemyIntentExecutedEvent(enemy, intent));
                    break;

                case IntentType.Debuff:
                    // Play skill animation for debuff
                    enemy.Visual?.PlaySkill();

                    // Apply Weakness to random team member
                    var debuffStatusMgr = ServiceLocator.Get<StatusEffectManager>();
                    if (context.Team != null && context.Team.Count > 0)
                    {
                        int targetIndex = Random.Range(0, context.Team.Count);
                        var target = context.Team[targetIndex];
                        int debuffStacks = intent.Value > 0 ? intent.Value : 2;
                        debuffStatusMgr?.ApplyStatus(target, StatusType.Weakness, debuffStacks, 2);
                        Debug.Log($"[EnemyPhase] {enemy.Name} applies {debuffStacks} Weakness to {target.Name}");
                    }
                    EventBus.Publish(new EnemyIntentExecutedEvent(enemy, intent));
                    break;

                case IntentType.Corrupt:
                    // Play skill animation for corruption
                    enemy.Visual?.PlaySkill();

                    // Apply corruption to all team members
                    int corruptionAmount = intent.Value > 0 ? intent.Value : 5;
                    foreach (var requiem in context.Team)
                    {
                        requiem.AddCorruption(corruptionAmount);
                    }
                    Debug.Log($"[EnemyPhase] {enemy.Name} corrupts team by {corruptionAmount}");
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
        /// Also adds corruption to a random Requiem based on unblocked damage.
        /// </summary>
        /// <param name="damage">Raw damage amount</param>
        /// <param name="context">Combat context</param>
        /// <param name="source">The enemy dealing damage (for relic triggers)</param>
        private void DealDamageToTeam(int damage, CombatContext context, EnemyInstance source = null)
        {
            int blocked = Mathf.Min(damage, context.TeamBlock);
            context.TeamBlock -= blocked;
            int remaining = damage - blocked;

            if (blocked > 0)
            {
                EventBus.Publish(new BlockChangedEvent(context.TeamBlock, context.TeamBlock + blocked));
            }

            // Publish DamageTakenEvent for relic triggers (even if fully blocked)
            EventBus.Publish(new DamageTakenEvent(source, remaining, blocked));

            if (remaining > 0)
            {
                context.TeamHP -= remaining;

                // Play hit animation on a random Requiem and add corruption
                if (context.Team != null && context.Team.Count > 0)
                {
                    int targetIndex = Random.Range(0, context.Team.Count);
                    var hitRequiem = context.Team[targetIndex];
                    hitRequiem?.Visual?.PlayHit();

                    // Add corruption based on damage taken (1 corruption per 5 damage, minimum 1)
                    int corruptionGain = Mathf.Max(1, remaining / 5);
                    hitRequiem?.AddCorruption(corruptionGain);
                    Debug.Log($"[EnemyPhase] {hitRequiem?.Name} gains {corruptionGain} corruption from damage");

                    // Include position in event for damage number spawning
                    EventBus.Publish(new TeamHPChangedEvent(context.TeamHP, context.TeamMaxHP, -remaining, hitRequiem?.Position));
                }
                else
                {
                    EventBus.Publish(new TeamHPChangedEvent(context.TeamHP, context.TeamMaxHP, -remaining));
                }

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
