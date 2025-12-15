// ============================================
// MetaGameUIPrefabGenerator.cs
// Editor tool to generate GlobalHeader, GlobalNavDock, Toast prefabs
// ============================================

#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;
using HNR.UI;
using HNR.UI.Toast;
using HNR.UI.Components;

namespace HNR.Editor
{
    /// <summary>
    /// Generates meta-game UI prefabs for GlobalHeader, GlobalNavDock, and Toast.
    /// Access via HNR > Generate Meta-Game UI Prefabs
    /// </summary>
    public static class MetaGameUIPrefabGenerator
    {
        private const string PREFAB_PATH = "Assets/_Project/Prefabs/UI";

        [MenuItem("HNR/Generate Meta-Game UI Prefabs")]
        public static void GenerateAllPrefabs()
        {
            EnsureDirectoryExists();

            GenerateToastPrefab();
            GenerateGlobalHeaderPrefab();
            GenerateGlobalNavDockPrefab();
            GenerateCurrencyTickerPrefab();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("[MetaGameUIPrefabGenerator] All prefabs generated successfully!");
        }

        [MenuItem("HNR/Generate Meta-Game UI Prefabs/Toast Only")]
        public static void GenerateToastPrefab()
        {
            EnsureDirectoryExists();

            // Create Toast prefab
            var toastRoot = new GameObject("Toast");
            var rectTransform = toastRoot.AddComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(400, 60);

            // CanvasGroup for fading
            var canvasGroup = toastRoot.AddComponent<CanvasGroup>();

            // Background
            var bg = new GameObject("Background");
            bg.transform.SetParent(toastRoot.transform, false);
            var bgRect = bg.AddComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.sizeDelta = Vector2.zero;
            var bgImage = bg.AddComponent<Image>();
            bgImage.color = new Color(0.15f, 0.15f, 0.2f, 0.95f);

            // Icon
            var icon = new GameObject("Icon");
            icon.transform.SetParent(toastRoot.transform, false);
            var iconRect = icon.AddComponent<RectTransform>();
            iconRect.anchorMin = new Vector2(0, 0.5f);
            iconRect.anchorMax = new Vector2(0, 0.5f);
            iconRect.pivot = new Vector2(0, 0.5f);
            iconRect.anchoredPosition = new Vector2(10, 0);
            iconRect.sizeDelta = new Vector2(40, 40);
            var iconImage = icon.AddComponent<Image>();
            iconImage.color = Color.white;

            // Message Text
            var message = new GameObject("Message");
            message.transform.SetParent(toastRoot.transform, false);
            var msgRect = message.AddComponent<RectTransform>();
            msgRect.anchorMin = new Vector2(0, 0);
            msgRect.anchorMax = new Vector2(1, 1);
            msgRect.offsetMin = new Vector2(60, 10);
            msgRect.offsetMax = new Vector2(-50, -10);
            var msgText = message.AddComponent<TextMeshProUGUI>();
            msgText.text = "Toast Message";
            msgText.fontSize = 18;
            msgText.alignment = TextAlignmentOptions.MidlineLeft;
            msgText.color = Color.white;

            // Dismiss Button
            var dismissBtn = new GameObject("DismissButton");
            dismissBtn.transform.SetParent(toastRoot.transform, false);
            var btnRect = dismissBtn.AddComponent<RectTransform>();
            btnRect.anchorMin = new Vector2(1, 0.5f);
            btnRect.anchorMax = new Vector2(1, 0.5f);
            btnRect.pivot = new Vector2(1, 0.5f);
            btnRect.anchoredPosition = new Vector2(-10, 0);
            btnRect.sizeDelta = new Vector2(30, 30);
            var btnImage = dismissBtn.AddComponent<Image>();
            btnImage.color = new Color(1, 1, 1, 0.5f);
            var btn = dismissBtn.AddComponent<Button>();
            btn.targetGraphic = btnImage;

            // Add ToastController component
            var controller = toastRoot.AddComponent<ToastController>();

            // Save prefab
            string path = $"{PREFAB_PATH}/Toast.prefab";
            PrefabUtility.SaveAsPrefabAsset(toastRoot, path);
            Object.DestroyImmediate(toastRoot);

            Debug.Log($"[MetaGameUIPrefabGenerator] Created Toast prefab at {path}");
        }

