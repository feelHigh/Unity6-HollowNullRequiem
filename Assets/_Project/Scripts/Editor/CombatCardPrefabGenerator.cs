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
using HNR.UI.Config;

namespace HNR.Editor
{
    /// <summary>
    /// Generates CombatCard.prefab based on CardBase.prefab visuals.
    /// Copies visual properties (sprites, colors) from CardBase.
    /// Regenerate after modifying CardBase to apply changes.
    ///
    /// New structure with GUI Pro-FantasyHero sprites:
    /// - GlowOutline: Aspect-colored glow (behind everything)
    /// - RarityGlow: Rarity-based color
    /// - CardBackground: Mask with Bg sprite (clips CardArt and text masks)
    ///   ├── CardArt: Full background art (masked)
    ///   ├── NameTextMask: RectMask2D for name text clipping
    ///   │   └── NameTextBackground → CardNameText
    ///   └── DescriptionTextMask: RectMask2D for description clipping
    ///       └── DescriptionTextBackground → DescriptionText
    /// - CardBorder: Border or BorderGem sprite (on top, rarity-based)
    /// - CostFrame: Layered cost display
    ///   ├── CostBg, CostBorder, CostGradient, CostInnerBorder (tinted)
    ///   └── CostText
    /// - TypeIcon, SelectionHighlight, UnplayableOverlay
    /// </summary>
    public static class CombatCardPrefabGenerator
    {
        private const string PREFAB_PATH = "Assets/_Project/Prefabs/UI/Combat";
        private const string SPRITE_CONFIG_PATH = "Assets/_Project/Data/Config/CardSpriteConfig.asset";

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

            // Load sprite config
            var spriteConfig = AssetDatabase.LoadAssetAtPath<CardSpriteConfigSO>(SPRITE_CONFIG_PATH);
            if (spriteConfig == null)
            {
                Debug.LogWarning("[CombatCardPrefabGenerator] CardSpriteConfig not found. Run HNR > Config > Generate Card Sprite Config first. Using fallback visuals.");
            }

            // Ensure directory exists
            if (!Directory.Exists(PREFAB_PATH))
            {
                Directory.CreateDirectory(PREFAB_PATH);
                AssetDatabase.Refresh();
            }

            // Extract visual properties from CardBase (for fonts, default colors, etc.)
            var baseVisuals = ExtractVisualsFromCardBase(cardBasePrefab);

            // Create CombatCard prefab
            GameObject cardRoot = new GameObject("CombatCard");

            // Add RectTransform (smaller than CardBase for combat, 5:7 ratio)
            RectTransform rootRect = cardRoot.AddComponent<RectTransform>();
            rootRect.sizeDelta = new Vector2(160, 224);

            // Add CanvasGroup for alpha control
            cardRoot.AddComponent<CanvasGroup>();

            // Add CombatCard script
            var combatCard = cardRoot.AddComponent<CombatCard>();

            // === Create visual elements ===

            // 1. Glow Outline (combat-specific, behind everything)
            GameObject glowOutlineObj = CreateStretchedChildImage(cardRoot, "GlowOutline", -5, -5, -5, -5);
            glowOutlineObj.transform.SetAsFirstSibling();
            Image glowOutlineImage = glowOutlineObj.GetComponent<Image>();
            glowOutlineImage.color = new Color(0.8f, 0.6f, 0.2f, 0f);

            // 2. Rarity Glow
            GameObject rarityGlowObj = CreateStretchedChildImage(cardRoot, "RarityGlow", -2, -2, -2, -2);
            rarityGlowObj.transform.SetSiblingIndex(1);
            Image rarityImage = rarityGlowObj.GetComponent<Image>();
            rarityImage.color = new Color(1f, 1f, 1f, 0.3f);

            // 3. Card Background with Mask (clips CardArt and text masks)
            GameObject bgObj = CreateStretchedChildImage(cardRoot, "CardBackground", 4, 4, 4, 4);
            Image bgImage = bgObj.GetComponent<Image>();
            if (spriteConfig != null)
            {
                // Use Bg sprite from config (pre-colored, Skill/Green as default)
                bgImage.sprite = spriteConfig.GetBackgroundSprite(HNR.Cards.CardType.Skill);
            }
            else
            {
                bgImage.sprite = baseVisuals.bgSprite;
            }
            bgImage.color = Color.white; // Pre-colored sprites, no tinting
            bgImage.type = Image.Type.Sliced;
            bgImage.raycastTarget = false;
            // Add Mask component
            Mask bgMask = bgObj.AddComponent<Mask>();
            bgMask.showMaskGraphic = true;

