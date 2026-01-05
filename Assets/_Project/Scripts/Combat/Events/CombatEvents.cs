// ============================================
// CombatEvents.cs
// Combat-specific events for EventBus
// ============================================

using UnityEngine;
using HNR.Core.Events;
using HNR.Characters;

// EnemyInstance is now defined in HNR.Combat.EnemyInstance

namespace HNR.Combat
{
    // ============================================
    // RESOURCE EVENTS
    // ============================================

    /// <summary>
    /// Published when Action Points change during combat.
    /// </summary>
    public class APChangedEvent : GameEvent
    {
        /// <summary>Current AP available.</summary>
        public int CurrentAP { get; }

        /// <summary>Maximum AP for this turn.</summary>
        public int MaxAP { get; }

        public APChangedEvent(int currentAP, int maxAP)
        {
            CurrentAP = currentAP;
            MaxAP = maxAP;
        }
    }

    /// <summary>
    /// Published when team HP changes (damage or healing).
    /// </summary>
    public class TeamHPChangedEvent : GameEvent
    {
        /// <summary>Current team HP.</summary>
        public int CurrentHP { get; }

        /// <summary>Maximum team HP.</summary>
        public int MaxHP { get; }

        /// <summary>The change in HP (negative = damage, positive = healing).</summary>
        public int Delta { get; }

        /// <summary>World position of the affected Requiem (for damage number spawning).</summary>
        public Vector3? TargetPosition { get; }

        public TeamHPChangedEvent(int currentHP, int maxHP, int delta = 0, Vector3? targetPosition = null)
        {
            CurrentHP = currentHP;
            MaxHP = maxHP;
            Delta = delta;
            TargetPosition = targetPosition;
        }
    }

    /// <summary>
    /// Published when team Block value changes.
    /// </summary>
    public class BlockChangedEvent : GameEvent
    {
        /// <summary>Current Block value.</summary>
        public int Block { get; }

        /// <summary>Previous Block value.</summary>
        public int PreviousBlock { get; }

        public BlockChangedEvent(int block, int previousBlock = 0)
        {
            Block = block;
            PreviousBlock = previousBlock;
        }
    }

    // ============================================
    // SOUL ESSENCE EVENTS
    // ============================================

    /// <summary>
    /// Published when Soul Essence resource changes.
    /// </summary>
    public class SoulEssenceChangedEvent : GameEvent
    {
        /// <summary>Current Soul Essence amount.</summary>
        public int Current { get; }

        /// <summary>The change in Soul Essence.</summary>
        public int Delta { get; }

        public SoulEssenceChangedEvent(int current, int delta)
        {
            Current = current;
            Delta = delta;
        }
    }

    // ============================================
    // COMBAT PHASE EVENTS
    // ============================================

    /// <summary>
    /// Published when combat phase changes.
    /// </summary>
    public class CombatPhaseChangedEvent : GameEvent
    {
        /// <summary>The phase we're leaving.</summary>
        public CombatPhase PreviousPhase { get; }

        /// <summary>The phase we're entering.</summary>
        public CombatPhase NewPhase { get; }

        public CombatPhaseChangedEvent(CombatPhase previous, CombatPhase newPhase)
        {
            PreviousPhase = previous;
            NewPhase = newPhase;
        }
    }

    // ============================================
    // ENEMY EVENTS
    // ============================================

    /// <summary>
    /// Published when an enemy takes damage.
    /// </summary>
    public class EnemyDamagedEvent : GameEvent
    {
        /// <summary>The enemy that took damage.</summary>
        public EnemyInstance Enemy { get; }

        /// <summary>Amount of damage dealt (after block).</summary>
        public int Damage { get; }

        /// <summary>Amount of damage blocked.</summary>
        public int Blocked { get; }

        public EnemyDamagedEvent(EnemyInstance enemy, int damage, int blocked = 0)
        {
            Enemy = enemy;
            Damage = damage;
            Blocked = blocked;
        }
    }

    /// <summary>
    /// Published when an enemy is defeated.
    /// </summary>
    public class EnemyDefeatedEvent : GameEvent
    {
        /// <summary>The defeated enemy instance.</summary>
        public EnemyInstance Enemy { get; }

        public EnemyDefeatedEvent(EnemyInstance enemy)
        {
            Enemy = enemy;
        }
    }

    /// <summary>
    /// Published when an enemy executes its intent.
    /// </summary>
    public class EnemyIntentExecutedEvent : GameEvent
    {
        /// <summary>The enemy that acted.</summary>
        public EnemyInstance Enemy { get; }

        /// <summary>The intent that was executed.</summary>
        public IntentStep Intent { get; }

        public EnemyIntentExecutedEvent(EnemyInstance enemy, IntentStep intent)
        {
            Enemy = enemy;
            Intent = intent;
        }
    }

    /// <summary>
    /// Published when an enemy's displayed intent changes.
    /// </summary>
    public class EnemyIntentChangedEvent : GameEvent
    {
        /// <summary>The enemy whose intent changed.</summary>
        public EnemyInstance Enemy { get; }

