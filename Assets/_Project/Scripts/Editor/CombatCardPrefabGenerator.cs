// ============================================
// CombatCardPrefabGenerator.cs
// Editor tool to generate CombatCard prefab based on CardBase visuals
// ============================================

using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;
using System.IO;
using HNR.UI.Combat;

namespace HNR.Editor
{
    /// <summary>
    /// Generates CombatCard.prefab based on CardBase.prefab visuals.
    /// Copies visual properties (sprites, colors) from CardBase.
    /// Regenerate after modifying CardBase to apply changes.
    /// </summary>
    public static class CombatCardPrefabGenerator
    {
        private const string PREFAB_PATH = "Assets/_Project/Prefabs/UI/Combat";

        public static void GenerateCombatCardPrefab()
        {
            // Ensure CardBase exists first
            var cardBasePrefab = CardBasePrefabGenerator.GetCardBasePrefab();
            if (cardBasePrefab == null)
            {
                Debug.Log("[CombatCardPrefabGenerator] CardBase not found, generating it first...");
                CardBasePrefabGenerator.GenerateCardBasePrefab();
                cardBasePrefab = CardBasePrefabGenerator.GetCardBasePrefab();
            }

            if (cardBasePrefab == null)
            {
                Debug.LogError("[CombatCardPrefabGenerator] Failed to get CardBase prefab");
                return;
            }

            // Ensure directory exists
            if (!Directory.Exists(PREFAB_PATH))
            {
                Directory.CreateDirectory(PREFAB_PATH);
                AssetDatabase.Refresh();
            }

            // Extract visual properties from CardBase
            var baseVisuals = ExtractVisualsFromCardBase(cardBasePrefab);

            // Create CombatCard prefab
            GameObject cardRoot = new GameObject("CombatCard");

            // Add RectTransform (smaller than CardBase for combat)
            RectTransform rootRect = cardRoot.AddComponent<RectTransform>();
            rootRect.sizeDelta = new Vector2(160, 220);

            // Add CanvasGroup for alpha control
            cardRoot.AddComponent<CanvasGroup>();

            // Add CombatCard script
            var combatCard = cardRoot.AddComponent<CombatCard>();

            // === Create visual elements with STRETCH anchors for proper scaling ===
            // This allows cards to scale properly when placed in different layout contexts

            // Glow Outline (combat-specific, behind everything) - stretch larger than parent
            GameObject glowOutlineObj = CreateStretchedChildImage(cardRoot, "GlowOutline", -5, -5, -5, -5);
            glowOutlineObj.transform.SetAsFirstSibling();
            Image glowOutlineImage = glowOutlineObj.GetComponent<Image>();
            glowOutlineImage.color = new Color(0.8f, 0.6f, 0.2f, 0f); // Start invisible

            // Rarity Glow (combat-specific) - stretch slightly smaller than glow
            GameObject rarityGlowObj = CreateStretchedChildImage(cardRoot, "RarityGlow", -2, -2, -2, -2);
            rarityGlowObj.transform.SetSiblingIndex(1);
            Image rarityImage = rarityGlowObj.GetComponent<Image>();
            rarityImage.color = new Color(1f, 1f, 1f, 0.3f);

            // Card Frame - stretch to fill parent
            GameObject frameObj = CreateStretchedChildImage(cardRoot, "CardFrame", 0, 0, 0, 0);
            Image frameImage = frameObj.GetComponent<Image>();
            frameImage.sprite = baseVisuals.frameSprite;
            frameImage.color = baseVisuals.frameColor;
            frameImage.type = baseVisuals.frameType;
            frameImage.raycastTarget = true;

            // Card Background - stretch with small padding
            GameObject bgObj = CreateStretchedChildImage(cardRoot, "CardBackground", 4, 4, 4, 4);
            Image bgImage = bgObj.GetComponent<Image>();
            bgImage.sprite = baseVisuals.bgSprite;
            bgImage.color = baseVisuals.bgColor;
            bgImage.type = baseVisuals.bgType;
            bgImage.raycastTarget = false;

            // Card Art - use relative anchors
            GameObject artObj = CreateChildImage(cardRoot, "CardArt", Vector2.zero);
            RectTransform artRect = artObj.GetComponent<RectTransform>();
            artRect.anchorMin = new Vector2(0.05f, 0.4f);
            artRect.anchorMax = new Vector2(0.95f, 0.92f);
            artRect.sizeDelta = Vector2.zero;
            artRect.anchoredPosition = Vector2.zero;
            Image artImage = artObj.GetComponent<Image>();
            artImage.sprite = baseVisuals.artSprite;
            artImage.color = baseVisuals.artColor;
            artImage.type = baseVisuals.artType;
            artImage.preserveAspect = baseVisuals.artPreserveAspect;
            artImage.raycastTarget = false;

            // Cost Background - anchor to top-left with relative size
            GameObject costBg = CreateChildImage(cardRoot, "CostBackground", Vector2.zero);
            RectTransform costBgRect = costBg.GetComponent<RectTransform>();
            costBgRect.anchorMin = new Vector2(0, 0.82f);
            costBgRect.anchorMax = new Vector2(0.22f, 1);
            costBgRect.pivot = new Vector2(0, 1);
            costBgRect.sizeDelta = Vector2.zero;
            costBgRect.offsetMin = new Vector2(4, 0);
            costBgRect.offsetMax = new Vector2(0, -4);
            Image costBgImage = costBg.GetComponent<Image>();
            costBgImage.sprite = baseVisuals.costBgSprite;
            costBgImage.color = baseVisuals.costBgColor;
            costBgImage.type = baseVisuals.costBgType;
            costBgImage.raycastTarget = false;

            // Cost Text - stretch to fill cost background with auto-sizing
            GameObject costTextObj = CreateChildText(costBg, "CostText", "1", 20);
            RectTransform costTextRect = costTextObj.GetComponent<RectTransform>();
            costTextRect.anchorMin = Vector2.zero;
            costTextRect.anchorMax = Vector2.one;
            costTextRect.sizeDelta = Vector2.zero;
            costTextRect.offsetMin = Vector2.zero;
            costTextRect.offsetMax = Vector2.zero;
            TextMeshProUGUI costText = costTextObj.GetComponent<TextMeshProUGUI>();
            costText.alignment = TextAlignmentOptions.Center;
            costText.color = baseVisuals.costTextColor;
            costText.fontStyle = FontStyles.Bold;
            costText.enableAutoSizing = true;
            costText.fontSizeMin = 8;
            costText.fontSizeMax = 20;
            if (baseVisuals.costFont != null) costText.font = baseVisuals.costFont;

            // Type Icon - anchor to top-right with relative size
            GameObject typeIcon = CreateChildImage(cardRoot, "TypeIcon", Vector2.zero);
            RectTransform typeRect = typeIcon.GetComponent<RectTransform>();
            typeRect.anchorMin = new Vector2(0.82f, 0.88f);
            typeRect.anchorMax = new Vector2(0.97f, 0.97f);
            typeRect.pivot = new Vector2(1, 1);
            typeRect.sizeDelta = Vector2.zero;
            typeRect.offsetMin = Vector2.zero;
            typeRect.offsetMax = Vector2.zero;
            Image typeImage = typeIcon.GetComponent<Image>();
            typeImage.color = Color.white;

            // Name Text - anchor to middle area with auto-sizing
            GameObject nameObj = CreateChildText(cardRoot, "CardNameText", "Card Name", 14);
            RectTransform nameRect = nameObj.GetComponent<RectTransform>();
            nameRect.anchorMin = new Vector2(0.05f, 0.3f);
            nameRect.anchorMax = new Vector2(0.95f, 0.4f);
            nameRect.sizeDelta = Vector2.zero;
            nameRect.offsetMin = Vector2.zero;
            nameRect.offsetMax = Vector2.zero;
            TextMeshProUGUI nameText = nameObj.GetComponent<TextMeshProUGUI>();
            nameText.alignment = TextAlignmentOptions.Center;
            nameText.color = baseVisuals.nameTextColor;
            nameText.fontStyle = FontStyles.Bold;
            nameText.enableAutoSizing = true;
            nameText.fontSizeMin = 6;
            nameText.fontSizeMax = 14;
            if (baseVisuals.nameFont != null) nameText.font = baseVisuals.nameFont;

            // Description Text - anchor to bottom area with auto-sizing
            GameObject descObj = CreateChildText(cardRoot, "DescriptionText", "Deal 6 damage.", 11);
            RectTransform descRect = descObj.GetComponent<RectTransform>();
            descRect.anchorMin = new Vector2(0.05f, 0.08f);
            descRect.anchorMax = new Vector2(0.95f, 0.28f);
            descRect.sizeDelta = Vector2.zero;
            descRect.offsetMin = Vector2.zero;
            descRect.offsetMax = Vector2.zero;
            TextMeshProUGUI descText = descObj.GetComponent<TextMeshProUGUI>();
            descText.alignment = TextAlignmentOptions.Center;
            descText.color = baseVisuals.descTextColor;
            descText.textWrappingMode = TextWrappingModes.Normal;
            descText.enableAutoSizing = true;
            descText.fontSizeMin = 6;
            descText.fontSizeMax = 11;
            if (baseVisuals.descFont != null) descText.font = baseVisuals.descFont;

            // Owner Portrait Container - anchor to bottom-center
            GameObject ownerContainer = new GameObject("OwnerPortraitContainer");
            ownerContainer.transform.SetParent(cardRoot.transform, false);
            RectTransform ownerContainerRect = ownerContainer.AddComponent<RectTransform>();
            ownerContainerRect.anchorMin = new Vector2(0.35f, 0);
            ownerContainerRect.anchorMax = new Vector2(0.65f, 0.12f);
            ownerContainerRect.pivot = new Vector2(0.5f, 0);
            ownerContainerRect.sizeDelta = Vector2.zero;
            ownerContainerRect.offsetMin = Vector2.zero;
            ownerContainerRect.offsetMax = Vector2.zero;

            GameObject ownerPortrait = CreateStretchedChildImage(ownerContainer, "OwnerPortrait", 0, 0, 0, 0);
            Image ownerImage = ownerPortrait.GetComponent<Image>();
            ownerImage.color = new Color(0.5f, 0.5f, 0.5f, 1f);

            // Selection Highlight - stretch larger than parent
            GameObject selectionHighlight = CreateStretchedChildImage(cardRoot, "SelectionHighlight", -4, -4, -4, -4);
            selectionHighlight.transform.SetAsLastSibling();
            Image selectionImage = selectionHighlight.GetComponent<Image>();
            selectionImage.sprite = baseVisuals.glowSprite;
            selectionImage.color = new Color(1f, 1f, 0f, 0.4f);
            selectionImage.type = baseVisuals.glowType;
            selectionHighlight.SetActive(false);

            // Unplayable Overlay - stretch to fill parent
            GameObject unplayableOverlay = CreateStretchedChildImage(cardRoot, "UnplayableOverlay", 0, 0, 0, 0);
            unplayableOverlay.transform.SetAsLastSibling();
            Image unplayableImage = unplayableOverlay.GetComponent<Image>();
            unplayableImage.color = new Color(0f, 0f, 0f, 0.5f);
            CanvasGroup unplayableGroup = unplayableOverlay.AddComponent<CanvasGroup>();
            unplayableGroup.alpha = 0f;
            unplayableGroup.blocksRaycasts = false;

            // === Wire up references ===
            SerializedObject so = new SerializedObject(combatCard);
            so.FindProperty("_costText").objectReferenceValue = costText;
            so.FindProperty("_costBackground").objectReferenceValue = costBgImage;
            so.FindProperty("_typeIcon").objectReferenceValue = typeImage;
            so.FindProperty("_cardNameText").objectReferenceValue = nameText;
            so.FindProperty("_descriptionText").objectReferenceValue = descText;
            so.FindProperty("_cardArt").objectReferenceValue = artImage;
            so.FindProperty("_cardFrame").objectReferenceValue = frameImage;
            so.FindProperty("_rarityGlow").objectReferenceValue = rarityImage;
            so.FindProperty("_ownerPortrait").objectReferenceValue = ownerImage;
            so.FindProperty("_ownerPortraitContainer").objectReferenceValue = ownerContainer;
            so.FindProperty("_glowOutline").objectReferenceValue = glowOutlineImage;
            so.FindProperty("_selectionHighlight").objectReferenceValue = selectionHighlight;
            so.FindProperty("_unplayableOverlay").objectReferenceValue = unplayableGroup;
            so.ApplyModifiedPropertiesWithoutUndo();

            // Save prefab
            string fullPath = $"{PREFAB_PATH}/CombatCard.prefab";
            PrefabUtility.SaveAsPrefabAsset(cardRoot, fullPath);

            Object.DestroyImmediate(cardRoot);

            AssetDatabase.Refresh();
            Debug.Log($"[CombatCardPrefabGenerator] CombatCard prefab created at {fullPath} (visuals copied from CardBase)");

            Selection.activeObject = AssetDatabase.LoadAssetAtPath<GameObject>(fullPath);
        }

