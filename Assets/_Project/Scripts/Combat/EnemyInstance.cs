// ============================================
// EnemyInstance.cs
// Runtime enemy instance implementing ICombatTarget
// ============================================

using System.Collections;
using UnityEngine;
using HNR.Core;
using HNR.Core.Events;
using HNR.Characters;
using HNR.Characters.Visuals;
using HNR.VFX;

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
        private ICharacterVisual _visual;

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

        /// <summary>Character visual component for animations.</summary>
        public ICharacterVisual Visual => _visual;

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

            // Set up basic visuals
            if (_sprite != null && data.Sprite != null)
            {
                _sprite.sprite = data.Sprite;
                _sprite.transform.localScale = Vector3.one * data.SpriteScale;
            }

            if (_highlightRing != null)
            {
                _highlightRing.SetActive(false);
            }

            // Initialize character visual (HeroEditor prefab)
            InitializeVisual();

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
                Debug.Log($"[EnemyInstance] {Name} highlight ring {(show ? "SHOWN" : "hidden")}");
            }
            else
            {
                Debug.LogWarning($"[EnemyInstance] {Name} has no highlight ring reference!");
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

            // Publish defeat event - SoulEssenceManager handles SE grant centrally
            EventBus.Publish(new EnemyDefeatedEvent(this));

            // Check for combat end - handled by phase logic listening to EnemyDefeatedEvent

            // Start death sequence with animation
            StartCoroutine(DeathSequence());
        }

        /// <summary>
        /// Death animation sequence - plays visual feedback before cleanup.
        /// </summary>
        private IEnumerator DeathSequence()
        {
            // Play death animation if visual exists
            _visual?.PlayDeath(true);

            // Spawn death VFX
            if (ServiceLocator.TryGet<VFXPoolManager>(out var vfxPool))
            {
                vfxPool.Spawn("vfx_corruption", transform.position, Quaternion.identity);
            }

            // Fade out sprite if available
            if (_sprite != null)
            {
                float fadeTime = 0.5f;
                float elapsed = 0f;
                Color startColor = _sprite.color;

                while (elapsed < fadeTime)
                {
                    elapsed += Time.deltaTime;
                    float alpha = Mathf.Lerp(1f, 0f, elapsed / fadeTime);
                    _sprite.color = new Color(startColor.r, startColor.g, startColor.b, alpha);
                    yield return null;
                }
            }
            else
            {
                // Wait for animation if no sprite fade
                yield return new WaitForSeconds(0.5f);
            }

            // Cleanup
            Destroy(gameObject);
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
        // Visual System
        // ============================================

        /// <summary>
        /// Initialize the character visual from the data prefab.
        /// </summary>
        private void InitializeVisual()
        {
            // Check if visual prefab is assigned
            if (_data?.VisualPrefab == null)
            {
                // Try to find existing ICharacterVisual component
                _visual = GetComponentInChildren<ICharacterVisual>();
                return;
            }

            // Destroy any existing visual
            if (_visual is MonoBehaviour existingVisual && existingVisual != null)
            {
                Destroy(existingVisual.gameObject);
            }

            // Instantiate the visual prefab
            var visualGO = Instantiate(_data.VisualPrefab, transform);
            visualGO.name = $"{Name}_Visual";
            visualGO.transform.localPosition = Vector3.zero;
            visualGO.transform.localScale = Vector3.one * _data.SpriteScale;

            // Get the ICharacterVisual component
            _visual = visualGO.GetComponent<ICharacterVisual>();

            if (_visual == null)
            {
                Debug.LogWarning($"[EnemyInstance] Visual prefab for {Name} has no ICharacterVisual component");
            }
            else
            {
                // Set facing (enemies face left by default)
                _visual.SetFacing(false);
            }
        }

        /// <summary>
        /// Set a pre-existing visual component (for testing or manual setup).
        /// </summary>
        public void SetVisual(ICharacterVisual visual)
        {
            _visual = visual;
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
