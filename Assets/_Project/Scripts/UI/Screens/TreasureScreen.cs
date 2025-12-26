// ============================================
// TreasureScreen.cs
// Treasure node reward selection screen
// ============================================

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using HNR.Core;
using HNR.Core.Interfaces;
using HNR.Core.Events;
using HNR.Cards;
using HNR.Map;

namespace HNR.UI.Screens
{
    /// <summary>
    /// Screen for treasure node rewards. Displays 3 card choices + skip option.
    /// Similar to Victory screen but for treasure nodes.
    /// </summary>
    public class TreasureScreen : ScreenBase
    {
        // ============================================
        // Display Elements
        // ============================================

        [Header("Display")]
        [SerializeField, Tooltip("Treasure title text")]
        private TMP_Text _titleText;

        [SerializeField, Tooltip("Subtitle/description text")]
        private TMP_Text _subtitleText;

        [SerializeField, Tooltip("Treasure chest icon")]
        private Image _treasureIcon;

        [SerializeField, Tooltip("Background glow overlay")]
        private CanvasGroup _glowOverlay;

        // ============================================
        // Card Rewards
        // ============================================

        [Header("Card Rewards")]
        [SerializeField, Tooltip("Container for card reward slots")]
        private Transform _cardRewardContainer;

        [SerializeField, Tooltip("Card reward slot prefab")]
        private GameObject _cardRewardSlotPrefab;

        [SerializeField, Tooltip("Skip reward button")]
        private Button _skipRewardButton;

        [SerializeField, Tooltip("Maximum cards to show")]
        private int _maxCardRewards = 3;

        // ============================================
        // Actions
        // ============================================

        [Header("Actions")]
        [SerializeField, Tooltip("Continue button to return to map")]
        private Button _continueButton;

        // ============================================
        // Animation
        // ============================================

        [Header("Animation")]
        [SerializeField] private float _fadeInDuration = 0.5f;
        [SerializeField] private float _cardStaggerDelay = 0.15f;
        [SerializeField] private Color _titleColor = new Color(0.83f, 0.69f, 0.22f); // Gold

        // ============================================
        // State
        // ============================================

        private List<CardDataSO> _cardRewards = new List<CardDataSO>();
        private CardDataSO _selectedCard;
        private List<GameObject> _spawnedCardSlots = new List<GameObject>();

        // ============================================
        // Screen Lifecycle
        // ============================================

        public override void OnShow()
        {
            base.OnShow();

            SetupButtons();
            GenerateRewards();
            UpdateDisplay();
            PlayShowAnimation();

            // Disable continue until selection made
            if (_continueButton != null)
            {
                _continueButton.interactable = false;
            }

            Debug.Log("[TreasureScreen] Treasure screen shown");
        }

        public override void OnHide()
        {
            base.OnHide();
            ClearCardSlots();
            DOTween.Kill(this);
            Debug.Log("[TreasureScreen] Treasure screen hidden");
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

            if (_skipRewardButton != null)
            {
                _skipRewardButton.onClick.RemoveAllListeners();
                _skipRewardButton.onClick.AddListener(OnSkipRewardClicked);
            }
        }

        private void GenerateRewards()
        {
            _cardRewards.Clear();
            _selectedCard = null;

            // Get zone for reward generation
            int zone = 1;
            var mapManager = ServiceLocator.Get<MapManager>();
            if (mapManager != null)
            {
                zone = mapManager.CurrentZone;
            }

            // Generate random cards from zone pool
            var cards = GetRandomCardsForZone(zone, _maxCardRewards);
            _cardRewards.AddRange(cards);
        }

        private List<CardDataSO> GetRandomCardsForZone(int zone, int count)
        {
            var result = new List<CardDataSO>();

            // Load all cards from Resources
            var allCards = Resources.LoadAll<CardDataSO>("Data/Cards");
            if (allCards == null || allCards.Length == 0)
            {
                // Fallback: try loading from Assets path
                allCards = Resources.LoadAll<CardDataSO>("");
            }

            if (allCards == null || allCards.Length == 0)
            {
                Debug.LogWarning("[TreasureScreen] No cards found for treasure rewards");
                return result;
            }

            // Shuffle and pick random cards
            var shuffled = new List<CardDataSO>(allCards);
            for (int i = shuffled.Count - 1; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);
                (shuffled[i], shuffled[j]) = (shuffled[j], shuffled[i]);
            }

            for (int i = 0; i < Mathf.Min(count, shuffled.Count); i++)
            {
                result.Add(shuffled[i]);
            }

            return result;
        }

        // ============================================
        // Display
        // ============================================

        private void UpdateDisplay()
        {
            if (_titleText != null)
            {
                _titleText.text = "TREASURE";
                _titleText.color = _titleColor;
            }

            if (_subtitleText != null)
            {
                _subtitleText.text = "Choose a card reward:";
            }

            SpawnCardRewardSlots();
        }

