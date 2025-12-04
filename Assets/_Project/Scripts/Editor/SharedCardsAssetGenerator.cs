// ============================================
// SharedCardsAssetGenerator.cs
// Editor tool to generate shared neutral cards
// ============================================

using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using HNR.Cards;

namespace HNR.Editor
{
    /// <summary>
    /// Editor tool to generate shared neutral cards (no owner).
    /// Menu: HNR/Generate Shared Cards
    /// </summary>
    public static class SharedCardsAssetGenerator
    {
        private const string CardDataPath = "Assets/_Project/Data/Cards/Shared";

        [MenuItem("HNR/Generate Shared Cards")]
        public static void GenerateSharedCards()
        {
            // Ensure directory exists
            EnsureDirectoryExists(CardDataPath);

            // Generate all cards
            var cards = CreateAllCards();

            // Save all assets
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("[SharedCardsAssetGenerator] Successfully generated shared cards:");
            Debug.Log($"  - Cards: {cards.Count} cards in {CardDataPath}/");
        }

        // ============================================
        // Card Generation
        // ============================================

        private static List<CardDataSO> CreateAllCards()
        {
            var cards = new List<CardDataSO>();

            // Card 1: Strike
            cards.Add(CreateCard("shared_strike", "Strike",
                "Deal [Damage] damage.",
                CardType.Strike, CardRarity.Common, 1, TargetType.SingleEnemy,
                new CardEffectData(EffectType.Damage, 6)));

            // Card 2: Defend
            cards.Add(CreateCard("shared_defend", "Defend",
                "Gain [Block] Block.",
                CardType.Guard, CardRarity.Common, 1, TargetType.None,
                new CardEffectData(EffectType.Block, 5)));

            // Card 3: Quick Draw
            cards.Add(CreateCard("shared_quick_draw", "Quick Draw",
                "Draw [DrawCards] cards.",
                CardType.Skill, CardRarity.Common, 1, TargetType.None,
                new CardEffectData(EffectType.DrawCards, 2)));

            // Card 4: Second Wind
            cards.Add(CreateCard("shared_second_wind", "Second Wind",
                "Heal [Heal] HP. Gain [GainAP] AP.",
                CardType.Skill, CardRarity.Uncommon, 0, TargetType.None,
                new CardEffectData(EffectType.Heal, 5),
                new CardEffectData(EffectType.GainAP, 1)));

            // Card 5: Void Siphon
            cards.Add(CreateCard("shared_void_siphon", "Void Siphon",
                "Gain [GainSE] Soul Essence.",
                CardType.Skill, CardRarity.Uncommon, 1, TargetType.None,
                new CardEffectData(EffectType.GainSE, 10)));

            // Card 6: Desperate Strike
            cards.Add(CreateCard("shared_desperate_strike", "Desperate Strike",
                "Deal [Damage] damage. Gain [CorruptionGain] Corruption.",
                CardType.Strike, CardRarity.Common, 0, TargetType.SingleEnemy,
                new CardEffectData(EffectType.Damage, 3),
                new CardEffectData(EffectType.CorruptionGain, 5)));

            // Card 7: Null Touch
            cards.Add(CreateCard("shared_null_touch", "Null Touch",
                "Gain [CorruptionGain] Corruption. Draw [DrawCards] cards.",
                CardType.Skill, CardRarity.Rare, 2, TargetType.None,
                new CardEffectData(EffectType.CorruptionGain, 20),
                new CardEffectData(EffectType.DrawCards, 2)));

            // Card 8: Sacrifice
            cards.Add(CreateCard("shared_sacrifice", "Sacrifice",
                "Take 10 damage. Gain [GainAP] AP.",
                CardType.Skill, CardRarity.Rare, 1, TargetType.None,
                new CardEffectData(EffectType.Custom, 10, 0, "SelfDamage"),
                new CardEffectData(EffectType.GainAP, 2)));

            return cards;
        }

