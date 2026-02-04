// ============================================
// StatusIconConfigSO.cs
// Configuration for status effect icon sprites
// ============================================

using System;
using UnityEngine;
using HNR.Characters;

namespace HNR.UI.Config
{
    /// <summary>
    /// ScriptableObject holding sprite references for all status effect types.
    /// Used by EnemyFloatingUI and StatusIconUI to display proper status icons.
    /// </summary>
    [CreateAssetMenu(fileName = "StatusIconConfig", menuName = "HNR/Config/Status Icon Config")]
    public class StatusIconConfigSO : ScriptableObject
    {
        // ============================================
        // Damage Over Time
        // ============================================

        [Header("Damage Over Time")]
        [SerializeField, Tooltip("Fire damage at end of turn")]
        private Sprite _burnIcon;

        [SerializeField, Tooltip("Poison damage at end of turn")]
        private Sprite _poisonIcon;

        // ============================================
        // Debuffs
        // ============================================

        [Header("Debuffs")]
        [SerializeField, Tooltip("Deal 25% less damage")]
        private Sprite _weaknessIcon;

        [SerializeField, Tooltip("Take 25% more damage")]
        private Sprite _vulnerabilityIcon;

        [SerializeField, Tooltip("Skip next action")]
        private Sprite _stunIcon;

        [SerializeField, Tooltip("Next card costs +1 AP")]
        private Sprite _dazedIcon;

        [SerializeField, Tooltip("Takes bonus damage from next attack")]
        private Sprite _markedIcon;

        // ============================================
        // Buffs
        // ============================================

        [Header("Buffs")]
        [SerializeField, Tooltip("Deal bonus damage per stack")]
        private Sprite _strengthIcon;

        [SerializeField, Tooltip("Gain bonus block per stack")]
        private Sprite _dexterityIcon;

        [SerializeField, Tooltip("Heal HP at turn start")]
        private Sprite _regenerationIcon;

        [SerializeField, Tooltip("Next card costs -1 AP")]
        private Sprite _energizedIcon;

        // ============================================
        // Special / Defense
        // ============================================

        [Header("Special")]
        [SerializeField, Tooltip("Immune to debuffs for duration")]
        private Sprite _shieldedIcon;

        [SerializeField, Tooltip("Thorns damage to attackers")]
        private Sprite _thornsIcon;

        [SerializeField, Tooltip("Draw extra cards at turn start")]
        private Sprite _drawBonusIcon;

        [SerializeField, Tooltip("Take 25% less damage")]
        private Sprite _protectedIcon;

        [SerializeField, Tooltip("Stacks build to trigger effects")]
        private Sprite _ritualIcon;

        // ============================================
        // Fallback
        // ============================================

        [Header("Fallback")]
        [SerializeField, Tooltip("Default icon when specific icon is not assigned")]
        private Sprite _defaultIcon;

        // ============================================
        // Public Properties
        // ============================================

        public Sprite BurnIcon => _burnIcon;
        public Sprite PoisonIcon => _poisonIcon;
        public Sprite WeaknessIcon => _weaknessIcon;
        public Sprite VulnerabilityIcon => _vulnerabilityIcon;
        public Sprite StunIcon => _stunIcon;
        public Sprite DazedIcon => _dazedIcon;
        public Sprite MarkedIcon => _markedIcon;
        public Sprite StrengthIcon => _strengthIcon;
        public Sprite DexterityIcon => _dexterityIcon;
        public Sprite RegenerationIcon => _regenerationIcon;
        public Sprite EnergizedIcon => _energizedIcon;
        public Sprite ShieldedIcon => _shieldedIcon;
        public Sprite ThornsIcon => _thornsIcon;
        public Sprite DrawBonusIcon => _drawBonusIcon;
        public Sprite ProtectedIcon => _protectedIcon;
        public Sprite RitualIcon => _ritualIcon;
        public Sprite DefaultIcon => _defaultIcon;

        // ============================================
        // Public Methods
        // ============================================

        /// <summary>
        /// Gets the sprite for a given status type.
        /// Returns default icon if specific icon is not assigned.
        /// </summary>
        public Sprite GetIcon(StatusType statusType)
        {
            var icon = statusType switch
            {
                StatusType.Burn => _burnIcon,
                StatusType.Poison => _poisonIcon,
                StatusType.Weakness => _weaknessIcon,
                StatusType.Vulnerability => _vulnerabilityIcon,
                StatusType.Stun => _stunIcon,
                StatusType.Dazed => _dazedIcon,
                StatusType.Marked => _markedIcon,
                StatusType.Strength => _strengthIcon,
                StatusType.Dexterity => _dexterityIcon,
                StatusType.Regeneration => _regenerationIcon,
                StatusType.Energized => _energizedIcon,
                StatusType.Shielded => _shieldedIcon,
                StatusType.Thorns => _thornsIcon,
                StatusType.DrawBonus => _drawBonusIcon,
                StatusType.Protected => _protectedIcon,
                StatusType.Ritual => _ritualIcon,
                _ => _defaultIcon
            };

            return icon != null ? icon : _defaultIcon;
        }

