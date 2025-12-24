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

            // Add RectTransform with same size as CardBase
            RectTransform rootRect = cardRoot.AddComponent<RectTransform>();
            rootRect.sizeDelta = new Vector2(200, 280);

            // Add Card-specific components
            Canvas canvas = cardRoot.AddComponent<Canvas>();
            canvas.overrideSorting = true;
            canvas.sortingOrder = 0;

            cardRoot.AddComponent<CanvasGroup>();
            cardRoot.AddComponent<GraphicRaycaster>();

            var cardScript = cardRoot.AddComponent<Cards.Card>();

            // === Create visual elements with copied properties from CardBase ===

            // Card Frame
            GameObject frameObj = CreateChildImage(cardRoot, "CardFrame", new Vector2(200, 280));
            Image frameImage = frameObj.GetComponent<Image>();
            frameImage.sprite = baseVisuals.frameSprite;
            frameImage.color = baseVisuals.frameColor;
            frameImage.type = Image.Type.Sliced;
            frameImage.raycastTarget = true;

            // Card Background
            GameObject bgObj = CreateChildImage(cardRoot, "CardBackground", new Vector2(192, 272));
            Image bgImage = bgObj.GetComponent<Image>();
            bgImage.sprite = baseVisuals.bgSprite;
            bgImage.color = baseVisuals.bgColor;
            bgImage.raycastTarget = false;

            // Card Art
            GameObject artObj = CreateChildImage(cardRoot, "CardArt", new Vector2(180, 140));
            RectTransform artRect = artObj.GetComponent<RectTransform>();
            artRect.anchoredPosition = new Vector2(0, 40);
            Image artImage = artObj.GetComponent<Image>();
            artImage.sprite = baseVisuals.artSprite;
            artImage.color = baseVisuals.artColor;
            artImage.raycastTarget = false;

            // Cost Background
            GameObject costBg = CreateChildImage(cardRoot, "CostBackground", new Vector2(40, 40));
            RectTransform costBgRect = costBg.GetComponent<RectTransform>();
            costBgRect.anchorMin = new Vector2(0, 1);
            costBgRect.anchorMax = new Vector2(0, 1);
            costBgRect.pivot = new Vector2(0, 1);
            costBgRect.anchoredPosition = new Vector2(5, -5);
            Image costBgImage = costBg.GetComponent<Image>();
            costBgImage.sprite = baseVisuals.costBgSprite;
            costBgImage.color = baseVisuals.costBgColor;
            costBgImage.raycastTarget = false;

            // Cost Text
            GameObject costTextObj = CreateChildText(costBg, "CostText", "1", 24);
            RectTransform costTextRect = costTextObj.GetComponent<RectTransform>();
            costTextRect.anchorMin = Vector2.zero;
            costTextRect.anchorMax = Vector2.one;
            costTextRect.sizeDelta = Vector2.zero;
            TextMeshProUGUI costText = costTextObj.GetComponent<TextMeshProUGUI>();
            costText.alignment = TextAlignmentOptions.Center;
            costText.color = baseVisuals.costTextColor;
            if (baseVisuals.costFont != null) costText.font = baseVisuals.costFont;

            // Name Text
            GameObject nameObj = CreateChildText(cardRoot, "NameText", "Card Name", 16);
            RectTransform nameRect = nameObj.GetComponent<RectTransform>();
            nameRect.anchoredPosition = new Vector2(0, -45);
            nameRect.sizeDelta = new Vector2(180, 30);
            TextMeshProUGUI nameText = nameObj.GetComponent<TextMeshProUGUI>();
            nameText.alignment = TextAlignmentOptions.Center;
            nameText.color = baseVisuals.nameTextColor;
            if (baseVisuals.nameFont != null) nameText.font = baseVisuals.nameFont;

            // Description Text
            GameObject descObj = CreateChildText(cardRoot, "DescriptionText", "Card effect description goes here.", 12);
            RectTransform descRect = descObj.GetComponent<RectTransform>();
            descRect.anchoredPosition = new Vector2(0, -100);
            descRect.sizeDelta = new Vector2(180, 60);
            TextMeshProUGUI descText = descObj.GetComponent<TextMeshProUGUI>();
            descText.alignment = TextAlignmentOptions.Center;
            descText.color = baseVisuals.descTextColor;
            descText.textWrappingMode = TextWrappingModes.Normal;
            if (baseVisuals.descFont != null) descText.font = baseVisuals.descFont;

            // Selection Glow
            GameObject glowObj = CreateChildImage(cardRoot, "SelectionGlow", new Vector2(220, 300));
            Image glowImage = glowObj.GetComponent<Image>();
            glowImage.sprite = baseVisuals.glowSprite;
            glowImage.color = baseVisuals.glowColor;
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

                // CostText
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
