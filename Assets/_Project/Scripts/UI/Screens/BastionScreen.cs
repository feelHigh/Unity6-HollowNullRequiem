// ============================================
// BastionScreen.cs
// Hub screen (Command Center) for run preparation
// ============================================

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using HNR.Core;
using HNR.Core.Interfaces;
using HNR.Core.Events;
using HNR.Characters;
using HNR.UI.Components;

namespace HNR.UI
{
    /// <summary>
    /// Bastion hub screen where players prepare for runs.
    /// Shows current team, currencies, and provides navigation to start runs.
    /// Reference: HollowNullRequiem_Mockup.jsx lines 1010-1154
    /// </summary>
    public class BastionScreen : ScreenBase
    {
        // ============================================
        // Header Section
        // ============================================

        [Header("Header")]
        [SerializeField, Tooltip("Title text displaying 'THE BASTION'")]
        private TMP_Text _titleText;

        [SerializeField, Tooltip("Subtitle text")]
        private TMP_Text _subtitleText;

        [SerializeField, Tooltip("Soul Crystals currency ticker")]
        private CurrencyTicker _soulCrystalsTicker;

        [SerializeField, Tooltip("Void Dust currency ticker")]
        private CurrencyTicker _voidDustTicker;

        // ============================================
        // Team Display Section
        // ============================================

        [Header("Team Display")]
        [SerializeField, Tooltip("Container for team member slots")]
        private Transform _teamContainer;

        [SerializeField, Tooltip("Team slot UI components (3 slots)")]
        private RequiemSlotUI[] _teamSlots;

        [SerializeField, Tooltip("Team section title (SELECTED TEAM)")]
        private TMP_Text _teamSectionTitle;

        [Header("Team Stats")]
        [SerializeField, Tooltip("Total HP display")]
        private TMP_Text _teamHPText;

        [SerializeField, Tooltip("Total ATK display")]
        private TMP_Text _teamATKText;

        [SerializeField, Tooltip("Total DEF display")]
        private TMP_Text _teamDEFText;

        // ============================================
        // Action Buttons
        // ============================================

        [Header("Action Buttons")]
        [SerializeField, Tooltip("Start new run button")]
        private Button _newRunButton;

        [SerializeField, Tooltip("Change team button")]
        private Button _changeTeamButton;

        [SerializeField, Tooltip("View deck button")]
        private Button _viewDeckButton;

        [SerializeField, Tooltip("Continue saved run button (hidden if no save)")]
        private Button _continueRunButton;

        [SerializeField, Tooltip("CanvasGroup for continue button visibility")]
        private CanvasGroup _continueButtonGroup;

        // ============================================
        // Animation Settings
        // ============================================

        [Header("Animation")]
        [SerializeField] private float _fadeAnimDuration = 0.3f;
        [SerializeField] private float _teamSlotStaggerDelay = 0.1f;

        // ============================================
        // State
        // ============================================

        private List<RequiemDataSO> _currentTeam = new List<RequiemDataSO>();
        private bool _hasSavedRun;

        // ============================================
        // Screen Lifecycle
        // ============================================

        public override void OnShow()
        {
            base.OnShow();

            SetupButtons();
            LoadTeamData();
            LoadCurrencyData();
            RefreshContinueButton();
            PlayShowAnimation();

            // Subscribe to currency changes
            EventBus.Subscribe<CurrencyChangedEvent>(OnCurrencyChanged);

            Debug.Log("[BastionScreen] Bastion hub shown");
        }

        public override void OnHide()
        {
            base.OnHide();

            // Unsubscribe from events
            EventBus.Unsubscribe<CurrencyChangedEvent>(OnCurrencyChanged);

            // Kill any running tweens
            DOTween.Kill(this);

            Debug.Log("[BastionScreen] Bastion hub hidden");
        }

        public override void OnResume()
        {
            base.OnResume();

            // Refresh data when returning from other screens
            LoadTeamData();
            RefreshContinueButton();
        }

        // ============================================
        // Setup
        // ============================================

        private void SetupButtons()
        {
            if (_newRunButton != null)
            {
                _newRunButton.onClick.RemoveAllListeners();
                _newRunButton.onClick.AddListener(OnNewRunClicked);
            }

            if (_changeTeamButton != null)
            {
                _changeTeamButton.onClick.RemoveAllListeners();
                _changeTeamButton.onClick.AddListener(OnChangeTeamClicked);
            }

            if (_viewDeckButton != null)
            {
                _viewDeckButton.onClick.RemoveAllListeners();
                _viewDeckButton.onClick.AddListener(OnViewDeckClicked);
            }

            if (_continueRunButton != null)
            {
                _continueRunButton.onClick.RemoveAllListeners();
                _continueRunButton.onClick.AddListener(OnContinueRunClicked);
            }
        }

        // ============================================
        // Data Loading
        // ============================================

