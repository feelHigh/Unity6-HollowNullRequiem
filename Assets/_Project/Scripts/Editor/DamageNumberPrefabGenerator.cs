// ============================================
// DamageNumberPrefabGenerator.cs
// Editor script to generate DamageNumber prefab
// ============================================

#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;

namespace HNR.Editor
{
    public static class DamageNumberPrefabGenerator
    {
        [MenuItem("HNR/Generate DamageNumber Prefab")]
        public static void GeneratePrefab()
        {
            // Create root GameObject
            var root = new GameObject("DamageNumber");
            var rootRect = root.AddComponent<RectTransform>();
            rootRect.sizeDelta = new Vector2(100, 40);

            // Add CanvasGroup for fading
            var canvasGroup = root.AddComponent<CanvasGroup>();
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;

            // Create text child
            var textGO = new GameObject("Text");
            textGO.transform.SetParent(root.transform, false);

            var textRect = textGO.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;

            var text = textGO.AddComponent<TextMeshProUGUI>();
            text.text = "999";
            text.fontSize = 22;
            text.fontStyle = FontStyles.Bold;
            text.alignment = TextAlignmentOptions.Center;
            text.color = Color.white;
            text.enableWordWrapping = false;
            text.overflowMode = TextOverflowModes.Overflow;

            // Add outline for visibility
            text.outlineWidth = 0.2f;
            text.outlineColor = Color.black;

            // Add DamageNumber component
            var damageNumber = root.AddComponent<HNR.UI.DamageNumber>();

            // Use SerializedObject to assign references
            var so = new SerializedObject(damageNumber);
            so.FindProperty("_text").objectReferenceValue = text;
            so.FindProperty("_floatDistance").floatValue = 50f;
            so.FindProperty("_duration").floatValue = 0.8f;
            so.FindProperty("_punchScale").floatValue = 1.3f;
            so.FindProperty("_horizontalRandomness").floatValue = 20f;
            so.FindProperty("_damageColor").colorValue = new Color(1f, 0.3f, 0.3f, 1f);
            so.FindProperty("_healColor").colorValue = new Color(0.3f, 1f, 0.3f, 1f);
            so.FindProperty("_blockColor").colorValue = new Color(0.5f, 0.7f, 1f, 1f);
            so.FindProperty("_criticalColor").colorValue = new Color(1f, 0.8f, 0f, 1f);
            so.ApplyModifiedPropertiesWithoutUndo();

            // Create prefab directory if needed
            string prefabDir = "Assets/_Project/Prefabs/UI/Effects";
            if (!AssetDatabase.IsValidFolder(prefabDir))
            {
                if (!AssetDatabase.IsValidFolder("Assets/_Project/Prefabs/UI"))
                {
                    AssetDatabase.CreateFolder("Assets/_Project/Prefabs", "UI");
                }
                AssetDatabase.CreateFolder("Assets/_Project/Prefabs/UI", "Effects");
            }

            // Save as prefab
            string prefabPath = $"{prefabDir}/DamageNumber.prefab";
            PrefabUtility.SaveAsPrefabAsset(root, prefabPath);

            // Clean up scene object
            Object.DestroyImmediate(root);

            AssetDatabase.Refresh();
            Debug.Log($"[DamageNumberPrefabGenerator] Prefab created at: {prefabPath}");

            // Select the prefab
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            Selection.activeObject = prefab;
            EditorGUIUtility.PingObject(prefab);
        }
    }
}
#endif
