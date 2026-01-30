// ============================================
// RelicShopOverlay.cs
// Modal overlay for viewing and purchasing relics in the shop
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
using HNR.Progression;
using HNR.UI.Config;

namespace HNR.UI.Components
{
    /// <summary>
    /// Modal overlay for viewing and purchasing relics.
    /// Shows available relics in a grid with detailed info panel.
    /// </summary>
    public class RelicShopOverlay : MonoBehaviour
    {
        // ============================================
        // UI References - Modal Container
        // ============================================

        [Header("Modal Container")]
        [SerializeField, Tooltip("The modal panel root")]
        private GameObject _modalPanel;

        [SerializeField, Tooltip("Canvas group for fade animation")]
        private CanvasGroup _canvasGroup;

        [SerializeField, Tooltip("Background overlay for blocking input")]
        private Image _backgroundOverlay;

        // ============================================
        // UI References - Header
        // ============================================

        [Header("Header")]
        [SerializeField, Tooltip("Modal title text")]
        private TMP_Text _titleText;

        [SerializeField, Tooltip("Current Void Shards balance text")]
        private TMP_Text _voidShardsText;

        // ============================================
        // UI References - Relic Display
        // ============================================

        [Header("Relic Display")]
        [SerializeField, Tooltip("Container for relic slots (GridLayoutGroup)")]
        private Transform _relicContainer;

        [SerializeField, Tooltip("Relic slot prefab")]
        private GameObject _relicSlotPrefab;

        [SerializeField, Tooltip("Scroll rect for relic list")]
        private ScrollRect _scrollRect;

        // ============================================
        // UI References - Details Panel
        // ============================================

        [Header("Details Panel")]
        [SerializeField, Tooltip("Details panel root")]
        private GameObject _detailsPanel;

        [SerializeField, Tooltip("Selected relic icon (96x96)")]
        private Image _selectedRelicIcon;

        [SerializeField, Tooltip("Selected relic name")]
        private TMP_Text _selectedRelicName;

        [SerializeField, Tooltip("Selected relic description")]
        private TMP_Text _selectedRelicDescription;

        [SerializeField, Tooltip("Selected relic price")]
        private TMP_Text _selectedRelicPrice;

        [SerializeField, Tooltip("Purchase button")]
        private Button _purchaseButton;

        [SerializeField, Tooltip("Purchase button text")]
        private TMP_Text _purchaseButtonText;

        // ============================================
        // UI References - Actions
        // ============================================

        [Header("Actions")]
        [SerializeField, Tooltip("Close button")]
        private Button _closeButton;

        // ============================================
        // Animation Settings
        // ============================================

        [Header("Animation")]
        [SerializeField] private float _fadeInDuration = 0.3f;
        [SerializeField] private float _fadeOutDuration = 0.2f;

        // ============================================
        // Colors
        // ============================================

        [Header("Colors")]
        [SerializeField] private Color _affordableColor = Color.white;
        [SerializeField] private Color _unaffordableColor = new Color(0.6f, 0.3f, 0.3f);
        [SerializeField] private Color _purchaseButtonActiveColor = new Color(0.2f, 0.5f, 0.3f, 1f);
        [SerializeField] private Color _purchaseButtonDisabledColor = new Color(0.3f, 0.3f, 0.3f, 0.6f);

        // ============================================
        // State
        // ============================================

        private IShopManager _shopManager;
        private ShopItem _selectedItem;
        private Action _onClosed;
        private List<RelicShopSlot> _relicSlots = new();

        // ============================================
        // Properties
        // ============================================

        /// <summary>Whether the overlay is currently visible.</summary>
        public bool IsVisible => _modalPanel != null && _modalPanel.activeSelf;

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
            if (_purchaseButton != null)
            {
                _purchaseButton.onClick.AddListener(OnPurchaseClicked);
            }

            if (_closeButton != null)
            {
                _closeButton.onClick.AddListener(OnCloseClicked);
            }
        }

        private void OnDestroy()
        {
            // Unsubscribe from events
            EventBus.Unsubscribe<VoidShardsChangedEvent>(OnVoidShardsChanged);

            if (_purchaseButton != null)
            {
                _purchaseButton.onClick.RemoveListener(OnPurchaseClicked);
            }

            if (_closeButton != null)
            {
                _closeButton.onClick.RemoveListener(OnCloseClicked);
            }

            ClearRelicSlots();
        }

