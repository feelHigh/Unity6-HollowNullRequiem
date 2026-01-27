// ============================================
// RuntimePrefabGenerator.cs
// Editor tool to generate runtime UI prefabs
// ============================================

#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;

namespace HNR.Editor.Generators
{
    /// <summary>
    /// Generates prefab assets for components that previously created GameObjects at runtime.
    /// These prefabs can be assigned in the Inspector for better configurability.
    /// </summary>
    public static class RuntimePrefabGenerator
    {
        private const string PREFAB_DIR = "Assets/_Project/Prefabs/UI/Runtime";
        private static Sprite _whiteSprite;

        // ============================================
        // Menu Items
        // ============================================

        [MenuItem("HNR/2. Prefabs/UI/Runtime Prefabs/Generate All Runtime Prefabs", false, 200)]
        public static void GenerateAllPrefabs()
        {
            EnsurePrefabDirectory();

            GenerateConfirmationDialogPrefab();
            GenerateStatusIconPrefab();
            GenerateDeckViewerCardSlotPrefab();
            GenerateRelicShopSlotPrefab();
            GenerateBannerSlidePrefab();
            GenerateBannerIndicatorPrefab();
            GenerateSimpleCardDisplayItemPrefab();
            GenerateSanctuaryCardSlotPrefab();
            GenerateSanctuaryUpgradeSlotPrefab();
            GenerateRequiemSelectionSlotPrefab();
            GenerateRequiemPortraitButtonPrefab();
            GenerateRewardCardSlotPrefab();
            GenerateRelicDisplayIconPrefab();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("[RuntimePrefabGenerator] Generated all 13 runtime UI prefabs");
        }

        [MenuItem("HNR/2. Prefabs/UI/Runtime Prefabs/1. ConfirmationDialog", false, 210)]
        public static void GenerateConfirmationDialogPrefab()
        {
            EnsurePrefabDirectory();

            // Create root with Canvas
            var root = new GameObject("ConfirmationDialog");

            var canvas = root.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 1000;

            var scaler = root.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;

            root.AddComponent<GraphicRaycaster>();

            // Overlay with CanvasGroup
            var overlayGO = CreateChild(root.transform, "Overlay");
            var overlayRect = StretchRectTransform(overlayGO);
            var overlayGroup = overlayGO.AddComponent<CanvasGroup>();
            overlayGroup.alpha = 0f;
            overlayGroup.interactable = false;
            overlayGroup.blocksRaycasts = false;

            // Background
            var bgGO = CreateChild(overlayGO.transform, "Background");
            StretchRectTransform(bgGO);
            var bgImage = bgGO.AddComponent<Image>();
            bgImage.color = new Color(0, 0, 0, 0.8f);

            // Dialog Panel
            var dialogGO = CreateChild(overlayGO.transform, "DialogPanel");
            var dialogRect = dialogGO.AddComponent<RectTransform>();
            CenterRectTransform(dialogRect, new Vector2(500, 250));
            var dialogImage = dialogGO.AddComponent<Image>();
            dialogImage.color = new Color(0.18f, 0.18f, 0.22f, 1f);

            // Title
            var titleGO = CreateChild(dialogGO.transform, "Title");
            var titleRect = titleGO.AddComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0, 1);
            titleRect.anchorMax = new Vector2(1, 1);
            titleRect.pivot = new Vector2(0.5f, 1);
            titleRect.anchoredPosition = new Vector2(0, -20);
            titleRect.sizeDelta = new Vector2(-40, 40);
            var titleText = titleGO.AddComponent<TextMeshProUGUI>();
            titleText.text = "Confirm";
            titleText.fontSize = 28;
            titleText.fontStyle = FontStyles.Bold;
            titleText.color = new Color(0f, 0.83f, 0.89f);
            titleText.alignment = TextAlignmentOptions.Center;

            // Message
            var messageGO = CreateChild(dialogGO.transform, "Message");
            var messageRect = messageGO.AddComponent<RectTransform>();
            messageRect.anchorMin = new Vector2(0, 0.35f);
            messageRect.anchorMax = new Vector2(1, 0.85f);
            messageRect.sizeDelta = new Vector2(-40, 0);
            messageRect.anchoredPosition = Vector2.zero;
            var messageText = messageGO.AddComponent<TextMeshProUGUI>();
            messageText.text = "Are you sure?";
            messageText.fontSize = 18;
            messageText.color = Color.white;
            messageText.alignment = TextAlignmentOptions.Center;

            // Confirm Button
            var confirmGO = CreateChild(dialogGO.transform, "ConfirmButton");
            var confirmRect = confirmGO.AddComponent<RectTransform>();
            confirmRect.anchorMin = new Vector2(0.55f, 0.1f);
            confirmRect.anchorMax = new Vector2(0.95f, 0.3f);
            confirmRect.sizeDelta = Vector2.zero;
            confirmRect.anchoredPosition = Vector2.zero;
            var confirmImage = confirmGO.AddComponent<Image>();
            confirmImage.color = new Color(0.85f, 0.35f, 0.2f, 1f);
            var confirmBtn = confirmGO.AddComponent<Button>();
            confirmBtn.targetGraphic = confirmImage;

