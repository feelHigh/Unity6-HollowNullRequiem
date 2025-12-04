// ============================================
// CardPrefabGenerator.cs
// Editor tool to generate Card prefab
// ============================================

using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;
using System.IO;

namespace HNR.Editor
{
    public static class CardPrefabGenerator
    {
        [MenuItem("HNR/Generate Card Prefab")]
        public static void GenerateCardPrefab()
        {
            // Ensure directory exists
            string prefabPath = "Assets/_Project/Prefabs/Cards";
            if (!Directory.Exists(prefabPath))
            {
                Directory.CreateDirectory(prefabPath);
                AssetDatabase.Refresh();
            }

            // Create root GameObject
            GameObject cardRoot = new GameObject("Card");

            // Add RectTransform
            RectTransform rootRect = cardRoot.AddComponent<RectTransform>();
            rootRect.sizeDelta = new Vector2(200, 280);

            // Add Canvas for individual card sorting
            Canvas canvas = cardRoot.AddComponent<Canvas>();
            canvas.overrideSorting = true;
            canvas.sortingOrder = 0;

            // Add CanvasGroup for alpha control
            CanvasGroup canvasGroup = cardRoot.AddComponent<CanvasGroup>();

            // Add GraphicRaycaster for input
            cardRoot.AddComponent<GraphicRaycaster>();

            // Add Card script
            var cardScript = cardRoot.AddComponent<Cards.Card>();

            // === Card Frame (Background) ===
            GameObject frameObj = CreateChildImage(cardRoot, "CardFrame", new Vector2(200, 280));
            Image frameImage = frameObj.GetComponent<Image>();
            frameImage.color = new Color(0.3f, 0.3f, 0.3f, 1f);

            // === Card Art ===
            GameObject artObj = CreateChildImage(cardRoot, "CardArt", new Vector2(180, 140));
            RectTransform artRect = artObj.GetComponent<RectTransform>();
            artRect.anchoredPosition = new Vector2(0, 40);
            Image artImage = artObj.GetComponent<Image>();
            artImage.color = new Color(0.5f, 0.5f, 0.5f, 1f);

            // === Cost Circle ===
            GameObject costBg = CreateChildImage(cardRoot, "CostBackground", new Vector2(40, 40));
            RectTransform costBgRect = costBg.GetComponent<RectTransform>();
            costBgRect.anchorMin = new Vector2(0, 1);
            costBgRect.anchorMax = new Vector2(0, 1);
            costBgRect.pivot = new Vector2(0, 1);
            costBgRect.anchoredPosition = new Vector2(5, -5);
            Image costBgImage = costBg.GetComponent<Image>();
            costBgImage.color = new Color(0.1f, 0.1f, 0.3f, 1f);

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
            GameObject nameObj = CreateChildText(cardRoot, "NameText", "Card Name", 16);
            RectTransform nameRect = nameObj.GetComponent<RectTransform>();
            nameRect.anchoredPosition = new Vector2(0, -45);
            nameRect.sizeDelta = new Vector2(180, 30);
            TextMeshProUGUI nameText = nameObj.GetComponent<TextMeshProUGUI>();
            nameText.alignment = TextAlignmentOptions.Center;
            nameText.color = Color.white;

            // === Description Text ===
            GameObject descObj = CreateChildText(cardRoot, "DescriptionText", "Card effect description goes here.", 12);
            RectTransform descRect = descObj.GetComponent<RectTransform>();
            descRect.anchoredPosition = new Vector2(0, -100);
            descRect.sizeDelta = new Vector2(180, 60);
            TextMeshProUGUI descText = descObj.GetComponent<TextMeshProUGUI>();
            descText.alignment = TextAlignmentOptions.Center;
            descText.color = new Color(0.8f, 0.8f, 0.8f, 1f);
            descText.textWrappingMode = TextWrappingModes.Normal;

            // === Selection Glow ===
            GameObject glowObj = CreateChildImage(cardRoot, "SelectionGlow", new Vector2(220, 300));
            Image glowImage = glowObj.GetComponent<Image>();
            glowImage.color = new Color(1f, 0.9f, 0.3f, 0.5f);
            glowObj.transform.SetAsFirstSibling(); // Behind everything
            glowObj.SetActive(false);

            // === Wire up references via SerializedObject ===
            SerializedObject so = new SerializedObject(cardScript);
            so.FindProperty("_cardFrame").objectReferenceValue = frameImage;
            so.FindProperty("_cardArt").objectReferenceValue = artImage;
            so.FindProperty("_nameText").objectReferenceValue = nameText;
            so.FindProperty("_costText").objectReferenceValue = costText;
            so.FindProperty("_descriptionText").objectReferenceValue = descText;
            so.FindProperty("_selectionGlow").objectReferenceValue = glowObj;
            so.ApplyModifiedPropertiesWithoutUndo();

            // Save as prefab
            string fullPath = $"{prefabPath}/Card.prefab";
            PrefabUtility.SaveAsPrefabAsset(cardRoot, fullPath);

            // Cleanup scene object
            Object.DestroyImmediate(cardRoot);

            AssetDatabase.Refresh();
            Debug.Log($"[CardPrefabGenerator] Card prefab created at {fullPath}");

            // Select the created prefab
            Selection.activeObject = AssetDatabase.LoadAssetAtPath<GameObject>(fullPath);
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