        private void LoadTeamData()
        {
            _currentTeam.Clear();

            var saveManager = ServiceLocator.Get<ISaveManager>();
            if (saveManager != null)
            {
                var saveData = saveManager.LoadRun();
                if (saveData?.Team?.RequiemIds != null && saveData.Team.RequiemIds.Count > 0)
                {
                    // Load RequiemDataSO assets from saved IDs
                    var allRequiems = Resources.LoadAll<RequiemDataSO>("Data/Characters/Requiems");
                    foreach (var id in saveData.Team.RequiemIds)
                    {
                        var requiem = System.Array.Find(allRequiems, r => r.RequiemId == id);
                        if (requiem != null)
                        {
                            _currentTeam.Add(requiem);
                        }
                    }
                }
            }

            // If no team saved, load default team
            if (_currentTeam.Count == 0)
            {
                LoadDefaultTeam();
            }

            UpdateTeamDisplay();
            UpdateTeamStats();
        }

        private void LoadDefaultTeam()
        {
            // Load default Requiems from Resources or predefined assets
            var defaultRequiems = Resources.LoadAll<RequiemDataSO>("Data/Characters/Requiems");

            // Take first 3 available Requiems
            for (int i = 0; i < Mathf.Min(3, defaultRequiems.Length); i++)
            {
                _currentTeam.Add(defaultRequiems[i]);
            }

            if (_currentTeam.Count == 0)
            {
                Debug.LogWarning("[BastionScreen] No default Requiems found in Resources");
            }
        }

        private void LoadCurrencyData()
        {
            // Note: Soul Crystals and Void Dust are meta-currencies stored separately
            // For now, display placeholder values - actual values would come from MetaSaveData
            int soulCrystals = 0;
            int voidDust = 0;

            var saveManager = ServiceLocator.Get<ISaveManager>();
            if (saveManager != null)
            {
                // Try to get meta currency data if available
                // This would typically be loaded from a separate meta save file
                var runData = saveManager.LoadRun();
                if (runData?.Progression != null)
                {
                    // VoidShards is the run-specific currency
                    voidDust = runData.Progression.VoidShards;
                }
            }

            if (_soulCrystalsTicker != null)
                _soulCrystalsTicker.SetValueImmediate(soulCrystals);

            if (_voidDustTicker != null)
                _voidDustTicker.SetValueImmediate(voidDust);
        }

        // ============================================
        // Team Display
        // ============================================

        private void UpdateTeamDisplay()
        {
            if (_teamSlots == null) return;

            for (int i = 0; i < _teamSlots.Length; i++)
            {
                if (_teamSlots[i] == null) continue;

                if (i < _currentTeam.Count && _currentTeam[i] != null)
                {
                    _teamSlots[i].gameObject.SetActive(true);
                    _teamSlots[i].Initialize(_currentTeam[i], OnTeamSlotClicked);
                }
                else
                {
                    // Empty slot
                    _teamSlots[i].gameObject.SetActive(false);
                }
            }
        }

        private void UpdateTeamStats()
        {
            int totalHP = 0;
            int totalATK = 0;
            int totalDEF = 0;

            foreach (var requiem in _currentTeam)
            {
                if (requiem != null)
                {
                    totalHP += requiem.BaseHP;
                    totalATK += requiem.BaseATK;
                    totalDEF += requiem.BaseDEF;
                }
            }

            if (_teamHPText != null)
                _teamHPText.text = $"{totalHP} HP";

            if (_teamATKText != null)
                _teamATKText.text = $"{totalATK} ATK";

            if (_teamDEFText != null)
                _teamDEFText.text = $"{totalDEF} DEF";
        }

        // ============================================
        // Continue Button
        // ============================================

        private void RefreshContinueButton()
        {
            var saveManager = ServiceLocator.Get<ISaveManager>();
            _hasSavedRun = saveManager?.HasSavedRun ?? false;

            if (_continueRunButton != null)
            {
                _continueRunButton.gameObject.SetActive(_hasSavedRun);
            }

            if (_hasSavedRun && _continueButtonGroup != null)
            {
                // Animate fade in
                _continueButtonGroup.alpha = 0f;
                _continueButtonGroup.DOFade(1f, _fadeAnimDuration).SetEase(Ease.OutQuad);
            }

            Debug.Log($"[BastionScreen] Saved run exists: {_hasSavedRun}");
        }

        // ============================================
        // Event Handlers
        // ============================================

        private void OnCurrencyChanged(CurrencyChangedEvent evt)
        {
            if (evt.CurrencyType == CurrencyType.SoulCrystals && _soulCrystalsTicker != null)
            {
                _soulCrystalsTicker.AnimateToValue(evt.NewValue);
            }
            else if (evt.CurrencyType == CurrencyType.VoidDust && _voidDustTicker != null)
            {
                _voidDustTicker.AnimateToValue(evt.NewValue);
            }
        }

        private void OnTeamSlotClicked(RequiemDataSO requiem)
        {
            // Show Requiem details or navigate to detail view
            Debug.Log($"[BastionScreen] Team slot clicked: {requiem?.RequiemName ?? "null"}");

            // TODO: Show Requiem detail popup or navigate to RequiemDetailScreen
        }

