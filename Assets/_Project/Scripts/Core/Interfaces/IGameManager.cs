// ============================================
// IGameManager.cs
// Core game state management interface
// ============================================

namespace HNR.Core.Interfaces
{
    /// <summary>
    /// Core game state management service.
    /// Controls high-level game flow and state transitions.
    /// </summary>
    /// <remarks>
    /// Register with ServiceLocator at startup.
    /// Implementation: GameManager (MonoBehaviour)
    /// </remarks>
    public interface IGameManager
    {
        /// <summary>
        /// Gets the current high-level game state.
        /// </summary>
        GameState CurrentState { get; }

        /// <summary>
        /// Transition to a new game state.
        /// Handles exit of current state and entry of new state.
        /// </summary>
        /// <param name="newState">The state to transition to</param>
        void ChangeState(GameState newState);

        /// <summary>
        /// Begin a new run with the currently selected team.
        /// Initializes run state and transitions to Run state.
        /// </summary>
        void StartNewRun();

        /// <summary>
        /// End the current run.
        /// Cleans up run state and transitions to Results state.
        /// </summary>
        /// <param name="victory">True if the run was completed successfully</param>
        void EndRun(bool victory);
    }
}
