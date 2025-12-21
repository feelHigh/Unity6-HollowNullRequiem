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
using HNR.Characters;
using HNR.VFX;
using HNR.Progression;
using HNR.UI;
using HNR.UI.Screens;
using HNR.UI.Combat;
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

            // === SanctuaryScreen (overlay) ===
            GameObject sanctuaryScreen = CreateSanctuaryScreen(overlayContainer);

            // === TreasureScreen (overlay) ===
            GameObject treasureScreen = CreateTreasureScreen(overlayContainer);

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
            var encounterManager = encounterObj.AddComponent<EncounterManager>();

            GameObject statusObj = new GameObject("StatusEffectManager");
            statusObj.transform.SetParent(managersParent.transform);
            statusObj.AddComponent<StatusEffectManager>();

            GameObject soulEssenceObj = new GameObject("SoulEssenceManager");
            soulEssenceObj.transform.SetParent(managersParent.transform);
            soulEssenceObj.AddComponent<SoulEssenceManager>();

            GameObject combatManagerObj = new GameObject("CombatManager");
            combatManagerObj.transform.SetParent(managersParent.transform);
            combatManagerObj.AddComponent<CombatManager>();

            GameObject corruptionManagerObj = new GameObject("CorruptionManager");
            corruptionManagerObj.transform.SetParent(managersParent.transform);
            corruptionManagerObj.AddComponent<CorruptionManager>();

            GameObject vfxPoolManagerObj = new GameObject("VFXPoolManager");
            vfxPoolManagerObj.transform.SetParent(managersParent.transform);
            var vfxPoolManager = vfxPoolManagerObj.AddComponent<VFXPoolManager>();

            GameObject relicManagerObj = new GameObject("RelicManager");
            relicManagerObj.transform.SetParent(managersParent.transform);
            relicManagerObj.AddComponent<RelicManager>();

            GameObject combatBootstrapObj = new GameObject("CombatBootstrap");
            combatBootstrapObj.transform.SetParent(managersParent.transform);
            var combatBootstrap = combatBootstrapObj.AddComponent<CombatBootstrap>();

            // === World Space UI Containers ===
            // These are plain transforms for world-space floating UIs (not under screen-space canvas)
            GameObject worldSpaceUIParent = new GameObject("--- WORLD SPACE UI ---");

            GameObject worldSpaceEnemyContainer = new GameObject("WorldSpaceEnemyUIContainer");
            worldSpaceEnemyContainer.transform.SetParent(worldSpaceUIParent.transform);

            GameObject worldSpaceAllyContainer = new GameObject("WorldSpaceAllyIndicatorContainer");
            worldSpaceAllyContainer.transform.SetParent(worldSpaceUIParent.transform);

            // === Main Canvas ===
            GameObject canvasObj = CreateMainCanvas("CombatCanvas");

            // === Screen Container ===
            GameObject screenContainer = CreateUIContainer(canvasObj, "ScreenContainer");

            // === CombatScreenCZN ===
            GameObject combatScreen = CreateCombatScreenCZN(screenContainer);

            // === ResultsScreen (Victory/Defeat) ===
            GameObject resultsScreen = CreateResultsScreen(screenContainer);
            resultsScreen.SetActive(false); // Hidden by default

            // === Hand Container ===
            GameObject handContainer = CreateHandContainer(canvasObj);

            // === Enemy Container ===
            GameObject enemyContainer = CreateEnemyContainer(canvasObj);

            // === DamageNumberSpawner ===
            GameObject damageSpawnerObj = new GameObject("DamageNumberSpawner");
            damageSpawnerObj.transform.SetParent(canvasObj.transform, false);
            damageSpawnerObj.AddComponent<DamageNumberSpawner>();

            // === Wire HandManager ===
            var cardPrefab = AssetDatabase.LoadAssetAtPath<Cards.Card>("Assets/_Project/Prefabs/Cards/Card.prefab");
            SerializedObject handSO = new SerializedObject(handManager);
            handSO.FindProperty("_handContainer").objectReferenceValue = handContainer.transform;
            if (cardPrefab != null)
            {
                handSO.FindProperty("_cardPrefab").objectReferenceValue = cardPrefab;
                Debug.Log("[ProductionSceneSetupGenerator] Wired HandManager with Card prefab");
            }
            else
            {
                Debug.LogWarning("[ProductionSceneSetupGenerator] Card.prefab not found - run HNR > 2. Prefabs > UI > Card Prefab first");
            }
            handSO.ApplyModifiedPropertiesWithoutUndo();

            // === Create World Space Enemy Slots ===
            GameObject enemySlotsParent = new GameObject("--- ENEMY SLOTS ---");
            Transform[] enemySlots = new Transform[3];

            // Position enemy slots in world space (right side of screen)
            float[] enemyXPositions = { 3f, 5f, 7f };
            for (int i = 0; i < 3; i++)
            {
                GameObject slot = new GameObject($"EnemySlot_{i}");
                slot.transform.SetParent(enemySlotsParent.transform);
                slot.transform.position = new Vector3(enemyXPositions[i], 0f, 0f);
                enemySlots[i] = slot.transform;
            }

            // === Create World Space Ally Slots ===
            GameObject allySlotsParent = new GameObject("--- ALLY SLOTS ---");
            Transform[] allySlots = new Transform[3];

            // Position ally slots in world space (left side of screen)
            float[] allyXPositions = { -7f, -5f, -3f };
            for (int i = 0; i < 3; i++)
            {
                GameObject slot = new GameObject($"AllySlot_{i}");
                slot.transform.SetParent(allySlotsParent.transform);
                slot.transform.position = new Vector3(allyXPositions[i], 0f, 0f);
                allySlots[i] = slot.transform;
            }

            // === Wire EncounterManager ===
            var enemyPrefab = AssetDatabase.LoadAssetAtPath<EnemyInstance>("Assets/_Project/Prefabs/Combat/EnemyInstance.prefab");
            if (enemyPrefab != null)
            {
                SerializedObject encounterSO = new SerializedObject(encounterManager);
                encounterSO.FindProperty("_enemyPrefab").objectReferenceValue = enemyPrefab;

                var slotsArray = encounterSO.FindProperty("_enemySlots");
                slotsArray.arraySize = 3;
                for (int i = 0; i < 3; i++)
                {
                    slotsArray.GetArrayElementAtIndex(i).objectReferenceValue = enemySlots[i];
                }

                encounterSO.ApplyModifiedPropertiesWithoutUndo();
                Debug.Log("[ProductionSceneSetupGenerator] Wired EncounterManager with enemy prefab and slots");
            }
            else
            {
                Debug.LogWarning("[ProductionSceneSetupGenerator] EnemyInstance.prefab not found - run HNR > Production > Create All Prefabs first");
            }

            // === Wire VFXPoolManager with VFX prefabs ===
            WireVFXPoolManager(vfxPoolManager);

            // === Wire DamageNumberSpawner ===
            var damageSpawner = damageSpawnerObj.GetComponent<DamageNumberSpawner>();
            var damagePrefab = AssetDatabase.LoadAssetAtPath<DamageNumber>("Assets/_Project/Prefabs/UI/Effects/DamageNumber.prefab");
            if (damageSpawner != null && damagePrefab != null)
            {
                SerializedObject dmgSO = new SerializedObject(damageSpawner);
                dmgSO.FindProperty("_prefab").objectReferenceValue = damagePrefab;
                dmgSO.FindProperty("_canvas").objectReferenceValue = canvasObj.GetComponent<Canvas>();
                dmgSO.ApplyModifiedPropertiesWithoutUndo();
                Debug.Log("[ProductionSceneSetupGenerator] Wired DamageNumberSpawner");
            }

            // === Wire CombatScreenCZN components ===
            WireCombatScreenCZN(combatScreen);

            // NOTE: No opaque background for Combat scene - we need to see world-space enemies
            // Camera background color provides the backdrop instead

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

            // Position camera at Z=-10 to view 2D objects at Z=0
            cameraObj.transform.position = new Vector3(0f, 0f, -10f);

            Camera camera = cameraObj.AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.05f, 0.02f, 0.1f);
            camera.orthographic = true;
            camera.orthographicSize = 5f;
            camera.nearClipPlane = 0.1f;
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

            // ============================================
            // ZONE HEADER (CZN Layout)
            // ============================================
            GameObject zoneHeader = CreateZoneHeaderCZN(screenObj);

            // === Map Container ===
            GameObject mapContainer = new GameObject("MapContainer");
            mapContainer.transform.SetParent(screenObj.transform, false);
            RectTransform mapRect = mapContainer.AddComponent<RectTransform>();
            mapRect.anchorMin = new Vector2(0, 0.08f);
            mapRect.anchorMax = new Vector2(1, 0.84f);
            mapRect.sizeDelta = Vector2.zero;

            Image mapBg = mapContainer.AddComponent<Image>();
            mapBg.color = new Color(0.07f, 0.04f, 0.1f, 0.8f);

            // ============================================
            // NODE TYPE LEGEND (Bottom Footer)
            // ============================================
            GameObject legend = CreateMapLegendCZN(screenObj);

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

            // Wire zone header references
            var zoneTitle = zoneHeader.transform.Find("TitleContainer/ZoneTitle")?.GetComponent<TMP_Text>();
            var zoneSubtitle = zoneHeader.transform.Find("TitleContainer/ZoneSubtitle")?.GetComponent<TMP_Text>();
            var hpText = zoneHeader.transform.Find("StatsContainer/HPContainer/HPText")?.GetComponent<TMP_Text>();
            var hpIcon = zoneHeader.transform.Find("StatsContainer/HPContainer/HPIcon")?.GetComponent<Image>();
            var currencyText = zoneHeader.transform.Find("StatsContainer/CurrencyContainer/CurrencyText")?.GetComponent<TMP_Text>();
            var currencyIcon = zoneHeader.transform.Find("StatsContainer/CurrencyContainer/CurrencyIcon")?.GetComponent<Image>();

            if (zoneTitle != null) so.FindProperty("_zoneTitle").objectReferenceValue = zoneTitle;
            if (zoneSubtitle != null) so.FindProperty("_zoneSubtitle").objectReferenceValue = zoneSubtitle;
            if (hpText != null) so.FindProperty("_hpText").objectReferenceValue = hpText;
            if (hpIcon != null) so.FindProperty("_hpIcon").objectReferenceValue = hpIcon;
            if (currencyText != null) so.FindProperty("_currencyText").objectReferenceValue = currencyText;
            if (currencyIcon != null) so.FindProperty("_currencyIcon").objectReferenceValue = currencyIcon;

            so.ApplyModifiedPropertiesWithoutUndo();

            Debug.Log("[ProductionSceneSetupGenerator] Created MapScreen with CZN zone header");

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

        private static GameObject CreateSanctuaryScreen(GameObject parent)
        {
            GameObject screenObj = new GameObject("SanctuaryScreen");
            screenObj.transform.SetParent(parent.transform, false);

            RectTransform rect = screenObj.AddComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.sizeDelta = Vector2.zero;

            screenObj.AddComponent<SanctuaryScreen>();
            screenObj.SetActive(false);

            // Background
            Image bg = screenObj.AddComponent<Image>();
            bg.color = new Color(0.05f, 0.08f, 0.06f, 0.98f); // Greenish dark

            // Title
            GameObject titleObj = CreateText(screenObj, "Title", "SANCTUARY", 32);
            RectTransform titleRect = titleObj.GetComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0.5f, 0.88f);
            titleRect.anchorMax = new Vector2(0.5f, 0.88f);
            titleRect.sizeDelta = new Vector2(300, 50);
            titleObj.GetComponent<TMP_Text>().color = new Color(0.18f, 0.8f, 0.44f); // Health green

            // Description
            GameObject descObj = CreateText(screenObj, "Description", "A moment of respite in the Null Rift...", 16);
            RectTransform descRect = descObj.GetComponent<RectTransform>();
            descRect.anchorMin = new Vector2(0.5f, 0.78f);
            descRect.anchorMax = new Vector2(0.5f, 0.78f);
            descRect.sizeDelta = new Vector2(500, 40);
            descObj.GetComponent<TMP_Text>().color = new Color(0.7f, 0.7f, 0.7f);

            // Choice buttons container
            GameObject choicesContainer = new GameObject("ChoicesContainer");
            choicesContainer.transform.SetParent(screenObj.transform, false);
            RectTransform choicesRect = choicesContainer.AddComponent<RectTransform>();
            choicesRect.anchorMin = new Vector2(0.15f, 0.25f);
            choicesRect.anchorMax = new Vector2(0.85f, 0.7f);
            choicesRect.sizeDelta = Vector2.zero;

            HorizontalLayoutGroup choicesLayout = choicesContainer.AddComponent<HorizontalLayoutGroup>();
            choicesLayout.spacing = 30;
            choicesLayout.childAlignment = TextAnchor.MiddleCenter;
            choicesLayout.childForceExpandWidth = true;
            choicesLayout.childForceExpandHeight = true;

            // Rest button (green)
            CreateSanctuaryChoice(choicesContainer, "RestButton", "REST", "Heal 30% HP", new Color(0.18f, 0.8f, 0.44f));

            // Purify button (cyan)
            CreateSanctuaryChoice(choicesContainer, "PurifyButton", "PURIFY", "Remove -30 Corruption", new Color(0f, 0.83f, 0.89f));

            // Upgrade button (gold)
            CreateSanctuaryChoice(choicesContainer, "UpgradeButton", "UPGRADE", "Upgrade a card", new Color(0.83f, 0.69f, 0.22f));

            // Skip button
            GameObject skipBtn = CreateMenuButton(screenObj, "SkipButton", "LEAVE");
            RectTransform skipRect = skipBtn.GetComponent<RectTransform>();
            skipRect.anchorMin = new Vector2(0.5f, 0.1f);
            skipRect.anchorMax = new Vector2(0.5f, 0.1f);
            skipRect.sizeDelta = new Vector2(150, 40);

            return screenObj;
        }

        private static void CreateSanctuaryChoice(GameObject parent, string name, string title, string desc, Color color)
        {
            GameObject choice = new GameObject(name);
            choice.transform.SetParent(parent.transform, false);

            Image choiceBg = choice.AddComponent<Image>();
            choiceBg.color = new Color(color.r * 0.2f, color.g * 0.2f, color.b * 0.2f, 0.8f);

            Button btn = choice.AddComponent<Button>();
            btn.targetGraphic = choiceBg;

            VerticalLayoutGroup layout = choice.AddComponent<VerticalLayoutGroup>();
            layout.spacing = 10;
            layout.padding = new RectOffset(15, 15, 20, 20);
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;

            // Icon placeholder
            GameObject icon = new GameObject("Icon");
            icon.transform.SetParent(choice.transform, false);
            RectTransform iconRect = icon.AddComponent<RectTransform>();
            iconRect.sizeDelta = new Vector2(60, 60);
            Image iconImg = icon.AddComponent<Image>();
            iconImg.color = color;
            var iconLayout = icon.AddComponent<LayoutElement>();
            iconLayout.preferredHeight = 60;

            // Title
            GameObject titleObj = CreateText(choice, "Title", title, 18);
            titleObj.GetComponent<TMP_Text>().color = color;
            titleObj.GetComponent<TMP_Text>().fontStyle = TMPro.FontStyles.Bold;

            // Description
            GameObject descObj = CreateText(choice, "Desc", desc, 12);
            descObj.GetComponent<TMP_Text>().color = new Color(0.7f, 0.7f, 0.7f);
        }

        private static GameObject CreateTreasureScreen(GameObject parent)
        {
            GameObject screenObj = new GameObject("TreasureScreen");
            screenObj.transform.SetParent(parent.transform, false);

            RectTransform rect = screenObj.AddComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.sizeDelta = Vector2.zero;

            screenObj.AddComponent<TreasureScreen>();
            screenObj.SetActive(false);

            // Background with gold tint
            Image bg = screenObj.AddComponent<Image>();
            bg.color = new Color(0.1f, 0.08f, 0.05f, 0.98f);

            // Title
            GameObject titleObj = CreateText(screenObj, "TitleText", "TREASURE", 32);
            RectTransform titleRect = titleObj.GetComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0.5f, 0.88f);
            titleRect.anchorMax = new Vector2(0.5f, 0.88f);
            titleRect.sizeDelta = new Vector2(300, 50);
            titleObj.GetComponent<TMP_Text>().color = new Color(0.83f, 0.69f, 0.22f); // Soul gold
            titleObj.GetComponent<TMP_Text>().fontStyle = TMPro.FontStyles.Bold;

            // Subtitle
            GameObject subtitleObj = CreateText(screenObj, "SubtitleText", "Choose a card reward:", 16);
            RectTransform subtitleRect = subtitleObj.GetComponent<RectTransform>();
            subtitleRect.anchorMin = new Vector2(0.5f, 0.8f);
            subtitleRect.anchorMax = new Vector2(0.5f, 0.8f);
            subtitleRect.sizeDelta = new Vector2(400, 30);

            // Card reward container
            GameObject cardContainer = new GameObject("CardRewardContainer");
            cardContainer.transform.SetParent(screenObj.transform, false);
            RectTransform cardRect = cardContainer.AddComponent<RectTransform>();
            cardRect.anchorMin = new Vector2(0.1f, 0.35f);
            cardRect.anchorMax = new Vector2(0.9f, 0.75f);
            cardRect.sizeDelta = Vector2.zero;

            HorizontalLayoutGroup cardLayout = cardContainer.AddComponent<HorizontalLayoutGroup>();
            cardLayout.spacing = 30;
            cardLayout.childAlignment = TextAnchor.MiddleCenter;
            cardLayout.childForceExpandWidth = false;
            cardLayout.childForceExpandHeight = true;

            // Skip button
            GameObject skipBtn = CreateMenuButton(screenObj, "SkipRewardButton", "SKIP REWARD");
            RectTransform skipRect = skipBtn.GetComponent<RectTransform>();
            skipRect.anchorMin = new Vector2(0.5f, 0.22f);
            skipRect.anchorMax = new Vector2(0.5f, 0.22f);
            skipRect.sizeDelta = new Vector2(180, 45);

            // Continue button
            GameObject continueBtn = CreateMenuButton(screenObj, "ContinueButton", "CONTINUE");
            RectTransform continueRect = continueBtn.GetComponent<RectTransform>();
            continueRect.anchorMin = new Vector2(0.5f, 0.1f);
            continueRect.anchorMax = new Vector2(0.5f, 0.1f);
            continueRect.sizeDelta = new Vector2(180, 45);
            continueBtn.GetComponent<Button>().interactable = false;

            // Wire TreasureScreen references
            var treasureScreen = screenObj.GetComponent<TreasureScreen>();
            SerializedObject so = new SerializedObject(treasureScreen);
            so.FindProperty("_titleText").objectReferenceValue = titleObj.GetComponent<TMP_Text>();
            so.FindProperty("_subtitleText").objectReferenceValue = subtitleObj.GetComponent<TMP_Text>();
            so.FindProperty("_cardRewardContainer").objectReferenceValue = cardContainer.transform;
            so.FindProperty("_skipRewardButton").objectReferenceValue = skipBtn.GetComponent<Button>();
            so.FindProperty("_continueButton").objectReferenceValue = continueBtn.GetComponent<Button>();
            so.ApplyModifiedPropertiesWithoutUndo();

            return screenObj;
        }

        /// <summary>
        /// Creates the ResultsScreen for victory/defeat display.
        /// </summary>
        private static GameObject CreateResultsScreen(GameObject parent)
        {
            GameObject screenObj = new GameObject("ResultsScreen");
            screenObj.transform.SetParent(parent.transform, false);

            RectTransform rect = screenObj.AddComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.sizeDelta = Vector2.zero;

            screenObj.AddComponent<ResultsScreen>();

            // Background
            Image bg = screenObj.AddComponent<Image>();
            bg.color = new Color(0.05f, 0.03f, 0.08f, 0.98f);

            // Victory glow overlay
            GameObject victoryGlow = new GameObject("VictoryGlow");
            victoryGlow.transform.SetParent(screenObj.transform, false);
            RectTransform glowRect = victoryGlow.AddComponent<RectTransform>();
            glowRect.anchorMin = Vector2.zero;
            glowRect.anchorMax = Vector2.one;
            glowRect.sizeDelta = Vector2.zero;
            Image glowImg = victoryGlow.AddComponent<Image>();
            glowImg.color = new Color(0.83f, 0.69f, 0.22f, 0.15f); // Gold glow
            CanvasGroup victoryGlowCG = victoryGlow.AddComponent<CanvasGroup>();

            // Defeat overlay
            GameObject defeatOverlay = new GameObject("DefeatOverlay");
            defeatOverlay.transform.SetParent(screenObj.transform, false);
            RectTransform defeatRect = defeatOverlay.AddComponent<RectTransform>();
            defeatRect.anchorMin = Vector2.zero;
            defeatRect.anchorMax = Vector2.one;
            defeatRect.sizeDelta = Vector2.zero;
            Image defeatImg = defeatOverlay.AddComponent<Image>();
            defeatImg.color = new Color(0.3f, 0.05f, 0.05f, 0.3f); // Red overlay
            CanvasGroup defeatOverlayCG = defeatOverlay.AddComponent<CanvasGroup>();
            defeatOverlay.SetActive(false);

            // Result title (VICTORY/DEFEAT)
            GameObject titleObj = CreateText(screenObj, "ResultTitleText", "VICTORY", 36);
            RectTransform titleRect = titleObj.GetComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0.5f, 0.85f);
            titleRect.anchorMax = new Vector2(0.5f, 0.85f);
            titleRect.sizeDelta = new Vector2(400, 60);
            var titleTmp = titleObj.GetComponent<TMP_Text>();
            titleTmp.color = new Color(0.83f, 0.69f, 0.22f); // Soul gold
            titleTmp.fontStyle = TMPro.FontStyles.Bold;

            // Summary text
            GameObject summaryObj = CreateText(screenObj, "SummaryText", "Enemy Defeated", 16);
            RectTransform summaryRect = summaryObj.GetComponent<RectTransform>();
            summaryRect.anchorMin = new Vector2(0.5f, 0.78f);
            summaryRect.anchorMax = new Vector2(0.5f, 0.78f);
            summaryRect.sizeDelta = new Vector2(500, 30);

            // Card reward title
            GameObject cardTitleObj = CreateText(screenObj, "CardRewardTitle", "Choose a card reward:", 18);
            RectTransform cardTitleRect = cardTitleObj.GetComponent<RectTransform>();
            cardTitleRect.anchorMin = new Vector2(0.5f, 0.72f);
            cardTitleRect.anchorMax = new Vector2(0.5f, 0.72f);
            cardTitleRect.sizeDelta = new Vector2(400, 30);

            // Card reward container
            GameObject cardContainer = new GameObject("CardRewardContainer");
            cardContainer.transform.SetParent(screenObj.transform, false);
            RectTransform cardRect = cardContainer.AddComponent<RectTransform>();
            cardRect.anchorMin = new Vector2(0.1f, 0.35f);
            cardRect.anchorMax = new Vector2(0.9f, 0.68f);
            cardRect.sizeDelta = Vector2.zero;

            HorizontalLayoutGroup cardLayout = cardContainer.AddComponent<HorizontalLayoutGroup>();
            cardLayout.spacing = 30;
            cardLayout.childAlignment = TextAnchor.MiddleCenter;
            cardLayout.childForceExpandWidth = false;
            cardLayout.childForceExpandHeight = true;

            // Skip button
            GameObject skipBtn = CreateMenuButton(screenObj, "SkipRewardButton", "SKIP REWARD");
            RectTransform skipRect = skipBtn.GetComponent<RectTransform>();
            skipRect.anchorMin = new Vector2(0.35f, 0.22f);
            skipRect.anchorMax = new Vector2(0.35f, 0.22f);
            skipRect.sizeDelta = new Vector2(160, 40);

            // Continue button (victory)
            GameObject continueBtn = CreateMenuButton(screenObj, "ContinueButton", "CONTINUE");
            RectTransform continueRect = continueBtn.GetComponent<RectTransform>();
            continueRect.anchorMin = new Vector2(0.65f, 0.22f);
            continueRect.anchorMax = new Vector2(0.65f, 0.22f);
            continueRect.sizeDelta = new Vector2(160, 40);
            continueBtn.GetComponent<Button>().interactable = false;

            // Retry button (defeat)
            GameObject retryBtn = CreateMenuButton(screenObj, "RetryButton", "RETRY");
            RectTransform retryRect = retryBtn.GetComponent<RectTransform>();
            retryRect.anchorMin = new Vector2(0.35f, 0.12f);
            retryRect.anchorMax = new Vector2(0.35f, 0.12f);
            retryRect.sizeDelta = new Vector2(160, 40);
            retryBtn.SetActive(false);

            // Abandon button (defeat)
            GameObject abandonBtn = CreateMenuButton(screenObj, "AbandonButton", "ABANDON RUN");
            RectTransform abandonRect = abandonBtn.GetComponent<RectTransform>();
            abandonRect.anchorMin = new Vector2(0.65f, 0.12f);
            abandonRect.anchorMax = new Vector2(0.65f, 0.12f);
            abandonRect.sizeDelta = new Vector2(160, 40);
            abandonBtn.SetActive(false);

            // Wire ResultsScreen references
            var resultsScreen = screenObj.GetComponent<ResultsScreen>();
            SerializedObject so = new SerializedObject(resultsScreen);
            so.FindProperty("_resultTitleText").objectReferenceValue = titleTmp;
            so.FindProperty("_summaryText").objectReferenceValue = summaryObj.GetComponent<TMP_Text>();
            so.FindProperty("_victoryGlow").objectReferenceValue = victoryGlowCG;
            so.FindProperty("_defeatOverlay").objectReferenceValue = defeatOverlayCG;
            so.FindProperty("_cardRewardTitle").objectReferenceValue = cardTitleObj.GetComponent<TMP_Text>();
            so.FindProperty("_cardRewardContainer").objectReferenceValue = cardContainer.transform;
            so.FindProperty("_skipRewardButton").objectReferenceValue = skipBtn.GetComponent<Button>();
            so.FindProperty("_continueButton").objectReferenceValue = continueBtn.GetComponent<Button>();
            so.FindProperty("_retryButton").objectReferenceValue = retryBtn.GetComponent<Button>();
            so.FindProperty("_abandonButton").objectReferenceValue = abandonBtn.GetComponent<Button>();
            so.ApplyModifiedPropertiesWithoutUndo();

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

            // ============================================
            // TOP HUD (48px) - Vitality Bar + System Menu
            // ============================================
            GameObject topHUD = new GameObject("TopHUD");
            topHUD.transform.SetParent(screenObj.transform, false);
            RectTransform topHUDRect = topHUD.AddComponent<RectTransform>();
            topHUDRect.anchorMin = new Vector2(0, 0.87f);
            topHUDRect.anchorMax = new Vector2(1, 1);
            topHUDRect.sizeDelta = Vector2.zero;
            Image topHUDBg = topHUD.AddComponent<Image>();
            topHUDBg.color = new Color(0f, 0f, 0f, 0.85f);

            // Shared Vitality Bar (left side of top HUD)
            GameObject vitalityBar = CreateSharedVitalityBarCZN(topHUD);

            // System Menu Bar (right side of top HUD)
            GameObject sysMenuBar = CreateSystemMenuBar(topHUD);

            // ============================================
            // LEFT SIDEBAR (80px) - Party Status
            // ============================================
            GameObject partySidebar = CreatePartySidebarCZN(screenObj);

            // ============================================
            // CENTER - Enemy Zone + Battle Area
            // ============================================
            GameObject centerArea = new GameObject("CenterArea");
            centerArea.transform.SetParent(screenObj.transform, false);
            RectTransform centerRect = centerArea.AddComponent<RectTransform>();
            centerRect.anchorMin = new Vector2(0.1f, 0.35f);
            centerRect.anchorMax = new Vector2(0.92f, 0.87f);
            centerRect.sizeDelta = Vector2.zero;

            // Enemy Zone (top half of center)
            GameObject enemyZone = new GameObject("EnemyZone");
            enemyZone.transform.SetParent(centerArea.transform, false);
            RectTransform enemyZoneRect = enemyZone.AddComponent<RectTransform>();
            enemyZoneRect.anchorMin = new Vector2(0.1f, 0.4f);
            enemyZoneRect.anchorMax = new Vector2(0.9f, 1f);
            enemyZoneRect.sizeDelta = Vector2.zero;

            HorizontalLayoutGroup enemyLayout = enemyZone.AddComponent<HorizontalLayoutGroup>();
            enemyLayout.spacing = 40f;
            enemyLayout.childAlignment = TextAnchor.MiddleCenter;
            enemyLayout.childForceExpandWidth = false;
            enemyLayout.childForceExpandHeight = false;

            // Ally Indicators Zone (bottom of center)
            GameObject allyZone = new GameObject("AllyIndicatorZone");
            allyZone.transform.SetParent(centerArea.transform, false);
            RectTransform allyZoneRect = allyZone.AddComponent<RectTransform>();
            allyZoneRect.anchorMin = new Vector2(0.3f, 0.1f);
            allyZoneRect.anchorMax = new Vector2(0.7f, 0.35f);
            allyZoneRect.sizeDelta = Vector2.zero;

            HorizontalLayoutGroup allyLayout = allyZone.AddComponent<HorizontalLayoutGroup>();
            allyLayout.spacing = 20f;
            allyLayout.childAlignment = TextAnchor.MiddleCenter;
            allyLayout.childForceExpandWidth = false;
            allyLayout.childForceExpandHeight = false;

            // Turn/Zone Info (bottom center)
            GameObject turnInfoContainer = new GameObject("TurnInfoContainer");
            turnInfoContainer.transform.SetParent(centerArea.transform, false);
            RectTransform turnInfoContainerRect = turnInfoContainer.AddComponent<RectTransform>();
            turnInfoContainerRect.anchorMin = new Vector2(0.5f, 0);
            turnInfoContainerRect.anchorMax = new Vector2(0.5f, 0.1f);
            turnInfoContainerRect.sizeDelta = new Vector2(150, 24);
            Image turnInfoBg = turnInfoContainer.AddComponent<Image>();
            turnInfoBg.color = new Color(0f, 0f, 0f, 0.5f);

            GameObject turnInfo = CreateText(turnInfoContainer, "TurnInfo", "Turn 1 • Zone 1", 12);
            RectTransform turnInfoRect = turnInfo.GetComponent<RectTransform>();
            turnInfoRect.anchorMin = Vector2.zero;
            turnInfoRect.anchorMax = Vector2.one;
            turnInfoRect.sizeDelta = Vector2.zero;
            turnInfoContainer.transform.SetAsFirstSibling();

            // ============================================
            // RIGHT SIDEBAR (60px) - Deck Info
            // ============================================
            GameObject rightSidebar = CreateDeckInfoSidebar(screenObj);

            // ============================================
            // BOTTOM COMMAND CENTER (120px)
            // ============================================
            GameObject bottomHUD = new GameObject("BottomCommandCenter");
            bottomHUD.transform.SetParent(screenObj.transform, false);
            RectTransform bottomRect = bottomHUD.AddComponent<RectTransform>();
            bottomRect.anchorMin = new Vector2(0, 0);
            bottomRect.anchorMax = new Vector2(1, 0.35f);
            bottomRect.sizeDelta = Vector2.zero;
            Image bottomBg = bottomHUD.AddComponent<Image>();
            bottomBg.color = new Color(0f, 0f, 0f, 0.8f);

            // AP Counter (top center of bottom area)
            GameObject apCounter = CreateAPCounterCZN(bottomHUD);

            // Execution Button (right side)
            GameObject executionBtn = CreateExecutionButtonCZN(bottomHUD);

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
            var apText = apNum.GetComponent<TMP_Text>();

            // AP Label
            GameObject apLabel = CreateText(counter, "APLabel", "AP", 18);
            RectTransform labelRect = apLabel.GetComponent<RectTransform>();
            labelRect.anchorMin = new Vector2(0.5f, 0.3f);
            labelRect.anchorMax = new Vector2(0.5f, 0.3f);
            labelRect.sizeDelta = new Vector2(50, 25);

            // Add APCounterDisplay component and wire references
            var apCounter = counter.AddComponent<APCounterDisplay>();
            var so = new SerializedObject(apCounter);
            so.FindProperty("_apText").objectReferenceValue = apText;
            so.FindProperty("_glowBackground").objectReferenceValue = bg;
            so.ApplyModifiedPropertiesWithoutUndo();

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

            // Add ExecutionButton component and wire references
            var execButton = btnObj.AddComponent<ExecutionButton>();
            var so = new SerializedObject(execButton);
            so.FindProperty("_buttonBackground").objectReferenceValue = bg;
            so.FindProperty("_button").objectReferenceValue = btn;
            so.ApplyModifiedPropertiesWithoutUndo();

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

        // ============================================
        // Combat Scene Wiring Helpers
        // ============================================

        /// <summary>
        /// Wires VFX prefabs to the VFXPoolManager.
        /// </summary>
        private static void WireVFXPoolManager(VFXPoolManager vfxPoolManager)
        {
            if (vfxPoolManager == null) return;

            SerializedObject so = new SerializedObject(vfxPoolManager);
            var configsProp = so.FindProperty("_poolConfigs");

            // Define VFX pool configurations
            var vfxConfigs = new[]
            {
                ("hit_flame", "Assets/_Project/Prefabs/VFX/hit_flame.prefab", 5, 10),
                ("hit_shadow", "Assets/_Project/Prefabs/VFX/hit_shadow.prefab", 5, 10),
                ("hit_nature", "Assets/_Project/Prefabs/VFX/hit_nature.prefab", 5, 10),
                ("hit_arcane", "Assets/_Project/Prefabs/VFX/hit_arcane.prefab", 5, 10),
                ("hit_light", "Assets/_Project/Prefabs/VFX/hit_light.prefab", 5, 10),
                ("vfx_slash", "Assets/_Project/Prefabs/VFX/vfx_slash.prefab", 3, 5),
                ("vfx_shield", "Assets/_Project/Prefabs/VFX/vfx_shield.prefab", 2, 3),
                ("vfx_heal", "Assets/_Project/Prefabs/VFX/vfx_heal.prefab", 2, 3),
                ("vfx_buff", "Assets/_Project/Prefabs/VFX/vfx_buff.prefab", 2, 5),
                ("vfx_debuff", "Assets/_Project/Prefabs/VFX/vfx_debuff.prefab", 2, 5),
                ("vfx_corruption", "Assets/_Project/Prefabs/VFX/vfx_corruption.prefab", 3, 5),
                ("vfx_null_burst", "Assets/_Project/Prefabs/VFX/vfx_null_burst.prefab", 1, 2),
                ("vfx_requiem_art", "Assets/_Project/Prefabs/VFX/vfx_requiem_art.prefab", 1, 2),
                ("vfx_card_draw", "Assets/_Project/Prefabs/VFX/vfx_card_draw.prefab", 3, 5),
                ("vfx_card_play", "Assets/_Project/Prefabs/VFX/vfx_card_play.prefab", 3, 5),
            };

            int validCount = 0;
            foreach (var (id, path, preWarm, maxActive) in vfxConfigs)
            {
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (prefab != null) validCount++;
            }

            configsProp.arraySize = validCount;
            int index = 0;

            foreach (var (id, path, preWarm, maxActive) in vfxConfigs)
            {
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (prefab == null) continue;

                var element = configsProp.GetArrayElementAtIndex(index);
                element.FindPropertyRelative("EffectId").stringValue = id;
                element.FindPropertyRelative("Prefab").objectReferenceValue = prefab;
                element.FindPropertyRelative("PreWarmCount").intValue = preWarm;
                element.FindPropertyRelative("MaxActive").intValue = maxActive;
                index++;
            }

            so.ApplyModifiedPropertiesWithoutUndo();
            Debug.Log($"[ProductionSceneSetupGenerator] Wired VFXPoolManager with {validCount} VFX prefabs");
        }

        /// <summary>
        /// Wires CombatScreenCZN component references.
        /// </summary>
        private static void WireCombatScreenCZN(GameObject combatScreenObj)
        {
            if (combatScreenObj == null) return;

            var screen = combatScreenObj.GetComponent<CombatScreenCZN>();
            if (screen == null) return;

            SerializedObject so = new SerializedObject(screen);

            // Find and wire child components
            var vitalityBar = combatScreenObj.GetComponentInChildren<SharedVitalityBarCZN>(true);
            var partySidebar = combatScreenObj.GetComponentInChildren<PartyStatusSidebar>(true);
            var cardFan = combatScreenObj.GetComponentInChildren<CardFanLayout>(true);
            var apCounter = combatScreenObj.GetComponentInChildren<APCounterDisplay>(true);
            var execButton = combatScreenObj.GetComponentInChildren<ExecutionButton>(true);
            var sysMenu = combatScreenObj.GetComponentInChildren<SystemMenuBar>(true);

            // Wire if found
            if (vitalityBar != null)
                SetPropertyIfExists(so, "_vitalityBar", vitalityBar);
            if (partySidebar != null)
                SetPropertyIfExists(so, "_partySidebar", partySidebar);
            if (cardFan != null)
                SetPropertyIfExists(so, "_cardFanLayout", cardFan);
            if (apCounter != null)
                SetPropertyIfExists(so, "_apCounter", apCounter);
            if (execButton != null)
                SetPropertyIfExists(so, "_executionButton", execButton);
            if (sysMenu != null)
                SetPropertyIfExists(so, "_systemMenu", sysMenu);

            // Wire enemy/ally world-space UI containers
            var worldSpaceEnemyContainer = GameObject.Find("WorldSpaceEnemyUIContainer");
            var worldSpaceAllyContainer = GameObject.Find("WorldSpaceAllyIndicatorContainer");

            if (worldSpaceEnemyContainer != null)
                SetPropertyIfExists(so, "_enemyUIContainer", worldSpaceEnemyContainer.transform);
            else
                Debug.LogWarning("[ProductionSceneSetupGenerator] WorldSpaceEnemyUIContainer not found - regenerate Combat scene");

            if (worldSpaceAllyContainer != null)
                SetPropertyIfExists(so, "_allyIndicatorContainer", worldSpaceAllyContainer.transform);
            else
                Debug.LogWarning("[ProductionSceneSetupGenerator] WorldSpaceAllyIndicatorContainer not found - regenerate Combat scene");

            // Wire ally slots
            var allySlotsParent = GameObject.Find("--- ALLY SLOTS ---");
            if (allySlotsParent != null)
            {
                var allySlotsArray = so.FindProperty("_allySlots");
                if (allySlotsArray != null)
                {
                    int slotCount = allySlotsParent.transform.childCount;
                    allySlotsArray.arraySize = slotCount;
                    for (int i = 0; i < slotCount; i++)
                    {
                        var slot = allySlotsParent.transform.GetChild(i);
                        allySlotsArray.GetArrayElementAtIndex(i).objectReferenceValue = slot;
                    }
                    Debug.Log($"[ProductionSceneSetupGenerator] Wired {slotCount} ally slots to CombatScreenCZN");
                }
            }
            else
            {
                Debug.LogWarning("[ProductionSceneSetupGenerator] Ally slots not found - regenerate Combat scene");
            }

            // Wire Combat UI prefabs
            var enemyFloatingUIPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Project/Prefabs/UI/Combat/EnemyFloatingUI.prefab");
            var allyIndicatorPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Project/Prefabs/UI/Combat/AllyIndicator.prefab");

            if (enemyFloatingUIPrefab != null)
            {
                var floatingUI = enemyFloatingUIPrefab.GetComponent<EnemyFloatingUI>();
                if (floatingUI != null)
                    SetPropertyIfExists(so, "_enemyUIPrefab", floatingUI);
            }
            else
            {
                Debug.LogWarning("[ProductionSceneSetupGenerator] EnemyFloatingUI.prefab not found - run HNR > 2. Prefabs > UI > Combat UI (All) first");
            }

            if (allyIndicatorPrefab != null)
            {
                var indicator = allyIndicatorPrefab.GetComponent<AllyIndicator>();
                if (indicator != null)
                    SetPropertyIfExists(so, "_allyIndicatorPrefab", indicator);
            }
            else
            {
                Debug.LogWarning("[ProductionSceneSetupGenerator] AllyIndicator.prefab not found - run HNR > 2. Prefabs > UI > Combat UI (All) first");
            }

            so.ApplyModifiedPropertiesWithoutUndo();
            Debug.Log("[ProductionSceneSetupGenerator] Wired CombatScreenCZN component references");
        }

        // ============================================
        // CZN Layout Helper Methods
        // ============================================

        /// <summary>
        /// Creates the CZN-style shared vitality bar with embedded party portraits.
        /// </summary>
        private static GameObject CreateSharedVitalityBarCZN(GameObject parent)
        {
            GameObject barObj = new GameObject("SharedVitalityBarCZN");
            barObj.transform.SetParent(parent.transform, false);

            RectTransform rect = barObj.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0, 0.5f);
            rect.anchorMax = new Vector2(0.6f, 0.5f);
            rect.pivot = new Vector2(0, 0.5f);
            rect.anchoredPosition = new Vector2(12, 0);
            rect.sizeDelta = new Vector2(0, 32);

            var layout = barObj.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = 8f;
            layout.childAlignment = TextAnchor.MiddleLeft;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;
            layout.padding = new RectOffset(8, 8, 4, 4);

            // Party portraits container (overlapping circles)
            GameObject portraitsContainer = new GameObject("PartyPortraits");
            portraitsContainer.transform.SetParent(barObj.transform, false);
            var portraitsLayout = portraitsContainer.AddComponent<HorizontalLayoutGroup>();
            portraitsLayout.spacing = -8f; // Negative for overlap
            portraitsLayout.childAlignment = TextAnchor.MiddleCenter;

            for (int i = 0; i < 3; i++)
            {
                GameObject portrait = new GameObject($"Portrait_{i}");
                portrait.transform.SetParent(portraitsContainer.transform, false);
                RectTransform pRect = portrait.AddComponent<RectTransform>();
                pRect.sizeDelta = new Vector2(28, 28);
                Image pImg = portrait.AddComponent<Image>();
                pImg.color = new Color(0.3f + i * 0.15f, 0.2f, 0.4f - i * 0.1f);
                var pLayout = portrait.AddComponent<LayoutElement>();
                pLayout.preferredWidth = 28;
                pLayout.preferredHeight = 28;
            }

            // HP Bar container
            GameObject hpBarContainer = new GameObject("HPBarContainer");
            hpBarContainer.transform.SetParent(barObj.transform, false);
            var hpLayout = hpBarContainer.AddComponent<LayoutElement>();
            hpLayout.preferredWidth = 200;
            hpLayout.preferredHeight = 24;
            hpLayout.flexibleWidth = 1;

            RectTransform hpContainerRect = hpBarContainer.AddComponent<RectTransform>();

            // HP Bar background
            Image hpBg = hpBarContainer.AddComponent<Image>();
            hpBg.color = new Color(0, 0, 0, 0.7f);

            // HP Fill
            GameObject hpFill = new GameObject("HealthFill");
            hpFill.transform.SetParent(hpBarContainer.transform, false);
            RectTransform fillRect = hpFill.AddComponent<RectTransform>();
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = new Vector2(0.8f, 1); // 80% fill example
            fillRect.sizeDelta = Vector2.zero;
            Image fillImg = hpFill.AddComponent<Image>();
            fillImg.color = new Color(0.18f, 0.8f, 0.44f); // Health green #2ECC71

            // Damage linger fill (behind health)
            GameObject damageFill = new GameObject("DamageFill");
            damageFill.transform.SetParent(hpBarContainer.transform, false);
            damageFill.transform.SetAsFirstSibling();
            RectTransform dmgRect = damageFill.AddComponent<RectTransform>();
            dmgRect.anchorMin = Vector2.zero;
            dmgRect.anchorMax = new Vector2(0.85f, 1);
            dmgRect.sizeDelta = Vector2.zero;
            Image dmgImg = damageFill.AddComponent<Image>();
            dmgImg.color = new Color(1f, 0.27f, 0.27f); // Corruption glow #FF4444

            // HP Text
            GameObject hpText = CreateText(hpBarContainer, "HPText", "150 / 150", 11);
            RectTransform hpTextRect = hpText.GetComponent<RectTransform>();
            hpTextRect.anchorMin = Vector2.zero;
            hpTextRect.anchorMax = Vector2.one;
            hpTextRect.sizeDelta = Vector2.zero;
            var hpTmp = hpText.GetComponent<TextMeshProUGUI>();
            hpTmp.fontStyle = TMPro.FontStyles.Bold;

            // Block indicator container
            GameObject blockContainer = new GameObject("BlockContainer");
            blockContainer.transform.SetParent(barObj.transform, false);
            var blockLayout = blockContainer.AddComponent<HorizontalLayoutGroup>();
            blockLayout.spacing = 4f;
            blockLayout.childAlignment = TextAnchor.MiddleCenter;
            blockLayout.padding = new RectOffset(8, 8, 0, 0);

            var blockLayoutElement = blockContainer.AddComponent<LayoutElement>();
            blockLayoutElement.preferredWidth = 50;

            // Shield icon
            GameObject shieldIcon = new GameObject("ShieldIcon");
            shieldIcon.transform.SetParent(blockContainer.transform, false);
            RectTransform shieldRect = shieldIcon.AddComponent<RectTransform>();
            shieldRect.sizeDelta = new Vector2(16, 16);
            Image shieldImg = shieldIcon.AddComponent<Image>();
            shieldImg.color = new Color(0.2f, 0.6f, 0.86f); // Block blue #3498DB

            // Block text
            GameObject blockText = CreateText(blockContainer, "BlockText", "12", 12);
            blockText.GetComponent<TextMeshProUGUI>().color = new Color(0.2f, 0.6f, 0.86f);
            blockText.GetComponent<TextMeshProUGUI>().fontStyle = TMPro.FontStyles.Bold;

            // Add SharedVitalityBarCZN component
            var vitalityComponent = barObj.AddComponent<SharedVitalityBarCZN>();
            SerializedObject so = new SerializedObject(vitalityComponent);
            so.FindProperty("_healthFill").objectReferenceValue = fillImg;
            so.FindProperty("_damageFill").objectReferenceValue = dmgImg;
            so.FindProperty("_hpText").objectReferenceValue = hpTmp;
            so.FindProperty("_blockContainer").objectReferenceValue = blockContainer;
            so.FindProperty("_shieldIcon").objectReferenceValue = shieldImg;
            so.FindProperty("_blockText").objectReferenceValue = blockText.GetComponent<TMP_Text>();
            so.ApplyModifiedPropertiesWithoutUndo();

            blockContainer.SetActive(false); // Hidden by default

            return barObj;
        }

        /// <summary>
        /// Creates the system menu bar with speed/auto/settings buttons.
        /// </summary>
        private static GameObject CreateSystemMenuBar(GameObject parent)
        {
            GameObject menuBar = new GameObject("SystemMenuBar");
            menuBar.transform.SetParent(parent.transform, false);

            RectTransform rect = menuBar.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(1, 0.5f);
            rect.anchorMax = new Vector2(1, 0.5f);
            rect.pivot = new Vector2(1, 0.5f);
            rect.anchoredPosition = new Vector2(-12, 0);
            rect.sizeDelta = new Vector2(150, 28);

            var layout = menuBar.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = 8f;
            layout.childAlignment = TextAnchor.MiddleRight;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;
            layout.reverseArrangement = true;

            // Settings button
            CreateSystemMenuButton(menuBar, "SettingsBtn", "⚙", new Color(0.1f, 0.08f, 0.15f));

            // Auto button
            CreateSystemMenuButton(menuBar, "AutoBtn", "▶", new Color(0.1f, 0.08f, 0.15f));

            // Speed button
            GameObject speedBtn = CreateSystemMenuButton(menuBar, "SpeedBtn", "1.5x", new Color(0.1f, 0.08f, 0.15f));

            return menuBar;
        }

        private static GameObject CreateSystemMenuButton(GameObject parent, string name, string label, Color bgColor)
        {
            GameObject btn = new GameObject(name);
            btn.transform.SetParent(parent.transform, false);

            RectTransform rect = btn.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(28, 28);

            Image bg = btn.AddComponent<Image>();
            bg.color = bgColor;

            Button button = btn.AddComponent<Button>();
            button.targetGraphic = bg;

            var layoutElement = btn.AddComponent<LayoutElement>();
            layoutElement.preferredWidth = 28;
            layoutElement.preferredHeight = 28;

            GameObject text = CreateText(btn, "Label", label, 10);
            RectTransform textRect = text.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.sizeDelta = Vector2.zero;
            text.GetComponent<TextMeshProUGUI>().color = new Color(0f, 0.83f, 0.89f); // Soul cyan

            return btn;
        }

        /// <summary>
        /// Creates the CZN-style party status sidebar.
        /// </summary>
        private static GameObject CreatePartySidebarCZN(GameObject parent)
        {
            GameObject sidebar = new GameObject("PartySidebar");
            sidebar.transform.SetParent(parent.transform, false);

            RectTransform rect = sidebar.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0, 0.35f);
            rect.anchorMax = new Vector2(0.1f, 0.87f);
            rect.sizeDelta = Vector2.zero;

            Image bg = sidebar.AddComponent<Image>();
            bg.color = new Color(0, 0, 0, 0.5f);

            VerticalLayoutGroup layout = sidebar.AddComponent<VerticalLayoutGroup>();
            layout.spacing = 4f;
            layout.padding = new RectOffset(6, 6, 8, 8);
            layout.childAlignment = TextAnchor.UpperCenter;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;

            // Create 3 party member slots
            for (int i = 0; i < 3; i++)
            {
                CreatePartyMemberSlotCZN(sidebar, i);
            }

            // Add PartyStatusSidebar component if exists
            sidebar.AddComponent<PartyStatusSidebar>();

            return sidebar;
        }

        private static GameObject CreatePartyMemberSlotCZN(GameObject parent, int index)
        {
            GameObject slot = new GameObject($"PartySlot_{index}");
            slot.transform.SetParent(parent.transform, false);

            var layoutElement = slot.AddComponent<LayoutElement>();
            layoutElement.preferredHeight = 80;
            layoutElement.flexibleHeight = 1;

            VerticalLayoutGroup layout = slot.AddComponent<VerticalLayoutGroup>();
            layout.spacing = 2f;
            layout.childAlignment = TextAnchor.UpperCenter;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;
            layout.padding = new RectOffset(4, 4, 4, 4);

            Image slotBg = slot.AddComponent<Image>();
            slotBg.color = new Color(0.2f, 0.15f, 0.25f, 0.6f);

            // Hexagonal portrait frame
            GameObject portrait = new GameObject("Portrait");
            portrait.transform.SetParent(slot.transform, false);
            RectTransform portraitRect = portrait.AddComponent<RectTransform>();
            portraitRect.sizeDelta = new Vector2(44, 50);
            Image portraitImg = portrait.AddComponent<Image>();
            portraitImg.color = new Color(0.3f, 0.25f, 0.35f);
            var portraitLayout = portrait.AddComponent<LayoutElement>();
            portraitLayout.preferredWidth = 44;
            portraitLayout.preferredHeight = 50;

            // Name label
            GameObject nameLabel = CreateText(slot, "Name", $"Requiem {index + 1}", 8);
            nameLabel.GetComponent<TextMeshProUGUI>().fontStyle = TMPro.FontStyles.Bold;

            // SE Gauge
            GameObject seGauge = new GameObject("SEGauge");
            seGauge.transform.SetParent(slot.transform, false);
            RectTransform seRect = seGauge.AddComponent<RectTransform>();
            seRect.sizeDelta = new Vector2(0, 6);
            var seLayoutElement = seGauge.AddComponent<LayoutElement>();
            seLayoutElement.preferredHeight = 6;

            Image seBg = seGauge.AddComponent<Image>();
            seBg.color = new Color(0, 0, 0, 0.6f);

            GameObject seFill = new GameObject("SEFill");
            seFill.transform.SetParent(seGauge.transform, false);
            RectTransform seFillRect = seFill.AddComponent<RectTransform>();
            seFillRect.anchorMin = Vector2.zero;
            seFillRect.anchorMax = new Vector2(0.6f, 1);
            seFillRect.sizeDelta = Vector2.zero;
            Image seFillImg = seFill.AddComponent<Image>();
            seFillImg.color = new Color(0.83f, 0.69f, 0.22f); // Soul gold #D4AF37

            // SE Label
            GameObject seLabel = CreateText(slot, "SELabel", "SE 24/40", 7);
            seLabel.GetComponent<TextMeshProUGUI>().color = new Color(0.83f, 0.69f, 0.22f);

            return slot;
        }

        /// <summary>
        /// Creates the deck info sidebar (draw/discard counts).
        /// </summary>
        private static GameObject CreateDeckInfoSidebar(GameObject parent)
        {
            GameObject sidebar = new GameObject("DeckInfoSidebar");
            sidebar.transform.SetParent(parent.transform, false);

            RectTransform rect = sidebar.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.92f, 0.35f);
            rect.anchorMax = new Vector2(1f, 0.87f);
            rect.sizeDelta = Vector2.zero;

            VerticalLayoutGroup layout = sidebar.AddComponent<VerticalLayoutGroup>();
            layout.spacing = 12f;
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;
            layout.padding = new RectOffset(8, 8, 20, 20);

            // Draw pile
            CreateDeckPileDisplay(sidebar, "DrawPile", "📚", "23", "Draw", new Color(0f, 0.83f, 0.89f));

            // Discard pile
            CreateDeckPileDisplay(sidebar, "DiscardPile", "🔄", "0", "Discard", new Color(0.63f, 0.63f, 0.63f));

            return sidebar;
        }

        private static void CreateDeckPileDisplay(GameObject parent, string name, string icon, string count, string label, Color color)
        {
            GameObject pile = new GameObject(name);
            pile.transform.SetParent(parent.transform, false);

            VerticalLayoutGroup layout = pile.AddComponent<VerticalLayoutGroup>();
            layout.spacing = 2f;
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;

            // Icon
            GameObject iconObj = CreateText(pile, "Icon", icon, 18);

            // Count
            GameObject countObj = CreateText(pile, "Count", count, 14);
            countObj.GetComponent<TextMeshProUGUI>().color = color;
            countObj.GetComponent<TextMeshProUGUI>().fontStyle = TMPro.FontStyles.Bold;

            // Label
            GameObject labelObj = CreateText(pile, "Label", label, 8);
            labelObj.GetComponent<TextMeshProUGUI>().color = new Color(0.63f, 0.63f, 0.63f);
        }

        /// <summary>
        /// Creates the CZN-style AP counter display.
        /// </summary>
        private static GameObject CreateAPCounterCZN(GameObject parent)
        {
            GameObject counter = new GameObject("APCounter");
            counter.transform.SetParent(parent.transform, false);

            RectTransform rect = counter.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.7f);
            rect.anchorMax = new Vector2(0.5f, 1f);
            rect.sizeDelta = new Vector2(60, 50);

            // Glow background
            GameObject glow = new GameObject("Glow");
            glow.transform.SetParent(counter.transform, false);
            RectTransform glowRect = glow.AddComponent<RectTransform>();
            glowRect.anchorMin = Vector2.zero;
            glowRect.anchorMax = Vector2.one;
            glowRect.sizeDelta = new Vector2(20, 20);
            Image glowImg = glow.AddComponent<Image>();
            glowImg.color = new Color(0f, 0.83f, 0.89f, 0.3f); // Soul cyan glow

            // AP Number
            GameObject apNum = CreateText(counter, "APNumber", "3", 36);
            RectTransform numRect = apNum.GetComponent<RectTransform>();
            numRect.anchorMin = new Vector2(0.5f, 0.55f);
            numRect.anchorMax = new Vector2(0.5f, 0.55f);
            numRect.sizeDelta = new Vector2(50, 40);
            var apText = apNum.GetComponent<TMP_Text>();
            apText.color = new Color(0f, 0.83f, 0.89f); // Soul cyan
            apText.fontStyle = TMPro.FontStyles.Bold;

            // AP Label
            GameObject apLabel = CreateText(counter, "APLabel", "AP", 10);
            RectTransform labelRect = apLabel.GetComponent<RectTransform>();
            labelRect.anchorMin = new Vector2(0.5f, 0.25f);
            labelRect.anchorMax = new Vector2(0.5f, 0.25f);
            labelRect.sizeDelta = new Vector2(30, 15);
            apLabel.GetComponent<TextMeshProUGUI>().color = new Color(0.63f, 0.63f, 0.63f);

            // Add APCounterDisplay component and wire references
            var apCounter = counter.AddComponent<APCounterDisplay>();
            var so = new SerializedObject(apCounter);
            so.FindProperty("_apText").objectReferenceValue = apText;
            so.FindProperty("_glowBackground").objectReferenceValue = glowImg;
            so.ApplyModifiedPropertiesWithoutUndo();

            return counter;
        }

        /// <summary>
        /// Creates the CZN-style zone header for MapScreen.
        /// </summary>
        private static GameObject CreateZoneHeaderCZN(GameObject parent)
        {
            GameObject header = new GameObject("ZoneHeader");
            header.transform.SetParent(parent.transform, false);

            RectTransform headerRect = header.AddComponent<RectTransform>();
            headerRect.anchorMin = new Vector2(0, 0.84f);
            headerRect.anchorMax = new Vector2(1, 1);
            headerRect.sizeDelta = Vector2.zero;

            Image headerBg = header.AddComponent<Image>();
            headerBg.color = new Color(0.07f, 0.07f, 0.13f, 0.95f); // Abyss blue

            HorizontalLayoutGroup layout = header.AddComponent<HorizontalLayoutGroup>();
            layout.padding = new RectOffset(16, 16, 8, 8);
            layout.spacing = 8;
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = true;

            // Back button area
            GameObject backBtn = new GameObject("BackButton");
            backBtn.transform.SetParent(header.transform, false);
            Image backImg = backBtn.AddComponent<Image>();
            backImg.color = new Color(0.15f, 0.12f, 0.2f);
            Button backButton = backBtn.AddComponent<Button>();
            backButton.targetGraphic = backImg;
            var backLayout = backBtn.AddComponent<LayoutElement>();
            backLayout.preferredWidth = 32;
            backLayout.preferredHeight = 32;

            GameObject backIcon = CreateText(backBtn, "Icon", "<", 18);
            RectTransform backIconRect = backIcon.GetComponent<RectTransform>();
            backIconRect.anchorMin = Vector2.zero;
            backIconRect.anchorMax = Vector2.one;
            backIconRect.sizeDelta = Vector2.zero;
            backIcon.GetComponent<TMP_Text>().color = new Color(0.63f, 0.63f, 0.63f);

            // Title container (left side)
            GameObject titleContainer = new GameObject("TitleContainer");
            titleContainer.transform.SetParent(header.transform, false);
            VerticalLayoutGroup titleLayout = titleContainer.AddComponent<VerticalLayoutGroup>();
            titleLayout.childAlignment = TextAnchor.MiddleLeft;
            titleLayout.childForceExpandWidth = false;
            titleLayout.childForceExpandHeight = false;
            titleLayout.spacing = 0;
            var titleLayoutElement = titleContainer.AddComponent<LayoutElement>();
            titleLayoutElement.flexibleWidth = 1;

            // Zone Title
            GameObject zoneTitle = CreateText(titleContainer, "ZoneTitle", "NULL RIFT", 14);
            zoneTitle.GetComponent<TMP_Text>().fontStyle = TMPro.FontStyles.Bold;
            zoneTitle.GetComponent<TMP_Text>().color = new Color(0.9f, 0.9f, 0.95f);

            // Zone Subtitle
            GameObject zoneSubtitle = CreateText(titleContainer, "ZoneSubtitle", "Zone 1 • The Outer Reaches", 10);
            zoneSubtitle.GetComponent<TMP_Text>().color = new Color(0.42f, 0.25f, 0.63f); // Hollow violet

            // Stats container (right side)
            GameObject statsContainer = new GameObject("StatsContainer");
            statsContainer.transform.SetParent(header.transform, false);
            HorizontalLayoutGroup statsLayout = statsContainer.AddComponent<HorizontalLayoutGroup>();
            statsLayout.spacing = 16;
            statsLayout.childAlignment = TextAnchor.MiddleRight;
            statsLayout.childForceExpandWidth = false;
            statsLayout.childForceExpandHeight = false;

            // HP Container
            GameObject hpContainer = new GameObject("HPContainer");
            hpContainer.transform.SetParent(statsContainer.transform, false);
            HorizontalLayoutGroup hpLayout = hpContainer.AddComponent<HorizontalLayoutGroup>();
            hpLayout.spacing = 4;
            hpLayout.childAlignment = TextAnchor.MiddleCenter;

            GameObject hpIcon = new GameObject("HPIcon");
            hpIcon.transform.SetParent(hpContainer.transform, false);
            RectTransform hpIconRect = hpIcon.AddComponent<RectTransform>();
            hpIconRect.sizeDelta = new Vector2(16, 16);
            Image hpIconImg = hpIcon.AddComponent<Image>();
            hpIconImg.color = new Color(0.18f, 0.8f, 0.44f); // Health green
            var hpIconLayout = hpIcon.AddComponent<LayoutElement>();
            hpIconLayout.preferredWidth = 16;
            hpIconLayout.preferredHeight = 16;

            GameObject hpText = CreateText(hpContainer, "HPText", "210/210", 12);
            hpText.GetComponent<TMP_Text>().fontStyle = TMPro.FontStyles.Bold;

            // Currency Container
            GameObject currencyContainer = new GameObject("CurrencyContainer");
            currencyContainer.transform.SetParent(statsContainer.transform, false);
            HorizontalLayoutGroup currencyLayout = currencyContainer.AddComponent<HorizontalLayoutGroup>();
            currencyLayout.spacing = 4;
            currencyLayout.childAlignment = TextAnchor.MiddleCenter;

            GameObject currencyIcon = new GameObject("CurrencyIcon");
            currencyIcon.transform.SetParent(currencyContainer.transform, false);
            RectTransform currIconRect = currencyIcon.AddComponent<RectTransform>();
            currIconRect.sizeDelta = new Vector2(16, 16);
            Image currIconImg = currencyIcon.AddComponent<Image>();
            currIconImg.color = new Color(0f, 0.83f, 0.89f); // Soul cyan
            var currIconLayout = currencyIcon.AddComponent<LayoutElement>();
            currIconLayout.preferredWidth = 16;
            currIconLayout.preferredHeight = 16;

            GameObject currencyText = CreateText(currencyContainer, "CurrencyText", "45", 12);
            currencyText.GetComponent<TMP_Text>().fontStyle = TMPro.FontStyles.Bold;

            return header;
        }

        /// <summary>
        /// Creates the node type legend for the map footer.
        /// </summary>
        private static GameObject CreateMapLegendCZN(GameObject parent)
        {
            GameObject legend = new GameObject("MapLegend");
            legend.transform.SetParent(parent.transform, false);

            RectTransform legendRect = legend.AddComponent<RectTransform>();
            legendRect.anchorMin = new Vector2(0, 0);
            legendRect.anchorMax = new Vector2(1, 0.08f);
            legendRect.sizeDelta = Vector2.zero;

            Image legendBg = legend.AddComponent<Image>();
            legendBg.color = new Color(0.07f, 0.07f, 0.13f, 0.95f);

            HorizontalLayoutGroup layout = legend.AddComponent<HorizontalLayoutGroup>();
            layout.padding = new RectOffset(12, 12, 4, 4);
            layout.spacing = 16;
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = true;

            // Node type legend items
            var nodeTypes = new (string icon, string label, Color color)[]
            {
                ("⚔️", "Combat", new Color(0.77f, 0.12f, 0.23f)),
                ("💀", "Elite", new Color(1f, 0.27f, 0.27f)),
                ("🛒", "Shop", new Color(0.83f, 0.69f, 0.22f)),
                ("❓", "Echo", new Color(0.42f, 0.25f, 0.63f)),
                ("🕯️", "Sanctuary", new Color(0.18f, 0.8f, 0.44f)),
                ("💎", "Treasure", new Color(0.83f, 0.69f, 0.22f)),
                ("👹", "Boss", new Color(1f, 0.27f, 0.27f)),
            };

            foreach (var (icon, label, color) in nodeTypes)
            {
                CreateLegendItem(legend, icon, label, color);
            }

            return legend;
        }

        private static void CreateLegendItem(GameObject parent, string icon, string label, Color color)
        {
            GameObject item = new GameObject($"Legend_{label}");
            item.transform.SetParent(parent.transform, false);

            HorizontalLayoutGroup layout = item.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = 3;
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;

            GameObject iconObj = CreateText(item, "Icon", icon, 12);
            GameObject labelObj = CreateText(item, "Label", label, 8);
            labelObj.GetComponent<TMP_Text>().color = new Color(0.63f, 0.63f, 0.63f);
        }

        /// <summary>
        /// Creates the CZN-style circular execution (end turn) button.
        /// </summary>
        private static GameObject CreateExecutionButtonCZN(GameObject parent)
        {
            GameObject btnObj = new GameObject("ExecutionButton");
            btnObj.transform.SetParent(parent.transform, false);

            RectTransform rect = btnObj.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.9f, 0.3f);
            rect.anchorMax = new Vector2(0.95f, 0.8f);
            rect.sizeDelta = Vector2.zero;

            // Glow ring
            GameObject glowRing = new GameObject("GlowRing");
            glowRing.transform.SetParent(btnObj.transform, false);
            RectTransform glowRect = glowRing.AddComponent<RectTransform>();
            glowRect.anchorMin = Vector2.zero;
            glowRect.anchorMax = Vector2.one;
            glowRect.sizeDelta = new Vector2(10, 10);
            Image glowImg = glowRing.AddComponent<Image>();
            glowImg.color = new Color(0f, 0.83f, 0.89f, 0.4f); // Cyan glow

            // Main button circle
            GameObject circle = new GameObject("Circle");
            circle.transform.SetParent(btnObj.transform, false);
            RectTransform circleRect = circle.AddComponent<RectTransform>();
            circleRect.anchorMin = new Vector2(0.1f, 0.1f);
            circleRect.anchorMax = new Vector2(0.9f, 0.9f);
            circleRect.sizeDelta = Vector2.zero;

            Image bg = circle.AddComponent<Image>();
            bg.color = new Color(0f, 0.83f, 0.89f); // Soul cyan

            Button btn = circle.AddComponent<Button>();
            btn.targetGraphic = bg;

            // Checkmark icon
            GameObject checkObj = CreateText(circle, "Check", "✓", 28);
            RectTransform checkRect = checkObj.GetComponent<RectTransform>();
            checkRect.anchorMin = Vector2.zero;
            checkRect.anchorMax = Vector2.one;
            checkRect.sizeDelta = Vector2.zero;
            checkObj.GetComponent<TextMeshProUGUI>().color = new Color(0.04f, 0.04f, 0.04f); // Void black

            // Add ExecutionButton component and wire references
            var execButton = btnObj.AddComponent<ExecutionButton>();
            var so = new SerializedObject(execButton);
            so.FindProperty("_buttonBackground").objectReferenceValue = bg;
            so.FindProperty("_button").objectReferenceValue = btn;
            so.ApplyModifiedPropertiesWithoutUndo();

            return btnObj;
        }

        private static void SetPropertyIfExists(SerializedObject so, string propertyName, Object value)
        {
            var prop = so.FindProperty(propertyName);
            if (prop != null && value != null)
            {
                prop.objectReferenceValue = value;
            }
        }
    }
}
