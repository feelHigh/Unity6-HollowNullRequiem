// ============================================
// SanctuaryScreen.cs
// Rest stop screen with heal/purify/upgrade choices
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
using HNR.Cards;

namespace HNR.UI
{
    /// <summary>
    /// Sanctuary rest stop screen between combats.
    /// Offers three mutually exclusive choices: Rest, Purify, or Upgrade.
    /// Reference: HollowNullRequiem_Mockup.jsx lines 1620-1702
    /// </summary>
    public class SanctuaryScreen : ScreenBase
    {
        // ============================================
        // Display Elements
        // ============================================

        [Header("Display")]
        [SerializeField, Tooltip("Title text (SANCTUARY)")]
        private TMP_Text _titleText;

        [SerializeField, Tooltip("Description text")]
        private TMP_Text _descriptionText;

        [SerializeField, Tooltip("Icon/illustration for sanctuary")]
        private Image _sanctuaryIcon;

        // ============================================
        // Choice Buttons
        // ============================================

        [Header("Choice Buttons")]
        [SerializeField, Tooltip("Rest button (heal HP)")]
        private Button _restButton;

        [SerializeField, Tooltip("Purify button (reduce corruption)")]
        private Button _purifyButton;

        [SerializeField, Tooltip("Upgrade button (enhance a card)")]
        private Button _upgradeButton;

        // ============================================
        // Choice Info Labels
        // ============================================

        [Header("Choice Info")]
        [SerializeField, Tooltip("Rest effect text")]
        private TMP_Text _restEffectText;

        [SerializeField, Tooltip("Purify effect text")]
        private TMP_Text _purifyEffectText;

        [SerializeField, Tooltip("Upgrade effect text")]
        private TMP_Text _upgradeEffectText;

        [SerializeField, Tooltip("Rest icon")]
        private Image _restIcon;

        [SerializeField, Tooltip("Purify icon")]
        private Image _purifyIcon;

        [SerializeField, Tooltip("Upgrade icon")]
        private Image _upgradeIcon;

        // ============================================
        // Card Upgrade Section
        // ============================================

        [Header("Card Upgrade")]
        [SerializeField, Tooltip("Card selection panel (shown when upgrade chosen)")]
        private GameObject _cardSelectionPanel;

        [SerializeField, Tooltip("Container for upgrade card slots")]
        private Transform _upgradeCardContainer;

        [SerializeField, Tooltip("Card slot prefab for upgrade selection")]
        private GameObject _cardSlotPrefab;

        [SerializeField, Tooltip("Confirm upgrade button")]
        private Button _confirmUpgradeButton;

        [SerializeField, Tooltip("Cancel upgrade button")]
        private Button _cancelUpgradeButton;

        // ============================================
        // Configuration
        // ============================================

        [Header("Configuration")]
        [SerializeField, Tooltip("Percentage of max HP to heal")]
        private float _healPercentage = 0.30f;

        [SerializeField, Tooltip("Corruption reduction amount")]
        private int _purifyAmount = 30;

        // ============================================
        // Animation
        // ============================================

        [Header("Animation")]
        [SerializeField] private float _fadeInDuration = 0.4f;
        [SerializeField] private float _choiceStaggerDelay = 0.1f;
        [SerializeField] private Color _restColor = new Color(0.18f, 0.8f, 0.44f);
        [SerializeField] private Color _purifyColor = new Color(0f, 0.83f, 0.89f);
        [SerializeField] private Color _upgradeColor = new Color(0.83f, 0.69f, 0.22f);

        // ============================================
        // State
        // ============================================

        private List<CardDataSO> _upgradableCards = new List<CardDataSO>();
        private CardDataSO _selectedCardForUpgrade;
        private List<GameObject> _spawnedCardSlots = new List<GameObject>();
        private bool _choiceMade;

        // ============================================
        // Screen Lifecycle
        // ============================================

        public override void OnShow()
        {
            base.OnShow();

            SetupButtons();
            UpdateDisplay();
            HideCardSelectionPanel();
            _choiceMade = false;

            PlayShowAnimation();

            Debug.Log("[SanctuaryScreen] Sanctuary screen shown");
        }

        public override void OnHide()
        {
            base.OnHide();

            ClearUpgradeCardSlots();
            DOTween.Kill(this);

            Debug.Log("[SanctuaryScreen] Sanctuary screen hidden");
        }

