// ============================================
// DeckViewerModal.cs
// Modal overlay for viewing and selecting cards from deck
// ============================================

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using HNR.Core;
using HNR.Core.Events;
using HNR.Core.Interfaces;
using HNR.Cards;
using HNR.UI.Screens;

namespace HNR.UI.Components
{
    /// <summary>
    /// Modal overlay for viewing deck and selecting cards.
    /// Supports view-only mode and card removal mode.
    /// Used by shop for card removal service.
    /// </summary>
    public class DeckViewerModal : MonoBehaviour
    {
        // ============================================
        // View Mode
        // ============================================

        public enum ViewMode
        {
            ViewOnly,       // Just view the deck
            RemoveCard      // Select a card to remove
        }

        // ============================================
        // UI References
        // ============================================

        [Header("Modal Container")]
        [SerializeField, Tooltip("The modal panel root")]
        private GameObject _modalPanel;

        [SerializeField, Tooltip("Canvas group for fade animation")]
        private CanvasGroup _canvasGroup;

        [SerializeField, Tooltip("Background overlay for blocking input")]
        private Image _backgroundOverlay;

        [Header("Header")]
        [SerializeField, Tooltip("Modal title text")]
        private TMP_Text _titleText;

        [SerializeField, Tooltip("Instruction text")]
        private TMP_Text _instructionText;

        [Header("Card Display")]
        [SerializeField, Tooltip("Container for card slots (with GridLayoutGroup)")]
        private Transform _cardContainer;

        [SerializeField, Tooltip("Card slot prefab")]
        private GameObject _cardSlotPrefab;

        [SerializeField, Tooltip("Scroll rect for card list")]
        private ScrollRect _scrollRect;

        [Header("Actions")]
        [SerializeField, Tooltip("Confirm selection button")]
        private Button _confirmButton;

        [SerializeField, Tooltip("Confirm button text")]
        private TMP_Text _confirmButtonText;

        [SerializeField, Tooltip("Cancel/Close button")]
        private Button _cancelButton;

        [Header("Animation")]
        [SerializeField] private float _fadeInDuration = 0.3f;
        [SerializeField] private float _fadeOutDuration = 0.2f;

        [Header("Colors")]
        [SerializeField] private Color _normalSlotColor = new Color(0.15f, 0.15f, 0.2f);
        [SerializeField] private Color _selectedSlotColor = new Color(0.2f, 0.35f, 0.5f);
        [SerializeField] private Color _hoverSlotColor = new Color(0.2f, 0.25f, 0.3f);

        // ============================================
        // State
        // ============================================

        private ViewMode _currentMode;
        private CardDataSO _selectedCard;
        private int _selectedIndex = -1;
        private Action<CardDataSO> _onCardSelected;
        private List<CardDataSO> _displayedCards = new();
        private List<GameObject> _spawnedSlots = new();

        // ============================================
        // Unity Lifecycle
        // ============================================

        private void Awake()
        {
            // Ensure modal starts hidden
            if (_modalPanel != null)
            {
                _modalPanel.SetActive(false);
            }

            // Setup buttons
            if (_confirmButton != null)
            {
                _confirmButton.onClick.AddListener(OnConfirmClicked);
            }

            if (_cancelButton != null)
            {
                _cancelButton.onClick.AddListener(OnCancelClicked);
            }
        }

        private void OnDestroy()
        {
            if (_confirmButton != null)
            {
                _confirmButton.onClick.RemoveListener(OnConfirmClicked);
            }

            if (_cancelButton != null)
            {
                _cancelButton.onClick.RemoveListener(OnCancelClicked);
            }

            ClearCardSlots();
        }

        // ============================================
        // Public API
        // ============================================

        /// <summary>
        /// Shows the deck viewer modal.
        /// </summary>
        /// <param name="mode">View mode (ViewOnly or RemoveCard)</param>
        /// <param name="onCardSelected">Callback when card is selected/removed (null if cancelled)</param>
        public void Show(ViewMode mode, Action<CardDataSO> onCardSelected = null)
        {
            _currentMode = mode;
            _onCardSelected = onCardSelected;
            _selectedCard = null;
            _selectedIndex = -1;

            // Update UI based on mode
            UpdateModeUI();

            // Populate cards from RunManager
            PopulateCards();

            // Show modal with animation
            if (_modalPanel != null)
            {
                _modalPanel.SetActive(true);
            }

            if (_canvasGroup != null)
            {
                _canvasGroup.alpha = 0f;
                _canvasGroup.DOFade(1f, _fadeInDuration).SetLink(gameObject);
            }

            // Disable confirm until selection
            if (_confirmButton != null)
            {
                _confirmButton.interactable = _currentMode == ViewMode.ViewOnly;
            }

            Debug.Log($"[DeckViewerModal] Showing modal in {mode} mode with {_displayedCards.Count} cards");
        }

