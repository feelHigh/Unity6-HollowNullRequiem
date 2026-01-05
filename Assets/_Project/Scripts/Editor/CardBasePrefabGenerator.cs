// ============================================
// CardBasePrefabGenerator.cs
// Editor tool to generate CardBase root prefab (visual-only, no scripts)
// ============================================

using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;
using System.IO;

namespace HNR.Editor
{
    /// <summary>
    /// Generates the CardBase.prefab root prefab with visual elements only.
    /// This serves as the base for Card.prefab and CombatCard.prefab variants.
    ///
    /// Structure:
    /// - CardFrame: Outer border/frame
    /// - CardBackground: Interior shape with Mask component (clips CardArt)
    ///   └── CardArt: Full background art (masked by CardBackground)
    /// - CostFrame: Frame around cost area (top-left)
    ///   └── CostBackground: Inner cost background
    ///      └── CostText
    /// - NameTextBackground: Semi-transparent bar (top-center)
    ///   └── NameText
    /// - DescriptionText: Card effect text (bottom)
    /// - SelectionGlow: Selection highlight (behind everything)
    /// </summary>
    public static class CardBasePrefabGenerator
    {
        private const string PREFAB_PATH = "Assets/_Project/Prefabs/Cards";
        private const string PREFAB_NAME = "CardBase.prefab";

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

            // Create root GameObject (visual-only, no scripts)
            GameObject cardBase = new GameObject("CardBase");

            // Add RectTransform with standard card size (5:7 ratio)
            RectTransform rootRect = cardBase.AddComponent<RectTransform>();
            rootRect.sizeDelta = new Vector2(200, 280);

            // === Card Frame (Border - sprite slot for custom frames) ===
            GameObject frameObj = CreateChildImage(cardBase, "CardFrame", new Vector2(200, 280));
            Image frameImage = frameObj.GetComponent<Image>();
            frameImage.color = new Color(0.3f, 0.3f, 0.3f, 1f); // Default neutral color
            frameImage.type = Image.Type.Sliced; // Support 9-slice scaling
            frameImage.raycastTarget = true;

            // === Card Background (mask shape for CardArt) ===
            GameObject bgObj = CreateChildImage(cardBase, "CardBackground", new Vector2(192, 272));
            Image bgImage = bgObj.GetComponent<Image>();
            bgImage.color = new Color(1f, 1f, 1f, 1f); // White (shows through CardArt)
            bgImage.raycastTarget = false;
            // Add Mask component to clip child CardArt
            Mask bgMask = bgObj.AddComponent<Mask>();
            bgMask.showMaskGraphic = true; // Show the background image as fallback

            // === Card Art (full background, child of CardBackground for masking) ===
            // Aspect ratio matches card: 5:7 (200x280)
            GameObject artObj = CreateChildImage(bgObj, "CardArt", Vector2.zero);
            RectTransform artRect = artObj.GetComponent<RectTransform>();
            // Stretch to fill parent (CardBackground)
            artRect.anchorMin = Vector2.zero;
            artRect.anchorMax = Vector2.one;
            artRect.offsetMin = Vector2.zero;
            artRect.offsetMax = Vector2.zero;
            Image artImage = artObj.GetComponent<Image>();
            artImage.color = new Color(0.5f, 0.5f, 0.5f, 1f); // Placeholder gray
            artImage.raycastTarget = false;
            artImage.preserveAspect = false; // Fill entire area

            // === Cost Frame (outer frame for cost, top-left corner) ===
            GameObject costFrame = CreateChildImage(cardBase, "CostFrame", new Vector2(48, 48));
            RectTransform costFrameRect = costFrame.GetComponent<RectTransform>();
            costFrameRect.anchorMin = new Vector2(0, 1);
            costFrameRect.anchorMax = new Vector2(0, 1);
            costFrameRect.pivot = new Vector2(0, 1);
            costFrameRect.anchoredPosition = new Vector2(2, -2);
            Image costFrameImage = costFrame.GetComponent<Image>();
            costFrameImage.color = new Color(0.2f, 0.2f, 0.25f, 1f); // Dark frame
            costFrameImage.type = Image.Type.Sliced;
            costFrameImage.raycastTarget = false;

