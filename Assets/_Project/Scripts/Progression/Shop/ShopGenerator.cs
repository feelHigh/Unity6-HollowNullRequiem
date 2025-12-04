// ============================================
// ShopGenerator.cs
// Procedural shop inventory generation
// ============================================

using System.Collections.Generic;
using UnityEngine;
using HNR.Cards;

namespace HNR.Progression
{
    /// <summary>
    /// Generates shop inventories with weighted random selection.
    /// Zone number affects rarity distribution (higher zones = rarer items).
    /// </summary>
    public static class ShopGenerator
    {
        // ============================================
        // Constants
        // ============================================

        /// <summary>
        /// Base rarity weights: Common 60%, Uncommon 30%, Rare 9%, Legendary 1%.
        /// Index matches CardRarity enum order.
        /// </summary>
        private static readonly float[] BaseCardRarityWeights = { 0.60f, 0.30f, 0.09f, 0.01f };

        /// <summary>
        /// Base relic rarity weights: Common 50%, Uncommon 35%, Rare 15%.
        /// Boss relics are excluded from shops.
        /// </summary>
        private static readonly float[] BaseRelicRarityWeights = { 0.50f, 0.35f, 0.15f };

        /// <summary>
        /// Per-zone rarity boost. Each zone shifts weights toward rarer items.
        /// </summary>
        private const float ZoneRarityBoost = 0.05f;

        // ============================================
        // Public Methods
        // ============================================

        /// <summary>
        /// Generate a complete shop inventory for a zone.
        /// </summary>
        /// <param name="config">Shop configuration asset.</param>
        /// <param name="zoneNumber">Current zone (1-3), affects rarity weights.</param>
        /// <returns>Generated shop inventory.</returns>
        public static ShopInventory GenerateInventory(ShopConfigSO config, int zoneNumber)
        {
            var inventory = new ShopInventory();
            var rng = new System.Random();
            var usedCardIds = new HashSet<string>();

            // Generate cards (3-4 based on config)
            int cardCount = rng.Next(config.MinCardCount, config.MaxCardCount + 1);
            var cardPool = Resources.LoadAll<CardDataSO>("Data/Cards");

            for (int i = 0; i < cardCount; i++)
            {
                var card = SelectWeightedCard(cardPool, rng, zoneNumber, usedCardIds);
                if (card != null)
                {
                    usedCardIds.Add(card.CardId);
                    inventory.AddItem(new ShopItem
                    {
                        Type = ShopItemType.Card,
                        CardData = card,
                        Price = config.GetCardPrice(card.Rarity)
                    });
                }
            }

            // Generate relics
            var relicPool = Resources.LoadAll<RelicDataSO>("Data/Relics");
            for (int i = 0; i < config.RelicCount; i++)
            {
                var relic = SelectWeightedRelic(relicPool, rng, zoneNumber);
                if (relic != null)
                {
                    inventory.AddItem(new ShopItem
                    {
                        Type = ShopItemType.Relic,
                        RelicData = relic,
                        Price = config.GetRelicPrice(relic.Rarity)
                    });
                }
            }

            // Always add services
            inventory.AddItem(new ShopItem
            {
                Type = ShopItemType.CardRemove,
                Price = config.CardRemovePrice
            });

            inventory.AddItem(new ShopItem
            {
                Type = ShopItemType.Purification,
                Price = config.PurificationPrice
            });

            Debug.Log($"[ShopGenerator] Generated {inventory.ItemCount} items for Zone {zoneNumber}");
            return inventory;
        }

        /// <summary>
        /// Generate inventory with a specific seed for deterministic results.
        /// </summary>
        /// <param name="config">Shop configuration asset.</param>
        /// <param name="zoneNumber">Current zone number.</param>
        /// <param name="seed">Random seed for reproducibility.</param>
        /// <returns>Generated shop inventory.</returns>
        public static ShopInventory GenerateInventory(ShopConfigSO config, int zoneNumber, int seed)
        {
            var inventory = new ShopInventory();
            var rng = new System.Random(seed);
            var usedCardIds = new HashSet<string>();

            int cardCount = rng.Next(config.MinCardCount, config.MaxCardCount + 1);
            var cardPool = Resources.LoadAll<CardDataSO>("Data/Cards");

            for (int i = 0; i < cardCount; i++)
            {
                var card = SelectWeightedCard(cardPool, rng, zoneNumber, usedCardIds);
                if (card != null)
                {
                    usedCardIds.Add(card.CardId);
                    inventory.AddItem(new ShopItem
                    {
                        Type = ShopItemType.Card,
                        CardData = card,
                        Price = config.GetCardPrice(card.Rarity)
                    });
                }
            }

            var relicPool = Resources.LoadAll<RelicDataSO>("Data/Relics");
            for (int i = 0; i < config.RelicCount; i++)
            {
                var relic = SelectWeightedRelic(relicPool, rng, zoneNumber);
                if (relic != null)
                {
                    inventory.AddItem(new ShopItem
                    {
                        Type = ShopItemType.Relic,
                        RelicData = relic,
                        Price = config.GetRelicPrice(relic.Rarity)
                    });
                }
            }

            inventory.AddItem(new ShopItem
            {
                Type = ShopItemType.CardRemove,
                Price = config.CardRemovePrice
            });

            inventory.AddItem(new ShopItem
            {
                Type = ShopItemType.Purification,
                Price = config.PurificationPrice
            });

            Debug.Log($"[ShopGenerator] Generated {inventory.ItemCount} items for Zone {zoneNumber} (seed: {seed})");
            return inventory;
        }

