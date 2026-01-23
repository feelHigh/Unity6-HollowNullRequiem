// ============================================
// BannerConfigGenerator.cs
// Editor tool to generate banner configuration asset
// ============================================

using UnityEngine;
using UnityEditor;
using System.IO;
using HNR.UI.Config;

namespace HNR.Editor
{
    /// <summary>
    /// Editor tool for generating and managing BannerConfigSO assets.
    /// </summary>
    public static class BannerConfigGenerator
    {
        private const string CONFIG_PATH = "Assets/_Project/Data/Config/BannerConfig.asset";
        private const string BANNERS_ART_PATH = "Assets/_Project/Art/Banners";

        /// <summary>
        /// Generates a new BannerConfigSO asset with default placeholder banners.
        /// </summary>
        public static void GenerateBannerConfig()
        {
            // Check if asset already exists
            var existing = AssetDatabase.LoadAssetAtPath<BannerConfigSO>(CONFIG_PATH);
            if (existing != null)
            {
                if (!EditorUtility.DisplayDialog("Banner Config Exists",
                    "BannerConfig.asset already exists. Do you want to regenerate it?\n\n" +
                    "This will overwrite the existing configuration.",
                    "Regenerate", "Cancel"))
                {
                    Selection.activeObject = existing;
                    EditorGUIUtility.PingObject(existing);
                    return;
                }
            }

            // Ensure directory exists
            string directory = Path.GetDirectoryName(CONFIG_PATH);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            // Ensure banner art directory exists
            if (!Directory.Exists(BANNERS_ART_PATH))
            {
                Directory.CreateDirectory(BANNERS_ART_PATH);
                AssetDatabase.Refresh();
            }

            // Create new config
            var config = ScriptableObject.CreateInstance<BannerConfigSO>();

            // Set default banners using SerializedObject
            SerializedObject so = new SerializedObject(config);

            // Create banner array with 5 placeholder banners
            SerializedProperty bannersArray = so.FindProperty("_banners");
            bannersArray.ClearArray();

            // Add placeholder banners
            AddPlaceholderBanner(bannersArray, 0, "Welcome Event", "Welcome to Hollow Null Requiem! Tap for rewards.");
            AddPlaceholderBanner(bannersArray, 1, "Void Festival", "Limited time event - Earn double Void Shards!");
            AddPlaceholderBanner(bannersArray, 2, "Kira Spotlight", "New Requiem Art revealed - Blazing Inferno!");
            AddPlaceholderBanner(bannersArray, 3, "New Content", "Zone 4 coming soon - The Abyss awaits.");
            AddPlaceholderBanner(bannersArray, 4, "Boss Challenge", "Elite Boss Challenge - Test your strength!");

            // Set default behavior values
            so.FindProperty("_autoAdvanceInterval").floatValue = 2f;
            so.FindProperty("_transitionDuration").floatValue = 0.3f;
            so.FindProperty("_enableLoop").boolValue = true;
            so.FindProperty("_pauseOnInteraction").boolValue = true;
            so.FindProperty("_resumeDelay").floatValue = 1.5f;

            // Set default visual values
            so.FindProperty("_activeIndicatorColor").colorValue = Color.white;
            so.FindProperty("_inactiveIndicatorColor").colorValue = new Color(1f, 1f, 1f, 0.4f);
            so.FindProperty("_indicatorSize").floatValue = 12f;
            so.FindProperty("_indicatorSpacing").floatValue = 8f;

            so.ApplyModifiedPropertiesWithoutUndo();

            // Save asset
            if (existing != null)
            {
                AssetDatabase.DeleteAsset(CONFIG_PATH);
            }
            AssetDatabase.CreateAsset(config, CONFIG_PATH);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            // Select and ping asset
            Selection.activeObject = config;
            EditorGUIUtility.PingObject(config);

            Debug.Log($"[BannerConfigGenerator] Created BannerConfig with 5 placeholder banners at {CONFIG_PATH}");
            Debug.Log($"[BannerConfigGenerator] Banner art directory: {BANNERS_ART_PATH}");
            Debug.Log("[BannerConfigGenerator] Add banner images to the config asset in the Inspector");
        }

        private static void AddPlaceholderBanner(SerializedProperty array, int priority, string title, string description)
        {
            array.InsertArrayElementAtIndex(array.arraySize);
            SerializedProperty element = array.GetArrayElementAtIndex(array.arraySize - 1);

            element.FindPropertyRelative("_image").objectReferenceValue = null;
            element.FindPropertyRelative("_title").stringValue = title;
            element.FindPropertyRelative("_description").stringValue = description;
            element.FindPropertyRelative("_isActive").boolValue = true;
            element.FindPropertyRelative("_priority").intValue = priority;
            element.FindPropertyRelative("_actionType").enumValueIndex = 0; // None
            element.FindPropertyRelative("_actionParameter").stringValue = "";
            element.FindPropertyRelative("_startDate").stringValue = "";
            element.FindPropertyRelative("_endDate").stringValue = "";
        }

        /// <summary>
        /// Verifies the banner configuration.
        /// </summary>
        public static void VerifyBannerConfig()
        {
            var config = AssetDatabase.LoadAssetAtPath<BannerConfigSO>(CONFIG_PATH);

            if (config == null)
            {
                Debug.LogWarning("[BannerConfigGenerator] BannerConfig not found. Run 'Generate Banner Config' first.");
                return;
            }

            bool isValid = config.Validate();

            Debug.Log($"[BannerConfigGenerator] Banner Config Verification:");
            Debug.Log($"  - Total banners: {config.BannerCount}");
            Debug.Log($"  - Active banners: {config.GetActiveCount()}");
            Debug.Log($"  - Banners with images: {config.GetBannersWithImageCount()}");
            Debug.Log($"  - Auto-advance enabled: {config.IsAutoAdvanceEnabled()} ({config.AutoAdvanceInterval}s)");
            Debug.Log($"  - Loop enabled: {config.EnableLoop}");
            Debug.Log($"  - Configuration valid: {isValid}");

            Selection.activeObject = config;
            EditorGUIUtility.PingObject(config);
        }

        /// <summary>
        /// Attempts to auto-link banner images from the art directory.
        /// </summary>
        public static void LinkBannerImages()
        {
            var config = AssetDatabase.LoadAssetAtPath<BannerConfigSO>(CONFIG_PATH);

            if (config == null)
            {
                Debug.LogWarning("[BannerConfigGenerator] BannerConfig not found. Run 'Generate Banner Config' first.");
                return;
            }

            // Find all sprites in banner art directory
            string[] guids = AssetDatabase.FindAssets("t:Sprite", new[] { BANNERS_ART_PATH });

            if (guids.Length == 0)
            {
                Debug.LogWarning($"[BannerConfigGenerator] No sprites found in {BANNERS_ART_PATH}");
                return;
            }

            SerializedObject so = new SerializedObject(config);
            SerializedProperty bannersArray = so.FindProperty("_banners");

            int linked = 0;
            for (int i = 0; i < Mathf.Min(guids.Length, bannersArray.arraySize); i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);

                if (sprite != null)
                {
                    SerializedProperty element = bannersArray.GetArrayElementAtIndex(i);
                    element.FindPropertyRelative("_image").objectReferenceValue = sprite;
                    linked++;
                }
            }

            so.ApplyModifiedPropertiesWithoutUndo();
            AssetDatabase.SaveAssets();

            Debug.Log($"[BannerConfigGenerator] Linked {linked} banner images");

            Selection.activeObject = config;
            EditorGUIUtility.PingObject(config);
        }
    }
}
