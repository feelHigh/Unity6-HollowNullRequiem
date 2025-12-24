// ============================================
// ProductionDataGenerator.cs
// Creates all production zone configs and encounters
// ============================================

#if UNITY_EDITOR
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEditor;
using HNR.Map;
using HNR.Combat;
using HNR.Progression;

namespace HNR.Editor
{
    /// <summary>
    /// Generates production-ready zone configurations and encounters.
    /// Creates Zone 1, 2, 3 configs with proper encounter linking.
    /// </summary>
    public static class ProductionDataGenerator
    {
        private const string ZONE_PATH = "Assets/_Project/Data/Zones";
        private const string ENCOUNTER_PATH = "Assets/_Project/Data/Encounters";
        private const string ENEMY_PATH = "Assets/_Project/Data/Enemies";
        private const string EVENTS_PATH = "Assets/_Project/Data/Events";
        private const string CONFIG_PATH = "Assets/_Project/Resources/Data/Config";

        // ============================================
        // Public Methods (called from EditorMenuOrganizer)
        // ============================================

        public static void GenerateAllZoneData()
        {
            EnsureDirectoryExists(ZONE_PATH);
            EnsureDirectoryExists(ENCOUNTER_PATH);

            // Load all existing enemies
            var enemies = LoadAllEnemies();
            Debug.Log($"[ProductionDataGenerator] Found {enemies.Count} enemies");

            // Load all echo events
            var echoEvents = LoadAllEchoEvents();
            Debug.Log($"[ProductionDataGenerator] Found {echoEvents.Count} echo events");

            // Load existing encounters or create new ones
            var encounters = LoadOrCreateEncounters(enemies);

            // Create zone configs
            CreateZone1Config(encounters, echoEvents);
            CreateZone2Config(encounters, echoEvents);
            CreateZone3Config(encounters, echoEvents);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog("Production Data Generated",
                "Created:\n" +
                "- Zone1_Config.asset with encounters\n" +
                "- Zone2_Config.asset with encounters\n" +
                "- Zone3_Config.asset with encounters\n" +
                "- 6 new encounter files (Zone2 + Zone3)",
                "OK");
        }

        public static void GenerateZoneConfigsOnly()
        {
            EnsureDirectoryExists(ZONE_PATH);

            var encounters = LoadAllEncounters();
            var echoEvents = LoadAllEchoEvents();

            CreateZone1Config(encounters, echoEvents);
            CreateZone2Config(encounters, echoEvents);
            CreateZone3Config(encounters, echoEvents);

            AssetDatabase.SaveAssets();
            Debug.Log("[ProductionDataGenerator] Zone configs generated");
        }

        public static void GenerateZone2And3Encounters()
        {
            EnsureDirectoryExists(ENCOUNTER_PATH);

            var enemies = LoadAllEnemies();
            CreateZone2Encounters(enemies);
            CreateZone3Encounters(enemies);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[ProductionDataGenerator] Zone 2 & 3 encounters generated");
        }

        /// <summary>
        /// Generates the ShopConfigSO asset for the Void Market.
        /// </summary>
        public static void GenerateShopConfig()
        {
            EnsureDirectoryExists(CONFIG_PATH);

            string path = $"{CONFIG_PATH}/ShopConfig.asset";

            // Check if already exists
            var existing = AssetDatabase.LoadAssetAtPath<ShopConfigSO>(path);
            if (existing != null)
            {
                Debug.Log("[ProductionDataGenerator] ShopConfig already exists");
                return;
            }

            // Create new ShopConfigSO with default values
            var config = ScriptableObject.CreateInstance<ShopConfigSO>();
            AssetDatabase.CreateAsset(config, path);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"[ProductionDataGenerator] Created ShopConfig at {path}");
        }

        // ============================================
        // Zone Config Creation
        // ============================================

