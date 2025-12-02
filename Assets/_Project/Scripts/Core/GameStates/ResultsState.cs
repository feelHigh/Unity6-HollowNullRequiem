// ============================================
// ResultsState.cs
// Results state - post-run statistics and rewards
// ============================================

using UnityEngine;
using HNR.Core.Interfaces;

namespace HNR.Core.GameStates
{
    /// <summary>
    /// Results state displays post-run statistics and rewards.
    /// Shows victory/defeat status, stats, and meta-progression gains.
    /// </summary>
    public class ResultsState : IGameState
    {
        private readonly GameManager _manager;

        /// <summary>
        /// Creates a new ResultsState.
        /// </summary>
        /// <param name="manager">Reference to the GameManager</param>
        public ResultsState(GameManager manager)
        {
            _manager = manager;
        }

        /// <summary>
        /// Show results screen with run statistics.
        /// </summary>
        public void Enter()
        {
            Debug.Log("[ResultsState] Showing run results...");

            // TODO: Show ResultsScreen via UIManager
            // TODO: Display run statistics
            // TODO: Show rewards and unlocks
            // TODO: Clear saved run data via SaveManager
        }

        /// <summary>
        /// Per-frame update for results state.
        /// </summary>
        public void Update()
        {
            // Results interactions handled by UI events
        }

        /// <summary>
        /// Cleanup when leaving results.
        /// </summary>
        public void Exit()
        {
            Debug.Log("[ResultsState] Exiting results...");

            // TODO: Apply meta-progression rewards
        }
    }
}
