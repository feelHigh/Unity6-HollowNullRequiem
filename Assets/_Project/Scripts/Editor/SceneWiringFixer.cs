// ============================================
// SceneWiringFixer.cs
// Editor tool to fix missing scene wiring
// ============================================

#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using HNR.Map;
using HNR.Combat;
using HNR.Cards;
using HNR.UI.Screens;
using HNR.UI.Combat;

namespace HNR.Editor
{
    /// <summary>
    /// Editor tool to fix missing wiring in production scenes.
    /// </summary>
    public static class SceneWiringFixer
    {
        private const string ScenesPath = "Assets/_Project/Scenes/";
        private const string CardPrefabPath = "Assets/_Project/Prefabs/UI/Combat/Card.prefab";

        // ============================================
        // Public API
        // ============================================

        /// <summary>
        /// Fixes all scene wiring issues in NullRift and Combat scenes.
        /// </summary>
        public static void FixAllSceneWiring()
        {
            bool anyFixed = false;

            // Save current scene
            var currentScene = SceneManager.GetActiveScene().path;

            // Fix NullRift scene
            if (FixNullRiftScene())
                anyFixed = true;

            // Fix Combat scene
            if (FixCombatScene())
                anyFixed = true;

            // Return to original scene
            if (!string.IsNullOrEmpty(currentScene))
            {
                EditorSceneManager.OpenScene(currentScene);
            }

            if (anyFixed)
            {
                Debug.Log("[SceneWiringFixer] All scene wiring fixes applied successfully!");
                EditorUtility.DisplayDialog("Scene Wiring Fixed",
                    "All scene wiring issues have been fixed.\n\n" +
                    "- NullRift: MapPathRenderer wired\n" +
                    "- Combat: SharedVitalityBar and Card prefab wired",
                    "OK");
            }
            else
            {
                Debug.Log("[SceneWiringFixer] No fixes needed - all scenes already wired correctly.");
                EditorUtility.DisplayDialog("Scene Wiring Check",
                    "All scenes are already wired correctly!",
                    "OK");
            }
        }

        /// <summary>
        /// Fixes NullRift scene wiring (MapPathRenderer).
        /// </summary>
        public static bool FixNullRiftScene()
        {
            string scenePath = ScenesPath + "NullRift.unity";

            if (!System.IO.File.Exists(scenePath))
            {
                Debug.LogError($"[SceneWiringFixer] NullRift scene not found at {scenePath}");
                return false;
            }

            var scene = EditorSceneManager.OpenScene(scenePath);
            bool modified = false;

            // Find MapScreen
            var mapScreen = Object.FindAnyObjectByType<MapScreen>();
            if (mapScreen == null)
            {
                Debug.LogError("[SceneWiringFixer] MapScreen not found in NullRift scene");
                return false;
            }

            // Check if _pathRenderer is null using SerializedObject
            var serializedMapScreen = new SerializedObject(mapScreen);
            var pathRendererProp = serializedMapScreen.FindProperty("_pathRenderer");

            if (pathRendererProp != null && pathRendererProp.objectReferenceValue == null)
            {
                // Find or create MapPathRenderer
                var pathRenderer = Object.FindAnyObjectByType<MapPathRenderer>();

                if (pathRenderer == null)
                {
                    // Find PathContainer or MapContainer to add the component
                    var pathContainer = GameObject.Find("PathContainer");
                    if (pathContainer == null)
                    {
                        // Create PathContainer under MapContainer
                        var mapContainer = GameObject.Find("MapContainer");
                        if (mapContainer != null)
                        {
                            pathContainer = new GameObject("PathContainer");
                            pathContainer.transform.SetParent(mapContainer.transform, false);
                            var rectTransform = pathContainer.AddComponent<RectTransform>();
                            rectTransform.anchorMin = Vector2.zero;
                            rectTransform.anchorMax = Vector2.one;
                            rectTransform.offsetMin = Vector2.zero;
                            rectTransform.offsetMax = Vector2.zero;
                        }
                        else
                        {
                            pathContainer = new GameObject("PathContainer");
                        }
                    }

                    pathRenderer = pathContainer.AddComponent<MapPathRenderer>();
                    Debug.Log("[SceneWiringFixer] Created MapPathRenderer on PathContainer");
                }

                // Wire the reference
                pathRendererProp.objectReferenceValue = pathRenderer;
                serializedMapScreen.ApplyModifiedProperties();
                modified = true;
                Debug.Log("[SceneWiringFixer] Wired MapScreen._pathRenderer");
            }

            if (modified)
            {
                EditorSceneManager.MarkSceneDirty(scene);
                EditorSceneManager.SaveScene(scene);
                Debug.Log("[SceneWiringFixer] NullRift scene saved with fixes");
            }
            else
            {
                Debug.Log("[SceneWiringFixer] NullRift scene already correctly wired");
            }

            return modified;
        }