        /// <summary>
        /// Extracts visual properties from CardBase prefab.
        /// </summary>
        private static CardBaseVisuals ExtractVisualsFromCardBase(GameObject cardBase)
        {
            var visuals = new CardBaseVisuals();
            Transform root = cardBase.transform;

            // CardFrame
            var frame = root.Find("CardFrame");
            if (frame != null)
            {
                var img = frame.GetComponent<Image>();
                if (img != null)
                {
                    visuals.frameSprite = img.sprite;
                    visuals.frameColor = img.color;
                    visuals.frameType = img.type;
                }
            }

            // CardBackground
            var bg = root.Find("CardBackground");
            if (bg != null)
            {
                var img = bg.GetComponent<Image>();
                if (img != null)
                {
                    visuals.bgSprite = img.sprite;
                    visuals.bgColor = img.color;
                    visuals.bgType = img.type;
                }
            }

            // CardArt
            var art = root.Find("CardArt");
            if (art != null)
            {
                var img = art.GetComponent<Image>();
                if (img != null)
                {
                    visuals.artSprite = img.sprite;
                    visuals.artColor = img.color;
                    visuals.artType = img.type;
                    visuals.artPreserveAspect = img.preserveAspect;
                }
            }

            // CostBackground
            var costBg = root.Find("CostBackground");
            if (costBg != null)
            {
                var img = costBg.GetComponent<Image>();
                if (img != null)
                {
                    visuals.costBgSprite = img.sprite;
                    visuals.costBgColor = img.color;
                    visuals.costBgType = img.type;
                }

                var costText = costBg.GetComponentInChildren<TextMeshProUGUI>();
                if (costText != null)
                {
                    visuals.costTextColor = costText.color;
                    visuals.costFont = costText.font;
                }
            }

            // NameText
            var nameT = root.Find("NameText");
            if (nameT != null)
            {
                var tmp = nameT.GetComponent<TextMeshProUGUI>();
                if (tmp != null)
                {
                    visuals.nameTextColor = tmp.color;
                    visuals.nameFont = tmp.font;
                }
            }

            // DescriptionText
            var descT = root.Find("DescriptionText");
            if (descT != null)
            {
                var tmp = descT.GetComponent<TextMeshProUGUI>();
                if (tmp != null)
                {
                    visuals.descTextColor = tmp.color;
                    visuals.descFont = tmp.font;
                }
            }

            // SelectionGlow
            var glow = root.Find("SelectionGlow");
            if (glow != null)
            {
                var img = glow.GetComponent<Image>();
                if (img != null)
                {
                    visuals.glowSprite = img.sprite;
                    visuals.glowColor = img.color;
                    visuals.glowType = img.type;
                }
            }

            return visuals;
        }

