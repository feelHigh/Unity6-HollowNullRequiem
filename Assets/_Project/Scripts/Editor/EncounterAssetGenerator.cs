// ============================================
// EncounterAssetGenerator.cs
// Editor tool to generate Enemy and Encounter assets
// ============================================

#if UNITY_EDITOR
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEditor;
using HNR.Combat;
using HNR.Cards;

namespace HNR.Editor
{
    /// <summary>
    /// Generates EnemyDataSO and EncounterDataSO assets for all zones.
    /// Called from EditorMenuOrganizer.
    /// </summary>
    public static class EncounterAssetGenerator
    {
        private const string ENEMY_PATH = "Assets/_Project/Data/Enemies";
        private const string ENCOUNTER_PATH = "Assets/_Project/Data/Encounters";

        // ============================================
        // Main Generation
        // ============================================

        public static void GenerateAllAssets()
        {
            EnsureDirectoriesExist();

            // Generate enemies first
            var enemies = GenerateAllEnemies();

            // Then generate encounters using those enemies
            GenerateAllEncounters(enemies);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("[EncounterAssetGenerator] All encounter assets generated successfully!");
            Debug.Log($"  - {enemies.Count} enemies");
            Debug.Log($"  - Zone 1: 3 Easy, 2 Normal encounters");
            Debug.Log($"  - Zone 2: 2 Normal, 2 Hard, 1 Elite encounters");
            Debug.Log($"  - Zone 3: 2 Hard, 1 Elite, 1 Boss encounters");
        }

        // ============================================
        // Enemy Generation
        // ============================================

        public static Dictionary<string, EnemyDataSO> GenerateAllEnemies()
        {
            EnsureDirectoriesExist();
            var enemies = new Dictionary<string, EnemyDataSO>();

            // Zone 1 - Basic enemies (HP: 18-30, Damage: 6-8)
            enemies["hollow_thrall"] = CreateEnemy("Hollow Thrall", "hollow_thrall",
                "A shambling husk of a former soul, mindlessly aggressive.",
                SoulAspect.None, 22, 6, 4, 2, 8, "Zone1");

            enemies["corruption_sprite"] = CreateEnemy("Corruption Sprite", "corruption_sprite",
                "A floating manifestation of pure corruption.",
                SoulAspect.Shadow, 18, 5, 3, 4, 10, "Zone1");

            enemies["void_hound"] = CreateEnemy("Void Hound", "void_hound",
                "A twisted beast that hunts in the Null Rift.",
                SoulAspect.None, 28, 8, 5, 2, 12, "Zone1");

            // Zone 2 - Moderate enemies (HP: 35-50, Damage: 10-14)
            enemies["null_cultist"] = CreateEnemy("Null Cultist", "null_cultist",
                "A devoted follower of the void, wielding dark magic.",
                SoulAspect.Shadow, 38, 10, 6, 4, 15, "Zone2");

            enemies["flame_wraith"] = CreateEnemy("Flame Wraith", "flame_wraith",
                "A spirit consumed by eternal flame.",
                SoulAspect.Flame, 32, 12, 4, 3, 18, "Zone2");

            enemies["frost_sentinel"] = CreateEnemy("Frost Sentinel", "frost_sentinel",
                "An ancient guardian frozen in time.",
                SoulAspect.Arcane, 45, 9, 10, 2, 16, "Zone2");

            // Zone 3 - Advanced enemies (HP: 50-70, Damage: 14-18)
            enemies["null_knight"] = CreateEnemy("Null Knight", "null_knight",
                "An elite warrior corrupted by the void.",
                SoulAspect.Shadow, 55, 14, 12, 5, 22, "Zone3");

            enemies["void_mage"] = CreateEnemy("Void Mage", "void_mage",
                "A powerful sorcerer channeling void energy.",
                SoulAspect.Arcane, 48, 16, 6, 6, 25, "Zone3");

            // Elite enemies (HP: 80-100)
            enemies["hollow_berserker"] = CreateEnemy("Hollow Berserker", "hollow_berserker",
                "A rage-fueled monstrosity that grows stronger with each hit.",
                SoulAspect.Flame, 85, 18, 8, 5, 40, "Elite", true);

            enemies["null_weaver"] = CreateEnemy("Null Weaver", "null_weaver",
                "A master of corruption that turns strength into weakness.",
                SoulAspect.Shadow, 75, 12, 15, 8, 50, "Elite", true);

            // Boss (HP: 250+)
            enemies["malchor"] = CreateEnemy("Malchor, the Hollowed Saint", "malchor",
                "Once a revered protector, now a vessel of pure corruption.",
                SoulAspect.Shadow, 280, 22, 20, 10, 100, "Boss", false, true);

            return enemies;
        }