        // ============================================
        // Private Methods - Card Selection
        // ============================================

        /// <summary>
        /// Select a weighted random card based on rarity and zone.
        /// </summary>
        private static CardDataSO SelectWeightedCard(
            CardDataSO[] pool,
            System.Random rng,
            int zoneNumber,
            HashSet<string> usedCardIds)
        {
            if (pool == null || pool.Length == 0)
            {
                Debug.LogWarning("[ShopGenerator] Card pool is empty");
                return null;
            }

            // Filter out already used cards
            var availableCards = new List<CardDataSO>();
            foreach (var card in pool)
            {
                if (card != null && !usedCardIds.Contains(card.CardId))
                {
                    availableCards.Add(card);
                }
            }

            if (availableCards.Count == 0)
            {
                Debug.LogWarning("[ShopGenerator] No available cards after filtering");
                return null;
            }

            // Get zone-adjusted weights
            float[] weights = GetZoneAdjustedCardWeights(zoneNumber);

            // Select rarity first
            CardRarity targetRarity = SelectWeightedRarity<CardRarity>(weights, rng);

            // Filter cards by rarity
            var matchingCards = availableCards.FindAll(c => c.Rarity == targetRarity);

            // If no cards of that rarity, try others
            if (matchingCards.Count == 0)
            {
                matchingCards = availableCards;
            }

            // Select random card from matching pool
            int index = rng.Next(matchingCards.Count);
            return matchingCards[index];
        }

        /// <summary>
        /// Get card rarity weights adjusted for zone number.
        /// Higher zones shift weights toward rarer cards.
        /// </summary>
        private static float[] GetZoneAdjustedCardWeights(int zoneNumber)
        {
            float[] weights = new float[BaseCardRarityWeights.Length];
            float zoneBonus = (zoneNumber - 1) * ZoneRarityBoost;

            // Shift weight from Common to higher rarities
            weights[0] = Mathf.Max(0.30f, BaseCardRarityWeights[0] - zoneBonus * 2); // Common
            weights[1] = BaseCardRarityWeights[1] + zoneBonus * 0.5f;                 // Uncommon
            weights[2] = BaseCardRarityWeights[2] + zoneBonus * 1.0f;                 // Rare
            weights[3] = BaseCardRarityWeights[3] + zoneBonus * 0.5f;                 // Legendary

            // Normalize weights
            NormalizeWeights(weights);
            return weights;
        }

        // ============================================
        // Private Methods - Relic Selection
        // ============================================

        /// <summary>
        /// Select a weighted random relic based on rarity.
        /// Boss relics are excluded.
        /// </summary>
        private static RelicDataSO SelectWeightedRelic(
            RelicDataSO[] pool,
            System.Random rng,
            int zoneNumber)
        {
            if (pool == null || pool.Length == 0)
            {
                Debug.LogWarning("[ShopGenerator] Relic pool is empty");
                return null;
            }

            // Filter out Boss relics
            var availableRelics = new List<RelicDataSO>();
            foreach (var relic in pool)
            {
                if (relic != null && relic.Rarity != RelicRarity.Boss)
                {
                    availableRelics.Add(relic);
                }
            }

            if (availableRelics.Count == 0)
            {
                Debug.LogWarning("[ShopGenerator] No available relics after filtering");
                return null;
            }

            // Get zone-adjusted weights
            float[] weights = GetZoneAdjustedRelicWeights(zoneNumber);

            // Select rarity first
            RelicRarity targetRarity = SelectWeightedRarity<RelicRarity>(weights, rng);

            // Filter relics by rarity
            var matchingRelics = availableRelics.FindAll(r => r.Rarity == targetRarity);

            // If no relics of that rarity, use all available
            if (matchingRelics.Count == 0)
            {
                matchingRelics = availableRelics;
            }

            // Select random relic from matching pool
            int index = rng.Next(matchingRelics.Count);
            return matchingRelics[index];
        }

        /// <summary>
        /// Get relic rarity weights adjusted for zone number.
        /// </summary>
        private static float[] GetZoneAdjustedRelicWeights(int zoneNumber)
        {
            float[] weights = new float[BaseRelicRarityWeights.Length];
            float zoneBonus = (zoneNumber - 1) * ZoneRarityBoost;

            weights[0] = Mathf.Max(0.25f, BaseRelicRarityWeights[0] - zoneBonus * 2); // Common
            weights[1] = BaseRelicRarityWeights[1] + zoneBonus;                        // Uncommon
            weights[2] = BaseRelicRarityWeights[2] + zoneBonus;                        // Rare

            NormalizeWeights(weights);
            return weights;
        }

        // ============================================
        // Private Methods - Utility
        // ============================================

        /// <summary>
        /// Select a rarity based on weighted random selection.
        /// </summary>
        private static T SelectWeightedRarity<T>(float[] weights, System.Random rng) where T : System.Enum
        {
            float roll = (float)rng.NextDouble();
            float cumulative = 0f;

            for (int i = 0; i < weights.Length; i++)
            {
                cumulative += weights[i];
                if (roll <= cumulative)
                {
                    return (T)(object)i;
                }
            }

            // Fallback to first (Common)
            return (T)(object)0;
        }

        /// <summary>
        /// Normalize weights to sum to 1.0.
        /// </summary>
        private static void NormalizeWeights(float[] weights)
        {
            float sum = 0f;
            foreach (float w in weights)
            {
                sum += w;
            }

            if (sum > 0f)
            {
                for (int i = 0; i < weights.Length; i++)
                {
                    weights[i] /= sum;
                }
            }
        }
    }
}
