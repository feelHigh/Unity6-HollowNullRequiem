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
using HNR.UI.Components;

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

        [Header("Shop Services")]
        [SerializeField, Tooltip("Button to remove a card from deck (costs shards)")]
        private Button _removeCardButton;

        [SerializeField, Tooltip("Remove card cost text")]
        private TMP_Text _removeCardCostText;

        [SerializeField, Tooltip("Button to purify corruption (costs shards)")]
        private Button _purifyButton;

        [SerializeField, Tooltip("Purify cost text")]
        private TMP_Text _purifyCostText;

        [SerializeField, Tooltip("Cost to remove a card")]
        private int _removeCardCost = 75;

        [SerializeField, Tooltip("Cost to purify corruption")]
        private int _purifyCost = 50;

        [SerializeField, Tooltip("Amount of corruption removed by purify")]
        private int _purifyAmount = 30;

        [SerializeField, Tooltip("Deck viewer modal for card removal")]
        private DeckViewerModal _deckViewerModal;

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
            RefreshServiceButtons();

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

            if (_removeCardButton != null)
            {
                _removeCardButton.onClick.RemoveAllListeners();
                _removeCardButton.onClick.AddListener(OnRemoveCardClicked);
            }

            if (_purifyButton != null)
            {
                _purifyButton.onClick.RemoveAllListeners();
                _purifyButton.onClick.AddListener(OnPurifyClicked);
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
            RefreshServiceButtons();
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

            // Complete the shop node
            var mapManager = ServiceLocator.Get<MapManager>();
            mapManager?.CompleteCurrentNode();

            // Navigate back to map
            var uiManager = ServiceLocator.Get<IUIManager>();
            uiManager?.ShowScreen<MapScreen>();
        }

        private void OnRemoveCardClicked()
        {
            if (_shopManager == null) return;

            int shards = _shopManager.VoidShards;
            if (shards < _removeCardCost)
            {
                Debug.Log("[ShopScreen] Not enough shards to remove card");
                return;
            }

            // Find or use assigned DeckViewerModal
            var modal = _deckViewerModal;
            if (modal == null)
            {
                modal = FindAnyObjectByType<DeckViewerModal>(FindObjectsInactive.Include);
            }

            if (modal == null)
            {
                Debug.LogWarning("[ShopScreen] DeckViewerModal not found - cannot show card removal UI");
                // Fallback: just publish event in case there's a handler
                EventBus.Publish(new ShopRemoveCardRequestedEvent());
                return;
            }

            // Spend shards first
            _shopManager.SpendVoidShards(_removeCardCost);

            // Show deck viewer in remove mode
            modal.Show(DeckViewerModal.ViewMode.RemoveCard, (removedCard) =>
            {
                if (removedCard != null)
                {
                    Debug.Log($"[ShopScreen] Card removed: {removedCard.CardName}");
                    RefreshServiceButtons();
                }
                else
                {
                    // User cancelled - refund the shards
                    _shopManager.AddVoidShards(_removeCardCost);
                    Debug.Log("[ShopScreen] Card removal cancelled - refunded shards");
                }
            });

            Debug.Log($"[ShopScreen] Remove card service initiated ({_removeCardCost} shards)");
        }

        private void OnPurifyClicked()
        {
            if (_shopManager == null) return;

            int shards = _shopManager.VoidShards;
            if (shards < _purifyCost)
            {
                Debug.Log("[ShopScreen] Not enough shards to purify");
                return;
            }

            _shopManager.SpendVoidShards(_purifyCost);
            EventBus.Publish(new ShopPurifyRequestedEvent(_purifyAmount));
            Debug.Log($"[ShopScreen] Purify service used ({_purifyCost} shards, -{_purifyAmount} corruption)");
        }

        // ============================================
        // Service Buttons
        // ============================================

        private void RefreshServiceButtons()
        {
            int shards = _shopManager?.VoidShards ?? 0;

            // Remove Card button
            if (_removeCardButton != null)
            {
                _removeCardButton.interactable = shards >= _removeCardCost;
            }
            if (_removeCardCostText != null)
            {
                _removeCardCostText.text = $"Remove Card ({_removeCardCost})";
                _removeCardCostText.color = shards >= _removeCardCost ? _affordableColor : _unaffordableColor;
            }

            // Purify button
            if (_purifyButton != null)
            {
                _purifyButton.interactable = shards >= _purifyCost;
            }
            if (_purifyCostText != null)
            {
                _purifyCostText.text = $"Purify -{_purifyAmount} ({_purifyCost})";
                _purifyCostText.color = shards >= _purifyCost ? _affordableColor : _unaffordableColor;
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
            RefreshServiceButtons();
        }
    }

    // ============================================
    // Supporting Events
    // ============================================

    /// <summary>
    /// Event fired when player requests to remove a card at the shop.
    /// </summary>
    public class ShopRemoveCardRequestedEvent : GameEvent
    {
    }

    /// <summary>
    /// Event fired when player purchases purify service at the shop.
    /// </summary>
    public class ShopPurifyRequestedEvent : GameEvent
    {
        public int PurifyAmount { get; }

        public ShopPurifyRequestedEvent(int purifyAmount)
        {
            PurifyAmount = purifyAmount;
        }
    }
}
