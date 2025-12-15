// ============================================
// StatBlock.cs
// Stat calculations with modifier support
// ============================================

using System;
using System.Collections.Generic;
using UnityEngine;

namespace HNR.Characters
{
    /// <summary>
    /// Manages a set of stats with modifier support (flat and percent).
    /// Used for buff/debuff systems and stat calculations.
    /// </summary>
    [Serializable]
    public class StatBlock
    {
        // ============================================
        // Base Stats
        // ============================================

        [SerializeField] private int _baseHP;
        [SerializeField] private int _baseATK;
        [SerializeField] private int _baseDEF;
        [SerializeField] private float _baseSERate = 1f;

        // ============================================
        // Modifiers
        // ============================================

        private readonly Dictionary<StatType, List<StatModifier>> _modifiers = new();

        // ============================================
        // Constructors
        // ============================================

        public StatBlock()
        {
            InitializeModifierDictionary();
        }

        public StatBlock(int hp, int atk, int def, float seRate = 1f)
        {
            _baseHP = hp;
            _baseATK = atk;
            _baseDEF = def;
            _baseSERate = seRate;
            InitializeModifierDictionary();
        }

        public StatBlock(RequiemDataSO data)
        {
            if (data != null)
            {
                _baseHP = data.BaseHP;
                _baseATK = data.BaseATK;
                _baseDEF = data.BaseDEF;
                _baseSERate = data.SERate;
            }
            InitializeModifierDictionary();
        }

        private void InitializeModifierDictionary()
        {
            foreach (StatType stat in Enum.GetValues(typeof(StatType)))
            {
                _modifiers[stat] = new List<StatModifier>();
            }
        }

        // ============================================
        // Base Stat Accessors
        // ============================================

        /// <summary>Base HP before modifiers.</summary>
        public int BaseHP
        {
            get => _baseHP;
            set => _baseHP = Mathf.Max(0, value);
        }

        /// <summary>Base ATK before modifiers.</summary>
        public int BaseATK
        {
            get => _baseATK;
            set => _baseATK = Mathf.Max(0, value);
        }

        /// <summary>Base DEF before modifiers.</summary>
        public int BaseDEF
        {
            get => _baseDEF;
            set => _baseDEF = Mathf.Max(0, value);
        }

        /// <summary>Base SE Rate before modifiers.</summary>
        public float BaseSERate
        {
            get => _baseSERate;
            set => _baseSERate = Mathf.Max(0f, value);
        }

        // ============================================
        // Calculated Stats (with modifiers)
        // ============================================

        /// <summary>Final HP after all modifiers.</summary>
        public int HP => CalculateStat(StatType.HP, _baseHP);

        /// <summary>Final ATK after all modifiers.</summary>
        public int ATK => CalculateStat(StatType.ATK, _baseATK);

        /// <summary>Final DEF after all modifiers.</summary>
        public int DEF => CalculateStat(StatType.DEF, _baseDEF);

        /// <summary>Final SE Rate after all modifiers.</summary>
        public float SERate => CalculateStatFloat(StatType.SERate, _baseSERate);

        // ============================================
        // Modifier Management
        // ============================================

        /// <summary>
        /// Add a modifier to a stat.
        /// </summary>
        /// <param name="modifier">Modifier to add.</param>
        public void AddModifier(StatModifier modifier)
        {
            if (modifier == null) return;

            if (_modifiers.TryGetValue(modifier.StatType, out var list))
            {
                list.Add(modifier);
                SortModifiers(list);
            }
        }

        /// <summary>
        /// Remove a specific modifier.
        /// </summary>
        /// <param name="modifier">Modifier to remove.</param>
        /// <returns>True if removed.</returns>
        public bool RemoveModifier(StatModifier modifier)
        {
            if (modifier == null) return false;

            if (_modifiers.TryGetValue(modifier.StatType, out var list))
            {
                return list.Remove(modifier);
            }
            return false;
        }

        /// <summary>
        /// Remove all modifiers from a specific source.
        /// </summary>
        /// <param name="source">Source object to remove modifiers from.</param>
        /// <returns>Number of modifiers removed.</returns>
        public int RemoveModifiersFromSource(object source)
        {
            int removed = 0;
            foreach (var list in _modifiers.Values)
            {
                removed += list.RemoveAll(m => m.Source == source);
            }
            return removed;
        }

        /// <summary>
        /// Remove all modifiers of a specific type.
        /// </summary>
        /// <param name="statType">Stat type to clear modifiers from.</param>
        public void ClearModifiers(StatType statType)
        {
            if (_modifiers.TryGetValue(statType, out var list))
            {
                list.Clear();
            }
        }

        /// <summary>
        /// Remove all modifiers from all stats.
        /// </summary>
        public void ClearAllModifiers()
        {
            foreach (var list in _modifiers.Values)
            {
                list.Clear();
            }
        }

        /// <summary>
        /// Get all modifiers for a stat type.
        /// </summary>
        /// <param name="statType">Stat type to query.</param>
        /// <returns>Read-only list of modifiers.</returns>
        public IReadOnlyList<StatModifier> GetModifiers(StatType statType)
        {
            if (_modifiers.TryGetValue(statType, out var list))
            {
                return list.AsReadOnly();
            }
            return new List<StatModifier>().AsReadOnly();
        }

