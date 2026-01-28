// ============================================
// EchoChoiceButtonPrefabGenerator.cs
// Editor script to generate EchoChoiceButton prefab for EchoEventScreen
// ============================================

#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;
using HNR.UI.Config;

namespace HNR.Editor
{
    /// <summary>
    /// Generates the EchoChoiceButton prefab for NullRift EchoEventScreen.
    /// Features two text fields: Text (cyan, main choice) and ResultText (dark, cost/result).
    /// Uses LayerLab Button_01_small purple styling.
    /// </summary>
    public static class EchoChoiceButtonPrefabGenerator
    {
        private const string PrefabDir = "Assets/_Project/Prefabs/UI/NullRift";
        private const string PrefabPath = PrefabDir + "/EchoChoiceButton.prefab";

        private static LayerLabSpriteConfigSO LoadLayerLabConfig()
        {
            return AssetDatabase.LoadAssetAtPath<LayerLabSpriteConfigSO>(
                "Assets/_Project/Data/Config/LayerLabSpriteConfig.asset");
        }

        [MenuItem("HNR/2. Prefabs/UI/NullRift/EchoChoiceButton Prefab", priority = 50)]
        public static void GeneratePrefab()
        {
            EnsureDirectoryExists();

            var layerLabConfig = LoadLayerLabConfig();

            // Create root button GameObject
            var root = new GameObject("EchoChoiceButton");
            var rootRect = root.AddComponent<RectTransform>();
            rootRect.sizeDelta = new Vector2(500, 85);

            // Add Image for button background
            var bgImage = root.AddComponent<Image>();
            if (layerLabConfig != null && layerLabConfig.Button01SmallPurpleBg != null)
            {
                bgImage.sprite = layerLabConfig.Button01SmallPurpleBg;
                bgImage.type = Image.Type.Sliced;
                bgImage.color = Color.white;
            }
            else
            {
                bgImage.color = new Color(0.25f, 0.15f, 0.35f, 0.95f); // Fallback purple
            }

            // Add Button component
            var button = root.AddComponent<Button>();
            button.targetGraphic = bgImage;

            // Add decoration layer if available
            if (layerLabConfig != null && layerLabConfig.Button01SmallPurpleDeco != null)
            {
                var decoGO = new GameObject("Deco");
                decoGO.transform.SetParent(root.transform, false);
                var decoRect = decoGO.AddComponent<RectTransform>();
                decoRect.anchorMin = Vector2.zero;
                decoRect.anchorMax = Vector2.one;
                decoRect.offsetMin = Vector2.zero;
                decoRect.offsetMax = Vector2.zero;
                var decoImage = decoGO.AddComponent<Image>();
                decoImage.sprite = layerLabConfig.Button01SmallPurpleDeco;
                decoImage.type = Image.Type.Sliced;
                decoImage.color = Color.white;
                decoImage.raycastTarget = false;
            }

            // Add LayoutElement for VerticalLayoutGroup integration
            var layoutElement = root.AddComponent<LayoutElement>();
            layoutElement.preferredHeight = 85;
            layoutElement.minWidth = 400;
            layoutElement.flexibleWidth = 1;

            // ============================================
            // Text - Main choice text (upper portion, cyan)
            // ============================================
            var textGO = new GameObject("Text");
            textGO.transform.SetParent(root.transform, false);
            var textRect = textGO.AddComponent<RectTransform>();
            textRect.anchorMin = new Vector2(0, 0.50f);
            textRect.anchorMax = new Vector2(1, 0.92f);
            textRect.offsetMin = new Vector2(25, 0);
            textRect.offsetMax = new Vector2(-25, 0);

            var textTMP = textGO.AddComponent<TextMeshProUGUI>();
            textTMP.text = "Choice Text";
            textTMP.fontSize = 22;
            textTMP.fontSizeMax = 22;
            textTMP.fontSizeMin = 16;
            textTMP.enableAutoSizing = true;
            textTMP.alignment = TextAlignmentOptions.Left;
            textTMP.color = new Color(0f, 0.95f, 1f); // Bright cyan
            textTMP.fontStyle = FontStyles.Normal;
            textTMP.raycastTarget = false;

            // Apply LayerLab font if available
            if (layerLabConfig != null && layerLabConfig.FontAfacadFlux != null)
            {
                textTMP.font = layerLabConfig.FontAfacadFlux;
            }

            // ============================================
            // ResultText - Cost/result text (lower portion, dark/black italic)
            // ============================================
            var resultTextGO = new GameObject("ResultText");
            resultTextGO.transform.SetParent(root.transform, false);
            var resultTextRect = resultTextGO.AddComponent<RectTransform>();
            resultTextRect.anchorMin = new Vector2(0, 0.10f);
            resultTextRect.anchorMax = new Vector2(1, 0.48f);
            resultTextRect.offsetMin = new Vector2(25, 0);
            resultTextRect.offsetMax = new Vector2(-25, 0);

            var resultTextTMP = resultTextGO.AddComponent<TextMeshProUGUI>();
            resultTextTMP.text = "";
            resultTextTMP.fontSize = 22; // Same size as main text for consistency
            resultTextTMP.fontSizeMax = 22;
            resultTextTMP.fontSizeMin = 14;
            resultTextTMP.enableAutoSizing = true;
            resultTextTMP.alignment = TextAlignmentOptions.Left;
            resultTextTMP.color = new Color(0.1f, 0.1f, 0.1f); // Dark/black
            resultTextTMP.fontStyle = FontStyles.Italic;
            resultTextTMP.raycastTarget = false;

            // Apply LayerLab font if available
            if (layerLabConfig != null && layerLabConfig.FontAfacadFlux != null)
            {
                resultTextTMP.font = layerLabConfig.FontAfacadFlux;
            }

            // Save as prefab
            bool success;
            PrefabUtility.SaveAsPrefabAsset(root, PrefabPath, out success);

            // Clean up scene object
            Object.DestroyImmediate(root);

            if (success)
            {
                AssetDatabase.Refresh();
                Debug.Log($"[EchoChoiceButtonPrefabGenerator] Prefab created at: {PrefabPath}");

                // Select the prefab
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
                Selection.activeObject = prefab;
                EditorGUIUtility.PingObject(prefab);
            }
            else
            {
                Debug.LogError("[EchoChoiceButtonPrefabGenerator] Failed to create prefab");
            }
        }

        private static void EnsureDirectoryExists()
        {
            if (!AssetDatabase.IsValidFolder("Assets/_Project/Prefabs/UI/NullRift"))
            {
                if (!AssetDatabase.IsValidFolder("Assets/_Project/Prefabs/UI"))
                {
                    AssetDatabase.CreateFolder("Assets/_Project/Prefabs", "UI");
                }
                AssetDatabase.CreateFolder("Assets/_Project/Prefabs/UI", "NullRift");
                AssetDatabase.Refresh();
            }
        }
    }
}
#endif
