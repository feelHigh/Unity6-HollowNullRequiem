// ============================================
// EnemyInstance.cs
// Runtime enemy instance implementing ICombatTarget
// ============================================

using UnityEngine;
using HNR.Core;
using HNR.Core.Events;
using HNR.Characters;

namespace HNR.Combat
{
    /// <summary>
    /// Runtime instance of an enemy in combat.
    /// Implements ICombatTarget for targeting system integration.
    /// </summary>
    public class EnemyInstance : MonoBehaviour, ICombatTarget
    {
        // ============================================
        // Configuration
        // ============================================

        [Header("References")]
        [SerializeField, Tooltip("Enemy data asset")]
        private EnemyDataSO _data;

        [SerializeField, Tooltip("Sprite renderer for enemy visuals")]
        private SpriteRenderer _sprite;

        [SerializeField, Tooltip("Highlight ring for targeting feedback")]
        private GameObject _highlightRing;

        // ============================================
        // Runtime State
        // ============================================

        private int _currentHP;
        private int _maxHP;
        private int _block;
        private int _zone = 1;
        private IntentPattern _intentPattern;
        private System.Collections.Generic.Dictionary<StatusType, int> _statusEffects = new();

        // ============================================
        // Properties
        // ============================================

        /// <summary>Enemy data asset.</summary>
        public EnemyDataSO Data => _data;

        /// <summary>Display name of the enemy.</summary>
        public string Name => _data?.EnemyName ?? "Unknown";

        /// <summary>World position for targeting and effects.</summary>
        public Vector3 Position => transform.position;

        /// <summary>True if this enemy has been defeated.</summary>
        public bool IsDead => _currentHP <= 0;

        /// <summary>Current HP.</summary>
        public int CurrentHP => _currentHP;

        /// <summary>Maximum HP (zone-scaled).</summary>
        public int MaxHP => _maxHP;

        /// <summary>Current Block value.</summary>
        public int Block => _block;

        /// <summary>Current zone (for scaling).</summary>
        public int Zone => _zone;

        // ============================================
        // Initialization
        // ============================================

        /// <summary>
        /// Initialize the enemy instance with data and zone scaling.
        /// </summary>
        /// <param name="data">Enemy data asset</param>
        /// <param name="zone">Current zone for stat scaling (1-3)</param>
        public void Initialize(EnemyDataSO data, int zone = 1)
        {
            _data = data;
            _zone = zone;
            _maxHP = data.GetScaledHP(zone);
            _currentHP = _maxHP;
            _block = 0;
            _statusEffects.Clear();

            // Clone intent pattern for runtime use
            _intentPattern = data.IntentPattern?.Clone();
            _intentPattern?.Reset();

            // Set up visuals
            if (_sprite != null && data.Sprite != null)
            {
                _sprite.sprite = data.Sprite;
                _sprite.transform.localScale = Vector3.one * data.SpriteScale;
            }

            if (_highlightRing != null)
            {
                _highlightRing.SetActive(false);
            }

            Debug.Log($"[EnemyInstance] {Name} initialized: HP {_currentHP}/{_maxHP} (Zone {zone})");
        }

        // ============================================
        // ICombatTarget Implementation
        // ============================================

        /// <summary>
        /// Apply damage to this enemy. Block absorbs damage first.
        /// </summary>
        /// <param name="amount">Raw damage amount before mitigation</param>
        public void TakeDamage(int amount)
        {
            if (IsDead) return;

            // Apply vulnerability if present
            if (HasStatus(StatusType.Vulnerability))
            {
                amount = Mathf.RoundToInt(amount * 1.5f);
            }

            // Block absorbs damage first
            int blocked = Mathf.Min(amount, _block);
            _block -= blocked;
            int remaining = amount - blocked;

            if (remaining > 0)
            {
                _currentHP = Mathf.Max(0, _currentHP - remaining);
                EventBus.Publish(new EnemyDamagedEvent(this, remaining, blocked));
            }

            Debug.Log($"[EnemyInstance] {Name} took {remaining} damage (blocked {blocked}). HP: {_currentHP}/{_maxHP}");

            if (_currentHP <= 0)
            {
                Die();
            }
        }

        /// <summary>
        /// Heal this enemy.
        /// </summary>
        /// <param name="amount">Amount to heal</param>
        public void Heal(int amount)
        {
            if (IsDead) return;

            int previousHP = _currentHP;
            _currentHP = Mathf.Min(_currentHP + amount, _maxHP);
            int actualHeal = _currentHP - previousHP;

            if (actualHeal > 0)
            {
                Debug.Log($"[EnemyInstance] {Name} healed for {actualHeal}. HP: {_currentHP}/{_maxHP}");
            }
        }

        /// <summary>
        /// Show or hide targeting highlight effect.
        /// </summary>
        /// <param name="show">True to show, false to hide</param>
        public void ShowTargetHighlight(bool show)
        {
            if (_highlightRing != null)
            {
                _highlightRing.SetActive(show);
            }
        }

        // ============================================
        // Block
        // ============================================

