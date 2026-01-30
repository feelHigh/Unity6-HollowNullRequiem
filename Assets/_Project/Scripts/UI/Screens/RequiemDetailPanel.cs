// ============================================
// RequiemDetailPanel.cs
// Detail panel showing Requiem stats and cards
// ============================================

using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using HNR.Characters;
using HNR.UI.Components;
using HNR.UI.Config;

namespace HNR.UI.Screens
{
    /// <summary>
    /// Panel showing detailed information about a Requiem.
    /// Includes stats tab and cards tab.
    /// </summary>
    public class RequiemDetailPanel : MonoBehaviour
    {
        // ============================================
        // Events
        // ============================================

        /// <summary>Invoked when a different Requiem is selected via sidebar.</summary>
        public event Action<RequiemDataSO> OnRequiemChanged;

        // ============================================
        // Serialized Fields
        // ============================================

        [Header("Container")]
        [SerializeField, Tooltip("Main panel canvas group")]
        private CanvasGroup _panelGroup;

        [SerializeField, Tooltip("Panel content")]
        private RectTransform _panelContent;

        [Header("Header")]
        [SerializeField, Tooltip("Close button")]
        private Button _closeButton;

        [SerializeField, Tooltip("Panel title")]
        private TMP_Text _titleText;

        [Header("Left Sidebar - Portrait List")]
        [SerializeField, Tooltip("Container for portrait list")]
        private Transform _portraitListContainer;

        [SerializeField, Tooltip("Sidebar portrait button prefab")]
        private GameObject _sidebarPortraitPrefab;

        [Header("Center - Character Art")]
        [SerializeField, Tooltip("Large character artwork image")]
        private Image _characterArtImage;

        [SerializeField, Tooltip("Character name")]
        private TMP_Text _characterNameText;

        [SerializeField, Tooltip("Character class/aspect")]
        private TMP_Text _characterClassText;

        [Header("Right - Stats Panel")]
        [SerializeField, Tooltip("Attack stat text")]
        private TMP_Text _attackText;

        [SerializeField, Tooltip("Defense stat text")]
        private TMP_Text _defenseText;

        [SerializeField, Tooltip("Health stat text")]
        private TMP_Text _healthText;

        [Header("Tabs")]
        [SerializeField, Tooltip("Stats tab button")]
        private Button _statsTabButton;

        [SerializeField, Tooltip("Cards tab button")]
        private Button _cardsTabButton;

        [SerializeField, Tooltip("Stats tab content")]
        private GameObject _statsTabContent;

        [SerializeField, Tooltip("Cards tab content")]
        private GameObject _cardsTabContent;

        [Header("Cards Display")]
        [SerializeField, Tooltip("Starting cards display")]
        private RequiemCardDisplay _startingCardsDisplay;

        [Header("Tab Colors")]
        [SerializeField] private Color _activeTabColor = new Color(0.9f, 0.7f, 0.2f, 1f);
        [SerializeField] private Color _inactiveTabColor = new Color(0.4f, 0.4f, 0.4f, 1f);

        [Header("Animation")]
        [SerializeField] private float _fadeInDuration = 0.25f;
        [SerializeField] private float _fadeOutDuration = 0.2f;

        // ============================================
        // State
        // ============================================

        private RequiemDataSO _currentRequiem;
        private RequiemDataSO[] _allRequiems;
        private bool _isShowing;
        private int _currentTab; // 0 = Stats, 1 = Cards
        private Tween _currentTween;

        // ============================================
        // Properties
        // ============================================

        /// <summary>Whether the panel is currently visible.</summary>
        public bool IsShowing => _isShowing;

        /// <summary>Currently displayed Requiem.</summary>
        public RequiemDataSO CurrentRequiem => _currentRequiem;

        // ============================================
        // Unity Lifecycle
        // ============================================

        private void Awake()
        {
            // Setup initial state
            if (_panelGroup != null)
            {
                _panelGroup.alpha = 0f;
                _panelGroup.interactable = false;
                _panelGroup.blocksRaycasts = false;
            }

            gameObject.SetActive(false);
        }

        private void OnEnable()
        {
            SetupButtons();
        }

        private void OnDisable()
        {
            CleanupButtons();
        }