            // 3a. Card Art - child of CardBackground, stretches to fill (masked)
            GameObject artObj = CreateStretchedChildImage(bgObj, "CardArt", 0, 0, 0, 0);
            Image artImage = artObj.GetComponent<Image>();
            artImage.sprite = baseVisuals.artSprite;
            artImage.color = baseVisuals.artColor;
            artImage.type = baseVisuals.artType;
            artImage.preserveAspect = false;
            artImage.raycastTarget = false;

            // 3b. Name Text Mask (RectMask2D, positioned at top within CardBackground)
            GameObject nameTextMask = CreateStretchedChildImage(bgObj, "NameTextMask", 0, 0, 0, 0);
            RectTransform nameMaskRect = nameTextMask.GetComponent<RectTransform>();
            nameMaskRect.anchorMin = new Vector2(0.20f, 0.82f);
            nameMaskRect.anchorMax = new Vector2(1.0f, 1.0f);
            nameMaskRect.offsetMin = Vector2.zero;
            nameMaskRect.offsetMax = Vector2.zero;
            // Remove Image, add RectMask2D
            Object.DestroyImmediate(nameTextMask.GetComponent<Image>());
            nameTextMask.AddComponent<RectMask2D>();

            // Name Text Background (child of mask)
            GameObject nameBgObj = CreateStretchedChildImage(nameTextMask, "NameTextBackground", 0, 0, 0, 0);
            Image nameBgImage = nameBgObj.GetComponent<Image>();
            nameBgImage.sprite = baseVisuals.nameBgSprite;
            nameBgImage.color = baseVisuals.nameBgColor;
            nameBgImage.raycastTarget = false;

            // Name Text
            GameObject nameObj = CreateChildText(nameBgObj, "CardNameText", "Card Name", 12);
            RectTransform nameRect = nameObj.GetComponent<RectTransform>();
            nameRect.anchorMin = Vector2.zero;
            nameRect.anchorMax = Vector2.one;
            nameRect.sizeDelta = Vector2.zero;
            nameRect.offsetMin = new Vector2(3, 2);
            nameRect.offsetMax = new Vector2(-3, -2);
            TextMeshProUGUI nameText = nameObj.GetComponent<TextMeshProUGUI>();
            nameText.alignment = TextAlignmentOptions.Center;
            nameText.color = baseVisuals.nameTextColor;
            nameText.fontStyle = FontStyles.Bold;
            nameText.enableAutoSizing = true;
            nameText.fontSizeMin = 6;
            nameText.fontSizeMax = 12;
            if (baseVisuals.nameFont != null) nameText.font = baseVisuals.nameFont;

            // 3c. Description Text Mask (RectMask2D, positioned at bottom within CardBackground)
            GameObject descTextMask = CreateStretchedChildImage(bgObj, "DescriptionTextMask", 0, 0, 0, 0);
            RectTransform descMaskRect = descTextMask.GetComponent<RectTransform>();
            descMaskRect.anchorMin = new Vector2(0f, 0f);
            descMaskRect.anchorMax = new Vector2(1.0f, 0.28f);
            descMaskRect.offsetMin = Vector2.zero;
            descMaskRect.offsetMax = Vector2.zero;
            // Remove Image, add RectMask2D
            Object.DestroyImmediate(descTextMask.GetComponent<Image>());
            descTextMask.AddComponent<RectMask2D>();

            // Description Text Background (child of mask)
            GameObject descBgObj = CreateStretchedChildImage(descTextMask, "DescriptionTextBackground", 0, 0, 0, 0);
            Image descBgImage = descBgObj.GetComponent<Image>();
            descBgImage.sprite = baseVisuals.descBgSprite;
            descBgImage.color = baseVisuals.descBgColor;
            descBgImage.raycastTarget = false;

            // Description Text
            GameObject descObj = CreateChildText(descBgObj, "DescriptionText", "Deal 6 damage.", 10);
            RectTransform descRect = descObj.GetComponent<RectTransform>();
            descRect.anchorMin = Vector2.zero;
            descRect.anchorMax = Vector2.one;
            descRect.sizeDelta = Vector2.zero;
            descRect.offsetMin = new Vector2(4, 3);
            descRect.offsetMax = new Vector2(-4, -3);
            TextMeshProUGUI descText = descObj.GetComponent<TextMeshProUGUI>();
            descText.alignment = TextAlignmentOptions.Center;
            descText.color = baseVisuals.descTextColor;
            descText.textWrappingMode = TextWrappingModes.Normal;
            descText.enableAutoSizing = true;
            descText.fontSizeMin = 6;
            descText.fontSizeMax = 10;
            if (baseVisuals.descFont != null) descText.font = baseVisuals.descFont;