        /// <summary>
        /// Gain Block that absorbs damage.
        /// </summary>
        /// <param name="amount">Amount of Block to gain</param>
        public void GainBlock(int amount)
        {
            _block += amount;
            Debug.Log($"[EnemyInstance] {Name} gained {amount} Block. Total: {_block}");
        }

        /// <summary>
        /// Reset Block to zero (called at start of enemy turn).
        /// </summary>
        public void ResetBlock()
        {
            _block = 0;
        }

        // ============================================
        // Intent System
        // ============================================

        /// <summary>
        /// Get the current intent step for display.
        /// </summary>
        /// <returns>Current intent or null if no pattern</returns>
        public IntentStep GetCurrentIntent()
        {
            return _intentPattern?.GetCurrentIntent();
        }

        /// <summary>
        /// Advance to the next intent in the pattern.
        /// Called at end of enemy turn.
        /// </summary>
        public void AdvanceIntent()
        {
            _intentPattern?.AdvanceIntent();
        }

        /// <summary>
        /// Get the next intent (for preview) without advancing.
        /// </summary>
        /// <returns>Next intent step</returns>
        public IntentStep PeekNextIntent()
        {
            return _intentPattern?.PeekNextIntent();
        }

        // ============================================
        // Status Effects
        // ============================================

        /// <summary>
        /// Check if enemy has a status effect.
        /// </summary>
        /// <param name="type">Status type to check</param>
        /// <returns>True if status is present with at least 1 stack</returns>
        public bool HasStatus(StatusType type)
        {
            return _statusEffects.ContainsKey(type) && _statusEffects[type] > 0;
        }

        /// <summary>
        /// Get the number of stacks for a status effect.
        /// </summary>
        /// <param name="type">Status type to check</param>
        /// <returns>Number of stacks (0 if not present)</returns>
        public int GetStatusStacks(StatusType type)
        {
            return _statusEffects.TryGetValue(type, out int stacks) ? stacks : 0;
        }

        /// <summary>
        /// Apply stacks of a status effect.
        /// </summary>
        /// <param name="type">Status type to apply</param>
        /// <param name="stacks">Number of stacks to add</param>
        public void ApplyStatus(StatusType type, int stacks)
        {
            if (_statusEffects.ContainsKey(type))
            {
                _statusEffects[type] += stacks;
            }
            else
            {
                _statusEffects[type] = stacks;
            }

            Debug.Log($"[EnemyInstance] {Name} gained {stacks} {type}. Total: {_statusEffects[type]}");
        }

        /// <summary>
        /// Remove a status effect entirely.
        /// </summary>
        /// <param name="type">Status type to remove</param>
        public void RemoveStatus(StatusType type)
        {
            if (_statusEffects.Remove(type))
            {
                Debug.Log($"[EnemyInstance] {Name} lost {type} status");
            }
        }

        /// <summary>
        /// Reduce stacks of a status effect.
        /// </summary>
        /// <param name="type">Status type to reduce</param>
        /// <param name="amount">Amount to reduce</param>
        public void ReduceStatus(StatusType type, int amount)
        {
            if (_statusEffects.ContainsKey(type))
            {
                _statusEffects[type] = Mathf.Max(0, _statusEffects[type] - amount);
                if (_statusEffects[type] <= 0)
                {
                    _statusEffects.Remove(type);
                }
            }
        }

        // ============================================
        // Death
        // ============================================

        private void Die()
        {
            Debug.Log($"[EnemyInstance] {Name} defeated!");

            // Grant Soul Essence on kill
            if (ServiceLocator.TryGet<TurnManager>(out var turnManager) && turnManager.Context != null)
            {
                int seGain = 5;
                turnManager.Context.SoulEssence += seGain;
                EventBus.Publish(new SoulEssenceChangedEvent(turnManager.Context.SoulEssence, seGain));
            }

            // Publish defeat event
            EventBus.Publish(new EnemyDefeatedEvent(this));

            // Check for combat end - handled by phase logic listening to EnemyDefeatedEvent

            // Play death animation/effects
            // TODO: Add death animation before destroying
        }

        // ============================================
        // Scaled Damage
        // ============================================

        /// <summary>
        /// Get zone-scaled damage value.
        /// </summary>
        /// <param name="baseDamage">Optional base damage override</param>
        /// <returns>Scaled damage value</returns>
        public int GetScaledDamage(int? baseDamage = null)
        {
            int damage = baseDamage ?? _data?.BaseDamage ?? 0;
            float multiplier = 1f + (_zone - 1) * 0.1f;

            // Apply strength bonus
            if (HasStatus(StatusType.Strength))
            {
                damage += GetStatusStacks(StatusType.Strength);
            }

            return Mathf.RoundToInt(damage * multiplier);
        }

        // ============================================
        // Debug
        // ============================================

        /// <summary>
        /// Get debug info for display.
        /// </summary>
        public string GetDebugInfo()
        {
            var intent = GetCurrentIntent();
            string intentText = intent?.GetDisplayText() ?? "None";

            return $"{Name}\n" +
                   $"HP: {_currentHP}/{_maxHP}\n" +
                   $"Block: {_block}\n" +
                   $"Intent: {intentText}";
        }
    }
}
