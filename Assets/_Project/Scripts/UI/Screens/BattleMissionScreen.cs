// ============================================
// BattleMissionScreen.cs
// Zone selection with difficulty for Battle Mission
// ============================================

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using HNR.Core;
using HNR.Core.Interfaces;
using HNR.Progression;
using HNR.UI.Components;
using HNR.UI.Toast;
using HNR.Map;

namespace HNR.UI.Screens
{
    /// <summary>
    /// Screen for selecting zone and difficulty in Battle Mission mode.
    /// Displays 3 zone nodes horizontally with difficulty selector.
    /// </summary>
    public class BattleMissionScreen : ScreenBase
    {
        // ============================================
        // Serialized Fields
        // ============================================

        [Header("Header")]
        [SerializeField, Tooltip("Back button to return to Missions")]
        private Button _backButton;

        [SerializeField, Tooltip("Screen title text")]
        private TMP_Text _titleText;

        [SerializeField, Tooltip("Settings button")]
        private Button _settingsButton;

        [Header("Zone Nodes")]
        [SerializeField, Tooltip("Container for zone nodes")]
        private Transform _zoneContainer;

        [SerializeField, Tooltip("Zone 1 node button")]
        private ZoneNodeButton _zone1Node;

        [SerializeField, Tooltip("Zone 2 node button")]
        private ZoneNodeButton _zone2Node;

        [SerializeField, Tooltip("Zone 3 node button")]
        private ZoneNodeButton _zone3Node;

        [Header("Zone Info")]
        [SerializeField, Tooltip("Zone description panel")]
        private GameObject _zoneInfoPanel;

        [SerializeField, Tooltip("Zone description title")]
        private TMP_Text _zoneInfoTitle;

        [SerializeField, Tooltip("Zone description text")]
        private TMP_Text _zoneInfoDescription;

        [Header("Difficulty")]
        [SerializeField, Tooltip("Difficulty selector component")]
        private DifficultySelector _difficultySelector;

        [SerializeField, Tooltip("Difficulty description text")]
        private TMP_Text _difficultyDescription;

        [Header("Zone Configs")]
        [SerializeField, Tooltip("Zone configuration assets")]
        private ZoneConfigSO[] _zoneConfigs;

        [Header("Animation")]
        [SerializeField] private float _nodeEntranceDelay = 0.1f;

        // ============================================
        // State
        // ============================================

        private ZoneNodeButton[] _zoneNodes;
        private int _selectedZone = -1;
        private Tween _currentTween;

        // ============================================
        // Configuration
        // ============================================

        protected override void Awake()
        {
            base.Awake();
            _showGlobalHeader = false;
            _showGlobalNav = false;

            // Build zone nodes array
            _zoneNodes = new[] { _zone1Node, _zone2Node, _zone3Node };
        }

        // ============================================
        // ScreenBase Overrides
        // ============================================

        public override void OnShow()
        {
            base.OnShow();
            SetupButtons();
            SetupZoneNodes();
            SetupDifficultySelector();
            RefreshZoneStates();
            HideZoneInfo();
            PlayEntranceAnimation();
        }

        public override void OnHide()
        {
            base.OnHide();
            _currentTween?.Kill();
        }

        public override bool OnBackPressed()
        {
            NavigateToMissions();
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

            // Title
            if (_titleText != null)
                _titleText.text = "Battle Mission";
        }

        private void SetupZoneNodes()
        {
            // Setup zone names from configs or defaults
            string[] zoneNames = new[] { "The Outer Reaches", "The Hollow Depths", "The Null Core" };

            if (_zoneConfigs != null)
            {
                for (int i = 0; i < Mathf.Min(_zoneConfigs.Length, zoneNames.Length); i++)
                {
                    if (_zoneConfigs[i] != null)
                    {
                        zoneNames[i] = _zoneConfigs[i].ZoneName;
                    }
                }
            }

            // Configure each zone node
            for (int i = 0; i < _zoneNodes.Length; i++)
            {
                var node = _zoneNodes[i];
                if (node == null) continue;

                int zoneNumber = i + 1;
                node.SetZone(zoneNumber, zoneNames[i]);

                // Subscribe to selection event
                node.OnZoneSelected -= OnZoneSelected;
                node.OnZoneSelected += OnZoneSelected;
            }
        }

        private void SetupDifficultySelector()
        {
            if (_difficultySelector != null)
            {
                _difficultySelector.RefreshUnlockStatus();
                _difficultySelector.OnDifficultyChanged -= OnDifficultyChanged;
                _difficultySelector.OnDifficultyChanged += OnDifficultyChanged;

                // For new games (no zones cleared), ensure Easy is selected and visually indicated
                var progressManager = BattleMissionProgressManager.Instance;
                if (progressManager != null && progressManager.GetZonesClearedCount(DifficultyLevel.Easy) == 0)
                {
                    // Force Easy selection for new players
                    _difficultySelector.SetDifficultyWithoutNotify(DifficultyLevel.Easy);
                }

                // Update description
                UpdateDifficultyDescription(_difficultySelector.CurrentDifficulty);
            }
        }

        // ============================================
        // State Updates
        // ============================================

