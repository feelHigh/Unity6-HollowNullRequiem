// ============================================
// CombatEnums.cs
// Combat system enumerations
// ============================================

namespace HNR.Combat
{
    /// <summary>
    /// Phases of a combat encounter.
    /// </summary>
    public enum CombatPhase
    {
        /// <summary>Initialize combat, apply start-of-combat effects.</summary>
        Setup,

        /// <summary>Player draws cards for the turn.</summary>
        DrawPhase,

        /// <summary>Player plays cards and uses abilities.</summary>
        PlayerPhase,

        /// <summary>End of player turn - discard hand, tick effects.</summary>
        EndPhase,

        /// <summary>Enemies execute their telegraphed intents.</summary>
        EnemyPhase,

        /// <summary>All enemies defeated - show rewards.</summary>
        Victory,

        /// <summary>Team HP reached 0 - run ends.</summary>
        Defeat
    }
}
