// ============================================
// AudioVFXConfigGenerator.cs
// Editor tool for generating Audio and VFX configuration assets
// ============================================

using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using HNR.Audio;
using HNR.VFX;

namespace HNR.Editor
{
    /// <summary>
    /// Editor tool for generating AudioConfigSO and VFX prefabs with VFXInstance.
    /// Creates placeholder configurations that can be populated with actual assets later.
    /// </summary>
    public static class AudioVFXConfigGenerator
    {
        private const string CONFIG_PATH = "Assets/_Project/Data/Config";
        private const string VFX_PREFAB_PATH = "Assets/_Project/Prefabs/VFX";
        private const string AUDIO_PATH = "Assets/_Project/Audio";

        // ============================================
        // Public Methods
        // ============================================

        public static void GenerateAllConfigs()
        {
            GenerateAudioConfig();
            GenerateVFXConfig();
            GenerateVFXPrefabs();

            EditorUtility.DisplayDialog("Audio & VFX Config Generated",
                "Created:\n" +
                "- AudioConfig.asset with placeholder entries\n" +
                "- VFXConfig.asset with effect configurations\n" +
                "- VFX prefabs with VFXInstance components\n\n" +
                "Assign actual AudioClips and CFXR prefabs to complete setup.",
                "OK");
        }

        public static void GenerateAudioConfig()
        {
            EnsureDirectoryExists($"{CONFIG_PATH}/placeholder.asset");

            string assetPath = $"{CONFIG_PATH}/AudioConfig.asset";

            // Check if already exists
            var existing = AssetDatabase.LoadAssetAtPath<AudioConfigSO>(assetPath);
            if (existing != null)
            {
                if (!EditorUtility.DisplayDialog("AudioConfig Exists",
                    "AudioConfig.asset already exists. Overwrite with empty config?",
                    "Yes", "No"))
                {
                    return;
                }
                AssetDatabase.DeleteAsset(assetPath);
            }

            // Create new empty AudioConfigSO
            var config = ScriptableObject.CreateInstance<AudioConfigSO>();

            // Save asset (empty - use AudioConfigSetup.SetupAudioConfig to auto-link files)
            AssetDatabase.CreateAsset(config, assetPath);
            AssetDatabase.SaveAssets();

            Debug.Log($"[AudioVFXConfigGenerator] Created empty AudioConfig at {assetPath}");
            Debug.Log("[AudioVFXConfigGenerator] Use 'HNR > 3. Audio & VFX > Setup Audio Config (Auto-Link)' to populate with audio files");
        }

