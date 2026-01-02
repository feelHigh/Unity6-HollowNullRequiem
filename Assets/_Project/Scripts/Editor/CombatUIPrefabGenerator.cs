// ============================================
// CombatUIPrefabGenerator.cs
// Editor script to generate EnemyFloatingUI and AllyIndicator prefabs
// ============================================

#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;

namespace HNR.Editor
{
    public static class CombatUIPrefabGenerator
    {
        private const string PrefabDir = "Assets/_Project/Prefabs/UI/Combat";

        /// <summary>
        /// Generates all Combat UI prefabs.
        /// </summary>
        public static void GenerateAll()
        {
            GenerateEnemyFloatingUI();
            GenerateAllyIndicator();
            GeneratePortraitCorruptionSlot();
            Debug.Log("[CombatUIPrefabGenerator] All Combat UI prefabs generated.");
        }

        /// <summary>
        /// Generates the PortraitCorruptionSlot prefab for SharedVitalityBar.
        /// Shows character portrait with a horizontal corruption slider below.
        /// </summary>
        public static void GeneratePortraitCorruptionSlot()
        {
            EnsureDirectoryExists();

            // Create root GameObject
            var root = new GameObject("PortraitCorruptionSlot");
            var rootRect = root.AddComponent<RectTransform>();
            rootRect.sizeDelta = new Vector2(48, 60); // 48x48 portrait + 6px bar + 6px spacing

            // ============================================
            // Portrait Frame (circular border)
            // ============================================
            var frameGO = CreateChild(root, "PortraitFrame");
            var frameRect = frameGO.GetComponent<RectTransform>();
            frameRect.anchorMin = new Vector2(0, 0.2f);
            frameRect.anchorMax = new Vector2(1, 1);
            frameRect.offsetMin = Vector2.zero;
            frameRect.offsetMax = Vector2.zero;
            var frameImage = frameGO.AddComponent<Image>();
            frameImage.color = new Color(0.4f, 0.4f, 0.5f, 1f); // Default gray frame

            // ============================================
            // Portrait Image
            // ============================================
            var portraitGO = CreateChild(root, "Portrait");
            var portraitRect = portraitGO.GetComponent<RectTransform>();
            portraitRect.anchorMin = new Vector2(0.05f, 0.22f);
            portraitRect.anchorMax = new Vector2(0.95f, 0.98f);
            portraitRect.offsetMin = Vector2.zero;
            portraitRect.offsetMax = Vector2.zero;
            var portraitImage = portraitGO.AddComponent<Image>();
            portraitImage.color = Color.white;
            portraitImage.preserveAspect = true;

            // ============================================
            // Corruption Bar Background
            // ============================================
            var corruptBgGO = CreateChild(root, "CorruptionBackground");
            var corruptBgRect = corruptBgGO.GetComponent<RectTransform>();
            corruptBgRect.anchorMin = new Vector2(0, 0);
            corruptBgRect.anchorMax = new Vector2(1, 0.15f);
            corruptBgRect.offsetMin = Vector2.zero;
            corruptBgRect.offsetMax = Vector2.zero;
            var corruptBgImage = corruptBgGO.AddComponent<Image>();
            corruptBgImage.color = new Color(0.15f, 0.15f, 0.2f, 0.9f);

            // ============================================
            // Corruption Slider
            // ============================================
            var corruptSliderGO = CreateChild(root, "CorruptionSlider");
            var corruptSliderRect = corruptSliderGO.GetComponent<RectTransform>();
            corruptSliderRect.anchorMin = new Vector2(0, 0);
            corruptSliderRect.anchorMax = new Vector2(1, 0.15f);
            corruptSliderRect.offsetMin = new Vector2(1, 1);
            corruptSliderRect.offsetMax = new Vector2(-1, -1);

            var corruptSlider = corruptSliderGO.AddComponent<Slider>();
            corruptSlider.direction = Slider.Direction.LeftToRight;
            corruptSlider.minValue = 0f;
            corruptSlider.maxValue = 1f;
            corruptSlider.value = 0f;
            corruptSlider.interactable = false;

            // Fill Area
            var fillAreaGO = CreateChild(corruptSliderGO, "Fill Area");
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
            fillImage.color = new Color(0.2f, 0.8f, 0.2f, 1f); // Start green (safe)
            corruptSlider.fillRect = fillRect;

            // ============================================
            // Add PortraitCorruptionSlot Component
            // ============================================
            var slotComponent = root.AddComponent<HNR.UI.Combat.PortraitCorruptionSlot>();

            // Wire up references via SerializedObject
            var so = new SerializedObject(slotComponent);
            so.FindProperty("_portrait").objectReferenceValue = portraitImage;
            so.FindProperty("_portraitFrame").objectReferenceValue = frameImage;
            so.FindProperty("_corruptionSlider").objectReferenceValue = corruptSlider;
            so.FindProperty("_corruptionFillImage").objectReferenceValue = fillImage;
            so.FindProperty("_corruptionBackground").objectReferenceValue = corruptBgImage;
            so.FindProperty("_fillSpeed").floatValue = 5f;
            so.FindProperty("_smoothTransition").boolValue = true;
            so.ApplyModifiedPropertiesWithoutUndo();

            // Save as prefab
            string prefabPath = $"{PrefabDir}/PortraitCorruptionSlot.prefab";
            PrefabUtility.SaveAsPrefabAsset(root, prefabPath);

            // Clean up scene object
            Object.DestroyImmediate(root);

            AssetDatabase.Refresh();
            Debug.Log($"[CombatUIPrefabGenerator] PortraitCorruptionSlot prefab created at: {prefabPath}");

            // Select the prefab
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            Selection.activeObject = prefab;
            EditorGUIUtility.PingObject(prefab);
        }

