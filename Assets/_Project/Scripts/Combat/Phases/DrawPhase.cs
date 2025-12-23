// ============================================
// DrawPhase.cs
// Player draws cards for the turn
// ============================================

using UnityEngine;
using HNR.Core;
using HNR.Core.Events;

namespace HNR.Combat
{
    /// <summary>
    /// Player draws cards for the turn.
    /// Replenishes AP and resets block.
    /// Uses sequential card drawing with proper animation timing.
    /// </summary>
    public class DrawPhase : ICombatPhase
    {
        private readonly int _cardsToDraw;
        private bool _drawComplete;
        private TurnManager _turnManager;

        public CombatPhase PhaseType => CombatPhase.DrawPhase;

        public DrawPhase(int cardsToDraw = 5)
        {
            _cardsToDraw = cardsToDraw;
        }

        public void Enter(CombatContext context)
        {
            _drawComplete = false;
            _turnManager = ServiceLocator.TryGet<TurnManager>(out var tm) ? tm : null;

            // Advance turn counter
            context.TurnNumber++;
            context.IsPlayerTurn = true;

            // Replenish AP
            context.CurrentAP = context.MaxAP;

            // Reset block (block doesn't carry over between turns)
            int previousBlock = context.TeamBlock;
            context.TeamBlock = 0;

            // Publish turn start events
            EventBus.Publish(new TurnStartedEvent(true, context.TurnNumber));
            EventBus.Publish(new APChangedEvent(context.CurrentAP, context.MaxAP));

            if (previousBlock > 0)
            {
                EventBus.Publish(new BlockChangedEvent(0, previousBlock));
            }

            Debug.Log($"[DrawPhase] Turn {context.TurnNumber} - AP: {context.CurrentAP}, Drawing {_cardsToDraw} cards sequentially");

            // Draw cards sequentially with animation delays
            // This properly handles reshuffle mid-draw with additional delay
            if (_turnManager != null)
            {
                _turnManager.DrawCardsSequential(_cardsToDraw, OnDrawComplete);
            }
            else
            {
                // Fallback to immediate draw if TurnManager not available
                var cards = context.DeckManager?.DrawCards(_cardsToDraw);
                if (cards != null)
                {
                    foreach (var card in cards)
                    {
                        context.HandManager?.AddCard(card);
                    }
                }
                _drawComplete = true;
            }
        }

        private void OnDrawComplete()
        {
            Debug.Log("[DrawPhase] Sequential draw complete, advancing to PlayerPhase");
            _drawComplete = true;
        }

        public void Update(CombatContext context)
        {
            // Wait for sequential draw animation to complete, then advance
            if (_drawComplete)
            {
                _drawComplete = false;
                _turnManager?.TransitionToPhase(GetNextPhase(context));
            }
        }

        public void Exit(CombatContext context)
        {
            // No cleanup needed
        }

        public CombatPhase GetNextPhase(CombatContext context)
        {
            return CombatPhase.PlayerPhase;
        }
    }
}
