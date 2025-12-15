// ============================================
// ProductionSceneSetupGenerator.cs
// Editor tool to set up all production scenes
// ============================================

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering.Universal;
using UnityEditor;
using UnityEditor.SceneManagement;
using TMPro;
using System.IO;
using System.Collections.Generic;
using HNR.Core;
using HNR.Combat;
using HNR.Cards;
using HNR.UI;
using HNR.UI.Screens;
using HNR.Map;

namespace HNR.Editor
{
    /// <summary>
    /// Editor tool to generate and configure all production scenes.
    /// Creates Boot, MainMenu, Bastion, NullRift, and Combat scenes.
    /// </summary>
    public static class ProductionSceneSetupGenerator
    {
        private const string SCENES_PATH = "Assets/_Project/Scenes";
        private const string PREFABS_PATH = "Assets/_Project/Prefabs";

        // ============================================
        // Menu Items
        // ============================================

        [MenuItem("HNR/Production Scenes/Setup All Scenes", priority = 100)]
        public static void SetupAllScenes()
        {
            if (!EditorUtility.DisplayDialog("Setup Production Scenes",
                "This will regenerate all production scenes:\n\n" +
                "- Boot\n- MainMenu\n- Bastion\n- NullRift\n- Combat\n\n" +
                "Existing scenes will be overwritten. Continue?",
                "Yes, Setup All", "Cancel"))
            {
                return;
            }

            SetupBootScene();
            SetupMainMenuScene();
            SetupBastionScene();
            SetupNullRiftScene();
            SetupCombatScene();
            ConfigureBuildSettings();

            EditorUtility.DisplayDialog("Production Scenes Setup Complete",
                "All production scenes have been created and configured.\n\n" +
                "Build Settings have been updated with the correct scene order.",
                "OK");
        }

        [MenuItem("HNR/Production Scenes/1. Setup Boot Scene", priority = 110)]
        public static void SetupBootScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // === Camera ===
            CreateMainCamera("Boot");

            // === Bootstrap ===
            GameObject bootstrapObj = new GameObject("[Bootstrap]");
            bootstrapObj.AddComponent<GameBootstrap>();

            // === EventSystem ===
            CreateEventSystem();

            // Save scene
            string scenePath = $"{SCENES_PATH}/Boot.unity";
            EnsureDirectoryExists(scenePath);
            EditorSceneManager.SaveScene(scene, scenePath);

            Debug.Log($"[ProductionSceneSetupGenerator] Created Boot scene at {scenePath}");
        }

        [MenuItem("HNR/Production Scenes/2. Setup MainMenu Scene", priority = 111)]
        public static void SetupMainMenuScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // === Camera ===
            CreateMainCamera("MainMenu");

            // === EventSystem ===
            CreateEventSystem();

            // === UIManager ===
            GameObject uiManagerObj = CreateUIManager();

            // === Main Canvas ===
            GameObject canvasObj = CreateMainCanvas("MainMenuCanvas");

            // === Screen Container ===
            GameObject screenContainer = CreateUIContainer(canvasObj, "ScreenContainer");

            // === MainMenuScreen ===
            GameObject mainMenuObj = CreateMainMenuScreen(screenContainer);

            // === Overlay Container ===
            GameObject overlayContainer = CreateUIContainer(canvasObj, "OverlayContainer");

            // === Fade Overlay ===
            GameObject fadeOverlay = CreateFadeOverlay(canvasObj);

            // === Wire UIManager references ===
            var uiManager = uiManagerObj.GetComponent<UIManager>();
            SerializedObject uiManagerSO = new SerializedObject(uiManager);
            uiManagerSO.FindProperty("_screenContainer").objectReferenceValue = screenContainer.transform;
            uiManagerSO.FindProperty("_overlayContainer").objectReferenceValue = overlayContainer.transform;
            uiManagerSO.FindProperty("_fadeOverlay").objectReferenceValue = fadeOverlay.GetComponent<CanvasGroup>();
            uiManagerSO.ApplyModifiedPropertiesWithoutUndo();

            // === Background ===
            CreateBackground(canvasObj, new Color(0.05f, 0.02f, 0.1f));

            // Save scene
            string scenePath = $"{SCENES_PATH}/MainMenu.unity";
            EditorSceneManager.SaveScene(scene, scenePath);

