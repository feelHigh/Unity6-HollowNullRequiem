// ============================================
// CombatCardPrefabGenerator.cs
// Editor tool to generate CombatCard prefab for CZN layout
// ============================================

using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;
using System.IO;
using HNR.UI.Combat;

namespace HNR.Editor
{
    public static class CombatCardPrefabGenerator
    {
        private const string PREFAB_PATH = "Assets/_Project/Prefabs/UI/Combat";

        public static void GenerateCombatCardPrefab()
        {
            // Ensure directory exists
            if (!Directory.Exists(PREFAB_PATH))
            {
                Directory.CreateDirectory(PREFAB_PATH);
                AssetDatabase.Refresh();
            }

            // Create root GameObject
            GameObject cardRoot = new GameObject("CombatCard");

            // Add RectTransform
            RectTransform rootRect = cardRoot.AddComponent<RectTransform>();
            rootRect.sizeDelta = new Vector2(160, 220);

            // Add CanvasGroup for alpha control and raycasting
            CanvasGroup canvasGroup = cardRoot.AddComponent<CanvasGroup>();

            // Add CombatCard script
            var combatCard = cardRoot.AddComponent<CombatCard>();

            // === Card Frame (Background) ===
            GameObject frameObj = CreateChildImage(cardRoot, "CardFrame", new Vector2(160, 220));
            Image frameImage = frameObj.GetComponent<Image>();
            frameImage.color = new Color(0.2f, 0.15f, 0.25f, 1f);

            // === Glow Outline (behind frame) ===
            GameObject glowObj = CreateChildImage(cardRoot, "GlowOutline", new Vector2(170, 230));
            glowObj.transform.SetAsFirstSibling();
            Image glowImage = glowObj.GetComponent<Image>();
            glowImage.color = new Color(0.8f, 0.6f, 0.2f, 0f); // Start invisible

            // === Rarity Glow ===
            GameObject rarityGlow = CreateChildImage(cardRoot, "RarityGlow", new Vector2(165, 225));
            rarityGlow.transform.SetSiblingIndex(1);
            Image rarityImage = rarityGlow.GetComponent<Image>();
            rarityImage.color = new Color(1f, 1f, 1f, 0.3f);

            // === Card Art ===
            GameObject artObj = CreateChildImage(cardRoot, "CardArt", new Vector2(140, 90));
            RectTransform artRect = artObj.GetComponent<RectTransform>();
            artRect.anchoredPosition = new Vector2(0, 35);
            Image artImage = artObj.GetComponent<Image>();
            artImage.color = new Color(0.4f, 0.35f, 0.45f, 1f);

            // === Cost Background ===
            GameObject costBg = CreateChildImage(cardRoot, "CostBackground", new Vector2(36, 36));
            RectTransform costBgRect = costBg.GetComponent<RectTransform>();
            costBgRect.anchorMin = new Vector2(0, 1);
            costBgRect.anchorMax = new Vector2(0, 1);
            costBgRect.pivot = new Vector2(0, 1);
            costBgRect.anchoredPosition = new Vector2(4, -4);
            Image costBgImage = costBg.GetComponent<Image>();
            costBgImage.color = new Color(0.1f, 0.05f, 0.2f, 1f);

            // === Cost Text ===
            GameObject costTextObj = CreateChildText(costBg, "CostText", "1", 20);
            RectTransform costTextRect = costTextObj.GetComponent<RectTransform>();
            costTextRect.anchorMin = Vector2.zero;
            costTextRect.anchorMax = Vector2.one;
            costTextRect.sizeDelta = Vector2.zero;
            TextMeshProUGUI costText = costTextObj.GetComponent<TextMeshProUGUI>();
            costText.alignment = TextAlignmentOptions.Center;
            costText.color = Color.white;
            costText.fontStyle = FontStyles.Bold;

            // === Type Icon ===
            GameObject typeIcon = CreateChildImage(cardRoot, "TypeIcon", new Vector2(24, 24));
            RectTransform typeRect = typeIcon.GetComponent<RectTransform>();
            typeRect.anchorMin = new Vector2(1, 1);
            typeRect.anchorMax = new Vector2(1, 1);
            typeRect.pivot = new Vector2(1, 1);
            typeRect.anchoredPosition = new Vector2(-4, -4);
            Image typeImage = typeIcon.GetComponent<Image>();
            typeImage.color = Color.white;

            // === Name Text ===
            GameObject nameObj = CreateChildText(cardRoot, "CardNameText", "Card Name", 14);
            RectTransform nameRect = nameObj.GetComponent<RectTransform>();
            nameRect.anchoredPosition = new Vector2(0, -25);
            nameRect.sizeDelta = new Vector2(140, 24);
            TextMeshProUGUI nameText = nameObj.GetComponent<TextMeshProUGUI>();
            nameText.alignment = TextAlignmentOptions.Center;
            nameText.color = Color.white;
            nameText.fontStyle = FontStyles.Bold;

            // === Description Text ===
            GameObject descObj = CreateChildText(cardRoot, "DescriptionText", "Deal 6 damage.", 11);
            RectTransform descRect = descObj.GetComponent<RectTransform>();
            descRect.anchoredPosition = new Vector2(0, -65);
            descRect.sizeDelta = new Vector2(140, 50);
            TextMeshProUGUI descText = descObj.GetComponent<TextMeshProUGUI>();
            descText.alignment = TextAlignmentOptions.Center;
            descText.color = new Color(0.85f, 0.85f, 0.85f, 1f);
            descText.textWrappingMode = TextWrappingModes.Normal;
            descText.fontSize = 11;

            // === Owner Portrait Container ===
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

            // === Selection Highlight ===
            GameObject selectionHighlight = CreateChildImage(cardRoot, "SelectionHighlight", new Vector2(168, 228));
            selectionHighlight.transform.SetAsLastSibling();
            Image selectionImage = selectionHighlight.GetComponent<Image>();
            selectionImage.color = new Color(1f, 1f, 0f, 0.4f);
            selectionHighlight.SetActive(false);

            // === Unplayable Overlay ===
            GameObject unplayableOverlay = CreateChildImage(cardRoot, "UnplayableOverlay", new Vector2(160, 220));
            unplayableOverlay.transform.SetAsLastSibling();
            Image unplayableImage = unplayableOverlay.GetComponent<Image>();
            unplayableImage.color = new Color(0f, 0f, 0f, 0.5f);
            CanvasGroup unplayableGroup = unplayableOverlay.AddComponent<CanvasGroup>();
            unplayableGroup.alpha = 0f;
            unplayableGroup.blocksRaycasts = false;

            // === Wire up references via SerializedObject ===
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
            so.FindProperty("_glowOutline").objectReferenceValue = glowImage;
            so.FindProperty("_selectionHighlight").objectReferenceValue = selectionHighlight;
            so.FindProperty("_unplayableOverlay").objectReferenceValue = unplayableGroup;
            so.ApplyModifiedPropertiesWithoutUndo();

            // Save as prefab
            string fullPath = $"{PREFAB_PATH}/CombatCard.prefab";
            PrefabUtility.SaveAsPrefabAsset(cardRoot, fullPath);

            // Cleanup scene object
            Object.DestroyImmediate(cardRoot);

            AssetDatabase.Refresh();
            Debug.Log($"[CombatCardPrefabGenerator] CombatCard prefab created at {fullPath}");

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
            rect.sizeDelta = new Vector2(140, 24);

            TextMeshProUGUI tmp = obj.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = fontSize;

            return obj;
        }
    }
}
