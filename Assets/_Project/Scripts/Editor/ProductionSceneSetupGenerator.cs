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
    ///
    /// Architecture:
    /// - Boot scene: Creates ALL persistent managers (GameManager, UIManager, etc.)
    /// - Other scenes: Only contain UI screens and scene-specific content
    /// - Managers use DontDestroyOnLoad and persist across scene transitions
    /// </summary>
    public static class ProductionSceneSetupGenerator
    {
        private const string SCENES_PATH = "Assets/_Project/Scenes";
        private const string PREFABS_PATH = "Assets/_Project/Prefabs";

        // ============================================
        // Public Methods - Menu Items
        // ============================================

        public static void SetupAllScenes()
        {
            if (!EditorUtility.DisplayDialog("Setup Production Scenes",
                "This will regenerate all production scenes:\n\n" +
                "- Boot (managers only)\n" +
                "- MainMenu\n" +
                "- Bastion\n" +
                "- NullRift\n" +
                "- Combat\n\n" +
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
                "Build Settings have been updated with the correct scene order.\n\n" +
                "IMPORTANT: Always start from Boot scene to ensure proper initialization.",
                "OK");
        }

        // ============================================
        // Boot Scene - Managers Only
        // ============================================

        public static void SetupBootScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // === Camera ===
            CreateMainCamera("Boot");

            // === EventSystem ===
            CreateEventSystem();

            // === Bootstrap (creates all managers) ===
            GameObject bootstrapObj = new GameObject("[Bootstrap]");
            bootstrapObj.AddComponent<GameBootstrap>();

            // Save scene
            string scenePath = $"{SCENES_PATH}/Boot.unity";
            EnsureDirectoryExists(scenePath);
            EditorSceneManager.SaveScene(scene, scenePath);

            Debug.Log($"[ProductionSceneSetupGenerator] Created Boot scene at {scenePath}");
        }

        // ============================================
        // MainMenu Scene - UI Only
        // ============================================

        public static void SetupMainMenuScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // === Camera ===
            CreateMainCamera("MainMenu");

            // === EventSystem ===
            CreateEventSystem();

            // === Main Canvas ===
            GameObject canvasObj = CreateMainCanvas("MainMenuCanvas");

            // === Screen Container ===
            GameObject screenContainer = CreateUIContainer(canvasObj, "ScreenContainer");

            // === MainMenuScreen ===
            GameObject mainMenuScreen = CreateMainMenuScreen(screenContainer);

            // === Overlay Container ===
            GameObject overlayContainer = CreateUIContainer(canvasObj, "OverlayContainer");

            // === Background ===
            CreateBackground(canvasObj, new Color(0.05f, 0.02f, 0.1f));

            // Save scene
            string scenePath = $"{SCENES_PATH}/MainMenu.unity";
            EditorSceneManager.SaveScene(scene, scenePath);

            Debug.Log($"[ProductionSceneSetupGenerator] Created MainMenu scene at {scenePath}");
        }

        // ============================================
        // Bastion Scene - Hub UI
        // ============================================

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

            // === BastionScreen ===
            GameObject bastionScreen = CreateBastionScreen(screenContainer);

            // === Overlay Container ===
            GameObject overlayContainer = CreateUIContainer(canvasObj, "OverlayContainer");

            // === RequiemSelectionScreen (overlay) ===
            GameObject requiemSelectionScreen = CreateRequiemSelectionScreen(overlayContainer);

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

        // ============================================
        // NullRift Scene - Map UI
        // ============================================

        public static void SetupNullRiftScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // === Camera ===
            CreateMainCamera("NullRift");

            // === EventSystem ===
            CreateEventSystem();

            // === Scene-Specific Managers ===
            GameObject managersParent = new GameObject("--- MANAGERS ---");

            GameObject mapManagerObj = new GameObject("MapManager");
            mapManagerObj.transform.SetParent(managersParent.transform);
            var mapManager = mapManagerObj.AddComponent<MapManager>();

            // Wire zone configs to MapManager
            WireZoneConfigs(mapManager);

            GameObject echoEventObj = new GameObject("EchoEventManager");
            echoEventObj.transform.SetParent(managersParent.transform);
            echoEventObj.AddComponent<EchoEventManager>();

            // === Main Canvas ===
            GameObject canvasObj = CreateMainCanvas("NullRiftCanvas");

            // === Screen Container ===
            GameObject screenContainer = CreateUIContainer(canvasObj, "ScreenContainer");

            // === MapScreen ===
            GameObject mapScreen = CreateMapScreen(screenContainer);

            // === Overlay Container ===
            GameObject overlayContainer = CreateUIContainer(canvasObj, "OverlayContainer");

            // === EchoEventScreen (overlay) ===
            GameObject echoScreen = CreateEchoEventScreen(overlayContainer);

            // === ShopScreen (overlay) ===
            GameObject shopScreen = CreateShopScreen(overlayContainer);

            // === Background ===
            CreateBackground(canvasObj, new Color(0.03f, 0.01f, 0.08f));

            // Save scene
            string scenePath = $"{SCENES_PATH}/NullRift.unity";
            EditorSceneManager.SaveScene(scene, scenePath);

            Debug.Log($"[ProductionSceneSetupGenerator] Created NullRift scene at {scenePath}");
        }

        // ============================================
        // Combat Scene - Combat UI
        // ============================================

        public static void SetupCombatScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // === Camera ===
            CreateMainCamera("Combat");

            // === EventSystem ===
            CreateEventSystem();

            // === Scene-Specific Managers ===
            GameObject managersParent = new GameObject("--- MANAGERS ---");

            GameObject turnManagerObj = new GameObject("TurnManager");
            turnManagerObj.transform.SetParent(managersParent.transform);
            turnManagerObj.AddComponent<TurnManager>();

            GameObject deckManagerObj = new GameObject("DeckManager");
            deckManagerObj.transform.SetParent(managersParent.transform);
            deckManagerObj.AddComponent<DeckManager>();

            GameObject handManagerObj = new GameObject("HandManager");
            handManagerObj.transform.SetParent(managersParent.transform);
            var handManager = handManagerObj.AddComponent<HandManager>();

            GameObject cardExecutorObj = new GameObject("CardExecutor");
            cardExecutorObj.transform.SetParent(managersParent.transform);
            cardExecutorObj.AddComponent<CardExecutor>();

            GameObject targetingObj = new GameObject("TargetingSystem");
            targetingObj.transform.SetParent(managersParent.transform);
            targetingObj.AddComponent<TargetingSystem>();

            GameObject encounterObj = new GameObject("EncounterManager");
            encounterObj.transform.SetParent(managersParent.transform);
            encounterObj.AddComponent<EncounterManager>();

            GameObject statusObj = new GameObject("StatusEffectManager");
            statusObj.transform.SetParent(managersParent.transform);
            statusObj.AddComponent<StatusEffectManager>();

            GameObject soulEssenceObj = new GameObject("SoulEssenceManager");
            soulEssenceObj.transform.SetParent(managersParent.transform);
            soulEssenceObj.AddComponent<SoulEssenceManager>();

            GameObject combatManagerObj = new GameObject("CombatManager");
            combatManagerObj.transform.SetParent(managersParent.transform);
            combatManagerObj.AddComponent<CombatManager>();

            GameObject combatBootstrapObj = new GameObject("CombatBootstrap");
            combatBootstrapObj.transform.SetParent(managersParent.transform);
            combatBootstrapObj.AddComponent<CombatBootstrap>();

            // === Main Canvas ===
            GameObject canvasObj = CreateMainCanvas("CombatCanvas");

            // === Screen Container ===
            GameObject screenContainer = CreateUIContainer(canvasObj, "ScreenContainer");

            // === CombatScreenCZN ===
            GameObject combatScreen = CreateCombatScreenCZN(screenContainer);

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

        // ============================================
        // Build Settings Configuration
        // ============================================

        public static void ConfigureBuildSettings()
        {
            List<EditorBuildSettingsScene> scenes = new List<EditorBuildSettingsScene>();

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

            cameraObj.AddComponent<UniversalAdditionalCameraData>();
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

        private static void WireZoneConfigs(MapManager mapManager)
        {
            // Load zone configs from Data folder
            var zone1 = AssetDatabase.LoadAssetAtPath<ZoneConfigSO>("Assets/_Project/Data/Zones/Zone1_Config.asset");
            var zone2 = AssetDatabase.LoadAssetAtPath<ZoneConfigSO>("Assets/_Project/Data/Zones/Zone2_Config.asset");
            var zone3 = AssetDatabase.LoadAssetAtPath<ZoneConfigSO>("Assets/_Project/Data/Zones/Zone3_Config.asset");

            // Wire via SerializedObject
            SerializedObject so = new SerializedObject(mapManager);
            var zoneConfigsProp = so.FindProperty("_zoneConfigs");

            // Count available configs
            int count = 0;
            if (zone1 != null) count++;
            if (zone2 != null) count++;
            if (zone3 != null) count++;

            zoneConfigsProp.arraySize = count;

            int index = 0;
            if (zone1 != null)
            {
                zoneConfigsProp.GetArrayElementAtIndex(index++).objectReferenceValue = zone1;
            }
            if (zone2 != null)
            {
                zoneConfigsProp.GetArrayElementAtIndex(index++).objectReferenceValue = zone2;
            }
            if (zone3 != null)
            {
                zoneConfigsProp.GetArrayElementAtIndex(index++).objectReferenceValue = zone3;
            }

            so.ApplyModifiedPropertiesWithoutUndo();

            Debug.Log($"[ProductionSceneSetupGenerator] Wired {count} zone configs to MapManager");
        }

        // ============================================
        // Screen Creation - MainMenu
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
            titleRect.anchorMin = new Vector2(0.5f, 0.75f);
            titleRect.anchorMax = new Vector2(0.5f, 0.75f);
            titleRect.anchoredPosition = Vector2.zero;
            titleRect.sizeDelta = new Vector2(800, 100);

            // === Button Container ===
            GameObject buttonContainer = new GameObject("ButtonContainer");
            buttonContainer.transform.SetParent(screenObj.transform, false);
            RectTransform buttonContainerRect = buttonContainer.AddComponent<RectTransform>();
            buttonContainerRect.anchorMin = new Vector2(0.5f, 0.4f);
            buttonContainerRect.anchorMax = new Vector2(0.5f, 0.4f);
            buttonContainerRect.sizeDelta = new Vector2(300, 350);

            VerticalLayoutGroup layout = buttonContainer.AddComponent<VerticalLayoutGroup>();
            layout.spacing = 20f;
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;
            layout.childControlWidth = true;
            layout.childControlHeight = false;

            // === Continue Button (hidden by default) ===
            GameObject continueBtn = CreateMenuButton(buttonContainer, "ContinueButton", "CONTINUE");
            CanvasGroup continueGroup = continueBtn.AddComponent<CanvasGroup>();
            continueBtn.SetActive(false);

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

        // ============================================
        // Screen Creation - Bastion
        // ============================================

        private static GameObject CreateBastionScreen(GameObject parent)
        {
            GameObject screenObj = new GameObject("BastionScreen");
            screenObj.transform.SetParent(parent.transform, false);

            RectTransform rect = screenObj.AddComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.sizeDelta = Vector2.zero;

            var bastionScreen = screenObj.AddComponent<BastionScreen>();

            // === Title ===
            GameObject titleObj = CreateText(screenObj, "Title", "THE BASTION", 48);
            RectTransform titleRect = titleObj.GetComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0.5f, 0.85f);
            titleRect.anchorMax = new Vector2(0.5f, 0.85f);
            titleRect.sizeDelta = new Vector2(400, 60);

            // === Subtitle ===
            GameObject subtitleObj = CreateText(screenObj, "Subtitle", "Command Center", 24);
            RectTransform subtitleRect = subtitleObj.GetComponent<RectTransform>();
            subtitleRect.anchorMin = new Vector2(0.5f, 0.8f);
            subtitleRect.anchorMax = new Vector2(0.5f, 0.8f);
            subtitleRect.sizeDelta = new Vector2(300, 40);

            // === Team Section ===
            GameObject teamSection = new GameObject("TeamSection");
            teamSection.transform.SetParent(screenObj.transform, false);
            RectTransform teamRect = teamSection.AddComponent<RectTransform>();
            teamRect.anchorMin = new Vector2(0.1f, 0.4f);
            teamRect.anchorMax = new Vector2(0.5f, 0.75f);
            teamRect.sizeDelta = Vector2.zero;

            GameObject teamTitle = CreateText(teamSection, "TeamSectionTitle", "SELECTED TEAM", 20);
            RectTransform teamTitleRect = teamTitle.GetComponent<RectTransform>();
            teamTitleRect.anchorMin = new Vector2(0.5f, 0.95f);
            teamTitleRect.anchorMax = new Vector2(0.5f, 0.95f);
            teamTitleRect.sizeDelta = new Vector2(200, 30);

            GameObject teamContainer = new GameObject("TeamContainer");
            teamContainer.transform.SetParent(teamSection.transform, false);
            RectTransform teamContainerRect = teamContainer.AddComponent<RectTransform>();
            teamContainerRect.anchorMin = new Vector2(0.05f, 0.1f);
            teamContainerRect.anchorMax = new Vector2(0.95f, 0.85f);
            teamContainerRect.sizeDelta = Vector2.zero;

            HorizontalLayoutGroup teamLayout = teamContainer.AddComponent<HorizontalLayoutGroup>();
            teamLayout.spacing = 20f;
            teamLayout.childAlignment = TextAnchor.MiddleCenter;
            teamLayout.childForceExpandWidth = false;
            teamLayout.childForceExpandHeight = false;

            // === Action Buttons Container ===
            GameObject buttonContainer = new GameObject("ActionButtons");
            buttonContainer.transform.SetParent(screenObj.transform, false);
            RectTransform containerRect = buttonContainer.AddComponent<RectTransform>();
            containerRect.anchorMin = new Vector2(0.55f, 0.4f);
            containerRect.anchorMax = new Vector2(0.9f, 0.75f);
            containerRect.sizeDelta = Vector2.zero;

            VerticalLayoutGroup btnLayout = buttonContainer.AddComponent<VerticalLayoutGroup>();
            btnLayout.spacing = 15f;
            btnLayout.childAlignment = TextAnchor.MiddleCenter;
            btnLayout.childForceExpandWidth = true;
            btnLayout.childForceExpandHeight = false;
            btnLayout.childControlWidth = true;
            btnLayout.childControlHeight = false;

            // Action buttons
            GameObject newRunBtn = CreateMenuButton(buttonContainer, "NewRunButton", "ENTER NULL RIFT");
            newRunBtn.GetComponent<RectTransform>().sizeDelta = new Vector2(280, 60);

            GameObject changeTeamBtn = CreateMenuButton(buttonContainer, "ChangeTeamButton", "CHANGE TEAM");
            changeTeamBtn.GetComponent<RectTransform>().sizeDelta = new Vector2(280, 50);

            GameObject viewDeckBtn = CreateMenuButton(buttonContainer, "ViewDeckButton", "VIEW DECK");
            viewDeckBtn.GetComponent<RectTransform>().sizeDelta = new Vector2(280, 50);

            GameObject continueBtn = CreateMenuButton(buttonContainer, "ContinueRunButton", "CONTINUE RUN");
            continueBtn.GetComponent<RectTransform>().sizeDelta = new Vector2(280, 50);
            CanvasGroup continueGroup = continueBtn.AddComponent<CanvasGroup>();
            continueBtn.SetActive(false);

            // === Team Stats ===
            GameObject statsContainer = new GameObject("TeamStats");
            statsContainer.transform.SetParent(screenObj.transform, false);
            RectTransform statsRect = statsContainer.AddComponent<RectTransform>();
            statsRect.anchorMin = new Vector2(0.1f, 0.3f);
            statsRect.anchorMax = new Vector2(0.5f, 0.38f);
            statsRect.sizeDelta = Vector2.zero;

            HorizontalLayoutGroup statsLayout = statsContainer.AddComponent<HorizontalLayoutGroup>();
            statsLayout.spacing = 30f;
            statsLayout.childAlignment = TextAnchor.MiddleCenter;

            GameObject hpText = CreateText(statsContainer, "TeamHPText", "150 HP", 18);
            GameObject atkText = CreateText(statsContainer, "TeamATKText", "30 ATK", 18);
            GameObject defText = CreateText(statsContainer, "TeamDEFText", "15 DEF", 18);

            // === Wire References ===
            SerializedObject screenSO = new SerializedObject(bastionScreen);
            screenSO.FindProperty("_titleText").objectReferenceValue = titleObj.GetComponent<TextMeshProUGUI>();
            screenSO.FindProperty("_subtitleText").objectReferenceValue = subtitleObj.GetComponent<TextMeshProUGUI>();
            screenSO.FindProperty("_teamContainer").objectReferenceValue = teamContainer.transform;
            screenSO.FindProperty("_teamSectionTitle").objectReferenceValue = teamTitle.GetComponent<TextMeshProUGUI>();
            screenSO.FindProperty("_newRunButton").objectReferenceValue = newRunBtn.GetComponent<Button>();
            screenSO.FindProperty("_changeTeamButton").objectReferenceValue = changeTeamBtn.GetComponent<Button>();
            screenSO.FindProperty("_viewDeckButton").objectReferenceValue = viewDeckBtn.GetComponent<Button>();
            screenSO.FindProperty("_continueRunButton").objectReferenceValue = continueBtn.GetComponent<Button>();
            screenSO.FindProperty("_continueButtonGroup").objectReferenceValue = continueGroup;
            screenSO.FindProperty("_teamHPText").objectReferenceValue = hpText.GetComponent<TextMeshProUGUI>();
            screenSO.FindProperty("_teamATKText").objectReferenceValue = atkText.GetComponent<TextMeshProUGUI>();
            screenSO.FindProperty("_teamDEFText").objectReferenceValue = defText.GetComponent<TextMeshProUGUI>();
            screenSO.FindProperty("_showGlobalHeader").boolValue = true;
            screenSO.FindProperty("_showGlobalNav").boolValue = true;
            screenSO.ApplyModifiedPropertiesWithoutUndo();

            return screenObj;
        }

        private static GameObject CreateRequiemSelectionScreen(GameObject parent)
        {
            GameObject screenObj = new GameObject("RequiemSelectionScreen");
            screenObj.transform.SetParent(parent.transform, false);

            RectTransform rect = screenObj.AddComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.sizeDelta = Vector2.zero;

            screenObj.AddComponent<RequiemSelectionScreen>();
            screenObj.SetActive(false);

            // === Panel Background ===
            Image bg = screenObj.AddComponent<Image>();
            bg.color = new Color(0.1f, 0.08f, 0.15f, 0.95f);

            // === Title ===
            GameObject titleObj = CreateText(screenObj, "Title", "SELECT YOUR REQUIEMS", 36);
            RectTransform titleRect = titleObj.GetComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0.5f, 0.9f);
            titleRect.anchorMax = new Vector2(0.5f, 0.9f);
            titleRect.sizeDelta = new Vector2(500, 50);

            // === Selection Container ===
            GameObject selectionContainer = new GameObject("SelectionContainer");
            selectionContainer.transform.SetParent(screenObj.transform, false);
            RectTransform selRect = selectionContainer.AddComponent<RectTransform>();
            selRect.anchorMin = new Vector2(0.1f, 0.2f);
            selRect.anchorMax = new Vector2(0.9f, 0.8f);
            selRect.sizeDelta = Vector2.zero;

            // === Confirm Button ===
            GameObject confirmBtn = CreateMenuButton(screenObj, "ConfirmButton", "CONFIRM TEAM");
            RectTransform confirmRect = confirmBtn.GetComponent<RectTransform>();
            confirmRect.anchorMin = new Vector2(0.5f, 0.08f);
            confirmRect.anchorMax = new Vector2(0.5f, 0.08f);
            confirmRect.sizeDelta = new Vector2(250, 50);

            return screenObj;
        }

        // ============================================
        // Screen Creation - NullRift
        // ============================================

        private static GameObject CreateMapScreen(GameObject parent)
        {
            GameObject screenObj = new GameObject("MapScreen");
            screenObj.transform.SetParent(parent.transform, false);

            RectTransform rect = screenObj.AddComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.sizeDelta = Vector2.zero;

            var mapScreen = screenObj.AddComponent<MapScreen>();

            // === Map Container ===
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

            // === MapPathRenderer ===
            GameObject pathRendererObj = new GameObject("MapPathRenderer");
            pathRendererObj.transform.SetParent(mapContainer.transform, false);
            var pathRenderer = pathRendererObj.AddComponent<MapPathRenderer>();

            // Create or load path prefab
            var pathPrefab = CreateOrLoadPathPrefab();

            // Wire MapPathRenderer
            SerializedObject pathSo = new SerializedObject(pathRenderer);
            pathSo.FindProperty("_pathPrefab").objectReferenceValue = pathPrefab;
            pathSo.FindProperty("_pathContainer").objectReferenceValue = pathContainer.transform;
            pathSo.FindProperty("_pathWidth").floatValue = 4f;
            pathSo.FindProperty("_availableColor").colorValue = new Color(0.4f, 0.8f, 1f, 0.8f); // Cyan-ish
            pathSo.FindProperty("_visitedColor").colorValue = new Color(0.5f, 0.5f, 0.5f, 0.5f);
            pathSo.FindProperty("_lockedColor").colorValue = new Color(0.3f, 0.3f, 0.3f, 0.3f);
            pathSo.ApplyModifiedPropertiesWithoutUndo();

            // === Wire MapScreen references ===
            var nodePrefab = AssetDatabase.LoadAssetAtPath<MapNodeUI>("Assets/_Project/Prefabs/UI/Map/MapNodeUI.prefab");

            SerializedObject so = new SerializedObject(mapScreen);
            so.FindProperty("_nodeContainer").objectReferenceValue = nodeContainer.transform;
            so.FindProperty("_mapContent").objectReferenceValue = mapRect;
            so.FindProperty("_pathRenderer").objectReferenceValue = pathRenderer;
            if (nodePrefab != null)
            {
                so.FindProperty("_nodePrefab").objectReferenceValue = nodePrefab;
                Debug.Log("[ProductionSceneSetupGenerator] Wired MapNodeUI prefab to MapScreen");
            }
            else
            {
                Debug.LogWarning("[ProductionSceneSetupGenerator] MapNodeUI prefab not found at Assets/_Project/Prefabs/UI/Map/MapNodeUI.prefab");
            }
            so.ApplyModifiedPropertiesWithoutUndo();

            Debug.Log("[ProductionSceneSetupGenerator] Created MapPathRenderer with path connections");

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

            // === Background ===
            Image bg = screenObj.AddComponent<Image>();
            bg.color = new Color(0.08f, 0.05f, 0.12f, 0.98f);

            // === Shop Title ===
            GameObject titleObj = CreateText(screenObj, "ShopTitle", "VOID MARKET", 42);
            RectTransform titleRect = titleObj.GetComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0.5f, 0.92f);
            titleRect.anchorMax = new Vector2(0.5f, 0.92f);
            titleRect.sizeDelta = new Vector2(400, 60);

            // === Currency Display ===
            GameObject currencyObj = CreateText(screenObj, "CurrencyDisplay", "Void Shards: 0", 24);
            RectTransform currencyRect = currencyObj.GetComponent<RectTransform>();
            currencyRect.anchorMin = new Vector2(0.85f, 0.92f);
            currencyRect.anchorMax = new Vector2(0.85f, 0.92f);
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

        // ============================================
        // Screen Creation - Combat
        // ============================================

        private static GameObject CreateCombatScreenCZN(GameObject parent)
        {
            GameObject screenObj = new GameObject("CombatScreenCZN");
            screenObj.transform.SetParent(parent.transform, false);

            RectTransform rect = screenObj.AddComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.sizeDelta = Vector2.zero;

            var combatScreen = screenObj.AddComponent<CombatScreenCZN>();

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
            SerializedObject screenSO = new SerializedObject(combatScreen);
            screenSO.FindProperty("_showGlobalHeader").boolValue = false;
            screenSO.FindProperty("_showGlobalNav").boolValue = false;
            screenSO.ApplyModifiedPropertiesWithoutUndo();

            return screenObj;
        }

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
        // Helper Methods - Global UI Placeholders
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

        /// <summary>
        /// Creates or loads the path line prefab for map connections.
        /// </summary>
        private static Image CreateOrLoadPathPrefab()
        {
            string prefabPath = "Assets/_Project/Prefabs/UI/Map/MapPathLine.prefab";

            // Try to load existing prefab
            var existingPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (existingPrefab != null)
            {
                var existingImage = existingPrefab.GetComponent<Image>();
                if (existingImage != null)
                {
                    Debug.Log("[ProductionSceneSetupGenerator] Loaded existing MapPathLine prefab");
                    return existingImage;
                }
            }

            // Create new path line prefab
            EnsureDirectoryExists(prefabPath);

            GameObject pathLineObj = new GameObject("MapPathLine");
            RectTransform rect = pathLineObj.AddComponent<RectTransform>();
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(100f, 4f); // Default size, will be stretched

            Image pathImage = pathLineObj.AddComponent<Image>();
            pathImage.color = Color.white;
            pathImage.raycastTarget = false;

            // Save as prefab
            bool success;
            GameObject savedPrefab = PrefabUtility.SaveAsPrefabAsset(pathLineObj, prefabPath, out success);
            Object.DestroyImmediate(pathLineObj);

            if (success)
            {
                Debug.Log($"[ProductionSceneSetupGenerator] Created MapPathLine prefab at {prefabPath}");
                return savedPrefab.GetComponent<Image>();
            }
            else
            {
                Debug.LogError("[ProductionSceneSetupGenerator] Failed to create MapPathLine prefab");
                return null;
            }
        }

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
