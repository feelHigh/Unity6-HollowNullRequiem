// ============================================
// BastionState.cs
// Bastion hub state - run preparation area
// ============================================

using UnityEngine;
using UnityEngine.SceneManagement;
using HNR.Core.Interfaces;

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

            // Load the Bastion scene
            LoadBastionScene();

            // Show Bastion UI
            ShowBastionScreen();

            // Play Bastion ambient music
            PlayBastionMusic();

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

            // Check if scene exists in build settings
            // SceneManager.LoadScene("Bastion", LoadSceneMode.Single);

            // TODO: Implement async scene loading with loading screen
            // var operation = SceneManager.LoadSceneAsync("Bastion");
            // operation.allowSceneActivation = true;
        }

        /// <summary>
        /// Show the Bastion UI screen.
        /// </summary>
        private void ShowBastionScreen()
        {
            Debug.Log("[BastionState] Showing BastionScreen...");

            // TODO: Show Bastion UI via UIManager
            // if (ServiceLocator.TryGet<IUIManager>(out var uiManager))
            // {
            //     uiManager.ShowScreen<BastionScreen>();
            // }
        }

        /// <summary>
        /// Hide the Bastion UI screen.
        /// </summary>
        private void HideBastionScreen()
        {
            Debug.Log("[BastionState] Hiding BastionScreen...");

            // TODO: Hide Bastion UI via UIManager
            // if (ServiceLocator.TryGet<IUIManager>(out var uiManager))
            // {
            //     uiManager.HideScreen<BastionScreen>();
            // }
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
