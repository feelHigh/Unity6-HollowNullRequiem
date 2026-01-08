// ============================================
// CardBasePrefabGenerator.cs
// Editor tool to generate CardBase root prefab (visual-only, no scripts)
// ============================================

using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;
using System.IO;
using HNR.UI.Config;

namespace HNR.Editor
{
    /// <summary>
    /// Generates the CardBase.prefab root prefab with visual elements only.
    /// This serves as the base for Card.prefab and CombatCard.prefab variants.
    ///
    /// New structure with GUI Pro-FantasyHero sprites:
    /// - SelectionGlow: Selection highlight (behind everything)
    /// - CardBackground: Mask with Bg sprite (clips CardArt and text masks)
    ///   ├── CardArt: Full background art (masked)
    ///   ├── NameTextMask: RectMask2D for name text clipping
    ///   │   └── NameTextBackground → NameText
    ///   └── DescriptionTextMask: RectMask2D for description clipping
    ///       └── DescriptionTextBackground → DescriptionText
    /// - CardBorder: Border or BorderGem sprite (on top, rarity-based)
    /// - CardFrame: Legacy frame (hidden, for backwards compatibility)
    /// - CostFrame: Layered cost display
    ///   ├── CostBg, CostBorder, CostGradient, CostInnerBorder (tinted)
    ///   └── CostText
    /// </summary>
    public static class CardBasePrefabGenerator
    {
        private const string PREFAB_PATH = "Assets/_Project/Prefabs/Cards";
        private const string PREFAB_NAME = "CardBase.prefab";
        private const string SPRITE_CONFIG_PATH = "Assets/_Project/Data/Config/CardSpriteConfig.asset";

