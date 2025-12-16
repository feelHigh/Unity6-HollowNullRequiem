// ============================================
// BastionState.cs
// Bastion hub state - run preparation area
// ============================================

using UnityEngine;
using UnityEngine.SceneManagement;
using HNR.Core.Interfaces;
using HNR.UI;

namespace HNR.Core.GameStates
{
    /// <summary>
    /// Bastion state handles the hub area where players prepare for runs.
    /// Players can select Requiems, view unlocks, and manage meta-progression.
    /// </summary>
    public class BastionState : IGameState
    {
        private readonly GameManager _manager;

        /// <summary>
        /// Creates a new BastionState.
        /// </summary>
        /// <param name="manager">Reference to the GameManager</param>
        public BastionState(GameManager manager)
        {
            _manager = manager;
        }

        /// <summary>
        /// Load Bastion scene, show UI, and play music.
        /// </summary>
        public void Enter()
        {
            Debug.Log("[BastionState] Entering Bastion hub...");

            // Subscribe to scene loaded to show UI after scene is ready
            SceneManager.sceneLoaded += OnBastionSceneLoaded;

            // Load the Bastion scene
            LoadBastionScene();

            // Play Bastion ambient music
            PlayBastionMusic();
        }

        /// <summary>
        /// Called when Bastion scene finishes loading.
        /// </summary>
        private void OnBastionSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (scene.name != "Bastion") return;

            // Unsubscribe to prevent multiple calls
            SceneManager.sceneLoaded -= OnBastionSceneLoaded;

            Debug.Log("[BastionState] Bastion scene loaded. Showing UI...");

            // Show Bastion UI now that scene is loaded
            ShowBastionScreen();

            Debug.Log("[BastionState] Bastion ready.");
        }

        /// <summary>
        /// Per-frame update (not typically used in Bastion state).
        /// </summary>
        public void Update()
        {
            // Bastion interactions are handled by UI events, not Update()
        }

        /// <summary>
        /// Cleanup when leaving the Bastion.
        /// </summary>
        public void Exit()
        {
            Debug.Log("[BastionState] Leaving Bastion...");

            // Unsubscribe from scene loaded in case we exit early
            SceneManager.sceneLoaded -= OnBastionSceneLoaded;

            // Stop Bastion music
            StopBastionMusic();

            // Hide Bastion screen
            HideBastionScreen();
        }

        // ============================================
        // Private Methods
        // ============================================

        /// <summary>
        /// Load the Bastion scene.
        /// </summary>
        private void LoadBastionScene()
        {
            Debug.Log("[BastionState] Loading Bastion scene...");

            // Only load if not already in Bastion scene
            if (SceneManager.GetActiveScene().name != "Bastion")
            {
                SceneManager.LoadScene("Bastion", LoadSceneMode.Single);
            }
        }

        /// <summary>
        /// Show the Bastion UI screen.
        /// </summary>
        private void ShowBastionScreen()
        {
            Debug.Log("[BastionState] Showing BastionScreen...");

            if (ServiceLocator.TryGet<IUIManager>(out var uiManager))
            {
                uiManager.ShowScreen<BastionScreen>();
            }
            else
            {
                Debug.LogWarning("[BastionState] UIManager not available - cannot show BastionScreen");
            }
        }

        /// <summary>
        /// Hide the Bastion UI screen.
        /// </summary>
        private void HideBastionScreen()
        {
            Debug.Log("[BastionState] Hiding BastionScreen...");

            // Screen is automatically hidden when another screen is shown via UIManager.ShowScreen
            // No explicit hide needed - the next state will show its own screen
        }

        /// <summary>
        /// Play the Bastion ambient music.
        /// </summary>
        private void PlayBastionMusic()
        {
            Debug.Log("[BastionState] Playing Bastion music...");

            // TODO: Play music via AudioManager
            // if (ServiceLocator.TryGet<IAudioManager>(out var audioManager))
            // {
            //     audioManager.PlayMusic("BastionTheme");
            // }
        }

        /// <summary>
        /// Stop the Bastion ambient music.
        /// </summary>
        private void StopBastionMusic()
        {
            Debug.Log("[BastionState] Stopping Bastion music...");

            // TODO: Stop music via AudioManager
            // if (ServiceLocator.TryGet<IAudioManager>(out var audioManager))
            // {
            //     audioManager.StopMusic();
            // }
        }
    }
}
