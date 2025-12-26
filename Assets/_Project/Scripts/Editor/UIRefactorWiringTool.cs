// ============================================
// UIRefactorWiringTool.cs
// Wires up UI elements added in the UI refactor
// ============================================

using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using TMPro;
using System.IO;
using HNR.UI;
using HNR.UI.Screens;
using HNR.UI.Combat;
using HNR.Map;

namespace HNR.Editor
{
    /// <summary>
    /// Editor tool for wiring up UI elements added in UI refactor:
    /// - SharedVitalityBar block indicator
    /// - MapScreen zone header
    /// - ShopScreen service buttons
    /// - TreasureScreen prefab
    /// </summary>
    public static class UIRefactorWiringTool
    {
        private const string PREFABS_PATH = "Assets/_Project/Prefabs";
        private const string SCENES_PATH = "Assets/_Project/Scenes";

        // ============================================
        // Main Entry Points
        // ============================================

        /// <summary>
        /// Runs all UI refactor wiring operations.
        /// </summary>
        public static void WireAllUIRefactorElements()
        {
            int totalWired = 0;

            Debug.Log("[UIRefactorWiringTool] Starting UI refactor wiring...");

            // Wire Combat scene elements
            totalWired += WireCombatSceneElements();

            // Wire NullRift scene elements
            totalWired += WireNullRiftSceneElements();

            // Create TreasureScreen prefab if needed
            totalWired += CreateTreasureScreenPrefab();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"[UIRefactorWiringTool] Complete! Wired {totalWired} elements.");
            EditorUtility.DisplayDialog("UI Refactor Wiring Complete",
                $"Successfully wired {totalWired} UI elements.\n\n" +
                "Please verify in Unity Editor:\n" +
                "• Combat Scene: SharedVitalityBar block indicator\n" +
                "• NullRift Scene: MapScreen zone header\n" +
                "• NullRift Scene: ShopScreen service buttons",
                "OK");
        }

        // ============================================
        // Combat Scene Wiring
        // ============================================

        public static int WireCombatSceneElements()
        {
            string scenePath = $"{SCENES_PATH}/Combat.unity";
            if (!File.Exists(scenePath))
            {
                Debug.LogWarning($"[UIRefactorWiringTool] Combat scene not found at {scenePath}");
                return 0;
            }

            var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Additive);
            int wired = 0;

            try
            {
                // Find SharedVitalityBar in scene
                var vitalityBar = Object.FindFirstObjectByType<SharedVitalityBar>();
                if (vitalityBar != null)
                {
                    wired += WireVitalityBarBlockIndicator(vitalityBar);
                }
                else
                {
                    Debug.LogWarning("[UIRefactorWiringTool] SharedVitalityBar not found in Combat scene");
                }

                if (wired > 0)
                {
                    EditorSceneManager.MarkSceneDirty(scene);
                    EditorSceneManager.SaveScene(scene);
                }
            }
            finally
            {
                EditorSceneManager.CloseScene(scene, true);
            }

