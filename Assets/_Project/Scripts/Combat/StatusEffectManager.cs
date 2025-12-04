// ============================================
// StatusEffectManager.cs
// Manages status effects on combat targets
// ============================================

using System.Collections.Generic;
using UnityEngine;
using HNR.Core;
using HNR.Core.Events;
using HNR.Characters;

namespace HNR.Combat
{
    /// <summary>
    /// Manages status effects applied to combat targets.
    /// Handles application, ticking, and removal of status effects.
    /// </summary>
    public class StatusEffectManager : MonoBehaviour
    {
        // ============================================
        // Private Fields
        // ============================================

        private Dictionary<ICombatTarget, List<StatusEffect>> _statusEffects = new();

        // ============================================
        // Unity Lifecycle
        // ============================================

        private void Awake()
        {
            ServiceLocator.Register(this);
        }

        private void OnDestroy()
        {
            ServiceLocator.Unregister<StatusEffectManager>();
        }

        private void OnEnable()
        {
            EventBus.Subscribe<TurnStartedEvent>(OnTurnStarted);
            EventBus.Subscribe<CombatEndedEvent>(OnCombatEnded);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<TurnStartedEvent>(OnTurnStarted);
            EventBus.Unsubscribe<CombatEndedEvent>(OnCombatEnded);
        }

        // ============================================
        // Event Handlers
        // ============================================

        private void OnTurnStarted(TurnStartedEvent evt)
        {
            if (evt.IsPlayerTurn)
            {
                TickAllEffects();
            }
        }

        private void OnCombatEnded(CombatEndedEvent evt)
        {
            ClearAllEffects();
        }

        // ============================================
        // Public Methods - Apply
        // ============================================

        /// <summary>
        /// Apply a status effect to a target.
        /// </summary>
        /// <param name="target">Target to apply effect to</param>
        /// <param name="statusType">Type of status effect</param>
        /// <param name="stacks">Number of stacks to apply</param>
        /// <param name="duration">Duration in turns (0 = permanent until cleared)</param>
        public void ApplyStatus(ICombatTarget target, StatusType statusType, int stacks = 1, int duration = 0)
        {
            if (target == null || stacks <= 0) return;

            if (!_statusEffects.ContainsKey(target))
            {
                _statusEffects[target] = new List<StatusEffect>();
            }

            // Check if effect already exists
            var existing = _statusEffects[target].Find(e => e.Type == statusType);
            if (existing != null)
            {
                // Stack the effect
                existing.Stacks += stacks;
                if (duration > 0 && duration > existing.Duration)
                {
                    existing.Duration = duration;
                }
                Debug.Log($"[StatusEffectManager] Stacked {statusType} on {target.Name}: {existing.Stacks} stacks");
            }
            else
            {
                // Add new effect
                var effect = new StatusEffect
                {
                    Type = statusType,
                    Stacks = stacks,
                    Duration = duration
                };
                _statusEffects[target].Add(effect);
                Debug.Log($"[StatusEffectManager] Applied {statusType} to {target.Name}: {stacks} stacks");
            }

            EventBus.Publish(new StatusAppliedEvent(target, statusType, stacks, duration));
        }

        /// <summary>
        /// Remove stacks of a status effect from a target.
        /// </summary>
        /// <param name="target">Target to remove effect from</param>
        /// <param name="statusType">Type of status effect</param>
        /// <param name="stacks">Number of stacks to remove (0 = all)</param>
        public void RemoveStatus(ICombatTarget target, StatusType statusType, int stacks = 0)
        {
            if (target == null) return;
            if (!_statusEffects.ContainsKey(target)) return;

            var effect = _statusEffects[target].Find(e => e.Type == statusType);
            if (effect == null) return;

            if (stacks <= 0 || stacks >= effect.Stacks)
            {
                // Remove entirely
                _statusEffects[target].Remove(effect);
                Debug.Log($"[StatusEffectManager] Removed {statusType} from {target.Name}");
            }
            else
            {
                // Reduce stacks
                effect.Stacks -= stacks;
                Debug.Log($"[StatusEffectManager] Reduced {statusType} on {target.Name}: {effect.Stacks} stacks remaining");
            }

            EventBus.Publish(new StatusRemovedEvent(target, statusType));
        }

        // ============================================
        // Public Methods - Query
        // ============================================

        /// <summary>
        /// Check if a target has a specific status effect.
        /// </summary>
        public bool HasStatus(ICombatTarget target, StatusType statusType)
        {
            if (target == null) return false;
            if (!_statusEffects.ContainsKey(target)) return false;

            return _statusEffects[target].Exists(e => e.Type == statusType);
        }

