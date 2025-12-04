// ============================================
// RequiemSelectionScreen.cs
// Team selection screen before starting a run
// ============================================

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using HNR.Core;
using HNR.Core.Interfaces;
using HNR.Core.Events;
using HNR.Characters;

namespace HNR.UI
{
    /// <summary>
    /// Screen for selecting up to 3 Requiems before starting a run.
    /// Displays all available Requiems with detailed stats preview.
    /// </summary>
    public class RequiemSelectionScreen : ScreenBase
    {
        // ============================================
        // References
        // ============================================

        [Header("Slot Configuration")]
        [SerializeField, Tooltip("Container for Requiem slots")]
        private Transform _slotContainer;

        [SerializeField, Tooltip("Prefab for individual Requiem slots")]
        private RequiemSlotUI _slotPrefab;

        [Header("Selection UI")]
        [SerializeField, Tooltip("Button to start the run")]
        private Button _startRunButton;

        [SerializeField, Tooltip("Text showing selected count")]
        private TextMeshProUGUI _selectedCountText;

        [SerializeField, Tooltip("Button to go back")]
        private Button _backButton;

        [Header("Preview Panel")]
        [SerializeField, Tooltip("Preview panel container")]
        private GameObject _previewPanel;

        [SerializeField, Tooltip("Preview portrait image")]
        private Image _previewPortrait;

        [SerializeField, Tooltip("Preview name text")]
        private TextMeshProUGUI _previewNameText;

        [SerializeField, Tooltip("Preview title text")]
        private TextMeshProUGUI _previewTitleText;

        [SerializeField, Tooltip("Preview class/aspect text")]
        private TextMeshProUGUI _previewClassText;

        [SerializeField, Tooltip("Preview stats text")]
        private TextMeshProUGUI _previewStatsText;

        [SerializeField, Tooltip("Preview backstory text")]
        private TextMeshProUGUI _previewBackstoryText;

        [SerializeField, Tooltip("Preview Requiem Art name")]
        private TextMeshProUGUI _previewArtNameText;

        [SerializeField, Tooltip("Preview Requiem Art description")]
        private TextMeshProUGUI _previewArtDescText;

        [SerializeField, Tooltip("Preview starting cards count")]
        private TextMeshProUGUI _previewCardsText;

        [Header("Team Stats")]
        [SerializeField, Tooltip("Combined team HP text")]
        private TextMeshProUGUI _teamHPText;

        [SerializeField, Tooltip("Combined team ATK text")]
        private TextMeshProUGUI _teamATKText;

        [SerializeField, Tooltip("Combined team DEF text")]
        private TextMeshProUGUI _teamDEFText;

        [Header("Data")]
        [SerializeField, Tooltip("All available Requiems")]
        private RequiemDataSO[] _availableRequiems;

        // ============================================
        // State
        // ============================================

        private List<RequiemSlotUI> _slots = new();
        private List<RequiemDataSO> _selectedRequiems = new();
        private RequiemDataSO _previewedRequiem;

        private const int MAX_TEAM_SIZE = 3;

        // ============================================
        // Lifecycle
        // ============================================

        private void Awake()
        {
            // Wire button listeners
            if (_startRunButton != null)
                _startRunButton.onClick.AddListener(OnStartRunClicked);

            if (_backButton != null)
                _backButton.onClick.AddListener(OnBackClicked);
        }

        public override void OnShow()
        {
            base.OnShow();
            _selectedRequiems.Clear();
            InitializeSlots();
            UpdateUI();
            HidePreview();
        }

        public override void OnHide()
        {
            base.OnHide();
            ClearSlots();
        }

        // ============================================
        // Slot Management
        // ============================================

        private void InitializeSlots()
        {
            ClearSlots();

            if (_availableRequiems == null || _slotPrefab == null || _slotContainer == null)
            {
                Debug.LogWarning("[RequiemSelectionScreen] Missing references for slot initialization");
                return;
            }

            foreach (var requiem in _availableRequiems)
            {
                if (requiem == null) continue;

                var slot = Instantiate(_slotPrefab, _slotContainer);
                slot.Initialize(requiem, OnSlotClicked, OnSlotHovered);
                _slots.Add(slot);
            }

            Debug.Log($"[RequiemSelectionScreen] Initialized {_slots.Count} Requiem slots");
        }

        private void ClearSlots()
        {
            foreach (var slot in _slots)
            {
                if (slot != null)
                    Destroy(slot.gameObject);
            }
            _slots.Clear();
        }

        // ============================================
        // Slot Callbacks
        // ============================================

        private void OnSlotClicked(RequiemDataSO requiem)
        {
            if (requiem == null) return;

            if (_selectedRequiems.Contains(requiem))
            {
                // Deselect
                _selectedRequiems.Remove(requiem);
                Debug.Log($"[RequiemSelectionScreen] Deselected: {requiem.RequiemName}");
            }
            else if (_selectedRequiems.Count < MAX_TEAM_SIZE)
            {
                // Select
                _selectedRequiems.Add(requiem);
                Debug.Log($"[RequiemSelectionScreen] Selected: {requiem.RequiemName}");
            }
            else
            {
                Debug.Log($"[RequiemSelectionScreen] Max team size reached ({MAX_TEAM_SIZE})");
            }

            UpdateUI();
        }

