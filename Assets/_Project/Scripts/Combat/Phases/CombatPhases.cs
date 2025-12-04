// ============================================
// CombatPhases.cs
// Combat phase implementations
// ============================================

using UnityEngine;
using HNR.Core;
using HNR.Core.Events;

namespace HNR.Combat
{
    // ============================================
    // SETUP PHASE
    // ============================================

    /// <summary>
    /// Initialize combat, apply start-of-combat effects.
    /// </summary>
    public class SetupPhase : ICombatPhase
    {
        public CombatPhase PhaseType => CombatPhase.Setup;

        public void Enter(CombatContext context)
        {
            context.TurnNumber = 0;
            context.IsPlayerTurn = true;
            Debug.Log("[SetupPhase] Combat setup complete");
        }

        public void Update(CombatContext context)
        {
            // Auto-advance after setup
            ServiceLocator.Get<TurnManager>()?.AdvancePhase();
        }

        public void Exit(CombatContext context) { }

        public CombatPhase GetNextPhase(CombatContext context)
        {
            return CombatPhase.DrawPhase;
        }
    }

    // ============================================
    // DRAW PHASE
    // ============================================

    /// <summary>
    /// Player draws cards for the turn.
    /// </summary>
    public class DrawPhase : ICombatPhase
    {
        private readonly int _cardsToDraw;

        public CombatPhase PhaseType => CombatPhase.DrawPhase;

        public DrawPhase(int cardsToDraw = 5)
        {
            _cardsToDraw = cardsToDraw;
        }

        public void Enter(CombatContext context)
        {
            context.TurnNumber++;
            context.IsPlayerTurn = true;
            context.CurrentAP = context.MaxAP;
            context.TeamBlock = 0; // Block resets each turn

            EventBus.Publish(new TurnStartedEvent(true, context.TurnNumber));
            EventBus.Publish(new APChangedEvent(context.CurrentAP, context.MaxAP));
            EventBus.Publish(new BlockChangedEvent(0, context.TeamBlock));

            // Draw cards
            var deckManager = context.DeckManager;
            var handManager = context.HandManager;
            if (deckManager != null && handManager != null)
            {
                var drawnCards = deckManager.DrawCards(_cardsToDraw);
                foreach (var card in drawnCards)
                {
                    handManager.AddCard(card);
                }
            }

            Debug.Log($"[DrawPhase] Turn {context.TurnNumber}: Drew {_cardsToDraw} cards, {context.CurrentAP} AP");
        }

        public void Update(CombatContext context)
        {
            // Auto-advance to player phase
            ServiceLocator.Get<TurnManager>()?.AdvancePhase();
        }

        public void Exit(CombatContext context) { }

        public CombatPhase GetNextPhase(CombatContext context)
        {
            return CombatPhase.PlayerPhase;
        }
    }

    // ============================================
    // PLAYER PHASE
    // ============================================

    /// <summary>
    /// Player plays cards and uses abilities.
    /// </summary>
    public class PlayerPhase : ICombatPhase
    {
        public CombatPhase PhaseType => CombatPhase.PlayerPhase;

        public void Enter(CombatContext context)
        {
            Debug.Log("[PlayerPhase] Player turn - waiting for input");
        }

        public void Update(CombatContext context)
        {
            // Wait for player input - no auto-advance
            // TurnManager.EndPlayerTurn() triggers transition
        }

        public void Exit(CombatContext context)
        {
            EventBus.Publish(new TurnEndedEvent(true));
        }

        public CombatPhase GetNextPhase(CombatContext context)
        {
            return CombatPhase.EndPhase;
        }
    }

    // ============================================
    // END PHASE
    // ============================================

    /// <summary>
    /// Discard hand, process end-of-turn effects.
    /// </summary>
    public class EndPhase : ICombatPhase
    {
        public CombatPhase PhaseType => CombatPhase.EndPhase;

        public void Enter(CombatContext context)
        {
            // Discard remaining hand
            var handManager = context.HandManager;
            var deckManager = context.DeckManager;

            if (handManager != null && deckManager != null)
            {
                var handCards = handManager.GetHandInstances();
                deckManager.DiscardAll(handCards);
                handManager.ClearHand();
            }

            // TODO: Process end-of-turn status effects

            Debug.Log("[EndPhase] Hand discarded, effects processed");
        }

        public void Update(CombatContext context)
        {
            ServiceLocator.Get<TurnManager>()?.AdvancePhase();
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

    // ============================================
    // ENEMY PHASE
    // ============================================

    /// <summary>
    /// Enemies execute their telegraphed intents.
    /// </summary>
    public class EnemyPhase : ICombatPhase
    {
        public CombatPhase PhaseType => CombatPhase.EnemyPhase;

        public void Enter(CombatContext context)
        {
            context.IsPlayerTurn = false;
            EventBus.Publish(new TurnStartedEvent(false, context.TurnNumber));

            // TODO: Execute enemy intents
            foreach (var enemy in context.Enemies)
            {
                // Execute intent based on enemy's current pattern step
                Debug.Log($"[EnemyPhase] Enemy executes intent");
            }

            Debug.Log("[EnemyPhase] All enemies acted");
        }

        public void Update(CombatContext context)
        {
            // Check for defeat
            if (context.TeamHP <= 0)
            {
                ServiceLocator.Get<TurnManager>()?.TransitionToPhase(CombatPhase.Defeat);
                return;
            }

            // Auto-advance to next turn
            ServiceLocator.Get<TurnManager>()?.AdvancePhase();
        }

        public void Exit(CombatContext context)
        {
            EventBus.Publish(new TurnEndedEvent(false));
        }

        public CombatPhase GetNextPhase(CombatContext context)
        {
            // Check if all enemies defeated
            bool allDefeated = true;
            foreach (var enemy in context.Enemies)
            {
                if (enemy.CurrentHP > 0)
                {
                    allDefeated = false;
                    break;
                }
            }

            if (allDefeated)
                return CombatPhase.Victory;

            return CombatPhase.DrawPhase; // Next turn
        }
    }

    // ============================================
    // VICTORY PHASE
    // ============================================

    /// <summary>
    /// All enemies defeated - show rewards.
    /// </summary>
    public class VictoryPhase : ICombatPhase
    {
        public CombatPhase PhaseType => CombatPhase.Victory;

        public void Enter(CombatContext context)
        {
            Debug.Log("[VictoryPhase] Victory!");
            ServiceLocator.Get<TurnManager>()?.EndCombat(true);
        }

        public void Update(CombatContext context) { }
        public void Exit(CombatContext context) { }
        public CombatPhase GetNextPhase(CombatContext context) => CombatPhase.Victory;
    }

    // ============================================
    // DEFEAT PHASE
    // ============================================

    /// <summary>
    /// Team HP reached 0 - run ends.
    /// </summary>
    public class DefeatPhase : ICombatPhase
    {
        public CombatPhase PhaseType => CombatPhase.Defeat;

        public void Enter(CombatContext context)
        {
            Debug.Log("[DefeatPhase] Defeat!");
            ServiceLocator.Get<TurnManager>()?.EndCombat(false);
        }

        public void Update(CombatContext context) { }
        public void Exit(CombatContext context) { }
        public CombatPhase GetNextPhase(CombatContext context) => CombatPhase.Defeat;
    }
}
