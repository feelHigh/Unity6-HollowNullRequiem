// ============================================
// MainMenuState.cs
// Main menu state - title screen and menu navigation
// ============================================

using UnityEngine;
using UnityEngine.SceneManagement;
using HNR.Core.Interfaces;

namespace HNR.Core.GameStates
{
    /// <summary>
    /// Main menu state handles the title screen and menu navigation.
    /// Player can start a new run, continue a saved run, or access settings.
    /// </summary>
    public class MainMenuState : IGameState
    {
        private readonly GameManager _manager;

        /// <summary>
        /// Creates a new MainMenuState.
        /// </summary>
        /// <param name="manager">Reference to the GameManager</param>
        public MainMenuState(GameManager manager)
        {
            _manager = manager;
        }

        /// <summary>
        /// Load main menu scene, show UI, and play music.
        /// </summary>
        public void Enter()
        {
            Debug.Log("[MainMenuState] Entering main menu...");

            // Load the main menu scene
            LoadMainMenuScene();

            // Show main menu UI
            ShowMainMenuScreen();

            // Play menu music
            PlayMenuMusic();

            Debug.Log("[MainMenuState] Main menu ready.");
        }

        /// <summary>
        /// Per-frame update (not typically used in menu state).
        /// </summary>
        public void Update()
        {
            // Menu interactions are handled by UI events, not Update()
        }

        /// <summary>
        /// Cleanup when leaving the main menu.
        /// </summary>
        public void Exit()
        {
            Debug.Log("[MainMenuState] Exiting main menu...");

            // Stop menu music
            StopMenuMusic();

            // Hide main menu screen
            HideMainMenuScreen();
        }

        // ============================================
        // Private Methods
        // ============================================

        /// <summary>
        /// Load the MainMenu scene.
        /// </summary>
        private void LoadMainMenuScene()
        {
            Debug.Log("[MainMenuState] Loading MainMenu scene...");

            // Check if scene exists in build settings
            // SceneManager.LoadScene("MainMenu", LoadSceneMode.Single);

            // TODO: Implement async scene loading with loading screen
            // var operation = SceneManager.LoadSceneAsync("MainMenu");
            // operation.allowSceneActivation = true;
        }

        /// <summary>
        /// Show the main menu UI screen.
        /// </summary>
        private void ShowMainMenuScreen()
        {
            Debug.Log("[MainMenuState] Showing MainMenuScreen...");

            // TODO: Show main menu via UIManager
            // if (ServiceLocator.TryGet<IUIManager>(out var uiManager))
            // {
            //     uiManager.ShowScreen<MainMenuScreen>();
            // }
        }

        /// <summary>
        /// Hide the main menu UI screen.
        /// </summary>
        private void HideMainMenuScreen()
        {
            Debug.Log("[MainMenuState] Hiding MainMenuScreen...");

            // TODO: Hide main menu via UIManager
            // if (ServiceLocator.TryGet<IUIManager>(out var uiManager))
            // {
            //     uiManager.HideScreen<MainMenuScreen>();
            // }
        }

        /// <summary>
        /// Play the main menu background music.
        /// </summary>
        private void PlayMenuMusic()
        {
            Debug.Log("[MainMenuState] Playing menu music...");

            // TODO: Play music via AudioManager
            // if (ServiceLocator.TryGet<IAudioManager>(out var audioManager))
            // {
            //     audioManager.PlayMusic("MainMenuTheme");
            // }
        }

        /// <summary>
        /// Stop the main menu background music.
        /// </summary>
        private void StopMenuMusic()
        {
            Debug.Log("[MainMenuState] Stopping menu music...");

            // TODO: Stop music via AudioManager
            // if (ServiceLocator.TryGet<IAudioManager>(out var audioManager))
            // {
            //     audioManager.StopMusic();
            // }
        }
    }
}