        private static GameObject CreateChildImage(GameObject parent, string name, Vector2 size)
        {
            GameObject obj = new GameObject(name);
            obj.transform.SetParent(parent.transform, false);
            RectTransform rect = obj.AddComponent<RectTransform>();
            rect.sizeDelta = size;
            obj.AddComponent<Image>();
            return obj;
        }

        /// <summary>
        /// Creates a child image with stretch anchors (0,0 to 1,1) and padding.
        /// This allows the element to scale with its parent.
        /// </summary>
        private static GameObject CreateStretchedChildImage(GameObject parent, string name, float paddingLeft, float paddingRight, float paddingTop, float paddingBottom)
        {
            GameObject obj = new GameObject(name);
            obj.transform.SetParent(parent.transform, false);
            RectTransform rect = obj.AddComponent<RectTransform>();

            // Stretch anchors - element will fill parent
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.pivot = new Vector2(0.5f, 0.5f);

            // Apply padding via offsets
            rect.offsetMin = new Vector2(paddingLeft, paddingBottom);
            rect.offsetMax = new Vector2(-paddingRight, -paddingTop);

            obj.AddComponent<Image>();
            return obj;
        }

        private static GameObject CreateChildText(GameObject parent, string name, string text, int fontSize)
        {
            GameObject obj = new GameObject(name);
            obj.transform.SetParent(parent.transform, false);
            RectTransform rect = obj.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(140, 24);
            TextMeshProUGUI tmp = obj.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = fontSize;
            return obj;
        }

        private struct CardBaseVisuals
        {
            public Sprite frameSprite;
            public Color frameColor;
            public Image.Type frameType;
            public Sprite bgSprite;
            public Color bgColor;
            public Image.Type bgType;
            public Sprite artSprite;
            public Color artColor;
            public Image.Type artType;
            public bool artPreserveAspect;
            public Sprite costBgSprite;
            public Color costBgColor;
            public Image.Type costBgType;
            public Color costTextColor;
            public TMP_FontAsset costFont;
            public Color nameTextColor;
            public TMP_FontAsset nameFont;
            public Color descTextColor;
            public TMP_FontAsset descFont;
            public Sprite glowSprite;
            public Color glowColor;
            public Image.Type glowType;
        }
    }
}