        /// <summary>
        /// Hides the deck viewer modal.
        /// </summary>
        public void Hide()
        {
            if (_canvasGroup != null)
            {
                _canvasGroup.DOFade(0f, _fadeOutDuration)
                    .OnComplete(() =>
                    {
                        if (_modalPanel != null)
                        {
                            _modalPanel.SetActive(false);
                        }
                        ClearCardSlots();
                    })
                    .SetLink(gameObject);
            }
            else
            {
                if (_modalPanel != null)
                {
                    _modalPanel.SetActive(false);
                }
                ClearCardSlots();
            }

            Debug.Log("[DeckViewerModal] Modal hidden");
        }

        /// <summary>
        /// Returns whether the modal is currently visible.
        /// </summary>
        public bool IsVisible => _modalPanel != null && _modalPanel.activeSelf;

        // ============================================
        // UI Setup
        // ============================================

        private void UpdateModeUI()
        {
            if (_titleText != null)
            {
                _titleText.text = _currentMode switch
                {
                    ViewMode.RemoveCard => "REMOVE A CARD",
                    _ => "YOUR DECK"
                };
            }

            if (_instructionText != null)
            {
                _instructionText.text = _currentMode switch
                {
                    ViewMode.RemoveCard => "Select a card to remove from your deck",
                    _ => ""
                };
                _instructionText.gameObject.SetActive(_currentMode != ViewMode.ViewOnly);
            }

            if (_confirmButtonText != null)
            {
                _confirmButtonText.text = _currentMode switch
                {
                    ViewMode.RemoveCard => "Remove",
                    _ => "Close"
                };
            }
        }

        private void PopulateCards()
        {
            ClearCardSlots();
            _displayedCards.Clear();

            // Get deck from RunManager
            if (ServiceLocator.TryGet<IRunManager>(out var runManager))
            {
                foreach (var card in runManager.Deck)
                {
                    if (card != null)
                    {
                        _displayedCards.Add(card);
                    }
                }
            }
            else
            {
                Debug.LogWarning("[DeckViewerModal] RunManager not available - cannot load deck");
                return;
            }

            // Create card slots
            if (_cardContainer == null)
            {
                Debug.LogWarning("[DeckViewerModal] Card container not assigned");
                return;
            }

            for (int i = 0; i < _displayedCards.Count; i++)
            {
                CreateCardSlot(_displayedCards[i], i);
            }
        }

        private void CreateCardSlot(CardDataSO card, int index)
        {
            GameObject slot;

            if (_cardSlotPrefab != null)
            {
                slot = Instantiate(_cardSlotPrefab, _cardContainer);
            }
            else
            {
                // Create placeholder slot
                slot = CreatePlaceholderSlot(card);
            }

            if (slot == null) return;

            _spawnedSlots.Add(slot);

            // Setup button click
            var button = slot.GetComponent<Button>();
            if (button != null)
            {
                int capturedIndex = index;
                button.onClick.AddListener(() => OnCardSlotClicked(capturedIndex));
            }

            // Set card data if component exists
            var cardDisplay = slot.GetComponent<ICardDisplay>();
            cardDisplay?.SetCard(card);
        }