        /// <summary>
        /// Fixes Combat scene wiring (SharedVitalityBar, Card prefab).
        /// </summary>
        public static bool FixCombatScene()
        {
            string scenePath = ScenesPath + "Combat.unity";

            if (!System.IO.File.Exists(scenePath))
            {
                Debug.LogError($"[SceneWiringFixer] Combat scene not found at {scenePath}");
                return false;
            }

            var scene = EditorSceneManager.OpenScene(scenePath);
            bool modified = false;

            // Fix 1: SharedVitalityBar
            modified |= FixSharedVitalityBar();

            // Fix 2: CombatScreen._vitalityBar reference
            modified |= FixCombatScreenVitalityBarRef();

            // Fix 3: HandManager._cardPrefab
            modified |= FixHandManagerCardPrefab();

            if (modified)
            {
                EditorSceneManager.MarkSceneDirty(scene);
                EditorSceneManager.SaveScene(scene);
                Debug.Log("[SceneWiringFixer] Combat scene saved with fixes");
            }
            else
            {
                Debug.Log("[SceneWiringFixer] Combat scene already correctly wired");
            }

            return modified;
        }

        // ============================================
        // Fix Methods
        // ============================================

        private static bool FixSharedVitalityBar()
        {
            // Find SharedVitalityBar GameObject
            var vitalityBarGO = GameObject.Find("SharedVitalityBar");
            if (vitalityBarGO == null)
            {
                Debug.LogWarning("[SceneWiringFixer] SharedVitalityBar GameObject not found");
                return false;
            }

            // Check if SharedVitalityBar component exists
            var vitalityBarComponent = vitalityBarGO.GetComponent<SharedVitalityBar>();
            if (vitalityBarComponent == null)
            {
                vitalityBarComponent = vitalityBarGO.AddComponent<SharedVitalityBar>();
                Debug.Log("[SceneWiringFixer] Added SharedVitalityBar component");

                // Try to wire internal references
                WireSharedVitalityBarInternals(vitalityBarComponent);
                return true;
            }

            return false;
        }

        private static void WireSharedVitalityBarInternals(SharedVitalityBar vitalityBar)
        {
            var serialized = new SerializedObject(vitalityBar);

            // Find child components
            var hpBar = vitalityBar.transform.Find("HPBar");
            var hpText = vitalityBar.transform.Find("HPText");
            var fill = vitalityBar.transform.Find("Fill");

            if (hpBar != null)
            {
                var hpBarProp = serialized.FindProperty("_hpBarFill");
                if (hpBarProp != null)
                {
                    var image = hpBar.GetComponent<UnityEngine.UI.Image>();
                    if (image != null)
                        hpBarProp.objectReferenceValue = image;
                }
            }

            if (fill != null)
            {
                var fillProp = serialized.FindProperty("_hpBarFill");
                if (fillProp != null)
                {
                    var image = fill.GetComponent<UnityEngine.UI.Image>();
                    if (image != null)
                        fillProp.objectReferenceValue = image;
                }
            }

            if (hpText != null)
            {
                var hpTextProp = serialized.FindProperty("_hpText");
                if (hpTextProp != null)
                {
                    var tmp = hpText.GetComponent<TMPro.TextMeshProUGUI>();
                    if (tmp != null)
                        hpTextProp.objectReferenceValue = tmp;
                }
            }

            serialized.ApplyModifiedProperties();
        }

        private static bool FixCombatScreenVitalityBarRef()
        {
            var combatScreen = Object.FindAnyObjectByType<CombatScreen>();
            if (combatScreen == null)
            {
                Debug.LogWarning("[SceneWiringFixer] CombatScreen not found");
                return false;
            }

            var serialized = new SerializedObject(combatScreen);
            var vitalityBarProp = serialized.FindProperty("_vitalityBar");

            if (vitalityBarProp != null && vitalityBarProp.objectReferenceValue == null)
            {
                var vitalityBar = Object.FindAnyObjectByType<SharedVitalityBar>();
                if (vitalityBar != null)
                {
                    vitalityBarProp.objectReferenceValue = vitalityBar;
                    serialized.ApplyModifiedProperties();
                    Debug.Log("[SceneWiringFixer] Wired CombatScreen._vitalityBar");
                    return true;
                }
            }

            return false;
        }

        private static bool FixHandManagerCardPrefab()
        {
            var handManager = Object.FindAnyObjectByType<HandManager>();
            if (handManager == null)
            {
                Debug.LogWarning("[SceneWiringFixer] HandManager not found");
                return false;
            }

            var serialized = new SerializedObject(handManager);
            var cardPrefabProp = serialized.FindProperty("_cardPrefab");

            if (cardPrefabProp != null && cardPrefabProp.objectReferenceValue == null)
            {
                // Load the Card prefab
                var cardPrefab = AssetDatabase.LoadAssetAtPath<Card>(CardPrefabPath);
                if (cardPrefab != null)
                {
                    cardPrefabProp.objectReferenceValue = cardPrefab;
                    serialized.ApplyModifiedProperties();
                    Debug.Log("[SceneWiringFixer] Wired HandManager._cardPrefab");
                    return true;
                }
                else
                {
                    Debug.LogWarning($"[SceneWiringFixer] Card prefab not found at {CardPrefabPath}");
                }
            }

            return false;
        }