        private void SpawnCardRewardSlots()
        {
            ClearCardSlots();

            if (_cardRewards.Count == 0 || _cardRewardContainer == null)
                return;

            for (int i = 0; i < _cardRewards.Count; i++)
            {
                var card = _cardRewards[i];
                GameObject slot = null;

                if (_cardRewardSlotPrefab != null)
                {
                    slot = Instantiate(_cardRewardSlotPrefab, _cardRewardContainer);
                }
                else
                {
                    slot = CreatePlaceholderCardSlot(card);
                }

                if (slot != null)
                {
                    _spawnedCardSlots.Add(slot);

                    // Try Button first (for legacy placeholder slots)
                    var button = slot.GetComponent<Button>();
                    if (button != null)
                    {
                        int index = i;
                        button.onClick.RemoveAllListeners();
                        button.onClick.AddListener(() => OnCardRewardSelected(index));
                    }
                    else
                    {
                        // Use Card's native click event (for Card prefab)
                        var cardComponent = slot.GetComponent<Card>();
                        if (cardComponent != null)
                        {
                            int index = i;
                            cardComponent.OnCardClicked += (clickedCard) => OnCardRewardSelected(index);
                        }
                    }

                    var cardDisplay = slot.GetComponent<ICardDisplay>();
                    cardDisplay?.SetCard(card);
                }
            }
        }

        private GameObject CreatePlaceholderCardSlot(CardDataSO card)
        {
            var slot = new GameObject($"CardSlot_{card.CardName}");
            slot.transform.SetParent(_cardRewardContainer, false);

            var rect = slot.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(90, 120);

            var image = slot.AddComponent<Image>();
            image.color = new Color(0.15f, 0.15f, 0.2f);

            var button = slot.AddComponent<Button>();
            button.targetGraphic = image;

            var textObj = new GameObject("CardName");
            textObj.transform.SetParent(slot.transform, false);

            var textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.sizeDelta = Vector2.zero;

            var text = textObj.AddComponent<TextMeshProUGUI>();
            text.text = card.CardName;
            text.fontSize = 12;
            text.alignment = TextAlignmentOptions.Center;
            text.color = Color.white;

            return slot;
        }

        private void ClearCardSlots()
        {
            foreach (var slot in _spawnedCardSlots)
            {
                if (slot != null)
                    Destroy(slot);
            }
            _spawnedCardSlots.Clear();
        }

        // ============================================
        // Card Selection
        // ============================================

        private void OnCardRewardSelected(int index)
        {
            if (index < 0 || index >= _cardRewards.Count) return;

            _selectedCard = _cardRewards[index];
            Debug.Log($"[TreasureScreen] Card selected: {_selectedCard.CardName}");

            for (int i = 0; i < _spawnedCardSlots.Count; i++)
            {
                var slot = _spawnedCardSlots[i];
                if (slot == null) continue;

                var image = slot.GetComponent<Image>();
                if (image != null)
                {
                    image.color = (i == index)
                        ? new Color(0.2f, 0.3f, 0.4f)
                        : new Color(0.15f, 0.15f, 0.2f);
                }

                slot.transform.DOScale(i == index ? 1.1f : 1f, 0.2f).SetLink(slot);
            }

            EnableContinueButton();
        }

        private void OnSkipRewardClicked()
        {
            Debug.Log("[TreasureScreen] Skip reward clicked");
            _selectedCard = null;

            foreach (var slot in _spawnedCardSlots)
            {
                if (slot == null) continue;
                var image = slot.GetComponent<Image>();
                if (image != null)
                    image.color = new Color(0.15f, 0.15f, 0.2f);
                slot.transform.DOScale(0.9f, 0.2f).SetLink(slot);
            }

            EnableContinueButton();
        }

        private void EnableContinueButton()
        {
            if (_continueButton != null)
            {
                _continueButton.interactable = true;
                _continueButton.transform.DOPunchScale(Vector3.one * 0.1f, 0.3f, 5).SetLink(gameObject);
            }
        }

        // ============================================
        // Button Handlers
        // ============================================

        private void OnContinueClicked()
        {
            Debug.Log("[TreasureScreen] Continue clicked");

            // Add selected card to deck
            if (_selectedCard != null)
            {
                EventBus.Publish(new CardAddedToDeckEvent(_selectedCard));
            }

            // Complete the treasure node
            var mapManager = ServiceLocator.Get<MapManager>();
            mapManager?.CompleteCurrentNode();

            // Return to map
            var uiManager = ServiceLocator.Get<IUIManager>();
            uiManager?.ShowScreen<MapScreen>();
        }

        // ============================================
        // Animation
        // ============================================

        protected override void PlayShowAnimation()
        {
            // Title animation
            if (_titleText != null)
            {
                _titleText.transform.localScale = Vector3.one * 1.5f;
                _titleText.transform.DOScale(1f, _fadeInDuration * 1.5f)
                    .SetEase(Ease.OutBack)
                    .SetLink(gameObject);

                var canvasGroup = _titleText.GetComponent<CanvasGroup>();
                if (canvasGroup == null)
                    canvasGroup = _titleText.gameObject.AddComponent<CanvasGroup>();
                canvasGroup.alpha = 0f;
                canvasGroup.DOFade(1f, _fadeInDuration).SetLink(gameObject);
            }

            // Glow pulse
            if (_glowOverlay != null)
            {
                _glowOverlay.alpha = 0f;
                _glowOverlay.DOFade(0.5f, _fadeInDuration * 2f)
                    .SetLoops(-1, LoopType.Yoyo)
                    .SetLink(gameObject);
            }

            // Card slots stagger
            for (int i = 0; i < _spawnedCardSlots.Count; i++)
            {
                var slot = _spawnedCardSlots[i];
                if (slot == null) continue;

                slot.transform.localScale = Vector3.zero;
                slot.transform.DOScale(1f, _fadeInDuration)
                    .SetDelay(_fadeInDuration + i * _cardStaggerDelay)
                    .SetEase(Ease.OutBack)
                    .SetLink(slot);
            }
        }

        // ============================================
        // Back Button
        // ============================================

        public override bool OnBackPressed()
        {
            // Don't allow back - must use continue or skip
            return true;
        }
    }
}
