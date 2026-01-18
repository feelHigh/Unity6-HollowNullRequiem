// ============================================
// CombatConfigGenerator.cs
// Editor tool to generate CombatConfig asset
// ============================================

using UnityEngine;
using UnityEditor;
using HNR.Combat;

namespace HNR.Editor.Config
{
    /// <summary>
    /// Editor utility to generate CombatConfig asset.
    /// </summary>
    public static class CombatConfigGenerator
    {
        private const string ConfigPath = "Assets/_Project/Data/Config/CombatConfig.asset";

        [MenuItem("HNR/5. Utilities/Config/Generate Combat Config", priority = 233)]
        public static void GenerateCombatConfig()
        {
            // Check if asset already exists
            var existing = AssetDatabase.LoadAssetAtPath<CombatConfigSO>(ConfigPath);
            if (existing != null)
            {
                Debug.Log("[CombatConfigGenerator] CombatConfig.asset already exists at " + ConfigPath);
                Selection.activeObject = existing;
                EditorGUIUtility.PingObject(existing);
                return;
            }

            // Create new config
            var config = ScriptableObject.CreateInstance<CombatConfigSO>();

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

            Debug.Log("[CombatConfigGenerator] CombatConfig.asset created successfully at " + ConfigPath);
            Debug.Log("[CombatConfigGenerator] Configure damage corruption settings in the Inspector.");

            // Select in project
            Selection.activeObject = config;
            EditorGUIUtility.PingObject(config);
        }
    }
}