        // ============================================
        // Setup
        // ============================================

        private void SetupButtons()
        {
            if (_restButton != null)
            {
                _restButton.onClick.RemoveAllListeners();
                _restButton.onClick.AddListener(OnRestClicked);
            }

            if (_purifyButton != null)
            {
                _purifyButton.onClick.RemoveAllListeners();
                _purifyButton.onClick.AddListener(OnPurifyClicked);
            }

            if (_upgradeButton != null)
            {
                _upgradeButton.onClick.RemoveAllListeners();
                _upgradeButton.onClick.AddListener(OnUpgradeClicked);
            }

            if (_confirmUpgradeButton != null)
            {
                _confirmUpgradeButton.onClick.RemoveAllListeners();
                _confirmUpgradeButton.onClick.AddListener(OnConfirmUpgradeClicked);
            }

            if (_cancelUpgradeButton != null)
            {
                _cancelUpgradeButton.onClick.RemoveAllListeners();
                _cancelUpgradeButton.onClick.AddListener(OnCancelUpgradeClicked);
            }
        }

        private void UpdateDisplay()
        {
            // Title and description
            if (_titleText != null)
                _titleText.text = "SANCTUARY";

            if (_descriptionText != null)
                _descriptionText.text = "A MOMENT OF PEACE\nThe sanctuary's light pushes back the corruption. Rest here to recover your strength.";

            // Calculate actual heal amount for display
            int healAmount = CalculateHealAmount();

            // Effect texts
            if (_restEffectText != null)
                _restEffectText.text = $"Heal {healAmount} HP\n({Mathf.RoundToInt(_healPercentage * 100)}% max HP)";

            if (_purifyEffectText != null)
                _purifyEffectText.text = $"-{_purifyAmount} Corruption\n(all Requiems)";

            if (_upgradeEffectText != null)
                _upgradeEffectText.text = "Enhance a card\nfrom your deck";

            // Apply colors to icons if assigned
            if (_restIcon != null)
                _restIcon.color = _restColor;

            if (_purifyIcon != null)
                _purifyIcon.color = _purifyColor;

            if (_upgradeIcon != null)
                _upgradeIcon.color = _upgradeColor;
        }

        // ============================================
        // Choice Handlers
        // ============================================

        private void OnRestClicked()
        {
            if (_choiceMade) return;
            _choiceMade = true;

            Debug.Log("[SanctuaryScreen] Rest chosen");
            ApplyRest();
            CompleteAndReturn();
        }

        private void OnPurifyClicked()
        {
            if (_choiceMade) return;
            _choiceMade = true;

            Debug.Log("[SanctuaryScreen] Purify chosen");
            ApplyPurify();
            CompleteAndReturn();
        }

        private void OnUpgradeClicked()
        {
            if (_choiceMade) return;

            Debug.Log("[SanctuaryScreen] Upgrade chosen - showing card selection");
            ShowCardSelectionPanel();
        }

        // ============================================
        // Rest Effect
        // ============================================

        private void ApplyRest()
        {
            int healAmount = CalculateHealAmount();

            // Publish heal event for combat system to handle
            EventBus.Publish(new SanctuaryHealEvent(healAmount));

            Debug.Log($"[SanctuaryScreen] Applied rest: healed {healAmount} HP");
        }

        private int CalculateHealAmount()
        {
            // Get max HP from run data or run manager
            int maxHP = 0;

            // Try run manager first (active run)
            var runManager = ServiceLocator.Get<IRunManager>();
            if (runManager != null && runManager.IsRunActive)
            {
                maxHP = runManager.TeamMaxHP;
            }

            // Fallback to save data
            if (maxHP == 0)
            {
                var saveManager = ServiceLocator.Get<ISaveManager>();
                var saveData = saveManager?.LoadRun();
                if (saveData?.Team != null)
                {
                    maxHP = saveData.Team.TeamMaxHP;
                }
            }

            // Default if no data
            if (maxHP == 0) maxHP = 210; // Default team HP

            return Mathf.RoundToInt(maxHP * _healPercentage);
        }

        // ============================================
        // Purify Effect
        // ============================================

        private void ApplyPurify()
        {
            // Publish purify event for corruption system to handle
            EventBus.Publish(new SanctuaryPurifyEvent(_purifyAmount));

            Debug.Log($"[SanctuaryScreen] Applied purify: reduced corruption by {_purifyAmount}");
        }

