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
    ///
    /// Structure mirrors CardBase with responsive anchors:
    /// - CardFrame: Outer border (stretched)
    /// - CardBackground: Mask shape with Mask component (stretched)
    ///   └── CardArt: Full background art (stretched, masked)
    /// - CostFrame: Frame around cost (top-left, anchored)
    ///   └── CostBackground
    ///      └── CostText
    /// - NameTextBackground: Semi-transparent bar (top-center, anchored)
    ///   └── CardNameText
    /// - DescriptionText (bottom, anchored)
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

            // Add RectTransform (smaller than CardBase for combat, 5:7 ratio)
            RectTransform rootRect = cardRoot.AddComponent<RectTransform>();
            rootRect.sizeDelta = new Vector2(160, 224);

            // Add CanvasGroup for alpha control
            cardRoot.AddComponent<CanvasGroup>();

            // Add CombatCard script
            var combatCard = cardRoot.AddComponent<CombatCard>();

            // === Create visual elements with STRETCH anchors for proper scaling ===

            // Glow Outline (combat-specific, behind everything)
            GameObject glowOutlineObj = CreateStretchedChildImage(cardRoot, "GlowOutline", -5, -5, -5, -5);
            glowOutlineObj.transform.SetAsFirstSibling();
            Image glowOutlineImage = glowOutlineObj.GetComponent<Image>();
            glowOutlineImage.color = new Color(0.8f, 0.6f, 0.2f, 0f);

            // Rarity Glow (combat-specific)
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

            // Card Background - stretch with small padding, add Mask for CardArt
            GameObject bgObj = CreateStretchedChildImage(cardRoot, "CardBackground", 4, 4, 4, 4);
            Image bgImage = bgObj.GetComponent<Image>();
            bgImage.sprite = baseVisuals.bgSprite;
            bgImage.color = baseVisuals.bgColor;
            bgImage.type = baseVisuals.bgType;
            bgImage.raycastTarget = false;
            // Add Mask component
            Mask bgMask = bgObj.AddComponent<Mask>();
            bgMask.showMaskGraphic = true;

            // Card Art - child of CardBackground, stretches to fill (masked)
            GameObject artObj = CreateStretchedChildImage(bgObj, "CardArt", 0, 0, 0, 0);
            Image artImage = artObj.GetComponent<Image>();
            artImage.sprite = baseVisuals.artSprite;
            artImage.color = baseVisuals.artColor;
            artImage.type = baseVisuals.artType;
            artImage.preserveAspect = false;
            artImage.raycastTarget = false;

            // === LAYOUT MATCHED TO CARDBASE PROPORTIONS (200x280) ===
            // All anchors calculated from CardBase pixel positions for visual consistency

            // Cost Frame - matches CardBase: pos(2,-2), size 48x48
            // Proportions: left 1%, right 25%, bottom 82.1%, top 99.3%
            GameObject costFrame = CreateChildImage(cardRoot, "CostFrame", Vector2.zero);
            RectTransform costFrameRect = costFrame.GetComponent<RectTransform>();
            costFrameRect.anchorMin = new Vector2(0.01f, 0.821f);
            costFrameRect.anchorMax = new Vector2(0.25f, 0.993f);
            costFrameRect.pivot = new Vector2(0.5f, 0.5f);
            costFrameRect.sizeDelta = Vector2.zero;
            costFrameRect.offsetMin = Vector2.zero;
            costFrameRect.offsetMax = Vector2.zero;
            Image costFrameImage = costFrame.GetComponent<Image>();
            costFrameImage.sprite = baseVisuals.costFrameSprite;
            costFrameImage.color = baseVisuals.costFrameColor;
            costFrameImage.type = baseVisuals.costFrameType;
            costFrameImage.raycastTarget = false;

            // Cost Background - child of CostFrame, inset by ~8% (matches 4px on 48px)
            GameObject costBg = CreateStretchedChildImage(costFrame, "CostBackground", 0.08f, 0.08f, 0.08f, 0.08f);
            RectTransform costBgRect = costBg.GetComponent<RectTransform>();
            // Convert padding to proper inset
            costBgRect.offsetMin = new Vector2(3, 3);
            costBgRect.offsetMax = new Vector2(-3, -3);
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

            // Type Icon - COMBAT-SPECIFIC: positioned below CostFrame
            // Small icon area below cost for card type indicator
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

            // Name Text Background - matches CardBase: pos(54,-10), size 140x32
            // Proportions: left 27%, right 97%, bottom 85%, top 96.4%
            GameObject nameBgObj = CreateChildImage(cardRoot, "NameTextBackground", Vector2.zero);
            RectTransform nameBgRect = nameBgObj.GetComponent<RectTransform>();
            nameBgRect.anchorMin = new Vector2(0.27f, 0.85f);
            nameBgRect.anchorMax = new Vector2(0.97f, 0.964f);
            nameBgRect.pivot = new Vector2(0.5f, 0.5f);
            nameBgRect.sizeDelta = Vector2.zero;
            nameBgRect.offsetMin = Vector2.zero;
            nameBgRect.offsetMax = Vector2.zero;
            Image nameBgImage = nameBgObj.GetComponent<Image>();
            nameBgImage.sprite = baseVisuals.nameBgSprite;
            nameBgImage.color = baseVisuals.nameBgColor;
            nameBgImage.raycastTarget = false;

            // Name Text - child of NameTextBackground with auto-sizing
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

            // Description Text Background - matches CardBase: pos(0,5), size 190x65
            // Proportions: left 2.5%, right 97.5%, bottom 1.8%, top 25%
            GameObject descBgObj = CreateChildImage(cardRoot, "DescriptionTextBackground", Vector2.zero);
            RectTransform descBgRect = descBgObj.GetComponent<RectTransform>();
            descBgRect.anchorMin = new Vector2(0.025f, 0.018f);
            descBgRect.anchorMax = new Vector2(0.975f, 0.25f);
            descBgRect.pivot = new Vector2(0.5f, 0.5f);
            descBgRect.sizeDelta = Vector2.zero;
            descBgRect.offsetMin = Vector2.zero;
            descBgRect.offsetMax = Vector2.zero;
            Image descBgImage = descBgObj.GetComponent<Image>();
            descBgImage.sprite = baseVisuals.descBgSprite;
            descBgImage.color = baseVisuals.descBgColor;
            descBgImage.raycastTarget = false;

            // Description Text - child of DescriptionTextBackground with auto-sizing
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
