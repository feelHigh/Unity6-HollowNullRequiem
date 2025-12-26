// ============================================
// ResultsScreen.cs
// Post-combat victory/defeat screen with rewards
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

namespace HNR.UI
{
    /// <summary>
    /// Results screen shown after combat ends.
    /// Displays victory/defeat status, rewards, and card selection.
    /// </summary>
    public class ResultsScreen : ScreenBase
    {
        // ============================================
        // Result Display
        // ============================================

        [Header("Result Display")]
        [SerializeField, Tooltip("Main result title (VICTORY or DEFEAT)")]
        private TMP_Text _resultTitleText;

        [SerializeField, Tooltip("Combat summary text (enemy name, rewards)")]
        private TMP_Text _summaryText;

        [SerializeField, Tooltip("CanvasGroup for victory glow effect")]
        private CanvasGroup _victoryGlow;

        [SerializeField, Tooltip("CanvasGroup for defeat overlay")]
        private CanvasGroup _defeatOverlay;

        // ============================================
        // Currency Rewards
        // ============================================

        [Header("Currency Rewards")]
        [SerializeField, Tooltip("Void Shards reward text")]
        private TMP_Text _voidShardsText;

        [SerializeField, Tooltip("Soul Essence reward text")]
        private TMP_Text _soulEssenceText;

        [SerializeField, Tooltip("Rewards container")]
        private Transform _rewardsContainer;

        // ============================================
        // Card Rewards
        // ============================================

        [Header("Card Rewards")]
        [SerializeField, Tooltip("Card reward section title")]
        private TMP_Text _cardRewardTitle;

        [SerializeField, Tooltip("Container for card reward slots")]
        private Transform _cardRewardContainer;

        [SerializeField, Tooltip("Card reward slot prefab")]
        private GameObject _cardRewardSlotPrefab;

        [SerializeField, Tooltip("Skip reward button")]
        private Button _skipRewardButton;

        [SerializeField, Tooltip("Maximum cards to show as rewards")]
        private int _maxCardRewards = 3;

        // ============================================
        // Actions
        // ============================================

        [Header("Actions")]
        [SerializeField, Tooltip("Continue button to return to map")]
        private Button _continueButton;

        [SerializeField, Tooltip("Retry button (defeat only)")]
        private Button _retryButton;

        [SerializeField, Tooltip("Abandon run button (defeat only)")]
        private Button _abandonButton;

        // ============================================
        // Animation
        // ============================================

        [Header("Animation")]
        [SerializeField] private float _fadeInDuration = 0.5f;
        [SerializeField] private float _cardStaggerDelay = 0.15f;
        [SerializeField] private float _titleScaleFrom = 1.5f;
        [SerializeField] private Color _victoryTitleColor = new Color(0.83f, 0.69f, 0.22f); // Gold
        [SerializeField] private Color _defeatTitleColor = new Color(0.77f, 0.12f, 0.23f);  // Red

        // ============================================
        // State
        // ============================================

        private bool _isVictory;
        private string _enemyName;
        private int _voidShardsReward;
        private int _soulEssenceReward;
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

            // Prevent immediate continue
            if (_continueButton != null)
            {
                _continueButton.interactable = false;
            }

            Debug.Log("[ResultsScreen] Results screen shown");
        }

        public override void OnHide()
        {
            base.OnHide();

            // Cleanup spawned card slots
            ClearCardSlots();

            // Kill tweens
            DOTween.Kill(this);

            Debug.Log("[ResultsScreen] Results screen hidden");
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

            if (_retryButton != null)
            {
                _retryButton.onClick.RemoveAllListeners();
                _retryButton.onClick.AddListener(OnRetryClicked);
            }

            if (_abandonButton != null)
            {
                _abandonButton.onClick.RemoveAllListeners();
                _abandonButton.onClick.AddListener(OnAbandonClicked);
            }
        }

        // ============================================
        // Public API - Set Results Data
        // ============================================

        /// <summary>
        /// Initialize the results screen with combat outcome data.
        /// </summary>
        /// <param name="isVictory">Whether the player won.</param>
        /// <param name="enemyName">Name of the defeated enemy.</param>
        /// <param name="voidShards">Void Shards earned.</param>
        /// <param name="soulEssence">Soul Essence earned.</param>
        /// <param name="cardRewards">Cards available as rewards.</param>
        public void SetResults(bool isVictory, string enemyName, int voidShards, int soulEssence,
            List<CardDataSO> cardRewards = null)
        {
            _isVictory = isVictory;
            _enemyName = enemyName;
            _voidShardsReward = voidShards;
            _soulEssenceReward = soulEssence;
            _selectedCard = null;

            _cardRewards.Clear();
            if (cardRewards != null)
            {
                int count = Mathf.Min(cardRewards.Count, _maxCardRewards);
                for (int i = 0; i < count; i++)
                {
                    _cardRewards.Add(cardRewards[i]);
                }
            }

            UpdateDisplay();
            PlayShowAnimation();
        }

        // ============================================
        // Display Updates
        // ============================================

