// ============================================
// EndPhase.cs
// Discard hand, process end-of-turn effects
// ============================================

using System.Collections.Generic;
using UnityEngine;
using HNR.Core;
using HNR.Core.Events;
using HNR.Cards;
using HNR.UI.Combat;

namespace HNR.Combat
{
    /// <summary>
    /// Discard hand, process end-of-turn effects.
    /// Ticks card modifiers and status effects.
    /// </summary>
    public class EndPhase : ICombatPhase
    {
        private bool _processComplete;

        public CombatPhase PhaseType => CombatPhase.EndPhase;

        public void Enter(CombatContext context)
        {
            _processComplete = false;
            Debug.Log("[EndPhase] Processing end of player turn");

            // Discard remaining hand - try CardFanLayout first (new system), then HandManager (legacy)
            bool cardsDiscarded = false;

            // Try CardFanLayout (CombatCard-based system)
            if (ServiceLocator.TryGet<CardFanLayout>(out var cardFanLayout) && cardFanLayout.Cards.Count > 0)
            {
                var cardInstances = new List<CardInstance>();
                foreach (var combatCard in cardFanLayout.Cards)
                {
                    if (combatCard?.CardData != null)
                    {
                        cardInstances.Add(combatCard.CardData);
                    }
                }

                if (cardInstances.Count > 0)
                {
                    context.DeckManager?.DiscardAll(cardInstances);
                    Debug.Log($"[EndPhase] Discarded {cardInstances.Count} cards from CardFanLayout");
                }

                cardFanLayout.ClearHand();
                cardsDiscarded = true;
            }

            // Fallback: HandManager (legacy Card-based system)
            if (!cardsDiscarded)
            {
                var handCards = context.HandManager?.GetHandInstances();
                if (handCards != null && handCards.Count > 0)
                {
                    context.DeckManager?.DiscardAll(handCards);
                    context.HandManager?.ClearHand();
                    Debug.Log($"[EndPhase] Discarded {handCards.Count} cards from HandManager");
                }
            }

            // Tick card modifiers (reduce durations, remove expired)
            var allCards = context.DeckManager?.AllCards;
            if (allCards != null)
            {
                foreach (var card in allCards)
                {
                    card.TickModifiers();
                }
            }

            // Tick status effects
            context.StatusManager?.TickAllEffects();

            EventBus.Publish(new TurnEndedEvent(true));
            _processComplete = true;
        }

        public void Update(CombatContext context)
        {
            if (_processComplete)
            {
                _processComplete = false;
                ServiceLocator.Get<TurnManager>()?.TransitionToPhase(GetNextPhase(context));
            }
        }

        public void Exit(CombatContext context)
        {
            context.IsPlayerTurn = false;
        }

        public CombatPhase GetNextPhase(CombatContext context)
        {
            return CombatPhase.EnemyPhase;
        }
    }
}
