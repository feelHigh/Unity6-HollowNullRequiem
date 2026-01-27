// ============================================
// LayerLabSpriteConfigGenerator.cs
// Editor tool to generate and populate LayerLab sprite config
// ============================================

using UnityEngine;
using UnityEditor;
using HNR.UI.Config;
using System.IO;
using TMPro;

namespace HNR.Editor.Config
{
    /// <summary>
    /// Editor utility to generate and populate LayerLabSpriteConfig asset.
    /// Automatically discovers and assigns sprites from LayerLab GUI Pro-FantasyHero package.
    /// </summary>
    public static class LayerLabSpriteConfigGenerator
    {
        private const string ConfigPath = "Assets/_Project/Data/Config/LayerLabSpriteConfig.asset";

        // LayerLab asset paths (Note: "Sptites" is a typo in the original LayerLab folder name)
        private const string LayerLabRoot = "Assets/ThirdParty/Layer Lab/GUI Pro-FantasyHero";
        private const string SpritesRoot = LayerLabRoot + "/ResourcesData/Sptites/Components";
        private const string ButtonSpritesPath = SpritesRoot + "/Button";
        private const string FrameSpritesPath = SpritesRoot + "/Frame";
        private const string IconSpritesPath = LayerLabRoot + "/ResourcesData/Sptites/Demo/Demo_IconMisc";
        private const string FontsPath = LayerLabRoot + "/ResourcesData/Fonts";
        private const string AfacadFluxFontPath = FontsPath + "/AfacadFlux-ExtraBold SDF.asset";

        [MenuItem("HNR/5. Utilities/Config/Generate LayerLab Sprite Config", priority = 233)]
        public static void GenerateLayerLabSpriteConfig()
        {
            // Check if asset already exists
            var existing = AssetDatabase.LoadAssetAtPath<LayerLabSpriteConfigSO>(ConfigPath);
            if (existing != null)
            {
                if (!EditorUtility.DisplayDialog("LayerLab Sprite Config Exists",
                    "LayerLabSpriteConfig.asset already exists.\n\nWould you like to re-populate it with sprites?",
                    "Re-populate", "Cancel"))
                {
                    Selection.activeObject = existing;
                    EditorGUIUtility.PingObject(existing);
                    return;
                }

                PopulateSpriteConfig(existing);
                EditorUtility.SetDirty(existing);
                AssetDatabase.SaveAssets();

                Debug.Log($"[LayerLabSpriteConfigGenerator] Re-populated LayerLabSpriteConfig.asset ({existing.GetAssignedCount()}/{existing.GetTotalSlots()} sprites)");
                Selection.activeObject = existing;
                EditorGUIUtility.PingObject(existing);
                return;
            }

            // Create new config
            var config = ScriptableObject.CreateInstance<LayerLabSpriteConfigSO>();

            // Ensure directory exists
            var dir = Path.GetDirectoryName(ConfigPath);
            if (!AssetDatabase.IsValidFolder(dir))
            {
                Directory.CreateDirectory(dir);
                AssetDatabase.Refresh();
            }

            // Populate with sprites
            PopulateSpriteConfig(config);

            // Save asset
            AssetDatabase.CreateAsset(config, ConfigPath);
            AssetDatabase.SaveAssets();

            Debug.Log($"[LayerLabSpriteConfigGenerator] LayerLabSpriteConfig.asset created successfully at {ConfigPath}");
            Debug.Log($"[LayerLabSpriteConfigGenerator] Assigned {config.GetAssignedCount()}/{config.GetTotalSlots()} sprites");

            // Select in project
            Selection.activeObject = config;
            EditorGUIUtility.PingObject(config);
        }

        [MenuItem("HNR/5. Utilities/Config/Validate LayerLab Sprite Config", priority = 234)]
        public static void ValidateLayerLabSpriteConfig()
        {
            var config = AssetDatabase.LoadAssetAtPath<LayerLabSpriteConfigSO>(ConfigPath);
            if (config == null)
            {
                Debug.LogWarning("[LayerLabSpriteConfigGenerator] LayerLabSpriteConfig.asset not found. Run 'Generate LayerLab Sprite Config' first.");
                return;
            }

            Debug.Log("=== LayerLab Sprite Config Validation ===");
            Debug.Log($"Assigned sprites: {config.GetAssignedCount()}/{config.GetTotalSlots()}");

            if (!config.HasAllButton01SmallSprites())
            {
                Debug.LogWarning("Missing Button_01 small sprites (Purple, Green, Red)");
            }
            else
            {
                Debug.Log("All Button_01 small sprites assigned");
            }

            if (!config.HasAllConvexButtonSprites())
            {
                Debug.LogWarning("Missing Convex button sprites (Rectangle, LeftFlush)");
            }
            else
            {
                Debug.Log("All Convex button sprites assigned");
            }

            if (!config.HasAllTabMenuSprites())
            {
                Debug.LogWarning("Missing TabMenu sprites");
            }
            else
            {
                Debug.Log("All TabMenu sprites assigned");
            }

            if (!config.HasAllStageFrameSprites())
            {
                Debug.LogWarning("Missing StageFrame sprites");
            }
            else
            {
                Debug.Log("All StageFrame sprites assigned");
            }

            if (config.FontAfacadFlux == null)
            {
                Debug.LogWarning("Missing AfacadFlux font asset");
            }
            else
            {
                Debug.Log("AfacadFlux font assigned");
            }

            if (config.IsValid())
            {
                Debug.Log("Config is VALID - minimum required sprites are present");
            }
            else
            {
                Debug.LogError("Config is INVALID - missing minimum required sprites");
            }
        }