        /// <summary>The new intent to display.</summary>
        public IntentStep Intent { get; }

        public EnemyIntentChangedEvent(EnemyInstance enemy, IntentStep intent)
        {
            Enemy = enemy;
            Intent = intent;
        }
    }

    // ============================================
    // STATUS EFFECT EVENTS
    // ============================================

    /// <summary>
    /// Published when a status effect is applied to a target.
    /// </summary>
    public class StatusAppliedEvent : GameEvent
    {
        /// <summary>The target receiving the status.</summary>
        public ICombatTarget Target { get; }

        /// <summary>The type of status applied.</summary>
        public StatusType StatusType { get; }

        /// <summary>Number of stacks applied.</summary>
        public int Stacks { get; }

        /// <summary>Duration in turns (0 = permanent until removed).</summary>
        public int Duration { get; }

        public StatusAppliedEvent(ICombatTarget target, StatusType statusType, int stacks, int duration = 0)
        {
            Target = target;
            StatusType = statusType;
            Stacks = stacks;
            Duration = duration;
        }
    }

    /// <summary>
    /// Published when a status effect is removed from a target.
    /// </summary>
    public class StatusRemovedEvent : GameEvent
    {
        /// <summary>The target losing the status.</summary>
        public ICombatTarget Target { get; }

        /// <summary>The type of status removed.</summary>
        public StatusType StatusType { get; }

        public StatusRemovedEvent(ICombatTarget target, StatusType statusType)
        {
            Target = target;
            StatusType = statusType;
        }
    }

    /// <summary>
    /// Published when a status effect ticks (deals damage, heals, etc.).
    /// </summary>
    public class StatusTickedEvent : GameEvent
    {
        /// <summary>The target affected.</summary>
        public ICombatTarget Target { get; }

        /// <summary>The status that ticked.</summary>
        public StatusType StatusType { get; }

        /// <summary>Value of the tick effect (damage, heal, etc.).</summary>
        public int Value { get; }

        public StatusTickedEvent(ICombatTarget target, StatusType statusType, int value)
        {
            Target = target;
            StatusType = statusType;
            Value = value;
        }
    }

    // ============================================
    // TARGETING EVENTS
    // ============================================

    /// <summary>
    /// Published when a card target is confirmed.
    /// </summary>
    public class CardTargetConfirmedEvent : GameEvent
    {
        /// <summary>The confirmed target.</summary>
        public ICombatTarget Target { get; }

        public CardTargetConfirmedEvent(ICombatTarget target)
        {
            Target = target;
        }
    }

    /// <summary>
    /// Published when card targeting is cancelled.
    /// </summary>
    public class CardTargetCancelledEvent : GameEvent
    {
        public CardTargetCancelledEvent() { }
    }

    // ============================================
    // REQUIEM ART EVENTS
    // ============================================

    /// <summary>
    /// Published when a Requiem activates their special Art ability.
    /// </summary>
    public class RequiemArtActivatedEvent : GameEvent
    {
        /// <summary>The Requiem who activated the Art.</summary>
        public RequiemInstance Requiem { get; }

        /// <summary>The Art that was activated.</summary>
        public RequiemArtDataSO Art { get; }

        public RequiemArtActivatedEvent(RequiemInstance requiem, RequiemArtDataSO art)
        {
            Requiem = requiem;
            Art = art;
        }
    }

    // ============================================
    // UI REQUEST EVENTS
    // ============================================

    /// <summary>
    /// Published when the player requests to end their turn via UI.
    /// </summary>
    public class EndTurnRequestedEvent : GameEvent
    {
        public EndTurnRequestedEvent() { }
    }

    /// <summary>
    /// Published when game speed is changed via UI.
    /// </summary>
    public class GameSpeedChangedEvent : GameEvent
    {
        /// <summary>New game speed multiplier (1x, 1.5x, 2x).</summary>
        public float Speed { get; }

        public GameSpeedChangedEvent(float speed)
        {
            Speed = speed;
        }
    }

    /// <summary>
    /// Published when auto-battle mode is toggled.
    /// </summary>
    public class AutoBattleToggledEvent : GameEvent
    {
        /// <summary>True if auto-battle is now enabled.</summary>
        public bool Enabled { get; }

        public AutoBattleToggledEvent(bool enabled)
        {
            Enabled = enabled;
        }
    }

    /// <summary>
    /// Published when settings screen is requested from combat.
    /// </summary>
    public class OpenSettingsRequestEvent : GameEvent
    {
        public OpenSettingsRequestEvent() { }
    }

    /// <summary>
    /// Published when pause menu is requested from combat.
    /// </summary>
    public class OpenPauseMenuRequestEvent : GameEvent
    {
        public OpenPauseMenuRequestEvent() { }
    }

    /// <summary>
    /// Published when settings overlay is closed.
    /// </summary>
    public class SettingsClosedEvent : GameEvent
    {
        public SettingsClosedEvent() { }
    }
}
