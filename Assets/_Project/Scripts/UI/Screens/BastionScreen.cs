// ============================================
// BastionScreen.cs
// Hub screen - Main navigation to Missions and Requiems
// ============================================

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using HNR.Core;
using HNR.Core.Interfaces;
using HNR.UI.Components;
using HNR.UI.Screens;

namespace HNR.UI
{
    /// <summary>
    /// Bastion hub screen - simplified navigation hub.
    /// Provides access to Missions and Requiems screens.
    /// Reference: BastionSceneDesignReference.jpg
    /// </summary>
    public class BastionScreen : ScreenBase
    {
        // ============================================
        // Header Section (Top Left)
        // ============================================

        [Header("Player Info (Top Left)")]
        [SerializeField, Tooltip("Player level text")]
        private TMP_Text _playerLevelText;

        [SerializeField, Tooltip("Player nickname text")]
        private TMP_Text _playerNicknameText;

        [SerializeField, Tooltip("Player XP progress bar (optional)")]
        private Image _xpProgressBar;

        // ============================================
        // Settings Button (Top Right)
        // ============================================

        [Header("Settings (Top Right)")]
        [SerializeField, Tooltip("Settings button (hamburger menu icon)")]
        private Button _settingsButton;

        // ============================================
        // Main Navigation Buttons
        // ============================================

        [Header("Navigation Buttons")]
        [SerializeField, Tooltip("Missions button")]
        private Button _missionsButton;

        [SerializeField, Tooltip("Missions button text")]
        private TMP_Text _missionsButtonText;

        [SerializeField, Tooltip("Missions button subtitle")]
        private TMP_Text _missionsButtonSubtitle;

        [SerializeField, Tooltip("Requiems button")]
        private Button _requiemsButton;

        [SerializeField, Tooltip("Requiems button text")]
        private TMP_Text _requiemsButtonText;

        [SerializeField, Tooltip("Requiems button subtitle")]
        private TMP_Text _requiemsButtonSubtitle;

        // ============================================
        // Event Banner Carousel
        // ============================================

        [Header("Event Banner")]
        [SerializeField, Tooltip("Event banner carousel component")]
        private EventBannerCarousel _eventBannerCarousel;

        // ============================================
        // Animation Settings
        // ============================================

        [Header("Animation")]
        [SerializeField] private float _buttonEntranceDelay = 0.1f;
        [SerializeField] private float _buttonEntranceDuration = 0.3f;

        // ============================================
        // State
        // ============================================

        private Tween _currentTween;

        // ============================================
        // Configuration
        // ============================================

        protected override void Awake()
        {
            base.Awake();
            // No global header or nav dock in new design
            _showGlobalHeader = false;
            _showGlobalNav = false;
        }

        // ============================================
        // Screen Lifecycle
        // ============================================

        public override void OnShow()
        {
            base.OnShow();

            SetupButtons();
            LoadPlayerData();
            PlayShowAnimation();

            // Initialize event banner carousel
            if (_eventBannerCarousel != null)
            {
                _eventBannerCarousel.Initialize();
            }

            Debug.Log("[BastionScreen] Bastion hub shown");
        }

        public override void OnHide()
        {
            base.OnHide();

            // Kill any running tweens
            _currentTween?.Kill();
            DOTween.Kill(this);

            // Pause event banner auto-advance
            if (_eventBannerCarousel != null)
            {
                _eventBannerCarousel.PauseAutoAdvance();
            }

            Debug.Log("[BastionScreen] Bastion hub hidden");
        }

        // ============================================
        // Setup
        // ============================================

        private void SetupButtons()
        {
            // Settings button
            if (_settingsButton != null)
            {
                _settingsButton.onClick.RemoveAllListeners();
                _settingsButton.onClick.AddListener(OnSettingsClicked);
            }

            // Missions button
            if (_missionsButton != null)
            {
                _missionsButton.onClick.RemoveAllListeners();
                _missionsButton.onClick.AddListener(OnMissionsClicked);
            }

            // Requiems button
            if (_requiemsButton != null)
            {
                _requiemsButton.onClick.RemoveAllListeners();
                _requiemsButton.onClick.AddListener(OnRequiemsClicked);
            }

            // Setup button text
            if (_missionsButtonText != null)
                _missionsButtonText.text = "Missions";

            if (_missionsButtonSubtitle != null)
                _missionsButtonSubtitle.text = "Enter the Null Rift";

            if (_requiemsButtonText != null)
                _requiemsButtonText.text = "Requiems";

            if (_requiemsButtonSubtitle != null)
                _requiemsButtonSubtitle.text = "View your combatants";
        }

        private void LoadPlayerData()
        {
            // Load player progression from PlayerProgressionManager
            int playerLevel = 1;
            string playerNickname = "Commander";
            float xpProgress = 0f;

            if (ServiceLocator.TryGet<Progression.PlayerProgressionManager>(out var playerProgress))
            {
                playerLevel = playerProgress.GetLevel();
                xpProgress = playerProgress.GetXPProgress();
            }

            // Update UI
            if (_playerLevelText != null)
            {
                _playerLevelText.text = $"LV\n{playerLevel}";
            }

            if (_playerNicknameText != null)
            {
                _playerNicknameText.text = playerNickname;
            }

            if (_xpProgressBar != null)
            {
                _xpProgressBar.fillAmount = xpProgress;
            }
        }

        // ============================================
        // Button Handlers
        // ============================================

        private void OnSettingsClicked()
        {
            Debug.Log("[BastionScreen] Settings clicked");
            SettingsOverlay.ShowSettings();
        }

        private void OnMissionsClicked()
        {
            Debug.Log("[BastionScreen] Missions clicked");

            // Navigate to Missions screen
            if (ServiceLocator.TryGet<IGameManager>(out var gameManager))
            {
                gameManager.ChangeState(GameState.Missions);
            }
        }

        private void OnRequiemsClicked()
        {
            Debug.Log("[BastionScreen] Requiems clicked");

            // Navigate to Requiems viewer
            if (ServiceLocator.TryGet<IGameManager>(out var gameManager))
            {
                gameManager.ChangeState(GameState.RequiemsViewer);
            }
        }

        // ============================================
        // Animation
        // ============================================

        protected override void PlayShowAnimation()
        {
            _currentTween?.Kill();

            var sequence = DOTween.Sequence();

            // Animate player info
            if (_playerLevelText != null)
            {
                _playerLevelText.transform.localScale = Vector3.zero;
                sequence.Append(_playerLevelText.transform.DOScale(1f, _buttonEntranceDuration)
                    .SetEase(Ease.OutBack));
            }

            // Animate Missions button
            if (_missionsButton != null)
            {
                _missionsButton.transform.localScale = Vector3.zero;
                sequence.Append(_missionsButton.transform.DOScale(1f, _buttonEntranceDuration)
                    .SetEase(Ease.OutBack)
                    .SetDelay(_buttonEntranceDelay));
            }

            // Animate Requiems button
            if (_requiemsButton != null)
            {
                _requiemsButton.transform.localScale = Vector3.zero;
                sequence.Append(_requiemsButton.transform.DOScale(1f, _buttonEntranceDuration)
                    .SetEase(Ease.OutBack)
                    .SetDelay(_buttonEntranceDelay));
            }

            sequence.SetLink(gameObject);
            _currentTween = sequence;
        }

        // ============================================
        // Back Button
        // ============================================

        public override bool OnBackPressed()
        {
            // Navigate to main menu
            if (ServiceLocator.TryGet<IGameManager>(out var gameManager))
            {
                gameManager.ChangeState(GameState.MainMenu);
            }
            return true;
        }
    }
}
