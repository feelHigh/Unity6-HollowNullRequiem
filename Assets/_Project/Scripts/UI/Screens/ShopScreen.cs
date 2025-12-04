// ============================================
// ShopScreen.cs
// Void Market shop UI screen
// ============================================

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using HNR.Core;
using HNR.Core.Events;
using HNR.Core.Interfaces;
using HNR.Progression;
using HNR.Map;

namespace HNR.UI
{
    /// <summary>
    /// UI screen for the Void Market shop.
    /// Displays purchasable items, handles selection, and processes transactions.
    /// </summary>
    public class ShopScreen : ScreenBase
    {
        // ============================================
        // Serialized Fields
        // ============================================

        [Header("Shop UI")]
        [SerializeField, Tooltip("Text displaying current Void Shard balance")]
        private TMP_Text _voidShardsText;

        [SerializeField, Tooltip("Container for owned relic icons")]
        private Transform _relicContainer;

        [SerializeField, Tooltip("Prefab for relic icon display")]
        private GameObject _relicIconPrefab;

        [SerializeField, Tooltip("Container for shop item slots")]
        private Transform _itemContainer;

        [SerializeField, Tooltip("Prefab for shop item slots")]
        private GameObject _itemSlotPrefab;

        [SerializeField, Tooltip("Button to leave the shop")]
        private Button _leaveButton;

        [Header("Item Details Panel")]
        [SerializeField, Tooltip("Panel showing selected item details")]
        private GameObject _detailsPanel;

        [SerializeField, Tooltip("Image showing selected item icon")]
        private Image _itemIconImage;

        [SerializeField, Tooltip("Text showing selected item name")]
        private TMP_Text _itemNameText;

        [SerializeField, Tooltip("Text showing selected item description")]
        private TMP_Text _itemDescriptionText;

        [SerializeField, Tooltip("Text showing selected item price")]
        private TMP_Text _itemPriceText;

        [SerializeField, Tooltip("Button to purchase selected item")]
        private Button _purchaseButton;

        [SerializeField, Tooltip("Text on purchase button")]
        private TMP_Text _purchaseButtonText;

        [Header("Visual Feedback")]
        [SerializeField, Tooltip("Color for affordable items")]
        private Color _affordableColor = Color.white;

        [SerializeField, Tooltip("Color for unaffordable items")]
        private Color _unaffordableColor = new Color(0.5f, 0.5f, 0.5f, 1f);

        [SerializeField, Tooltip("Color for purchased items")]
        private Color _purchasedColor = new Color(0.3f, 0.3f, 0.3f, 0.5f);

        // ============================================
        // Private Fields
        // ============================================

        private IShopManager _shopManager;
        private IRelicManager _relicManager;
        private ShopItem _selectedItem;
        private readonly List<ShopItemSlot> _itemSlots = new();

        // ============================================
        // Screen Lifecycle
        // ============================================

        public override void OnShow()
        {
            base.OnShow();

            // Get service references
            _shopManager = ServiceLocator.Get<IShopManager>();
            _relicManager = ServiceLocator.Get<IRelicManager>();

            // Subscribe to events
            EventBus.Subscribe<VoidShardsChangedEvent>(OnVoidShardsChanged);
            EventBus.Subscribe<ShopItemPurchasedEvent>(OnItemPurchased);

            // Setup UI
            SetupButtons();
            RefreshVoidShards();
            RefreshRelicDisplay();
            CreateItemSlots();
            ClearSelection();

            Debug.Log("[ShopScreen] Shop screen opened");
        }

        public override void OnHide()
        {
            base.OnHide();

            // Unsubscribe from events
            EventBus.Unsubscribe<VoidShardsChangedEvent>(OnVoidShardsChanged);
            EventBus.Unsubscribe<ShopItemPurchasedEvent>(OnItemPurchased);

            // Close the shop
            _shopManager?.CloseShop();

            Debug.Log("[ShopScreen] Shop screen closed");
        }

        // ============================================
        // Setup
        // ============================================

        private void SetupButtons()
        {
            if (_leaveButton != null)
            {
                _leaveButton.onClick.RemoveAllListeners();
                _leaveButton.onClick.AddListener(OnLeaveClicked);
            }

            if (_purchaseButton != null)
            {
                _purchaseButton.onClick.RemoveAllListeners();
                _purchaseButton.onClick.AddListener(OnPurchaseClicked);
            }
        }

        // ============================================
        // Event Handlers
        // ============================================

        private void OnVoidShardsChanged(VoidShardsChangedEvent evt)
        {
            RefreshVoidShards();
            RefreshItemSlots();
            RefreshDetailsPanel();
        }

        private void OnItemPurchased(ShopItemPurchasedEvent evt)
        {
            RefreshItemSlots();
            RefreshRelicDisplay();

            // Clear selection if purchased item was selected
            if (_selectedItem == evt.Item)
            {
                ClearSelection();
            }

            Debug.Log($"[ShopScreen] Item purchased: {evt.Item.DisplayName}");
        }

        // ============================================
        // UI Refresh
        // ============================================

        private void RefreshVoidShards()
        {
            if (_voidShardsText != null && _shopManager != null)
            {
                _voidShardsText.text = _shopManager.VoidShards.ToString("N0");
            }
        }