            var confirmTextGO = CreateChild(confirmGO.transform, "Text");
            StretchRectTransform(confirmTextGO);
            var confirmBtnText = confirmTextGO.AddComponent<TextMeshProUGUI>();
            confirmBtnText.text = "Confirm";
            confirmBtnText.fontSize = 18;
            confirmBtnText.fontStyle = FontStyles.Bold;
            confirmBtnText.color = Color.white;
            confirmBtnText.alignment = TextAlignmentOptions.Center;

            // Cancel Button
            var cancelGO = CreateChild(dialogGO.transform, "CancelButton");
            var cancelRect = cancelGO.AddComponent<RectTransform>();
            cancelRect.anchorMin = new Vector2(0.05f, 0.1f);
            cancelRect.anchorMax = new Vector2(0.45f, 0.3f);
            cancelRect.sizeDelta = Vector2.zero;
            cancelRect.anchoredPosition = Vector2.zero;
            var cancelImage = cancelGO.AddComponent<Image>();
            cancelImage.color = new Color(0.3f, 0.3f, 0.35f, 1f);
            var cancelBtn = cancelGO.AddComponent<Button>();
            cancelBtn.targetGraphic = cancelImage;

            var cancelTextGO = CreateChild(cancelGO.transform, "Text");
            StretchRectTransform(cancelTextGO);
            var cancelBtnText = cancelTextGO.AddComponent<TextMeshProUGUI>();
            cancelBtnText.text = "Cancel";
            cancelBtnText.fontSize = 18;
            cancelBtnText.fontStyle = FontStyles.Bold;
            cancelBtnText.color = new Color(0f, 0.83f, 0.89f);
            cancelBtnText.alignment = TextAlignmentOptions.Center;

            // Add ConfirmationDialog component and wire references
            var dialog = root.AddComponent<HNR.UI.Components.ConfirmationDialog>();
            WireSerializedField(dialog, "_overlay", overlayGroup);
            WireSerializedField(dialog, "_backgroundPanel", bgImage);
            WireSerializedField(dialog, "_dialogPanel", dialogRect);
            WireSerializedField(dialog, "_titleText", titleText);
            WireSerializedField(dialog, "_messageText", messageText);
            WireSerializedField(dialog, "_confirmButton", confirmBtn);
            WireSerializedField(dialog, "_confirmButtonText", confirmBtnText);
            WireSerializedField(dialog, "_cancelButton", cancelBtn);
            WireSerializedField(dialog, "_cancelButtonText", cancelBtnText);

            SavePrefab(root, "ConfirmationDialog.prefab");
        }

        [MenuItem("HNR/2. Prefabs/UI/Runtime Prefabs/2. StatusIcon", false, 211)]
        public static void GenerateStatusIconPrefab()
        {
            EnsurePrefabDirectory();

            var iconGO = new GameObject("StatusIcon");

            var rect = iconGO.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(16, 16);

            var layoutElement = iconGO.AddComponent<LayoutElement>();
            layoutElement.preferredWidth = 16;
            layoutElement.preferredHeight = 16;

            var bgImage = iconGO.AddComponent<Image>();
            bgImage.color = new Color(0.5f, 0.5f, 0.5f, 1f);
            bgImage.sprite = GetWhiteSprite();

            // Stack text
            var stacksGO = CreateChild(iconGO.transform, "Stacks");
            StretchRectTransform(stacksGO);
            var stacksText = stacksGO.AddComponent<TextMeshProUGUI>();
            stacksText.text = "";
            stacksText.fontSize = 10;
            stacksText.fontStyle = FontStyles.Bold;
            stacksText.alignment = TextAlignmentOptions.Center;
            stacksText.color = Color.white;

            SavePrefab(iconGO, "StatusIcon.prefab");
        }

        [MenuItem("HNR/2. Prefabs/UI/Runtime Prefabs/3. DeckViewerCardSlot", false, 212)]
        public static void GenerateDeckViewerCardSlotPrefab()
        {
            EnsurePrefabDirectory();

            var slot = new GameObject("DeckViewerCardSlot");

            var rect = slot.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(200, 280);

            var image = slot.AddComponent<Image>();
            image.color = new Color(0.15f, 0.15f, 0.2f, 1f);

            var button = slot.AddComponent<Button>();
            button.targetGraphic = image;

            // Card name text
            var textObj = CreateChild(slot.transform, "CardName");
            StretchWithPadding(textObj, 5);
            var text = textObj.AddComponent<TextMeshProUGUI>();
            text.text = "Card Name";
            text.fontSize = 18;
            text.alignment = TextAlignmentOptions.Center;
            text.color = Color.white;
            text.textWrappingMode = TextWrappingModes.Normal;

            // Cost indicator
            var costObj = CreateChild(slot.transform, "Cost");
            var costRect = costObj.AddComponent<RectTransform>();
            costRect.anchorMin = new Vector2(0, 1);
            costRect.anchorMax = new Vector2(0, 1);
            costRect.pivot = new Vector2(0, 1);
            costRect.anchoredPosition = new Vector2(8, -8);
            costRect.sizeDelta = new Vector2(40, 40);
            var costBg = costObj.AddComponent<Image>();
            costBg.color = new Color(0.2f, 0.4f, 0.8f, 1f);

            var costTextObj = CreateChild(costObj.transform, "CostText");
            StretchRectTransform(costTextObj);
            var costText = costTextObj.AddComponent<TextMeshProUGUI>();
            costText.text = "0";
            costText.fontSize = 24;
            costText.fontStyle = FontStyles.Bold;
            costText.alignment = TextAlignmentOptions.Center;
            costText.color = Color.white;

            SavePrefab(slot, "DeckViewerCardSlot.prefab");
        }

