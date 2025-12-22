// ============================================
// SetupPhase.cs
// Initialize combat, apply start-of-combat effects
// ============================================

using System.Collections.Generic;
using UnityEngine;
using HNR.Core;
using HNR.Core.Interfaces;
using HNR.Cards;

namespace HNR.Combat
{
    /// <summary>
    /// Initialize combat, apply start-of-combat effects.
    /// Collects team cards and initializes the deck.
    /// </summary>
    public class SetupPhase : ICombatPhase
    {
        public CombatPhase PhaseType => CombatPhase.Setup;

        public void Enter(CombatContext context)
        {
            Debug.Log("[SetupPhase] Initializing combat");

            context.TurnNumber = 0;
            context.TeamBlock = 0;
            context.IsPlayerTurn = true;

            // Collect all cards from RunManager's deck (includes starting cards + rewards)
            var teamCards = new List<CardDataSO>();

            // First, try to get cards from RunManager (contains starting cards + any acquired cards)
            if (ServiceLocator.TryGet<IRunManager>(out var runManager) && runManager.Deck != null && runManager.Deck.Count > 0)
            {
                foreach (var card in runManager.Deck)
                {
                    teamCards.Add(card);
                }
                Debug.Log($"[SetupPhase] Loaded {teamCards.Count} cards from RunManager.Deck");
            }
            else
            {
                // Fallback: Collect from team's starting decks (only if RunManager has no deck)
                foreach (var requiem in context.Team)
                {
                    if (requiem.Data != null && requiem.Data.StartingCards != null)
                    {
                        foreach (var card in requiem.Data.StartingCards)
                        {
                            teamCards.Add(card);
                        }
                    }
                }
                Debug.Log($"[SetupPhase] Fallback: Loaded {teamCards.Count} cards from StartingCards");
            }

            // Initialize deck with collected cards
            context.DeckManager?.InitializeDeck(teamCards);

            Debug.Log($"[SetupPhase] Deck initialized with {teamCards.Count} cards from {context.Team.Count} Requiems");
        }

        public void Update(CombatContext context)
        {
            // Setup is instant - transition to draw phase
            ServiceLocator.Get<TurnManager>()?.TransitionToPhase(GetNextPhase(context));
        }

        public void Exit(CombatContext context)
        {
            // No cleanup needed
        }

        public CombatPhase GetNextPhase(CombatContext context)
        {
            return CombatPhase.DrawPhase;
        }
    }
}
