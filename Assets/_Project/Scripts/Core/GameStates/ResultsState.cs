// ============================================
// ResultsState.cs
// Results state - post-run statistics and rewards
// ============================================

using UnityEngine;
using HNR.Core.Interfaces;
using HNR.UI;

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

            // Show ResultsScreen via UIManager
            if (ServiceLocator.TryGet<IUIManager>(out var uiManager))
            {
                uiManager.ShowScreen<ResultsScreen>();
            }
            else
            {
                Debug.LogWarning("[ResultsState] UIManager not available - cannot show ResultsScreen");
            }
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

            // Screen is automatically hidden when another screen is shown via UIManager.ShowScreen
            // No explicit hide needed - the next state will show its own screen

            // Clear saved run data after viewing results
            if (ServiceLocator.TryGet<ISaveManager>(out var saveManager))
            {
                saveManager.DeleteRun();
                Debug.Log("[ResultsState] Cleared run save data");
            }
        }
    }
}
