// ============================================
// EndPhase.cs
// Discard hand, process end-of-turn effects
// ============================================

using UnityEngine;
using HNR.Core;
using HNR.Core.Events;

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

            // Discard remaining hand
            var handCards = context.HandManager?.GetHandInstances();
            if (handCards != null && handCards.Count > 0)
            {
                context.DeckManager?.DiscardAll(handCards);
                context.HandManager?.ClearHand();
                Debug.Log($"[EndPhase] Discarded {handCards.Count} cards");
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
            context.StatusManager?.TickEffects();

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