        [MenuItem("HNR/2. Prefabs/UI/Runtime Prefabs/4. RelicShopSlot", false, 213)]
        public static void GenerateRelicShopSlotPrefab()
        {
            EnsurePrefabDirectory();

            var slot = new GameObject("RelicShopSlot");

            var rect = slot.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(100, 140);

            var bg = slot.AddComponent<Image>();
            bg.color = new Color(0.15f, 0.15f, 0.2f, 0.8f);

            var button = slot.AddComponent<Button>();
            button.targetGraphic = bg;

            slot.AddComponent<HNR.UI.Components.RelicShopSlot>();

            // Icon container
            var iconObj = CreateChild(slot.transform, "Icon");
            var iconRect = iconObj.AddComponent<RectTransform>();
            iconRect.anchorMin = new Vector2(0.5f, 0.6f);
            iconRect.anchorMax = new Vector2(0.5f, 0.6f);
            iconRect.sizeDelta = new Vector2(64, 64);
            var iconImage = iconObj.AddComponent<Image>();
            iconImage.color = Color.white;

            // Price text
            var priceObj = CreateChild(slot.transform, "Price");
            var priceRect = priceObj.AddComponent<RectTransform>();
            priceRect.anchorMin = new Vector2(0, 0);
            priceRect.anchorMax = new Vector2(1, 0);
            priceRect.pivot = new Vector2(0.5f, 0);
            priceRect.anchoredPosition = new Vector2(0, 10);
            priceRect.sizeDelta = new Vector2(0, 30);
            var priceText = priceObj.AddComponent<TextMeshProUGUI>();
            priceText.text = "100";
            priceText.fontSize = 18;
            priceText.alignment = TextAlignmentOptions.Center;
            priceText.color = Color.white;

            // Selection border
            var borderObj = CreateChild(slot.transform, "SelectionBorder");
            var borderRect = borderObj.AddComponent<RectTransform>();
            borderRect.anchorMin = Vector2.zero;
            borderRect.anchorMax = Vector2.one;
            borderRect.sizeDelta = new Vector2(6, 6);
            borderRect.anchoredPosition = Vector2.zero;
            var borderImage = borderObj.AddComponent<Image>();
            borderImage.color = new Color(0.2f, 0.6f, 1f, 1f);
            borderImage.type = Image.Type.Sliced;
            borderImage.raycastTarget = false;
            borderObj.SetActive(false);

            SavePrefab(slot, "RelicShopSlot.prefab");
        }

        [MenuItem("HNR/2. Prefabs/UI/Runtime Prefabs/5. BannerSlide", false, 214)]
        public static void GenerateBannerSlidePrefab()
        {
            EnsurePrefabDirectory();

            var slide = new GameObject("BannerSlide");

            var rect = slide.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0, 0.5f);
            rect.anchorMax = new Vector2(0, 0.5f);
            rect.pivot = new Vector2(0, 0.5f);
            rect.sizeDelta = new Vector2(400, 150);

            var layout = slide.AddComponent<LayoutElement>();
            layout.preferredWidth = 400;
            layout.preferredHeight = 150;
            layout.flexibleWidth = 0;
            layout.flexibleHeight = 0;

            var bgImage = slide.AddComponent<Image>();
            bgImage.color = new Color(0.1f, 0.1f, 0.15f, 1f);
            bgImage.raycastTarget = true;

            var button = slide.AddComponent<Button>();
            button.targetGraphic = bgImage;
            button.transition = Selectable.Transition.None;

