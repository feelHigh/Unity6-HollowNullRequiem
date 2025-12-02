// ============================================
// CardDataSO.cs
// ScriptableObject defining card static data
// ============================================

using System.Collections.Generic;
using UnityEngine;
using HNR.Characters;

namespace HNR.Cards
{
    /// <summary>
    /// Defines a card's static data. Instances created at design time.
    /// Use [CreateAssetMenu] to create new cards in Unity Editor.
    /// </summary>
    [CreateAssetMenu(fileName = "New Card", menuName = "HNR/Card Data")]
    public class CardDataSO : ScriptableObject
    {
        // ============================================
        // Identity
        // ============================================

        [Header("Identity")]
        [SerializeField, Tooltip("Unique identifier for save/load")]
        private string _cardId;

        [SerializeField, Tooltip("Display name shown on card")]
        private string _cardName;

        [SerializeField, TextArea(2, 4), Tooltip("Effect description with placeholders like [Damage]")]
        private string _description;

        [SerializeField, TextArea(1, 2), Tooltip("Lore/flavor text in italics")]
        private string _flavorText;

        // ============================================
        // Classification
        // ============================================

        [Header("Classification")]
        [SerializeField, Tooltip("Card type affects frame color")]
        private CardType _cardType;

        [SerializeField, Tooltip("Rarity affects drop rates and shop prices")]
        private CardRarity _rarity;

        [SerializeField, Tooltip("Soul Aspect for effectiveness calculations")]
        private SoulAspect _soulAspect;

        [SerializeField, Tooltip("Requiem who owns this card (null for neutral cards)")]
        private RequiemDataSO _owner;

        // ============================================
        // Cost
        // ============================================

        [Header("Cost")]
        [SerializeField, Range(0, 3), Tooltip("Action Point cost to play")]
        private int _apCost = 1;

        // ============================================
        // Targeting
        // ============================================

        [Header("Targeting")]
        [SerializeField, Tooltip("How targets are selected")]
        private TargetType _targetType;

        [SerializeField, Range(1, 4), Tooltip("Number of targets (for multi-target cards)")]
        private int _targetCount = 1;

        // ============================================
        // Effects
        // ============================================

        [Header("Effects")]
        [SerializeField, Tooltip("List of effects applied when card is played")]
        private List<CardEffectData> _effects = new();

        // ============================================
        // Upgrade
        // ============================================

        [Header("Upgrade")]
        [SerializeField, Tooltip("Reference to upgraded version of this card")]
        private CardDataSO _upgradedVersion;

        // ============================================
        // Visuals
        // ============================================

        [Header("Visuals")]
        [SerializeField, Tooltip("Card artwork (400x560 recommended)")]
        private Sprite _cardArt;

        [SerializeField, Tooltip("Border/frame tint color")]
        private Color _borderColor = Color.white;

        // ============================================
        // Public Accessors
        // ============================================

        /// <summary>Unique identifier for save/load.</summary>
        public string CardId => _cardId;

        /// <summary>Display name shown on card.</summary>
        public string CardName => _cardName;

        /// <summary>Effect description template.</summary>
        public string Description => _description;

        /// <summary>Lore/flavor text.</summary>
        public string FlavorText => _flavorText;

        /// <summary>Card type (Strike, Guard, Skill, Power).</summary>
        public CardType CardType => _cardType;

        /// <summary>Card rarity tier.</summary>
        public CardRarity Rarity => _rarity;

        /// <summary>Soul Aspect for effectiveness calculations.</summary>
        public SoulAspect SoulAspect => _soulAspect;

        /// <summary>Requiem who owns this card (null for neutral).</summary>
        public RequiemDataSO Owner => _owner;

        /// <summary>Action Point cost to play.</summary>
        public int APCost => _apCost;

        /// <summary>Targeting mode.</summary>
        public TargetType TargetType => _targetType;

        /// <summary>Number of targets for multi-target cards.</summary>
        public int TargetCount => _targetCount;

        /// <summary>List of effects applied when played.</summary>
        public IReadOnlyList<CardEffectData> Effects => _effects;

        /// <summary>Reference to upgraded version (null if this is upgraded).</summary>
        public CardDataSO UpgradedVersion => _upgradedVersion;

        /// <summary>Card artwork sprite.</summary>
        public Sprite CardArt => _cardArt;

        /// <summary>Border/frame tint color.</summary>
        public Color BorderColor => _borderColor;

        /// <summary>True if this card has no further upgrades.</summary>
        public bool IsUpgraded => _upgradedVersion == null;

        // ============================================
        // Public Methods
        // ============================================

        /// <summary>
        /// Create runtime description with actual effect values.
        /// Replaces placeholders like [Damage] with numeric values.
        /// </summary>
        /// <returns>Formatted description string</returns>
        public string GetFormattedDescription()
        {
            var desc = _description;
            foreach (var effect in _effects)
            {
                desc = desc.Replace($"[{effect.EffectType}]", effect.Value.ToString());
            }
            return desc;
        }

        /// <summary>
        /// Calculate total damage from all damage effects.
        /// </summary>
        /// <returns>Sum of all damage values</returns>
        public int GetTotalDamage()
        {
            int total = 0;
            foreach (var effect in _effects)
            {
                if (effect.EffectType == EffectType.Damage)
                    total += effect.Value;
                else if (effect.EffectType == EffectType.DamageMultiple)
                    total += effect.Value * effect.Duration;
            }
            return total;
        }

        /// <summary>
        /// Calculate total block from all block effects.
        /// </summary>
        /// <returns>Sum of all block values</returns>
        public int GetTotalBlock()
        {
            int total = 0;
            foreach (var effect in _effects)
            {
                if (effect.EffectType == EffectType.Block)
                    total += effect.Value;
            }
            return total;
        }

        /// <summary>
        /// Check if card has a specific effect type.
        /// </summary>
        /// <param name="effectType">Effect type to search for</param>
        /// <returns>True if card contains the effect</returns>
        public bool HasEffect(EffectType effectType)
        {
            foreach (var effect in _effects)
            {
                if (effect.EffectType == effectType)
                    return true;
            }
            return false;
        }

        // ============================================
        // Editor Helpers
        // ============================================

#if UNITY_EDITOR
        private void OnValidate()
        {
            // Auto-generate card ID from asset name if empty
            if (string.IsNullOrEmpty(_cardId))
            {
                _cardId = name.ToLower().Replace(" ", "_");
            }
        }
#endif
    }
}