            Debug.Log($"[ProductionSceneSetupGenerator] Created MainMenu scene at {scenePath}");
        }

        [MenuItem("HNR/Production Scenes/3. Setup Bastion Scene", priority = 112)]
        public static void SetupBastionScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // === Camera ===
            CreateMainCamera("Bastion");

            // === EventSystem ===
            CreateEventSystem();

            // === Main Canvas ===
            GameObject canvasObj = CreateMainCanvas("BastionCanvas");

            // === Screen Container ===
            GameObject screenContainer = CreateUIContainer(canvasObj, "ScreenContainer");

            // === Bastion Hub UI ===
            GameObject bastionHub = CreateBastionHubUI(screenContainer);

            // === Global Header Placeholder ===
            GameObject header = CreateGlobalHeaderPlaceholder(canvasObj);

            // === Global Nav Dock Placeholder ===
            GameObject navDock = CreateGlobalNavDockPlaceholder(canvasObj);

            // === Background ===
            CreateBackground(canvasObj, new Color(0.08f, 0.05f, 0.12f));

            // Save scene
            string scenePath = $"{SCENES_PATH}/Bastion.unity";
            EditorSceneManager.SaveScene(scene, scenePath);

            Debug.Log($"[ProductionSceneSetupGenerator] Created Bastion scene at {scenePath}");
        }

        [MenuItem("HNR/Production Scenes/4. Setup NullRift Scene", priority = 113)]
        public static void SetupNullRiftScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // === Camera ===
            CreateMainCamera("NullRift");

            // === EventSystem ===
            CreateEventSystem();

            // === Managers ===
            GameObject managersParent = new GameObject("--- MANAGERS ---");

            GameObject mapManagerObj = new GameObject("MapManager");
            mapManagerObj.transform.SetParent(managersParent.transform);
            mapManagerObj.AddComponent<MapManager>();

            // Note: MapGenerator is a plain C# class, not a MonoBehaviour
            // It's instantiated by MapManager when needed

            GameObject echoEventObj = new GameObject("EchoEventManager");
            echoEventObj.transform.SetParent(managersParent.transform);
            echoEventObj.AddComponent<EchoEventManager>();

            // === Main Canvas ===
            GameObject canvasObj = CreateMainCanvas("NullRiftCanvas");

            // === Screen Container ===
            GameObject screenContainer = CreateUIContainer(canvasObj, "ScreenContainer");

            // === MapScreen ===
            GameObject mapScreenObj = CreateMapScreen(screenContainer);

            // === EchoEventScreen ===
            GameObject echoScreenObj = CreateEchoEventScreen(screenContainer);

            // === ShopScreen ===
            GameObject shopScreenObj = CreateShopScreen(screenContainer);

            // === Overlay Container ===
            GameObject overlayContainer = CreateUIContainer(canvasObj, "OverlayContainer");

            // === Background ===
            CreateBackground(canvasObj, new Color(0.03f, 0.01f, 0.08f));

            // Save scene
            string scenePath = $"{SCENES_PATH}/NullRift.unity";
            EditorSceneManager.SaveScene(scene, scenePath);

            Debug.Log($"[ProductionSceneSetupGenerator] Created NullRift scene at {scenePath}");
        }

        [MenuItem("HNR/Production Scenes/5. Setup Combat Scene", priority = 114)]
        public static void SetupCombatScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // === Camera ===
            CreateMainCamera("Combat");

            // === EventSystem ===
            CreateEventSystem();

            // === Managers Parent ===
            GameObject managersParent = new GameObject("--- MANAGERS ---");

            // === TurnManager ===
            GameObject turnManagerObj = new GameObject("TurnManager");
            turnManagerObj.transform.SetParent(managersParent.transform);
            turnManagerObj.AddComponent<TurnManager>();

            // === DeckManager ===
            GameObject deckManagerObj = new GameObject("DeckManager");
            deckManagerObj.transform.SetParent(managersParent.transform);
            deckManagerObj.AddComponent<DeckManager>();

            // === HandManager ===
            GameObject handManagerObj = new GameObject("HandManager");
            handManagerObj.transform.SetParent(managersParent.transform);
            var handManager = handManagerObj.AddComponent<HandManager>();

            // === CardExecutor ===
            GameObject cardExecutorObj = new GameObject("CardExecutor");
            cardExecutorObj.transform.SetParent(managersParent.transform);
            cardExecutorObj.AddComponent<CardExecutor>();

            // === TargetingSystem ===
            GameObject targetingObj = new GameObject("TargetingSystem");
            targetingObj.transform.SetParent(managersParent.transform);
            targetingObj.AddComponent<TargetingSystem>();

            // === EncounterManager ===
            GameObject encounterObj = new GameObject("EncounterManager");
            encounterObj.transform.SetParent(managersParent.transform);
            encounterObj.AddComponent<EncounterManager>();

            // === StatusEffectManager ===
            GameObject statusObj = new GameObject("StatusEffectManager");
            statusObj.transform.SetParent(managersParent.transform);
            statusObj.AddComponent<StatusEffectManager>();

            // === SoulEssenceManager ===
            GameObject soulEssenceObj = new GameObject("SoulEssenceManager");
            soulEssenceObj.transform.SetParent(managersParent.transform);
            soulEssenceObj.AddComponent<SoulEssenceManager>();

            // === CombatManager ===
            GameObject combatManagerObj = new GameObject("CombatManager");
            combatManagerObj.transform.SetParent(managersParent.transform);
            combatManagerObj.AddComponent<CombatManager>();

            // === Main Canvas ===
            GameObject canvasObj = CreateMainCanvas("CombatCanvas");

            // === Screen Container ===
            GameObject screenContainer = CreateUIContainer(canvasObj, "ScreenContainer");

            // === CombatScreen (CZN Layout) ===
            GameObject combatScreenObj = CreateCombatScreenCZN(screenContainer);

            // === Hand Container ===
            GameObject handContainer = CreateHandContainer(canvasObj);

            // === Enemy Container ===
            GameObject enemyContainer = CreateEnemyContainer(canvasObj);

            // === DamageNumberSpawner ===
            GameObject damageSpawnerObj = new GameObject("DamageNumberSpawner");
            damageSpawnerObj.transform.SetParent(canvasObj.transform, false);
            damageSpawnerObj.AddComponent<DamageNumberSpawner>();

            // === Wire HandManager container ===
            SerializedObject handSO = new SerializedObject(handManager);
            handSO.FindProperty("_handContainer").objectReferenceValue = handContainer.transform;
            handSO.ApplyModifiedPropertiesWithoutUndo();

            // === Background ===
            CreateBackground(canvasObj, new Color(0.02f, 0.01f, 0.05f));

            // Save scene
            string scenePath = $"{SCENES_PATH}/Combat.unity";
            EditorSceneManager.SaveScene(scene, scenePath);

            Debug.Log($"[ProductionSceneSetupGenerator] Created Combat scene at {scenePath}");
        }

        [MenuItem("HNR/Production Scenes/Configure Build Settings", priority = 130)]
        public static void ConfigureBuildSettings()
        {
            List<EditorBuildSettingsScene> scenes = new List<EditorBuildSettingsScene>();

            // Add scenes in correct order
            string[] sceneOrder = new string[]
            {
                $"{SCENES_PATH}/Boot.unity",
                $"{SCENES_PATH}/MainMenu.unity",
                $"{SCENES_PATH}/Bastion.unity",
                $"{SCENES_PATH}/NullRift.unity",
                $"{SCENES_PATH}/Combat.unity"
            };

            foreach (string scenePath in sceneOrder)
            {
                if (File.Exists(scenePath))
                {
                    scenes.Add(new EditorBuildSettingsScene(scenePath, true));
                }
                else
                {
                    Debug.LogWarning($"[ProductionSceneSetupGenerator] Scene not found: {scenePath}");
                }
            }

            EditorBuildSettings.scenes = scenes.ToArray();
            Debug.Log($"[ProductionSceneSetupGenerator] Build Settings configured with {scenes.Count} scenes");
        }

        // ============================================
        // Helper Methods - Core Objects
        // ============================================

        private static GameObject CreateMainCamera(string sceneName)
        {
            GameObject cameraObj = new GameObject("Main Camera");
            cameraObj.tag = "MainCamera";

            Camera camera = cameraObj.AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.05f, 0.02f, 0.1f);
            camera.orthographic = true;
            camera.orthographicSize = 5f;
            camera.nearClipPlane = -10f;
            camera.farClipPlane = 100f;

            // Add URP camera data
            cameraObj.AddComponent<UniversalAdditionalCameraData>();

            // Add AudioListener
            cameraObj.AddComponent<AudioListener>();

            return cameraObj;
        }

        private static GameObject CreateEventSystem()
        {
            GameObject eventSystemObj = new GameObject("EventSystem");
            eventSystemObj.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eventSystemObj.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
            return eventSystemObj;
        }

        private static GameObject CreateUIManager()
        {
            GameObject uiManagerObj = new GameObject("[UIManager]");
            uiManagerObj.AddComponent<UIManager>();
            return uiManagerObj;
        }

        // ============================================
        // Helper Methods - Canvas & Containers
        // ============================================

        private static GameObject CreateMainCanvas(string name)
        {
            GameObject canvasObj = new GameObject(name);
            Canvas canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 0;

            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;

            canvasObj.AddComponent<GraphicRaycaster>();

            return canvasObj;
        }

        private static GameObject CreateUIContainer(GameObject parent, string name)
        {
            GameObject container = new GameObject(name);
            container.transform.SetParent(parent.transform, false);
            RectTransform rect = container.AddComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.sizeDelta = Vector2.zero;
            rect.anchoredPosition = Vector2.zero;
            return container;
        }

        private static GameObject CreateFadeOverlay(GameObject parent)
        {
            GameObject overlay = new GameObject("FadeOverlay");
            overlay.transform.SetParent(parent.transform, false);

            RectTransform rect = overlay.AddComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.sizeDelta = Vector2.zero;

            Image img = overlay.AddComponent<Image>();
            img.color = Color.black;
            img.raycastTarget = true;

            CanvasGroup group = overlay.AddComponent<CanvasGroup>();
            group.alpha = 0f;
            group.blocksRaycasts = false;

            overlay.SetActive(false);

            return overlay;
        }

        private static void CreateBackground(GameObject canvas, Color color)
        {
            GameObject bg = new GameObject("Background");
            bg.transform.SetParent(canvas.transform, false);
            bg.transform.SetAsFirstSibling();

            RectTransform rect = bg.AddComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.sizeDelta = Vector2.zero;

            Image img = bg.AddComponent<Image>();
            img.color = color;
            img.raycastTarget = false;
        }

        // ============================================
        // Helper Methods - Screen Creation
        // ============================================

        private static GameObject CreateMainMenuScreen(GameObject parent)
        {
            GameObject screenObj = new GameObject("MainMenuScreen");
            screenObj.transform.SetParent(parent.transform, false);

            RectTransform rect = screenObj.AddComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.sizeDelta = Vector2.zero;

            var screen = screenObj.AddComponent<MainMenuScreen>();

            // === Title ===
            GameObject titleObj = CreateText(screenObj, "Title", "HOLLOW NULL REQUIEM", 64);
            RectTransform titleRect = titleObj.GetComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0.5f, 0.8f);
            titleRect.anchorMax = new Vector2(0.5f, 0.8f);
            titleRect.anchoredPosition = Vector2.zero;
            titleRect.sizeDelta = new Vector2(800, 100);

            // === Button Container ===
            GameObject buttonContainer = new GameObject("ButtonContainer");
            buttonContainer.transform.SetParent(screenObj.transform, false);
            RectTransform buttonContainerRect = buttonContainer.AddComponent<RectTransform>();
            buttonContainerRect.anchorMin = new Vector2(0.5f, 0.5f);
            buttonContainerRect.anchorMax = new Vector2(0.5f, 0.5f);
            buttonContainerRect.sizeDelta = new Vector2(300, 400);

            VerticalLayoutGroup layout = buttonContainer.AddComponent<VerticalLayoutGroup>();
            layout.spacing = 20f;
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;
            layout.childControlWidth = true;
            layout.childControlHeight = false;

            // === Continue Button ===
            GameObject continueBtn = CreateMenuButton(buttonContainer, "ContinueButton", "CONTINUE");
            CanvasGroup continueGroup = continueBtn.AddComponent<CanvasGroup>();
            continueBtn.SetActive(false); // Hidden by default

            // === New Run Button ===
            GameObject newRunBtn = CreateMenuButton(buttonContainer, "NewRunButton", "NEW RUN");

            // === Settings Button ===
            GameObject settingsBtn = CreateMenuButton(buttonContainer, "SettingsButton", "SETTINGS");

            // === Quit Button ===
            GameObject quitBtn = CreateMenuButton(buttonContainer, "QuitButton", "QUIT");

            // === Version Text ===
            GameObject versionObj = CreateText(screenObj, "VersionText", "v0.1.0", 18);
            RectTransform versionRect = versionObj.GetComponent<RectTransform>();
            versionRect.anchorMin = new Vector2(1, 0);
            versionRect.anchorMax = new Vector2(1, 0);
            versionRect.anchoredPosition = new Vector2(-20, 20);
            versionRect.sizeDelta = new Vector2(100, 30);
            versionObj.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.BottomRight;

            // === Wire References ===
            SerializedObject screenSO = new SerializedObject(screen);
            screenSO.FindProperty("_continueButton").objectReferenceValue = continueBtn.GetComponent<Button>();
            screenSO.FindProperty("_continueButtonGroup").objectReferenceValue = continueGroup;
            screenSO.FindProperty("_newRunButton").objectReferenceValue = newRunBtn.GetComponent<Button>();
            screenSO.FindProperty("_settingsButton").objectReferenceValue = settingsBtn.GetComponent<Button>();
            screenSO.FindProperty("_quitButton").objectReferenceValue = quitBtn.GetComponent<Button>();
            screenSO.FindProperty("_versionText").objectReferenceValue = versionObj.GetComponent<TextMeshProUGUI>();
            screenSO.FindProperty("_showGlobalHeader").boolValue = false;
            screenSO.FindProperty("_showGlobalNav").boolValue = false;
            screenSO.ApplyModifiedPropertiesWithoutUndo();

            return screenObj;
        }

        private static GameObject CreateBastionHubUI(GameObject parent)
        {
            GameObject hubObj = new GameObject("BastionHub");
            hubObj.transform.SetParent(parent.transform, false);

            RectTransform rect = hubObj.AddComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.sizeDelta = Vector2.zero;

            // === Title ===
            GameObject titleObj = CreateText(hubObj, "Title", "THE BASTION", 48);
            RectTransform titleRect = titleObj.GetComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0.5f, 0.9f);
            titleRect.anchorMax = new Vector2(0.5f, 0.9f);
            titleRect.sizeDelta = new Vector2(400, 60);

            // === Hub Buttons Container ===
            GameObject buttonContainer = new GameObject("HubButtons");
            buttonContainer.transform.SetParent(hubObj.transform, false);
            RectTransform containerRect = buttonContainer.AddComponent<RectTransform>();
            containerRect.anchorMin = new Vector2(0.5f, 0.5f);
            containerRect.anchorMax = new Vector2(0.5f, 0.5f);
            containerRect.sizeDelta = new Vector2(600, 300);

            GridLayoutGroup grid = buttonContainer.AddComponent<GridLayoutGroup>();
            grid.cellSize = new Vector2(250, 80);
            grid.spacing = new Vector2(30, 20);
            grid.childAlignment = TextAnchor.MiddleCenter;
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = 2;

            // Hub buttons
            CreateMenuButton(buttonContainer, "EnterRiftButton", "ENTER NULL RIFT");
            CreateMenuButton(buttonContainer, "RequiemsButton", "REQUIEMS");
            CreateMenuButton(buttonContainer, "CollectionButton", "COLLECTION");
            CreateMenuButton(buttonContainer, "AchievementsButton", "ACHIEVEMENTS");

            return hubObj;
        }

        private static GameObject CreateMapScreen(GameObject parent)
        {
            GameObject screenObj = new GameObject("MapScreen");
            screenObj.transform.SetParent(parent.transform, false);

            RectTransform rect = screenObj.AddComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.sizeDelta = Vector2.zero;

            screenObj.AddComponent<MapScreen>();

            // === Map Container (scrollable area) ===
            GameObject mapContainer = new GameObject("MapContainer");
            mapContainer.transform.SetParent(screenObj.transform, false);
            RectTransform mapRect = mapContainer.AddComponent<RectTransform>();
            mapRect.anchorMin = new Vector2(0.1f, 0.1f);
            mapRect.anchorMax = new Vector2(0.9f, 0.9f);
            mapRect.sizeDelta = Vector2.zero;

            Image mapBg = mapContainer.AddComponent<Image>();
            mapBg.color = new Color(0.1f, 0.05f, 0.15f, 0.5f);

            // === Zone Title ===
            GameObject zoneTitleObj = CreateText(screenObj, "ZoneTitle", "ZONE 1: THE FRACTURED DEPTHS", 32);
            RectTransform zoneRect = zoneTitleObj.GetComponent<RectTransform>();
            zoneRect.anchorMin = new Vector2(0.5f, 0.95f);
            zoneRect.anchorMax = new Vector2(0.5f, 0.95f);
            zoneRect.sizeDelta = new Vector2(600, 50);

            // === Node Container ===
            GameObject nodeContainer = new GameObject("NodeContainer");
            nodeContainer.transform.SetParent(mapContainer.transform, false);
            RectTransform nodeRect = nodeContainer.AddComponent<RectTransform>();
            nodeRect.anchorMin = Vector2.zero;
            nodeRect.anchorMax = Vector2.one;
            nodeRect.sizeDelta = Vector2.zero;

            // === Path Container ===
            GameObject pathContainer = new GameObject("PathContainer");
            pathContainer.transform.SetParent(mapContainer.transform, false);
            pathContainer.transform.SetAsFirstSibling();
            RectTransform pathRect = pathContainer.AddComponent<RectTransform>();
            pathRect.anchorMin = Vector2.zero;
            pathRect.anchorMax = Vector2.one;
            pathRect.sizeDelta = Vector2.zero;

            return screenObj;
        }

        private static GameObject CreateEchoEventScreen(GameObject parent)
        {
            GameObject screenObj = new GameObject("EchoEventScreen");
            screenObj.transform.SetParent(parent.transform, false);

            RectTransform rect = screenObj.AddComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.sizeDelta = Vector2.zero;

            screenObj.AddComponent<EchoEventScreen>();
            screenObj.SetActive(false);

            // === Event Panel ===
            GameObject panel = new GameObject("EventPanel");
            panel.transform.SetParent(screenObj.transform, false);
            RectTransform panelRect = panel.AddComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.15f, 0.1f);
            panelRect.anchorMax = new Vector2(0.85f, 0.9f);
            panelRect.sizeDelta = Vector2.zero;

            Image panelBg = panel.AddComponent<Image>();
            panelBg.color = new Color(0.1f, 0.08f, 0.15f, 0.95f);

            // === Event Title ===
            GameObject titleObj = CreateText(panel, "EventTitle", "ECHO OF THE VOID", 36);
            RectTransform titleRect = titleObj.GetComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0.5f, 0.9f);
            titleRect.anchorMax = new Vector2(0.5f, 0.9f);
            titleRect.sizeDelta = new Vector2(500, 50);

            // === Event Description ===
            GameObject descObj = CreateText(panel, "EventDescription", "Event description text goes here...", 20);
            RectTransform descRect = descObj.GetComponent<RectTransform>();
            descRect.anchorMin = new Vector2(0.1f, 0.4f);
            descRect.anchorMax = new Vector2(0.9f, 0.8f);
            descRect.sizeDelta = Vector2.zero;
            descObj.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.TopLeft;

            // === Choice Container ===
            GameObject choiceContainer = new GameObject("ChoiceContainer");
            choiceContainer.transform.SetParent(panel.transform, false);
            RectTransform choiceRect = choiceContainer.AddComponent<RectTransform>();
            choiceRect.anchorMin = new Vector2(0.1f, 0.05f);
            choiceRect.anchorMax = new Vector2(0.9f, 0.35f);
            choiceRect.sizeDelta = Vector2.zero;

            VerticalLayoutGroup choiceLayout = choiceContainer.AddComponent<VerticalLayoutGroup>();
            choiceLayout.spacing = 10f;
            choiceLayout.childAlignment = TextAnchor.UpperCenter;
            choiceLayout.childForceExpandWidth = true;
            choiceLayout.childForceExpandHeight = false;

            return screenObj;
        }

        private static GameObject CreateShopScreen(GameObject parent)
        {
            GameObject screenObj = new GameObject("ShopScreen");
            screenObj.transform.SetParent(parent.transform, false);

            RectTransform rect = screenObj.AddComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.sizeDelta = Vector2.zero;

            screenObj.AddComponent<ShopScreen>();
            screenObj.SetActive(false);

            // === Shop Title ===
            GameObject titleObj = CreateText(screenObj, "ShopTitle", "VOID MARKET", 42);
            RectTransform titleRect = titleObj.GetComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0.5f, 0.92f);
            titleRect.anchorMax = new Vector2(0.5f, 0.92f);
            titleRect.sizeDelta = new Vector2(400, 60);

            // === Currency Display ===
            GameObject currencyObj = CreateText(screenObj, "CurrencyDisplay", "Void Shards: 0", 24);
            RectTransform currencyRect = currencyObj.GetComponent<RectTransform>();
            currencyRect.anchorMin = new Vector2(0.9f, 0.92f);
            currencyRect.anchorMax = new Vector2(0.9f, 0.92f);
            currencyRect.sizeDelta = new Vector2(200, 40);

            // === Items Container ===
            GameObject itemsContainer = new GameObject("ItemsContainer");
            itemsContainer.transform.SetParent(screenObj.transform, false);
            RectTransform itemsRect = itemsContainer.AddComponent<RectTransform>();
            itemsRect.anchorMin = new Vector2(0.05f, 0.15f);
            itemsRect.anchorMax = new Vector2(0.95f, 0.85f);
            itemsRect.sizeDelta = Vector2.zero;

            GridLayoutGroup grid = itemsContainer.AddComponent<GridLayoutGroup>();
            grid.cellSize = new Vector2(200, 280);
            grid.spacing = new Vector2(20, 20);
            grid.childAlignment = TextAnchor.UpperCenter;
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = 4;

            // === Leave Button ===
            GameObject leaveBtn = CreateMenuButton(screenObj, "LeaveButton", "LEAVE SHOP");
            RectTransform leaveRect = leaveBtn.GetComponent<RectTransform>();
            leaveRect.anchorMin = new Vector2(0.5f, 0.05f);
            leaveRect.anchorMax = new Vector2(0.5f, 0.05f);
            leaveRect.sizeDelta = new Vector2(200, 50);

            return screenObj;
        }

        private static GameObject CreateCombatScreenCZN(GameObject parent)
        {
            GameObject screenObj = new GameObject("CombatScreenCZN");
            screenObj.transform.SetParent(parent.transform, false);

            RectTransform rect = screenObj.AddComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.sizeDelta = Vector2.zero;

            // Add CombatScreenCZN
            var combatScreenCZN = screenObj.AddComponent<CombatScreenCZN>();

            // === Top Section - Shared Vitality Bar ===
            GameObject vitalityBar = CreateSharedVitalityBar(screenObj);

            // === Left Section - Party Status Sidebar ===
            GameObject partySidebar = CreatePartySidebar(screenObj);

            // === Right Section - AP Counter ===
            GameObject apCounter = CreateAPCounter(screenObj);

            // === Bottom Section - Execution Button ===
            GameObject executionBtn = CreateExecutionButton(screenObj);

            // === Enemy Area ===
            GameObject enemyArea = new GameObject("EnemyArea");
            enemyArea.transform.SetParent(screenObj.transform, false);
            RectTransform enemyRect = enemyArea.AddComponent<RectTransform>();
            enemyRect.anchorMin = new Vector2(0.2f, 0.5f);
            enemyRect.anchorMax = new Vector2(0.85f, 0.85f);
            enemyRect.sizeDelta = Vector2.zero;

            // === Turn Indicator ===
            GameObject turnObj = CreateText(screenObj, "TurnText", "Turn 1", 28);
            RectTransform turnRect = turnObj.GetComponent<RectTransform>();
            turnRect.anchorMin = new Vector2(0.5f, 0.95f);
            turnRect.anchorMax = new Vector2(0.5f, 0.95f);
            turnRect.sizeDelta = new Vector2(150, 40);

            // === Phase Indicator ===
            GameObject phaseObj = CreateText(screenObj, "PhaseText", "PLAYER PHASE", 20);
            RectTransform phaseRect = phaseObj.GetComponent<RectTransform>();
            phaseRect.anchorMin = new Vector2(0.5f, 0.9f);
            phaseRect.anchorMax = new Vector2(0.5f, 0.9f);
            phaseRect.sizeDelta = new Vector2(200, 30);

            // Wire references
            SerializedObject screenSO = new SerializedObject(combatScreenCZN);
            screenSO.FindProperty("_showGlobalHeader").boolValue = false;
            screenSO.FindProperty("_showGlobalNav").boolValue = false;
            screenSO.ApplyModifiedPropertiesWithoutUndo();

            return screenObj;
        }

        // ============================================
        // Helper Methods - Combat UI Components
        // ============================================

        private static GameObject CreateSharedVitalityBar(GameObject parent)
        {
            GameObject barObj = new GameObject("SharedVitalityBar");
            barObj.transform.SetParent(parent.transform, false);

            RectTransform rect = barObj.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.15f, 0.88f);
            rect.anchorMax = new Vector2(0.85f, 0.95f);
            rect.sizeDelta = Vector2.zero;

            Image bg = barObj.AddComponent<Image>();
            bg.color = new Color(0.15f, 0.1f, 0.2f, 0.9f);

            // HP Bar
            GameObject hpBar = new GameObject("HPBar");
            hpBar.transform.SetParent(barObj.transform, false);
            RectTransform hpRect = hpBar.AddComponent<RectTransform>();
            hpRect.anchorMin = new Vector2(0.02f, 0.2f);
            hpRect.anchorMax = new Vector2(0.98f, 0.8f);
            hpRect.sizeDelta = Vector2.zero;

            Image hpBg = hpBar.AddComponent<Image>();
            hpBg.color = new Color(0.3f, 0.1f, 0.1f);

            GameObject hpFill = new GameObject("Fill");
            hpFill.transform.SetParent(hpBar.transform, false);
            RectTransform fillRect = hpFill.AddComponent<RectTransform>();
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = Vector2.one;
            fillRect.sizeDelta = Vector2.zero;

            Image fillImg = hpFill.AddComponent<Image>();
            fillImg.color = new Color(0.8f, 0.2f, 0.2f);

            // HP Text
            GameObject hpText = CreateText(barObj, "HPText", "150 / 150", 22);
            RectTransform textRect = hpText.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.sizeDelta = Vector2.zero;

            return barObj;
        }

        private static GameObject CreatePartySidebar(GameObject parent)
        {
            GameObject sidebar = new GameObject("PartySidebar");
            sidebar.transform.SetParent(parent.transform, false);

            RectTransform rect = sidebar.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0, 0.2f);
            rect.anchorMax = new Vector2(0.12f, 0.85f);
            rect.sizeDelta = Vector2.zero;

            Image bg = sidebar.AddComponent<Image>();
            bg.color = new Color(0.1f, 0.08f, 0.15f, 0.8f);

            // Party slots container
            VerticalLayoutGroup layout = sidebar.AddComponent<VerticalLayoutGroup>();
            layout.spacing = 10f;
            layout.padding = new RectOffset(5, 5, 10, 10);
            layout.childAlignment = TextAnchor.UpperCenter;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;

            // Create 3 party member slots
            for (int i = 0; i < 3; i++)
            {
                GameObject slot = new GameObject($"PartySlot_{i}");
                slot.transform.SetParent(sidebar.transform, false);

                RectTransform slotRect = slot.AddComponent<RectTransform>();
                slotRect.sizeDelta = new Vector2(100, 120);

                Image slotBg = slot.AddComponent<Image>();
                slotBg.color = new Color(0.2f, 0.15f, 0.25f, 0.6f);

                // Portrait placeholder
                GameObject portrait = new GameObject("Portrait");
                portrait.transform.SetParent(slot.transform, false);
                RectTransform portraitRect = portrait.AddComponent<RectTransform>();
                portraitRect.anchorMin = new Vector2(0.1f, 0.4f);
                portraitRect.anchorMax = new Vector2(0.9f, 0.95f);
                portraitRect.sizeDelta = Vector2.zero;

                Image portraitImg = portrait.AddComponent<Image>();
                portraitImg.color = new Color(0.3f, 0.25f, 0.35f);

                // EP Bar placeholder
                GameObject epBar = new GameObject("EPBar");
                epBar.transform.SetParent(slot.transform, false);
                RectTransform epRect = epBar.AddComponent<RectTransform>();
                epRect.anchorMin = new Vector2(0.1f, 0.1f);
                epRect.anchorMax = new Vector2(0.9f, 0.3f);
                epRect.sizeDelta = Vector2.zero;

                Image epBg = epBar.AddComponent<Image>();
                epBg.color = new Color(0.2f, 0.15f, 0.3f);
            }

            return sidebar;
        }

        private static GameObject CreateAPCounter(GameObject parent)
        {
            GameObject counter = new GameObject("APCounter");
            counter.transform.SetParent(parent.transform, false);

            RectTransform rect = counter.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.88f, 0.4f);
            rect.anchorMax = new Vector2(1f, 0.6f);
            rect.sizeDelta = Vector2.zero;

            Image bg = counter.AddComponent<Image>();
            bg.color = new Color(0.15f, 0.1f, 0.25f, 0.9f);

            // AP Number
            GameObject apNum = CreateText(counter, "APNumber", "3", 48);
            RectTransform numRect = apNum.GetComponent<RectTransform>();
            numRect.anchorMin = new Vector2(0.5f, 0.6f);
            numRect.anchorMax = new Vector2(0.5f, 0.6f);
            numRect.sizeDelta = new Vector2(80, 60);

            // AP Label
            GameObject apLabel = CreateText(counter, "APLabel", "AP", 18);
            RectTransform labelRect = apLabel.GetComponent<RectTransform>();
            labelRect.anchorMin = new Vector2(0.5f, 0.3f);
            labelRect.anchorMax = new Vector2(0.5f, 0.3f);
            labelRect.sizeDelta = new Vector2(50, 25);

            return counter;
        }

        private static GameObject CreateExecutionButton(GameObject parent)
        {
            GameObject btnObj = new GameObject("ExecutionButton");
            btnObj.transform.SetParent(parent.transform, false);

            RectTransform rect = btnObj.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.85f, 0.05f);
            rect.anchorMax = new Vector2(0.95f, 0.15f);
            rect.sizeDelta = Vector2.zero;

            Image bg = btnObj.AddComponent<Image>();
            bg.color = new Color(0.6f, 0.3f, 0.1f);

            Button btn = btnObj.AddComponent<Button>();
            btn.targetGraphic = bg;

            // Button text
            GameObject textObj = CreateText(btnObj, "Text", "END\nTURN", 16);
            RectTransform textRect = textObj.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.sizeDelta = Vector2.zero;

            return btnObj;
        }

        private static GameObject CreateHandContainer(GameObject canvas)
        {
            GameObject container = new GameObject("HandContainer");
            container.transform.SetParent(canvas.transform, false);

            RectTransform rect = container.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.15f, 0);
            rect.anchorMax = new Vector2(0.85f, 0.35f);
            rect.sizeDelta = Vector2.zero;

            return container;
        }

        private static GameObject CreateEnemyContainer(GameObject canvas)
        {
            GameObject container = new GameObject("EnemyContainer");
            container.transform.SetParent(canvas.transform, false);

            RectTransform rect = container.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.2f, 0.5f);
            rect.anchorMax = new Vector2(0.85f, 0.85f);
            rect.sizeDelta = Vector2.zero;

            HorizontalLayoutGroup layout = container.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = 30f;
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;

            return container;
        }

        // ============================================
        // Helper Methods - Global UI
        // ============================================

        private static GameObject CreateGlobalHeaderPlaceholder(GameObject canvas)
        {
            GameObject header = new GameObject("GlobalHeader");
            header.transform.SetParent(canvas.transform, false);

            RectTransform rect = header.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0, 0.92f);
            rect.anchorMax = new Vector2(1, 1);
            rect.sizeDelta = Vector2.zero;

            Image bg = header.AddComponent<Image>();
            bg.color = new Color(0.1f, 0.08f, 0.15f, 0.95f);

            // Placeholder text
            GameObject text = CreateText(header, "HeaderText", "GLOBAL HEADER - Currency / Profile", 18);
            RectTransform textRect = text.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.sizeDelta = Vector2.zero;

            return header;
        }

        private static GameObject CreateGlobalNavDockPlaceholder(GameObject canvas)
        {
            GameObject dock = new GameObject("GlobalNavDock");
            dock.transform.SetParent(canvas.transform, false);

            RectTransform rect = dock.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0, 0);
            rect.anchorMax = new Vector2(1, 0.08f);
            rect.sizeDelta = Vector2.zero;

            Image bg = dock.AddComponent<Image>();
            bg.color = new Color(0.1f, 0.08f, 0.15f, 0.95f);

            // Nav buttons container
            HorizontalLayoutGroup layout = dock.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = 50f;
            layout.padding = new RectOffset(50, 50, 10, 10);
            layout.childAlignment = TextAnchor.MiddleCenter;

            string[] navItems = { "Bastion", "Requiems", "Collection", "Settings" };
            foreach (string item in navItems)
            {
                GameObject navBtn = CreateText(dock, $"Nav_{item}", item, 16);
                navBtn.GetComponent<RectTransform>().sizeDelta = new Vector2(100, 40);
            }

            return dock;
        }

        // ============================================
        // Helper Methods - UI Elements
        // ============================================

        private static GameObject CreateText(GameObject parent, string name, string text, int fontSize)
        {
            GameObject obj = new GameObject(name);
            obj.transform.SetParent(parent.transform, false);
            obj.AddComponent<RectTransform>().sizeDelta = new Vector2(200, 50);

            TextMeshProUGUI tmp = obj.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = Color.white;

            return obj;
        }

        private static GameObject CreateMenuButton(GameObject parent, string name, string text)
        {
            GameObject obj = new GameObject(name);
            obj.transform.SetParent(parent.transform, false);

            RectTransform rect = obj.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(250, 60);

            Image img = obj.AddComponent<Image>();
            img.color = new Color(0.25f, 0.15f, 0.35f);

            Button btn = obj.AddComponent<Button>();
            btn.targetGraphic = img;

            ColorBlock colors = btn.colors;
            colors.normalColor = new Color(0.25f, 0.15f, 0.35f);
            colors.highlightedColor = new Color(0.35f, 0.25f, 0.45f);
            colors.pressedColor = new Color(0.2f, 0.1f, 0.3f);
            colors.selectedColor = new Color(0.3f, 0.2f, 0.4f);
            btn.colors = colors;

            GameObject textObj = CreateText(obj, "Text", text, 24);
            RectTransform textRect = textObj.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.sizeDelta = Vector2.zero;

            return obj;
        }

        // ============================================
        // Utility
        // ============================================

        private static void EnsureDirectoryExists(string filePath)
        {
            string directory = Path.GetDirectoryName(filePath);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
        }
    }
}
