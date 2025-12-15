// ============================================
// ProductionSetupTool.cs
// Comprehensive editor tool for production scene setup
// ============================================

using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using TMPro;
using System.IO;
using System.Collections.Generic;
using HNR.Core;
using HNR.Combat;
using HNR.Cards;
using HNR.Characters;
using HNR.UI;
using HNR.UI.Screens;
using HNR.UI.Combat;
using HNR.Map;

namespace HNR.Editor
{
    /// <summary>
    /// Comprehensive production setup tool for wiring scenes,
    /// creating prefabs, and linking data assets.
    /// </summary>
    public static class ProductionSetupTool
    {
        private const string PREFABS_PATH = "Assets/_Project/Prefabs";
        private const string DATA_PATH = "Assets/_Project/Data";

        // ============================================
        // Menu Items
        // ============================================

        [MenuItem("HNR/Production Setup/Run Full Setup", priority = 50)]
        public static void RunFullSetup()
        {
            if (!EditorUtility.DisplayDialog("Run Full Production Setup",
                "This will:\n\n" +
                "1. Create missing UI prefabs\n" +
                "2. Link visual prefabs to data assets\n" +
                "3. Create EnemyInstance prefab\n\n" +
                "Continue?",
                "Yes, Run Setup", "Cancel"))
            {
                return;
            }

            CreateAllPrefabs();
            LinkAllVisualPrefabs();

            EditorUtility.DisplayDialog("Production Setup Complete",
                "All prefabs created and data assets linked.\n\n" +
                "Note: Scene wiring should be done via Unity Editor\n" +
                "or by running HNR > Production Scenes > Setup All Scenes.",
                "OK");
        }

        // ============================================
        // Prefab Creation
        // ============================================

        [MenuItem("HNR/Production Setup/1. Create All Prefabs", priority = 60)]
        public static void CreateAllPrefabs()
        {
            int created = 0;
            created += CreateCardPrefab();
            created += CreateMapNodePrefab();
            created += CreateEnemyInstancePrefab();
            created += CreateMissingEnemyVisualPrefabs();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"[ProductionSetupTool] Created {created} prefabs");
        }

        [MenuItem("HNR/Production Setup/2. Link Visual Prefabs to Data", priority = 61)]
        public static void LinkAllVisualPrefabs()
        {
            int linked = 0;
            linked += LinkRequiemVisualPrefabs();
            linked += LinkEnemyVisualPrefabs();

            AssetDatabase.SaveAssets();
            Debug.Log($"[ProductionSetupTool] Linked {linked} visual prefabs to data assets");
        }

        // ============================================
        // Card Prefab
        // ============================================

