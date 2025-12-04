// ============================================
// ThornwickAssetGenerator.cs
// Editor tool to generate Thornwick's character assets
// ============================================

using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using HNR.Characters;
using HNR.Cards;

namespace HNR.Editor
{
    /// <summary>
    /// Editor tool to generate Thornwick's RequiemDataSO, RequiemArtDataSO, and 10 cards.
    /// Menu: HNR/Generate Thornwick Assets
    /// </summary>
    public static class ThornwickAssetGenerator
    {
        // Asset paths
        private const string RequiemDataPath = "Assets/_Project/Data/Characters/Requiems";
        private const string RequiemArtPath = "Assets/_Project/Data/Characters/Arts";
        private const string CardDataPath = "Assets/_Project/Data/Cards/Thornwick";

        [MenuItem("HNR/Generate Thornwick Assets")]
        public static void GenerateThornwickAssets()
        {
            // Ensure directories exist
            EnsureDirectoryExists(RequiemDataPath);
            EnsureDirectoryExists(RequiemArtPath);
            EnsureDirectoryExists(CardDataPath);

            // Generate all assets
            var cards = GenerateThornwickCards();
            var requiemArt = GenerateRequiemArt();
            var requiemData = GenerateRequiemData(requiemArt, cards);

            // Wire owner references on cards
            WireCardOwnerReferences(cards, requiemData);

            // Save all assets
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("[ThornwickAssetGenerator] Successfully generated Thornwick assets:");
            Debug.Log($"  - RequiemDataSO: {RequiemDataPath}/Thornwick_Data.asset");
            Debug.Log($"  - RequiemArtDataSO: {RequiemArtPath}/Thornwick_EarthenPrison.asset");
            Debug.Log($"  - Cards: {cards.Count} cards in {CardDataPath}/");
        }

        // ============================================
        // Requiem Data Generation
        // ============================================

        private static RequiemDataSO GenerateRequiemData(RequiemArtDataSO art, List<CardDataSO> cards)
        {
            string assetPath = $"{RequiemDataPath}/Thornwick_Data.asset";

            // Check if already exists
            var existing = AssetDatabase.LoadAssetAtPath<RequiemDataSO>(assetPath);
            if (existing != null)
            {
                Debug.Log("[ThornwickAssetGenerator] Thornwick_Data.asset already exists, updating...");
                UpdateRequiemData(existing, art, cards);
                EditorUtility.SetDirty(existing);
                return existing;
            }

            // Create new asset
            var requiem = ScriptableObject.CreateInstance<RequiemDataSO>();
            AssetDatabase.CreateAsset(requiem, assetPath);

            UpdateRequiemData(requiem, art, cards);
            EditorUtility.SetDirty(requiem);

            return requiem;
        }

        private static void UpdateRequiemData(RequiemDataSO requiem, RequiemArtDataSO art, List<CardDataSO> cards)
        {
            var so = new SerializedObject(requiem);

            // Identity
            so.FindProperty("_requiemId").stringValue = "thornwick";
            so.FindProperty("_requiemName").stringValue = "Thornwick";
            so.FindProperty("_title").stringValue = "Grove Warden";
            so.FindProperty("_backstory").stringValue = "An ancient treant spirit bound to the Void, Thornwick remembers when forests covered the world. Now he stands as an immovable guardian, his bark-like skin turning aside all harm.";

            // Classification
            so.FindProperty("_class").enumValueIndex = (int)RequiemClass.Tank;
            so.FindProperty("_soulAspect").enumValueIndex = (int)SoulAspect.Nature;

            // Base Stats
            so.FindProperty("_baseHP").intValue = 100;
            so.FindProperty("_baseATK").intValue = 8;
            so.FindProperty("_baseDEF").intValue = 10;
            so.FindProperty("_seRate").floatValue = 1.0f;

            // Null State
            so.FindProperty("_nullStateEffect").enumValueIndex = (int)NullStateEffect.DefenseRegen;
            so.FindProperty("_nullStateDescription").stringValue = "Regenerate 5 HP at turn start. All Block effects increased by 50%.";

            // Requiem Art
            so.FindProperty("_requiemArt").objectReferenceValue = art;

            // Visuals - Nature green color
            so.FindProperty("_aspectColor").colorValue = new Color(0.2f, 0.6f, 0.2f, 1f); // Forest green

            // Cards - Starting (1-6) and Unlockable (7-10)
            var startingCards = so.FindProperty("_startingCards");
            startingCards.ClearArray();
            for (int i = 0; i < 6 && i < cards.Count; i++)
            {
                startingCards.InsertArrayElementAtIndex(i);
                startingCards.GetArrayElementAtIndex(i).objectReferenceValue = cards[i];
            }

            var unlockableCards = so.FindProperty("_unlockableCards");
            unlockableCards.ClearArray();
            for (int i = 6; i < cards.Count; i++)
            {
                int idx = i - 6;
                unlockableCards.InsertArrayElementAtIndex(idx);
                unlockableCards.GetArrayElementAtIndex(idx).objectReferenceValue = cards[i];
            }

            so.ApplyModifiedPropertiesWithoutUndo();
        }