        [MenuItem("HNR/Generate Meta-Game UI Prefabs/GlobalHeader Only")]
        public static void GenerateGlobalHeaderPrefab()
        {
            EnsureDirectoryExists();

            // Create GlobalHeader prefab
            var headerRoot = new GameObject("GlobalHeader");
            var rectTransform = headerRoot.AddComponent<RectTransform>();
            rectTransform.anchorMin = new Vector2(0, 1);
            rectTransform.anchorMax = new Vector2(1, 1);
            rectTransform.pivot = new Vector2(0.5f, 1);
            rectTransform.sizeDelta = new Vector2(0, 80);

            // Background
            var bg = CreateChildWithImage(headerRoot, "Background", new Color(0.1f, 0.1f, 0.12f, 0.95f));
            StretchFill(bg);

            // Left Panel - Player Profile
            var leftPanel = CreatePanel(headerRoot, "PlayerProfile", new Vector2(200, 0), new Vector2(0, 0.5f));
            leftPanel.anchoredPosition = new Vector2(10, 0);

            // Avatar Frame
            var avatarFrame = CreateChildWithImage(leftPanel.gameObject, "AvatarFrame", Color.white);
            var avatarRect = avatarFrame.GetComponent<RectTransform>();
            avatarRect.anchorMin = avatarRect.anchorMax = new Vector2(0, 0.5f);
            avatarRect.pivot = new Vector2(0, 0.5f);
            avatarRect.sizeDelta = new Vector2(60, 60);

            // Avatar Image
            var avatar = CreateChildWithImage(avatarFrame, "AvatarImage", Color.gray);
            var avatarImgRect = avatar.GetComponent<RectTransform>();
            avatarImgRect.anchorMin = Vector2.zero;
            avatarImgRect.anchorMax = Vector2.one;
            avatarImgRect.offsetMin = new Vector2(5, 5);
            avatarImgRect.offsetMax = new Vector2(-5, -5);

            // Player Name
            var playerName = CreateTextChild(leftPanel.gameObject, "PlayerName", "Player Name", 16);
            var nameRect = playerName.GetComponent<RectTransform>();
            nameRect.anchorMin = nameRect.anchorMax = new Vector2(0, 0.5f);
            nameRect.pivot = new Vector2(0, 0.5f);
            nameRect.anchoredPosition = new Vector2(70, 10);
            nameRect.sizeDelta = new Vector2(120, 24);

            // Player Level
            var playerLevel = CreateTextChild(leftPanel.gameObject, "PlayerLevel", "Lv.1", 14);
            var levelRect = playerLevel.GetComponent<RectTransform>();
            levelRect.anchorMin = levelRect.anchorMax = new Vector2(0, 0.5f);
            levelRect.pivot = new Vector2(0, 0.5f);
            levelRect.anchoredPosition = new Vector2(70, -10);
            levelRect.sizeDelta = new Vector2(60, 20);

            // EXP Bar
            var expBar = CreateChildWithImage(leftPanel.gameObject, "ExpBar", UIColors.SoulCyan);
            var expRect = expBar.GetComponent<RectTransform>();
            expRect.anchorMin = expRect.anchorMax = new Vector2(0, 0.5f);
            expRect.pivot = new Vector2(0, 0.5f);
            expRect.anchoredPosition = new Vector2(130, -10);
            expRect.sizeDelta = new Vector2(60, 8);
            expBar.GetComponent<Image>().type = Image.Type.Filled;
            expBar.GetComponent<Image>().fillMethod = Image.FillMethod.Horizontal;

            // Right Panel - Currency Tickers (placeholders)
            var rightPanel = CreatePanel(headerRoot, "CurrencyPanel", new Vector2(300, 60), new Vector2(1, 0.5f));
            rightPanel.anchoredPosition = new Vector2(-10, 0);
            rightPanel.pivot = new Vector2(1, 0.5f);

            // Add GlobalHeader component
            var header = headerRoot.AddComponent<GlobalHeader>();

            // Save prefab
            string path = $"{PREFAB_PATH}/GlobalHeader.prefab";
            PrefabUtility.SaveAsPrefabAsset(headerRoot, path);
            Object.DestroyImmediate(headerRoot);

            Debug.Log($"[MetaGameUIPrefabGenerator] Created GlobalHeader prefab at {path}");
        }