            // 4. Card Border (separate layer on top of background)
            GameObject borderObj = CreateStretchedChildImage(cardRoot, "CardBorder", 0, 0, 0, 0);
            Image borderImage = borderObj.GetComponent<Image>();
            if (spriteConfig != null)
            {
                // Use Border sprite from config (pre-colored, Skill/Green as default)
                borderImage.sprite = spriteConfig.GetFrameSet(HNR.Cards.CardType.Skill).Border;
            }
            else
            {
                borderImage.sprite = baseVisuals.frameSprite;
            }
            borderImage.color = Color.white; // Pre-colored sprites
            borderImage.type = Image.Type.Sliced;
            borderImage.raycastTarget = true;

            // 5. Legacy Card Frame (kept for backwards compatibility, hidden by default)
            GameObject frameObj = CreateStretchedChildImage(cardRoot, "CardFrame", 0, 0, 0, 0);
            Image frameImage = frameObj.GetComponent<Image>();
            frameImage.sprite = baseVisuals.frameSprite;
            frameImage.color = baseVisuals.frameColor;
            frameImage.type = baseVisuals.frameType;
            frameImage.raycastTarget = false;
            frameObj.SetActive(false); // Hidden by default, CardBorder is used instead

            // 6. Cost Frame with layered sprites
            GameObject costFrame = CreateChildImage(cardRoot, "CostFrame", Vector2.zero);
            RectTransform costFrameRect = costFrame.GetComponent<RectTransform>();
            costFrameRect.anchorMin = new Vector2(0.01f, 0.821f);
            costFrameRect.anchorMax = new Vector2(0.25f, 0.993f);
            costFrameRect.pivot = new Vector2(0.5f, 0.5f);
            costFrameRect.sizeDelta = Vector2.zero;
            costFrameRect.offsetMin = Vector2.zero;
            costFrameRect.offsetMax = Vector2.zero;
            // Remove image from container - it's just a layout parent
            Object.DestroyImmediate(costFrame.GetComponent<Image>());

            // 6a. Cost Bg layer
            GameObject costBgObj = CreateStretchedChildImage(costFrame, "CostBg", 0, 0, 0, 0);
            Image costBgImage = costBgObj.GetComponent<Image>();
            if (spriteConfig != null && spriteConfig.CostFrameBg != null)
            {
                costBgImage.sprite = spriteConfig.CostFrameBg;
            }
            costBgImage.color = new Color(0.2f, 0.7f, 0.3f); // Default Skill/Green tint
            costBgImage.type = Image.Type.Sliced;
            costBgImage.raycastTarget = false;

            // 6b. Cost Border layer
            GameObject costBorderObj = CreateStretchedChildImage(costFrame, "CostBorder", 0, 0, 0, 0);
            Image costBorderImage = costBorderObj.GetComponent<Image>();
            if (spriteConfig != null && spriteConfig.CostFrameBorder != null)
            {
                costBorderImage.sprite = spriteConfig.CostFrameBorder;
            }
            costBorderImage.color = new Color(0.2f, 0.7f, 0.3f);
            costBorderImage.type = Image.Type.Sliced;
            costBorderImage.raycastTarget = false;

            // 6c. Cost Gradient layer
            GameObject costGradientObj = CreateStretchedChildImage(costFrame, "CostGradient", 2, 2, 2, 2);
            Image costGradientImage = costGradientObj.GetComponent<Image>();
            if (spriteConfig != null && spriteConfig.CostFrameGradient != null)
            {
                costGradientImage.sprite = spriteConfig.CostFrameGradient;
            }
            costGradientImage.color = new Color(0.2f, 0.7f, 0.3f);
            costGradientImage.type = Image.Type.Sliced;
            costGradientImage.raycastTarget = false;

            // 6d. Cost Inner Border layer
            GameObject costInnerBorderObj = CreateStretchedChildImage(costFrame, "CostInnerBorder", 4, 4, 4, 4);
            Image costInnerBorderImage = costInnerBorderObj.GetComponent<Image>();
            if (spriteConfig != null && spriteConfig.CostFrameInnerBorder != null)
            {
                costInnerBorderImage.sprite = spriteConfig.CostFrameInnerBorder;
            }
            costInnerBorderImage.color = new Color(0.2f, 0.7f, 0.3f);
            costInnerBorderImage.type = Image.Type.Sliced;
            costInnerBorderImage.raycastTarget = false;

