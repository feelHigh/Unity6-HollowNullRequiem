// ============================================
// ElaraAssetGenerator.cs
// Editor tool to generate Elara's character assets
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
    /// Editor tool to generate Elara's RequiemDataSO, RequiemArtDataSO, and 10 cards.
    /// Menu: HNR/Generate Elara Assets
    /// </summary>
    public static class ElaraAssetGenerator
    {
        // Asset paths
        private const string RequiemDataPath = "Assets/_Project/Data/Characters/Requiems";
        private const string RequiemArtPath = "Assets/_Project/Data/Characters/Arts";
        private const string CardDataPath = "Assets/_Project/Data/Cards/Elara";

        [MenuItem("HNR/Generate Elara Assets")]
        public static void GenerateElaraAssets()
        {
            // Ensure directories exist
            EnsureDirectoryExists(RequiemDataPath);
            EnsureDirectoryExists(RequiemArtPath);
            EnsureDirectoryExists(CardDataPath);

            // Generate all assets
            var cards = GenerateElaraCards();
            var requiemArt = GenerateRequiemArt();
            var requiemData = GenerateRequiemData(requiemArt, cards);

            // Wire owner references on cards
            WireCardOwnerReferences(cards, requiemData);

            // Save all assets
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("[ElaraAssetGenerator] Successfully generated Elara assets:");
            Debug.Log($"  - RequiemDataSO: {RequiemDataPath}/Elara_Data.asset");
            Debug.Log($"  - RequiemArtDataSO: {RequiemArtPath}/Elara_DivineAegis.asset");
            Debug.Log($"  - Cards: {cards.Count} cards in {CardDataPath}/");
        }

        // ============================================
        // Requiem Data Generation
        // ============================================

        private static RequiemDataSO GenerateRequiemData(RequiemArtDataSO art, List<CardDataSO> cards)
        {
            string assetPath = $"{RequiemDataPath}/Elara_Data.asset";

            // Check if already exists
            var existing = AssetDatabase.LoadAssetAtPath<RequiemDataSO>(assetPath);
            if (existing != null)
            {
                Debug.Log("[ElaraAssetGenerator] Elara_Data.asset already exists, updating...");
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
            so.FindProperty("_requiemId").stringValue = "elara";
            so.FindProperty("_requiemName").stringValue = "Elara";
            so.FindProperty("_title").stringValue = "Lightkeeper";
            so.FindProperty("_backstory").stringValue = "Once a priestess of the Dawn Temple, Elara now channels divine light through the Void. Her healing powers mend both body and soul, though each miracle draws her closer to the darkness.";

            // Classification
            so.FindProperty("_class").enumValueIndex = (int)RequiemClass.Support;
            so.FindProperty("_soulAspect").enumValueIndex = (int)SoulAspect.Light;

            // Base Stats
            so.FindProperty("_baseHP").intValue = 80;
            so.FindProperty("_baseATK").intValue = 6;
            so.FindProperty("_baseDEF").intValue = 8;
            so.FindProperty("_seRate").floatValue = 1.0f;

            // Null State
            so.FindProperty("_nullStateEffect").enumValueIndex = (int)NullStateEffect.HealingDamage;
            so.FindProperty("_nullStateDescription").stringValue = "Healing effects also deal equal damage to a random enemy. Light damage ignores enemy Block.";

            // Requiem Art
            so.FindProperty("_requiemArt").objectReferenceValue = art;

            // Visuals - Light gold color
            so.FindProperty("_aspectColor").colorValue = new Color(1f, 0.9f, 0.4f, 1f); // Golden yellow

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
            string assetPath = $"{RequiemArtPath}/Elara_DivineAegis.asset";

            // Check if already exists
            var existing = AssetDatabase.LoadAssetAtPath<RequiemArtDataSO>(assetPath);
            if (existing != null)
            {
                Debug.Log("[ElaraAssetGenerator] Elara_DivineAegis.asset already exists, updating...");
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
            so.FindProperty("_artName").stringValue = "Divine Aegis";
            so.FindProperty("_description").stringValue = "Grant 100 Block to all allies. Heal all allies for 10 HP.";
            so.FindProperty("_flavorText").stringValue = "\"The light shall be your shield.\"";

            // Cost & Activation
            so.FindProperty("_seCost").intValue = 45;
            so.FindProperty("_oncePerCombat").boolValue = true;

            // Targeting
            so.FindProperty("_targetType").enumValueIndex = (int)TargetType.AllAllies;

            // Effects: Block 100, Heal 10
            var effects = so.FindProperty("_effects");
            effects.ClearArray();

            // Effect 1: Block 100
            effects.InsertArrayElementAtIndex(0);
            var blockEffect = effects.GetArrayElementAtIndex(0);
            blockEffect.FindPropertyRelative("_effectType").enumValueIndex = (int)EffectType.Block;
            blockEffect.FindPropertyRelative("_value").intValue = 100;
            blockEffect.FindPropertyRelative("_duration").intValue = 0;
            blockEffect.FindPropertyRelative("_customData").stringValue = "";

            // Effect 2: Heal 10
            effects.InsertArrayElementAtIndex(1);
            var healEffect = effects.GetArrayElementAtIndex(1);
            healEffect.FindPropertyRelative("_effectType").enumValueIndex = (int)EffectType.Heal;
            healEffect.FindPropertyRelative("_value").intValue = 10;
            healEffect.FindPropertyRelative("_duration").intValue = 0;
            healEffect.FindPropertyRelative("_customData").stringValue = "";

            // Visuals
            so.FindProperty("_flashColor").colorValue = new Color(1f, 1f, 0.8f, 1f); // Bright golden flash
            so.FindProperty("_effectDuration").floatValue = 2.5f;

            so.ApplyModifiedPropertiesWithoutUndo();
        }

        // ============================================
        // Card Generation
        // ============================================

        private static List<CardDataSO> GenerateElaraCards()
        {
            var cards = new List<CardDataSO>();

            // Card 1: Basic Strike
            cards.Add(CreateCard("elara_basic_strike", "Basic Strike",
                "Deal [Damage] damage.",
                CardType.Strike, CardRarity.Common, 1, TargetType.SingleEnemy,
                new CardEffectData(EffectType.Damage, 6)));

            // Card 2: Basic Guard
            cards.Add(CreateCard("elara_basic_guard", "Basic Guard",
                "Gain [Block] Block.",
                CardType.Guard, CardRarity.Common, 1, TargetType.None,
                new CardEffectData(EffectType.Block, 5)));

            // Card 3: Radiant Heal
            cards.Add(CreateCard("elara_radiant_heal", "Radiant Heal",
                "Heal [Heal] HP.",
                CardType.Skill, CardRarity.Common, 1, TargetType.None,
                new CardEffectData(EffectType.Heal, 10)));

            // Card 4: Purifying Light
            cards.Add(CreateCard("elara_purifying_light", "Purifying Light",
                "Reduce [CorruptionReduce] Corruption.",
                CardType.Skill, CardRarity.Uncommon, 2, TargetType.None,
                new CardEffectData(EffectType.CorruptionReduce, 15)));

            // Card 5: Sanctuary
            cards.Add(CreateCard("elara_sanctuary", "Sanctuary",
                "Gain [Block] Block. Heal [Heal] HP.",
                CardType.Guard, CardRarity.Uncommon, 2, TargetType.None,
                new CardEffectData(EffectType.Block, 12),
                new CardEffectData(EffectType.Heal, 5)));

            // Card 6: Blessing
            cards.Add(CreateCard("elara_blessing", "Blessing",
                "Draw [DrawCards] card. Heal [Heal] HP.",
                CardType.Skill, CardRarity.Common, 0, TargetType.None,
                new CardEffectData(EffectType.DrawCards, 1),
                new CardEffectData(EffectType.Heal, 3)));

            // Card 7: Divine Intervention
            cards.Add(CreateCard("elara_divine_intervention", "Divine Intervention",
                "Prevent death this turn. Heal to full HP. Exhaust.",
                CardType.Skill, CardRarity.Rare, 3, TargetType.None,
                new CardEffectData(EffectType.Custom, 1, 1, "PreventDeath"),
                new CardEffectData(EffectType.HealPercent, 100),
                new CardEffectData(EffectType.Exhaust, 0)));

            // Card 8: Cleansing Wave
            cards.Add(CreateCard("elara_cleansing_wave", "Cleansing Wave",
                "Heal all allies for [Heal] HP. Remove all debuffs.",
                CardType.Skill, CardRarity.Rare, 2, TargetType.AllAllies,
                new CardEffectData(EffectType.Heal, 8),
                new CardEffectData(EffectType.Custom, 0, 0, "RemoveDebuffs")));

            // Card 9: Light Barrier
            cards.Add(CreateCard("elara_light_barrier", "Light Barrier",
                "This combat: Gain 3 Block at the start of each turn.",
                CardType.Power, CardRarity.Rare, 1, TargetType.None,
                new CardEffectData(EffectType.Custom, 3, 0, "AutoBlockPerTurn")));

            // Card 10: Resurrection
            cards.Add(CreateCard("elara_resurrection", "Resurrection",
                "Revive a fallen teammate at 50% HP. Exhaust.",
                CardType.Skill, CardRarity.Rare, 3, TargetType.SingleAlly,
                new CardEffectData(EffectType.Custom, 50, 0, "Revive"),
                new CardEffectData(EffectType.Exhaust, 0)));

            return cards;
        }

        private static CardDataSO CreateCard(string cardId, string cardName, string description,
            CardType cardType, CardRarity rarity, int apCost, TargetType targetType,
            params CardEffectData[] effects)
        {
            string fileName = cardId.Replace("elara_", "Elara_");
            fileName = System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(fileName.Replace("_", " ")).Replace(" ", "_");
            string assetPath = $"{CardDataPath}/{fileName}.asset";

            // Check if already exists
            var existing = AssetDatabase.LoadAssetAtPath<CardDataSO>(assetPath);
            if (existing != null)
            {
                Debug.Log($"[ElaraAssetGenerator] {fileName}.asset already exists, updating...");
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
            so.FindProperty("_soulAspect").enumValueIndex = (int)SoulAspect.Light;
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
                "elara_basic_strike" => "\"Even light can pierce.\"",
                "elara_basic_guard" => "\"Stand behind the dawn.\"",
                "elara_radiant_heal" => "\"Let the light mend you.\"",
                "elara_purifying_light" => "\"Darkness cannot endure the dawn.\"",
                "elara_sanctuary" => "\"Within these walls, no harm shall find you.\"",
                "elara_blessing" => "\"May the light guide your path.\"",
                "elara_divine_intervention" => "\"Not yet. Your time has not come.\"",
                "elara_cleansing_wave" => "\"Be purified in holy radiance.\"",
                "elara_light_barrier" => "\"An eternal vigil of light.\"",
                "elara_resurrection" => "\"Rise again, for the dawn awaits.\"",
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