        /// <summary>
        /// Generates the EnemyFloatingUI prefab for world-space enemy HP and intent display.
        /// </summary>
        public static void GenerateEnemyFloatingUI()
        {
            EnsureDirectoryExists();

            // Create root GameObject with world space canvas
            var root = new GameObject("EnemyFloatingUI");

            // Add world space Canvas
            var canvas = root.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.sortingLayerName = "UI"; // Ensure UI renders above sprites
            canvas.sortingOrder = 100;

            var canvasScaler = root.AddComponent<CanvasScaler>();
            canvasScaler.dynamicPixelsPerUnit = 100;

            root.AddComponent<GraphicRaycaster>();

            var rootRect = root.GetComponent<RectTransform>();
            rootRect.sizeDelta = new Vector2(120, 60);
            rootRect.localScale = Vector3.one * 0.01f; // Scale down for world space

            // ============================================
            // HP Bar Container
            // ============================================
            var hpContainer = CreateChild(root, "HPBarContainer");
            var hpContainerRect = hpContainer.GetComponent<RectTransform>();
            hpContainerRect.anchorMin = new Vector2(0, 0.5f);
            hpContainerRect.anchorMax = new Vector2(1, 0.8f);
            hpContainerRect.offsetMin = new Vector2(5, 0);
            hpContainerRect.offsetMax = new Vector2(-5, 0);

            // HP Bar Background
            var hpBgGO = CreateChild(hpContainer, "HPBackground");
            var hpBgRect = hpBgGO.GetComponent<RectTransform>();
            hpBgRect.anchorMin = Vector2.zero;
            hpBgRect.anchorMax = Vector2.one;
            hpBgRect.offsetMin = Vector2.zero;
            hpBgRect.offsetMax = Vector2.zero;
            var hpBgImage = hpBgGO.AddComponent<Image>();
            hpBgImage.color = new Color(0.15f, 0.15f, 0.2f, 0.9f);

            // HP Slider
            var hpSliderGO = CreateChild(hpContainer, "HPSlider");
            var hpSliderRect = hpSliderGO.GetComponent<RectTransform>();
            hpSliderRect.anchorMin = Vector2.zero;
            hpSliderRect.anchorMax = Vector2.one;
            hpSliderRect.offsetMin = new Vector2(2, 2);
            hpSliderRect.offsetMax = new Vector2(-2, -2);

            var hpSlider = hpSliderGO.AddComponent<Slider>();
            hpSlider.direction = Slider.Direction.LeftToRight;
            hpSlider.minValue = 0f;
            hpSlider.maxValue = 1f;
            hpSlider.value = 1f;
            hpSlider.interactable = false;

            // HP Fill Area
            var hpFillAreaGO = CreateChild(hpSliderGO, "Fill Area");
            var hpFillAreaRect = hpFillAreaGO.GetComponent<RectTransform>();
            hpFillAreaRect.anchorMin = Vector2.zero;
            hpFillAreaRect.anchorMax = Vector2.one;
            hpFillAreaRect.offsetMin = Vector2.zero;
            hpFillAreaRect.offsetMax = Vector2.zero;

            // HP Fill
            var hpFillGO = CreateChild(hpFillAreaGO, "Fill");
            var hpFillRect = hpFillGO.GetComponent<RectTransform>();
            hpFillRect.anchorMin = Vector2.zero;
            hpFillRect.anchorMax = Vector2.one;
            hpFillRect.offsetMin = Vector2.zero;
            hpFillRect.offsetMax = Vector2.zero;
            var hpFillImage = hpFillGO.AddComponent<Image>();
            hpFillImage.color = new Color(0.9f, 0.2f, 0.2f, 1f);
            hpSlider.fillRect = hpFillRect;

            // HP Text
            var hpTextGO = CreateChild(hpContainer, "HPText");
            var hpTextRect = hpTextGO.GetComponent<RectTransform>();
            hpTextRect.anchorMin = Vector2.zero;
            hpTextRect.anchorMax = Vector2.one;
            hpTextRect.offsetMin = Vector2.zero;
            hpTextRect.offsetMax = Vector2.zero;
            var hpText = hpTextGO.AddComponent<TextMeshProUGUI>();
            hpText.text = "30";
            hpText.fontSize = 12;
            hpText.alignment = TextAlignmentOptions.Center;
            hpText.color = Color.white;
            hpText.fontStyle = FontStyles.Bold;

            // ============================================
            // Intent Container (Diamond shape)
            // ============================================
            var intentContainer = CreateChild(root, "IntentContainer");
            var intentContainerRect = intentContainer.GetComponent<RectTransform>();
            intentContainerRect.anchorMin = new Vector2(0.3f, 0);
            intentContainerRect.anchorMax = new Vector2(0.7f, 0.45f);
            intentContainerRect.offsetMin = Vector2.zero;
            intentContainerRect.offsetMax = Vector2.zero;

            // Intent Diamond Background (rotated 45 degrees for diamond shape)
            var intentDiamondGO = CreateChild(intentContainer, "IntentDiamond");
            var intentDiamondRect = intentDiamondGO.GetComponent<RectTransform>();
            intentDiamondRect.anchorMin = new Vector2(0.15f, 0.15f);
            intentDiamondRect.anchorMax = new Vector2(0.85f, 0.85f);
            intentDiamondRect.offsetMin = Vector2.zero;
            intentDiamondRect.offsetMax = Vector2.zero;
            intentDiamondRect.localRotation = Quaternion.Euler(0, 0, 45);
            var intentDiamond = intentDiamondGO.AddComponent<Image>();
            intentDiamond.color = new Color(1f, 0.3f, 0.3f, 0.9f);

            // Intent Icon
            var intentIconGO = CreateChild(intentContainer, "IntentIcon");
            var intentIconRect = intentIconGO.GetComponent<RectTransform>();
            intentIconRect.anchorMin = new Vector2(0.25f, 0.25f);
            intentIconRect.anchorMax = new Vector2(0.75f, 0.75f);
            intentIconRect.offsetMin = Vector2.zero;
            intentIconRect.offsetMax = Vector2.zero;
            var intentIcon = intentIconGO.AddComponent<Image>();
            intentIcon.color = Color.white;
            intentIcon.preserveAspect = true;

            // Intent Value Text
            var intentValueGO = CreateChild(intentContainer, "IntentValue");
            var intentValueRect = intentValueGO.GetComponent<RectTransform>();
            intentValueRect.anchorMin = new Vector2(0, -0.3f);
            intentValueRect.anchorMax = new Vector2(1, 0.15f);
            intentValueRect.offsetMin = Vector2.zero;
            intentValueRect.offsetMax = Vector2.zero;
            var intentValue = intentValueGO.AddComponent<TextMeshProUGUI>();
            intentValue.text = "12";
            intentValue.fontSize = 14;
            intentValue.fontStyle = FontStyles.Bold;
            intentValue.alignment = TextAlignmentOptions.Center;
            intentValue.color = Color.white;

            // Intent Countdown (for multi-turn attacks)
            var intentCountdownGO = CreateChild(intentContainer, "IntentCountdown");
            var intentCountdownRect = intentCountdownGO.GetComponent<RectTransform>();
            intentCountdownRect.anchorMin = new Vector2(0.7f, 0.7f);
            intentCountdownRect.anchorMax = new Vector2(1.1f, 1.1f);
            intentCountdownRect.offsetMin = Vector2.zero;
            intentCountdownRect.offsetMax = Vector2.zero;
            var intentCountdown = intentCountdownGO.AddComponent<TextMeshProUGUI>();
            intentCountdown.text = "2";
            intentCountdown.fontSize = 10;
            intentCountdown.fontStyle = FontStyles.Bold;
            intentCountdown.alignment = TextAlignmentOptions.Center;
            intentCountdown.color = new Color(1f, 0.9f, 0.4f, 1f);
            intentCountdownGO.SetActive(false);

            // ============================================
            // Add EnemyFloatingUI Component
            // ============================================
            var enemyFloatingUI = root.AddComponent<HNR.UI.Combat.EnemyFloatingUI>();

            // Wire up references via SerializedObject
            var so = new SerializedObject(enemyFloatingUI);
            so.FindProperty("_offset").vector3Value = new Vector3(0, 3f, 0);
            so.FindProperty("_hpSlider").objectReferenceValue = hpSlider;
            so.FindProperty("_hpFillImage").objectReferenceValue = hpFillImage;
            so.FindProperty("_hpBarBackground").objectReferenceValue = hpBgImage;
            so.FindProperty("_hpText").objectReferenceValue = hpText;
            so.FindProperty("_intentContainer").objectReferenceValue = intentContainerRect;
            so.FindProperty("_intentDiamond").objectReferenceValue = intentDiamond;
            so.FindProperty("_intentIcon").objectReferenceValue = intentIcon;
            so.FindProperty("_intentValue").objectReferenceValue = intentValue;
            so.FindProperty("_intentCountdown").objectReferenceValue = intentCountdown;
            so.FindProperty("_intentAppearDuration").floatValue = 0.3f;
            so.FindProperty("_pulseDuration").floatValue = 0.5f;
            so.ApplyModifiedPropertiesWithoutUndo();

            // Save as prefab
            string prefabPath = $"{PrefabDir}/EnemyFloatingUI.prefab";
            PrefabUtility.SaveAsPrefabAsset(root, prefabPath);

            // Clean up scene object
            Object.DestroyImmediate(root);

            AssetDatabase.Refresh();
            Debug.Log($"[CombatUIPrefabGenerator] EnemyFloatingUI prefab created at: {prefabPath}");

            // Select the prefab
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            Selection.activeObject = prefab;
            EditorGUIUtility.PingObject(prefab);
        }