        private static void CreateZone1Config(Dictionary<string, EncounterDataSO> encounters, List<EchoEventDataSO> echoEvents)
        {
            string path = $"{ZONE_PATH}/Zone1_Config.asset";

            var config = LoadOrCreateAsset<ZoneConfigSO>(path);
            var so = new SerializedObject(config);

            // Zone Info
            so.FindProperty("_zoneNumber").intValue = 1;
            so.FindProperty("_zoneName").stringValue = "The Hollow Depths";
            so.FindProperty("_zoneDescription").stringValue = "Entry to the Null Rift. Weaker enemies test your resolve.";

            // Layout
            so.FindProperty("_rowCount").intValue = 5;
            so.FindProperty("_minNodesPerRow").intValue = 2;
            so.FindProperty("_maxNodesPerRow").intValue = 3;

            // Weights
            so.FindProperty("_combatWeight").intValue = 50;
            so.FindProperty("_eliteWeight").intValue = 5;
            so.FindProperty("_shopWeight").intValue = 12;
            so.FindProperty("_echoWeight").intValue = 18;
            so.FindProperty("_sanctuaryWeight").intValue = 10;
            so.FindProperty("_treasureWeight").intValue = 5;

            // Rules
            so.FindProperty("_eliteMinRow").intValue = 3;
            so.FindProperty("_guaranteedShop").boolValue = true;
            so.FindProperty("_guaranteedSanctuary").boolValue = true;

            // Combat Encounters (keys have underscores removed in LoadAllEncounters)
            var combatProp = so.FindProperty("_combatEncounters");
            combatProp.ClearArray();
            AddEncounterToList(combatProp, encounters.GetValueOrDefault("zone1easy"));
            AddEncounterToList(combatProp, encounters.GetValueOrDefault("zone1medium"));
            AddEncounterToList(combatProp, encounters.GetValueOrDefault("zone1hard"));

            // Elite Encounters
            var eliteProp = so.FindProperty("_eliteEncounters");
            eliteProp.ClearArray();
            AddEncounterToList(eliteProp, encounters.GetValueOrDefault("elitewarden"));

            // Boss - Zone 1 doesn't have a boss
            so.FindProperty("_bossEncounter").objectReferenceValue = null;

            // Echo Events
            var echoProp = so.FindProperty("_echoEvents");
            echoProp.ClearArray();
            foreach (var evt in echoEvents)
            {
                AddToList(echoProp, evt);
            }

            // Visual
            so.FindProperty("_horizontalSpacing").floatValue = 200f;
            so.FindProperty("_verticalSpacing").floatValue = 150f;
            so.FindProperty("_nodeJitter").floatValue = 20f;

            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(config);
            Debug.Log($"[ProductionDataGenerator] Created {path}");
        }

        private static void CreateZone2Config(Dictionary<string, EncounterDataSO> encounters, List<EchoEventDataSO> echoEvents)
        {
            string path = $"{ZONE_PATH}/Zone2_Config.asset";

            var config = LoadOrCreateAsset<ZoneConfigSO>(path);
            var so = new SerializedObject(config);

            // Zone Info
            so.FindProperty("_zoneNumber").intValue = 2;
            so.FindProperty("_zoneName").stringValue = "The Fractured Halls";
            so.FindProperty("_zoneDescription").stringValue = "Deeper into the void. Stronger foes and greater corruption await.";

            // Layout - More nodes, more paths
            so.FindProperty("_rowCount").intValue = 6;
            so.FindProperty("_minNodesPerRow").intValue = 2;
            so.FindProperty("_maxNodesPerRow").intValue = 4;

            // Weights - More elites, less easy encounters
            so.FindProperty("_combatWeight").intValue = 45;
            so.FindProperty("_eliteWeight").intValue = 12;
            so.FindProperty("_shopWeight").intValue = 10;
            so.FindProperty("_echoWeight").intValue = 15;
            so.FindProperty("_sanctuaryWeight").intValue = 12;
            so.FindProperty("_treasureWeight").intValue = 6;

            // Rules
            so.FindProperty("_eliteMinRow").intValue = 2;
            so.FindProperty("_guaranteedShop").boolValue = true;
            so.FindProperty("_guaranteedSanctuary").boolValue = true;

            // Combat Encounters (keys have underscores removed)
            var combatProp = so.FindProperty("_combatEncounters");
            combatProp.ClearArray();
            AddEncounterToList(combatProp, encounters.GetValueOrDefault("zone2easy"));
            AddEncounterToList(combatProp, encounters.GetValueOrDefault("zone2medium"));
            AddEncounterToList(combatProp, encounters.GetValueOrDefault("zone2hard"));

            // Elite Encounters
            var eliteProp = so.FindProperty("_eliteEncounters");
            eliteProp.ClearArray();
            AddEncounterToList(eliteProp, encounters.GetValueOrDefault("elitewarden"));
            AddEncounterToList(eliteProp, encounters.GetValueOrDefault("eliteherald"));

            // Boss - Zone 2 doesn't have a boss
            so.FindProperty("_bossEncounter").objectReferenceValue = null;

            // Echo Events
            var echoProp = so.FindProperty("_echoEvents");
            echoProp.ClearArray();
            foreach (var evt in echoEvents)
            {
                AddToList(echoProp, evt);
            }

            // Visual
            so.FindProperty("_horizontalSpacing").floatValue = 200f;
            so.FindProperty("_verticalSpacing").floatValue = 150f;
            so.FindProperty("_nodeJitter").floatValue = 25f;

            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(config);
            Debug.Log($"[ProductionDataGenerator] Created {path}");
        }

