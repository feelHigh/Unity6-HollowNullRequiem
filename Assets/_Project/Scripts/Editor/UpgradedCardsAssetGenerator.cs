// ============================================
// UpgradedCardsAssetGenerator.cs
// Editor tool to generate upgraded card versions
// ============================================

using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using HNR.Cards;

namespace HNR.Editor
{
    /// <summary>
    /// Editor tool to generate upgraded versions of cards and link them.
    /// Called from EditorMenuOrganizer.
    /// </summary>
    public static class UpgradedCardsAssetGenerator
    {
        private static int _linkedCount;

        public static void GenerateUpgradedCards()
        {
            int createdCount = 0;
            _linkedCount = 0;

            // Generate Kira upgrades
            createdCount += GenerateKiraUpgrades();

            // Generate Mordren upgrades
            createdCount += GenerateMordrenUpgrades();

            // Generate Elara upgrades
            createdCount += GenerateElaraUpgrades();

            // Generate Thornwick upgrades
            createdCount += GenerateThornwickUpgrades();

            // Save all assets
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"[UpgradedCardsAssetGenerator] Created {createdCount} upgraded cards, linked {_linkedCount} base cards");
        }

        // ============================================
        // Kira Upgrades
        // ============================================

        private static int GenerateKiraUpgrades()
        {
            string basePath = "Assets/_Project/Data/Cards/Kira";
            int count = 0;

            // 1. Basic Strike → Strike+ (8 damage instead of 6)
            count += CreateUpgrade(basePath, "Kira_Basic_Strike", "Kira_Strike_Plus",
                "Strike+", "Deal [Damage] damage.",
                1, TargetType.SingleEnemy, SoulAspect.Flame,
                new[] { new CardEffectData(EffectType.Damage, 8) });

            // 2. Inferno Strike → Inferno Strike+ (18 damage, Burn 4)
            count += CreateUpgrade(basePath, "Kira_Inferno_Strike", "Kira_Inferno_Strike_Plus",
                "Inferno Strike+", "Deal [Damage] damage. Apply [ApplyBurn] Burn.",
                2, TargetType.SingleEnemy, SoulAspect.Flame,
                new[] { new CardEffectData(EffectType.Damage, 18), new CardEffectData(EffectType.ApplyBurn, 4, 3) });

            // 3. Ember Dance → Ember Dance+ (6×2 damage)
            count += CreateUpgrade(basePath, "Kira_Ember_Dance", "Kira_Ember_Dance_Plus",
                "Ember Dance+", "Deal [DamageMultiple] damage 2 times.",
                1, TargetType.SingleEnemy, SoulAspect.Flame,
                new[] { new CardEffectData(EffectType.DamageMultiple, 6, 2) });

            // 4. Kindle → Kindle+ (Burn 7, Draw 2)
            count += CreateUpgrade(basePath, "Kira_Kindle", "Kira_Kindle_Plus",
                "Kindle+", "Apply [ApplyBurn] Burn. Draw [DrawCards] cards.",
                0, TargetType.SingleEnemy, SoulAspect.Flame,
                new[] { new CardEffectData(EffectType.ApplyBurn, 7, 3), new CardEffectData(EffectType.DrawCards, 2) });

            // 5. Combustion → Combustion+ (1 cost instead of 2)
            count += CreateUpgrade(basePath, "Kira_Combustion", "Kira_Combustion_Plus",
                "Combustion+", "Trigger all Burn on all enemies immediately.",
                1, TargetType.AllEnemies, SoulAspect.Flame,
                new[] { new CardEffectData(EffectType.Custom, 0, 0, "TriggerAllBurn") });

            return count;
        }

        // ============================================
        // Mordren Upgrades
        // ============================================

