// ============================================
// GameState.cs
// High-level game state definitions
// ============================================

namespace HNR.Core
{
    /// <summary>
    /// High-level game states representing major game modes.
    /// Used by GameManager for state machine transitions.
    /// </summary>
    public enum GameState
    {
        /// <summary>
        /// Initial loading state. Initializes core systems.
        /// </summary>
        Boot,

        /// <summary>
        /// Title screen and main menu.
        /// </summary>
        MainMenu,

        /// <summary>
        /// Hub area where player prepares for runs.
        /// </summary>
        Bastion,

        /// <summary>
        /// Active run in progress. Player navigates the Null Rift map.
        /// </summary>
        Run,

        /// <summary>
        /// Active combat encounter.
        /// </summary>
        Combat,

        /// <summary>
        /// Post-run results screen showing statistics.
        /// </summary>
        Results
    }
}
