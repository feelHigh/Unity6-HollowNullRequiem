// ============================================
// ApplyStatusEffect.cs
// Status effect application implementation
// ============================================

using UnityEngine;
using HNR.Core;
using HNR.Core.Events;
using HNR.Combat;
using HNR.Characters;

namespace HNR.Cards
{
    /// <summary>
    /// Apply a status effect to a target.
    /// Each instance is configured with a specific StatusType.
    /// </summary>
    public class ApplyStatusEffect : ICardEffect
    {
        private readonly StatusType _statusType;

        /// <summary>
        /// Create a status effect applicator for a specific status type.
        /// </summary>
        /// <param name="statusType">The type of status to apply</param>
        public ApplyStatusEffect(StatusType statusType)
        {
            _statusType = statusType;
        }

        public void Execute(CardEffectData data, EffectContext context)
        {
            if (context.Target == null)
            {
                Debug.LogWarning($"[ApplyStatusEffect] No target for {_statusType}");
                return;
            }

            int stacks = data.Value;
            int duration = data.Duration;

            // Try to get StatusEffectManager from context or ServiceLocator
            var statusManager = context.CombatContext?.StatusManager;
            if (statusManager == null)
            {
                ServiceLocator.TryGet<StatusEffectManager>(out statusManager);
            }

            if (statusManager != null)
            {
                // Apply through StatusEffectManager
                statusManager.ApplyStatus(context.Target, _statusType, stacks, duration);
                Debug.Log($"[ApplyStatusEffect] Applied {stacks} {_statusType} to {context.Target.Name} via StatusEffectManager");
            }
            else
            {
                // Log intended effect if no manager available
                Debug.Log($"[ApplyStatusEffect] Would apply {stacks} {_statusType} to {context.Target.Name} for {duration} turns (StatusEffectManager not available)");
            }

            // Always publish event for UI updates
            EventBus.Publish(new StatusAppliedEvent(context.Target, _statusType, stacks, duration));
        }
    }

    /// <summary>
    /// Remove a status effect from a target.
    /// </summary>
    public class RemoveStatusEffect : ICardEffect
    {
        private readonly StatusType _statusType;

        public RemoveStatusEffect(StatusType statusType)
        {
            _statusType = statusType;
        }

        public void Execute(CardEffectData data, EffectContext context)
        {
            if (context.Target == null)
            {
                Debug.LogWarning($"[RemoveStatusEffect] No target for {_statusType}");
                return;
            }

            var statusManager = context.CombatContext?.StatusManager;
            if (statusManager == null)
            {
                ServiceLocator.TryGet<StatusEffectManager>(out statusManager);
            }

            if (statusManager != null)
            {
                statusManager.RemoveStatus(context.Target, _statusType);
                Debug.Log($"[RemoveStatusEffect] Removed {_statusType} from {context.Target.Name}");
            }

            EventBus.Publish(new StatusRemovedEvent(context.Target, _statusType));
        }
    }

    /// <summary>
    /// Apply status effect to all enemies.
    /// </summary>
    public class ApplyStatusAllEnemiesEffect : ICardEffect
    {
        private readonly StatusType _statusType;

        public ApplyStatusAllEnemiesEffect(StatusType statusType)
        {
            _statusType = statusType;
        }

        public void Execute(CardEffectData data, EffectContext context)
        {
            var enemies = context.GetAllEnemies();
            if (enemies.Count == 0)
            {
                Debug.LogWarning($"[ApplyStatusAllEnemiesEffect] No enemies to target");
                return;
            }

            int stacks = data.Value;
            int duration = data.Duration;

            var statusManager = context.CombatContext?.StatusManager;
            if (statusManager == null)
            {
                ServiceLocator.TryGet<StatusEffectManager>(out statusManager);
            }

            foreach (var enemy in enemies)
            {
                if (enemy.IsDead) continue;

                if (statusManager != null)
                {
                    statusManager.ApplyStatus(enemy, _statusType, stacks, duration);
                }

                // Publish event for each enemy
                // Note: EnemyInstance doesn't implement ICombatTarget yet
                // EventBus.Publish(new StatusAppliedEvent(enemy, _statusType, stacks, duration));
            }

            Debug.Log($"[ApplyStatusAllEnemiesEffect] Applied {stacks} {_statusType} to {enemies.Count} enemies");
        }
    }
}
