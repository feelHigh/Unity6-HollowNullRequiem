// ============================================
// CardPrefabWiringTool.cs
// Editor tool to wire Card prefab references to screens
// ============================================

using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using System.Linq;

namespace HNR.Editor
{
    using HNR.UI;
    using HNR.Map;
    using HNR.Cards;

    /// <summary>
    /// Wires Card prefab references to SanctuaryScreen and EchoEventScreen.
    /// Run from HNR > Utilities > Wire Card Prefab References
    /// </summary>
    public static class CardPrefabWiringTool
    {
        private const string CARD_PREFAB_PATH = "Assets/_Project/Prefabs/Cards/Card.prefab";
        private const string NULLRIFT_SCENE_PATH = "Assets/_Project/Scenes/NullRift.unity";

        [MenuItem("HNR/Utilities/Wire Card Prefab References", false, 210)]
        public static void WireCardPrefabReferences()
        {
            // Load Card prefab
            var cardPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(CARD_PREFAB_PATH);
            if (cardPrefab == null)
            {
                Debug.LogError($"[CardPrefabWiringTool] Card prefab not found at {CARD_PREFAB_PATH}");
                return;
            }

            var cardComponent = cardPrefab.GetComponent<Card>();
            if (cardComponent == null)
            {
                Debug.LogError("[CardPrefabWiringTool] Card prefab missing Card component");
                return;
            }

            // Save current scene
            var currentScenePath = SceneManager.GetActiveScene().path;
            bool sceneChanged = false;

            // Open NullRift scene
            var nullRiftScene = EditorSceneManager.OpenScene(NULLRIFT_SCENE_PATH, OpenSceneMode.Single);
            if (!nullRiftScene.IsValid())
            {
                Debug.LogError($"[CardPrefabWiringTool] Failed to open NullRift scene at {NULLRIFT_SCENE_PATH}");
                return;
            }

            int wiringCount = 0;

            // Find and wire SanctuaryScreen
            var sanctuaryScreen = FindObjectInScene<SanctuaryScreen>(nullRiftScene);
            if (sanctuaryScreen != null)
            {
                wiringCount += WireSanctuaryScreen(sanctuaryScreen, cardComponent);
            }
            else
            {
                Debug.LogWarning("[CardPrefabWiringTool] SanctuaryScreen not found in NullRift scene");
            }

            // Find and wire EchoEventScreen
            var echoEventScreen = FindObjectInScene<EchoEventScreen>(nullRiftScene);
            if (echoEventScreen != null)
            {
                wiringCount += WireEchoEventScreen(echoEventScreen, cardComponent);
            }
            else
            {
                Debug.LogWarning("[CardPrefabWiringTool] EchoEventScreen not found in NullRift scene");
            }

            // Save scene if changes were made
            if (wiringCount > 0)
            {
                EditorSceneManager.MarkSceneDirty(nullRiftScene);
                EditorSceneManager.SaveScene(nullRiftScene);
                Debug.Log($"[CardPrefabWiringTool] Wired {wiringCount} references and saved NullRift scene");
            }
            else
            {
                Debug.Log("[CardPrefabWiringTool] No references needed wiring");
            }

            // Return to original scene if different
            if (!string.IsNullOrEmpty(currentScenePath) && currentScenePath != NULLRIFT_SCENE_PATH)
            {
                EditorSceneManager.OpenScene(currentScenePath, OpenSceneMode.Single);
            }
        }

        private static T FindObjectInScene<T>(Scene scene) where T : MonoBehaviour
        {
            var rootObjects = scene.GetRootGameObjects();
            foreach (var root in rootObjects)
            {
                var component = root.GetComponentInChildren<T>(true);
                if (component != null)
                    return component;
            }
            return null;
        }

