// ============================================
// EnemyVisualWiringTool.cs
// Editor tool to wire enemy visual prefabs to data assets
// ============================================

#if UNITY_EDITOR
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEditor;
using HNR.Characters.Visuals;
using HNR.Combat;
using Assets.HeroEditor.Common.Scripts.CharacterScripts;

namespace HNR.Editor
{
    /// <summary>
    /// Editor tool to wire enemy visual prefabs with AnimatedCharacterVisual
    /// and link them to EnemyDataSO assets.
    /// </summary>
    public static class EnemyVisualWiringTool
    {
        private const string ENEMY_PREFAB_PATH = "Assets/_Project/Prefabs/Characters/Enemies";
        private const string ENEMY_DATA_PATH = "Assets/_Project/Data/Enemies";

        // Attack type mapping based on TDD specifications
        private static readonly Dictionary<string, AttackType> ATTACK_TYPE_MAP = new()
        {
            { "CorruptedWisp", AttackType.Jab },
            { "ShadowCrawler", AttackType.Jab },
            { "HollowShade", AttackType.Slash },
            { "VoidBeast", AttackType.Slash },
            { "FracturedKnight", AttackType.Slash },
            { "NullSpecter", AttackType.Shoot },
            { "HollowAmalgam", AttackType.Slash },
            { "VoidExecutioner", AttackType.Slash },
            { "CorruptedWarden", AttackType.Slash },
            { "NullHerald", AttackType.Shoot },
            { "HollowKing", AttackType.Slash }
        };

        // Scale mapping based on TDD specifications
        private static readonly Dictionary<string, float> SCALE_MAP = new()
        {
            { "CorruptedWisp", 0.7f },
            { "ShadowCrawler", 0.8f },
            { "HollowShade", 1.0f },
            { "VoidBeast", 1.4f },
            { "FracturedKnight", 1.2f },
            { "NullSpecter", 1.0f },
            { "HollowAmalgam", 1.5f },
            { "VoidExecutioner", 1.3f },
            { "CorruptedWarden", 1.4f },
            { "NullHerald", 1.3f },
            { "HollowKing", 1.6f }
        };

        // ============================================
        // Main Entry Point
        // ============================================

        /// <summary>
        /// Wire all enemy visual prefabs: add AnimatedCharacterVisual,
        /// wire Character reference, and link to EnemyDataSO assets.
        /// </summary>
        public static void WireAllEnemyVisuals()
        {
            int prefabsUpdated = 0;
            int dataAssetsUpdated = 0;

            // Step 1: Find all enemy prefabs
            string[] prefabGuids = AssetDatabase.FindAssets("t:Prefab", new[] { ENEMY_PREFAB_PATH });
            Debug.Log($"[EnemyVisualWiringTool] Found {prefabGuids.Length} prefabs in {ENEMY_PREFAB_PATH}");

            foreach (string guid in prefabGuids)
            {
                string prefabPath = AssetDatabase.GUIDToAssetPath(guid);
                if (UpdateEnemyPrefab(prefabPath))
                {
                    prefabsUpdated++;
                }
            }

            // Step 2: Link prefabs to EnemyDataSO assets
            dataAssetsUpdated = LinkPrefabsToDataAssets();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog(
                "Enemy Visual Wiring Complete",
                $"Updated {prefabsUpdated} prefab(s) with AnimatedCharacterVisual.\n" +
                $"Linked {dataAssetsUpdated} EnemyDataSO asset(s) to visual prefabs.\n\n" +
                "Run HNR > Production > Complete Production Finalization to finalize setup.",
                "OK"
            );
        }

        // ============================================
        // Prefab Update
        // ============================================

