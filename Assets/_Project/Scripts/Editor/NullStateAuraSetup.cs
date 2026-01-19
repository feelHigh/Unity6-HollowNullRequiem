// ============================================
// NullStateAuraSetup.cs
// Editor tool to configure the Null State aura VFX
// ============================================

using UnityEngine;
using UnityEditor;
using System.IO;
using HNR.VFX;

namespace HNR.Editor
{
    /// <summary>
    /// Sets up the Null State aura VFX by creating a prefab variant
    /// of the Hovl Studio Darkness aura with VFXInstance component.
    /// </summary>
    public static class NullStateAuraSetup
    {
        private const string SOURCE_PREFAB_PATH = "Assets/ThirdParty/Hovl Studio/Auras pack/Prefabs/Darkness aura.prefab";
        private const string TARGET_PREFAB_PATH = "Assets/_Project/Prefabs/VFX/vfx_null_aura.prefab";
        private const string VFX_CONFIG_PATH = "Assets/_Project/Data/Config/VFXConfig.asset";
        private const string EFFECT_ID = "vfx_null_aura";

        [MenuItem("HNR/2. Prefabs/VFX/Setup Null State Aura", priority = 45)]
        public static void SetupNullStateAura()
        {
            // Load source prefab
            var sourcePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(SOURCE_PREFAB_PATH);
            if (sourcePrefab == null)
            {
                Debug.LogError($"[NullStateAuraSetup] Source prefab not found at: {SOURCE_PREFAB_PATH}");
                return;
            }

            // Ensure target directory exists
            string targetDir = Path.GetDirectoryName(TARGET_PREFAB_PATH);
            if (!Directory.Exists(targetDir))
            {
                Directory.CreateDirectory(targetDir);
                AssetDatabase.Refresh();
            }

            // Create prefab variant
            GameObject variantInstance = (GameObject)PrefabUtility.InstantiatePrefab(sourcePrefab);
            variantInstance.name = "vfx_null_aura";

            // Add VFXInstance component if not present
            var vfxInstance = variantInstance.GetComponent<VFXInstance>();
            if (vfxInstance == null)
            {
                vfxInstance = variantInstance.AddComponent<VFXInstance>();
            }

            // Configure VFXInstance via SerializedObject
            // Note: VFXInstance uses _autoReturn = true by default, which we want to override
            // for persistent effects. The code will call SetPersistent(true) at runtime.

            // Set scale (2,1,1) - wider aura for better visibility
            variantInstance.transform.localScale = new Vector3(2f, 1f, 1f);

            // Set Y position offset (0.5) - center on character
            variantInstance.transform.localPosition = new Vector3(0f, 0.5f, 0f);

            // Save as prefab variant
            GameObject savedPrefab = PrefabUtility.SaveAsPrefabAsset(variantInstance, TARGET_PREFAB_PATH);

            // Cleanup scene instance
            Object.DestroyImmediate(variantInstance);

            if (savedPrefab == null)
            {
                Debug.LogError("[NullStateAuraSetup] Failed to save prefab variant");
                return;
            }

            Debug.Log($"[NullStateAuraSetup] Created prefab variant at: {TARGET_PREFAB_PATH}");

            // Add to VFXConfig
            AddToVFXConfig(savedPrefab);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            // Select the created prefab
            Selection.activeObject = savedPrefab;
            EditorGUIUtility.PingObject(savedPrefab);

            Debug.Log("[NullStateAuraSetup] Null State aura VFX setup complete!");
        }

        private static void AddToVFXConfig(GameObject prefab)
        {
            var vfxConfig = AssetDatabase.LoadAssetAtPath<VFXConfigSO>(VFX_CONFIG_PATH);
            if (vfxConfig == null)
            {
                Debug.LogError($"[NullStateAuraSetup] VFXConfig not found at: {VFX_CONFIG_PATH}");
                return;
            }

            // Check if entry already exists
            if (vfxConfig.HasEntry(EFFECT_ID))
            {
                Debug.Log($"[NullStateAuraSetup] Entry '{EFFECT_ID}' already exists in VFXConfig, updating prefab reference");
            }

            // Add/update entry via SerializedObject
            SerializedObject so = new SerializedObject(vfxConfig);
            SerializedProperty specialEntries = so.FindProperty("_specialEntries");

            // Find existing entry or create new one
            int existingIndex = -1;
            for (int i = 0; i < specialEntries.arraySize; i++)
            {
                var entry = specialEntries.GetArrayElementAtIndex(i);
                if (entry.FindPropertyRelative("EffectId").stringValue == EFFECT_ID)
                {
                    existingIndex = i;
                    break;
                }
            }

            SerializedProperty entryProp;
            if (existingIndex >= 0)
            {
                entryProp = specialEntries.GetArrayElementAtIndex(existingIndex);
            }
            else
            {
                specialEntries.InsertArrayElementAtIndex(specialEntries.arraySize);
                entryProp = specialEntries.GetArrayElementAtIndex(specialEntries.arraySize - 1);
            }

            // Configure entry
            entryProp.FindPropertyRelative("EffectId").stringValue = EFFECT_ID;
            entryProp.FindPropertyRelative("Prefab").objectReferenceValue = prefab;
            entryProp.FindPropertyRelative("Category").enumValueIndex = (int)VFXCategory.Special;
            entryProp.FindPropertyRelative("PreWarmCount").intValue = 3;
            entryProp.FindPropertyRelative("MaxActive").intValue = 3;
            // Purple/violet color for corruption
            entryProp.FindPropertyRelative("DefaultColor").colorValue = new Color(0.5f, 0.1f, 0.5f, 1f);
            entryProp.FindPropertyRelative("DefaultScale").floatValue = 1f;

            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(vfxConfig);

            Debug.Log($"[NullStateAuraSetup] Added '{EFFECT_ID}' to VFXConfig (Special Effects category)");
        }

        [MenuItem("HNR/2. Prefabs/VFX/Setup Null State Aura", true)]
        public static bool ValidateSetupNullStateAura()
        {
            // Check if source prefab exists
            return AssetDatabase.LoadAssetAtPath<GameObject>(SOURCE_PREFAB_PATH) != null;
        }
    }
}