        /// <summary>
        /// Generates the CardBase prefab with masked card art background.
        /// </summary>
        public static void GenerateCardBasePrefab()
        {
            // Ensure directory exists
            if (!Directory.Exists(PREFAB_PATH))
            {
                Directory.CreateDirectory(PREFAB_PATH);
                AssetDatabase.Refresh();
            }

            // Load sprite config
            var spriteConfig = AssetDatabase.LoadAssetAtPath<CardSpriteConfigSO>(SPRITE_CONFIG_PATH);
            if (spriteConfig == null)
            {
                Debug.LogWarning("[CardBasePrefabGenerator] CardSpriteConfig not found. Run HNR > Config > Generate Card Sprite Config first. Using fallback visuals.");
            }

            // Create root GameObject (visual-only, no scripts)
            GameObject cardBase = new GameObject("CardBase");

            // Add RectTransform with standard card size (5:7 ratio)
            RectTransform rootRect = cardBase.AddComponent<RectTransform>();
            rootRect.sizeDelta = new Vector2(200, 280);

            // === 1. Selection Glow (behind everything, initially inactive) ===
            GameObject glowObj = CreateStretchedChildImage(cardBase, "SelectionGlow", -10, -10, -10, -10);
            Image glowImage = glowObj.GetComponent<Image>();
            glowImage.color = new Color(1f, 0.9f, 0.3f, 0.5f); // Yellow tint
            glowImage.raycastTarget = false;
            glowObj.transform.SetAsFirstSibling();
            glowObj.SetActive(false);

            // === 2. Card Background with Mask (clips CardArt and text masks) ===
            GameObject bgObj = CreateStretchedChildImage(cardBase, "CardBackground", 4, 4, 4, 4);
            Image bgImage = bgObj.GetComponent<Image>();
            if (spriteConfig != null)
            {
                bgImage.sprite = spriteConfig.GetBackgroundSprite(HNR.Cards.CardType.Skill);
            }
            bgImage.color = Color.white;
            bgImage.type = Image.Type.Sliced;
            bgImage.raycastTarget = false;
            Mask bgMask = bgObj.AddComponent<Mask>();
            bgMask.showMaskGraphic = true;

            // === 2a. Card Art (child of CardBackground, masked) ===
            GameObject artObj = CreateStretchedChildImage(bgObj, "CardArt", 0, 0, 0, 0);
            Image artImage = artObj.GetComponent<Image>();
            artImage.color = new Color(0.5f, 0.5f, 0.5f, 1f); // Placeholder gray
            artImage.raycastTarget = false;
            artImage.preserveAspect = false;

            // === 2b. Name Text Mask (RectMask2D, positioned at top within CardBackground) ===
            GameObject nameTextMask = CreateStretchedChildImage(bgObj, "NameTextMask", 0, 0, 0, 0);
            RectTransform nameMaskRect = nameTextMask.GetComponent<RectTransform>();
            nameMaskRect.anchorMin = new Vector2(0.20f, 0.82f);
            nameMaskRect.anchorMax = new Vector2(1.0f, 1.0f);
            nameMaskRect.offsetMin = Vector2.zero;
            nameMaskRect.offsetMax = Vector2.zero;
            Object.DestroyImmediate(nameTextMask.GetComponent<Image>());
            nameTextMask.AddComponent<RectMask2D>();

            // Name Text Background (child of mask)
            GameObject nameBgObj = CreateStretchedChildImage(nameTextMask, "NameTextBackground", 0, 0, 0, 0);
            Image nameBgImage = nameBgObj.GetComponent<Image>();
            nameBgImage.color = new Color(0f, 0f, 0f, 0.5f);
            nameBgImage.raycastTarget = false;

            // Name Text
            GameObject nameObj = CreateChildText(nameBgObj, "NameText", "Card Name", 14);
            RectTransform nameRect = nameObj.GetComponent<RectTransform>();
            nameRect.anchorMin = Vector2.zero;
            nameRect.anchorMax = Vector2.one;
            nameRect.sizeDelta = Vector2.zero;
            nameRect.offsetMin = new Vector2(4, 2);
            nameRect.offsetMax = new Vector2(-4, -2);
            TextMeshProUGUI nameText = nameObj.GetComponent<TextMeshProUGUI>();
            nameText.alignment = TextAlignmentOptions.Center;
            nameText.color = Color.white;
            nameText.fontStyle = FontStyles.Bold;
            nameText.enableAutoSizing = true;
            nameText.fontSizeMin = 8;
            nameText.fontSizeMax = 14;

            // === 2c. Description Text Mask (RectMask2D, positioned at bottom within CardBackground) ===
            GameObject descTextMask = CreateStretchedChildImage(bgObj, "DescriptionTextMask", 0, 0, 0, 0);
            RectTransform descMaskRect = descTextMask.GetComponent<RectTransform>();
            descMaskRect.anchorMin = new Vector2(0f, 0f);
            descMaskRect.anchorMax = new Vector2(1.0f, 0.28f);
            descMaskRect.offsetMin = Vector2.zero;
            descMaskRect.offsetMax = Vector2.zero;
            Object.DestroyImmediate(descTextMask.GetComponent<Image>());
            descTextMask.AddComponent<RectMask2D>();

            // Description Text Background (child of mask)
            GameObject descBgObj = CreateStretchedChildImage(descTextMask, "DescriptionTextBackground", 0, 0, 0, 0);
            Image descBgImage = descBgObj.GetComponent<Image>();
            descBgImage.color = new Color(0f, 0f, 0f, 0.5f);
            descBgImage.raycastTarget = false;

            // Description Text
            GameObject descObj = CreateChildText(descBgObj, "DescriptionText", "Card effect description goes here.", 11);
            RectTransform descRect = descObj.GetComponent<RectTransform>();
            descRect.anchorMin = Vector2.zero;
            descRect.anchorMax = Vector2.one;
            descRect.sizeDelta = Vector2.zero;
            descRect.offsetMin = new Vector2(6, 4);
            descRect.offsetMax = new Vector2(-6, -4);
            TextMeshProUGUI descText = descObj.GetComponent<TextMeshProUGUI>();
            descText.alignment = TextAlignmentOptions.Center;
            descText.color = new Color(0.95f, 0.95f, 0.95f, 1f);
            descText.textWrappingMode = TextWrappingModes.Normal;
            descText.enableAutoSizing = true;
            descText.fontSizeMin = 8;
            descText.fontSizeMax = 11;

            // === 3. Card Border (separate layer on top of background) ===
            GameObject borderObj = CreateStretchedChildImage(cardBase, "CardBorder", 0, 0, 0, 0);
            Image borderImage = borderObj.GetComponent<Image>();
            if (spriteConfig != null)
            {
                borderImage.sprite = spriteConfig.GetFrameSet(HNR.Cards.CardType.Skill).Border;
            }
            borderImage.color = Color.white;
            borderImage.type = Image.Type.Sliced;
            borderImage.raycastTarget = true;

            // === 4. Legacy Card Frame (hidden, for backwards compatibility) ===
            GameObject frameObj = CreateStretchedChildImage(cardBase, "CardFrame", 0, 0, 0, 0);
            Image frameImage = frameObj.GetComponent<Image>();
            frameImage.color = new Color(0.3f, 0.3f, 0.3f, 1f);
            frameImage.type = Image.Type.Sliced;
            frameImage.raycastTarget = false;
            frameObj.SetActive(false);

            // === 5. Cost Frame with layered sprites ===
            GameObject costFrame = new GameObject("CostFrame");
            costFrame.transform.SetParent(cardBase.transform, false);
            RectTransform costFrameRect = costFrame.AddComponent<RectTransform>();
            costFrameRect.anchorMin = new Vector2(0, 1);
            costFrameRect.anchorMax = new Vector2(0, 1);
            costFrameRect.pivot = new Vector2(0, 1);
            costFrameRect.anchoredPosition = new Vector2(2, -2);
            costFrameRect.sizeDelta = new Vector2(48, 48);

            // 5a. Cost Bg layer
            GameObject costBgObj = CreateStretchedChildImage(costFrame, "CostBg", 0, 0, 0, 0);
            Image costBgImage = costBgObj.GetComponent<Image>();
            if (spriteConfig != null && spriteConfig.CostFrameBg != null)
            {
                costBgImage.sprite = spriteConfig.CostFrameBg;
            }
            costBgImage.color = new Color(0.2f, 0.7f, 0.3f);
            costBgImage.type = Image.Type.Sliced;
            costBgImage.raycastTarget = false;

            // 5b. Cost Border layer
            GameObject costBorderObj = CreateStretchedChildImage(costFrame, "CostBorder", 0, 0, 0, 0);
            Image costBorderImage = costBorderObj.GetComponent<Image>();
            if (spriteConfig != null && spriteConfig.CostFrameBorder != null)
            {
                costBorderImage.sprite = spriteConfig.CostFrameBorder;
            }
            costBorderImage.color = new Color(0.2f, 0.7f, 0.3f);
            costBorderImage.type = Image.Type.Sliced;
            costBorderImage.raycastTarget = false;

            // 5c. Cost Gradient layer
            GameObject costGradientObj = CreateStretchedChildImage(costFrame, "CostGradient", 2, 2, 2, 2);
            Image costGradientImage = costGradientObj.GetComponent<Image>();
            if (spriteConfig != null && spriteConfig.CostFrameGradient != null)
            {
                costGradientImage.sprite = spriteConfig.CostFrameGradient;
            }
            costGradientImage.color = new Color(0.2f, 0.7f, 0.3f);
            costGradientImage.type = Image.Type.Sliced;
            costGradientImage.raycastTarget = false;

            // 5d. Cost Inner Border layer
            GameObject costInnerBorderObj = CreateStretchedChildImage(costFrame, "CostInnerBorder", 4, 4, 4, 4);
            Image costInnerBorderImage = costInnerBorderObj.GetComponent<Image>();
            if (spriteConfig != null && spriteConfig.CostFrameInnerBorder != null)
            {
                costInnerBorderImage.sprite = spriteConfig.CostFrameInnerBorder;
            }
            costInnerBorderImage.color = new Color(0.2f, 0.7f, 0.3f);
            costInnerBorderImage.type = Image.Type.Sliced;
            costInnerBorderImage.raycastTarget = false;

            // 5e. Cost Text
            GameObject costTextObj = CreateChildText(costFrame, "CostText", "1", 24);
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
            costText.fontSizeMin = 12;
            costText.fontSizeMax = 24;

            // 5f. Legacy Cost Background (hidden)
            GameObject legacyCostBg = CreateStretchedChildImage(costFrame, "CostBackground", 4, 4, 4, 4);
            Image legacyCostBgImage = legacyCostBg.GetComponent<Image>();
            legacyCostBgImage.color = new Color(0.1f, 0.1f, 0.3f, 1f);
            legacyCostBgImage.raycastTarget = false;
            legacyCostBg.SetActive(false);

            // Save as prefab
            string fullPath = $"{PREFAB_PATH}/{PREFAB_NAME}";
            PrefabUtility.SaveAsPrefabAsset(cardBase, fullPath);

            // Cleanup scene object
            Object.DestroyImmediate(cardBase);

            AssetDatabase.Refresh();
            Debug.Log($"[CardBasePrefabGenerator] CardBase prefab created at {fullPath}");
            if (spriteConfig != null)
            {
                Debug.Log("[CardBasePrefabGenerator] CardSpriteConfig applied successfully");
            }

            // Select the created prefab
            Selection.activeObject = AssetDatabase.LoadAssetAtPath<GameObject>(fullPath);
        }

        /// <summary>
        /// Gets the CardBase prefab asset.
        /// </summary>
        public static GameObject GetCardBasePrefab()
        {
            string fullPath = $"{PREFAB_PATH}/{PREFAB_NAME}";
            return AssetDatabase.LoadAssetAtPath<GameObject>(fullPath);
        }

        private static GameObject CreateStretchedChildImage(GameObject parent, string name, float paddingLeft, float paddingRight, float paddingTop, float paddingBottom)
        {
            GameObject obj = new GameObject(name);
            obj.transform.SetParent(parent.transform, false);
            RectTransform rect = obj.AddComponent<RectTransform>();

            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.pivot = new Vector2(0.5f, 0.5f);
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
            rect.sizeDelta = new Vector2(180, 30);

            TextMeshProUGUI tmp = obj.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = fontSize;

            return obj;
        }
    }
}