        private static int CreateCardPrefab()
        {
            string prefabPath = $"{PREFABS_PATH}/UI/Combat/Card.prefab";

            if (File.Exists(prefabPath))
            {
                Debug.Log("[ProductionSetupTool] Card prefab already exists");
                return 0;
            }

            EnsureDirectoryExists(prefabPath);

            // Create card root
            GameObject cardObj = new GameObject("Card");

            RectTransform rect = cardObj.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(140, 200);

            // Add Card component
            var card = cardObj.AddComponent<Card>();

            // Card background
            Image cardBg = cardObj.AddComponent<Image>();
            cardBg.color = new Color(0.15f, 0.12f, 0.2f);

            // Add CanvasGroup for animations
            cardObj.AddComponent<CanvasGroup>();

            // Card frame
            GameObject frame = new GameObject("Frame");
            frame.transform.SetParent(cardObj.transform, false);
            RectTransform frameRect = frame.AddComponent<RectTransform>();
            frameRect.anchorMin = Vector2.zero;
            frameRect.anchorMax = Vector2.one;
            frameRect.sizeDelta = new Vector2(-4, -4);
            Image frameImg = frame.AddComponent<Image>();
            frameImg.color = new Color(0.4f, 0.3f, 0.5f);
            frameImg.raycastTarget = false;

            // Card art area
            GameObject artArea = new GameObject("CardArt");
            artArea.transform.SetParent(cardObj.transform, false);
            RectTransform artRect = artArea.AddComponent<RectTransform>();
            artRect.anchorMin = new Vector2(0.1f, 0.45f);
            artRect.anchorMax = new Vector2(0.9f, 0.85f);
            artRect.sizeDelta = Vector2.zero;
            Image artImg = artArea.AddComponent<Image>();
            artImg.color = new Color(0.25f, 0.2f, 0.3f);
            artImg.raycastTarget = false;

            // Cost badge
            GameObject costBadge = new GameObject("CostBadge");
            costBadge.transform.SetParent(cardObj.transform, false);
            RectTransform costBadgeRect = costBadge.AddComponent<RectTransform>();
            costBadgeRect.anchorMin = new Vector2(0, 1);
            costBadgeRect.anchorMax = new Vector2(0, 1);
            costBadgeRect.pivot = new Vector2(0, 1);
            costBadgeRect.anchoredPosition = new Vector2(5, -5);
            costBadgeRect.sizeDelta = new Vector2(30, 30);
            Image costBadgeImg = costBadge.AddComponent<Image>();
            costBadgeImg.color = new Color(0.2f, 0.4f, 0.8f);
            costBadgeImg.raycastTarget = false;

            // Cost text
            GameObject costText = CreateTextObject(costBadge, "CostText", "1", 18);
            RectTransform costTextRect = costText.GetComponent<RectTransform>();
            costTextRect.anchorMin = Vector2.zero;
            costTextRect.anchorMax = Vector2.one;
            costTextRect.sizeDelta = Vector2.zero;

            // Card name
            GameObject nameText = CreateTextObject(cardObj, "NameText", "Card Name", 14);
            RectTransform nameRect = nameText.GetComponent<RectTransform>();
            nameRect.anchorMin = new Vector2(0.1f, 0.88f);
            nameRect.anchorMax = new Vector2(0.9f, 0.98f);
            nameRect.sizeDelta = Vector2.zero;
            nameText.GetComponent<TextMeshProUGUI>().fontStyle = FontStyles.Bold;

            // Card type indicator
            GameObject typeText = CreateTextObject(cardObj, "TypeText", "ATTACK", 10);
            RectTransform typeRect = typeText.GetComponent<RectTransform>();
            typeRect.anchorMin = new Vector2(0.1f, 0.38f);
            typeRect.anchorMax = new Vector2(0.9f, 0.45f);
            typeRect.sizeDelta = Vector2.zero;
            typeText.GetComponent<TextMeshProUGUI>().color = new Color(0.7f, 0.6f, 0.8f);

            // Description area
            GameObject descArea = new GameObject("DescriptionArea");
            descArea.transform.SetParent(cardObj.transform, false);
            RectTransform descAreaRect = descArea.AddComponent<RectTransform>();
            descAreaRect.anchorMin = new Vector2(0.08f, 0.05f);
            descAreaRect.anchorMax = new Vector2(0.92f, 0.38f);
            descAreaRect.sizeDelta = Vector2.zero;
            Image descBg = descArea.AddComponent<Image>();
            descBg.color = new Color(0.1f, 0.08f, 0.15f, 0.8f);
            descBg.raycastTarget = false;

            // Description text
            GameObject descText = CreateTextObject(descArea, "DescriptionText", "Card effect description goes here.", 11);
            RectTransform descTextRect = descText.GetComponent<RectTransform>();
            descTextRect.anchorMin = new Vector2(0.05f, 0.05f);
            descTextRect.anchorMax = new Vector2(0.95f, 0.95f);
            descTextRect.sizeDelta = Vector2.zero;
            var descTMP = descText.GetComponent<TextMeshProUGUI>();
            descTMP.alignment = TextAlignmentOptions.Top;
            descTMP.enableWordWrapping = true;

            // Aspect indicator (colored bar)
            GameObject aspectBar = new GameObject("AspectBar");
            aspectBar.transform.SetParent(cardObj.transform, false);
            RectTransform aspectRect = aspectBar.AddComponent<RectTransform>();
            aspectRect.anchorMin = new Vector2(0, 0);
            aspectRect.anchorMax = new Vector2(1, 0);
            aspectRect.pivot = new Vector2(0.5f, 0);
            aspectRect.anchoredPosition = Vector2.zero;
            aspectRect.sizeDelta = new Vector2(0, 4);
            Image aspectImg = aspectBar.AddComponent<Image>();
            aspectImg.color = new Color(0.8f, 0.3f, 0.2f); // Default flame color
            aspectImg.raycastTarget = false;

            // Selection glow (hidden by default)
            GameObject selectionGlow = new GameObject("SelectionGlow");
            selectionGlow.transform.SetParent(cardObj.transform, false);
            selectionGlow.transform.SetAsFirstSibling();
            RectTransform glowRect = selectionGlow.AddComponent<RectTransform>();
            glowRect.anchorMin = Vector2.zero;
            glowRect.anchorMax = Vector2.one;
            glowRect.sizeDelta = new Vector2(8, 8);
            Image glowImg = selectionGlow.AddComponent<Image>();
            glowImg.color = new Color(1f, 0.8f, 0.3f, 0.8f);
            glowImg.raycastTarget = false;
            selectionGlow.SetActive(false);

            // Wire Card component references (matches Card.cs field names)
            SerializedObject cardSO = new SerializedObject(card);
            cardSO.FindProperty("_cardArt").objectReferenceValue = artImg;
            cardSO.FindProperty("_cardFrame").objectReferenceValue = frameImg;
            cardSO.FindProperty("_nameText").objectReferenceValue = nameText.GetComponent<TextMeshProUGUI>();
            cardSO.FindProperty("_costText").objectReferenceValue = costText.GetComponent<TextMeshProUGUI>();
            cardSO.FindProperty("_descriptionText").objectReferenceValue = descTMP;
            cardSO.FindProperty("_selectionGlow").objectReferenceValue = selectionGlow;
            cardSO.ApplyModifiedPropertiesWithoutUndo();

            // Save prefab
            bool success;
            PrefabUtility.SaveAsPrefabAsset(cardObj, prefabPath, out success);
            Object.DestroyImmediate(cardObj);

            if (success)
            {
                Debug.Log($"[ProductionSetupTool] Created Card prefab at {prefabPath}");
                return 1;
            }

            Debug.LogError("[ProductionSetupTool] Failed to create Card prefab");
            return 0;
        }

