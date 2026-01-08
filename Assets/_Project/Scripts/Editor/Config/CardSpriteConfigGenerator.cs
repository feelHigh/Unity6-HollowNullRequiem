// ============================================
// CardSpriteConfigGenerator.cs
// Editor tool to generate CardSpriteConfig asset with auto-populated sprites
// ============================================

using UnityEngine;
using UnityEditor;
using HNR.UI.Config;

namespace HNR.Editor.Config
{
    /// <summary>
    /// Editor utility to generate and populate CardSpriteConfig asset.
    /// </summary>
    public static class CardSpriteConfigGenerator
    {
        private const string ConfigPath = "Assets/_Project/Data/Config/CardSpriteConfig.asset";
        private const string FrameSpritePath = "Assets/ThirdParty/Layer Lab/GUI Pro-FantasyHero/ResourcesData/Sptites/Components/Frame";
        private const string LabelSpritePath = "Assets/ThirdParty/Layer Lab/GUI Pro-FantasyHero/ResourcesData/Sptites/Components/Label";

        [MenuItem("HNR/Config/Generate Card Sprite Config", priority = 200)]
        public static void GenerateCardSpriteConfig()
        {
            // Check if asset already exists
            var existing = AssetDatabase.LoadAssetAtPath<CardSpriteConfigSO>(ConfigPath);
            if (existing != null)
            {
                if (!EditorUtility.DisplayDialog(
                    "Card Sprite Config Exists",
                    "CardSpriteConfig.asset already exists. Do you want to regenerate it? This will overwrite existing settings.",
                    "Regenerate", "Cancel"))
                {
                    return;
                }
                AssetDatabase.DeleteAsset(ConfigPath);
            }

            // Create new config
            var config = ScriptableObject.CreateInstance<CardSpriteConfigSO>();

            // Use SerializedObject for proper editing
            var so = new SerializedObject(config);

            // Populate frame sets
            PopulateFrameSet(so, "_strikeFrames", "Red", new Color(0.8f, 0.2f, 0.2f));
            PopulateFrameSet(so, "_guardFrames", "Blue", new Color(0.2f, 0.4f, 0.8f));
            PopulateFrameSet(so, "_skillFrames", "Green", new Color(0.2f, 0.7f, 0.3f));
            PopulateFrameSet(so, "_powerFrames", "Purple", new Color(0.6f, 0.2f, 0.7f));
            PopulateFrameSet(so, "_specialFrames", "Yellow", new Color(0.8f, 0.8f, 0.2f));

            // Populate cost frame sprites
            PopulateCostFrameSprites(so);

            so.ApplyModifiedPropertiesWithoutUndo();

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

            // Validate
            var issues = config.Validate();
            if (issues.Count > 0)
            {
                Debug.LogWarning($"[CardSpriteConfigGenerator] Config created with {issues.Count} missing sprites:");
                foreach (var issue in issues)
                {
                    Debug.LogWarning($"  - {issue}");
                }
            }
            else
            {
                Debug.Log("[CardSpriteConfigGenerator] CardSpriteConfig.asset created successfully with all sprites assigned!");
            }

            // Select in project
            Selection.activeObject = config;
            EditorGUIUtility.PingObject(config);
        }

        private static void PopulateFrameSet(SerializedObject so, string fieldName, string color, Color tintColor)
        {
            var frameProp = so.FindProperty(fieldName);

            // Background
            var bgSprite = LoadFrameSprite($"CardFrame_Rectangle_01_{color}_Bg");
            frameProp.FindPropertyRelative("Background").objectReferenceValue = bgSprite;

            // Border
            var borderSprite = LoadFrameSprite($"CardFrame_Rectangle_01_{color}_Border");
            frameProp.FindPropertyRelative("Border").objectReferenceValue = borderSprite;

            // BorderGem
            var borderGemSprite = LoadFrameSprite($"CardFrame_Rectangle_01_{color}_BorderGem");
            frameProp.FindPropertyRelative("BorderGem").objectReferenceValue = borderGemSprite;

            // TintColor
            frameProp.FindPropertyRelative("TintColor").colorValue = tintColor;
        }

        private static Sprite LoadFrameSprite(string spriteName)
        {
            var path = $"{FrameSpritePath}/{spriteName}.png";
            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (sprite == null)
            {
                Debug.LogWarning($"[CardSpriteConfigGenerator] Could not find sprite: {path}");
            }
            return sprite;
        }

        private static void PopulateCostFrameSprites(SerializedObject so)
        {
            so.FindProperty("_costFrameBg").objectReferenceValue = LoadLabelSprite("Label_Flag_01_Bg");
            so.FindProperty("_costFrameBorder").objectReferenceValue = LoadLabelSprite("Label_Flag_01_Border");
            so.FindProperty("_costFrameGradient").objectReferenceValue = LoadLabelSprite("Label_Flag_01_Gradient");
            so.FindProperty("_costFrameInnerBorder").objectReferenceValue = LoadLabelSprite("Label_Flag_01_InnerBorder");
        }

        private static Sprite LoadLabelSprite(string spriteName)
        {
            var path = $"{LabelSpritePath}/{spriteName}.png";
            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (sprite == null)
            {
                Debug.LogWarning($"[CardSpriteConfigGenerator] Could not find sprite: {path}");
            }
            return sprite;
        }

        [MenuItem("HNR/Config/Validate Card Sprite Config", priority = 201)]
        public static void ValidateCardSpriteConfig()
        {
            var config = AssetDatabase.LoadAssetAtPath<CardSpriteConfigSO>(ConfigPath);
            if (config == null)
            {
                Debug.LogError("[CardSpriteConfigGenerator] CardSpriteConfig.asset not found. Use 'Generate Card Sprite Config' first.");
                return;
            }

            var issues = config.Validate();
            if (issues.Count == 0)
            {
                Debug.Log("[CardSpriteConfigGenerator] CardSpriteConfig.asset is valid - all sprites assigned!");
            }
            else
            {
                Debug.LogWarning($"[CardSpriteConfigGenerator] CardSpriteConfig.asset has {issues.Count} issues:");
                foreach (var issue in issues)
                {
                    Debug.LogWarning($"  - {issue}");
                }
            }

            Selection.activeObject = config;
        }
    }
}