        private static EnemyDataSO CreateEnemy(string name, string id, string description,
            SoulAspect aspect, int hp, int damage, int block, int corruptionOnHit, int reward,
            string folder, bool isElite = false, bool isBoss = false)
        {
            var enemy = ScriptableObject.CreateInstance<EnemyDataSO>();

            SetField(enemy, "_enemyId", id);
            SetField(enemy, "_enemyName", name);
            SetField(enemy, "_description", description);
            SetField(enemy, "_soulAspect", aspect);
            SetField(enemy, "_baseHP", hp);
            SetField(enemy, "_baseDamage", damage);
            SetField(enemy, "_baseBlock", block);
            SetField(enemy, "_corruptionOnHit", corruptionOnHit);
            SetField(enemy, "_voidShardReward", reward);
            SetField(enemy, "_isElite", isElite);
            SetField(enemy, "_isBoss", isBoss);
            SetField(enemy, "_cardDropChance", isElite ? 0.5f : (isBoss ? 1f : 0.3f));

            string path = $"{ENEMY_PATH}/{folder}/{name.Replace(" ", "_").Replace(",", "")}.asset";
            EnsureSubfolderExists($"{ENEMY_PATH}/{folder}");
            SaveAsset(enemy, path);

            return enemy;
        }

        // ============================================
        // Encounter Generation
        // ============================================

        public static void GenerateEncountersMenuItem()
        {
            GenerateAllEncounters(null);
        }

