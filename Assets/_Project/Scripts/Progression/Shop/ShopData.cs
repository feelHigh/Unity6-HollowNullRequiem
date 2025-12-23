// ============================================
// ShopData.cs
// Shop data structures for Void Market
// ============================================

using System;
using System.Collections.Generic;
using UnityEngine;
using HNR.Cards;

namespace HNR.Progression
{
    // ============================================
    // SHOP ENUMS
    // ============================================

    /// <summary>
    /// Types of items available in the Void Market shop.
    /// </summary>
    public enum ShopItemType
    {
        /// <summary>A card to add to the deck.</summary>
        Card,

        /// <summary>A relic with passive effects.</summary>
        Relic,

        /// <summary>A single-use consumable item.</summary>
        Consumable,

        /// <summary>Service to remove a card from the deck.</summary>
        CardRemove,

        /// <summary>Service to reduce corruption on a Requiem.</summary>
        Purification
    }

    // ============================================
    // SHOP ITEM
    // ============================================

    /// <summary>
    /// Represents a single purchasable item in the shop.
    /// </summary>
    [Serializable]
    public class ShopItem
    {
        // ============================================
        // Fields
        // ============================================

        [SerializeField, Tooltip("Type of shop item")]
        private ShopItemType _type;

        [SerializeField, Tooltip("Price in Void Shards")]
        private int _price;

        [SerializeField, Tooltip("Card data (for Card type)")]
        private CardDataSO _cardData;

        [SerializeField, Tooltip("Relic data (for Relic type)")]
        private RelicDataSO _relicData;

        [SerializeField, Tooltip("Whether this item has been purchased")]
        private bool _isPurchased;

        // ============================================
        // Properties
        // ============================================

        /// <summary>Type of this shop item.</summary>
        public ShopItemType Type
        {
            get => _type;
            set => _type = value;
        }

        /// <summary>Price in Void Shards.</summary>
        public int Price
        {
            get => _price;
            set => _price = value;
        }

        /// <summary>Card data for Card type items.</summary>
        public CardDataSO CardData
        {
            get => _cardData;
            set => _cardData = value;
        }

        /// <summary>Relic data for Relic type items.</summary>
        public RelicDataSO RelicData
        {
            get => _relicData;
            set => _relicData = value;
        }

        /// <summary>Whether this item has been purchased.</summary>
        public bool IsPurchased
        {
            get => _isPurchased;
            set => _isPurchased = value;
        }

        /// <summary>
        /// Display name based on item type.
        /// Returns the card/relic name or service description.
        /// </summary>
        public string DisplayName => _type switch
        {
            ShopItemType.Card => _cardData?.CardName ?? "Unknown Card",
            ShopItemType.Relic => _relicData?.RelicName ?? "Unknown Relic",
            ShopItemType.Consumable => "Consumable",
            ShopItemType.CardRemove => "Remove Card",
            ShopItemType.Purification => "Purification",
            _ => "Unknown Item"
        };

        /// <summary>
        /// Description text for the item.
        /// Returns card/relic description or service description.
        /// </summary>
        public string Description => _type switch
        {
            ShopItemType.Card => _cardData?.GetFormattedDescription() ?? "",
            ShopItemType.Relic => _relicData?.GetFormattedDescription() ?? "",
            ShopItemType.Consumable => "Single-use item",
            ShopItemType.CardRemove => "Remove a card from your deck permanently.",
            ShopItemType.Purification => "Reduce corruption on one Requiem by 25.",
            _ => ""
        };

        /// <summary>
        /// Icon sprite for the item.
        /// </summary>
        public Sprite Icon => _type switch
        {
            ShopItemType.Card => _cardData?.CardArt,
            ShopItemType.Relic => _relicData?.Icon,
            _ => null
        };

        /// <summary>
        /// Check if this item can be purchased.
        /// </summary>
        /// <param name="availableShards">Current Void Shard balance.</param>
        /// <returns>True if item can be purchased.</returns>
        public bool CanPurchase(int availableShards)
        {
            return !_isPurchased && availableShards >= _price;
        }
    }

    // ============================================
    // SHOP INVENTORY
    // ============================================

    /// <summary>
    /// Contains all items available in a shop instance.
    /// Generated when entering a Shop node on the map.
    /// </summary>
    [Serializable]
    public class ShopInventory
    {
        // ============================================
        // Fields
        // ============================================

        [SerializeField, Tooltip("All items in this shop")]
        private List<ShopItem> _items = new();

        // ============================================
        // Properties
        // ============================================

        /// <summary>All items in this shop.</summary>
        public List<ShopItem> Items
        {
            get => _items;
            set => _items = value;
        }

        /// <summary>Total number of items in the shop.</summary>
        public int ItemCount => _items.Count;

        /// <summary>Number of items not yet purchased.</summary>
        public int AvailableCount => _items.FindAll(i => !i.IsPurchased).Count;

        // ============================================
        // Methods
        // ============================================

        /// <summary>
        /// Get all items of a specific type.
        /// </summary>
        /// <param name="type">Item type to filter by.</param>
        /// <returns>List of matching items.</returns>
        public List<ShopItem> GetItemsByType(ShopItemType type)
        {
            return _items.FindAll(i => i.Type == type);
        }

        /// <summary>
        /// Get all available (not purchased) items.
        /// </summary>
        /// <returns>List of available items.</returns>
        public List<ShopItem> GetAvailableItems()
        {
            return _items.FindAll(i => !i.IsPurchased);
        }

        /// <summary>
        /// Check if shop has any available items.
        /// </summary>
        /// <returns>True if at least one item is available.</returns>
        public bool HasAvailableItems()
        {
            return AvailableCount > 0;
        }

        /// <summary>
        /// Add an item to the shop inventory.
        /// </summary>
        /// <param name="item">Item to add.</param>
        public void AddItem(ShopItem item)
        {
            _items.Add(item);
        }

        /// <summary>
        /// Clear all items from the inventory.
        /// </summary>
        public void Clear()
        {
            _items.Clear();
        }
    }
}
