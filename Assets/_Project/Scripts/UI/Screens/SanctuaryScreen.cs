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
using HNR.Map;
using HNR.Progression;
using HNR.UI;
using HNR.UI.Config;

namespace HNR.UI
{
    /// <summary>
    /// Sanctuary rest stop screen between combats.
    /// Offers three mutually exclusive choices: Rest, Purify, or Upgrade.
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
        // Visual Elements
        // ============================================

        [Header("Visuals")]
        [SerializeField, Tooltip("Controller for world-space Requiem visuals")]
        private SanctuaryVisualController _visualController;

        [SerializeField, Tooltip("Container for visual anchor positions (legacy - not used)")]
        private RectTransform _visualAnchorsContainer;

        [SerializeField, Tooltip("Left Requiem visual anchor position (legacy - not used)")]
        private RectTransform _leftVisualAnchor;

        [SerializeField, Tooltip("Center/Back Requiem visual anchor position (legacy - not used)")]
        private RectTransform _centerVisualAnchor;

        [SerializeField, Tooltip("Right Requiem visual anchor position (legacy - not used)")]
        private RectTransform _rightVisualAnchor;

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

        [SerializeField, Tooltip("Leave button (skip without making a choice)")]
        private Button _leaveButton;

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

        [SerializeField, Tooltip("Card prefab for upgrade display (fallback if cardSlotPrefab is null)")]
        private Card _cardPrefab;

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

            // Show Requiem visuals in world-space
            if (_visualController != null)
            {
                _visualController.ShowVisuals();
            }

            // Play sanctuary theme music
            if (ServiceLocator.TryGet<IAudioManager>(out var audioManager))
            {
                audioManager.PlayMusic("sanctuary_theme");
            }

