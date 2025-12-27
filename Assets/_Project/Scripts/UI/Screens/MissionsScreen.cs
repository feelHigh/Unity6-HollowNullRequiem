// ============================================
// MissionsScreen.cs
// Mission type selection screen (Story/Battle Mission)
// ============================================

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using HNR.Core;
using HNR.Core.Interfaces;
using HNR.Progression;
using HNR.UI.Toast;

namespace HNR.UI.Screens
{
    /// <summary>
    /// Screen for selecting mission type.
    /// Allows choosing between Story Mission (placeholder) and Battle Mission.
    /// </summary>
    public class MissionsScreen : ScreenBase
    {
        // ============================================
        // Serialized Fields
        // ============================================

        [Header("Header")]
        [SerializeField, Tooltip("Back button to return to Bastion")]
        private Button _backButton;

        [SerializeField, Tooltip("Screen title text")]
        private TMP_Text _titleText;

        [SerializeField, Tooltip("Settings button")]
        private Button _settingsButton;

        [Header("Mission Buttons")]
        [SerializeField, Tooltip("Story Mission button (placeholder)")]
        private Button _storyMissionButton;

        [SerializeField, Tooltip("Story Mission title")]
        private TMP_Text _storyMissionTitle;

        [SerializeField, Tooltip("Story Mission subtitle")]
        private TMP_Text _storyMissionSubtitle;

        [SerializeField, Tooltip("Battle Mission button")]
        private Button _battleMissionButton;

        [SerializeField, Tooltip("Battle Mission title")]
        private TMP_Text _battleMissionTitle;

        [SerializeField, Tooltip("Battle Mission subtitle")]
        private TMP_Text _battleMissionSubtitle;

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
            _showGlobalHeader = false;
            _showGlobalNav = false;
        }

        // ============================================
        // ScreenBase Overrides
        // ============================================

        public override void OnShow()
        {
            base.OnShow();
            SetupButtons();
            PlayEntranceAnimation();
        }

        public override void OnHide()
        {
            base.OnHide();
            _currentTween?.Kill();
        }

        public override bool OnBackPressed()
        {
            NavigateToBastion();
            return true;
        }

        // ============================================
        // Setup
        // ============================================

        private void SetupButtons()
        {
            // Back button
            if (_backButton != null)
            {
                _backButton.onClick.RemoveAllListeners();
                _backButton.onClick.AddListener(OnBackClicked);
            }

            // Settings button
            if (_settingsButton != null)
            {
                _settingsButton.onClick.RemoveAllListeners();
                _settingsButton.onClick.AddListener(OnSettingsClicked);
            }

            // Story Mission button (placeholder)
            if (_storyMissionButton != null)
            {
                _storyMissionButton.onClick.RemoveAllListeners();
                _storyMissionButton.onClick.AddListener(OnStoryMissionClicked);
            }

            // Battle Mission button
            if (_battleMissionButton != null)
            {
                _battleMissionButton.onClick.RemoveAllListeners();
                _battleMissionButton.onClick.AddListener(OnBattleMissionClicked);
            }

            // Setup text
            if (_titleText != null)
                _titleText.text = "Missions";

            if (_storyMissionTitle != null)
                _storyMissionTitle.text = "Story";

            if (_storyMissionSubtitle != null)
                _storyMissionSubtitle.text = "Coming Soon";

            if (_battleMissionTitle != null)
                _battleMissionTitle.text = "Battle Mission";

            if (_battleMissionSubtitle != null)
                _battleMissionSubtitle.text = GetProgressionSubtitle();
        }

        /// <summary>
        /// Gets the progression subtitle based on cleared zones.
        /// Shows the highest cleared zone on current difficulty.
        /// </summary>
        private string GetProgressionSubtitle()
        {
            // Get progress from BattleMissionProgressManager
            var progressManager = BattleMissionProgressManager.Instance;
            if (progressManager != null)
            {
                var difficulty = progressManager.CurrentDifficulty;
                int zonesCleared = progressManager.GetZonesClearedCount(difficulty);

                if (zonesCleared > 0)
                {
                    string difficultyStr = difficulty == DifficultyLevel.Easy ? "" : $" ({difficulty})";
                    return $"Zone {zonesCleared} Cleared{difficultyStr}";
                }
            }

            // Default subtitle when no progress
            return "Challenge the Null Rift";
        }

        // ============================================
        // Button Handlers
        // ============================================

        private void OnBackClicked()
        {
            NavigateToBastion();
        }

        private void OnSettingsClicked()
        {
            SettingsOverlay.ShowSettings();
        }

        private void OnStoryMissionClicked()
        {
            // Show coming soon toast
            ToastManager.Instance?.ShowInfo("Story Mission coming soon!");

            // Play feedback animation
            if (_storyMissionButton != null)
            {
                _storyMissionButton.transform.DOShakePosition(0.3f, 5f, 15)
                    .SetLink(gameObject);
            }
        }

        private void OnBattleMissionClicked()
        {
            // Navigate to Battle Mission screen
            if (ServiceLocator.TryGet<IGameManager>(out var gameManager))
            {
                gameManager.ChangeState(GameState.BattleMission);
            }
        }

        // ============================================
        // Navigation
        // ============================================

        private void NavigateToBastion()
        {
            if (ServiceLocator.TryGet<IGameManager>(out var gameManager))
            {
                gameManager.ChangeState(GameState.Bastion);
            }
        }

        // ============================================
        // Animation
        // ============================================

        private void PlayEntranceAnimation()
        {
            _currentTween?.Kill();

            var sequence = DOTween.Sequence();

            // Animate title
            if (_titleText != null)
            {
                _titleText.transform.localScale = Vector3.zero;
                sequence.Append(_titleText.transform.DOScale(1f, _buttonEntranceDuration)
                    .SetEase(Ease.OutBack));
            }

            // Animate Story Mission button
            if (_storyMissionButton != null)
            {
                _storyMissionButton.transform.localScale = Vector3.zero;
                sequence.Append(_storyMissionButton.transform.DOScale(1f, _buttonEntranceDuration)
                    .SetEase(Ease.OutBack));
            }

            // Animate Battle Mission button
            if (_battleMissionButton != null)
            {
                _battleMissionButton.transform.localScale = Vector3.zero;
                sequence.Append(_battleMissionButton.transform.DOScale(1f, _buttonEntranceDuration)
                    .SetEase(Ease.OutBack));
            }

            sequence.SetLink(gameObject);
            _currentTween = sequence;
        }
    }
}
