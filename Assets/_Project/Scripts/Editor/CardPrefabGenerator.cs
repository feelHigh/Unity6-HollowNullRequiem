// ============================================
// CardPrefabGenerator.cs
// Editor tool to generate Card prefab based on CardBase visuals
// ============================================

using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;
using System.IO;

namespace HNR.Editor
{
    /// <summary>
    /// Generates Card.prefab based on CardBase.prefab visuals.
    /// Copies visual properties (sprites, colors) from CardBase.
    /// Regenerate after modifying CardBase to apply changes.
    ///
    /// Structure mirrors CardBase:
    /// - CardFrame: Outer border
    /// - CardBackground: Mask shape with Mask component
    ///   └── CardArt: Full background art (masked)
    /// - CostFrame: Frame around cost (top-left)
    ///   └── CostBackground
    ///      └── CostText
    /// - NameTextBackground: Semi-transparent bar (top-center)
    ///   └── NameText
    /// - DescriptionText
    /// - SelectionGlow
    /// </summary>
    public static class CardPrefabGenerator
    {
        private const string PREFAB_PATH = "Assets/_Project/Prefabs/Cards";
        private const string PREFAB_NAME = "Card.prefab";

        /// <summary>
        /// Generates the Card prefab, copying visuals from CardBase.
        /// </summary>
        public static void GenerateCardPrefab()
        {
            // Ensure CardBase exists first
            var cardBasePrefab = CardBasePrefabGenerator.GetCardBasePrefab();
            if (cardBasePrefab == null)
            {
                Debug.Log("[CardPrefabGenerator] CardBase not found, generating it first...");
                CardBasePrefabGenerator.GenerateCardBasePrefab();
                cardBasePrefab = CardBasePrefabGenerator.GetCardBasePrefab();
            }

            if (cardBasePrefab == null)
            {
                Debug.LogError("[CardPrefabGenerator] Failed to get CardBase prefab");
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

            // Create new Card prefab with copied visuals
            GameObject cardRoot = new GameObject("Card");

            // Add RectTransform with same size as CardBase (5:7 ratio)
            RectTransform rootRect = cardRoot.AddComponent<RectTransform>();
            rootRect.sizeDelta = baseVisuals.rootSize != Vector2.zero ? baseVisuals.rootSize : new Vector2(200, 280);

            // Add Card-specific components
            Canvas canvas = cardRoot.AddComponent<Canvas>();
            canvas.overrideSorting = true;
            canvas.sortingOrder = 0;

            cardRoot.AddComponent<CanvasGroup>();
            cardRoot.AddComponent<GraphicRaycaster>();

            var cardScript = cardRoot.AddComponent<Cards.Card>();

            // === Create visual elements matching CardBase structure ===

            // Card Frame - outer border
            Vector2 frameSize = baseVisuals.frameSize != Vector2.zero ? baseVisuals.frameSize : new Vector2(200, 280);
            GameObject frameObj = CreateChildImage(cardRoot, "CardFrame", frameSize);
            Image frameImage = frameObj.GetComponent<Image>();
            frameImage.sprite = baseVisuals.frameSprite;
            frameImage.color = baseVisuals.frameColor;
            frameImage.type = baseVisuals.frameType;
            frameImage.raycastTarget = true;

            // Card Background - mask shape for CardArt
            Vector2 bgSize = baseVisuals.bgSize != Vector2.zero ? baseVisuals.bgSize : new Vector2(192, 272);
            GameObject bgObj = CreateChildImage(cardRoot, "CardBackground", bgSize);
            Image bgImage = bgObj.GetComponent<Image>();
            bgImage.sprite = baseVisuals.bgSprite;
            bgImage.color = baseVisuals.bgColor;
            bgImage.type = baseVisuals.bgType;
            bgImage.raycastTarget = false;
            // Add Mask component
            Mask bgMask = bgObj.AddComponent<Mask>();
            bgMask.showMaskGraphic = true;

            // Card Art - full background, child of CardBackground for masking
            GameObject artObj = CreateChildImage(bgObj, "CardArt", Vector2.zero);
            RectTransform artRect = artObj.GetComponent<RectTransform>();
            artRect.anchorMin = Vector2.zero;
            artRect.anchorMax = Vector2.one;
            artRect.offsetMin = Vector2.zero;
            artRect.offsetMax = Vector2.zero;
            Image artImage = artObj.GetComponent<Image>();
            artImage.sprite = baseVisuals.artSprite;
            artImage.color = baseVisuals.artColor;
            artImage.type = baseVisuals.artType;
            artImage.preserveAspect = false;
            artImage.raycastTarget = false;

            // Cost Frame - outer frame for cost (top-left)
            Vector2 costFrameSize = baseVisuals.costFrameSize != Vector2.zero ? baseVisuals.costFrameSize : new Vector2(48, 48);
            GameObject costFrame = CreateChildImage(cardRoot, "CostFrame", costFrameSize);
            RectTransform costFrameRect = costFrame.GetComponent<RectTransform>();
            costFrameRect.anchorMin = new Vector2(0, 1);
            costFrameRect.anchorMax = new Vector2(0, 1);
            costFrameRect.pivot = new Vector2(0, 1);
            costFrameRect.anchoredPosition = new Vector2(2, -2);
            Image costFrameImage = costFrame.GetComponent<Image>();
            costFrameImage.sprite = baseVisuals.costFrameSprite;
            costFrameImage.color = baseVisuals.costFrameColor;
            costFrameImage.type = baseVisuals.costFrameType;
            costFrameImage.raycastTarget = false;

            // Cost Background - inner, inside CostFrame
            Vector2 costBgSize = baseVisuals.costBgSize != Vector2.zero ? baseVisuals.costBgSize : new Vector2(40, 40);
            GameObject costBg = CreateChildImage(costFrame, "CostBackground", costBgSize);
            RectTransform costBgRect = costBg.GetComponent<RectTransform>();
            costBgRect.anchorMin = new Vector2(0.5f, 0.5f);
            costBgRect.anchorMax = new Vector2(0.5f, 0.5f);
            costBgRect.pivot = new Vector2(0.5f, 0.5f);
            costBgRect.anchoredPosition = Vector2.zero;
            Image costBgImage = costBg.GetComponent<Image>();
            costBgImage.sprite = baseVisuals.costBgSprite;
            costBgImage.color = baseVisuals.costBgColor;
            costBgImage.type = baseVisuals.costBgType;
            costBgImage.raycastTarget = false;

            // Cost Text - fills cost background
            GameObject costTextObj = CreateChildText(costBg, "CostText", "1", 24);
            RectTransform costTextRect = costTextObj.GetComponent<RectTransform>();
            costTextRect.anchorMin = Vector2.zero;
            costTextRect.anchorMax = Vector2.one;
            costTextRect.sizeDelta = Vector2.zero;
            costTextRect.offsetMin = Vector2.zero;
            costTextRect.offsetMax = Vector2.zero;
            TextMeshProUGUI costText = costTextObj.GetComponent<TextMeshProUGUI>();
            costText.alignment = TextAlignmentOptions.Center;
            costText.color = baseVisuals.costTextColor;
            if (baseVisuals.costFont != null) costText.font = baseVisuals.costFont;

            // Name Text Background - positioned next to CostFrame (top area)
            Vector2 nameBgSize = baseVisuals.nameBgSize != Vector2.zero ? baseVisuals.nameBgSize : new Vector2(140, 32);
            GameObject nameBgObj = CreateChildImage(cardRoot, "NameTextBackground", nameBgSize);
            RectTransform nameBgRect = nameBgObj.GetComponent<RectTransform>();
            nameBgRect.anchorMin = new Vector2(0, 1);
            nameBgRect.anchorMax = new Vector2(0, 1);
            nameBgRect.pivot = new Vector2(0, 1);
            nameBgRect.anchoredPosition = new Vector2(54, -10); // Right of CostFrame
            Image nameBgImage = nameBgObj.GetComponent<Image>();
            nameBgImage.sprite = baseVisuals.nameBgSprite;
            nameBgImage.color = baseVisuals.nameBgColor;
            nameBgImage.raycastTarget = false;

            // Name Text - child of NameTextBackground
            GameObject nameObj = CreateChildText(nameBgObj, "NameText", "Card Name", 14);
            RectTransform nameRect = nameObj.GetComponent<RectTransform>();
            nameRect.anchorMin = Vector2.zero;
            nameRect.anchorMax = Vector2.one;
            nameRect.sizeDelta = Vector2.zero;
            nameRect.offsetMin = new Vector2(4, 2);
            nameRect.offsetMax = new Vector2(-4, -2);
            TextMeshProUGUI nameText = nameObj.GetComponent<TextMeshProUGUI>();
            nameText.alignment = TextAlignmentOptions.Center;
            nameText.color = baseVisuals.nameTextColor;
            if (baseVisuals.nameFont != null) nameText.font = baseVisuals.nameFont;

            // Description Text Background - semi-transparent black (bottom area)
            Vector2 descBgSize = baseVisuals.descBgSize != Vector2.zero ? baseVisuals.descBgSize : new Vector2(190, 65);
            GameObject descBgObj = CreateChildImage(cardRoot, "DescriptionTextBackground", descBgSize);
            RectTransform descBgRect = descBgObj.GetComponent<RectTransform>();
            descBgRect.anchorMin = new Vector2(0.5f, 0);
            descBgRect.anchorMax = new Vector2(0.5f, 0);
            descBgRect.pivot = new Vector2(0.5f, 0);
            descBgRect.anchoredPosition = new Vector2(0, 5);
            Image descBgImage = descBgObj.GetComponent<Image>();
            descBgImage.sprite = baseVisuals.descBgSprite;
            descBgImage.color = baseVisuals.descBgColor;
            descBgImage.raycastTarget = false;

            // Description Text - child of DescriptionTextBackground
            GameObject descObj = CreateChildText(descBgObj, "DescriptionText", "Card effect description goes here.", 11);
            RectTransform descRect = descObj.GetComponent<RectTransform>();
            descRect.anchorMin = Vector2.zero;
            descRect.anchorMax = Vector2.one;
            descRect.sizeDelta = Vector2.zero;
            descRect.offsetMin = new Vector2(6, 4);
            descRect.offsetMax = new Vector2(-6, -4);
            TextMeshProUGUI descText = descObj.GetComponent<TextMeshProUGUI>();
            descText.alignment = TextAlignmentOptions.Center;
            descText.color = baseVisuals.descTextColor;
            descText.textWrappingMode = TextWrappingModes.Normal;
            if (baseVisuals.descFont != null) descText.font = baseVisuals.descFont;

            // Selection Glow - larger than card, behind everything
            Vector2 glowSize = baseVisuals.glowSize != Vector2.zero ? baseVisuals.glowSize : new Vector2(220, 300);
            GameObject glowObj = CreateChildImage(cardRoot, "SelectionGlow", glowSize);
            Image glowImage = glowObj.GetComponent<Image>();
            glowImage.sprite = baseVisuals.glowSprite;
            glowImage.color = baseVisuals.glowColor;
            glowImage.type = baseVisuals.glowType;
            glowImage.raycastTarget = false;
            glowObj.transform.SetAsFirstSibling();
            glowObj.SetActive(false);

            // === Wire up references ===
            SerializedObject so = new SerializedObject(cardScript);
            so.FindProperty("_cardFrame").objectReferenceValue = frameImage;
            so.FindProperty("_cardArt").objectReferenceValue = artImage;
            so.FindProperty("_nameText").objectReferenceValue = nameText;
            so.FindProperty("_costText").objectReferenceValue = costText;
            so.FindProperty("_descriptionText").objectReferenceValue = descText;
            so.FindProperty("_selectionGlow").objectReferenceValue = glowObj;
            so.ApplyModifiedPropertiesWithoutUndo();

            // Save prefab
            string fullPath = $"{PREFAB_PATH}/{PREFAB_NAME}";
            PrefabUtility.SaveAsPrefabAsset(cardRoot, fullPath);

            Object.DestroyImmediate(cardRoot);

            AssetDatabase.Refresh();
            Debug.Log($"[CardPrefabGenerator] Card prefab created at {fullPath} (visuals copied from CardBase)");

            Selection.activeObject = AssetDatabase.LoadAssetAtPath<GameObject>(fullPath);
        }

        /// <summary>
        /// Extracts visual properties from CardBase prefab.
        /// </summary>
        private static CardBaseVisuals ExtractVisualsFromCardBase(GameObject cardBase)
        {
            var visuals = new CardBaseVisuals();
            Transform root = cardBase.transform;

            // Root size
            var rootRect = cardBase.GetComponent<RectTransform>();
            if (rootRect != null)
            {
                visuals.rootSize = rootRect.sizeDelta;
            }

            // CardFrame
            var frame = root.Find("CardFrame");
            if (frame != null)
            {
                var img = frame.GetComponent<Image>();
                var rect = frame.GetComponent<RectTransform>();
                if (img != null)
                {
                    visuals.frameSprite = img.sprite;
                    visuals.frameColor = img.color;
                    visuals.frameType = img.type;
                }
                if (rect != null)
                {
                    visuals.frameSize = rect.sizeDelta;
                }
            }

            // CardBackground
            var bg = root.Find("CardBackground");
            if (bg != null)
            {
                var img = bg.GetComponent<Image>();
                var rect = bg.GetComponent<RectTransform>();
                if (img != null)
                {
                    visuals.bgSprite = img.sprite;
                    visuals.bgColor = img.color;
                    visuals.bgType = img.type;
                }
                if (rect != null)
                {
                    visuals.bgSize = rect.sizeDelta;
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
                var rect = costFrame.GetComponent<RectTransform>();
                if (img != null)
                {
                    visuals.costFrameSprite = img.sprite;
                    visuals.costFrameColor = img.color;
                    visuals.costFrameType = img.type;
                }
                if (rect != null)
                {
                    visuals.costFrameSize = rect.sizeDelta;
                }

                // CostBackground is child of CostFrame
                var costBg = costFrame.Find("CostBackground");
                if (costBg != null)
                {
                    var bgImg = costBg.GetComponent<Image>();
                    var bgRect = costBg.GetComponent<RectTransform>();
                    if (bgImg != null)
                    {
                        visuals.costBgSprite = bgImg.sprite;
                        visuals.costBgColor = bgImg.color;
                        visuals.costBgType = bgImg.type;
                    }
                    if (bgRect != null)
                    {
                        visuals.costBgSize = bgRect.sizeDelta;
                    }

                    // CostText
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
                var rect = nameBg.GetComponent<RectTransform>();
                if (img != null)
                {
                    visuals.nameBgSprite = img.sprite;
                    visuals.nameBgColor = img.color;
                }
                if (rect != null)
                {
                    visuals.nameBgSize = rect.sizeDelta;
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
                var rect = descBg.GetComponent<RectTransform>();
                if (img != null)
                {
                    visuals.descBgSprite = img.sprite;
                    visuals.descBgColor = img.color;
                }
                if (rect != null)
                {
                    visuals.descBgSize = rect.sizeDelta;
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
                var rect = glow.GetComponent<RectTransform>();
                if (img != null)
                {
                    visuals.glowSprite = img.sprite;
                    visuals.glowColor = img.color;
                    visuals.glowType = img.type;
                }
                if (rect != null)
                {
                    visuals.glowSize = rect.sizeDelta;
                }
            }

            return visuals;
        }

        public static GameObject GetCardPrefab()
        {
            string fullPath = $"{PREFAB_PATH}/{PREFAB_NAME}";
            return AssetDatabase.LoadAssetAtPath<GameObject>(fullPath);
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
            rect.sizeDelta = new Vector2(180, 30);
            TextMeshProUGUI tmp = obj.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = fontSize;
            return obj;
        }

        /// <summary>
        /// Holds visual properties extracted from CardBase.
        /// </summary>
        private struct CardBaseVisuals
        {
            public Sprite frameSprite;
            public Color frameColor;
            public Image.Type frameType;
            public Vector2 frameSize;
            public Sprite bgSprite;
            public Color bgColor;
            public Image.Type bgType;
            public Vector2 bgSize;
            public Sprite artSprite;
            public Color artColor;
            public Image.Type artType;
            public Sprite costFrameSprite;
            public Color costFrameColor;
            public Image.Type costFrameType;
            public Vector2 costFrameSize;
            public Sprite costBgSprite;
            public Color costBgColor;
            public Image.Type costBgType;
            public Vector2 costBgSize;
            public Color costTextColor;
            public TMP_FontAsset costFont;
            public Sprite nameBgSprite;
            public Color nameBgColor;
            public Vector2 nameBgSize;
            public Color nameTextColor;
            public TMP_FontAsset nameFont;
            public Sprite descBgSprite;
            public Color descBgColor;
            public Vector2 descBgSize;
            public Color descTextColor;
            public TMP_FontAsset descFont;
            public Sprite glowSprite;
            public Color glowColor;
            public Image.Type glowType;
            public Vector2 glowSize;
            public Vector2 rootSize;
        }
    }
}