        private void RefreshZoneStates()
        {
            var currentDifficulty = _difficultySelector?.CurrentDifficulty ?? DifficultyLevel.Easy;

            foreach (var node in _zoneNodes)
            {
                if (node != null)
                {
                    node.UpdateFromProgressManager(currentDifficulty);
                }
            }
        }

        private void UpdateDifficultyDescription(DifficultyLevel difficulty)
        {
            if (_difficultyDescription != null)
            {
                _difficultyDescription.text = DifficultySelector.GetDifficultyDescription(difficulty);
            }
        }

        // ============================================
        // Button Handlers
        // ============================================

        private void OnBackClicked()
        {
            NavigateToMissions();
        }

        private void OnSettingsClicked()
        {
            SettingsOverlay.ShowSettings();
        }

        // ============================================
        // Zone Selection
        // ============================================

        private void OnZoneSelected(int zoneNumber)
        {
            _selectedZone = zoneNumber;

            // Show zone info
            ShowZoneInfo(zoneNumber);

            // Start the run with team selection
            StartZoneRun(zoneNumber);
        }

        private void ShowZoneInfo(int zoneNumber)
        {
            if (_zoneInfoPanel == null) return;

            _zoneInfoPanel.SetActive(true);

            // Get zone config
            ZoneConfigSO config = null;
            if (_zoneConfigs != null && zoneNumber - 1 < _zoneConfigs.Length)
            {
                config = _zoneConfigs[zoneNumber - 1];
            }

            if (_zoneInfoTitle != null)
            {
                _zoneInfoTitle.text = config != null ? config.ZoneName : $"Zone {zoneNumber}";
            }

            if (_zoneInfoDescription != null)
            {
                _zoneInfoDescription.text = config != null ? config.ZoneDescription : "Enter the Null Rift...";
            }
        }

        private void HideZoneInfo()
        {
            if (_zoneInfoPanel != null)
            {
                _zoneInfoPanel.SetActive(false);
            }
        }

        private void StartZoneRun(int zoneNumber)
        {
            var currentDifficulty = _difficultySelector?.CurrentDifficulty ?? DifficultyLevel.Easy;

            Debug.Log($"[BattleMissionScreen] Starting Zone {zoneNumber} on {currentDifficulty}");

            // Store zone and difficulty context in RunManager
            if (ServiceLocator.TryGet<IRunManager>(out var runManager))
            {
                // Set context for the upcoming run
                // RunManager will use this when starting the run
                var runManagerImpl = runManager as RunManager;
                if (runManagerImpl != null)
                {
                    runManagerImpl.SetBattleMissionContext(zoneNumber, currentDifficulty);
                }
            }

            // Navigate to RequiemSelectionScreen (which handles team selection before run)
            // We need to show the RequiemSelectionScreen as an overlay or transition
            ShowRequiemSelection();
        }

        private void ShowRequiemSelection()
        {
            // Get UIManager and show RequiemSelectionScreen
            if (ServiceLocator.TryGet<IUIManager>(out var uiManager))
            {
                uiManager.ShowScreen<RequiemSelectionScreen>();
            }
            else
            {
                // Fallback: Find and show directly
                var requiemScreen = FindFirstObjectByType<RequiemSelectionScreen>(FindObjectsInactive.Include);
                if (requiemScreen != null)
                {
                    requiemScreen.gameObject.SetActive(true);
                    requiemScreen.OnShow();
                }
                else
                {
                    ToastManager.Instance?.ShowWarning("RequiemSelectionScreen not found");
                }
            }
        }

        // ============================================
        // Difficulty Changed
        // ============================================

        private void OnDifficultyChanged(DifficultyLevel difficulty)
        {
            Debug.Log($"[BattleMissionScreen] Difficulty changed to: {difficulty}");

            // Refresh zone states for new difficulty
            RefreshZoneStates();

            // Update description
            UpdateDifficultyDescription(difficulty);

            // Hide zone info when difficulty changes
            HideZoneInfo();
        }

        // ============================================
        // Navigation
        // ============================================

        private void NavigateToMissions()
        {
            if (ServiceLocator.TryGet<IGameManager>(out var gameManager))
            {
                gameManager.ChangeState(GameState.Missions);
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
                sequence.Append(_titleText.transform.DOScale(1f, 0.3f).SetEase(Ease.OutBack));
            }

            // Animate zone nodes with stagger
            for (int i = 0; i < _zoneNodes.Length; i++)
            {
                var node = _zoneNodes[i];
                if (node != null)
                {
                    node.PlayEntranceAnimation(i * _nodeEntranceDelay);
                }
            }

            // Animate difficulty selector
            if (_difficultySelector != null)
            {
                _difficultySelector.transform.localScale = Vector3.zero;
                sequence.Append(_difficultySelector.transform.DOScale(1f, 0.3f)
                    .SetEase(Ease.OutBack)
                    .SetDelay(0.2f));
            }

            sequence.SetLink(gameObject);
            _currentTween = sequence;
        }

        // ============================================
        // Cleanup
        // ============================================

        private void OnDestroy()
        {
            _currentTween?.Kill();

            // Unsubscribe from events
            foreach (var node in _zoneNodes)
            {
                if (node != null)
                {
                    node.OnZoneSelected -= OnZoneSelected;
                }
            }

            if (_difficultySelector != null)
            {
                _difficultySelector.OnDifficultyChanged -= OnDifficultyChanged;
            }
        }
    }
}
