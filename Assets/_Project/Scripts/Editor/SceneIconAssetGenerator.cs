// ============================================
// SceneIconAssetGenerator.cs
// Generates placeholder sprites and SceneIconConfig asset
// ============================================

using System.IO;
using UnityEditor;
using UnityEngine;

namespace HNR.Editor
{
    /// <summary>
    /// Editor utility to generate placeholder sprites and SceneIconConfig asset.
    /// Creates magenta placeholder sprites that can be replaced with final art.
    /// </summary>
    public static class SceneIconAssetGenerator
    {
        private const string ICONS_PATH = "Assets/_Project/Art/UI/Icons/Scene";
        private const string CONFIG_PATH = "Assets/_Project/Data/Config/SceneIconConfig.asset";

        /// <summary>
        /// Icon definition with name and size.
        /// </summary>
        private struct IconDef
        {
            public string Name;
            public int Size;
            public string FieldName;

            public IconDef(string name, int size, string fieldName)
            {
                Name = name;
                Size = size;
                FieldName = fieldName;
            }
        }

        /// <summary>
        /// All icon definitions to generate.
        /// </summary>
        private static readonly IconDef[] IconDefinitions = new IconDef[]
        {
            // Deck Display Icons
            new IconDef("icon_deck_draw", 32, "_drawPileIcon"),
            new IconDef("icon_deck_discard", 32, "_discardPileIcon"),

            // System Menu Icons
            new IconDef("icon_system_settings", 24, "_settingsIcon"),

            // Map Legend Icons
            new IconDef("icon_node_combat", 24, "_combatNodeIcon"),
            new IconDef("icon_node_elite", 24, "_eliteNodeIcon"),
            new IconDef("icon_node_shop", 24, "_shopNodeIcon"),
            new IconDef("icon_node_echo", 24, "_echoEventIcon"),
            new IconDef("icon_node_sanctuary", 24, "_sanctuaryIcon"),
            new IconDef("icon_node_treasure", 24, "_treasureIcon"),
            new IconDef("icon_node_boss", 24, "_bossNodeIcon"),

            // Combat UI Icons
            new IconDef("icon_combat_checkmark", 36, "_checkmarkIcon"),

            // Settings Category Icons
            new IconDef("icon_settings_display", 24, "_displaySettingsIcon"),
            new IconDef("icon_settings_audio", 24, "_audioSettingsIcon"),
            new IconDef("icon_settings_game", 24, "_gameSettingsIcon"),
            new IconDef("icon_settings_network", 24, "_networkSettingsIcon"),
            new IconDef("icon_settings_account", 24, "_accountSettingsIcon"),
        };

        /// <summary>
        /// Generate all scene icons and config asset.
        /// Called from HNR > Icons > Generate Scene Icons menu.
        /// </summary>
        public static void GenerateSceneIcons()
        {
            Debug.Log("[SceneIconAssetGenerator] Starting icon generation...");

            // Ensure directories exist
            EnsureDirectoryExists(ICONS_PATH);
            EnsureDirectoryExists(Path.GetDirectoryName(CONFIG_PATH));

            // Generate all placeholder sprites
            var sprites = new Sprite[IconDefinitions.Length];
            for (int i = 0; i < IconDefinitions.Length; i++)
            {
                sprites[i] = CreatePlaceholderSprite(IconDefinitions[i].Name, IconDefinitions[i].Size);
            }

            // Create or load config asset
            var config = AssetDatabase.LoadAssetAtPath<SceneIconConfigSO>(CONFIG_PATH);
            if (config == null)
            {
                config = ScriptableObject.CreateInstance<SceneIconConfigSO>();
                AssetDatabase.CreateAsset(config, CONFIG_PATH);
                Debug.Log($"[SceneIconAssetGenerator] Created new SceneIconConfig at {CONFIG_PATH}");
            }

            // Assign sprites to config using SerializedObject
            var serializedConfig = new SerializedObject(config);
            for (int i = 0; i < IconDefinitions.Length; i++)
            {
                var property = serializedConfig.FindProperty(IconDefinitions[i].FieldName);
                if (property != null)
                {
                    property.objectReferenceValue = sprites[i];
                }
                else
                {
                    Debug.LogWarning($"[SceneIconAssetGenerator] Could not find property: {IconDefinitions[i].FieldName}");
                }
            }
            serializedConfig.ApplyModifiedPropertiesWithoutUndo();

            // Save assets
            EditorUtility.SetDirty(config);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"[SceneIconAssetGenerator] Generated {IconDefinitions.Length} placeholder sprites at {ICONS_PATH}");
            Debug.Log($"[SceneIconAssetGenerator] SceneIconConfig asset updated at {CONFIG_PATH}");
            Debug.Log("[SceneIconAssetGenerator] Done! Replace placeholders with your final art in the SceneIconConfig asset.");

            // Ping the config in Project window for easy access
            EditorGUIUtility.PingObject(config);
            Selection.activeObject = config;
        }

        /// <summary>
        /// Create a magenta placeholder sprite.
        /// </summary>
        private static Sprite CreatePlaceholderSprite(string name, int size)
        {
            string path = $"{ICONS_PATH}/{name}.png";

            // Create texture with magenta fill
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
            Color magenta = new Color(1f, 0f, 1f, 1f); // #FF00FF

            // Fill with magenta
            Color[] pixels = new Color[size * size];
            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i] = magenta;
            }
            texture.SetPixels(pixels);
            texture.Apply();

            // Save as PNG
            byte[] pngData = texture.EncodeToPNG();
            string absolutePath = Path.Combine(Application.dataPath, "..", path);
            File.WriteAllBytes(absolutePath, pngData);

            // Clean up texture
            Object.DestroyImmediate(texture);

            // Import the asset
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);

            // Configure texture import settings
            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer != null)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.filterMode = FilterMode.Point;
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                importer.spritePixelsPerUnit = 100;
                importer.SaveAndReimport();
            }

            // Load and return the sprite
            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            Debug.Log($"[SceneIconAssetGenerator] Created: {path} ({size}x{size})");
            return sprite;
        }

        /// <summary>
        /// Ensure directory exists, creating it if necessary.
        /// </summary>
        private static void EnsureDirectoryExists(string path)
        {
            if (string.IsNullOrEmpty(path)) return;

            // Convert to absolute path for Directory operations
            string absolutePath = Path.Combine(Application.dataPath, "..", path);
            if (!Directory.Exists(absolutePath))
            {
                Directory.CreateDirectory(absolutePath);
                Debug.Log($"[SceneIconAssetGenerator] Created directory: {path}");
            }
        }

        /// <summary>
        /// Verify all icons exist and are assigned.
        /// </summary>
        public static void VerifySceneIcons()
        {
            var config = AssetDatabase.LoadAssetAtPath<SceneIconConfigSO>(CONFIG_PATH);
            if (config == null)
            {
                Debug.LogError($"[SceneIconAssetGenerator] SceneIconConfig not found at {CONFIG_PATH}. Run Generate Scene Icons first.");
                return;
            }

            int assigned = config.GetAssignedCount();
            int total = IconDefinitions.Length;

            if (assigned == total)
            {
                Debug.Log($"[SceneIconAssetGenerator] All {total} icons are assigned.");
                config.ValidateAllIcons();
            }
            else
            {
                Debug.LogWarning($"[SceneIconAssetGenerator] {assigned}/{total} icons assigned. Some icons may be missing.");
                config.ValidateAllIcons();
            }
        }
    }
}
