// ============================================
// BattleMissionState.cs
// Battle Mission state - Zone/difficulty selection
// ============================================

using UnityEngine;
using UnityEngine.SceneManagement;
using HNR.Core.Interfaces;
using HNR.UI.Screens;
using HNR.Progression;

namespace HNR.Core.GameStates
{
    /// <summary>
    /// Battle Mission state handles zone and difficulty selection.
    /// Players choose which zone to enter and at what difficulty level.
    /// </summary>
    public class BattleMissionState : IGameState
    {
        private readonly GameManager _manager;

        /// <summary>
        /// Creates a new BattleMissionState.
        /// </summary>
        /// <param name="manager">Reference to the GameManager</param>
        public BattleMissionState(GameManager manager)
        {
            _manager = manager;
        }

        /// <summary>
        /// Load BattleMission scene, show UI, initialize progress manager.
        /// </summary>
        public void Enter()
        {
            Debug.Log("[BattleMissionState] Entering Battle Mission screen...");

            // Ensure progress manager is initialized
            InitializeProgressManager();

            // Subscribe to scene loaded to show UI after scene is ready
            SceneManager.sceneLoaded += OnBattleMissionSceneLoaded;

            // Load the BattleMission scene
            LoadBattleMissionScene();
        }

        /// <summary>
        /// Called when BattleMission scene finishes loading.
        /// </summary>
        private void OnBattleMissionSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (scene.name != "BattleMission") return;

            // Unsubscribe to prevent multiple calls
            SceneManager.sceneLoaded -= OnBattleMissionSceneLoaded;

            Debug.Log("[BattleMissionState] BattleMission scene loaded. Showing UI...");

            // Show BattleMission UI now that scene is loaded
            ShowBattleMissionScreen();

            Debug.Log("[BattleMissionState] BattleMission ready.");
        }

        /// <summary>
        /// Per-frame update (not typically used in BattleMission state).
        /// </summary>
        public void Update()
        {
            // BattleMission interactions are handled by UI events, not Update()
        }

        /// <summary>
        /// Cleanup when leaving the BattleMission screen.
        /// </summary>
        public void Exit()
        {
            Debug.Log("[BattleMissionState] Leaving BattleMission...");

            // Unsubscribe from scene loaded in case we exit early
            SceneManager.sceneLoaded -= OnBattleMissionSceneLoaded;

            // Hide BattleMission screen
            HideBattleMissionScreen();
        }

        // ============================================
        // Private Methods
        // ============================================

        /// <summary>
        /// Initialize the BattleMissionProgressManager if not already present.
        /// </summary>
        private void InitializeProgressManager()
        {
            if (BattleMissionProgressManager.Instance == null)
            {
                // Create progress manager if it doesn't exist
                var progressManagerObj = new GameObject("BattleMissionProgressManager");
                progressManagerObj.AddComponent<BattleMissionProgressManager>();
                Object.DontDestroyOnLoad(progressManagerObj);
                Debug.Log("[BattleMissionState] Created BattleMissionProgressManager");
            }
            else
            {
                // Refresh progress data
                BattleMissionProgressManager.Instance.LoadProgress();
            }
        }

        /// <summary>
        /// Load the BattleMission scene.
        /// </summary>
        private void LoadBattleMissionScene()
        {
            Debug.Log("[BattleMissionState] Loading BattleMission scene...");

            // Only load if not already in BattleMission scene
            if (SceneManager.GetActiveScene().name != "BattleMission")
            {
                SceneManager.LoadScene("BattleMission", LoadSceneMode.Single);
            }
            else
            {
                // Already in scene, show UI immediately
                ShowBattleMissionScreen();
            }
        }

        /// <summary>
        /// Show the BattleMission UI screen.
        /// </summary>
        private void ShowBattleMissionScreen()
        {
            Debug.Log("[BattleMissionState] Showing BattleMissionScreen...");

            if (ServiceLocator.TryGet<IUIManager>(out var uiManager))
            {
                uiManager.ShowScreen<BattleMissionScreen>();
            }
            else
            {
                Debug.LogWarning("[BattleMissionState] UIManager not available - cannot show BattleMissionScreen");
            }
        }

        /// <summary>
        /// Hide the BattleMission UI screen.
        /// </summary>
        private void HideBattleMissionScreen()
        {
            Debug.Log("[BattleMissionState] Hiding BattleMissionScreen...");
            // Screen is automatically hidden when another screen is shown via UIManager.ShowScreen
        }
    }
}
