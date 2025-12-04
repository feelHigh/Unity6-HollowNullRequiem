// ============================================
// CardUpgradeManager.cs
// Manages card upgrades during runs
// ============================================

using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using HNR.Core;
using HNR.Core.Events;
using HNR.Combat;

namespace HNR.Cards
{
    /// <summary>
    /// Manages card upgrades during runs.
    /// Accessed from Sanctuary nodes via UI.
    /// </summary>
    public class CardUpgradeManager : MonoBehaviour
    {
        // ============================================
        // Unity Lifecycle
        // ============================================

        private void Awake()
        {
            ServiceLocator.Register(this);
        }

        private void OnDestroy()
        {
            ServiceLocator.Unregister<CardUpgradeManager>();
        }

        // ============================================
        // Query Methods
        // ============================================

        /// <summary>
        /// Get all upgradeable cards in the current deck.
        /// </summary>
        /// <returns>List of cards that can be upgraded</returns>
        public List<CardInstance> GetUpgradeableCards()
        {
            if (!ServiceLocator.TryGet<DeckManager>(out var deckManager))
            {
                Debug.LogWarning("[CardUpgradeManager] DeckManager not found");
                return new List<CardInstance>();
            }

            return deckManager.AllCards
                .Where(c => c.CanUpgrade)
                .ToList();
        }

        /// <summary>
        /// Get all upgradeable cards from a provided deck list.
        /// Use this when not in combat (deck not initialized in DeckManager).
        /// </summary>
        /// <param name="deck">List of card instances to check</param>
        /// <returns>List of cards that can be upgraded</returns>
        public List<CardInstance> GetUpgradeableCards(IEnumerable<CardInstance> deck)
        {
            if (deck == null) return new List<CardInstance>();

            return deck
                .Where(c => c != null && c.CanUpgrade)
                .ToList();
        }

        /// <summary>
        /// Check if a specific card can be upgraded.
        /// </summary>
        /// <param name="card">Card to check</param>
        /// <returns>True if card can be upgraded</returns>
        public bool CanUpgrade(CardInstance card)
        {
            return card != null && card.CanUpgrade;
        }

        // ============================================
        // Upgrade Methods
        // ============================================

        /// <summary>
        /// Upgrade a card instance to its upgraded version.
        /// </summary>
        /// <param name="card">Card to upgrade</param>
        /// <returns>True if upgrade succeeded</returns>
        public bool UpgradeCard(CardInstance card)
        {
            if (card == null)
            {
                Debug.LogWarning("[CardUpgradeManager] Cannot upgrade null card");
                return false;
            }

            if (!card.CanUpgrade)
            {
                Debug.LogWarning($"[CardUpgradeManager] {card.Data.CardName} cannot be upgraded");
                return false;
            }

            // Apply the upgrade
            if (!card.ApplyUpgrade())
            {
                return false;
            }

            // Publish event
            EventBus.Publish(new CardUpgradedEvent(card));

            Debug.Log($"[CardUpgradeManager] Successfully upgraded card to {card.Data.CardName}");
            return true;
        }

        /// <summary>
        /// Upgrade a random upgradeable card from the deck.
        /// Useful for Echo Events that grant random upgrades.
        /// </summary>
        /// <param name="deck">Deck to upgrade from</param>
        /// <returns>The upgraded card, or null if none available</returns>
        public CardInstance UpgradeRandomCard(IEnumerable<CardInstance> deck)
        {
            var upgradeableCards = GetUpgradeableCards(deck);
            if (upgradeableCards.Count == 0)
            {
                Debug.Log("[CardUpgradeManager] No upgradeable cards available");
                return null;
            }

            var randomCard = upgradeableCards[Random.Range(0, upgradeableCards.Count)];
            if (UpgradeCard(randomCard))
            {
                return randomCard;
            }

            return null;
        }

        // ============================================
        // UI Helper Methods
        // ============================================

        /// <summary>
        /// Get upgrade preview information for a card.
        /// </summary>
        /// <param name="card">Card to preview upgrade for</param>
        /// <returns>Tuple of (before, after) card data, or null if not upgradeable</returns>
        public (CardDataSO before, CardDataSO after)? GetUpgradePreview(CardInstance card)
        {
            if (card == null || !card.CanUpgrade)
                return null;

            return (card.Data, card.Data.UpgradedVersion);
        }

        /// <summary>
        /// Get a formatted string showing upgrade changes.
        /// </summary>
        /// <param name="card">Card to describe upgrade for</param>
        /// <returns>Description of upgrade changes</returns>
        public string GetUpgradeDescription(CardInstance card)
        {
            var preview = GetUpgradePreview(card);
            if (!preview.HasValue)
                return "No upgrade available";

            var before = preview.Value.before;
            var after = preview.Value.after;

            var changes = new List<string>();

            // Check damage change
            int damageBefore = before.GetTotalDamage();
            int damageAfter = after.GetTotalDamage();
            if (damageAfter != damageBefore)
                changes.Add($"Damage: {damageBefore} → {damageAfter}");

            // Check block change
            int blockBefore = before.GetTotalBlock();
            int blockAfter = after.GetTotalBlock();
            if (blockAfter != blockBefore)
                changes.Add($"Block: {blockBefore} → {blockAfter}");

            // Check cost change
            if (after.APCost != before.APCost)
                changes.Add($"Cost: {before.APCost} → {after.APCost}");

            if (changes.Count == 0)
                changes.Add("Enhanced effects");

            return string.Join("\n", changes);
        }
    }
}