        private static void CreateZone3Config(Dictionary<string, EncounterDataSO> encounters, List<EchoEventDataSO> echoEvents)
        {
            string path = $"{ZONE_PATH}/Zone3_Config.asset";

            var config = LoadOrCreateAsset<ZoneConfigSO>(path);
            var so = new SerializedObject(config);

            // Zone Info
            so.FindProperty("_zoneNumber").intValue = 3;
            so.FindProperty("_zoneName").stringValue = "The Null Heart";
            so.FindProperty("_zoneDescription").stringValue = "The core of corruption. Only the strongest survive to face the Hollow King.";

            // Layout - Longer zone, more branching
            so.FindProperty("_rowCount").intValue = 7;
            so.FindProperty("_minNodesPerRow").intValue = 2;
            so.FindProperty("_maxNodesPerRow").intValue = 4;

            // Weights - Even more elites, highest difficulty
            so.FindProperty("_combatWeight").intValue = 40;
            so.FindProperty("_eliteWeight").intValue = 18;
            so.FindProperty("_shopWeight").intValue = 8;
            so.FindProperty("_echoWeight").intValue = 12;
            so.FindProperty("_sanctuaryWeight").intValue = 15;
            so.FindProperty("_treasureWeight").intValue = 7;

            // Rules
            so.FindProperty("_eliteMinRow").intValue = 2;
            so.FindProperty("_guaranteedShop").boolValue = true;
            so.FindProperty("_guaranteedSanctuary").boolValue = true;

            // Combat Encounters (keys have underscores removed)
            var combatProp = so.FindProperty("_combatEncounters");
            combatProp.ClearArray();
            AddEncounterToList(combatProp, encounters.GetValueOrDefault("zone3easy"));
            AddEncounterToList(combatProp, encounters.GetValueOrDefault("zone3medium"));
            AddEncounterToList(combatProp, encounters.GetValueOrDefault("zone3hard"));

            // Elite Encounters
            var eliteProp = so.FindProperty("_eliteEncounters");
            eliteProp.ClearArray();
            AddEncounterToList(eliteProp, encounters.GetValueOrDefault("eliteherald"));
            AddEncounterToList(eliteProp, encounters.GetValueOrDefault("elitewarden"));

            // Boss - Zone 3 has the final boss
            so.FindProperty("_bossEncounter").objectReferenceValue = encounters.GetValueOrDefault("bosshollowking");

            // Echo Events
            var echoProp = so.FindProperty("_echoEvents");
            echoProp.ClearArray();
            foreach (var evt in echoEvents)
            {
                AddToList(echoProp, evt);
            }

            // Visual
            so.FindProperty("_horizontalSpacing").floatValue = 200f;
            so.FindProperty("_verticalSpacing").floatValue = 150f;
            so.FindProperty("_nodeJitter").floatValue = 30f;

            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(config);
            Debug.Log($"[ProductionDataGenerator] Created {path}");
        }

        // ============================================
        // Encounter Creation
        // ============================================

        private static Dictionary<string, EncounterDataSO> LoadOrCreateEncounters(Dictionary<string, EnemyDataSO> enemies)
        {
            var encounters = LoadAllEncounters();

            // Populate Zone 1 encounters with enemies
            PopulateZone1Encounters(enemies, encounters);

            // Populate elite and boss encounters
            PopulateEliteAndBossEncounters(enemies, encounters);

            // Create Zone 2 encounters if they don't exist
            CreateZone2Encounters(enemies, encounters);

            // Create Zone 3 encounters if they don't exist
            CreateZone3Encounters(enemies, encounters);

            return encounters;
        }