        // ============================================
        // Upgrade Card Selection
        // ============================================

        private void ShowCardSelectionPanel()
        {
            if (_cardSelectionPanel != null)
            {
                _cardSelectionPanel.SetActive(true);

                // Animate panel in
                var canvasGroup = _cardSelectionPanel.GetComponent<CanvasGroup>();
                if (canvasGroup == null)
                {
                    canvasGroup = _cardSelectionPanel.AddComponent<CanvasGroup>();
                }
                canvasGroup.alpha = 0f;
                canvasGroup.DOFade(1f, _fadeInDuration);
            }

            LoadUpgradableCards();
            SpawnUpgradeCardSlots();

            // Reset selection
            _selectedCardForUpgrade = null;
            if (_confirmUpgradeButton != null)
            {
                _confirmUpgradeButton.interactable = false;
            }
        }

        private void HideCardSelectionPanel()
        {
            if (_cardSelectionPanel != null)
            {
                _cardSelectionPanel.SetActive(false);
            }

            ClearUpgradeCardSlots();
        }

        private void LoadUpgradableCards()
        {
            _upgradableCards.Clear();

            // Get cards from current deck that can be upgraded
            var runManager = ServiceLocator.Get<IRunManager>();
            var deck = runManager?.Deck;

            if (deck != null && runManager != null)
            {
                foreach (var card in deck)
                {
                    // Only show cards that aren't already upgraded
                    if (card != null && !runManager.IsCardUpgraded(card.CardId))
                    {
                        _upgradableCards.Add(card);
                    }
                }
            }

            Debug.Log($"[SanctuaryScreen] Found {_upgradableCards.Count} upgradable cards");
        }

        private void SpawnUpgradeCardSlots()
        {
            ClearUpgradeCardSlots();

            if (_upgradeCardContainer == null) return;

            for (int i = 0; i < _upgradableCards.Count; i++)
            {
                var card = _upgradableCards[i];
                GameObject slot = null;

                if (_cardSlotPrefab != null)
                {
                    slot = Instantiate(_cardSlotPrefab, _upgradeCardContainer);
                }
                else
                {
                    slot = CreatePlaceholderCardSlot(card);
                }

                if (slot != null)
                {
                    _spawnedCardSlots.Add(slot);

                    var button = slot.GetComponent<Button>();
                    if (button != null)
                    {
                        int index = i;
                        button.onClick.RemoveAllListeners();
                        button.onClick.AddListener(() => OnUpgradeCardSelected(index));
                    }

                    // Set card display if component exists
                    var cardDisplay = slot.GetComponent<ICardDisplay>();
                    cardDisplay?.SetCard(card);
                }
            }
        }

        private GameObject CreatePlaceholderCardSlot(CardDataSO card)
        {
            var slot = new GameObject($"UpgradeSlot_{card?.CardName ?? "Unknown"}");
            slot.transform.SetParent(_upgradeCardContainer, false);

            var rect = slot.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(80, 110);

            var image = slot.AddComponent<Image>();
            image.color = new Color(0.15f, 0.15f, 0.2f);

            var button = slot.AddComponent<Button>();
            button.targetGraphic = image;

            // Card name text
            var textObj = new GameObject("CardName");
            textObj.transform.SetParent(slot.transform, false);

            var textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.sizeDelta = Vector2.zero;

            var text = textObj.AddComponent<TextMeshProUGUI>();
            text.text = card?.CardName ?? "Unknown";
            text.fontSize = 10;
            text.alignment = TextAlignmentOptions.Center;
            text.color = Color.white;

            return slot;
        }

        private void ClearUpgradeCardSlots()
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

        private void OnUpgradeCardSelected(int index)
        {
            if (index < 0 || index >= _upgradableCards.Count) return;

            _selectedCardForUpgrade = _upgradableCards[index];
            Debug.Log($"[SanctuaryScreen] Card selected for upgrade: {_selectedCardForUpgrade.CardName}");

            // Visual feedback
            for (int i = 0; i < _spawnedCardSlots.Count; i++)
            {
                var slot = _spawnedCardSlots[i];
                if (slot == null) continue;

                var image = slot.GetComponent<Image>();
                if (image != null)
                {
                    image.color = (i == index)
                        ? new Color(0.3f, 0.4f, 0.3f)
                        : new Color(0.15f, 0.15f, 0.2f);
                }

                slot.transform.DOScale(i == index ? 1.1f : 1f, 0.2f);
            }

            // Enable confirm button
            if (_confirmUpgradeButton != null)
            {
                _confirmUpgradeButton.interactable = true;
            }
        }

