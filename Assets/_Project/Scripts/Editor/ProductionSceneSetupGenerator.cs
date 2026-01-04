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
using HNR.Audio;
using HNR.Progression;
using HNR.UI;
using HNR.UI.Screens;
using HNR.UI.Combat;
using HNR.UI.Components;
using HNR.UI.Config;
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
        private const string ICON_CONFIG_PATH = "Assets/_Project/Data/Config/SceneIconConfig.asset";

        // Cached icon config for current generation run
        private static SceneIconConfigSO _iconConfig;

        /// <summary>
        /// Loads the scene icon configuration asset.
        /// </summary>
        /// <returns>The icon config or null if not found</returns>
        private static SceneIconConfigSO LoadIconConfig()
        {
            if (_iconConfig == null)
            {
                _iconConfig = AssetDatabase.LoadAssetAtPath<SceneIconConfigSO>(ICON_CONFIG_PATH);
                if (_iconConfig == null)
                {
                    Debug.LogWarning("[ProductionSceneSetupGenerator] SceneIconConfig not found. Run 'HNR > 2. Prefabs > Icons > Generate Scene Icons' first. Using text fallback.");
                }
            }
            return _iconConfig;
        }

        /// <summary>
        /// Clears the cached icon config (call after scene generation batch completes).
        /// </summary>
        private static void ClearIconConfigCache()
        {
            _iconConfig = null;
        }

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
                "- Missions\n" +
                "- BattleMission\n" +
                "- Requiems\n" +
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
            SetupMissionsScene();
            SetupBattleMissionScene();
            SetupRequiemsScene();
            SetupNullRiftScene();
            SetupCombatScene();
            UpdateBuildSettings();

            EditorUtility.DisplayDialog("Production Scenes Setup Complete",
                "All 8 production scenes have been created and configured.\n\n" +
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

            // === AudioManager with AudioConfigSO (must be root for DontDestroyOnLoad) ===
            // AudioManager uses DontDestroyOnLoad and must be at root level
            GameObject audioManagerObj = new GameObject("[AudioManager]");
            var audioManager = audioManagerObj.AddComponent<AudioManager>();
            WireAudioManager(audioManager);

            // === Bootstrap (creates remaining managers) ===
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

            // === Settings Overlay ===
            CreateSettingsOverlay(canvasObj.transform);

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
            var echoEventManager = echoEventObj.AddComponent<EchoEventManager>();

            // Wire available Echo events to EchoEventManager
            WireEchoEvents(echoEventManager);

            // NodeEventHandler for handling Sanctuary/Shop events
            GameObject nodeEventHandlerObj = new GameObject("NodeEventHandler");
            nodeEventHandlerObj.transform.SetParent(managersParent.transform);
            var nodeEventHandler = nodeEventHandlerObj.AddComponent<NodeEventHandler>();

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

            // === ConfirmationDialog (overlay) ===
            GameObject confirmationDialog = CreateConfirmationDialog(overlayContainer);

            // === DeckViewerModal (overlay for card removal) ===
            GameObject deckViewerModal = CreateDeckViewerModal(overlayContainer);

            // Wire DeckViewerModal to NodeEventHandler
            SerializedObject nodeEventSo = new SerializedObject(nodeEventHandler);
            nodeEventSo.FindProperty("_deckViewerModal").objectReferenceValue = deckViewerModal.GetComponent<DeckViewerModal>();
            nodeEventSo.ApplyModifiedPropertiesWithoutUndo();

            // Wire DeckViewerModal to EchoEventScreen for card upgrade selection
            var echoEventScreen = echoScreen.GetComponent<EchoEventScreen>();
            if (echoEventScreen != null)
            {
                var echoEventSo = new SerializedObject(echoEventScreen);
                echoEventSo.FindProperty("_deckViewerModal").objectReferenceValue = deckViewerModal.GetComponent<DeckViewerModal>();
                echoEventSo.ApplyModifiedPropertiesWithoutUndo();
                Debug.Log("[ProductionSceneSetupGenerator] Wired DeckViewerModal to EchoEventScreen");
            }

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

            // === Combat VFX Controller (event-driven VFX spawning) ===
            GameObject combatVFXControllerObj = new GameObject("CombatVFXController");
            combatVFXControllerObj.transform.SetParent(managersParent.transform);
            combatVFXControllerObj.AddComponent<CombatVFXController>();

            // === Combat Audio Controller (event-driven audio) ===
            GameObject combatAudioControllerObj = new GameObject("CombatAudioController");
            combatAudioControllerObj.transform.SetParent(managersParent.transform);
            combatAudioControllerObj.AddComponent<CombatAudioController>();

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

            // === CombatScreen ===
            GameObject combatScreen = CreateCombatScreen(screenContainer);

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

            // === Wire CombatScreen components ===
            WireCombatScreen(combatScreen);

            // === Overlay Container (for modals/dialogs) ===
            GameObject overlayContainer = CreateUIContainer(canvasObj, "OverlayContainer");
            overlayContainer.transform.SetAsLastSibling(); // Ensure overlays render on top

            // === PauseMenuOverlay ===
            GameObject pauseMenuOverlay = CreatePauseMenuOverlay(overlayContainer);

            // === ConfirmationDialog ===
            GameObject confirmationDialog = CreateConfirmationDialog(overlayContainer);

            // === NullStateModal ===
            GameObject nullStateModal = CreateNullStateModal(overlayContainer);

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

            // All 8 production scenes in correct game flow order:
            // Boot -> MainMenu -> Bastion -> Missions -> BattleMission -> NullRift <-> Combat
            // Requiems is accessible from Bastion as a character viewer
            string[] sceneOrder = new string[]
            {
                $"{SCENES_PATH}/Boot.unity",           // 0: Entry point, creates all managers
                $"{SCENES_PATH}/MainMenu.unity",       // 1: Main menu
                $"{SCENES_PATH}/Bastion.unity",        // 2: Hub area
                $"{SCENES_PATH}/Missions.unity",       // 3: Mission type selection (Story/Battle)
                $"{SCENES_PATH}/BattleMission.unity",  // 4: Zone/difficulty selection
                $"{SCENES_PATH}/Requiems.unity",       // 5: Character viewer (from Bastion)
                $"{SCENES_PATH}/NullRift.unity",       // 6: Map exploration (run state)
                $"{SCENES_PATH}/Combat.unity"          // 7: Combat encounters
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

        private static void WireEchoEvents(EchoEventManager echoEventManager)
        {
            // Try to load Echo events from Resources folder (where EchoEventGenerator creates them)
            var events = AssetDatabase.FindAssets("t:EchoEventDataSO", new[] { "Assets/_Project/Resources/Data/Events" });

            // Also try the regular Data folder
            if (events == null || events.Length == 0)
            {
                events = AssetDatabase.FindAssets("t:EchoEventDataSO", new[] { "Assets/_Project/Data/Events" });
            }

            if (events == null || events.Length == 0)
            {
                Debug.LogWarning("[ProductionSceneSetupGenerator] No Echo events found. Run HNR > Events > Echo Events to generate them.");
                return;
            }

            SerializedObject so = new SerializedObject(echoEventManager);
            var availableEventsProp = so.FindProperty("_availableEvents");
            availableEventsProp.arraySize = events.Length;

            for (int i = 0; i < events.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(events[i]);
                var eventAsset = AssetDatabase.LoadAssetAtPath<EchoEventDataSO>(path);
                availableEventsProp.GetArrayElementAtIndex(i).objectReferenceValue = eventAsset;
            }

            so.ApplyModifiedPropertiesWithoutUndo();

            Debug.Log($"[ProductionSceneSetupGenerator] Wired {events.Length} Echo events to EchoEventManager");
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

            // ============================================
            // Player Info (Top Left)
            // ============================================
            GameObject playerInfoContainer = new GameObject("PlayerInfo");
            playerInfoContainer.transform.SetParent(screenObj.transform, false);
            RectTransform playerInfoRect = playerInfoContainer.AddComponent<RectTransform>();
            playerInfoRect.anchorMin = new Vector2(0, 0.85f);
            playerInfoRect.anchorMax = new Vector2(0.3f, 1f);
            playerInfoRect.offsetMin = new Vector2(20, 10);
            playerInfoRect.offsetMax = new Vector2(-10, -10);

            HorizontalLayoutGroup playerInfoHLG = playerInfoContainer.AddComponent<HorizontalLayoutGroup>();
            playerInfoHLG.spacing = 15;
            playerInfoHLG.childAlignment = TextAnchor.MiddleLeft;
            playerInfoHLG.childForceExpandWidth = false;
            playerInfoHLG.childForceExpandHeight = false;

            // Level badge
            GameObject levelBadge = new GameObject("LevelBadge");
            levelBadge.transform.SetParent(playerInfoContainer.transform, false);
            levelBadge.AddComponent<RectTransform>();
            LayoutElement levelBadgeLE = levelBadge.AddComponent<LayoutElement>();
            levelBadgeLE.preferredWidth = 60;
            levelBadgeLE.preferredHeight = 60;
            Image levelBadgeImage = levelBadge.AddComponent<Image>();
            levelBadgeImage.color = new Color(0.3f, 0.4f, 0.5f, 0.9f);

            GameObject levelTextObj = CreateText(levelBadge, "LevelText", "LV\n1", 18);
            RectTransform levelTextRect = levelTextObj.GetComponent<RectTransform>();
            levelTextRect.anchorMin = Vector2.zero;
            levelTextRect.anchorMax = Vector2.one;
            levelTextRect.sizeDelta = Vector2.zero;
            TextMeshProUGUI levelTMP = levelTextObj.GetComponent<TextMeshProUGUI>();
            levelTMP.alignment = TextAlignmentOptions.Center;
            levelTMP.fontStyle = FontStyles.Bold;

            // Nickname
            GameObject nicknameObj = CreateText(playerInfoContainer, "Nickname", "Commander", 24);
            LayoutElement nicknameLE = nicknameObj.AddComponent<LayoutElement>();
            nicknameLE.preferredWidth = 150;
            nicknameLE.preferredHeight = 50;
            TextMeshProUGUI nicknameText = nicknameObj.GetComponent<TextMeshProUGUI>();
            nicknameText.fontStyle = FontStyles.Bold;
            nicknameText.alignment = TextAlignmentOptions.Left;

            // ============================================
            // Settings Button (Top Right)
            // ============================================
            GameObject settingsButton = CreateMenuButton(screenObj, "SettingsButton", "≡");
            RectTransform settingsRect = settingsButton.GetComponent<RectTransform>();
            settingsRect.anchorMin = new Vector2(1, 1);
            settingsRect.anchorMax = new Vector2(1, 1);
            settingsRect.pivot = new Vector2(1, 1);
            settingsRect.sizeDelta = new Vector2(60, 60);
            settingsRect.anchoredPosition = new Vector2(-20, -20);
            settingsButton.GetComponentInChildren<TextMeshProUGUI>().fontSize = 32;

            // ============================================
            // Navigation Buttons (Right Side - Vertical Stack)
            // ============================================
            GameObject navContainer = new GameObject("NavigationButtons");
            navContainer.transform.SetParent(screenObj.transform, false);
            RectTransform navRect = navContainer.AddComponent<RectTransform>();
            // Position on right side like reference image
            navRect.anchorMin = new Vector2(0.65f, 0.25f);
            navRect.anchorMax = new Vector2(0.98f, 0.80f);
            navRect.offsetMin = Vector2.zero;
            navRect.offsetMax = Vector2.zero;

            VerticalLayoutGroup navVLG = navContainer.AddComponent<VerticalLayoutGroup>();
            navVLG.spacing = 15;
            navVLG.childAlignment = TextAnchor.UpperRight;
            navVLG.childForceExpandWidth = true;
            navVLG.childForceExpandHeight = false;
            navVLG.padding = new RectOffset(0, 0, 10, 10);

            // Missions button (wide horizontal, no subtitle)
            GameObject missionsButton = CreateWideNavButton(navContainer, "MissionsButton", "Missions");
            LayoutElement missionsLE = missionsButton.AddComponent<LayoutElement>();
            missionsLE.preferredHeight = 70;

            // Requiems button (wide horizontal, no subtitle)
            GameObject requiemsButton = CreateWideNavButton(navContainer, "RequiemsButton", "Requiems");
            LayoutElement requiemsLE = requiemsButton.AddComponent<LayoutElement>();
            requiemsLE.preferredHeight = 70;

            // ============================================
            // Wire References
            // ============================================
            SerializedObject screenSO = new SerializedObject(bastionScreen);
            screenSO.FindProperty("_playerLevelText").objectReferenceValue = levelTMP;
            screenSO.FindProperty("_playerNicknameText").objectReferenceValue = nicknameText;
            screenSO.FindProperty("_settingsButton").objectReferenceValue = settingsButton.GetComponent<Button>();
            screenSO.FindProperty("_missionsButton").objectReferenceValue = missionsButton.GetComponent<Button>();
            screenSO.FindProperty("_missionsButtonText").objectReferenceValue = missionsButton.transform.Find("ButtonText")?.GetComponent<TextMeshProUGUI>();
            screenSO.FindProperty("_requiemsButton").objectReferenceValue = requiemsButton.GetComponent<Button>();
            screenSO.FindProperty("_requiemsButtonText").objectReferenceValue = requiemsButton.transform.Find("ButtonText")?.GetComponent<TextMeshProUGUI>();
            screenSO.FindProperty("_showGlobalHeader").boolValue = false;
            screenSO.FindProperty("_showGlobalNav").boolValue = false;
            screenSO.ApplyModifiedPropertiesWithoutUndo();

            return screenObj;
        }

        /// <summary>
        /// Creates a wide navigation button (horizontal, single-line title, no subtitle).
        /// Matches the reference design from BastionSceneDesignReference.jpg
        /// </summary>
        private static GameObject CreateWideNavButton(GameObject parent, string name, string title)
        {
            GameObject buttonObj = new GameObject(name);
            buttonObj.transform.SetParent(parent.transform, false);
            buttonObj.AddComponent<RectTransform>();

            Image image = buttonObj.AddComponent<Image>();
            image.color = new Color(0.15f, 0.15f, 0.2f, 0.85f);

            Button button = buttonObj.AddComponent<Button>();
            button.targetGraphic = image;

            // Button text (centered, single line)
            GameObject textObj = CreateText(buttonObj, "ButtonText", title, 22);
            RectTransform textRect = textObj.GetComponent<RectTransform>();
            textRect.anchorMin = new Vector2(0, 0);
            textRect.anchorMax = new Vector2(0.85f, 1);
            textRect.offsetMin = new Vector2(20, 0);
            textRect.offsetMax = new Vector2(-10, 0);
            TextMeshProUGUI buttonText = textObj.GetComponent<TextMeshProUGUI>();
            buttonText.fontStyle = FontStyles.Normal;
            buttonText.alignment = TextAlignmentOptions.Left;

            // Icon placeholder on right side
            GameObject iconObj = new GameObject("Icon");
            iconObj.transform.SetParent(buttonObj.transform, false);
            RectTransform iconRect = iconObj.AddComponent<RectTransform>();
            iconRect.anchorMin = new Vector2(0.85f, 0.2f);
            iconRect.anchorMax = new Vector2(0.95f, 0.8f);
            iconRect.offsetMin = Vector2.zero;
            iconRect.offsetMax = Vector2.zero;
            Image iconImage = iconObj.AddComponent<Image>();
            iconImage.color = new Color(0.6f, 0.6f, 0.7f, 0.5f);

            return buttonObj;
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
            // ZONE HEADER
            // ============================================
            GameObject zoneHeader = CreateZoneHeader(screenObj);

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
            GameObject legend = CreateMapLegend(screenObj);

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
            var hpDisplay = zoneHeader.transform.Find("StatsContainer/HPContainer/HPDisplay")?.GetComponent<AnimatedStatDisplay>();
            var currencyDisplay = zoneHeader.transform.Find("StatsContainer/CurrencyContainer/CurrencyDisplay")?.GetComponent<AnimatedStatDisplay>();

            if (zoneTitle != null) so.FindProperty("_zoneTitle").objectReferenceValue = zoneTitle;
            if (zoneSubtitle != null) so.FindProperty("_zoneSubtitle").objectReferenceValue = zoneSubtitle;
            if (hpDisplay != null) so.FindProperty("_hpDisplay").objectReferenceValue = hpDisplay;
            if (currencyDisplay != null) so.FindProperty("_currencyDisplay").objectReferenceValue = currencyDisplay;

            // Wire back button
            var backButton = zoneHeader.transform.Find("BackButton")?.GetComponent<Button>();
            if (backButton != null) so.FindProperty("_backButton").objectReferenceValue = backButton;

            so.ApplyModifiedPropertiesWithoutUndo();

            Debug.Log("[ProductionSceneSetupGenerator] Created MapScreen with zone header");

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

            // === Illustration Panel (Left 200px per mockup) ===
            GameObject illustrationPanel = new GameObject("IllustrationPanel");
            illustrationPanel.transform.SetParent(panel.transform, false);
            RectTransform illustRect = illustrationPanel.AddComponent<RectTransform>();
            illustRect.anchorMin = new Vector2(0, 0.1f);
            illustRect.anchorMax = new Vector2(0, 0.9f);
            illustRect.pivot = new Vector2(0, 0.5f);
            illustRect.anchoredPosition = new Vector2(20, 0);
            illustRect.sizeDelta = new Vector2(200, 0);

            Image illustBg = illustrationPanel.AddComponent<Image>();
            illustBg.color = new Color(0.15f, 0.12f, 0.2f, 0.9f);

            // Illustration icon placeholder
            GameObject illustIcon = new GameObject("IllustrationIcon");
            illustIcon.transform.SetParent(illustrationPanel.transform, false);
            RectTransform iconRect = illustIcon.AddComponent<RectTransform>();
            iconRect.anchorMin = new Vector2(0.1f, 0.2f);
            iconRect.anchorMax = new Vector2(0.9f, 0.8f);
            iconRect.sizeDelta = Vector2.zero;
            Image bgImage = illustIcon.AddComponent<Image>();
            bgImage.color = new Color(0.42f, 0.25f, 0.63f); // Hollow Violet for echo events

            // === Content Panel (Right side) ===
            GameObject contentPanel = new GameObject("ContentPanel");
            contentPanel.transform.SetParent(panel.transform, false);
            RectTransform contentRect = contentPanel.AddComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0, 0);
            contentRect.anchorMax = new Vector2(1, 1);
            contentRect.offsetMin = new Vector2(240, 20); // Leave space for illustration
            contentRect.offsetMax = new Vector2(-20, -20);

            // === Event Title (Soul Gold per mockup) ===
            GameObject titleObj = CreateText(contentPanel, "EventTitle", "ECHO OF THE VOID", 28);
            RectTransform titleRect = titleObj.GetComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0, 0.88f);
            titleRect.anchorMax = new Vector2(1, 0.98f);
            titleRect.sizeDelta = Vector2.zero;
            var titleTmp = titleObj.GetComponent<TextMeshProUGUI>();
            titleTmp.color = new Color(0.83f, 0.69f, 0.22f); // Soul Gold #D4AF37
            titleTmp.fontStyle = TMPro.FontStyles.Bold;
            titleTmp.characterSpacing = 4f; // Letter spacing per mockup
            titleTmp.alignment = TextAlignmentOptions.Left;

            // === Event Description / Narrative ===
            GameObject descObj = CreateText(contentPanel, "NarrativeText", "Event narrative text goes here...", 18);
            RectTransform descRect = descObj.GetComponent<RectTransform>();
            descRect.anchorMin = new Vector2(0, 0.45f);
            descRect.anchorMax = new Vector2(1, 0.85f);
            descRect.sizeDelta = Vector2.zero;
            var narrativeTmp = descObj.GetComponent<TextMeshProUGUI>();
            narrativeTmp.alignment = TextAlignmentOptions.TopLeft;
            narrativeTmp.color = new Color(0.85f, 0.85f, 0.85f);

            // === Choice Container ===
            GameObject choiceContainer = new GameObject("ChoiceContainer");
            choiceContainer.transform.SetParent(contentPanel.transform, false);
            RectTransform choiceRect = choiceContainer.AddComponent<RectTransform>();
            choiceRect.anchorMin = new Vector2(0, 0.05f);
            choiceRect.anchorMax = new Vector2(1, 0.42f);
            choiceRect.sizeDelta = Vector2.zero;

            VerticalLayoutGroup choiceLayout = choiceContainer.AddComponent<VerticalLayoutGroup>();
            choiceLayout.spacing = 8f;
            choiceLayout.childAlignment = TextAnchor.UpperLeft;
            choiceLayout.childForceExpandWidth = true;
            choiceLayout.childForceExpandHeight = false;
            choiceLayout.padding = new RectOffset(0, 0, 5, 5);

            // === Outcome Panel (hidden by default) ===
            GameObject outcomePanel = new GameObject("OutcomePanel");
            outcomePanel.transform.SetParent(panel.transform, false);
            RectTransform outcomeRect = outcomePanel.AddComponent<RectTransform>();
            outcomeRect.anchorMin = new Vector2(0.1f, 0.1f);
            outcomeRect.anchorMax = new Vector2(0.9f, 0.9f);
            outcomeRect.sizeDelta = Vector2.zero;

            Image outcomeBg = outcomePanel.AddComponent<Image>();
            outcomeBg.color = new Color(0.08f, 0.06f, 0.12f, 0.98f);

            // OutcomeText - positioned at top of outcome panel
            GameObject outcomeText = CreateText(outcomePanel, "OutcomeText", "Outcome text...", 20);
            RectTransform outcomeTextRect = outcomeText.GetComponent<RectTransform>();
            outcomeTextRect.anchorMin = new Vector2(0.1f, 0.65f);
            outcomeTextRect.anchorMax = new Vector2(0.9f, 0.90f);
            outcomeTextRect.sizeDelta = Vector2.zero;
            var outcomeTmp = outcomeText.GetComponent<TextMeshProUGUI>();
            outcomeTmp.alignment = TextAlignmentOptions.Center;

            // OutcomeCardContainer - displays card received from event choice
            GameObject outcomeCardContainer = new GameObject("OutcomeCardContainer");
            outcomeCardContainer.transform.SetParent(outcomePanel.transform, false);
            RectTransform cardContainerRect = outcomeCardContainer.AddComponent<RectTransform>();
            cardContainerRect.anchorMin = new Vector2(0.5f, 0.25f);
            cardContainerRect.anchorMax = new Vector2(0.5f, 0.60f);
            cardContainerRect.pivot = new Vector2(0.5f, 0.5f);
            cardContainerRect.sizeDelta = new Vector2(220, 0); // Width for card, height stretches with anchors

            // Add HorizontalLayoutGroup for potential multiple cards
            var cardContainerLayout = outcomeCardContainer.AddComponent<HorizontalLayoutGroup>();
            cardContainerLayout.childAlignment = TextAnchor.MiddleCenter;
            cardContainerLayout.spacing = 10f;
            cardContainerLayout.childControlWidth = false;
            cardContainerLayout.childControlHeight = false;
            cardContainerLayout.childForceExpandWidth = false;
            cardContainerLayout.childForceExpandHeight = false;

            // Continue button in outcome panel - positioned at bottom
            GameObject continueBtn = new GameObject("ContinueButton");
            continueBtn.transform.SetParent(outcomePanel.transform, false);
            RectTransform continueBtnRect = continueBtn.AddComponent<RectTransform>();
            continueBtnRect.anchorMin = new Vector2(0.5f, 0.08f);
            continueBtnRect.anchorMax = new Vector2(0.5f, 0.08f);
            continueBtnRect.sizeDelta = new Vector2(140, 45);

            Image continueBtnImg = continueBtn.AddComponent<Image>();
            continueBtnImg.color = new Color(0.18f, 0.8f, 0.44f); // Green for continue

            Button continueBtnComponent = continueBtn.AddComponent<Button>();
            continueBtnComponent.targetGraphic = continueBtnImg;

            GameObject continueBtnText = CreateText(continueBtn, "Text", "CONTINUE", 16);
            RectTransform continueBtnTextRect = continueBtnText.GetComponent<RectTransform>();
            continueBtnTextRect.anchorMin = Vector2.zero;
            continueBtnTextRect.anchorMax = Vector2.one;
            continueBtnTextRect.sizeDelta = Vector2.zero;
            continueBtnText.GetComponent<TextMeshProUGUI>().fontStyle = TMPro.FontStyles.Bold;

            outcomePanel.SetActive(false);

            // === Skip Button (for empty events or when no choices) ===
            GameObject skipBtn = new GameObject("SkipButton");
            skipBtn.transform.SetParent(panel.transform, false);
            RectTransform skipBtnRect = skipBtn.AddComponent<RectTransform>();
            skipBtnRect.anchorMin = new Vector2(0.5f, 0.08f);
            skipBtnRect.anchorMax = new Vector2(0.5f, 0.08f);
            skipBtnRect.sizeDelta = new Vector2(160, 50);

            Image skipBtnImg = skipBtn.AddComponent<Image>();
            skipBtnImg.color = new Color(0.4f, 0.35f, 0.5f); // Muted purple

            Button skipBtnComponent = skipBtn.AddComponent<Button>();
            skipBtnComponent.targetGraphic = skipBtnImg;

            GameObject skipBtnText = CreateText(skipBtn, "Text", "CONTINUE", 16);
            RectTransform skipBtnTextRect = skipBtnText.GetComponent<RectTransform>();
            skipBtnTextRect.anchorMin = Vector2.zero;
            skipBtnTextRect.anchorMax = Vector2.one;
            skipBtnTextRect.sizeDelta = Vector2.zero;
            skipBtnText.GetComponent<TextMeshProUGUI>().fontStyle = TMPro.FontStyles.Bold;

            skipBtn.SetActive(false); // Hidden by default, shown when no choices

            // === Create Choice Button Template (for prefab) ===
            GameObject choiceButtonTemplate = CreateEchoChoiceButton(screenObj);
            choiceButtonTemplate.SetActive(false); // Hidden template

            // === Wire EchoEventScreen references ===
            var echoScreen = screenObj.GetComponent<EchoEventScreen>();
            if (echoScreen != null)
            {
                SerializedObject so = new SerializedObject(echoScreen);
                so.FindProperty("_titleText").objectReferenceValue = titleTmp;
                so.FindProperty("_narrativeText").objectReferenceValue = narrativeTmp;
                so.FindProperty("_backgroundImage").objectReferenceValue = bgImage;
                so.FindProperty("_choiceContainer").objectReferenceValue = choiceContainer.transform;
                so.FindProperty("_choiceButtonPrefab").objectReferenceValue = choiceButtonTemplate.GetComponent<Button>();
                so.FindProperty("_outcomePanel").objectReferenceValue = outcomePanel;
                so.FindProperty("_outcomeText").objectReferenceValue = outcomeTmp;
                so.FindProperty("_continueButton").objectReferenceValue = continueBtnComponent;
                so.FindProperty("_skipButton").objectReferenceValue = skipBtnComponent;
                so.FindProperty("_outcomeCardContainer").objectReferenceValue = outcomeCardContainer.transform;

                // Wire Card prefab for outcome display
                var cardPrefab = AssetDatabase.LoadAssetAtPath<Cards.Card>("Assets/_Project/Prefabs/Cards/Card.prefab");
                if (cardPrefab != null)
                {
                    so.FindProperty("_cardPrefab").objectReferenceValue = cardPrefab;
                    Debug.Log("[ProductionSceneSetupGenerator] Wired Card.prefab to EchoEventScreen._cardPrefab");
                }
                else
                {
                    Debug.LogWarning("[ProductionSceneSetupGenerator] Card.prefab not found for EchoEventScreen - run HNR > 2. Prefabs > UI > Card Prefab first");
                }

                so.ApplyModifiedPropertiesWithoutUndo();
            }

            return screenObj;
        }

        /// <summary>
        /// Creates a choice button template for EchoEventScreen.
        /// </summary>
        private static GameObject CreateEchoChoiceButton(GameObject parent)
        {
            GameObject buttonObj = new GameObject("ChoiceButtonTemplate");
            buttonObj.transform.SetParent(parent.transform, false);

            RectTransform rect = buttonObj.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(0, 45);

            Image bg = buttonObj.AddComponent<Image>();
            bg.color = new Color(0.15f, 0.12f, 0.2f, 0.9f);

            Button button = buttonObj.AddComponent<Button>();
            button.targetGraphic = bg;

            // Add layout element for proper sizing
            var layoutElement = buttonObj.AddComponent<LayoutElement>();
            layoutElement.preferredHeight = 45;

            // Button text
            GameObject textObj = CreateText(buttonObj, "Text", "Choice Text", 14);
            RectTransform textRect = textObj.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = new Vector2(15, 0);
            textRect.offsetMax = new Vector2(-15, 0);
            var tmp = textObj.GetComponent<TextMeshProUGUI>();
            tmp.alignment = TextAlignmentOptions.Left;
            tmp.color = Color.white;

            return buttonObj;
        }

        private static GameObject CreateShopScreen(GameObject parent)
        {
            GameObject screenObj = new GameObject("ShopScreen");
            screenObj.transform.SetParent(parent.transform, false);

            RectTransform rect = screenObj.AddComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.sizeDelta = Vector2.zero;

            var shopScreen = screenObj.AddComponent<ShopScreen>();
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
            titleObj.GetComponent<TMP_Text>().color = new Color(0.83f, 0.69f, 0.22f); // Soul gold

            // === Currency Display ===
            GameObject currencyObj = CreateText(screenObj, "CurrencyDisplay", "0", 24);
            RectTransform currencyRect = currencyObj.GetComponent<RectTransform>();
            currencyRect.anchorMin = new Vector2(0.85f, 0.92f);
            currencyRect.anchorMax = new Vector2(0.85f, 0.92f);
            currencyRect.sizeDelta = new Vector2(200, 40);
            var currencyText = currencyObj.GetComponent<TMP_Text>();

            // === Items Container ===
            GameObject itemsContainer = new GameObject("ItemsContainer");
            itemsContainer.transform.SetParent(screenObj.transform, false);
            RectTransform itemsRect = itemsContainer.AddComponent<RectTransform>();
            itemsRect.anchorMin = new Vector2(0.05f, 0.18f);
            itemsRect.anchorMax = new Vector2(0.7f, 0.85f);
            itemsRect.sizeDelta = Vector2.zero;

            GridLayoutGroup grid = itemsContainer.AddComponent<GridLayoutGroup>();
            grid.cellSize = new Vector2(180, 250);
            grid.spacing = new Vector2(15, 15);
            grid.childAlignment = TextAnchor.MiddleCenter; // Centered for better visual alignment
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = 3;
            grid.padding = new RectOffset(10, 10, 10, 10);

            // === Services Panel (Right side) ===
            GameObject servicesPanel = new GameObject("ServicesPanel");
            servicesPanel.transform.SetParent(screenObj.transform, false);
            RectTransform servicesRect = servicesPanel.AddComponent<RectTransform>();
            servicesRect.anchorMin = new Vector2(0.72f, 0.18f);
            servicesRect.anchorMax = new Vector2(0.95f, 0.85f);
            servicesRect.sizeDelta = Vector2.zero;

            Image servicesBg = servicesPanel.AddComponent<Image>();
            servicesBg.color = new Color(0.1f, 0.08f, 0.12f, 0.9f);

            VerticalLayoutGroup servicesLayout = servicesPanel.AddComponent<VerticalLayoutGroup>();
            servicesLayout.padding = new RectOffset(10, 10, 10, 10);
            servicesLayout.spacing = 10;
            servicesLayout.childAlignment = TextAnchor.UpperCenter;
            servicesLayout.childForceExpandWidth = true;
            servicesLayout.childForceExpandHeight = false;

            // Services title
            GameObject servicesTitleObj = CreateText(servicesPanel, "ServicesTitle", "SERVICES", 18);
            servicesTitleObj.GetComponent<TMP_Text>().fontStyle = FontStyles.Bold;
            var servTitleLayout = servicesTitleObj.AddComponent<LayoutElement>();
            servTitleLayout.preferredHeight = 30;

            // Remove Card Button
            GameObject removeCardBtn = CreateShopServiceButton(servicesPanel, "RemoveCardButton", "Remove Card", "75 Shards");
            var removeCardButton = removeCardBtn.GetComponent<Button>();

            // Purify Button
            GameObject purifyBtn = CreateShopServiceButton(servicesPanel, "PurifyButton", "Purify", "50 Shards");
            var purifyButton = purifyBtn.GetComponent<Button>();

            // === Leave Button ===
            GameObject leaveBtn = CreateMenuButton(screenObj, "LeaveButton", "LEAVE SHOP");
            RectTransform leaveRect = leaveBtn.GetComponent<RectTransform>();
            leaveRect.anchorMin = new Vector2(0.5f, 0.05f);
            leaveRect.anchorMax = new Vector2(0.5f, 0.05f);
            leaveRect.sizeDelta = new Vector2(200, 50);
            var leaveButton = leaveBtn.GetComponent<Button>();

            // === Wire ShopScreen references ===
            SerializedObject so = new SerializedObject(shopScreen);
            so.FindProperty("_voidShardsText").objectReferenceValue = currencyText;
            so.FindProperty("_itemContainer").objectReferenceValue = itemsContainer.transform;
            so.FindProperty("_leaveButton").objectReferenceValue = leaveButton;
            so.FindProperty("_removeCardButton").objectReferenceValue = removeCardButton;
            so.FindProperty("_purifyButton").objectReferenceValue = purifyButton;
            so.ApplyModifiedPropertiesWithoutUndo();

            Debug.Log("[ProductionSceneSetupGenerator] Created ShopScreen with wired references");
            return screenObj;
        }

        /// <summary>
        /// Creates a service button for the shop.
        /// </summary>
        private static GameObject CreateShopServiceButton(GameObject parent, string name, string label, string cost)
        {
            GameObject btnObj = new GameObject(name);
            btnObj.transform.SetParent(parent.transform, false);

            Image bg = btnObj.AddComponent<Image>();
            bg.color = new Color(0.15f, 0.12f, 0.2f, 0.9f);

            Button button = btnObj.AddComponent<Button>();
            button.targetGraphic = bg;

            var layoutElement = btnObj.AddComponent<LayoutElement>();
            layoutElement.preferredHeight = 60;

            VerticalLayoutGroup layout = btnObj.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(8, 8, 5, 5);
            layout.spacing = 2;
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;

            // Label text
            GameObject labelObj = CreateText(btnObj, "Label", label, 14);
            labelObj.GetComponent<TMP_Text>().fontStyle = FontStyles.Bold;
            labelObj.GetComponent<TMP_Text>().color = Color.white;

            // Cost text
            GameObject costObj = CreateText(btnObj, "Cost", cost, 11);
            costObj.GetComponent<TMP_Text>().color = new Color(0f, 0.83f, 0.89f);

            return btnObj;
        }

        private static GameObject CreateSanctuaryScreen(GameObject parent)
        {
            GameObject screenObj = new GameObject("SanctuaryScreen");
            screenObj.transform.SetParent(parent.transform, false);

            RectTransform rect = screenObj.AddComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.sizeDelta = Vector2.zero;

            var sanctuaryScreen = screenObj.AddComponent<SanctuaryScreen>();
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
            var titleText = titleObj.GetComponent<TMP_Text>();
            titleText.color = new Color(0.18f, 0.8f, 0.44f); // Health green
            titleText.fontStyle = FontStyles.Bold;

            // Description
            GameObject descObj = CreateText(screenObj, "Description", "A moment of respite in the Null Rift...", 16);
            RectTransform descRect = descObj.GetComponent<RectTransform>();
            descRect.anchorMin = new Vector2(0.5f, 0.78f);
            descRect.anchorMax = new Vector2(0.5f, 0.78f);
            descRect.sizeDelta = new Vector2(500, 40);
            var descText = descObj.GetComponent<TMP_Text>();
            descText.color = new Color(0.7f, 0.7f, 0.7f);

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
            GameObject restBtn = CreateSanctuaryChoice(choicesContainer, "RestButton", "REST", "Heal 30% HP", new Color(0.18f, 0.8f, 0.44f));

            // Purify button (cyan)
            GameObject purifyBtn = CreateSanctuaryChoice(choicesContainer, "PurifyButton", "PURIFY", "Remove -30 Corruption", new Color(0f, 0.83f, 0.89f));

            // Upgrade button (gold)
            GameObject upgradeBtn = CreateSanctuaryChoice(choicesContainer, "UpgradeButton", "UPGRADE", "Upgrade a card", new Color(0.83f, 0.69f, 0.22f));

            // Leave button - allows skipping sanctuary without making a choice
            GameObject leaveBtn = CreateMenuButton(screenObj, "LeaveButton", "LEAVE");
            RectTransform leaveRect = leaveBtn.GetComponent<RectTransform>();
            leaveRect.anchorMin = new Vector2(0.5f, 0.12f);
            leaveRect.anchorMax = new Vector2(0.5f, 0.12f);
            leaveRect.sizeDelta = new Vector2(150, 40);
            // Style as a muted button
            leaveBtn.GetComponent<Image>().color = new Color(0.25f, 0.25f, 0.25f, 0.8f);

            // === Card Selection Panel (shown when Upgrade is clicked) ===
            GameObject cardSelectionPanel = new GameObject("CardSelectionPanel");
            cardSelectionPanel.transform.SetParent(screenObj.transform, false);
            RectTransform cardPanelRect = cardSelectionPanel.AddComponent<RectTransform>();
            cardPanelRect.anchorMin = Vector2.zero;
            cardPanelRect.anchorMax = Vector2.one;
            cardPanelRect.sizeDelta = Vector2.zero;

            Image cardPanelBg = cardSelectionPanel.AddComponent<Image>();
            cardPanelBg.color = new Color(0.05f, 0.05f, 0.08f, 0.98f);

            // Panel title
            GameObject panelTitle = CreateText(cardSelectionPanel, "PanelTitle", "SELECT A CARD TO UPGRADE", 24);
            RectTransform panelTitleRect = panelTitle.GetComponent<RectTransform>();
            panelTitleRect.anchorMin = new Vector2(0.5f, 0.88f);
            panelTitleRect.anchorMax = new Vector2(0.5f, 0.88f);
            panelTitleRect.sizeDelta = new Vector2(400, 40);
            panelTitle.GetComponent<TMP_Text>().color = new Color(0.83f, 0.69f, 0.22f);
            panelTitle.GetComponent<TMP_Text>().fontStyle = FontStyles.Bold;

            // Card container with scroll capability
            GameObject scrollView = new GameObject("ScrollView");
            scrollView.transform.SetParent(cardSelectionPanel.transform, false);
            RectTransform scrollRect = scrollView.AddComponent<RectTransform>();
            scrollRect.anchorMin = new Vector2(0.1f, 0.2f);
            scrollRect.anchorMax = new Vector2(0.9f, 0.82f);
            scrollRect.sizeDelta = Vector2.zero;

            Image scrollBg = scrollView.AddComponent<Image>();
            scrollBg.color = new Color(0.08f, 0.08f, 0.12f, 0.8f);

            ScrollRect scroll = scrollView.AddComponent<ScrollRect>();

            // Viewport
            GameObject viewport = new GameObject("Viewport");
            viewport.transform.SetParent(scrollView.transform, false);
            RectTransform viewportRect = viewport.AddComponent<RectTransform>();
            viewportRect.anchorMin = Vector2.zero;
            viewportRect.anchorMax = Vector2.one;
            viewportRect.sizeDelta = Vector2.zero;
            // Use RectMask2D instead of Mask - more reliable for scroll views
            // RectMask2D clips children to RectTransform bounds without needing an Image sprite
            viewport.AddComponent<RectMask2D>();

            // Content (card container)
            GameObject upgradeCardContainer = new GameObject("UpgradeCardContainer");
            upgradeCardContainer.transform.SetParent(viewport.transform, false);
            RectTransform contentRect = upgradeCardContainer.AddComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0, 1);
            contentRect.anchorMax = new Vector2(1, 1);
            contentRect.pivot = new Vector2(0.5f, 1);
            contentRect.sizeDelta = new Vector2(0, 0);

            GridLayoutGroup grid = upgradeCardContainer.AddComponent<GridLayoutGroup>();
            grid.cellSize = new Vector2(200, 280); // Match Card.prefab native size for proper scaling
            grid.spacing = new Vector2(20, 20);
            grid.padding = new RectOffset(30, 30, 20, 20);
            grid.childAlignment = TextAnchor.UpperCenter;

            ContentSizeFitter fitter = upgradeCardContainer.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            scroll.content = contentRect;
            scroll.viewport = viewportRect;
            scroll.horizontal = false;
            scroll.vertical = true;

            // Confirm button
            GameObject confirmUpgradeBtn = CreateMenuButton(cardSelectionPanel, "ConfirmUpgradeButton", "CONFIRM UPGRADE");
            RectTransform confirmRect = confirmUpgradeBtn.GetComponent<RectTransform>();
            confirmRect.anchorMin = new Vector2(0.35f, 0.08f);
            confirmRect.anchorMax = new Vector2(0.35f, 0.08f);
            confirmRect.sizeDelta = new Vector2(180, 45);
            confirmUpgradeBtn.GetComponent<Image>().color = new Color(0.18f, 0.7f, 0.35f);
            confirmUpgradeBtn.GetComponent<Button>().interactable = false;

            // Cancel button
            GameObject cancelUpgradeBtn = CreateMenuButton(cardSelectionPanel, "CancelUpgradeButton", "CANCEL");
            RectTransform cancelRect = cancelUpgradeBtn.GetComponent<RectTransform>();
            cancelRect.anchorMin = new Vector2(0.65f, 0.08f);
            cancelRect.anchorMax = new Vector2(0.65f, 0.08f);
            cancelRect.sizeDelta = new Vector2(140, 45);
            cancelUpgradeBtn.GetComponent<Image>().color = new Color(0.5f, 0.3f, 0.3f);

            cardSelectionPanel.SetActive(false);

            // === Wire SanctuaryScreen references ===
            SerializedObject so = new SerializedObject(sanctuaryScreen);
            so.FindProperty("_titleText").objectReferenceValue = titleText;
            so.FindProperty("_descriptionText").objectReferenceValue = descText;
            so.FindProperty("_restButton").objectReferenceValue = restBtn.GetComponent<Button>();
            so.FindProperty("_purifyButton").objectReferenceValue = purifyBtn.GetComponent<Button>();
            so.FindProperty("_upgradeButton").objectReferenceValue = upgradeBtn.GetComponent<Button>();
            so.FindProperty("_leaveButton").objectReferenceValue = leaveBtn.GetComponent<Button>();
            so.FindProperty("_cardSelectionPanel").objectReferenceValue = cardSelectionPanel;
            so.FindProperty("_upgradeCardContainer").objectReferenceValue = upgradeCardContainer.transform;
            so.FindProperty("_confirmUpgradeButton").objectReferenceValue = confirmUpgradeBtn.GetComponent<Button>();
            so.FindProperty("_cancelUpgradeButton").objectReferenceValue = cancelUpgradeBtn.GetComponent<Button>();

            // Wire Card prefab for proper card display (implements ICardDisplay)
            var cardPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Project/Prefabs/Cards/Card.prefab");
            if (cardPrefab != null)
            {
                var cardSlotProp = so.FindProperty("_cardSlotPrefab");
                if (cardSlotProp != null)
                {
                    cardSlotProp.objectReferenceValue = cardPrefab;
                    Debug.Log($"[ProductionSceneSetupGenerator] Wired Card.prefab to SanctuaryScreen._cardSlotPrefab");
                }
                else
                {
                    Debug.LogError("[ProductionSceneSetupGenerator] Failed to find _cardSlotPrefab property on SanctuaryScreen");
                }
            }
            else
            {
                Debug.LogWarning("[ProductionSceneSetupGenerator] Card.prefab not found at Assets/_Project/Prefabs/Cards/Card.prefab - run HNR > 2. Prefabs > UI > Card Prefab first");
            }

            so.ApplyModifiedPropertiesWithoutUndo();

            // Mark component dirty to ensure changes persist
            EditorUtility.SetDirty(sanctuaryScreen);

            Debug.Log("[ProductionSceneSetupGenerator] Created SanctuaryScreen with wired references");
            return screenObj;
        }

        private static GameObject CreateSanctuaryChoice(GameObject parent, string name, string title, string desc, Color color)
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

            return choice;
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

            // Wire Card prefab for proper card display (implements ICardDisplay)
            var cardPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Project/Prefabs/Cards/Card.prefab");
            if (cardPrefab != null)
            {
                var cardSlotProp = so.FindProperty("_cardRewardSlotPrefab");
                if (cardSlotProp != null)
                {
                    cardSlotProp.objectReferenceValue = cardPrefab;
                    Debug.Log("[ProductionSceneSetupGenerator] Wired Card.prefab to TreasureScreen._cardRewardSlotPrefab");
                }
                else
                {
                    Debug.LogError("[ProductionSceneSetupGenerator] Failed to find _cardRewardSlotPrefab property on TreasureScreen");
                }
            }
            else
            {
                Debug.LogWarning("[ProductionSceneSetupGenerator] Card.prefab not found for TreasureScreen - run HNR > 2. Prefabs > UI > Card Prefab first");
            }

            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(treasureScreen);

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

            // Wire Card prefab for proper card display (implements ICardDisplay)
            var cardPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Project/Prefabs/Cards/Card.prefab");
            if (cardPrefab != null)
            {
                var cardSlotProp = so.FindProperty("_cardRewardSlotPrefab");
                if (cardSlotProp != null)
                {
                    cardSlotProp.objectReferenceValue = cardPrefab;
                    Debug.Log("[ProductionSceneSetupGenerator] Wired Card.prefab to ResultsScreen._cardRewardSlotPrefab");
                }
                else
                {
                    Debug.LogError("[ProductionSceneSetupGenerator] Failed to find _cardRewardSlotPrefab property on ResultsScreen");
                }
            }
            else
            {
                Debug.LogWarning("[ProductionSceneSetupGenerator] Card.prefab not found for ResultsScreen - run HNR > 2. Prefabs > UI > Card Prefab first");
            }

            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(resultsScreen);

            return screenObj;
        }

        // ============================================
        // Screen Creation - Combat
        // ============================================

        private static GameObject CreateCombatScreen(GameObject parent)
        {
            GameObject screenObj = new GameObject("CombatScreen");
            screenObj.transform.SetParent(parent.transform, false);

            RectTransform rect = screenObj.AddComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.sizeDelta = Vector2.zero;

            var combatScreen = screenObj.AddComponent<CombatScreen>();

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
            GameObject vitalityBar = CreateSharedVitalityBarFull(topHUD);

            // System Menu Bar (right side of top HUD)
            GameObject sysMenuBar = CreateSystemMenuBar(topHUD);

            // NOTE: PartySidebar is now created as child of BottomCommandCenter (see below)
            // This ensures proper click interaction and layering

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

            // TurnInfoContainer removed per UI refactor - turn info now in SharedVitalityBar

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
            GameObject apCounter = CreateAPCounter(bottomHUD);

            // Execution Button (right side)
            GameObject executionBtn = CreateExecutionButton(bottomHUD);

            // ============================================
            // LEFT SIDEBAR - Party Status (child of BottomCommandCenter for click interaction)
            // ============================================
            GameObject partySidebar = CreatePartySidebar(bottomHUD);

            // ============================================
            // LEFT SIDEBAR - Relic Display (below party sidebar in TopHUD area)
            // ============================================
            GameObject relicDisplayBar = CreateRelicDisplayBar(screenObj);

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

        /// <summary>
        /// Creates an Image component with a sprite icon.
        /// Falls back to text if sprite is null.
        /// </summary>
        /// <param name="parent">Parent GameObject</param>
        /// <param name="name">Name of the new GameObject</param>
        /// <param name="sprite">Sprite to display (can be null for fallback)</param>
        /// <param name="size">Size of the icon</param>
        /// <param name="tint">Optional color tint</param>
        /// <param name="fallbackText">Text to display if sprite is null</param>
        /// <param name="fallbackFontSize">Font size for fallback text</param>
        /// <returns>The created GameObject</returns>
        private static GameObject CreateIconImage(GameObject parent, string name, Sprite sprite, Vector2 size, Color? tint = null, string fallbackText = null, int fallbackFontSize = 12)
        {
            GameObject obj = new GameObject(name);
            obj.transform.SetParent(parent.transform, false);

            RectTransform rect = obj.AddComponent<RectTransform>();
            rect.sizeDelta = size;

            if (sprite != null)
            {
                // Use sprite-based Image
                Image image = obj.AddComponent<Image>();
                image.sprite = sprite;
                image.preserveAspect = true;
                image.color = tint ?? Color.white;
            }
            else if (!string.IsNullOrEmpty(fallbackText))
            {
                // Fallback to text if no sprite
                TextMeshProUGUI tmp = obj.AddComponent<TextMeshProUGUI>();
                tmp.text = fallbackText;
                tmp.fontSize = fallbackFontSize;
                tmp.alignment = TextAlignmentOptions.Center;
                tmp.color = tint ?? Color.white;
            }
            else
            {
                // No sprite and no fallback - create placeholder Image
                Image image = obj.AddComponent<Image>();
                image.color = tint ?? new Color(1f, 0f, 1f, 1f); // Magenta placeholder
            }

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
        /// Wires VFXConfigSO to the VFXPoolManager.
        /// Uses centralized VFXConfigSO for VFX configuration.
        /// Falls back to direct prefab wiring if VFXConfigSO not found.
        /// </summary>
        private static void WireVFXPoolManager(VFXPoolManager vfxPoolManager)
        {
            if (vfxPoolManager == null) return;

            SerializedObject so = new SerializedObject(vfxPoolManager);

            // Try to load VFXConfigSO first (preferred approach)
            const string VFX_CONFIG_PATH = "Assets/_Project/Data/Config/VFXConfig.asset";
            var vfxConfig = AssetDatabase.LoadAssetAtPath<VFXConfigSO>(VFX_CONFIG_PATH);

            if (vfxConfig != null)
            {
                // Wire VFXConfigSO
                so.FindProperty("_vfxConfig").objectReferenceValue = vfxConfig;

                // Clear direct pool configs since VFXConfigSO takes priority
                so.FindProperty("_poolConfigs").arraySize = 0;

                so.ApplyModifiedPropertiesWithoutUndo();
                Debug.Log($"[ProductionSceneSetupGenerator] Wired VFXPoolManager with VFXConfigSO ({vfxConfig.TotalEntryCount} effects)");
            }
            else
            {
                // Fallback: Wire individual prefabs directly
                Debug.LogWarning("[ProductionSceneSetupGenerator] VFXConfigSO not found at " + VFX_CONFIG_PATH +
                    ". Run 'HNR > 3. Audio & VFX > Generate VFX Config' first. Using fallback direct wiring.");

                WireVFXPoolManagerFallback(so);
            }
        }

        /// <summary>
        /// Fallback method for wiring VFX prefabs directly when VFXConfigSO is not available.
        /// </summary>
        private static void WireVFXPoolManagerFallback(SerializedObject so)
        {
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
            Debug.Log($"[ProductionSceneSetupGenerator] Wired VFXPoolManager with {validCount} VFX prefabs (fallback mode)");
        }

        /// <summary>
        /// Wires AudioConfigSO to the AudioManager.
        /// </summary>
        private static void WireAudioManager(AudioManager audioManager)
        {
            if (audioManager == null) return;

            const string AUDIO_CONFIG_PATH = "Assets/_Project/Data/Config/AudioConfig.asset";
            var audioConfig = AssetDatabase.LoadAssetAtPath<AudioConfigSO>(AUDIO_CONFIG_PATH);

            SerializedObject so = new SerializedObject(audioManager);

            if (audioConfig != null)
            {
                so.FindProperty("_audioConfig").objectReferenceValue = audioConfig;
                so.ApplyModifiedPropertiesWithoutUndo();
                Debug.Log($"[ProductionSceneSetupGenerator] Wired AudioManager with AudioConfigSO ({audioConfig.TotalEntryCount} entries)");
            }
            else
            {
                Debug.LogWarning("[ProductionSceneSetupGenerator] AudioConfigSO not found at " + AUDIO_CONFIG_PATH +
                    ". Run 'HNR > 3. Audio & VFX > Generate Audio Config' first.");
                so.ApplyModifiedPropertiesWithoutUndo();
            }
        }

        /// <summary>
        /// Wires CombatScreen component references.
        /// </summary>
        private static void WireCombatScreen(GameObject combatScreenObj)
        {
            if (combatScreenObj == null) return;

            var screen = combatScreenObj.GetComponent<CombatScreen>();
            if (screen == null) return;

            SerializedObject so = new SerializedObject(screen);

            // Find and wire child components
            var vitalityBar = combatScreenObj.GetComponentInChildren<SharedVitalityBar>(true);
            var partySidebar = combatScreenObj.GetComponentInChildren<PartyStatusSidebar>(true);
            var relicDisplayBar = combatScreenObj.GetComponentInChildren<RelicDisplayBar>(true);
            var cardFan = combatScreenObj.GetComponentInChildren<CardFanLayout>(true);
            var apCounter = combatScreenObj.GetComponentInChildren<APCounterDisplay>(true);
            var execButton = combatScreenObj.GetComponentInChildren<ExecutionButton>(true);
            var sysMenu = combatScreenObj.GetComponentInChildren<SystemMenuBar>(true);

            // Wire if found
            if (vitalityBar != null)
                SetPropertyIfExists(so, "_vitalityBar", vitalityBar);
            if (partySidebar != null)
                SetPropertyIfExists(so, "_partySidebar", partySidebar);
            if (relicDisplayBar != null)
                SetPropertyIfExists(so, "_relicDisplayBar", relicDisplayBar);
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
                    Debug.Log($"[ProductionSceneSetupGenerator] Wired {slotCount} ally slots to CombatScreen");
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

            // Wire CombatCard prefab for card instantiation during combat
            var combatCardPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Project/Prefabs/UI/Combat/CombatCard.prefab");
            if (combatCardPrefab != null)
            {
                var combatCard = combatCardPrefab.GetComponent<CombatCard>();
                if (combatCard != null)
                {
                    SetPropertyIfExists(so, "_combatCardPrefab", combatCard);
                    Debug.Log("[ProductionSceneSetupGenerator] Wired CombatCard.prefab to CombatScreen._combatCardPrefab");
                }
            }
            else
            {
                Debug.LogWarning("[ProductionSceneSetupGenerator] CombatCard.prefab not found - run HNR > 2. Prefabs > UI > CombatCard Prefab first");
            }

            so.ApplyModifiedPropertiesWithoutUndo();
            Debug.Log("[ProductionSceneSetupGenerator] Wired CombatScreen component references");
        }

        // ============================================
        // Layout Helper Methods
        // ============================================

        /// <summary>
        /// Creates the shared vitality bar with HP bar on top and party portraits with corruption bars below.
        /// Layout: HP Bar at top, 3 PortraitCorruptionSlots horizontally below.
        /// </summary>
        private static GameObject CreateSharedVitalityBarFull(GameObject parent)
        {
            GameObject barObj = new GameObject("SharedVitalityBar");
            barObj.transform.SetParent(parent.transform, false);

            RectTransform rect = barObj.AddComponent<RectTransform>();
            // Fill left portion of TopHUD, stretch vertically within parent
            rect.anchorMin = new Vector2(0, 0);
            rect.anchorMax = new Vector2(0.6f, 1); // 60% of TopHUD width, full height
            rect.pivot = new Vector2(0, 0.5f);
            rect.offsetMin = new Vector2(12, 4); // Left and bottom margin
            rect.offsetMax = new Vector2(0, -4); // Right edge at anchor, top margin

            // Vertical layout: HP bar on top, portraits below
            var layout = barObj.AddComponent<VerticalLayoutGroup>();
            layout.spacing = 4f;
            layout.childAlignment = TextAnchor.UpperLeft;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.padding = new RectOffset(4, 4, 2, 2);

            // ============================================
            // HP Bar Row (top) - WIDE RECTANGLE
            // ============================================
            GameObject hpRow = new GameObject("HPBarRow");
            hpRow.transform.SetParent(barObj.transform, false);

            RectTransform hpRowRect = hpRow.AddComponent<RectTransform>();

            var hpRowLayoutElement = hpRow.AddComponent<LayoutElement>();
            hpRowLayoutElement.preferredHeight = 28;
            hpRowLayoutElement.minHeight = 24;

            var hpRowLayout = hpRow.AddComponent<HorizontalLayoutGroup>();
            hpRowLayout.spacing = 8f;
            hpRowLayout.childAlignment = TextAnchor.MiddleLeft;
            hpRowLayout.childForceExpandWidth = false;
            hpRowLayout.childForceExpandHeight = true;
            hpRowLayout.childControlWidth = true;
            hpRowLayout.childControlHeight = true;

            // HP Bar container - WIDE rectangle (flexibleWidth makes it expand)
            GameObject hpBarContainer = new GameObject("HPBarContainer");
            hpBarContainer.transform.SetParent(hpRow.transform, false);

            RectTransform hpContainerRect = hpBarContainer.AddComponent<RectTransform>();

            var hpLayout = hpBarContainer.AddComponent<LayoutElement>();
            hpLayout.minWidth = 200;
            hpLayout.preferredHeight = 24;
            hpLayout.flexibleWidth = 1; // Expand to fill available width

            // HP Bar background
            Image hpBg = hpBarContainer.AddComponent<Image>();
            hpBg.color = new Color(0, 0, 0, 0.7f);

            // Damage Slider (behind health - shows linger effect)
            GameObject damageSliderObj = new GameObject("DamageSlider");
            damageSliderObj.transform.SetParent(hpBarContainer.transform, false);
            RectTransform dmgSliderRect = damageSliderObj.AddComponent<RectTransform>();
            dmgSliderRect.anchorMin = Vector2.zero;
            dmgSliderRect.anchorMax = Vector2.one;
            dmgSliderRect.offsetMin = new Vector2(2, 2);
            dmgSliderRect.offsetMax = new Vector2(-2, -2);

            Slider damageSlider = damageSliderObj.AddComponent<Slider>();
            damageSlider.direction = Slider.Direction.LeftToRight;
            damageSlider.minValue = 0f;
            damageSlider.maxValue = 1f;
            damageSlider.value = 1f;
            damageSlider.interactable = false;

            // Damage Fill Area
            GameObject dmgFillAreaObj = new GameObject("Fill Area");
            dmgFillAreaObj.transform.SetParent(damageSliderObj.transform, false);
            RectTransform dmgFillAreaRect = dmgFillAreaObj.AddComponent<RectTransform>();
            dmgFillAreaRect.anchorMin = Vector2.zero;
            dmgFillAreaRect.anchorMax = Vector2.one;
            dmgFillAreaRect.offsetMin = Vector2.zero;
            dmgFillAreaRect.offsetMax = Vector2.zero;

            // Damage Fill
            GameObject dmgFillObj = new GameObject("Fill");
            dmgFillObj.transform.SetParent(dmgFillAreaObj.transform, false);
            RectTransform dmgFillRect = dmgFillObj.AddComponent<RectTransform>();
            dmgFillRect.anchorMin = Vector2.zero;
            dmgFillRect.anchorMax = Vector2.one;
            dmgFillRect.offsetMin = Vector2.zero;
            dmgFillRect.offsetMax = Vector2.zero;
            Image dmgFillImg = dmgFillObj.AddComponent<Image>();
            dmgFillImg.color = new Color(1f, 0.27f, 0.27f); // Corruption glow #FF4444
            damageSlider.fillRect = dmgFillRect;

            // Health Slider (in front of damage)
            GameObject healthSliderObj = new GameObject("HealthSlider");
            healthSliderObj.transform.SetParent(hpBarContainer.transform, false);
            RectTransform hpSliderRect = healthSliderObj.AddComponent<RectTransform>();
            hpSliderRect.anchorMin = Vector2.zero;
            hpSliderRect.anchorMax = Vector2.one;
            hpSliderRect.offsetMin = new Vector2(2, 2);
            hpSliderRect.offsetMax = new Vector2(-2, -2);

            Slider healthSlider = healthSliderObj.AddComponent<Slider>();
            healthSlider.direction = Slider.Direction.LeftToRight;
            healthSlider.minValue = 0f;
            healthSlider.maxValue = 1f;
            healthSlider.value = 1f;
            healthSlider.interactable = false;

            // Health Fill Area
            GameObject hpFillAreaObj = new GameObject("Fill Area");
            hpFillAreaObj.transform.SetParent(healthSliderObj.transform, false);
            RectTransform hpFillAreaRect = hpFillAreaObj.AddComponent<RectTransform>();
            hpFillAreaRect.anchorMin = Vector2.zero;
            hpFillAreaRect.anchorMax = Vector2.one;
            hpFillAreaRect.offsetMin = Vector2.zero;
            hpFillAreaRect.offsetMax = Vector2.zero;

            // Health Fill
            GameObject hpFillObj = new GameObject("Fill");
            hpFillObj.transform.SetParent(hpFillAreaObj.transform, false);
            RectTransform hpFillRect = hpFillObj.AddComponent<RectTransform>();
            hpFillRect.anchorMin = Vector2.zero;
            hpFillRect.anchorMax = Vector2.one;
            hpFillRect.offsetMin = Vector2.zero;
            hpFillRect.offsetMax = Vector2.zero;
            Image hpFillImg = hpFillObj.AddComponent<Image>();
            hpFillImg.color = new Color(0.18f, 0.8f, 0.44f); // Health green #2ECC71
            healthSlider.fillRect = hpFillRect;

            // HP Text (on top of sliders)
            GameObject hpText = CreateText(hpBarContainer, "HPText", "210 / 210", 11);
            RectTransform hpTextRect = hpText.GetComponent<RectTransform>();
            hpTextRect.anchorMin = Vector2.zero;
            hpTextRect.anchorMax = Vector2.one;
            hpTextRect.sizeDelta = Vector2.zero;
            var hpTmp = hpText.GetComponent<TextMeshProUGUI>();
            hpTmp.fontStyle = TMPro.FontStyles.Bold;

            // Block indicator container
            GameObject blockContainer = new GameObject("BlockContainer");
            blockContainer.transform.SetParent(hpRow.transform, false);
            var blockLayout = blockContainer.AddComponent<HorizontalLayoutGroup>();
            blockLayout.spacing = 4f;
            blockLayout.childAlignment = TextAnchor.MiddleCenter;
            blockLayout.padding = new RectOffset(4, 4, 0, 0);

            var blockLayoutElement = blockContainer.AddComponent<LayoutElement>();
            blockLayoutElement.preferredWidth = 50;
            blockLayoutElement.preferredHeight = 28;

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

            blockContainer.SetActive(false); // Hidden by default

            // ============================================
            // Portrait Row (below HP bar) - with corruption bars
            // ============================================
            GameObject portraitContainer = new GameObject("PortraitContainer");
            portraitContainer.transform.SetParent(barObj.transform, false);
            var portraitsLayout = portraitContainer.AddComponent<HorizontalLayoutGroup>();
            portraitsLayout.spacing = 8f;
            portraitsLayout.childAlignment = TextAnchor.MiddleLeft;
            portraitsLayout.childForceExpandWidth = false;
            portraitsLayout.childForceExpandHeight = false;
            var portraitContainerLayout = portraitContainer.AddComponent<LayoutElement>();
            portraitContainerLayout.preferredHeight = 36;

            // Create 3 PortraitCorruptionSlot instances
            List<PortraitCorruptionSlot> portraitSlots = new List<PortraitCorruptionSlot>();
            for (int i = 0; i < 3; i++)
            {
                GameObject slotObj = CreatePortraitCorruptionSlotInline(portraitContainer, i);
                var slotComponent = slotObj.GetComponent<PortraitCorruptionSlot>();
                if (slotComponent != null)
                {
                    portraitSlots.Add(slotComponent);
                }
            }

            // Add SharedVitalityBar component and wire references
            var vitalityComponent = barObj.AddComponent<SharedVitalityBar>();
            SerializedObject so = new SerializedObject(vitalityComponent);
            so.FindProperty("_healthSlider").objectReferenceValue = healthSlider;
            so.FindProperty("_healthFillImage").objectReferenceValue = hpFillImg;
            so.FindProperty("_damageSlider").objectReferenceValue = damageSlider;
            so.FindProperty("_damageFillImage").objectReferenceValue = dmgFillImg;
            so.FindProperty("_hpText").objectReferenceValue = hpTmp;
            so.FindProperty("_blockContainer").objectReferenceValue = blockContainer;
            so.FindProperty("_shieldIcon").objectReferenceValue = shieldImg;
            so.FindProperty("_blockText").objectReferenceValue = blockText.GetComponent<TMP_Text>();
            so.FindProperty("_portraitContainer").objectReferenceValue = portraitContainer.GetComponent<RectTransform>();

            // Wire portrait slots array
            var slotsProperty = so.FindProperty("_portraitSlots");
            slotsProperty.arraySize = portraitSlots.Count;
            for (int i = 0; i < portraitSlots.Count; i++)
            {
                slotsProperty.GetArrayElementAtIndex(i).objectReferenceValue = portraitSlots[i];
            }

            so.ApplyModifiedPropertiesWithoutUndo();

            return barObj;
        }

        /// <summary>
        /// Creates a PortraitCorruptionSlot inline for the SharedVitalityBar.
        /// Uses Mask component to crop portrait sprite (show face only, not shrunk).
        /// Uses Slider component for reliable corruption gauge updates.
        /// </summary>
        private static GameObject CreatePortraitCorruptionSlotInline(GameObject parent, int index)
        {
            GameObject slotObj = new GameObject($"PortraitCorruptionSlot_{index}");
            slotObj.transform.SetParent(parent.transform, false);

            RectTransform slotRect = slotObj.AddComponent<RectTransform>();
            slotRect.sizeDelta = new Vector2(56, 44); // Wider for rectangular portrait + corruption bar

            var slotLayoutElement = slotObj.AddComponent<LayoutElement>();
            slotLayoutElement.preferredWidth = 56;
            slotLayoutElement.preferredHeight = 44;

            // Portrait frame (rectangular border) - acts as mask container
            GameObject frameObj = new GameObject("PortraitFrame");
            frameObj.transform.SetParent(slotObj.transform, false);
            RectTransform frameRect = frameObj.AddComponent<RectTransform>();
            frameRect.anchorMin = new Vector2(0, 0.18f);
            frameRect.anchorMax = new Vector2(1, 1);
            frameRect.offsetMin = Vector2.zero;
            frameRect.offsetMax = Vector2.zero;
            Image frameImg = frameObj.AddComponent<Image>();
            frameImg.color = new Color(0.4f, 0.4f, 0.5f, 1f);

            // Portrait mask container - clips the portrait to show only face area
            GameObject maskContainer = new GameObject("PortraitMask");
            maskContainer.transform.SetParent(frameObj.transform, false);
            RectTransform maskRect = maskContainer.AddComponent<RectTransform>();
            maskRect.anchorMin = new Vector2(0.04f, 0.04f);
            maskRect.anchorMax = new Vector2(0.96f, 0.96f);
            maskRect.offsetMin = Vector2.zero;
            maskRect.offsetMax = Vector2.zero;
            Image maskImg = maskContainer.AddComponent<Image>();
            maskImg.color = Color.white;
            // Add Mask component to clip children
            var mask = maskContainer.AddComponent<Mask>();
            mask.showMaskGraphic = false; // Don't show the mask image itself

            // Portrait image - positioned to show upper body/face, NOT shrunk
            // The image will be larger than the mask and positioned so face is visible
            GameObject portraitObj = new GameObject("Portrait");
            portraitObj.transform.SetParent(maskContainer.transform, false);
            RectTransform portraitRect = portraitObj.AddComponent<RectTransform>();
            // Position image so upper portion (face) shows - anchor at top-center
            portraitRect.anchorMin = new Vector2(0.5f, 1f);
            portraitRect.anchorMax = new Vector2(0.5f, 1f);
            portraitRect.pivot = new Vector2(0.5f, 0.85f); // Pivot near top of face
            portraitRect.sizeDelta = new Vector2(80, 100); // Larger than mask area
            portraitRect.anchoredPosition = Vector2.zero;
            Image portraitImg = portraitObj.AddComponent<Image>();
            portraitImg.color = Color.white; // Full color for actual sprite
            portraitImg.preserveAspect = true; // Keep aspect ratio

            // Corruption bar background
            GameObject corruptBgObj = new GameObject("CorruptionBackground");
            corruptBgObj.transform.SetParent(slotObj.transform, false);
            RectTransform corruptBgRect = corruptBgObj.AddComponent<RectTransform>();
            corruptBgRect.anchorMin = new Vector2(0, 0);
            corruptBgRect.anchorMax = new Vector2(1, 0.16f);
            corruptBgRect.offsetMin = Vector2.zero;
            corruptBgRect.offsetMax = Vector2.zero;
            Image corruptBgImg = corruptBgObj.AddComponent<Image>();
            corruptBgImg.color = new Color(0.15f, 0.15f, 0.2f, 0.9f);

            // Corruption Slider container
            GameObject corruptSliderObj = new GameObject("CorruptionSlider");
            corruptSliderObj.transform.SetParent(slotObj.transform, false);
            RectTransform corruptSliderRect = corruptSliderObj.AddComponent<RectTransform>();
            corruptSliderRect.anchorMin = new Vector2(0, 0);
            corruptSliderRect.anchorMax = new Vector2(1, 0.16f);
            corruptSliderRect.offsetMin = new Vector2(1, 1);
            corruptSliderRect.offsetMax = new Vector2(-1, -1);

            // Slider component
            Slider corruptSlider = corruptSliderObj.AddComponent<Slider>();
            corruptSlider.direction = Slider.Direction.LeftToRight;
            corruptSlider.minValue = 0f;
            corruptSlider.maxValue = 1f;
            corruptSlider.value = 0f;
            corruptSlider.interactable = false;

            // Fill Area for slider
            GameObject fillAreaObj = new GameObject("Fill Area");
            fillAreaObj.transform.SetParent(corruptSliderObj.transform, false);
            RectTransform fillAreaRect = fillAreaObj.AddComponent<RectTransform>();
            fillAreaRect.anchorMin = Vector2.zero;
            fillAreaRect.anchorMax = Vector2.one;
            fillAreaRect.offsetMin = Vector2.zero;
            fillAreaRect.offsetMax = Vector2.zero;

            // Fill image for slider
            GameObject fillObj = new GameObject("Fill");
            fillObj.transform.SetParent(fillAreaObj.transform, false);
            RectTransform fillRect = fillObj.AddComponent<RectTransform>();
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = Vector2.one;
            fillRect.offsetMin = Vector2.zero;
            fillRect.offsetMax = Vector2.zero;
            Image fillImg = fillObj.AddComponent<Image>();
            fillImg.color = new Color(0.2f, 0.8f, 0.2f, 1f); // Start green (safe)

            // Wire slider fill rect
            corruptSlider.fillRect = fillRect;

            // Add PortraitCorruptionSlot component and wire
            var slotComponent = slotObj.AddComponent<PortraitCorruptionSlot>();
            SerializedObject so = new SerializedObject(slotComponent);
            so.FindProperty("_portrait").objectReferenceValue = portraitImg;
            so.FindProperty("_portraitFrame").objectReferenceValue = frameImg;
            so.FindProperty("_corruptionSlider").objectReferenceValue = corruptSlider;
            so.FindProperty("_corruptionFillImage").objectReferenceValue = fillImg;
            so.FindProperty("_corruptionBackground").objectReferenceValue = corruptBgImg;
            so.FindProperty("_fillSpeed").floatValue = 5f;
            so.FindProperty("_smoothTransition").boolValue = true;
            so.ApplyModifiedPropertiesWithoutUndo();

            return slotObj;
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

            // Load icon config for system menu
            var iconConfig = LoadIconConfig();

            // Settings button - use sprite if available
            CreateSystemMenuButton(menuBar, "SettingsBtn", "\u2699", new Color(0.1f, 0.08f, 0.15f), iconConfig?.SettingsIcon);

            // Auto button (text only)
            CreateSystemMenuButton(menuBar, "AutoBtn", "\u25B6", new Color(0.1f, 0.08f, 0.15f));

            // Speed button (text only)
            GameObject speedBtn = CreateSystemMenuButton(menuBar, "SpeedBtn", "1.5x", new Color(0.1f, 0.08f, 0.15f));

            return menuBar;
        }

        private static GameObject CreateSystemMenuButton(GameObject parent, string name, string label, Color bgColor, Sprite iconSprite = null)
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

            // Use sprite if available, otherwise use text
            Color cyanColor = new Color(0f, 0.83f, 0.89f); // Soul cyan
            GameObject content = CreateIconImage(btn, "Label", iconSprite, new Vector2(16, 16), cyanColor, label, 10);
            RectTransform contentRect = content.GetComponent<RectTransform>();
            contentRect.anchorMin = Vector2.zero;
            contentRect.anchorMax = Vector2.one;
            contentRect.sizeDelta = Vector2.zero;

            return btn;
        }

        /// <summary>
        /// Creates the party status sidebar with shared SE gauge on the left.
        /// Styled after Chaos Zero Nightmare - vertical SE gauge + portrait stack.
        /// Uses Slider component for reliable SE gauge updates.
        /// Now created as child of BottomCommandCenter for proper click interaction.
        /// </summary>
        private static GameObject CreatePartySidebar(GameObject parent)
        {
            GameObject sidebar = new GameObject("PartySidebar");
            sidebar.transform.SetParent(parent.transform, false);

            // Positioned at left edge of BottomCommandCenter, filling its full height
            // Parent (BottomCommandCenter) has anchors (0,0) to (1, 0.35f)
            RectTransform rect = sidebar.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0, 0);
            rect.anchorMax = new Vector2(0, 1); // Fill full height of parent
            rect.pivot = new Vector2(0, 0);
            rect.offsetMin = new Vector2(8, 8); // Left and bottom margin
            rect.offsetMax = new Vector2(88, -8); // 80px width total

            Image bg = sidebar.AddComponent<Image>();
            bg.color = new Color(0, 0, 0, 0.6f);

            // Horizontal layout - SE gauge on left, portrait slots on right
            HorizontalLayoutGroup mainLayout = sidebar.AddComponent<HorizontalLayoutGroup>();
            mainLayout.spacing = 4f;
            mainLayout.padding = new RectOffset(4, 4, 4, 4);
            mainLayout.childAlignment = TextAnchor.MiddleLeft;
            mainLayout.childForceExpandWidth = false;
            mainLayout.childForceExpandHeight = true; // Children fill full height
            mainLayout.childControlHeight = true;
            mainLayout.childControlWidth = false;

            // ============================================
            // Shared SE Gauge (vertical Slider, full height on left)
            // ============================================
            GameObject seGaugeContainer = new GameObject("SharedSEGauge");
            seGaugeContainer.transform.SetParent(sidebar.transform, false);

            // RectTransform first, then add components
            RectTransform seContainerRect = seGaugeContainer.AddComponent<RectTransform>();

            var seGaugeLayoutElement = seGaugeContainer.AddComponent<LayoutElement>();
            seGaugeLayoutElement.preferredWidth = 24;
            seGaugeLayoutElement.minWidth = 24;
            // No flexibleHeight - let HorizontalLayoutGroup control height via childForceExpandHeight

            Image seBg = seGaugeContainer.AddComponent<Image>();
            seBg.color = new Color(0.08f, 0.08f, 0.12f, 0.9f);

            // SE Slider (vertical fill from bottom to top)
            GameObject seSliderObj = new GameObject("SESlider");
            seSliderObj.transform.SetParent(seGaugeContainer.transform, false);
            RectTransform seSliderRect = seSliderObj.AddComponent<RectTransform>();
            seSliderRect.anchorMin = new Vector2(0, 0);
            seSliderRect.anchorMax = new Vector2(1, 1); // Full stretch
            seSliderRect.offsetMin = new Vector2(3, 16); // Margin for SE text at bottom
            seSliderRect.offsetMax = new Vector2(-3, -16); // Margin for SE label at top

            // Slider component configured for vertical bottom-to-top
            Slider seSlider = seSliderObj.AddComponent<Slider>();
            seSlider.direction = Slider.Direction.BottomToTop;
            seSlider.minValue = 0f;
            seSlider.maxValue = 1f;
            seSlider.value = 0f;
            seSlider.interactable = false;

            // Fill Area for slider
            GameObject seFillAreaObj = new GameObject("Fill Area");
            seFillAreaObj.transform.SetParent(seSliderObj.transform, false);
            RectTransform seFillAreaRect = seFillAreaObj.AddComponent<RectTransform>();
            seFillAreaRect.anchorMin = Vector2.zero;
            seFillAreaRect.anchorMax = Vector2.one;
            seFillAreaRect.offsetMin = Vector2.zero;
            seFillAreaRect.offsetMax = Vector2.zero;

            // Fill image for slider
            GameObject seFillObj = new GameObject("Fill");
            seFillObj.transform.SetParent(seFillAreaObj.transform, false);
            RectTransform seFillRect = seFillObj.AddComponent<RectTransform>();
            seFillRect.anchorMin = Vector2.zero;
            seFillRect.anchorMax = Vector2.one;
            seFillRect.offsetMin = Vector2.zero;
            seFillRect.offsetMax = Vector2.zero;
            Image seFillImg = seFillObj.AddComponent<Image>();
            seFillImg.color = new Color(0f, 0.83f, 0.89f); // Soul cyan

            // Wire slider fill rect
            seSlider.fillRect = seFillRect;

            // SE Text at bottom (shows current SE value)
            GameObject seTextObj = CreateText(seGaugeContainer, "SEText", "0", 10);
            RectTransform seTextRect = seTextObj.GetComponent<RectTransform>();
            seTextRect.anchorMin = new Vector2(0, 0);
            seTextRect.anchorMax = new Vector2(1, 0);
            seTextRect.pivot = new Vector2(0.5f, 0);
            seTextRect.anchoredPosition = new Vector2(0, 2);
            seTextRect.sizeDelta = new Vector2(0, 14);
            var seTmp = seTextObj.GetComponent<TextMeshProUGUI>();
            seTmp.fontStyle = TMPro.FontStyles.Bold;
            seTmp.fontSize = 9;
            seTmp.color = new Color(0f, 0.83f, 0.89f); // Soul cyan

            // SE Label at top
            GameObject seLabelObj = CreateText(seGaugeContainer, "SELabel", "SE", 8);
            RectTransform seLabelRect = seLabelObj.GetComponent<RectTransform>();
            seLabelRect.anchorMin = new Vector2(0, 1);
            seLabelRect.anchorMax = new Vector2(1, 1);
            seLabelRect.pivot = new Vector2(0.5f, 1);
            seLabelRect.anchoredPosition = new Vector2(0, -2);
            seLabelRect.sizeDelta = new Vector2(0, 12);
            var seLabelTmp = seLabelObj.GetComponent<TextMeshProUGUI>();
            seLabelTmp.fontStyle = TMPro.FontStyles.Bold;
            seLabelTmp.fontSize = 8;
            seLabelTmp.color = new Color(0.6f, 0.6f, 0.7f);

            // ============================================
            // Party Member Slots Container (vertical stack on right, fills height)
            // ============================================
            GameObject slotsContainer = new GameObject("SlotsContainer");
            slotsContainer.transform.SetParent(sidebar.transform, false);

            RectTransform slotsContainerRect = slotsContainer.AddComponent<RectTransform>();

            var slotsLayoutElement = slotsContainer.AddComponent<LayoutElement>();
            slotsLayoutElement.preferredWidth = 48;
            slotsLayoutElement.minWidth = 48;
            // No flexibleHeight - let HorizontalLayoutGroup control height via childForceExpandHeight

            VerticalLayoutGroup slotsLayout = slotsContainer.AddComponent<VerticalLayoutGroup>();
            slotsLayout.spacing = 4f;
            slotsLayout.padding = new RectOffset(0, 0, 2, 2);
            slotsLayout.childAlignment = TextAnchor.MiddleCenter;
            slotsLayout.childForceExpandWidth = true;
            slotsLayout.childForceExpandHeight = true; // Slots expand to fill height evenly
            slotsLayout.childControlHeight = true;
            slotsLayout.childControlWidth = true;

            // Create 3 party member slots (clickable for Requiem Art)
            List<PartyMemberSlot> memberSlots = new List<PartyMemberSlot>();
            for (int i = 0; i < 3; i++)
            {
                GameObject slotObj = CreatePartyMemberSlot(slotsContainer, i);
                var slotComponent = slotObj.GetComponent<PartyMemberSlot>();
                if (slotComponent != null)
                {
                    memberSlots.Add(slotComponent);
                }
            }

            // Add PartyStatusSidebar component and wire shared SE slider
            var sidebarComponent = sidebar.AddComponent<PartyStatusSidebar>();
            SerializedObject so = new SerializedObject(sidebarComponent);

            // Wire shared SE slider
            so.FindProperty("_sharedSESlider").objectReferenceValue = seSlider;
            so.FindProperty("_sharedSEFillImage").objectReferenceValue = seFillImg;
            so.FindProperty("_sharedSEText").objectReferenceValue = seTmp;

            // Wire member slots array
            var slotsProperty = so.FindProperty("_memberSlots");
            slotsProperty.arraySize = memberSlots.Count;
            for (int i = 0; i < memberSlots.Count; i++)
            {
                slotsProperty.GetArrayElementAtIndex(i).objectReferenceValue = memberSlots[i];
            }

            so.ApplyModifiedPropertiesWithoutUndo();

            return sidebar;
        }

        /// <summary>
        /// Creates a party member slot with portrait, status icons, and click handler for Requiem Art.
        /// Uses Mask for circular portrait cropping. Clickable to trigger Requiem Art.
        /// Slot fills available height from parent VerticalLayoutGroup.
        /// </summary>
        private static GameObject CreatePartyMemberSlot(GameObject parent, int index)
        {
            GameObject slot = new GameObject($"PartySlot_{index}");
            slot.transform.SetParent(parent.transform, false);

            RectTransform slotRect = slot.AddComponent<RectTransform>();

            // LayoutElement - let parent VerticalLayoutGroup control size via childForceExpandHeight
            var layoutElement = slot.AddComponent<LayoutElement>();
            layoutElement.minHeight = 32; // Minimum height for each slot
            // No preferredHeight or flexibleHeight - parent controls via childForceExpandHeight

            // Add Button component for click interaction (Requiem Art activation)
            Image slotBg = slot.AddComponent<Image>();
            slotBg.color = new Color(0.15f, 0.12f, 0.22f, 0.9f);

            Button slotButton = slot.AddComponent<Button>();
            slotButton.targetGraphic = slotBg;
            var buttonColors = slotButton.colors;
            buttonColors.normalColor = new Color(0.15f, 0.12f, 0.22f, 0.9f);
            buttonColors.highlightedColor = new Color(0.25f, 0.2f, 0.35f, 1f);
            buttonColors.pressedColor = new Color(0f, 0.7f, 0.8f, 1f); // Cyan press
            buttonColors.selectedColor = new Color(0.2f, 0.16f, 0.28f, 0.95f);
            slotButton.colors = buttonColors;

            // Portrait frame (circular) with mask - centered in slot, size relative to slot
            GameObject portraitFrame = new GameObject("PortraitFrame");
            portraitFrame.transform.SetParent(slot.transform, false);
            RectTransform frameRect = portraitFrame.AddComponent<RectTransform>();
            // Use stretch anchors with padding to fill most of the slot
            frameRect.anchorMin = new Vector2(0.1f, 0.1f);
            frameRect.anchorMax = new Vector2(0.9f, 0.9f);
            frameRect.offsetMin = Vector2.zero;
            frameRect.offsetMax = Vector2.zero;
            Image frameImg = portraitFrame.AddComponent<Image>();
            frameImg.color = new Color(0.4f, 0.4f, 0.5f, 1f);

            // Portrait mask (circular crop)
            GameObject maskObj = new GameObject("PortraitMask");
            maskObj.transform.SetParent(portraitFrame.transform, false);
            RectTransform maskRect = maskObj.AddComponent<RectTransform>();
            maskRect.anchorMin = new Vector2(0.05f, 0.05f);
            maskRect.anchorMax = new Vector2(0.95f, 0.95f);
            maskRect.offsetMin = Vector2.zero;
            maskRect.offsetMax = Vector2.zero;
            Image maskImg = maskObj.AddComponent<Image>();
            maskImg.color = Color.white;
            var mask = maskObj.AddComponent<Mask>();
            mask.showMaskGraphic = false;

            // Portrait image (larger than mask, positioned to show face)
            GameObject portrait = new GameObject("Portrait");
            portrait.transform.SetParent(maskObj.transform, false);
            RectTransform portraitRect = portrait.AddComponent<RectTransform>();
            // Fill mask area and extend beyond to allow cropping
            portraitRect.anchorMin = new Vector2(0.5f, 0.5f);
            portraitRect.anchorMax = new Vector2(0.5f, 0.5f);
            portraitRect.pivot = new Vector2(0.5f, 0.65f); // Pivot slightly above center for face
            portraitRect.sizeDelta = new Vector2(60, 80); // Larger than mask area
            portraitRect.anchoredPosition = Vector2.zero;
            Image portraitImg = portrait.AddComponent<Image>();
            portraitImg.color = Color.white;
            portraitImg.preserveAspect = true;

            // Status effects container (below portrait)
            GameObject statusContainer = new GameObject("StatusContainer");
            statusContainer.transform.SetParent(slot.transform, false);
            RectTransform statusRect = statusContainer.AddComponent<RectTransform>();
            statusRect.anchorMin = new Vector2(0, 0);
            statusRect.anchorMax = new Vector2(1, 0);
            statusRect.pivot = new Vector2(0.5f, 0);
            statusRect.anchoredPosition = new Vector2(0, 2);
            statusRect.sizeDelta = new Vector2(0, 10);

            HorizontalLayoutGroup statusLayout = statusContainer.AddComponent<HorizontalLayoutGroup>();
            statusLayout.spacing = 1f;
            statusLayout.childAlignment = TextAnchor.MiddleCenter;
            statusLayout.childForceExpandWidth = false;
            statusLayout.childForceExpandHeight = false;

            // Active glow (for highlighting when Art is ready)
            GameObject activeGlow = new GameObject("ActiveGlow");
            activeGlow.transform.SetParent(slot.transform, false);
            activeGlow.transform.SetAsFirstSibling();
            RectTransform glowRect = activeGlow.AddComponent<RectTransform>();
            glowRect.anchorMin = Vector2.zero;
            glowRect.anchorMax = Vector2.one;
            glowRect.offsetMin = new Vector2(-2, -2);
            glowRect.offsetMax = new Vector2(2, 2);
            Image glowImg = activeGlow.AddComponent<Image>();
            glowImg.color = new Color(0f, 0.83f, 0.89f, 0.4f); // Soul cyan with alpha
            activeGlow.SetActive(false); // Hidden by default

            // Add PartyMemberSlot component and wire references
            var slotComponent = slot.AddComponent<PartyMemberSlot>();
            SerializedObject so = new SerializedObject(slotComponent);
            so.FindProperty("_portrait").objectReferenceValue = portraitImg;
            so.FindProperty("_portraitFrame").objectReferenceValue = frameImg;
            so.FindProperty("_statusContainer").objectReferenceValue = statusContainer.transform;
            so.FindProperty("_activeGlow").objectReferenceValue = glowImg;
            so.FindProperty("_slotButton").objectReferenceValue = slotButton;
            so.ApplyModifiedPropertiesWithoutUndo();

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

            // Load icon config for deck icons
            var iconConfig = LoadIconConfig();

            // Draw pile
            var drawPile = CreateDeckPileDisplay(sidebar, "DrawPile", iconConfig?.DrawPileIcon, "\U0001F4DA", "23", "Draw", new Color(0f, 0.83f, 0.89f));

            // Discard pile
            var discardPile = CreateDeckPileDisplay(sidebar, "DiscardPile", iconConfig?.DiscardPileIcon, "\U0001F504", "0", "Discard", new Color(0.63f, 0.63f, 0.63f));

            // Add DeckInfoSidebar component and wire references
            var deckInfoComponent = sidebar.AddComponent<DeckInfoSidebar>();
            var so = new SerializedObject(deckInfoComponent);

            // Wire draw pile references
            var drawCountText = drawPile.transform.Find("Count")?.GetComponent<TMP_Text>();
            var drawLabelText = drawPile.transform.Find("Label")?.GetComponent<TMP_Text>();
            if (drawCountText != null) so.FindProperty("_drawPileText").objectReferenceValue = drawCountText;
            if (drawLabelText != null) so.FindProperty("_drawPileLabel").objectReferenceValue = drawLabelText;

            // Wire discard pile references
            var discardCountText = discardPile.transform.Find("Count")?.GetComponent<TMP_Text>();
            var discardLabelText = discardPile.transform.Find("Label")?.GetComponent<TMP_Text>();
            if (discardCountText != null) so.FindProperty("_discardPileText").objectReferenceValue = discardCountText;
            if (discardLabelText != null) so.FindProperty("_discardPileLabel").objectReferenceValue = discardLabelText;

            so.ApplyModifiedPropertiesWithoutUndo();

            return sidebar;
        }

        private static GameObject CreateDeckPileDisplay(GameObject parent, string name, Sprite iconSprite, string fallbackIcon, string count, string label, Color color)
        {
            GameObject pile = new GameObject(name);
            pile.transform.SetParent(parent.transform, false);

            VerticalLayoutGroup layout = pile.AddComponent<VerticalLayoutGroup>();
            layout.spacing = 2f;
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;

            // Icon - use sprite if available, fallback to text
            GameObject iconObj = CreateIconImage(pile, "Icon", iconSprite, new Vector2(24, 24), color, fallbackIcon, 18);
            var iconLayout = iconObj.AddComponent<LayoutElement>();
            iconLayout.preferredWidth = 24;
            iconLayout.preferredHeight = 24;

            // Count
            GameObject countObj = CreateText(pile, "Count", count, 14);
            countObj.GetComponent<TextMeshProUGUI>().color = color;
            countObj.GetComponent<TextMeshProUGUI>().fontStyle = TMPro.FontStyles.Bold;

            // Label
            GameObject labelObj = CreateText(pile, "Label", label, 8);
            labelObj.GetComponent<TextMeshProUGUI>().color = new Color(0.63f, 0.63f, 0.63f);

            return pile;
        }

        /// <summary>
        /// Creates the AP counter display.
        /// </summary>
        private static GameObject CreateAPCounter(GameObject parent)
        {
            GameObject counter = new GameObject("APCounter");
            counter.transform.SetParent(parent.transform, false);

            // Positioned at center bottom of screen - square 70x70 per UI refactor
            RectTransform rect = counter.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0f);
            rect.anchorMax = new Vector2(0.5f, 0f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = new Vector2(0, 50); // 50px above bottom edge
            rect.sizeDelta = new Vector2(70, 70);

            // Add Canvas with override sorting to render ABOVE cards
            // Cards are in HandContainer which is a sibling after ScreenContainer,
            // so we need explicit sorting order to ensure AP counter renders on top
            Canvas apCanvas = counter.AddComponent<Canvas>();
            apCanvas.overrideSorting = true;
            apCanvas.sortingOrder = 100; // High sorting order to render above cards
            counter.AddComponent<GraphicRaycaster>(); // Required for UI interactions

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
        /// Creates the zone header for MapScreen.
        /// </summary>
        private static GameObject CreateZoneHeader(GameObject parent)
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
            GameObject zoneTitle = CreateText(titleContainer, "ZoneTitle", "NULL RIFT", 24);
            zoneTitle.GetComponent<TMP_Text>().fontStyle = TMPro.FontStyles.Bold;
            zoneTitle.GetComponent<TMP_Text>().color = new Color(0.9f, 0.9f, 0.95f);

            // Zone Subtitle
            GameObject zoneSubtitle = CreateText(titleContainer, "ZoneSubtitle", "Zone 1 • The Outer Reaches", 16);
            zoneSubtitle.GetComponent<TMP_Text>().color = new Color(0.42f, 0.25f, 0.63f); // Hollow violet

            // Stats container (right side)
            GameObject statsContainer = new GameObject("StatsContainer");
            statsContainer.transform.SetParent(header.transform, false);
            HorizontalLayoutGroup statsLayout = statsContainer.AddComponent<HorizontalLayoutGroup>();
            statsLayout.spacing = 16;
            statsLayout.childAlignment = TextAnchor.MiddleRight;
            statsLayout.childForceExpandWidth = false;
            statsLayout.childForceExpandHeight = false;

            // HP Container with AnimatedStatDisplay
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

            // HP AnimatedStatDisplay (DualValue mode for "current/max")
            GameObject hpDisplayObj = new GameObject("HPDisplay");
            hpDisplayObj.transform.SetParent(hpContainer.transform, false);
            var hpDisplay = hpDisplayObj.AddComponent<AnimatedStatDisplay>();
            var hpDisplayLayout = hpDisplayObj.AddComponent<LayoutElement>();
            hpDisplayLayout.preferredWidth = 80;
            hpDisplayLayout.preferredHeight = 20;

            // Create text for HP display
            GameObject hpText = CreateText(hpDisplayObj, "ValueText", "210/210", 12);
            hpText.GetComponent<TMP_Text>().fontStyle = TMPro.FontStyles.Bold;
            RectTransform hpTextRect = hpText.GetComponent<RectTransform>();
            hpTextRect.anchorMin = Vector2.zero;
            hpTextRect.anchorMax = Vector2.one;
            hpTextRect.sizeDelta = Vector2.zero;

            // Wire HP AnimatedStatDisplay
            SerializedObject hpDisplaySo = new SerializedObject(hpDisplay);
            hpDisplaySo.FindProperty("_valueText").objectReferenceValue = hpText.GetComponent<TMP_Text>();
            hpDisplaySo.FindProperty("_displayMode").enumValueIndex = 1; // DualValue
            hpDisplaySo.FindProperty("_animationSpeed").floatValue = 5f;
            hpDisplaySo.FindProperty("_punchScale").floatValue = 1.1f;
            hpDisplaySo.FindProperty("_punchDuration").floatValue = 0.2f;
            hpDisplaySo.FindProperty("_normalColor").colorValue = Color.white;
            hpDisplaySo.FindProperty("_increaseColor").colorValue = new Color(0.18f, 0.8f, 0.44f); // Health green
            hpDisplaySo.FindProperty("_decreaseColor").colorValue = new Color(0.77f, 0.12f, 0.23f); // Crimson
            hpDisplaySo.ApplyModifiedPropertiesWithoutUndo();

            // Currency Container with AnimatedStatDisplay
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

            // Currency AnimatedStatDisplay (SingleValue mode for just number)
            GameObject currDisplayObj = new GameObject("CurrencyDisplay");
            currDisplayObj.transform.SetParent(currencyContainer.transform, false);
            var currDisplay = currDisplayObj.AddComponent<AnimatedStatDisplay>();
            var currDisplayLayout = currDisplayObj.AddComponent<LayoutElement>();
            currDisplayLayout.preferredWidth = 50;
            currDisplayLayout.preferredHeight = 20;

            // Create text for currency display (starts at 0)
            GameObject currencyText = CreateText(currDisplayObj, "ValueText", "0", 12);
            currencyText.GetComponent<TMP_Text>().fontStyle = TMPro.FontStyles.Bold;
            RectTransform currTextRect = currencyText.GetComponent<RectTransform>();
            currTextRect.anchorMin = Vector2.zero;
            currTextRect.anchorMax = Vector2.one;
            currTextRect.sizeDelta = Vector2.zero;

            // Wire Currency AnimatedStatDisplay
            SerializedObject currDisplaySo = new SerializedObject(currDisplay);
            currDisplaySo.FindProperty("_valueText").objectReferenceValue = currencyText.GetComponent<TMP_Text>();
            currDisplaySo.FindProperty("_displayMode").enumValueIndex = 0; // SingleValue
            currDisplaySo.FindProperty("_animationSpeed").floatValue = 5f;
            currDisplaySo.FindProperty("_punchScale").floatValue = 1.1f;
            currDisplaySo.FindProperty("_punchDuration").floatValue = 0.2f;
            currDisplaySo.FindProperty("_normalColor").colorValue = Color.white;
            currDisplaySo.FindProperty("_increaseColor").colorValue = new Color(0.18f, 0.8f, 0.44f); // Green for gains
            currDisplaySo.FindProperty("_decreaseColor").colorValue = new Color(0.77f, 0.12f, 0.23f); // Red for spending
            currDisplaySo.ApplyModifiedPropertiesWithoutUndo();

            return header;
        }

        /// <summary>
        /// Creates the node type legend for the map footer.
        /// </summary>
        private static GameObject CreateMapLegend(GameObject parent)
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

            // Load icon config for map legend
            var iconConfig = LoadIconConfig();

            // Node type legend items - sprite, fallback text, label, color
            var nodeTypes = new (Sprite sprite, string fallback, string label, Color color)[]
            {
                (iconConfig?.CombatNodeIcon, "\u2694\uFE0F", "Combat", new Color(0.77f, 0.12f, 0.23f)),
                (iconConfig?.EliteNodeIcon, "\U0001F480", "Elite", new Color(1f, 0.27f, 0.27f)),
                (iconConfig?.ShopNodeIcon, "\U0001F6D2", "Shop", new Color(0.83f, 0.69f, 0.22f)),
                (iconConfig?.EchoEventIcon, "\u2753", "Echo", new Color(0.42f, 0.25f, 0.63f)),
                (iconConfig?.SanctuaryIcon, "\U0001F56F\uFE0F", "Sanctuary", new Color(0.18f, 0.8f, 0.44f)),
                (iconConfig?.TreasureIcon, "\U0001F48E", "Treasure", new Color(0.83f, 0.69f, 0.22f)),
                (iconConfig?.BossNodeIcon, "\U0001F479", "Boss", new Color(1f, 0.27f, 0.27f)),
            };

            foreach (var (sprite, fallback, label, color) in nodeTypes)
            {
                CreateLegendItem(legend, sprite, fallback, label, color);
            }

            return legend;
        }

        private static void CreateLegendItem(GameObject parent, Sprite iconSprite, string fallbackIcon, string label, Color color)
        {
            GameObject item = new GameObject($"Legend_{label}");
            item.transform.SetParent(parent.transform, false);

            HorizontalLayoutGroup layout = item.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = 3;
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;

            // Icon - use sprite if available, fallback to text
            GameObject iconObj = CreateIconImage(item, "Icon", iconSprite, new Vector2(16, 16), color, fallbackIcon, 12);
            var iconLayout = iconObj.AddComponent<LayoutElement>();
            iconLayout.preferredWidth = 16;
            iconLayout.preferredHeight = 16;

            GameObject labelObj = CreateText(item, "Label", label, 8);
            labelObj.GetComponent<TMP_Text>().color = new Color(0.63f, 0.63f, 0.63f);
        }

        /// <summary>
        /// Creates the circular execution (end turn) button.
        /// </summary>
        private static GameObject CreateExecutionButton(GameObject parent)
        {
            GameObject btnObj = new GameObject("ExecutionButton");
            btnObj.transform.SetParent(parent.transform, false);

            // Square 70x70 button, positioned right side per UI refactor
            RectTransform rect = btnObj.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(1f, 0.5f);
            rect.anchorMax = new Vector2(1f, 0.5f);
            rect.pivot = new Vector2(1f, 0.5f);
            rect.anchoredPosition = new Vector2(-20, 40); // 20px from right edge, slightly above center
            rect.sizeDelta = new Vector2(70, 70);

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

            // Checkmark icon - use sprite if available from icon config
            var iconConfig = LoadIconConfig();
            Color voidBlack = new Color(0.04f, 0.04f, 0.04f);
            GameObject checkObj = CreateIconImage(circle, "Check", iconConfig?.CheckmarkIcon, new Vector2(36, 36), voidBlack, "\u2713", 28);
            RectTransform checkRect = checkObj.GetComponent<RectTransform>();
            checkRect.anchorMin = Vector2.zero;
            checkRect.anchorMax = Vector2.one;
            checkRect.sizeDelta = Vector2.zero;

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

        /// <summary>
        /// Creates the pause menu overlay for Combat scene.
        /// </summary>
        private static GameObject CreatePauseMenuOverlay(GameObject parent)
        {
            GameObject overlay = new GameObject("PauseMenuOverlay");
            overlay.transform.SetParent(parent.transform, false);

            RectTransform overlayRect = overlay.AddComponent<RectTransform>();
            overlayRect.anchorMin = Vector2.zero;
            overlayRect.anchorMax = Vector2.one;
            overlayRect.sizeDelta = Vector2.zero;

            CanvasGroup canvasGroup = overlay.AddComponent<CanvasGroup>();
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;

            // Dark background
            Image bgImage = overlay.AddComponent<Image>();
            bgImage.color = new Color(0f, 0f, 0f, 0.8f);

            // Menu panel (centered)
            GameObject menuPanel = new GameObject("MenuPanel");
            menuPanel.transform.SetParent(overlay.transform, false);
            RectTransform menuRect = menuPanel.AddComponent<RectTransform>();
            menuRect.anchorMin = new Vector2(0.5f, 0.5f);
            menuRect.anchorMax = new Vector2(0.5f, 0.5f);
            menuRect.sizeDelta = new Vector2(300, 280);

            Image menuBg = menuPanel.AddComponent<Image>();
            menuBg.color = new Color(0.1f, 0.08f, 0.15f, 0.98f);

            VerticalLayoutGroup menuLayout = menuPanel.AddComponent<VerticalLayoutGroup>();
            menuLayout.padding = new RectOffset(20, 20, 20, 20);
            menuLayout.spacing = 16f;
            menuLayout.childAlignment = TextAnchor.UpperCenter;
            menuLayout.childForceExpandWidth = true;
            menuLayout.childForceExpandHeight = false;

            // Title
            GameObject title = CreateText(menuPanel, "Title", "PAUSED", 28);
            title.GetComponent<TMP_Text>().fontStyle = TMPro.FontStyles.Bold;
            title.GetComponent<TMP_Text>().color = new Color(0f, 0.83f, 0.89f); // Soul cyan

            // Resume button
            GameObject resumeBtn = CreateMenuButton(menuPanel, "ResumeButton", "Resume");

            // Settings button
            GameObject settingsBtn = CreateMenuButton(menuPanel, "SettingsButton", "Settings");

            // Abandon button
            GameObject abandonBtn = CreateMenuButton(menuPanel, "AbandonButton", "Abandon Run");
            abandonBtn.GetComponent<Image>().color = new Color(1f, 0.27f, 0.27f, 0.8f); // Corruption red

            // Add PauseMenuOverlay component and wire references
            var pauseMenu = overlay.AddComponent<PauseMenuOverlay>();
            var so = new SerializedObject(pauseMenu);
            so.FindProperty("_overlay").objectReferenceValue = canvasGroup;
            so.FindProperty("_backgroundPanel").objectReferenceValue = bgImage;
            so.FindProperty("_menuPanel").objectReferenceValue = menuRect;
            so.FindProperty("_titleText").objectReferenceValue = title.GetComponent<TMP_Text>();
            so.FindProperty("_resumeButton").objectReferenceValue = resumeBtn.GetComponent<Button>();
            so.FindProperty("_settingsButton").objectReferenceValue = settingsBtn.GetComponent<Button>();
            so.FindProperty("_abandonButton").objectReferenceValue = abandonBtn.GetComponent<Button>();
            so.ApplyModifiedPropertiesWithoutUndo();

            overlay.SetActive(false);
            return overlay;
        }

        /// <summary>
        /// Creates the confirmation dialog for Combat and NullRift scenes.
        /// </summary>
        private static GameObject CreateConfirmationDialog(GameObject parent)
        {
            GameObject overlay = new GameObject("ConfirmationDialog");
            overlay.transform.SetParent(parent.transform, false);

            RectTransform overlayRect = overlay.AddComponent<RectTransform>();
            overlayRect.anchorMin = Vector2.zero;
            overlayRect.anchorMax = Vector2.one;
            overlayRect.sizeDelta = Vector2.zero;

            CanvasGroup canvasGroup = overlay.AddComponent<CanvasGroup>();
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;

            // Dark background
            Image bgImage = overlay.AddComponent<Image>();
            bgImage.color = new Color(0f, 0f, 0f, 0.8f);

            // Dialog panel (centered)
            GameObject dialogPanel = new GameObject("DialogPanel");
            dialogPanel.transform.SetParent(overlay.transform, false);
            RectTransform dialogRect = dialogPanel.AddComponent<RectTransform>();
            dialogRect.anchorMin = new Vector2(0.5f, 0.5f);
            dialogRect.anchorMax = new Vector2(0.5f, 0.5f);
            dialogRect.sizeDelta = new Vector2(400, 200);

            Image dialogBg = dialogPanel.AddComponent<Image>();
            dialogBg.color = new Color(0.1f, 0.08f, 0.15f, 0.98f);

            VerticalLayoutGroup dialogLayout = dialogPanel.AddComponent<VerticalLayoutGroup>();
            dialogLayout.padding = new RectOffset(20, 20, 16, 16);
            dialogLayout.spacing = 12f;
            dialogLayout.childAlignment = TextAnchor.UpperCenter;
            dialogLayout.childForceExpandWidth = true;
            dialogLayout.childForceExpandHeight = false;

            // Title
            GameObject title = CreateText(dialogPanel, "Title", "Confirm", 22);
            title.GetComponent<TMP_Text>().fontStyle = TMPro.FontStyles.Bold;
            title.GetComponent<TMP_Text>().color = new Color(0f, 0.83f, 0.89f); // Soul cyan

            // Message
            GameObject message = CreateText(dialogPanel, "Message", "Are you sure?", 14);
            message.GetComponent<TMP_Text>().alignment = TextAlignmentOptions.Center;
            var messageLayout = message.AddComponent<LayoutElement>();
            messageLayout.preferredHeight = 50;
            messageLayout.flexibleHeight = 1;

            // Button container
            GameObject buttonContainer = new GameObject("ButtonContainer");
            buttonContainer.transform.SetParent(dialogPanel.transform, false);
            HorizontalLayoutGroup btnLayout = buttonContainer.AddComponent<HorizontalLayoutGroup>();
            btnLayout.spacing = 16f;
            btnLayout.childAlignment = TextAnchor.MiddleCenter;
            btnLayout.childForceExpandWidth = true;
            btnLayout.childForceExpandHeight = false;
            var btnContainerLayout = buttonContainer.AddComponent<LayoutElement>();
            btnContainerLayout.preferredHeight = 40;

            // Cancel button
            GameObject cancelBtn = new GameObject("CancelButton");
            cancelBtn.transform.SetParent(buttonContainer.transform, false);
            Image cancelBg = cancelBtn.AddComponent<Image>();
            cancelBg.color = new Color(0.15f, 0.12f, 0.2f, 0.9f);
            Button cancelButton = cancelBtn.AddComponent<Button>();
            cancelButton.targetGraphic = cancelBg;
            var cancelLayout = cancelBtn.AddComponent<LayoutElement>();
            cancelLayout.flexibleWidth = 1;

            GameObject cancelLabel = CreateText(cancelBtn, "Label", "Cancel", 16);
            RectTransform cancelLabelRect = cancelLabel.GetComponent<RectTransform>();
            cancelLabelRect.anchorMin = Vector2.zero;
            cancelLabelRect.anchorMax = Vector2.one;
            cancelLabelRect.sizeDelta = Vector2.zero;
            cancelLabel.GetComponent<TMP_Text>().color = new Color(0f, 0.83f, 0.89f); // Soul cyan

            // Confirm button
            GameObject confirmBtn = new GameObject("ConfirmButton");
            confirmBtn.transform.SetParent(buttonContainer.transform, false);
            Image confirmBg = confirmBtn.AddComponent<Image>();
            confirmBg.color = new Color(1f, 0.27f, 0.27f, 0.9f); // Corruption red
            Button confirmButton = confirmBtn.AddComponent<Button>();
            confirmButton.targetGraphic = confirmBg;
            var confirmLayout = confirmBtn.AddComponent<LayoutElement>();
            confirmLayout.flexibleWidth = 1;

            GameObject confirmLabel = CreateText(confirmBtn, "Label", "Confirm", 16);
            RectTransform confirmLabelRect = confirmLabel.GetComponent<RectTransform>();
            confirmLabelRect.anchorMin = Vector2.zero;
            confirmLabelRect.anchorMax = Vector2.one;
            confirmLabelRect.sizeDelta = Vector2.zero;
            confirmLabel.GetComponent<TMP_Text>().fontStyle = TMPro.FontStyles.Bold;

            // Add ConfirmationDialog component and wire references
            var dialog = overlay.AddComponent<ConfirmationDialog>();
            var so = new SerializedObject(dialog);
            so.FindProperty("_overlay").objectReferenceValue = canvasGroup;
            so.FindProperty("_backgroundPanel").objectReferenceValue = bgImage;
            so.FindProperty("_dialogPanel").objectReferenceValue = dialogRect;
            so.FindProperty("_titleText").objectReferenceValue = title.GetComponent<TMP_Text>();
            so.FindProperty("_messageText").objectReferenceValue = message.GetComponent<TMP_Text>();
            so.FindProperty("_confirmButton").objectReferenceValue = confirmButton;
            so.FindProperty("_confirmButtonText").objectReferenceValue = confirmLabel.GetComponent<TMP_Text>();
            so.FindProperty("_cancelButton").objectReferenceValue = cancelButton;
            so.FindProperty("_cancelButtonText").objectReferenceValue = cancelLabel.GetComponent<TMP_Text>();
            so.ApplyModifiedPropertiesWithoutUndo();

            overlay.SetActive(false);
            return overlay;
        }

        /// <summary>
        /// Creates the NullStateModal overlay for when a Requiem enters Null State.
        /// Shows effects and requires player confirmation to continue.
        /// </summary>
        private static GameObject CreateNullStateModal(GameObject parent)
        {
            GameObject overlay = new GameObject("NullStateModal");
            overlay.transform.SetParent(parent.transform, false);

            RectTransform overlayRect = overlay.AddComponent<RectTransform>();
            overlayRect.anchorMin = Vector2.zero;
            overlayRect.anchorMax = Vector2.one;
            overlayRect.sizeDelta = Vector2.zero;

            CanvasGroup canvasGroup = overlay.AddComponent<CanvasGroup>();
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;

            // Dark background with corruption vignette
            Image bgImage = overlay.AddComponent<Image>();
            bgImage.color = new Color(0f, 0f, 0f, 0.9f);

            // Main content panel (centered)
            GameObject contentPanel = new GameObject("ContentPanel");
            contentPanel.transform.SetParent(overlay.transform, false);
            RectTransform contentRect = contentPanel.AddComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0.5f, 0.5f);
            contentRect.anchorMax = new Vector2(0.5f, 0.5f);
            contentRect.sizeDelta = new Vector2(400, 350);

            Image contentBg = contentPanel.AddComponent<Image>();
            contentBg.color = new Color(0.12f, 0.05f, 0.1f, 0.98f);

            VerticalLayoutGroup contentLayout = contentPanel.AddComponent<VerticalLayoutGroup>();
            contentLayout.padding = new RectOffset(20, 20, 20, 20);
            contentLayout.spacing = 12f;
            contentLayout.childAlignment = TextAnchor.UpperCenter;
            contentLayout.childForceExpandWidth = true;
            contentLayout.childForceExpandHeight = false;

            // Title "NULL STATE"
            GameObject title = CreateText(contentPanel, "Title", "NULL STATE", 32);
            var titleTmp = title.GetComponent<TMP_Text>();
            titleTmp.fontStyle = TMPro.FontStyles.Bold;
            titleTmp.color = new Color(0.77f, 0.12f, 0.23f); // Null state red

            // Requiem name text
            GameObject requiemName = CreateText(contentPanel, "RequiemName", "Unknown Requiem", 24);
            var nameTmp = requiemName.GetComponent<TMP_Text>();
            nameTmp.fontStyle = TMPro.FontStyles.Bold;

            // Portrait placeholder
            GameObject portraitFrame = new GameObject("PortraitFrame");
            portraitFrame.transform.SetParent(contentPanel.transform, false);
            var portraitLayout = portraitFrame.AddComponent<LayoutElement>();
            portraitLayout.preferredHeight = 80;
            portraitLayout.preferredWidth = 80;
            Image portraitFrameImg = portraitFrame.AddComponent<Image>();
            portraitFrameImg.color = new Color(0.77f, 0.12f, 0.23f);

            GameObject portrait = new GameObject("Portrait");
            portrait.transform.SetParent(portraitFrame.transform, false);
            RectTransform portraitRect = portrait.AddComponent<RectTransform>();
            portraitRect.anchorMin = new Vector2(0.1f, 0.1f);
            portraitRect.anchorMax = new Vector2(0.9f, 0.9f);
            portraitRect.sizeDelta = Vector2.zero;
            Image portraitImg = portrait.AddComponent<Image>();
            portraitImg.color = new Color(0.3f, 0.1f, 0.15f);

            // Effects text
            GameObject effects = CreateText(contentPanel, "Effects", "-33% Current HP\n+50% Damage", 18);
            effects.GetComponent<TMP_Text>().alignment = TextAlignmentOptions.Center;

            // Art name
            GameObject artName = CreateText(contentPanel, "ArtName", "REQUIEM ART: Infernal Cascade", 14);
            var artTmp = artName.GetComponent<TMP_Text>();
            artTmp.fontStyle = TMPro.FontStyles.Italic;
            artTmp.color = new Color(0f, 0.83f, 0.89f);

            // Disclaimer
            GameObject disclaimer = CreateText(contentPanel, "Disclaimer", "Corruption resets to 50% after combat", 11);
            disclaimer.GetComponent<TMP_Text>().color = new Color(0.6f, 0.5f, 0.55f);

            // Unleash button
            GameObject unleashBtn = CreateMenuButton(contentPanel, "UnleashButton", "UNLEASH");
            Image btnImg = unleashBtn.GetComponent<Image>();
            btnImg.color = new Color(0.77f, 0.12f, 0.23f, 0.9f);
            unleashBtn.GetComponentInChildren<TMP_Text>().fontStyle = TMPro.FontStyles.Bold;

            // Add NullStateModal component and wire references
            var modal = overlay.AddComponent<NullStateModal>();
            var so = new SerializedObject(modal);
            so.FindProperty("_overlay").objectReferenceValue = canvasGroup;
            so.FindProperty("_backgroundPanel").objectReferenceValue = bgImage;
            so.FindProperty("_requiemPortrait").objectReferenceValue = portraitImg;
            so.FindProperty("_portraitFrame").objectReferenceValue = portraitFrameImg;
            so.FindProperty("_requiemNameText").objectReferenceValue = requiemName.GetComponent<TMP_Text>();
            so.FindProperty("_titleText").objectReferenceValue = title.GetComponent<TMP_Text>();
            so.FindProperty("_effectsText").objectReferenceValue = effects.GetComponent<TMP_Text>();
            so.FindProperty("_artNameText").objectReferenceValue = artName.GetComponent<TMP_Text>();
            so.FindProperty("_disclaimerText").objectReferenceValue = disclaimer.GetComponent<TMP_Text>();
            so.FindProperty("_unleashButton").objectReferenceValue = unleashBtn.GetComponent<Button>();
            so.FindProperty("_unleashButtonText").objectReferenceValue = unleashBtn.GetComponentInChildren<TMP_Text>();
            so.ApplyModifiedPropertiesWithoutUndo();

            // Keep active for event subscription
            return overlay;
        }

        /// <summary>
        /// Creates the RelicDisplayBar for showing owned relics in combat.
        /// Positioned in left sidebar area above PartySidebar.
        /// </summary>
        private static GameObject CreateRelicDisplayBar(GameObject parent)
        {
            GameObject barObj = new GameObject("RelicDisplayBar");
            barObj.transform.SetParent(parent.transform, false);

            RectTransform rect = barObj.AddComponent<RectTransform>();
            // Position at top-left, below TopHUD
            rect.anchorMin = new Vector2(0, 0.35f);
            rect.anchorMax = new Vector2(0, 0.87f);
            rect.pivot = new Vector2(0, 0.5f);
            rect.anchoredPosition = new Vector2(8, 0);
            rect.sizeDelta = new Vector2(50, 0); // 50px wide, stretches vertically

            Image bg = barObj.AddComponent<Image>();
            bg.color = new Color(0.08f, 0.06f, 0.12f, 0.7f);

            // Relic container with vertical layout
            GameObject relicContainer = new GameObject("RelicContainer");
            relicContainer.transform.SetParent(barObj.transform, false);
            RectTransform containerRect = relicContainer.AddComponent<RectTransform>();
            containerRect.anchorMin = Vector2.zero;
            containerRect.anchorMax = Vector2.one;
            containerRect.offsetMin = new Vector2(4, 4);
            containerRect.offsetMax = new Vector2(-4, -4);

            VerticalLayoutGroup layout = relicContainer.AddComponent<VerticalLayoutGroup>();
            layout.spacing = 4f;
            layout.padding = new RectOffset(2, 2, 2, 2);
            layout.childAlignment = TextAnchor.UpperCenter;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;

            // Tooltip panel (hidden by default)
            GameObject tooltipPanel = new GameObject("TooltipPanel");
            tooltipPanel.transform.SetParent(barObj.transform, false);
            RectTransform tooltipRect = tooltipPanel.AddComponent<RectTransform>();
            tooltipRect.anchorMin = new Vector2(1, 0.5f);
            tooltipRect.anchorMax = new Vector2(1, 0.5f);
            tooltipRect.pivot = new Vector2(0, 0.5f);
            tooltipRect.anchoredPosition = new Vector2(8, 0);
            tooltipRect.sizeDelta = new Vector2(200, 80);

            Image tooltipBg = tooltipPanel.AddComponent<Image>();
            tooltipBg.color = new Color(0.1f, 0.08f, 0.15f, 0.95f);

            VerticalLayoutGroup tooltipLayout = tooltipPanel.AddComponent<VerticalLayoutGroup>();
            tooltipLayout.padding = new RectOffset(8, 8, 6, 6);
            tooltipLayout.spacing = 4f;
            tooltipLayout.childForceExpandWidth = true;
            tooltipLayout.childForceExpandHeight = false;

            // Tooltip content
            GameObject tooltipName = CreateText(tooltipPanel, "TooltipName", "Relic Name", 14);
            tooltipName.GetComponent<TMP_Text>().fontStyle = TMPro.FontStyles.Bold;

            GameObject tooltipDesc = CreateText(tooltipPanel, "TooltipDescription", "Relic description goes here.", 11);
            tooltipDesc.GetComponent<TMP_Text>().alignment = TextAlignmentOptions.TopLeft;

            GameObject tooltipRarity = CreateText(tooltipPanel, "TooltipRarity", "Common", 10);
            tooltipRarity.GetComponent<TMP_Text>().fontStyle = TMPro.FontStyles.Italic;

            tooltipPanel.SetActive(false);

            // Add RelicDisplayBar component and wire references
            var relicBar = barObj.AddComponent<RelicDisplayBar>();
            var so = new SerializedObject(relicBar);
            so.FindProperty("_relicContainer").objectReferenceValue = relicContainer.transform;
            so.FindProperty("_tooltipPanel").objectReferenceValue = tooltipPanel;
            so.FindProperty("_tooltipName").objectReferenceValue = tooltipName.GetComponent<TMP_Text>();
            so.FindProperty("_tooltipDescription").objectReferenceValue = tooltipDesc.GetComponent<TMP_Text>();
            so.FindProperty("_tooltipRarity").objectReferenceValue = tooltipRarity.GetComponent<TMP_Text>();
            so.ApplyModifiedPropertiesWithoutUndo();

            return barObj;
        }

        /// <summary>
        /// Creates the DeckViewerModal overlay for viewing/removing cards.
        /// Used by shop's card removal service.
        /// </summary>
        private static GameObject CreateDeckViewerModal(GameObject parent)
        {
            GameObject modal = new GameObject("DeckViewerModal");
            modal.transform.SetParent(parent.transform, false);

            RectTransform modalRect = modal.AddComponent<RectTransform>();
            modalRect.anchorMin = Vector2.zero;
            modalRect.anchorMax = Vector2.one;
            modalRect.sizeDelta = Vector2.zero;

            // === Modal Panel (holds all content) ===
            GameObject modalPanel = new GameObject("ModalPanel");
            modalPanel.transform.SetParent(modal.transform, false);
            RectTransform panelRect = modalPanel.AddComponent<RectTransform>();
            panelRect.anchorMin = Vector2.zero;
            panelRect.anchorMax = Vector2.one;
            panelRect.sizeDelta = Vector2.zero;
            modalPanel.SetActive(false);

            // === Canvas Group for fade animation ===
            CanvasGroup canvasGroup = modalPanel.AddComponent<CanvasGroup>();
            canvasGroup.alpha = 0f;

            // === Background Overlay (semi-transparent) ===
            Image bgOverlay = modalPanel.AddComponent<Image>();
            bgOverlay.color = new Color(0f, 0f, 0f, 0.85f);

            // === Content Container (centered panel) ===
            GameObject contentPanel = new GameObject("ContentPanel");
            contentPanel.transform.SetParent(modalPanel.transform, false);
            RectTransform contentRect = contentPanel.AddComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0.1f, 0.08f);
            contentRect.anchorMax = new Vector2(0.9f, 0.92f);
            contentRect.sizeDelta = Vector2.zero;

            Image contentBg = contentPanel.AddComponent<Image>();
            contentBg.color = new Color(0.08f, 0.06f, 0.12f, 0.98f);

            // === Header ===
            GameObject header = new GameObject("Header");
            header.transform.SetParent(contentPanel.transform, false);
            RectTransform headerRect = header.AddComponent<RectTransform>();
            headerRect.anchorMin = new Vector2(0, 0.88f);
            headerRect.anchorMax = new Vector2(1, 1);
            headerRect.sizeDelta = Vector2.zero;

            Image headerBg = header.AddComponent<Image>();
            headerBg.color = new Color(0.1f, 0.08f, 0.15f, 0.95f);

            // Title
            GameObject title = CreateText(header, "Title", "YOUR DECK", 28);
            RectTransform titleRect = title.GetComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0, 0);
            titleRect.anchorMax = new Vector2(1, 1);
            titleRect.offsetMin = new Vector2(20, 0);
            titleRect.offsetMax = new Vector2(-20, 0);
            var titleTmp = title.GetComponent<TMP_Text>();
            titleTmp.alignment = TextAlignmentOptions.Left;
            titleTmp.fontStyle = TMPro.FontStyles.Bold;
            titleTmp.color = new Color(0.83f, 0.69f, 0.22f); // Soul gold

            // Instruction text
            GameObject instruction = CreateText(header, "Instruction", "", 14);
            RectTransform instrRect = instruction.GetComponent<RectTransform>();
            instrRect.anchorMin = new Vector2(0.5f, 0);
            instrRect.anchorMax = new Vector2(1, 1);
            instrRect.offsetMin = new Vector2(0, 0);
            instrRect.offsetMax = new Vector2(-20, 0);
            var instrTmp = instruction.GetComponent<TMP_Text>();
            instrTmp.alignment = TextAlignmentOptions.Right;
            instrTmp.color = new Color(0.7f, 0.7f, 0.7f);
            instruction.SetActive(false);

            // === Scroll Area ===
            GameObject scrollArea = new GameObject("ScrollArea");
            scrollArea.transform.SetParent(contentPanel.transform, false);
            RectTransform scrollRect = scrollArea.AddComponent<RectTransform>();
            scrollRect.anchorMin = new Vector2(0, 0.12f);
            scrollRect.anchorMax = new Vector2(1, 0.86f);
            scrollRect.sizeDelta = Vector2.zero;

            ScrollRect scroll = scrollArea.AddComponent<ScrollRect>();
            scroll.horizontal = false;
            scroll.vertical = true;
            scroll.scrollSensitivity = 20f;

            Image scrollMask = scrollArea.AddComponent<Image>();
            scrollMask.color = new Color(0.05f, 0.03f, 0.08f, 0.5f);
            scrollArea.AddComponent<Mask>().showMaskGraphic = true;

            // === Viewport ===
            GameObject viewport = new GameObject("Viewport");
            viewport.transform.SetParent(scrollArea.transform, false);
            RectTransform viewportRect = viewport.AddComponent<RectTransform>();
            viewportRect.anchorMin = Vector2.zero;
            viewportRect.anchorMax = Vector2.one;
            viewportRect.sizeDelta = Vector2.zero;

            // === Card Container (GridLayoutGroup) ===
            GameObject cardContainer = new GameObject("CardContainer");
            cardContainer.transform.SetParent(viewport.transform, false);
            RectTransform cardContainerRect = cardContainer.AddComponent<RectTransform>();
            cardContainerRect.anchorMin = new Vector2(0, 1);
            cardContainerRect.anchorMax = new Vector2(1, 1);
            cardContainerRect.pivot = new Vector2(0.5f, 1);
            cardContainerRect.sizeDelta = new Vector2(0, 300);

            GridLayoutGroup grid = cardContainer.AddComponent<GridLayoutGroup>();
            grid.cellSize = new Vector2(200, 280); // Match Card.prefab native size (same as Sanctuary)
            grid.spacing = new Vector2(20, 20);
            grid.startCorner = GridLayoutGroup.Corner.UpperLeft;
            grid.startAxis = GridLayoutGroup.Axis.Horizontal;
            grid.childAlignment = TextAnchor.UpperCenter;
            grid.padding = new RectOffset(30, 30, 20, 20);

            ContentSizeFitter fitter = cardContainer.AddComponent<ContentSizeFitter>();
            fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            scroll.content = cardContainerRect;
            scroll.viewport = viewportRect;

            // === Footer / Button Area ===
            GameObject footer = new GameObject("Footer");
            footer.transform.SetParent(contentPanel.transform, false);
            RectTransform footerRect = footer.AddComponent<RectTransform>();
            footerRect.anchorMin = new Vector2(0, 0);
            footerRect.anchorMax = new Vector2(1, 0.1f);
            footerRect.sizeDelta = Vector2.zero;

            HorizontalLayoutGroup footerLayout = footer.AddComponent<HorizontalLayoutGroup>();
            footerLayout.padding = new RectOffset(20, 20, 8, 8);
            footerLayout.spacing = 16;
            footerLayout.childAlignment = TextAnchor.MiddleRight;
            footerLayout.childForceExpandWidth = false;
            footerLayout.childForceExpandHeight = true;

            // Spacer to push buttons right
            GameObject spacer = new GameObject("Spacer");
            spacer.transform.SetParent(footer.transform, false);
            var spacerLayout = spacer.AddComponent<LayoutElement>();
            spacerLayout.flexibleWidth = 1;

            // Cancel button
            GameObject cancelBtn = new GameObject("CancelButton");
            cancelBtn.transform.SetParent(footer.transform, false);
            Image cancelBg = cancelBtn.AddComponent<Image>();
            cancelBg.color = new Color(0.15f, 0.12f, 0.2f, 0.9f);
            Button cancelButton = cancelBtn.AddComponent<Button>();
            cancelButton.targetGraphic = cancelBg;
            var cancelLayout = cancelBtn.AddComponent<LayoutElement>();
            cancelLayout.preferredWidth = 120;
            cancelLayout.preferredHeight = 40;

            GameObject cancelText = CreateText(cancelBtn, "Text", "Cancel", 16);
            RectTransform cancelTextRect = cancelText.GetComponent<RectTransform>();
            cancelTextRect.anchorMin = Vector2.zero;
            cancelTextRect.anchorMax = Vector2.one;
            cancelTextRect.sizeDelta = Vector2.zero;
            cancelText.GetComponent<TMP_Text>().color = new Color(0f, 0.83f, 0.89f); // Soul cyan

            // Confirm button
            GameObject confirmBtn = new GameObject("ConfirmButton");
            confirmBtn.transform.SetParent(footer.transform, false);
            Image confirmBg = confirmBtn.AddComponent<Image>();
            confirmBg.color = new Color(0.77f, 0.12f, 0.23f, 0.9f); // Crimson
            Button confirmButton = confirmBtn.AddComponent<Button>();
            confirmButton.targetGraphic = confirmBg;
            var confirmLayout = confirmBtn.AddComponent<LayoutElement>();
            confirmLayout.preferredWidth = 120;
            confirmLayout.preferredHeight = 40;

            GameObject confirmText = CreateText(confirmBtn, "Text", "Close", 16);
            RectTransform confirmTextRect = confirmText.GetComponent<RectTransform>();
            confirmTextRect.anchorMin = Vector2.zero;
            confirmTextRect.anchorMax = Vector2.one;
            confirmTextRect.sizeDelta = Vector2.zero;
            confirmText.GetComponent<TMP_Text>().fontStyle = TMPro.FontStyles.Bold;

            // === Add DeckViewerModal component and wire references ===
            var deckViewer = modal.AddComponent<DeckViewerModal>();
            var so = new SerializedObject(deckViewer);
            so.FindProperty("_modalPanel").objectReferenceValue = modalPanel;
            so.FindProperty("_canvasGroup").objectReferenceValue = canvasGroup;
            so.FindProperty("_backgroundOverlay").objectReferenceValue = bgOverlay;
            so.FindProperty("_titleText").objectReferenceValue = titleTmp;
            so.FindProperty("_instructionText").objectReferenceValue = instrTmp;
            so.FindProperty("_cardContainer").objectReferenceValue = cardContainer.transform;
            so.FindProperty("_scrollRect").objectReferenceValue = scroll;
            so.FindProperty("_confirmButton").objectReferenceValue = confirmButton;
            so.FindProperty("_confirmButtonText").objectReferenceValue = confirmText.GetComponent<TMP_Text>();
            so.FindProperty("_cancelButton").objectReferenceValue = cancelButton;
            so.FindProperty("_fadeInDuration").floatValue = 0.3f;
            so.FindProperty("_fadeOutDuration").floatValue = 0.2f;
            so.FindProperty("_normalSlotColor").colorValue = new Color(0.15f, 0.15f, 0.2f);
            so.FindProperty("_selectedSlotColor").colorValue = new Color(0.2f, 0.35f, 0.5f);
            so.FindProperty("_hoverSlotColor").colorValue = new Color(0.2f, 0.25f, 0.3f);

            // Wire Card.prefab for proper card display (matching Sanctuary upgrade window)
            var cardPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Project/Prefabs/Cards/Card.prefab");
            if (cardPrefab != null)
            {
                so.FindProperty("_cardSlotPrefab").objectReferenceValue = cardPrefab;
                Debug.Log("[ProductionSceneSetupGenerator] Wired Card.prefab to DeckViewerModal._cardSlotPrefab");
            }
            else
            {
                Debug.LogWarning("[ProductionSceneSetupGenerator] Card.prefab not found for DeckViewerModal - run HNR > 2. Prefabs > UI > Card Prefab first");
            }

            so.ApplyModifiedPropertiesWithoutUndo();

            Debug.Log("[ProductionSceneSetupGenerator] Created DeckViewerModal");
            return modal;
        }

        // ============================================
        // Missions Scene
        // ============================================

        public static void SetupMissionsScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            CreateMainCamera("Missions");
            CreateEventSystem();

            var canvas = CreateMainCanvas("MissionsCanvas");
            CreateBackground(canvas, new Color(0.08f, 0.08f, 0.12f));
            CreateMissionsScreen(canvas.transform);
            CreateSettingsOverlay(canvas.transform);

            string scenePath = $"{SCENES_PATH}/Missions.unity";
            EditorSceneManager.SaveScene(scene, scenePath);
            Debug.Log($"[ProductionSceneSetupGenerator] Created Missions scene at {scenePath}");
        }

        private static void CreateMissionsScreen(Transform parent)
        {
            var screenContainer = CreateSimpleUIObject("ScreenContainer", parent);
            SetFullStretchRect(screenContainer);

            var missionsScreenObj = CreateSimpleUIObject("MissionsScreen", screenContainer.transform);
            SetFullStretchRect(missionsScreenObj);
            var missionsScreen = missionsScreenObj.AddComponent<MissionsScreen>();

            // Header
            var header = CreateSimpleUIObject("Header", missionsScreenObj.transform);
            var headerRect = header.GetComponent<RectTransform>();
            headerRect.anchorMin = new Vector2(0, 0.9f);
            headerRect.anchorMax = new Vector2(1, 1);
            headerRect.offsetMin = new Vector2(20, 0);
            headerRect.offsetMax = new Vector2(-20, -10);

            // Back button
            var backButton = CreateSimpleButton("BackButton", header.transform, "<");
            var backButtonRect = backButton.GetComponent<RectTransform>();
            backButtonRect.anchorMin = new Vector2(0, 0.5f);
            backButtonRect.anchorMax = new Vector2(0, 0.5f);
            backButtonRect.pivot = new Vector2(0, 0.5f);
            backButtonRect.sizeDelta = new Vector2(60, 60);
            backButtonRect.anchoredPosition = Vector2.zero;

            // Title
            var title = CreateSimpleTextObject("Title", header.transform, "Missions");
            var titleRect = title.GetComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0, 0.5f);
            titleRect.anchorMax = new Vector2(0, 0.5f);
            titleRect.pivot = new Vector2(0, 0.5f);
            titleRect.sizeDelta = new Vector2(200, 50);
            titleRect.anchoredPosition = new Vector2(80, 0);
            var titleText = title.GetComponent<TMP_Text>();
            titleText.fontSize = 32;
            titleText.fontStyle = FontStyles.Bold;

            // Settings button
            var settingsButton = CreateSimpleButton("SettingsButton", header.transform, "=");
            var settingsButtonRect = settingsButton.GetComponent<RectTransform>();
            settingsButtonRect.anchorMin = new Vector2(1, 0.5f);
            settingsButtonRect.anchorMax = new Vector2(1, 0.5f);
            settingsButtonRect.pivot = new Vector2(1, 0.5f);
            settingsButtonRect.sizeDelta = new Vector2(60, 60);
            settingsButtonRect.anchoredPosition = Vector2.zero;

            // Content area
            var content = CreateSimpleUIObject("Content", missionsScreenObj.transform);
            var contentRect = content.GetComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0.1f, 0.15f);
            contentRect.anchorMax = new Vector2(0.9f, 0.85f);
            contentRect.offsetMin = Vector2.zero;
            contentRect.offsetMax = Vector2.zero;

            var hlg = content.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 50;
            hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.childForceExpandWidth = true;
            hlg.childForceExpandHeight = true;

            // Story button (placeholder)
            var storyButton = CreateLargeButtonWithSubtitle("StoryButton", content.transform, "Story", "Coming Soon");

            // Battle Mission button
            var battleMissionButton = CreateLargeButtonWithSubtitle("BattleMissionButton", content.transform, "Battle Mission", "Challenge the Null Rift");

            // Wire references
            var so = new SerializedObject(missionsScreen);
            so.FindProperty("_backButton").objectReferenceValue = backButton.GetComponent<Button>();
            so.FindProperty("_titleText").objectReferenceValue = title.GetComponent<TMP_Text>();
            so.FindProperty("_settingsButton").objectReferenceValue = settingsButton.GetComponent<Button>();
            so.FindProperty("_storyMissionButton").objectReferenceValue = storyButton.GetComponent<Button>();
            so.FindProperty("_storyMissionTitle").objectReferenceValue = storyButton.transform.Find("Title")?.GetComponent<TMP_Text>();
            so.FindProperty("_storyMissionSubtitle").objectReferenceValue = storyButton.transform.Find("Subtitle")?.GetComponent<TMP_Text>();
            so.FindProperty("_battleMissionButton").objectReferenceValue = battleMissionButton.GetComponent<Button>();
            so.FindProperty("_battleMissionTitle").objectReferenceValue = battleMissionButton.transform.Find("Title")?.GetComponent<TMP_Text>();
            so.FindProperty("_battleMissionSubtitle").objectReferenceValue = battleMissionButton.transform.Find("Subtitle")?.GetComponent<TMP_Text>();
            so.ApplyModifiedProperties();
        }

        // ============================================
        // BattleMission Scene
        // ============================================

        public static void SetupBattleMissionScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            CreateMainCamera("BattleMission");
            CreateEventSystem();

            var canvas = CreateMainCanvas("BattleMissionCanvas");
            CreateBackground(canvas, new Color(0.08f, 0.08f, 0.12f));
            CreateBattleMissionScreen(canvas.transform);

            // Create overlay container for modals
            var overlayContainer = CreateSimpleUIObject("OverlayContainer", canvas.transform);
            SetFullStretchRect(overlayContainer);

            // Add RequiemSelectionScreen overlay
            CreateBMRequiemSelectionScreen(overlayContainer.transform);

            // Add SettingsOverlay
            CreateSettingsOverlay(canvas.transform);

            string scenePath = $"{SCENES_PATH}/BattleMission.unity";
            EditorSceneManager.SaveScene(scene, scenePath);
            Debug.Log($"[ProductionSceneSetupGenerator] Created BattleMission scene at {scenePath}");
        }

        private static void CreateBattleMissionScreen(Transform parent)
        {
            var screenContainer = CreateSimpleUIObject("ScreenContainer", parent);
            SetFullStretchRect(screenContainer);

            var screenObj = CreateSimpleUIObject("BattleMissionScreen", screenContainer.transform);
            SetFullStretchRect(screenObj);
            var screen = screenObj.AddComponent<BattleMissionScreen>();

            // Header
            var header = CreateSimpleUIObject("Header", screenObj.transform);
            var headerRect = header.GetComponent<RectTransform>();
            headerRect.anchorMin = new Vector2(0, 0.9f);
            headerRect.anchorMax = new Vector2(1, 1);
            headerRect.offsetMin = new Vector2(20, 0);
            headerRect.offsetMax = new Vector2(-20, -10);

            var backButton = CreateSimpleButton("BackButton", header.transform, "<");
            var backRect = backButton.GetComponent<RectTransform>();
            backRect.anchorMin = new Vector2(0, 0.5f);
            backRect.anchorMax = new Vector2(0, 0.5f);
            backRect.pivot = new Vector2(0, 0.5f);
            backRect.sizeDelta = new Vector2(60, 60);

            var title = CreateSimpleTextObject("Title", header.transform, "Battle Mission");
            var titleRect = title.GetComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0, 0.5f);
            titleRect.anchorMax = new Vector2(0, 0.5f);
            titleRect.pivot = new Vector2(0, 0.5f);
            titleRect.sizeDelta = new Vector2(300, 50);
            titleRect.anchoredPosition = new Vector2(80, 0);
            title.GetComponent<TMP_Text>().fontSize = 32;

            var settingsButton = CreateSimpleButton("SettingsButton", header.transform, "=");
            var settingsRect = settingsButton.GetComponent<RectTransform>();
            settingsRect.anchorMin = new Vector2(1, 0.5f);
            settingsRect.anchorMax = new Vector2(1, 0.5f);
            settingsRect.pivot = new Vector2(1, 0.5f);
            settingsRect.sizeDelta = new Vector2(60, 60);

            // Zone container
            var zoneContainer = CreateSimpleUIObject("ZoneContainer", screenObj.transform);
            var zoneRect = zoneContainer.GetComponent<RectTransform>();
            zoneRect.anchorMin = new Vector2(0.1f, 0.3f);
            zoneRect.anchorMax = new Vector2(0.9f, 0.8f);
            zoneRect.offsetMin = Vector2.zero;
            zoneRect.offsetMax = Vector2.zero;

            var hlg = zoneContainer.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 30;
            hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.childForceExpandWidth = true;
            hlg.childForceExpandHeight = true;

            // Zone nodes
            var zone1 = CreateZoneNode("Zone1Node", zoneContainer.transform, 1, "The Outer Reaches");
            var zone2 = CreateZoneNode("Zone2Node", zoneContainer.transform, 2, "The Hollow Depths");
            var zone3 = CreateZoneNode("Zone3Node", zoneContainer.transform, 3, "The Null Core");

            // Difficulty selector (bottom center, matching reference design)
            var difficultySection = CreateDifficultySelectorUI(screenObj.transform);
            var diffSelector = difficultySection.GetComponent<DifficultySelector>();

            // Connection lines between zones
            CreateZoneConnectionLines(screenObj.transform, zone1, zone2, zone3);

            // Wire references
            var so = new SerializedObject(screen);
            so.FindProperty("_backButton").objectReferenceValue = backButton.GetComponent<Button>();
            so.FindProperty("_titleText").objectReferenceValue = title.GetComponent<TMP_Text>();
            so.FindProperty("_settingsButton").objectReferenceValue = settingsButton.GetComponent<Button>();
            so.FindProperty("_zoneContainer").objectReferenceValue = zoneContainer.transform;
            so.FindProperty("_zone1Node").objectReferenceValue = zone1.GetComponent<ZoneNodeButton>();
            so.FindProperty("_zone2Node").objectReferenceValue = zone2.GetComponent<ZoneNodeButton>();
            so.FindProperty("_zone3Node").objectReferenceValue = zone3.GetComponent<ZoneNodeButton>();
            so.FindProperty("_difficultySelector").objectReferenceValue = diffSelector;
            so.ApplyModifiedProperties();
        }

        private static GameObject CreateDifficultySelectorUI(Transform parent)
        {
            var container = CreateSimpleUIObject("DifficultySelector", parent);
            var containerRect = container.GetComponent<RectTransform>();
            // Position at bottom center like reference image
            containerRect.anchorMin = new Vector2(0.25f, 0.03f);
            containerRect.anchorMax = new Vector2(0.75f, 0.12f);
            containerRect.offsetMin = Vector2.zero;
            containerRect.offsetMax = Vector2.zero;

            var bgImage = container.AddComponent<Image>();
            bgImage.color = new Color(0.15f, 0.15f, 0.2f, 0.9f);

            var diffSelector = container.AddComponent<DifficultySelector>();

            // Create a buttons container for the layout group (NOT the selector itself)
            var buttonsContainer = CreateSimpleUIObject("ButtonsContainer", container.transform);
            SetFullStretchRect(buttonsContainer);
            buttonsContainer.GetComponent<RectTransform>().offsetMin = new Vector2(10, 5);
            buttonsContainer.GetComponent<RectTransform>().offsetMax = new Vector2(-10, -5);

            var hlg = buttonsContainer.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 0;
            hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.childForceExpandWidth = true;
            hlg.childForceExpandHeight = true;

            // Create difficulty buttons INSIDE the buttons container (so they're part of layout)
            var easyButton = CreateDifficultyButton(buttonsContainer.transform, "EASY");
            var normalButton = CreateDifficultyButton(buttonsContainer.transform, "NORMAL");
            var hardButton = CreateDifficultyButton(buttonsContainer.transform, "HARD");

            // Selection indicator is OUTSIDE the layout group, positioned absolutely
            var selectionIndicator = CreateSimpleUIObject("SelectionIndicator", container.transform);
            var indicatorRect = selectionIndicator.GetComponent<RectTransform>();
            // Position for first button (Easy) - spans roughly 1/3 width
            indicatorRect.anchorMin = new Vector2(0, 0);
            indicatorRect.anchorMax = new Vector2(0.333f, 1);
            indicatorRect.offsetMin = new Vector2(10, 5);
            indicatorRect.offsetMax = new Vector2(-5, -5);
            var indicatorImage = selectionIndicator.AddComponent<Image>();
            indicatorImage.color = new Color(0.9f, 0.7f, 0.2f, 0.3f);
            // Move indicator behind buttons
            selectionIndicator.transform.SetAsFirstSibling();

            var so = new SerializedObject(diffSelector);
            so.FindProperty("_easyButton").objectReferenceValue = easyButton.GetComponent<Button>();
            so.FindProperty("_normalButton").objectReferenceValue = normalButton.GetComponent<Button>();
            so.FindProperty("_hardButton").objectReferenceValue = hardButton.GetComponent<Button>();
            so.FindProperty("_easyText").objectReferenceValue = easyButton.GetComponentInChildren<TMP_Text>();
            so.FindProperty("_normalText").objectReferenceValue = normalButton.GetComponentInChildren<TMP_Text>();
            so.FindProperty("_hardText").objectReferenceValue = hardButton.GetComponentInChildren<TMP_Text>();
            so.FindProperty("_selectionIndicator").objectReferenceValue = indicatorImage;
            so.ApplyModifiedProperties();

            return container;
        }

        private static GameObject CreateDifficultyButton(Transform parent, string text)
        {
            var buttonObj = CreateSimpleUIObject(text + "Button", parent);
            var image = buttonObj.AddComponent<Image>();
            image.color = new Color(0.2f, 0.2f, 0.25f, 0.8f);

            var button = buttonObj.AddComponent<Button>();
            button.targetGraphic = image;

            var textObj = CreateSimpleTextObject("Text", buttonObj.transform, text);
            SetFullStretchRect(textObj);
            var tmpText = textObj.GetComponent<TMP_Text>();
            tmpText.alignment = TextAlignmentOptions.Center;
            tmpText.fontSize = 20;
            tmpText.fontStyle = FontStyles.Bold;

            return buttonObj;
        }

        private static void CreateZoneConnectionLines(Transform parent, GameObject zone1, GameObject zone2, GameObject zone3)
        {
            var linesContainer = CreateSimpleUIObject("ConnectionLines", parent);
            var linesRect = linesContainer.GetComponent<RectTransform>();
            // Same area as zone container
            linesRect.anchorMin = new Vector2(0.1f, 0.3f);
            linesRect.anchorMax = new Vector2(0.9f, 0.8f);
            linesRect.offsetMin = Vector2.zero;
            linesRect.offsetMax = Vector2.zero;

            // Line from Zone 1 to Zone 2 (short line between nodes, not spanning entire width)
            // Zones are at roughly 1/6, 3/6, 5/6 of container width due to HLG
            // Line 1->2 goes from edge of zone1 to edge of zone2
            var line1 = CreateConnectionLine(linesContainer.transform, "Line1to2");
            var line1Rect = line1.GetComponent<RectTransform>();
            line1Rect.anchorMin = new Vector2(0.25f, 0.48f);  // After zone 1
            line1Rect.anchorMax = new Vector2(0.42f, 0.52f);  // Before zone 2
            line1Rect.offsetMin = Vector2.zero;
            line1Rect.offsetMax = Vector2.zero;

            // Line from Zone 2 to Zone 3 (short line between nodes)
            var line2 = CreateConnectionLine(linesContainer.transform, "Line2to3");
            var line2Rect = line2.GetComponent<RectTransform>();
            line2Rect.anchorMin = new Vector2(0.58f, 0.48f);  // After zone 2
            line2Rect.anchorMax = new Vector2(0.75f, 0.52f);  // Before zone 3
            line2Rect.offsetMin = Vector2.zero;
            line2Rect.offsetMax = Vector2.zero;

            // Put lines behind zone nodes
            linesContainer.transform.SetAsFirstSibling();
        }

        private static GameObject CreateConnectionLine(Transform parent, string name)
        {
            var lineObj = CreateSimpleUIObject(name, parent);
            var lineImage = lineObj.AddComponent<Image>();
            lineImage.color = new Color(0.5f, 0.5f, 0.6f, 0.8f);
            return lineObj;
        }

        private static GameObject CreateZoneNode(string name, Transform parent, int zoneNumber, string zoneName)
        {
            var nodeObj = CreateSimpleUIObject(name, parent);
            var nodeImage = nodeObj.AddComponent<Image>();
            nodeImage.color = new Color(0.2f, 0.3f, 0.4f, 0.9f);

            var button = nodeObj.AddComponent<Button>();
            var nodeComponent = nodeObj.AddComponent<ZoneNodeButton>();

            var numberText = CreateSimpleTextObject("ZoneNumber", nodeObj.transform, $"Zone {zoneNumber}");
            var numRect = numberText.GetComponent<RectTransform>();
            numRect.anchorMin = new Vector2(0, 0.6f);
            numRect.anchorMax = new Vector2(1, 0.9f);
            numRect.offsetMin = new Vector2(10, 0);
            numRect.offsetMax = new Vector2(-10, 0);
            numberText.GetComponent<TMP_Text>().fontSize = 24;

            var nameText = CreateSimpleTextObject("ZoneName", nodeObj.transform, zoneName);
            var nameRect = nameText.GetComponent<RectTransform>();
            nameRect.anchorMin = new Vector2(0, 0.3f);
            nameRect.anchorMax = new Vector2(1, 0.55f);
            nameRect.offsetMin = new Vector2(10, 0);
            nameRect.offsetMax = new Vector2(-10, 0);
            nameText.GetComponent<TMP_Text>().fontSize = 16;

            var so = new SerializedObject(nodeComponent);
            so.FindProperty("_zoneNumber").intValue = zoneNumber;
            so.FindProperty("_zoneName").stringValue = zoneName;
            so.FindProperty("_button").objectReferenceValue = button;
            so.FindProperty("_zoneNumberText").objectReferenceValue = numberText.GetComponent<TMP_Text>();
            so.FindProperty("_zoneNameText").objectReferenceValue = nameText.GetComponent<TMP_Text>();
            so.FindProperty("_backgroundImage").objectReferenceValue = nodeImage;
            so.ApplyModifiedProperties();

            return nodeObj;
        }

        private static void CreateBMRequiemSelectionScreen(Transform parent)
        {
            var screenObj = CreateSimpleUIObject("RequiemSelectionScreen", parent);
            SetFullStretchRect(screenObj);
            var screen = screenObj.AddComponent<RequiemSelectionScreen>();

            var bgImage = screenObj.AddComponent<Image>();
            bgImage.color = new Color(0.1f, 0.08f, 0.15f, 0.95f);

            var titleObj = CreateSimpleTextObject("Title", screenObj.transform, "SELECT YOUR REQUIEMS");
            var titleRect = titleObj.GetComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0.5f, 0.9f);
            titleRect.anchorMax = new Vector2(0.5f, 0.95f);
            titleRect.pivot = new Vector2(0.5f, 0.5f);
            titleRect.sizeDelta = new Vector2(500, 50);
            var titleText = titleObj.GetComponent<TMP_Text>();
            titleText.fontSize = 36;
            titleText.fontStyle = FontStyles.Bold;
            titleText.alignment = TextAlignmentOptions.Center;

            // Slot container for Requiem selection slots
            var slotContainer = CreateSimpleUIObject("SlotContainer", screenObj.transform);
            var slotRect = slotContainer.GetComponent<RectTransform>();
            slotRect.anchorMin = new Vector2(0.1f, 0.2f);
            slotRect.anchorMax = new Vector2(0.9f, 0.85f);
            slotRect.offsetMin = Vector2.zero;
            slotRect.offsetMax = Vector2.zero;

            var grid = slotContainer.AddComponent<GridLayoutGroup>();
            // 1:2 aspect ratio to match portrait dimensions (doubled for larger display)
            grid.cellSize = new Vector2(350, 700);
            grid.spacing = new Vector2(30, 30);
            grid.childAlignment = TextAnchor.MiddleCenter;
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = 4;

            // Start run button
            var startRunButton = CreateSimpleButton("StartRunButton", screenObj.transform, "CONFIRM TEAM");
            var startRunRect = startRunButton.GetComponent<RectTransform>();
            startRunRect.anchorMin = new Vector2(0.5f, 0.08f);
            startRunRect.anchorMax = new Vector2(0.5f, 0.08f);
            startRunRect.pivot = new Vector2(0.5f, 0.5f);
            startRunRect.sizeDelta = new Vector2(250, 60);
            var startRunImage = startRunButton.GetComponent<Image>();
            startRunImage.color = new Color(0.3f, 0.6f, 0.3f, 0.9f);

            // Load Requiem data assets for portrait display
            var requiemAssets = AssetDatabase.FindAssets("t:RequiemDataSO", new[] { "Assets/_Project/Data/Characters/Requiems" });
            var requiemDataList = new List<RequiemDataSO>();
            foreach (var guid in requiemAssets)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var data = AssetDatabase.LoadAssetAtPath<RequiemDataSO>(path);
                if (data != null)
                {
                    requiemDataList.Add(data);
                }
            }

            // Load AspectIconConfig for aspect badges
            var aspectIconConfig = AssetDatabase.LoadAssetAtPath<AspectIconConfigSO>(
                "Assets/_Project/Data/Config/AspectIconConfig.asset");

            // Wire references using correct field names from RequiemSelectionScreen
            var so = new SerializedObject(screen);
            so.FindProperty("_slotContainer").objectReferenceValue = slotContainer.transform;
            so.FindProperty("_startRunButton").objectReferenceValue = startRunButton.GetComponent<Button>();

            // Wire Requiem data array so portraits display correctly
            var requiemDataProp = so.FindProperty("_availableRequiems");
            requiemDataProp.arraySize = requiemDataList.Count;
            for (int i = 0; i < requiemDataList.Count; i++)
            {
                requiemDataProp.GetArrayElementAtIndex(i).objectReferenceValue = requiemDataList[i];
            }

            // Wire AspectIconConfig for aspect badge sprites
            if (aspectIconConfig != null)
            {
                so.FindProperty("_aspectIconConfig").objectReferenceValue = aspectIconConfig;
                Debug.Log("[ProductionSceneSetupGenerator] Wired AspectIconConfig to RequiemSelectionScreen");
            }
            else
            {
                Debug.LogWarning("[ProductionSceneSetupGenerator] AspectIconConfig not found at Assets/_Project/Data/Config/AspectIconConfig.asset - aspect badges will use colored squares");
            }

            so.ApplyModifiedProperties();
            Debug.Log($"[ProductionSceneSetupGenerator] Wired {requiemDataList.Count} Requiem data assets to RequiemSelectionScreen");

            screenObj.SetActive(false);
        }

        // ============================================
        // Requiems Scene
        // ============================================

        public static void SetupRequiemsScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            CreateMainCamera("Requiems");
            CreateEventSystem();

            var canvas = CreateMainCanvas("RequiemsCanvas");
            CreateBackground(canvas, new Color(0.08f, 0.08f, 0.12f));
            CreateRequiemsScreen(canvas.transform);
            CreateSettingsOverlay(canvas.transform);

            string scenePath = $"{SCENES_PATH}/Requiems.unity";
            EditorSceneManager.SaveScene(scene, scenePath);
            Debug.Log($"[ProductionSceneSetupGenerator] Created Requiems scene at {scenePath}");
        }

        private static void CreateRequiemsScreen(Transform parent)
        {
            var screenContainer = CreateSimpleUIObject("ScreenContainer", parent);
            SetFullStretchRect(screenContainer);

            var screenObj = CreateSimpleUIObject("RequiemsListScreen", screenContainer.transform);
            SetFullStretchRect(screenObj);
            var screen = screenObj.AddComponent<RequiemsListScreen>();

            // Header
            var header = CreateSimpleUIObject("Header", screenObj.transform);
            var headerRect = header.GetComponent<RectTransform>();
            headerRect.anchorMin = new Vector2(0, 0.9f);
            headerRect.anchorMax = new Vector2(1, 1);
            headerRect.offsetMin = new Vector2(20, 0);
            headerRect.offsetMax = new Vector2(-20, -10);

            var backButton = CreateSimpleButton("BackButton", header.transform, "<");
            var backRect = backButton.GetComponent<RectTransform>();
            backRect.anchorMin = new Vector2(0, 0.5f);
            backRect.anchorMax = new Vector2(0, 0.5f);
            backRect.pivot = new Vector2(0, 0.5f);
            backRect.sizeDelta = new Vector2(60, 60);

            var title = CreateSimpleTextObject("Title", header.transform, "Requiems");
            var titleRect = title.GetComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0, 0.5f);
            titleRect.anchorMax = new Vector2(0, 0.5f);
            titleRect.pivot = new Vector2(0, 0.5f);
            titleRect.sizeDelta = new Vector2(200, 50);
            titleRect.anchoredPosition = new Vector2(80, 0);
            title.GetComponent<TMP_Text>().fontSize = 32;

            var settingsButton = CreateSimpleButton("SettingsButton", header.transform, "=");
            var settingsRect = settingsButton.GetComponent<RectTransform>();
            settingsRect.anchorMin = new Vector2(1, 0.5f);
            settingsRect.anchorMax = new Vector2(1, 0.5f);
            settingsRect.pivot = new Vector2(1, 0.5f);
            settingsRect.sizeDelta = new Vector2(60, 60);

            // Portrait grid container
            var gridContainer = CreateSimpleUIObject("PortraitGrid", screenObj.transform);
            var gridRect = gridContainer.GetComponent<RectTransform>();
            gridRect.anchorMin = new Vector2(0.1f, 0.15f);
            gridRect.anchorMax = new Vector2(0.9f, 0.85f);
            gridRect.offsetMin = Vector2.zero;
            gridRect.offsetMax = Vector2.zero;

            var grid = gridContainer.AddComponent<GridLayoutGroup>();
            grid.cellSize = new Vector2(200, 280);
            grid.spacing = new Vector2(30, 30);
            grid.startCorner = GridLayoutGroup.Corner.UpperLeft;
            grid.startAxis = GridLayoutGroup.Axis.Horizontal;
            grid.childAlignment = TextAnchor.MiddleCenter;
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = 2;

            // Detail panel (hidden by default)
            var detailPanelObj = CreateRequiemDetailPanelUI(screenObj.transform);
            var panelComponent = detailPanelObj.GetComponent<RequiemDetailPanel>();

            // Load Requiem data assets
            var requiemAssets = AssetDatabase.FindAssets("t:RequiemDataSO", new[] { "Assets/_Project/Data/Characters/Requiems" });
            var requiemDataList = new List<RequiemDataSO>();
            foreach (var guid in requiemAssets)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var data = AssetDatabase.LoadAssetAtPath<RequiemDataSO>(path);
                if (data != null)
                {
                    requiemDataList.Add(data);
                }
            }

            // Wire references
            var so = new SerializedObject(screen);
            so.FindProperty("_backButton").objectReferenceValue = backButton.GetComponent<Button>();
            so.FindProperty("_titleText").objectReferenceValue = title.GetComponent<TMP_Text>();
            so.FindProperty("_settingsButton").objectReferenceValue = settingsButton.GetComponent<Button>();
            so.FindProperty("_portraitContainer").objectReferenceValue = gridContainer.transform;
            so.FindProperty("_detailPanel").objectReferenceValue = panelComponent;

            // Wire Requiem data array
            var requiemDataProp = so.FindProperty("_requiemData");
            requiemDataProp.arraySize = requiemDataList.Count;
            for (int i = 0; i < requiemDataList.Count; i++)
            {
                requiemDataProp.GetArrayElementAtIndex(i).objectReferenceValue = requiemDataList[i];
            }

            so.ApplyModifiedProperties();
            Debug.Log($"[ProductionSceneSetupGenerator] Wired {requiemDataList.Count} Requiem data assets to RequiemsListScreen");
        }

        private static GameObject CreateRequiemDetailPanelUI(Transform parent)
        {
            var panelObj = CreateSimpleUIObject("RequiemDetailPanel", parent);
            SetFullStretchRect(panelObj);
            var detailPanel = panelObj.AddComponent<RequiemDetailPanel>();

            var canvasGroup = panelObj.AddComponent<CanvasGroup>();
            canvasGroup.alpha = 0;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;

            var bgImage = panelObj.AddComponent<Image>();
            bgImage.color = new Color(0.92f, 0.92f, 0.94f, 1f); // Light gray background like reference

            var panelContent = CreateSimpleUIObject("PanelContent", panelObj.transform);
            var contentRect = panelContent.GetComponent<RectTransform>();
            contentRect.anchorMin = Vector2.zero;
            contentRect.anchorMax = Vector2.one;
            contentRect.offsetMin = Vector2.zero;
            contentRect.offsetMax = Vector2.zero;

            // Header (top bar)
            var header = CreateSimpleUIObject("Header", panelContent.transform);
            var headerRect = header.GetComponent<RectTransform>();
            headerRect.anchorMin = new Vector2(0, 0.92f);
            headerRect.anchorMax = new Vector2(1, 1);
            headerRect.offsetMin = Vector2.zero;
            headerRect.offsetMax = Vector2.zero;

            var headerBg = header.AddComponent<Image>();
            headerBg.color = new Color(0.95f, 0.95f, 0.97f, 1f);

            var closeButton = CreateSimpleButton("CloseButton", header.transform, "<");
            var closeRect = closeButton.GetComponent<RectTransform>();
            closeRect.anchorMin = new Vector2(0, 0.5f);
            closeRect.anchorMax = new Vector2(0, 0.5f);
            closeRect.pivot = new Vector2(0, 0.5f);
            closeRect.sizeDelta = new Vector2(60, 50);
            closeRect.anchoredPosition = new Vector2(10, 0);
            closeButton.GetComponent<Image>().color = new Color(0.3f, 0.35f, 0.45f, 1f);
            var closeBtnText = closeButton.GetComponentInChildren<TMP_Text>();
            if (closeBtnText != null)
            {
                closeBtnText.fontSize = 24;
                closeBtnText.fontStyle = FontStyles.Bold;
            }

            var titleObj = CreateSimpleTextObject("Title", header.transform, "Details");
            var titleRect = titleObj.GetComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0, 0.5f);
            titleRect.anchorMax = new Vector2(0.3f, 0.5f);
            titleRect.pivot = new Vector2(0, 0.5f);
            titleRect.sizeDelta = new Vector2(150, 40);
            titleRect.anchoredPosition = new Vector2(80, 0);
            var titleText = titleObj.GetComponent<TMP_Text>();
            titleText.fontSize = 24;
            titleText.fontStyle = FontStyles.Bold;
            titleText.color = Color.black;

            // ==============================
            // Left Sidebar - Portrait List
            // ==============================
            var leftSidebar = CreateSimpleUIObject("LeftSidebar", panelContent.transform);
            var leftRect = leftSidebar.GetComponent<RectTransform>();
            leftRect.anchorMin = new Vector2(0, 0);
            leftRect.anchorMax = new Vector2(0.08f, 0.92f);
            leftRect.offsetMin = Vector2.zero;
            leftRect.offsetMax = Vector2.zero;

            var leftBg = leftSidebar.AddComponent<Image>();
            leftBg.color = new Color(0.85f, 0.85f, 0.88f, 1f);

            var portraitListContainer = CreateSimpleUIObject("PortraitList", leftSidebar.transform);
            SetFullStretchRect(portraitListContainer);
            portraitListContainer.GetComponent<RectTransform>().offsetMin = new Vector2(5, 5);
            portraitListContainer.GetComponent<RectTransform>().offsetMax = new Vector2(-5, -5);

            var vlg = portraitListContainer.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = 8;
            vlg.childAlignment = TextAnchor.UpperCenter;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;
            vlg.padding = new RectOffset(3, 3, 10, 10);

            // ==============================
            // Menu Sidebar - Stats/Cards Tabs (Vertical)
            // ==============================
            var menuSidebar = CreateSimpleUIObject("MenuSidebar", panelContent.transform);
            var menuRect = menuSidebar.GetComponent<RectTransform>();
            menuRect.anchorMin = new Vector2(0.08f, 0);
            menuRect.anchorMax = new Vector2(0.22f, 0.92f);
            menuRect.offsetMin = Vector2.zero;
            menuRect.offsetMax = Vector2.zero;

            var menuBg = menuSidebar.AddComponent<Image>();
            menuBg.color = new Color(0.95f, 0.95f, 0.97f, 1f);

            // Menu buttons container
            var menuButtonsContainer = CreateSimpleUIObject("MenuButtons", menuSidebar.transform);
            var menuBtnsRect = menuButtonsContainer.GetComponent<RectTransform>();
            menuBtnsRect.anchorMin = new Vector2(0, 0.5f);
            menuBtnsRect.anchorMax = new Vector2(1, 0.95f);
            menuBtnsRect.offsetMin = new Vector2(5, 0);
            menuBtnsRect.offsetMax = new Vector2(-5, -5);

            var menuVlg = menuButtonsContainer.AddComponent<VerticalLayoutGroup>();
            menuVlg.spacing = 5;
            menuVlg.childAlignment = TextAnchor.UpperLeft;
            menuVlg.childForceExpandWidth = true;
            menuVlg.childForceExpandHeight = false;
            menuVlg.padding = new RectOffset(5, 5, 10, 10);

            // Stats tab button (vertical menu item)
            var statsTabButton = CreateMenuTabButton(menuButtonsContainer.transform, "StatsTab", "Stats", true);
            var cardsTabButton = CreateMenuTabButton(menuButtonsContainer.transform, "CardsTab", "Cards", false);

            // ==============================
            // Stats Tab Content - Character Visual + Stats
            // ==============================
            var statsContent = CreateSimpleUIObject("StatsTabContent", panelContent.transform);
            var statsContentRect = statsContent.GetComponent<RectTransform>();
            statsContentRect.anchorMin = new Vector2(0.22f, 0);
            statsContentRect.anchorMax = new Vector2(1, 0.92f);
            statsContentRect.offsetMin = Vector2.zero;
            statsContentRect.offsetMax = Vector2.zero;

            // Character Visual Area (center of stats view)
            var characterArea = CreateSimpleUIObject("CharacterArea", statsContent.transform);
            var characterAreaRect = characterArea.GetComponent<RectTransform>();
            characterAreaRect.anchorMin = new Vector2(0, 0);
            characterAreaRect.anchorMax = new Vector2(0.65f, 1);
            characterAreaRect.offsetMin = Vector2.zero;
            characterAreaRect.offsetMax = Vector2.zero;

            var characterBg = characterArea.AddComponent<Image>();
            characterBg.color = new Color(0.88f, 0.9f, 0.95f, 1f); // Light blue-ish like reference

            var artContainer = CreateSimpleUIObject("CharacterArt", characterArea.transform);
            var artRect = artContainer.GetComponent<RectTransform>();
            artRect.anchorMin = new Vector2(0.1f, 0.05f);
            artRect.anchorMax = new Vector2(0.9f, 0.95f);
            artRect.offsetMin = Vector2.zero;
            artRect.offsetMax = Vector2.zero;

            var artImage = artContainer.AddComponent<Image>();
            artImage.color = Color.white; // Full alpha for proper portrait display
            artImage.preserveAspect = true;

            // Stats Panel (right side of stats view)
            var statsPanel = CreateSimpleUIObject("StatsPanel", statsContent.transform);
            var statsPanelRect = statsPanel.GetComponent<RectTransform>();
            statsPanelRect.anchorMin = new Vector2(0.65f, 0);
            statsPanelRect.anchorMax = new Vector2(1, 1);
            statsPanelRect.offsetMin = Vector2.zero;
            statsPanelRect.offsetMax = Vector2.zero;

            var statsPanelBg = statsPanel.AddComponent<Image>();
            statsPanelBg.color = new Color(0.95f, 0.95f, 0.97f, 1f);

            // Character name and level at top of stats panel
            var nameObj = CreateSimpleTextObject("CharacterName", statsPanel.transform, "Requiem Name");
            var nameRect = nameObj.GetComponent<RectTransform>();
            nameRect.anchorMin = new Vector2(0, 0.88f);
            nameRect.anchorMax = new Vector2(1, 0.95f);
            nameRect.offsetMin = new Vector2(15, 0);
            nameRect.offsetMax = new Vector2(-15, 0);
            var nameText = nameObj.GetComponent<TMP_Text>();
            nameText.fontSize = 22;
            nameText.fontStyle = FontStyles.Bold;
            nameText.color = Color.black;
            nameText.alignment = TextAlignmentOptions.Left;

            var classObj = CreateSimpleTextObject("CharacterClass", statsPanel.transform, "Class | Aspect");
            var classRect = classObj.GetComponent<RectTransform>();
            classRect.anchorMin = new Vector2(0, 0.82f);
            classRect.anchorMax = new Vector2(1, 0.88f);
            classRect.offsetMin = new Vector2(15, 0);
            classRect.offsetMax = new Vector2(-15, 0);
            var classText = classObj.GetComponent<TMP_Text>();
            classText.fontSize = 14;
            classText.color = new Color(0.4f, 0.4f, 0.4f);
            classText.alignment = TextAlignmentOptions.Left;

            // Stats header
            var statsHeaderObj = CreateSimpleTextObject("StatsHeader", statsPanel.transform, "Stats");
            var statsHeaderRect = statsHeaderObj.GetComponent<RectTransform>();
            statsHeaderRect.anchorMin = new Vector2(0, 0.72f);
            statsHeaderRect.anchorMax = new Vector2(1, 0.78f);
            statsHeaderRect.offsetMin = new Vector2(15, 0);
            statsHeaderRect.offsetMax = new Vector2(-15, 0);
            var statsHeaderText = statsHeaderObj.GetComponent<TMP_Text>();
            statsHeaderText.fontSize = 14;
            statsHeaderText.color = new Color(0.5f, 0.5f, 0.5f);
            statsHeaderText.alignment = TextAlignmentOptions.Left;

            // Stats rows container
            var statsRowsContainer = CreateSimpleUIObject("StatsRows", statsPanel.transform);
            var statsRowsRect = statsRowsContainer.GetComponent<RectTransform>();
            statsRowsRect.anchorMin = new Vector2(0, 0.35f);
            statsRowsRect.anchorMax = new Vector2(1, 0.72f);
            statsRowsRect.offsetMin = new Vector2(15, 0);
            statsRowsRect.offsetMax = new Vector2(-15, 0);

            var statsVlg = statsRowsContainer.AddComponent<VerticalLayoutGroup>();
            statsVlg.spacing = 8;
            statsVlg.childForceExpandWidth = true;
            statsVlg.childForceExpandHeight = false;
            statsVlg.padding = new RectOffset(0, 0, 5, 5);

            var atkRow = CreateStatRowForDetailRedesigned(statsRowsContainer.transform, "Attack", "0");
            var defRow = CreateStatRowForDetailRedesigned(statsRowsContainer.transform, "Defense", "0");
            var hpRow = CreateStatRowForDetailRedesigned(statsRowsContainer.transform, "Health", "0");

            // ==============================
            // Cards Tab Content - Card Grid
            // ==============================
            var cardsContent = CreateSimpleUIObject("CardsTabContent", panelContent.transform);
            var cardsContentRect = cardsContent.GetComponent<RectTransform>();
            cardsContentRect.anchorMin = new Vector2(0.22f, 0);
            cardsContentRect.anchorMax = new Vector2(1, 0.92f);
            cardsContentRect.offsetMin = Vector2.zero;
            cardsContentRect.offsetMax = Vector2.zero;
            cardsContent.SetActive(false);

            var cardsContentBg = cardsContent.AddComponent<Image>();
            cardsContentBg.color = new Color(0.95f, 0.95f, 0.97f, 1f);

            // Starting Cards section - expanded to fill most of the content area
            var startingCardsSection = CreateSimpleUIObject("StartingCardsSection", cardsContent.transform);
            var startingRect = startingCardsSection.GetComponent<RectTransform>();
            startingRect.anchorMin = new Vector2(0, 0);
            startingRect.anchorMax = new Vector2(1, 1);
            startingRect.offsetMin = new Vector2(20, 20);
            startingRect.offsetMax = new Vector2(-20, -20);

            var startingHeader = CreateSimpleTextObject("StartingCardsHeader", startingCardsSection.transform, "Starting Cards");
            var startingHeaderRect = startingHeader.GetComponent<RectTransform>();
            startingHeaderRect.anchorMin = new Vector2(0, 0.93f);
            startingHeaderRect.anchorMax = new Vector2(1, 1);
            startingHeaderRect.offsetMin = Vector2.zero;
            startingHeaderRect.offsetMax = Vector2.zero;
            var startingHeaderText = startingHeader.GetComponent<TMP_Text>();
            startingHeaderText.fontSize = 18;
            startingHeaderText.fontStyle = FontStyles.Bold;
            startingHeaderText.color = Color.black;
            startingHeaderText.alignment = TextAlignmentOptions.Left;

            // ScrollView for cards (matching Sanctuary screen pattern)
            var cardsScrollView = CreateSimpleUIObject("CardsScrollView", startingCardsSection.transform);
            var scrollViewRect = cardsScrollView.GetComponent<RectTransform>();
            scrollViewRect.anchorMin = new Vector2(0, 0);
            scrollViewRect.anchorMax = new Vector2(1, 0.92f);
            scrollViewRect.offsetMin = Vector2.zero;
            scrollViewRect.offsetMax = Vector2.zero;

            // Add background to scroll view
            var scrollBg = cardsScrollView.AddComponent<Image>();
            scrollBg.color = new Color(0.92f, 0.92f, 0.94f, 0.5f);

            var scrollRect = cardsScrollView.AddComponent<ScrollRect>();

            // Viewport with RectMask2D (not Mask - more reliable for scroll views)
            var viewport = CreateSimpleUIObject("Viewport", cardsScrollView.transform);
            SetFullStretchRect(viewport);
            viewport.AddComponent<RectMask2D>(); // RectMask2D clips without needing Image

            // Card container (content of scroll view) - GridLayoutGroup directly on content
            var cardContainer = CreateSimpleUIObject("CardContainer", viewport.transform);
            var cardContainerRect = cardContainer.GetComponent<RectTransform>();
            cardContainerRect.anchorMin = new Vector2(0, 1);
            cardContainerRect.anchorMax = new Vector2(1, 1);
            cardContainerRect.pivot = new Vector2(0.5f, 1);
            cardContainerRect.sizeDelta = new Vector2(0, 0);

            // GridLayoutGroup directly on the scroll content (matching Sanctuary pattern)
            var cardGrid = cardContainer.AddComponent<GridLayoutGroup>();
            cardGrid.cellSize = new Vector2(200, 280); // Match Card.prefab native size
            cardGrid.spacing = new Vector2(20, 20);
            cardGrid.padding = new RectOffset(20, 20, 15, 15);
            cardGrid.childAlignment = TextAnchor.UpperCenter;

            // ContentSizeFitter on same object as GridLayoutGroup
            var contentFitter = cardContainer.AddComponent<ContentSizeFitter>();
            contentFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            // Wire scroll rect
            scrollRect.content = cardContainerRect;
            scrollRect.viewport = viewport.GetComponent<RectTransform>();
            scrollRect.horizontal = false;
            scrollRect.vertical = true;

            // RequiemCardDisplay component directly on card container
            // Cards are added as children of _cardContainer, which is this same object
            var cardDisplayComponent = cardContainer.AddComponent<RequiemCardDisplay>();

            var cardDisplaySO = new SerializedObject(cardDisplayComponent);
            cardDisplaySO.FindProperty("_cardContainer").objectReferenceValue = cardContainer.transform;

            // Load and wire Card.prefab for proper card display
            var cardPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Project/Prefabs/Cards/Card.prefab");
            if (cardPrefab != null)
            {
                cardDisplaySO.FindProperty("_cardItemPrefab").objectReferenceValue = cardPrefab;
                Debug.Log("[ProductionSceneSetupGenerator] Wired Card.prefab to RequiemCardDisplay");
            }
            else
            {
                Debug.LogWarning("[ProductionSceneSetupGenerator] Card.prefab not found at Assets/_Project/Prefabs/Cards/Card.prefab");
            }

            cardDisplaySO.ApplyModifiedProperties();

            // Wire all references
            var so = new SerializedObject(detailPanel);
            so.FindProperty("_panelGroup").objectReferenceValue = canvasGroup;
            so.FindProperty("_panelContent").objectReferenceValue = panelContent.GetComponent<RectTransform>();
            so.FindProperty("_closeButton").objectReferenceValue = closeButton.GetComponent<Button>();
            so.FindProperty("_titleText").objectReferenceValue = titleText;
            so.FindProperty("_portraitListContainer").objectReferenceValue = portraitListContainer.transform;
            so.FindProperty("_characterArtImage").objectReferenceValue = artImage;
            so.FindProperty("_characterNameText").objectReferenceValue = nameText;
            so.FindProperty("_characterClassText").objectReferenceValue = classText;
            so.FindProperty("_attackText").objectReferenceValue = atkRow.transform.Find("Value").GetComponent<TMP_Text>();
            so.FindProperty("_defenseText").objectReferenceValue = defRow.transform.Find("Value").GetComponent<TMP_Text>();
            so.FindProperty("_healthText").objectReferenceValue = hpRow.transform.Find("Value").GetComponent<TMP_Text>();
            so.FindProperty("_statsTabButton").objectReferenceValue = statsTabButton.GetComponent<Button>();
            so.FindProperty("_cardsTabButton").objectReferenceValue = cardsTabButton.GetComponent<Button>();
            so.FindProperty("_statsTabContent").objectReferenceValue = statsContent;
            so.FindProperty("_cardsTabContent").objectReferenceValue = cardsContent;
            so.FindProperty("_startingCardsDisplay").objectReferenceValue = cardDisplayComponent;
            so.ApplyModifiedProperties();

            panelObj.SetActive(false);
            return panelObj;
        }

        /// <summary>
        /// Creates a vertical menu tab button for the RequiemDetailPanel.
        /// </summary>
        private static GameObject CreateMenuTabButton(Transform parent, string name, string label, bool isSelected)
        {
            var buttonObj = new GameObject(name);
            buttonObj.transform.SetParent(parent, false);

            var layoutElement = buttonObj.AddComponent<LayoutElement>();
            layoutElement.preferredHeight = 45;

            var image = buttonObj.AddComponent<Image>();
            image.color = isSelected ? new Color(0.3f, 0.35f, 0.45f, 1f) : new Color(0.9f, 0.9f, 0.92f, 1f);

            var button = buttonObj.AddComponent<Button>();
            button.targetGraphic = image;

            // Icon placeholder
            var iconObj = new GameObject("Icon");
            iconObj.transform.SetParent(buttonObj.transform, false);
            var iconRect = iconObj.AddComponent<RectTransform>();
            iconRect.anchorMin = new Vector2(0, 0.5f);
            iconRect.anchorMax = new Vector2(0, 0.5f);
            iconRect.pivot = new Vector2(0, 0.5f);
            iconRect.sizeDelta = new Vector2(20, 20);
            iconRect.anchoredPosition = new Vector2(10, 0);

            var iconImage = iconObj.AddComponent<Image>();
            iconImage.color = isSelected ? Color.white : new Color(0.4f, 0.4f, 0.4f);

            // Label text
            var textObj = new GameObject("Label");
            textObj.transform.SetParent(buttonObj.transform, false);
            var textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = new Vector2(0, 0);
            textRect.anchorMax = new Vector2(1, 1);
            textRect.offsetMin = new Vector2(35, 0);
            textRect.offsetMax = new Vector2(-5, 0);

            var tmpText = textObj.AddComponent<TextMeshProUGUI>();
            tmpText.text = label;
            tmpText.fontSize = 16;
            tmpText.color = isSelected ? Color.white : new Color(0.3f, 0.3f, 0.3f);
            tmpText.alignment = TextAlignmentOptions.Left;
            tmpText.verticalAlignment = VerticalAlignmentOptions.Middle;

            return buttonObj;
        }

        /// <summary>
        /// Creates a stat row for the redesigned detail panel.
        /// </summary>
        private static GameObject CreateStatRowForDetailRedesigned(Transform parent, string label, string value)
        {
            var row = CreateSimpleUIObject(label + "Row", parent);
            var layoutElement = row.AddComponent<LayoutElement>();
            layoutElement.preferredHeight = 30;

            var labelObj = CreateSimpleTextObject("Label", row.transform, label);
            var labelRect = labelObj.GetComponent<RectTransform>();
            labelRect.anchorMin = new Vector2(0, 0);
            labelRect.anchorMax = new Vector2(0.6f, 1);
            labelRect.offsetMin = Vector2.zero;
            labelRect.offsetMax = Vector2.zero;
            var labelText = labelObj.GetComponent<TMP_Text>();
            labelText.fontSize = 16;
            labelText.color = new Color(0.3f, 0.3f, 0.3f);
            labelText.alignment = TextAlignmentOptions.Left;

            var valueObj = CreateSimpleTextObject("Value", row.transform, value);
            var valueRect = valueObj.GetComponent<RectTransform>();
            valueRect.anchorMin = new Vector2(0.6f, 0);
            valueRect.anchorMax = new Vector2(1, 1);
            valueRect.offsetMin = Vector2.zero;
            valueRect.offsetMax = Vector2.zero;
            var valueText = valueObj.GetComponent<TMP_Text>();
            valueText.fontSize = 18;
            valueText.fontStyle = FontStyles.Bold;
            valueText.color = Color.black;
            valueText.alignment = TextAlignmentOptions.Right;

            return row;
        }

        // ============================================
        // Settings Overlay (shared across all scenes)
        // ============================================

        public static void CreateSettingsOverlay(Transform parent)
        {
            var overlayObj = CreateSimpleUIObject("SettingsOverlay", parent);
            SetFullStretchRect(overlayObj);
            var overlay = overlayObj.AddComponent<SettingsOverlay>();

            var canvasGroup = overlayObj.AddComponent<CanvasGroup>();
            canvasGroup.alpha = 0;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;

            // Semi-transparent background
            var bg = CreateSimpleUIObject("Background", overlayObj.transform);
            SetFullStretchRect(bg);
            var bgImage = bg.AddComponent<Image>();
            bgImage.color = new Color(0, 0, 0, 0.6f);

            // Main panel container (matches reference design)
            var panel = CreateSimpleUIObject("Panel", overlayObj.transform);
            var panelRect = panel.GetComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.1f, 0.1f);
            panelRect.anchorMax = new Vector2(0.9f, 0.9f);
            panelRect.offsetMin = Vector2.zero;
            panelRect.offsetMax = Vector2.zero;

            // ============================================
            // Left Sidebar (category icons)
            // ============================================
            var leftSidebar = CreateSimpleUIObject("LeftSidebar", panel.transform);
            var sidebarRect = leftSidebar.GetComponent<RectTransform>();
            sidebarRect.anchorMin = new Vector2(0, 0);
            sidebarRect.anchorMax = new Vector2(0.08f, 1);
            sidebarRect.offsetMin = Vector2.zero;
            sidebarRect.offsetMax = Vector2.zero;

            var sidebarBg = leftSidebar.AddComponent<Image>();
            sidebarBg.color = new Color(0.12f, 0.12f, 0.15f, 1f);

            var sidebarVlg = leftSidebar.AddComponent<VerticalLayoutGroup>();
            sidebarVlg.spacing = 5;
            sidebarVlg.padding = new RectOffset(5, 5, 10, 10);
            sidebarVlg.childAlignment = TextAnchor.UpperCenter;
            sidebarVlg.childForceExpandWidth = true;
            sidebarVlg.childForceExpandHeight = false;

            // Load icon config for settings categories
            var iconConfig = LoadIconConfig();

            // Category buttons - use sprites if available, fallback to emoji text
            CreateSettingsCategoryButton(leftSidebar.transform, "DisplayBtn", iconConfig?.DisplaySettingsIcon, "\U0001F5A5", false);
            CreateSettingsCategoryButton(leftSidebar.transform, "AudioBtn", iconConfig?.AudioSettingsIcon, "\U0001F3A7", true);   // Selected
            CreateSettingsCategoryButton(leftSidebar.transform, "GameBtn", iconConfig?.GameSettingsIcon, "\u2699", false);
            CreateSettingsCategoryButton(leftSidebar.transform, "NetworkBtn", iconConfig?.NetworkSettingsIcon, "\U0001F310", false);
            CreateSettingsCategoryButton(leftSidebar.transform, "AccountBtn", iconConfig?.AccountSettingsIcon, "\U0001F464", false);

            // ============================================
            // Main Content Area
            // ============================================
            var mainContent = CreateSimpleUIObject("MainContent", panel.transform);
            var mainRect = mainContent.GetComponent<RectTransform>();
            mainRect.anchorMin = new Vector2(0.08f, 0);
            mainRect.anchorMax = new Vector2(1, 1);
            mainRect.offsetMin = Vector2.zero;
            mainRect.offsetMax = Vector2.zero;

            var mainBg = mainContent.AddComponent<Image>();
            mainBg.color = new Color(0.9f, 0.9f, 0.92f, 1f);  // Light gray like reference

            // Header with title and close button
            var header = CreateSimpleUIObject("Header", mainContent.transform);
            var headerRect = header.GetComponent<RectTransform>();
            headerRect.anchorMin = new Vector2(0, 0.92f);
            headerRect.anchorMax = new Vector2(1, 1);
            headerRect.offsetMin = new Vector2(10, 0);
            headerRect.offsetMax = new Vector2(-10, -5);

            // Orange accent bar on left of title
            var accentBar = CreateSimpleUIObject("AccentBar", header.transform);
            var accentRect = accentBar.GetComponent<RectTransform>();
            accentRect.anchorMin = new Vector2(0, 0.2f);
            accentRect.anchorMax = new Vector2(0, 0.8f);
            accentRect.pivot = new Vector2(0, 0.5f);
            accentRect.sizeDelta = new Vector2(4, 0);
            accentRect.anchoredPosition = new Vector2(5, 0);
            var accentImage = accentBar.AddComponent<Image>();
            accentImage.color = new Color(0.9f, 0.6f, 0.1f, 1f);

            // Title "Sound Settings"
            var title = CreateSimpleTextObject("Title", header.transform, "Sound Settings");
            var titleRect = title.GetComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0, 0);
            titleRect.anchorMax = new Vector2(0.8f, 1);
            titleRect.offsetMin = new Vector2(20, 0);
            titleRect.offsetMax = new Vector2(0, 0);
            var titleText = title.GetComponent<TMP_Text>();
            titleText.fontSize = 26;
            titleText.fontStyle = FontStyles.Bold;
            titleText.color = Color.black;
            titleText.alignment = TextAlignmentOptions.Left;

            // Close button (X) on right
            var closeButton = CreateSimpleButton("CloseButton", header.transform, "×");
            var closeRect = closeButton.GetComponent<RectTransform>();
            closeRect.anchorMin = new Vector2(1, 0.5f);
            closeRect.anchorMax = new Vector2(1, 0.5f);
            closeRect.pivot = new Vector2(1, 0.5f);
            closeRect.sizeDelta = new Vector2(40, 40);
            closeRect.anchoredPosition = new Vector2(-10, 0);
            closeButton.GetComponent<Image>().color = new Color(0.85f, 0.85f, 0.88f, 1f);
            closeButton.GetComponentInChildren<TMP_Text>().color = Color.black;
            closeButton.GetComponentInChildren<TMP_Text>().fontSize = 28;

            // ============================================
            // Volume Sliders Content
            // ============================================
            var slidersContainer = CreateSimpleUIObject("Sliders", mainContent.transform);
            var slidersRect = slidersContainer.GetComponent<RectTransform>();
            slidersRect.anchorMin = new Vector2(0, 0.1f);
            slidersRect.anchorMax = new Vector2(1, 0.9f);
            slidersRect.offsetMin = new Vector2(20, 10);
            slidersRect.offsetMax = new Vector2(-20, -10);

            var vlg = slidersContainer.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = 15;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;
            vlg.padding = new RectOffset(10, 10, 10, 10);

            // Section header: "Overall Volume"
            var overallHeader = CreateSectionHeader(slidersContainer.transform, "Overall Volume");

            // Master volume slider (renamed to "Overall Volume" to match reference)
            var masterSlider = CreateSettingsSliderRow("MasterVolume", slidersContainer.transform, "Overall Volume");

            // Section header: "Volume Details Settings"
            var detailsHeader = CreateSectionHeader(slidersContainer.transform, "Volume Details Settings");

            // Detail sliders
            var musicSlider = CreateSettingsSliderRow("MusicVolume", slidersContainer.transform, "Background Sound");
            var voiceSlider = CreateSettingsSliderRow("VoiceVolume", slidersContainer.transform, "Voice");  // Placeholder
            var sfxSlider = CreateSettingsSliderRow("SFXVolume", slidersContainer.transform, "SFX");

            // Wire references
            var so = new SerializedObject(overlay);
            so.FindProperty("_overlay").objectReferenceValue = canvasGroup;
            so.FindProperty("_backgroundPanel").objectReferenceValue = bgImage;
            so.FindProperty("_settingsPanel").objectReferenceValue = panel.GetComponent<RectTransform>();
            so.FindProperty("_titleText").objectReferenceValue = titleText;
            so.FindProperty("_closeButton").objectReferenceValue = closeButton.GetComponent<Button>();
            so.FindProperty("_masterVolumeSlider").objectReferenceValue = masterSlider.GetComponentInChildren<Slider>();
            so.FindProperty("_musicVolumeSlider").objectReferenceValue = musicSlider.GetComponentInChildren<Slider>();
            so.FindProperty("_sfxVolumeSlider").objectReferenceValue = sfxSlider.GetComponentInChildren<Slider>();
            so.ApplyModifiedProperties();

            overlayObj.SetActive(false);
        }

        private static GameObject CreateSettingsCategoryButton(Transform parent, string name, Sprite iconSprite, string fallbackIcon, bool isSelected)
        {
            var buttonObj = CreateSimpleUIObject(name, parent);
            var layoutElement = buttonObj.AddComponent<LayoutElement>();
            layoutElement.preferredHeight = 50;
            layoutElement.preferredWidth = 50;

            var buttonImage = buttonObj.AddComponent<Image>();
            buttonImage.color = isSelected ? new Color(0.2f, 0.2f, 0.25f, 1f) : new Color(0.15f, 0.15f, 0.18f, 1f);

            var button = buttonObj.AddComponent<Button>();
            button.targetGraphic = buttonImage;

            // Orange highlight bar for selected item
            if (isSelected)
            {
                var highlight = CreateSimpleUIObject("Highlight", buttonObj.transform);
                var highlightRect = highlight.GetComponent<RectTransform>();
                highlightRect.anchorMin = new Vector2(0, 0);
                highlightRect.anchorMax = new Vector2(0, 1);
                highlightRect.pivot = new Vector2(0, 0.5f);
                highlightRect.sizeDelta = new Vector2(4, 0);
                var highlightImage = highlight.AddComponent<Image>();
                highlightImage.color = new Color(0.9f, 0.6f, 0.1f, 1f);
            }

            // Icon - use sprite if available, fallback to text
            Color iconColor = isSelected ? Color.white : new Color(0.6f, 0.6f, 0.65f, 1f);
            var iconObj = CreateIconImage(buttonObj, "Icon", iconSprite, new Vector2(24, 24), iconColor, fallbackIcon, 20);
            SetFullStretchRect(iconObj);

            return buttonObj;
        }

        private static GameObject CreateSectionHeader(Transform parent, string text)
        {
            var headerObj = CreateSimpleUIObject(text.Replace(" ", "") + "Header", parent);
            var layoutElement = headerObj.AddComponent<LayoutElement>();
            layoutElement.preferredHeight = 30;

            var textObj = CreateSimpleTextObject("Text", headerObj.transform, text);
            SetFullStretchRect(textObj);
            var tmpText = textObj.GetComponent<TMP_Text>();
            tmpText.fontSize = 16;
            tmpText.fontStyle = FontStyles.Bold;
            tmpText.color = new Color(0.3f, 0.3f, 0.35f, 1f);
            tmpText.alignment = TextAlignmentOptions.Left;

            return headerObj;
        }

        private static GameObject CreateSettingsSliderRow(string name, Transform parent, string labelText)
        {
            var row = CreateSimpleUIObject(name, parent);
            var layoutElement = row.AddComponent<LayoutElement>();
            layoutElement.preferredHeight = 45;

            // Row background (light)
            var rowBg = row.AddComponent<Image>();
            rowBg.color = new Color(0.95f, 0.95f, 0.97f, 1f);

            // Label
            var label = CreateSimpleTextObject("Label", row.transform, labelText);
            var labelRect = label.GetComponent<RectTransform>();
            labelRect.anchorMin = new Vector2(0, 0);
            labelRect.anchorMax = new Vector2(0.25f, 1);
            labelRect.offsetMin = new Vector2(10, 0);
            labelRect.offsetMax = new Vector2(0, 0);
            var labelTmp = label.GetComponent<TMP_Text>();
            labelTmp.fontSize = 16;
            labelTmp.color = Color.black;
            labelTmp.alignment = TextAlignmentOptions.Left;

            // Slider
            var sliderObj = CreateSimpleUIObject("Slider", row.transform);
            var sliderRect = sliderObj.GetComponent<RectTransform>();
            sliderRect.anchorMin = new Vector2(0.27f, 0.25f);
            sliderRect.anchorMax = new Vector2(0.72f, 0.75f);
            sliderRect.offsetMin = Vector2.zero;
            sliderRect.offsetMax = Vector2.zero;

            var slider = sliderObj.AddComponent<Slider>();
            slider.minValue = 0;
            slider.maxValue = 1;
            slider.value = 1;

            var sliderBgObj = CreateSimpleUIObject("Background", sliderObj.transform);
            SetFullStretchRect(sliderBgObj);
            var sliderBgImage = sliderBgObj.AddComponent<Image>();
            sliderBgImage.color = new Color(0.75f, 0.75f, 0.78f, 1f);
            slider.targetGraphic = sliderBgImage;

            var fillArea = CreateSimpleUIObject("FillArea", sliderObj.transform);
            var fillAreaRect = fillArea.GetComponent<RectTransform>();
            fillAreaRect.anchorMin = Vector2.zero;
            fillAreaRect.anchorMax = Vector2.one;
            fillAreaRect.offsetMin = Vector2.zero;
            fillAreaRect.offsetMax = Vector2.zero;

            var fill = CreateSimpleUIObject("Fill", fillArea.transform);
            SetFullStretchRect(fill);
            var fillImage = fill.AddComponent<Image>();
            fillImage.color = new Color(0.9f, 0.6f, 0.1f, 1f);  // Orange fill
            slider.fillRect = fill.GetComponent<RectTransform>();

            var handleArea = CreateSimpleUIObject("HandleSlideArea", sliderObj.transform);
            SetFullStretchRect(handleArea);

            var handle = CreateSimpleUIObject("Handle", handleArea.transform);
            var handleRect = handle.GetComponent<RectTransform>();
            handleRect.sizeDelta = new Vector2(18, 18);
            var handleImage = handle.AddComponent<Image>();
            handleImage.color = new Color(0.9f, 0.6f, 0.1f, 1f);  // Orange handle
            slider.handleRect = handleRect;

            // Percentage text
            var percentText = CreateSimpleTextObject("Percent", row.transform, "100%");
            var percentRect = percentText.GetComponent<RectTransform>();
            percentRect.anchorMin = new Vector2(0.74f, 0);
            percentRect.anchorMax = new Vector2(0.82f, 1);
            percentRect.offsetMin = Vector2.zero;
            percentRect.offsetMax = Vector2.zero;
            var percentTmp = percentText.GetComponent<TMP_Text>();
            percentTmp.fontSize = 14;
            percentTmp.color = Color.black;
            percentTmp.alignment = TextAlignmentOptions.Center;

            // Mute label
            var muteLabel = CreateSimpleTextObject("MuteLabel", row.transform, "Mute");
            var muteLabelRect = muteLabel.GetComponent<RectTransform>();
            muteLabelRect.anchorMin = new Vector2(0.83f, 0);
            muteLabelRect.anchorMax = new Vector2(0.92f, 1);
            muteLabelRect.offsetMin = Vector2.zero;
            muteLabelRect.offsetMax = Vector2.zero;
            var muteLabelTmp = muteLabel.GetComponent<TMP_Text>();
            muteLabelTmp.fontSize = 14;
            muteLabelTmp.color = new Color(0.5f, 0.5f, 0.55f, 1f);
            muteLabelTmp.alignment = TextAlignmentOptions.Center;

            // Mute checkbox placeholder
            var muteBox = CreateSimpleUIObject("MuteCheckbox", row.transform);
            var muteBoxRect = muteBox.GetComponent<RectTransform>();
            muteBoxRect.anchorMin = new Vector2(0.93f, 0.25f);
            muteBoxRect.anchorMax = new Vector2(0.98f, 0.75f);
            muteBoxRect.offsetMin = Vector2.zero;
            muteBoxRect.offsetMax = Vector2.zero;
            var muteBoxImage = muteBox.AddComponent<Image>();
            muteBoxImage.color = new Color(0.8f, 0.8f, 0.82f, 1f);

            return row;
        }

        private static GameObject CreateSliderRow(string name, Transform parent, string labelText)
        {
            var row = CreateSimpleUIObject(name, parent);
            var layoutElement = row.AddComponent<LayoutElement>();
            layoutElement.preferredHeight = 60;

            var label = CreateSimpleTextObject("Label", row.transform, labelText);
            var labelRect = label.GetComponent<RectTransform>();
            labelRect.anchorMin = new Vector2(0, 0);
            labelRect.anchorMax = new Vector2(0.3f, 1);
            labelRect.offsetMin = Vector2.zero;
            labelRect.offsetMax = Vector2.zero;
            label.GetComponent<TMP_Text>().fontSize = 18;

            var sliderObj = CreateSimpleUIObject("Slider", row.transform);
            var sliderRect = sliderObj.GetComponent<RectTransform>();
            sliderRect.anchorMin = new Vector2(0.35f, 0.3f);
            sliderRect.anchorMax = new Vector2(0.85f, 0.7f);
            sliderRect.offsetMin = Vector2.zero;
            sliderRect.offsetMax = Vector2.zero;

            var slider = sliderObj.AddComponent<Slider>();
            slider.minValue = 0;
            slider.maxValue = 1;
            slider.value = 1;

            var bgObj = CreateSimpleUIObject("Background", sliderObj.transform);
            SetFullStretchRect(bgObj);
            var bgImage = bgObj.AddComponent<Image>();
            bgImage.color = new Color(0.3f, 0.3f, 0.3f, 1f);
            slider.targetGraphic = bgImage;

            var fillArea = CreateSimpleUIObject("FillArea", sliderObj.transform);
            var fillAreaRect = fillArea.GetComponent<RectTransform>();
            fillAreaRect.anchorMin = Vector2.zero;
            fillAreaRect.anchorMax = Vector2.one;
            fillAreaRect.offsetMin = new Vector2(5, 5);
            fillAreaRect.offsetMax = new Vector2(-5, -5);

            var fill = CreateSimpleUIObject("Fill", fillArea.transform);
            SetFullStretchRect(fill);
            var fillImage = fill.AddComponent<Image>();
            fillImage.color = new Color(0.8f, 0.6f, 0.2f, 1f);
            slider.fillRect = fill.GetComponent<RectTransform>();

            var handleArea = CreateSimpleUIObject("HandleSlideArea", sliderObj.transform);
            SetFullStretchRect(handleArea);
            handleArea.GetComponent<RectTransform>().offsetMin = new Vector2(10, 0);
            handleArea.GetComponent<RectTransform>().offsetMax = new Vector2(-10, 0);

            var handle = CreateSimpleUIObject("Handle", handleArea.transform);
            var handleRect = handle.GetComponent<RectTransform>();
            handleRect.sizeDelta = new Vector2(20, 0);
            var handleImage = handle.AddComponent<Image>();
            handleImage.color = Color.white;
            slider.handleRect = handleRect;

            var valueText = CreateSimpleTextObject("Value", row.transform, "100%");
            var valueRect = valueText.GetComponent<RectTransform>();
            valueRect.anchorMin = new Vector2(0.87f, 0);
            valueRect.anchorMax = new Vector2(1, 1);
            valueRect.offsetMin = Vector2.zero;
            valueRect.offsetMax = Vector2.zero;
            valueText.GetComponent<TMP_Text>().fontSize = 16;

            return row;
        }

        // ============================================
        // Build Settings
        // ============================================

        public static void UpdateBuildSettings()
        {
            var scenes = new EditorBuildSettingsScene[]
            {
                new EditorBuildSettingsScene($"{SCENES_PATH}/Boot.unity", true),
                new EditorBuildSettingsScene($"{SCENES_PATH}/MainMenu.unity", true),
                new EditorBuildSettingsScene($"{SCENES_PATH}/Bastion.unity", true),
                new EditorBuildSettingsScene($"{SCENES_PATH}/Missions.unity", true),
                new EditorBuildSettingsScene($"{SCENES_PATH}/BattleMission.unity", true),
                new EditorBuildSettingsScene($"{SCENES_PATH}/Requiems.unity", true),
                new EditorBuildSettingsScene($"{SCENES_PATH}/NullRift.unity", true),
                new EditorBuildSettingsScene($"{SCENES_PATH}/Combat.unity", true)
            };

            EditorBuildSettings.scenes = scenes;
            Debug.Log("[ProductionSceneSetupGenerator] Build settings updated with 8 scenes");
        }

        // ============================================
        // Simple Helper Methods (for new scenes)
        // ============================================

        private static GameObject CreateSimpleUIObject(string name, Transform parent)
        {
            var obj = new GameObject(name);
            obj.transform.SetParent(parent, false);
            obj.AddComponent<RectTransform>();
            return obj;
        }

        private static void SetFullStretchRect(GameObject obj)
        {
            var rect = obj.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        private static GameObject CreateSimpleButton(string name, Transform parent, string text)
        {
            var buttonObj = CreateSimpleUIObject(name, parent);
            var image = buttonObj.AddComponent<Image>();
            image.color = new Color(0.25f, 0.25f, 0.3f, 0.9f);

            var button = buttonObj.AddComponent<Button>();
            button.targetGraphic = image;

            var textObj = CreateSimpleTextObject("Text", buttonObj.transform, text);
            SetFullStretchRect(textObj);
            var tmpText = textObj.GetComponent<TMP_Text>();
            tmpText.alignment = TextAlignmentOptions.Center;
            tmpText.fontSize = 24;

            return buttonObj;
        }

        private static GameObject CreateLargeButtonWithSubtitle(string name, Transform parent, string title, string subtitle)
        {
            var buttonObj = CreateSimpleUIObject(name, parent);
            var image = buttonObj.AddComponent<Image>();
            image.color = new Color(0.2f, 0.25f, 0.35f, 0.9f);

            var button = buttonObj.AddComponent<Button>();
            button.targetGraphic = image;

            var titleObj = CreateSimpleTextObject("Title", buttonObj.transform, title);
            var titleRect = titleObj.GetComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0, 0.5f);
            titleRect.anchorMax = new Vector2(1, 0.8f);
            titleRect.offsetMin = new Vector2(20, 0);
            titleRect.offsetMax = new Vector2(-20, 0);
            var titleText = titleObj.GetComponent<TMP_Text>();
            titleText.fontSize = 28;
            titleText.fontStyle = FontStyles.Bold;
            titleText.alignment = TextAlignmentOptions.Center;

            var subtitleObj = CreateSimpleTextObject("Subtitle", buttonObj.transform, subtitle);
            var subtitleRect = subtitleObj.GetComponent<RectTransform>();
            subtitleRect.anchorMin = new Vector2(0, 0.2f);
            subtitleRect.anchorMax = new Vector2(1, 0.45f);
            subtitleRect.offsetMin = new Vector2(20, 0);
            subtitleRect.offsetMax = new Vector2(-20, 0);
            var subtitleText = subtitleObj.GetComponent<TMP_Text>();
            subtitleText.fontSize = 16;
            subtitleText.color = new Color(0.7f, 0.7f, 0.7f, 1f);
            subtitleText.alignment = TextAlignmentOptions.Center;

            return buttonObj;
        }

        private static GameObject CreateSimpleTextObject(string name, Transform parent, string text)
        {
            var textObj = CreateSimpleUIObject(name, parent);
            var tmpText = textObj.AddComponent<TextMeshProUGUI>();
            tmpText.text = text;
            tmpText.color = Color.white;
            tmpText.fontSize = 20;
            tmpText.alignment = TextAlignmentOptions.Left;
            return textObj;
        }
    }
}
