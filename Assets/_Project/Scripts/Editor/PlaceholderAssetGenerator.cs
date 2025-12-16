// ============================================
// PlaceholderAssetGenerator.cs
// Editor tool to generate placeholder data assets
// ============================================

#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using HNR.Characters;
using HNR.Cards;
using HNR.Combat;

namespace HNR.Editor
{
    /// <summary>
    /// Editor utility to generate placeholder ScriptableObject assets.
    /// Called from EditorMenuOrganizer.
    /// </summary>
    public static class PlaceholderAssetGenerator
    {
        private const string DATA_PATH = "Assets/_Project/Resources/Data";
        private const string REQUIEMS_PATH = DATA_PATH + "/Characters/Requiems";
        private const string CARDS_PATH = DATA_PATH + "/Cards";
        private const string ENEMIES_PATH = DATA_PATH + "/Enemies";

        public static void GenerateAll()
        {
            EnsureFoldersExist();
            GenerateRequiems();
            GenerateCards();
            GenerateEnemies();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[PlaceholderAssetGenerator] All placeholder assets generated!");
        }

        public static void GenerateRequiemsOnly()
        {
            EnsureFoldersExist();
            GenerateRequiems();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        public static void GenerateCardsOnly()
        {
            EnsureFoldersExist();
            GenerateCards();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        public static void GenerateEnemiesOnly()
        {
            EnsureFoldersExist();
            GenerateEnemies();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private static void EnsureFoldersExist()
        {
            CreateFolderIfNeeded("Assets/_Project", "Resources");
            CreateFolderIfNeeded("Assets/_Project/Resources", "Data");
            CreateFolderIfNeeded(DATA_PATH, "Characters");
            CreateFolderIfNeeded(DATA_PATH + "/Characters", "Requiems");
            CreateFolderIfNeeded(DATA_PATH + "/Characters", "Arts");
            CreateFolderIfNeeded(DATA_PATH, "Cards");
            CreateFolderIfNeeded(DATA_PATH, "Enemies");
        }

        private static void CreateFolderIfNeeded(string parent, string folderName)
        {
            string fullPath = $"{parent}/{folderName}";
            if (!AssetDatabase.IsValidFolder(fullPath))
            {
                AssetDatabase.CreateFolder(parent, folderName);
            }
        }

        // ============================================
        // Requiem Generation
        // ============================================

        private static void GenerateRequiems()
        {
            CreateRequiem("Kira_Data", "kira", "Kira", "Ember Blade",
                RequiemClass.Striker, SoulAspect.Flame, 70, 12, 4, 1.5f,
                NullStateEffect.DamageBoost, "All Burn effects deal double damage.");

            CreateRequiem("Mordren_Data", "mordren", "Mordren", "Shadow Reaper",
                RequiemClass.Controller, SoulAspect.Shadow, 60, 8, 6, 2.0f,
                NullStateEffect.LifestealBoost, "HP drain effects heal for double.");

            CreateRequiem("Elara_Data", "elara", "Elara", "Light Bringer",
                RequiemClass.Support, SoulAspect.Light, 80, 6, 8, 1.0f,
                NullStateEffect.HealingDamage, "Healing effects also deal equal damage.");

            CreateRequiem("Thornwick_Data", "thornwick", "Thornwick", "Iron Root",
                RequiemClass.Tank, SoulAspect.Nature, 100, 8, 10, 1.0f,
                NullStateEffect.DefenseRegen, "Regenerate 5 HP at turn start. +50% Block.");

            Debug.Log("[PlaceholderAssetGenerator] Generated 4 Requiem assets");
        }

        private static void CreateRequiem(string fileName, string id, string displayName, string title,
            RequiemClass requiemClass, SoulAspect aspect, int hp, int atk, int def, float seRate,
            NullStateEffect nullEffect, string nullDescription)
        {
            string path = $"{REQUIEMS_PATH}/{fileName}.asset";

            // Check if already exists
            var existing = AssetDatabase.LoadAssetAtPath<RequiemDataSO>(path);
            if (existing != null)
            {
                Debug.Log($"[PlaceholderAssetGenerator] Requiem already exists: {fileName}");
                return;
            }

            var asset = ScriptableObject.CreateInstance<RequiemDataSO>();
            var so = new SerializedObject(asset);

            so.FindProperty("_requiemId").stringValue = id;
            so.FindProperty("_requiemName").stringValue = displayName;
            so.FindProperty("_title").stringValue = title;
            so.FindProperty("_backstory").stringValue = $"A fallen hero of the {aspect} aspect.";
            so.FindProperty("_class").enumValueIndex = (int)requiemClass;
            so.FindProperty("_soulAspect").enumValueIndex = (int)aspect;
            so.FindProperty("_baseHP").intValue = hp;
            so.FindProperty("_baseATK").intValue = atk;
            so.FindProperty("_baseDEF").intValue = def;
            so.FindProperty("_seRate").floatValue = seRate;
            so.FindProperty("_nullStateEffect").enumValueIndex = (int)nullEffect;
            so.FindProperty("_nullStateDescription").stringValue = nullDescription;

            so.ApplyModifiedPropertiesWithoutUndo();

            AssetDatabase.CreateAsset(asset, path);
            Debug.Log($"[PlaceholderAssetGenerator] Created Requiem: {fileName}");
        }

        // ============================================
        // Card Generation
        // ============================================

        private static void GenerateCards()
        {
            CreateCard("Strike_Basic", "strike_basic", "Strike",
                CardType.Strike, CardRarity.Common, SoulAspect.None, 1,
                TargetType.SingleEnemy, "Deal [Damage] damage.",
                EffectType.Damage, 6);

            CreateCard("Guard_Basic", "guard_basic", "Guard",
                CardType.Guard, CardRarity.Common, SoulAspect.None, 1,
                TargetType.Self, "Gain [Block] Block.",
                EffectType.Block, 5);

            CreateCard("Flame_Burst", "flame_burst", "Flame Burst",
                CardType.Strike, CardRarity.Uncommon, SoulAspect.Flame, 2,
                TargetType.SingleEnemy, "Deal [Damage] Flame damage.",
                EffectType.Damage, 12);

            CreateCard("Heal_Light", "heal_light", "Light's Embrace",
                CardType.Skill, CardRarity.Common, SoulAspect.Light, 1,
                TargetType.SingleAlly, "Heal [Heal] HP.",
                EffectType.Heal, 8);

            CreateCard("Draw_Arcane", "draw_arcane", "Arcane Insight",
                CardType.Skill, CardRarity.Uncommon, SoulAspect.Arcane, 0,
                TargetType.None, "Draw [DrawCards] cards.",
                EffectType.DrawCards, 2);

            Debug.Log("[PlaceholderAssetGenerator] Generated 5 Card assets");
        }

        private static void CreateCard(string fileName, string id, string displayName,
            CardType cardType, CardRarity rarity, SoulAspect aspect, int apCost,
            TargetType targetType, string description,
            EffectType effectType, int effectValue)
        {
            string path = $"{CARDS_PATH}/{fileName}.asset";

            var existing = AssetDatabase.LoadAssetAtPath<CardDataSO>(path);
            if (existing != null)
            {
                Debug.Log($"[PlaceholderAssetGenerator] Card already exists: {fileName}");
                return;
            }

            var asset = ScriptableObject.CreateInstance<CardDataSO>();
            var so = new SerializedObject(asset);

            so.FindProperty("_cardId").stringValue = id;
            so.FindProperty("_cardName").stringValue = displayName;
            so.FindProperty("_description").stringValue = description;
            so.FindProperty("_cardType").enumValueIndex = (int)cardType;
            so.FindProperty("_rarity").enumValueIndex = (int)rarity;
            so.FindProperty("_soulAspect").enumValueIndex = (int)aspect;
            so.FindProperty("_apCost").intValue = apCost;
            so.FindProperty("_targetType").enumValueIndex = (int)targetType;

            // Add effect
            var effectsProp = so.FindProperty("_effects");
            effectsProp.ClearArray();
            effectsProp.InsertArrayElementAtIndex(0);
            var effectProp = effectsProp.GetArrayElementAtIndex(0);
            effectProp.FindPropertyRelative("_effectType").enumValueIndex = (int)effectType;
            effectProp.FindPropertyRelative("_value").intValue = effectValue;
            effectProp.FindPropertyRelative("_duration").intValue = 0;
            effectProp.FindPropertyRelative("_customData").stringValue = "";

            so.ApplyModifiedPropertiesWithoutUndo();

            AssetDatabase.CreateAsset(asset, path);
            Debug.Log($"[PlaceholderAssetGenerator] Created Card: {fileName}");
        }

        // ============================================
        // Enemy Generation
        // ============================================

        private static void GenerateEnemies()
        {
            CreateEnemy("Shade_Lesser", "shade_lesser", "Lesser Shade",
                SoulAspect.Shadow, false, false, 30, 8, 0, 2);

            CreateEnemy("Flame_Wisp", "flame_wisp", "Flame Wisp",
                SoulAspect.Flame, false, false, 25, 10, 0, 3);

            CreateEnemy("Thorn_Beast", "thorn_beast", "Thorn Beast",
                SoulAspect.Nature, false, false, 40, 6, 8, 2);

            Debug.Log("[PlaceholderAssetGenerator] Generated 3 Enemy assets");
        }

        private static void CreateEnemy(string fileName, string id, string displayName,
            SoulAspect aspect, bool isElite, bool isBoss, int hp, int damage, int block, int corruption)
        {
            string path = $"{ENEMIES_PATH}/{fileName}.asset";

            var existing = AssetDatabase.LoadAssetAtPath<EnemyDataSO>(path);
            if (existing != null)
            {
                Debug.Log($"[PlaceholderAssetGenerator] Enemy already exists: {fileName}");
                return;
            }

            var asset = ScriptableObject.CreateInstance<EnemyDataSO>();
            var so = new SerializedObject(asset);

            so.FindProperty("_enemyId").stringValue = id;
            so.FindProperty("_enemyName").stringValue = displayName;
            so.FindProperty("_description").stringValue = $"A corrupted {aspect} creature.";
            so.FindProperty("_soulAspect").enumValueIndex = (int)aspect;
            so.FindProperty("_isElite").boolValue = isElite;
            so.FindProperty("_isBoss").boolValue = isBoss;
            so.FindProperty("_baseHP").intValue = hp;
            so.FindProperty("_baseDamage").intValue = damage;
            so.FindProperty("_baseBlock").intValue = block;
            so.FindProperty("_corruptionOnHit").intValue = corruption;
            so.FindProperty("_voidShardReward").intValue = hp / 3;

            so.ApplyModifiedPropertiesWithoutUndo();

            AssetDatabase.CreateAsset(asset, path);
            Debug.Log($"[PlaceholderAssetGenerator] Created Enemy: {fileName}");
        }
    }
}
#endif
