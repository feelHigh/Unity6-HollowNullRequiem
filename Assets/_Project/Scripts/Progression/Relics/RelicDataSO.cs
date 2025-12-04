// ============================================
// RelicDataSO.cs
// Relic data definition ScriptableObject
// ============================================

using UnityEngine;

namespace HNR.Progression
{
    // ============================================
    // RELIC ENUMS
    // ============================================

    /// <summary>
    /// Relic rarity tiers affecting shop prices and drop rates.
    /// </summary>
    public enum RelicRarity
    {
        /// <summary>Common relics, lower price (100-150 shards).</summary>
        Common,

        /// <summary>Uncommon relics, moderate price (150-200 shards).</summary>
        Uncommon,

        /// <summary>Rare relics, higher price (200-300 shards).</summary>
        Rare,

        /// <summary>Boss-exclusive relics, not sold in shops.</summary>
        Boss
    }

    /// <summary>
    /// When the relic effect triggers.
    /// </summary>
    public enum RelicTrigger
    {
        /// <summary>Always active, applies immediately on acquisition.</summary>
        Passive,

        /// <summary>Triggers at the start of each combat.</summary>
        OnCombatStart,

        /// <summary>Triggers at the end of each combat.</summary>
        OnCombatEnd,

        /// <summary>Triggers at the start of each player turn.</summary>
        OnTurnStart,

        /// <summary>Triggers when any card is played.</summary>
        OnCardPlay,

        /// <summary>Triggers when dealing damage to enemies.</summary>
        OnDamageDealt,

        /// <summary>Triggers when taking damage.</summary>
        OnDamageTaken,

        /// <summary>Triggers when killing an enemy.</summary>
        OnKill,

        /// <summary>Triggers when a Requiem enters Null State.</summary>
        OnNullStateEntered,

        /// <summary>Triggers when completing an Echo Event.</summary>
        OnEventComplete
    }

    /// <summary>
    /// Type of effect the relic provides.
    /// </summary>
    public enum RelicEffectType
    {
        /// <summary>Increase maximum HP.</summary>
        ModifyMaxHP,

        /// <summary>Modify damage dealt.</summary>
        ModifyDamage,

        /// <summary>Gain or modify block.</summary>
        ModifyBlock,

        /// <summary>Gain Soul Essence.</summary>
        GainSoulEssence,

        /// <summary>Reduce corruption on team.</summary>
        ReduceCorruption,

        /// <summary>Draw additional cards.</summary>
        DrawCard,

        /// <summary>Heal the team.</summary>
        Healing,

        /// <summary>Gain Void Shards.</summary>
        GainVoidShards,

        /// <summary>Gain additional AP.</summary>
        GainAP
    }

    // ============================================
    // RELIC DATA SCRIPTABLEOBJECT
    // ============================================

    /// <summary>
    /// Defines a relic's static data. Relics provide passive or triggered effects.
    /// </summary>
    [CreateAssetMenu(fileName = "New Relic", menuName = "HNR/Relic")]
    public class RelicDataSO : ScriptableObject
    {
        // ============================================
        // Identity
        // ============================================

        [Header("Identity")]
        [SerializeField, Tooltip("Unique identifier for save/load")]
        private string _relicId;

        [SerializeField, Tooltip("Display name shown in UI")]
        private string _relicName;

        [SerializeField, TextArea(2, 4), Tooltip("Effect description with {value} placeholder")]
        private string _description;

        [SerializeField, TextArea(1, 2), Tooltip("Lore/flavor text")]
        private string _flavorText;

        // ============================================
        // Classification
        // ============================================

        [Header("Classification")]
        [SerializeField, Tooltip("Rarity affects shop price and drop rate")]
        private RelicRarity _rarity;

        [SerializeField, Tooltip("When the relic effect triggers")]
        private RelicTrigger _trigger;

        // ============================================
        // Effect
        // ============================================

        [Header("Effect")]
        [SerializeField, Tooltip("Type of effect this relic provides")]
        private RelicEffectType _effectType;

        [SerializeField, Tooltip("Numeric value for the effect")]
        private int _effectValue;

        // ============================================
        // Visuals
        // ============================================

        [Header("Visuals")]
        [SerializeField, Tooltip("Relic icon (64x64 recommended)")]
        private Sprite _icon;

        // ============================================
        // Public Accessors
        // ============================================

        /// <summary>Unique identifier for save/load.</summary>
        public string RelicId => _relicId;

        /// <summary>Display name shown in UI.</summary>
        public string RelicName => _relicName;

        /// <summary>Effect description template.</summary>
        public string Description => _description;

        /// <summary>Lore/flavor text.</summary>
        public string FlavorText => _flavorText;

        /// <summary>Relic rarity tier.</summary>
        public RelicRarity Rarity => _rarity;

        /// <summary>When the effect triggers.</summary>
        public RelicTrigger Trigger => _trigger;

        /// <summary>Type of effect provided.</summary>
        public RelicEffectType EffectType => _effectType;

        /// <summary>Numeric value for the effect.</summary>
        public int EffectValue => _effectValue;

        /// <summary>Relic icon sprite.</summary>
        public Sprite Icon => _icon;

        // ============================================
        // Public Methods
        // ============================================

        /// <summary>
        /// Get formatted description with effect value substituted.
        /// Replaces {value} placeholder with actual effect value.
        /// </summary>
        /// <returns>Formatted description string.</returns>
        public string GetFormattedDescription()
        {
            if (string.IsNullOrEmpty(_description))
                return string.Empty;

            return _description.Replace("{value}", _effectValue.ToString());
        }

        /// <summary>
        /// Check if this relic is a passive (always-on) effect.
        /// </summary>
        public bool IsPassive => _trigger == RelicTrigger.Passive;

        // ============================================
        // Editor Helpers
        // ============================================

#if UNITY_EDITOR
        private void OnValidate()
        {
            // Auto-generate relic ID from asset name if empty
            if (string.IsNullOrEmpty(_relicId))
            {
                _relicId = name.ToLower().Replace(" ", "_");
            }
        }
#endif
    }
}