        /// <summary>
        /// Populates the config with sprites discovered from LayerLab assets.
        /// Note: LayerLab uses "l" for large buttons, "Gary" (typo) for Gray, and single deco sprites shared across colors.
        /// </summary>
        private static void PopulateSpriteConfig(LayerLabSpriteConfigSO config)
        {
            SerializedObject so = new SerializedObject(config);

            // Shared deco sprite for all Button_01 buttons (large version)
            Sprite largeDeco = FindSprite("Button_01_Mian_l_Deco");
            Sprite smallDeco = FindSprite("Button_01_Mian_s_Deco");

            // ============================================
            // Button_01 Sprites (using large "l" versions as they're more complete)
            // Note: LayerLab has typo "Gary" instead of "Gray"
            // ============================================

            // Purple
            AssignSprite(so, "_button01SmallPurpleBg", FindSprite("Button_01_Mian_l_Bg_Purple"));
            AssignSprite(so, "_button01SmallPurpleDeco", largeDeco);

            // Green
            AssignSprite(so, "_button01SmallGreenBg", FindSprite("Button_01_Mian_l_Bg_Green"));
            AssignSprite(so, "_button01SmallGreenDeco", largeDeco);

            // Red
            AssignSprite(so, "_button01SmallRedBg", FindSprite("Button_01_Mian_l_Bg_Red"));
            AssignSprite(so, "_button01SmallRedDeco", largeDeco);

            // Gray (LayerLab has typo "Gary")
            AssignSprite(so, "_button01SmallGrayBg", FindSprite("Button_01_Mian_l_Bg_Gary"));
            AssignSprite(so, "_button01SmallGrayDeco", largeDeco);

            // ============================================
            // Convex Button Sprites
            // ============================================

            // Convex Rectangle (single sprite, used for both bg and styled as button)
            Sprite convexRect = FindSprite("Button_Convex_Rectangle_01_Gray");
            AssignSprite(so, "_buttonConvexRectangleGrayBg", convexRect);
            AssignSprite(so, "_buttonConvexRectangleGrayBorder", convexRect); // Same sprite, no separate border

            // Convex LeftFlush
            Sprite convexLeftFlush = FindSprite("Button_Convex_LeftFlush_01_Gray");
            AssignSprite(so, "_buttonConvexLeftFlushGrayBg", convexLeftFlush);
            AssignSprite(so, "_buttonConvexLeftFlushGrayBorder", convexLeftFlush); // Same sprite, no separate border

            // ============================================
            // Tab Sprites (named "Tab_" not "TabMenu_")
            // ============================================

            AssignSprite(so, "_tabMenuBg", FindSprite("Tab_Middle_02_Bg"));
            AssignSprite(so, "_tabMenuGlow", FindSprite("Tab_Middle_02_Glow"));
            AssignSprite(so, "_tabMenuBorder", FindSprite("Tab_Middle_02_Border"));
            AssignSprite(so, "_tabFocusGlow", FindSprite("Tab_Middle_02_FocusGlow"));
            AssignSprite(so, "_tabFocus", FindSprite("Tab_Middle_02_Focus"));

            // ============================================
            // StageFrame Sprites
            // ============================================

            AssignSprite(so, "_stageFrameBackBg", FindSprite("StageFrame_01_DecoBg"));
            AssignSprite(so, "_stageFrameBg", FindSprite("StageFrame_02_Bg"));
            AssignSprite(so, "_stageFrameBorder", FindSprite("StageFrame_02_Border"));
            AssignSprite(so, "_stageFrameDeco", FindSprite("StageFrame_01_DecoBorder"));
            AssignSprite(so, "_stageFrameFocus", FindSprite("StageFrame_01_Focus"));

            // ============================================
            // Icon Sprites (from Demo/Demo_IconMisc folder)
            // ============================================

            AssignSprite(so, "_iconSettings", FindSprite("Icon_Setting"));
            AssignSprite(so, "_iconBack", FindSprite("Icon_Arrow_03_Prev"));

            // ============================================
            // Font Asset
            // ============================================

            TMP_FontAsset font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(AfacadFluxFontPath);
            if (font != null)
            {
                var fontProp = so.FindProperty("_fontAfacadFlux");
                if (fontProp != null)
                {
                    fontProp.objectReferenceValue = font;
                    Debug.Log($"[LayerLabSpriteConfigGenerator] Found font: AfacadFlux-ExtraBold SDF");
                }
            }
            else
            {
                Debug.LogWarning($"[LayerLabSpriteConfigGenerator] Font not found: {AfacadFluxFontPath}");
            }

            so.ApplyModifiedPropertiesWithoutUndo();
        }

