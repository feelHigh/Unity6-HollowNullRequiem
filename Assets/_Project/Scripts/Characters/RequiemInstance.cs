// ============================================
// RequiemInstance.cs
// Runtime Requiem instance implementing ICombatTarget
// ============================================

using UnityEngine;
using HNR.Core;
using HNR.Core.Events;
using HNR.Combat;
using HNR.Cards;

namespace HNR.Characters
{
    /// <summary>
    /// Runtime instance of a Requiem character in combat.
    /// Implements ICombatTarget for targeting system integration.
    /// </summary>
    public class RequiemInstance : MonoBehaviour, ICombatTarget
    {
        // ============================================
        // Configuration
        // ============================================

        [Header("References")]
        [SerializeField, Tooltip("Requiem data asset")]
        private RequiemDataSO _data;

        [SerializeField, Tooltip("Sprite renderer for character visuals")]
        private SpriteRenderer _sprite;

        [SerializeField, Tooltip("Highlight ring for targeting feedback")]
        private GameObject _highlightRing;

        // ============================================
        // Runtime State
        // ============================================

        private int _currentHP;
        private int _maxHP;
        private int _block;
        private int _corruption;
        private int _soulEssence;
        private bool _inNullState;
        private bool _hasUsedArtThisCombat;

        // Null State Modifiers
        private float _burnDamageMultiplier = 1.0f;
        private float _lifestealMultiplier = 1.0f;
        private bool _healingDamagesEnemies = false;
        private int _nullStateBlockRegen = 0;
        private int _nullStateHealRegen = 0;

        // ============================================
        // Properties
        // ============================================

        /// <summary>Requiem data asset.</summary>
        public RequiemDataSO Data => _data;

        /// <summary>Display name of the Requiem.</summary>
        public string Name => _data?.RequiemName ?? "Unknown";

        /// <summary>World position for targeting and effects.</summary>
        public Vector3 Position => transform.position;

        /// <summary>True if this Requiem has been defeated.</summary>
        public bool IsDead => _currentHP <= 0;

        /// <summary>Current HP.</summary>
        public int CurrentHP => _currentHP;

        /// <summary>Maximum HP.</summary>
        public int MaxHP => _maxHP;

        /// <summary>Current Block value.</summary>
        public int Block => _block;

        /// <summary>Current Corruption value (0-100).</summary>
        public int Corruption => _corruption;

        /// <summary>Current Soul Essence.</summary>
        public int SoulEssence => _soulEssence;

        /// <summary>True if in Null State (100 corruption).</summary>
        public bool InNullState => _inNullState;

        /// <summary>Soul Aspect for effectiveness calculations.</summary>
        public SoulAspect SoulAspect => _data?.SoulAspect ?? SoulAspect.None;

        /// <summary>Combat class/role.</summary>
        public RequiemClass Class => _data?.Class ?? RequiemClass.Striker;

        /// <summary>Whether this Requiem has used their Art this combat.</summary>
        public bool HasUsedArtThisCombat
        {
            get => _hasUsedArtThisCombat;
            set => _hasUsedArtThisCombat = value;
        }

        // ============================================
        // Null State Modifier Properties
        // ============================================

        /// <summary>Multiplier for Burn damage (Kira Null State).</summary>
        public float BurnDamageMultiplier => _burnDamageMultiplier;

        /// <summary>Multiplier for lifesteal/drain effects (Mordren Null State).</summary>
        public float LifestealMultiplier => _lifestealMultiplier;

        /// <summary>Whether healing effects also damage enemies (Elara Null State).</summary>
        public bool HealingDamagesEnemies => _healingDamagesEnemies;

        /// <summary>Block gained at turn start in Null State (Thornwick).</summary>
        public int NullStateBlockRegen => _nullStateBlockRegen;

        /// <summary>HP healed at turn start in Null State (Thornwick).</summary>
        public int NullStateHealRegen => _nullStateHealRegen;

        // ============================================
        // Initialization
        // ============================================