        private void OnDestroy()
        {
            _currentTween?.Kill();
        }

        // ============================================
        // Setup
        // ============================================

        private void SetupButtons()
        {
            if (_closeButton != null)
            {
                _closeButton.onClick.AddListener(OnCloseClicked);
            }

            if (_statsTabButton != null)
            {
                _statsTabButton.onClick.AddListener(() => SelectTab(0));
            }

            if (_cardsTabButton != null)
            {
                _cardsTabButton.onClick.AddListener(() => SelectTab(1));
            }
        }

        private void CleanupButtons()
        {
            if (_closeButton != null)
                _closeButton.onClick.RemoveListener(OnCloseClicked);

            if (_statsTabButton != null)
                _statsTabButton.onClick.RemoveAllListeners();

            if (_cardsTabButton != null)
                _cardsTabButton.onClick.RemoveAllListeners();
        }

        // ============================================
        // Public API
        // ============================================

        /// <summary>
        /// Shows the detail panel with the specified Requiem.
        /// </summary>
        public void ShowRequiemDetails(RequiemDataSO requiem, RequiemDataSO[] allRequiems = null)
        {
            _currentRequiem = requiem;
            _allRequiems = allRequiems;

            gameObject.SetActive(true);
            _isShowing = true;

            UpdateDisplay();
            UpdatePortraitList();
            SelectTab(0); // Default to stats tab

            AnimateShow();
        }

        /// <summary>
        /// Hides the detail panel.
        /// </summary>
        public void Hide()
        {
            if (!_isShowing) return;

            AnimateHide();
        }

        /// <summary>
        /// Switches to display a different Requiem.
        /// </summary>
        public void SwitchRequiem(RequiemDataSO requiem)
        {
            if (requiem == null || requiem == _currentRequiem) return;

            _currentRequiem = requiem;
            UpdateDisplay();

            // Notify listeners
            OnRequiemChanged?.Invoke(requiem);
        }

        // ============================================
        // Display Updates
        // ============================================

        private void UpdateDisplay()
        {
            if (_currentRequiem == null) return;

            // Update title
            if (_titleText != null)
                _titleText.text = "Details";

            // Update character art - use full body sprite with full alpha
            if (_characterArtImage != null)
            {
                // Try full body sprite first, fall back to portrait
                var sprite = _currentRequiem.FullBodySprite ?? _currentRequiem.Portrait;
                if (sprite != null)
                {
                    _characterArtImage.sprite = sprite;
                    _characterArtImage.color = Color.white; // Full alpha for proper display
                }
            }

            // Update name
            if (_characterNameText != null)
            {
                _characterNameText.text = _currentRequiem.RequiemName;
            }

            // Update class/aspect
            if (_characterClassText != null)
            {
                _characterClassText.text = $"{_currentRequiem.Class} | {_currentRequiem.SoulAspect}";
            }

            // Update stats
            UpdateStats();

            // Update cards
            UpdateCards();
        }

        private void UpdateStats()
        {
            if (_currentRequiem == null) return;

            if (_attackText != null)
            {
                _attackText.text = _currentRequiem.BaseATK.ToString();
            }

            if (_defenseText != null)
            {
                _defenseText.text = _currentRequiem.BaseDEF.ToString();
            }

            if (_healthText != null)
            {
                _healthText.text = _currentRequiem.BaseHP.ToString();
            }
        }

        private void UpdateCards()
        {
            if (_startingCardsDisplay != null && _currentRequiem != null)
            {
                _startingCardsDisplay.DisplayStartingCards(_currentRequiem);
            }
        }

        // ============================================
        // Portrait List
        // ============================================

        private void UpdatePortraitList()
        {
            // Clear existing portraits
            if (_portraitListContainer != null)
            {
                foreach (Transform child in _portraitListContainer)
                {
                    Destroy(child.gameObject);
                }
            }

            if (_allRequiems == null || _portraitListContainer == null) return;

            // Create portrait buttons for each Requiem
            foreach (var requiem in _allRequiems)
            {
                if (requiem == null) continue;

                CreateSidebarPortrait(requiem);
            }
        }