        public static void GenerateVFXConfig()
        {
            EnsureDirectoryExists($"{CONFIG_PATH}/placeholder.asset");

            string assetPath = $"{CONFIG_PATH}/VFXConfig.asset";

            // Check if already exists
            var existing = AssetDatabase.LoadAssetAtPath<VFXConfigSO>(assetPath);
            if (existing != null)
            {
                if (!EditorUtility.DisplayDialog("VFXConfig Exists",
                    "VFXConfig.asset already exists. Overwrite?",
                    "Yes", "No"))
                {
                    return;
                }
                AssetDatabase.DeleteAsset(assetPath);
            }

            // Create new VFXConfigSO
            var config = ScriptableObject.CreateInstance<VFXConfigSO>();

            // Use SerializedObject to populate lists
            SerializedObject so = new SerializedObject(config);

            // Hit effect entries (by Soul Aspect)
            PopulateVFXList(so, "_hitEntries", new[]
            {
                ("hit_flame", VFXCategory.Hit, 5, 10, new Color(1f, 0.5f, 0.2f)),
                ("hit_shadow", VFXCategory.Hit, 5, 10, new Color(0.3f, 0.2f, 0.4f)),
                ("hit_nature", VFXCategory.Hit, 5, 10, new Color(0.3f, 0.8f, 0.3f)),
                ("hit_arcane", VFXCategory.Hit, 5, 10, new Color(0.6f, 0.3f, 0.9f)),
                ("hit_light", VFXCategory.Hit, 5, 10, new Color(1f, 0.95f, 0.7f)),
            });

            // Combat effect entries
            PopulateVFXList(so, "_combatEntries", new[]
            {
                ("vfx_slash", VFXCategory.Combat, 3, 5, Color.white),
                ("vfx_shield", VFXCategory.Combat, 2, 3, new Color(0.4f, 0.6f, 1f)),
                ("vfx_heal", VFXCategory.Combat, 2, 3, new Color(0.4f, 1f, 0.5f)),
            });

            // Status effect entries
            PopulateVFXList(so, "_statusEntries", new[]
            {
                ("vfx_buff", VFXCategory.Status, 2, 4, new Color(0.5f, 1f, 0.5f)),
                ("vfx_debuff", VFXCategory.Status, 2, 4, new Color(0.8f, 0.3f, 0.3f)),
                ("vfx_corruption", VFXCategory.Status, 3, 5, new Color(0.5f, 0.1f, 0.3f)),
            });

            // Special effect entries
            PopulateVFXList(so, "_specialEntries", new[]
            {
                ("vfx_null_burst", VFXCategory.Special, 1, 2, new Color(0.8f, 0.7f, 1f)),
                ("vfx_requiem_art", VFXCategory.Special, 1, 2, Color.white),
                // Character-specific Requiem Art VFX
                ("vfx_requiem_art_kira", VFXCategory.Special, 1, 2, new Color(1f, 0.5f, 0.2f)),
                ("vfx_requiem_art_mordren", VFXCategory.Special, 1, 2, new Color(0.3f, 0.2f, 0.5f)),
                ("vfx_requiem_art_elara", VFXCategory.Special, 1, 2, new Color(1f, 0.95f, 0.7f)),
                ("vfx_requiem_art_thornwick", VFXCategory.Special, 1, 2, new Color(0.4f, 0.7f, 0.3f)),
            });

            // Card effect entries
            PopulateVFXList(so, "_cardEntries", new[]
            {
                ("vfx_card_draw", VFXCategory.Card, 2, 4, new Color(0.7f, 0.9f, 1f)),
                ("vfx_card_play", VFXCategory.Card, 2, 4, Color.white),
            });

            so.ApplyModifiedPropertiesWithoutUndo();

            // Save asset
            AssetDatabase.CreateAsset(config, assetPath);
            AssetDatabase.SaveAssets();

            Debug.Log($"[AudioVFXConfigGenerator] Created VFXConfig at {assetPath}");
        }

        public static void GenerateVFXPrefabs()
        {
            EnsureDirectoryExists($"{VFX_PREFAB_PATH}/placeholder.prefab");

            // VFX effect configurations: (effectId, preWarmCount, maxActive)
            var vfxConfigs = new[]
            {
                // Hit effects by aspect
                ("hit_flame", 5, 10),
                ("hit_shadow", 5, 10),
                ("hit_nature", 5, 10),
                ("hit_arcane", 5, 10),
                ("hit_light", 5, 10),

                // Combat effects
                ("vfx_slash", 3, 5),
                ("vfx_shield", 2, 3),
                ("vfx_heal", 2, 3),
                ("vfx_buff", 2, 4),
                ("vfx_debuff", 2, 4),

                // Special effects
                ("vfx_corruption", 3, 5),
                ("vfx_null_burst", 1, 2),
                ("vfx_requiem_art", 1, 2),
                ("vfx_card_draw", 2, 4),
                ("vfx_card_play", 2, 4)
            };

            int created = 0;
            foreach (var (effectId, preWarm, maxActive) in vfxConfigs)
            {
                string prefabPath = $"{VFX_PREFAB_PATH}/{effectId}.prefab";

                if (File.Exists(prefabPath))
                {
                    continue;
                }

                // Create VFX prefab
                GameObject vfxObj = new GameObject(effectId);

                // Add VFXInstance component (auto-adds ParticleSystem via RequireComponent)
                var vfxInstance = vfxObj.AddComponent<VFXInstance>();

                // Get the auto-added ParticleSystem
                var ps = vfxObj.GetComponent<ParticleSystem>();
                var main = ps.main;
                main.duration = 1f;
                main.loop = false;
                main.startLifetime = 0.5f;
                main.startSpeed = 2f;
                main.startSize = 0.3f;
                main.startColor = GetEffectColor(effectId);
                main.maxParticles = 50;
                main.playOnAwake = false;
                main.stopAction = ParticleSystemStopAction.Callback;

                var emission = ps.emission;
                emission.rateOverTime = 0;
                emission.SetBursts(new[] { new ParticleSystem.Burst(0f, 10) });

                var shape = ps.shape;
                shape.shapeType = ParticleSystemShapeType.Sphere;
                shape.radius = 0.2f;

                // VFXInstance will auto-find ParticleSystem via GetComponent in Awake

                // Save prefab
                bool success;
                PrefabUtility.SaveAsPrefabAsset(vfxObj, prefabPath, out success);
                Object.DestroyImmediate(vfxObj);

                if (success)
                {
                    created++;
                    Debug.Log($"[AudioVFXConfigGenerator] Created VFX prefab: {effectId}");
                }
            }

            AssetDatabase.Refresh();
            Debug.Log($"[AudioVFXConfigGenerator] Created {created} VFX prefabs");
        }