        private static int GenerateMordrenUpgrades()
        {
            string basePath = "Assets/_Project/Data/Cards/Mordren";
            int count = 0;

            // 1. Basic Strike → Strike+ (8 damage)
            count += CreateUpgrade(basePath, "Mordren_Basic_Strike", "Mordren_Strike_Plus",
                "Strike+", "Deal [Damage] damage.",
                1, TargetType.SingleEnemy, SoulAspect.Shadow,
                new[] { new CardEffectData(EffectType.Damage, 8) });

            // 2. Drain Soul → Drain Soul+ (10 damage, Heal 7)
            count += CreateUpgrade(basePath, "Mordren_Drain_Soul", "Mordren_Drain_Soul_Plus",
                "Drain Soul+", "Deal [Damage] damage. Heal [Heal] HP.",
                2, TargetType.SingleEnemy, SoulAspect.Shadow,
                new[] { new CardEffectData(EffectType.Damage, 10), new CardEffectData(EffectType.Heal, 7) });

            // 3. Life Leech → Life Leech+ (6 damage, Heal 6)
            count += CreateUpgrade(basePath, "Mordren_Life_Leech", "Mordren_Life_Leech_Plus",
                "Life Leech+", "Deal [Damage] damage. Heal [Heal] HP.",
                1, TargetType.SingleEnemy, SoulAspect.Shadow,
                new[] { new CardEffectData(EffectType.Damage, 6), new CardEffectData(EffectType.Heal, 6) });

            // 4. Shadow Bind → Shadow Bind+ (Weakness 4)
            count += CreateUpgrade(basePath, "Mordren_Shadow_Bind", "Mordren_Shadow_Bind_Plus",
                "Shadow Bind+", "Apply [ApplyWeakness] Weakness.",
                1, TargetType.SingleEnemy, SoulAspect.Shadow,
                new[] { new CardEffectData(EffectType.ApplyWeakness, 4, 4) });

            // 5. Creeping Darkness → Creeping Darkness+ (1 cost instead of 2)
            count += CreateUpgrade(basePath, "Mordren_Creeping_Darkness", "Mordren_Creeping_Darkness_Plus",
                "Creeping Darkness+", "Apply [ApplyWeakness] Weakness. Apply [ApplyVulnerability] Vulnerable.",
                1, TargetType.SingleEnemy, SoulAspect.Shadow,
                new[] { new CardEffectData(EffectType.ApplyWeakness, 2, 2), new CardEffectData(EffectType.ApplyVulnerability, 2, 2) });

            return count;
        }

        // ============================================
        // Elara Upgrades
        // ============================================

        private static int GenerateElaraUpgrades()
        {
            string basePath = "Assets/_Project/Data/Cards/Elara";
            int count = 0;

            // 1. Basic Strike → Strike+ (8 damage)
            count += CreateUpgrade(basePath, "Elara_Basic_Strike", "Elara_Strike_Plus",
                "Strike+", "Deal [Damage] damage.",
                1, TargetType.SingleEnemy, SoulAspect.Light,
                new[] { new CardEffectData(EffectType.Damage, 8) });

            // 2. Radiant Heal → Radiant Heal+ (14 HP)
            count += CreateUpgrade(basePath, "Elara_Radiant_Heal", "Elara_Radiant_Heal_Plus",
                "Radiant Heal+", "Heal [Heal] HP.",
                1, TargetType.None, SoulAspect.Light,
                new[] { new CardEffectData(EffectType.Heal, 14) });

            // 3. Sanctuary → Sanctuary+ (Block 16, Heal 7)
            count += CreateUpgrade(basePath, "Elara_Sanctuary", "Elara_Sanctuary_Plus",
                "Sanctuary+", "Gain [Block] Block. Heal [Heal] HP.",
                2, TargetType.None, SoulAspect.Light,
                new[] { new CardEffectData(EffectType.Block, 16), new CardEffectData(EffectType.Heal, 7) });

            // 4. Blessing → Blessing+ (Draw 2, Heal 4)
            count += CreateUpgrade(basePath, "Elara_Blessing", "Elara_Blessing_Plus",
                "Blessing+", "Draw [DrawCards] cards. Heal [Heal] HP.",
                0, TargetType.None, SoulAspect.Light,
                new[] { new CardEffectData(EffectType.DrawCards, 2), new CardEffectData(EffectType.Heal, 4) });

            // 5. Cleansing Wave → Cleansing Wave+ (Heal 12 all)
            count += CreateUpgrade(basePath, "Elara_Cleansing_Wave", "Elara_Cleansing_Wave_Plus",
                "Cleansing Wave+", "Heal all allies for [Heal] HP. Remove all debuffs.",
                2, TargetType.AllAllies, SoulAspect.Light,
                new[] { new CardEffectData(EffectType.Heal, 12), new CardEffectData(EffectType.Custom, 0, 0, "RemoveDebuffs") });

            return count;
        }

        // ============================================
        // Thornwick Upgrades
        // ============================================

