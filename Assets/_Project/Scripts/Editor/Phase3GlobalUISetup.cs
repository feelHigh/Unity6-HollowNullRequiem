// ============================================
// Phase3GlobalUISetup.cs
// Completes Phase 3 Global UI Setup
// ============================================

#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using TMPro;
using HNR.UI;
using HNR.UI.Screens;
using HNR.UI.Toast;

namespace HNR.Editor
{
    /// <summary>
    /// Completes Phase 3 Global UI Setup.
    /// Creates UIManager prefab and adds LoadingScreen to scenes.
    /// </summary>
    public static class Phase3GlobalUISetup
    {
        private const string PrefabsPath = "Assets/_Project/Prefabs/UI/";
        private const string ScenesPath = "Assets/_Project/Scenes/";

        // ============================================
        // Public API
        // ============================================

        /// <summary>
        /// Completes all Phase 3 setup.
        /// </summary>
        public static void CompletePhase3()
        {
            bool prefabCreated = CreateUIManagerPrefab();
            bool loadingAdded = AddLoadingScreenToScenes();

            string message = "Phase 3 Global UI Setup:\n\n";
            message += $"UIManager Prefab: {(prefabCreated ? "Created" : "Already exists")}\n";
            message += $"LoadingScreen: {(loadingAdded ? "Added to scenes" : "Already exists")}";

            Debug.Log($"[Phase3GlobalUISetup] {message}");
            EditorUtility.DisplayDialog("Phase 3 Complete", message, "OK");
        }

        /// <summary>
        /// Creates UIManager prefab with proper configuration.
        /// </summary>
        public static bool CreateUIManagerPrefab()
        {
            string prefabPath = PrefabsPath + "UIManager.prefab";

            // Check if already exists
            var existing = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (existing != null)
            {
                Debug.Log("[Phase3GlobalUISetup] UIManager prefab already exists");
                return false;
            }

            // Ensure directory exists
            if (!AssetDatabase.IsValidFolder(PrefabsPath.TrimEnd('/')))
            {
                System.IO.Directory.CreateDirectory(PrefabsPath);
            }

            // Create UIManager GameObject
            var uiManagerGO = new GameObject("[UIManager]");

            // Add Canvas for UI hierarchy
            var canvas = uiManagerGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;

            var scaler = uiManagerGO.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;

            uiManagerGO.AddComponent<GraphicRaycaster>();

            // Create ScreenContainer
            var screenContainer = CreateContainer(uiManagerGO.transform, "ScreenContainer");

            // Create OverlayContainer
            var overlayContainer = CreateContainer(uiManagerGO.transform, "OverlayContainer");

            // Create FadeOverlay
            var fadeOverlay = CreateFadeOverlay(uiManagerGO.transform);

            // Add UIManager component
            var uiManager = uiManagerGO.AddComponent<UIManager>();

            // Wire references via SerializedObject
            var serialized = new SerializedObject(uiManager);

            var screenContainerProp = serialized.FindProperty("_screenContainer");
            if (screenContainerProp != null)
                screenContainerProp.objectReferenceValue = screenContainer.transform;

            var overlayContainerProp = serialized.FindProperty("_overlayContainer");
            if (overlayContainerProp != null)
                overlayContainerProp.objectReferenceValue = overlayContainer.transform;

            var fadeOverlayProp = serialized.FindProperty("_fadeOverlay");
            if (fadeOverlayProp != null)
                fadeOverlayProp.objectReferenceValue = fadeOverlay.GetComponent<CanvasGroup>();

            // Load and wire GlobalHeader prefab
            var globalHeaderPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabsPath + "GlobalHeader.prefab");
            var globalHeaderProp = serialized.FindProperty("_globalHeader");
            if (globalHeaderProp != null && globalHeaderPrefab != null)
            {
                var headerInstance = (GameObject)PrefabUtility.InstantiatePrefab(globalHeaderPrefab, uiManagerGO.transform);
                headerInstance.name = "GlobalHeader";
                globalHeaderProp.objectReferenceValue = headerInstance;
            }

            // Load and wire GlobalNavDock prefab
            var globalNavDockPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabsPath + "GlobalNavDock.prefab");
            var globalNavDockProp = serialized.FindProperty("_globalNavDock");
            if (globalNavDockProp != null && globalNavDockPrefab != null)
            {
                var navDockInstance = (GameObject)PrefabUtility.InstantiatePrefab(globalNavDockPrefab, uiManagerGO.transform);
                navDockInstance.name = "GlobalNavDock";
                globalNavDockProp.objectReferenceValue = navDockInstance;
            }

            serialized.ApplyModifiedProperties();