        public static void CreateVFXPoolManagerConfig()
        {
            // Find VFXPoolManager in scene or create one
            var poolManager = Object.FindAnyObjectByType<VFXPoolManager>();
            if (poolManager == null)
            {
                Debug.LogWarning("[AudioVFXConfigGenerator] No VFXPoolManager found in scene. Add one to configure.");
                return;
            }

            // Get all VFX prefabs
            string[] prefabGuids = AssetDatabase.FindAssets("t:Prefab", new[] { VFX_PREFAB_PATH });

            List<VFXPoolConfig> configs = new List<VFXPoolConfig>();

            foreach (string guid in prefabGuids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);

                if (prefab == null) continue;
                if (prefab.GetComponent<VFXInstance>() == null) continue;

                string effectId = Path.GetFileNameWithoutExtension(path);

                // Determine pre-warm and max based on effect type
                int preWarm = 3;
                int maxActive = 10;

                if (effectId.StartsWith("hit_"))
                {
                    preWarm = 5;
                    maxActive = 10;
                }
                else if (effectId.Contains("null") || effectId.Contains("requiem"))
                {
                    preWarm = 1;
                    maxActive = 2;
                }
                else if (effectId.Contains("shield") || effectId.Contains("heal"))
                {
                    preWarm = 2;
                    maxActive = 3;
                }

                configs.Add(new VFXPoolConfig
                {
                    EffectId = effectId,
                    Prefab = prefab,
                    PreWarmCount = preWarm,
                    MaxActive = maxActive
                });
            }

            // Apply to VFXPoolManager
            SerializedObject so = new SerializedObject(poolManager);
            var configsProp = so.FindProperty("_poolConfigs");
            configsProp.ClearArray();

            for (int i = 0; i < configs.Count; i++)
            {
                configsProp.InsertArrayElementAtIndex(i);
                var element = configsProp.GetArrayElementAtIndex(i);
                element.FindPropertyRelative("EffectId").stringValue = configs[i].EffectId;
                element.FindPropertyRelative("Prefab").objectReferenceValue = configs[i].Prefab;
                element.FindPropertyRelative("PreWarmCount").intValue = configs[i].PreWarmCount;
                element.FindPropertyRelative("MaxActive").intValue = configs[i].MaxActive;
            }

            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(poolManager);

            Debug.Log($"[AudioVFXConfigGenerator] Configured VFXPoolManager with {configs.Count} effects");
            EditorUtility.DisplayDialog("VFXPoolManager Configured",
                $"Added {configs.Count} VFX effect configurations to VFXPoolManager.",
                "OK");
        }

        // ============================================
        // Helper Methods
        // ============================================