        // ============================================
        // Verification
        // ============================================

        /// <summary>
        /// Verifies wiring status of all production scenes.
        /// </summary>
        public static void VerifyAllSceneWiring()
        {
            var report = new System.Text.StringBuilder();
            report.AppendLine("=== Scene Wiring Verification ===\n");

            // Save current scene
            var currentScene = SceneManager.GetActiveScene().path;

            // Verify NullRift
            report.AppendLine("NullRift Scene:");
            report.AppendLine(VerifyNullRiftWiring());
            report.AppendLine();

            // Verify Combat
            report.AppendLine("Combat Scene:");
            report.AppendLine(VerifyCombatWiring());

            // Return to original scene
            if (!string.IsNullOrEmpty(currentScene))
            {
                EditorSceneManager.OpenScene(currentScene);
            }

            Debug.Log(report.ToString());
            EditorUtility.DisplayDialog("Wiring Verification", report.ToString(), "OK");
        }

        private static string VerifyNullRiftWiring()
        {
            var sb = new System.Text.StringBuilder();
            string scenePath = ScenesPath + "NullRift.unity";

            if (!System.IO.File.Exists(scenePath))
            {
                return "  Scene not found!";
            }

            EditorSceneManager.OpenScene(scenePath);

            var mapScreen = Object.FindAnyObjectByType<MapScreen>();
            if (mapScreen == null)
            {
                sb.AppendLine("  MapScreen: NOT FOUND");
                return sb.ToString();
            }

            sb.AppendLine("  MapScreen: OK");

            var serialized = new SerializedObject(mapScreen);

            var nodeContainer = serialized.FindProperty("_nodeContainer");
            sb.AppendLine($"  _nodeContainer: {(nodeContainer?.objectReferenceValue != null ? "OK" : "NULL")}");

            var nodePrefab = serialized.FindProperty("_nodePrefab");
            sb.AppendLine($"  _nodePrefab: {(nodePrefab?.objectReferenceValue != null ? "OK" : "NULL")}");

            var pathRenderer = serialized.FindProperty("_pathRenderer");
            sb.AppendLine($"  _pathRenderer: {(pathRenderer?.objectReferenceValue != null ? "OK" : "NULL")}");

            var mapContent = serialized.FindProperty("_mapContent");
            sb.AppendLine($"  _mapContent: {(mapContent?.objectReferenceValue != null ? "OK" : "NULL")}");

            return sb.ToString();
        }

        private static string VerifyCombatWiring()
        {
            var sb = new System.Text.StringBuilder();
            string scenePath = ScenesPath + "Combat.unity";

            if (!System.IO.File.Exists(scenePath))
            {
                return "  Scene not found!";
            }

            EditorSceneManager.OpenScene(scenePath);

            // CombatScreen
            var combatScreen = Object.FindAnyObjectByType<CombatScreen>();
            if (combatScreen == null)
            {
                sb.AppendLine("  CombatScreen: NOT FOUND");
            }
            else
            {
                sb.AppendLine("  CombatScreen: OK");
                var serialized = new SerializedObject(combatScreen);

                var vitalityBar = serialized.FindProperty("_vitalityBar");
                sb.AppendLine($"    _vitalityBar: {(vitalityBar?.objectReferenceValue != null ? "OK" : "NULL")}");

                var partySidebar = serialized.FindProperty("_partySidebar");
                sb.AppendLine($"    _partySidebar: {(partySidebar?.objectReferenceValue != null ? "OK" : "NULL")}");

                var cardFanLayout = serialized.FindProperty("_cardFanLayout");
                sb.AppendLine($"    _cardFanLayout: {(cardFanLayout?.objectReferenceValue != null ? "OK" : "NULL")}");

                var apCounter = serialized.FindProperty("_apCounter");
                sb.AppendLine($"    _apCounter: {(apCounter?.objectReferenceValue != null ? "OK" : "NULL")}");

                var executionButton = serialized.FindProperty("_executionButton");
                sb.AppendLine($"    _executionButton: {(executionButton?.objectReferenceValue != null ? "OK" : "NULL")}");
            }

            // HandManager
            var handManager = Object.FindAnyObjectByType<HandManager>();
            if (handManager == null)
            {
                sb.AppendLine("  HandManager: NOT FOUND");
            }
            else
            {
                sb.AppendLine("  HandManager: OK");
                var serialized = new SerializedObject(handManager);

                var handContainer = serialized.FindProperty("_handContainer");
                sb.AppendLine($"    _handContainer: {(handContainer?.objectReferenceValue != null ? "OK" : "NULL")}");

                var cardPrefab = serialized.FindProperty("_cardPrefab");
                sb.AppendLine($"    _cardPrefab: {(cardPrefab?.objectReferenceValue != null ? "OK" : "NULL")}");
            }

            // SharedVitalityBar
            var sharedVitalityBar = Object.FindAnyObjectByType<SharedVitalityBar>();
            sb.AppendLine($"  SharedVitalityBar: {(sharedVitalityBar != null ? "OK" : "NOT FOUND")}");

            return sb.ToString();
        }
    }
}
#endif