        private static int WireSanctuaryScreen(SanctuaryScreen screen, Card cardPrefab)
        {
            int count = 0;
            var so = new SerializedObject(screen);

            // Wire _cardPrefab
            var cardPrefabProp = so.FindProperty("_cardPrefab");
            if (cardPrefabProp != null && cardPrefabProp.objectReferenceValue == null)
            {
                cardPrefabProp.objectReferenceValue = cardPrefab;
                count++;
                Debug.Log("[CardPrefabWiringTool] Wired SanctuaryScreen._cardPrefab");
            }

            so.ApplyModifiedPropertiesWithoutUndo();
            return count;
        }

        private static int WireEchoEventScreen(EchoEventScreen screen, Card cardPrefab)
        {
            int count = 0;
            var so = new SerializedObject(screen);

            // Wire _cardPrefab
            var cardPrefabProp = so.FindProperty("_cardPrefab");
            if (cardPrefabProp != null && cardPrefabProp.objectReferenceValue == null)
            {
                cardPrefabProp.objectReferenceValue = cardPrefab;
                count++;
                Debug.Log("[CardPrefabWiringTool] Wired EchoEventScreen._cardPrefab");
            }

            // Check if _outcomeCardContainer exists and create if needed
            var containerProp = so.FindProperty("_outcomeCardContainer");
            if (containerProp != null && containerProp.objectReferenceValue == null)
            {
                // Find OutcomePanel and create container
                var outcomeContainer = CreateOutcomeCardContainer(screen);
                if (outcomeContainer != null)
                {
                    containerProp.objectReferenceValue = outcomeContainer;
                    count++;
                    Debug.Log("[CardPrefabWiringTool] Created and wired EchoEventScreen._outcomeCardContainer");
                }
            }

            so.ApplyModifiedPropertiesWithoutUndo();
            return count;
        }

        private static Transform CreateOutcomeCardContainer(EchoEventScreen screen)
        {
            // Find OutcomePanel in hierarchy
            var screenTransform = screen.transform;
            var outcomePanel = FindChildRecursive(screenTransform, "OutcomePanel");

            if (outcomePanel == null)
            {
                Debug.LogWarning("[CardPrefabWiringTool] OutcomePanel not found in EchoEventScreen hierarchy");
                return null;
            }

            // Check if OutcomeCardContainer already exists
            var existingContainer = outcomePanel.Find("OutcomeCardContainer");
            if (existingContainer != null)
            {
                return existingContainer;
            }

            // Create new container
            var containerObj = new GameObject("OutcomeCardContainer");
            containerObj.transform.SetParent(outcomePanel, false);

            // Add RectTransform
            var rectTransform = containerObj.AddComponent<RectTransform>();
            rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
            rectTransform.sizeDelta = new Vector2(220, 300);
            rectTransform.anchoredPosition = new Vector2(0, 50); // Above center

            // Add HorizontalLayoutGroup for multiple cards
            var layoutGroup = containerObj.AddComponent<UnityEngine.UI.HorizontalLayoutGroup>();
            layoutGroup.childAlignment = TextAnchor.MiddleCenter;
            layoutGroup.spacing = 10f;
            layoutGroup.childControlWidth = false;
            layoutGroup.childControlHeight = false;
            layoutGroup.childForceExpandWidth = false;
            layoutGroup.childForceExpandHeight = false;

            // Position after OutcomeText, before ContinueButton
            // Find OutcomeText to position after it
            var outcomeText = FindChildRecursive(outcomePanel, "OutcomeText");
            if (outcomeText != null)
            {
                int siblingIndex = outcomeText.GetSiblingIndex() + 1;
                containerObj.transform.SetSiblingIndex(siblingIndex);
            }

            Debug.Log("[CardPrefabWiringTool] Created OutcomeCardContainer in OutcomePanel");
            return containerObj.transform;
        }

        private static Transform FindChildRecursive(Transform parent, string name)
        {
            foreach (Transform child in parent)
            {
                if (child.name == name)
                    return child;

                var found = FindChildRecursive(child, name);
                if (found != null)
                    return found;
            }
            return null;
        }
    }
}
