// ============================================
// MordrenAssetGenerator.cs
// Editor tool to generate Mordren's character assets
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
    /// Editor tool to generate Mordren's RequiemDataSO, RequiemArtDataSO, and 10 cards.
    /// Called from EditorMenuOrganizer.
    /// </summary>
    public static class MordrenAssetGenerator
    {
        // Asset paths
        private const string RequiemDataPath = "Assets/_Project/Resources/Data/Characters/Requiems";
        private const string RequiemArtPath = "Assets/_Project/Resources/Data/Characters/Arts";
        private const string CardDataPath = "Assets/_Project/Resources/Data/Cards/Mordren";

        public static void GenerateMordrenAssets()
        {
            // Ensure directories exist
            EnsureDirectoryExists(RequiemDataPath);
            EnsureDirectoryExists(RequiemArtPath);
            EnsureDirectoryExists(CardDataPath);

            // Generate all assets
            var cards = GenerateMordrenCards();
            var requiemArt = GenerateRequiemArt();
            var requiemData = GenerateRequiemData(requiemArt, cards);

            // Wire owner references on cards
            WireCardOwnerReferences(cards, requiemData);

            // Save all assets
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("[MordrenAssetGenerator] Successfully generated Mordren assets:");
            Debug.Log($"  - RequiemDataSO: {RequiemDataPath}/Mordren_Data.asset");
            Debug.Log($"  - RequiemArtDataSO: {RequiemArtPath}/Mordren_SoulHarvest.asset");
            Debug.Log($"  - Cards: {cards.Count} cards in {CardDataPath}/");
        }

        // ============================================
        // Requiem Data Generation
        // ============================================

        private static RequiemDataSO GenerateRequiemData(RequiemArtDataSO art, List<CardDataSO> cards)
        {
            string assetPath = $"{RequiemDataPath}/Mordren_Data.asset";

            // Check if already exists
            var existing = AssetDatabase.LoadAssetAtPath<RequiemDataSO>(assetPath);
            if (existing != null)
            {
                Debug.Log("[MordrenAssetGenerator] Mordren_Data.asset already exists, updating...");
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
            so.FindProperty("_requiemId").stringValue = "mordren";
            so.FindProperty("_requiemName").stringValue = "Mordren";
            so.FindProperty("_title").stringValue = "Shadow Reaper";
            so.FindProperty("_backstory").stringValue = "A former necromancer who bound his soul to the Void, Mordren now walks between life and death. He drains the essence of his enemies to sustain his cursed existence.";

            // Classification
            so.FindProperty("_class").enumValueIndex = (int)RequiemClass.Controller;
            so.FindProperty("_soulAspect").enumValueIndex = (int)SoulAspect.Shadow;

            // Base Stats
            so.FindProperty("_baseHP").intValue = 60;
            so.FindProperty("_baseATK").intValue = 8;
            so.FindProperty("_baseDEF").intValue = 6;
            so.FindProperty("_seRate").floatValue = 2.0f;

            // Null State
            so.FindProperty("_nullStateEffect").enumValueIndex = (int)NullStateEffect.LifestealBoost;
            so.FindProperty("_nullStateDescription").stringValue = "Drain effects heal for double. Enemies take 50% of damage they deal to themselves.";

            // Requiem Art
            so.FindProperty("_requiemArt").objectReferenceValue = art;

            // Visuals - Shadow purple color
            so.FindProperty("_aspectColor").colorValue = new Color(0.4f, 0.1f, 0.6f, 1f); // Dark purple

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
            string assetPath = $"{RequiemArtPath}/Mordren_SoulHarvest.asset";

            // Check if already exists
            var existing = AssetDatabase.LoadAssetAtPath<RequiemArtDataSO>(assetPath);
            if (existing != null)
            {
                Debug.Log("[MordrenAssetGenerator] Mordren_SoulHarvest.asset already exists, updating...");
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
            so.FindProperty("_artName").stringValue = "Soul Harvest";
            so.FindProperty("_description").stringValue = "Steal 20 HP from each enemy. Apply 3 Weakness to all enemies.";
            so.FindProperty("_flavorText").stringValue = "\"Your souls belong to the Void now.\"";

            // Cost & Activation
            so.FindProperty("_seCost").intValue = 35;
            so.FindProperty("_oncePerCombat").boolValue = true;

            // Targeting
            so.FindProperty("_targetType").enumValueIndex = (int)TargetType.AllEnemies;

            // Effects: Damage 20 + Heal 20 (lifesteal), ApplyWeakness 3
            var effects = so.FindProperty("_effects");
            effects.ClearArray();

            // Effect 1: Damage 20 (the steal damage)
            effects.InsertArrayElementAtIndex(0);
            var damageEffect = effects.GetArrayElementAtIndex(0);
            damageEffect.FindPropertyRelative("_effectType").enumValueIndex = (int)EffectType.Damage;
            damageEffect.FindPropertyRelative("_value").intValue = 20;
            damageEffect.FindPropertyRelative("_duration").intValue = 0;
            damageEffect.FindPropertyRelative("_customData").stringValue = "Lifesteal";

            // Effect 2: Heal 20 (from the steal)
            effects.InsertArrayElementAtIndex(1);
            var healEffect = effects.GetArrayElementAtIndex(1);
            healEffect.FindPropertyRelative("_effectType").enumValueIndex = (int)EffectType.Heal;
            healEffect.FindPropertyRelative("_value").intValue = 20;
            healEffect.FindPropertyRelative("_duration").intValue = 0;
            healEffect.FindPropertyRelative("_customData").stringValue = "";

            // Effect 3: ApplyWeakness 3
            effects.InsertArrayElementAtIndex(2);
            var weaknessEffect = effects.GetArrayElementAtIndex(2);
            weaknessEffect.FindPropertyRelative("_effectType").enumValueIndex = (int)EffectType.ApplyWeakness;
            weaknessEffect.FindPropertyRelative("_value").intValue = 3;
            weaknessEffect.FindPropertyRelative("_duration").intValue = 3;
            weaknessEffect.FindPropertyRelative("_customData").stringValue = "";

            // Visuals
            so.FindProperty("_flashColor").colorValue = new Color(0.3f, 0f, 0.5f, 1f); // Dark purple flash
            so.FindProperty("_effectDuration").floatValue = 2f;

            so.ApplyModifiedPropertiesWithoutUndo();
        }

        // ============================================
        // Card Generation
        // ============================================

        private static List<CardDataSO> GenerateMordrenCards()
        {
            var cards = new List<CardDataSO>();

            // Card 1: Basic Strike
            cards.Add(CreateCard("mordren_basic_strike", "Basic Strike",
                "Deal [Damage] damage.",
                CardType.Strike, CardRarity.Common, 1, TargetType.SingleEnemy,
                new CardEffectData(EffectType.Damage, 6)));

            // Card 2: Basic Guard
            cards.Add(CreateCard("mordren_basic_guard", "Basic Guard",
                "Gain [Block] Block.",
                CardType.Guard, CardRarity.Common, 1, TargetType.None,
                new CardEffectData(EffectType.Block, 5)));

            // Card 3: Drain Soul
            cards.Add(CreateCard("mordren_drain_soul", "Drain Soul",
                "Deal [Damage] damage. Heal [Heal] HP.",
                CardType.Strike, CardRarity.Uncommon, 2, TargetType.SingleEnemy,
                new CardEffectData(EffectType.Damage, 8),
                new CardEffectData(EffectType.Heal, 5)));

            // Card 4: Shadow Bind
            cards.Add(CreateCard("mordren_shadow_bind", "Shadow Bind",
                "Apply [ApplyWeakness] Weakness.",
                CardType.Skill, CardRarity.Common, 1, TargetType.SingleEnemy,
                new CardEffectData(EffectType.ApplyWeakness, 3, 3)));

            // Card 5: Umbral Strike
            cards.Add(CreateCard("mordren_umbral_strike", "Umbral Strike",
                "Deal [Damage] damage. Draw [DrawCards] card.",
                CardType.Strike, CardRarity.Uncommon, 1, TargetType.SingleEnemy,
                new CardEffectData(EffectType.Damage, 6),
                new CardEffectData(EffectType.DrawCards, 1)));

            // Card 6: Life Leech
            cards.Add(CreateCard("mordren_life_leech", "Life Leech",
                "Deal [Damage] damage. Heal [Heal] HP.",
                CardType.Strike, CardRarity.Common, 1, TargetType.SingleEnemy,
                new CardEffectData(EffectType.Damage, 4),
                new CardEffectData(EffectType.Heal, 4)));

            // Card 7: Creeping Darkness
            cards.Add(CreateCard("mordren_creeping_darkness", "Creeping Darkness",
                "Apply [ApplyWeakness] Weakness. Apply [ApplyVulnerability] Vulnerable.",
                CardType.Skill, CardRarity.Rare, 2, TargetType.SingleEnemy,
                new CardEffectData(EffectType.ApplyWeakness, 2, 2),
                new CardEffectData(EffectType.ApplyVulnerability, 2, 2)));

            // Card 8: Shadow Clone
            cards.Add(CreateCard("mordren_shadow_clone", "Shadow Clone",
                "Copy the last Strike card played this combat.",
                CardType.Skill, CardRarity.Rare, 1, TargetType.None,
                new CardEffectData(EffectType.CopyCard, 1, 0, "LastStrike")));

            // Card 9: Siphon Power
            cards.Add(CreateCard("mordren_siphon_power", "Siphon Power",
                "Steal 3 Strength from an enemy.",
                CardType.Skill, CardRarity.Uncommon, 0, TargetType.SingleEnemy,
                new CardEffectData(EffectType.Custom, 3, 0, "StealStrength")));

            // Card 10: Veil of Night
            cards.Add(CreateCard("mordren_veil_of_night", "Veil of Night",
                "This combat: Enemies have 20% chance to miss attacks.",
                CardType.Power, CardRarity.Rare, 2, TargetType.None,
                new CardEffectData(EffectType.Custom, 20, 0, "EnemyMissChance")));

            return cards;
        }

        private static CardDataSO CreateCard(string cardId, string cardName, string description,
            CardType cardType, CardRarity rarity, int apCost, TargetType targetType,
            params CardEffectData[] effects)
        {
            string fileName = cardId.Replace("mordren_", "Mordren_");
            fileName = System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(fileName.Replace("_", " ")).Replace(" ", "_");
            string assetPath = $"{CardDataPath}/{fileName}.asset";

            // Check if already exists
            var existing = AssetDatabase.LoadAssetAtPath<CardDataSO>(assetPath);
            if (existing != null)
            {
                Debug.Log($"[MordrenAssetGenerator] {fileName}.asset already exists, updating...");
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
            so.FindProperty("_soulAspect").enumValueIndex = (int)SoulAspect.Shadow;
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
                "mordren_basic_strike" => "\"Even shadows can cut.\"",
                "mordren_basic_guard" => "\"Darkness shields me.\"",
                "mordren_drain_soul" => "\"Your life force sustains me.\"",
                "mordren_shadow_bind" => "\"You cannot escape the dark.\"",
                "mordren_umbral_strike" => "\"Strike from the shadows, learn their secrets.\"",
                "mordren_life_leech" => "\"A taste of your essence.\"",
                "mordren_creeping_darkness" => "\"The void consumes all strength.\"",
                "mordren_shadow_clone" => "\"Every shadow remembers.\"",
                "mordren_siphon_power" => "\"Your power becomes mine.\"",
                "mordren_veil_of_night" => "\"In darkness, none can find their mark.\"",
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
