// ============================================
// BackgroundArtLinker.cs
// Editor tool to link generated background art to config assets
// ============================================

using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using HNR.UI.Config;
using HNR.Map;

namespace HNR.Editor
{
    /// <summary>
    /// Links AI-generated background art images to BackgroundConfigSO and EchoEventDataSO assets.
    /// Randomly selects from available variants (v01-v04) per background.
    /// </summary>
    public static class BackgroundArtLinker
    {
        private const string BACKGROUND_ART_PATH = "Assets/_Project/Art/UI/Backgrounds";
        private const string BACKGROUND_CONFIG_PATH = "Assets/_Project/Data/Config/BackgroundConfig.asset";
        private const string ECHO_EVENTS_PATH = "Assets/_Project/Data/Events";

        // Scene background mappings: config property name -> image filename (without variant/extension)
        private static readonly Dictionary<string, string> SceneBackgroundMappings = new()
        {
            { "_mainMenuBackground", "Scenes/bg_main_menu" },
            { "_bastionBackground", "Scenes/bg_bastion" },
            { "_missionsBackground", "Scenes/bg_missions" },
            { "_battleMissionBackground", "Scenes/bg_battle_mission" },
            { "_requiemsBackground", "Scenes/bg_requiems" },
            { "_nullRiftZone1Background", "NullRift/bg_zone1_outer_reaches" },
            { "_nullRiftZone2Background", "NullRift/bg_zone2_hollow_depths" },
            { "_nullRiftZone3Background", "NullRift/bg_zone3_null_core" },
            { "_combatNormalBackground", "Combat/bg_combat_normal" },
            { "_combatEliteBackground", "Combat/bg_combat_elite" },
            { "_combatBossBackground", "Combat/bg_combat_boss" },
            { "_sanctuaryBackground", "Screens/bg_sanctuary" },
            { "_shopBackground", "Screens/bg_shop" },
            { "_campfireSprite", "Screens/campfire" }
        };

        // Echo event mappings: asset filename (without extension) -> image filename (without variant/extension)
        private static readonly Dictionary<string, string> EchoEventMappings = new()
        {
            { "Echo_AbandonedCache", "EchoEvents/echo_abandoned_cache" },
            { "Echo_AncientShrine", "EchoEvents/echo_ancient_shrine" },
            { "Echo_MemoryFragment", "EchoEvents/echo_memory_fragment" },
            { "Echo_ShadowMerchant", "EchoEvents/echo_shadow_merchant" },
            { "Echo_VoidRift", "EchoEvents/echo_void_rift" },
            { "Echo_WoundedTraveler", "EchoEvents/echo_wounded_traveler" }
        };

        /// <summary>
        /// Links all background art to config assets, randomly selecting from variants.
        /// </summary>
        [MenuItem("HNR/1. Data Assets/Backgrounds/Link Background Art (Random)")]
        public static void LinkAllBackgroundArt()
        {
            LinkAllBackgroundArt(null);
        }

        /// <summary>
        /// Links all background art with a fixed seed for reproducible selection.
        /// </summary>
        [MenuItem("HNR/1. Data Assets/Backgrounds/Link Background Art (Seeded)")]
        public static void LinkAllBackgroundArtSeeded()
        {
            LinkAllBackgroundArt(42);
        }

        /// <summary>
        /// Preview what would be linked without making changes.
        /// </summary>
        [MenuItem("HNR/1. Data Assets/Backgrounds/Preview Background Art Linking")]
        public static void PreviewLinking()
        {
            int sceneCount = 0;
            int echoCount = 0;
            int notFound = 0;
            var notFoundList = new List<string>();

            Debug.Log("=== Background Art Linking Preview ===");

            // Preview scene backgrounds
            foreach (var mapping in SceneBackgroundMappings)
            {
                var variants = FindArtVariants(mapping.Value);
                if (variants.Count > 0)
                {
                    sceneCount++;
                    Debug.Log($"[FOUND] {mapping.Key} -> {variants.Count} variants available");
                }
                else
                {
                    notFound++;
                    notFoundList.Add(mapping.Value);
                }
            }

            // Preview echo event backgrounds
            foreach (var mapping in EchoEventMappings)
            {
                var variants = FindArtVariants(mapping.Value);
                if (variants.Count > 0)
                {
                    echoCount++;
                    Debug.Log($"[FOUND] {mapping.Key} -> {variants.Count} variants available");
                }
                else
                {
                    notFound++;
                    notFoundList.Add(mapping.Value);
                }
            }

            Debug.Log($"\n=== Summary ===");
            Debug.Log($"Scene backgrounds found: {sceneCount}/{SceneBackgroundMappings.Count}");
            Debug.Log($"Echo event backgrounds found: {echoCount}/{EchoEventMappings.Count}");
            Debug.Log($"Not found: {notFound}");

            if (notFoundList.Count > 0)
            {
                Debug.LogWarning($"Missing images:\n- {string.Join("\n- ", notFoundList)}");
            }
        }