        /// <summary>
        /// Generates the AllyIndicator prefab for world-space ally identification.
        /// </summary>
        public static void GenerateAllyIndicator()
        {
            EnsureDirectoryExists();

            // Create root GameObject with world space canvas
            var root = new GameObject("AllyIndicator");

            // Add world space Canvas
            var canvas = root.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.sortingOrder = 90;

            var canvasScaler = root.AddComponent<CanvasScaler>();
            canvasScaler.dynamicPixelsPerUnit = 100;

            root.AddComponent<GraphicRaycaster>();

            var rootRect = root.GetComponent<RectTransform>();
            rootRect.sizeDelta = new Vector2(50, 50);
            rootRect.localScale = Vector3.one * 0.01f; // Scale down for world space

            // ============================================
            // Circle Frame
            // ============================================
            var frameGO = CreateChild(root, "CircleFrame");
            var frameRect = frameGO.GetComponent<RectTransform>();
            frameRect.anchorMin = Vector2.zero;
            frameRect.anchorMax = Vector2.one;
            frameRect.offsetMin = Vector2.zero;
            frameRect.offsetMax = Vector2.zero;
            var frameImage = frameGO.AddComponent<Image>();
            frameImage.color = new Color(0.5f, 0.5f, 0.5f, 0.8f);
            // Note: For circular frame, assign a circular sprite in Inspector

            // ============================================
            // Mini Portrait
            // ============================================
            var portraitGO = CreateChild(root, "MiniPortrait");
            var portraitRect = portraitGO.GetComponent<RectTransform>();
            portraitRect.anchorMin = new Vector2(0.1f, 0.1f);
            portraitRect.anchorMax = new Vector2(0.9f, 0.9f);
            portraitRect.offsetMin = Vector2.zero;
            portraitRect.offsetMax = Vector2.zero;
            var portraitImage = portraitGO.AddComponent<Image>();
            portraitImage.color = Color.white;
            portraitImage.preserveAspect = true;

            // ============================================
            // Add AllyIndicator Component
            // ============================================
            var allyIndicator = root.AddComponent<HNR.UI.Combat.AllyIndicator>();

            // Wire up references via SerializedObject
            var so = new SerializedObject(allyIndicator);
            so.FindProperty("_miniPortrait").objectReferenceValue = portraitImage;
            so.FindProperty("_circleFrame").objectReferenceValue = frameImage;
            so.FindProperty("_indicatorSize").floatValue = 40f;
            so.FindProperty("_offsetFromModel").vector3Value = new Vector3(0, -0.5f, 0);
            so.ApplyModifiedPropertiesWithoutUndo();

            // Save as prefab
            string prefabPath = $"{PrefabDir}/AllyIndicator.prefab";
            PrefabUtility.SaveAsPrefabAsset(root, prefabPath);

            // Clean up scene object
            Object.DestroyImmediate(root);

            AssetDatabase.Refresh();
            Debug.Log($"[CombatUIPrefabGenerator] AllyIndicator prefab created at: {prefabPath}");

            // Select the prefab
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            Selection.activeObject = prefab;
            EditorGUIUtility.PingObject(prefab);
        }

        private static void EnsureDirectoryExists()
        {
            if (!AssetDatabase.IsValidFolder(PrefabDir))
            {
                System.IO.Directory.CreateDirectory(Application.dataPath + "/_Project/Prefabs/UI/Combat");
                AssetDatabase.Refresh();
            }
        }

        private static GameObject CreateChild(GameObject parent, string name)
        {
            var child = new GameObject(name);
            child.transform.SetParent(parent.transform, false);
            child.AddComponent<RectTransform>();
            return child;
        }
    }
}
#endif
