// ============================================
// GameEvents.cs
// All game event definitions for EventBus
// ============================================

using System.Collections.Generic;
using HNR.Core.Interfaces;
using HNR.Cards;
using HNR.Combat;
using HNR.Characters;

namespace HNR.Core.Events
{
    // ============================================
    // GAME STATE EVENTS
    // ============================================

    /// <summary>
    /// Published when the game transitions between high-level states.
    /// </summary>
    public class GameStateChangedEvent : GameEvent
    {
        /// <summary>The state we're leaving.</summary>
        public GameState PreviousState { get; }

        /// <summary>The state we're entering.</summary>
        public GameState NewState { get; }

        public GameStateChangedEvent(GameState previous, GameState newState)
        {
            PreviousState = previous;
            NewState = newState;
        }
    }

    /// <summary>
    /// Published when a new run begins.
    /// </summary>
    public class RunStartedEvent : GameEvent
    {
        /// <summary>The team of Requiems selected for this run.</summary>
        public IReadOnlyList<RequiemDataSO> SelectedTeam { get; }

        public RunStartedEvent(List<RequiemDataSO> selectedTeam)
        {
            SelectedTeam = selectedTeam?.AsReadOnly();
        }
    }

    /// <summary>
    /// Published when a team is selected for a new run.
    /// </summary>
    public class TeamSelectedEvent : GameEvent
    {
        /// <summary>The selected team of Requiems.</summary>
        public IReadOnlyList<RequiemDataSO> SelectedTeam { get; }

        public TeamSelectedEvent(RequiemDataSO[] team)
        {
            SelectedTeam = new List<RequiemDataSO>(team).AsReadOnly();
        }
    }

    /// <summary>
    /// Published when a run ends (victory or defeat).
    /// </summary>
    public class RunEndedEvent : GameEvent
    {
        /// <summary>True if the run was completed successfully.</summary>
        public bool Victory { get; }

        /// <summary>The final floor reached.</summary>
        public int FinalFloor { get; }

        /// <summary>Total enemies defeated during the run.</summary>
        public int EnemiesDefeated { get; }

        public RunEndedEvent(bool victory, int finalFloor, int enemiesDefeated)
        {
            Victory = victory;
            FinalFloor = finalFloor;
            EnemiesDefeated = enemiesDefeated;
        }
    }

    // ============================================
    // COMBAT EVENTS
    // ============================================

    /// <summary>
    /// Published when a combat encounter begins.
    /// </summary>
    public class CombatStartedEvent : GameEvent
    {
        /// <summary>The enemies in this encounter.</summary>
        public IReadOnlyList<EnemyInstance> Enemies { get; }

        public CombatStartedEvent(List<EnemyInstance> enemies)
        {
            Enemies = enemies?.AsReadOnly();
        }
    }

    /// <summary>
    /// Published when a combat encounter ends.
    /// </summary>
    public class CombatEndedEvent : GameEvent
    {
        /// <summary>True if the player won the combat.</summary>
        public bool Victory { get; }

        public CombatEndedEvent(bool victory)
        {
            Victory = victory;
        }
    }

    /// <summary>
    /// Published at the start of each turn.
    /// </summary>
    public class TurnStartedEvent : GameEvent
    {
        /// <summary>True if this is the player's turn.</summary>
        public bool IsPlayerTurn { get; }

        /// <summary>The current turn number (1-indexed).</summary>
        public int TurnNumber { get; }

        public TurnStartedEvent(bool isPlayerTurn, int turnNumber)
        {
            IsPlayerTurn = isPlayerTurn;
            TurnNumber = turnNumber;
        }
    }

    /// <summary>
    /// Published at the end of each turn.
    /// </summary>
    public class TurnEndedEvent : GameEvent
    {
        /// <summary>True if the ending turn was the player's.</summary>
        public bool WasPlayerTurn { get; }

        public TurnEndedEvent(bool wasPlayerTurn)
        {
            WasPlayerTurn = wasPlayerTurn;
        }
    }

    // ============================================
    // CARD EVENTS
    // ============================================

    /// <summary>
    /// Published when a card is drawn from the deck.
    /// </summary>
    public class CardDrawnEvent : GameEvent
    {
        /// <summary>The card that was drawn.</summary>
        public CardInstance Card { get; }

        public CardDrawnEvent(CardInstance card)
        {
            Card = card;
        }
    }

    /// <summary>
    /// Published when a card is played.
    /// </summary>
    public class CardPlayedEvent : GameEvent
    {
        /// <summary>The card that was played.</summary>
        public CardInstance Card { get; }

        /// <summary>The target of the card (may be null for untargeted cards).</summary>
        public ICombatTarget Target { get; }

