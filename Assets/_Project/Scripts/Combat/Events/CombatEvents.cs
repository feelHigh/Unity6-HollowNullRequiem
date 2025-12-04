// ============================================
// CombatEvents.cs
// Combat-specific events for EventBus
// ============================================

using HNR.Core.Events;

// EnemyInstance is defined in HNR.Core.Events (placeholder)

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

        public TeamHPChangedEvent(int currentHP, int maxHP, int delta = 0)
        {
            CurrentHP = currentHP;
            MaxHP = maxHP;
            Delta = delta;
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
}
