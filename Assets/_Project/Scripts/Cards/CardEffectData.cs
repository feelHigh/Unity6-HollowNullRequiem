// ============================================
// CardEffectData.cs
// Serializable effect definition for card effects
// ============================================

using System;
using UnityEngine;

namespace HNR.Cards
{
    /// <summary>
    /// Serializable effect definition within a card.
    /// Multiple CardEffectData instances form a card's complete effect sequence.
    /// </summary>
    [Serializable]
    public class CardEffectData
    {
        // ============================================
        // Serialized Fields
        // ============================================

        [SerializeField, Tooltip("Type of effect to apply")]
        private EffectType _effectType;

        [SerializeField, Tooltip("Primary value (damage amount, heal amount, etc.)")]
        private int _value;

        [SerializeField, Tooltip("Duration in turns (for DoTs, buffs) or hit count")]
        private int _duration;

        [SerializeField, Tooltip("Custom data for special effects (JSON or key-value)")]
        private string _customData;

        // ============================================
        // Public Accessors
        // ============================================

        /// <summary>Type of effect to apply.</summary>
        public EffectType EffectType => _effectType;

        /// <summary>Primary value (damage, heal, stack count, etc.).</summary>
        public int Value => _value;

        /// <summary>Duration in turns or hit count for multi-hit effects.</summary>
        public int Duration => _duration;

        /// <summary>Custom data string for special effect parameters.</summary>
        public string CustomData => _customData;

        // ============================================
        // Constructors
        // ============================================

        /// <summary>
        /// Default constructor for serialization.
        /// </summary>
        public CardEffectData()
        {
            _effectType = EffectType.Damage;
            _value = 0;
            _duration = 0;
            _customData = "";
        }

        /// <summary>
        /// Create a new CardEffectData instance.
        /// </summary>
        /// <param name="type">Type of effect</param>
        /// <param name="value">Primary value</param>
        /// <param name="duration">Duration or hit count</param>
        /// <param name="customData">Custom parameters</param>
        public CardEffectData(EffectType type, int value, int duration = 0, string customData = "")
        {
            _effectType = type;
            _value = value;
            _duration = duration;
            _customData = customData ?? "";
        }

        // ============================================
        // Public Methods
        // ============================================

        /// <summary>
        /// Get formatted description for UI display.
        /// </summary>
        /// <returns>Human-readable effect description</returns>
        public string GetDescription()
        {
            return _effectType switch
            {
                // Damage
                EffectType.Damage => $"Deal {_value} damage",
                EffectType.DamageMultiple => $"Deal {_value} damage {_duration} times",

                // Defense
                EffectType.Block => $"Gain {_value} Block",

                // Healing
                EffectType.Heal => $"Heal {_value} HP",
                EffectType.HealPercent => $"Heal {_value}% HP",

                // Status Effects
                EffectType.ApplyBurn => $"Apply {_value} Burn for {_duration} turns",
                EffectType.ApplyPoison => $"Apply {_value} Poison for {_duration} turns",
                EffectType.ApplyWeakness => $"Apply Weakness for {_duration} turns",
                EffectType.ApplyVulnerability => $"Apply Vulnerable for {_duration} turns",
                EffectType.ApplyStun => $"Apply Stun for {_duration} turns",

                // Corruption
                EffectType.CorruptionGain => $"Gain {_value} Corruption",
                EffectType.CorruptionReduce => $"Reduce {_value} Corruption",

                // Card Manipulation
                EffectType.DrawCards => $"Draw {_value} cards",
                EffectType.DiscardRandom => $"Discard {_value} random cards",
                EffectType.Exhaust => "Exhaust",

                // Resources
                EffectType.GainAP => $"Gain {_value} AP",
                EffectType.GainSE => $"Gain {_value} Soul Energy",

                // Special
                EffectType.SummonEntity => $"Summon entity",
                EffectType.CopyCard => "Copy a card",
                EffectType.Custom => !string.IsNullOrEmpty(_customData) ? _customData : "Special effect",

                // Fallback
                _ => $"{_effectType}: {_value}"
            };
        }

        /// <summary>
        /// Create a copy of this effect data.
        /// </summary>
        /// <returns>New CardEffectData with same values</returns>
        public CardEffectData Clone()
        {
            return new CardEffectData(_effectType, _value, _duration, _customData);
        }
    }
}