        [MenuItem("HNR/Generate Meta-Game UI Prefabs/GlobalNavDock Only")]
        public static void GenerateGlobalNavDockPrefab()
        {
            EnsureDirectoryExists();

            // Create GlobalNavDock prefab
            var dockRoot = new GameObject("GlobalNavDock");
            var rectTransform = dockRoot.AddComponent<RectTransform>();
            rectTransform.anchorMin = new Vector2(0, 0);
            rectTransform.anchorMax = new Vector2(1, 0);
            rectTransform.pivot = new Vector2(0.5f, 0);
            rectTransform.sizeDelta = new Vector2(0, 80);

            // Background
            var bg = CreateChildWithImage(dockRoot, "Background", new Color(0.08f, 0.08f, 0.1f, 0.98f));
            StretchFill(bg);

            // Button Container
            var buttonContainer = new GameObject("ButtonContainer");
            buttonContainer.transform.SetParent(dockRoot.transform, false);
            var containerRect = buttonContainer.AddComponent<RectTransform>();
            StretchFill(buttonContainer);
            var layout = buttonContainer.AddComponent<HorizontalLayoutGroup>();
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;
            layout.spacing = 20;
            layout.padding = new RectOffset(40, 40, 10, 10);

            // Create 4 nav buttons
            string[] buttonNames = { "Bastion", "Requiems", "Inventory", "Settings" };
            for (int i = 0; i < buttonNames.Length; i++)
            {
                CreateNavButton(buttonContainer, buttonNames[i], i);
            }

            // Add GlobalNavDock component
            var dock = dockRoot.AddComponent<GlobalNavDock>();

            // Save prefab
            string path = $"{PREFAB_PATH}/GlobalNavDock.prefab";
            PrefabUtility.SaveAsPrefabAsset(dockRoot, path);
            Object.DestroyImmediate(dockRoot);

            Debug.Log($"[MetaGameUIPrefabGenerator] Created GlobalNavDock prefab at {path}");
        }

        [MenuItem("HNR/Generate Meta-Game UI Prefabs/CurrencyTicker Only")]
        public static void GenerateCurrencyTickerPrefab()
        {
            EnsureDirectoryExists();

            // Create CurrencyTicker prefab
            var tickerRoot = new GameObject("CurrencyTicker");
            var rectTransform = tickerRoot.AddComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(100, 40);

            // Icon
            var icon = CreateChildWithImage(tickerRoot, "Icon", Color.white);
            var iconRect = icon.GetComponent<RectTransform>();
            iconRect.anchorMin = iconRect.anchorMax = new Vector2(0, 0.5f);
            iconRect.pivot = new Vector2(0, 0.5f);
            iconRect.anchoredPosition = new Vector2(5, 0);
            iconRect.sizeDelta = new Vector2(30, 30);

            // Value Text
            var valueText = CreateTextChild(tickerRoot, "ValueText", "0", 18);
            var valueRect = valueText.GetComponent<RectTransform>();
            valueRect.anchorMin = new Vector2(0, 0);
            valueRect.anchorMax = new Vector2(1, 1);
            valueRect.offsetMin = new Vector2(40, 5);
            valueRect.offsetMax = new Vector2(-5, -5);
            valueText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.MidlineRight;

            // Add CurrencyTicker component
            var ticker = tickerRoot.AddComponent<CurrencyTicker>();

            // Save prefab
            string path = $"{PREFAB_PATH}/CurrencyTicker.prefab";
            PrefabUtility.SaveAsPrefabAsset(tickerRoot, path);
            Object.DestroyImmediate(tickerRoot);

            Debug.Log($"[MetaGameUIPrefabGenerator] Created CurrencyTicker prefab at {path}");
        }

