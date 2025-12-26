// ============================================
// RequiemsListScreen.cs
// Requiem roster screen with portrait grid
// ============================================

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using HNR.Core;
using HNR.Core.Interfaces;
using HNR.Characters;
using HNR.UI.Components;

namespace HNR.UI.Screens
{
    /// <summary>
    /// Screen displaying a grid of Requiem portraits.
    /// Clicking a portrait opens the detail panel.
    /// </summary>
    public class RequiemsListScreen : ScreenBase
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

        [Header("Portrait Grid")]
        [SerializeField, Tooltip("Container for portrait buttons")]
        private Transform _portraitContainer;

        [SerializeField, Tooltip("Portrait button prefab")]
        private GameObject _portraitButtonPrefab;

        [Header("Requiem Data")]
        [SerializeField, Tooltip("Requiem data assets to display")]
        private RequiemDataSO[] _requiemData;

        [Header("Detail Panel")]
        [SerializeField, Tooltip("Detail panel component")]
        private RequiemDetailPanel _detailPanel;

        [Header("Animation")]
        [SerializeField] private float _portraitEntranceDelay = 0.1f;

        // ============================================
        // State
        // ============================================

        private List<RequiemPortraitButton> _portraitButtons = new();
        private RequiemDataSO _selectedRequiem;
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
            LoadRequiemData();
            CreatePortraitButtons();
            HideDetailPanel();
            PlayEntranceAnimation();
        }

        public override void OnHide()
        {
            base.OnHide();
            _currentTween?.Kill();
            ClearPortraitButtons();
        }

        public override bool OnBackPressed()
        {
            // If detail panel is showing, close it first
            if (_detailPanel != null && _detailPanel.IsShowing)
            {
                _detailPanel.Hide();
                return true;
            }

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

            // Title
            if (_titleText != null)
                _titleText.text = "Requiems";
        }

        private void LoadRequiemData()
        {
            // If no data assigned, try to load from Resources
            if (_requiemData == null || _requiemData.Length == 0)
            {
                _requiemData = Resources.LoadAll<RequiemDataSO>("Data/Characters/Requiems");
            }

            Debug.Log($"[RequiemsListScreen] Loaded {_requiemData?.Length ?? 0} Requiems");
        }

        // ============================================
        // Portrait Button Management
        // ============================================

        private void CreatePortraitButtons()
        {
            ClearPortraitButtons();

            if (_requiemData == null || _portraitContainer == null) return;

            for (int i = 0; i < _requiemData.Length; i++)
            {
                var requiem = _requiemData[i];
                if (requiem == null) continue;

                RequiemPortraitButton button = CreatePortraitButton(requiem);
                if (button != null)
                {
                    _portraitButtons.Add(button);
                }
            }
        }

        private RequiemPortraitButton CreatePortraitButton(RequiemDataSO requiem)
        {
            GameObject buttonObj;

            if (_portraitButtonPrefab != null)
            {
                buttonObj = Instantiate(_portraitButtonPrefab, _portraitContainer);
            }
            else
            {
                // Create simple button if no prefab
                buttonObj = CreateSimplePortraitButton(requiem);
            }

            var portraitButton = buttonObj.GetComponent<RequiemPortraitButton>();
            if (portraitButton == null)
            {
                portraitButton = buttonObj.AddComponent<RequiemPortraitButton>();

                // Wire the button field using reflection since _button is private/serialized
                var buttonComponent = buttonObj.GetComponent<Button>();
                if (buttonComponent != null)
                {
                    var buttonField = typeof(RequiemPortraitButton).GetField("_button",
                        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    if (buttonField != null)
                    {
                        buttonField.SetValue(portraitButton, buttonComponent);
                    }

                    // Also wire the click event directly as fallback
                    buttonComponent.onClick.AddListener(() => OnPortraitClicked(requiem));
                }
            }

            portraitButton.SetRequiemData(requiem);
            portraitButton.OnPortraitClicked += OnPortraitClicked;
            portraitButton.OnPortraitHovered += OnPortraitHovered;
            portraitButton.OnPortraitUnhovered += OnPortraitUnhovered;

            return portraitButton;
        }

        private GameObject CreateSimplePortraitButton(RequiemDataSO requiem)
        {
            // Create a simple portrait button layout
            var buttonObj = new GameObject($"Portrait_{requiem.RequiemName}");
            buttonObj.transform.SetParent(_portraitContainer, false);

            // Add layout element
            var layoutElement = buttonObj.AddComponent<LayoutElement>();
            layoutElement.preferredWidth = 150;
            layoutElement.preferredHeight = 200;

            // Add button
            var button = buttonObj.AddComponent<Button>();
            var buttonImage = buttonObj.AddComponent<Image>();
            buttonImage.color = new Color(0.2f, 0.2f, 0.25f, 0.9f);

            // Create portrait image
            var portraitObj = new GameObject("Portrait");
            portraitObj.transform.SetParent(buttonObj.transform, false);

            var portraitRect = portraitObj.AddComponent<RectTransform>();
            portraitRect.anchorMin = new Vector2(0.1f, 0.3f);
            portraitRect.anchorMax = new Vector2(0.9f, 0.95f);
            portraitRect.offsetMin = Vector2.zero;
            portraitRect.offsetMax = Vector2.zero;

            var portraitImage = portraitObj.AddComponent<Image>();
            if (requiem.Portrait != null)
            {
                portraitImage.sprite = requiem.Portrait;
            }

            // Create name text
            var nameObj = new GameObject("Name");
            nameObj.transform.SetParent(buttonObj.transform, false);

            var nameRect = nameObj.AddComponent<RectTransform>();
            nameRect.anchorMin = new Vector2(0, 0);
            nameRect.anchorMax = new Vector2(1, 0.25f);
            nameRect.offsetMin = new Vector2(5, 5);
            nameRect.offsetMax = new Vector2(-5, -5);

            var nameText = nameObj.AddComponent<TextMeshProUGUI>();
            nameText.text = requiem.RequiemName;
            nameText.fontSize = 16;
            nameText.color = Color.white;
            nameText.alignment = TextAlignmentOptions.Center;

            return buttonObj;
        }

        private void ClearPortraitButtons()
        {
            foreach (var button in _portraitButtons)
            {
                if (button != null)
                {
                    button.OnPortraitClicked -= OnPortraitClicked;
                    button.OnPortraitHovered -= OnPortraitHovered;
                    button.OnPortraitUnhovered -= OnPortraitUnhovered;
                    Destroy(button.gameObject);
                }
            }
            _portraitButtons.Clear();
        }

        // ============================================
        // Portrait Event Handlers
        // ============================================

        private void OnPortraitClicked(RequiemDataSO requiem)
        {
            if (requiem == null) return;

            _selectedRequiem = requiem;
            ShowDetailPanel(requiem);
        }

        private void OnPortraitHovered(RequiemDataSO requiem)
        {
            // Optional: Show quick tooltip or highlight
        }

        private void OnPortraitUnhovered()
        {
            // Optional: Hide quick tooltip
        }

        // ============================================
        // Detail Panel
        // ============================================

        private void ShowDetailPanel(RequiemDataSO requiem)
        {
            if (_detailPanel != null)
            {
                _detailPanel.ShowRequiemDetails(requiem, _requiemData);
            }
            else
            {
                Debug.LogWarning("[RequiemsListScreen] Detail panel not assigned");
            }
        }

        private void HideDetailPanel()
        {
            if (_detailPanel != null)
            {
                _detailPanel.Hide();
            }
        }

        /// <summary>
        /// Called by detail panel when a different Requiem is selected.
        /// </summary>
        public void OnDetailPanelRequiemChanged(RequiemDataSO requiem)
        {
            _selectedRequiem = requiem;

            // Update selection state on portrait buttons
            foreach (var button in _portraitButtons)
            {
                if (button != null)
                {
                    button.SetSelected(button.RequiemData == requiem);
                }
            }
        }

        // ============================================
        // Button Handlers
        // ============================================

        private void OnBackClicked()
        {
            if (_detailPanel != null && _detailPanel.IsShowing)
            {
                _detailPanel.Hide();
                return;
            }

            NavigateToBastion();
        }

        private void OnSettingsClicked()
        {
            SettingsOverlay.ShowSettings();
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
                sequence.Append(_titleText.transform.DOScale(1f, 0.3f).SetEase(Ease.OutBack));
            }

            // Animate portrait buttons with stagger
            for (int i = 0; i < _portraitButtons.Count; i++)
            {
                var button = _portraitButtons[i];
                if (button != null)
                {
                    button.PlayEntranceAnimation(i * _portraitEntranceDelay);
                }
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
            ClearPortraitButtons();
        }
    }
}
