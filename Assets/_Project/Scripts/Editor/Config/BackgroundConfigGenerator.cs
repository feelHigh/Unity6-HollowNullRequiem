// ============================================
// BackgroundConfigGenerator.cs
// Editor tool to generate BackgroundConfig asset
// ============================================

using UnityEngine;
using UnityEditor;
using HNR.UI.Config;

namespace HNR.Editor.Config
{
    /// <summary>
    /// Editor utility to generate and populate BackgroundConfig asset.
    /// </summary>
    public static class BackgroundConfigGenerator
    {
        private const string ConfigPath = "Assets/_Project/Data/Config/BackgroundConfig.asset";

        [MenuItem("HNR/5. Utilities/Config/Generate Background Config", priority = 231)]
        public static void GenerateBackgroundConfig()
        {
            // Check if asset already exists
            var existing = AssetDatabase.LoadAssetAtPath<BackgroundConfigSO>(ConfigPath);
            if (existing != null)
            {
                Debug.Log("[BackgroundConfigGenerator] BackgroundConfig.asset already exists at " + ConfigPath);
                Selection.activeObject = existing;
                EditorGUIUtility.PingObject(existing);
                return;
            }

            // Create new config
            var config = ScriptableObject.CreateInstance<BackgroundConfigSO>();

            // Ensure directory exists
            var dir = System.IO.Path.GetDirectoryName(ConfigPath);
            if (!AssetDatabase.IsValidFolder(dir))
            {
                System.IO.Directory.CreateDirectory(dir);
                AssetDatabase.Refresh();
            }

            // Save asset
            AssetDatabase.CreateAsset(config, ConfigPath);
            AssetDatabase.SaveAssets();

            Debug.Log("[BackgroundConfigGenerator] BackgroundConfig.asset created successfully at " + ConfigPath);
            Debug.Log("[BackgroundConfigGenerator] To populate with sprites, use 'HNR > 1. Data Assets > Backgrounds > Link Background Art' after generating background images.");

            // Select in project
            Selection.activeObject = config;
            EditorGUIUtility.PingObject(config);
        }

        /// <summary>
        /// Validates the BackgroundConfig and reports any missing sprites.
        /// </summary>
        [MenuItem("HNR/5. Utilities/Config/Validate Background Config", priority = 232)]
        public static void ValidateBackgroundConfig()
        {
            var config = AssetDatabase.LoadAssetAtPath<BackgroundConfigSO>(ConfigPath);
            if (config == null)
            {
                Debug.LogWarning("[BackgroundConfigGenerator] BackgroundConfig.asset not found. Run 'Generate Background Config' first.");
                return;
            }

            int assigned = config.GetAssignedCount();
            int total = 13; // 5 scenes + 3 zones + 3 combat + 2 screens

            Debug.Log($"=== BackgroundConfig Validation ===");
            Debug.Log($"Assigned backgrounds: {assigned}/{total}");

            if (!config.HasAllSceneBackgrounds())
            {
                Debug.LogWarning("Missing scene backgrounds (MainMenu, Bastion, Missions, BattleMission, Requiems)");
            }
            else
            {
                Debug.Log("All scene backgrounds assigned");
            }

            if (!config.HasAllZoneBackgrounds())
            {
                Debug.LogWarning("Missing zone backgrounds (Zone1, Zone2, Zone3)");
            }
            else
            {
                Debug.Log("All zone backgrounds assigned");
            }

            if (!config.HasAllCombatBackgrounds())
            {
                Debug.LogWarning("Missing combat backgrounds (Normal, Elite, Boss)");
            }
            else
            {
                Debug.Log("All combat backgrounds assigned");
            }

            if (!config.HasAllScreenBackgrounds())
            {
                Debug.LogWarning("Missing screen backgrounds (Sanctuary, Shop)");
            }
            else
            {
                Debug.Log("All screen backgrounds assigned");
            }
        }
    }
}