        // ============================================
        // Map Node Prefab
        // ============================================

        private static int CreateMapNodePrefab()
        {
            string prefabPath = $"{PREFABS_PATH}/UI/Map/MapNodeUI.prefab";

            if (File.Exists(prefabPath))
            {
                Debug.Log("[ProductionSetupTool] MapNodeUI prefab already exists");
                return 0;
            }

            EnsureDirectoryExists(prefabPath);

            // Create node root
            GameObject nodeObj = new GameObject("MapNodeUI");

            RectTransform rect = nodeObj.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(60, 60);

            // Add MapNodeUI component
            var nodeUI = nodeObj.AddComponent<MapNodeUI>();

            // Node background (circular)
            Image nodeBg = nodeObj.AddComponent<Image>();
            nodeBg.color = new Color(0.2f, 0.15f, 0.25f);

            // Icon container
            GameObject iconObj = new GameObject("Icon");
            iconObj.transform.SetParent(nodeObj.transform, false);
            RectTransform iconRect = iconObj.AddComponent<RectTransform>();
            iconRect.anchorMin = new Vector2(0.15f, 0.15f);
            iconRect.anchorMax = new Vector2(0.85f, 0.85f);
            iconRect.sizeDelta = Vector2.zero;
            Image iconImg = iconObj.AddComponent<Image>();
            iconImg.color = Color.white;
            iconImg.raycastTarget = false;

            // Highlight ring (hidden by default)
            GameObject ring = new GameObject("HighlightRing");
            ring.transform.SetParent(nodeObj.transform, false);
            ring.transform.SetAsFirstSibling();
            RectTransform ringRect = ring.AddComponent<RectTransform>();
            ringRect.anchorMin = Vector2.zero;
            ringRect.anchorMax = Vector2.one;
            ringRect.sizeDelta = new Vector2(10, 10);
            Image ringImg = ring.AddComponent<Image>();
            ringImg.color = new Color(0.9f, 0.7f, 0.2f);
            ringImg.raycastTarget = false;
            ring.SetActive(false);

            // Current indicator (hidden by default)
            GameObject currentInd = new GameObject("CurrentIndicator");
            currentInd.transform.SetParent(nodeObj.transform, false);
            RectTransform currentRect = currentInd.AddComponent<RectTransform>();
            currentRect.anchorMin = new Vector2(0.5f, -0.2f);
            currentRect.anchorMax = new Vector2(0.5f, -0.2f);
            currentRect.sizeDelta = new Vector2(20, 20);
            Image currentImg = currentInd.AddComponent<Image>();
            currentImg.color = new Color(1f, 0.84f, 0f); // Gold
            currentImg.raycastTarget = false;
            currentInd.SetActive(false);

            // Wire MapNodeUI references (matches MapNodeUI.cs field names)
            SerializedObject nodeSO = new SerializedObject(nodeUI);
            nodeSO.FindProperty("_nodeIcon").objectReferenceValue = iconImg;
            nodeSO.FindProperty("_background").objectReferenceValue = nodeBg;
            nodeSO.FindProperty("_highlightRing").objectReferenceValue = ring;
            nodeSO.FindProperty("_currentIndicator").objectReferenceValue = currentInd;
            nodeSO.ApplyModifiedPropertiesWithoutUndo();

            // Save prefab
            bool success;
            PrefabUtility.SaveAsPrefabAsset(nodeObj, prefabPath, out success);
            Object.DestroyImmediate(nodeObj);

            if (success)
            {
                Debug.Log($"[ProductionSetupTool] Created MapNodeUI prefab at {prefabPath}");
                return 1;
            }

            Debug.LogError("[ProductionSetupTool] Failed to create MapNodeUI prefab");
            return 0;
        }

