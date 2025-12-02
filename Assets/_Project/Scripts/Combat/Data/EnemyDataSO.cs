// ============================================
// EnemyDataSO.cs
// ScriptableObject defining enemy static data
// ============================================

using System.Collections.Generic;
using UnityEngine;
using HNR.Cards;

namespace HNR.Combat
{
    /// <summary>
    /// Defines an enemy type's static data.
    /// Create one asset per enemy type.
    /// </summary>
    /// <remarks>
    /// Enemy types per GDD:
    /// - Basic (8): Hollow Thrall, Corruption Sprite, Void Hound, etc.
    /// - Elite (2): Hollow Berserker, Null Weaver
    /// - Boss (1): Malchor, the Hollowed Saint
    /// </remarks>
    [CreateAssetMenu(fileName = "New Enemy", menuName = "HNR/Enemy Data")]
    public class EnemyDataSO : ScriptableObject
    {
        // ============================================
        // Identity
        // ============================================

        [Header("Identity")]
        [SerializeField, Tooltip("Unique identifier for save/load")]
        private string _enemyId;

        [SerializeField, Tooltip("Display name")]
        private string _enemyName;

        [SerializeField, TextArea(2, 4), Tooltip("Lore description")]
        private string _description;

        // ============================================
        // Classification
        // ============================================

        [Header("Classification")]
        [SerializeField, Tooltip("Elemental affinity (affects damage calculations)")]
        private SoulAspect _soulAspect;

        [SerializeField, Tooltip("Elite enemies have special abilities and better rewards")]
        private bool _isElite;

        [SerializeField, Tooltip("Boss enemies end zones and have phases")]
        private bool _isBoss;

        // ============================================
        // Stats
        // ============================================

        [Header("Stats")]
        [SerializeField, Range(10, 500), Tooltip("Base HP (scaled by zone). Basic:18-50, Elite:80-100, Boss:250+")]
        private int _baseHP = 30;

        [SerializeField, Range(0, 50), Tooltip("Base damage (scaled by zone)")]
        private int _baseDamage = 8;

        [SerializeField, Range(0, 30), Tooltip("Base block per defend action")]
        private int _baseBlock = 5;

        // ============================================
        // Corruption
        // ============================================

        [Header("Corruption")]
        [SerializeField, Range(0, 20), Tooltip("Corruption applied on hit. Low:2, Medium:4-5, High:8-10")]
        private int _corruptionOnHit = 2;

        // ============================================
        // Behavior
        // ============================================

        [Header("Behavior")]
        [SerializeField, Tooltip("Sequence of intents the enemy follows")]
        private IntentPattern _intentPattern;

        // ============================================
        // Rewards
        // ============================================

        [Header("Rewards")]
        [SerializeField, Range(0, 200), Tooltip("Void Shards dropped on defeat")]
        private int _voidShardReward = 10;

        [SerializeField, Range(0f, 1f), Tooltip("Chance to drop a card (0.0-1.0)")]
        private float _cardDropChance = 0.3f;

        [SerializeField, Tooltip("Specific cards this enemy can drop (random selection)")]
        private List<CardDataSO> _cardDropPool = new();

        // ============================================
        // Visuals
        // ============================================

        [Header("Visuals")]
        [SerializeField, Tooltip("Combat sprite (256x256)")]
        private Sprite _sprite;

        [SerializeField, Tooltip("Animator controller for combat animations")]
        private RuntimeAnimatorController _animator;

        [SerializeField, Tooltip("Scale multiplier for sprite")]
        private float _spriteScale = 1f;

        // ============================================
        // Audio
        // ============================================

        [Header("Audio")]
        [SerializeField, Tooltip("Sound when attacking")]
        private AudioClip _attackSound;

        [SerializeField, Tooltip("Sound when taking damage")]
        private AudioClip _hitSound;

        [SerializeField, Tooltip("Sound on death")]
        private AudioClip _deathSound;

        // ============================================
        // Public Accessors
        // ============================================

        /// <summary>Unique identifier.</summary>
        public string EnemyId => _enemyId;

        /// <summary>Display name.</summary>
        public string EnemyName => _enemyName;

        /// <summary>Lore description.</summary>
        public string Description => _description;

        /// <summary>Elemental affinity.</summary>
        public SoulAspect SoulAspect => _soulAspect;

        /// <summary>Whether this is an elite enemy.</summary>
        public bool IsElite => _isElite;

        /// <summary>Whether this is a boss enemy.</summary>
        public bool IsBoss => _isBoss;

        /// <summary>Base HP (before zone scaling).</summary>
        public int BaseHP => _baseHP;

        /// <summary>Base damage (before zone scaling).</summary>
        public int BaseDamage => _baseDamage;

        /// <summary>Base block amount.</summary>
        public int BaseBlock => _baseBlock;

        /// <summary>Corruption applied on hit.</summary>
        public int CorruptionOnHit => _corruptionOnHit;

        /// <summary>Intent pattern definition.</summary>
        public IntentPattern IntentPattern => _intentPattern;

        /// <summary>Void Shards dropped on defeat.</summary>
        public int VoidShardReward => _voidShardReward;

        /// <summary>Card drop chance (0.0-1.0).</summary>
        public float CardDropChance => _cardDropChance;

        /// <summary>Pool of cards this enemy can drop.</summary>
        public IReadOnlyList<CardDataSO> CardDropPool => _cardDropPool;

        /// <summary>Combat sprite.</summary>
        public Sprite Sprite => _sprite;

        /// <summary>Animation controller.</summary>
        public RuntimeAnimatorController Animator => _animator;

        /// <summary>Sprite scale multiplier.</summary>
        public float SpriteScale => _spriteScale;

        /// <summary>Attack sound effect.</summary>
        public AudioClip AttackSound => _attackSound;

        /// <summary>Hit reaction sound.</summary>
        public AudioClip HitSound => _hitSound;

        /// <summary>Death sound effect.</summary>
        public AudioClip DeathSound => _deathSound;

        // ============================================
        // Zone Scaling Methods
        // ============================================

        /// <summary>
        /// Get HP scaled by zone (+15% per zone after first).
        /// </summary>
        /// <param name="zone">Current zone (1-3)</param>
        /// <returns>Scaled HP value</returns>
        public int GetScaledHP(int zone)
        {
            float multiplier = 1f + (zone - 1) * 0.15f;
            return Mathf.RoundToInt(_baseHP * multiplier);
        }

        /// <summary>
        /// Get damage scaled by zone (+10% per zone after first).
        /// </summary>
        /// <param name="zone">Current zone (1-3)</param>
        /// <returns>Scaled damage value</returns>
        public int GetScaledDamage(int zone)
        {
            float multiplier = 1f + (zone - 1) * 0.1f;
            return Mathf.RoundToInt(_baseDamage * multiplier);
        }

        /// <summary>
        /// Get Void Shard reward scaled by zone (+25% per zone after first).
        /// </summary>
        /// <param name="zone">Current zone (1-3)</param>
        /// <returns>Scaled reward value</returns>
        public int GetScaledReward(int zone)
        {
            float multiplier = 1f + (zone - 1) * 0.25f;
            return Mathf.RoundToInt(_voidShardReward * multiplier);
        }

        // ============================================
        // Editor Helpers
        // ============================================

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (string.IsNullOrEmpty(_enemyId))
            {
                _enemyId = name.ToLower().Replace(" ", "_");
            }
        }
#endif
    }
}
