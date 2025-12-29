// ============================================
// AspectIconConfigGenerator.cs
// Editor tool to generate AspectIconConfig asset
// ============================================

using UnityEngine;
using UnityEditor;
using System.IO;
using HNR.UI.Config;

namespace HNR.Editor
{
    /// <summary>
    /// Generates the AspectIconConfig ScriptableObject asset.
    /// Automatically loads aspect icon sprites from the standard location.
    /// </summary>
    public static class AspectIconConfigGenerator
    {
        private const string CONFIG_PATH = "Assets/_Project/Data/Config/AspectIconConfig.asset";
        private const string ICONS_PATH = "Assets/_Project/Art/UI/Icons/Aspects";

        /// <summary>
        /// Generates or updates the AspectIconConfig asset.
        /// </summary>
        public static void GenerateAspectIconConfig()
        {
            // Ensure directory exists
            string directory = Path.GetDirectoryName(CONFIG_PATH);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            // Create icons directory if it doesn't exist
            if (!Directory.Exists(ICONS_PATH))
            {
                Directory.CreateDirectory(ICONS_PATH);
                AssetDatabase.Refresh();
                Debug.Log($"[AspectIconConfigGenerator] Created icons directory at {ICONS_PATH}");
            }

            // Load or create the config asset
            var config = AssetDatabase.LoadAssetAtPath<AspectIconConfigSO>(CONFIG_PATH);
            if (config == null)
            {
                config = ScriptableObject.CreateInstance<AspectIconConfigSO>();
                AssetDatabase.CreateAsset(config, CONFIG_PATH);
                Debug.Log($"[AspectIconConfigGenerator] Created new AspectIconConfig at {CONFIG_PATH}");
            }

            // Try to auto-wire sprites if they exist
            var so = new SerializedObject(config);
            int iconsWired = 0;

            iconsWired += TryWireSprite(so, "_flameIcon", "Aspect_Flame");
            iconsWired += TryWireSprite(so, "_shadowIcon", "Aspect_Shadow");
            iconsWired += TryWireSprite(so, "_lightIcon", "Aspect_Light");
            iconsWired += TryWireSprite(so, "_natureIcon", "Aspect_Nature");
            iconsWired += TryWireSprite(so, "_arcaneIcon", "Aspect_Arcane");
            iconsWired += TryWireSprite(so, "_noneIcon", "Aspect_None");

            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(config);
            AssetDatabase.SaveAssets();

            if (iconsWired > 0)
            {
                Debug.Log($"[AspectIconConfigGenerator] Auto-wired {iconsWired} aspect icon sprites");
            }
            else
            {
                Debug.Log($"[AspectIconConfigGenerator] AspectIconConfig created. Place icon sprites at:\n" +
                          $"  {ICONS_PATH}/Aspect_Flame.png\n" +
                          $"  {ICONS_PATH}/Aspect_Shadow.png\n" +
                          $"  {ICONS_PATH}/Aspect_Light.png\n" +
                          $"  {ICONS_PATH}/Aspect_Nature.png\n" +
                          $"  {ICONS_PATH}/Aspect_Arcane.png\n" +
                          $"  {ICONS_PATH}/Aspect_None.png\n" +
                          "Then run this generator again to auto-wire them.");
            }

            // Select the created asset
            Selection.activeObject = config;
            EditorGUIUtility.PingObject(config);
        }

        private static int TryWireSprite(SerializedObject so, string propertyName, string spriteName)
        {
            // Try common extensions
            string[] extensions = { ".png", ".jpg", ".jpeg" };
            foreach (var ext in extensions)
            {
                string path = $"{ICONS_PATH}/{spriteName}{ext}";
                var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
                if (sprite != null)
                {
                    so.FindProperty(propertyName).objectReferenceValue = sprite;
                    Debug.Log($"[AspectIconConfigGenerator] Wired {spriteName} from {path}");
                    return 1;
                }
            }
            return 0;
        }

        /// <summary>
        /// Verifies the AspectIconConfig asset exists and has all icons assigned.
        /// </summary>
        public static void VerifyAspectIconConfig()
        {
            var config = AssetDatabase.LoadAssetAtPath<AspectIconConfigSO>(CONFIG_PATH);
            if (config == null)
            {
                Debug.LogWarning($"[AspectIconConfigGenerator] AspectIconConfig not found at {CONFIG_PATH}. Run 'Generate Aspect Icon Config' first.");
                return;
            }

            bool allAssigned = config.HasAllIcons();
            if (allAssigned)
            {
                Debug.Log("[AspectIconConfigGenerator] ✓ AspectIconConfig has all icons assigned.");
            }
            else
            {
                Debug.LogWarning("[AspectIconConfigGenerator] AspectIconConfig is missing some icons:\n" +
                                 $"  Flame: {(config.FlameIcon != null ? "✓" : "✗")}\n" +
                                 $"  Shadow: {(config.ShadowIcon != null ? "✓" : "✗")}\n" +
                                 $"  Light: {(config.LightIcon != null ? "✓" : "✗")}\n" +
                                 $"  Nature: {(config.NatureIcon != null ? "✓" : "✗")}\n" +
                                 $"  Arcane: {(config.ArcaneIcon != null ? "✓" : "✗")}\n" +
                                 $"  None: {(config.NoneIcon != null ? "✓" : "✗")}");
            }

            Selection.activeObject = config;
        }
    }
}