        // ============================================
        // Enemy Instance Prefab
        // ============================================

        private static int CreateEnemyInstancePrefab()
        {
            string prefabPath = $"{PREFABS_PATH}/Combat/EnemyInstance.prefab";

            if (File.Exists(prefabPath))
            {
                Debug.Log("[ProductionSetupTool] EnemyInstance prefab already exists");
                return 0;
            }

            EnsureDirectoryExists(prefabPath);

            // Create enemy root
            GameObject enemyObj = new GameObject("EnemyInstance");

            // Add EnemyInstance component
            var enemy = enemyObj.AddComponent<EnemyInstance>();

            // Visual placeholder (will be replaced by visual prefab at runtime)
            GameObject visualPlaceholder = new GameObject("VisualPlaceholder");
            visualPlaceholder.transform.SetParent(enemyObj.transform, false);
            var spriteRenderer = visualPlaceholder.AddComponent<SpriteRenderer>();
            spriteRenderer.color = new Color(0.5f, 0.3f, 0.4f);

            // Intent indicator parent
            GameObject intentParent = new GameObject("IntentIndicator");
            intentParent.transform.SetParent(enemyObj.transform, false);
            intentParent.transform.localPosition = new Vector3(0, 1.5f, 0);

            // HP bar parent
            GameObject hpBarParent = new GameObject("HPBar");
            hpBarParent.transform.SetParent(enemyObj.transform, false);
            hpBarParent.transform.localPosition = new Vector3(0, -0.8f, 0);

            // Save prefab
            bool success;
            PrefabUtility.SaveAsPrefabAsset(enemyObj, prefabPath, out success);
            Object.DestroyImmediate(enemyObj);

            if (success)
            {
                Debug.Log($"[ProductionSetupTool] Created EnemyInstance prefab at {prefabPath}");
                return 1;
            }

            Debug.LogError("[ProductionSetupTool] Failed to create EnemyInstance prefab");
            return 0;
        }

