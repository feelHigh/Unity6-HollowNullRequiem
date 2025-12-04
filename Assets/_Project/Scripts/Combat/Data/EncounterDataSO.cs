// ============================================
// EncounterDataSO.cs
// ScriptableObject defining encounter configurations
// ============================================

using System.Collections.Generic;
using UnityEngine;

namespace HNR.Combat
{
    /// <summary>
    /// Defines an encounter configuration with enemy pool and spawn rules.
    /// Create assets for each encounter type: Easy, Medium, Hard, Elite, Boss.
    /// </summary>
    [CreateAssetMenu(fileName = "New Encounter", menuName = "HNR/Encounter Data")]
    public class EncounterDataSO : ScriptableObject
    {
        // ============================================
        // Identity
        // ============================================

        [Header("Identity")]
        [SerializeField, Tooltip("Unique identifier for this encounter")]
        private string _encounterId;

        [SerializeField, Tooltip("Display name for UI")]
        private string _encounterName;

        // ============================================
        // Classification
        // ============================================

        [Header("Classification")]
        [SerializeField, Tooltip("Zone this encounter appears in (1-3)")]
        private int _zone = 1;

        [SerializeField, Tooltip("Encounter difficulty tier")]
        private EncounterDifficulty _difficulty = EncounterDifficulty.Normal;

        [SerializeField, Tooltip("Elite encounter with special rewards")]
        private bool _isElite;

        [SerializeField, Tooltip("Boss encounter ending the zone")]
        private bool _isBoss;

        // ============================================
        // Enemy Configuration
        // ============================================

        [Header("Enemy Configuration")]
        [SerializeField, Tooltip("Pool of enemies that can appear")]
        private List<EnemyDataSO> _enemyPool = new();

        [SerializeField, Range(1, 3), Tooltip("Minimum enemies to spawn")]
        private int _minEnemies = 1;

        [SerializeField, Range(1, 3), Tooltip("Maximum enemies to spawn")]
        private int _maxEnemies = 2;

        [SerializeField, Tooltip("Specific formation (overrides random if set)")]
        private List<EnemyDataSO> _fixedFormation = new();

        // ============================================
        // Rewards
        // ============================================

        [Header("Rewards")]
        [SerializeField, Tooltip("Base Void Shard reward multiplier")]
        private float _rewardMultiplier = 1f;

        [SerializeField, Tooltip("Guaranteed card rarity (None = random)")]
        private CardRewardTier _guaranteedCardTier = CardRewardTier.None;

        // ============================================
        // Arena Effects
        // ============================================

        [Header("Arena Effects")]
        [SerializeField, Tooltip("Corruption applied to team each turn")]
        private int _arenaCorruptionPerTurn = 0;

        [SerializeField, TextArea(2, 4), Tooltip("Special rules description")]
        private string _arenaDescription;

        // ============================================
        // Public Accessors
        // ============================================

        /// <summary>Unique encounter identifier.</summary>
        public string EncounterId => _encounterId;

        /// <summary>Display name.</summary>
        public string EncounterName => _encounterName;

        /// <summary>Zone number (1-3).</summary>
        public int Zone => _zone;

        /// <summary>Difficulty tier.</summary>
        public EncounterDifficulty Difficulty => _difficulty;

        /// <summary>Whether this is an Elite encounter.</summary>
        public bool IsElite => _isElite;

        /// <summary>Whether this is a Boss encounter.</summary>
        public bool IsBoss => _isBoss;

        /// <summary>Pool of possible enemies.</summary>
        public IReadOnlyList<EnemyDataSO> EnemyPool => _enemyPool;

        /// <summary>Minimum enemy count.</summary>
        public int MinEnemies => _minEnemies;

        /// <summary>Maximum enemy count.</summary>
        public int MaxEnemies => _maxEnemies;

        /// <summary>Fixed formation (if any).</summary>
        public IReadOnlyList<EnemyDataSO> FixedFormation => _fixedFormation;

        /// <summary>Whether this encounter uses a fixed formation.</summary>
        public bool HasFixedFormation => _fixedFormation != null && _fixedFormation.Count > 0;

        /// <summary>Reward multiplier.</summary>
        public float RewardMultiplier => _rewardMultiplier;

        /// <summary>Guaranteed card tier.</summary>
        public CardRewardTier GuaranteedCardTier => _guaranteedCardTier;

        /// <summary>Arena corruption per turn.</summary>
        public int ArenaCorruptionPerTurn => _arenaCorruptionPerTurn;

        /// <summary>Arena effect description.</summary>
        public string ArenaDescription => _arenaDescription;

        // ============================================
        // Editor
        // ============================================

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (string.IsNullOrEmpty(_encounterId))
            {
                _encounterId = name.ToLower().Replace(" ", "_");
            }

            _minEnemies = Mathf.Clamp(_minEnemies, 1, 3);
            _maxEnemies = Mathf.Clamp(_maxEnemies, _minEnemies, 3);
        }
#endif
    }

    /// <summary>
    /// Encounter difficulty tiers.
    /// </summary>
    public enum EncounterDifficulty
    {
        Easy,
        Normal,
        Hard,
        Elite,
        Boss
    }

    /// <summary>
    /// Card reward tier for guaranteed drops.
    /// </summary>
    public enum CardRewardTier
    {
        None,
        Common,
        Uncommon,
        Rare,
        Legendary
    }
}