        private GameObject CreatePlaceholderSlot(CardDataSO card)
        {
            var slot = new GameObject($"CardSlot_{card?.CardName ?? "Unknown"}");
            slot.transform.SetParent(_cardContainer, false);

            // Add rect transform
            var rect = slot.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(90, 120);

            // Add background image
            var image = slot.AddComponent<Image>();
            image.color = _normalSlotColor;

            // Add button
            var button = slot.AddComponent<Button>();
            button.targetGraphic = image;

            // Card name text
            var textObj = new GameObject("CardName");
            textObj.transform.SetParent(slot.transform, false);

            var textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.sizeDelta = Vector2.zero;
            textRect.offsetMin = new Vector2(5, 5);
            textRect.offsetMax = new Vector2(-5, -5);

            var text = textObj.AddComponent<TextMeshProUGUI>();
            text.text = card?.CardName ?? "Unknown";
            text.fontSize = 11;
            text.alignment = TextAlignmentOptions.Center;
            text.color = Color.white;
            text.textWrappingMode = TMPro.TextWrappingModes.Normal;

            // Cost indicator
            var costObj = new GameObject("Cost");
            costObj.transform.SetParent(slot.transform, false);

            var costRect = costObj.AddComponent<RectTransform>();
            costRect.anchorMin = new Vector2(0, 1);
            costRect.anchorMax = new Vector2(0, 1);
            costRect.pivot = new Vector2(0, 1);
            costRect.anchoredPosition = new Vector2(5, -5);
            costRect.sizeDelta = new Vector2(20, 20);

            var costBg = costObj.AddComponent<Image>();
            costBg.color = new Color(0.2f, 0.4f, 0.8f);

            var costTextObj = new GameObject("CostText");
            costTextObj.transform.SetParent(costObj.transform, false);

            var costTextRect = costTextObj.AddComponent<RectTransform>();
            costTextRect.anchorMin = Vector2.zero;
            costTextRect.anchorMax = Vector2.one;
            costTextRect.sizeDelta = Vector2.zero;

            var costText = costTextObj.AddComponent<TextMeshProUGUI>();
            costText.text = card?.APCost.ToString() ?? "?";
            costText.fontSize = 12;
            costText.alignment = TextAlignmentOptions.Center;
            costText.color = Color.white;
            costText.fontStyle = FontStyles.Bold;

            return slot;
        }

        private void ClearCardSlots()
        {
            foreach (var slot in _spawnedSlots)
            {
                if (slot != null)
                {
                    var button = slot.GetComponent<Button>();
                    button?.onClick.RemoveAllListeners();
                    Destroy(slot);
                }
            }
            _spawnedSlots.Clear();
        }

        // ============================================
        // Card Selection
        // ============================================

        private void OnCardSlotClicked(int index)
        {
            if (_currentMode == ViewMode.ViewOnly) return;
            if (index < 0 || index >= _displayedCards.Count) return;

            _selectedIndex = index;
            _selectedCard = _displayedCards[index];

            Debug.Log($"[DeckViewerModal] Card selected: {_selectedCard.CardName}");

            // Update visual selection
            UpdateSelectionVisuals();

            // Enable confirm button
            if (_confirmButton != null)
            {
                _confirmButton.interactable = true;
            }
        }

        private void UpdateSelectionVisuals()
        {
            for (int i = 0; i < _spawnedSlots.Count; i++)
            {
                var slot = _spawnedSlots[i];
                if (slot == null) continue;

                var image = slot.GetComponent<Image>();
                if (image != null)
                {
                    image.color = (i == _selectedIndex) ? _selectedSlotColor : _normalSlotColor;
                }

                // Scale animation
                Vector3 targetScale = (i == _selectedIndex) ? Vector3.one * 1.05f : Vector3.one;
                slot.transform.DOScale(targetScale, 0.2f).SetLink(slot);
            }
        }

        // ============================================
        // Button Handlers
        // ============================================

        private void OnConfirmClicked()
        {
            if (_currentMode == ViewMode.ViewOnly)
            {
                // Just close
                Hide();
                _onCardSelected?.Invoke(null);
                return;
            }

            if (_selectedCard == null)
            {
                Debug.Log("[DeckViewerModal] No card selected");
                return;
            }

            // Remove the card from deck
            if (ServiceLocator.TryGet<IRunManager>(out var runManager))
            {
                runManager.RemoveCardFromDeck(_selectedCard);
                EventBus.Publish(new CardRemovedFromDeckEvent(_selectedCard));
                Debug.Log($"[DeckViewerModal] Card removed: {_selectedCard.CardName}");
            }

            // Invoke callback
            var removedCard = _selectedCard;
            Hide();
            _onCardSelected?.Invoke(removedCard);
        }

        private void OnCancelClicked()
        {
            Debug.Log("[DeckViewerModal] Cancelled");
            Hide();
            _onCardSelected?.Invoke(null);
        }
    }
}