        private static bool UpdateEnemyPrefab(string prefabPath)
        {
            // Load the prefab
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (prefab == null)
            {
                Debug.LogWarning($"[EnemyVisualWiringTool] Could not load prefab at {prefabPath}");
                return false;
            }

            // Check if AnimatedCharacterVisual already exists
            var existingVisual = prefab.GetComponent<AnimatedCharacterVisual>();
            if (existingVisual != null)
            {
                Debug.Log($"[EnemyVisualWiringTool] {prefab.name} already has AnimatedCharacterVisual");

                // Still check if Character reference is wired
                if (VerifyCharacterReference(existingVisual, prefab))
                {
                    return true;
                }
            }

            // Open prefab for editing
            string prefabAssetPath = AssetDatabase.GetAssetPath(prefab);
            GameObject prefabRoot = PrefabUtility.LoadPrefabContents(prefabAssetPath);

            try
            {
                // Find HeroEditor Character component
                Character character = prefabRoot.GetComponentInChildren<Character>();
                if (character == null)
                {
                    Debug.LogWarning($"[EnemyVisualWiringTool] {prefab.name} has no HeroEditor Character component");
                    PrefabUtility.UnloadPrefabContents(prefabRoot);
                    return false;
                }

                // Add or get AnimatedCharacterVisual
                AnimatedCharacterVisual visual = prefabRoot.GetComponent<AnimatedCharacterVisual>();
                if (visual == null)
                {
                    visual = prefabRoot.AddComponent<AnimatedCharacterVisual>();
                    Debug.Log($"[EnemyVisualWiringTool] Added AnimatedCharacterVisual to {prefab.name}");
                }

                // Wire the Character reference using reflection
                var characterField = typeof(AnimatedCharacterVisual).GetField("_character",
                    BindingFlags.NonPublic | BindingFlags.Instance);
                if (characterField != null)
                {
                    characterField.SetValue(visual, character);
                    Debug.Log($"[EnemyVisualWiringTool] Wired Character reference for {prefab.name}");
                }

                // Save the prefab
                PrefabUtility.SaveAsPrefabAsset(prefabRoot, prefabAssetPath);
                Debug.Log($"[EnemyVisualWiringTool] Saved prefab: {prefab.name}");
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(prefabRoot);
            }

            return true;
        }

        private static bool VerifyCharacterReference(AnimatedCharacterVisual visual, GameObject prefab)
        {
            var characterField = typeof(AnimatedCharacterVisual).GetField("_character",
                BindingFlags.NonPublic | BindingFlags.Instance);

            if (characterField != null)
            {
                var character = characterField.GetValue(visual) as Character;
                if (character == null)
                {
                    // Need to wire it
                    return false;
                }
            }
            return true;
        }

        // ============================================
        // Data Asset Linking
        // ============================================