        // ============================================
        // Public API
        // ============================================

        /// <summary>
        /// Shows the relic shop overlay.
        /// </summary>
        /// <param name="onClosed">Callback when overlay is closed.</param>
        public void Show(Action onClosed = null)
        {
            _onClosed = onClosed;
            _selectedItem = null;

            // Get service references
            _shopManager = ServiceLocator.Get<IShopManager>();

            // Subscribe to events
            EventBus.Subscribe<VoidShardsChangedEvent>(OnVoidShardsChanged);

            // Update UI
            UpdateHeader();
            PopulateRelics();
            ClearSelection();

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

            Debug.Log($"[RelicShopOverlay] Showing overlay with {_relicSlots.Count} relics");
        }

        /// <summary>
        /// Hides the relic shop overlay.
        /// </summary>
        public void Hide()
        {
            // Unsubscribe from events
            EventBus.Unsubscribe<VoidShardsChangedEvent>(OnVoidShardsChanged);

            if (_canvasGroup != null)
            {
                _canvasGroup.DOFade(0f, _fadeOutDuration)
                    .OnComplete(() =>
                    {
                        if (_modalPanel != null)
                        {
                            _modalPanel.SetActive(false);
                        }
                        ClearRelicSlots();
                        _onClosed?.Invoke();
                    })
                    .SetLink(gameObject);
            }
            else
            {
                if (_modalPanel != null)
                {
                    _modalPanel.SetActive(false);
                }
                ClearRelicSlots();
                _onClosed?.Invoke();
            }

            Debug.Log("[RelicShopOverlay] Overlay hidden");
        }

        // ============================================
        // UI Setup
        // ============================================

        private void UpdateHeader()
        {
            if (_titleText != null)
            {
                _titleText.text = "RELICS";
            }

            RefreshVoidShards();
        }

        private void RefreshVoidShards()
        {
            if (_voidShardsText != null && _shopManager != null)
            {
                _voidShardsText.text = _shopManager.VoidShards.ToString("N0");
            }
        }

        private void PopulateRelics()
        {
            ClearRelicSlots();

            if (_shopManager?.CurrentInventory == null)
            {
                Debug.LogWarning("[RelicShopOverlay] No shop inventory available");
                return;
            }

            // Get only relic items
            var relics = _shopManager.CurrentInventory.GetItemsByType(ShopItemType.Relic);

            if (relics.Count == 0)
            {
                Debug.Log("[RelicShopOverlay] No relics in shop inventory");
                return;
            }

            if (_relicContainer == null)
            {
                Debug.LogWarning("[RelicShopOverlay] Relic container not assigned");
                return;
            }

            foreach (var relic in relics)
            {
                CreateRelicSlot(relic);
            }

            Debug.Log($"[RelicShopOverlay] Created {_relicSlots.Count} relic slots");
        }

        private void CreateRelicSlot(ShopItem item)
        {
            // Use local prefab or fall back to config
            var prefab = _relicSlotPrefab ?? RuntimeUIPrefabConfigSO.Instance?.RelicShopSlotPrefab;

            if (prefab == null)
            {
                Debug.LogError($"[RelicShopOverlay] Relic slot prefab not assigned. Check RuntimeUIPrefabConfig.");
                return;
            }

            var slotObj = Instantiate(prefab, _relicContainer);
            slotObj.name = $"RelicSlot_{item.DisplayName}";

            var slot = slotObj.GetComponent<RelicShopSlot>();
            if (slot != null)
            {
                slot.Initialize(item, OnRelicSelected);
                slot.UpdateVisuals(_shopManager?.VoidShards ?? 0);
                _relicSlots.Add(slot);
            }
            else
            {
                Debug.LogWarning("[RelicShopOverlay] Relic slot prefab missing RelicShopSlot component");
            }
        }

        private void ClearRelicSlots()
        {
            foreach (var slot in _relicSlots)
            {
                if (slot != null)
                {
                    Destroy(slot.gameObject);
                }
            }
            _relicSlots.Clear();
        }

        // ============================================
        // Selection
        // ============================================

        private void OnRelicSelected(ShopItem item)
        {
            _selectedItem = item;

            // Update slot selection visuals
            foreach (var slot in _relicSlots)
            {
                slot?.SetSelected(slot.Item == item);
            }

            RefreshDetailsPanel();
            Debug.Log($"[RelicShopOverlay] Selected relic: {item.DisplayName}");
        }