            // Title
            var titleObj = CreateChild(slide.transform, "Title");
            var titleRect = titleObj.AddComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0, 0.5f);
            titleRect.anchorMax = new Vector2(1, 0.8f);
            titleRect.offsetMin = new Vector2(20, 0);
            titleRect.offsetMax = new Vector2(-20, 0);
            var titleText = titleObj.AddComponent<TextMeshProUGUI>();
            titleText.text = "Banner Title";
            titleText.fontSize = 28;
            titleText.fontStyle = FontStyles.Bold;
            titleText.alignment = TextAlignmentOptions.Center;
            titleText.color = Color.white;

            // Description
            var descObj = CreateChild(slide.transform, "Description");
            var descRect = descObj.AddComponent<RectTransform>();
            descRect.anchorMin = new Vector2(0, 0.2f);
            descRect.anchorMax = new Vector2(1, 0.5f);
            descRect.offsetMin = new Vector2(20, 0);
            descRect.offsetMax = new Vector2(-20, 0);
            var descText = descObj.AddComponent<TextMeshProUGUI>();
            descText.text = "Banner description";
            descText.fontSize = 18;
            descText.alignment = TextAlignmentOptions.Center;
            descText.color = new Color(0.8f, 0.8f, 0.8f, 1f);

            SavePrefab(slide, "BannerSlide.prefab");
        }

        [MenuItem("HNR/2. Prefabs/UI/Runtime Prefabs/6. BannerIndicator", false, 215)]
        public static void GenerateBannerIndicatorPrefab()
        {
            EnsurePrefabDirectory();

            var indicator = new GameObject("BannerIndicator");

            var rect = indicator.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(12, 12);

            var image = indicator.AddComponent<Image>();
            image.color = new Color(1f, 1f, 1f, 0.4f);

            SavePrefab(indicator, "BannerIndicator.prefab");
        }

        [MenuItem("HNR/2. Prefabs/UI/Runtime Prefabs/7. SimpleCardDisplayItem", false, 216)]
        public static void GenerateSimpleCardDisplayItemPrefab()
        {
            EnsurePrefabDirectory();

            var item = new GameObject("SimpleCardDisplayItem");

            var layoutElement = item.AddComponent<LayoutElement>();
            layoutElement.preferredHeight = 170;
            layoutElement.preferredWidth = 120;

            var image = item.AddComponent<Image>();
            image.color = new Color(0.25f, 0.25f, 0.3f, 0.9f);

            // Cost badge
            var costBadge = CreateChild(item.transform, "CostBadge");
            var costBadgeRect = costBadge.AddComponent<RectTransform>();
            costBadgeRect.anchorMin = new Vector2(0, 1);
            costBadgeRect.anchorMax = new Vector2(0, 1);
            costBadgeRect.pivot = new Vector2(0, 1);
            costBadgeRect.sizeDelta = new Vector2(30, 30);
            costBadgeRect.anchoredPosition = new Vector2(5, -5);
            var costBg = costBadge.AddComponent<Image>();
            costBg.color = new Color(0.1f, 0.1f, 0.15f, 0.9f);

            var costTextObj = CreateChild(costBadge.transform, "CostText");
            StretchRectTransform(costTextObj);
            var costText = costTextObj.AddComponent<TextMeshProUGUI>();
            costText.text = "0";
            costText.fontSize = 16;
            costText.fontStyle = FontStyles.Bold;
            costText.color = Color.white;
            costText.alignment = TextAlignmentOptions.Center;

            // Name text
            var nameObj = CreateChild(item.transform, "Name");
            var nameRect = nameObj.AddComponent<RectTransform>();
            nameRect.anchorMin = new Vector2(0, 0.7f);
            nameRect.anchorMax = new Vector2(1, 0.85f);
            nameRect.offsetMin = new Vector2(5, 0);
            nameRect.offsetMax = new Vector2(-5, 0);
            var nameText = nameObj.AddComponent<TextMeshProUGUI>();
            nameText.text = "Card Name";
            nameText.fontSize = 12;
            nameText.fontStyle = FontStyles.Bold;
            nameText.color = Color.white;
            nameText.alignment = TextAlignmentOptions.Center;

            // Type text
            var typeObj = CreateChild(item.transform, "Type");
            var typeRect = typeObj.AddComponent<RectTransform>();
            typeRect.anchorMin = new Vector2(0, 0.55f);
            typeRect.anchorMax = new Vector2(1, 0.7f);
            typeRect.offsetMin = new Vector2(5, 0);
            typeRect.offsetMax = new Vector2(-5, 0);
            var typeText = typeObj.AddComponent<TextMeshProUGUI>();
            typeText.text = "Skill";
            typeText.fontSize = 10;
            typeText.color = new Color(0.8f, 0.8f, 0.8f, 1f);
            typeText.alignment = TextAlignmentOptions.Center;

            // Description text
            var descObj = CreateChild(item.transform, "Description");
            var descRect = descObj.AddComponent<RectTransform>();
            descRect.anchorMin = new Vector2(0, 0.05f);
            descRect.anchorMax = new Vector2(1, 0.55f);
            descRect.offsetMin = new Vector2(8, 5);
            descRect.offsetMax = new Vector2(-8, -5);
            var descText = descObj.AddComponent<TextMeshProUGUI>();
            descText.text = "Description";
            descText.fontSize = 9;
            descText.color = Color.white;
            descText.alignment = TextAlignmentOptions.Center;
            descText.textWrappingMode = TextWrappingModes.Normal;

            // Add CardDisplayItem component
            item.AddComponent<HNR.UI.Components.CardDisplayItem>();

            SavePrefab(item, "SimpleCardDisplayItem.prefab");
        }

        [MenuItem("HNR/2. Prefabs/UI/Runtime Prefabs/8. SanctuaryCardSlot", false, 217)]
        public static void GenerateSanctuaryCardSlotPrefab()
        {
            EnsurePrefabDirectory();

            var slot = new GameObject("SanctuaryCardSlot");

            var rect = slot.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(140, 196);

            var bgImage = slot.AddComponent<Image>();
            bgImage.color = new Color(0.15f, 0.15f, 0.2f, 1f);
            bgImage.raycastTarget = true;

            var button = slot.AddComponent<Button>();
            button.targetGraphic = bgImage;

            // Cost text
            var costObj = CreateChild(slot.transform, "CostText");
            var costRect = costObj.AddComponent<RectTransform>();
            costRect.anchorMin = new Vector2(0, 1);
            costRect.anchorMax = new Vector2(0, 1);
            costRect.pivot = new Vector2(0, 1);
            costRect.anchoredPosition = new Vector2(8, -8);
            costRect.sizeDelta = new Vector2(30, 30);
            var costText = costObj.AddComponent<TextMeshProUGUI>();
            costText.text = "0";
            costText.fontSize = 18;
            costText.fontStyle = FontStyles.Bold;
            costText.alignment = TextAlignmentOptions.Center;
            costText.color = new Color(0.5f, 0.85f, 1f, 1f);

            // Name text
            var nameObj = CreateChild(slot.transform, "NameText");
            var nameRect = nameObj.AddComponent<RectTransform>();
            nameRect.anchorMin = new Vector2(0, 0.4f);
            nameRect.anchorMax = new Vector2(1, 0.6f);
            nameRect.offsetMin = new Vector2(8, 0);
            nameRect.offsetMax = new Vector2(-8, 0);
            var nameText = nameObj.AddComponent<TextMeshProUGUI>();
            nameText.text = "Card Name";
            nameText.fontSize = 14;
            nameText.fontStyle = FontStyles.Bold;
            nameText.alignment = TextAlignmentOptions.Center;
            nameText.color = Color.white;

            // Description text
            var descObj = CreateChild(slot.transform, "DescText");
            var descRect = descObj.AddComponent<RectTransform>();
            descRect.anchorMin = new Vector2(0, 0);
            descRect.anchorMax = new Vector2(1, 0.35f);
            descRect.offsetMin = new Vector2(8, 8);
            descRect.offsetMax = new Vector2(-8, 0);
            var descText = descObj.AddComponent<TextMeshProUGUI>();
            descText.text = "Description";
            descText.fontSize = 10;
            descText.alignment = TextAlignmentOptions.Center;
            descText.color = new Color(0.8f, 0.8f, 0.8f, 1f);
            descText.textWrappingMode = TextWrappingModes.Normal;

            SavePrefab(slot, "SanctuaryCardSlot.prefab");
        }

        [MenuItem("HNR/2. Prefabs/UI/Runtime Prefabs/9. SanctuaryUpgradeSlot", false, 218)]
        public static void GenerateSanctuaryUpgradeSlotPrefab()
        {
            EnsurePrefabDirectory();

            var slot = new GameObject("SanctuaryUpgradeSlot");

            var rect = slot.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(100, 140);

            slot.AddComponent<CanvasRenderer>();

            var bgImage = slot.AddComponent<Image>();
            bgImage.color = new Color(0.15f, 0.25f, 0.45f, 1f);
            bgImage.raycastTarget = true;

            var button = slot.AddComponent<Button>();
            button.targetGraphic = bgImage;

            // Border
            var borderObj = CreateChild(slot.transform, "Border");
            var borderRect = borderObj.AddComponent<RectTransform>();
            borderRect.anchorMin = Vector2.zero;
            borderRect.anchorMax = Vector2.one;
            borderRect.sizeDelta = Vector2.zero;
            borderRect.offsetMin = new Vector2(-3, -3);
            borderRect.offsetMax = new Vector2(3, 3);
            borderObj.transform.SetAsFirstSibling();
            var borderImage = borderObj.AddComponent<Image>();
            borderImage.color = new Color(0.3f, 0.5f, 0.8f, 1f);
            borderImage.raycastTarget = false;

            // Cost orb
            var orbObj = CreateChild(slot.transform, "CostOrb");
            var orbRect = orbObj.AddComponent<RectTransform>();
            orbRect.anchorMin = new Vector2(0, 1);
            orbRect.anchorMax = new Vector2(0, 1);
            orbRect.pivot = new Vector2(0, 1);
            orbRect.anchoredPosition = new Vector2(4, -4);
            orbRect.sizeDelta = new Vector2(26, 26);
            var orbImage = orbObj.AddComponent<Image>();
            orbImage.color = new Color(0.1f, 0.15f, 0.35f, 1f);
            orbImage.raycastTarget = false;

            var costTextObj = CreateChild(orbObj.transform, "CostText");
            StretchRectTransform(costTextObj);
            var costText = costTextObj.AddComponent<TextMeshProUGUI>();
            costText.text = "0";
            costText.fontSize = 14;
            costText.fontStyle = FontStyles.Bold;
            costText.alignment = TextAlignmentOptions.Center;
            costText.color = new Color(0.5f, 0.85f, 1f, 1f);

            // Name text
            var nameObj = CreateChild(slot.transform, "NameText");
            var nameRect = nameObj.AddComponent<RectTransform>();
            nameRect.anchorMin = new Vector2(0, 0.65f);
            nameRect.anchorMax = new Vector2(1, 0.85f);
            nameRect.offsetMin = new Vector2(4, 0);
            nameRect.offsetMax = new Vector2(-4, 0);
            var nameText = nameObj.AddComponent<TextMeshProUGUI>();
            nameText.text = "Card";
            nameText.fontSize = 11;
            nameText.fontStyle = FontStyles.Bold;
            nameText.alignment = TextAlignmentOptions.Center;
            nameText.color = Color.white;

            // Type text
            var typeObj = CreateChild(slot.transform, "TypeText");
            var typeRect = typeObj.AddComponent<RectTransform>();
            typeRect.anchorMin = new Vector2(0, 0.45f);
            typeRect.anchorMax = new Vector2(1, 0.65f);
            typeRect.offsetMin = new Vector2(4, 0);
            typeRect.offsetMax = new Vector2(-4, 0);
            var typeText = typeObj.AddComponent<TextMeshProUGUI>();
            typeText.text = "Skill";
            typeText.fontSize = 9;
            typeText.alignment = TextAlignmentOptions.Center;
            typeText.color = new Color(0.7f, 0.7f, 0.7f, 1f);

            // Description text
            var descObj = CreateChild(slot.transform, "DescText");
            var descRect = descObj.AddComponent<RectTransform>();
            descRect.anchorMin = new Vector2(0, 0.05f);
            descRect.anchorMax = new Vector2(1, 0.45f);
            descRect.offsetMin = new Vector2(4, 0);
            descRect.offsetMax = new Vector2(-4, 0);
            var descText = descObj.AddComponent<TextMeshProUGUI>();
            descText.text = "Description";
            descText.fontSize = 8;
            descText.alignment = TextAlignmentOptions.Center;
            descText.color = Color.white;
            descText.textWrappingMode = TextWrappingModes.Normal;
            descText.overflowMode = TextOverflowModes.Ellipsis;

            SavePrefab(slot, "SanctuaryUpgradeSlot.prefab");
        }

        [MenuItem("HNR/2. Prefabs/UI/Runtime Prefabs/10. RequiemSelectionSlot", false, 219)]
        public static void GenerateRequiemSelectionSlotPrefab()
        {
            EnsurePrefabDirectory();

            var slot = new GameObject("RequiemSelectionSlot");
            slot.AddComponent<CanvasRenderer>();

            var slotRect = slot.AddComponent<RectTransform>();
            slotRect.sizeDelta = new Vector2(350, 700);

            var slotBg = slot.AddComponent<Image>();
            slotBg.color = new Color(0.15f, 0.15f, 0.2f, 0.95f);

            var button = slot.AddComponent<Button>();
            button.targetGraphic = slotBg;

            // Selection border
            var borderGO = CreateChild(slot.transform, "SelectionBorder");
            var borderRect = borderGO.AddComponent<RectTransform>();
            borderRect.anchorMin = Vector2.zero;
            borderRect.anchorMax = Vector2.one;
            borderRect.offsetMin = Vector2.zero;
            borderRect.offsetMax = Vector2.zero;
            borderGO.SetActive(false);

            Color borderColor = new Color(0.2f, 0.6f, 1f, 1f);
            float borderWidth = 4f;
            CreateBorderEdge(borderGO.transform, "TopBorder", borderColor, borderWidth, new Vector2(0, 1), new Vector2(1, 1));
            CreateBorderEdge(borderGO.transform, "BottomBorder", borderColor, borderWidth, new Vector2(0, 0), new Vector2(1, 0));
            CreateBorderEdge(borderGO.transform, "LeftBorder", borderColor, borderWidth, new Vector2(0, 0), new Vector2(0, 1));
            CreateBorderEdge(borderGO.transform, "RightBorder", borderColor, borderWidth, new Vector2(1, 0), new Vector2(1, 1));

            // Portrait
            var portraitGO = CreateChild(slot.transform, "Portrait");
            var portraitRect = portraitGO.AddComponent<RectTransform>();
            portraitRect.anchorMin = Vector2.zero;
            portraitRect.anchorMax = Vector2.one;
            portraitRect.offsetMin = Vector2.zero;
            portraitRect.offsetMax = Vector2.zero;
            var portraitImage = portraitGO.AddComponent<Image>();
            portraitImage.preserveAspect = false;
            portraitImage.raycastTarget = false;
            portraitImage.color = new Color(0.5f, 0.5f, 0.5f, 0.8f);
            portraitGO.transform.SetAsFirstSibling();

            // Info panel
            var infoPanelGO = CreateChild(slot.transform, "InfoPanel");
            var infoPanelRect = infoPanelGO.AddComponent<RectTransform>();
            infoPanelRect.anchorMin = new Vector2(0, 0);
            infoPanelRect.anchorMax = new Vector2(1, 0.18f);
            infoPanelRect.offsetMin = Vector2.zero;
            infoPanelRect.offsetMax = Vector2.zero;
            var infoPanelBg = infoPanelGO.AddComponent<Image>();
            infoPanelBg.color = new Color(0.1f, 0.1f, 0.15f, 0.9f);
            infoPanelBg.raycastTarget = false;

            // Name text
            var nameGO = CreateChild(infoPanelGO.transform, "Name");
            var nameRect = nameGO.AddComponent<RectTransform>();
            nameRect.anchorMin = new Vector2(0, 0.4f);
            nameRect.anchorMax = new Vector2(1, 1);
            nameRect.offsetMin = new Vector2(8, 0);
            nameRect.offsetMax = new Vector2(-8, -2);
            var nameText = nameGO.AddComponent<TextMeshProUGUI>();
            nameText.text = "Requiem Name";
            nameText.fontSize = 24;
            nameText.fontStyle = FontStyles.Bold;
            nameText.color = Color.white;
            nameText.alignment = TextAlignmentOptions.Left;
            nameText.raycastTarget = false;

            // Class text
            var classGO = CreateChild(infoPanelGO.transform, "Class");
            var classRect = classGO.AddComponent<RectTransform>();
            classRect.anchorMin = new Vector2(0, 0);
            classRect.anchorMax = new Vector2(1, 0.45f);
            classRect.offsetMin = new Vector2(8, 2);
            classRect.offsetMax = new Vector2(-8, 0);
            var classText = classGO.AddComponent<TextMeshProUGUI>();
            classText.text = "Class | Aspect";
            classText.fontSize = 16;
            classText.color = new Color(0.7f, 0.7f, 0.7f, 1f);
            classText.alignment = TextAlignmentOptions.Left;
            classText.raycastTarget = false;

            // Aspect badge
            var badgeGO = CreateChild(slot.transform, "AspectBadge");
            var badgeRect = badgeGO.AddComponent<RectTransform>();
            badgeRect.anchorMin = new Vector2(0, 1);
            badgeRect.anchorMax = new Vector2(0, 1);
            badgeRect.pivot = new Vector2(0, 1);
            badgeRect.sizeDelta = new Vector2(72, 72);
            badgeRect.anchoredPosition = new Vector2(8, -8);
            var badgeImage = badgeGO.AddComponent<Image>();
            badgeImage.raycastTarget = false;
            badgeImage.preserveAspect = true;
            badgeImage.color = new Color(0.5f, 0.5f, 0.5f, 0.8f);

            // HP text
            var hpGO = CreateChild(infoPanelGO.transform, "HP");
            var hpRect = hpGO.AddComponent<RectTransform>();
            hpRect.anchorMin = new Vector2(0.6f, 0);
            hpRect.anchorMax = new Vector2(1, 1);
            hpRect.offsetMin = new Vector2(0, 2);
            hpRect.offsetMax = new Vector2(-8, -2);
            var hpText = hpGO.AddComponent<TextMeshProUGUI>();
            hpText.text = "HP 100";
            hpText.fontSize = 18;
            hpText.fontStyle = FontStyles.Bold;
            hpText.color = new Color(0.9f, 0.9f, 0.9f, 1f);
            hpText.alignment = TextAlignmentOptions.Right;
            hpText.raycastTarget = false;

            SavePrefab(slot, "RequiemSelectionSlot.prefab");
        }

        [MenuItem("HNR/2. Prefabs/UI/Runtime Prefabs/11. RequiemPortraitButton", false, 220)]
        public static void GenerateRequiemPortraitButtonPrefab()
        {
            EnsurePrefabDirectory();

            var buttonObj = new GameObject("RequiemPortraitButton");

            var layoutElement = buttonObj.AddComponent<LayoutElement>();
            layoutElement.preferredWidth = 150;
            layoutElement.preferredHeight = 200;

            var button = buttonObj.AddComponent<Button>();
            var buttonImage = buttonObj.AddComponent<Image>();
            buttonImage.color = new Color(0.2f, 0.2f, 0.25f, 0.9f);
            button.targetGraphic = buttonImage;

            // Portrait image
            var portraitObj = CreateChild(buttonObj.transform, "Portrait");
            var portraitRect = portraitObj.AddComponent<RectTransform>();
            portraitRect.anchorMin = new Vector2(0.1f, 0.3f);
            portraitRect.anchorMax = new Vector2(0.9f, 0.95f);
            portraitRect.offsetMin = Vector2.zero;
            portraitRect.offsetMax = Vector2.zero;
            var portraitImage = portraitObj.AddComponent<Image>();
            portraitImage.color = Color.white;

            // Name text
            var nameObj = CreateChild(buttonObj.transform, "Name");
            var nameRect = nameObj.AddComponent<RectTransform>();
            nameRect.anchorMin = new Vector2(0, 0);
            nameRect.anchorMax = new Vector2(1, 0.25f);
            nameRect.offsetMin = new Vector2(5, 5);
            nameRect.offsetMax = new Vector2(-5, -5);
            var nameText = nameObj.AddComponent<TextMeshProUGUI>();
            nameText.text = "Requiem";
            nameText.fontSize = 16;
            nameText.color = Color.white;
            nameText.alignment = TextAlignmentOptions.Center;

            // Add RequiemPortraitButton component
            buttonObj.AddComponent<HNR.UI.Components.RequiemPortraitButton>();

            SavePrefab(buttonObj, "RequiemPortraitButton.prefab");
        }

        [MenuItem("HNR/2. Prefabs/UI/Runtime Prefabs/12. RewardCardSlot", false, 221)]
        public static void GenerateRewardCardSlotPrefab()
        {
            EnsurePrefabDirectory();

            var slot = new GameObject("RewardCardSlot");

            var rect = slot.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(90, 120);

            var image = slot.AddComponent<Image>();
            image.color = new Color(0.15f, 0.15f, 0.2f, 1f);

            var button = slot.AddComponent<Button>();
            button.targetGraphic = image;

            var textObj = CreateChild(slot.transform, "CardName");
            StretchRectTransform(textObj);
            var text = textObj.AddComponent<TextMeshProUGUI>();
            text.text = "Card";
            text.fontSize = 12;
            text.alignment = TextAlignmentOptions.Center;
            text.color = Color.white;

            SavePrefab(slot, "RewardCardSlot.prefab");
        }

        [MenuItem("HNR/2. Prefabs/UI/Runtime Prefabs/13. RelicDisplayIcon", false, 222)]
        public static void GenerateRelicDisplayIconPrefab()
        {
            EnsurePrefabDirectory();

            var iconGO = new GameObject("RelicDisplayIcon");

            var rectTransform = iconGO.AddComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(32, 32);

            var image = iconGO.AddComponent<Image>();
            image.color = Color.white;

            var button = iconGO.AddComponent<Button>();
            button.targetGraphic = image;

            // Frame
            var frameGO = CreateChild(iconGO.transform, "Frame");
            var frameRect = frameGO.AddComponent<RectTransform>();
            frameRect.anchorMin = Vector2.zero;
            frameRect.anchorMax = Vector2.one;
            frameRect.sizeDelta = new Vector2(4, 4);
            frameRect.anchoredPosition = Vector2.zero;
            var frameImage = frameGO.AddComponent<Image>();
            frameImage.color = new Color(0.3f, 0.3f, 0.35f, 1f);
            frameImage.raycastTarget = false;
            frameGO.transform.SetAsFirstSibling();

            iconGO.AddComponent<HNR.UI.Combat.RelicIconSlot>();

            SavePrefab(iconGO, "RelicDisplayIcon.prefab");
        }

        // ============================================
        // Helper Methods
        // ============================================

        private static void EnsurePrefabDirectory()
        {
            if (!AssetDatabase.IsValidFolder(PREFAB_DIR))
            {
                if (!AssetDatabase.IsValidFolder("Assets/_Project/Prefabs/UI"))
                {
                    AssetDatabase.CreateFolder("Assets/_Project/Prefabs", "UI");
                }
                AssetDatabase.CreateFolder("Assets/_Project/Prefabs/UI", "Runtime");
            }
        }

        private static GameObject CreateChild(Transform parent, string name)
        {
            var child = new GameObject(name);
            child.transform.SetParent(parent, false);
            return child;
        }

        private static RectTransform StretchRectTransform(GameObject obj)
        {
            var rect = obj.AddComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.sizeDelta = Vector2.zero;
            rect.anchoredPosition = Vector2.zero;
            return rect;
        }

        private static void CenterRectTransform(RectTransform rect, Vector2 size)
        {
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = size;
            rect.anchoredPosition = Vector2.zero;
        }

        private static void StretchWithPadding(GameObject obj, float padding)
        {
            var rect = obj.AddComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = new Vector2(padding, padding);
            rect.offsetMax = new Vector2(-padding, -padding);
        }

        private static void CreateBorderEdge(Transform parent, string name, Color color, float width, Vector2 anchorMin, Vector2 anchorMax)
        {
            var edgeGO = new GameObject(name);
            edgeGO.transform.SetParent(parent, false);
            var rect = edgeGO.AddComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            // Make edge have thickness
            if (anchorMin.x == anchorMax.x) // Vertical edge
            {
                rect.sizeDelta = new Vector2(width, 0);
                rect.pivot = new Vector2(anchorMin.x, 0.5f);
            }
            else // Horizontal edge
            {
                rect.sizeDelta = new Vector2(0, width);
                rect.pivot = new Vector2(0.5f, anchorMin.y);
            }

            var image = edgeGO.AddComponent<Image>();
            image.color = color;
            image.raycastTarget = false;
        }

        private static Sprite GetWhiteSprite()
        {
            if (_whiteSprite == null)
            {
                var whiteTex = new Texture2D(4, 4);
                var pixels = new Color32[16];
                for (int i = 0; i < 16; i++) pixels[i] = new Color32(255, 255, 255, 255);
                whiteTex.SetPixels32(pixels);
                whiteTex.Apply();
                _whiteSprite = Sprite.Create(whiteTex, new Rect(0, 0, 4, 4), Vector2.one * 0.5f);
                _whiteSprite.name = "WhiteSprite";
            }
            return _whiteSprite;
        }

        private static void WireSerializedField(Object component, string fieldName, Object value)
        {
            var so = new SerializedObject(component);
            var prop = so.FindProperty(fieldName);
            if (prop != null)
            {
                prop.objectReferenceValue = value;
                so.ApplyModifiedPropertiesWithoutUndo();
            }
            else
            {
                Debug.LogWarning($"[RuntimePrefabGenerator] Could not find field '{fieldName}' on {component.GetType().Name}");
            }
        }

        private static void SavePrefab(GameObject root, string fileName)
        {
            string prefabPath = $"{PREFAB_DIR}/{fileName}";
            PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
            Object.DestroyImmediate(root);

            Debug.Log($"[RuntimePrefabGenerator] Created prefab: {prefabPath}");

            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            Selection.activeObject = prefab;
        }
    }
}
#endif