        /// <summary>
        /// Gets the tint color for a given status type.
        /// Used to tint the icon sprite for visual distinction.
        /// </summary>
        public Color GetTintColor(StatusType statusType)
        {
            return statusType switch
            {
                // Damage Over Time - warm colors
                StatusType.Burn => new Color(1f, 0.5f, 0.2f),           // Orange
                StatusType.Poison => new Color(0.4f, 0.8f, 0.2f),       // Green

                // Debuffs - red/purple tones
                StatusType.Weakness => new Color(0.8f, 0.4f, 0.4f),     // Muted Red
                StatusType.Vulnerability => new Color(1f, 0.8f, 0.2f),  // Yellow
                StatusType.Stun => new Color(0.6f, 0.8f, 1f),           // Light Blue
                StatusType.Dazed => new Color(0.7f, 0.5f, 0.8f),        // Purple
                StatusType.Marked => new Color(1f, 0.3f, 0.3f),         // Bright Red

                // Buffs - positive colors
                StatusType.Strength => new Color(1f, 0.4f, 0.2f),       // Red-Orange
                StatusType.Dexterity => new Color(0.4f, 0.8f, 1f),      // Cyan
                StatusType.Regeneration => new Color(0.3f, 0.9f, 0.4f), // Bright Green
                StatusType.Energized => new Color(1f, 0.9f, 0.3f),      // Gold

                // Special - varied
                StatusType.Shielded => new Color(0.9f, 0.8f, 0.3f),     // Gold
                StatusType.Thorns => new Color(0.6f, 0.4f, 0.2f),       // Brown
                StatusType.DrawBonus => new Color(0.5f, 0.7f, 1f),      // Light Blue
                StatusType.Protected => new Color(0.4f, 0.7f, 1f),      // Blue
                StatusType.Ritual => new Color(0.8f, 0.5f, 1f),         // Violet

                _ => Color.white
            };
        }

        /// <summary>
        /// Returns true if this is a debuff status type.
        /// </summary>
        public bool IsDebuff(StatusType statusType)
        {
            return statusType switch
            {
                StatusType.Burn => true,
                StatusType.Poison => true,
                StatusType.Weakness => true,
                StatusType.Vulnerability => true,
                StatusType.Stun => true,
                StatusType.Dazed => true,
                StatusType.Marked => true,
                _ => false
            };
        }

        /// <summary>
        /// Returns true if this is a buff status type.
        /// </summary>
        public bool IsBuff(StatusType statusType)
        {
            return statusType switch
            {
                StatusType.Strength => true,
                StatusType.Dexterity => true,
                StatusType.Regeneration => true,
                StatusType.Energized => true,
                StatusType.Shielded => true,
                StatusType.Protected => true,
                StatusType.DrawBonus => true,
                _ => false
            };
        }

        // ============================================
        // Validation
        // ============================================

        /// <summary>
        /// Returns true if all required icons are assigned.
        /// </summary>
        public bool IsValid()
        {
            return _burnIcon != null &&
                   _poisonIcon != null &&
                   _weaknessIcon != null &&
                   _vulnerabilityIcon != null &&
                   _stunIcon != null &&
                   _strengthIcon != null &&
                   _regenerationIcon != null &&
                   _shieldedIcon != null &&
                   _protectedIcon != null;
        }

        /// <summary>
        /// Returns count of assigned vs total icons.
        /// </summary>
        public (int assigned, int total) GetAssignmentStatus()
        {
            int total = 16;
            int assigned = 0;

            if (_burnIcon != null) assigned++;
            if (_poisonIcon != null) assigned++;
            if (_weaknessIcon != null) assigned++;
            if (_vulnerabilityIcon != null) assigned++;
            if (_stunIcon != null) assigned++;
            if (_dazedIcon != null) assigned++;
            if (_markedIcon != null) assigned++;
            if (_strengthIcon != null) assigned++;
            if (_dexterityIcon != null) assigned++;
            if (_regenerationIcon != null) assigned++;
            if (_energizedIcon != null) assigned++;
            if (_shieldedIcon != null) assigned++;
            if (_thornsIcon != null) assigned++;
            if (_drawBonusIcon != null) assigned++;
            if (_protectedIcon != null) assigned++;
            if (_ritualIcon != null) assigned++;

            return (assigned, total);
        }

        // ============================================
        // Static Helper
        // ============================================

        private static StatusIconConfigSO _instance;

        /// <summary>
        /// Gets the StatusIconConfig from Resources.
        /// </summary>
        public static StatusIconConfigSO Instance
        {
            get
            {
                if (_instance == null || !_instance)
                {
                    _instance = Resources.Load<StatusIconConfigSO>("Config/StatusIconConfig");
                    if (_instance == null)
                    {
                        Debug.LogWarning("[StatusIconConfigSO] Config not found in Resources/Config/StatusIconConfig");
                    }
                }
                return _instance;
            }
        }

        /// <summary>
        /// Clears the cached instance.
        /// </summary>
        public static void ClearCache()
        {
            _instance = null;
        }

        /// <summary>
        /// Reset static cache on domain reload.
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStaticState()
        {
            _instance = null;
        }
    }
}
