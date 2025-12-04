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
    /// </summary>
    public class DrawPhase : ICombatPhase
    {
        private readonly int _cardsToDraw;
        private bool _drawComplete;

        public CombatPhase PhaseType => CombatPhase.DrawPhase;

        public DrawPhase(int cardsToDraw = 5)
        {
            _cardsToDraw = cardsToDraw;
        }

        public void Enter(CombatContext context)
        {
            _drawComplete = false;

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

            Debug.Log($"[DrawPhase] Turn {context.TurnNumber} - AP: {context.CurrentAP}, Drawing {_cardsToDraw} cards");

            // Draw cards from deck to hand
            var cards = context.DeckManager?.DrawCards(_cardsToDraw);
            if (cards != null)
            {
                foreach (var card in cards)
                {
                    context.HandManager?.AddCard(card);
                }
                Debug.Log($"[DrawPhase] Drew {cards.Count} cards");
            }

            _drawComplete = true;
        }

        public void Update(CombatContext context)
        {
            // Wait for draw animation, then advance
            if (_drawComplete)
            {
                _drawComplete = false;
                ServiceLocator.Get<TurnManager>()?.TransitionToPhase(GetNextPhase(context));
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