        /// <summary>
        /// Links all background art to config assets with optional seed for reproducibility.
        /// </summary>
        /// <param name="seed">Random seed (null for random selection each time)</param>
        public static void LinkAllBackgroundArt(int? seed)
        {
            var random = seed.HasValue ? new System.Random(seed.Value) : new System.Random();

            int linked = 0;
            int skipped = 0;
            int notFound = 0;
            var notFoundList = new List<string>();

            EditorUtility.DisplayProgressBar("Linking Background Art", "Loading config...", 0f);

            try
            {
                // Link scene backgrounds to BackgroundConfigSO
                var bgConfig = AssetDatabase.LoadAssetAtPath<BackgroundConfigSO>(BACKGROUND_CONFIG_PATH);
                if (bgConfig != null)
                {
                    var so = new SerializedObject(bgConfig);

                    int count = 0;
                    int total = SceneBackgroundMappings.Count;

                    foreach (var mapping in SceneBackgroundMappings)
                    {
                        EditorUtility.DisplayProgressBar("Linking Background Art",
                            $"Processing {mapping.Key}...", (float)count / total * 0.5f);

                        var variants = FindArtVariants(mapping.Value);
                        if (variants.Count > 0)
                        {
                            int selectedIndex = random.Next(variants.Count);
                            string selectedPath = variants[selectedIndex];
                            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(selectedPath);

                            if (sprite != null)
                            {
                                var prop = so.FindProperty(mapping.Key);
                                if (prop != null)
                                {
                                    prop.objectReferenceValue = sprite;
                                    linked++;
                                    Debug.Log($"[BackgroundArtLinker] Linked {mapping.Key} -> {Path.GetFileName(selectedPath)}");
                                }
                                else
                                {
                                    Debug.LogWarning($"[BackgroundArtLinker] Property not found: {mapping.Key}");
                                    skipped++;
                                }
                            }
                            else
                            {
                                Debug.LogWarning($"[BackgroundArtLinker] Failed to load sprite: {selectedPath}");
                                skipped++;
                            }
                        }
                        else
                        {
                            notFound++;
                            notFoundList.Add(mapping.Value);
                        }

                        count++;
                    }

                    so.ApplyModifiedPropertiesWithoutUndo();
                    EditorUtility.SetDirty(bgConfig);
                    AssetDatabase.SaveAssets();
                }
                else
                {
                    Debug.LogWarning($"[BackgroundArtLinker] BackgroundConfig not found at {BACKGROUND_CONFIG_PATH}. Create via 'Create > HNR > Config > Background Config'.");
                }

                // Link echo event backgrounds to EchoEventDataSO assets
                var echoGuids = AssetDatabase.FindAssets("t:EchoEventDataSO", new[] { ECHO_EVENTS_PATH });

                for (int i = 0; i < echoGuids.Length; i++)
                {
                    string assetPath = AssetDatabase.GUIDToAssetPath(echoGuids[i]);
                    string assetName = Path.GetFileNameWithoutExtension(assetPath);

                    EditorUtility.DisplayProgressBar("Linking Background Art",
                        $"Processing {assetName}...", 0.5f + ((float)i / echoGuids.Length * 0.5f));

                    if (EchoEventMappings.TryGetValue(assetName, out string imagePath))
                    {
                        var variants = FindArtVariants(imagePath);
                        if (variants.Count > 0)
                        {
                            int selectedIndex = random.Next(variants.Count);
                            string selectedPath = variants[selectedIndex];
                            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(selectedPath);

                            if (sprite != null)
                            {
                                var echoEvent = AssetDatabase.LoadAssetAtPath<EchoEventDataSO>(assetPath);
                                if (echoEvent != null)
                                {
                                    var so = new SerializedObject(echoEvent);
                                    var prop = so.FindProperty("_backgroundImage");
                                    if (prop != null)
                                    {
                                        prop.objectReferenceValue = sprite;
                                        so.ApplyModifiedPropertiesWithoutUndo();
                                        EditorUtility.SetDirty(echoEvent);
                                        linked++;
                                        Debug.Log($"[BackgroundArtLinker] Linked {assetName} -> {Path.GetFileName(selectedPath)}");
                                    }
                                }
                            }
                        }
                        else
                        {
                            notFound++;
                            notFoundList.Add(imagePath);
                        }
                    }
                }

                AssetDatabase.SaveAssets();
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }

            // Report results
            Debug.Log($"\n=== Background Art Linking Complete ===");
            Debug.Log($"Linked: {linked}");
            Debug.Log($"Skipped: {skipped}");
            Debug.Log($"Not Found: {notFound}");

            if (notFoundList.Count > 0)
            {
                Debug.LogWarning($"Missing images (generate with Stable Diffusion):\n- {string.Join("\n- ", notFoundList)}");
            }

            if (linked > 0)
            {
                EditorUtility.DisplayDialog("Background Art Linking Complete",
                    $"Successfully linked {linked} background images.\n\n" +
                    $"Skipped: {skipped}\n" +
                    $"Not Found: {notFound}",
                    "OK");
            }
        }

