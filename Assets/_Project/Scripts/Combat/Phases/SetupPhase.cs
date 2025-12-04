// ============================================
// SetupPhase.cs
// Initialize combat, apply start-of-combat effects
// ============================================

using System.Collections.Generic;
using UnityEngine;
using HNR.Core;
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

            // Collect all cards from team's starting decks
            var teamCards = new List<CardDataSO>();
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