        private void UpdateDisplay()
        {
            // Result title
            if (_resultTitleText != null)
            {
                _resultTitleText.text = _isVictory ? "VICTORY" : "DEFEAT";
                _resultTitleText.color = _isVictory ? _victoryTitleColor : _defeatTitleColor;
            }

            // Summary text
            if (_summaryText != null)
            {
                if (_isVictory)
                {
                    _summaryText.text = $"{_enemyName} Defeated";
                    if (_voidShardsReward > 0 || _soulEssenceReward > 0)
                    {
                        string rewards = "";
                        if (_voidShardsReward > 0) rewards += $"+{_voidShardsReward} Void Shards";
                        if (_voidShardsReward > 0 && _soulEssenceReward > 0) rewards += " • ";
                        if (_soulEssenceReward > 0) rewards += $"+{_soulEssenceReward} Soul Essence";
                        _summaryText.text += $" • {rewards}";
                    }
                }
                else
                {
                    _summaryText.text = $"Fallen to {_enemyName}";
                }
            }

            // Victory/Defeat overlays
            if (_victoryGlow != null)
                _victoryGlow.gameObject.SetActive(_isVictory);

            if (_defeatOverlay != null)
                _defeatOverlay.gameObject.SetActive(!_isVictory);

            // Currency rewards
            if (_voidShardsText != null)
            {
                _voidShardsText.text = $"+{_voidShardsReward}";
                _voidShardsText.gameObject.SetActive(_isVictory && _voidShardsReward > 0);
            }

            if (_soulEssenceText != null)
            {
                _soulEssenceText.text = $"+{_soulEssenceReward}";
                _soulEssenceText.gameObject.SetActive(_isVictory && _soulEssenceReward > 0);
            }

            // Card rewards section
            bool showCardRewards = _isVictory && _cardRewards.Count > 0;

            if (_cardRewardTitle != null)
            {
                _cardRewardTitle.gameObject.SetActive(showCardRewards);
                _cardRewardTitle.text = "Choose a card reward:";
            }

            if (_skipRewardButton != null)
                _skipRewardButton.gameObject.SetActive(showCardRewards);

            // Buttons visibility based on victory/defeat
            if (_continueButton != null)
                _continueButton.gameObject.SetActive(_isVictory);

            if (_retryButton != null)
                _retryButton.gameObject.SetActive(!_isVictory);

            if (_abandonButton != null)
                _abandonButton.gameObject.SetActive(!_isVictory);

            // Spawn card reward slots
            SpawnCardRewardSlots();
        }

        // ============================================
        // Card Reward Slots
        // ============================================

        private void SpawnCardRewardSlots()
        {
            ClearCardSlots();

            if (!_isVictory || _cardRewards.Count == 0 || _cardRewardContainer == null)
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
                    // Create placeholder slot if no prefab assigned
                    slot = CreatePlaceholderCardSlot(card);
                }

