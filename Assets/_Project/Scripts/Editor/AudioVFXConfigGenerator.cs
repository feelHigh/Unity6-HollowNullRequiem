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
        // Menu Items
        // ============================================

        [MenuItem("HNR/Audio & VFX/Generate All Configs", priority = 70)]
        public static void GenerateAllConfigs()
        {
            GenerateAudioConfig();
            GenerateVFXPrefabs();

            EditorUtility.DisplayDialog("Audio & VFX Config Generated",
                "Created:\n" +
                "- AudioConfig.asset with placeholder entries\n" +
                "- VFX prefabs with VFXInstance components\n\n" +
                "Assign actual AudioClips and ParticleSystems to complete setup.",
                "OK");
        }

        [MenuItem("HNR/Audio & VFX/1. Generate Audio Config", priority = 71)]
        public static void GenerateAudioConfig()
        {
            EnsureDirectoryExists($"{CONFIG_PATH}/placeholder.asset");

            string assetPath = $"{CONFIG_PATH}/AudioConfig.asset";

            // Check if already exists
            var existing = AssetDatabase.LoadAssetAtPath<AudioConfigSO>(assetPath);
            if (existing != null)
            {
                if (!EditorUtility.DisplayDialog("AudioConfig Exists",
                    "AudioConfig.asset already exists. Overwrite?",
                    "Yes", "No"))
                {
                    return;
                }
                AssetDatabase.DeleteAsset(assetPath);
            }

            // Create new AudioConfigSO
            var config = ScriptableObject.CreateInstance<AudioConfigSO>();

            // Use SerializedObject to populate lists
            SerializedObject so = new SerializedObject(config);

            // Music entries
            PopulateAudioList(so, "_musicEntries", new[]
            {
                ("menu_theme", AudioCategory.Music, true),
                ("bastion_theme", AudioCategory.Music, true),
                ("combat_theme", AudioCategory.Music, true),
                ("boss_theme", AudioCategory.Music, true),
                ("victory_theme", AudioCategory.Music, false),
                ("defeat_theme", AudioCategory.Music, false)
            });

            // UI entries
            PopulateAudioList(so, "_uiEntries", new[]
            {
                ("ui_click", AudioCategory.UI, false),
                ("ui_hover", AudioCategory.UI, false),
                ("ui_confirm", AudioCategory.UI, false),
                ("ui_cancel", AudioCategory.UI, false),
                ("ui_error", AudioCategory.UI, false),
                ("ui_navigate", AudioCategory.UI, false)
            });

            // Combat entries
            PopulateAudioList(so, "_combatEntries", new[]
            {
                ("card_draw", AudioCategory.Combat, false),
                ("card_play", AudioCategory.Combat, false),
                ("card_discard", AudioCategory.Combat, false),
                ("damage_hit", AudioCategory.Combat, false),
                ("damage_critical", AudioCategory.Combat, false),
                ("block_gain", AudioCategory.Combat, false),
                ("block_break", AudioCategory.Combat, false),
                ("heal", AudioCategory.Combat, false),
                ("buff_apply", AudioCategory.Combat, false),
                ("debuff_apply", AudioCategory.Combat, false),
                ("turn_start", AudioCategory.Combat, false),
                ("turn_end", AudioCategory.Combat, false),
                ("enemy_attack", AudioCategory.Combat, false),
                ("enemy_die", AudioCategory.Combat, false),
                ("requiem_art", AudioCategory.Combat, false)
            });

            // Ambient entries
            PopulateAudioList(so, "_ambientEntries", new[]
            {
                ("corruption_pulse", AudioCategory.Ambient, false),
                ("null_state_trigger", AudioCategory.Ambient, false),
                ("void_ambient", AudioCategory.Ambient, true),
                ("combat_ambient", AudioCategory.Ambient, true)
            });

            so.ApplyModifiedPropertiesWithoutUndo();

            // Save asset
            AssetDatabase.CreateAsset(config, assetPath);
            AssetDatabase.SaveAssets();

            Debug.Log($"[AudioVFXConfigGenerator] Created AudioConfig at {assetPath}");
        }

        [MenuItem("HNR/Audio & VFX/2. Generate VFX Prefabs", priority = 72)]
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

                // Add VFXInstance component
                var vfxInstance = vfxObj.AddComponent<VFXInstance>();

                // Add placeholder ParticleSystem
                var ps = vfxObj.AddComponent<ParticleSystem>();
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

        [MenuItem("HNR/Audio & VFX/3. Create VFXPoolManager Config", priority = 73)]
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

        private static void PopulateAudioList(SerializedObject so, string propertyName,
            (string id, AudioCategory category, bool loop)[] entries)
        {
            var listProp = so.FindProperty(propertyName);
            listProp.ClearArray();

            for (int i = 0; i < entries.Length; i++)
            {
                var (id, category, loop) = entries[i];

                listProp.InsertArrayElementAtIndex(i);
                var element = listProp.GetArrayElementAtIndex(i);

                element.FindPropertyRelative("Id").stringValue = id;
                element.FindPropertyRelative("Category").enumValueIndex = (int)category;
                element.FindPropertyRelative("Volume").floatValue = 1f;
                element.FindPropertyRelative("Pitch").floatValue = 1f;
                element.FindPropertyRelative("Loop").boolValue = loop;
                // Clip remains null - to be assigned manually
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
    }
}