        private void RefreshRelicDisplay()
        {
            if (_relicContainer == null || _relicManager == null) return;

            // Clear existing icons
            foreach (Transform child in _relicContainer)
            {
                Destroy(child.gameObject);
            }

            // Create icons for owned relics
            var ownedRelics = _relicManager.OwnedRelics;
            foreach (var relic in ownedRelics)
            {
                if (_relicIconPrefab != null)
                {
                    var iconGO = Instantiate(_relicIconPrefab, _relicContainer);
                    var iconImage = iconGO.GetComponent<Image>();
                    if (iconImage != null && relic.Icon != null)
                    {
                        iconImage.sprite = relic.Icon;
                    }
                }
            }
        }

        // ============================================
        // Item Slots
        // ============================================

        private void CreateItemSlots()
        {
            ClearItemSlots();

            if (_shopManager?.CurrentInventory == null)
            {
                Debug.LogWarning("[ShopScreen] No shop inventory available");
                return;
            }

            foreach (var item in _shopManager.CurrentInventory.Items)
            {
                CreateItemSlot(item);
            }

            Debug.Log($"[ShopScreen] Created {_itemSlots.Count} item slots");
        }

        private void CreateItemSlot(ShopItem item)
        {
            if (_itemSlotPrefab == null || _itemContainer == null) return;

            var slotGO = Instantiate(_itemSlotPrefab, _itemContainer);
            var slot = slotGO.GetComponent<ShopItemSlot>();

            if (slot != null)
            {
                slot.Initialize(item, OnItemSelected);
                slot.UpdateVisuals(_shopManager?.VoidShards ?? 0);
                _itemSlots.Add(slot);
            }
            else
            {
                Debug.LogWarning("[ShopScreen] Item slot prefab missing ShopItemSlot component");
            }
        }

        private void ClearItemSlots()
        {
            foreach (var slot in _itemSlots)
            {
                if (slot != null)
                {
                    Destroy(slot.gameObject);
                }
            }
            _itemSlots.Clear();
        }

        private void RefreshItemSlots()
        {
            int currentShards = _shopManager?.VoidShards ?? 0;
            foreach (var slot in _itemSlots)
            {
                slot?.UpdateVisuals(currentShards);
            }
        }

        // ============================================
        // Selection
        // ============================================

        private void OnItemSelected(ShopItem item)
        {
            _selectedItem = item;

            // Update slot selection visuals
            foreach (var slot in _itemSlots)
            {
                slot?.SetSelected(slot.Item == item);
            }

            RefreshDetailsPanel();
            Debug.Log($"[ShopScreen] Selected item: {item.DisplayName}");
        }

        private void ClearSelection()
        {
            _selectedItem = null;

            foreach (var slot in _itemSlots)
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

            // Update icon
            if (_itemIconImage != null)
            {
                _itemIconImage.sprite = _selectedItem.Icon;
                _itemIconImage.gameObject.SetActive(_selectedItem.Icon != null);
            }

            // Update name
            if (_itemNameText != null)
            {
                _itemNameText.text = _selectedItem.DisplayName;
            }

            // Update description
            if (_itemDescriptionText != null)
            {
                _itemDescriptionText.text = _selectedItem.Description;
            }

            // Update price
            if (_itemPriceText != null)
            {
                _itemPriceText.text = $"{_selectedItem.Price} Shards";
            }

            // Update purchase button
            UpdatePurchaseButton();
        }

        private void UpdatePurchaseButton()
        {
            if (_purchaseButton == null) return;

            bool canPurchase = _selectedItem != null &&
                               !_selectedItem.IsPurchased &&
                               _shopManager != null &&
                               _selectedItem.CanPurchase(_shopManager.VoidShards);

            _purchaseButton.interactable = canPurchase;

            if (_purchaseButtonText != null)
            {
                if (_selectedItem == null)
                {
                    _purchaseButtonText.text = "Select Item";
                }
                else if (_selectedItem.IsPurchased)
                {
                    _purchaseButtonText.text = "Sold";
                }
                else if (!_selectedItem.CanPurchase(_shopManager?.VoidShards ?? 0))
                {
                    _purchaseButtonText.text = "Not Enough Shards";
                }
                else
                {
                    _purchaseButtonText.text = "Purchase";
                }
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
                Debug.Log($"[ShopScreen] Successfully purchased: {_selectedItem.DisplayName}");
                // Events will handle UI refresh
            }
            else
            {
                Debug.Log($"[ShopScreen] Failed to purchase: {_selectedItem.DisplayName}");
            }
        }

        private void OnLeaveClicked()
        {
            Debug.Log("[ShopScreen] Leaving shop");

            // Navigate back to map
            var uiManager = ServiceLocator.Get<IUIManager>();
            if (uiManager != null)
            {
                uiManager.ShowScreen<MapScreen>();
            }
            else
            {
                // Fallback: try to complete the node
                var mapManager = ServiceLocator.Get<MapManager>();
                mapManager?.CompleteCurrentNode();
            }
        }

        // ============================================
        // Public Methods
        // ============================================

        /// <summary>
        /// Force refresh all shop UI elements.
        /// </summary>
        public void RefreshAll()
        {
            RefreshVoidShards();
            RefreshRelicDisplay();
            RefreshItemSlots();
            RefreshDetailsPanel();
        }
    }
}