            // === Cost Background (inner, inside CostFrame) ===
            GameObject costBg = CreateChildImage(costFrame, "CostBackground", new Vector2(40, 40));
            RectTransform costBgRect = costBg.GetComponent<RectTransform>();
            costBgRect.anchorMin = new Vector2(0.5f, 0.5f);
            costBgRect.anchorMax = new Vector2(0.5f, 0.5f);
            costBgRect.pivot = new Vector2(0.5f, 0.5f);
            costBgRect.anchoredPosition = Vector2.zero;
            Image costBgImage = costBg.GetComponent<Image>();
            costBgImage.color = new Color(0.1f, 0.1f, 0.3f, 1f); // Dark blue
            costBgImage.raycastTarget = false;

            // === Cost Text ===
            GameObject costTextObj = CreateChildText(costBg, "CostText", "1", 24);
            RectTransform costTextRect = costTextObj.GetComponent<RectTransform>();
            costTextRect.anchorMin = Vector2.zero;
            costTextRect.anchorMax = Vector2.one;
            costTextRect.sizeDelta = Vector2.zero;
            costTextRect.offsetMin = Vector2.zero;
            costTextRect.offsetMax = Vector2.zero;
            TextMeshProUGUI costText = costTextObj.GetComponent<TextMeshProUGUI>();
            costText.alignment = TextAlignmentOptions.Center;
            costText.color = Color.white;

            // === Name Text Background (positioned next to CostFrame, top area) ===
            // CostFrame is 48x48 at (2,-2), so NameTextBackground starts at X=54 with same Y
            GameObject nameBgObj = CreateChildImage(cardBase, "NameTextBackground", new Vector2(140, 32));
            RectTransform nameBgRect = nameBgObj.GetComponent<RectTransform>();
            nameBgRect.anchorMin = new Vector2(0, 1);
            nameBgRect.anchorMax = new Vector2(0, 1);
            nameBgRect.pivot = new Vector2(0, 1);
            nameBgRect.anchoredPosition = new Vector2(54, -10); // Right of CostFrame, vertically centered with it
            Image nameBgImage = nameBgObj.GetComponent<Image>();
            nameBgImage.color = new Color(0f, 0f, 0f, 0.5f); // Black 50% transparent
            nameBgImage.raycastTarget = false;

            // === Name Text (child of NameTextBackground) ===
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

            // === Description Text Background (semi-transparent black, bottom area) ===
            GameObject descBgObj = CreateChildImage(cardBase, "DescriptionTextBackground", new Vector2(190, 65));
            RectTransform descBgRect = descBgObj.GetComponent<RectTransform>();
            descBgRect.anchorMin = new Vector2(0.5f, 0);
            descBgRect.anchorMax = new Vector2(0.5f, 0);
            descBgRect.pivot = new Vector2(0.5f, 0);
            descBgRect.anchoredPosition = new Vector2(0, 5);
            Image descBgImage = descBgObj.GetComponent<Image>();
            descBgImage.color = new Color(0f, 0f, 0f, 0.5f); // Black 50% transparent
            descBgImage.raycastTarget = false;

            // === Description Text (child of DescriptionTextBackground) ===
            GameObject descObj = CreateChildText(descBgObj, "DescriptionText", "Card effect description goes here.", 11);
            RectTransform descRect = descObj.GetComponent<RectTransform>();
            descRect.anchorMin = Vector2.zero;
            descRect.anchorMax = Vector2.one;
            descRect.sizeDelta = Vector2.zero;
            descRect.offsetMin = new Vector2(6, 4);
            descRect.offsetMax = new Vector2(-6, -4);
            TextMeshProUGUI descText = descObj.GetComponent<TextMeshProUGUI>();
            descText.alignment = TextAlignmentOptions.Center;
            descText.color = new Color(0.95f, 0.95f, 0.95f, 1f); // White
            descText.textWrappingMode = TextWrappingModes.Normal;

            // === Selection Glow (behind everything, initially inactive) ===
            GameObject glowObj = CreateChildImage(cardBase, "SelectionGlow", new Vector2(220, 300));
            Image glowImage = glowObj.GetComponent<Image>();
            glowImage.color = new Color(1f, 0.9f, 0.3f, 0.5f); // Yellow tint
            glowImage.raycastTarget = false;
            glowObj.transform.SetAsFirstSibling(); // Behind everything
            glowObj.SetActive(false);

            // Save as prefab
            string fullPath = $"{PREFAB_PATH}/{PREFAB_NAME}";
            PrefabUtility.SaveAsPrefabAsset(cardBase, fullPath);

            // Cleanup scene object
            Object.DestroyImmediate(cardBase);

            AssetDatabase.Refresh();
            Debug.Log($"[CardBasePrefabGenerator] CardBase prefab created at {fullPath}");

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
    }
}