        public static void GenerateAllEncounters(Dictionary<string, EnemyDataSO> enemies)
        {
            EnsureDirectoriesExist();

            // Load enemies if not provided
            if (enemies == null || enemies.Count == 0)
            {
                enemies = LoadAllEnemies();
            }

            // Zone 1 Encounters
            CreateEncounter("Zone1_Easy_1", "Hollow Patrol", 1, EncounterDifficulty.Easy,
                new[] { enemies.GetValueOrDefault("hollow_thrall") }, 1, 2, 0.8f);

            CreateEncounter("Zone1_Easy_2", "Sprite Swarm", 1, EncounterDifficulty.Easy,
                new[] { enemies.GetValueOrDefault("corruption_sprite") }, 2, 3, 0.9f);

            CreateEncounter("Zone1_Easy_3", "Lone Hound", 1, EncounterDifficulty.Easy,
                new[] { enemies.GetValueOrDefault("void_hound") }, 1, 1, 0.85f);

            CreateEncounter("Zone1_Normal_1", "Mixed Patrol", 1, EncounterDifficulty.Normal,
                new[] { enemies.GetValueOrDefault("hollow_thrall"), enemies.GetValueOrDefault("corruption_sprite") }, 2, 2, 1.0f);

            CreateEncounter("Zone1_Normal_2", "Hound Pack", 1, EncounterDifficulty.Normal,
                new[] { enemies.GetValueOrDefault("void_hound"), enemies.GetValueOrDefault("hollow_thrall") }, 2, 3, 1.1f);

            // Zone 2 Encounters
            CreateEncounter("Zone2_Normal_1", "Cultist Gathering", 2, EncounterDifficulty.Normal,
                new[] { enemies.GetValueOrDefault("null_cultist") }, 1, 2, 1.0f);

            CreateEncounter("Zone2_Normal_2", "Elemental Clash", 2, EncounterDifficulty.Normal,
                new[] { enemies.GetValueOrDefault("flame_wraith"), enemies.GetValueOrDefault("frost_sentinel") }, 1, 2, 1.1f);

            CreateEncounter("Zone2_Hard_1", "Sentinel Guard", 2, EncounterDifficulty.Hard,
                new[] { enemies.GetValueOrDefault("frost_sentinel"), enemies.GetValueOrDefault("null_cultist") }, 2, 2, 1.3f);

            CreateEncounter("Zone2_Hard_2", "Flame Assault", 2, EncounterDifficulty.Hard,
                new[] { enemies.GetValueOrDefault("flame_wraith") }, 2, 3, 1.25f);

            CreateEncounter("Zone2_Elite_1", "The Berserker", 2, EncounterDifficulty.Elite,
                new[] { enemies.GetValueOrDefault("hollow_berserker") }, 1, 1, 1.5f, true);

            // Zone 3 Encounters
            CreateEncounter("Zone3_Hard_1", "Knight's Challenge", 3, EncounterDifficulty.Hard,
                new[] { enemies.GetValueOrDefault("null_knight") }, 1, 2, 1.3f);

            CreateEncounter("Zone3_Hard_2", "Mage Conclave", 3, EncounterDifficulty.Hard,
                new[] { enemies.GetValueOrDefault("void_mage"), enemies.GetValueOrDefault("null_cultist") }, 2, 2, 1.4f);

            CreateEncounter("Zone3_Elite_1", "The Weaver", 3, EncounterDifficulty.Elite,
                new[] { enemies.GetValueOrDefault("null_weaver") }, 1, 1, 1.6f, true);

            CreateEncounter("Zone3_Boss", "Malchor's Chamber", 3, EncounterDifficulty.Boss,
                new[] { enemies.GetValueOrDefault("malchor") }, 1, 1, 2.0f, false, true, 2);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private static void CreateEncounter(string id, string name, int zone, EncounterDifficulty difficulty,
            EnemyDataSO[] enemyPool, int minEnemies, int maxEnemies, float rewardMult,
            bool isElite = false, bool isBoss = false, int arenaCorruption = 0)
        {
            var encounter = ScriptableObject.CreateInstance<EncounterDataSO>();

            SetField(encounter, "_encounterId", id.ToLower());
            SetField(encounter, "_encounterName", name);
            SetField(encounter, "_zone", zone);
            SetField(encounter, "_difficulty", difficulty);
            SetField(encounter, "_isElite", isElite);
            SetField(encounter, "_isBoss", isBoss);

            var pool = new List<EnemyDataSO>();
            foreach (var e in enemyPool)
            {
                if (e != null) pool.Add(e);
            }
            SetField(encounter, "_enemyPool", pool);
            SetField(encounter, "_minEnemies", minEnemies);
            SetField(encounter, "_maxEnemies", maxEnemies);
            SetField(encounter, "_rewardMultiplier", rewardMult);
            SetField(encounter, "_arenaCorruptionPerTurn", arenaCorruption);

            if (isBoss)
            {
                SetField(encounter, "_guaranteedCardTier", CardRewardTier.Rare);
                SetField(encounter, "_arenaDescription", "The corruption is overwhelming. +2 Corruption per turn.");
            }
            else if (isElite)
            {
                SetField(encounter, "_guaranteedCardTier", CardRewardTier.Uncommon);
            }

            string folder = $"Zone{zone}";
            string path = $"{ENCOUNTER_PATH}/{folder}/{id}.asset";
            EnsureSubfolderExists($"{ENCOUNTER_PATH}/{folder}");
            SaveAsset(encounter, path);
        }

        private static Dictionary<string, EnemyDataSO> LoadAllEnemies()
        {
            var enemies = new Dictionary<string, EnemyDataSO>();
            string[] guids = AssetDatabase.FindAssets("t:EnemyDataSO", new[] { ENEMY_PATH });

            foreach (var guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var enemy = AssetDatabase.LoadAssetAtPath<EnemyDataSO>(path);
                if (enemy != null)
                {
                    enemies[enemy.EnemyId] = enemy;
                }
            }

            return enemies;
        }

        // ============================================
        // Helper Methods
        // ============================================

        private static void EnsureDirectoriesExist()
        {
            EnsureSubfolderExists(ENEMY_PATH);
            EnsureSubfolderExists(ENCOUNTER_PATH);
        }

        private static void EnsureSubfolderExists(string path)
        {
            if (!AssetDatabase.IsValidFolder(path))
            {
                string[] parts = path.Split('/');
                string currentPath = parts[0];
                for (int i = 1; i < parts.Length; i++)
                {
                    string nextPath = currentPath + "/" + parts[i];
                    if (!AssetDatabase.IsValidFolder(nextPath))
                    {
                        AssetDatabase.CreateFolder(currentPath, parts[i]);
                    }
                    currentPath = nextPath;
                }
            }
        }

        private static void SetField(object target, string fieldName, object value)
        {
            var field = target.GetType().GetField(fieldName,
                BindingFlags.NonPublic | BindingFlags.Instance);
            if (field != null)
            {
                field.SetValue(target, value);
            }
        }

        private static void SaveAsset(Object asset, string path)
        {
            var existing = AssetDatabase.LoadAssetAtPath<Object>(path);
            if (existing != null)
            {
                EditorUtility.CopySerialized(asset, existing);
                EditorUtility.SetDirty(existing);
                Object.DestroyImmediate(asset);
                Debug.Log($"[EncounterAssetGenerator] Updated {path}");
            }
            else
            {
                AssetDatabase.CreateAsset(asset, path);
                Debug.Log($"[EncounterAssetGenerator] Created {path}");
            }
        }
    }
}
#endif