        private void CreateSidebarPortrait(RequiemDataSO requiem)
        {
            // Use local prefab or fall back to config
            var prefab = _sidebarPortraitPrefab ?? RuntimeUIPrefabConfigSO.Instance?.RequiemPortraitButtonPrefab;

            if (prefab == null)
            {
                Debug.LogError("[RequiemDetailPanel] Sidebar portrait prefab not assigned. Check RuntimeUIPrefabConfig.");
                return;
            }

            var portraitObj = Instantiate(prefab, _portraitListContainer);
            portraitObj.name = $"SidebarPortrait_{requiem.RequiemName}";

            // Set portrait image if available
            var image = portraitObj.GetComponent<Image>();
            if (image != null && requiem.Portrait != null)
            {
                image.sprite = requiem.Portrait;
                image.color = Color.white;
            }

            // Setup button click
            var buttonComponent = portraitObj.GetComponent<Button>();
            if (buttonComponent != null)
            {
                var capturedRequiem = requiem;
                buttonComponent.onClick.AddListener(() => SwitchRequiem(capturedRequiem));
            }

            // Highlight current selection
            if (requiem == _currentRequiem)
            {
                var outline = portraitObj.GetComponent<Outline>();
                if (outline == null)
                {
                    outline = portraitObj.AddComponent<Outline>();
                }
                outline.effectColor = _activeTabColor;
                outline.effectDistance = new Vector2(3, 3);
            }
        }

        // ============================================
        // Tab System
        // ============================================

        private void SelectTab(int tabIndex)
        {
            _currentTab = tabIndex;

            // Update tab button visuals
            UpdateTabButtonVisuals();

            // Show/hide tab content
            if (_statsTabContent != null)
            {
                _statsTabContent.SetActive(tabIndex == 0);
            }

            if (_cardsTabContent != null)
            {
                _cardsTabContent.SetActive(tabIndex == 1);
            }
        }

        private void UpdateTabButtonVisuals()
        {
            // Stats tab
            if (_statsTabButton != null)
            {
                var image = _statsTabButton.GetComponent<Image>();
                if (image != null)
                {
                    image.color = _currentTab == 0 ? _activeTabColor : _inactiveTabColor;
                }
            }

            // Cards tab
            if (_cardsTabButton != null)
            {
                var image = _cardsTabButton.GetComponent<Image>();
                if (image != null)
                {
                    image.color = _currentTab == 1 ? _activeTabColor : _inactiveTabColor;
                }
            }
        }

        // ============================================
        // Button Handlers
        // ============================================

        private void OnCloseClicked()
        {
            Hide();
        }

        // ============================================
        // Animation
        // ============================================

        private void AnimateShow()
        {
            _currentTween?.Kill();

            if (_panelGroup != null)
            {
                _panelGroup.alpha = 0f;
                _panelGroup.interactable = true;
                _panelGroup.blocksRaycasts = true;
            }

            if (_panelContent != null)
            {
                _panelContent.localScale = Vector3.one * 0.9f;
            }

            var sequence = DOTween.Sequence();
            sequence.Append(_panelGroup.DOFade(1f, _fadeInDuration).SetEase(Ease.OutQuad));

            if (_panelContent != null)
            {
                sequence.Join(_panelContent.DOScale(1f, _fadeInDuration).SetEase(Ease.OutBack));
            }

            sequence.SetUpdate(true);
            sequence.SetLink(gameObject);
            _currentTween = sequence;
        }

        private void AnimateHide()
        {
            _currentTween?.Kill();

            var sequence = DOTween.Sequence();

            if (_panelContent != null)
            {
                sequence.Append(_panelContent.DOScale(0.9f, _fadeOutDuration).SetEase(Ease.InQuad));
            }

            sequence.Join(_panelGroup.DOFade(0f, _fadeOutDuration).SetEase(Ease.InQuad));
            sequence.OnComplete(() =>
            {
                if (_panelGroup != null)
                {
                    _panelGroup.interactable = false;
                    _panelGroup.blocksRaycasts = false;
                }
                gameObject.SetActive(false);
                _isShowing = false;
            });

            sequence.SetUpdate(true);
            sequence.SetLink(gameObject);
            _currentTween = sequence;
        }
    }
}