        // ============================================
        // Stat Calculation
        // ============================================

        private int CalculateStat(StatType statType, int baseValue)
        {
            float finalValue = baseValue;

            if (!_modifiers.TryGetValue(statType, out var modifiers))
            {
                return baseValue;
            }

            float flatSum = 0f;
            float percentAddSum = 0f;
            float percentMultProduct = 1f;

            foreach (var mod in modifiers)
            {
                switch (mod.ModType)
                {
                    case ModifierType.Flat:
                        flatSum += mod.Value;
                        break;
                    case ModifierType.PercentAdd:
                        percentAddSum += mod.Value;
                        break;
                    case ModifierType.PercentMult:
                        percentMultProduct *= (1f + mod.Value);
                        break;
                }
            }

            // Order: Base + Flat, then * (1 + PercentAdd), then * PercentMult
            finalValue = (baseValue + flatSum) * (1f + percentAddSum) * percentMultProduct;

            return Mathf.RoundToInt(Mathf.Max(0, finalValue));
        }

        private float CalculateStatFloat(StatType statType, float baseValue)
        {
            float finalValue = baseValue;

            if (!_modifiers.TryGetValue(statType, out var modifiers))
            {
                return baseValue;
            }

            float flatSum = 0f;
            float percentAddSum = 0f;
            float percentMultProduct = 1f;

            foreach (var mod in modifiers)
            {
                switch (mod.ModType)
                {
                    case ModifierType.Flat:
                        flatSum += mod.Value;
                        break;
                    case ModifierType.PercentAdd:
                        percentAddSum += mod.Value;
                        break;
                    case ModifierType.PercentMult:
                        percentMultProduct *= (1f + mod.Value);
                        break;
                }
            }

            finalValue = (baseValue + flatSum) * (1f + percentAddSum) * percentMultProduct;

            return Mathf.Max(0f, finalValue);
        }

        private void SortModifiers(List<StatModifier> list)
        {
            // Sort by order: Flat first, then PercentAdd, then PercentMult
            list.Sort((a, b) => a.ModType.CompareTo(b.ModType));
        }

        // ============================================
        // Utility
        // ============================================

        /// <summary>
        /// Create a copy of this stat block.
        /// </summary>
        /// <returns>New StatBlock with same base values (no modifiers).</returns>
        public StatBlock Clone()
        {
            return new StatBlock(_baseHP, _baseATK, _baseDEF, _baseSERate);
        }

        /// <summary>
        /// Get a debug string of all stats and modifiers.
        /// </summary>
        public override string ToString()
        {
            return $"StatBlock[HP:{BaseHP}({HP}), ATK:{BaseATK}({ATK}), DEF:{BaseDEF}({DEF}), SERate:{BaseSERate}({SERate})]";
        }
    }

    // ============================================
    // Supporting Types
    // ============================================

    /// <summary>
    /// Types of stats that can be modified.
    /// </summary>
    public enum StatType
    {
        HP,
        ATK,
        DEF,
        SERate,
        Block,
        Damage,
        Healing,
        CritChance,
        CritDamage
    }

    /// <summary>
    /// Types of stat modifiers.
    /// </summary>
    public enum ModifierType
    {
        /// <summary>Added directly to base value.</summary>
        Flat = 0,
        /// <summary>Percentage added together then applied.</summary>
        PercentAdd = 1,
        /// <summary>Percentage multiplied together then applied.</summary>
        PercentMult = 2
    }

    /// <summary>
    /// A modifier that affects a stat value.
    /// </summary>
    [Serializable]
    public class StatModifier
    {
        /// <summary>Which stat this affects.</summary>
        public StatType StatType { get; }

        /// <summary>Type of modification (flat, percent).</summary>
        public ModifierType ModType { get; }

        /// <summary>Value of the modification.</summary>
        public float Value { get; }

        /// <summary>Source of this modifier (for removal tracking).</summary>
        public object Source { get; }

        /// <summary>Optional duration in turns (-1 for permanent).</summary>
        public int Duration { get; set; }

        public StatModifier(StatType statType, ModifierType modType, float value, object source = null, int duration = -1)
        {
            StatType = statType;
            ModType = modType;
            Value = value;
            Source = source;
            Duration = duration;
        }

        /// <summary>
        /// Create a flat modifier.
        /// </summary>
        public static StatModifier Flat(StatType stat, float value, object source = null, int duration = -1)
        {
            return new StatModifier(stat, ModifierType.Flat, value, source, duration);
        }

        /// <summary>
        /// Create an additive percent modifier.
        /// </summary>
        public static StatModifier PercentAdd(StatType stat, float value, object source = null, int duration = -1)
        {
            return new StatModifier(stat, ModifierType.PercentAdd, value, source, duration);
        }

        /// <summary>
        /// Create a multiplicative percent modifier.
        /// </summary>
        public static StatModifier PercentMult(StatType stat, float value, object source = null, int duration = -1)
        {
            return new StatModifier(stat, ModifierType.PercentMult, value, source, duration);
        }

        public override string ToString()
        {
            string sign = Value >= 0 ? "+" : "";
            string valueStr = ModType == ModifierType.Flat ? $"{sign}{Value}" : $"{sign}{Value * 100}%";
            return $"{StatType} {valueStr} ({ModType})";
        }
    }
}