        /// <summary>
        /// Initialize the Requiem instance with data.
        /// </summary>
        /// <param name="data">Requiem data asset (can be null for testing)</param>
        /// <param name="defaultHP">Default HP if data is null (for testing)</param>
        public void Initialize(RequiemDataSO data, int defaultHP = 100)
        {
            _data = data;
            _maxHP = data != null ? data.BaseHP : defaultHP;
            _currentHP = _maxHP;
            _block = 0;
            _corruption = 0;
            _soulEssence = 0;
            _inNullState = false;
            _hasUsedArtThisCombat = false;

            // Set up visuals
            if (_sprite != null && data?.Portrait != null)
            {
                _sprite.sprite = data.Portrait;
            }

            if (_highlightRing != null)
            {
                _highlightRing.SetActive(false);
            }

            Debug.Log($"[RequiemInstance] {Name} initialized: HP {_currentHP}/{_maxHP}");
        }

        // ============================================
        // ICombatTarget Implementation
        // ============================================

        /// <summary>
        /// Apply damage to this Requiem. Block absorbs damage first.
        /// </summary>
        /// <param name="amount">Raw damage amount before mitigation</param>
        public void TakeDamage(int amount)
        {
            if (IsDead) return;

            // Block absorbs damage first
            int blocked = Mathf.Min(amount, _block);
            _block -= blocked;
            int remaining = amount - blocked;

            if (remaining > 0)
            {
                _currentHP = Mathf.Max(0, _currentHP - remaining);
            }

            Debug.Log($"[RequiemInstance] {Name} took {remaining} damage (blocked {blocked}). HP: {_currentHP}/{_maxHP}");

            if (_currentHP <= 0)
            {
                Die();
            }
        }