        // ============================================
        // Requiem Art Generation
        // ============================================

        private static RequiemArtDataSO GenerateRequiemArt()
        {
            string assetPath = $"{RequiemArtPath}/Thornwick_EarthenPrison.asset";

            // Check if already exists
            var existing = AssetDatabase.LoadAssetAtPath<RequiemArtDataSO>(assetPath);
            if (existing != null)
            {
                Debug.Log("[ThornwickAssetGenerator] Thornwick_EarthenPrison.asset already exists, updating...");
                UpdateRequiemArt(existing);
                EditorUtility.SetDirty(existing);
                return existing;
            }

            // Create new asset
            var art = ScriptableObject.CreateInstance<RequiemArtDataSO>();
            AssetDatabase.CreateAsset(art, assetPath);

            UpdateRequiemArt(art);
            EditorUtility.SetDirty(art);

            return art;
        }

        private static void UpdateRequiemArt(RequiemArtDataSO art)
        {
            var so = new SerializedObject(art);

            // Identity
            so.FindProperty("_artName").stringValue = "Earthen Prison";
            so.FindProperty("_description").stringValue = "Stun all enemies for 2 turns. Gain 30 Block.";
            so.FindProperty("_flavorText").stringValue = "\"The earth itself rises to bind you.\"";

            // Cost & Activation
            so.FindProperty("_seCost").intValue = 30;
            so.FindProperty("_oncePerCombat").boolValue = true;

            // Targeting
            so.FindProperty("_targetType").enumValueIndex = (int)TargetType.AllEnemies;

            // Effects: ApplyStun 2, Block 30
            var effects = so.FindProperty("_effects");
            effects.ClearArray();

            // Effect 1: ApplyStun 2 turns
            effects.InsertArrayElementAtIndex(0);
            var stunEffect = effects.GetArrayElementAtIndex(0);
            stunEffect.FindPropertyRelative("_effectType").enumValueIndex = (int)EffectType.ApplyStun;
            stunEffect.FindPropertyRelative("_value").intValue = 1;
            stunEffect.FindPropertyRelative("_duration").intValue = 2;
            stunEffect.FindPropertyRelative("_customData").stringValue = "";

            // Effect 2: Block 30
            effects.InsertArrayElementAtIndex(1);
            var blockEffect = effects.GetArrayElementAtIndex(1);
            blockEffect.FindPropertyRelative("_effectType").enumValueIndex = (int)EffectType.Block;
            blockEffect.FindPropertyRelative("_value").intValue = 30;
            blockEffect.FindPropertyRelative("_duration").intValue = 0;
            blockEffect.FindPropertyRelative("_customData").stringValue = "";

            // Visuals
            so.FindProperty("_flashColor").colorValue = new Color(0.4f, 0.3f, 0.1f, 1f); // Earthy brown flash
            so.FindProperty("_effectDuration").floatValue = 2f;

            so.ApplyModifiedPropertiesWithoutUndo();
        }

        // ============================================
        // Card Generation
        // ============================================