        private static void PopulateZone1Encounters(Dictionary<string, EnemyDataSO> enemies, Dictionary<string, EncounterDataSO> encounters)
        {
            // Find Zone 1 enemies
            var wisp = enemies.GetValueOrDefault("zone1corruptedwisp") ?? FindEnemyByPartialName(enemies, "Wisp");
            var shade = enemies.GetValueOrDefault("zone1hollowshade") ?? FindEnemyByPartialName(enemies, "Shade");
            var crawler = enemies.GetValueOrDefault("zone1shadowcrawler") ?? FindEnemyByPartialName(enemies, "Crawler");
            var beast = enemies.GetValueOrDefault("zone1voidbeast") ?? FindEnemyByPartialName(enemies, "Beast");

            Debug.Log($"[ProductionDataGenerator] Zone 1 enemies found: Wisp={wisp != null}, Shade={shade != null}, Crawler={crawler != null}, Beast={beast != null}");

            // Populate Zone1_Easy
            var zone1Easy = encounters.GetValueOrDefault("zone1easy");
            if (zone1Easy != null)
            {
                PopulateEncounterEnemies(zone1Easy, new[] { wisp, crawler });
            }

            // Populate Zone1_Medium
            var zone1Medium = encounters.GetValueOrDefault("zone1medium");
            if (zone1Medium != null)
            {
                PopulateEncounterEnemies(zone1Medium, new[] { shade, beast });
            }

            // Populate Zone1_Hard
            var zone1Hard = encounters.GetValueOrDefault("zone1hard");
            if (zone1Hard != null)
            {
                PopulateEncounterEnemies(zone1Hard, new[] { shade, beast, wisp });
            }
        }

        private static void PopulateEliteAndBossEncounters(Dictionary<string, EnemyDataSO> enemies, Dictionary<string, EncounterDataSO> encounters)
        {
            // Find elite enemies
            var warden = enemies.GetValueOrDefault("elitecorruptedwarden") ?? FindEnemyByPartialName(enemies, "Warden");
            var herald = enemies.GetValueOrDefault("elitenullherald") ?? FindEnemyByPartialName(enemies, "Herald");
            var hollowKing = enemies.GetValueOrDefault("bosshollowking") ?? FindEnemyByPartialName(enemies, "King");

            Debug.Log($"[ProductionDataGenerator] Elite/Boss enemies found: Warden={warden != null}, Herald={herald != null}, HollowKing={hollowKing != null}");

            // Populate Elite_Warden
            var eliteWarden = encounters.GetValueOrDefault("elitewarden");
            if (eliteWarden != null && warden != null)
            {
                PopulateEncounterEnemies(eliteWarden, new[] { warden });
            }

            // Populate Elite_Herald
            var eliteHerald = encounters.GetValueOrDefault("eliteherald");
            if (eliteHerald != null && herald != null)
            {
                PopulateEncounterEnemies(eliteHerald, new[] { herald });
            }

            // Populate Boss_HollowKing
            var bossHollowKing = encounters.GetValueOrDefault("bosshollowking");
            if (bossHollowKing != null && hollowKing != null)
            {
                PopulateEncounterEnemies(bossHollowKing, new[] { hollowKing });
            }
        }

        private static void PopulateEncounterEnemies(EncounterDataSO encounter, EnemyDataSO[] enemyPool)
        {
            var validEnemies = new List<EnemyDataSO>();
            foreach (var enemy in enemyPool)
            {
                if (enemy != null) validEnemies.Add(enemy);
            }

            if (validEnemies.Count == 0)
            {
                Debug.LogWarning($"[ProductionDataGenerator] No valid enemies to add to {encounter.name}");
                return;
            }

            SetField(encounter, "_enemyPool", validEnemies);
            EditorUtility.SetDirty(encounter);
            Debug.Log($"[ProductionDataGenerator] Populated {encounter.name} with {validEnemies.Count} enemies");
        }

