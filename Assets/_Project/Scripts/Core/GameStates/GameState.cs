// ============================================
// GameState.cs
// High-level game state enumeration
// ============================================

namespace HNR.Core
{
    /// <summary>
    /// High-level game states representing major game modes.
    /// Used by GameManager for state machine transitions.
    /// </summary>
    /// <remarks>
    /// State flow: Boot → MainMenu → Bastion → Run ↔ Combat → Results → MainMenu
    /// </remarks>
    public enum GameState
    {
        /// <summary>
        /// Initial loading state. Initializes core systems and services.
        /// Transitions to MainMenu after initialization completes.
        /// </summary>
        Boot = 0,

        /// <summary>
        /// Title screen and main menu.
        /// Player can start new run, continue saved run, or access settings.
        /// </summary>
        MainMenu = 1,

        /// <summary>
        /// Hub area (The Bastion) where player prepares for runs.
        /// Select Requiems, view unlocks, manage meta-progression.
        /// </summary>
        Bastion = 2,

        /// <summary>
        /// Active run in progress. Player navigates the Null Rift map.
        /// Choose paths, encounter events, enter combat nodes.
        /// </summary>
        Run = 3,

        /// <summary>
        /// Active combat encounter against enemies.
        /// Turn-based card combat with Requiem team.
        /// </summary>
        Combat = 4,

        /// <summary>
        /// Post-run results screen showing statistics.
        /// Displays victory/defeat, stats, and rewards.
        /// </summary>
        Results = 5
    }
}