        // ============================================
        // Helper Methods
        // ============================================

        private static void EnsureDirectoryExists()
        {
            if (!AssetDatabase.IsValidFolder(PREFAB_PATH))
            {
                string[] parts = PREFAB_PATH.Split('/');
                string currentPath = parts[0];
                for (int i = 1; i < parts.Length; i++)
                {
                    string nextPath = currentPath + "/" + parts[i];
                    if (!AssetDatabase.IsValidFolder(nextPath))
                    {
                        AssetDatabase.CreateFolder(currentPath, parts[i]);
                    }
                    currentPath = nextPath;
                }
            }
        }

        private static GameObject CreateChildWithImage(GameObject parent, string name, Color color)
        {
            var child = new GameObject(name);
            child.transform.SetParent(parent.transform, false);
            child.AddComponent<RectTransform>();
            var image = child.AddComponent<Image>();
            image.color = color;
            return child;
        }

        private static GameObject CreateTextChild(GameObject parent, string name, string text, int fontSize)
        {
            var child = new GameObject(name);
            child.transform.SetParent(parent.transform, false);
            child.AddComponent<RectTransform>();
            var tmp = child.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.color = Color.white;
            return child;
        }

        private static RectTransform CreatePanel(GameObject parent, string name, Vector2 size, Vector2 anchor)
        {
            var panel = new GameObject(name);
            panel.transform.SetParent(parent.transform, false);
            var rect = panel.AddComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = anchor;
            rect.sizeDelta = size;
            return rect;
        }

        private static void StretchFill(GameObject obj)
        {
            var rect = obj.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.sizeDelta = Vector2.zero;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        private static void CreateNavButton(GameObject parent, string name, int index)
        {
            var button = new GameObject($"NavButton_{name}");
            button.transform.SetParent(parent.transform, false);

            var rect = button.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(60, 60);

            var layout = button.AddComponent<LayoutElement>();
            layout.preferredWidth = 60;
            layout.preferredHeight = 60;

            // Button background
            var bgImage = button.AddComponent<Image>();
            bgImage.color = new Color(0.2f, 0.2f, 0.25f, 0.8f);

            var btn = button.AddComponent<Button>();
            btn.targetGraphic = bgImage;

            // Icon
            var icon = CreateChildWithImage(button, "Icon", Color.white);
            var iconRect = icon.GetComponent<RectTransform>();
            iconRect.anchorMin = iconRect.anchorMax = new Vector2(0.5f, 0.6f);
            iconRect.sizeDelta = new Vector2(30, 30);

            // Glow Ring
            var glow = CreateChildWithImage(button, "GlowRing", UIColors.SoulCyan);
            StretchFill(glow);
            glow.GetComponent<Image>().color = new Color(UIColors.SoulCyan.r, UIColors.SoulCyan.g, UIColors.SoulCyan.b, 0f);

            // Label
            var label = CreateTextChild(button, "Label", name, 10);
            var labelRect = label.GetComponent<RectTransform>();
            labelRect.anchorMin = new Vector2(0, 0);
            labelRect.anchorMax = new Vector2(1, 0.3f);
            labelRect.offsetMin = Vector2.zero;
            labelRect.offsetMax = Vector2.zero;
            label.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;

            // Notification Badge
            var badge = CreateChildWithImage(button, "NotificationBadge", Color.red);
            var badgeRect = badge.GetComponent<RectTransform>();
            badgeRect.anchorMin = badgeRect.anchorMax = new Vector2(1, 1);
            badgeRect.pivot = new Vector2(1, 1);
            badgeRect.anchoredPosition = new Vector2(5, 5);
            badgeRect.sizeDelta = new Vector2(20, 20);
            badge.SetActive(false);

            var badgeText = CreateTextChild(badge, "Count", "0", 12);
            StretchFill(badgeText);
            badgeText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;

            // Add NavDockButton component
            button.AddComponent<NavDockButton>();
        }
    }
}
#endif