            Debug.Log("[SanctuaryScreen] Sanctuary screen shown");
        }

        public override void OnHide()
        {
            base.OnHide();

            // Hide Requiem visuals
            if (_visualController != null)
            {
                _visualController.HideVisuals();
            }

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

            if (_leaveButton != null)
            {
                _leaveButton.onClick.RemoveAllListeners();
                _leaveButton.onClick.AddListener(OnLeaveClicked);
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

        private void OnLeaveClicked()
        {
            if (_choiceMade) return;
            _choiceMade = true;

            Debug.Log("[SanctuaryScreen] Leave chosen - skipping sanctuary");
            CompleteAndReturn();
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
                canvasGroup.DOFade(1f, _fadeInDuration).SetLink(_cardSelectionPanel);

                // Fix Mask/Viewport issue: Replace Mask with RectMask2D for proper clipping
                FixViewportMask();
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

        /// <summary>
        /// Fix the viewport mask to ensure cards are visible.
        /// Unity's Mask component requires an Image with proper alpha to work.
        /// RectMask2D is more reliable for scrollable content.
        /// </summary>
        private void FixViewportMask()
        {
            if (_upgradeCardContainer == null) return;

            // Find the viewport (parent of the card container)
            var viewport = _upgradeCardContainer.parent;
            if (viewport == null) return;

            // Check if there's a problematic Mask component
            var mask = viewport.GetComponent<Mask>();
            if (mask != null)
            {
                Debug.Log("[SanctuaryScreen] Found Mask on viewport, replacing with RectMask2D for better visibility");

                // Remove the old Mask component
                Destroy(mask);

                // Add RectMask2D which clips to RectTransform bounds without needing an Image sprite
                if (viewport.GetComponent<RectMask2D>() == null)
                {
                    viewport.gameObject.AddComponent<RectMask2D>();
                }

                // The Image on viewport is no longer needed for masking, but keep it for layout
                var viewportImage = viewport.GetComponent<Image>();
                if (viewportImage != null)
                {
                    // Make it fully transparent so it doesn't show but doesn't interfere
                    viewportImage.color = Color.clear;
                    viewportImage.raycastTarget = false;
                }
            }

            Debug.Log($"[SanctuaryScreen] Viewport mask fixed. Viewport: {viewport.name}");
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

            if (runManager == null)
            {
                Debug.LogWarning("[SanctuaryScreen] RunManager not available");
                return;
            }

            if (!runManager.IsRunActive)
            {
                Debug.LogWarning("[SanctuaryScreen] No active run");
                return;
            }

            var deck = runManager.Deck;
            Debug.Log($"[SanctuaryScreen] Run active: {runManager.IsRunActive}, Team size: {runManager.Team?.Count ?? 0}, Deck size: {deck?.Count ?? 0}");

            if (deck == null || deck.Count == 0)
            {
                Debug.LogWarning("[SanctuaryScreen] Deck is empty or null. Check that RequiemDataSO assets have StartingCards assigned.");
                Debug.Log("[SanctuaryScreen] Run HNR > 1. Data Assets > Requiems > [Each Requiem] to regenerate Requiem assets with cards.");
                return;
            }

            foreach (var card in deck)
            {
                // Only show cards that aren't already upgraded
                if (card != null && !runManager.IsCardUpgraded(card.CardId))
                {
                    _upgradableCards.Add(card);
                }
            }

            Debug.Log($"[SanctuaryScreen] Found {_upgradableCards.Count} upgradable cards out of {deck.Count} total cards");
        }

        private void SpawnUpgradeCardSlots()
        {
            ClearUpgradeCardSlots();

            if (_upgradeCardContainer == null)
            {
                Debug.LogError("[SanctuaryScreen] _upgradeCardContainer is null!");
                return;
            }

            Debug.Log($"[SanctuaryScreen] Spawning cards in container: {_upgradeCardContainer.name}, parent: {_upgradeCardContainer.parent?.name}");

            // Show message if no upgradable cards
            if (_upgradableCards.Count == 0)
            {
                ShowEmptyStateMessage();
                return;
            }

            // Use local prefab or fall back to config
            var prefab = _cardSlotPrefab ?? _cardPrefab?.gameObject ?? RuntimeUIPrefabConfigSO.Instance?.SanctuaryCardSlotPrefab;

            if (prefab == null)
            {
                Debug.LogError("[SanctuaryScreen] Card slot prefab not assigned. Check RuntimeUIPrefabConfig.");
                return;
            }

            for (int i = 0; i < _upgradableCards.Count; i++)
            {
                var card = _upgradableCards[i];
                var slot = Instantiate(prefab, _upgradeCardContainer);
                slot.name = $"CardSlot_{card?.CardName ?? "Unknown"}";

                _spawnedCardSlots.Add(slot);

                // Try Card's native click event first (for Card prefab)
                var cardComponent = slot.GetComponent<Card>();
                if (cardComponent != null)
                {
                    int index = i;
                    cardComponent.OnCardClicked += (clickedCard) => OnUpgradeCardSelected(index);
                }
                else
                {
                    // Fallback to Button (for legacy slots)
                    var button = slot.GetComponent<Button>();
                    if (button != null)
                    {
                        int index = i;
                        button.onClick.RemoveAllListeners();
                        button.onClick.AddListener(() => OnUpgradeCardSelected(index));
                    }
                }

                // Set card display if component exists
                var cardDisplay = slot.GetComponent<ICardDisplay>();
                cardDisplay?.SetCard(card);

                Debug.Log($"[SanctuaryScreen] Created card slot {i}: {card?.CardName} at {slot.transform.position}");
            }

            // Force layout rebuild after adding all cards
            if (_upgradeCardContainer is RectTransform containerRect)
            {
                Canvas.ForceUpdateCanvases();
                UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(containerRect);
                Debug.Log($"[SanctuaryScreen] Forced layout rebuild. Container size: {containerRect.rect.size}");
            }
        }

        private void ShowEmptyStateMessage()
        {
            // Create empty state message using TMP
            var emptyObj = new GameObject("EmptyState");
            emptyObj.transform.SetParent(_upgradeCardContainer, false);

            var rect = emptyObj.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(300, 100);

            var text = emptyObj.AddComponent<TextMeshProUGUI>();
            text.text = "No cards available to upgrade.\n<size=80%><color=#888888>Your deck is empty or all cards are already upgraded.</color></size>";
            text.fontSize = 14;
            text.alignment = TextAlignmentOptions.Center;
            text.color = new Color(0.7f, 0.7f, 0.7f);

            _spawnedCardSlots.Add(emptyObj);
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

            // Visual feedback - scale animation for selected/unselected
            for (int i = 0; i < _spawnedCardSlots.Count; i++)
            {
                var slot = _spawnedCardSlots[i];
                if (slot == null) continue;

                bool isSelected = (i == index);

                // Scale animation - selected card scales up, others scale down
                slot.transform.DOScale(isSelected ? 1.1f : 0.95f, 0.2f).SetEase(Ease.OutBack).SetLink(slot);

                // Optional: dim unselected cards via CanvasGroup alpha
                var canvasGroup = slot.GetComponent<CanvasGroup>();
                if (canvasGroup == null)
                {
                    canvasGroup = slot.AddComponent<CanvasGroup>();
                }
                canvasGroup.alpha = isSelected ? 1f : 0.6f;
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
                // Mark node as complete before navigation
                if (ServiceLocator.TryGet<MapManager>(out var mapManager))
                {
                    mapManager.CompleteCurrentNode();
                }

                NavigateToMap();
            }).SetLink(gameObject);
        }

        private void NavigateToMap()
        {
            // Use UIManager to show MapScreen (we're already in Run state, just switching screens)
            if (ServiceLocator.TryGet<IUIManager>(out var uiManager))
            {
                uiManager.ShowScreen<MapScreen>();
            }
            else
            {
                Debug.LogWarning("[SanctuaryScreen] UIManager not available for screen transition");
            }
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
                canvasGroup.DOFade(1f, _fadeInDuration).SetLink(gameObject);
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
                canvasGroup.DOFade(1f, _fadeInDuration).SetDelay(_fadeInDuration * 0.5f).SetLink(gameObject);
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
                    .SetEase(Ease.OutBack)
                    .SetLink(button.gameObject);

                var canvasGroup = button.GetComponent<CanvasGroup>();
                if (canvasGroup == null)
                {
                    canvasGroup = button.gameObject.AddComponent<CanvasGroup>();
                }
                canvasGroup.alpha = 0f;
                canvasGroup.DOFade(1f, _fadeInDuration)
                    .SetDelay(_fadeInDuration + i * _choiceStaggerDelay)
                    .SetLink(button.gameObject);
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
