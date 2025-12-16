// ============================================
// RunState.cs
// Run state - active run navigation
// ============================================

using UnityEngine;
using UnityEngine.SceneManagement;
using HNR.Core.Interfaces;
using HNR.Map;
using HNR.UI;

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

            // Subscribe to scene loaded event before loading
            SceneManager.sceneLoaded += OnNullRiftSceneLoaded;

            // Load NullRift scene
            if (SceneManager.GetActiveScene().name != "NullRift")
            {
                SceneManager.LoadScene("NullRift", LoadSceneMode.Single);
            }
            else
            {
                // Already in NullRift, show UI directly
                OnNullRiftSceneLoaded(SceneManager.GetActiveScene(), LoadSceneMode.Single);
            }
        }

        private void OnNullRiftSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (scene.name != "NullRift") return;

            // Unsubscribe immediately
            SceneManager.sceneLoaded -= OnNullRiftSceneLoaded;

            Debug.Log("[RunState] NullRift scene loaded, initializing map...");

            // Generate map if needed
            var mapManager = ServiceLocator.Get<MapManager>();
            if (mapManager != null)
            {
                if (!mapManager.HasActiveMap)
                {
                    // Get current zone from RunManager
                    var runManager = ServiceLocator.Get<IRunManager>();
                    int zone = runManager?.CurrentZone ?? 1;
                    int seed = runManager?.RunSeed ?? -1;

                    mapManager.GenerateMap(zone, seed);
                    Debug.Log($"[RunState] Generated map for zone {zone}");
                }
            }
            else
            {
                Debug.LogWarning("[RunState] MapManager not found in scene");
            }

            // Show MapScreen
            ShowMapScreen();
        }

        private void ShowMapScreen()
        {
            if (ServiceLocator.TryGet<IUIManager>(out var uiManager))
            {
                uiManager.ShowScreen<MapScreen>();
                Debug.Log("[RunState] MapScreen shown");
            }
            else
            {
                Debug.LogWarning("[RunState] UIManager not available - cannot show MapScreen");
            }
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

            // Ensure we're unsubscribed
            SceneManager.sceneLoaded -= OnNullRiftSceneLoaded;

            // Save run state
            var runManager = ServiceLocator.Get<IRunManager>();
            if (runManager?.IsRunActive == true)
            {
                runManager.SaveRun();
            }
        }
    }
}