        /// <summary>
        /// Finds a sprite by name in the LayerLab assets.
        /// </summary>
        private static Sprite FindSprite(string spriteName)
        {
            // Search in Button sprites folder first
            string[] buttonGuids = AssetDatabase.FindAssets($"{spriteName} t:Sprite", new[] { ButtonSpritesPath });
            if (buttonGuids.Length > 0)
            {
                string path = AssetDatabase.GUIDToAssetPath(buttonGuids[0]);
                var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
                if (sprite != null)
                {
                    Debug.Log($"[LayerLabSpriteConfigGenerator] Found sprite: {spriteName} at {path}");
                    return sprite;
                }
            }

            // Search in Frame sprites folder
            string[] frameGuids = AssetDatabase.FindAssets($"{spriteName} t:Sprite", new[] { FrameSpritesPath });
            if (frameGuids.Length > 0)
            {
                string path = AssetDatabase.GUIDToAssetPath(frameGuids[0]);
                var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
                if (sprite != null)
                {
                    Debug.Log($"[LayerLabSpriteConfigGenerator] Found sprite: {spriteName} at {path}");
                    return sprite;
                }
            }

            // Search in Icon sprites folder
            string[] iconGuids = AssetDatabase.FindAssets($"{spriteName} t:Sprite", new[] { IconSpritesPath });
            if (iconGuids.Length > 0)
            {
                string path = AssetDatabase.GUIDToAssetPath(iconGuids[0]);
                var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
                if (sprite != null)
                {
                    Debug.Log($"[LayerLabSpriteConfigGenerator] Found sprite: {spriteName} at {path}");
                    return sprite;
                }
            }

            // Search entire LayerLab folder as fallback
            string[] allGuids = AssetDatabase.FindAssets($"{spriteName} t:Sprite", new[] { LayerLabRoot });
            if (allGuids.Length > 0)
            {
                string path = AssetDatabase.GUIDToAssetPath(allGuids[0]);
                var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
                if (sprite != null)
                {
                    Debug.Log($"[LayerLabSpriteConfigGenerator] Found sprite (fallback): {spriteName} at {path}");
                    return sprite;
                }
            }

            Debug.LogWarning($"[LayerLabSpriteConfigGenerator] Sprite not found: {spriteName}");
            return null;
        }

        /// <summary>
        /// Assigns a sprite to a serialized property if the sprite is not null.
        /// </summary>
        private static void AssignSprite(SerializedObject so, string propertyName, Sprite sprite)
        {
            var prop = so.FindProperty(propertyName);
            if (prop != null)
            {
                prop.objectReferenceValue = sprite;
            }
            else
            {
                Debug.LogWarning($"[LayerLabSpriteConfigGenerator] Property not found: {propertyName}");
            }
        }

        /// <summary>
        /// Lists all available sprites in LayerLab folders (for debugging).
        /// </summary>
        [MenuItem("HNR/5. Utilities/Config/List LayerLab Sprites (Debug)", priority = 235)]
        public static void ListLayerLabSprites()
        {
            Debug.Log("=== LayerLab Button Sprites ===");
            ListSpritesInFolder(ButtonSpritesPath);

            Debug.Log("=== LayerLab Frame Sprites ===");
            ListSpritesInFolder(FrameSpritesPath);

            Debug.Log("=== LayerLab Demo Icon Sprites ===");
            ListSpritesInFolder(IconSpritesPath);

            Debug.Log("=== Searching for key sprites ===");
            Debug.Log("Button_01_Mian_l_Bg_Purple: " + (FindSprite("Button_01_Mian_l_Bg_Purple") != null ? "FOUND" : "NOT FOUND"));
            Debug.Log("Button_Convex_Rectangle_01_Gray: " + (FindSprite("Button_Convex_Rectangle_01_Gray") != null ? "FOUND" : "NOT FOUND"));
            Debug.Log("Tab_Middle_02_Bg: " + (FindSprite("Tab_Middle_02_Bg") != null ? "FOUND" : "NOT FOUND"));
            Debug.Log("StageFrame_02_Bg: " + (FindSprite("StageFrame_02_Bg") != null ? "FOUND" : "NOT FOUND"));
            Debug.Log("Icon_Setting: " + (FindSprite("Icon_Setting") != null ? "FOUND" : "NOT FOUND"));
        }

        private static void ListSpritesInFolder(string folderPath)
        {
            if (!AssetDatabase.IsValidFolder(folderPath))
            {
                Debug.LogWarning($"Folder not found: {folderPath}");
                return;
            }

            string[] guids = AssetDatabase.FindAssets("t:Sprite", new[] { folderPath });
            foreach (var guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                string fileName = Path.GetFileNameWithoutExtension(path);
                Debug.Log($"  {fileName}");
            }
            Debug.Log($"  Total: {guids.Length} sprites");
        }
    }
}
