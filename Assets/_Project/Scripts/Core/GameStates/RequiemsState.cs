// ============================================
// RequiemsState.cs
// Requiems viewer state - Character roster
// ============================================

using UnityEngine;
using UnityEngine.SceneManagement;
using HNR.Core.Interfaces;
using HNR.UI.Screens;

namespace HNR.Core.GameStates
{
    /// <summary>
    /// Requiems state handles the character roster/viewer screen.
    /// Players can view Requiem details, stats, and cards.
    /// </summary>
    public class RequiemsState : IGameState
    {
        private readonly GameManager _manager;

        /// <summary>
        /// Creates a new RequiemsState.
        /// </summary>
        /// <param name="manager">Reference to the GameManager</param>
        public RequiemsState(GameManager manager)
        {
            _manager = manager;
        }

        /// <summary>
        /// Load Requiems scene, show UI.
        /// </summary>
        public void Enter()
        {
            Debug.Log("[RequiemsState] Entering Requiems viewer...");

            // Subscribe to scene loaded to show UI after scene is ready
            SceneManager.sceneLoaded += OnRequiemsSceneLoaded;

            // Load the Requiems scene
            LoadRequiemsScene();
        }

        /// <summary>
        /// Called when Requiems scene finishes loading.
        /// </summary>
        private void OnRequiemsSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (scene.name != "Requiems") return;

            // Unsubscribe to prevent multiple calls
            SceneManager.sceneLoaded -= OnRequiemsSceneLoaded;

            Debug.Log("[RequiemsState] Requiems scene loaded. Showing UI...");

            // Show Requiems UI now that scene is loaded
            ShowRequiemsScreen();

            Debug.Log("[RequiemsState] Requiems viewer ready.");
        }

        /// <summary>
        /// Per-frame update (not typically used in Requiems state).
        /// </summary>
        public void Update()
        {
            // Requiems interactions are handled by UI events, not Update()
        }

        /// <summary>
        /// Cleanup when leaving the Requiems screen.
        /// </summary>
        public void Exit()
        {
            Debug.Log("[RequiemsState] Leaving Requiems viewer...");

            // Unsubscribe from scene loaded in case we exit early
            SceneManager.sceneLoaded -= OnRequiemsSceneLoaded;

            // Hide Requiems screen
            HideRequiemsScreen();
        }

        // ============================================
        // Private Methods
        // ============================================

        /// <summary>
        /// Load the Requiems scene.
        /// </summary>
        private void LoadRequiemsScene()
        {
            Debug.Log("[RequiemsState] Loading Requiems scene...");

            // Only load if not already in Requiems scene
            if (SceneManager.GetActiveScene().name != "Requiems")
            {
                SceneManager.LoadScene("Requiems", LoadSceneMode.Single);
            }
            else
            {
                // Already in scene, show UI immediately
                ShowRequiemsScreen();
            }
        }

        /// <summary>
        /// Show the Requiems UI screen.
        /// </summary>
        private void ShowRequiemsScreen()
        {
            Debug.Log("[RequiemsState] Showing RequiemsListScreen...");

            if (ServiceLocator.TryGet<IUIManager>(out var uiManager))
            {
                uiManager.ShowScreen<RequiemsListScreen>();
            }
            else
            {
                Debug.LogWarning("[RequiemsState] UIManager not available - cannot show RequiemsListScreen");
            }
        }

        /// <summary>
        /// Hide the Requiems UI screen.
        /// </summary>
        private void HideRequiemsScreen()
        {
            Debug.Log("[RequiemsState] Hiding RequiemsListScreen...");
            // Screen is automatically hidden when another screen is shown via UIManager.ShowScreen
        }
    }
}
