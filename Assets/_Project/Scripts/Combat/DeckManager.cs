// ============================================
// DeckManager.cs
// Manages the shared deck during combat
// ============================================

using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using HNR.Core;
using HNR.Core.Events;
using HNR.Core.Interfaces;
using HNR.Cards;

namespace HNR.Combat
{
    /// <summary>
    /// Manages the shared deck during combat.
    /// Handles draw pile, discard pile, and exhaust pile.
    /// </summary>
    public class DeckManager : MonoBehaviour
    {
        // ============================================
        // Fields
        // ============================================

        private List<CardInstance> _drawPile = new();
        private List<CardInstance> _discardPile = new();
        private List<CardInstance> _exhaustPile = new();

        // ============================================
        // Properties
        // ============================================

        /// <summary>Number of cards in draw pile.</summary>
        public int DrawPileCount => _drawPile.Count;

        /// <summary>Number of cards in discard pile.</summary>
        public int DiscardPileCount => _discardPile.Count;

        /// <summary>Number of exhausted cards.</summary>
        public int ExhaustPileCount => _exhaustPile.Count;

        /// <summary>Read-only access to draw pile for card modifiers.</summary>
        public IReadOnlyList<CardInstance> DrawPile => _drawPile;

        /// <summary>Read-only access to discard pile for card modifiers.</summary>
        public IReadOnlyList<CardInstance> DiscardPile => _discardPile;

        /// <summary>Read-only access to exhaust pile.</summary>
        public IReadOnlyList<CardInstance> ExhaustPile => _exhaustPile;

        /// <summary>All cards currently in deck (draw + discard, excludes exhaust).</summary>
        public IEnumerable<CardInstance> AllCards => _drawPile.Concat(_discardPile);

        // ============================================
        // Unity Lifecycle
        // ============================================

        private void Awake()
        {
            ServiceLocator.Register(this);
        }

        private void OnDestroy()
        {
            ServiceLocator.Unregister<DeckManager>();
        }

        // ============================================
        // Initialization
        // ============================================

        /// <summary>
        /// Initialize deck with team's cards at combat start.
        /// Checks RunManager for upgraded cards and applies upgrade state.
        /// </summary>
        /// <param name="teamCards">All cards from the team's combined deck</param>
        public void InitializeDeck(List<CardDataSO> teamCards)
        {
            _drawPile.Clear();
            _discardPile.Clear();
            _exhaustPile.Clear();

            // Get RunManager to check for upgraded cards
            IRunManager runManager = null;
            ServiceLocator.TryGet(out runManager);

            int upgradedCount = 0;
            foreach (var cardData in teamCards)
            {
                // Check if this card is upgraded
                bool isUpgraded = runManager != null && runManager.IsCardUpgraded(cardData.CardId);
                var cardInstance = new CardInstance(cardData, isUpgraded);

                // If upgraded and has upgraded version, apply the upgrade
                if (isUpgraded && cardData.UpgradedVersion != null)
                {
                    cardInstance.ApplyUpgrade(cardData.UpgradedVersion);
                    upgradedCount++;
                }

                _drawPile.Add(cardInstance);
            }

            Shuffle(_drawPile);
            Debug.Log($"[DeckManager] Deck initialized with {_drawPile.Count} cards ({upgradedCount} upgraded)");
        }

        // ============================================
        // Draw Operations
        // ============================================

        /// <summary>
        /// Draw a single card from the deck.
        /// Reshuffles discard pile if draw pile is empty.
        /// </summary>
        /// <returns>The drawn card, or null if deck is empty</returns>
        public CardInstance Draw()
        {
            if (_drawPile.Count == 0)
            {
                if (_discardPile.Count == 0) return null;
                ReshuffleDiscardIntoDraw();
            }

            if (_drawPile.Count > 0)
            {
                var card = _drawPile[0];
                _drawPile.RemoveAt(0);
                EventBus.Publish(new CardDrawnEvent(card));
                return card;
            }

            return null;
        }

        /// <summary>
        /// Draw cards from the deck.
        /// Reshuffles discard pile if draw pile is empty.
        /// </summary>
        /// <param name="count">Number of cards to draw</param>
        /// <returns>List of drawn cards</returns>
        public List<CardInstance> DrawCards(int count)
        {
            var drawn = new List<CardInstance>();

            for (int i = 0; i < count; i++)
            {
                if (_drawPile.Count == 0)
                {
                    if (_discardPile.Count == 0) break;
                    ReshuffleDiscardIntoDraw();
                }

                if (_drawPile.Count > 0)
                {
                    var card = _drawPile[0];
                    _drawPile.RemoveAt(0);
                    drawn.Add(card);
                    EventBus.Publish(new CardDrawnEvent(card));
                }
            }

            return drawn;
        }

        // ============================================
        // Discard Operations
        // ============================================

        /// <summary>
        /// Discard a single card to the discard pile.
        /// </summary>
        /// <param name="card">Card to discard</param>
        public void Discard(CardInstance card)
        {
            if (card == null) return;
            _discardPile.Add(card);
            EventBus.Publish(new CardDiscardedEvent(card, false));
        }

        /// <summary>
        /// Discard multiple cards to the discard pile.
        /// </summary>
        /// <param name="cards">Cards to discard</param>
        public void DiscardAll(IEnumerable<CardInstance> cards)
        {
            foreach (var card in cards)
            {
                Discard(card);
            }
        }

        // ============================================
        // Exhaust Operations
        // ============================================

        /// <summary>
        /// Exhaust a card (remove from combat permanently).
        /// Exhausted cards do not reshuffle back into the deck.
        /// </summary>
        /// <param name="card">Card to exhaust</param>
        public void Exhaust(CardInstance card)
        {
            if (card == null) return;
            _exhaustPile.Add(card);
            EventBus.Publish(new CardDiscardedEvent(card, true));
        }

        // ============================================
        // Shuffle Operations
        // ============================================

        /// <summary>
        /// Force reshuffle discard pile into draw pile.
        /// Called by ShuffleDiscardEffect or other forced reshuffle mechanics.
        /// </summary>
        public void ForceReshuffle()
        {
            if (_discardPile.Count == 0)
            {
                Debug.Log("[DeckManager] No cards in discard pile to reshuffle");
                return;
            }
            ReshuffleDiscardIntoDraw();
        }

        /// <summary>
        /// Reshuffle all discarded cards back into the draw pile.
        /// Called automatically when draw pile is empty.
        /// </summary>
        private void ReshuffleDiscardIntoDraw()
        {
            int reshuffleCount = _discardPile.Count;
            Debug.Log($"[DeckManager] Reshuffling {reshuffleCount} cards into draw pile");
            _drawPile.AddRange(_discardPile);
            _discardPile.Clear();
            Shuffle(_drawPile);

            // Publish reshuffle event for UI/animation handling
            EventBus.Publish(new DeckReshuffledEvent(reshuffleCount));
        }

        /// <summary>
        /// Fisher-Yates shuffle algorithm.
        /// </summary>
        private void Shuffle<T>(List<T> list)
        {
            var rng = new System.Random();
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                (list[k], list[n]) = (list[n], list[k]);
            }
        }

        // ============================================
        // Debug
        // ============================================

        /// <summary>
        /// Get debug info string for UI display.
        /// </summary>
        public string GetDebugInfo()
        {
            return $"Draw: {DrawPileCount}, Discard: {DiscardPileCount}, Exhaust: {ExhaustPileCount}";
        }
    }
}