        private static int LinkPrefabsToDataAssets()
        {
            int linked = 0;

            // Load all prefabs into a dictionary for lookup
            var prefabDict = new Dictionary<string, GameObject>();
            string[] prefabGuids = AssetDatabase.FindAssets("t:Prefab", new[] { ENEMY_PREFAB_PATH });

            foreach (string guid in prefabGuids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (prefab != null)
                {
                    // Extract base name (remove _Visual suffix)
                    string baseName = prefab.name.Replace("_Visual", "");
                    prefabDict[baseName] = prefab;
                    Debug.Log($"[EnemyVisualWiringTool] Registered prefab: {baseName} -> {prefab.name}");
                }
            }

            // Find all EnemyDataSO assets
            string[] enemyGuids = AssetDatabase.FindAssets("t:EnemyDataSO", new[] { ENEMY_DATA_PATH });

            foreach (string guid in enemyGuids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                EnemyDataSO enemyData = AssetDatabase.LoadAssetAtPath<EnemyDataSO>(path);

                if (enemyData == null) continue;

                // Try to find matching prefab
                string enemyName = enemyData.EnemyName.Replace(" ", "");
                GameObject matchingPrefab = null;

                // Try exact match first
                if (prefabDict.TryGetValue(enemyName, out matchingPrefab))
                {
                    // Found exact match
                }
                else
                {
                    // Try partial match
                    foreach (var kvp in prefabDict)
                    {
                        if (enemyName.Contains(kvp.Key) || kvp.Key.Contains(enemyName))
                        {
                            matchingPrefab = kvp.Value;
                            break;
                        }
                    }
                }

                if (matchingPrefab != null)
                {
                    SerializedObject so = new SerializedObject(enemyData);

                    // Set visual prefab
                    SerializedProperty visualProp = so.FindProperty("_visualPrefab");
                    if (visualProp != null)
                    {
                        visualProp.objectReferenceValue = matchingPrefab;
                    }

                    // Set attack type based on mapping
                    string prefabBaseName = matchingPrefab.name.Replace("_Visual", "");
                    if (ATTACK_TYPE_MAP.TryGetValue(prefabBaseName, out AttackType attackType))
                    {
                        SerializedProperty attackProp = so.FindProperty("_preferredAttackType");
                        if (attackProp != null)
                        {
                            attackProp.enumValueIndex = (int)attackType;
                        }
                    }

                    // Set scale based on mapping
                    if (SCALE_MAP.TryGetValue(prefabBaseName, out float scale))
                    {
                        SerializedProperty scaleProp = so.FindProperty("_spriteScale");
                        if (scaleProp != null)
                        {
                            scaleProp.floatValue = scale;
                        }
                    }

                    so.ApplyModifiedProperties();
                    EditorUtility.SetDirty(enemyData);

                    Debug.Log($"[EnemyVisualWiringTool] Linked {enemyData.EnemyName} -> {matchingPrefab.name} (Attack: {attackType})");
                    linked++;
                }
                else
                {
                    Debug.LogWarning($"[EnemyVisualWiringTool] No matching prefab found for {enemyData.EnemyName}");
                }
            }

            return linked;
        }

        // ============================================
        // Individual Prefab Tools
        // ============================================

        /// <summary>
        /// Add AnimatedCharacterVisual to a single prefab.
        /// </summary>
        public static void AddAnimatedCharacterVisualToPrefab()
        {
            GameObject selected = Selection.activeGameObject;
            if (selected == null)
            {
                EditorUtility.DisplayDialog("No Selection", "Please select a prefab in the Project window.", "OK");
                return;
            }

            string path = AssetDatabase.GetAssetPath(selected);
            if (string.IsNullOrEmpty(path) || !path.EndsWith(".prefab"))
            {
                EditorUtility.DisplayDialog("Invalid Selection", "Please select a prefab asset.", "OK");
                return;
            }

            if (UpdateEnemyPrefab(path))
            {
                AssetDatabase.SaveAssets();
                EditorUtility.DisplayDialog("Success", $"Updated {selected.name} with AnimatedCharacterVisual.", "OK");
            }
        }

        /// <summary>
        /// Verify all enemy prefabs have proper wiring.
        /// </summary>
        public static void VerifyAllEnemyPrefabs()
        {
            int total = 0;
            int valid = 0;
            int missing = 0;

            string[] prefabGuids = AssetDatabase.FindAssets("t:Prefab", new[] { ENEMY_PREFAB_PATH });

            foreach (string guid in prefabGuids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (prefab == null) continue;

                total++;

                var visual = prefab.GetComponent<AnimatedCharacterVisual>();
                var character = prefab.GetComponentInChildren<Character>();

                if (visual != null && character != null)
                {
                    valid++;
                    Debug.Log($"[Verify] {prefab.name}: OK");
                }
                else
                {
                    missing++;
                    Debug.LogWarning($"[Verify] {prefab.name}: Missing - Visual:{visual != null}, Character:{character != null}");
                }
            }

            EditorUtility.DisplayDialog(
                "Enemy Prefab Verification",
                $"Total: {total}\nValid: {valid}\nMissing: {missing}\n\nSee Console for details.",
                "OK"
            );
        }
    }
}
#endif
