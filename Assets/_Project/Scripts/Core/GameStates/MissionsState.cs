// ============================================
// MissionsState.cs
// Missions selection state - Story/Battle Mission choice
// ============================================

using UnityEngine;
using UnityEngine.SceneManagement;
using HNR.Core.Interfaces;
using HNR.UI.Screens;

namespace HNR.Core.GameStates
{
    /// <summary>
    /// Missions state handles the mission type selection screen.
    /// Players choose between Story Mission (placeholder) and Battle Mission.
    /// </summary>
    public class MissionsState : IGameState
    {
        private readonly GameManager _manager;

        /// <summary>
        /// Creates a new MissionsState.
        /// </summary>
        /// <param name="manager">Reference to the GameManager</param>
        public MissionsState(GameManager manager)
        {
            _manager = manager;
        }

        /// <summary>
        /// Load Missions scene, show UI.
        /// </summary>
        public void Enter()
        {
            Debug.Log("[MissionsState] Entering Missions screen...");

            // Subscribe to scene loaded to show UI after scene is ready
            SceneManager.sceneLoaded += OnMissionsSceneLoaded;

            // Load the Missions scene
            LoadMissionsScene();
        }

        /// <summary>
        /// Called when Missions scene finishes loading.
        /// </summary>
        private void OnMissionsSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (scene.name != "Missions") return;

            // Unsubscribe to prevent multiple calls
            SceneManager.sceneLoaded -= OnMissionsSceneLoaded;

            Debug.Log("[MissionsState] Missions scene loaded. Showing UI...");

            // Show Missions UI now that scene is loaded
            ShowMissionsScreen();

            Debug.Log("[MissionsState] Missions ready.");
        }

        /// <summary>
        /// Per-frame update (not typically used in Missions state).
        /// </summary>
        public void Update()
        {
            // Missions interactions are handled by UI events, not Update()
        }

        /// <summary>
        /// Cleanup when leaving the Missions screen.
        /// </summary>
        public void Exit()
        {
            Debug.Log("[MissionsState] Leaving Missions...");

            // Unsubscribe from scene loaded in case we exit early
            SceneManager.sceneLoaded -= OnMissionsSceneLoaded;

            // Hide Missions screen
            HideMissionsScreen();
        }

        // ============================================
        // Private Methods
        // ============================================

        /// <summary>
        /// Load the Missions scene.
        /// </summary>
        private void LoadMissionsScene()
        {
            Debug.Log("[MissionsState] Loading Missions scene...");

            // Only load if not already in Missions scene
            if (SceneManager.GetActiveScene().name != "Missions")
            {
                SceneManager.LoadScene("Missions", LoadSceneMode.Single);
            }
            else
            {
                // Already in scene, show UI immediately
                ShowMissionsScreen();
            }
        }

        /// <summary>
        /// Show the Missions UI screen.
        /// </summary>
        private void ShowMissionsScreen()
        {
            Debug.Log("[MissionsState] Showing MissionsScreen...");

            if (ServiceLocator.TryGet<IUIManager>(out var uiManager))
            {
                uiManager.ShowScreen<MissionsScreen>();
            }
            else
            {
                Debug.LogWarning("[MissionsState] UIManager not available - cannot show MissionsScreen");
            }
        }

        /// <summary>
        /// Hide the Missions UI screen.
        /// </summary>
        private void HideMissionsScreen()
        {
            Debug.Log("[MissionsState] Hiding MissionsScreen...");
            // Screen is automatically hidden when another screen is shown via UIManager.ShowScreen
        }
    }
}
