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

            // === Create visual elements with copied properties from CardBase ===

            // Glow Outline (combat-specific, behind everything)
            GameObject glowOutlineObj = CreateChildImage(cardRoot, "GlowOutline", new Vector2(170, 230));
            glowOutlineObj.transform.SetAsFirstSibling();
            Image glowOutlineImage = glowOutlineObj.GetComponent<Image>();
            glowOutlineImage.color = new Color(0.8f, 0.6f, 0.2f, 0f); // Start invisible

            // Rarity Glow (combat-specific)
            GameObject rarityGlowObj = CreateChildImage(cardRoot, "RarityGlow", new Vector2(165, 225));
            rarityGlowObj.transform.SetSiblingIndex(1);
            Image rarityImage = rarityGlowObj.GetComponent<Image>();
            rarityImage.color = new Color(1f, 1f, 1f, 0.3f);

            // Card Frame (scaled from CardBase)
            GameObject frameObj = CreateChildImage(cardRoot, "CardFrame", new Vector2(160, 220));
            Image frameImage = frameObj.GetComponent<Image>();
            frameImage.sprite = baseVisuals.frameSprite;
            frameImage.color = baseVisuals.frameColor;
            frameImage.type = Image.Type.Sliced;
            frameImage.raycastTarget = true;

            // Card Background (scaled)
            GameObject bgObj = CreateChildImage(cardRoot, "CardBackground", new Vector2(152, 212));
            Image bgImage = bgObj.GetComponent<Image>();
            bgImage.sprite = baseVisuals.bgSprite;
            bgImage.color = baseVisuals.bgColor;
            bgImage.raycastTarget = false;

            // Card Art (scaled and repositioned)
            GameObject artObj = CreateChildImage(cardRoot, "CardArt", new Vector2(140, 90));
            RectTransform artRect = artObj.GetComponent<RectTransform>();
            artRect.anchoredPosition = new Vector2(0, 35);
            Image artImage = artObj.GetComponent<Image>();
            artImage.sprite = baseVisuals.artSprite;
            artImage.color = baseVisuals.artColor;
            artImage.raycastTarget = false;

            // Cost Background (scaled)
            GameObject costBg = CreateChildImage(cardRoot, "CostBackground", new Vector2(36, 36));
            RectTransform costBgRect = costBg.GetComponent<RectTransform>();
            costBgRect.anchorMin = new Vector2(0, 1);
            costBgRect.anchorMax = new Vector2(0, 1);
            costBgRect.pivot = new Vector2(0, 1);
            costBgRect.anchoredPosition = new Vector2(4, -4);
            Image costBgImage = costBg.GetComponent<Image>();
            costBgImage.sprite = baseVisuals.costBgSprite;
            costBgImage.color = baseVisuals.costBgColor;
            costBgImage.raycastTarget = false;

            // Cost Text
            GameObject costTextObj = CreateChildText(costBg, "CostText", "1", 20);
            RectTransform costTextRect = costTextObj.GetComponent<RectTransform>();
            costTextRect.anchorMin = Vector2.zero;
            costTextRect.anchorMax = Vector2.one;
            costTextRect.sizeDelta = Vector2.zero;
            TextMeshProUGUI costText = costTextObj.GetComponent<TextMeshProUGUI>();
            costText.alignment = TextAlignmentOptions.Center;
            costText.color = baseVisuals.costTextColor;
            costText.fontStyle = FontStyles.Bold;
            if (baseVisuals.costFont != null) costText.font = baseVisuals.costFont;

            // Type Icon (combat-specific)
            GameObject typeIcon = CreateChildImage(cardRoot, "TypeIcon", new Vector2(24, 24));
            RectTransform typeRect = typeIcon.GetComponent<RectTransform>();
            typeRect.anchorMin = new Vector2(1, 1);
            typeRect.anchorMax = new Vector2(1, 1);
            typeRect.pivot = new Vector2(1, 1);
            typeRect.anchoredPosition = new Vector2(-4, -4);
            Image typeImage = typeIcon.GetComponent<Image>();
            typeImage.color = Color.white;

            // Name Text (scaled)
            GameObject nameObj = CreateChildText(cardRoot, "CardNameText", "Card Name", 14);
            RectTransform nameRect = nameObj.GetComponent<RectTransform>();
            nameRect.anchoredPosition = new Vector2(0, -25);
            nameRect.sizeDelta = new Vector2(140, 24);
            TextMeshProUGUI nameText = nameObj.GetComponent<TextMeshProUGUI>();
            nameText.alignment = TextAlignmentOptions.Center;
            nameText.color = baseVisuals.nameTextColor;
            nameText.fontStyle = FontStyles.Bold;
            if (baseVisuals.nameFont != null) nameText.font = baseVisuals.nameFont;

            // Description Text (scaled)
            GameObject descObj = CreateChildText(cardRoot, "DescriptionText", "Deal 6 damage.", 11);
            RectTransform descRect = descObj.GetComponent<RectTransform>();
            descRect.anchoredPosition = new Vector2(0, -65);
            descRect.sizeDelta = new Vector2(140, 50);
            TextMeshProUGUI descText = descObj.GetComponent<TextMeshProUGUI>();
            descText.alignment = TextAlignmentOptions.Center;
            descText.color = baseVisuals.descTextColor;
            descText.textWrappingMode = TextWrappingModes.Normal;
            if (baseVisuals.descFont != null) descText.font = baseVisuals.descFont;

            // Owner Portrait Container (combat-specific)
            GameObject ownerContainer = new GameObject("OwnerPortraitContainer");
            ownerContainer.transform.SetParent(cardRoot.transform, false);
            RectTransform ownerContainerRect = ownerContainer.AddComponent<RectTransform>();
            ownerContainerRect.anchorMin = new Vector2(0.5f, 0);
            ownerContainerRect.anchorMax = new Vector2(0.5f, 0);
            ownerContainerRect.pivot = new Vector2(0.5f, 0);
            ownerContainerRect.anchoredPosition = new Vector2(0, 8);
            ownerContainerRect.sizeDelta = new Vector2(28, 28);

            GameObject ownerPortrait = CreateChildImage(ownerContainer, "OwnerPortrait", new Vector2(28, 28));
            Image ownerImage = ownerPortrait.GetComponent<Image>();
            ownerImage.color = new Color(0.5f, 0.5f, 0.5f, 1f);

            // Selection Highlight (from CardBase glow, resized)
            GameObject selectionHighlight = CreateChildImage(cardRoot, "SelectionHighlight", new Vector2(168, 228));
            selectionHighlight.transform.SetAsLastSibling();
            Image selectionImage = selectionHighlight.GetComponent<Image>();
            selectionImage.sprite = baseVisuals.glowSprite;
            selectionImage.color = new Color(1f, 1f, 0f, 0.4f);
            selectionHighlight.SetActive(false);

            // Unplayable Overlay (combat-specific)
            GameObject unplayableOverlay = CreateChildImage(cardRoot, "UnplayableOverlay", new Vector2(160, 220));
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
            public Sprite bgSprite;
            public Color bgColor;
            public Sprite artSprite;
            public Color artColor;
            public Sprite costBgSprite;
            public Color costBgColor;
            public Color costTextColor;
            public TMP_FontAsset costFont;
            public Color nameTextColor;
            public TMP_FontAsset nameFont;
            public Color descTextColor;
            public TMP_FontAsset descFont;
            public Sprite glowSprite;
            public Color glowColor;
        }
    }
}