        private static void PopulateVFXList(SerializedObject so, string propertyName,
            (string effectId, VFXCategory category, int preWarm, int maxActive, Color defaultColor)[] entries)
        {
            var listProp = so.FindProperty(propertyName);
            listProp.ClearArray();

            for (int i = 0; i < entries.Length; i++)
            {
                var (effectId, category, preWarm, maxActive, defaultColor) = entries[i];

                listProp.InsertArrayElementAtIndex(i);
                var element = listProp.GetArrayElementAtIndex(i);

                element.FindPropertyRelative("EffectId").stringValue = effectId;
                element.FindPropertyRelative("Category").enumValueIndex = (int)category;
                element.FindPropertyRelative("PreWarmCount").intValue = preWarm;
                element.FindPropertyRelative("MaxActive").intValue = maxActive;

                // Set default color
                var colorProp = element.FindPropertyRelative("DefaultColor");
                colorProp.colorValue = defaultColor;

                element.FindPropertyRelative("DefaultScale").floatValue = 1f;

                // Prefab must be assigned manually in the Inspector
            }
        }

        private static Color GetEffectColor(string effectId)
        {
            if (effectId.Contains("flame")) return new Color(1f, 0.5f, 0.2f);
            if (effectId.Contains("shadow")) return new Color(0.3f, 0.2f, 0.4f);
            if (effectId.Contains("nature")) return new Color(0.3f, 0.8f, 0.3f);
            if (effectId.Contains("arcane")) return new Color(0.6f, 0.3f, 0.9f);
            if (effectId.Contains("light")) return new Color(1f, 0.95f, 0.7f);
            if (effectId.Contains("heal")) return new Color(0.4f, 1f, 0.5f);
            if (effectId.Contains("shield")) return new Color(0.4f, 0.6f, 1f);
            if (effectId.Contains("corruption")) return new Color(0.5f, 0.1f, 0.3f);
            if (effectId.Contains("null")) return new Color(0.8f, 0.7f, 1f);
            return Color.white;
        }