        // ============================================
        // Enemy Visual Prefabs
        // ============================================

        private static int CreateMissingEnemyVisualPrefabs()
        {
            string[] enemyNames = new string[]
            {
                // Zone 1
                "HollowShade", "CorruptedWisp", "VoidBeast", "ShadowCrawler",
                // Zone 2
                "FracturedKnight", "NullSpecter",
                // Zone 3
                "VoidExecutioner", "HollowAmalgam",
                // Elites
                "CorruptedWarden", "NullHerald",
                // Bosses
                "HollowKing",
                // Legacy
                "Shade_Lesser", "Flame_Wisp", "Thorn_Beast"
            };

            int created = 0;
            string enemyPrefabPath = $"{PREFABS_PATH}/Characters/Enemies";
            EnsureDirectoryExists($"{enemyPrefabPath}/placeholder.prefab");

            foreach (string enemyName in enemyNames)
            {
                string prefabPath = $"{enemyPrefabPath}/{enemyName}_Visual.prefab";

                if (File.Exists(prefabPath))
                {
                    continue;
                }

                // Create simple visual prefab
                GameObject visualObj = new GameObject($"{enemyName}_Visual");

                // Add SimpleCharacterVisual for basic sprite rendering
                var visual = visualObj.AddComponent<HNR.Characters.Visuals.SimpleCharacterVisual>();

                // Add sprite renderer
                var spriteRenderer = visualObj.AddComponent<SpriteRenderer>();
                spriteRenderer.color = GetEnemyColor(enemyName);

                // Save prefab
                bool success;
                PrefabUtility.SaveAsPrefabAsset(visualObj, prefabPath, out success);
                Object.DestroyImmediate(visualObj);

                if (success)
                {
                    Debug.Log($"[ProductionSetupTool] Created enemy visual: {prefabPath}");
                    created++;
                }
            }

            return created;
        }

        private static Color GetEnemyColor(string enemyName)
        {
            // Assign colors based on enemy type
            if (enemyName.Contains("Hollow") || enemyName.Contains("Shade"))
                return new Color(0.3f, 0.25f, 0.4f);
            if (enemyName.Contains("Void"))
                return new Color(0.2f, 0.1f, 0.3f);
            if (enemyName.Contains("Corrupt"))
                return new Color(0.5f, 0.2f, 0.3f);
            if (enemyName.Contains("Flame") || enemyName.Contains("Wisp"))
                return new Color(0.8f, 0.4f, 0.2f);
            if (enemyName.Contains("Null") || enemyName.Contains("Specter"))
                return new Color(0.6f, 0.5f, 0.7f);
            if (enemyName.Contains("Knight") || enemyName.Contains("Warden"))
                return new Color(0.4f, 0.35f, 0.45f);
            if (enemyName.Contains("Thorn") || enemyName.Contains("Beast"))
                return new Color(0.3f, 0.5f, 0.3f);

            return new Color(0.4f, 0.3f, 0.5f);
        }

        // ============================================
        // Visual Prefab Linking
        // ============================================

