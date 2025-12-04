// ============================================
// GameEvents.cs
// All game event definitions for EventBus
// ============================================

using System.Collections.Generic;
using HNR.Core.Interfaces;
using HNR.Cards;

namespace HNR.Core.Events
{
    // ============================================
    // FORWARD DECLARATIONS - Placeholder Types
    // TODO: Move to proper locations when implemented
    // ============================================

    /// <summary>
    /// Placeholder: ScriptableObject data for a Requiem character.
    /// TODO: Implement in Scripts/Characters/Data/RequiemDataSO.cs
    /// </summary>
    public class RequiemDataSO : UnityEngine.ScriptableObject { }

    /// <summary>
    /// Placeholder: Runtime instance of a Requiem character.
    /// TODO: Implement in Scripts/Characters/RequiemInstance.cs
    /// </summary>
    public class RequiemInstance
    {
        public HNR.Characters.RequiemDataSO Data { get; set; }
        public int MaxHP { get; set; } = 100;
        public int CurrentHP { get; set; } = 100;
        public bool IsDead => CurrentHP <= 0;
    }

    /// <summary>
    /// Placeholder: Runtime instance of an enemy.
    /// TODO: Implement in Scripts/Enemies/EnemyInstance.cs
    /// </summary>
    public class EnemyInstance
    {
        public HNR.Combat.EnemyDataSO Data { get; set; }
        public string Name => Data?.EnemyName ?? "Enemy";
        public int CurrentHP { get; set; }
        public int Block { get; set; }
        public bool IsDead => CurrentHP <= 0;

        private HNR.Combat.IntentPattern _intentPattern;
        private int _intentIndex;

        public void Initialize()
        {
            if (Data != null)
            {
                CurrentHP = Data.BaseHP;
                _intentPattern = Data.IntentPattern?.Clone();
            }
        }

        public HNR.Combat.IntentStep GetCurrentIntent()
        {
            return _intentPattern?.GetCurrentIntent();
        }

        public void AdvanceIntent()
        {
            _intentPattern?.AdvanceIntent();
        }

        public void GainBlock(int amount)
        {
            Block += amount;
        }

        public void TakeDamage(int amount)
        {
            int blocked = UnityEngine.Mathf.Min(amount, Block);
            Block -= blocked;
            CurrentHP -= (amount - blocked);
        }
    }

    // CardInstance is now implemented in HNR.Cards.CardInstance

    /// <summary>
    /// Placeholder: Interface for anything that can be targeted.
    /// TODO: Implement in Scripts/Combat/ITargetable.cs
    /// </summary>
    public interface ITargetable { }

    /// <summary>
    /// Placeholder: Interface for combat participants (Requiems and Enemies).
    /// TODO: Implement in Scripts/Combat/ICombatant.cs
    /// </summary>
    public interface ICombatant : ITargetable
    {
        int CurrentHP { get; }
        int MaxHP { get; }
        int Block { get; }
    }

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
        public IReadOnlyList<HNR.Combat.EnemyInstance> Enemies { get; }

        public CombatStartedEvent(List<HNR.Combat.EnemyInstance> enemies)
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
        public ITargetable Target { get; }

        public CardPlayedEvent(CardInstance card, ITargetable target)
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

    // ============================================
    // DAMAGE/HEALING EVENTS
    // ============================================

    /// <summary>
    /// Published when damage is dealt to a combatant.
    /// </summary>
    public class DamageDealtEvent : GameEvent
    {
        /// <summary>The source of the damage.</summary>
        public ICombatant Source { get; }

        /// <summary>The target receiving damage.</summary>
        public ICombatant Target { get; }

        /// <summary>The amount of damage dealt (after block reduction).</summary>
        public int Amount { get; }

        /// <summary>The amount of damage blocked.</summary>
        public int BlockedAmount { get; }

        /// <summary>True if this was a critical hit.</summary>
        public bool IsCritical { get; }

        public DamageDealtEvent(ICombatant source, ICombatant target, int amount, int blockedAmount, bool isCritical)
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
        public ICombatant Target { get; }

        /// <summary>The amount healed.</summary>
        public int Amount { get; }

        public HealingReceivedEvent(ICombatant target, int amount)
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
        public ICombatant Target { get; }

        /// <summary>The amount of block gained.</summary>
        public int Amount { get; }

        public BlockGainedEvent(ICombatant target, int amount)
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
    // PROGRESSION EVENTS
    // ============================================

    /// <summary>
    /// Published when Void Shards (currency) amount changes.
    /// </summary>
    public class VoidShardsChangedEvent : GameEvent
    {
        /// <summary>Amount before the change.</summary>
        public int OldAmount { get; }

        /// <summary>Amount after the change.</summary>
        public int NewAmount { get; }

        /// <summary>The change in shards (positive = gained, negative = spent).</summary>
        public int Delta => NewAmount - OldAmount;

        public VoidShardsChangedEvent(int oldAmount, int newAmount)
        {
            OldAmount = oldAmount;
            NewAmount = newAmount;
        }
    }

    /// <summary>
    /// Published when a relic is acquired.
    /// </summary>
    public class RelicAcquiredEvent : GameEvent
    {
        /// <summary>ID of the acquired relic.</summary>
        public string RelicId { get; }

        public RelicAcquiredEvent(string relicId)
        {
            RelicId = relicId;
        }
    }

    /// <summary>
    /// Published when a card is added to the deck.
    /// </summary>
    public class CardAddedToDeckEvent : GameEvent
    {
        /// <summary>ID of the card added.</summary>
        public string CardId { get; }

        public CardAddedToDeckEvent(string cardId)
        {
            CardId = cardId;
        }
    }

    /// <summary>
    /// Published when a card is removed from the deck.
    /// </summary>
    public class CardRemovedFromDeckEvent : GameEvent
    {
        /// <summary>ID of the card removed.</summary>
        public string CardId { get; }

        public CardRemovedFromDeckEvent(string cardId)
        {
            CardId = cardId;
        }
    }
}