            // 6e. Cost Text (on top of all cost layers)
            GameObject costTextObj = CreateChildText(costFrame, "CostText", "1", 20);
            RectTransform costTextRect = costTextObj.GetComponent<RectTransform>();
            costTextRect.anchorMin = Vector2.zero;
            costTextRect.anchorMax = Vector2.one;
            costTextRect.sizeDelta = Vector2.zero;
            costTextRect.offsetMin = Vector2.zero;
            costTextRect.offsetMax = Vector2.zero;
            TextMeshProUGUI costText = costTextObj.GetComponent<TextMeshProUGUI>();
            costText.alignment = TextAlignmentOptions.Center;
            costText.color = Color.white;
            costText.fontStyle = FontStyles.Bold;
            costText.enableAutoSizing = true;
            costText.fontSizeMin = 8;
            costText.fontSizeMax = 20;
            if (baseVisuals.costFont != null) costText.font = baseVisuals.costFont;

            // 7. Legacy Cost Background (kept for backwards compatibility)
            GameObject legacyCostBg = CreateStretchedChildImage(costFrame, "CostBackground", 3, 3, 3, 3);
            Image legacyCostBgImage = legacyCostBg.GetComponent<Image>();
            legacyCostBgImage.sprite = baseVisuals.costBgSprite;
            legacyCostBgImage.color = baseVisuals.costBgColor;
            legacyCostBgImage.type = baseVisuals.costBgType;
            legacyCostBgImage.raycastTarget = false;
            legacyCostBg.SetActive(false); // Hidden by default

            // 8. Type Icon
            GameObject typeIcon = CreateChildImage(cardRoot, "TypeIcon", Vector2.zero);
            RectTransform typeRect = typeIcon.GetComponent<RectTransform>();
            typeRect.anchorMin = new Vector2(0.04f, 0.71f);
            typeRect.anchorMax = new Vector2(0.16f, 0.80f);
            typeRect.pivot = new Vector2(0.5f, 0.5f);
            typeRect.sizeDelta = Vector2.zero;
            typeRect.offsetMin = Vector2.zero;
            typeRect.offsetMax = Vector2.zero;
            Image typeImage = typeIcon.GetComponent<Image>();
            typeImage.color = Color.white;

            // 9. Selection Highlight
            GameObject selectionHighlight = CreateStretchedChildImage(cardRoot, "SelectionHighlight", -4, -4, -4, -4);
            selectionHighlight.transform.SetAsLastSibling();
            Image selectionImage = selectionHighlight.GetComponent<Image>();
            selectionImage.sprite = baseVisuals.glowSprite;
            selectionImage.color = new Color(1f, 1f, 0f, 0.4f);
            selectionImage.type = baseVisuals.glowType;
            selectionHighlight.SetActive(false);

            // 10. Unplayable Overlay
            GameObject unplayableOverlay = CreateStretchedChildImage(cardRoot, "UnplayableOverlay", 0, 0, 0, 0);
            unplayableOverlay.transform.SetAsLastSibling();
            Image unplayableImage = unplayableOverlay.GetComponent<Image>();
            unplayableImage.color = new Color(0f, 0f, 0f, 0.5f);
            CanvasGroup unplayableGroup = unplayableOverlay.AddComponent<CanvasGroup>();
            unplayableGroup.alpha = 0f;
            unplayableGroup.blocksRaycasts = false;

            // === Wire up references ===
            SerializedObject so = new SerializedObject(combatCard);

            // Original fields
            so.FindProperty("_costText").objectReferenceValue = costText;
            so.FindProperty("_costBackground").objectReferenceValue = legacyCostBgImage;
            so.FindProperty("_typeIcon").objectReferenceValue = typeImage;
            so.FindProperty("_cardNameText").objectReferenceValue = nameText;
            so.FindProperty("_descriptionText").objectReferenceValue = descText;
            so.FindProperty("_cardArt").objectReferenceValue = artImage;
            so.FindProperty("_cardFrame").objectReferenceValue = frameImage;
            so.FindProperty("_rarityGlow").objectReferenceValue = rarityImage;
            so.FindProperty("_glowOutline").objectReferenceValue = glowOutlineImage;
            so.FindProperty("_selectionHighlight").objectReferenceValue = selectionHighlight;
            so.FindProperty("_unplayableOverlay").objectReferenceValue = unplayableGroup;

