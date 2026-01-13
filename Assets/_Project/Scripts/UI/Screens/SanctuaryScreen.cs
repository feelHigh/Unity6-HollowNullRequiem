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
                var emptySlot = CreateEmptyStateSlot();
                if (emptySlot != null)
                {
                    _spawnedCardSlots.Add(emptySlot);
                }
                return;
            }

            for (int i = 0; i < _upgradableCards.Count; i++)
            {
                var card = _upgradableCards[i];
                GameObject slot = null;

                // Use Card prefab for consistent visual display
                if (_cardSlotPrefab != null)
                {
                    slot = Instantiate(_cardSlotPrefab, _upgradeCardContainer);
                }
                else if (_cardPrefab != null)
                {
                    // Use Card prefab (unified card design)
                    var cardInstance = Instantiate(_cardPrefab, _upgradeCardContainer);
                    slot = cardInstance.gameObject;
                }
                else
                {
                    // Ultimate fallback - create minimal slot without problematic border
                    slot = CreateMinimalCardSlot(card);
                }

                if (slot != null)
                {
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
            }

            // Force layout rebuild after adding all cards
            if (_upgradeCardContainer is RectTransform containerRect)
            {
                Canvas.ForceUpdateCanvases();
                UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(containerRect);
                Debug.Log($"[SanctuaryScreen] Forced layout rebuild. Container size: {containerRect.rect.size}");
            }
        }

        /// <summary>
        /// Creates a minimal card slot without the problematic colored border.
        /// Used as ultimate fallback when no prefab is available.
        /// </summary>
        private GameObject CreateMinimalCardSlot(CardDataSO card)
        {
            var slot = new GameObject($"CardSlot_{card?.CardName ?? "Unknown"}");
            slot.transform.SetParent(_upgradeCardContainer, false);

            var rect = slot.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(140, 196); // Card aspect ratio

            // Neutral background (no colored border)
            var bgImage = slot.AddComponent<Image>();
            bgImage.color = new Color(0.15f, 0.15f, 0.2f, 1f); // Neutral dark
            bgImage.raycastTarget = true;

            var button = slot.AddComponent<Button>();
            button.targetGraphic = bgImage;

            // Cost text (top left)
            var costObj = new GameObject("CostText");
            costObj.transform.SetParent(slot.transform, false);
            var costRect = costObj.AddComponent<RectTransform>();
            costRect.anchorMin = new Vector2(0, 1);
            costRect.anchorMax = new Vector2(0, 1);
            costRect.pivot = new Vector2(0, 1);
            costRect.anchoredPosition = new Vector2(8, -8);
            costRect.sizeDelta = new Vector2(30, 30);
            var costText = costObj.AddComponent<TextMeshProUGUI>();
            SetupTMPText(costText, card?.APCost.ToString() ?? "?", 18, FontStyles.Bold, TextAlignmentOptions.Center);
            costText.color = new Color(0.5f, 0.85f, 1f);

            // Name text (center)
            var nameObj = new GameObject("NameText");
            nameObj.transform.SetParent(slot.transform, false);
            var nameRect = nameObj.AddComponent<RectTransform>();
            nameRect.anchorMin = new Vector2(0, 0.4f);
            nameRect.anchorMax = new Vector2(1, 0.6f);
            nameRect.offsetMin = new Vector2(8, 0);
            nameRect.offsetMax = new Vector2(-8, 0);
            var nameText = nameObj.AddComponent<TextMeshProUGUI>();
            SetupTMPText(nameText, card?.CardName ?? "Unknown", 14, FontStyles.Bold, TextAlignmentOptions.Center);
            nameText.color = Color.white;

            // Description text (bottom)
            var descObj = new GameObject("DescText");
            descObj.transform.SetParent(slot.transform, false);
            var descRect = descObj.AddComponent<RectTransform>();
            descRect.anchorMin = new Vector2(0, 0);
            descRect.anchorMax = new Vector2(1, 0.35f);
            descRect.offsetMin = new Vector2(8, 8);
            descRect.offsetMax = new Vector2(-8, 0);
            var descText = descObj.AddComponent<TextMeshProUGUI>();
            SetupTMPText(descText, card?.GetFormattedDescription() ?? "", 10, FontStyles.Normal, TextAlignmentOptions.Center);
            descText.color = new Color(0.8f, 0.8f, 0.8f);
            descText.textWrappingMode = TextWrappingModes.Normal;

            return slot;
        }

        private GameObject CreatePlaceholderCardSlot(CardDataSO card)
        {
            var slot = new GameObject($"UpgradeSlot_{card?.CardName ?? "Unknown"}");
            slot.transform.SetParent(_upgradeCardContainer, false);

            // GridLayoutGroup will control size, but set initial size
            var rect = slot.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(100, 140);

            // Ensure CanvasRenderer exists for UI rendering
            if (slot.GetComponent<CanvasRenderer>() == null)
            {
                slot.AddComponent<CanvasRenderer>();
            }

            // Card background - use BRIGHT visible colors
            var bgImage = slot.AddComponent<Image>();
            bgImage.color = GetCardBackgroundColorBright(card?.CardType ?? CardType.Skill);
            bgImage.raycastTarget = true;

            // Create a border child for the frame effect (more reliable than Outline)
            CreateCardBorder(slot.transform, card?.CardType ?? CardType.Skill);

            var button = slot.AddComponent<Button>();
            button.targetGraphic = bgImage;

            // Configure button colors - use actual color values not tints
            var colors = button.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(1f, 1f, 0.8f); // Slight yellow highlight
            colors.pressedColor = new Color(0.8f, 0.8f, 0.8f);
            colors.selectedColor = new Color(0.9f, 1f, 0.9f); // Slight green for selected
            colors.disabledColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);
            button.colors = colors;

            // Cost orb (top left)
            CreateCostOrb(slot.transform, card?.APCost ?? 0);

            // Card name (top center)
            CreateCardNameText(slot.transform, card?.CardName ?? "Unknown");

            // Card type indicator (middle)
            CreateCardTypeText(slot.transform, card?.CardType ?? CardType.Skill);

            // Card description (bottom)
            CreateCardDescription(slot.transform, card?.GetFormattedDescription() ?? "");

            Debug.Log($"[SanctuaryScreen] Created card visual: {card?.CardName}, BG color: {bgImage.color}");

            return slot;
        }

        private void CreateCardBorder(Transform parent, CardType type)
        {
            var borderObj = new GameObject("Border");
            borderObj.transform.SetParent(parent, false);

            var borderRect = borderObj.AddComponent<RectTransform>();
            borderRect.anchorMin = Vector2.zero;
            borderRect.anchorMax = Vector2.one;
            borderRect.sizeDelta = Vector2.zero;
            borderRect.offsetMin = new Vector2(-3, -3);
            borderRect.offsetMax = new Vector2(3, 3);

            // Put border behind parent by setting sibling index
            borderObj.transform.SetAsFirstSibling();

            var borderImage = borderObj.AddComponent<Image>();
            borderImage.color = GetCardFrameColor(type);
            borderImage.raycastTarget = false;
        }

        private Color GetCardBackgroundColorBright(CardType type)
        {
            // Use much brighter, more visible colors
            return type switch
            {
                CardType.Strike => new Color(0.4f, 0.15f, 0.15f), // Visible dark red
                CardType.Guard => new Color(0.15f, 0.25f, 0.45f),  // Visible dark blue
                CardType.Skill => new Color(0.15f, 0.35f, 0.2f),   // Visible dark green
                CardType.Power => new Color(0.35f, 0.15f, 0.4f),   // Visible dark purple
                _ => new Color(0.3f, 0.3f, 0.35f)                   // Visible gray
            };
        }

        private void CreateCostOrb(Transform parent, int cost)
        {
            var orbObj = new GameObject("CostOrb");
            orbObj.transform.SetParent(parent, false);

            var orbRect = orbObj.AddComponent<RectTransform>();
            orbRect.anchorMin = new Vector2(0, 1);
            orbRect.anchorMax = new Vector2(0, 1);
            orbRect.pivot = new Vector2(0, 1);
            orbRect.anchoredPosition = new Vector2(4, -4);
            orbRect.sizeDelta = new Vector2(26, 26);

            var orbImage = orbObj.AddComponent<Image>();
            orbImage.color = new Color(0.1f, 0.15f, 0.35f, 1f); // Visible dark blue
            orbImage.raycastTarget = false;

            // Cost text
            var costTextObj = new GameObject("CostText");
            costTextObj.transform.SetParent(orbObj.transform, false);

            var costTextRect = costTextObj.AddComponent<RectTransform>();
            costTextRect.anchorMin = Vector2.zero;
            costTextRect.anchorMax = Vector2.one;
            costTextRect.sizeDelta = Vector2.zero;
            costTextRect.offsetMin = Vector2.zero;
            costTextRect.offsetMax = Vector2.zero;

            var costText = costTextObj.AddComponent<TextMeshProUGUI>();
            SetupTMPText(costText, cost.ToString(), 14, FontStyles.Bold, TextAlignmentOptions.Center);
            costText.color = new Color(0.5f, 0.85f, 1f); // Bright cyan for AP
        }

        /// <summary>
        /// Helper to set up TMP text with default font.
        /// </summary>
        private void SetupTMPText(TextMeshProUGUI tmp, string text, float fontSize, FontStyles style, TextAlignmentOptions alignment)
        {
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.fontStyle = style;
            tmp.alignment = alignment;
            tmp.textWrappingMode = TextWrappingModes.Normal;
            tmp.overflowMode = TextOverflowModes.Ellipsis;

            // Ensure font is assigned - TMP requires font asset
            if (tmp.font == null)
            {
                // Try to get default font from TMP Settings
                var defaultFont = TMPro.TMP_Settings.defaultFontAsset;
                if (defaultFont != null)
                {
                    tmp.font = defaultFont;
                }
                else
                {
                    // Fallback: try to load LiberationSans SDF
                    var fallbackFont = Resources.Load<TMP_FontAsset>("Fonts & Materials/LiberationSans SDF");
                    if (fallbackFont != null)
                    {
                        tmp.font = fallbackFont;
                    }
                    else
                    {
                        Debug.LogWarning("[SanctuaryScreen] No TMP font available - text may not render");
                    }
                }
            }
        }

        private void CreateCardNameText(Transform parent, string cardName)
        {
            var nameObj = new GameObject("CardName");
            nameObj.transform.SetParent(parent, false);

            var nameRect = nameObj.AddComponent<RectTransform>();
            nameRect.anchorMin = new Vector2(0, 1);
            nameRect.anchorMax = new Vector2(1, 1);
            nameRect.pivot = new Vector2(0.5f, 1);
            nameRect.anchoredPosition = new Vector2(0, -4);
            nameRect.sizeDelta = new Vector2(-8, 28);

            var nameText = nameObj.AddComponent<TextMeshProUGUI>();
            SetupTMPText(nameText, cardName, 11, FontStyles.Bold, TextAlignmentOptions.Center);
            nameText.color = Color.white;
        }

        private void CreateCardTypeText(Transform parent, CardType cardType)
        {
            var typeObj = new GameObject("CardType");
            typeObj.transform.SetParent(parent, false);

            var typeRect = typeObj.AddComponent<RectTransform>();
            typeRect.anchorMin = new Vector2(0, 0.5f);
            typeRect.anchorMax = new Vector2(1, 0.5f);
            typeRect.pivot = new Vector2(0.5f, 0.5f);
            typeRect.anchoredPosition = new Vector2(0, 15);
            typeRect.sizeDelta = new Vector2(-8, 16);

            var typeText = typeObj.AddComponent<TextMeshProUGUI>();
            SetupTMPText(typeText, $"[{cardType}]", 9, FontStyles.Normal, TextAlignmentOptions.Center);
            typeText.color = GetCardFrameColor(cardType);
        }

        private void CreateCardDescription(Transform parent, string description)
        {
            var descObj = new GameObject("Description");
            descObj.transform.SetParent(parent, false);

            var descRect = descObj.AddComponent<RectTransform>();
            descRect.anchorMin = new Vector2(0, 0);
            descRect.anchorMax = new Vector2(1, 0.5f);
            descRect.pivot = new Vector2(0.5f, 0);
            descRect.anchoredPosition = new Vector2(0, 6);
            descRect.sizeDelta = new Vector2(-8, -16);

            var descText = descObj.AddComponent<TextMeshProUGUI>();
            SetupTMPText(descText, description, 8, FontStyles.Normal, TextAlignmentOptions.Center);
            descText.color = new Color(0.9f, 0.9f, 0.9f);
        }

        private Color GetCardBackgroundColor(CardType type)
        {
            return type switch
            {
                CardType.Strike => new Color(0.25f, 0.12f, 0.12f), // Dark red
                CardType.Guard => new Color(0.12f, 0.18f, 0.28f),  // Dark blue
                CardType.Skill => new Color(0.12f, 0.22f, 0.15f),  // Dark green
                CardType.Power => new Color(0.2f, 0.12f, 0.25f),   // Dark purple
                _ => new Color(0.18f, 0.18f, 0.22f)                // Dark gray
            };
        }

        private Color GetCardFrameColor(CardType type)
        {
            return type switch
            {
                CardType.Strike => new Color(0.9f, 0.3f, 0.3f),  // Red
                CardType.Guard => new Color(0.3f, 0.5f, 0.9f),   // Blue
                CardType.Skill => new Color(0.3f, 0.8f, 0.4f),   // Green
                CardType.Power => new Color(0.7f, 0.3f, 0.8f),   // Purple
                _ => new Color(0.6f, 0.6f, 0.6f)                 // Gray
            };
        }

        private GameObject CreateEmptyStateSlot()
        {
            var slot = new GameObject("EmptyState");
            slot.transform.SetParent(_upgradeCardContainer, false);

            var rect = slot.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(300, 100);

            // Empty state message
            var textObj = new GameObject("Message");
            textObj.transform.SetParent(slot.transform, false);

            var textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.sizeDelta = Vector2.zero;

            var text = textObj.AddComponent<TextMeshProUGUI>();
            text.text = "No cards available to upgrade.\n<size=80%><color=#888888>Your deck is empty or all cards are already upgraded.</color></size>";
            text.fontSize = 14;
            text.alignment = TextAlignmentOptions.Center;
            text.color = new Color(0.7f, 0.7f, 0.7f);

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

            // Visual feedback - highlight selected, dim others
            for (int i = 0; i < _spawnedCardSlots.Count; i++)
            {
                var slot = _spawnedCardSlots[i];
                if (slot == null) continue;

                bool isSelected = (i == index);

                // Get original card type color or use default
                CardType cardType = (i < _upgradableCards.Count) ? _upgradableCards[i].CardType : CardType.Skill;
                Color baseColor = GetCardBackgroundColorBright(cardType);

                var image = slot.GetComponent<Image>();
                if (image != null)
                {
                    if (isSelected)
                    {
                        // Brighten selected card
                        image.color = new Color(
                            Mathf.Min(baseColor.r * 1.5f, 1f),
                            Mathf.Min(baseColor.g * 1.5f, 1f),
                            Mathf.Min(baseColor.b * 1.5f, 1f),
                            1f
                        );
                    }
                    else
                    {
                        // Dim unselected cards slightly
                        image.color = new Color(baseColor.r * 0.6f, baseColor.g * 0.6f, baseColor.b * 0.6f, 0.8f);
                    }
                }

                // Scale animation
                slot.transform.DOScale(isSelected ? 1.1f : 0.95f, 0.2f).SetEase(Ease.OutBack).SetLink(slot);
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
