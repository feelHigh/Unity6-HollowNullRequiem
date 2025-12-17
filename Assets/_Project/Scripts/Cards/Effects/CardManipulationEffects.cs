// ============================================
// CardManipulationEffects.cs
// Card manipulation effect implementations
// ============================================

using System.Collections.Generic;
using UnityEngine;
using HNR.Core;
using HNR.Core.Events;
using HNR.Combat;

namespace HNR.Cards
{
    /// <summary>
    /// Draw cards from the deck into hand.
    /// </summary>
    public class DrawCardsEffect : ICardEffect
    {
        public void Execute(CardEffectData data, EffectContext context)
        {
            int cardsToDraw = data.Value;

            // Get managers from context or ServiceLocator
            var deckManager = context.DeckManager;
            var handManager = context.CombatContext?.HandManager;

            if (deckManager == null)
            {
                ServiceLocator.TryGet<DeckManager>(out deckManager);
            }
            if (handManager == null)
            {
                ServiceLocator.TryGet<HandManager>(out handManager);
            }

            if (deckManager == null || handManager == null)
            {
                Debug.LogWarning("[DrawCardsEffect] DeckManager or HandManager not available");
                return;
            }

            // Draw cards (DeckManager handles reshuffling and events)
            var drawnCards = deckManager.DrawCards(cardsToDraw);

            // Add each card to hand
            foreach (var card in drawnCards)
            {
                handManager.AddCard(card);
            }

            Debug.Log($"[DrawCardsEffect] Drew {drawnCards.Count} cards");
        }
    }

    /// <summary>
    /// Discard random cards from hand.
    /// </summary>
    public class DiscardRandomEffect : ICardEffect
    {
        public void Execute(CardEffectData data, EffectContext context)
        {
            int cardsToDiscard = data.Value;

            var handManager = context.CombatContext?.HandManager;
            var deckManager = context.DeckManager;

            if (handManager == null)
            {
                ServiceLocator.TryGet<HandManager>(out handManager);
            }
            if (deckManager == null)
            {
                ServiceLocator.TryGet<DeckManager>(out deckManager);
            }

            if (handManager == null)
            {
                Debug.LogWarning("[DiscardRandomEffect] HandManager not available");
                return;
            }

            // Get current hand and randomly select cards to discard
            var hand = new List<Card>(handManager.Hand);
            var toDiscard = new List<Card>();

            // Shuffle indices and take first N
            var indices = new List<int>();
            for (int i = 0; i < hand.Count; i++)
            {
                indices.Add(i);
            }

            // Fisher-Yates shuffle
            var rng = new System.Random();
            for (int i = indices.Count - 1; i > 0; i--)
            {
                int j = rng.Next(i + 1);
                (indices[i], indices[j]) = (indices[j], indices[i]);
            }

            // Take up to cardsToDiscard
            int discardCount = Mathf.Min(cardsToDiscard, hand.Count);
            for (int i = 0; i < discardCount; i++)
            {
                toDiscard.Add(hand[indices[i]]);
            }

            // Discard selected cards
            foreach (var card in toDiscard)
            {
                handManager.RemoveCard(card);
                if (deckManager != null && card.CardInstance != null)
                {
                    deckManager.Discard(card.CardInstance);
                }
                EventBus.Publish(new CardDiscardedEvent(card.CardInstance, false));
            }

            Debug.Log($"[DiscardRandomEffect] Discarded {toDiscard.Count} random cards");
        }
    }

    /// <summary>
    /// Exhaust the played card (remove from combat permanently).
    /// Exhausted cards do not return to the deck this combat.
    /// </summary>
    public class ExhaustEffect : ICardEffect
    {
        public void Execute(CardEffectData data, EffectContext context)
        {
            if (context.Card == null)
            {
                Debug.LogWarning("[ExhaustEffect] No card in context");
                return;
            }

            var deckManager = context.DeckManager;
            if (deckManager == null)
            {
                ServiceLocator.TryGet<DeckManager>(out deckManager);
            }

            if (deckManager != null)
            {
                deckManager.Exhaust(context.Card);
            }

            EventBus.Publish(new CardDiscardedEvent(context.Card, exhausted: true));
            Debug.Log($"[ExhaustEffect] Card exhausted: {context.Card.Data?.CardName ?? "Unknown"}");
        }
    }

    /// <summary>
    /// Add a copy of a card to hand.
    /// </summary>
    public class CopyCardEffect : ICardEffect
    {
        public void Execute(CardEffectData data, EffectContext context)
        {
            if (context.Card?.Data == null)
            {
                Debug.LogWarning("[CopyCardEffect] No card to copy");
                return;
            }

            var handManager = context.CombatContext?.HandManager;
            if (handManager == null)
            {
                ServiceLocator.TryGet<HandManager>(out handManager);
            }

            if (handManager == null)
            {
                Debug.LogWarning("[CopyCardEffect] HandManager not available");
                return;
            }

            // Create a copy of the card instance
            var copy = context.Card.Clone();

            // Add to hand
            handManager.AddCard(copy);

            Debug.Log($"[CopyCardEffect] Created copy of {context.Card.Data.CardName}");
        }
    }

    /// <summary>
    /// Shuffle discard pile back into draw pile.
    /// </summary>
    public class ShuffleDiscardEffect : ICardEffect
    {
        public void Execute(CardEffectData data, EffectContext context)
        {
            var deckManager = context.DeckManager;
            if (deckManager == null)
            {
                ServiceLocator.TryGet<DeckManager>(out deckManager);
            }

            if (deckManager == null)
            {
                Debug.LogWarning("[ShuffleDiscardEffect] DeckManager not available");
                return;
            }

            // Force immediate reshuffle of discard pile into draw pile
            deckManager.ForceReshuffle();
            Debug.Log("[ShuffleDiscardEffect] Forced reshuffle of discard pile into draw pile");
        }
    }
}
