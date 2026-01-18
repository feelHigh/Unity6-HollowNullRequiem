// ============================================
// CombatConfigSO.cs
// Centralized combat configuration settings
// ============================================

using UnityEngine;

namespace HNR.Combat
{
    /// <summary>
    /// Centralized configuration for combat mechanics.
    /// Controls damage, corruption, and other combat-related settings.
    /// </summary>
    [CreateAssetMenu(fileName = "CombatConfig", menuName = "HNR/Config/Combat Config")]
    public class CombatConfigSO : ScriptableObject
    {
        // ============================================
        // Corruption Settings
        // ============================================

        [Header("Damage-Based Corruption")]
        [SerializeField, Tooltip("Enable corruption gain when taking unblocked damage")]
        private bool _enableDamageCorruption = true;

        [SerializeField, Range(1, 100), Tooltip("Damage required per 1 corruption (e.g., 5 = 1 corruption per 5 damage)")]
        private int _damagePerCorruption = 5;

        [SerializeField, Range(0, 100), Tooltip("Minimum corruption gained per hit (even if damage is low)")]
        private int _minimumCorruptionPerHit = 1;

        [SerializeField, Range(0, 100), Tooltip("Maximum corruption that can be gained from a single hit (0 = unlimited)")]
        private int _maximumCorruptionPerHit = 0;

        // ============================================
        // Properties
        // ============================================

        /// <summary>Whether damage-based corruption is enabled.</summary>
        public bool EnableDamageCorruption => _enableDamageCorruption;

        /// <summary>Damage required to gain 1 corruption point.</summary>
        public int DamagePerCorruption => _damagePerCorruption;

        /// <summary>Minimum corruption gained per hit.</summary>
        public int MinimumCorruptionPerHit => _minimumCorruptionPerHit;

        /// <summary>Maximum corruption per hit (0 = unlimited).</summary>
        public int MaximumCorruptionPerHit => _maximumCorruptionPerHit;

        // ============================================
        // Calculation Methods
        // ============================================

        /// <summary>
        /// Calculate corruption gain from unblocked damage.
        /// </summary>
        /// <param name="unblockedDamage">Amount of damage that bypassed block</param>
        /// <returns>Corruption amount to apply</returns>
        public int CalculateCorruptionFromDamage(int unblockedDamage)
        {
            if (!_enableDamageCorruption || unblockedDamage <= 0)
                return 0;

            int corruption = unblockedDamage / _damagePerCorruption;
            corruption = Mathf.Max(_minimumCorruptionPerHit, corruption);

            if (_maximumCorruptionPerHit > 0)
            {
                corruption = Mathf.Min(_maximumCorruptionPerHit, corruption);
            }

            return corruption;
        }
    }
}
