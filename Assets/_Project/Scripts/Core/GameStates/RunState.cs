// ============================================
// RunState.cs
// Run state - active run navigation
// ============================================

using UnityEngine;
using UnityEngine.SceneManagement;
using HNR.Core.Interfaces;

namespace HNR.Core.GameStates
{
    /// <summary>
    /// Run state handles active run navigation through the Null Rift map.
    /// Players choose paths, encounter events, and enter combat nodes.
    /// </summary>
    public class RunState : IGameState
    {
        private readonly GameManager _manager;

        /// <summary>
        /// Creates a new RunState.
        /// </summary>
        /// <param name="manager">Reference to the GameManager</param>
        public RunState(GameManager manager)
        {
            _manager = manager;
        }

        /// <summary>
        /// Load NullRift scene, show map UI, and play music.
        /// </summary>
        public void Enter()
        {
            Debug.Log("[RunState] Starting run navigation...");

            // TODO: Load NullRift scene
            // SceneManager.LoadScene("NullRift", LoadSceneMode.Single);

            // TODO: Show MapScreen via UIManager
            // TODO: Generate or load map
            // TODO: Play zone music
        }

        /// <summary>
        /// Per-frame update for run state.
        /// </summary>
        public void Update()
        {
            // Map interactions handled by UI events
        }

        /// <summary>
        /// Cleanup when leaving run navigation.
        /// </summary>
        public void Exit()
        {
            Debug.Log("[RunState] Exiting run navigation...");

            // TODO: Save run state
        }
    }
}
