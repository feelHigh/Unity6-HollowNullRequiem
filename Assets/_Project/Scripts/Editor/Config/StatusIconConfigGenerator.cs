// ============================================
// StatusIconConfigGenerator.cs
// Editor tool to generate StatusIconConfig asset
// ============================================

using UnityEngine;
using UnityEditor;
using System.IO;

namespace HNR.Editor.Config
{
    /// <summary>
    /// Editor tool to generate and configure StatusIconConfigSO asset.
    /// </summary>
    public static class StatusIconConfigGenerator
    {
        private const string CONFIG_PATH = "Assets/_Project/Resources/Config/StatusIconConfig.asset";
        private const string LAYERLAB_ICONS_PATH = "Assets/ThirdParty/Layer Lab/GUI Pro-FantasyHero/ResourcesData/Sptites/Components/Icon_PictoIcons/128";

        // Icon mapping: StatusType field name -> LayerLab icon filename
        private static readonly (string fieldName, string iconFile)[] IconMappings = new[]
        {
            // Damage Over Time
            ("_burnIcon", "PictoIcon_Fire.Png"),
            ("_poisonIcon", "PictoIcon_Skull.Png"),

            // Debuffs
            ("_weaknessIcon", "PictoIcon_Defense.Png"),
            ("_vulnerabilityIcon", "PictoIcon_Thunder.Png"),
            ("_stunIcon", "PictoIcon_Snowflake.Png"),
            ("_dazedIcon", "PictoIcon_Timer.Png"),
            ("_markedIcon", "PictoIcon_Location.Png"),

            // Buffs
            ("_strengthIcon", "PictoIcon_Attack_Power.Png"),
            ("_dexterityIcon", "PictoIcon_Boots.Png"),
            ("_regenerationIcon", "PictoIcon_Health.Png"),
            ("_energizedIcon", "PictoIcon_Power.Png"),

            // Special
            ("_shieldedIcon", "PictoIcon_Shield.Png"),
            ("_thornsIcon", "PictoIcon_Pick.Png"),
            ("_drawBonusIcon", "PictoIcon_Card.Png"),
            ("_protectedIcon", "PictoIcon_Armor.Png"),
            ("_ritualIcon", "PictoIcon_Star.Png"),

            // Fallback
            ("_defaultIcon", "PictoIcon_Help.Png")
        };

        [MenuItem("HNR/5. Utilities/Config/Generate Status Icon Config", priority = 550)]
        public static void GenerateConfig()
        {
            // Ensure Resources/Config directory exists
            var resourcesConfigPath = "Assets/_Project/Resources/Config";
            if (!Directory.Exists(resourcesConfigPath))
            {
                Directory.CreateDirectory(resourcesConfigPath);
                AssetDatabase.Refresh();
            }

            // Check if config already exists
            var existingConfig = AssetDatabase.LoadAssetAtPath<HNR.UI.Config.StatusIconConfigSO>(CONFIG_PATH);
            if (existingConfig != null)
            {
                if (!EditorUtility.DisplayDialog(
                    "Status Icon Config Exists",
                    "StatusIconConfig already exists. Do you want to overwrite it with fresh icon assignments?",
                    "Overwrite",
                    "Cancel"))
                {
                    return;
                }
            }

            // Create new config
            var config = ScriptableObject.CreateInstance<HNR.UI.Config.StatusIconConfigSO>();

            // Assign icons using SerializedObject
            int assignedCount = 0;
            int totalCount = IconMappings.Length;

            foreach (var (fieldName, iconFile) in IconMappings)
            {
                var iconPath = $"{LAYERLAB_ICONS_PATH}/{iconFile}";
                var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(iconPath);

                if (sprite != null)
                {
                    // Use reflection to set private field
                    var field = typeof(HNR.UI.Config.StatusIconConfigSO).GetField(
                        fieldName,
                        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

                    if (field != null)
                    {
                        field.SetValue(config, sprite);
                        assignedCount++;
                        Debug.Log($"[StatusIconConfigGenerator] Assigned {fieldName} = {iconFile}");
                    }
                    else
                    {
                        Debug.LogWarning($"[StatusIconConfigGenerator] Field not found: {fieldName}");
                    }
                }
                else
                {
                    Debug.LogWarning($"[StatusIconConfigGenerator] Icon not found: {iconPath}");
                }
            }

            // Save config
            if (existingConfig != null)
            {
                // Copy values to existing asset
                EditorUtility.CopySerialized(config, existingConfig);
                EditorUtility.SetDirty(existingConfig);
                Object.DestroyImmediate(config);
            }
            else
            {
                AssetDatabase.CreateAsset(config, CONFIG_PATH);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            // Clear cache so Instance reloads
            HNR.UI.Config.StatusIconConfigSO.ClearCache();

            Debug.Log($"[StatusIconConfigGenerator] StatusIconConfig generated at {CONFIG_PATH}");
            Debug.Log($"[StatusIconConfigGenerator] Assigned {assignedCount}/{totalCount} icons");

            // Select the created asset
            var createdConfig = AssetDatabase.LoadAssetAtPath<HNR.UI.Config.StatusIconConfigSO>(CONFIG_PATH);
            if (createdConfig != null)
            {
                Selection.activeObject = createdConfig;
                EditorGUIUtility.PingObject(createdConfig);
            }
        }

        [MenuItem("HNR/5. Utilities/Config/Validate Status Icon Config", priority = 551)]
        public static void ValidateConfig()
        {
            var config = AssetDatabase.LoadAssetAtPath<HNR.UI.Config.StatusIconConfigSO>(CONFIG_PATH);

            if (config == null)
            {
                Debug.LogError("[StatusIconConfigGenerator] StatusIconConfig not found. Run 'Generate Status Icon Config' first.");
                return;
            }

            var (assigned, total) = config.GetAssignmentStatus();
            bool isValid = config.IsValid();

            if (isValid)
            {
                Debug.Log($"[StatusIconConfigGenerator] StatusIconConfig is valid: {assigned}/{total} icons assigned");
            }
            else
            {
                Debug.LogWarning($"[StatusIconConfigGenerator] StatusIconConfig is incomplete: {assigned}/{total} icons assigned");

                // List missing icons
                var fields = new[]
                {
                    ("Burn", config.BurnIcon),
                    ("Poison", config.PoisonIcon),
                    ("Weakness", config.WeaknessIcon),
                    ("Vulnerability", config.VulnerabilityIcon),
                    ("Stun", config.StunIcon),
                    ("Strength", config.StrengthIcon),
                    ("Regeneration", config.RegenerationIcon),
                    ("Shielded", config.ShieldedIcon),
                    ("Protected", config.ProtectedIcon)
                };

                foreach (var (name, sprite) in fields)
                {
                    if (sprite == null)
                    {
                        Debug.LogWarning($"  - Missing: {name}Icon");
                    }
                }
            }

            Selection.activeObject = config;
        }
    }
}