        private static int GenerateThornwickUpgrades()
        {
            string basePath = "Assets/_Project/Data/Cards/Thornwick";
            int count = 0;

            // 1. Basic Strike → Strike+ (8 damage)
            count += CreateUpgrade(basePath, "Thornwick_Basic_Strike", "Thornwick_Strike_Plus",
                "Strike+", "Deal [Damage] damage.",
                1, TargetType.SingleEnemy, SoulAspect.Nature,
                new[] { new CardEffectData(EffectType.Damage, 8) });

            // 2. Iron Bark → Iron Bark+ (20 Block)
            count += CreateUpgrade(basePath, "Thornwick_Iron_Bark", "Thornwick_Iron_Bark_Plus",
                "Iron Bark+", "Gain [Block] Block.",
                2, TargetType.None, SoulAspect.Nature,
                new[] { new CardEffectData(EffectType.Block, 20) });

            // 3. Thorny Embrace → Thorny Embrace+ (Block 11, Thorns 6)
            count += CreateUpgrade(basePath, "Thornwick_Thorny_Embrace", "Thornwick_Thorny_Embrace_Plus",
                "Thorny Embrace+", "Gain [Block] Block. Deal 6 damage back to attackers this turn.",
                1, TargetType.None, SoulAspect.Nature,
                new[] { new CardEffectData(EffectType.Block, 11), new CardEffectData(EffectType.Custom, 6, 1, "Thorns") });

            // 4. Regenerate → Regenerate+ (Heal 9, Regen 4)
            count += CreateUpgrade(basePath, "Thornwick_Regenerate", "Thornwick_Regenerate_Plus",
                "Regenerate+", "Heal [Heal] HP. Gain Regeneration 4.",
                1, TargetType.None, SoulAspect.Nature,
                new[] { new CardEffectData(EffectType.Heal, 9), new CardEffectData(EffectType.Custom, 4, 4, "Regeneration") });

            // 5. Entangle → Entangle+ (1 cost instead of 2)
            count += CreateUpgrade(basePath, "Thornwick_Entangle", "Thornwick_Entangle_Plus",
                "Entangle+", "Apply Stun for 2 turns. Apply Weakness for 2 turns.",
                1, TargetType.SingleEnemy, SoulAspect.Nature,
                new[] { new CardEffectData(EffectType.ApplyStun, 1, 2), new CardEffectData(EffectType.ApplyWeakness, 2, 2) });

            return count;
        }

        // ============================================
        // Upgrade Creation Helper
        // ============================================

        private static int CreateUpgrade(string basePath, string baseCardName, string upgradedCardName,
            string displayName, string description, int apCost, TargetType targetType, SoulAspect aspect,
            CardEffectData[] effects)
        {
            string baseAssetPath = $"{basePath}/{baseCardName}.asset";
            string upgradedAssetPath = $"{basePath}/{upgradedCardName}.asset";

            // Load base card
            var baseCard = AssetDatabase.LoadAssetAtPath<CardDataSO>(baseAssetPath);
            if (baseCard == null)
            {
                Debug.LogWarning($"[UpgradedCardsAssetGenerator] Base card not found: {baseAssetPath}");
                return 0;
            }

            // Get card type and rarity from base
            CardType cardType = baseCard.CardType;
            CardRarity rarity = baseCard.Rarity;

            // Check if upgraded version already exists
            var existingUpgrade = AssetDatabase.LoadAssetAtPath<CardDataSO>(upgradedAssetPath);
            CardDataSO upgradedCard;

            if (existingUpgrade != null)
            {
                Debug.Log($"[UpgradedCardsAssetGenerator] {upgradedCardName}.asset already exists, updating...");
                upgradedCard = existingUpgrade;
            }
            else
            {
                upgradedCard = ScriptableObject.CreateInstance<CardDataSO>();
                AssetDatabase.CreateAsset(upgradedCard, upgradedAssetPath);
            }

            // Update upgraded card
            var so = new SerializedObject(upgradedCard);

            // Identity
            so.FindProperty("_cardId").stringValue = upgradedCardName.ToLower();
            so.FindProperty("_cardName").stringValue = displayName;
            so.FindProperty("_description").stringValue = description;
            so.FindProperty("_flavorText").stringValue = baseCard.FlavorText;

            // Classification - same as base but no further upgrade
            so.FindProperty("_cardType").enumValueIndex = (int)cardType;
            so.FindProperty("_rarity").enumValueIndex = (int)rarity;
            so.FindProperty("_soulAspect").enumValueIndex = (int)aspect;
            so.FindProperty("_owner").objectReferenceValue = baseCard.Owner;
            so.FindProperty("_upgradedVersion").objectReferenceValue = null; // No further upgrades

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

            // Border color - slightly brighter for upgraded
            Color borderColor = cardType switch
            {
                CardType.Strike => new Color(1f, 0.3f, 0.2f),   // Brighter Red
                CardType.Guard => new Color(0.3f, 0.5f, 1f),    // Brighter Blue
                CardType.Skill => new Color(0.3f, 0.9f, 0.4f),  // Brighter Green
                CardType.Power => new Color(0.7f, 0.3f, 1f),    // Brighter Purple
                _ => Color.white
            };
            so.FindProperty("_borderColor").colorValue = borderColor;

            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(upgradedCard);

            // Link base card to upgraded version
            var baseSO = new SerializedObject(baseCard);
            baseSO.FindProperty("_upgradedVersion").objectReferenceValue = upgradedCard;
            baseSO.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(baseCard);
            _linkedCount++;

            Debug.Log($"[UpgradedCardsAssetGenerator] Created/Updated: {displayName}, linked from {baseCard.CardName}");
            return 1;
        }
    }
}