        private void OnSlotHovered(RequiemDataSO requiem)
        {
            if (requiem != null)
            {
                ShowPreview(requiem);
            }
        }

        // ============================================
        // UI Updates
        // ============================================

        private void UpdateUI()
        {
            // Update slot selection states
            foreach (var slot in _slots)
            {
                if (slot != null && slot.Requiem != null)
                {
                    slot.SetSelected(_selectedRequiems.Contains(slot.Requiem));
                }
            }

            // Update selection count text
            if (_selectedCountText != null)
            {
                _selectedCountText.text = $"{_selectedRequiems.Count}/{MAX_TEAM_SIZE}";

                // Color based on selection state
                _selectedCountText.color = _selectedRequiems.Count == MAX_TEAM_SIZE
                    ? new Color(0.3f, 0.9f, 0.3f) // Green when full
                    : Color.white;
            }

            // Update start button
            if (_startRunButton != null)
            {
                _startRunButton.interactable = _selectedRequiems.Count == MAX_TEAM_SIZE;
            }

            // Update team stats
            UpdateTeamStats();
        }

        private void UpdateTeamStats()
        {
            int totalHP = 0;
            int totalATK = 0;
            int totalDEF = 0;

            foreach (var requiem in _selectedRequiems)
            {
                totalHP += requiem.BaseHP;
                totalATK += requiem.BaseATK;
                totalDEF += requiem.BaseDEF;
            }

            if (_teamHPText != null)
                _teamHPText.text = $"Team HP: {totalHP}";

            if (_teamATKText != null)
                _teamATKText.text = $"Team ATK: {totalATK}";

            if (_teamDEFText != null)
                _teamDEFText.text = $"Team DEF: {totalDEF}";
        }

        // ============================================
        // Preview Panel
        // ============================================

        private void ShowPreview(RequiemDataSO requiem)
        {
            _previewedRequiem = requiem;

            if (_previewPanel != null)
                _previewPanel.SetActive(true);

            // Portrait
            if (_previewPortrait != null && requiem.Portrait != null)
                _previewPortrait.sprite = requiem.Portrait;

            // Identity
            if (_previewNameText != null)
                _previewNameText.text = requiem.RequiemName;

            if (_previewTitleText != null)
                _previewTitleText.text = requiem.Title;

            // Classification
            if (_previewClassText != null)
                _previewClassText.text = $"{requiem.Class} - {requiem.SoulAspect}";

            // Stats
            if (_previewStatsText != null)
            {
                _previewStatsText.text = $"HP: {requiem.BaseHP}\n" +
                                         $"ATK: {requiem.BaseATK}\n" +
                                         $"DEF: {requiem.BaseDEF}\n" +
                                         $"SE Rate: {requiem.SERate:F1}x";
            }

            // Backstory
            if (_previewBackstoryText != null)
                _previewBackstoryText.text = requiem.Backstory;

            // Requiem Art
            if (requiem.RequiemArt != null)
            {
                if (_previewArtNameText != null)
                    _previewArtNameText.text = requiem.RequiemArt.ArtName;

                if (_previewArtDescText != null)
                    _previewArtDescText.text = requiem.RequiemArt.Description;
            }

            // Cards
            if (_previewCardsText != null)
                _previewCardsText.text = $"Starting Cards: {requiem.StartingCardCount}";
        }

        private void HidePreview()
        {
            _previewedRequiem = null;

            if (_previewPanel != null)
                _previewPanel.SetActive(false);
        }

        // ============================================
        // Button Handlers
        // ============================================

        public void OnStartRunClicked()
        {
            if (_selectedRequiems.Count != MAX_TEAM_SIZE)
            {
                Debug.LogWarning("[RequiemSelectionScreen] Cannot start run - team not full");
                return;
            }

            // Publish team selection event
            EventBus.Publish(new TeamSelectedEvent(_selectedRequiems.ToArray()));

            // Start the run via GameManager
            var gameManager = ServiceLocator.Get<IGameManager>();
            if (gameManager != null)
            {
                Debug.Log($"[RequiemSelectionScreen] Starting run with team: " +
                          $"{string.Join(", ", _selectedRequiems.ConvertAll(r => r.RequiemName))}");
                gameManager.StartNewRun();
            }
            else
            {
                Debug.LogError("[RequiemSelectionScreen] GameManager not found!");
            }
        }

        private void OnBackClicked()
        {
            // Navigate back to Bastion
            var gameManager = ServiceLocator.Get<IGameManager>();
            gameManager?.ChangeState(GameState.Bastion);
        }

        public override bool OnBackPressed()
        {
            OnBackClicked();
            return true;
        }

        // ============================================
        // Public API
        // ============================================

        /// <summary>
        /// Get the currently selected Requiems.
        /// </summary>
        public IReadOnlyList<RequiemDataSO> SelectedRequiems => _selectedRequiems;

        /// <summary>
        /// Programmatically select a Requiem.
        /// </summary>
        public void SelectRequiem(RequiemDataSO requiem)
        {
            if (requiem != null && !_selectedRequiems.Contains(requiem) && _selectedRequiems.Count < MAX_TEAM_SIZE)
            {
                _selectedRequiems.Add(requiem);
                UpdateUI();
            }
        }

        /// <summary>
        /// Clear all selections.
        /// </summary>
        public void ClearSelection()
        {
            _selectedRequiems.Clear();
            UpdateUI();
        }
    }
}