        /// <summary>
        /// Heal this Requiem.
        /// </summary>
        /// <param name="amount">Amount to heal</param>
        public void Heal(int amount)
        {
            if (IsDead) return;

            // Apply corruption healing penalty (reduce healing at high corruption)
            if (_corruption >= 75)
            {
                amount = Mathf.RoundToInt(amount * 0.5f);
            }
            else if (_corruption >= 50)
            {
                amount = Mathf.RoundToInt(amount * 0.75f);
            }

            int previousHP = _currentHP;
            _currentHP = Mathf.Min(_currentHP + amount, _maxHP);
            int actualHeal = _currentHP - previousHP;

            if (actualHeal > 0)
            {
                Debug.Log($"[RequiemInstance] {Name} healed for {actualHeal}. HP: {_currentHP}/{_maxHP}");
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
            Debug.Log($"[RequiemInstance] {Name} gained {amount} Block. Total: {_block}");
        }

        /// <summary>
        /// Reset Block to zero (called at start of turn).
        /// </summary>
        public void ResetBlock()
        {
            _block = 0;
        }

        // ============================================
        // Corruption
        // ============================================

        /// <summary>
        /// Add corruption to this Requiem.
        /// </summary>
        /// <param name="amount">Amount of corruption to add</param>
        public void AddCorruption(int amount)
        {
            int previousCorruption = _corruption;
            _corruption = Mathf.Clamp(_corruption + amount, 0, 100);

            if (_corruption != previousCorruption)
            {
                EventBus.Publish(new CorruptionChangedEvent(this, previousCorruption, _corruption));
            }

            // Check for Null State entry
            if (_corruption >= 100 && !_inNullState)
            {
                EnterNullState();
            }
        }

        /// <summary>
        /// Remove corruption from this Requiem.
        /// </summary>
        /// <param name="amount">Amount of corruption to remove</param>
        public void RemoveCorruption(int amount)
        {
            int previousCorruption = _corruption;
            _corruption = Mathf.Clamp(_corruption - amount, 0, 100);

            if (_corruption != previousCorruption)
            {
                EventBus.Publish(new CorruptionChangedEvent(this, previousCorruption, _corruption));
            }

            // Check for Null State exit
            if (_corruption < 100 && _inNullState)
            {
                ExitNullState();
            }
        }

        private void EnterNullState()
        {
            _inNullState = true;
            EventBus.Publish(new NullStateEnteredEvent(this));
            Debug.Log($"[RequiemInstance] {Name} entered Null State!");

            // Apply Null State effect based on character
            // TODO: Implement specific Null State effects per Requiem
        }

        private void ExitNullState()
        {
            _inNullState = false;
            EventBus.Publish(new NullStateExitedEvent(this));
            Debug.Log($"[RequiemInstance] {Name} exited Null State");
        }

        // ============================================
        // Soul Essence
        // ============================================

        /// <summary>
        /// Add Soul Essence to this Requiem.
        /// </summary>
        /// <param name="amount">Base amount (will be multiplied by SERate)</param>
        public void AddSoulEssence(int amount)
        {
            float rate = _data?.SERate ?? 1f;
            int actual = Mathf.RoundToInt(amount * rate);
            _soulEssence += actual;
            Debug.Log($"[RequiemInstance] {Name} gained {actual} Soul Essence (rate: {rate}x). Total: {_soulEssence}");
        }

        /// <summary>
        /// Spend Soul Essence.
        /// </summary>
        /// <param name="amount">Amount to spend</param>
        /// <returns>True if had enough SE to spend</returns>
        public bool SpendSoulEssence(int amount)
        {
            if (_soulEssence < amount) return false;

            _soulEssence -= amount;
            return true;
        }

        // ============================================
        // Save/Load Restoration
        // ============================================

        /// <summary>
        /// Set HP values directly (for save/load restoration).
        /// </summary>
        public void SetHP(int currentHP, int maxHP)
        {
            _maxHP = maxHP;
            _currentHP = Mathf.Clamp(currentHP, 0, _maxHP);
        }

        /// <summary>
        /// Set corruption value directly (for save/load restoration).
        /// </summary>
        public void SetCorruption(int corruption)
        {
            _corruption = Mathf.Clamp(corruption, 0, 100);
            _inNullState = _corruption >= 100;
        }

        /// <summary>
        /// Set soul essence value directly (for save/load restoration).
        /// </summary>
        public void SetSoulEssence(int soulEssence)
        {
            _soulEssence = Mathf.Max(0, soulEssence);
        }

        // ============================================
        // Death
        // ============================================

        private void Die()
        {
            Debug.Log($"[RequiemInstance] {Name} has fallen!");
            // Note: In team-based HP system, individual Requiem death
            // may not end combat - handled by TurnManager
        }

        // ============================================
        // Null State Modifier Setters
        // ============================================

        /// <summary>
        /// Set Burn damage multiplier (Kira Null State).
        /// </summary>
        public void SetBurnDamageMultiplier(float multiplier)
        {
            _burnDamageMultiplier = multiplier;
        }

        /// <summary>
        /// Set lifesteal/drain multiplier (Mordren Null State).
        /// </summary>
        public void SetLifestealMultiplier(float multiplier)
        {
            _lifestealMultiplier = multiplier;
        }

        /// <summary>
        /// Set whether healing damages enemies (Elara Null State).
        /// </summary>
        public void SetHealingDamagesEnemies(bool active)
        {
            _healingDamagesEnemies = active;
        }

        /// <summary>
        /// Set Null State regeneration values (Thornwick Null State).
        /// </summary>
        public void SetNullStateRegen(int blockRegen, int healRegen)
        {
            _nullStateBlockRegen = blockRegen;
            _nullStateHealRegen = healRegen;
        }

        /// <summary>
        /// Reset all Null State modifiers to default values.
        /// </summary>
        public void ResetNullStateModifiers()
        {
            _burnDamageMultiplier = 1.0f;
            _lifestealMultiplier = 1.0f;
            _healingDamagesEnemies = false;
            _nullStateBlockRegen = 0;
            _nullStateHealRegen = 0;
        }

        /// <summary>
        /// Apply turn-start Null State regeneration effects.
        /// </summary>
        public void ApplyNullStateRegen()
        {
            if (!_inNullState) return;

            if (_nullStateBlockRegen > 0)
            {
                GainBlock(_nullStateBlockRegen);
                Debug.Log($"[RequiemInstance] {Name} Null State regen: +{_nullStateBlockRegen} Block");
            }

            if (_nullStateHealRegen > 0)
            {
                Heal(_nullStateHealRegen);
                Debug.Log($"[RequiemInstance] {Name} Null State regen: +{_nullStateHealRegen} HP");
            }
        }

        // ============================================
        // Debug
        // ============================================

        /// <summary>
        /// Get debug info for display.
        /// </summary>
        public string GetDebugInfo()
        {
            return $"{Name} ({Class})\n" +
                   $"HP: {_currentHP}/{_maxHP}\n" +
                   $"Block: {_block}\n" +
                   $"Corruption: {_corruption}/100\n" +
                   $"SE: {_soulEssence}";
        }
    }
}