            // New fields for sprite config system
            so.FindProperty("_cardBorder").objectReferenceValue = borderImage;
            so.FindProperty("_cardBackground").objectReferenceValue = bgImage;
            so.FindProperty("_costBg").objectReferenceValue = costBgImage;
            so.FindProperty("_costBorder").objectReferenceValue = costBorderImage;
            so.FindProperty("_costGradient").objectReferenceValue = costGradientImage;
            so.FindProperty("_costInnerBorder").objectReferenceValue = costInnerBorderImage;
            so.FindProperty("_spriteConfig").objectReferenceValue = spriteConfig;

            so.ApplyModifiedPropertiesWithoutUndo();

            // Save prefab
            string fullPath = $"{PREFAB_PATH}/CombatCard.prefab";
            PrefabUtility.SaveAsPrefabAsset(cardRoot, fullPath);

            Object.DestroyImmediate(cardRoot);

            AssetDatabase.Refresh();
            Debug.Log($"[CombatCardPrefabGenerator] CombatCard prefab created at {fullPath}");
            if (spriteConfig != null)
            {
                Debug.Log("[CombatCardPrefabGenerator] CardSpriteConfig assigned successfully");
            }

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

                // CardArt is now child of CardBackground
                var art = bg.Find("CardArt");
                if (art != null)
                {
                    var artImg = art.GetComponent<Image>();
                    if (artImg != null)
                    {
                        visuals.artSprite = artImg.sprite;
                        visuals.artColor = artImg.color;
                        visuals.artType = artImg.type;
                    }
                }
            }

            // CostFrame
            var costFrame = root.Find("CostFrame");
            if (costFrame != null)
            {
                var img = costFrame.GetComponent<Image>();
                if (img != null)
                {
                    visuals.costFrameSprite = img.sprite;
                    visuals.costFrameColor = img.color;
                    visuals.costFrameType = img.type;
                }

                // CostBackground is child of CostFrame
                var costBg = costFrame.Find("CostBackground");
                if (costBg != null)
                {
                    var bgImg = costBg.GetComponent<Image>();
                    if (bgImg != null)
                    {
                        visuals.costBgSprite = bgImg.sprite;
                        visuals.costBgColor = bgImg.color;
                        visuals.costBgType = bgImg.type;
                    }

                    var costText = costBg.GetComponentInChildren<TextMeshProUGUI>();
                    if (costText != null)
                    {
                        visuals.costTextColor = costText.color;
                        visuals.costFont = costText.font;
                    }
                }
            }

            // NameTextBackground
            var nameBg = root.Find("NameTextBackground");
            if (nameBg != null)
            {
                var img = nameBg.GetComponent<Image>();
                if (img != null)
                {
                    visuals.nameBgSprite = img.sprite;
                    visuals.nameBgColor = img.color;
                }

                // NameText is child of NameTextBackground
                var nameT = nameBg.Find("NameText");
                if (nameT != null)
                {
                    var tmp = nameT.GetComponent<TextMeshProUGUI>();
                    if (tmp != null)
                    {
                        visuals.nameTextColor = tmp.color;
                        visuals.nameFont = tmp.font;
                    }
                }
            }

            // DescriptionTextBackground
            var descBg = root.Find("DescriptionTextBackground");
            if (descBg != null)
            {
                var img = descBg.GetComponent<Image>();
                if (img != null)
                {
                    visuals.descBgSprite = img.sprite;
                    visuals.descBgColor = img.color;
                }

                // DescriptionText is child of DescriptionTextBackground
                var descT = descBg.Find("DescriptionText");
                if (descT != null)
                {
                    var tmp = descT.GetComponent<TextMeshProUGUI>();
                    if (tmp != null)
                    {
                        visuals.descTextColor = tmp.color;
                        visuals.descFont = tmp.font;
                    }
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
            public Sprite costFrameSprite;
            public Color costFrameColor;
            public Image.Type costFrameType;
            public Sprite costBgSprite;
            public Color costBgColor;
            public Image.Type costBgType;
            public Color costTextColor;
            public TMP_FontAsset costFont;
            public Sprite nameBgSprite;
            public Color nameBgColor;
            public Color nameTextColor;
            public TMP_FontAsset nameFont;
            public Sprite descBgSprite;
            public Color descBgColor;
            public Color descTextColor;
            public TMP_FontAsset descFont;
            public Sprite glowSprite;
            public Color glowColor;
            public Image.Type glowType;
        }
    }
}
