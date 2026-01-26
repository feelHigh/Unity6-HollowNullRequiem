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
using HNR.Progression;
using HNR.UI.Screens;

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

        private bool _hasActiveRun;
        private bool _hasAnyProgress;

        private void RefreshContinueButton()
        {
            var saveManager = ServiceLocator.Get<ISaveManager>();
            var saveManagerImpl = saveManager as SaveManager;

            // Check for active run (interrupted mid-progress)
            _hasActiveRun = saveManager?.HasSavedRun ?? false;

            // Check for any saved progress (zones cleared, level > 1, or active run)
            _hasAnyProgress = _hasActiveRun;

            if (!_hasAnyProgress && saveManagerImpl != null)
            {
                // Check for Battle Mission progress (zones cleared)
                var battleProgress = saveManagerImpl.LoadBattleMissionProgress();
                if (battleProgress != null && battleProgress.ZoneClearStatus.Count > 0)
                {
                    _hasAnyProgress = true;
                }

                // Check for player level progress
                if (!_hasAnyProgress)
                {
                    var metaData = saveManagerImpl.LoadMeta();
                    if (metaData != null && metaData.PlayerLevel > 1)
                    {
                        _hasAnyProgress = true;
                    }
                }
            }

            if (_continueButton != null)
            {
                _continueButton.gameObject.SetActive(_hasAnyProgress);
            }

            if (_hasAnyProgress && _continueButtonGroup != null)
            {
                // Animate fade in
                _continueButtonGroup.alpha = 0f;
                _continueButtonGroup.DOFade(1f, _fadeAnimDuration).SetEase(Ease.OutQuad);
            }

            // Update save info display
            UpdateSaveInfo(saveManagerImpl);

            Debug.Log($"[MainMenuScreen] Continue button visible: {_hasAnyProgress} (activeRun: {_hasActiveRun})");
        }

        private void UpdateSaveInfo(SaveManager saveManager)
        {
            if (_saveInfoText == null) return;

            if (!_hasAnyProgress)
            {
                _saveInfoText.gameObject.SetActive(false);
                return;
            }

            if (_hasActiveRun)
            {
                // Show active run info
                var saveData = saveManager?.LoadRun();
                if (saveData != null)
                {
                    string zoneText = $"Zone {saveData.Progression.CurrentZone}";
                    string timeText = FormatPlayTime(saveData.Stats.PlayTime);
                    _saveInfoText.text = $"{zoneText} - {timeText}";
                    _saveInfoText.gameObject.SetActive(true);
                    return;
                }
            }

            // Show progress info (level/zones cleared)
            if (saveManager != null)
            {
                var metaData = saveManager.LoadMeta();
                int level = metaData?.PlayerLevel ?? 1;
                _saveInfoText.text = $"Level {level}";
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
            Debug.Log("[MainMenuScreen] Continue clicked");

            // Check if there's an active run to resume
            if (_hasActiveRun)
            {
                Debug.Log("[MainMenuScreen] Resuming active run...");
                var runManager = ServiceLocator.Get<IRunManager>();
                if (runManager != null && runManager.LoadRun())
                {
                    // Transition to map scene
                    TransitionToGame();
                    return;
                }
                else
                {
                    Debug.LogWarning("[MainMenuScreen] Failed to load saved run");
                }
            }

            // No active run - go to Bastion hub with existing progress
            Debug.Log("[MainMenuScreen] No active run - going to Bastion with existing progress");
            if (ServiceLocator.TryGet<IGameManager>(out var gameManager))
            {
                gameManager.ChangeState(GameState.Bastion);
            }
            else
            {
                Debug.LogWarning("[MainMenuScreen] GameManager not found!");
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

            // Reset Battle Mission progress for fresh start
            var progressManager = BattleMissionProgressManager.Instance;
            if (progressManager != null)
            {
                progressManager.ResetAllProgress();
                Debug.Log("[MainMenuScreen] Reset Battle Mission progress via manager");
            }
            else
            {
                // Manager doesn't exist yet, delete directly from save file
                const string SAVE_FILE = "HNR_Save.es3";
                const string BATTLE_MISSION_KEY = "BattleMissionProgress";
                if (ES3.KeyExists(BATTLE_MISSION_KEY, SAVE_FILE))
                {
                    ES3.DeleteKey(BATTLE_MISSION_KEY, SAVE_FILE);
                    Debug.Log("[MainMenuScreen] Reset Battle Mission progress via ES3");
                }
            }

            // Reset player level/XP progression
            if (ServiceLocator.TryGet<PlayerProgressionManager>(out var playerProgress))
            {
                playerProgress.ResetProgression();
                Debug.Log("[MainMenuScreen] Reset player level/XP progression");
            }
            else
            {
                // Manager doesn't exist yet, reset MetaSaveData directly
                var saveManagerImpl = saveManager as SaveManager;
                if (saveManagerImpl != null)
                {
                    var metaData = saveManagerImpl.LoadMeta() ?? new MetaSaveData();
                    metaData.PlayerLevel = 1;
                    metaData.CurrentXP = 0;
                    metaData.TotalXP = 0;
                    saveManagerImpl.SaveMeta(metaData);
                    Debug.Log("[MainMenuScreen] Reset player level/XP via SaveManager");
                }
            }

            // Navigate to Bastion (hub scene where team selection happens)
            if (ServiceLocator.TryGet<IGameManager>(out var gameManager))
            {
                gameManager.ChangeState(GameState.Bastion);
            }
            else
            {
                Debug.LogWarning("[MainMenuScreen] GameManager not found!");
            }
        }

        private void OnSettingsClicked()
        {
            Debug.Log("[MainMenuScreen] Settings clicked");
            SettingsOverlay.ShowSettings();
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