            // Save as prefab
            var prefab = PrefabUtility.SaveAsPrefabAsset(uiManagerGO, prefabPath);
            Object.DestroyImmediate(uiManagerGO);

            Debug.Log($"[Phase3GlobalUISetup] Created UIManager prefab at {prefabPath}");
            AssetDatabase.Refresh();

            return true;
        }

        /// <summary>
        /// Adds LoadingScreen to Boot and other transitional scenes.
        /// </summary>
        public static bool AddLoadingScreenToScenes()
        {
            bool anyModified = false;
            var currentScene = SceneManager.GetActiveScene().path;

            // Add to Boot scene (primary location for loading screen)
            anyModified |= AddLoadingScreenToScene("Boot");

            // Restore original scene
            if (!string.IsNullOrEmpty(currentScene))
            {
                EditorSceneManager.OpenScene(currentScene);
            }

            return anyModified;
        }

        /// <summary>
        /// Creates ToastManager prefab if needed.
        /// </summary>
        public static bool CreateToastManagerPrefab()
        {
            string prefabPath = PrefabsPath + "ToastManager.prefab";

            var existing = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (existing != null)
            {
                Debug.Log("[Phase3GlobalUISetup] ToastManager prefab already exists");
                return false;
            }

            var toastManagerGO = new GameObject("[ToastManager]");

            // Add Canvas
            var canvas = toastManagerGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 200; // Above UIManager

            var scaler = toastManagerGO.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;

            toastManagerGO.AddComponent<GraphicRaycaster>();

            // Create toast container
            var toastContainer = new GameObject("ToastContainer");
            toastContainer.transform.SetParent(toastManagerGO.transform, false);

            var containerRect = toastContainer.AddComponent<RectTransform>();
            containerRect.anchorMin = new Vector2(0.3f, 0.7f);
            containerRect.anchorMax = new Vector2(0.7f, 0.95f);
            containerRect.offsetMin = Vector2.zero;
            containerRect.offsetMax = Vector2.zero;

            var layout = toastContainer.AddComponent<VerticalLayoutGroup>();
            layout.spacing = 10;
            layout.childAlignment = TextAnchor.UpperCenter;
            layout.childControlHeight = false;
            layout.childControlWidth = true;
            layout.childForceExpandHeight = false;
            layout.childForceExpandWidth = true;

            // Add ToastManager component
            var toastManager = toastManagerGO.AddComponent<ToastManager>();

            // Wire container reference
            var serialized = new SerializedObject(toastManager);
            var containerProp = serialized.FindProperty("_toastContainer");
            if (containerProp != null)
                containerProp.objectReferenceValue = toastContainer.transform;

            // Wire toast prefab
            var toastPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabsPath + "Toast.prefab");
            var prefabProp = serialized.FindProperty("_toastPrefab");
            if (prefabProp != null && toastPrefab != null)
                prefabProp.objectReferenceValue = toastPrefab;

            serialized.ApplyModifiedProperties();

            // Save as prefab
            PrefabUtility.SaveAsPrefabAsset(toastManagerGO, prefabPath);
            Object.DestroyImmediate(toastManagerGO);

            Debug.Log($"[Phase3GlobalUISetup] Created ToastManager prefab at {prefabPath}");
            AssetDatabase.Refresh();

            return true;
        }

        // ============================================
        // Scene Helpers
        // ============================================

        private static bool AddLoadingScreenToScene(string sceneName)
        {
            string scenePath = ScenesPath + sceneName + ".unity";
            if (!System.IO.File.Exists(scenePath))
            {
                Debug.LogWarning($"[Phase3GlobalUISetup] Scene not found: {scenePath}");
                return false;
            }

            var scene = EditorSceneManager.OpenScene(scenePath);

            // Check if LoadingScreen already exists
            if (Object.FindAnyObjectByType<LoadingScreen>() != null)
            {
                Debug.Log($"[Phase3GlobalUISetup] LoadingScreen already exists in {sceneName}");
                return false;
            }

            // Create LoadingScreen canvas
            var loadingGO = new GameObject("LoadingScreen");

            var canvas = loadingGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 1000; // Above everything

            var scaler = loadingGO.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;

            loadingGO.AddComponent<GraphicRaycaster>();

            // Background
            var background = new GameObject("Background");
            background.transform.SetParent(loadingGO.transform, false);
            var bgRect = background.AddComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.offsetMin = Vector2.zero;
            bgRect.offsetMax = Vector2.zero;

            var bgImage = background.AddComponent<Image>();
            bgImage.color = new Color(0.02f, 0.01f, 0.05f, 1f);

            // Title
            var title = new GameObject("LoadingTitle");
            title.transform.SetParent(loadingGO.transform, false);
            var titleRect = title.AddComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0.3f, 0.5f);
            titleRect.anchorMax = new Vector2(0.7f, 0.6f);
            titleRect.offsetMin = Vector2.zero;
            titleRect.offsetMax = Vector2.zero;

            var titleTmp = title.AddComponent<TextMeshProUGUI>();
            titleTmp.text = "Loading...";
            titleTmp.fontSize = 36;
            titleTmp.alignment = TextAlignmentOptions.Center;
            titleTmp.color = new Color(0.8f, 0.7f, 0.9f);

            // Progress bar background
            var progressBG = new GameObject("ProgressBarBG");
            progressBG.transform.SetParent(loadingGO.transform, false);
            var progressBGRect = progressBG.AddComponent<RectTransform>();
            progressBGRect.anchorMin = new Vector2(0.25f, 0.4f);
            progressBGRect.anchorMax = new Vector2(0.75f, 0.45f);
            progressBGRect.offsetMin = Vector2.zero;
            progressBGRect.offsetMax = Vector2.zero;

            var progressBGImage = progressBG.AddComponent<Image>();
            progressBGImage.color = new Color(0.1f, 0.08f, 0.15f);

            // Progress bar fill
            var progressFill = new GameObject("ProgressBarFill");
            progressFill.transform.SetParent(progressBG.transform, false);
            var progressFillRect = progressFill.AddComponent<RectTransform>();
            progressFillRect.anchorMin = Vector2.zero;
            progressFillRect.anchorMax = new Vector2(0f, 1f); // Start at 0 width
            progressFillRect.offsetMin = Vector2.zero;
            progressFillRect.offsetMax = Vector2.zero;

            var progressFillImage = progressFill.AddComponent<Image>();
            progressFillImage.color = new Color(0.6f, 0.3f, 0.8f);

            // Tip text
            var tip = new GameObject("TipText");
            tip.transform.SetParent(loadingGO.transform, false);
            var tipRect = tip.AddComponent<RectTransform>();
            tipRect.anchorMin = new Vector2(0.2f, 0.25f);
            tipRect.anchorMax = new Vector2(0.8f, 0.35f);
            tipRect.offsetMin = Vector2.zero;
            tipRect.offsetMax = Vector2.zero;

            var tipTmp = tip.AddComponent<TextMeshProUGUI>();
            tipTmp.text = "Tip: Manage your corruption wisely...";
            tipTmp.fontSize = 20;
            tipTmp.alignment = TextAlignmentOptions.Center;
            tipTmp.color = new Color(0.5f, 0.5f, 0.6f);
            tipTmp.fontStyle = FontStyles.Italic;

            // Add LoadingScreen component
            var loadingScreen = loadingGO.AddComponent<LoadingScreen>();

            // Wire references
            var serialized = new SerializedObject(loadingScreen);

            var progressBarProp = serialized.FindProperty("_progressBar");
            if (progressBarProp != null)
                progressBarProp.objectReferenceValue = progressFillImage;

            var tipTextProp = serialized.FindProperty("_tipText");
            if (tipTextProp != null)
                tipTextProp.objectReferenceValue = tipTmp;

            var canvasGroupProp = serialized.FindProperty("_canvasGroup");
            if (canvasGroupProp != null)
            {
                var canvasGroup = loadingGO.AddComponent<CanvasGroup>();
                canvasGroupProp.objectReferenceValue = canvasGroup;
            }

            serialized.ApplyModifiedProperties();

            // Start inactive
            loadingGO.SetActive(false);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);

            Debug.Log($"[Phase3GlobalUISetup] Added LoadingScreen to {sceneName}");
            return true;
        }

        // ============================================
        // UI Creation Helpers
        // ============================================

        private static GameObject CreateContainer(Transform parent, string name)
        {
            var container = new GameObject(name);
            container.transform.SetParent(parent, false);

            var rect = container.AddComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            return container;
        }

        private static GameObject CreateFadeOverlay(Transform parent)
        {
            var overlay = new GameObject("FadeOverlay");
            overlay.transform.SetParent(parent, false);

            var rect = overlay.AddComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            var image = overlay.AddComponent<Image>();
            image.color = Color.black;
            image.raycastTarget = false;

            var canvasGroup = overlay.AddComponent<CanvasGroup>();
            canvasGroup.alpha = 0;
            canvasGroup.blocksRaycasts = false;
            canvasGroup.interactable = false;

            // Set sibling index to be on top
            overlay.transform.SetAsLastSibling();

            return overlay;
        }
    }
}
#endif