        private static CardDataSO CreateCard(string cardId, string cardName, string description,
            CardType cardType, CardRarity rarity, int apCost, TargetType targetType,
            params CardEffectData[] effects)
        {
            string fileName = cardId.Replace("shared_", "Shared_");
            fileName = System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(fileName.Replace("_", " ")).Replace(" ", "_");
            string assetPath = $"{CardDataPath}/{fileName}.asset";

            // Check if already exists
            var existing = AssetDatabase.LoadAssetAtPath<CardDataSO>(assetPath);
            if (existing != null)
            {
                Debug.Log($"[SharedCardsAssetGenerator] {fileName}.asset already exists, updating...");
                UpdateCard(existing, cardId, cardName, description, cardType, rarity, apCost, targetType, effects);
                EditorUtility.SetDirty(existing);
                return existing;
            }

            // Create new asset
            var card = ScriptableObject.CreateInstance<CardDataSO>();
            AssetDatabase.CreateAsset(card, assetPath);

            UpdateCard(card, cardId, cardName, description, cardType, rarity, apCost, targetType, effects);
            EditorUtility.SetDirty(card);

            return card;
        }

        private static void UpdateCard(CardDataSO card, string cardId, string cardName, string description,
            CardType cardType, CardRarity rarity, int apCost, TargetType targetType,
            CardEffectData[] effects)
        {
            var so = new SerializedObject(card);

            // Identity
            so.FindProperty("_cardId").stringValue = cardId;
            so.FindProperty("_cardName").stringValue = cardName;
            so.FindProperty("_description").stringValue = description;
            so.FindProperty("_flavorText").stringValue = GetFlavorText(cardId);

            // Classification - No owner (neutral), No aspect
            so.FindProperty("_cardType").enumValueIndex = (int)cardType;
            so.FindProperty("_rarity").enumValueIndex = (int)rarity;
            so.FindProperty("_soulAspect").enumValueIndex = (int)SoulAspect.None;
            so.FindProperty("_owner").objectReferenceValue = null; // Neutral card

            // Cost
            so.FindProperty("_apCost").intValue = apCost;

            // Targeting
            so.FindProperty("_targetType").enumValueIndex = (int)targetType;
            so.FindProperty("_targetCount").intValue = 1;

            // Effects
            var effectsProp = so.FindProperty("_effects");
            effectsProp.ClearArray();
            for (int i = 0; i < effects.Length; i++)
            {
                effectsProp.InsertArrayElementAtIndex(i);
                var effectProp = effectsProp.GetArrayElementAtIndex(i);
                effectProp.FindPropertyRelative("_effectType").enumValueIndex = (int)effects[i].EffectType;
                effectProp.FindPropertyRelative("_value").intValue = effects[i].Value;
                effectProp.FindPropertyRelative("_duration").intValue = effects[i].Duration;
                effectProp.FindPropertyRelative("_customData").stringValue = effects[i].CustomData;
            }

            // Border color based on card type
            Color borderColor = cardType switch
            {
                CardType.Strike => new Color(0.8f, 0.2f, 0.1f), // Red
                CardType.Guard => new Color(0.2f, 0.4f, 0.8f),  // Blue
                CardType.Skill => new Color(0.2f, 0.7f, 0.3f),  // Green
                CardType.Power => new Color(0.6f, 0.2f, 0.8f),  // Purple
                _ => Color.white
            };
            so.FindProperty("_borderColor").colorValue = borderColor;

            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static string GetFlavorText(string cardId)
        {
            return cardId switch
            {
                "shared_strike" => "\"A basic attack, but effective.\"",
                "shared_defend" => "\"Brace yourself.\"",
                "shared_quick_draw" => "\"Fortune favors the swift.\"",
                "shared_second_wind" => "\"Catch your breath, then strike.\"",
                "shared_void_siphon" => "\"Draw power from the Void itself.\"",
                "shared_desperate_strike" => "\"Desperation breeds dangerous power.\"",
                "shared_null_touch" => "\"Embrace the darkness within.\"",
                "shared_sacrifice" => "\"Power demands a price.\"",
                _ => ""
            };
        }

        // ============================================
        // Utility
        // ============================================

        private static void EnsureDirectoryExists(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
                AssetDatabase.Refresh();
            }
        }
    }
}