        // ============================================
        // Button Handlers
        // ============================================

        private void OnNewRunClicked()
        {
            Debug.Log("[BastionScreen] New Run clicked");

            // Clear existing save if any
            var saveManager = ServiceLocator.Get<ISaveManager>();
            if (saveManager?.HasSavedRun == true)
            {
                saveManager.DeleteRun();
                Debug.Log("[BastionScreen] Deleted existing saved run");
            }

            // Navigate to Requiem selection if team not complete
            if (_currentTeam.Count < 3)
            {
                NavigateToRequiemSelection();
            }
            else
            {
                // Team is ready, start the run
                StartNewRun();
            }
        }

        private void OnChangeTeamClicked()
        {
            Debug.Log("[BastionScreen] Change Team clicked");
            NavigateToRequiemSelection();
        }

        private void OnViewDeckClicked()
        {
            Debug.Log("[BastionScreen] View Deck clicked");

            // TODO: Navigate to deck viewer screen
            // var uiManager = ServiceLocator.Get<IUIManager>();
            // uiManager?.ShowScreen<DeckViewerScreen>();
        }

        private void OnContinueRunClicked()
        {
            Debug.Log("[BastionScreen] Continue Run clicked");

            var runManager = ServiceLocator.Get<IRunManager>();
            if (runManager != null && runManager.LoadRun())
            {
                // Transition to map scene
                var gameManager = ServiceLocator.Get<IGameManager>();
                gameManager?.ChangeState(GameState.Run);
            }
            else
            {
                Debug.LogWarning("[BastionScreen] Failed to load saved run");
                RefreshContinueButton();
            }
        }

        // ============================================
        // Navigation
        // ============================================

        private void NavigateToRequiemSelection()
        {
            var uiManager = ServiceLocator.Get<IUIManager>();
            if (uiManager != null)
            {
                uiManager.ShowScreen<RequiemSelectionScreen>();
            }
            else
            {
                Debug.LogWarning("[BastionScreen] UIManager not available for navigation");
            }
        }

        private void StartNewRun()
        {
            Debug.Log("[BastionScreen] Starting new run with current team");

            var runManager = ServiceLocator.Get<IRunManager>();
            if (runManager != null)
            {
                runManager.InitializeNewRun(_currentTeam);
            }

            // Transition to map
            var gameManager = ServiceLocator.Get<IGameManager>();
            if (gameManager != null)
            {
                gameManager.ChangeState(GameState.Run);
            }
        }

        // ============================================
        // Animation
        // ============================================

        protected override void PlayShowAnimation()
        {
            // Fade in team slots with stagger
            if (_teamSlots != null)
            {
                for (int i = 0; i < _teamSlots.Length; i++)
                {
                    if (_teamSlots[i] == null) continue;

                    var canvasGroup = _teamSlots[i].GetComponent<CanvasGroup>();
                    if (canvasGroup == null)
                    {
                        canvasGroup = _teamSlots[i].gameObject.AddComponent<CanvasGroup>();
                    }

                    canvasGroup.alpha = 0f;
                    canvasGroup.DOFade(1f, _fadeAnimDuration)
                        .SetDelay(i * _teamSlotStaggerDelay)
                        .SetEase(Ease.OutQuad);

                    // Scale punch
                    _teamSlots[i].transform.localScale = Vector3.one * 0.8f;
                    _teamSlots[i].transform.DOScale(1f, _fadeAnimDuration)
                        .SetDelay(i * _teamSlotStaggerDelay)
                        .SetEase(Ease.OutBack);
                }
            }

            // Fade in buttons
            if (_newRunButton != null)
            {
                _newRunButton.transform.localScale = Vector3.one * 0.9f;
                _newRunButton.transform.DOScale(1f, _fadeAnimDuration)
                    .SetDelay(0.2f)
                    .SetEase(Ease.OutBack);
            }
        }

        // ============================================
        // Back Button
        // ============================================

        public override bool OnBackPressed()
        {
            // Navigate to main menu
            var gameManager = ServiceLocator.Get<IGameManager>();
            gameManager?.ChangeState(GameState.MainMenu);
            return true;
        }

        // ============================================
        // Public Methods
        // ============================================

        /// <summary>
        /// Sets the team to display. Called after Requiem selection.
        /// </summary>
        /// <param name="team">List of selected Requiems.</param>
        public void SetTeam(List<RequiemDataSO> team)
        {
            _currentTeam.Clear();
            if (team != null)
            {
                _currentTeam.AddRange(team);
            }

            UpdateTeamDisplay();
            UpdateTeamStats();
        }

        /// <summary>
        /// Force refresh the screen data.
        /// </summary>
        public void RefreshUI()
        {
            LoadTeamData();
            LoadCurrencyData();
            RefreshContinueButton();
        }
    }
}