        private static List<CardDataSO> GenerateThornwickCards()
        {
            var cards = new List<CardDataSO>();

            // Card 1: Basic Strike
            cards.Add(CreateCard("thornwick_basic_strike", "Basic Strike",
                "Deal [Damage] damage.",
                CardType.Strike, CardRarity.Common, 1, TargetType.SingleEnemy,
                new CardEffectData(EffectType.Damage, 6)));

            // Card 2: Basic Guard
            cards.Add(CreateCard("thornwick_basic_guard", "Basic Guard",
                "Gain [Block] Block.",
                CardType.Guard, CardRarity.Common, 1, TargetType.None,
                new CardEffectData(EffectType.Block, 5)));

            // Card 3: Iron Bark
            cards.Add(CreateCard("thornwick_iron_bark", "Iron Bark",
                "Gain [Block] Block.",
                CardType.Guard, CardRarity.Common, 2, TargetType.None,
                new CardEffectData(EffectType.Block, 15)));

            // Card 4: Thorny Embrace
            cards.Add(CreateCard("thornwick_thorny_embrace", "Thorny Embrace",
                "Gain [Block] Block. Deal 4 damage back to attackers this turn.",
                CardType.Guard, CardRarity.Uncommon, 1, TargetType.None,
                new CardEffectData(EffectType.Block, 8),
                new CardEffectData(EffectType.Custom, 4, 1, "Thorns")));

            // Card 5: Root Strike
            cards.Add(CreateCard("thornwick_root_strike", "Root Strike",
                "Deal [Damage] damage. Apply Stun for 1 turn.",
                CardType.Strike, CardRarity.Uncommon, 1, TargetType.SingleEnemy,
                new CardEffectData(EffectType.Damage, 5),
                new CardEffectData(EffectType.ApplyStun, 1, 1)));

            // Card 6: Regenerate
            cards.Add(CreateCard("thornwick_regenerate", "Regenerate",
                "Heal [Heal] HP. Gain Regeneration 3.",
                CardType.Skill, CardRarity.Common, 1, TargetType.None,
                new CardEffectData(EffectType.Heal, 6),
                new CardEffectData(EffectType.Custom, 3, 3, "Regeneration")));

            // Card 7: Fortress
            cards.Add(CreateCard("thornwick_fortress", "Fortress",
                "This combat: All Guard cards grant +5 additional Block.",
                CardType.Power, CardRarity.Rare, 2, TargetType.None,
                new CardEffectData(EffectType.Custom, 5, 0, "GuardBlockBonus")));

            // Card 8: Nature's Wrath
            cards.Add(CreateCard("thornwick_natures_wrath", "Nature's Wrath",
                "Deal damage equal to your current Block.",
                CardType.Strike, CardRarity.Rare, 3, TargetType.SingleEnemy,
                new CardEffectData(EffectType.Custom, 0, 0, "DamageEqualsBlock")));

            // Card 9: Entangle
            cards.Add(CreateCard("thornwick_entangle", "Entangle",
                "Apply Stun for 2 turns. Apply Weakness for 2 turns.",
                CardType.Skill, CardRarity.Rare, 2, TargetType.SingleEnemy,
                new CardEffectData(EffectType.ApplyStun, 1, 2),
                new CardEffectData(EffectType.ApplyWeakness, 2, 2)));

            // Card 10: Ancient Grove
            cards.Add(CreateCard("thornwick_ancient_grove", "Ancient Grove",
                "This combat: Start each turn with 10 Block.",
                CardType.Power, CardRarity.Rare, 2, TargetType.None,
                new CardEffectData(EffectType.Custom, 10, 0, "StartTurnBlock")));

            return cards;
        }

        private static CardDataSO CreateCard(string cardId, string cardName, string description,
            CardType cardType, CardRarity rarity, int apCost, TargetType targetType,
            params CardEffectData[] effects)
        {
            string fileName = cardId.Replace("thornwick_", "Thornwick_");
            fileName = System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(fileName.Replace("_", " ")).Replace(" ", "_");
            string assetPath = $"{CardDataPath}/{fileName}.asset";

            // Check if already exists
            var existing = AssetDatabase.LoadAssetAtPath<CardDataSO>(assetPath);
            if (existing != null)
            {
                Debug.Log($"[ThornwickAssetGenerator] {fileName}.asset already exists, updating...");
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

            // Classification
            so.FindProperty("_cardType").enumValueIndex = (int)cardType;
            so.FindProperty("_rarity").enumValueIndex = (int)rarity;
            so.FindProperty("_soulAspect").enumValueIndex = (int)SoulAspect.Nature;
            // Owner will be set later

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

        private static void WireCardOwnerReferences(List<CardDataSO> cards, RequiemDataSO owner)
        {
            foreach (var card in cards)
            {
                var so = new SerializedObject(card);
                so.FindProperty("_owner").objectReferenceValue = owner;
                so.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(card);
            }
        }

        private static string GetFlavorText(string cardId)
        {
            return cardId switch
            {
                "thornwick_basic_strike" => "\"Even roots can strike.\"",
                "thornwick_basic_guard" => "\"Stand firm like the ancient oaks.\"",
                "thornwick_iron_bark" => "\"My bark is harder than steel.\"",
                "thornwick_thorny_embrace" => "\"Come closer... if you dare.\"",
                "thornwick_root_strike" => "\"The earth holds you fast.\"",
                "thornwick_regenerate" => "\"Life finds a way to return.\"",
                "thornwick_fortress" => "\"I am the wall that does not break.\"",
                "thornwick_natures_wrath" => "\"Feel the weight of the mountain.\"",
                "thornwick_entangle" => "\"The forest claims its prey.\"",
                "thornwick_ancient_grove" => "\"Where I stand, sanctuary grows.\"",
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
