// ============================================
// EndPhase.cs
// Discard hand, process end-of-turn effects
// ============================================

using UnityEngine;
using HNR.Core;

namespace HNR.Combat
{
    /// <summary>
    /// Discard hand, process end-of-turn effects.
    /// Transitions to enemy phase after completion.
    /// </summary>
    public class EndPhase : ICombatPhase
    {
        private bool _complete;

        public CombatPhase PhaseType => CombatPhase.EndPhase;

        public void Enter(CombatContext context)
        {
            _complete = false;

            // Discard remaining hand
            var handManager = context.HandManager;
            var deckManager = context.DeckManager;

            if (handManager != null && deckManager != null)
            {
                var handCards = handManager.GetHandInstances();
                deckManager.DiscardAll(handCards);
                handManager.ClearHand();
                Debug.Log($"[EndPhase] Discarded {handCards.Count} cards");
            }

            // TODO: Process end-of-turn status effects
            // - Tick down durations
            // - Apply damage-over-time
            // - Remove expired effects

            _complete = true;
            Debug.Log("[EndPhase] End of turn effects processed");
        }

        public void Update(CombatContext context)
        {
            if (_complete)
            {
                _complete = false;
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