        private static void EnsureDirectoryExists(string filePath)
        {
            string directory = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
                AssetDatabase.Refresh();
            }
        }

        // ============================================
        // CFXR Prefab Wiring
        // ============================================

        private const string CFXR_PATH = "Assets/ThirdParty/JMO Assets/Cartoon FX Remaster/CFXR Prefabs";

        /// <summary>
        /// Wires CFXR prefabs to VFXConfig.asset entries with optimal effect selections.
        /// </summary>
        public static void WireCFXRPrefabsToVFXConfig()
        {
            string assetPath = $"{CONFIG_PATH}/VFXConfig.asset";
            var config = AssetDatabase.LoadAssetAtPath<VFXConfigSO>(assetPath);

            if (config == null)
            {
                Debug.LogError($"[AudioVFXConfigGenerator] VFXConfig.asset not found at {assetPath}");
                return;
            }

            SerializedObject so = new SerializedObject(config);

            // Wire Hit Effects (Aspect-based damage impacts)
            WireVFXListWithCFXR(so, "_hitEntries", new Dictionary<string, (string prefabPath, Color color)>
            {
                { "hit_flame", ($"{CFXR_PATH}/Impacts/CFXR Hit A (Red).prefab", new Color(1f, 0.42f, 0.21f)) },
                { "hit_shadow", ($"{CFXR_PATH}/Impacts/CFXR Hit B 3D (Blue).prefab", new Color(0.29f, 0.05f, 0.31f)) },
                { "hit_nature", ($"{CFXR_PATH}/Impacts/Variants/CFXR Hit B 3D (Green).prefab", new Color(0.18f, 0.35f, 0.15f)) },
                { "hit_arcane", ($"{CFXR_PATH}/Electric/CFXR Lightning Impact.prefab", new Color(0.36f, 0.17f, 0.44f)) },
                { "hit_light", ($"{CFXR_PATH}/Impacts/CFXR Hit D 3D (Yellow).prefab", new Color(0.96f, 0.82f, 0.25f)) },
            });

            // Wire Combat Effects
            WireVFXListWithCFXR(so, "_combatEntries", new Dictionary<string, (string prefabPath, Color color)>
            {
                { "vfx_slash", ($"{CFXR_PATH}/Impacts/Variants/CFXR Slash (Cross, Blue).prefab", Color.white) },
                { "vfx_shield", ($"{CFXR_PATH}/Electric/CFXR Electric Barrier (HDR).prefab", new Color(0f, 0.83f, 0.89f)) },
                { "vfx_heal", ($"{CFXR_PATH}/Misc/CFXR Magical Source.prefab", new Color(0.18f, 0.55f, 0.18f)) },
            });

            // Wire Status Effects
            WireVFXListWithCFXR(so, "_statusEntries", new Dictionary<string, (string prefabPath, Color color)>
            {
                { "vfx_buff", ($"{CFXR_PATH}/Electric/Variants/CFXR Electrified 1 (Green).prefab", new Color(0.5f, 1f, 0.5f)) },
                { "vfx_debuff", ($"{CFXR_PATH}/Electric/Variants/CFXR Electrified 1 (Purple).prefab", new Color(0.8f, 0.3f, 0.3f)) },
                { "vfx_corruption", ($"{CFXR_PATH}/Misc/CFXR Portal.prefab", new Color(0.5f, 0.1f, 0.3f)) },
            });

            // Wire Special Effects
            WireVFXListWithCFXR(so, "_specialEntries", new Dictionary<string, (string prefabPath, Color color)>
            {
                { "vfx_null_burst", ($"{CFXR_PATH}/Explosions/CFXR Explosion 3 Bigger.prefab", new Color(0.8f, 0.5f, 1f)) },
                { "vfx_requiem_art", ($"{CFXR_PATH}/Explosions/CFXR Explosion 3 Bigger.prefab", Color.white) },
                { "vfx_requiem_art_kira", ($"{CFXR_PATH}/Fire/CFXR Fire Breath.prefab", new Color(1f, 0.5f, 0.2f)) },
                { "vfx_requiem_art_mordren", ($"{CFXR_PATH}/Electric/Variants/CFXR Electric Barrier Simple (HDR, Purple).prefab", new Color(0.29f, 0.05f, 0.31f)) },
                { "vfx_requiem_art_elara", ($"{CFXR_PATH}/Misc/CFXR Flash.prefab", new Color(1f, 0.95f, 0.7f)) },
                { "vfx_requiem_art_thornwick", ($"{CFXR_PATH}/Misc/CFXR Magical Source.prefab", new Color(0.4f, 0.7f, 0.4f)) },
            });

            // Wire Card Effects
            WireVFXListWithCFXR(so, "_cardEntries", new Dictionary<string, (string prefabPath, Color color)>
            {
                { "vfx_card_draw", ($"{CFXR_PATH}/Misc/CFXR Magic Poof.prefab", new Color(0.7f, 0.9f, 1f)) },
                { "vfx_card_play", ($"{CFXR_PATH}/Impacts/Variants/CFXR Impact Contrast (HDR).prefab", Color.white) },
            });

            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(config);
            AssetDatabase.SaveAssets();

            Debug.Log("[AudioVFXConfigGenerator] Successfully wired CFXR prefabs to VFXConfig.asset");
            EditorUtility.DisplayDialog("VFX Config Updated",
                "Successfully wired CFXR prefabs to VFXConfig.asset!\n\n" +
                "Hit Effects: 5 entries\n" +
                "Combat Effects: 3 entries\n" +
                "Status Effects: 3 entries\n" +
                "Special Effects: 6 entries\n" +
                "Card Effects: 2 entries",
                "OK");
        }

        private static void WireVFXListWithCFXR(SerializedObject so, string listPropertyName,
            Dictionary<string, (string prefabPath, Color color)> effectMappings)
        {
            var listProp = so.FindProperty(listPropertyName);

            for (int i = 0; i < listProp.arraySize; i++)
            {
                var element = listProp.GetArrayElementAtIndex(i);
                string effectId = element.FindPropertyRelative("EffectId").stringValue;

                if (effectMappings.TryGetValue(effectId, out var mapping))
                {
                    // Load prefab
                    var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(mapping.prefabPath);
                    if (prefab != null)
                    {
                        element.FindPropertyRelative("Prefab").objectReferenceValue = prefab;
                        element.FindPropertyRelative("DefaultColor").colorValue = mapping.color;
                        Debug.Log($"[AudioVFXConfigGenerator] Wired {effectId} -> {mapping.prefabPath}");
                    }
                    else
                    {
                        Debug.LogWarning($"[AudioVFXConfigGenerator] Prefab not found: {mapping.prefabPath}");
                    }
                }
            }
        }
    }
}
