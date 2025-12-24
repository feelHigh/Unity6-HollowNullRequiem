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
    /// </summary>
    public static class CardBasePrefabGenerator
    {
        private const string PREFAB_PATH = "Assets/_Project/Prefabs/Cards";
        private const string PREFAB_NAME = "CardBase.prefab";

        /// <summary>
        /// Generates the CardBase prefab with sprite-based frame support.
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

            // Add RectTransform with standard card size
            RectTransform rootRect = cardBase.AddComponent<RectTransform>();
            rootRect.sizeDelta = new Vector2(200, 280);

            // === Card Frame (Border - sprite slot for custom frames) ===
            GameObject frameObj = CreateChildImage(cardBase, "CardFrame", new Vector2(200, 280));
            Image frameImage = frameObj.GetComponent<Image>();
            frameImage.color = new Color(0.3f, 0.3f, 0.3f, 1f); // Default neutral color
            frameImage.type = Image.Type.Sliced; // Support 9-slice scaling
            frameImage.raycastTarget = true;

            // === Card Background (neutral interior behind content) ===
            GameObject bgObj = CreateChildImage(cardBase, "CardBackground", new Vector2(192, 272));
            Image bgImage = bgObj.GetComponent<Image>();
            bgImage.color = new Color(0.12f, 0.12f, 0.15f, 1f); // Dark neutral background
            bgImage.raycastTarget = false;

            // === Card Art (sprite slot for card artwork) ===
            GameObject artObj = CreateChildImage(cardBase, "CardArt", new Vector2(180, 140));
            RectTransform artRect = artObj.GetComponent<RectTransform>();
            artRect.anchoredPosition = new Vector2(0, 40);
            Image artImage = artObj.GetComponent<Image>();
            artImage.color = new Color(0.5f, 0.5f, 0.5f, 1f); // Placeholder gray
            artImage.raycastTarget = false;

            // === Cost Background (top-left corner) ===
            GameObject costBg = CreateChildImage(cardBase, "CostBackground", new Vector2(40, 40));
            RectTransform costBgRect = costBg.GetComponent<RectTransform>();
            costBgRect.anchorMin = new Vector2(0, 1);
            costBgRect.anchorMax = new Vector2(0, 1);
            costBgRect.pivot = new Vector2(0, 1);
            costBgRect.anchoredPosition = new Vector2(5, -5);
            Image costBgImage = costBg.GetComponent<Image>();
            costBgImage.color = new Color(0.1f, 0.1f, 0.3f, 1f); // Dark blue
            costBgImage.raycastTarget = false;

            // === Cost Text ===
            GameObject costTextObj = CreateChildText(costBg, "CostText", "1", 24);
            RectTransform costTextRect = costTextObj.GetComponent<RectTransform>();
            costTextRect.anchorMin = Vector2.zero;
            costTextRect.anchorMax = Vector2.one;
            costTextRect.sizeDelta = Vector2.zero;
            TextMeshProUGUI costText = costTextObj.GetComponent<TextMeshProUGUI>();
            costText.alignment = TextAlignmentOptions.Center;
            costText.color = Color.white;

            // === Name Text ===
            GameObject nameObj = CreateChildText(cardBase, "NameText", "Card Name", 16);
            RectTransform nameRect = nameObj.GetComponent<RectTransform>();
            nameRect.anchoredPosition = new Vector2(0, -45);
            nameRect.sizeDelta = new Vector2(180, 30);
            TextMeshProUGUI nameText = nameObj.GetComponent<TextMeshProUGUI>();
            nameText.alignment = TextAlignmentOptions.Center;
            nameText.color = Color.white;

            // === Description Text ===
            GameObject descObj = CreateChildText(cardBase, "DescriptionText", "Card effect description goes here.", 12);
            RectTransform descRect = descObj.GetComponent<RectTransform>();
            descRect.anchoredPosition = new Vector2(0, -100);
            descRect.sizeDelta = new Vector2(180, 60);
            TextMeshProUGUI descText = descObj.GetComponent<TextMeshProUGUI>();
            descText.alignment = TextAlignmentOptions.Center;
            descText.color = new Color(0.8f, 0.8f, 0.8f, 1f); // Light gray
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
