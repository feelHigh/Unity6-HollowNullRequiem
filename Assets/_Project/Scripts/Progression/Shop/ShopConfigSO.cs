// ============================================
// ShopConfigSO.cs
// Configuration for Void Market shop generation
// ============================================

using UnityEngine;
using HNR.Cards;

namespace HNR.Progression
{
    /// <summary>
    /// Configuration asset for shop inventory generation.
    /// Controls item counts, price ranges, and service costs.
    /// </summary>
    [CreateAssetMenu(fileName = "ShopConfig", menuName = "HNR/Config/Shop Config")]
    public class ShopConfigSO : ScriptableObject
    {
        // ============================================
        // Inventory Counts
        // ============================================

        [Header("Inventory Counts")]
        [SerializeField, Tooltip("Minimum cards offered in shop")]
        private int _minCardCount = 3;

        [SerializeField, Tooltip("Maximum cards offered in shop")]
        private int _maxCardCount = 4;

        [SerializeField, Tooltip("Number of relics offered in shop")]
        private int _relicCount = 1;

        // ============================================
        // Service Prices
        // ============================================

        [Header("Service Prices")]
        [SerializeField, Tooltip("Cost to remove a card from deck")]
        private int _cardRemovePrice = 75;

        [SerializeField, Tooltip("Cost for purification service")]
        private int _purificationPrice = 50;

        [SerializeField, Tooltip("Corruption reduction from purification")]
        private int _purificationAmount = 15;

        // ============================================
        // Card Prices by Rarity
        // ============================================

        [Header("Card Prices by Rarity")]
        [SerializeField, Tooltip("Price range for Common cards (min, max)")]
        private Vector2Int _commonCardPrice = new(30, 50);

        [SerializeField, Tooltip("Price range for Uncommon cards (min, max)")]
        private Vector2Int _uncommonCardPrice = new(60, 90);

        [SerializeField, Tooltip("Price range for Rare cards (min, max)")]
        private Vector2Int _rareCardPrice = new(100, 150);

        [SerializeField, Tooltip("Price range for Legendary cards (min, max)")]
        private Vector2Int _legendaryCardPrice = new(200, 300);

        // ============================================
        // Relic Prices by Rarity
        // ============================================

        [Header("Relic Prices by Rarity")]
        [SerializeField, Tooltip("Price range for Common relics (min, max)")]
        private Vector2Int _commonRelicPrice = new(100, 150);

        [SerializeField, Tooltip("Price range for Uncommon relics (min, max)")]
        private Vector2Int _uncommonRelicPrice = new(150, 200);

        [SerializeField, Tooltip("Price range for Rare relics (min, max)")]
        private Vector2Int _rareRelicPrice = new(200, 300);

        // ============================================
        // Public Accessors - Inventory
        // ============================================

        /// <summary>Minimum number of cards offered in shop.</summary>
        public int MinCardCount => _minCardCount;

        /// <summary>Maximum number of cards offered in shop.</summary>
        public int MaxCardCount => _maxCardCount;

        /// <summary>Number of relics offered in shop.</summary>
        public int RelicCount => _relicCount;

        // ============================================
        // Public Accessors - Services
        // ============================================

        /// <summary>Cost in Void Shards to remove a card.</summary>
        public int CardRemovePrice => _cardRemovePrice;

        /// <summary>Cost in Void Shards for purification.</summary>
        public int PurificationPrice => _purificationPrice;

        /// <summary>Amount of corruption reduced by purification.</summary>
        public int PurificationAmount => _purificationAmount;

        // ============================================
        // Price Calculation Methods
        // ============================================

        /// <summary>
        /// Calculate a random price for a card based on its rarity.
        /// </summary>
        /// <param name="rarity">Card rarity tier.</param>
        /// <returns>Price in Void Shards.</returns>
        public int GetCardPrice(CardRarity rarity)
        {
            return rarity switch
            {
                CardRarity.Common => Random.Range(_commonCardPrice.x, _commonCardPrice.y + 1),
                CardRarity.Uncommon => Random.Range(_uncommonCardPrice.x, _uncommonCardPrice.y + 1),
                CardRarity.Rare => Random.Range(_rareCardPrice.x, _rareCardPrice.y + 1),
                CardRarity.Legendary => Random.Range(_legendaryCardPrice.x, _legendaryCardPrice.y + 1),
                _ => 50
            };
        }

        /// <summary>
        /// Calculate a random price for a relic based on its rarity.
        /// </summary>
        /// <param name="rarity">Relic rarity tier.</param>
        /// <returns>Price in Void Shards.</returns>
        public int GetRelicPrice(RelicRarity rarity)
        {
            return rarity switch
            {
                RelicRarity.Common => Random.Range(_commonRelicPrice.x, _commonRelicPrice.y + 1),
                RelicRarity.Uncommon => Random.Range(_uncommonRelicPrice.x, _uncommonRelicPrice.y + 1),
                RelicRarity.Rare => Random.Range(_rareRelicPrice.x, _rareRelicPrice.y + 1),
                _ => 100
            };
        }

        /// <summary>
        /// Get a random card count for shop generation.
        /// </summary>
        /// <returns>Number of cards to generate.</returns>
        public int GetRandomCardCount()
        {
            return Random.Range(_minCardCount, _maxCardCount + 1);
        }

        // ============================================
        // Price Range Accessors (for UI display)
        // ============================================

        /// <summary>Get price range for a card rarity.</summary>
        public Vector2Int GetCardPriceRange(CardRarity rarity)
        {
            return rarity switch
            {
                CardRarity.Common => _commonCardPrice,
                CardRarity.Uncommon => _uncommonCardPrice,
                CardRarity.Rare => _rareCardPrice,
                CardRarity.Legendary => _legendaryCardPrice,
                _ => new Vector2Int(50, 50)
            };
        }

        /// <summary>Get price range for a relic rarity.</summary>
        public Vector2Int GetRelicPriceRange(RelicRarity rarity)
        {
            return rarity switch
            {
                RelicRarity.Common => _commonRelicPrice,
                RelicRarity.Uncommon => _uncommonRelicPrice,
                RelicRarity.Rare => _rareRelicPrice,
                _ => new Vector2Int(100, 100)
            };
        }
    }
}
