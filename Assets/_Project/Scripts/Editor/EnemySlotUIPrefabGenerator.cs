// ============================================
// EnemySlotUIPrefabGenerator.cs
// Editor script to generate EnemySlotUI prefab
// ============================================

#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;

namespace HNR.Editor
{
    public static class EnemySlotUIPrefabGenerator
    {
        [MenuItem("HNR/Generate EnemySlotUI Prefab")]
        public static void GeneratePrefab()
        {
            // Create root GameObject
            var root = new GameObject("EnemySlotUI");
            var rootRect = root.AddComponent<RectTransform>();
            root.AddComponent<CanvasRenderer>();
            var rootImage = root.AddComponent<Image>();
            rootImage.color = new Color(0.1f, 0.1f, 0.15f, 0.8f);

            rootRect.sizeDelta = new Vector2(200, 280);

            // Enemy Sprite
            var enemySprite = CreateChild(root, "EnemySprite");
            var spriteRect = enemySprite.GetComponent<RectTransform>();
            spriteRect.anchorMin = new Vector2(0.1f, 0.4f);
            spriteRect.anchorMax = new Vector2(0.9f, 0.95f);
            spriteRect.offsetMin = Vector2.zero;
            spriteRect.offsetMax = Vector2.zero;
            var spriteImage = enemySprite.AddComponent<Image>();
            spriteImage.color = Color.white;

            // HP Bar (using Slider)
            var hpBarGO = new GameObject("HPBar");
            hpBarGO.transform.SetParent(root.transform, false);
            var hpBarRect = hpBarGO.AddComponent<RectTransform>();
            hpBarRect.anchorMin = new Vector2(0.05f, 0.28f);
            hpBarRect.anchorMax = new Vector2(0.95f, 0.35f);
            hpBarRect.offsetMin = Vector2.zero;
            hpBarRect.offsetMax = Vector2.zero;

            var slider = hpBarGO.AddComponent<Slider>();
            slider.interactable = false;
            slider.transition = Selectable.Transition.None;
            slider.minValue = 0;
            slider.maxValue = 1;
            slider.value = 1;

            // Background
            var bgGO = CreateChild(hpBarGO, "Background");
            var bgRect = bgGO.GetComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.offsetMin = Vector2.zero;
            bgRect.offsetMax = Vector2.zero;
            var bgImage = bgGO.AddComponent<Image>();
            bgImage.color = new Color(0.2f, 0.2f, 0.2f, 1f);

            // Fill Area
            var fillAreaGO = CreateChild(hpBarGO, "Fill Area");
            var fillAreaRect = fillAreaGO.GetComponent<RectTransform>();
            fillAreaRect.anchorMin = Vector2.zero;
            fillAreaRect.anchorMax = Vector2.one;
            fillAreaRect.offsetMin = Vector2.zero;
            fillAreaRect.offsetMax = Vector2.zero;

            // Fill
            var fillGO = CreateChild(fillAreaGO, "Fill");
            var fillRect = fillGO.GetComponent<RectTransform>();
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = Vector2.one;
            fillRect.offsetMin = Vector2.zero;
            fillRect.offsetMax = Vector2.zero;
            var fillImage = fillGO.AddComponent<Image>();
            fillImage.color = new Color(0.8f, 0.2f, 0.2f, 1f); // Red for enemy HP

            slider.fillRect = fillRect;

            // HP Text
            var hpTextGO = CreateChild(root, "HPText");
            var hpTextRect = hpTextGO.GetComponent<RectTransform>();
            hpTextRect.anchorMin = new Vector2(0.05f, 0.2f);
            hpTextRect.anchorMax = new Vector2(0.95f, 0.28f);
            hpTextRect.offsetMin = Vector2.zero;
            hpTextRect.offsetMax = Vector2.zero;
            var hpText = hpTextGO.AddComponent<TextMeshProUGUI>();
            hpText.text = "30/30";
            hpText.fontSize = 14;
            hpText.alignment = TextAlignmentOptions.Center;
            hpText.color = Color.white;

            // Block Container
            var blockContainer = CreateChild(root, "BlockContainer");
            var blockContainerRect = blockContainer.GetComponent<RectTransform>();
            blockContainerRect.anchorMin = new Vector2(0.7f, 0.35f);
            blockContainerRect.anchorMax = new Vector2(0.95f, 0.45f);
            blockContainerRect.offsetMin = Vector2.zero;
            blockContainerRect.offsetMax = Vector2.zero;

            // Block Icon
            var blockIconGO = CreateChild(blockContainer, "BlockIcon");
            var blockIconRect = blockIconGO.GetComponent<RectTransform>();
            blockIconRect.anchorMin = new Vector2(0, 0);
            blockIconRect.anchorMax = new Vector2(0.5f, 1);
            blockIconRect.offsetMin = Vector2.zero;
            blockIconRect.offsetMax = Vector2.zero;
            var blockIcon = blockIconGO.AddComponent<Image>();
            blockIcon.color = new Color(0.5f, 0.7f, 1f, 1f);

            // Block Text
            var blockTextGO = CreateChild(blockContainer, "BlockText");
            var blockTextRect = blockTextGO.GetComponent<RectTransform>();
            blockTextRect.anchorMin = new Vector2(0.5f, 0);
            blockTextRect.anchorMax = new Vector2(1, 1);
            blockTextRect.offsetMin = Vector2.zero;
            blockTextRect.offsetMax = Vector2.zero;
            var blockText = blockTextGO.AddComponent<TextMeshProUGUI>();
            blockText.text = "5";
            blockText.fontSize = 14;
            blockText.alignment = TextAlignmentOptions.Center;
            blockText.color = Color.white;

            // Intent Container
            var intentContainer = CreateChild(root, "IntentContainer");
            var intentContainerRect = intentContainer.GetComponent<RectTransform>();
            intentContainerRect.anchorMin = new Vector2(0.3f, 0.02f);
            intentContainerRect.anchorMax = new Vector2(0.7f, 0.18f);
            intentContainerRect.offsetMin = Vector2.zero;
            intentContainerRect.offsetMax = Vector2.zero;

            // Intent Icon
            var intentIconGO = CreateChild(intentContainer, "IntentIcon");
            var intentIconRect = intentIconGO.GetComponent<RectTransform>();
            intentIconRect.anchorMin = new Vector2(0, 0);
            intentIconRect.anchorMax = new Vector2(0.5f, 1);
            intentIconRect.offsetMin = Vector2.zero;
            intentIconRect.offsetMax = Vector2.zero;
            var intentIcon = intentIconGO.AddComponent<Image>();
            intentIcon.color = new Color(1f, 0.3f, 0.3f, 1f);

            // Intent Value Text
            var intentTextGO = CreateChild(intentContainer, "IntentValueText");
            var intentTextRect = intentTextGO.GetComponent<RectTransform>();
            intentTextRect.anchorMin = new Vector2(0.5f, 0);
            intentTextRect.anchorMax = new Vector2(1, 1);
            intentTextRect.offsetMin = Vector2.zero;
            intentTextRect.offsetMax = Vector2.zero;
            var intentText = intentTextGO.AddComponent<TextMeshProUGUI>();
            intentText.text = "12";
            intentText.fontSize = 18;
            intentText.fontStyle = FontStyles.Bold;
            intentText.alignment = TextAlignmentOptions.Center;
            intentText.color = Color.white;

            // Highlight Ring
            var highlightGO = CreateChild(root, "HighlightRing");
            var highlightRect = highlightGO.GetComponent<RectTransform>();
            highlightRect.anchorMin = new Vector2(-0.05f, -0.02f);
            highlightRect.anchorMax = new Vector2(1.05f, 1.02f);
            highlightRect.offsetMin = Vector2.zero;
            highlightRect.offsetMax = Vector2.zero;
            var highlightImage = highlightGO.AddComponent<Image>();
            highlightImage.color = new Color(0f, 0.8f, 0.9f, 0.5f);
            highlightImage.raycastTarget = false;
            highlightGO.SetActive(false);

            // Move highlight to back
            highlightGO.transform.SetAsFirstSibling();

            // Add EnemySlotUI component
            var enemySlotUI = root.AddComponent<HNR.Combat.EnemySlotUI>();

            // Use SerializedObject to assign references
            var so = new SerializedObject(enemySlotUI);
            so.FindProperty("_enemySprite").objectReferenceValue = spriteImage;
            so.FindProperty("_hpBar").objectReferenceValue = slider;
            so.FindProperty("_hpText").objectReferenceValue = hpText;
            so.FindProperty("_blockText").objectReferenceValue = blockText;
            so.FindProperty("_blockIcon").objectReferenceValue = blockContainer;
            so.FindProperty("_intentIcon").objectReferenceValue = intentIcon;
            so.FindProperty("_intentValueText").objectReferenceValue = intentText;
            so.FindProperty("_highlightRing").objectReferenceValue = highlightGO;
            so.ApplyModifiedPropertiesWithoutUndo();

            // Create prefab directory if needed
            string prefabDir = "Assets/_Project/Prefabs/UI/Combat";
            if (!AssetDatabase.IsValidFolder(prefabDir))
            {
                System.IO.Directory.CreateDirectory(Application.dataPath + "/_Project/Prefabs/UI/Combat");
                AssetDatabase.Refresh();
            }

            // Save as prefab
            string prefabPath = $"{prefabDir}/EnemySlotUI.prefab";
            PrefabUtility.SaveAsPrefabAsset(root, prefabPath);

            // Clean up scene object
            Object.DestroyImmediate(root);

            AssetDatabase.Refresh();
            Debug.Log($"[EnemySlotUIPrefabGenerator] Prefab created at: {prefabPath}");

            // Select the prefab
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            Selection.activeObject = prefab;
            EditorGUIUtility.PingObject(prefab);
        }

        private static GameObject CreateChild(GameObject parent, string name)
        {
            var child = new GameObject(name);
            child.transform.SetParent(parent.transform, false);
            child.AddComponent<RectTransform>();
            child.AddComponent<CanvasRenderer>();
            return child;
        }
    }
}
#endif
