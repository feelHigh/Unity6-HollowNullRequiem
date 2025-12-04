// ============================================
// CardInstance.cs
// Runtime instance of a card during gameplay
// ============================================

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace HNR.Cards
{
    /// <summary>
    /// Runtime instance of a card during gameplay.
    /// Wraps CardDataSO with combat-specific state and modifiers.
    /// </summary>
    public class CardInstance
    {
        // ============================================
        // Backing Fields
        // ============================================

        private CardDataSO _currentData;
        private CardDataSO _originalData;

        // ============================================
        // Properties
        // ============================================

        /// <summary>Current card data (may be upgraded version).</summary>
        public CardDataSO Data => _currentData;

        /// <summary>Original card data before any upgrades.</summary>
        public CardDataSO OriginalData => _originalData;

        /// <summary>Whether this instance uses upgraded card data.</summary>
        public bool IsUpgraded { get; private set; }

        /// <summary>Current AP cost after modifiers.</summary>
        public int CurrentCost { get; private set; }

        /// <summary>Active modifiers on this card.</summary>
        public List<CardModifier> Modifiers { get; }

        /// <summary>Unique identifier for this instance.</summary>
        public Guid InstanceId { get; }

        // ============================================
        // Constructor
        // ============================================

        /// <summary>
        /// Create a new card instance from card data.
        /// </summary>
        /// <param name="data">Source card data</param>
        /// <param name="upgraded">Whether to use upgraded version</param>
        /// <exception cref="ArgumentNullException">If data is null</exception>
        public CardInstance(CardDataSO data, bool upgraded = false)
        {
            _originalData = data ?? throw new ArgumentNullException(nameof(data));
            _currentData = data;
            IsUpgraded = upgraded;
            CurrentCost = data.APCost;
            Modifiers = new List<CardModifier>();
            InstanceId = Guid.NewGuid();
        }

        // ============================================
        // Upgrade Support
        // ============================================

        /// <summary>
        /// Check if this card can be upgraded.
        /// </summary>
        public bool CanUpgrade => !IsUpgraded && _currentData.UpgradedVersion != null;

        /// <summary>
        /// Apply an upgrade, replacing the card's data with the upgraded version.
        /// </summary>
        /// <param name="upgradedData">Upgraded card data (optional - uses UpgradedVersion if null)</param>
        /// <returns>True if upgrade was applied</returns>
        public bool ApplyUpgrade(CardDataSO upgradedData = null)
        {
            if (IsUpgraded)
            {
                Debug.LogWarning($"[CardInstance] {_currentData.CardName} is already upgraded");
                return false;
            }

            var targetData = upgradedData ?? _currentData.UpgradedVersion;
            if (targetData == null)
            {
                Debug.LogWarning($"[CardInstance] {_currentData.CardName} has no upgrade path");
                return false;
            }

            _currentData = targetData;
            IsUpgraded = true;
            RecalculateCost();

            Debug.Log($"[CardInstance] Upgraded to {_currentData.CardName}");
            return true;
        }

        // ============================================
        // Modifier Management
        // ============================================

        /// <summary>
        /// Add a temporary modifier to this card.
        /// </summary>
        /// <param name="modifier">Modifier to apply</param>
        public void AddModifier(CardModifier modifier)
        {
            if (modifier == null) return;
            Modifiers.Add(modifier);
            RecalculateCost();
        }

        /// <summary>
        /// Remove a specific modifier from this card.
        /// </summary>
        /// <param name="modifier">Modifier to remove</param>
        public void RemoveModifier(CardModifier modifier)
        {
            if (Modifiers.Remove(modifier))
            {
                RecalculateCost();
            }
        }

        /// <summary>
        /// Tick all modifiers, removing expired ones.
        /// Call at end of turn.
        /// </summary>
        public void TickModifiers()
        {
            Modifiers.RemoveAll(m => m.Tick());
            RecalculateCost();
        }

        /// <summary>
        /// Remove all modifiers from this card.
        /// </summary>
        public void ClearModifiers()
        {
            Modifiers.Clear();
            RecalculateCost();
        }

        /// <summary>
        /// Recalculate current cost based on base cost and modifiers.
        /// </summary>
        public void RecalculateCost()
        {
            CurrentCost = _currentData.APCost;

            foreach (var mod in Modifiers.Where(m => m.Type == ModifierType.Cost))
            {
                CurrentCost += mod.Value;
            }

            // Cost cannot go below 0
            CurrentCost = Mathf.Max(0, CurrentCost);
        }

        // ============================================
        // Playability Checks
        // ============================================

        /// <summary>
        /// Check if card can be played with current resources and targets.
        /// </summary>
        /// <param name="availableAP">Available Action Points</param>
        /// <param name="hasValidTarget">Whether a valid target exists</param>
        /// <returns>True if card can be played</returns>
        public bool CanPlay(int availableAP, bool hasValidTarget = true)
        {
            // Check AP cost
            if (CurrentCost > availableAP) return false;

            // Check targeting requirements
            if (Data.TargetType != TargetType.None &&
                Data.TargetType != TargetType.Self &&
                !hasValidTarget)
            {
                return false;
            }

            return true;
        }

        // ============================================
        // Modified Values
        // ============================================

        /// <summary>
        /// Get damage value with modifiers applied.
        /// </summary>
        /// <param name="baseDamage">Base damage from effect</param>
        /// <returns>Modified damage value</returns>
        public int GetModifiedDamage(int baseDamage)
        {
            int bonus = Modifiers
                .Where(m => m.Type == ModifierType.DamageBonus)
                .Sum(m => m.Value);
            return Mathf.Max(0, baseDamage + bonus);
        }

        /// <summary>
        /// Get block value with modifiers applied.
        /// </summary>
        /// <param name="baseBlock">Base block from effect</param>
        /// <returns>Modified block value</returns>
        public int GetModifiedBlock(int baseBlock)
        {
            int bonus = Modifiers
                .Where(m => m.Type == ModifierType.BlockBonus)
                .Sum(m => m.Value);
            return Mathf.Max(0, baseBlock + bonus);
        }

        /// <summary>
        /// Get total modified damage from all damage effects on this card.
        /// </summary>
        /// <returns>Total modified damage</returns>
        public int GetTotalModifiedDamage()
        {
            return GetModifiedDamage(Data.GetTotalDamage());
        }

        /// <summary>
        /// Get total modified block from all block effects on this card.
        /// </summary>
        /// <returns>Total modified block</returns>
        public int GetTotalModifiedBlock()
        {
            return GetModifiedBlock(Data.GetTotalBlock());
        }

        // ============================================
        // Utility
        // ============================================

        /// <summary>
        /// Create a copy of this card instance with new InstanceId.
        /// </summary>
        /// <returns>New CardInstance with same data and modifiers</returns>
        public CardInstance Clone()
        {
            var clone = new CardInstance(_originalData, IsUpgraded);
            // If this card was upgraded, apply the same upgrade to the clone
            if (IsUpgraded && _currentData != _originalData)
            {
                clone._currentData = _currentData;
                clone.IsUpgraded = true;
            }
            foreach (var mod in Modifiers)
            {
                clone.Modifiers.Add(new CardModifier(mod.Type, mod.Value, mod.RemainingTurns, mod.Source));
            }
            clone.RecalculateCost();
            return clone;
        }

        /// <summary>
        /// Get formatted string for debugging.
        /// </summary>
        public override string ToString()
        {
            return $"{Data.CardName} (Cost: {CurrentCost}, Mods: {Modifiers.Count})";
        }
    }
}