        private void OnConfirmUpgradeClicked()
        {
            if (_selectedCardForUpgrade == null) return;

            _choiceMade = true;
            ApplyUpgrade();
            HideCardSelectionPanel();
            CompleteAndReturn();
        }

        private void OnCancelUpgradeClicked()
        {
            HideCardSelectionPanel();
            _selectedCardForUpgrade = null;
        }

        private void ApplyUpgrade()
        {
            if (_selectedCardForUpgrade == null) return;

            // Apply upgrade via run manager
            var runManager = ServiceLocator.Get<IRunManager>();
            runManager?.UpgradeCard(_selectedCardForUpgrade);

            // Publish event
            EventBus.Publish(new CardUpgradedEvent(_selectedCardForUpgrade));

            Debug.Log($"[SanctuaryScreen] Upgraded card: {_selectedCardForUpgrade.CardName}");
        }

        // ============================================
        // Completion
        // ============================================

        private void CompleteAndReturn()
        {
            // Brief delay for feedback
            DOVirtual.DelayedCall(0.5f, () =>
            {
                NavigateToMap();
            });
        }

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
            // Title fade in
            if (_titleText != null)
            {
                var canvasGroup = _titleText.GetComponent<CanvasGroup>();
                if (canvasGroup == null)
                {
                    canvasGroup = _titleText.gameObject.AddComponent<CanvasGroup>();
                }
                canvasGroup.alpha = 0f;
                canvasGroup.DOFade(1f, _fadeInDuration);
            }

            // Description fade in with delay
            if (_descriptionText != null)
            {
                var canvasGroup = _descriptionText.GetComponent<CanvasGroup>();
                if (canvasGroup == null)
                {
                    canvasGroup = _descriptionText.gameObject.AddComponent<CanvasGroup>();
                }
                canvasGroup.alpha = 0f;
                canvasGroup.DOFade(1f, _fadeInDuration).SetDelay(_fadeInDuration * 0.5f);
            }

            // Stagger choice buttons
            Button[] buttons = { _restButton, _purifyButton, _upgradeButton };
            for (int i = 0; i < buttons.Length; i++)
            {
                var button = buttons[i];
                if (button == null) continue;

                button.transform.localScale = Vector3.one * 0.8f;
                button.transform.DOScale(1f, _fadeInDuration)
                    .SetDelay(_fadeInDuration + i * _choiceStaggerDelay)
                    .SetEase(Ease.OutBack);

                var canvasGroup = button.GetComponent<CanvasGroup>();
                if (canvasGroup == null)
                {
                    canvasGroup = button.gameObject.AddComponent<CanvasGroup>();
                }
                canvasGroup.alpha = 0f;
                canvasGroup.DOFade(1f, _fadeInDuration)
                    .SetDelay(_fadeInDuration + i * _choiceStaggerDelay);
            }
        }

        // ============================================
        // Back Button
        // ============================================

        public override bool OnBackPressed()
        {
            if (_cardSelectionPanel != null && _cardSelectionPanel.activeSelf)
            {
                // Cancel upgrade selection
                OnCancelUpgradeClicked();
                return true;
            }

            // Don't allow back from sanctuary - must make a choice
            return true;
        }
    }

    // ============================================
    // Supporting Events
    // ============================================

    /// <summary>
    /// Event fired when sanctuary heal is applied.
    /// </summary>
    public class SanctuaryHealEvent : GameEvent
    {
        public int HealAmount { get; }

        public SanctuaryHealEvent(int healAmount)
        {
            HealAmount = healAmount;
        }
    }

    /// <summary>
    /// Event fired when sanctuary purify is applied.
    /// </summary>
    public class SanctuaryPurifyEvent : GameEvent
    {
        public int PurifyAmount { get; }

        public SanctuaryPurifyEvent(int purifyAmount)
        {
            PurifyAmount = purifyAmount;
        }
    }

    /// <summary>
    /// Event fired when a card is upgraded.
    /// </summary>
    public class CardUpgradedEvent : GameEvent
    {
        public CardDataSO Card { get; }

        public CardUpgradedEvent(CardDataSO card)
        {
            Card = card;
        }
    }
}