        private static void CreateZone2Encounters(Dictionary<string, EnemyDataSO> enemies, Dictionary<string, EncounterDataSO> existingEncounters = null)
        {
            existingEncounters ??= LoadAllEncounters();

            // Zone 2 enemies (keys have underscores removed in LoadAllEnemies)
            var knight = enemies.GetValueOrDefault("zone2fracturedknight") ?? enemies.GetValueOrDefault("fracturedknight");
            var specter = enemies.GetValueOrDefault("zone2nullspecter") ?? enemies.GetValueOrDefault("nullspecter");

            // Find by partial name if not found
            if (knight == null) knight = FindEnemyByPartialName(enemies, "Knight");
            if (specter == null) specter = FindEnemyByPartialName(enemies, "Specter");

            if (knight == null && specter == null)
            {
                Debug.LogWarning("[ProductionDataGenerator] No Zone 2 enemies found. Using Zone 1 enemies as fallback.");
                knight = FindEnemyByPartialName(enemies, "Shade");
                specter = FindEnemyByPartialName(enemies, "Crawler");
            }

            // Create Zone 2 Easy (keys have underscores removed)
            if (!existingEncounters.ContainsKey("zone2easy"))
            {
                CreateEncounter("zone2_easy", "Fractured Patrol", 2, EncounterDifficulty.Easy,
                    new[] { knight ?? specter }, 1, 2, 1.0f);
            }

            // Create Zone 2 Medium
            if (!existingEncounters.ContainsKey("zone2medium"))
            {
                CreateEncounter("zone2_medium", "Null Ambush", 2, EncounterDifficulty.Normal,
                    new[] { knight, specter }, 2, 2, 1.2f);
            }

            // Create Zone 2 Hard
            if (!existingEncounters.ContainsKey("zone2hard"))
            {
                CreateEncounter("zone2_hard", "Spectral Convergence", 2, EncounterDifficulty.Hard,
                    new[] { specter, knight }, 2, 3, 1.4f);
            }
        }

        private static void CreateZone3Encounters(Dictionary<string, EnemyDataSO> enemies, Dictionary<string, EncounterDataSO> existingEncounters = null)
        {
            existingEncounters ??= LoadAllEncounters();

            // Zone 3 enemies (keys have underscores removed in LoadAllEnemies)
            var amalgam = enemies.GetValueOrDefault("zone3hollowamalgam") ?? FindEnemyByPartialName(enemies, "Amalgam");
            var executioner = enemies.GetValueOrDefault("zone3voidexecutioner") ?? FindEnemyByPartialName(enemies, "Executioner");

            if (amalgam == null && executioner == null)
            {
                Debug.LogWarning("[ProductionDataGenerator] No Zone 3 enemies found. Using Zone 2 enemies as fallback.");
                amalgam = FindEnemyByPartialName(enemies, "Knight");
                executioner = FindEnemyByPartialName(enemies, "Specter");
            }

            // Create Zone 3 Easy (keys have underscores removed)
            if (!existingEncounters.ContainsKey("zone3easy"))
            {
                CreateEncounter("zone3_easy", "Hollow Manifestation", 3, EncounterDifficulty.Easy,
                    new[] { amalgam ?? executioner }, 1, 2, 1.2f);
            }

            // Create Zone 3 Medium
            if (!existingEncounters.ContainsKey("zone3medium"))
            {
                CreateEncounter("zone3_medium", "Void Legion", 3, EncounterDifficulty.Normal,
                    new[] { amalgam, executioner }, 2, 2, 1.4f);
            }

            // Create Zone 3 Hard
            if (!existingEncounters.ContainsKey("zone3hard"))
            {
                CreateEncounter("zone3_hard", "Executioner's Judgment", 3, EncounterDifficulty.Hard,
                    new[] { executioner, amalgam }, 2, 3, 1.6f);
            }
        }