                if (slot != null)
                {
                    _spawnedCardSlots.Add(slot);

                    // Wire up card slot - try Button first (for legacy placeholder slots)
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

                    // Try to set card data if component exists
                    var cardDisplay = slot.GetComponent<ICardDisplay>();
                    cardDisplay?.SetCard(card);
                }
            }
        }

        private GameObject CreatePlaceholderCardSlot(CardDataSO card)
        {
            // Create a simple placeholder if no prefab is assigned
            var slot = new GameObject($"CardSlot_{card.CardName}");
            slot.transform.SetParent(_cardRewardContainer, false);

            var rect = slot.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(90, 120);

            var image = slot.AddComponent<Image>();
            image.color = new Color(0.15f, 0.15f, 0.2f);

            var button = slot.AddComponent<Button>();
            button.targetGraphic = image;

            // Add card name text
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
                {
                    Destroy(slot);
                }
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
            Debug.Log($"[ResultsScreen] Card selected: {_selectedCard.CardName}");

            // Visual feedback - highlight selected
            for (int i = 0; i < _spawnedCardSlots.Count; i++)
            {
                var slot = _spawnedCardSlots[i];
                if (slot == null) continue;

                var image = slot.GetComponent<Image>();
                if (image != null)
                {
                    image.color = (i == index)
                        ? new Color(0.2f, 0.3f, 0.4f)  // Selected
                        : new Color(0.15f, 0.15f, 0.2f); // Normal
                }

                // Scale animation
                slot.transform.DOScale(i == index ? 1.1f : 1f, 0.2f).SetLink(slot);
            }

            // Enable continue button
            EnableContinueButton();
        }

        private void OnSkipRewardClicked()
        {
            Debug.Log("[ResultsScreen] Skip reward clicked");
            _selectedCard = null;

            // Reset slot visuals
            foreach (var slot in _spawnedCardSlots)
            {
                if (slot == null) continue;
                var image = slot.GetComponent<Image>();
                if (image != null)
                {
                    image.color = new Color(0.15f, 0.15f, 0.2f);
                }
                slot.transform.DOScale(0.9f, 0.2f).SetLink(slot);
            }

            EnableContinueButton();
        }

        private void EnableContinueButton()
        {
            if (_continueButton != null)
            {
                _continueButton.interactable = true;

                // Pulse animation to draw attention
                _continueButton.transform.DOPunchScale(Vector3.one * 0.1f, 0.3f, 5).SetLink(gameObject);
            }
        }

        // ============================================
        // Button Handlers
        // ============================================

        private void OnContinueClicked()
        {
            Debug.Log("[ResultsScreen] Continue clicked");

            // Add selected card to deck if any
            if (_selectedCard != null)
            {
                AddCardToDeck(_selectedCard);
            }

            // Apply currency rewards
            ApplyRewards();

            // Navigate back to map
            NavigateToMap();
        }

        private void OnRetryClicked()
        {
            Debug.Log("[ResultsScreen] Retry clicked");

            // Restart the same combat
            var gameManager = ServiceLocator.Get<IGameManager>();
            gameManager?.ChangeState(GameState.Combat);
        }

        private void OnAbandonClicked()
        {
            Debug.Log("[ResultsScreen] Abandon run clicked");

            // Clear run data and return to bastion
            var saveManager = ServiceLocator.Get<ISaveManager>();
            saveManager?.DeleteRun();

            var gameManager = ServiceLocator.Get<IGameManager>();
            gameManager?.ChangeState(GameState.Bastion);
        }

        // ============================================
        // Rewards Application
        // ============================================

        private void AddCardToDeck(CardDataSO card)
        {
            Debug.Log($"[ResultsScreen] Adding card to deck: {card.CardName}");

            // Publish event for deck system to handle
            EventBus.Publish(new CardAddedToDeckEvent(card));
        }

        private void ApplyRewards()
        {
            // Add Void Shards via ShopManager (publishes VoidShardsChangedEvent for UI updates)
            if (_voidShardsReward > 0)
            {
                if (ServiceLocator.TryGet<IShopManager>(out var shopManager))
                {
                    shopManager.AddVoidShards(_voidShardsReward);
                    Debug.Log($"[ResultsScreen] Added {_voidShardsReward} Void Shards via ShopManager");
                }
                else
                {
                    Debug.LogWarning("[ResultsScreen] ShopManager not available - could not add Void Shards");
                }
            }

            // Soul Essence is typically added per-Requiem during combat
            // But we can publish a general event if needed
        }

        // ============================================
        // Navigation
        // ============================================

        private void NavigateToMap()
        {
            var gameManager = ServiceLocator.Get<IGameManager>();
            gameManager?.ChangeState(GameState.Run);
        }

        // ============================================
        // Animation
        // ============================================

        protected override void PlayShowAnimation()
        {
            // Title animation
            if (_resultTitleText != null)
            {
                _resultTitleText.transform.localScale = Vector3.one * _titleScaleFrom;
                _resultTitleText.transform.DOScale(1f, _fadeInDuration * 1.5f)
                    .SetEase(Ease.OutBack)
                    .SetLink(gameObject);

                var canvasGroup = _resultTitleText.GetComponent<CanvasGroup>();
                if (canvasGroup == null)
                {
                    canvasGroup = _resultTitleText.gameObject.AddComponent<CanvasGroup>();
                }
                canvasGroup.alpha = 0f;
                canvasGroup.DOFade(1f, _fadeInDuration).SetLink(gameObject);
            }

            // Summary fade in
            if (_summaryText != null)
            {
                var canvasGroup = _summaryText.GetComponent<CanvasGroup>();
                if (canvasGroup == null)
                {
                    canvasGroup = _summaryText.gameObject.AddComponent<CanvasGroup>();
                }
                canvasGroup.alpha = 0f;
                canvasGroup.DOFade(1f, _fadeInDuration).SetDelay(_fadeInDuration * 0.5f).SetLink(gameObject);
            }

            // Victory glow pulse
            if (_isVictory && _victoryGlow != null)
            {
                _victoryGlow.alpha = 0f;
                _victoryGlow.DOFade(0.5f, _fadeInDuration * 2f)
                    .SetLoops(-1, LoopType.Yoyo)
                    .SetLink(gameObject);
            }

            // Card slots stagger animation
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

            // Enable continue after card animations finish
            float totalDelay = _fadeInDuration + _spawnedCardSlots.Count * _cardStaggerDelay + 0.5f;

            // If no card rewards, enable continue button after title animation
            if (!_isVictory || _cardRewards.Count == 0)
            {
                DOVirtual.DelayedCall(_fadeInDuration, () =>
                {
                    if (_continueButton != null)
                        _continueButton.interactable = true;
                }).SetLink(gameObject);
            }
        }

        // ============================================
        // Back Button
        // ============================================

        public override bool OnBackPressed()
        {
            // Don't allow back during results - must use continue/abandon
            return true;
        }
    }

    // ============================================
    // Supporting Interface
    // ============================================

    /// <summary>
    /// Interface for card display components.
    /// </summary>
    public interface ICardDisplay
    {
        void SetCard(CardDataSO card);
    }

    // NOTE: CardAddedToDeckEvent is defined in HNR.Core.Events.ShopEvents.cs
    // Do not duplicate event definitions - use the centralized one
}