        private static int LinkRequiemVisualPrefabs()
        {
            string[] requiemNames = { "Kira", "Mordren", "Elara", "Thornwick" };
            int linked = 0;

            foreach (string name in requiemNames)
            {
                string dataPath = $"{DATA_PATH}/Characters/Requiems/{name}_Data.asset";
                string prefabPath = $"{PREFABS_PATH}/Characters/Requiems/{name}_Visual.prefab";

                var requiemData = AssetDatabase.LoadAssetAtPath<RequiemDataSO>(dataPath);
                var visualPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);

                if (requiemData == null)
                {
                    Debug.LogWarning($"[ProductionSetupTool] Requiem data not found: {dataPath}");
                    continue;
                }

                if (visualPrefab == null)
                {
                    Debug.LogWarning($"[ProductionSetupTool] Visual prefab not found: {prefabPath}");
                    continue;
                }

                // Check if already linked
                if (requiemData.VisualPrefab != null)
                {
                    continue;
                }

                // Link via SerializedObject
                SerializedObject so = new SerializedObject(requiemData);
                SerializedProperty visualProp = so.FindProperty("_visualPrefab");

                if (visualProp != null)
                {
                    visualProp.objectReferenceValue = visualPrefab;
                    so.ApplyModifiedProperties();
                    linked++;
                    Debug.Log($"[ProductionSetupTool] Linked {visualPrefab.name} to {requiemData.RequiemName}");
                }
            }

            return linked;
        }

        private static int LinkEnemyVisualPrefabs()
        {
            // Find all enemy data assets
            string[] enemyGuids = AssetDatabase.FindAssets("t:EnemyDataSO", new[] { DATA_PATH });
            int linked = 0;

            foreach (string guid in enemyGuids)
            {
                string dataPath = AssetDatabase.GUIDToAssetPath(guid);
                var enemyData = AssetDatabase.LoadAssetAtPath<EnemyDataSO>(dataPath);

                if (enemyData == null || enemyData.VisualPrefab != null)
                {
                    continue;
                }

                // Try to find matching visual prefab
                string enemyName = enemyData.EnemyName.Replace(" ", "");
                string[] possiblePaths = new[]
                {
                    $"{PREFABS_PATH}/Characters/Enemies/{enemyName}_Visual.prefab",
                    $"{PREFABS_PATH}/Characters/Enemies/{enemyData.name.Replace("_Data", "")}_Visual.prefab",
                    $"{PREFABS_PATH}/Characters/Enemies/{ExtractEnemyName(dataPath)}_Visual.prefab"
                };

                GameObject visualPrefab = null;
                foreach (string prefabPath in possiblePaths)
                {
                    visualPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
                    if (visualPrefab != null) break;
                }

                if (visualPrefab == null)
                {
                    Debug.LogWarning($"[ProductionSetupTool] No visual prefab found for enemy: {enemyData.EnemyName}");
                    continue;
                }

                // Link via SerializedObject
                SerializedObject so = new SerializedObject(enemyData);
                SerializedProperty visualProp = so.FindProperty("_visualPrefab");

                if (visualProp != null)
                {
                    visualProp.objectReferenceValue = visualPrefab;
                    so.ApplyModifiedProperties();
                    linked++;
                    Debug.Log($"[ProductionSetupTool] Linked {visualPrefab.name} to {enemyData.EnemyName}");
                }
            }

            return linked;
        }

        private static string ExtractEnemyName(string path)
        {
            // Extract enemy name from data path like "Zone1_HollowShade"
            string fileName = Path.GetFileNameWithoutExtension(path);
            if (fileName.Contains("_"))
            {
                string[] parts = fileName.Split('_');
                if (parts.Length >= 2)
                {
                    return parts[1];
                }
            }
            return fileName;
        }

        // ============================================
        // Utility Methods
        // ============================================

        private static GameObject CreateTextObject(GameObject parent, string name, string text, int fontSize)
        {
            GameObject obj = new GameObject(name);
            obj.transform.SetParent(parent.transform, false);
            obj.AddComponent<RectTransform>().sizeDelta = new Vector2(100, 30);

            TextMeshProUGUI tmp = obj.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = Color.white;
            tmp.raycastTarget = false;

            return obj;
        }

        private static void EnsureDirectoryExists(string filePath)
        {
            string directory = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
                AssetDatabase.Refresh();
            }
        }
    }
}