        private static void CreateEncounter(string id, string name, int zone, EncounterDifficulty difficulty,
            EnemyDataSO[] enemyPool, int minEnemies, int maxEnemies, float rewardMult)
        {
            string path = $"{ENCOUNTER_PATH}/{id.Replace("_", "").Replace("zone", "Zone").Replace("easy", "_Easy").Replace("medium", "_Medium").Replace("hard", "_Hard")}.asset";

            var encounter = ScriptableObject.CreateInstance<EncounterDataSO>();

            SetField(encounter, "_encounterId", id);
            SetField(encounter, "_encounterName", name);
            SetField(encounter, "_zone", zone);
            SetField(encounter, "_difficulty", difficulty);
            SetField(encounter, "_isElite", false);
            SetField(encounter, "_isBoss", false);

            var pool = new List<EnemyDataSO>();
            foreach (var e in enemyPool)
            {
                if (e != null) pool.Add(e);
            }
            SetField(encounter, "_enemyPool", pool);
            SetField(encounter, "_minEnemies", minEnemies);
            SetField(encounter, "_maxEnemies", maxEnemies);
            SetField(encounter, "_rewardMultiplier", rewardMult);

            AssetDatabase.CreateAsset(encounter, path);
            Debug.Log($"[ProductionDataGenerator] Created encounter: {path}");
        }

        // ============================================
        // Helper Methods
        // ============================================

        private static Dictionary<string, EnemyDataSO> LoadAllEnemies()
        {
            var enemies = new Dictionary<string, EnemyDataSO>();
            string[] guids = AssetDatabase.FindAssets("t:EnemyDataSO", new[] { ENEMY_PATH });

            foreach (var guid in guids)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                var enemy = AssetDatabase.LoadAssetAtPath<EnemyDataSO>(assetPath);
                if (enemy != null)
                {
                    // Add by ID (lowercase)
                    string id = enemy.EnemyId?.ToLower() ?? "";
                    if (!string.IsNullOrEmpty(id))
                    {
                        enemies[id] = enemy;
                    }

                    // Also add by filename for easy lookup
                    string filename = System.IO.Path.GetFileNameWithoutExtension(assetPath).ToLower().Replace("_", "");
                    enemies[filename] = enemy;
                }
            }

            return enemies;
        }

        private static EnemyDataSO FindEnemyByPartialName(Dictionary<string, EnemyDataSO> enemies, string partialName)
        {
            string lower = partialName.ToLower();
            foreach (var kvp in enemies)
            {
                if (kvp.Key.Contains(lower))
                {
                    return kvp.Value;
                }
            }
            return null;
        }

        private static List<EchoEventDataSO> LoadAllEchoEvents()
        {
            var events = new List<EchoEventDataSO>();
            string[] guids = AssetDatabase.FindAssets("t:EchoEventDataSO", new[] { EVENTS_PATH });

            foreach (var guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var evt = AssetDatabase.LoadAssetAtPath<EchoEventDataSO>(path);
                if (evt != null)
                {
                    events.Add(evt);
                }
            }

            return events;
        }

        private static Dictionary<string, EncounterDataSO> LoadAllEncounters()
        {
            var encounters = new Dictionary<string, EncounterDataSO>();
            string[] guids = AssetDatabase.FindAssets("t:EncounterDataSO", new[] { ENCOUNTER_PATH });

            foreach (var guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var enc = AssetDatabase.LoadAssetAtPath<EncounterDataSO>(path);
                if (enc != null)
                {
                    // Add by ID
                    string id = enc.EncounterId?.ToLower() ?? "";
                    if (!string.IsNullOrEmpty(id))
                    {
                        encounters[id] = enc;
                    }

                    // Also by filename
                    string filename = System.IO.Path.GetFileNameWithoutExtension(path).ToLower().Replace("_", "");
                    encounters[filename] = enc;
                }
            }

            return encounters;
        }

        private static T LoadOrCreateAsset<T>(string path) where T : ScriptableObject
        {
            var existing = AssetDatabase.LoadAssetAtPath<T>(path);
            if (existing != null)
            {
                return existing;
            }

            var asset = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(asset, path);
            return asset;
        }

        private static void AddEncounterToList(SerializedProperty listProp, EncounterDataSO encounter)
        {
            if (encounter == null) return;
            int index = listProp.arraySize;
            listProp.InsertArrayElementAtIndex(index);
            listProp.GetArrayElementAtIndex(index).objectReferenceValue = encounter;
        }

        private static void AddToList(SerializedProperty listProp, Object obj)
        {
            if (obj == null) return;
            int index = listProp.arraySize;
            listProp.InsertArrayElementAtIndex(index);
            listProp.GetArrayElementAtIndex(index).objectReferenceValue = obj;
        }

        private static void EnsureDirectoryExists(string path)
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
    }
}
#endif