        /// <summary>
        /// Clears all background art assignments from config.
        /// </summary>
        [MenuItem("HNR/1. Data Assets/Backgrounds/Clear All Background Art")]
        public static void ClearAllBackgroundArt()
        {
            if (!EditorUtility.DisplayDialog("Clear All Background Art",
                "This will remove all background sprite assignments from:\n\n" +
                "- BackgroundConfig\n" +
                "- All EchoEventDataSO assets\n\n" +
                "Continue?",
                "Yes, Clear All", "Cancel"))
            {
                return;
            }

            int cleared = 0;

            // Clear BackgroundConfigSO
            var bgConfig = AssetDatabase.LoadAssetAtPath<BackgroundConfigSO>(BACKGROUND_CONFIG_PATH);
            if (bgConfig != null)
            {
                var so = new SerializedObject(bgConfig);
                foreach (var mapping in SceneBackgroundMappings)
                {
                    var prop = so.FindProperty(mapping.Key);
                    if (prop != null)
                    {
                        prop.objectReferenceValue = null;
                        cleared++;
                    }
                }
                so.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(bgConfig);
            }

            // Clear EchoEventDataSO assets
            var echoGuids = AssetDatabase.FindAssets("t:EchoEventDataSO", new[] { ECHO_EVENTS_PATH });
            foreach (var guid in echoGuids)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                var echoEvent = AssetDatabase.LoadAssetAtPath<EchoEventDataSO>(assetPath);
                if (echoEvent != null)
                {
                    var so = new SerializedObject(echoEvent);
                    var prop = so.FindProperty("_backgroundImage");
                    if (prop != null)
                    {
                        prop.objectReferenceValue = null;
                        so.ApplyModifiedPropertiesWithoutUndo();
                        EditorUtility.SetDirty(echoEvent);
                        cleared++;
                    }
                }
            }

            AssetDatabase.SaveAssets();
            Debug.Log($"[BackgroundArtLinker] Cleared {cleared} background assignments");
        }

        /// <summary>
        /// Finds all art variants for a given base path.
        /// </summary>
        /// <param name="basePath">Base path relative to BACKGROUND_ART_PATH (without extension)</param>
        /// <returns>List of full asset paths for found variants</returns>
        private static List<string> FindArtVariants(string basePath)
        {
            var variants = new List<string>();
            string fullBasePath = $"{BACKGROUND_ART_PATH}/{basePath}";

            // Check for variants v01-v04
            for (int i = 1; i <= 4; i++)
            {
                string variantPath = $"{fullBasePath}_v{i:D2}.png";
                if (File.Exists(variantPath))
                {
                    variants.Add(variantPath);
                }
            }

            // If no variants found, check for base file without variant suffix
            if (variants.Count == 0)
            {
                string basePngPath = $"{fullBasePath}.png";
                if (File.Exists(basePngPath))
                {
                    variants.Add(basePngPath);
                }
            }

            return variants;
        }
    }
}
