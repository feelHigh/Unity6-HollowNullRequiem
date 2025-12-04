// ============================================
// KiraAssetGenerator.cs
// Editor tool to generate Kira's character assets
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
    /// Editor tool to generate Kira's RequiemDataSO, RequiemArtDataSO, and 10 cards.
    /// Menu: HNR/Generate Kira Assets
    /// </summary>
    public static class KiraAssetGenerator
    {
        // Asset paths
        private const string RequiemDataPath = "Assets/_Project/Data/Characters/Requiems";
        private const string RequiemArtPath = "Assets/_Project/Data/Characters/Arts";
        private const string CardDataPath = "Assets/_Project/Data/Cards/Kira";

        [MenuItem("HNR/Generate Kira Assets")]
        public static void GenerateKiraAssets()
        {
            // Ensure directories exist
            EnsureDirectoryExists(RequiemDataPath);
            EnsureDirectoryExists(RequiemArtPath);
            EnsureDirectoryExists(CardDataPath);

            // Generate all assets
            var cards = GenerateKiraCards();
            var requiemArt = GenerateRequiemArt();
            var requiemData = GenerateRequiemData(requiemArt, cards);

            // Wire owner references on cards
            WireCardOwnerReferences(cards, requiemData);

            // Save all assets
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("[KiraAssetGenerator] Successfully generated Kira assets:");
            Debug.Log($"  - RequiemDataSO: {RequiemDataPath}/Kira_Data.asset");
            Debug.Log($"  - RequiemArtDataSO: {RequiemArtPath}/Kira_InfernosWrath.asset");
            Debug.Log($"  - Cards: {cards.Count} cards in {CardDataPath}/");
        }

        // ============================================
        // Requiem Data Generation
        // ============================================

        private static RequiemDataSO GenerateRequiemData(RequiemArtDataSO art, List<CardDataSO> cards)
        {
            string assetPath = $"{RequiemDataPath}/Kira_Data.asset";

            // Check if already exists
            var existing = AssetDatabase.LoadAssetAtPath<RequiemDataSO>(assetPath);
            if (existing != null)
            {
                Debug.Log("[KiraAssetGenerator] Kira_Data.asset already exists, updating...");
                UpdateRequiemData(existing, art, cards);
                EditorUtility.SetDirty(existing);
                return existing;
            }

            // Create new asset
            var requiem = ScriptableObject.CreateInstance<RequiemDataSO>();

            // Use SerializedObject to set private fields
            AssetDatabase.CreateAsset(requiem, assetPath);

            UpdateRequiemData(requiem, art, cards);
            EditorUtility.SetDirty(requiem);

            return requiem;
        }

        private static void UpdateRequiemData(RequiemDataSO requiem, RequiemArtDataSO art, List<CardDataSO> cards)
        {
            var so = new SerializedObject(requiem);

            // Identity
            so.FindProperty("_requiemId").stringValue = "kira";
            so.FindProperty("_requiemName").stringValue = "Kira";
            so.FindProperty("_title").stringValue = "Ember Blade";
            so.FindProperty("_backstory").stringValue = "Once a soldier of the Hollow Legion, Kira now burns with a fire that consumes everything in its path. Her blade carries the fury of a thousand fallen comrades.";

            // Classification
            so.FindProperty("_class").enumValueIndex = (int)RequiemClass.Striker;
            so.FindProperty("_soulAspect").enumValueIndex = (int)SoulAspect.Flame;

            // Base Stats
            so.FindProperty("_baseHP").intValue = 70;
            so.FindProperty("_baseATK").intValue = 12;
            so.FindProperty("_baseDEF").intValue = 4;
            so.FindProperty("_seRate").floatValue = 1.5f;

            // Null State
            so.FindProperty("_nullStateEffect").enumValueIndex = (int)NullStateEffect.DamageBoost;
            so.FindProperty("_nullStateDescription").stringValue = "Burn effects deal double damage. Cards gain 'Apply 2 Burn' if they didn't already apply Burn.";

            // Requiem Art
            so.FindProperty("_requiemArt").objectReferenceValue = art;

            // Visuals - Flame color
            so.FindProperty("_aspectColor").colorValue = new Color(1f, 0.4f, 0.1f, 1f); // Orange-red flame

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
            string assetPath = $"{RequiemArtPath}/Kira_InfernosWrath.asset";

            // Check if already exists
            var existing = AssetDatabase.LoadAssetAtPath<RequiemArtDataSO>(assetPath);
            if (existing != null)
            {
                Debug.Log("[KiraAssetGenerator] Kira_InfernosWrath.asset already exists, updating...");
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
            so.FindProperty("_artName").stringValue = "Inferno's Wrath";
            so.FindProperty("_description").stringValue = "Deal 50 damage to all enemies. Apply 5 Burn to all.";
            so.FindProperty("_flavorText").stringValue = "\"Let them burn in the fires of my vengeance!\"";

            // Cost & Activation
            so.FindProperty("_seCost").intValue = 40;
            so.FindProperty("_oncePerCombat").boolValue = true;

            // Targeting
            so.FindProperty("_targetType").enumValueIndex = (int)TargetType.AllEnemies;

            // Effects: Damage 50, ApplyBurn 5
            var effects = so.FindProperty("_effects");
            effects.ClearArray();

            // Effect 1: Damage 50
            effects.InsertArrayElementAtIndex(0);
            var damageEffect = effects.GetArrayElementAtIndex(0);
            damageEffect.FindPropertyRelative("_effectType").enumValueIndex = (int)EffectType.Damage;
            damageEffect.FindPropertyRelative("_value").intValue = 50;
            damageEffect.FindPropertyRelative("_duration").intValue = 0;
            damageEffect.FindPropertyRelative("_customData").stringValue = "";

            // Effect 2: ApplyBurn 5
            effects.InsertArrayElementAtIndex(1);
            var burnEffect = effects.GetArrayElementAtIndex(1);
            burnEffect.FindPropertyRelative("_effectType").enumValueIndex = (int)EffectType.ApplyBurn;
            burnEffect.FindPropertyRelative("_value").intValue = 5;
            burnEffect.FindPropertyRelative("_duration").intValue = 3;
            burnEffect.FindPropertyRelative("_customData").stringValue = "";

            // Visuals
            so.FindProperty("_flashColor").colorValue = new Color(1f, 0.5f, 0f, 1f); // Orange flash
            so.FindProperty("_effectDuration").floatValue = 2f;

            so.ApplyModifiedPropertiesWithoutUndo();
        }

        // ============================================
        // Card Generation
        // ============================================

        private static List<CardDataSO> GenerateKiraCards()
        {
            var cards = new List<CardDataSO>();

            // Card 1: Basic Strike
            cards.Add(CreateCard("kira_basic_strike", "Basic Strike",
                "Deal [Damage] damage.",
                CardType.Strike, CardRarity.Common, 1, TargetType.SingleEnemy,
                new CardEffectData(EffectType.Damage, 6)));

            // Card 2: Basic Guard
            cards.Add(CreateCard("kira_basic_guard", "Basic Guard",
                "Gain [Block] Block.",
                CardType.Guard, CardRarity.Common, 1, TargetType.None,
                new CardEffectData(EffectType.Block, 5)));

            // Card 3: Inferno Strike
            cards.Add(CreateCard("kira_inferno_strike", "Inferno Strike",
                "Deal [Damage] damage. Apply [ApplyBurn] Burn.",
                CardType.Strike, CardRarity.Uncommon, 2, TargetType.SingleEnemy,
                new CardEffectData(EffectType.Damage, 15),
                new CardEffectData(EffectType.ApplyBurn, 3, 3)));

            // Card 4: Ember Dance
            cards.Add(CreateCard("kira_ember_dance", "Ember Dance",
                "Deal [DamageMultiple] damage 2 times.",
                CardType.Strike, CardRarity.Uncommon, 1, TargetType.SingleEnemy,
                new CardEffectData(EffectType.DamageMultiple, 5, 2)));

            // Card 5: Flame Shield
            cards.Add(CreateCard("kira_flame_shield", "Flame Shield",
                "Gain [Block] Block. Deal [Damage] damage.",
                CardType.Guard, CardRarity.Common, 1, TargetType.SingleEnemy,
                new CardEffectData(EffectType.Block, 6),
                new CardEffectData(EffectType.Damage, 3)));

            // Card 6: Kindle
            cards.Add(CreateCard("kira_kindle", "Kindle",
                "Apply [ApplyBurn] Burn. Draw [DrawCards] card.",
                CardType.Skill, CardRarity.Common, 0, TargetType.SingleEnemy,
                new CardEffectData(EffectType.ApplyBurn, 5, 3),
                new CardEffectData(EffectType.DrawCards, 1)));

            // Card 7: Combustion
            cards.Add(CreateCard("kira_combustion", "Combustion",
                "Trigger all Burn on all enemies immediately.",
                CardType.Skill, CardRarity.Rare, 2, TargetType.AllEnemies,
                new CardEffectData(EffectType.Custom, 0, 0, "TriggerAllBurn")));

            // Card 8: Fire Within
            cards.Add(CreateCard("kira_fire_within", "Fire Within",
                "This combat: All Flame cards deal +2 damage.",
                CardType.Power, CardRarity.Rare, 1, TargetType.None,
                new CardEffectData(EffectType.Custom, 2, 0, "FlameDamageBoost")));

            // Card 9: Blazing Rush
            cards.Add(CreateCard("kira_blazing_rush", "Blazing Rush",
                "Deal [Damage] damage. Gain [GainAP] AP.",
                CardType.Strike, CardRarity.Uncommon, 1, TargetType.SingleEnemy,
                new CardEffectData(EffectType.Damage, 4),
                new CardEffectData(EffectType.GainAP, 1)));

            // Card 10: Phoenix Feather
            cards.Add(CreateCard("kira_phoenix_feather", "Phoenix Feather",
                "If you would take lethal damage this turn, survive with 1 HP instead. Exhaust.",
                CardType.Skill, CardRarity.Rare, 2, TargetType.None,
                new CardEffectData(EffectType.Custom, 1, 1, "SurviveLethal"),
                new CardEffectData(EffectType.Exhaust, 0)));

            return cards;
        }

        private static CardDataSO CreateCard(string cardId, string cardName, string description,
            CardType cardType, CardRarity rarity, int apCost, TargetType targetType,
            params CardEffectData[] effects)
        {
            string fileName = cardId.Replace("kira_", "Kira_");
            fileName = System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(fileName.Replace("_", " ")).Replace(" ", "_");
            string assetPath = $"{CardDataPath}/{fileName}.asset";

            // Check if already exists
            var existing = AssetDatabase.LoadAssetAtPath<CardDataSO>(assetPath);
            if (existing != null)
            {
                Debug.Log($"[KiraAssetGenerator] {fileName}.asset already exists, updating...");
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
            so.FindProperty("_soulAspect").enumValueIndex = (int)SoulAspect.Flame;
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
                "kira_basic_strike" => "\"The first blow ignites the flame.\"",
                "kira_basic_guard" => "\"Even flames need shelter from the storm.\"",
                "kira_inferno_strike" => "\"Feel the heat of my fury!\"",
                "kira_ember_dance" => "\"Each step leaves fire in its wake.\"",
                "kira_flame_shield" => "\"Touch me and burn.\"",
                "kira_kindle" => "\"A spark is all it takes.\"",
                "kira_combustion" => "\"Let it all burn at once!\"",
                "kira_fire_within" => "\"The flames grow stronger with every breath.\"",
                "kira_blazing_rush" => "\"Speed fueled by fire.\"",
                "kira_phoenix_feather" => "\"From ashes, we rise.\"",
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