            return wired;
        }

        private static int WireVitalityBarBlockIndicator(SharedVitalityBar vitalityBar)
        {
            SerializedObject so = new SerializedObject(vitalityBar);
            int wired = 0;

            // Check if block container already exists
            var blockContainerProp = so.FindProperty("_blockContainer");
            if (blockContainerProp != null && blockContainerProp.objectReferenceValue != null)
            {
                Debug.Log("[UIRefactorWiringTool] Block indicator already wired");
                return 0;
            }

            // Find or create block indicator elements
            Transform barTransform = vitalityBar.transform;

            // Look for existing BlockContainer
            Transform blockContainer = barTransform.Find("BlockContainer");
            if (blockContainer == null)
            {
                // Create block indicator UI
                blockContainer = CreateBlockIndicator(barTransform);
                wired++;
                Debug.Log("[UIRefactorWiringTool] Created block indicator for SharedVitalityBar");
            }

            // Wire the references
            if (blockContainer != null)
            {
                blockContainerProp.objectReferenceValue = blockContainer.gameObject;

                var shieldIconProp = so.FindProperty("_shieldIcon");
                var blockTextProp = so.FindProperty("_blockText");

                var shieldIcon = blockContainer.Find("ShieldIcon")?.GetComponent<Image>();
                var blockText = blockContainer.Find("BlockText")?.GetComponent<TMP_Text>();

                if (shieldIconProp != null && shieldIcon != null)
                    shieldIconProp.objectReferenceValue = shieldIcon;

                if (blockTextProp != null && blockText != null)
                    blockTextProp.objectReferenceValue = blockText;

                so.ApplyModifiedProperties();
                wired++;
            }

            return wired;
        }

        private static Transform CreateBlockIndicator(Transform parent)
        {
            // Create BlockContainer
            GameObject container = new GameObject("BlockContainer");
            container.transform.SetParent(parent, false);

            RectTransform containerRect = container.AddComponent<RectTransform>();
            containerRect.anchorMin = new Vector2(1, 0.5f);
            containerRect.anchorMax = new Vector2(1, 0.5f);
            containerRect.pivot = new Vector2(0, 0.5f);
            containerRect.anchoredPosition = new Vector2(10, 0);
            containerRect.sizeDelta = new Vector2(60, 30);

            var layout = container.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = 5;
            layout.childAlignment = TextAnchor.MiddleLeft;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;

            // Shield icon
            GameObject iconObj = new GameObject("ShieldIcon");
            iconObj.transform.SetParent(container.transform, false);
            RectTransform iconRect = iconObj.AddComponent<RectTransform>();
            iconRect.sizeDelta = new Vector2(24, 24);
            Image iconImage = iconObj.AddComponent<Image>();
            iconImage.color = new Color(0.2f, 0.6f, 0.86f, 1f); // Cyan #3498DB
            var iconLayout = iconObj.AddComponent<LayoutElement>();
            iconLayout.preferredWidth = 24;
            iconLayout.preferredHeight = 24;

            // Block text
            GameObject textObj = new GameObject("BlockText");
            textObj.transform.SetParent(container.transform, false);
            RectTransform textRect = textObj.AddComponent<RectTransform>();
            textRect.sizeDelta = new Vector2(30, 24);
            TMP_Text text = textObj.AddComponent<TextMeshProUGUI>();
            text.text = "0";
            text.fontSize = 18;
            text.fontStyle = FontStyles.Bold;
            text.alignment = TextAlignmentOptions.Left;
            text.color = new Color(0.2f, 0.6f, 0.86f, 1f); // Cyan #3498DB

            // Start hidden
            container.SetActive(false);

            return container.transform;
        }

        // ============================================
        // NullRift Scene Wiring
        // ============================================

        public static int WireNullRiftSceneElements()
        {
            string scenePath = $"{SCENES_PATH}/NullRift.unity";
            if (!File.Exists(scenePath))
            {
                Debug.LogWarning($"[UIRefactorWiringTool] NullRift scene not found at {scenePath}");
                return 0;
            }

            var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Additive);
            int wired = 0;

            try
            {
                // Find MapScreen and wire zone header
                var mapScreen = Object.FindFirstObjectByType<MapScreen>();
                if (mapScreen != null)
                {
                    wired += WireMapScreenZoneHeader(mapScreen);
                }
                else
                {
                    Debug.LogWarning("[UIRefactorWiringTool] MapScreen not found in NullRift scene");
                }

                // Find ShopScreen and wire service buttons
                var shopScreen = Object.FindFirstObjectByType<ShopScreen>();
                if (shopScreen != null)
                {
                    wired += WireShopScreenServiceButtons(shopScreen);
                }
                else
                {
                    Debug.LogWarning("[UIRefactorWiringTool] ShopScreen not found in NullRift scene");
                }

                if (wired > 0)
                {
                    EditorSceneManager.MarkSceneDirty(scene);
                    EditorSceneManager.SaveScene(scene);
                }
            }
            finally
            {
                EditorSceneManager.CloseScene(scene, true);
            }

            return wired;
        }

        private static int WireMapScreenZoneHeader(MapScreen mapScreen)
        {
            SerializedObject so = new SerializedObject(mapScreen);
            int wired = 0;

            // Check if zone header already wired
            var zoneTitleProp = so.FindProperty("_zoneTitle");
            if (zoneTitleProp != null && zoneTitleProp.objectReferenceValue != null)
            {
                Debug.Log("[UIRefactorWiringTool] Zone header already wired");
                return 0;
            }

            // Find or create zone header
            Transform screenTransform = mapScreen.transform;
            Transform header = screenTransform.Find("ZoneHeader");

            if (header == null)
            {
                header = CreateZoneHeader(screenTransform);
                wired++;
                Debug.Log("[UIRefactorWiringTool] Created zone header for MapScreen");
            }

            // Wire references
            if (header != null)
            {
                var zoneSubtitleProp = so.FindProperty("_zoneSubtitle");
                var hpTextProp = so.FindProperty("_hpText");
                var hpIconProp = so.FindProperty("_hpIcon");
                var currencyTextProp = so.FindProperty("_currencyText");
                var currencyIconProp = so.FindProperty("_currencyIcon");

                if (zoneTitleProp != null)
                    zoneTitleProp.objectReferenceValue = header.Find("TitleContainer/ZoneTitle")?.GetComponent<TMP_Text>();
                if (zoneSubtitleProp != null)
                    zoneSubtitleProp.objectReferenceValue = header.Find("TitleContainer/ZoneSubtitle")?.GetComponent<TMP_Text>();
                if (hpTextProp != null)
                    hpTextProp.objectReferenceValue = header.Find("StatsContainer/HPContainer/HPText")?.GetComponent<TMP_Text>();
                if (hpIconProp != null)
                    hpIconProp.objectReferenceValue = header.Find("StatsContainer/HPContainer/HPIcon")?.GetComponent<Image>();
                if (currencyTextProp != null)
                    currencyTextProp.objectReferenceValue = header.Find("StatsContainer/CurrencyContainer/CurrencyText")?.GetComponent<TMP_Text>();
                if (currencyIconProp != null)
                    currencyIconProp.objectReferenceValue = header.Find("StatsContainer/CurrencyContainer/CurrencyIcon")?.GetComponent<Image>();

                so.ApplyModifiedProperties();
                wired++;
            }

            return wired;
        }

        private static Transform CreateZoneHeader(Transform parent)
        {
            // Create header container
            GameObject header = new GameObject("ZoneHeader");
            header.transform.SetParent(parent, false);
            header.transform.SetAsFirstSibling();

            RectTransform headerRect = header.AddComponent<RectTransform>();
            headerRect.anchorMin = new Vector2(0, 1);
            headerRect.anchorMax = new Vector2(1, 1);
            headerRect.pivot = new Vector2(0.5f, 1);
            headerRect.anchoredPosition = Vector2.zero;
            headerRect.sizeDelta = new Vector2(0, 60);

            Image headerBg = header.AddComponent<Image>();
            headerBg.color = new Color(0.08f, 0.06f, 0.12f, 0.9f);

            var headerLayout = header.AddComponent<HorizontalLayoutGroup>();
            headerLayout.padding = new RectOffset(20, 20, 10, 10);
            headerLayout.spacing = 20;
            headerLayout.childAlignment = TextAnchor.MiddleCenter;
            headerLayout.childForceExpandWidth = true;
            headerLayout.childForceExpandHeight = true;

            // Title container (left side)
            GameObject titleContainer = new GameObject("TitleContainer");
            titleContainer.transform.SetParent(header.transform, false);
            var titleLayout = titleContainer.AddComponent<VerticalLayoutGroup>();
            titleLayout.childAlignment = TextAnchor.MiddleLeft;
            titleLayout.childForceExpandWidth = false;
            titleLayout.childForceExpandHeight = false;
            var titleLayoutElement = titleContainer.AddComponent<LayoutElement>();
            titleLayoutElement.flexibleWidth = 1;

            // Zone title
            CreateTextElement(titleContainer.transform, "ZoneTitle", "NULL RIFT", 18, FontStyles.Bold,
                new Color(0.8f, 0.7f, 0.9f));

            // Zone subtitle
            CreateTextElement(titleContainer.transform, "ZoneSubtitle", "Zone 1 • The Outer Reaches", 12, FontStyles.Normal,
                new Color(0.6f, 0.5f, 0.7f));

            // Stats container (right side)
            GameObject statsContainer = new GameObject("StatsContainer");
            statsContainer.transform.SetParent(header.transform, false);
            var statsLayout = statsContainer.AddComponent<HorizontalLayoutGroup>();
            statsLayout.spacing = 20;
            statsLayout.childAlignment = TextAnchor.MiddleRight;
            statsLayout.childForceExpandWidth = false;
            statsLayout.childForceExpandHeight = false;

            // HP container
            CreateStatDisplay(statsContainer.transform, "HPContainer", "HPIcon", "HPText",
                new Color(0.8f, 0.2f, 0.2f), "100/100");

            // Currency container
            CreateStatDisplay(statsContainer.transform, "CurrencyContainer", "CurrencyIcon", "CurrencyText",
                new Color(0.6f, 0.4f, 0.8f), "50");

            return header.transform;
        }

        private static void CreateStatDisplay(Transform parent, string containerName, string iconName, string textName,
            Color iconColor, string defaultText)
        {
            GameObject container = new GameObject(containerName);
            container.transform.SetParent(parent, false);
            var layout = container.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = 5;
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;

            // Icon
            GameObject icon = new GameObject(iconName);
            icon.transform.SetParent(container.transform, false);
            var iconRect = icon.AddComponent<RectTransform>();
            iconRect.sizeDelta = new Vector2(20, 20);
            var iconImage = icon.AddComponent<Image>();
            iconImage.color = iconColor;
            var iconLayoutElement = icon.AddComponent<LayoutElement>();
            iconLayoutElement.preferredWidth = 20;
            iconLayoutElement.preferredHeight = 20;

            // Text
            CreateTextElement(container.transform, textName, defaultText, 14, FontStyles.Bold, Color.white);
        }

        private static void CreateTextElement(Transform parent, string name, string text, int fontSize,
            FontStyles style, Color color)
        {
            GameObject textObj = new GameObject(name);
            textObj.transform.SetParent(parent, false);
            var textComponent = textObj.AddComponent<TextMeshProUGUI>();
            textComponent.text = text;
            textComponent.fontSize = fontSize;
            textComponent.fontStyle = style;
            textComponent.color = color;
            textComponent.alignment = TextAlignmentOptions.Left;
        }

        private static int WireShopScreenServiceButtons(ShopScreen shopScreen)
        {
            SerializedObject so = new SerializedObject(shopScreen);
            int wired = 0;

            // Check if service buttons already wired
            var removeCardBtnProp = so.FindProperty("_removeCardButton");
            if (removeCardBtnProp != null && removeCardBtnProp.objectReferenceValue != null)
            {
                Debug.Log("[UIRefactorWiringTool] Shop service buttons already wired");
                return 0;
            }

            // Find or create service buttons
            Transform screenTransform = shopScreen.transform;
            Transform footer = screenTransform.Find("ServiceButtons");

            if (footer == null)
            {
                footer = CreateShopServiceButtons(screenTransform);
                wired++;
                Debug.Log("[UIRefactorWiringTool] Created service buttons for ShopScreen");
            }

            // Wire references
            if (footer != null)
            {
                var removeCardCostTextProp = so.FindProperty("_removeCardCostText");
                var purifyBtnProp = so.FindProperty("_purifyButton");
                var purifyCostTextProp = so.FindProperty("_purifyCostText");

                if (removeCardBtnProp != null)
                    removeCardBtnProp.objectReferenceValue = footer.Find("RemoveCardButton")?.GetComponent<Button>();
                if (removeCardCostTextProp != null)
                    removeCardCostTextProp.objectReferenceValue = footer.Find("RemoveCardButton/Text")?.GetComponent<TMP_Text>();
                if (purifyBtnProp != null)
                    purifyBtnProp.objectReferenceValue = footer.Find("PurifyButton")?.GetComponent<Button>();
                if (purifyCostTextProp != null)
                    purifyCostTextProp.objectReferenceValue = footer.Find("PurifyButton/Text")?.GetComponent<TMP_Text>();

                so.ApplyModifiedProperties();
                wired++;
            }

            return wired;
        }

        private static Transform CreateShopServiceButtons(Transform parent)
        {
            GameObject container = new GameObject("ServiceButtons");
            container.transform.SetParent(parent, false);

            RectTransform containerRect = container.AddComponent<RectTransform>();
            containerRect.anchorMin = new Vector2(0, 0);
            containerRect.anchorMax = new Vector2(1, 0);
            containerRect.pivot = new Vector2(0.5f, 0);
            containerRect.anchoredPosition = new Vector2(0, 20);
            containerRect.sizeDelta = new Vector2(-40, 50);

            var layout = container.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = 20;
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = true;

            // Remove Card button
            CreateServiceButton(container.transform, "RemoveCardButton", "Remove Card (75)",
                new Color(0.6f, 0.3f, 0.3f));

            // Purify button
            CreateServiceButton(container.transform, "PurifyButton", "Purify -30 (50)",
                new Color(0f, 0.6f, 0.7f));

            return container.transform;
        }

        private static void CreateServiceButton(Transform parent, string name, string text, Color bgColor)
        {
            GameObject buttonObj = new GameObject(name);
            buttonObj.transform.SetParent(parent, false);

            var buttonRect = buttonObj.AddComponent<RectTransform>();
            buttonRect.sizeDelta = new Vector2(150, 40);

            var buttonImage = buttonObj.AddComponent<Image>();
            buttonImage.color = bgColor;

            var button = buttonObj.AddComponent<Button>();
            button.targetGraphic = buttonImage;

            var layoutElement = buttonObj.AddComponent<LayoutElement>();
            layoutElement.preferredHeight = 40;
            layoutElement.flexibleWidth = 1;

            // Button text
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(buttonObj.transform, false);
            var textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.sizeDelta = Vector2.zero;

            var textComponent = textObj.AddComponent<TextMeshProUGUI>();
            textComponent.text = text;
            textComponent.fontSize = 14;
            textComponent.fontStyle = FontStyles.Bold;
            textComponent.color = Color.white;
            textComponent.alignment = TextAlignmentOptions.Center;
        }

        // ============================================
        // TreasureScreen Prefab
        // ============================================

        public static int CreateTreasureScreenPrefab()
        {
            string prefabPath = $"{PREFABS_PATH}/UI/Screens/TreasureScreen.prefab";

            if (File.Exists(prefabPath))
            {
                Debug.Log("[UIRefactorWiringTool] TreasureScreen prefab already exists");
                return 0;
            }

            EnsureDirectoryExists(prefabPath);

            // Create screen root
            GameObject screenObj = new GameObject("TreasureScreen");
            screenObj.AddComponent<RectTransform>();
            var treasureScreen = screenObj.AddComponent<TreasureScreen>();
            var canvasGroup = screenObj.AddComponent<CanvasGroup>();

            // Background
            GameObject bg = new GameObject("Background");
            bg.transform.SetParent(screenObj.transform, false);
            var bgRect = bg.AddComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.sizeDelta = Vector2.zero;
            var bgImage = bg.AddComponent<Image>();
            bgImage.color = new Color(0.08f, 0.06f, 0.1f, 0.95f);

            // Title
            GameObject title = new GameObject("TitleText");
            title.transform.SetParent(screenObj.transform, false);
            var titleRect = title.AddComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0.5f, 0.85f);
            titleRect.anchorMax = new Vector2(0.5f, 0.95f);
            titleRect.sizeDelta = new Vector2(300, 50);
            var titleText = title.AddComponent<TextMeshProUGUI>();
            titleText.text = "TREASURE";
            titleText.fontSize = 28;
            titleText.fontStyle = FontStyles.Bold;
            titleText.alignment = TextAlignmentOptions.Center;
            titleText.color = new Color(0.83f, 0.69f, 0.22f); // Gold

            // Subtitle
            GameObject subtitle = new GameObject("SubtitleText");
            subtitle.transform.SetParent(screenObj.transform, false);
            var subtitleRect = subtitle.AddComponent<RectTransform>();
            subtitleRect.anchorMin = new Vector2(0.5f, 0.78f);
            subtitleRect.anchorMax = new Vector2(0.5f, 0.85f);
            subtitleRect.sizeDelta = new Vector2(300, 30);
            var subtitleText = subtitle.AddComponent<TextMeshProUGUI>();
            subtitleText.text = "Choose a card reward:";
            subtitleText.fontSize = 16;
            subtitleText.alignment = TextAlignmentOptions.Center;
            subtitleText.color = Color.white;

            // Card reward container
            GameObject cardContainer = new GameObject("CardRewardContainer");
            cardContainer.transform.SetParent(screenObj.transform, false);
            var containerRect = cardContainer.AddComponent<RectTransform>();
            containerRect.anchorMin = new Vector2(0.1f, 0.35f);
            containerRect.anchorMax = new Vector2(0.9f, 0.75f);
            containerRect.sizeDelta = Vector2.zero;
            var containerLayout = cardContainer.AddComponent<HorizontalLayoutGroup>();
            containerLayout.spacing = 20;
            containerLayout.childAlignment = TextAnchor.MiddleCenter;
            containerLayout.childForceExpandWidth = false;
            containerLayout.childForceExpandHeight = true;

            // Skip button
            GameObject skipBtn = CreateSimpleButton(screenObj.transform, "SkipRewardButton", "Skip Reward",
                new Vector2(0.5f, 0.25f), new Vector2(150, 40), new Color(0.4f, 0.35f, 0.45f));

            // Continue button
            GameObject continueBtn = CreateSimpleButton(screenObj.transform, "ContinueButton", "Continue",
                new Vector2(0.5f, 0.15f), new Vector2(150, 40), new Color(0.3f, 0.5f, 0.4f));

            // Wire references
            SerializedObject so = new SerializedObject(treasureScreen);
            so.FindProperty("_titleText").objectReferenceValue = titleText;
            so.FindProperty("_subtitleText").objectReferenceValue = subtitleText;
            so.FindProperty("_cardRewardContainer").objectReferenceValue = cardContainer.transform;
            so.FindProperty("_skipRewardButton").objectReferenceValue = skipBtn.GetComponent<Button>();
            so.FindProperty("_continueButton").objectReferenceValue = continueBtn.GetComponent<Button>();
            so.ApplyModifiedPropertiesWithoutUndo();

            // Save prefab
            bool success;
            PrefabUtility.SaveAsPrefabAsset(screenObj, prefabPath, out success);
            Object.DestroyImmediate(screenObj);

            if (success)
            {
                Debug.Log($"[UIRefactorWiringTool] Created TreasureScreen prefab at {prefabPath}");
                return 1;
            }

            Debug.LogError("[UIRefactorWiringTool] Failed to create TreasureScreen prefab");
            return 0;
        }

        private static GameObject CreateSimpleButton(Transform parent, string name, string text,
            Vector2 anchorPos, Vector2 size, Color bgColor)
        {
            GameObject buttonObj = new GameObject(name);
            buttonObj.transform.SetParent(parent, false);

            var buttonRect = buttonObj.AddComponent<RectTransform>();
            buttonRect.anchorMin = anchorPos;
            buttonRect.anchorMax = anchorPos;
            buttonRect.sizeDelta = size;

            var buttonImage = buttonObj.AddComponent<Image>();
            buttonImage.color = bgColor;

            var button = buttonObj.AddComponent<Button>();
            button.targetGraphic = buttonImage;

            // Button text
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(buttonObj.transform, false);
            var textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.sizeDelta = Vector2.zero;

            var textComponent = textObj.AddComponent<TextMeshProUGUI>();
            textComponent.text = text;
            textComponent.fontSize = 14;
            textComponent.fontStyle = FontStyles.Bold;
            textComponent.color = Color.white;
            textComponent.alignment = TextAlignmentOptions.Center;

            return buttonObj;
        }

        private static void EnsureDirectoryExists(string filePath)
        {
            string directory = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
                AssetDatabase.Refresh();
            }
        }
    }
}