        private void ClearSelection()
        {
            _selectedItem = null;

            foreach (var slot in _relicSlots)
            {
                slot?.SetSelected(false);
            }

            if (_detailsPanel != null)
            {
                _detailsPanel.SetActive(false);
            }
        }

        private void RefreshDetailsPanel()
        {
            if (_detailsPanel == null) return;

            if (_selectedItem == null)
            {
                _detailsPanel.SetActive(false);
                return;
            }

            _detailsPanel.SetActive(true);

            var relicData = _selectedItem.RelicData;

            // Update icon
            if (_selectedRelicIcon != null)
            {
                var icon = relicData?.Icon;
                _selectedRelicIcon.sprite = icon;

                // If no icon, show placeholder color instead of hiding
                if (icon == null)
                {
                    _selectedRelicIcon.color = new Color(0.4f, 0.3f, 0.5f, 0.8f); // Purple placeholder
                }
                else
                {
                    _selectedRelicIcon.color = Color.white;
                }
                _selectedRelicIcon.gameObject.SetActive(true);
            }

            // Update name
            if (_selectedRelicName != null)
            {
                _selectedRelicName.text = relicData?.RelicName ?? "Unknown Relic";
            }

            // Update description
            if (_selectedRelicDescription != null)
            {
                _selectedRelicDescription.text = relicData?.GetFormattedDescription() ?? "";
            }

            // Update price
            if (_selectedRelicPrice != null)
            {
                _selectedRelicPrice.text = $"{_selectedItem.Price} Shards";
            }

            RefreshPurchaseButton();
        }

        private void RefreshPurchaseButton()
        {
            if (_purchaseButton == null) return;

            bool canPurchase = _selectedItem != null &&
                               !_selectedItem.IsPurchased &&
                               _shopManager != null &&
                               _selectedItem.CanPurchase(_shopManager.VoidShards);

            _purchaseButton.interactable = canPurchase;

            // Update button visual appearance
            var buttonImage = _purchaseButton.GetComponent<Image>();
            if (buttonImage != null)
            {
                buttonImage.color = canPurchase ? _purchaseButtonActiveColor : _purchaseButtonDisabledColor;
            }

            if (_purchaseButtonText != null)
            {
                if (_selectedItem == null)
                {
                    _purchaseButtonText.text = "Select Relic";
                }
                else if (_selectedItem.IsPurchased)
                {
                    _purchaseButtonText.text = "Sold";
                }
                else
                {
                    // Always show "Buy" when relic is selected and not purchased
                    _purchaseButtonText.text = "Buy";
                }

                // Adjust text color based on affordability
                _purchaseButtonText.color = canPurchase ? Color.white : new Color(0.7f, 0.7f, 0.7f, 0.8f);
            }
        }

        // ============================================
        // Event Handlers
        // ============================================

        private void OnVoidShardsChanged(VoidShardsChangedEvent evt)
        {
            RefreshVoidShards();
            RefreshRelicSlots();
            RefreshPurchaseButton();
        }

        private void RefreshRelicSlots()
        {
            int currentShards = _shopManager?.VoidShards ?? 0;
            foreach (var slot in _relicSlots)
            {
                slot?.UpdateVisuals(currentShards);
            }
        }

        // ============================================
        // Button Handlers
        // ============================================

        private void OnPurchaseClicked()
        {
            if (_selectedItem == null || _shopManager == null) return;

            if (_shopManager.PurchaseItem(_selectedItem))
            {
                Debug.Log($"[RelicShopOverlay] Successfully purchased relic: {_selectedItem.DisplayName}");

                // Refresh visuals after purchase
                RefreshRelicSlots();
                RefreshDetailsPanel();

                // Check if all relics are sold
                bool hasAvailable = _relicSlots.Exists(s => s.Item != null && !s.Item.IsPurchased);
                if (!hasAvailable)
                {
                    Debug.Log("[RelicShopOverlay] All relics purchased, closing overlay");
                    Hide();
                }
            }
            else
            {
                Debug.Log($"[RelicShopOverlay] Failed to purchase relic: {_selectedItem.DisplayName}");
            }
        }

        private void OnCloseClicked()
        {
            Debug.Log("[RelicShopOverlay] Close button clicked");
            Hide();
        }
    }
}
