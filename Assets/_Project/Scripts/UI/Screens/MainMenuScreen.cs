// ============================================
// MainMenuScreen.cs
// Main menu with Continue, New Run, Settings, and Quit
// ============================================

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using DG.Tweening;
using HNR.Core;
using HNR.Core.Interfaces;

namespace HNR.UI
{
    /// <summary>
    /// Main menu screen with Continue run, New Run, Settings, and Quit options.
    /// Continue button only visible when a saved run exists.
    /// </summary>
    public class MainMenuScreen : ScreenBase
    {
        // ============================================
        // Serialized Fields
        // ============================================

        [Header("Menu Buttons")]
        [SerializeField, Tooltip("Button to continue saved run")]
        private Button _continueButton;

        [SerializeField, Tooltip("CanvasGroup for continue button fade animation")]
        private CanvasGroup _continueButtonGroup;

        [SerializeField, Tooltip("Button to start new run")]
        private Button _newRunButton;

        [SerializeField, Tooltip("Button to open settings")]
        private Button _settingsButton;

        [SerializeField, Tooltip("Button to quit game")]
        private Button _quitButton;

        [Header("Save Info Display")]
        [SerializeField, Tooltip("Optional text showing save info (zone, time, etc.)")]
        private TMP_Text _saveInfoText;

        [Header("Animation")]
        [SerializeField, Tooltip("Duration for button fade animations")]
        private float _fadeAnimDuration = 0.3f;

        [SerializeField, Tooltip("Duration for button slide animations")]
        private float _slideAnimDuration = 0.4f;

        [Header("Version Display")]
        [SerializeField, Tooltip("Text showing game version")]
        private TMP_Text _versionText;

        // ============================================
        // Screen Lifecycle
        // ============================================

        public override void OnShow()
        {
            base.OnShow();

            SetupButtons();
            RefreshContinueButton();
            UpdateVersionText();

            Debug.Log("[MainMenuScreen] Main menu shown");
        }

        public override void OnHide()
        {
            base.OnHide();

            // Kill any running tweens
            if (_continueButtonGroup != null)
            {
                DOTween.Kill(_continueButtonGroup);
            }

            Debug.Log("[MainMenuScreen] Main menu hidden");
        }

        // ============================================
        // Setup
        // ============================================

        private void SetupButtons()
        {
            if (_continueButton != null)
            {
                _continueButton.onClick.RemoveAllListeners();
                _continueButton.onClick.AddListener(OnContinueClicked);
            }

            if (_newRunButton != null)
            {
                _newRunButton.onClick.RemoveAllListeners();
                _newRunButton.onClick.AddListener(OnNewRunClicked);
            }

            if (_settingsButton != null)
            {
                _settingsButton.onClick.RemoveAllListeners();
                _settingsButton.onClick.AddListener(OnSettingsClicked);
            }

            if (_quitButton != null)
            {
                _quitButton.onClick.RemoveAllListeners();
                _quitButton.onClick.AddListener(OnQuitClicked);
            }
        }

        // ============================================
        // Continue Button
        // ============================================

        private void RefreshContinueButton()
        {
            var saveManager = ServiceLocator.Get<ISaveManager>();
            bool hasSave = saveManager?.HasSavedRun ?? false;

            if (_continueButton != null)
            {
                _continueButton.gameObject.SetActive(hasSave);
            }

            if (hasSave && _continueButtonGroup != null)
            {
                // Animate fade in
                _continueButtonGroup.alpha = 0f;
                _continueButtonGroup.DOFade(1f, _fadeAnimDuration).SetEase(Ease.OutQuad);
            }

            // Update save info display
            UpdateSaveInfo(saveManager, hasSave);

            Debug.Log($"[MainMenuScreen] Continue button visible: {hasSave}");
        }

        private void UpdateSaveInfo(ISaveManager saveManager, bool hasSave)
        {
            if (_saveInfoText == null) return;

            if (!hasSave)
            {
                _saveInfoText.gameObject.SetActive(false);
                return;
            }

            // Load save data to display info
            var saveData = saveManager?.LoadRun();
            if (saveData != null)
            {
                string zoneText = $"Zone {saveData.Progression.CurrentZone}";
                string timeText = FormatPlayTime(saveData.Stats.PlayTime);
                _saveInfoText.text = $"{zoneText} - {timeText}";
                _saveInfoText.gameObject.SetActive(true);
            }
            else
            {
                _saveInfoText.gameObject.SetActive(false);
            }
        }

        private string FormatPlayTime(float seconds)
        {
            int minutes = Mathf.FloorToInt(seconds / 60f);
            int secs = Mathf.FloorToInt(seconds % 60f);

            if (minutes >= 60)
            {
                int hours = minutes / 60;
                minutes = minutes % 60;
                return $"{hours}h {minutes}m";
            }

            return $"{minutes}m {secs}s";
        }

        private void UpdateVersionText()
        {
            if (_versionText != null)
            {
                _versionText.text = $"v{Application.version}";
            }
        }

        // ============================================
        // Button Handlers
        // ============================================

        private void OnContinueClicked()
        {
            Debug.Log("[MainMenuScreen] Continue clicked - loading saved run");

            var runManager = ServiceLocator.Get<IRunManager>();
            if (runManager != null && runManager.LoadRun())
            {
                // Transition to map scene
                TransitionToGame();
            }
            else
            {
                Debug.LogWarning("[MainMenuScreen] Failed to load saved run");
                // Refresh button state in case save is corrupted
                RefreshContinueButton();
            }
        }

        private void OnNewRunClicked()
        {
            Debug.Log("[MainMenuScreen] New Run clicked");

            // Delete any existing save
            var saveManager = ServiceLocator.Get<ISaveManager>();
            if (saveManager?.HasSavedRun == true)
            {
                saveManager.DeleteRun();
                Debug.Log("[MainMenuScreen] Deleted existing saved run");
            }

            // Navigate to Requiem selection
            var uiManager = ServiceLocator.Get<IUIManager>();
            if (uiManager != null)
            {
                uiManager.ShowScreen<RequiemSelectionScreen>();
            }
            else
            {
                // Fallback: change game state directly
                var gameManager = ServiceLocator.Get<IGameManager>();
                gameManager?.ChangeState(GameState.Bastion);
            }
        }

        private void OnSettingsClicked()
        {
            Debug.Log("[MainMenuScreen] Settings clicked");

            // TODO: Show settings screen/overlay
            // var uiManager = ServiceLocator.Get<IUIManager>();
            // uiManager?.PushOverlay<SettingsScreen>();
        }

        private void OnQuitClicked()
        {
            Debug.Log("[MainMenuScreen] Quit clicked");

#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        // ============================================
        // Transitions
        // ============================================

        private void TransitionToGame()
        {
            // Use GameManager for proper state transition
            var gameManager = ServiceLocator.Get<IGameManager>();
            if (gameManager != null)
            {
                gameManager.ChangeState(GameState.Run);
            }
            else
            {
                // Fallback: load scene directly
                SceneManager.LoadScene("NullRift");
            }
        }

        // ============================================
        // Public Methods
        // ============================================

        /// <summary>
        /// Force refresh the continue button state.
        /// Call after save data changes.
        /// </summary>
        public void RefreshUI()
        {
            RefreshContinueButton();
        }

        public override bool OnBackPressed()
        {
            // On main menu, back should not do anything or show quit confirmation
            return true;
        }
    }
}