        public CardPlayedEvent(CardInstance card, ICombatTarget target)
        {
            Card = card;
            Target = target;
        }
    }

    /// <summary>
    /// Published when a card is discarded.
    /// </summary>
    public class CardDiscardedEvent : GameEvent
    {
        /// <summary>The card that was discarded.</summary>
        public CardInstance Card { get; }

        /// <summary>True if the card was exhausted (removed from deck for this combat).</summary>
        public bool Exhausted { get; }

        public CardDiscardedEvent(CardInstance card, bool exhausted = false)
        {
            Card = card;
            Exhausted = exhausted;
        }
    }

    /// <summary>
    /// Published when a card is upgraded.
    /// </summary>
    public class CardUpgradedEvent : GameEvent
    {
        /// <summary>The card that was upgraded.</summary>
        public CardInstance Card { get; }

        public CardUpgradedEvent(CardInstance card)
        {
            Card = card;
        }
    }

    // ============================================
    // DAMAGE/HEALING EVENTS
    // ============================================

    /// <summary>
    /// Published when damage is dealt to a combatant.
    /// </summary>
    public class DamageDealtEvent : GameEvent
    {
        /// <summary>The source of the damage (may be null for environmental damage).</summary>
        public ICombatTarget Source { get; }

        /// <summary>The target receiving damage.</summary>
        public ICombatTarget Target { get; }

        /// <summary>The amount of damage dealt (after block reduction).</summary>
        public int Amount { get; }

        /// <summary>The amount of damage blocked.</summary>
        public int BlockedAmount { get; }

        /// <summary>True if this was a critical hit.</summary>
        public bool IsCritical { get; }

        public DamageDealtEvent(ICombatTarget source, ICombatTarget target, int amount, int blockedAmount, bool isCritical)
        {
            Source = source;
            Target = target;
            Amount = amount;
            BlockedAmount = blockedAmount;
            IsCritical = isCritical;
        }
    }

    /// <summary>
    /// Published when a combatant receives healing.
    /// </summary>
    public class HealingReceivedEvent : GameEvent
    {
        /// <summary>The combatant receiving healing.</summary>
        public ICombatTarget Target { get; }

        /// <summary>The amount healed.</summary>
        public int Amount { get; }

        public HealingReceivedEvent(ICombatTarget target, int amount)
        {
            Target = target;
            Amount = amount;
        }
    }

    /// <summary>
    /// Published when a combatant gains block.
    /// </summary>
    public class BlockGainedEvent : GameEvent
    {
        /// <summary>The combatant gaining block.</summary>
        public ICombatTarget Target { get; }

        /// <summary>The amount of block gained.</summary>
        public int Amount { get; }

        public BlockGainedEvent(ICombatTarget target, int amount)
        {
            Target = target;
            Amount = amount;
        }
    }

    // ============================================
    // CORRUPTION EVENTS
    // ============================================

    /// <summary>
    /// Published when a Requiem's corruption value changes.
    /// </summary>
    public class CorruptionChangedEvent : GameEvent
    {
        /// <summary>The Requiem whose corruption changed.</summary>
        public RequiemInstance Requiem { get; }

        /// <summary>The corruption value before the change.</summary>
        public int PreviousValue { get; }

        /// <summary>The corruption value after the change.</summary>
        public int NewValue { get; }

        /// <summary>The change in corruption (positive = gained, negative = lost).</summary>
        public int Delta => NewValue - PreviousValue;

        public CorruptionChangedEvent(RequiemInstance requiem, int previousValue, int newValue)
        {
            Requiem = requiem;
            PreviousValue = previousValue;
            NewValue = newValue;
        }
    }

    /// <summary>
    /// Published when a Requiem enters Null State (100 corruption).
    /// </summary>
    public class NullStateEnteredEvent : GameEvent
    {
        /// <summary>The Requiem entering Null State.</summary>
        public RequiemInstance Requiem { get; }

        public NullStateEnteredEvent(RequiemInstance requiem)
        {
            Requiem = requiem;
        }
    }

    /// <summary>
    /// Published when a Requiem exits Null State.
    /// </summary>
    public class NullStateExitedEvent : GameEvent
    {
        /// <summary>The Requiem exiting Null State.</summary>
        public RequiemInstance Requiem { get; }

        public NullStateExitedEvent(RequiemInstance requiem)
        {
            Requiem = requiem;
        }
    }

    // ============================================
    // PROGRESSION/SHOP EVENTS
    // ============================================
    // See ShopEvents.cs for:
    // - VoidShardsChangedEvent
    // - ShopOpenedEvent, ShopClosedEvent, ShopItemPurchasedEvent
    // - CardAddedToDeckEvent, CardRemovedFromDeckEvent
    // - RelicAcquiredEvent
}