        /// <summary>
        /// Get the number of stacks of a status effect on a target.
        /// </summary>
        public int GetStatusStacks(ICombatTarget target, StatusType statusType)
        {
            if (target == null) return 0;
            if (!_statusEffects.ContainsKey(target)) return 0;

            var effect = _statusEffects[target].Find(e => e.Type == statusType);
            return effect?.Stacks ?? 0;
        }

        /// <summary>
        /// Get all status effects on a target.
        /// </summary>
        public List<StatusEffect> GetAllStatuses(ICombatTarget target)
        {
            if (target == null) return new List<StatusEffect>();
            if (!_statusEffects.ContainsKey(target)) return new List<StatusEffect>();

            return new List<StatusEffect>(_statusEffects[target]);
        }

        // ============================================
        // Turn Processing
        // ============================================

        /// <summary>
        /// Tick all status effects at turn start.
        /// Reduces durations and triggers per-turn effects.
        /// </summary>
        public void TickAllEffects()
        {
            var toRemove = new List<(ICombatTarget target, StatusEffect effect)>();

            foreach (var kvp in _statusEffects)
            {
                var target = kvp.Key;
                foreach (var effect in kvp.Value)
                {
                    // Apply per-turn effects
                    int tickValue = ApplyPerTurnEffect(target, effect);

                    // Reduce duration
                    if (effect.Duration > 0)
                    {
                        effect.Duration--;
                        if (effect.Duration <= 0)
                        {
                            toRemove.Add((target, effect));
                        }
                    }

                    EventBus.Publish(new StatusTickedEvent(target, effect.Type, tickValue));
                }
            }

            // Remove expired effects
            foreach (var (target, effect) in toRemove)
            {
                _statusEffects[target].Remove(effect);
                Debug.Log($"[StatusEffectManager] {effect.Type} expired on {target.Name}");
                EventBus.Publish(new StatusRemovedEvent(target, effect.Type));
            }
        }

        private int ApplyPerTurnEffect(ICombatTarget target, StatusEffect effect)
        {
            int tickValue = 0;

            switch (effect.Type)
            {
                case StatusType.Poison:
                    target.TakeDamage(effect.Stacks);
                    tickValue = effect.Stacks;
                    Debug.Log($"[StatusEffectManager] Poison dealt {effect.Stacks} damage to {target.Name}");
                    break;

                case StatusType.Regeneration:
                    target.Heal(effect.Stacks);
                    tickValue = effect.Stacks;
                    Debug.Log($"[StatusEffectManager] Regeneration healed {effect.Stacks} on {target.Name}");
                    break;

                case StatusType.Burn:
                    target.TakeDamage(effect.Stacks);
                    tickValue = effect.Stacks;
                    effect.Stacks = Mathf.Max(0, effect.Stacks - 1); // Burn reduces by 1 each turn
                    Debug.Log($"[StatusEffectManager] Burn dealt damage, reduced to {effect.Stacks} stacks");
                    break;

                // Other per-turn effects can be added here
            }

            return tickValue;
        }

        /// <summary>
        /// Clear all status effects from a target.
        /// </summary>
        public void ClearEffects(ICombatTarget target)
        {
            if (target == null) return;
            if (_statusEffects.ContainsKey(target))
            {
                _statusEffects[target].Clear();
                Debug.Log($"[StatusEffectManager] Cleared all effects from {target.Name}");
            }
        }

        /// <summary>
        /// Clear all status effects (end of combat).
        /// </summary>
        public void ClearAllEffects()
        {
            _statusEffects.Clear();
            Debug.Log("[StatusEffectManager] Cleared all status effects");
        }

        // ============================================
        // Modifier Methods
        // ============================================

        /// <summary>
        /// Get damage multiplier from status effects.
        /// </summary>
        public float GetDamageMultiplier(ICombatTarget target)
        {
            float multiplier = 1f;

            if (HasStatus(target, StatusType.Strength))
            {
                multiplier += 0.5f; // +50% damage
            }

            if (HasStatus(target, StatusType.Weakness))
            {
                multiplier -= 0.25f; // -25% damage
            }

            return Mathf.Max(0.1f, multiplier);
        }

        /// <summary>
        /// Get damage taken multiplier from status effects.
        /// </summary>
        public float GetDamageTakenMultiplier(ICombatTarget target)
        {
            float multiplier = 1f;

            if (HasStatus(target, StatusType.Vulnerability))
            {
                multiplier += 0.5f; // +50% damage taken
            }

            if (HasStatus(target, StatusType.Protected))
            {
                multiplier -= 0.25f; // -25% damage taken
            }

            return Mathf.Max(0.1f, multiplier);
        }
    }

    // ============================================
    // Status Effect Data
    // ============================================

    /// <summary>
    /// Runtime status effect instance.
    /// </summary>
    public class StatusEffect
    {
        public StatusType Type;
        public int Stacks;
        public int Duration; // 0 = permanent until cleared
    }
}
