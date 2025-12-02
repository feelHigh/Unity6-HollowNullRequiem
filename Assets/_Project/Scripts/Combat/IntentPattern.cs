// ============================================
// IntentPattern.cs
// Enemy intent pattern and step definitions
// ============================================

using System;
using System.Collections.Generic;
using UnityEngine;
using HNR.Characters;

namespace HNR.Combat
{
    /// <summary>
    /// Type of action an enemy intends to take.
    /// Displayed as icons above enemies during combat.
    /// </summary>
    public enum IntentType
    {
        /// <summary>Deal damage to a single target.</summary>
        Attack,

        /// <summary>Deal damage multiple times (Value x SecondaryValue hits).</summary>
        AttackMultiple,

        /// <summary>Deal damage to all Requiems.</summary>
        AttackAll,

        /// <summary>Gain Block.</summary>
        Defend,

        /// <summary>Buff self (Strength, etc.).</summary>
        Buff,

        /// <summary>Debuff player team (Weakness, Vulnerability).</summary>
        Debuff,

        /// <summary>Apply Corruption directly.</summary>
        Corrupt,

        /// <summary>Heal self or allies.</summary>
        Heal,

        /// <summary>Spawn additional enemies.</summary>
        Summon,

        /// <summary>Become untargetable (Void Stalker).</summary>
        Stealth,

        /// <summary>Boss/Elite special move.</summary>
        Special,

        /// <summary>Hidden intent (not revealed to player).</summary>
        Unknown
    }

    /// <summary>
    /// Single step in an enemy's intent pattern.
    /// </summary>
    [Serializable]
    public class IntentStep
    {
        // ============================================
        // Fields
        // ============================================

        [SerializeField, Tooltip("Type of intent action")]
        private IntentType _intentType;

        [SerializeField, Tooltip("Primary value (damage, block, heal amount)")]
        private int _value;

        [SerializeField, Tooltip("Secondary value (hit count, duration, stacks)")]
        private int _secondaryValue;

        [SerializeField, Tooltip("Status effect to apply (if applicable)")]
        private StatusType _statusType;

        [SerializeField, Tooltip("Custom description for Special intents")]
        private string _customDescription;

        // ============================================
        // Accessors
        // ============================================

        /// <summary>Type of intent action.</summary>
        public IntentType IntentType => _intentType;

        /// <summary>Primary value (damage, block, heal amount).</summary>
        public int Value => _value;

        /// <summary>Secondary value (hit count, duration, stacks).</summary>
        public int SecondaryValue => _secondaryValue;

        /// <summary>Status effect to apply.</summary>
        public StatusType StatusType => _statusType;

        /// <summary>Custom description for Special intents.</summary>
        public string CustomDescription => _customDescription;

        // ============================================
        // Constructor
        // ============================================

        public IntentStep()
        {
            _intentType = IntentType.Attack;
            _value = 0;
            _secondaryValue = 0;
            _customDescription = "";
        }

        public IntentStep(IntentType type, int value, int secondaryValue = 0, StatusType statusType = StatusType.Burn, string customDescription = "")
        {
            _intentType = type;
            _value = value;
            _secondaryValue = secondaryValue;
            _statusType = statusType;
            _customDescription = customDescription ?? "";
        }

        // ============================================
        // Utility
        // ============================================

        /// <summary>
        /// Get display text for this intent.
        /// </summary>
        public string GetDisplayText()
        {
            return _intentType switch
            {
                IntentType.Attack => $"{_value}",
                IntentType.AttackMultiple => $"{_value}x{_secondaryValue}",
                IntentType.AttackAll => $"{_value} ALL",
                IntentType.Defend => $"{_value}",
                IntentType.Buff => "BUFF",
                IntentType.Debuff => "DEBUFF",
                IntentType.Corrupt => $"+{_value}",
                IntentType.Heal => $"+{_value}",
                IntentType.Summon => "SUMMON",
                IntentType.Stealth => "STEALTH",
                IntentType.Special => !string.IsNullOrEmpty(_customDescription) ? _customDescription : "SPECIAL",
                IntentType.Unknown => "?",
                _ => ""
            };
        }
    }

    /// <summary>
    /// Defines the sequence of actions an enemy will take.
    /// Pattern cycles through steps each turn.
    /// </summary>
    [Serializable]
    public class IntentPattern
    {
        // ============================================
        // Fields
        // ============================================

        [SerializeField, Tooltip("Sequence of intent steps")]
        private List<IntentStep> _steps = new();

        [SerializeField, Tooltip("True to loop back to start, false to repeat last step")]
        private bool _looping = true;

        // ============================================
        // Runtime State
        // ============================================

        private int _currentIndex = 0;

        // ============================================
        // Accessors
        // ============================================

        /// <summary>All steps in the pattern.</summary>
        public IReadOnlyList<IntentStep> Steps => _steps;

        /// <summary>Whether pattern loops or repeats final step.</summary>
        public bool Looping => _looping;

        /// <summary>Current step index.</summary>
        public int CurrentIndex => _currentIndex;

        /// <summary>Total number of steps.</summary>
        public int StepCount => _steps.Count;

        // ============================================
        // Pattern Control
        // ============================================

        /// <summary>
        /// Get the current intent step.
        /// </summary>
        /// <returns>Current intent or null if empty</returns>
        public IntentStep GetCurrentIntent()
        {
            if (_steps.Count == 0) return null;
            return _steps[_currentIndex];
        }

        /// <summary>
        /// Get the next intent (for preview) without advancing.
        /// </summary>
        /// <returns>Next intent step</returns>
        public IntentStep PeekNextIntent()
        {
            if (_steps.Count == 0) return null;

            int nextIndex = _currentIndex + 1;
            if (nextIndex >= _steps.Count)
            {
                nextIndex = _looping ? 0 : _steps.Count - 1;
            }
            return _steps[nextIndex];
        }

        /// <summary>
        /// Advance to the next intent step.
        /// Called at end of enemy turn.
        /// </summary>
        public void AdvanceIntent()
        {
            if (_steps.Count == 0) return;

            _currentIndex++;
            if (_currentIndex >= _steps.Count)
            {
                _currentIndex = _looping ? 0 : _steps.Count - 1;
            }
        }

        /// <summary>
        /// Reset pattern to first step.
        /// Called at combat start.
        /// </summary>
        public void Reset()
        {
            _currentIndex = 0;
        }

        /// <summary>
        /// Create a runtime copy of this pattern.
        /// </summary>
        public IntentPattern Clone()
        {
            var clone = new IntentPattern
            {
                _looping = _looping,
                _currentIndex = 0
            };

            foreach (var step in _steps)
            {
                clone._steps.Add(new IntentStep(
                    step.IntentType,
                    step.Value,
                    step.SecondaryValue,
                    step.StatusType,
                    step.CustomDescription
                ));
            }

            return clone;
        }
    }
}
