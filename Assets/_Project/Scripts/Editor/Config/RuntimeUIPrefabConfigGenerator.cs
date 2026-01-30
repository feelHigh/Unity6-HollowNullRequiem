// ============================================
// RuntimeUIPrefabConfigGenerator.cs
// Editor tool to generate and wire RuntimeUIPrefabConfig
// ============================================

#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using HNR.UI.Config;

namespace HNR.Editor.Config
{
    /// <summary>
    /// Generates and wires the RuntimeUIPrefabConfig ScriptableObject.
    /// </summary>
    public static class RuntimeUIPrefabConfigGenerator
    {
        private const string CONFIG_PATH = "Assets/_Project/Data/Config/RuntimeUIPrefabConfig.asset";
        private const string RESOURCES_PATH = "Assets/_Project/Resources/Config/RuntimeUIPrefabConfig.asset";
        private const string PREFAB_DIR = "Assets/_Project/Prefabs/UI/Runtime";

        [MenuItem("HNR/5. Utilities/Config/Generate Runtime UI Prefab Config", false, 550)]
        public static void GenerateConfig()
        {
            // Ensure directories exist
            EnsureDirectories();

            // Create or load existing config
            var config = AssetDatabase.LoadAssetAtPath<RuntimeUIPrefabConfigSO>(RESOURCES_PATH);
            if (config == null)
            {
                config = ScriptableObject.CreateInstance<RuntimeUIPrefabConfigSO>();
                AssetDatabase.CreateAsset(config, RESOURCES_PATH);
                Debug.Log($"[RuntimeUIPrefabConfigGenerator] Created config at: {RESOURCES_PATH}");
            }

            // Wire prefab references
            WirePrefabs(config);

            EditorUtility.SetDirty(config);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            // Select the config
            Selection.activeObject = config;
            EditorGUIUtility.PingObject(config);

            var status = config.GetAssignmentStatus();
            Debug.Log($"[RuntimeUIPrefabConfigGenerator] Config updated: {status.assigned}/{status.total} prefabs assigned");
        }

        [MenuItem("HNR/5. Utilities/Config/Validate Runtime UI Prefab Config", false, 551)]
        public static void ValidateConfig()
        {
            var config = AssetDatabase.LoadAssetAtPath<RuntimeUIPrefabConfigSO>(RESOURCES_PATH);
            if (config == null)
            {
                Debug.LogWarning("[RuntimeUIPrefabConfigGenerator] Config not found. Run 'Generate Runtime UI Prefab Config' first.");
                return;
            }

            var status = config.GetAssignmentStatus();

            if (config.IsValid())
            {
                Debug.Log($"[RuntimeUIPrefabConfigGenerator] Config is VALID: All {status.total} prefabs assigned");
            }
            else
            {
                Debug.LogWarning($"[RuntimeUIPrefabConfigGenerator] Config INCOMPLETE: {status.assigned}/{status.total} prefabs assigned");

                // Log missing prefabs
                LogMissingPrefabs(config);
            }
        }

        private static void EnsureDirectories()
        {
            if (!AssetDatabase.IsValidFolder("Assets/_Project/Resources"))
            {
                AssetDatabase.CreateFolder("Assets/_Project", "Resources");
            }
            if (!AssetDatabase.IsValidFolder("Assets/_Project/Resources/Config"))
            {
                AssetDatabase.CreateFolder("Assets/_Project/Resources", "Config");
            }
        }

        private static void WirePrefabs(RuntimeUIPrefabConfigSO config)
        {
            var so = new SerializedObject(config);

            // Wire each prefab
            WirePrefab(so, "_confirmationDialogPrefab", "ConfirmationDialog.prefab");
            WirePrefab(so, "_statusIconPrefab", "StatusIcon.prefab");
            WirePrefab(so, "_relicDisplayIconPrefab", "RelicDisplayIcon.prefab");
            WirePrefab(so, "_deckViewerCardSlotPrefab", "DeckViewerCardSlot.prefab");
            WirePrefab(so, "_simpleCardDisplayItemPrefab", "SimpleCardDisplayItem.prefab");
            WirePrefab(so, "_sanctuaryCardSlotPrefab", "SanctuaryCardSlot.prefab");
            WirePrefab(so, "_sanctuaryUpgradeSlotPrefab", "SanctuaryUpgradeSlot.prefab");
            WirePrefab(so, "_rewardCardSlotPrefab", "RewardCardSlot.prefab");
            WirePrefab(so, "_relicShopSlotPrefab", "RelicShopSlot.prefab");
            WirePrefab(so, "_bannerSlidePrefab", "BannerSlide.prefab");
            WirePrefab(so, "_bannerIndicatorPrefab", "BannerIndicator.prefab");
            WirePrefab(so, "_requiemSelectionSlotPrefab", "RequiemSelectionSlot.prefab");
            WirePrefab(so, "_requiemPortraitButtonPrefab", "RequiemPortraitButton.prefab");
            WirePrefab(so, "_sidebarPortraitPrefab", "SidebarPortrait.prefab");
            WirePrefab(so, "_emptyStatePrefab", "EmptyState.prefab");
            WirePrefab(so, "_statusContainerPrefab", "StatusContainer.prefab");
            WirePrefab(so, "_confirmTeamButtonPrefab", "ConfirmTeamButton.prefab");

            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void WirePrefab(SerializedObject so, string fieldName, string prefabFileName)
        {
            string prefabPath = $"{PREFAB_DIR}/{prefabFileName}";
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);

            if (prefab != null)
            {
                var prop = so.FindProperty(fieldName);
                if (prop != null)
                {
                    prop.objectReferenceValue = prefab;
                }
            }
            else
            {
                Debug.LogWarning($"[RuntimeUIPrefabConfigGenerator] Prefab not found: {prefabPath}");
            }
        }

        private static void LogMissingPrefabs(RuntimeUIPrefabConfigSO config)
        {
            var so = new SerializedObject(config);

            string[] fields = {
                "_confirmationDialogPrefab",
                "_statusIconPrefab",
                "_relicDisplayIconPrefab",
                "_deckViewerCardSlotPrefab",
                "_simpleCardDisplayItemPrefab",
                "_sanctuaryCardSlotPrefab",
                "_sanctuaryUpgradeSlotPrefab",
                "_rewardCardSlotPrefab",
                "_relicShopSlotPrefab",
                "_bannerSlidePrefab",
                "_bannerIndicatorPrefab",
                "_requiemSelectionSlotPrefab",
                "_requiemPortraitButtonPrefab",
                "_sidebarPortraitPrefab",
                "_emptyStatePrefab",
                "_statusContainerPrefab",
                "_confirmTeamButtonPrefab"
            };

            foreach (var fieldName in fields)
            {
                var prop = so.FindProperty(fieldName);
                if (prop != null && prop.objectReferenceValue == null)
                {
                    Debug.LogWarning($"  Missing: {fieldName}");
                }
            }
        }
    }
}
#endif
