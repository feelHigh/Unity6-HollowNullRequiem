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
using HNR.Editor.LayerLab;

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
        private const string BACKGROUND_CONFIG_PATH = "Assets/_Project/Data/Config/BackgroundConfig.asset";
        private const string COMBAT_CONFIG_PATH = "Assets/_Project/Data/Config/CombatConfig.asset";
        private const string BANNER_CONFIG_PATH = "Assets/_Project/Data/Config/BannerConfig.asset";
        private const string LAYERLAB_CONFIG_PATH = "Assets/_Project/Data/Config/LayerLabSpriteConfig.asset";
        private const string RUNTIME_PREFAB_CONFIG_PATH = "Assets/_Project/Resources/Config/RuntimeUIPrefabConfig.asset";

        // Cached background config for current generation run
        private static BackgroundConfigSO _backgroundConfig;
        // Cached LayerLab sprite config for current generation run (also contains all icon sprites)
        private static LayerLabSpriteConfigSO _layerLabConfig;
        // Cached banner config for current generation run
        private static BannerConfigSO _bannerConfig;
        // Cached runtime UI prefab config for current generation run
        private static RuntimeUIPrefabConfigSO _runtimePrefabConfig;

        /// <summary>
        /// Loads the background configuration asset.
        /// </summary>
        /// <returns>The background config or null if not found</returns>
        private static BackgroundConfigSO LoadBackgroundConfig()
        {
            if (_backgroundConfig == null)
            {
                _backgroundConfig = AssetDatabase.LoadAssetAtPath<BackgroundConfigSO>(BACKGROUND_CONFIG_PATH);
                if (_backgroundConfig == null)
                {
                    Debug.LogWarning("[ProductionSceneSetupGenerator] BackgroundConfig not found at " + BACKGROUND_CONFIG_PATH + ". Using color fallback. Create via 'Create > HNR > Config > Background Config'.");
                }
            }
            return _backgroundConfig;
        }

        /// <summary>
        /// Loads the banner configuration asset.
        /// </summary>
        /// <returns>The banner config or null if not found</returns>
        private static BannerConfigSO LoadBannerConfig()
        {
            if (_bannerConfig == null)
            {
                _bannerConfig = AssetDatabase.LoadAssetAtPath<BannerConfigSO>(BANNER_CONFIG_PATH);
                if (_bannerConfig == null)
                {
                    Debug.LogWarning("[ProductionSceneSetupGenerator] BannerConfig not found at " + BANNER_CONFIG_PATH + ". Run 'HNR > 5. Utilities > Config > Generate Banner Config' first.");
                }
            }
            return _bannerConfig;
        }

        /// <summary>
        /// Clears all cached configs (call after scene generation batch completes).
        /// </summary>
        private static void ClearAllConfigCaches()
        {
            _backgroundConfig = null;
            _bannerConfig = null;
            _layerLabConfig = null;
            _runtimePrefabConfig = null;
        }

        /// <summary>
        /// Loads the runtime UI prefab configuration asset.
        /// </summary>
        /// <returns>The runtime prefab config or null if not found</returns>
        private static RuntimeUIPrefabConfigSO LoadRuntimePrefabConfig()
        {
            if (_runtimePrefabConfig == null)
            {
                _runtimePrefabConfig = AssetDatabase.LoadAssetAtPath<RuntimeUIPrefabConfigSO>(RUNTIME_PREFAB_CONFIG_PATH);
                if (_runtimePrefabConfig == null)
                {
                    Debug.LogWarning("[ProductionSceneSetupGenerator] RuntimeUIPrefabConfig not found. Run 'HNR > 5. Utilities > Config > Generate Runtime UI Prefab Config' first.");
                }
            }
            return _runtimePrefabConfig;
        }

        /// <summary>
        /// Loads the LayerLab sprite configuration asset.
        /// </summary>
        /// <returns>The LayerLab config or null if not found</returns>
        private static LayerLabSpriteConfigSO LoadLayerLabConfig()
        {
            if (_layerLabConfig == null)
            {
                _layerLabConfig = AssetDatabase.LoadAssetAtPath<LayerLabSpriteConfigSO>(LAYERLAB_CONFIG_PATH);
                if (_layerLabConfig == null)
                {
                    Debug.LogWarning("[ProductionSceneSetupGenerator] LayerLabSpriteConfig not found. Run 'HNR > 5. Utilities > Config > Generate LayerLab Sprite Config' first. Using fallback styling.");
                }
            }
            return _layerLabConfig;
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

            // === Settings Overlay ===
            CreateSettingsOverlay(canvasObj.transform);

            // === Background ===
            CreateSpriteBackground(canvasObj, "MainMenu", new Color(0.05f, 0.02f, 0.1f));

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

            // === Event Banner Carousel ===
            GameObject eventBannerCarousel = CreateEventBannerCarousel(bastionScreen);
            if (eventBannerCarousel != null)
            {
                // Wire carousel to BastionScreen
                var bastionScreenComp = bastionScreen.GetComponent<BastionScreen>();
                if (bastionScreenComp != null)
                {
                    SerializedObject bastionSO = new SerializedObject(bastionScreenComp);
                    bastionSO.FindProperty("_eventBannerCarousel").objectReferenceValue =
                        eventBannerCarousel.GetComponent<EventBannerCarousel>();
                    bastionSO.ApplyModifiedPropertiesWithoutUndo();
                }
            }

            // === Overlay Container ===
            GameObject overlayContainer = CreateUIContainer(canvasObj, "OverlayContainer");

            // === RequiemSelectionScreen (overlay) ===
            GameObject requiemSelectionScreen = CreateRequiemSelectionScreen(overlayContainer);

            // === Settings Overlay ===
            CreateSettingsOverlay(canvasObj.transform);

            // === Background ===
            CreateSpriteBackground(canvasObj, "Bastion", new Color(0.08f, 0.05f, 0.12f));

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

            // === Sanctuary World-Space Setup (like Combat scene) ===
            // Uses world-space background and Requiem positioning
            // UI canvas is transparent, allowing world-space objects to show through
            GameObject sanctuaryVisualsParent = new GameObject("--- SANCTUARY VISUALS ---");

            // Create world-space background for Sanctuary (like Combat's WorldBackground)
            // This is shown/hidden when Sanctuary screen shows/hides
            GameObject sanctuaryWorldBgObj = new GameObject("SanctuaryWorldBackground");
            sanctuaryWorldBgObj.transform.SetParent(sanctuaryVisualsParent.transform);
            sanctuaryWorldBgObj.transform.position = new Vector3(0, 0, 10f); // Behind everything
            var sanctuaryBgRenderer = sanctuaryWorldBgObj.AddComponent<SpriteRenderer>();
            sanctuaryBgRenderer.sortingOrder = -1000; // Render behind everything
            sanctuaryBgRenderer.drawMode = SpriteDrawMode.Sliced;
            sanctuaryBgRenderer.size = new Vector2(24f, 14f); // Cover camera view

            // Load and assign Sanctuary background sprite from config
            var bgConfig = LoadBackgroundConfig();
            if (bgConfig?.SanctuaryBackground != null)
            {
                sanctuaryBgRenderer.sprite = bgConfig.SanctuaryBackground;
                Debug.Log("[ProductionSceneSetupGenerator] Assigned Sanctuary background to world-space SpriteRenderer");
            }
            else
            {
                sanctuaryBgRenderer.color = new Color(0.05f, 0.08f, 0.06f); // Greenish dark fallback
            }
            sanctuaryWorldBgObj.SetActive(false); // Start hidden

            // Create world-space slots for Requiem visuals (in camera view)
            // Positions calibrated for orthographic camera with size ~5
            // Y positions lowered to place Requiems near bottom of screen
            GameObject leftSlot = new GameObject("SanctuarySlot_Left");
            leftSlot.transform.SetParent(sanctuaryVisualsParent.transform);
            leftSlot.transform.position = new Vector3(-3f, -4f, 0f);

            GameObject centerSlot = new GameObject("SanctuarySlot_Center");
            centerSlot.transform.SetParent(sanctuaryVisualsParent.transform);
            centerSlot.transform.position = new Vector3(0f, -3.5f, 0f);

            GameObject rightSlot = new GameObject("SanctuarySlot_Right");
            rightSlot.transform.SetParent(sanctuaryVisualsParent.transform);
            rightSlot.transform.position = new Vector3(3f, -4f, 0f);

            // Create SanctuaryVisualController
            GameObject sanctuaryVisualControllerObj = new GameObject("SanctuaryVisualController");
            sanctuaryVisualControllerObj.transform.SetParent(managersParent.transform);
            var sanctuaryVisualController = sanctuaryVisualControllerObj.AddComponent<SanctuaryVisualController>();

            // Wire slot and world background references to controller
            SerializedObject visualControllerSO = new SerializedObject(sanctuaryVisualController);
            visualControllerSO.FindProperty("_leftSlot").objectReferenceValue = leftSlot.transform;
            visualControllerSO.FindProperty("_centerSlot").objectReferenceValue = centerSlot.transform;
            visualControllerSO.FindProperty("_rightSlot").objectReferenceValue = rightSlot.transform;
            visualControllerSO.FindProperty("_worldBackground").objectReferenceValue = sanctuaryBgRenderer;
            visualControllerSO.ApplyModifiedPropertiesWithoutUndo();

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

            // Wire SanctuaryVisualController to SanctuaryScreen and wire RawImage to controller
            var sanctuaryScreenComp = sanctuaryScreen.GetComponent<SanctuaryScreen>();
            if (sanctuaryScreenComp != null)
            {
                var sanctuaryScreenSO = new SerializedObject(sanctuaryScreenComp);
                sanctuaryScreenSO.FindProperty("_visualController").objectReferenceValue = sanctuaryVisualController;
                sanctuaryScreenSO.ApplyModifiedPropertiesWithoutUndo();
                Debug.Log("[ProductionSceneSetupGenerator] Wired SanctuaryVisualController to SanctuaryScreen");
            }

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

            // === RelicShopOverlay (overlay for relic purchases) ===
            GameObject relicShopOverlay = CreateRelicShopOverlay(overlayContainer);

            // Wire RelicShopOverlay and DeckViewerModal to ShopScreen
            var shopScreenComp = shopScreen.GetComponent<ShopScreen>();
            if (shopScreenComp != null)
            {
                var shopScreenSo = new SerializedObject(shopScreenComp);
                shopScreenSo.FindProperty("_relicShopOverlay").objectReferenceValue = relicShopOverlay.GetComponent<RelicShopOverlay>();
                shopScreenSo.FindProperty("_deckViewerModal").objectReferenceValue = deckViewerModal.GetComponent<DeckViewerModal>();
                shopScreenSo.ApplyModifiedPropertiesWithoutUndo();
                Debug.Log("[ProductionSceneSetupGenerator] Wired RelicShopOverlay and DeckViewerModal to ShopScreen");
            }

            // === Background ===
            GameObject zoneBackgroundObj = CreateZoneBackground(canvasObj, 1, new Color(0.03f, 0.01f, 0.08f));

            // Wire ZoneBackground to SanctuaryVisualController so it can be hidden when Sanctuary shows
            var zoneBackgroundImage = zoneBackgroundObj.GetComponent<Image>();
            if (zoneBackgroundImage != null && sanctuaryVisualController != null)
            {
                SerializedObject visualControllerSO2 = new SerializedObject(sanctuaryVisualController);
                visualControllerSO2.FindProperty("_zoneBackground").objectReferenceValue = zoneBackgroundImage;
                visualControllerSO2.ApplyModifiedPropertiesWithoutUndo();
                Debug.Log("[ProductionSceneSetupGenerator] Wired ZoneBackground to SanctuaryVisualController");
            }

            // Add ZoneBackgroundController for dynamic zone-based backgrounds
            var zoneBackgroundController = zoneBackgroundObj.AddComponent<ZoneBackgroundController>();
            if (zoneBackgroundController != null)
            {
                SerializedObject zoneBgSo = new SerializedObject(zoneBackgroundController);
                zoneBgSo.FindProperty("_backgroundImage").objectReferenceValue = zoneBackgroundImage;
                zoneBgSo.FindProperty("_backgroundConfig").objectReferenceValue = bgConfig;
                zoneBgSo.ApplyModifiedPropertiesWithoutUndo();
                Debug.Log("[ProductionSceneSetupGenerator] Added ZoneBackgroundController with config wiring");
            }

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
            var turnManager = turnManagerObj.AddComponent<TurnManager>();
            WireTurnManager(turnManager);

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

            // === Requiem Art Executor (executes ultimate ability effects) ===
            GameObject requiemArtExecutorObj = new GameObject("RequiemArtExecutor");
            requiemArtExecutorObj.transform.SetParent(managersParent.transform);
            requiemArtExecutorObj.AddComponent<RequiemArtExecutor>();

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

            // === Auto-Battle Controller ===
            GameObject autoBattleObj = new GameObject("AutoBattleController");
            autoBattleObj.transform.SetParent(managersParent.transform);
            autoBattleObj.AddComponent<AutoBattleController>();

            // === Combat Background Controller ===
            GameObject combatBgControllerObj = new GameObject("CombatBackgroundController");
            combatBgControllerObj.transform.SetParent(managersParent.transform);
            var combatBgController = combatBgControllerObj.AddComponent<CombatBackgroundController>();

            // === World-Space Background ===
            // Background SpriteRenderer placed behind all world-space elements
            GameObject worldBgObj = new GameObject("WorldBackground");
            worldBgObj.transform.position = new Vector3(0, 0, 10f); // Behind everything
            var bgRenderer = worldBgObj.AddComponent<SpriteRenderer>();
            bgRenderer.sortingOrder = -1000; // Render behind everything
            bgRenderer.drawMode = SpriteDrawMode.Sliced;
            bgRenderer.size = new Vector2(24f, 14f); // Cover camera view

            // Wire CombatBackgroundController
            var bgConfig = LoadBackgroundConfig();
            SerializedObject combatBgSO = new SerializedObject(combatBgController);
            combatBgSO.FindProperty("_backgroundRenderer").objectReferenceValue = bgRenderer;
            if (bgConfig != null)
            {
                combatBgSO.FindProperty("_backgroundConfig").objectReferenceValue = bgConfig;
            }
            combatBgSO.ApplyModifiedPropertiesWithoutUndo();

            // === Combat Ground (Floor Surface) ===
            // Ground SpriteRenderer placed beneath characters to prevent floating appearance
            // Characters (enemy/ally slots) are at Y=0, ground covers from character base to bottom of screen
            // Camera view is roughly 14 units tall (based on WorldBackground), centered at origin
            GameObject combatGroundObj = new GameObject("CombatGround");
            combatGroundObj.transform.position = new Vector3(0, -3f, 5f); // Center at Y=-3, top at Y=2, bottom at Y=-8
            var groundRenderer = combatGroundObj.AddComponent<SpriteRenderer>();
            groundRenderer.sortingOrder = -900; // In front of background (-1000), behind characters
            groundRenderer.drawMode = SpriteDrawMode.Sliced;
            groundRenderer.size = new Vector2(24f, 10f); // Covers from character feet to bottom of screen

            // Assign ground sprite from config if available
            if (bgConfig?.CombatGroundSprite != null)
            {
                groundRenderer.sprite = bgConfig.CombatGroundSprite;
                Debug.Log("[ProductionSceneSetupGenerator] Assigned Combat ground sprite from BackgroundConfig");
            }
            else
            {
                // Use a dark color as fallback (ground will be visible but plain)
                groundRenderer.color = new Color(0.05f, 0.03f, 0.08f, 0.9f);
                Debug.Log("[ProductionSceneSetupGenerator] Using fallback color for Combat ground (no sprite assigned)");
            }

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
                slot.transform.position = new Vector3(enemyXPositions[i], -1f, 0f);
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
                slot.transform.position = new Vector3(allyXPositions[i], -1f, 0f);
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

            // === SettingsOverlay (for combat settings with pause) ===
            CreateSettingsOverlay(overlayContainer.transform);

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

        /// <summary>
        /// Creates a background with sprite support, falling back to color if sprite not available.
        /// </summary>
        /// <param name="canvas">Parent canvas GameObject</param>
        /// <param name="sceneName">Scene name for background lookup</param>
        /// <param name="fallbackColor">Color to use if sprite not available</param>
        /// <returns>The background GameObject</returns>
        private static GameObject CreateSpriteBackground(GameObject canvas, string sceneName, Color fallbackColor)
        {
            GameObject bg = new GameObject("Background");
            bg.transform.SetParent(canvas.transform, false);
            bg.transform.SetAsFirstSibling();

            RectTransform rect = bg.AddComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.sizeDelta = Vector2.zero;

            Image img = bg.AddComponent<Image>();
            img.raycastTarget = false;

            var bgConfig = LoadBackgroundConfig();
            Sprite bgSprite = bgConfig?.GetSceneBackground(sceneName);

            if (bgSprite != null)
            {
                img.sprite = bgSprite;
                img.color = Color.white;
                img.preserveAspect = false; // Stretch to fill
                Debug.Log($"[ProductionSceneSetupGenerator] Applied sprite background for {sceneName}");
            }
            else
            {
                img.color = fallbackColor;
                Debug.Log($"[ProductionSceneSetupGenerator] Using fallback color for {sceneName} background (sprite not assigned)");
            }

            return bg;
        }

        /// <summary>
        /// Creates a background specifically for NullRift with zone support.
        /// </summary>
        /// <param name="canvas">Parent canvas GameObject</param>
        /// <param name="defaultZone">Default zone number (1-3)</param>
        /// <param name="fallbackColor">Fallback color if sprite not available</param>
        /// <returns>The background GameObject</returns>
        private static GameObject CreateZoneBackground(GameObject canvas, int defaultZone, Color fallbackColor)
        {
            GameObject bg = new GameObject("ZoneBackground");
            bg.transform.SetParent(canvas.transform, false);
            bg.transform.SetAsFirstSibling();

            RectTransform rect = bg.AddComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.sizeDelta = Vector2.zero;

            Image img = bg.AddComponent<Image>();
            img.raycastTarget = false;

            var bgConfig = LoadBackgroundConfig();
            Sprite bgSprite = bgConfig?.GetZoneBackground(defaultZone);

            if (bgSprite != null)
            {
                img.sprite = bgSprite;
                img.color = Color.white;
                img.preserveAspect = false;
                Debug.Log($"[ProductionSceneSetupGenerator] Applied Zone {defaultZone} background sprite");
            }
            else
            {
                img.color = fallbackColor;
                Debug.Log($"[ProductionSceneSetupGenerator] Using fallback color for Zone {defaultZone} background");
            }

            return bg;
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

            // === Title with LayerLab Frame ===
            var layerLabConfig = LoadLayerLabConfig();

            // Title frame container (using ListFrame for elegant background)
            GameObject titleFrameObj = new GameObject("TitleFrame");
            titleFrameObj.transform.SetParent(screenObj.transform, false);
            RectTransform titleFrameRect = titleFrameObj.AddComponent<RectTransform>();
            titleFrameRect.anchorMin = new Vector2(0.5f, 0.72f);
            titleFrameRect.anchorMax = new Vector2(0.5f, 0.82f);
            titleFrameRect.anchoredPosition = Vector2.zero;
            titleFrameRect.sizeDelta = new Vector2(900, 120);

            // Frame background using LayerLab sprite if available
            Image frameBg = titleFrameObj.AddComponent<Image>();
            if (layerLabConfig != null && layerLabConfig.TabMenuBg != null)
            {
                frameBg.sprite = layerLabConfig.TabMenuBg;
                frameBg.type = Image.Type.Sliced;
                frameBg.color = new Color(0.15f, 0.08f, 0.25f, 0.85f); // Dark purple tint

                // Add border layer if available
                if (layerLabConfig.TabMenuBorder != null)
                {
                    GameObject borderObj = new GameObject("Border");
                    borderObj.transform.SetParent(titleFrameObj.transform, false);
                    RectTransform borderRect = borderObj.AddComponent<RectTransform>();
                    borderRect.anchorMin = Vector2.zero;
                    borderRect.anchorMax = Vector2.one;
                    borderRect.sizeDelta = Vector2.zero;
                    Image borderImg = borderObj.AddComponent<Image>();
                    borderImg.sprite = layerLabConfig.TabMenuBorder;
                    borderImg.type = Image.Type.Sliced;
                    borderImg.color = new Color(0.6f, 0.4f, 0.8f, 0.9f); // Purple border
                    borderImg.raycastTarget = false;
                }
            }
            else
            {
                frameBg.color = new Color(0.1f, 0.05f, 0.15f, 0.85f); // Fallback dark purple
            }
            frameBg.raycastTarget = false;

            // Title text - larger font (80 instead of 64)
            GameObject titleObj = CreateText(titleFrameObj, "Title", "HOLLOW NULL REQUIEM", 80);
            RectTransform titleRect = titleObj.GetComponent<RectTransform>();
            titleRect.anchorMin = Vector2.zero;
            titleRect.anchorMax = Vector2.one;
            titleRect.anchoredPosition = Vector2.zero;
            titleRect.sizeDelta = Vector2.zero;
            titleRect.offsetMin = new Vector2(20, 10);
            titleRect.offsetMax = new Vector2(-20, -10);

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

            // === Version Text (positioned to avoid cutoff) ===
            GameObject versionObj = CreateText(screenObj, "VersionText", "v0.1.0", 18);
            RectTransform versionRect = versionObj.GetComponent<RectTransform>();
            versionRect.anchorMin = new Vector2(1, 0);
            versionRect.anchorMax = new Vector2(1, 0);
            versionRect.anchoredPosition = new Vector2(-30, 30); // Increased margin to avoid cutoff
            versionRect.sizeDelta = new Vector2(150, 40); // Increased width for safety
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
            playerInfoHLG.spacing = 25; // Increased spacing between LevelBadge and Nickname
            playerInfoHLG.childAlignment = TextAnchor.MiddleLeft;
            playerInfoHLG.childForceExpandWidth = false;
            playerInfoHLG.childForceExpandHeight = false;

            // Level badge with LayerLab styling
            var bastionLayerLabConfig = LoadLayerLabConfig();

            GameObject levelBadge = new GameObject("LevelBadge");
            levelBadge.transform.SetParent(playerInfoContainer.transform, false);
            levelBadge.AddComponent<RectTransform>();
            LayoutElement levelBadgeLE = levelBadge.AddComponent<LayoutElement>();
            levelBadgeLE.preferredWidth = 70; // Slightly larger for LayerLab frame
            levelBadgeLE.preferredHeight = 70;

            // Use LayerLab StageFrame for badge if available
            Image levelBadgeImage = levelBadge.AddComponent<Image>();
            if (bastionLayerLabConfig != null && bastionLayerLabConfig.StageFrameBg != null)
            {
                levelBadgeImage.sprite = bastionLayerLabConfig.StageFrameBg;
                levelBadgeImage.type = Image.Type.Sliced;
                levelBadgeImage.color = new Color(0.5f, 0.35f, 0.7f, 0.95f); // Purple tint

                // Add border layer for badge
                if (bastionLayerLabConfig.StageFrameBorder != null)
                {
                    GameObject badgeBorder = new GameObject("Border");
                    badgeBorder.transform.SetParent(levelBadge.transform, false);
                    RectTransform badgeBorderRect = badgeBorder.AddComponent<RectTransform>();
                    badgeBorderRect.anchorMin = Vector2.zero;
                    badgeBorderRect.anchorMax = Vector2.one;
                    badgeBorderRect.sizeDelta = Vector2.zero;
                    Image badgeBorderImg = badgeBorder.AddComponent<Image>();
                    badgeBorderImg.sprite = bastionLayerLabConfig.StageFrameBorder;
                    badgeBorderImg.type = Image.Type.Sliced;
                    badgeBorderImg.color = new Color(0.7f, 0.5f, 0.9f, 1f); // Bright purple border
                    badgeBorderImg.raycastTarget = false;
                }

                // Add glow layer for emphasis
                if (bastionLayerLabConfig.StageFrameFocus != null)
                {
                    GameObject badgeGlow = new GameObject("Glow");
                    badgeGlow.transform.SetParent(levelBadge.transform, false);
                    RectTransform badgeGlowRect = badgeGlow.AddComponent<RectTransform>();
                    badgeGlowRect.anchorMin = new Vector2(-0.1f, -0.1f);
                    badgeGlowRect.anchorMax = new Vector2(1.1f, 1.1f);
                    badgeGlowRect.sizeDelta = Vector2.zero;
                    Image badgeGlowImg = badgeGlow.AddComponent<Image>();
                    badgeGlowImg.sprite = bastionLayerLabConfig.StageFrameFocus;
                    badgeGlowImg.type = Image.Type.Sliced;
                    badgeGlowImg.color = new Color(0.6f, 0.4f, 0.9f, 0.4f); // Subtle purple glow
                    badgeGlowImg.raycastTarget = false;
                    badgeGlow.transform.SetAsFirstSibling(); // Behind everything
                }
            }
            else
            {
                levelBadgeImage.color = new Color(0.3f, 0.4f, 0.5f, 0.9f); // Fallback
            }

            GameObject levelTextObj = CreateText(levelBadge, "LevelText", "LV\n1", 28); // Larger font size
            RectTransform levelTextRect = levelTextObj.GetComponent<RectTransform>();
            levelTextRect.anchorMin = Vector2.zero;
            levelTextRect.anchorMax = Vector2.one;
            levelTextRect.sizeDelta = Vector2.zero;
            TextMeshProUGUI levelTMP = levelTextObj.GetComponent<TextMeshProUGUI>();
            levelTMP.alignment = TextAlignmentOptions.Center;
            levelTMP.fontStyle = FontStyles.Bold;
            levelTMP.color = Color.white; // White for better visibility

            // Nickname - larger font
            GameObject nicknameObj = CreateText(playerInfoContainer, "Nickname", "Commander", 32);
            LayoutElement nicknameLE = nicknameObj.AddComponent<LayoutElement>();
            nicknameLE.preferredWidth = 200; // Wider for larger text
            nicknameLE.preferredHeight = 60;
            TextMeshProUGUI nicknameText = nicknameObj.GetComponent<TextMeshProUGUI>();
            nicknameText.fontStyle = FontStyles.Bold;
            nicknameText.alignment = TextAlignmentOptions.Left;

            // ============================================
            // Settings Button (Top Right) - LayerLab Convex Rectangle style
            // ============================================
            var layerLabConfig = LoadLayerLabConfig();
            Sprite settingsIcon = layerLabConfig?.IconSettings;
            GameObject settingsButton = CreateLayerLabIconButton(screenObj, "SettingsButton", settingsIcon);
            RectTransform settingsRect = settingsButton.GetComponent<RectTransform>();
            settingsRect.anchorMin = new Vector2(1, 1);
            settingsRect.anchorMax = new Vector2(1, 1);
            settingsRect.pivot = new Vector2(1, 1);
            settingsRect.sizeDelta = new Vector2(60, 60);
            settingsRect.anchoredPosition = new Vector2(-20, -20);

            // ============================================
            // Navigation Buttons (Right Side - Vertical Stack)
            // ============================================
            GameObject navContainer = new GameObject("NavigationButtons");
            navContainer.transform.SetParent(screenObj.transform, false);
            RectTransform navRect = navContainer.AddComponent<RectTransform>();
            // Position on right side of screen (raised to align with event banner)
            navRect.anchorMin = new Vector2(0.7f, 0.55f);
            navRect.anchorMax = new Vector2(0.98f, 0.85f);
            navRect.offsetMin = Vector2.zero;
            navRect.offsetMax = Vector2.zero;

            VerticalLayoutGroup navVLG = navContainer.AddComponent<VerticalLayoutGroup>();
            navVLG.spacing = 15;
            navVLG.childAlignment = TextAnchor.MiddleRight;
            navVLG.childControlWidth = true;
            navVLG.childControlHeight = true;
            navVLG.childForceExpandWidth = false; // Don't force expand - use fixed width
            navVLG.childForceExpandHeight = false;
            navVLG.childScaleWidth = false;
            navVLG.childScaleHeight = false;
            navVLG.padding = new RectOffset(10, 10, 10, 10);

            // Missions button (larger - 300x75)
            GameObject missionsButton = CreateWideNavButton(navContainer, "MissionsButton", "Missions");
            LayoutElement missionsLE = missionsButton.AddComponent<LayoutElement>();
            missionsLE.preferredWidth = 300;
            missionsLE.preferredHeight = 75;
            missionsLE.minWidth = 280;
            missionsLE.minHeight = 60;

            // Requiems button (larger - 300x75)
            GameObject requiemsButton = CreateWideNavButton(navContainer, "RequiemsButton", "Requiems");
            LayoutElement requiemsLE = requiemsButton.AddComponent<LayoutElement>();
            requiemsLE.preferredWidth = 300;
            requiemsLE.preferredHeight = 75;
            requiemsLE.minWidth = 280;
            requiemsLE.minHeight = 60;

            // === Placeholder buttons for future features (gray/disabled style) ===
            // Collection button (placeholder)
            GameObject collectionButton = CreateWideNavButton(navContainer, "CollectionButton", "Collection");
            LayoutElement collectionLE = collectionButton.AddComponent<LayoutElement>();
            collectionLE.preferredWidth = 300;
            collectionLE.preferredHeight = 65;
            collectionLE.minWidth = 280;
            collectionLE.minHeight = 55;
            SetButtonDisabledStyle(collectionButton);

            // Shop button (placeholder)
            GameObject shopButton = CreateWideNavButton(navContainer, "ShopButton", "Shop");
            LayoutElement shopLE = shopButton.AddComponent<LayoutElement>();
            shopLE.preferredWidth = 300;
            shopLE.preferredHeight = 65;
            shopLE.minWidth = 280;
            shopLE.minHeight = 55;
            SetButtonDisabledStyle(shopButton);

            // Achievements button (placeholder)
            GameObject achievementsButton = CreateWideNavButton(navContainer, "AchievementsButton", "Achievements");
            LayoutElement achievementsLE = achievementsButton.AddComponent<LayoutElement>();
            achievementsLE.preferredWidth = 300;
            achievementsLE.preferredHeight = 65;
            achievementsLE.minWidth = 280;
            achievementsLE.minHeight = 55;
            SetButtonDisabledStyle(achievementsButton);

            // Events button (placeholder)
            GameObject eventsButton = CreateWideNavButton(navContainer, "EventsButton", "Events");
            LayoutElement eventsLE = eventsButton.AddComponent<LayoutElement>();
            eventsLE.preferredWidth = 300;
            eventsLE.preferredHeight = 65;
            eventsLE.minWidth = 280;
            eventsLE.minHeight = 55;
            SetButtonDisabledStyle(eventsButton);

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
        /// Creates the event banner carousel for the Bastion screen.
        /// Positioned below the player info area.
        /// </summary>
        private static GameObject CreateEventBannerCarousel(GameObject parent)
        {
            var bannerConfig = LoadBannerConfig();

            // Create carousel container
            GameObject carouselObj = new GameObject("EventBannerCarousel");
            carouselObj.transform.SetParent(parent.transform, false);

            RectTransform carouselRect = carouselObj.AddComponent<RectTransform>();
            // Position below player info area (top-left quadrant) - half width
            carouselRect.anchorMin = new Vector2(0.02f, 0.65f);
            carouselRect.anchorMax = new Vector2(0.285f, 0.83f);
            carouselRect.offsetMin = Vector2.zero;
            carouselRect.offsetMax = Vector2.zero;

            // Background
            Image carouselBg = carouselObj.AddComponent<Image>();
            carouselBg.color = new Color(0.08f, 0.08f, 0.12f, 0.9f);

            // Create viewport (leave space at bottom for indicators)
            GameObject viewportObj = new GameObject("Viewport");
            viewportObj.transform.SetParent(carouselObj.transform, false);

            RectTransform viewportRect = viewportObj.AddComponent<RectTransform>();
            viewportRect.anchorMin = new Vector2(0, 0.15f); // Leave 15% for indicators
            viewportRect.anchorMax = Vector2.one;
            viewportRect.offsetMin = Vector2.zero;
            viewportRect.offsetMax = Vector2.zero;

            Image viewportImage = viewportObj.AddComponent<Image>();
            viewportImage.color = Color.white; // Must be opaque for Mask to work!
            Mask viewportMask = viewportObj.AddComponent<Mask>();
            viewportMask.showMaskGraphic = false; // Hide the white image, but mask still uses its alpha

            // Create content container
            GameObject contentObj = new GameObject("Content");
            contentObj.transform.SetParent(viewportObj.transform, false);

            RectTransform contentRect = contentObj.AddComponent<RectTransform>();
            // Anchor to left edge, stretch vertically
            contentRect.anchorMin = new Vector2(0, 0);
            contentRect.anchorMax = new Vector2(0, 1);
            contentRect.pivot = new Vector2(0, 0.5f);
            contentRect.anchoredPosition = Vector2.zero;
            contentRect.offsetMin = Vector2.zero;
            contentRect.offsetMax = Vector2.zero;
            // Width will be set by ContentSizeFitter based on children

            HorizontalLayoutGroup contentHLG = contentObj.AddComponent<HorizontalLayoutGroup>();
            contentHLG.spacing = 0;
            contentHLG.childAlignment = TextAnchor.MiddleLeft;
            contentHLG.childForceExpandWidth = false;
            contentHLG.childForceExpandHeight = false;
            contentHLG.childControlWidth = true;  // Use LayoutElement preferred width
            contentHLG.childControlHeight = false; // Let children use their own height
            contentHLG.childScaleWidth = false;
            contentHLG.childScaleHeight = false;

            ContentSizeFitter contentSizeFitter = contentObj.AddComponent<ContentSizeFitter>();
            contentSizeFitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            contentSizeFitter.verticalFit = ContentSizeFitter.FitMode.Unconstrained; // Height from viewport

            // Create ScrollRect
            ScrollRect scrollRect = carouselObj.AddComponent<ScrollRect>();
            scrollRect.content = contentRect;
            scrollRect.viewport = viewportRect;
            scrollRect.horizontal = true;
            scrollRect.vertical = false;
            scrollRect.movementType = ScrollRect.MovementType.Elastic;
            scrollRect.elasticity = 0.1f;
            scrollRect.inertia = true;
            scrollRect.decelerationRate = 0.135f;
            scrollRect.scrollSensitivity = 1f;

            // Create indicator container
            GameObject indicatorContainer = new GameObject("IndicatorContainer");
            indicatorContainer.transform.SetParent(carouselObj.transform, false);

            RectTransform indicatorRect = indicatorContainer.AddComponent<RectTransform>();
            indicatorRect.anchorMin = new Vector2(0.3f, 0);
            indicatorRect.anchorMax = new Vector2(0.7f, 0.15f);
            indicatorRect.offsetMin = Vector2.zero;
            indicatorRect.offsetMax = Vector2.zero;

            HorizontalLayoutGroup indicatorHLG = indicatorContainer.AddComponent<HorizontalLayoutGroup>();
            indicatorHLG.spacing = bannerConfig?.IndicatorSpacing ?? 8f;
            indicatorHLG.childAlignment = TextAnchor.MiddleCenter;
            indicatorHLG.childForceExpandWidth = false;
            indicatorHLG.childForceExpandHeight = false;

            // Create drag handler
            BannerDragHandler dragHandler = viewportObj.AddComponent<BannerDragHandler>();

            // Add EventBannerCarousel component
            EventBannerCarousel carousel = carouselObj.AddComponent<EventBannerCarousel>();

            // Wire references
            SerializedObject carouselSO = new SerializedObject(carousel);
            carouselSO.FindProperty("_bannerConfig").objectReferenceValue = bannerConfig;
            carouselSO.FindProperty("_scrollRect").objectReferenceValue = scrollRect;
            carouselSO.FindProperty("_contentContainer").objectReferenceValue = contentRect;
            carouselSO.FindProperty("_indicatorContainer").objectReferenceValue = indicatorRect;
            carouselSO.FindProperty("_dragHandler").objectReferenceValue = dragHandler;

            // Wire runtime prefabs if available
            var runtimePrefabConfig = LoadRuntimePrefabConfig();
            if (runtimePrefabConfig != null)
            {
                if (runtimePrefabConfig.BannerSlidePrefab != null)
                {
                    carouselSO.FindProperty("_slidePrefab").objectReferenceValue = runtimePrefabConfig.BannerSlidePrefab;
                    Debug.Log("[ProductionSceneSetupGenerator] Wired BannerSlide.prefab to EventBannerCarousel._slidePrefab");
                }
                if (runtimePrefabConfig.BannerIndicatorPrefab != null)
                {
                    carouselSO.FindProperty("_indicatorPrefab").objectReferenceValue = runtimePrefabConfig.BannerIndicatorPrefab;
                    Debug.Log("[ProductionSceneSetupGenerator] Wired BannerIndicator.prefab to EventBannerCarousel._indicatorPrefab");
                }
            }

            carouselSO.ApplyModifiedPropertiesWithoutUndo();

            Debug.Log("[ProductionSceneSetupGenerator] Created EventBannerCarousel");

            return carouselObj;
        }

        /// <summary>
        /// Creates a wide navigation button (horizontal, single-line title, no subtitle).
        /// Matches the reference design from BastionSceneDesignReference.jpg
        /// </summary>
        /// <summary>
        /// Creates a wide navigation button using LayerLab TabMenu style.
        /// Falls back to simple styled button if LayerLab config not available.
        /// </summary>
        private static GameObject CreateWideNavButton(GameObject parent, string name, string title)
        {
            var layerLabConfig = LoadLayerLabConfig();

            // Use LayerLab tab button if config is available
            if (layerLabConfig != null && layerLabConfig.HasAllTabMenuSprites())
            {
                return LayerLabTabMenuBuilder.CreateWideNavButton(parent, name, title);
            }

            // Fallback to simple styled button
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

        /// <summary>
        /// Creates an icon-only button using LayerLab Convex Rectangle style.
        /// Used for settings buttons in screens that use the old CreateMenuButton pattern.
        /// </summary>
        private static GameObject CreateLayerLabIconButton(GameObject parent, string name, Sprite icon = null)
        {
            var layerLabConfig = LoadLayerLabConfig();

            // Use LayerLab convex button if config is available
            if (layerLabConfig != null && layerLabConfig.HasAllConvexButtonSprites())
            {
                var buttonObj = LayerLabButtonBuilder.CreateConvexRectangleButton(parent, name, icon);
                // Scale to larger size for better mobile tappability
                RectTransform rect = buttonObj.GetComponent<RectTransform>();
                rect.sizeDelta = new Vector2(80, 80);
                return buttonObj;
            }

            // Fallback to simple icon button
            GameObject obj = new GameObject(name);
            obj.transform.SetParent(parent.transform, false);

            RectTransform rect2 = obj.AddComponent<RectTransform>();
            rect2.sizeDelta = new Vector2(80, 80);

            Image img = obj.AddComponent<Image>();
            img.color = new Color(0.25f, 0.25f, 0.3f);

            Button btn = obj.AddComponent<Button>();
            btn.targetGraphic = img;

            // Icon
            GameObject iconObj = new GameObject("Icon");
            iconObj.transform.SetParent(obj.transform, false);
            RectTransform iconRect = iconObj.AddComponent<RectTransform>();
            iconRect.anchorMin = new Vector2(0.2f, 0.2f);
            iconRect.anchorMax = new Vector2(0.8f, 0.8f);
            iconRect.sizeDelta = Vector2.zero;

            Image iconImage = iconObj.AddComponent<Image>();
            if (icon != null)
            {
                iconImage.sprite = icon;
                iconImage.preserveAspect = true;
            }
            else
            {
                iconImage.color = new Color(0.6f, 0.6f, 0.7f, 0.8f);
            }
            iconImage.raycastTarget = false;

            return obj;
        }

        /// <summary>
        /// Creates a back button using LayerLab Convex LeftFlush style.
        /// </summary>
        private static GameObject CreateBackButton(GameObject parent, string name)
        {
            var layerLabConfig = LoadLayerLabConfig();

            // Use LayerLab convex left flush button if config is available
            if (layerLabConfig != null && layerLabConfig.HasAllConvexButtonSprites())
            {
                var buttonObj = LayerLabButtonBuilder.CreateConvexLeftFlushButton(parent, name, layerLabConfig.IconBack);
                // Scale to larger size for better mobile tappability
                RectTransform rect = buttonObj.GetComponent<RectTransform>();
                rect.sizeDelta = new Vector2(100, 80);
                return buttonObj;
            }

            // Fallback to simple back button
            GameObject obj = new GameObject(name);
            obj.transform.SetParent(parent.transform, false);

            RectTransform rect2 = obj.AddComponent<RectTransform>();
            rect2.sizeDelta = new Vector2(100, 80);

            Image img = obj.AddComponent<Image>();
            img.color = new Color(0.25f, 0.25f, 0.3f);

            Button btn = obj.AddComponent<Button>();
            btn.targetGraphic = img;

            // Arrow text fallback
            GameObject textObj = CreateText(obj, "Text", "<", 32);
            RectTransform textRect = textObj.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.sizeDelta = Vector2.zero;

            return obj;
        }

        /// <summary>
        /// Creates a large button with title and subtitle using LayerLab StageFrame style.
        /// Used for mission buttons in Missions scene.
        /// </summary>
        private static GameObject CreateLayerLabLargeButton(string name, Transform parent, string title, string subtitle)
        {
            var layerLabConfig = LoadLayerLabConfig();

            // Use LayerLab StageFrame style if config is available
            if (layerLabConfig != null && layerLabConfig.HasAllStageFrameSprites())
            {
                var buttonObj = LayerLabStageFrameBuilder.CreateStyledStageFrame(parent.gameObject, name, title, subtitle, true);
                return buttonObj;
            }

            // Fallback to simple button
            return CreateLargeButtonWithSubtitle(name, parent, title, subtitle);
        }

        /// <summary>
        /// Creates a card-style mission button with icon, title, and subtitle.
        /// Size: 280x420 (taller, more card-like).
        /// </summary>
        private static GameObject CreateMissionCardButton(Transform parent, string name, string title, string subtitle, bool isEnabled, string iconType)
        {
            var layerLabConfig = LoadLayerLabConfig();

            GameObject buttonObj = new GameObject(name);
            buttonObj.transform.SetParent(parent, false);

            RectTransform rect = buttonObj.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(280, 420);

            // Background image - use CardFrame if available, otherwise solid color
            Image bgImage = buttonObj.AddComponent<Image>();
            Sprite cardBg = null;

            // Determine card color based on type
            if (layerLabConfig != null)
            {
                if (iconType == "book")
                    cardBg = layerLabConfig.CardFrameRectanglePurpleBg;
                else if (iconType == "battle")
                    cardBg = layerLabConfig.CardFrameRectangleGreenBg;
            }

            if (cardBg != null)
            {
                bgImage.sprite = cardBg;
                bgImage.type = Image.Type.Sliced;
                bgImage.color = Color.white;
            }
            else
            {
                // Fallback color
                bgImage.color = iconType == "book"
                    ? new Color(0.4f, 0.2f, 0.5f, 0.95f)  // Purple for Story
                    : new Color(0.2f, 0.5f, 0.3f, 0.95f); // Green for Battle
            }

            // Add button component
            Button button = buttonObj.AddComponent<Button>();
            button.targetGraphic = bgImage;
            button.interactable = isEnabled;

            // Icon area (top 35%)
            GameObject iconArea = new GameObject("IconArea");
            iconArea.transform.SetParent(buttonObj.transform, false);
            RectTransform iconAreaRect = iconArea.AddComponent<RectTransform>();
            iconAreaRect.anchorMin = new Vector2(0.1f, 0.65f);
            iconAreaRect.anchorMax = new Vector2(0.9f, 0.95f);
            iconAreaRect.sizeDelta = Vector2.zero;

            // Icon image
            Image iconImage = iconArea.AddComponent<Image>();
            iconImage.preserveAspect = true;
            iconImage.raycastTarget = false;

            // Try to load icon sprite
            Sprite iconSprite = null;
            if (layerLabConfig != null)
            {
                iconSprite = iconType == "book" ? layerLabConfig.PictoIconBook : layerLabConfig.PictoIconBattle;
            }

            if (iconSprite != null)
            {
                iconImage.sprite = iconSprite;
                iconImage.color = Color.white;
            }
            else
            {
                // Fallback to colored placeholder
                iconImage.color = new Color(1f, 1f, 1f, 0.5f);
            }

            // Title area (middle 30%)
            GameObject titleObj = CreateText(buttonObj, "Title", title, 32);
            RectTransform titleRect = titleObj.GetComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0, 0.35f);
            titleRect.anchorMax = new Vector2(1, 0.60f);
            titleRect.offsetMin = new Vector2(15, 0);
            titleRect.offsetMax = new Vector2(-15, 0);
            var titleTmp = titleObj.GetComponent<TMP_Text>();
            titleTmp.alignment = TextAlignmentOptions.Center;
            titleTmp.fontStyle = FontStyles.Bold;
            titleTmp.color = Color.white;

            // Subtitle area (bottom 25%)
            GameObject subtitleObj = CreateText(buttonObj, "Subtitle", subtitle, 20);
            RectTransform subtitleRect = subtitleObj.GetComponent<RectTransform>();
            subtitleRect.anchorMin = new Vector2(0, 0.12f);
            subtitleRect.anchorMax = new Vector2(1, 0.32f);
            subtitleRect.offsetMin = new Vector2(15, 0);
            subtitleRect.offsetMax = new Vector2(-15, 0);
            var subtitleTmp = subtitleObj.GetComponent<TMP_Text>();
            subtitleTmp.alignment = TextAlignmentOptions.Center;
            subtitleTmp.color = new Color(0.8f, 0.8f, 0.8f);

            // "Coming Soon" overlay for disabled buttons
            if (!isEnabled)
            {
                GameObject overlay = new GameObject("DisabledOverlay");
                overlay.transform.SetParent(buttonObj.transform, false);
                RectTransform overlayRect = overlay.AddComponent<RectTransform>();
                overlayRect.anchorMin = Vector2.zero;
                overlayRect.anchorMax = Vector2.one;
                overlayRect.sizeDelta = Vector2.zero;

                Image overlayImage = overlay.AddComponent<Image>();
                overlayImage.color = new Color(0, 0, 0, 0.6f);
                overlayImage.raycastTarget = false;

                // Lock icon in overlay
                GameObject lockIcon = new GameObject("LockIcon");
                lockIcon.transform.SetParent(overlay.transform, false);
                RectTransform lockRect = lockIcon.AddComponent<RectTransform>();
                lockRect.anchorMin = new Vector2(0.5f, 0.5f);
                lockRect.anchorMax = new Vector2(0.5f, 0.5f);
                lockRect.sizeDelta = new Vector2(64, 64);

                Image lockImage = lockIcon.AddComponent<Image>();
                lockImage.raycastTarget = false;

                if (layerLabConfig?.PictoIconLock != null)
                {
                    lockImage.sprite = layerLabConfig.PictoIconLock;
                    lockImage.color = new Color(1f, 1f, 1f, 0.8f);
                }
                else
                {
                    lockImage.color = new Color(0.6f, 0.6f, 0.6f, 0.8f);
                }
            }

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

            // === Event Panel (expanded height for better button fit) ===
            GameObject panel = new GameObject("EventPanel");
            panel.transform.SetParent(screenObj.transform, false);
            RectTransform panelRect = panel.AddComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.15f, 0.20f);
            panelRect.anchorMax = new Vector2(0.85f, 0.88f);
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

            // No Image component on IllustrationPanel - only the child IllustrationIcon has an image

            // Illustration icon - fills entire panel area
            GameObject illustIcon = new GameObject("IllustrationIcon");
            illustIcon.transform.SetParent(illustrationPanel.transform, false);
            RectTransform iconRect = illustIcon.AddComponent<RectTransform>();
            iconRect.anchorMin = Vector2.zero;
            iconRect.anchorMax = Vector2.one;
            iconRect.sizeDelta = Vector2.zero;
            Image bgImage = illustIcon.AddComponent<Image>();
            bgImage.color = Color.white; // White to properly display illustrations (not violet tint)
            bgImage.preserveAspect = true;

            // === Content Panel (Right side) ===
            GameObject contentPanel = new GameObject("ContentPanel");
            contentPanel.transform.SetParent(panel.transform, false);
            RectTransform contentRect = contentPanel.AddComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0, 0);
            contentRect.anchorMax = new Vector2(1, 1);
            contentRect.offsetMin = new Vector2(240, 20); // Leave space for illustration
            contentRect.offsetMax = new Vector2(-20, -20);

            // === Event Title (Soul Gold per mockup) - larger font ===
            GameObject titleObj = CreateText(contentPanel, "EventTitle", "ECHO OF THE VOID", 36);
            RectTransform titleRect = titleObj.GetComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0, 0.85f);
            titleRect.anchorMax = new Vector2(1, 0.98f);
            titleRect.sizeDelta = Vector2.zero;
            var titleTmp = titleObj.GetComponent<TextMeshProUGUI>();
            titleTmp.color = new Color(0.83f, 0.69f, 0.22f); // Soul Gold #D4AF37
            titleTmp.fontStyle = TMPro.FontStyles.Bold;
            titleTmp.characterSpacing = 4f; // Letter spacing per mockup
            titleTmp.alignment = TextAlignmentOptions.Left;

            // === Event Description / Narrative - larger font for readability ===
            GameObject descObj = CreateText(contentPanel, "NarrativeText", "Event narrative text goes here...", 26);
            RectTransform descRect = descObj.GetComponent<RectTransform>();
            descRect.anchorMin = new Vector2(0, 0.58f);
            descRect.anchorMax = new Vector2(1, 0.82f);
            descRect.sizeDelta = Vector2.zero;
            var narrativeTmp = descObj.GetComponent<TextMeshProUGUI>();
            narrativeTmp.alignment = TextAlignmentOptions.TopLeft;
            narrativeTmp.color = new Color(0.85f, 0.85f, 0.85f);

            // === Choice Container - expanded area for buttons ===
            GameObject choiceContainer = new GameObject("ChoiceContainer");
            choiceContainer.transform.SetParent(contentPanel.transform, false);
            RectTransform choiceRect = choiceContainer.AddComponent<RectTransform>();
            choiceRect.anchorMin = new Vector2(0, 0.05f);
            choiceRect.anchorMax = new Vector2(1, 0.60f);
            choiceRect.sizeDelta = Vector2.zero;

            VerticalLayoutGroup choiceLayout = choiceContainer.AddComponent<VerticalLayoutGroup>();
            choiceLayout.spacing = 12f;
            choiceLayout.childAlignment = TextAnchor.UpperLeft;
            choiceLayout.childForceExpandWidth = true;
            choiceLayout.childForceExpandHeight = false;
            choiceLayout.padding = new RectOffset(0, 0, 8, 8);

            // === Outcome Panel (hidden by default) ===
            GameObject outcomePanel = new GameObject("OutcomePanel");
            outcomePanel.transform.SetParent(panel.transform, false);
            RectTransform outcomeRect = outcomePanel.AddComponent<RectTransform>();
            outcomeRect.anchorMin = new Vector2(0.1f, 0.1f);
            outcomeRect.anchorMax = new Vector2(0.9f, 0.9f);
            outcomeRect.sizeDelta = Vector2.zero;

            Image outcomeBg = outcomePanel.AddComponent<Image>();
            outcomeBg.color = new Color(0.08f, 0.06f, 0.12f, 0.98f);

            // OutcomeText - positioned at top of outcome panel with larger font
            GameObject outcomeText = CreateText(outcomePanel, "OutcomeText", "Outcome text...", 28);
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

            // Continue button in outcome panel - positioned at bottom (LayerLab green style)
            var echoLayerLabConfig = LoadLayerLabConfig();
            GameObject continueBtn;
            if (echoLayerLabConfig != null && echoLayerLabConfig.HasAllButton01SmallSprites())
            {
                continueBtn = LayerLabButtonBuilder.CreateButton01Small(outcomePanel, "ContinueButton", "CONTINUE", "green");
            }
            else
            {
                continueBtn = new GameObject("ContinueButton");
                continueBtn.transform.SetParent(outcomePanel.transform, false);
                Image continueBtnImg = continueBtn.AddComponent<Image>();
                continueBtnImg.color = new Color(0.18f, 0.8f, 0.44f);
                Button continueBtnComp = continueBtn.AddComponent<Button>();
                continueBtnComp.targetGraphic = continueBtnImg;
                GameObject continueBtnText = CreateText(continueBtn, "Text", "CONTINUE", 16);
                RectTransform continueBtnTextRect = continueBtnText.GetComponent<RectTransform>();
                continueBtnTextRect.anchorMin = Vector2.zero;
                continueBtnTextRect.anchorMax = Vector2.one;
                continueBtnTextRect.sizeDelta = Vector2.zero;
            }
            RectTransform continueBtnRect = continueBtn.GetComponent<RectTransform>();
            continueBtnRect.anchorMin = new Vector2(0.5f, 0.08f);
            continueBtnRect.anchorMax = new Vector2(0.5f, 0.08f);
            continueBtnRect.pivot = new Vector2(0.5f, 0.5f);
            continueBtnRect.sizeDelta = new Vector2(180, 70);
            Button continueBtnComponent = continueBtn.GetComponent<Button>();

            outcomePanel.SetActive(false);

            // === Skip Button (for empty events or when no choices) - LayerLab purple style ===
            GameObject skipBtn;
            if (echoLayerLabConfig != null && echoLayerLabConfig.HasAllButton01SmallSprites())
            {
                skipBtn = LayerLabButtonBuilder.CreateButton01Small(panel, "SkipButton", "CONTINUE", "purple");
            }
            else
            {
                skipBtn = new GameObject("SkipButton");
                skipBtn.transform.SetParent(panel.transform, false);
                Image skipBtnImg = skipBtn.AddComponent<Image>();
                skipBtnImg.color = new Color(0.4f, 0.35f, 0.5f);
                Button skipBtnComp = skipBtn.AddComponent<Button>();
                skipBtnComp.targetGraphic = skipBtnImg;
                GameObject skipBtnText = CreateText(skipBtn, "Text", "CONTINUE", 16);
                RectTransform skipBtnTextRect = skipBtnText.GetComponent<RectTransform>();
                skipBtnTextRect.anchorMin = Vector2.zero;
                skipBtnTextRect.anchorMax = Vector2.one;
                skipBtnTextRect.sizeDelta = Vector2.zero;
            }
            RectTransform skipBtnRect = skipBtn.GetComponent<RectTransform>();
            skipBtnRect.anchorMin = new Vector2(0.5f, 0.08f);
            skipBtnRect.anchorMax = new Vector2(0.5f, 0.08f);
            skipBtnRect.pivot = new Vector2(0.5f, 0.5f);
            skipBtnRect.sizeDelta = new Vector2(180, 70);
            Button skipBtnComponent = skipBtn.GetComponent<Button>();

            skipBtn.SetActive(false); // Hidden by default, shown when no choices

            // === Wire EchoEventScreen references ===
            var echoScreen = screenObj.GetComponent<EchoEventScreen>();
            if (echoScreen != null)
            {
                SerializedObject so = new SerializedObject(echoScreen);
                so.FindProperty("_titleText").objectReferenceValue = titleTmp;
                so.FindProperty("_narrativeText").objectReferenceValue = narrativeTmp;
                so.FindProperty("_backgroundImage").objectReferenceValue = bgImage;
                so.FindProperty("_choiceContainer").objectReferenceValue = choiceContainer.transform;

                // Wire EchoChoiceButton prefab asset instead of inline template
                var echoChoiceButtonPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(
                    "Assets/_Project/Prefabs/UI/NullRift/EchoChoiceButton.prefab");
                if (echoChoiceButtonPrefab != null)
                {
                    so.FindProperty("_choiceButtonPrefab").objectReferenceValue = echoChoiceButtonPrefab;
                    Debug.Log("[ProductionSceneSetupGenerator] Wired EchoChoiceButton.prefab to EchoEventScreen");
                }
                else
                {
                    Debug.LogWarning("[ProductionSceneSetupGenerator] EchoChoiceButton.prefab not found - run HNR > 2. Prefabs > UI > NullRift > EchoChoiceButton Prefab first");
                }

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
        /// Creates a choice button template for EchoEventScreen using LayerLab styling.
        /// Features two text areas: MainText (cyan, upper) for choice description and ResultText (black, lower) for cost/result.
        /// </summary>
        private static GameObject CreateEchoChoiceButton(GameObject parent)
        {
            var layerLabConfig = LoadLayerLabConfig();

            GameObject buttonObj;
            if (layerLabConfig != null && layerLabConfig.HasAllButton01SmallSprites())
            {
                // Use LayerLab purple button for choices
                buttonObj = LayerLabButtonBuilder.CreateButton01Small(parent, "ChoiceButtonTemplate", "", "purple");

                // Adjust size for choice layout - reduced height to fit better
                var rect = buttonObj.GetComponent<RectTransform>();
                rect.sizeDelta = new Vector2(500, 85);

                // Add layout element for vertical layout
                var layoutElement = buttonObj.AddComponent<LayoutElement>();
                layoutElement.preferredHeight = 85;
                layoutElement.minWidth = 400;
                layoutElement.flexibleWidth = 1;

                // Rename existing text to MainText and reposition for upper portion (50-92% Y)
                var existingText = buttonObj.transform.Find("Text");
                if (existingText != null)
                {
                    existingText.name = "Text"; // Keep as "Text" for EchoEventScreen compatibility
                    var mainTmp = existingText.GetComponent<TextMeshProUGUI>();
                    if (mainTmp != null)
                    {
                        mainTmp.alignment = TextAlignmentOptions.Left;
                        mainTmp.text = "Choice Text";
                        mainTmp.fontSize = 22;
                        mainTmp.fontSizeMax = 22;
                        mainTmp.fontSizeMin = 16;
                        mainTmp.color = new Color(0f, 0.95f, 1f); // Bright cyan for main choice text
                    }
                    var mainTextRect = existingText.GetComponent<RectTransform>();
                    mainTextRect.anchorMin = new Vector2(0, 0.50f);
                    mainTextRect.anchorMax = new Vector2(1, 0.92f);
                    mainTextRect.offsetMin = new Vector2(25, 0);
                    mainTextRect.offsetMax = new Vector2(-25, 0);
                }

                // Create ResultText for lower portion (10-48% Y) - same font size as main for consistency
                GameObject resultTextObj = CreateText(buttonObj, "ResultText", "", 22);
                RectTransform resultRect = resultTextObj.GetComponent<RectTransform>();
                resultRect.anchorMin = new Vector2(0, 0.10f);
                resultRect.anchorMax = new Vector2(1, 0.48f);
                resultRect.offsetMin = new Vector2(25, 0);
                resultRect.offsetMax = new Vector2(-25, 0);
                var resultTmp = resultTextObj.GetComponent<TextMeshProUGUI>();
                resultTmp.alignment = TextAlignmentOptions.Left;
                resultTmp.color = new Color(0.1f, 0.1f, 0.1f); // Dark/black for result text
                resultTmp.fontStyle = FontStyles.Italic;
                resultTmp.fontSize = 22; // Same as main text
                resultTmp.fontSizeMax = 22;
                resultTmp.fontSizeMin = 14;
            }
            else
            {
                // Fallback to simple button with two text areas
                buttonObj = new GameObject("ChoiceButtonTemplate");
                buttonObj.transform.SetParent(parent.transform, false);

                RectTransform rect = buttonObj.AddComponent<RectTransform>();
                rect.sizeDelta = new Vector2(500, 85);

                Image bg = buttonObj.AddComponent<Image>();
                bg.color = new Color(0.15f, 0.12f, 0.2f, 0.9f);

                Button button = buttonObj.AddComponent<Button>();
                button.targetGraphic = bg;

                var layoutElement = buttonObj.AddComponent<LayoutElement>();
                layoutElement.preferredHeight = 85;
                layoutElement.minWidth = 400;
                layoutElement.flexibleWidth = 1;

                // MainText - upper portion (50-92% Y), cyan color, font size 22
                GameObject mainTextObj = CreateText(buttonObj, "Text", "Choice Text", 22);
                RectTransform mainTextRect = mainTextObj.GetComponent<RectTransform>();
                mainTextRect.anchorMin = new Vector2(0, 0.50f);
                mainTextRect.anchorMax = new Vector2(1, 0.92f);
                mainTextRect.offsetMin = new Vector2(20, 0);
                mainTextRect.offsetMax = new Vector2(-20, 0);
                var mainTmp = mainTextObj.GetComponent<TextMeshProUGUI>();
                mainTmp.alignment = TextAlignmentOptions.Left;
                mainTmp.color = new Color(0f, 0.95f, 1f); // Bright cyan

                // ResultText - lower portion (10-48% Y), font size 22 to match main
                GameObject resultTextObj = CreateText(buttonObj, "ResultText", "", 22);
                RectTransform resultTextRect = resultTextObj.GetComponent<RectTransform>();
                resultTextRect.anchorMin = new Vector2(0, 0.10f);
                resultTextRect.anchorMax = new Vector2(1, 0.48f);
                resultTextRect.offsetMin = new Vector2(20, 0);
                resultTextRect.offsetMax = new Vector2(-20, 0);
                var resultTmp = resultTextObj.GetComponent<TextMeshProUGUI>();
                resultTmp.alignment = TextAlignmentOptions.Left;
                resultTmp.color = new Color(0.1f, 0.1f, 0.1f); // Dark/black
                resultTmp.fontStyle = FontStyles.Italic;
            }

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

            // Load background from config, fallback to color if not available
            var bgConfig = LoadBackgroundConfig();
            if (bgConfig?.ShopBackground != null)
            {
                bg.sprite = bgConfig.ShopBackground;
                bg.type = Image.Type.Sliced;
                bg.color = Color.white;
                Debug.Log("[ProductionSceneSetupGenerator] Assigned Shop background sprite from BackgroundConfig");
            }
            else
            {
                bg.color = bgConfig?.ShopFallbackColor ?? new Color(0.08f, 0.05f, 0.12f, 0.98f);
                Debug.Log("[ProductionSceneSetupGenerator] Using fallback color for Shop background");
            }

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

            // === Services Panel (Right side) with LayerLab styling ===
            var shopLayerLabConfig = LoadLayerLabConfig();

            GameObject servicesPanel = new GameObject("ServicesPanel");
            servicesPanel.transform.SetParent(screenObj.transform, false);
            RectTransform servicesRect = servicesPanel.AddComponent<RectTransform>();
            servicesRect.anchorMin = new Vector2(0.72f, 0.18f);
            servicesRect.anchorMax = new Vector2(0.95f, 0.85f);
            servicesRect.sizeDelta = Vector2.zero;

            // Services panel background with LayerLab frame if available
            Image servicesBg = servicesPanel.AddComponent<Image>();
            if (shopLayerLabConfig != null && shopLayerLabConfig.TabMenuBg != null)
            {
                servicesBg.sprite = shopLayerLabConfig.TabMenuBg;
                servicesBg.type = Image.Type.Sliced;
                servicesBg.color = new Color(0.2f, 0.15f, 0.3f, 0.92f); // Purple tint

                // Add border if available
                if (shopLayerLabConfig.TabMenuBorder != null)
                {
                    GameObject servicesBorder = new GameObject("Border");
                    servicesBorder.transform.SetParent(servicesPanel.transform, false);
                    RectTransform borderRect = servicesBorder.AddComponent<RectTransform>();
                    borderRect.anchorMin = Vector2.zero;
                    borderRect.anchorMax = Vector2.one;
                    borderRect.sizeDelta = Vector2.zero;
                    Image borderImg = servicesBorder.AddComponent<Image>();
                    borderImg.sprite = shopLayerLabConfig.TabMenuBorder;
                    borderImg.type = Image.Type.Sliced;
                    borderImg.color = new Color(0.6f, 0.45f, 0.8f, 0.85f);
                    borderImg.raycastTarget = false;
                }
            }
            else
            {
                servicesBg.color = new Color(0.1f, 0.08f, 0.12f, 0.9f);
            }

            VerticalLayoutGroup servicesLayout = servicesPanel.AddComponent<VerticalLayoutGroup>();
            servicesLayout.padding = new RectOffset(15, 15, 15, 15);
            servicesLayout.spacing = 15;
            servicesLayout.childAlignment = TextAnchor.UpperCenter;
            servicesLayout.childForceExpandWidth = true;
            servicesLayout.childForceExpandHeight = false;

            // Services title - larger font (32 instead of 24)
            GameObject servicesTitleObj = CreateText(servicesPanel, "ServicesTitle", "SERVICES", 32);
            servicesTitleObj.GetComponent<TMP_Text>().fontStyle = FontStyles.Bold;
            servicesTitleObj.GetComponent<TMP_Text>().color = new Color(0.83f, 0.69f, 0.22f); // Soul gold
            var servTitleLayout = servicesTitleObj.AddComponent<LayoutElement>();
            servTitleLayout.preferredHeight = 50; // Increased from 40

            // Remove Card Button
            GameObject removeCardBtn = CreateShopServiceButton(servicesPanel, "RemoveCardButton", "Remove Card", "75 Shards");
            var removeCardButton = removeCardBtn.GetComponent<Button>();

            // Purify Button
            GameObject purifyBtn = CreateShopServiceButton(servicesPanel, "PurifyButton", "Purify", "50 Shards");
            var purifyButton = purifyBtn.GetComponent<Button>();

            // Buy Relic Button
            GameObject buyRelicBtn = CreateShopServiceButton(servicesPanel, "BuyRelicButton", "Buy Relic", "View Relics");
            var buyRelicButton = buyRelicBtn.GetComponent<Button>();

            // === Leave Button (larger for better accessibility) ===
            GameObject leaveBtn = CreateMenuButton(screenObj, "LeaveButton", "LEAVE SHOP");
            RectTransform leaveRect = leaveBtn.GetComponent<RectTransform>();
            leaveRect.anchorMin = new Vector2(0.5f, 0.05f);
            leaveRect.anchorMax = new Vector2(0.5f, 0.05f);
            leaveRect.sizeDelta = new Vector2(260, 60); // Increased from (200, 50)
            var leaveButton = leaveBtn.GetComponent<Button>();

            // === Wire ShopScreen references ===
            SerializedObject so = new SerializedObject(shopScreen);
            so.FindProperty("_voidShardsText").objectReferenceValue = currencyText;
            so.FindProperty("_itemContainer").objectReferenceValue = itemsContainer.transform;
            so.FindProperty("_leaveButton").objectReferenceValue = leaveButton;
            so.FindProperty("_removeCardButton").objectReferenceValue = removeCardButton;
            so.FindProperty("_purifyButton").objectReferenceValue = purifyButton;
            so.FindProperty("_buyRelicButton").objectReferenceValue = buyRelicButton;
            so.ApplyModifiedPropertiesWithoutUndo();

            Debug.Log("[ProductionSceneSetupGenerator] Created ShopScreen with wired references");
            return screenObj;
        }

        /// <summary>
        /// Creates a service button for the shop.
        /// </summary>
        private static GameObject CreateShopServiceButton(GameObject parent, string name, string label, string cost)
        {
            var layerLabConfig = LoadLayerLabConfig();

            GameObject btnObj;
            if (layerLabConfig != null && layerLabConfig.HasAllButton01SmallSprites())
            {
                // Use LayerLab purple button for shop services
                btnObj = LayerLabButtonBuilder.CreateButton01Small(parent, name, label, "purple");

                // Adjust button size for better proportions
                var btnRect = btnObj.GetComponent<RectTransform>();
                btnRect.sizeDelta = new Vector2(btnRect.sizeDelta.x, 90);

                // Add layout element for vertical layout
                var layoutElement = btnObj.AddComponent<LayoutElement>();
                layoutElement.preferredHeight = 90;

                // Adjust main text for centered upper portion (55-85% Y) - both texts centered together
                var textObj = btnObj.transform.Find("Text");
                if (textObj != null)
                {
                    var textRect = textObj.GetComponent<RectTransform>();
                    textRect.anchorMin = new Vector2(0, 0.55f);
                    textRect.anchorMax = new Vector2(1, 0.85f);
                    textRect.sizeDelta = Vector2.zero;
                    var mainTmp = textObj.GetComponent<TMP_Text>();
                    if (mainTmp != null)
                    {
                        mainTmp.fontSize = 22;
                        mainTmp.fontSizeMax = 22;
                        mainTmp.fontSizeMin = 16;
                        mainTmp.color = Color.white; // White for better contrast
                        mainTmp.fontStyle = FontStyles.Bold;
                    }
                }

                // Create cost text below main label - centered lower (20-50% Y), closer to label
                GameObject costObj = CreateText(btnObj, "Cost", cost, 16);
                RectTransform costRect = costObj.GetComponent<RectTransform>();
                costRect.anchorMin = new Vector2(0, 0.20f);
                costRect.anchorMax = new Vector2(1, 0.50f);
                costRect.sizeDelta = Vector2.zero;
                var costTmp = costObj.GetComponent<TMP_Text>();
                costTmp.color = new Color(0f, 0.9f, 1f); // Bright cyan for visibility
                costTmp.alignment = TextAlignmentOptions.Center;
            }
            else
            {
                // Fallback to simple button
                btnObj = new GameObject(name);
                btnObj.transform.SetParent(parent.transform, false);

                Image bg = btnObj.AddComponent<Image>();
                bg.color = new Color(0.15f, 0.12f, 0.2f, 0.9f);

                Button button = btnObj.AddComponent<Button>();
                button.targetGraphic = bg;

                var layoutElement = btnObj.AddComponent<LayoutElement>();
                layoutElement.preferredHeight = 90;

                VerticalLayoutGroup layout = btnObj.AddComponent<VerticalLayoutGroup>();
                layout.padding = new RectOffset(10, 10, 15, 15);
                layout.spacing = 4;
                layout.childAlignment = TextAnchor.MiddleCenter;
                layout.childForceExpandWidth = true;
                layout.childForceExpandHeight = false;

                GameObject labelObj = CreateText(btnObj, "Label", label, 20);
                labelObj.GetComponent<TMP_Text>().fontStyle = FontStyles.Bold;
                labelObj.GetComponent<TMP_Text>().color = Color.white;

                GameObject costObj = CreateText(btnObj, "Cost", cost, 16);
                costObj.GetComponent<TMP_Text>().color = new Color(0f, 0.9f, 1f); // Bright cyan
            }

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

            // NO opaque background Image - world-space background shows through transparent UI
            // Background is handled by SanctuaryWorldBackground SpriteRenderer

            // Note: No RequiemVisualsDisplay RawImage needed - Requiems render in world-space
            // like Combat scene, and show through the transparent UI canvas

            // Legacy visual anchors container (not used, but kept for SanctuaryScreen field compatibility)
            GameObject visualAnchorsContainer = new GameObject("VisualAnchors_Legacy");
            visualAnchorsContainer.transform.SetParent(screenObj.transform, false);
            RectTransform visualAnchorsRect = visualAnchorsContainer.AddComponent<RectTransform>();
            visualAnchorsRect.anchorMin = new Vector2(0.1f, 0.15f);
            visualAnchorsRect.anchorMax = new Vector2(0.9f, 0.55f);
            visualAnchorsRect.sizeDelta = Vector2.zero;
            visualAnchorsContainer.SetActive(false); // Hidden - legacy

            // Left visual anchor (legacy)
            GameObject leftAnchor = new GameObject("LeftVisualAnchor");
            leftAnchor.transform.SetParent(visualAnchorsContainer.transform, false);
            RectTransform leftRect = leftAnchor.AddComponent<RectTransform>();
            leftRect.anchorMin = new Vector2(0.15f, 0.2f);
            leftRect.anchorMax = new Vector2(0.15f, 0.2f);
            leftRect.sizeDelta = new Vector2(150, 200);

            // Center/Back visual anchor (legacy)
            GameObject centerAnchor = new GameObject("CenterVisualAnchor");
            centerAnchor.transform.SetParent(visualAnchorsContainer.transform, false);
            RectTransform centerRect = centerAnchor.AddComponent<RectTransform>();
            centerRect.anchorMin = new Vector2(0.5f, 0.4f);
            centerRect.anchorMax = new Vector2(0.5f, 0.4f);
            centerRect.sizeDelta = new Vector2(150, 200);

            // Right visual anchor
            GameObject rightAnchor = new GameObject("RightVisualAnchor");
            rightAnchor.transform.SetParent(visualAnchorsContainer.transform, false);
            RectTransform rightRect = rightAnchor.AddComponent<RectTransform>();
            rightRect.anchorMin = new Vector2(0.85f, 0.2f);
            rightRect.anchorMax = new Vector2(0.85f, 0.2f);
            rightRect.sizeDelta = new Vector2(150, 200);

            // === Merged Text Background Container - single 70% black background for both title and description ===
            GameObject textBackgroundContainer = new GameObject("TextBackgroundContainer");
            textBackgroundContainer.transform.SetParent(screenObj.transform, false);
            RectTransform textBgContainerRect = textBackgroundContainer.AddComponent<RectTransform>();
            // Anchor-stretch to cover both title and description area
            textBgContainerRect.anchorMin = new Vector2(0.1f, 0.66f);
            textBgContainerRect.anchorMax = new Vector2(0.9f, 0.90f);
            textBgContainerRect.sizeDelta = Vector2.zero;

            // Single merged background (70% black)
            Image textBgImage = textBackgroundContainer.AddComponent<Image>();
            textBgImage.color = new Color(0f, 0f, 0f, 0.7f);
            textBgImage.raycastTarget = false;

            // === Title Container (inside merged background) ===
            GameObject titleContainer = new GameObject("TitleContainer");
            titleContainer.transform.SetParent(textBackgroundContainer.transform, false);
            RectTransform titleContainerRect = titleContainer.AddComponent<RectTransform>();
            // Position in upper portion of merged container
            titleContainerRect.anchorMin = new Vector2(0, 0.50f);
            titleContainerRect.anchorMax = new Vector2(1, 1f);
            titleContainerRect.sizeDelta = Vector2.zero;
            // No background on titleContainer - uses parent's merged background

            // Title text - larger font with better contrast
            GameObject titleObj = CreateText(titleContainer, "Title", "SANCTUARY", 48);
            RectTransform titleRect = titleObj.GetComponent<RectTransform>();
            titleRect.anchorMin = Vector2.zero;
            titleRect.anchorMax = Vector2.one;
            titleRect.sizeDelta = Vector2.zero;
            titleRect.offsetMin = new Vector2(15, 8);
            titleRect.offsetMax = new Vector2(-15, -8);
            var titleText = titleObj.GetComponent<TMP_Text>();
            titleText.color = new Color(0.3f, 0.95f, 0.55f); // Brighter green for better contrast
            titleText.fontStyle = FontStyles.Bold;

            // === Description Container (inside merged background) ===
            GameObject descContainer = new GameObject("DescriptionContainer");
            descContainer.transform.SetParent(textBackgroundContainer.transform, false);
            RectTransform descContainerRect = descContainer.AddComponent<RectTransform>();
            // Position in lower portion of merged container
            descContainerRect.anchorMin = new Vector2(0, 0f);
            descContainerRect.anchorMax = new Vector2(1, 0.50f);
            descContainerRect.sizeDelta = Vector2.zero;
            // No background on descContainer - uses parent's merged background

            // Description text - larger font with better contrast
            GameObject descObj = CreateText(descContainer, "Description", "A moment of respite in the Null Rift...", 28);
            RectTransform descRect = descObj.GetComponent<RectTransform>();
            descRect.anchorMin = Vector2.zero;
            descRect.anchorMax = Vector2.one;
            descRect.sizeDelta = Vector2.zero;
            descRect.offsetMin = new Vector2(20, 10);
            descRect.offsetMax = new Vector2(-20, -10);
            var descText = descObj.GetComponent<TMP_Text>();
            descText.color = new Color(0.9f, 0.9f, 0.9f); // Brighter for better contrast

            // Choice buttons container - centered horizontally
            GameObject choicesContainer = new GameObject("ChoicesContainer");
            choicesContainer.transform.SetParent(screenObj.transform, false);
            RectTransform choicesRect = choicesContainer.AddComponent<RectTransform>();
            choicesRect.anchorMin = new Vector2(0.2f, 0.42f); // Centered (was 0.15, 0.58)
            choicesRect.anchorMax = new Vector2(0.8f, 0.58f); // Centered (was 0.85, 0.75)
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

            // Leave button - allows skipping sanctuary without making a choice (gray/muted style)
            // Positioned lower and larger for better accessibility
            GameObject leaveBtn = CreateMenuButton(screenObj, "LeaveButton", "LEAVE", "gray");
            RectTransform leaveRect = leaveBtn.GetComponent<RectTransform>();
            leaveRect.anchorMin = new Vector2(0.5f, 0.05f);
            leaveRect.anchorMax = new Vector2(0.5f, 0.05f);
            leaveRect.sizeDelta = new Vector2(280, 65); // Increased from (220, 55)

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

            // Confirm button (green for positive action)
            GameObject confirmUpgradeBtn = CreateMenuButton(cardSelectionPanel, "ConfirmUpgradeButton", "CONFIRM UPGRADE", "green");
            RectTransform confirmRect = confirmUpgradeBtn.GetComponent<RectTransform>();
            confirmRect.anchorMin = new Vector2(0.35f, 0.08f);
            confirmRect.anchorMax = new Vector2(0.35f, 0.08f);
            confirmRect.sizeDelta = new Vector2(180, 45);
            confirmUpgradeBtn.GetComponent<Button>().interactable = false;

            // Cancel button (red for negative/cancel action)
            GameObject cancelUpgradeBtn = CreateMenuButton(cardSelectionPanel, "CancelUpgradeButton", "CANCEL", "red");
            RectTransform cancelRect = cancelUpgradeBtn.GetComponent<RectTransform>();
            cancelRect.anchorMin = new Vector2(0.65f, 0.08f);
            cancelRect.anchorMax = new Vector2(0.65f, 0.08f);
            cancelRect.sizeDelta = new Vector2(140, 45);

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

            // Wire visual anchor references (legacy)
            so.FindProperty("_visualAnchorsContainer").objectReferenceValue = visualAnchorsRect;
            so.FindProperty("_leftVisualAnchor").objectReferenceValue = leftRect;
            so.FindProperty("_centerVisualAnchor").objectReferenceValue = centerRect;
            so.FindProperty("_rightVisualAnchor").objectReferenceValue = rightRect;

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
            var layerLabConfig = LoadLayerLabConfig();

            // Determine LayerLab button color based on semantic color
            string buttonColor = "purple"; // Default
            if (color.g > 0.7f && color.r < 0.3f) buttonColor = "green"; // Health green
            else if (color.b > 0.8f) buttonColor = "purple"; // Cyan maps to purple
            else if (color.r > 0.7f && color.g > 0.5f) buttonColor = "purple"; // Gold maps to purple

            GameObject choice;
            if (layerLabConfig != null && layerLabConfig.HasAllButton01SmallSprites())
            {
                // Use LayerLab button with title as main text
                choice = LayerLabButtonBuilder.CreateButton01Small(parent, name, title, buttonColor);

                // Adjust size for sanctuary layout - larger buttons
                var rect = choice.GetComponent<RectTransform>();
                rect.sizeDelta = new Vector2(220, 110);

                // Adjust main text positioning - closer to center for better visual balance
                var textObj = choice.transform.Find("Text");
                if (textObj != null)
                {
                    var textRect = textObj.GetComponent<RectTransform>();
                    textRect.anchorMin = new Vector2(0, 0.52f);
                    textRect.anchorMax = new Vector2(1, 0.90f);
                    textRect.sizeDelta = Vector2.zero;
                    var titleTmp = textObj.GetComponent<TMP_Text>();
                    if (titleTmp != null)
                    {
                        titleTmp.fontSize = 28;
                        titleTmp.fontSizeMax = 28;
                        titleTmp.fontSizeMin = 20;
                        titleTmp.color = Color.white; // White for better contrast
                        titleTmp.fontStyle = FontStyles.Bold;
                    }
                }

                // Add description below title - positioned closer to title for better visual balance
                GameObject descObj = CreateText(choice, "Desc", desc, 18);
                RectTransform descRect = descObj.GetComponent<RectTransform>();
                descRect.anchorMin = new Vector2(0, 0.15f);
                descRect.anchorMax = new Vector2(1, 0.50f);
                descRect.sizeDelta = Vector2.zero;
                var descTmp = descObj.GetComponent<TMP_Text>();
                descTmp.color = new Color(0.35f, 0.35f, 0.4f); // Dark gray for contrast
                descTmp.alignment = TextAlignmentOptions.Center;
            }
            else
            {
                // Fallback to simple styled button - larger with better contrast
                choice = new GameObject(name);
                choice.transform.SetParent(parent.transform, false);

                Image choiceBg = choice.AddComponent<Image>();
                choiceBg.color = new Color(color.r * 0.3f, color.g * 0.3f, color.b * 0.3f, 0.9f);

                Button btn = choice.AddComponent<Button>();
                btn.targetGraphic = choiceBg;

                var rect = choice.AddComponent<RectTransform>();
                rect.sizeDelta = new Vector2(200, 110);

                VerticalLayoutGroup layout = choice.AddComponent<VerticalLayoutGroup>();
                layout.spacing = 10;
                layout.padding = new RectOffset(15, 15, 18, 18);
                layout.childAlignment = TextAnchor.MiddleCenter;
                layout.childForceExpandWidth = true;
                layout.childForceExpandHeight = false;

                // Title text - larger and white for better contrast
                GameObject titleObj = CreateText(choice, "Title", title, 26);
                titleObj.GetComponent<TMP_Text>().color = Color.white;
                titleObj.GetComponent<TMP_Text>().fontStyle = TMPro.FontStyles.Bold;
                var titleLayout = titleObj.AddComponent<LayoutElement>();
                titleLayout.preferredHeight = 35;

                // Description text - larger with dark gray color
                GameObject descObj = CreateText(choice, "Desc", desc, 18);
                descObj.GetComponent<TMP_Text>().color = new Color(0.35f, 0.35f, 0.4f); // Dark gray
                var descLayout = descObj.AddComponent<LayoutElement>();
                descLayout.preferredHeight = 28;
            }

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

            // Add CardFanLayout component for card display and management
            var cardFanLayout = container.AddComponent<CardFanLayout>();

            // Configure CardFanLayout positioning for the HandContainer area
            // HandContainer occupies bottom 35% of screen (anchors 0-0.35 vertically)
            // Formula: y = cos(angle) * radius - radius + center.y
            // For center card (angle=0): y = center.y
            // So center.y=100 places center card 100px above container center
            SerializedObject fanSO = new SerializedObject(cardFanLayout);
            fanSO.FindProperty("_fanAngle").floatValue = 30f;           // Default fan spread
            fanSO.FindProperty("_fanRadius").floatValue = 500f;         // Arc radius
            fanSO.FindProperty("_fanCenter").vector2Value = new Vector2(0, 80f);  // Cards positioned above center
            fanSO.FindProperty("_cardSize").vector2Value = new Vector2(140, 190);   // Original card size
            fanSO.FindProperty("_hoverLiftY").floatValue = 50f;         // Original hover lift
            fanSO.FindProperty("_hoverScale").floatValue = 1.2f;        // Original hover scale
            fanSO.ApplyModifiedPropertiesWithoutUndo();

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

            // Apply LayerLab font if available
            var layerLabConfig = LoadLayerLabConfig();
            if (layerLabConfig != null && layerLabConfig.FontAfacadFlux != null)
            {
                tmp.font = layerLabConfig.FontAfacadFlux;
            }

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
            return CreateMenuButton(parent, name, text, "purple");
        }

        /// <summary>
        /// Creates a menu button using LayerLab Button_01 small style.
        /// Falls back to simple colored button if LayerLab config not available.
        /// </summary>
        /// <param name="parent">Parent GameObject</param>
        /// <param name="name">Button name</param>
        /// <param name="text">Button text</param>
        /// <param name="color">Button color: "purple", "green", "red", "gray"</param>
        /// <returns>The button GameObject</returns>
        private static GameObject CreateMenuButton(GameObject parent, string name, string text, string color)
        {
            var layerLabConfig = LoadLayerLabConfig();

            // Use LayerLab button if config is available and valid
            if (layerLabConfig != null && layerLabConfig.IsValid())
            {
                var buttonObj = LayerLabButtonBuilder.CreateButton01Small(parent, name, text, color);
                // Scale to match expected menu button size (250x60 vs native 230x104)
                RectTransform rect = buttonObj.GetComponent<RectTransform>();
                rect.sizeDelta = new Vector2(250, 60);
                return buttonObj;
            }

            // Fallback to simple colored button
            GameObject obj = new GameObject(name);
            obj.transform.SetParent(parent.transform, false);

            RectTransform rect2 = obj.AddComponent<RectTransform>();
            rect2.sizeDelta = new Vector2(250, 60);

            Image img = obj.AddComponent<Image>();
            img.color = GetFallbackButtonColor(color);

            Button btn = obj.AddComponent<Button>();
            btn.targetGraphic = img;

            ColorBlock colors = btn.colors;
            colors.normalColor = GetFallbackButtonColor(color);
            colors.highlightedColor = GetFallbackButtonColor(color) * 1.2f;
            colors.pressedColor = GetFallbackButtonColor(color) * 0.8f;
            colors.selectedColor = GetFallbackButtonColor(color) * 1.1f;
            btn.colors = colors;

            GameObject textObj = CreateText(obj, "Text", text, 24);
            RectTransform textRect = textObj.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.sizeDelta = Vector2.zero;

            return obj;
        }

        /// <summary>
        /// Gets fallback button color when LayerLab sprites not available.
        /// </summary>
        private static Color GetFallbackButtonColor(string color)
        {
            return color?.ToLower() switch
            {
                "purple" => new Color(0.25f, 0.15f, 0.35f),
                "green" => new Color(0.15f, 0.35f, 0.2f),
                "red" => new Color(0.35f, 0.15f, 0.15f),
                "gray" => new Color(0.25f, 0.25f, 0.28f),
                _ => new Color(0.25f, 0.15f, 0.35f)
            };
        }

        /// <summary>
        /// Sets a button to disabled/grayed out style for placeholder features.
        /// </summary>
        private static void SetButtonDisabledStyle(GameObject buttonObj)
        {
            var button = buttonObj.GetComponent<Button>();
            if (button != null)
            {
                button.interactable = false;

                // Gray out the button colors
                ColorBlock colors = button.colors;
                colors.disabledColor = new Color(0.4f, 0.4f, 0.45f, 0.6f);
                button.colors = colors;
            }

            // Gray out background image
            var bgImage = buttonObj.GetComponent<Image>();
            if (bgImage != null)
            {
                bgImage.color = new Color(0.25f, 0.25f, 0.3f, 0.7f);
            }

            // Gray out text
            var textObj = buttonObj.transform.Find("ButtonText") ?? buttonObj.transform.Find("Text");
            if (textObj != null)
            {
                var tmp = textObj.GetComponent<TextMeshProUGUI>();
                if (tmp != null)
                {
                    tmp.color = new Color(0.5f, 0.5f, 0.55f, 0.8f);
                }
            }

            // "Coming Soon" text removed per Phase 4 UI cleanup
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
        /// Wires CombatConfigSO to the TurnManager.
        /// </summary>
        private static void WireTurnManager(TurnManager turnManager)
        {
            if (turnManager == null) return;

            var combatConfig = AssetDatabase.LoadAssetAtPath<CombatConfigSO>(COMBAT_CONFIG_PATH);

            SerializedObject so = new SerializedObject(turnManager);

            if (combatConfig != null)
            {
                so.FindProperty("_combatConfig").objectReferenceValue = combatConfig;
                so.ApplyModifiedPropertiesWithoutUndo();
                Debug.Log("[ProductionSceneSetupGenerator] Wired TurnManager with CombatConfigSO");
            }
            else
            {
                Debug.LogWarning("[ProductionSceneSetupGenerator] CombatConfigSO not found at " + COMBAT_CONFIG_PATH +
                    ". Run 'HNR > 5. Utilities > Config > Generate Combat Config' first.");
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
            var apCounter = combatScreenObj.GetComponentInChildren<APCounterDisplay>(true);
            var execButton = combatScreenObj.GetComponentInChildren<ExecutionButton>(true);
            var sysMenu = combatScreenObj.GetComponentInChildren<SystemMenuBar>(true);

            // CardFanLayout is on HandContainer (sibling of ScreenContainer), find it at canvas level
            var canvas = combatScreenObj.GetComponentInParent<Canvas>();
            var cardFan = canvas != null ? canvas.GetComponentInChildren<CardFanLayout>(true) : null;

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
                {
                    SetPropertyIfExists(so, "_enemyUIPrefab", floatingUI);

                    // Wire status icon prefab to EnemyFloatingUI
                    var runtimePrefabConfig = LoadRuntimePrefabConfig();
                    if (runtimePrefabConfig != null && runtimePrefabConfig.StatusIconPrefab != null)
                    {
                        var floatingUISO = new SerializedObject(floatingUI);
                        var statusIconProp = floatingUISO.FindProperty("_statusIconPrefab");
                        if (statusIconProp != null)
                        {
                            statusIconProp.objectReferenceValue = runtimePrefabConfig.StatusIconPrefab;
                            floatingUISO.ApplyModifiedPropertiesWithoutUndo();
                            Debug.Log("[ProductionSceneSetupGenerator] Wired StatusIcon.prefab to EnemyFloatingUI._statusIconPrefab");
                        }
                    }
                }
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

            // Load LayerLab config for styling
            var layerLabConfig = LoadLayerLabConfig();

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
            // HP Bar Row (top) - WIDE RECTANGLE with LayerLab styling
            // ============================================
            GameObject hpRow = new GameObject("HPBarRow");
            hpRow.transform.SetParent(barObj.transform, false);

            RectTransform hpRowRect = hpRow.AddComponent<RectTransform>();

            var hpRowLayoutElement = hpRow.AddComponent<LayoutElement>();
            hpRowLayoutElement.preferredHeight = 32; // Taller for better visibility
            hpRowLayoutElement.minHeight = 28;

            var hpRowLayout = hpRow.AddComponent<HorizontalLayoutGroup>();
            hpRowLayout.spacing = 10f;
            hpRowLayout.childAlignment = TextAnchor.MiddleLeft;
            hpRowLayout.childForceExpandWidth = false;
            hpRowLayout.childForceExpandHeight = true;
            hpRowLayout.childControlWidth = true;
            hpRowLayout.childControlHeight = true;

            // HP Bar container - WIDE rectangle with LayerLab frame styling
            GameObject hpBarContainer = new GameObject("HPBarContainer");
            hpBarContainer.transform.SetParent(hpRow.transform, false);

            RectTransform hpContainerRect = hpBarContainer.AddComponent<RectTransform>();

            var hpLayout = hpBarContainer.AddComponent<LayoutElement>();
            hpLayout.minWidth = 640;
            hpLayout.preferredWidth = 640;
            hpLayout.preferredHeight = 28; // Taller for better visibility
            hpLayout.flexibleWidth = -1; // Fixed width - do not expand/shrink

            // HP Bar background with LayerLab slider styling
            Image hpBg = hpBarContainer.AddComponent<Image>();

            // Add Mask to clip child elements properly
            var hpMask = hpBarContainer.AddComponent<Mask>();
            hpMask.showMaskGraphic = true;

            if (layerLabConfig != null && layerLabConfig.SliderBorderTaperedBg != null)
            {
                hpBg.sprite = layerLabConfig.SliderBorderTaperedBg;
                hpBg.type = Image.Type.Sliced;
                hpBg.color = new Color(0.3f, 0.25f, 0.4f, 0.85f); // Purple tint

                // Add border layer using slider border
                if (layerLabConfig.SliderBorderTaperedBorder != null)
                {
                    GameObject borderObj = new GameObject("Border");
                    borderObj.transform.SetParent(hpBarContainer.transform, false);
                    RectTransform borderRect = borderObj.AddComponent<RectTransform>();
                    borderRect.anchorMin = Vector2.zero;
                    borderRect.anchorMax = Vector2.one;
                    borderRect.sizeDelta = Vector2.zero;
                    Image borderImg = borderObj.AddComponent<Image>();
                    borderImg.sprite = layerLabConfig.SliderBorderTaperedBorder;
                    borderImg.type = Image.Type.Sliced;
                    borderImg.color = new Color(0.6f, 0.5f, 0.8f, 0.8f);
                    borderImg.raycastTarget = false;
                    borderObj.transform.SetAsFirstSibling();
                }
            }
            else if (layerLabConfig != null && layerLabConfig.TabMenuBg != null)
            {
                // Fallback to TabMenu styling if slider sprites not available
                hpBg.sprite = layerLabConfig.TabMenuBg;
                hpBg.type = Image.Type.Sliced;
                hpBg.color = new Color(0.3f, 0.25f, 0.4f, 0.85f);

                if (layerLabConfig.TabMenuBorder != null)
                {
                    GameObject borderObj = new GameObject("Border");
                    borderObj.transform.SetParent(hpBarContainer.transform, false);
                    RectTransform borderRect = borderObj.AddComponent<RectTransform>();
                    borderRect.anchorMin = Vector2.zero;
                    borderRect.anchorMax = Vector2.one;
                    borderRect.sizeDelta = Vector2.zero;
                    Image borderImg = borderObj.AddComponent<Image>();
                    borderImg.sprite = layerLabConfig.TabMenuBorder;
                    borderImg.type = Image.Type.Sliced;
                    borderImg.color = new Color(0.6f, 0.5f, 0.8f, 0.6f);
                    borderImg.raycastTarget = false;
                    borderObj.transform.SetAsFirstSibling();
                }
            }
            else
            {
                hpBg.color = new Color(0.1f, 0.08f, 0.15f, 0.85f);
            }

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

            // Damage Fill - with LayerLab slider fill sprite if available
            GameObject dmgFillObj = new GameObject("Fill");
            dmgFillObj.transform.SetParent(dmgFillAreaObj.transform, false);
            RectTransform dmgFillRect = dmgFillObj.AddComponent<RectTransform>();
            dmgFillRect.anchorMin = Vector2.zero;
            dmgFillRect.anchorMax = Vector2.one;
            dmgFillRect.offsetMin = Vector2.zero;
            dmgFillRect.offsetMax = Vector2.zero;
            Image dmgFillImg = dmgFillObj.AddComponent<Image>();
            if (layerLabConfig != null && layerLabConfig.SliderBorderTaperedFill != null)
            {
                dmgFillImg.sprite = layerLabConfig.SliderBorderTaperedFill;
                dmgFillImg.type = Image.Type.Sliced;
            }
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

            // Health Fill - with LayerLab slider fill sprite if available
            GameObject hpFillObj = new GameObject("Fill");
            hpFillObj.transform.SetParent(hpFillAreaObj.transform, false);
            RectTransform hpFillRect = hpFillObj.AddComponent<RectTransform>();
            hpFillRect.anchorMin = Vector2.zero;
            hpFillRect.anchorMax = Vector2.one;
            hpFillRect.offsetMin = Vector2.zero;
            hpFillRect.offsetMax = Vector2.zero;
            Image hpFillImg = hpFillObj.AddComponent<Image>();
            if (layerLabConfig != null && layerLabConfig.SliderBorderTaperedFill != null)
            {
                hpFillImg.sprite = layerLabConfig.SliderBorderTaperedFill;
                hpFillImg.type = Image.Type.Sliced;
            }
            hpFillImg.color = new Color(0.18f, 0.8f, 0.44f); // Health green #2ECC71
            healthSlider.fillRect = hpFillRect;

            // HP Text (on top of sliders) - LARGER font (16 instead of 11)
            GameObject hpText = CreateText(hpBarContainer, "HPText", "210 / 210", 16);
            RectTransform hpTextRect = hpText.GetComponent<RectTransform>();
            hpTextRect.anchorMin = Vector2.zero;
            hpTextRect.anchorMax = Vector2.one;
            hpTextRect.sizeDelta = Vector2.zero;
            var hpTmp = hpText.GetComponent<TextMeshProUGUI>();
            hpTmp.fontStyle = TMPro.FontStyles.Bold;
            hpTmp.color = Color.white;

            // Block indicator container - LARGER for better visibility
            GameObject blockContainer = new GameObject("BlockContainer");
            blockContainer.transform.SetParent(hpRow.transform, false);
            var blockLayout = blockContainer.AddComponent<HorizontalLayoutGroup>();
            blockLayout.spacing = 4f;
            blockLayout.childAlignment = TextAnchor.MiddleCenter;
            blockLayout.padding = new RectOffset(4, 4, 0, 0);
            blockLayout.childForceExpandWidth = false;
            blockLayout.childForceExpandHeight = false;
            blockLayout.childControlWidth = false;
            blockLayout.childControlHeight = false;

            var blockLayoutElement = blockContainer.AddComponent<LayoutElement>();
            blockLayoutElement.minWidth = 80;
            blockLayoutElement.preferredWidth = 80;
            blockLayoutElement.preferredHeight = 32;

            // Shield icon - LARGER (20x20 instead of 16x16) with sprite from config
            GameObject shieldIcon = new GameObject("ShieldIcon");
            shieldIcon.transform.SetParent(blockContainer.transform, false);
            RectTransform shieldRect = shieldIcon.AddComponent<RectTransform>();
            shieldRect.sizeDelta = new Vector2(20, 20);
            Image shieldImg = shieldIcon.AddComponent<Image>();
            if (layerLabConfig != null && layerLabConfig.ItemIconShield != null)
            {
                shieldImg.sprite = layerLabConfig.ItemIconShield;
                shieldImg.color = Color.white; // Show sprite with native colors
            }
            else
            {
                shieldImg.color = new Color(0.2f, 0.6f, 0.86f); // Fallback block blue #3498DB
            }

            // Block text - LARGER font (16 instead of 12)
            GameObject blockText = CreateText(blockContainer, "BlockText", "12", 16);
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
        /// Uses LayerLab ConvexRectangle buttons for consistent styling.
        /// </summary>
        private static GameObject CreateSystemMenuBar(GameObject parent)
        {
            GameObject menuBar = new GameObject("SystemMenuBar");
            menuBar.transform.SetParent(parent.transform, false);

            RectTransform rect = menuBar.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(1, 0.5f);
            rect.anchorMax = new Vector2(1, 0.5f);
            rect.pivot = new Vector2(1, 0.5f);
            rect.anchoredPosition = new Vector2(-8, 0);
            rect.sizeDelta = new Vector2(180, 48); // Larger to accommodate bigger buttons

            var layout = menuBar.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = 8f;
            layout.childAlignment = TextAnchor.MiddleRight;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;
            layout.reverseArrangement = true;

            // Load LayerLab config for system menu icons
            var layerLabConfig = LoadLayerLabConfig();

            // Settings button - use LayerLab ConvexRectangle with settings icon
            GameObject settingsBtn = CreateSystemMenuButtonLayerLab(menuBar, "SettingsBtn", "\u2699", layerLabConfig?.IconSettings, layerLabConfig);

            // Auto-battle button - use LayerLab ConvexRectangle with attack icon
            GameObject autoBtn = CreateSystemMenuButtonLayerLab(menuBar, "AutoBtn", "\u25B6", layerLabConfig?.PictoIconAttack, layerLabConfig);

            // Speed button - use LayerLab ConvexRectangle with timer icon
            GameObject speedBtn = CreateSystemMenuButtonLayerLab(menuBar, "SpeedBtn", "1x", layerLabConfig?.SpeedIcon1x, layerLabConfig);

            // Add SystemMenuBar component and wire references
            var sysMenuBar = menuBar.AddComponent<SystemMenuBar>();
            SerializedObject so = new SerializedObject(sysMenuBar);

            // Wire speed toggle
            so.FindProperty("_speedToggle").objectReferenceValue = speedBtn.GetComponent<Button>();
            // Wire the speed icon image for sprite swapping
            var speedIconImage = speedBtn.transform.Find("Icon")?.GetComponent<Image>() ?? speedBtn.transform.Find("Label")?.GetComponent<Image>();
            if (speedIconImage != null)
                so.FindProperty("_speedIcon").objectReferenceValue = speedIconImage;
            // Wire both speed sprites from LayerLab config
            if (layerLabConfig?.SpeedIcon1x != null)
                so.FindProperty("_speed1xSprite").objectReferenceValue = layerLabConfig.SpeedIcon1x;
            if (layerLabConfig?.SpeedIcon2x != null)
                so.FindProperty("_speed2xSprite").objectReferenceValue = layerLabConfig.SpeedIcon2x;

            // Wire auto-battle toggle
            so.FindProperty("_autoBattleToggle").objectReferenceValue = autoBtn.GetComponent<Button>();
            // Wire the icon image (child "Icon" or "Label" contains the sprite/text) for proper color state feedback
            var autoBattleIconImage = autoBtn.transform.Find("Icon")?.GetComponent<Image>() ?? autoBtn.transform.Find("Label")?.GetComponent<Image>();
            so.FindProperty("_autoBattleIcon").objectReferenceValue = autoBattleIconImage != null ? autoBattleIconImage : autoBtn.GetComponent<Image>();

            // Wire settings button
            so.FindProperty("_settingsButton").objectReferenceValue = settingsBtn.GetComponent<Button>();

            so.ApplyModifiedPropertiesWithoutUndo();

            return menuBar;
        }

        /// <summary>
        /// Creates a system menu button with LayerLab ConvexRectangle styling.
        /// </summary>
        private static GameObject CreateSystemMenuButtonLayerLab(GameObject parent, string name, string fallbackLabel, Sprite iconSprite, LayerLabSpriteConfigSO layerLabConfig)
        {
            const float buttonSize = 48f;

            // Use LayerLab ConvexRectangle if available
            if (layerLabConfig != null && layerLabConfig.HasAllConvexButtonSprites())
            {
                var buttonObj = LayerLabButtonBuilder.CreateConvexRectangleButton(parent, name, iconSprite);
                RectTransform rect = buttonObj.GetComponent<RectTransform>();
                rect.sizeDelta = new Vector2(buttonSize, buttonSize);

                var layoutElement = buttonObj.AddComponent<LayoutElement>();
                layoutElement.preferredWidth = buttonSize;
                layoutElement.preferredHeight = buttonSize;

                return buttonObj;
            }

            // Fallback to simple button
            GameObject btn = new GameObject(name);
            btn.transform.SetParent(parent.transform, false);

            RectTransform rectFallback = btn.AddComponent<RectTransform>();
            rectFallback.sizeDelta = new Vector2(buttonSize, buttonSize);

            Image bg = btn.AddComponent<Image>();
            bg.color = new Color(0.15f, 0.12f, 0.22f, 0.9f);

            Button button = btn.AddComponent<Button>();
            button.targetGraphic = bg;

            var layoutElementFallback = btn.AddComponent<LayoutElement>();
            layoutElementFallback.preferredWidth = buttonSize;
            layoutElementFallback.preferredHeight = buttonSize;

            // Use sprite if available, otherwise use text
            Color cyanColor = new Color(0f, 0.83f, 0.89f); // Soul cyan
            GameObject content = CreateIconImage(btn, "Label", iconSprite, new Vector2(28, 28), cyanColor, fallbackLabel, 18);
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
            rect.offsetMax = new Vector2(95, -8); // 87px width total (slightly wider for LayerLab styling)

            // Load LayerLab config for frame styling
            var layerLabConfig = LoadLayerLabConfig();

            // No background image on PartySidebar - keep it transparent

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
            seGaugeLayoutElement.preferredWidth = 28; // Slightly wider for better visibility
            seGaugeLayoutElement.minWidth = 28;
            // No flexibleHeight - let HorizontalLayoutGroup control height via childForceExpandHeight

            // SharedSEGauge container has NO Image - restructured per Phase 4 spec
            // Structure: SELabel > SESlider > SEText (sibling order in hierarchy)
            // SESlider internally: Bg (with BgLeft, BgRight) > FillArea > Fill (with FillBorder)

            // SE Label at top (created first for proper sibling order)
            GameObject seLabelObj = CreateText(seGaugeContainer, "SELabel", "SE", 20);
            RectTransform seLabelRect = seLabelObj.GetComponent<RectTransform>();
            seLabelRect.anchorMin = new Vector2(0, 1);
            seLabelRect.anchorMax = new Vector2(1, 1);
            seLabelRect.pivot = new Vector2(0.5f, 1);
            seLabelRect.anchoredPosition = new Vector2(0, 0);
            seLabelRect.sizeDelta = new Vector2(0, 20);
            var seLabelTmp = seLabelObj.GetComponent<TextMeshProUGUI>();
            seLabelTmp.fontStyle = TMPro.FontStyles.Bold;
            seLabelTmp.fontSize = 20;
            seLabelTmp.color = new Color(0.7f, 0.7f, 0.8f);

            // SE Slider (vertical fill from bottom to top)
            GameObject seSliderObj = new GameObject("SESlider");
            seSliderObj.transform.SetParent(seGaugeContainer.transform, false);
            RectTransform seSliderRect = seSliderObj.AddComponent<RectTransform>();
            seSliderRect.anchorMin = new Vector2(0, 0);
            seSliderRect.anchorMax = new Vector2(1, 1); // Full stretch
            seSliderRect.offsetMin = new Vector2(0, 20); // Margin for SE text at bottom
            seSliderRect.offsetMax = new Vector2(0, -20); // Margin for SE label at top

            // Slider component configured for vertical bottom-to-top
            Slider seSlider = seSliderObj.AddComponent<Slider>();
            seSlider.direction = Slider.Direction.BottomToTop;
            seSlider.minValue = 0f;
            seSlider.maxValue = 1f;
            seSlider.value = 0f;
            seSlider.interactable = false;

            // Bg container (with sprite from LayerLab if available)
            // Color: #1C1B33 = new Color(0.11f, 0.106f, 0.2f)
            GameObject seBgObj = new GameObject("Bg");
            seBgObj.transform.SetParent(seSliderObj.transform, false);
            RectTransform seBgRect = seBgObj.AddComponent<RectTransform>();
            seBgRect.anchorMin = Vector2.zero;
            seBgRect.anchorMax = Vector2.one;
            seBgRect.offsetMin = Vector2.zero;
            seBgRect.offsetMax = Vector2.zero;
            Image seBgImg = seBgObj.AddComponent<Image>();
            if (layerLabConfig != null && layerLabConfig.VerticalSliderBg != null)
            {
                seBgImg.sprite = layerLabConfig.VerticalSliderBg;
                seBgImg.type = Image.Type.Sliced;
            }
            seBgImg.color = new Color(0.11f, 0.106f, 0.2f); // #1C1B33

            // BgLeft - left edge decoration
            // Color: #4C3C73 = new Color(0.298f, 0.235f, 0.451f)
            GameObject seBgLeftObj = new GameObject("BgLeft");
            seBgLeftObj.transform.SetParent(seBgObj.transform, false);
            RectTransform seBgLeftRect = seBgLeftObj.AddComponent<RectTransform>();
            seBgLeftRect.anchorMin = Vector2.zero;
            seBgLeftRect.anchorMax = Vector2.one;
            seBgLeftRect.offsetMin = Vector2.zero;
            seBgLeftRect.offsetMax = Vector2.zero;
            Image seBgLeftImg = seBgLeftObj.AddComponent<Image>();
            if (layerLabConfig != null && layerLabConfig.VerticalSliderBgLeft != null)
            {
                seBgLeftImg.sprite = layerLabConfig.VerticalSliderBgLeft;
                seBgLeftImg.type = Image.Type.Sliced;
            }
            seBgLeftImg.color = new Color(0.298f, 0.235f, 0.451f); // #4C3C73
            seBgLeftImg.raycastTarget = false;

            // BgRight - right edge decoration
            // Color: #6C3B95 = new Color(0.424f, 0.231f, 0.584f)
            GameObject seBgRightObj = new GameObject("BgRight");
            seBgRightObj.transform.SetParent(seBgObj.transform, false);
            RectTransform seBgRightRect = seBgRightObj.AddComponent<RectTransform>();
            seBgRightRect.anchorMin = Vector2.zero;
            seBgRightRect.anchorMax = Vector2.one;
            seBgRightRect.offsetMin = Vector2.zero;
            seBgRightRect.offsetMax = Vector2.zero;
            Image seBgRightImg = seBgRightObj.AddComponent<Image>();
            if (layerLabConfig != null && layerLabConfig.VerticalSliderBgRight != null)
            {
                seBgRightImg.sprite = layerLabConfig.VerticalSliderBgRight;
                seBgRightImg.type = Image.Type.Sliced;
            }
            seBgRightImg.color = new Color(0.424f, 0.231f, 0.584f); // #6C3B95
            seBgRightImg.raycastTarget = false;

            // Fill Area for slider
            GameObject seFillAreaObj = new GameObject("FillArea");
            seFillAreaObj.transform.SetParent(seSliderObj.transform, false);
            RectTransform seFillAreaRect = seFillAreaObj.AddComponent<RectTransform>();
            seFillAreaRect.anchorMin = Vector2.zero;
            seFillAreaRect.anchorMax = Vector2.one;
            seFillAreaRect.offsetMin = new Vector2(2, 2);
            seFillAreaRect.offsetMax = new Vector2(-2, -2);

            // Fill image for slider - white base color
            GameObject seFillObj = new GameObject("Fill");
            seFillObj.transform.SetParent(seFillAreaObj.transform, false);
            RectTransform seFillRect = seFillObj.AddComponent<RectTransform>();
            seFillRect.anchorMin = Vector2.zero;
            seFillRect.anchorMax = Vector2.one;
            seFillRect.offsetMin = Vector2.zero;
            seFillRect.offsetMax = Vector2.zero;
            Image seFillImg = seFillObj.AddComponent<Image>();
            seFillImg.color = Color.white; // White fill, tinted by FillBorder sprite

            // FillBorder - border overlay for fill
            // Color: #EB9A19 = new Color(0.922f, 0.604f, 0.098f)
            GameObject seFillBorderObj = new GameObject("FillBorder");
            seFillBorderObj.transform.SetParent(seFillObj.transform, false);
            RectTransform seFillBorderRect = seFillBorderObj.AddComponent<RectTransform>();
            seFillBorderRect.anchorMin = Vector2.zero;
            seFillBorderRect.anchorMax = Vector2.one;
            seFillBorderRect.offsetMin = Vector2.zero;
            seFillBorderRect.offsetMax = Vector2.zero;
            Image seFillBorderImg = seFillBorderObj.AddComponent<Image>();
            if (layerLabConfig != null && layerLabConfig.VerticalSliderFillBorder != null)
            {
                seFillBorderImg.sprite = layerLabConfig.VerticalSliderFillBorder;
                seFillBorderImg.type = Image.Type.Sliced;
            }
            seFillBorderImg.color = new Color(0.922f, 0.604f, 0.098f); // #EB9A19
            seFillBorderImg.raycastTarget = false;

            // Wire slider fill rect
            seSlider.fillRect = seFillRect;

            // SE Text at bottom (shows current SE value) - created after slider for proper sibling order
            GameObject seTextObj = CreateText(seGaugeContainer, "SEText", "0", 22);
            RectTransform seTextRect = seTextObj.GetComponent<RectTransform>();
            seTextRect.anchorMin = new Vector2(0, 0);
            seTextRect.anchorMax = new Vector2(1, 0);
            seTextRect.pivot = new Vector2(0.5f, 0);
            seTextRect.anchoredPosition = new Vector2(0, 0);
            seTextRect.sizeDelta = new Vector2(0, 22);
            var seTmp = seTextObj.GetComponent<TextMeshProUGUI>();
            seTmp.fontStyle = TMPro.FontStyles.Bold;
            seTmp.fontSize = 22;
            seTmp.color = new Color(0f, 0.83f, 0.89f); // Soul cyan

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
            // Load LayerLab config for party slot sprites
            var layerLabConfig = LoadLayerLabConfig();

            GameObject slot = new GameObject($"PartySlot_{index}");
            slot.transform.SetParent(parent.transform, false);

            RectTransform slotRect = slot.AddComponent<RectTransform>();

            // LayoutElement - let parent VerticalLayoutGroup control size via childForceExpandHeight
            var layoutElement = slot.AddComponent<LayoutElement>();
            layoutElement.minHeight = 32; // Minimum height for each slot
            // No preferredHeight or flexibleHeight - parent controls via childForceExpandHeight

            // Add Button component for click interaction (Requiem Art activation)
            // Background is transparent but Image needed for Button targetGraphic
            Image slotBg = slot.AddComponent<Image>();
            slotBg.color = new Color(0, 0, 0, 0); // Transparent background

            Button slotButton = slot.AddComponent<Button>();
            slotButton.targetGraphic = slotBg;
            var buttonColors = slotButton.colors;
            buttonColors.normalColor = new Color(0, 0, 0, 0); // Transparent
            buttonColors.highlightedColor = new Color(0.25f, 0.2f, 0.35f, 0.3f); // Subtle highlight
            buttonColors.pressedColor = new Color(0f, 0.7f, 0.8f, 0.3f); // Subtle cyan press
            buttonColors.selectedColor = new Color(0.2f, 0.16f, 0.28f, 0.2f);
            slotButton.colors = buttonColors;

            // Portrait frame (circular) with mask - centered in slot, size relative to slot
            // Apply LayerLab party slot frame if available
            GameObject portraitFrame = new GameObject("PortraitFrame");
            portraitFrame.transform.SetParent(slot.transform, false);
            RectTransform frameRect = portraitFrame.AddComponent<RectTransform>();
            // Use stretch anchors with padding to fill most of the slot
            frameRect.anchorMin = new Vector2(0.1f, 0.1f);
            frameRect.anchorMax = new Vector2(0.9f, 0.9f);
            frameRect.offsetMin = Vector2.zero;
            frameRect.offsetMax = Vector2.zero;
            Image frameImg = portraitFrame.AddComponent<Image>();
            if (layerLabConfig != null && layerLabConfig.PartySlotFrame != null)
            {
                frameImg.sprite = layerLabConfig.PartySlotFrame;
                frameImg.type = Image.Type.Sliced;
                frameImg.color = Color.white; // Show sprite with native colors
            }
            else
            {
                frameImg.color = new Color(0.4f, 0.4f, 0.5f, 1f); // Fallback color
            }

            // Portrait mask (circular crop) - use stretch anchors with offset padding
            GameObject maskObj = new GameObject("PortraitMask");
            maskObj.transform.SetParent(portraitFrame.transform, false);
            RectTransform maskRect = maskObj.AddComponent<RectTransform>();
            maskRect.anchorMin = Vector2.zero;
            maskRect.anchorMax = Vector2.one;
            maskRect.offsetMin = new Vector2(8, 8); // Left=8, Bottom=8
            maskRect.offsetMax = new Vector2(-8, -8); // Right=8, Top=8
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
            portraitRect.sizeDelta = new Vector2(120, 160); // Larger size for better visibility
            portraitRect.anchoredPosition = new Vector2(0, 8); // Slight upward offset
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
            // Apply LayerLab party slot active glow if available
            GameObject activeGlow = new GameObject("ActiveGlow");
            activeGlow.transform.SetParent(slot.transform, false);
            activeGlow.transform.SetAsFirstSibling();
            RectTransform glowRect = activeGlow.AddComponent<RectTransform>();
            glowRect.anchorMin = Vector2.zero;
            glowRect.anchorMax = Vector2.one;
            glowRect.offsetMin = new Vector2(-2, -2);
            glowRect.offsetMax = new Vector2(2, 2);
            Image glowImg = activeGlow.AddComponent<Image>();
            if (layerLabConfig != null && layerLabConfig.PartySlotActiveGlow != null)
            {
                glowImg.sprite = layerLabConfig.PartySlotActiveGlow;
                glowImg.type = Image.Type.Sliced;
                glowImg.color = Color.white; // White to show sprite native colors
            }
            else
            {
                glowImg.color = new Color(0f, 0.83f, 0.89f, 0.4f); // Fallback soul cyan with alpha
            }
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
        /// Uses LayerLab frames around each pile for consistent styling.
        /// </summary>
        private static GameObject CreateDeckInfoSidebar(GameObject parent)
        {
            GameObject sidebar = new GameObject("DeckInfoSidebar");
            sidebar.transform.SetParent(parent.transform, false);

            RectTransform rect = sidebar.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.90f, 0.35f);
            rect.anchorMax = new Vector2(1f, 0.87f);
            rect.sizeDelta = Vector2.zero;

            VerticalLayoutGroup layout = sidebar.AddComponent<VerticalLayoutGroup>();
            layout.spacing = 30f; // Increased spacing between Draw and Discard piles
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;
            layout.padding = new RectOffset(6, 6, 12, 12);

            // Load LayerLab config for deck icons
            var layerLabConfig = LoadLayerLabConfig();

            // Draw pile - with LayerLab frame
            var drawPile = CreateDeckPileDisplayLayerLab(sidebar, "DrawPile", layerLabConfig?.DeckIconDraw, "\U0001F4DA", "23", "Draw", new Color(0f, 0.83f, 0.89f), layerLabConfig);

            // Discard pile - with LayerLab frame
            var discardPile = CreateDeckPileDisplayLayerLab(sidebar, "DiscardPile", layerLabConfig?.DeckIconDiscard, "\U0001F504", "0", "Discard", new Color(0.7f, 0.5f, 0.8f), layerLabConfig);

            // Add DeckInfoSidebar component and wire references
            var deckInfoComponent = sidebar.AddComponent<DeckInfoSidebar>();
            var so = new SerializedObject(deckInfoComponent);

            // Wire draw pile references
            var drawCountText = drawPile.transform.Find("Content/Count")?.GetComponent<TMP_Text>() ?? drawPile.transform.Find("Count")?.GetComponent<TMP_Text>();
            var drawLabelText = drawPile.transform.Find("Content/Label")?.GetComponent<TMP_Text>() ?? drawPile.transform.Find("Label")?.GetComponent<TMP_Text>();
            if (drawCountText != null) so.FindProperty("_drawPileText").objectReferenceValue = drawCountText;
            if (drawLabelText != null) so.FindProperty("_drawPileLabel").objectReferenceValue = drawLabelText;

            // Wire discard pile references
            var discardCountText = discardPile.transform.Find("Content/Count")?.GetComponent<TMP_Text>() ?? discardPile.transform.Find("Count")?.GetComponent<TMP_Text>();
            var discardLabelText = discardPile.transform.Find("Content/Label")?.GetComponent<TMP_Text>() ?? discardPile.transform.Find("Label")?.GetComponent<TMP_Text>();
            if (discardCountText != null) so.FindProperty("_discardPileText").objectReferenceValue = discardCountText;
            if (discardLabelText != null) so.FindProperty("_discardPileLabel").objectReferenceValue = discardLabelText;

            so.ApplyModifiedPropertiesWithoutUndo();

            return sidebar;
        }

        /// <summary>
        /// Creates a deck pile display with LayerLab frame styling.
        /// </summary>
        private static GameObject CreateDeckPileDisplayLayerLab(GameObject parent, string name, Sprite iconSprite, string fallbackIcon, string count, string label, Color color, LayerLabSpriteConfigSO layerLabConfig)
        {
            GameObject pile = new GameObject(name);
            pile.transform.SetParent(parent.transform, false);

            RectTransform pileRect = pile.AddComponent<RectTransform>();

            // Add LayoutElement for sizing
            var pileLayoutElement = pile.AddComponent<LayoutElement>();
            pileLayoutElement.preferredHeight = 90;
            pileLayoutElement.minHeight = 80;

            // Add LayerLab frame background
            if (layerLabConfig != null && layerLabConfig.StageFrameBg != null)
            {
                Image bgImage = pile.AddComponent<Image>();
                bgImage.sprite = layerLabConfig.StageFrameBg;
                bgImage.type = Image.Type.Sliced;
                bgImage.color = new Color(0.5f, 0.4f, 0.7f, 0.6f); // Purple tint with transparency

                // Add border layer
                if (layerLabConfig.StageFrameBorder != null)
                {
                    GameObject borderObj = new GameObject("Border");
                    borderObj.transform.SetParent(pile.transform, false);
                    RectTransform borderRect = borderObj.AddComponent<RectTransform>();
                    borderRect.anchorMin = Vector2.zero;
                    borderRect.anchorMax = Vector2.one;
                    borderRect.sizeDelta = Vector2.zero;
                    Image borderImg = borderObj.AddComponent<Image>();
                    borderImg.sprite = layerLabConfig.StageFrameBorder;
                    borderImg.type = Image.Type.Sliced;
                    borderImg.color = new Color(0.7f, 0.55f, 0.9f, 0.8f);
                    borderImg.raycastTarget = false;
                }
            }
            else
            {
                Image bgImage = pile.AddComponent<Image>();
                bgImage.color = new Color(0.15f, 0.12f, 0.22f, 0.85f);
            }

            // Content container
            GameObject content = new GameObject("Content");
            content.transform.SetParent(pile.transform, false);
            RectTransform contentRect = content.AddComponent<RectTransform>();
            contentRect.anchorMin = Vector2.zero;
            contentRect.anchorMax = Vector2.one;
            contentRect.offsetMin = new Vector2(4, 4);
            contentRect.offsetMax = new Vector2(-4, -4);

            VerticalLayoutGroup contentLayout = content.AddComponent<VerticalLayoutGroup>();
            contentLayout.spacing = 4f;
            contentLayout.childAlignment = TextAnchor.MiddleCenter;
            contentLayout.childForceExpandWidth = true;
            contentLayout.childForceExpandHeight = false;
            contentLayout.padding = new RectOffset(2, 2, 6, 6);

            // Icon - LARGER (32x32 instead of 24x24)
            GameObject iconObj = CreateIconImage(content, "Icon", iconSprite, new Vector2(32, 32), color, fallbackIcon, 22);
            var iconLayout = iconObj.AddComponent<LayoutElement>();
            iconLayout.preferredWidth = 32;
            iconLayout.preferredHeight = 32;

            // Count - LARGER font (20 instead of 14)
            GameObject countObj = CreateText(content, "Count", count, 20);
            countObj.GetComponent<TextMeshProUGUI>().color = color;
            countObj.GetComponent<TextMeshProUGUI>().fontStyle = TMPro.FontStyles.Bold;

            // Label - LARGER font (18 for better visibility) with fixed height
            GameObject labelObj = CreateText(content, "Label", label, 18);
            labelObj.GetComponent<TextMeshProUGUI>().color = new Color(0.8f, 0.8f, 0.85f);
            var labelLayout = labelObj.AddComponent<LayoutElement>();
            labelLayout.preferredHeight = 30; // Fixed height per plan

            return pile;
        }

        /// <summary>
        /// Creates the AP counter display.
        /// </summary>
        private static GameObject CreateAPCounter(GameObject parent)
        {
            GameObject counter = new GameObject("APCounter");
            counter.transform.SetParent(parent.transform, false);

            // Positioned at center bottom of screen - larger 90x90 for better visibility
            RectTransform rect = counter.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0f);
            rect.anchorMax = new Vector2(0.5f, 0f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = new Vector2(0, 60); // 60px above bottom edge
            rect.sizeDelta = new Vector2(90, 90);

            // Add Canvas with override sorting to render ABOVE cards
            // Cards are in HandContainer which is a sibling after ScreenContainer,
            // so we need explicit sorting order to ensure AP counter renders on top
            Canvas apCanvas = counter.AddComponent<Canvas>();
            apCanvas.overrideSorting = true;
            apCanvas.sortingOrder = 100; // High sorting order to render above cards
            counter.AddComponent<GraphicRaycaster>(); // Required for UI interactions

            // Load LayerLab config for frame styling
            var layerLabConfig = LoadLayerLabConfig();

            // Background frame using LayerLab StageFrame if available
            if (layerLabConfig != null && layerLabConfig.StageFrameBg != null)
            {
                Image bgFrame = counter.AddComponent<Image>();
                bgFrame.sprite = layerLabConfig.StageFrameBg;
                bgFrame.type = Image.Type.Sliced;
                bgFrame.color = new Color(0.5f, 0.4f, 0.7f, 0.8f); // Purple tint
            }

            // Glow background
            GameObject glow = new GameObject("Glow");
            glow.transform.SetParent(counter.transform, false);
            RectTransform glowRect = glow.AddComponent<RectTransform>();
            glowRect.anchorMin = Vector2.zero;
            glowRect.anchorMax = Vector2.one;
            glowRect.sizeDelta = new Vector2(24, 24);
            Image glowImg = glow.AddComponent<Image>();
            glowImg.color = new Color(0f, 0.83f, 0.89f, 0.35f); // Soul cyan glow

            // AP Number - LARGER font (48 instead of 36)
            GameObject apNum = CreateText(counter, "APNumber", "3", 48);
            RectTransform numRect = apNum.GetComponent<RectTransform>();
            numRect.anchorMin = new Vector2(0.5f, 0.55f);
            numRect.anchorMax = new Vector2(0.5f, 0.55f);
            numRect.sizeDelta = new Vector2(60, 50);
            var apText = apNum.GetComponent<TMP_Text>();
            apText.color = new Color(0f, 0.83f, 0.89f); // Soul cyan
            apText.fontStyle = TMPro.FontStyles.Bold;

            // AP Label - LARGER font (16 instead of 10)
            GameObject apLabel = CreateText(counter, "APLabel", "AP", 16);
            RectTransform labelRect = apLabel.GetComponent<RectTransform>();
            labelRect.anchorMin = new Vector2(0.5f, 0.22f);
            labelRect.anchorMax = new Vector2(0.5f, 0.22f);
            labelRect.sizeDelta = new Vector2(40, 20);
            apLabel.GetComponent<TextMeshProUGUI>().color = new Color(0.7f, 0.7f, 0.75f);

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
            layout.spacing = 12;
            layout.childAlignment = TextAnchor.MiddleLeft;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;
            layout.childControlWidth = true;
            layout.childControlHeight = true;

            // Back button area - use LayerLab style if available
            var zoneHeaderConfig = LoadLayerLabConfig();
            GameObject backBtn;
            if (zoneHeaderConfig != null && zoneHeaderConfig.HasAllConvexButtonSprites())
            {
                backBtn = LayerLabButtonBuilder.CreateConvexLeftFlushButton(header, "BackButton", zoneHeaderConfig.IconBack);
                // Scale to larger size for better mobile tappability
                var backRect = backBtn.GetComponent<RectTransform>();
                backRect.sizeDelta = new Vector2(80, 65);
            }
            else
            {
                backBtn = new GameObject("BackButton");
                backBtn.transform.SetParent(header.transform, false);
                Image backImg = backBtn.AddComponent<Image>();
                backImg.color = new Color(0.15f, 0.12f, 0.2f);
                Button backButton = backBtn.AddComponent<Button>();
                backButton.targetGraphic = backImg;

                GameObject backIcon = CreateText(backBtn, "Icon", "<", 18);
                RectTransform backIconRect = backIcon.GetComponent<RectTransform>();
                backIconRect.anchorMin = Vector2.zero;
                backIconRect.anchorMax = Vector2.one;
                backIconRect.sizeDelta = Vector2.zero;
                backIcon.GetComponent<TMP_Text>().color = new Color(0.63f, 0.63f, 0.63f);
            }
            var backLayout = backBtn.AddComponent<LayoutElement>();
            backLayout.preferredWidth = 60;
            backLayout.preferredHeight = 50;
            backLayout.flexibleWidth = 0;

            // Title container - positioned absolutely centered on screen (ignores layout)
            GameObject titleContainer = new GameObject("TitleContainer");
            titleContainer.transform.SetParent(header.transform, false);
            RectTransform titleContainerRect = titleContainer.AddComponent<RectTransform>();
            // Center on screen by using center anchors and ignoring HorizontalLayoutGroup
            titleContainerRect.anchorMin = new Vector2(0.5f, 0);
            titleContainerRect.anchorMax = new Vector2(0.5f, 1);
            titleContainerRect.anchoredPosition = Vector2.zero;
            titleContainerRect.sizeDelta = new Vector2(500, 0);

            VerticalLayoutGroup titleLayout = titleContainer.AddComponent<VerticalLayoutGroup>();
            titleLayout.childAlignment = TextAnchor.MiddleCenter;
            titleLayout.childForceExpandWidth = true;
            titleLayout.childForceExpandHeight = false;
            titleLayout.spacing = 2;

            // Add LayoutElement to ignore parent HorizontalLayoutGroup
            var titleLayoutElement = titleContainer.AddComponent<LayoutElement>();
            titleLayoutElement.ignoreLayout = true;

            // Zone Title - larger font for prominence
            GameObject zoneTitle = CreateText(titleContainer, "ZoneTitle", "NULL RIFT", 36);
            zoneTitle.GetComponent<TMP_Text>().fontStyle = TMPro.FontStyles.Bold;
            zoneTitle.GetComponent<TMP_Text>().color = new Color(0.9f, 0.9f, 0.95f);
            zoneTitle.GetComponent<TMP_Text>().alignment = TextAlignmentOptions.Center;

            // Zone Subtitle - larger font for better readability
            GameObject zoneSubtitle = CreateText(titleContainer, "ZoneSubtitle", "Zone 1 • The Outer Reaches", 22);
            zoneSubtitle.GetComponent<TMP_Text>().color = new Color(0.42f, 0.25f, 0.63f); // Hollow violet
            zoneSubtitle.GetComponent<TMP_Text>().alignment = TextAlignmentOptions.Center;

            // Stats container - positioned absolutely on far right (ignores parent layout)
            GameObject statsContainer = new GameObject("StatsContainer");
            statsContainer.transform.SetParent(header.transform, false);
            RectTransform statsRect = statsContainer.AddComponent<RectTransform>();
            statsRect.anchorMin = new Vector2(1, 0);
            statsRect.anchorMax = new Vector2(1, 1);
            statsRect.pivot = new Vector2(1, 0.5f);
            statsRect.anchoredPosition = new Vector2(-20, 0); // 20px from right edge
            statsRect.sizeDelta = new Vector2(420, 0); // Width for both containers

            HorizontalLayoutGroup statsLayout = statsContainer.AddComponent<HorizontalLayoutGroup>();
            statsLayout.spacing = 20;
            statsLayout.childAlignment = TextAnchor.MiddleRight;
            statsLayout.childForceExpandWidth = false;
            statsLayout.childForceExpandHeight = false;

            // Ignore parent HorizontalLayoutGroup
            var statsLayoutElement = statsContainer.AddComponent<LayoutElement>();
            statsLayoutElement.ignoreLayout = true;

            // Load LayerLab config for HP and Currency icons
            var layerLabConfig = LoadLayerLabConfig();

            // HP Container with AnimatedStatDisplay - larger width to prevent overlap
            GameObject hpContainer = new GameObject("HPContainer");
            hpContainer.transform.SetParent(statsContainer.transform, false);
            RectTransform hpContainerRect = hpContainer.AddComponent<RectTransform>();
            hpContainerRect.sizeDelta = new Vector2(200, 40);
            HorizontalLayoutGroup hpLayout = hpContainer.AddComponent<HorizontalLayoutGroup>();
            hpLayout.spacing = 8;
            hpLayout.childAlignment = TextAnchor.MiddleCenter;
            var hpContainerLayout = hpContainer.AddComponent<LayoutElement>();
            hpContainerLayout.preferredWidth = 200;
            hpContainerLayout.preferredHeight = 40;

            // HP Icon - white color to properly display icon sprite (sprite has its own color)
            GameObject hpIcon = CreateIconImage(hpContainer, "HPIcon", layerLabConfig?.ItemIconHeart, new Vector2(32, 32), Color.white, "\u2764", 24);
            var hpIconLayout = hpIcon.AddComponent<LayoutElement>();
            hpIconLayout.preferredWidth = 32;
            hpIconLayout.preferredHeight = 32;

            // HP AnimatedStatDisplay (DualValue mode for "current/max")
            GameObject hpDisplayObj = new GameObject("HPDisplay");
            hpDisplayObj.transform.SetParent(hpContainer.transform, false);
            var hpDisplay = hpDisplayObj.AddComponent<AnimatedStatDisplay>();
            var hpDisplayLayout = hpDisplayObj.AddComponent<LayoutElement>();
            hpDisplayLayout.preferredWidth = 150;
            hpDisplayLayout.preferredHeight = 32;

            // Create text for HP display - larger font
            GameObject hpText = CreateText(hpDisplayObj, "ValueText", "210/210", 20);
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

            // Currency Container with AnimatedStatDisplay - larger width to prevent overlap
            GameObject currencyContainer = new GameObject("CurrencyContainer");
            currencyContainer.transform.SetParent(statsContainer.transform, false);
            RectTransform currencyContainerRect = currencyContainer.AddComponent<RectTransform>();
            currencyContainerRect.sizeDelta = new Vector2(200, 40);
            HorizontalLayoutGroup currencyLayout = currencyContainer.AddComponent<HorizontalLayoutGroup>();
            currencyLayout.spacing = 8;
            currencyLayout.childAlignment = TextAnchor.MiddleCenter;
            var currContainerLayout = currencyContainer.AddComponent<LayoutElement>();
            currContainerLayout.preferredWidth = 200;
            currContainerLayout.preferredHeight = 40;

            // Currency Icon - larger size for better visibility
            Color soulCyan = new Color(0f, 0.83f, 0.89f);
            GameObject currencyIcon = CreateIconImage(currencyContainer, "CurrencyIcon", layerLabConfig?.ItemIconCurrency, new Vector2(32, 32), soulCyan, "\u25C6", 24);
            var currIconLayout = currencyIcon.AddComponent<LayoutElement>();
            currIconLayout.preferredWidth = 32;
            currIconLayout.preferredHeight = 32;

            // Currency AnimatedStatDisplay (SingleValue mode for just number)
            GameObject currDisplayObj = new GameObject("CurrencyDisplay");
            currDisplayObj.transform.SetParent(currencyContainer.transform, false);
            var currDisplay = currDisplayObj.AddComponent<AnimatedStatDisplay>();
            var currDisplayLayout = currDisplayObj.AddComponent<LayoutElement>();
            currDisplayLayout.preferredWidth = 150;
            currDisplayLayout.preferredHeight = 32;

            // Create text for currency display (starts at 0) - larger font
            GameObject currencyText = CreateText(currDisplayObj, "ValueText", "0", 20);
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
            layout.padding = new RectOffset(20, 20, 4, 4);
            layout.spacing = 48; // More spacing between legend items
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;
            layout.childControlWidth = false;
            layout.childControlHeight = false;

            // Load LayerLab config for map legend icons
            var layerLabConfig = LoadLayerLabConfig();

            // Node type legend items - sprite, fallback text, label, color
            var nodeTypes = new (Sprite sprite, string fallback, string label, Color color)[]
            {
                (layerLabConfig?.MapIconCombat, "\u2694\uFE0F", "Combat", new Color(0.77f, 0.12f, 0.23f)),
                (layerLabConfig?.MapIconElite, "\U0001F480", "Elite", new Color(1f, 0.27f, 0.27f)),
                (layerLabConfig?.MapIconShop, "\U0001F6D2", "Shop", new Color(0.83f, 0.69f, 0.22f)),
                (layerLabConfig?.MapIconEcho, "\u2753", "Echo", new Color(0.2f, 0.5f, 0.9f)), // Blue to match node color
                (layerLabConfig?.MapIconSanctuary, "\U0001F56F\uFE0F", "Sanctuary", new Color(0.18f, 0.8f, 0.44f)),
                (layerLabConfig?.MapIconTreasure, "\U0001F48E", "Treasure", new Color(0.83f, 0.69f, 0.22f)),
                (layerLabConfig?.MapIconBoss, "\U0001F479", "Boss", new Color(1f, 0.27f, 0.27f)),
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

            // Add LayoutElement to control item size
            var itemLayout = item.AddComponent<LayoutElement>();
            itemLayout.preferredWidth = 120; // Icon (20) + spacing (6) + label (100) - some margin
            itemLayout.preferredHeight = 24;

            HorizontalLayoutGroup layout = item.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = 6;
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;
            layout.childControlWidth = false;
            layout.childControlHeight = false;

            // Icon - use WHITE color so sprite displays with its native colors
            // Only use semantic color for fallback text
            Color iconColor = iconSprite != null ? Color.white : color;
            GameObject iconObj = CreateIconImage(item, "Icon", iconSprite, new Vector2(20, 20), iconColor, fallbackIcon, 14);
            var iconLayout = iconObj.AddComponent<LayoutElement>();
            iconLayout.minWidth = 20;
            iconLayout.preferredWidth = 20;
            iconLayout.minHeight = 20;
            iconLayout.preferredHeight = 20;

            // Label with fixed width 100 and font size 16
            GameObject labelObj = CreateText(item, "Label", label, 16);
            var labelText = labelObj.GetComponent<TMP_Text>();
            labelText.color = new Color(0.75f, 0.75f, 0.75f);
            labelText.alignment = TextAlignmentOptions.Left; // Ensure left alignment for labels
            // Override CreateText's default sizeDelta (200, 50) to match our desired width
            labelObj.GetComponent<RectTransform>().sizeDelta = new Vector2(100, 24);
            var labelLayout = labelObj.AddComponent<LayoutElement>();
            labelLayout.minWidth = 100;
            labelLayout.preferredWidth = 100;
            labelLayout.flexibleWidth = 0; // Prevent stretching
        }

        /// <summary>
        /// Creates the End Turn execution button using LayerLab Button_01_small styling.
        /// </summary>
        private static GameObject CreateExecutionButton(GameObject parent)
        {
            var layerLabConfig = LoadLayerLabConfig();

            // Use LayerLab Button_01_small if available
            if (layerLabConfig != null && layerLabConfig.HasAllButton01SmallSprites())
            {
                // Create LayerLab styled button with "END TURN" text
                var buttonObj = LayerLabButtonBuilder.CreateButton01Small(parent, "ExecutionButton", "END TURN", "purple");

                // Position on right side of parent
                RectTransform rect = buttonObj.GetComponent<RectTransform>();
                rect.anchorMin = new Vector2(1f, 0.5f);
                rect.anchorMax = new Vector2(1f, 0.5f);
                rect.pivot = new Vector2(1f, 0.5f);
                rect.anchoredPosition = new Vector2(-16, 40); // 16px from right edge, above center
                rect.sizeDelta = new Vector2(160, 72); // Scaled size for visibility

                // Get button component
                Button btn = buttonObj.GetComponent<Button>();
                Image bg = buttonObj.GetComponent<Image>();

                // Add ExecutionButton component and wire references
                var execButton = buttonObj.AddComponent<ExecutionButton>();
                var so = new SerializedObject(execButton);
                so.FindProperty("_buttonBackground").objectReferenceValue = bg;
                so.FindProperty("_button").objectReferenceValue = btn;
                so.ApplyModifiedPropertiesWithoutUndo();

                return buttonObj;
            }

            // Fallback to styled button without LayerLab
            GameObject btnObj = new GameObject("ExecutionButton");
            btnObj.transform.SetParent(parent.transform, false);

            // Rectangle button positioned right side
            RectTransform rectFallback = btnObj.AddComponent<RectTransform>();
            rectFallback.anchorMin = new Vector2(1f, 0.5f);
            rectFallback.anchorMax = new Vector2(1f, 0.5f);
            rectFallback.pivot = new Vector2(1f, 0.5f);
            rectFallback.anchoredPosition = new Vector2(-16, 40);
            rectFallback.sizeDelta = new Vector2(160, 72);

            // Glow ring
            GameObject glowRing = new GameObject("GlowRing");
            glowRing.transform.SetParent(btnObj.transform, false);
            RectTransform glowRect = glowRing.AddComponent<RectTransform>();
            glowRect.anchorMin = Vector2.zero;
            glowRect.anchorMax = Vector2.one;
            glowRect.sizeDelta = new Vector2(8, 8);
            Image glowImg = glowRing.AddComponent<Image>();
            glowImg.color = new Color(0.6f, 0.4f, 0.85f, 0.4f); // Purple glow

            // Main button background
            GameObject bgObj = new GameObject("Background");
            bgObj.transform.SetParent(btnObj.transform, false);
            RectTransform bgRect = bgObj.AddComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.sizeDelta = Vector2.zero;

            Image fallbackBg = bgObj.AddComponent<Image>();
            fallbackBg.color = new Color(0.4f, 0.25f, 0.6f, 0.95f); // Purple

            Button fallbackBtn = bgObj.AddComponent<Button>();
            fallbackBtn.targetGraphic = fallbackBg;

            // Button text - "END TURN"
            GameObject textObj = CreateText(bgObj, "Text", "END TURN", 24);
            RectTransform textRect = textObj.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.sizeDelta = Vector2.zero;
            var tmp = textObj.GetComponent<TextMeshProUGUI>();
            tmp.color = Color.white;
            tmp.fontStyle = TMPro.FontStyles.Bold;
            tmp.alignment = TextAlignmentOptions.Center;

            // Add ExecutionButton component and wire references
            var fallbackExecButton = btnObj.AddComponent<ExecutionButton>();
            var fallbackSO = new SerializedObject(fallbackExecButton);
            fallbackSO.FindProperty("_buttonBackground").objectReferenceValue = fallbackBg;
            fallbackSO.FindProperty("_button").objectReferenceValue = fallbackBtn;
            fallbackSO.ApplyModifiedPropertiesWithoutUndo();

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
        /// Uses prefab instantiation if ConfirmationDialog.prefab is available.
        /// </summary>
        private static GameObject CreateConfirmationDialog(GameObject parent)
        {
            // Try to instantiate from prefab first (preferred approach)
            var runtimePrefabConfig = LoadRuntimePrefabConfig();
            if (runtimePrefabConfig != null && runtimePrefabConfig.ConfirmationDialogPrefab != null)
            {
                var prefabInstance = PrefabUtility.InstantiatePrefab(runtimePrefabConfig.ConfirmationDialogPrefab) as GameObject;
                if (prefabInstance != null)
                {
                    prefabInstance.transform.SetParent(parent.transform, false);

                    // Ensure RectTransform stretches to fill parent
                    var rect = prefabInstance.GetComponent<RectTransform>();
                    if (rect != null)
                    {
                        rect.anchorMin = Vector2.zero;
                        rect.anchorMax = Vector2.one;
                        rect.sizeDelta = Vector2.zero;
                        rect.anchoredPosition = Vector2.zero;
                    }

                    prefabInstance.SetActive(false);
                    Debug.Log("[ProductionSceneSetupGenerator] Instantiated ConfirmationDialog from prefab");
                    return prefabInstance;
                }
            }

            // Fallback: Create manually if prefab not available
            Debug.LogWarning("[ProductionSceneSetupGenerator] ConfirmationDialog.prefab not found, creating manually. Run 'HNR > 2. Prefabs > UI > Runtime Prefabs > 1. ConfirmationDialog' first.");

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

            // Cancel button - LayerLab gray style
            var confirmDialogConfig = LoadLayerLabConfig();
            GameObject cancelBtn;
            GameObject cancelLabel;
            if (confirmDialogConfig != null && confirmDialogConfig.HasAllButton01SmallSprites())
            {
                cancelBtn = LayerLabButtonBuilder.CreateButton01Small(buttonContainer, "CancelButton", "Cancel", "gray");
                var cancelLayout = cancelBtn.AddComponent<LayoutElement>();
                cancelLayout.flexibleWidth = 1;
                cancelLabel = cancelBtn.transform.Find("Text")?.gameObject;
            }
            else
            {
                cancelBtn = new GameObject("CancelButton");
                cancelBtn.transform.SetParent(buttonContainer.transform, false);
                Image cancelBg = cancelBtn.AddComponent<Image>();
                cancelBg.color = new Color(0.15f, 0.12f, 0.2f, 0.9f);
                Button cancelButton = cancelBtn.AddComponent<Button>();
                cancelButton.targetGraphic = cancelBg;
                var cancelLayout = cancelBtn.AddComponent<LayoutElement>();
                cancelLayout.flexibleWidth = 1;

                cancelLabel = CreateText(cancelBtn, "Label", "Cancel", 16);
                RectTransform cancelLabelRect = cancelLabel.GetComponent<RectTransform>();
                cancelLabelRect.anchorMin = Vector2.zero;
                cancelLabelRect.anchorMax = Vector2.one;
                cancelLabelRect.sizeDelta = Vector2.zero;
                cancelLabel.GetComponent<TMP_Text>().color = new Color(0f, 0.83f, 0.89f);
            }

            // Confirm button - LayerLab red style
            GameObject confirmBtn;
            GameObject confirmLabel;
            if (confirmDialogConfig != null && confirmDialogConfig.HasAllButton01SmallSprites())
            {
                confirmBtn = LayerLabButtonBuilder.CreateButton01Small(buttonContainer, "ConfirmButton", "Confirm", "red");
                var confirmLayout = confirmBtn.AddComponent<LayoutElement>();
                confirmLayout.flexibleWidth = 1;
                confirmLabel = confirmBtn.transform.Find("Text")?.gameObject;
            }
            else
            {
                confirmBtn = new GameObject("ConfirmButton");
                confirmBtn.transform.SetParent(buttonContainer.transform, false);
                Image confirmBg = confirmBtn.AddComponent<Image>();
                confirmBg.color = new Color(1f, 0.27f, 0.27f, 0.9f);
                Button confirmButton = confirmBtn.AddComponent<Button>();
                confirmButton.targetGraphic = confirmBg;
                var confirmLayout = confirmBtn.AddComponent<LayoutElement>();
                confirmLayout.flexibleWidth = 1;

                confirmLabel = CreateText(confirmBtn, "Label", "Confirm", 16);
                RectTransform confirmLabelRect = confirmLabel.GetComponent<RectTransform>();
                confirmLabelRect.anchorMin = Vector2.zero;
                confirmLabelRect.anchorMax = Vector2.one;
                confirmLabelRect.sizeDelta = Vector2.zero;
                confirmLabel.GetComponent<TMP_Text>().fontStyle = TMPro.FontStyles.Bold;
            }

            // Add ConfirmationDialog component and wire references
            var dialog = overlay.AddComponent<ConfirmationDialog>();
            var so = new SerializedObject(dialog);
            so.FindProperty("_overlay").objectReferenceValue = canvasGroup;
            so.FindProperty("_backgroundPanel").objectReferenceValue = bgImage;
            so.FindProperty("_dialogPanel").objectReferenceValue = dialogRect;
            so.FindProperty("_titleText").objectReferenceValue = title.GetComponent<TMP_Text>();
            so.FindProperty("_messageText").objectReferenceValue = message.GetComponent<TMP_Text>();
            so.FindProperty("_confirmButton").objectReferenceValue = confirmBtn.GetComponent<Button>();
            so.FindProperty("_confirmButtonText").objectReferenceValue = confirmLabel?.GetComponent<TMP_Text>();
            so.FindProperty("_cancelButton").objectReferenceValue = cancelBtn.GetComponent<Button>();
            so.FindProperty("_cancelButtonText").objectReferenceValue = cancelLabel?.GetComponent<TMP_Text>();
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
            // Load Layer Lab bubble frame sprite
            var tooltipSprite = AssetDatabase.LoadAssetAtPath<Sprite>(
                "Assets/ThirdParty/Layer Lab/GUI Pro-FantasyHero/ResourcesData/Sptites/Components/Frame/BubbleFrame_01_Bg.png");
            if (tooltipSprite != null)
            {
                tooltipBg.sprite = tooltipSprite;
                tooltipBg.type = Image.Type.Sliced;
            }
            // Color #9C3DBC (purple)
            tooltipBg.color = new Color(0.612f, 0.239f, 0.737f, 1f);

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

            // Wire relic icon prefab if available
            var runtimePrefabConfig = LoadRuntimePrefabConfig();
            if (runtimePrefabConfig != null && runtimePrefabConfig.RelicDisplayIconPrefab != null)
            {
                so.FindProperty("_relicIconPrefab").objectReferenceValue = runtimePrefabConfig.RelicDisplayIconPrefab;
                Debug.Log("[ProductionSceneSetupGenerator] Wired RelicDisplayIcon.prefab to RelicDisplayBar._relicIconPrefab");
            }

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

            // Cancel button - LayerLab gray style
            var deckViewerConfig = LoadLayerLabConfig();
            GameObject cancelBtn;
            GameObject cancelText;
            if (deckViewerConfig != null && deckViewerConfig.HasAllButton01SmallSprites())
            {
                cancelBtn = LayerLabButtonBuilder.CreateButton01Small(footer, "CancelButton", "Cancel", "gray");
                var cancelLayout = cancelBtn.AddComponent<LayoutElement>();
                cancelLayout.preferredWidth = 120;
                cancelLayout.preferredHeight = 40;
                cancelText = cancelBtn.transform.Find("Text")?.gameObject;
            }
            else
            {
                cancelBtn = new GameObject("CancelButton");
                cancelBtn.transform.SetParent(footer.transform, false);
                Image cancelBg = cancelBtn.AddComponent<Image>();
                cancelBg.color = new Color(0.15f, 0.12f, 0.2f, 0.9f);
                Button cancelButton = cancelBtn.AddComponent<Button>();
                cancelButton.targetGraphic = cancelBg;
                var cancelLayout = cancelBtn.AddComponent<LayoutElement>();
                cancelLayout.preferredWidth = 120;
                cancelLayout.preferredHeight = 40;

                cancelText = CreateText(cancelBtn, "Text", "Cancel", 16);
                RectTransform cancelTextRect = cancelText.GetComponent<RectTransform>();
                cancelTextRect.anchorMin = Vector2.zero;
                cancelTextRect.anchorMax = Vector2.one;
                cancelTextRect.sizeDelta = Vector2.zero;
                cancelText.GetComponent<TMP_Text>().color = new Color(0f, 0.83f, 0.89f);
            }

            // Confirm button - LayerLab purple style
            GameObject confirmBtn;
            GameObject confirmText;
            if (deckViewerConfig != null && deckViewerConfig.HasAllButton01SmallSprites())
            {
                confirmBtn = LayerLabButtonBuilder.CreateButton01Small(footer, "ConfirmButton", "Close", "purple");
                var confirmLayout = confirmBtn.AddComponent<LayoutElement>();
                confirmLayout.preferredWidth = 120;
                confirmLayout.preferredHeight = 40;
                confirmText = confirmBtn.transform.Find("Text")?.gameObject;
            }
            else
            {
                confirmBtn = new GameObject("ConfirmButton");
                confirmBtn.transform.SetParent(footer.transform, false);
                Image confirmBg = confirmBtn.AddComponent<Image>();
                confirmBg.color = new Color(0.77f, 0.12f, 0.23f, 0.9f);
                Button confirmButton = confirmBtn.AddComponent<Button>();
                confirmButton.targetGraphic = confirmBg;
                var confirmLayout = confirmBtn.AddComponent<LayoutElement>();
                confirmLayout.preferredWidth = 120;
                confirmLayout.preferredHeight = 40;

                confirmText = CreateText(confirmBtn, "Text", "Close", 16);
                RectTransform confirmTextRect = confirmText.GetComponent<RectTransform>();
                confirmTextRect.anchorMin = Vector2.zero;
                confirmTextRect.anchorMax = Vector2.one;
                confirmTextRect.sizeDelta = Vector2.zero;
                confirmText.GetComponent<TMP_Text>().fontStyle = TMPro.FontStyles.Bold;
            }

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
            so.FindProperty("_confirmButton").objectReferenceValue = confirmBtn.GetComponent<Button>();
            so.FindProperty("_confirmButtonText").objectReferenceValue = confirmText?.GetComponent<TMP_Text>();
            so.FindProperty("_cancelButton").objectReferenceValue = cancelBtn.GetComponent<Button>();
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

        /// <summary>
        /// Creates the RelicShopOverlay for viewing and purchasing relics in shop.
        /// </summary>
        private static GameObject CreateRelicShopOverlay(GameObject parent)
        {
            GameObject modal = new GameObject("RelicShopOverlay");
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

            // Load LayerLab config for frame styling
            var relicLayerLabConfig = LoadLayerLabConfig();

            // === Content Container (centered panel) with LayerLab styling ===
            GameObject contentPanel = new GameObject("ContentPanel");
            contentPanel.transform.SetParent(modalPanel.transform, false);
            RectTransform contentRect = contentPanel.AddComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0.15f, 0.12f);
            contentRect.anchorMax = new Vector2(0.85f, 0.88f);
            contentRect.sizeDelta = Vector2.zero;

            // Apply LayerLab ListFrame to content panel
            Image contentBg = contentPanel.AddComponent<Image>();
            if (relicLayerLabConfig != null && relicLayerLabConfig.ListFrameBg != null)
            {
                contentBg.sprite = relicLayerLabConfig.ListFrameBg;
                contentBg.type = Image.Type.Sliced;
                contentBg.color = new Color(0.25f, 0.2f, 0.35f, 0.98f); // Purple tint

                // Add ListFrame border
                if (relicLayerLabConfig.ListFrameBorder != null)
                {
                    GameObject contentBorder = new GameObject("Border");
                    contentBorder.transform.SetParent(contentPanel.transform, false);
                    RectTransform borderRect = contentBorder.AddComponent<RectTransform>();
                    borderRect.anchorMin = Vector2.zero;
                    borderRect.anchorMax = Vector2.one;
                    borderRect.sizeDelta = Vector2.zero;
                    Image borderImg = contentBorder.AddComponent<Image>();
                    borderImg.sprite = relicLayerLabConfig.ListFrameBorder;
                    borderImg.type = Image.Type.Sliced;
                    borderImg.color = new Color(0.6f, 0.5f, 0.8f, 0.8f);
                    borderImg.raycastTarget = false;
                }
            }
            else
            {
                contentBg.color = new Color(0.08f, 0.06f, 0.12f, 0.98f);
            }

            // === Header with LayerLab BaseFrame styling ===
            GameObject header = new GameObject("Header");
            header.transform.SetParent(contentPanel.transform, false);
            RectTransform headerRect = header.AddComponent<RectTransform>();
            headerRect.anchorMin = new Vector2(0, 0.88f);
            headerRect.anchorMax = new Vector2(1, 1);
            headerRect.sizeDelta = Vector2.zero;

            Image headerBg = header.AddComponent<Image>();
            if (relicLayerLabConfig != null && relicLayerLabConfig.BaseFrameBorderRectH60 != null)
            {
                headerBg.sprite = relicLayerLabConfig.BaseFrameBorderRectH60;
                headerBg.type = Image.Type.Sliced;
                headerBg.color = new Color(0.35f, 0.28f, 0.5f, 0.95f); // Purple tint
            }
            else
            {
                headerBg.color = new Color(0.1f, 0.08f, 0.15f, 0.95f);
            }

            // Title
            GameObject title = CreateText(header, "Title", "RELICS", 28);
            RectTransform titleRect = title.GetComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0, 0);
            titleRect.anchorMax = new Vector2(0.5f, 1);
            titleRect.offsetMin = new Vector2(20, 0);
            titleRect.offsetMax = new Vector2(0, 0);
            var titleTmp = title.GetComponent<TMP_Text>();
            titleTmp.alignment = TextAlignmentOptions.Left;
            titleTmp.fontStyle = TMPro.FontStyles.Bold;
            titleTmp.color = new Color(0.83f, 0.69f, 0.22f); // Soul gold

            // Void Shards display
            GameObject shardsDisplay = CreateText(header, "VoidShardsText", "0", 20);
            RectTransform shardsRect = shardsDisplay.GetComponent<RectTransform>();
            shardsRect.anchorMin = new Vector2(0.7f, 0);
            shardsRect.anchorMax = new Vector2(1, 1);
            shardsRect.offsetMin = new Vector2(0, 0);
            shardsRect.offsetMax = new Vector2(-20, 0);
            var shardsTmp = shardsDisplay.GetComponent<TMP_Text>();
            shardsTmp.alignment = TextAlignmentOptions.Right;
            shardsTmp.color = new Color(0f, 0.83f, 0.89f); // Cyan

            // === Main Content Area (horizontal layout) ===
            GameObject mainArea = new GameObject("MainArea");
            mainArea.transform.SetParent(contentPanel.transform, false);
            RectTransform mainRect = mainArea.AddComponent<RectTransform>();
            mainRect.anchorMin = new Vector2(0, 0.12f);
            mainRect.anchorMax = new Vector2(1, 0.86f);
            mainRect.sizeDelta = Vector2.zero;

            // === Relic Grid (left side) with LayerLab PanelFrame styling ===
            GameObject relicGridArea = new GameObject("RelicGridArea");
            relicGridArea.transform.SetParent(mainArea.transform, false);
            RectTransform gridAreaRect = relicGridArea.AddComponent<RectTransform>();
            gridAreaRect.anchorMin = new Vector2(0, 0);
            gridAreaRect.anchorMax = new Vector2(0.55f, 1);
            gridAreaRect.sizeDelta = Vector2.zero;

            ScrollRect scroll = relicGridArea.AddComponent<ScrollRect>();
            scroll.horizontal = false;
            scroll.vertical = true;
            scroll.scrollSensitivity = 20f;

            Image scrollMask = relicGridArea.AddComponent<Image>();
            if (relicLayerLabConfig != null && relicLayerLabConfig.PanelFrameBg != null)
            {
                scrollMask.sprite = relicLayerLabConfig.PanelFrameBg;
                scrollMask.type = Image.Type.Sliced;
                scrollMask.color = new Color(0.2f, 0.15f, 0.3f, 0.7f); // Purple tint with transparency
            }
            else
            {
                scrollMask.color = new Color(0.05f, 0.03f, 0.08f, 0.5f);
            }
            relicGridArea.AddComponent<Mask>().showMaskGraphic = true;

            // Viewport
            GameObject viewport = new GameObject("Viewport");
            viewport.transform.SetParent(relicGridArea.transform, false);
            RectTransform viewportRect = viewport.AddComponent<RectTransform>();
            viewportRect.anchorMin = Vector2.zero;
            viewportRect.anchorMax = Vector2.one;
            viewportRect.sizeDelta = Vector2.zero;

            // Relic Container (GridLayoutGroup)
            GameObject relicContainer = new GameObject("RelicContainer");
            relicContainer.transform.SetParent(viewport.transform, false);
            RectTransform relicContainerRect = relicContainer.AddComponent<RectTransform>();
            relicContainerRect.anchorMin = new Vector2(0, 1);
            relicContainerRect.anchorMax = new Vector2(1, 1);
            relicContainerRect.pivot = new Vector2(0.5f, 1);
            relicContainerRect.sizeDelta = new Vector2(0, 200);

            GridLayoutGroup grid = relicContainer.AddComponent<GridLayoutGroup>();
            grid.cellSize = new Vector2(100, 140);
            grid.spacing = new Vector2(15, 15);
            grid.startCorner = GridLayoutGroup.Corner.UpperLeft;
            grid.startAxis = GridLayoutGroup.Axis.Horizontal;
            grid.childAlignment = TextAnchor.UpperLeft;
            grid.padding = new RectOffset(15, 15, 15, 15);

            ContentSizeFitter fitter = relicContainer.AddComponent<ContentSizeFitter>();
            fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            scroll.content = relicContainerRect;
            scroll.viewport = viewportRect;

            // === Details Panel (right side) with LayerLab PanelFrame styling ===
            GameObject detailsPanel = new GameObject("DetailsPanel");
            detailsPanel.transform.SetParent(mainArea.transform, false);
            RectTransform detailsRect = detailsPanel.AddComponent<RectTransform>();
            detailsRect.anchorMin = new Vector2(0.58f, 0);
            detailsRect.anchorMax = new Vector2(1, 1);
            detailsRect.sizeDelta = Vector2.zero;

            Image detailsBg = detailsPanel.AddComponent<Image>();
            if (relicLayerLabConfig != null && relicLayerLabConfig.PanelFrameBg != null)
            {
                detailsBg.sprite = relicLayerLabConfig.PanelFrameBg;
                detailsBg.type = Image.Type.Sliced;
                detailsBg.color = new Color(0.22f, 0.18f, 0.32f, 0.95f); // Purple tint

                // Add border
                if (relicLayerLabConfig.PanelFrameBorder != null)
                {
                    GameObject detailsBorder = new GameObject("Border");
                    detailsBorder.transform.SetParent(detailsPanel.transform, false);
                    RectTransform borderRect = detailsBorder.AddComponent<RectTransform>();
                    borderRect.anchorMin = Vector2.zero;
                    borderRect.anchorMax = Vector2.one;
                    borderRect.sizeDelta = Vector2.zero;
                    Image borderImg = detailsBorder.AddComponent<Image>();
                    borderImg.sprite = relicLayerLabConfig.PanelFrameBorder;
                    borderImg.type = Image.Type.Sliced;
                    borderImg.color = new Color(0.55f, 0.45f, 0.75f, 0.8f);
                    borderImg.raycastTarget = false;
                }

                // Add bottom deco
                if (relicLayerLabConfig.PanelFrameDeco != null)
                {
                    GameObject detailsDeco = new GameObject("Deco");
                    detailsDeco.transform.SetParent(detailsPanel.transform, false);
                    RectTransform decoRect = detailsDeco.AddComponent<RectTransform>();
                    decoRect.anchorMin = new Vector2(0, 0);
                    decoRect.anchorMax = new Vector2(1, 0.15f);
                    decoRect.sizeDelta = Vector2.zero;
                    Image decoImg = detailsDeco.AddComponent<Image>();
                    decoImg.sprite = relicLayerLabConfig.PanelFrameDeco;
                    decoImg.type = Image.Type.Sliced;
                    decoImg.color = new Color(0.5f, 0.4f, 0.7f, 0.6f);
                    decoImg.raycastTarget = false;
                    decoImg.preserveAspect = true;
                }
            }
            else
            {
                detailsBg.color = new Color(0.1f, 0.08f, 0.12f, 0.9f);
            }

            VerticalLayoutGroup detailsLayout = detailsPanel.AddComponent<VerticalLayoutGroup>();
            detailsLayout.padding = new RectOffset(15, 15, 15, 15);
            detailsLayout.spacing = 10;
            detailsLayout.childAlignment = TextAnchor.UpperCenter;
            detailsLayout.childForceExpandWidth = true;
            detailsLayout.childForceExpandHeight = false;

            // Selected Relic Icon
            GameObject iconContainer = new GameObject("IconContainer");
            iconContainer.transform.SetParent(detailsPanel.transform, false);
            var iconContainerLayout = iconContainer.AddComponent<LayoutElement>();
            iconContainerLayout.preferredHeight = 100;

            GameObject selectedIcon = new GameObject("SelectedRelicIcon");
            selectedIcon.transform.SetParent(iconContainer.transform, false);
            RectTransform iconRect = selectedIcon.AddComponent<RectTransform>();
            iconRect.anchorMin = new Vector2(0.5f, 0.5f);
            iconRect.anchorMax = new Vector2(0.5f, 0.5f);
            iconRect.sizeDelta = new Vector2(96, 96);
            Image selectedIconImg = selectedIcon.AddComponent<Image>();
            selectedIconImg.color = Color.white;

            // Selected Relic Name
            GameObject relicNameObj = CreateText(detailsPanel, "SelectedRelicName", "Select a Relic", 20);
            relicNameObj.GetComponent<TMP_Text>().fontStyle = TMPro.FontStyles.Bold;
            relicNameObj.GetComponent<TMP_Text>().color = new Color(0.83f, 0.69f, 0.22f);
            var nameTmp = relicNameObj.GetComponent<TMP_Text>();
            var nameLayout = relicNameObj.AddComponent<LayoutElement>();
            nameLayout.preferredHeight = 30;

            // Selected Relic Description
            GameObject relicDescObj = CreateText(detailsPanel, "SelectedRelicDescription", "", 14);
            var descTmp = relicDescObj.GetComponent<TMP_Text>();
            descTmp.color = new Color(0.8f, 0.8f, 0.8f);
            descTmp.alignment = TextAlignmentOptions.Top;
            var descLayout = relicDescObj.AddComponent<LayoutElement>();
            descLayout.preferredHeight = 80;
            descLayout.flexibleHeight = 1;

            // Selected Relic Price
            GameObject relicPriceObj = CreateText(detailsPanel, "SelectedRelicPrice", "", 18);
            var priceTmp = relicPriceObj.GetComponent<TMP_Text>();
            priceTmp.color = new Color(0f, 0.83f, 0.89f);
            var priceLayout = relicPriceObj.AddComponent<LayoutElement>();
            priceLayout.preferredHeight = 30;

            // Purchase Button - LayerLab green style (reuse relicLayerLabConfig from above)
            GameObject purchaseBtn;
            if (relicLayerLabConfig != null && relicLayerLabConfig.HasAllButton01SmallSprites())
            {
                purchaseBtn = LayerLabButtonBuilder.CreateButton01Small(detailsPanel, "PurchaseButton", "Select Relic", "green");
                var purchaseLayout = purchaseBtn.AddComponent<LayoutElement>();
                purchaseLayout.preferredHeight = 50;
            }
            else
            {
                purchaseBtn = new GameObject("PurchaseButton");
                purchaseBtn.transform.SetParent(detailsPanel.transform, false);
                Image purchaseBg = purchaseBtn.AddComponent<Image>();
                purchaseBg.color = new Color(0.2f, 0.5f, 0.3f, 0.9f);
                Button purchaseButton = purchaseBtn.AddComponent<Button>();
                purchaseButton.targetGraphic = purchaseBg;
                var purchaseLayout = purchaseBtn.AddComponent<LayoutElement>();
                purchaseLayout.preferredHeight = 50;

                GameObject purchaseText = CreateText(purchaseBtn, "Text", "Select Relic", 16);
                RectTransform purchaseTextRect = purchaseText.GetComponent<RectTransform>();
                purchaseTextRect.anchorMin = Vector2.zero;
                purchaseTextRect.anchorMax = Vector2.one;
                purchaseTextRect.sizeDelta = Vector2.zero;
                var purchaseTextTmp = purchaseText.GetComponent<TMP_Text>();
                purchaseTextTmp.fontStyle = TMPro.FontStyles.Bold;
            }

            detailsPanel.SetActive(false); // Hidden until relic selected

            // === Footer / Close Button ===
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

            // Spacer
            GameObject spacer = new GameObject("Spacer");
            spacer.transform.SetParent(footer.transform, false);
            var spacerLayout = spacer.AddComponent<LayoutElement>();
            spacerLayout.flexibleWidth = 1;

            // Close button - LayerLab gray style
            GameObject closeBtn;
            if (relicLayerLabConfig != null && relicLayerLabConfig.HasAllButton01SmallSprites())
            {
                closeBtn = LayerLabButtonBuilder.CreateButton01Small(footer, "CloseButton", "Close", "gray");
                var closeLayout = closeBtn.AddComponent<LayoutElement>();
                closeLayout.preferredWidth = 120;
                closeLayout.preferredHeight = 40;
            }
            else
            {
                closeBtn = new GameObject("CloseButton");
                closeBtn.transform.SetParent(footer.transform, false);
                Image closeBg = closeBtn.AddComponent<Image>();
                closeBg.color = new Color(0.15f, 0.12f, 0.2f, 0.9f);
                Button closeButton = closeBtn.AddComponent<Button>();
                closeButton.targetGraphic = closeBg;
                var closeLayout = closeBtn.AddComponent<LayoutElement>();
                closeLayout.preferredWidth = 120;
                closeLayout.preferredHeight = 40;

                GameObject closeText = CreateText(closeBtn, "Text", "Close", 16);
                RectTransform closeTextRect = closeText.GetComponent<RectTransform>();
                closeTextRect.anchorMin = Vector2.zero;
                closeTextRect.anchorMax = Vector2.one;
                closeTextRect.sizeDelta = Vector2.zero;
                closeText.GetComponent<TMP_Text>().color = new Color(0f, 0.83f, 0.89f);
            }

            // === Add RelicShopOverlay component and wire references ===
            var relicOverlay = modal.AddComponent<RelicShopOverlay>();
            var so = new SerializedObject(relicOverlay);
            so.FindProperty("_modalPanel").objectReferenceValue = modalPanel;
            so.FindProperty("_canvasGroup").objectReferenceValue = canvasGroup;
            so.FindProperty("_backgroundOverlay").objectReferenceValue = bgOverlay;
            so.FindProperty("_titleText").objectReferenceValue = titleTmp;
            so.FindProperty("_voidShardsText").objectReferenceValue = shardsTmp;
            so.FindProperty("_relicContainer").objectReferenceValue = relicContainer.transform;
            so.FindProperty("_scrollRect").objectReferenceValue = scroll;
            so.FindProperty("_detailsPanel").objectReferenceValue = detailsPanel;
            so.FindProperty("_selectedRelicIcon").objectReferenceValue = selectedIconImg;
            so.FindProperty("_selectedRelicName").objectReferenceValue = nameTmp;
            so.FindProperty("_selectedRelicDescription").objectReferenceValue = descTmp;
            so.FindProperty("_selectedRelicPrice").objectReferenceValue = priceTmp;
            so.FindProperty("_purchaseButton").objectReferenceValue = purchaseBtn.GetComponent<Button>();
            so.FindProperty("_purchaseButtonText").objectReferenceValue = purchaseBtn.transform.Find("Text")?.GetComponent<TMP_Text>();
            so.FindProperty("_closeButton").objectReferenceValue = closeBtn.GetComponent<Button>();
            so.FindProperty("_fadeInDuration").floatValue = 0.3f;
            so.FindProperty("_fadeOutDuration").floatValue = 0.2f;
            so.FindProperty("_purchaseButtonActiveColor").colorValue = new Color(0.2f, 0.5f, 0.3f, 1f);
            so.FindProperty("_purchaseButtonDisabledColor").colorValue = new Color(0.3f, 0.3f, 0.3f, 0.6f);

            // Wire relic slot prefab if available
            var runtimePrefabConfig = LoadRuntimePrefabConfig();
            if (runtimePrefabConfig != null && runtimePrefabConfig.RelicShopSlotPrefab != null)
            {
                so.FindProperty("_relicSlotPrefab").objectReferenceValue = runtimePrefabConfig.RelicShopSlotPrefab;
                Debug.Log("[ProductionSceneSetupGenerator] Wired RelicShopSlot.prefab to RelicShopOverlay._relicSlotPrefab");
            }

            so.ApplyModifiedPropertiesWithoutUndo();

            Debug.Log("[ProductionSceneSetupGenerator] Created RelicShopOverlay");
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
            CreateSpriteBackground(canvas, "Missions", new Color(0.08f, 0.08f, 0.12f));
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

            // Back button - use LayerLab style
            var backButton = CreateBackButton(header, "BackButton");
            var backButtonRect = backButton.GetComponent<RectTransform>();
            backButtonRect.anchorMin = new Vector2(0, 0.5f);
            backButtonRect.anchorMax = new Vector2(0, 0.5f);
            backButtonRect.pivot = new Vector2(0, 0.5f);
            backButtonRect.anchoredPosition = Vector2.zero;

            // Title
            var title = CreateSimpleTextObject("Title", header.transform, "Missions");
            var titleRect = title.GetComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0, 0.5f);
            titleRect.anchorMax = new Vector2(0, 0.5f);
            titleRect.pivot = new Vector2(0, 0.5f);
            titleRect.sizeDelta = new Vector2(200, 50);
            titleRect.anchoredPosition = new Vector2(100, 0);
            var titleText = title.GetComponent<TMP_Text>();
            titleText.fontSize = 32;
            titleText.fontStyle = FontStyles.Bold;

            // Settings button - use LayerLab style
            var layerLabConfig = LoadLayerLabConfig();
            var settingsButton = CreateLayerLabIconButton(header, "SettingsButton", layerLabConfig?.IconSettings);
            var settingsButtonRect = settingsButton.GetComponent<RectTransform>();
            settingsButtonRect.anchorMin = new Vector2(1, 0.5f);
            settingsButtonRect.anchorMax = new Vector2(1, 0.5f);
            settingsButtonRect.pivot = new Vector2(1, 0.5f);
            settingsButtonRect.anchoredPosition = new Vector2(0, 0);

            // Content area
            var content = CreateSimpleUIObject("Content", missionsScreenObj.transform);
            var contentRect = content.GetComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0.1f, 0.15f);
            contentRect.anchorMax = new Vector2(0.9f, 0.85f);
            contentRect.offsetMin = Vector2.zero;
            contentRect.offsetMax = Vector2.zero;

            var hlg = content.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 60;
            hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.childForceExpandWidth = false;
            hlg.childForceExpandHeight = false;
            hlg.childControlWidth = false;
            hlg.childControlHeight = false;

            // Story button (placeholder) - Card style (280x420) with icon and disabled overlay
            var storyButton = CreateMissionCardButton(content.transform, "StoryButton", "Story", "Coming Soon", false, "book");
            var storyLayout = storyButton.AddComponent<LayoutElement>();
            storyLayout.preferredWidth = 280;
            storyLayout.preferredHeight = 420;

            // Battle Mission button - Card style (280x420) with icon, enabled
            var battleMissionButton = CreateMissionCardButton(content.transform, "BattleMissionButton", "Battle Mission", "Challenge the Null Rift", true, "battle");
            var battleLayout = battleMissionButton.AddComponent<LayoutElement>();
            battleLayout.preferredWidth = 280;
            battleLayout.preferredHeight = 420;

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
            CreateSpriteBackground(canvas, "BattleMission", new Color(0.08f, 0.08f, 0.12f));
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

            // Back button - use LayerLab style
            var backButton = CreateBackButton(header, "BackButton");
            var backRect = backButton.GetComponent<RectTransform>();
            backRect.anchorMin = new Vector2(0, 0.5f);
            backRect.anchorMax = new Vector2(0, 0.5f);
            backRect.pivot = new Vector2(0, 0.5f);

            var title = CreateSimpleTextObject("Title", header.transform, "Battle Mission");
            var titleRect = title.GetComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0, 0.5f);
            titleRect.anchorMax = new Vector2(0, 0.5f);
            titleRect.pivot = new Vector2(0, 0.5f);
            titleRect.sizeDelta = new Vector2(300, 50);
            titleRect.anchoredPosition = new Vector2(100, 0);
            title.GetComponent<TMP_Text>().fontSize = 32;

            // Settings button - use LayerLab style
            var layerLabConfig = LoadLayerLabConfig();
            var settingsButton = CreateLayerLabIconButton(header, "SettingsButton", layerLabConfig?.IconSettings);
            var settingsRect = settingsButton.GetComponent<RectTransform>();
            settingsRect.anchorMin = new Vector2(1, 0.5f);
            settingsRect.anchorMax = new Vector2(1, 0.5f);
            settingsRect.pivot = new Vector2(1, 0.5f);

            // Zone container
            var zoneContainer = CreateSimpleUIObject("ZoneContainer", screenObj.transform);
            var zoneRect = zoneContainer.GetComponent<RectTransform>();
            zoneRect.anchorMin = new Vector2(0.1f, 0.3f);
            zoneRect.anchorMax = new Vector2(0.9f, 0.8f);
            zoneRect.offsetMin = Vector2.zero;
            zoneRect.offsetMax = Vector2.zero;

            var hlg = zoneContainer.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 0; // No spacing - connection lines are children
            hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.childForceExpandWidth = false;
            hlg.childForceExpandHeight = false;
            hlg.childControlWidth = false;
            hlg.childControlHeight = false;

            // Zone nodes - 240x240 each, with connection lines between them
            var zone1 = CreateZoneNode("Zone1Node", zoneContainer.transform, 1, "The Outer Reaches");
            var zone1Layout = zone1.AddComponent<LayoutElement>();
            zone1Layout.preferredWidth = 240;
            zone1Layout.preferredHeight = 240;
            zone1.GetComponent<RectTransform>().sizeDelta = new Vector2(240, 240);

            // Connection line between Zone 1 and Zone 2 (inside HLG)
            var conn1 = CreateInlineConnectionLine("Connection1to2", zoneContainer.transform, 80);

            var zone2 = CreateZoneNode("Zone2Node", zoneContainer.transform, 2, "The Hollow Depths");
            var zone2Layout = zone2.AddComponent<LayoutElement>();
            zone2Layout.preferredWidth = 240;
            zone2Layout.preferredHeight = 240;
            zone2.GetComponent<RectTransform>().sizeDelta = new Vector2(240, 240);

            // Connection line between Zone 2 and Zone 3 (inside HLG)
            var conn2 = CreateInlineConnectionLine("Connection2to3", zoneContainer.transform, 80);

            var zone3 = CreateZoneNode("Zone3Node", zoneContainer.transform, 3, "The Null Core");
            var zone3Layout = zone3.AddComponent<LayoutElement>();
            zone3Layout.preferredWidth = 240;
            zone3Layout.preferredHeight = 240;
            zone3.GetComponent<RectTransform>().sizeDelta = new Vector2(240, 240);

            // Difficulty selector (bottom center, matching reference design)
            var difficultySection = CreateDifficultySelectorUI(screenObj.transform);
            var diffSelector = difficultySection.GetComponent<DifficultySelector>();

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

            // No background image - difficulty buttons stand alone
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

            // No selection indicator - active difficulty shows original colors, inactive ones are faded
            // DifficultySelector handles this via alpha-based fading when _selectionIndicator is null

            var so = new SerializedObject(diffSelector);
            so.FindProperty("_easyButton").objectReferenceValue = easyButton.GetComponent<Button>();
            so.FindProperty("_normalButton").objectReferenceValue = normalButton.GetComponent<Button>();
            so.FindProperty("_hardButton").objectReferenceValue = hardButton.GetComponent<Button>();
            so.FindProperty("_easyText").objectReferenceValue = easyButton.GetComponentInChildren<TMP_Text>();
            so.FindProperty("_normalText").objectReferenceValue = normalButton.GetComponentInChildren<TMP_Text>();
            so.FindProperty("_hardText").objectReferenceValue = hardButton.GetComponentInChildren<TMP_Text>();
            // Leave _selectionIndicator as null - triggers alpha-based fading in DifficultySelector
            so.ApplyModifiedProperties();

            return container;
        }

        private static GameObject CreateDifficultyButton(Transform parent, string text)
        {
            var layerLabConfig = LoadLayerLabConfig();

            // Determine button color based on difficulty text
            string color = text.ToUpper() switch
            {
                "EASY" => "green",
                "NORMAL" => "purple",
                "HARD" => "red",
                _ => "gray"
            };

            // Use LayerLab Button_01 small if config is available
            if (layerLabConfig != null && layerLabConfig.HasAllButton01SmallSprites())
            {
                var buttonObj = LayerLabButtonBuilder.CreateButton01Small(parent.gameObject, text + "Button", text, color);
                // Size will be controlled by layout group
                return buttonObj;
            }

            // Fallback to simple colored button
            var fallbackObj = CreateSimpleUIObject(text + "Button", parent);
            var image = fallbackObj.AddComponent<Image>();
            image.color = color switch
            {
                "green" => new Color(0.15f, 0.35f, 0.2f, 0.9f),
                "red" => new Color(0.35f, 0.15f, 0.15f, 0.9f),
                _ => new Color(0.25f, 0.15f, 0.35f, 0.9f)
            };

            var button = fallbackObj.AddComponent<Button>();
            button.targetGraphic = image;

            var textObj = CreateSimpleTextObject("Text", fallbackObj.transform, text);
            SetFullStretchRect(textObj);
            var tmpText = textObj.GetComponent<TMP_Text>();
            tmpText.alignment = TextAlignmentOptions.Center;
            tmpText.fontSize = 20;
            tmpText.fontStyle = FontStyles.Bold;

            return fallbackObj;
        }

        /// <summary>
        /// Creates an inline connection line element for HorizontalLayoutGroup.
        /// Contains dotted line visual that sits between zone nodes.
        /// </summary>
        private static GameObject CreateInlineConnectionLine(string name, Transform parent, float width)
        {
            var connObj = CreateSimpleUIObject(name, parent);
            var rect = connObj.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(width, 240); // Same height as zone nodes

            // Add LayoutElement to give it the desired width
            var layout = connObj.AddComponent<LayoutElement>();
            layout.preferredWidth = width;
            layout.preferredHeight = 240;

            // Connection line color (matches NullRift map lines)
            Color lineColor = new Color(0.4f, 0.35f, 0.5f, 0.9f);

            // Create dotted line in the center of this element
            int dotCount = 5;
            float dotWidth = 8f;
            float dotHeight = 3f;
            float totalDotsWidth = dotCount * dotWidth;
            float gapWidth = (width - totalDotsWidth) / (dotCount + 1);

            for (int i = 0; i < dotCount; i++)
            {
                var dot = CreateSimpleUIObject($"Dot{i}", connObj.transform);
                var dotRect = dot.GetComponent<RectTransform>();
                dotRect.anchorMin = new Vector2(0.5f, 0.5f);
                dotRect.anchorMax = new Vector2(0.5f, 0.5f);
                dotRect.pivot = new Vector2(0.5f, 0.5f);
                dotRect.sizeDelta = new Vector2(dotWidth, dotHeight);

                // Position dots horizontally across the connection
                float startX = -width / 2 + gapWidth + dotWidth / 2;
                float xPos = startX + i * (dotWidth + gapWidth);
                dotRect.anchoredPosition = new Vector2(xPos, 0);

                var dotImage = dot.AddComponent<Image>();
                dotImage.color = lineColor;
            }

            return connObj;
        }

        private static GameObject CreateZoneNode(string name, Transform parent, int zoneNumber, string zoneName)
        {
            var layerLabConfig = LoadLayerLabConfig();

            // Create a clean, simple zone node (no complex deco)
            var nodeObj = CreateSimpleUIObject(name, parent);

            // Background with LayerLab sprites if available, fallback to color
            Image bgImage = nodeObj.AddComponent<Image>();
            if (layerLabConfig != null)
            {
                var (_, bg, _, _, _) = layerLabConfig.GetStageFrameSprites();
                if (bg != null)
                {
                    bgImage.sprite = bg;
                    bgImage.type = Image.Type.Sliced;
                    bgImage.color = new Color(0.6f, 0.45f, 0.85f, 0.5f); // Purple tint with transparency
                }
                else
                {
                    bgImage.color = new Color(0.2f, 0.15f, 0.3f, 0.8f);
                }
            }
            else
            {
                bgImage.color = new Color(0.2f, 0.15f, 0.3f, 0.8f);
            }

            // Border layer (simple, no deco)
            if (layerLabConfig != null)
            {
                var (_, _, border, _, _) = layerLabConfig.GetStageFrameSprites();
                if (border != null)
                {
                    var borderObj = CreateSimpleUIObject("Border", nodeObj.transform);
                    SetFullStretchRect(borderObj);
                    var borderImage = borderObj.AddComponent<Image>();
                    borderImage.sprite = border;
                    borderImage.type = Image.Type.Sliced;
                    borderImage.color = new Color(0.7f, 0.55f, 0.9f, 1f); // Purple border tint
                    borderImage.raycastTarget = false;
                }
            }

            // Zone number text - centered in upper portion
            var numberText = CreateSimpleTextObject("ZoneNumber", nodeObj.transform, $"ZONE {zoneNumber}");
            var numRect = numberText.GetComponent<RectTransform>();
            numRect.anchorMin = new Vector2(0, 0.52f); // Centered around 0.6 vertically
            numRect.anchorMax = new Vector2(1, 0.72f);
            numRect.offsetMin = new Vector2(10, 0);
            numRect.offsetMax = new Vector2(-10, 0);
            var numTmp = numberText.GetComponent<TMP_Text>();
            numTmp.fontSize = 28;
            numTmp.fontStyle = FontStyles.Bold;
            numTmp.alignment = TextAlignmentOptions.Center;
            numTmp.color = new Color(0.83f, 0.69f, 0.22f); // Soul gold

            // Zone name text - centered below zone number
            var nameTextObj = CreateSimpleTextObject("ZoneName", nodeObj.transform, zoneName);
            var nameRect = nameTextObj.GetComponent<RectTransform>();
            nameRect.anchorMin = new Vector2(0, 0.28f); // Centered around 0.35 vertically
            nameRect.anchorMax = new Vector2(1, 0.50f);
            nameRect.offsetMin = new Vector2(10, 0);
            nameRect.offsetMax = new Vector2(-10, 0);
            var nameTmp = nameTextObj.GetComponent<TMP_Text>();
            nameTmp.fontSize = 20;
            nameTmp.alignment = TextAlignmentOptions.Center;
            nameTmp.color = new Color(0.75f, 0.75f, 0.8f);

            // Button component
            var button = nodeObj.AddComponent<Button>();
            button.targetGraphic = bgImage;
            button.transition = Selectable.Transition.ColorTint;

            ColorBlock colors = button.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(1.2f, 1.2f, 1.3f);
            colors.pressedColor = new Color(0.85f, 0.85f, 0.9f);
            colors.selectedColor = new Color(1.1f, 1.1f, 1.15f);
            colors.disabledColor = new Color(0.5f, 0.5f, 0.5f, 0.8f);
            button.colors = colors;

            // ZoneNodeButton component
            var nodeComponent = nodeObj.AddComponent<ZoneNodeButton>();

            var so = new SerializedObject(nodeComponent);
            so.FindProperty("_zoneNumber").intValue = zoneNumber;
            so.FindProperty("_zoneName").stringValue = zoneName;
            so.FindProperty("_button").objectReferenceValue = button;
            so.FindProperty("_zoneNumberText").objectReferenceValue = numberText.GetComponent<TMP_Text>();
            so.FindProperty("_zoneNameText").objectReferenceValue = nameTextObj.GetComponent<TMP_Text>();
            so.FindProperty("_backgroundImage").objectReferenceValue = bgImage;
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

            // Start run button - use LayerLab Button_01 green style
            var layerLabConfig = LoadLayerLabConfig();
            GameObject startRunButton;
            if (layerLabConfig != null && layerLabConfig.HasAllButton01SmallSprites())
            {
                startRunButton = LayerLabButtonBuilder.CreateButton01Small(screenObj, "StartRunButton", "CONFIRM TEAM", "green");
            }
            else
            {
                startRunButton = CreateSimpleButton("StartRunButton", screenObj.transform, "CONFIRM TEAM");
                var startRunImage = startRunButton.GetComponent<Image>();
                startRunImage.color = new Color(0.3f, 0.6f, 0.3f, 0.9f);
            }
            var startRunRect = startRunButton.GetComponent<RectTransform>();
            startRunRect.anchorMin = new Vector2(0.5f, 0.08f);
            startRunRect.anchorMax = new Vector2(0.5f, 0.08f);
            startRunRect.pivot = new Vector2(0.5f, 0.5f);
            startRunRect.sizeDelta = new Vector2(250, 80);

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
            CreateSpriteBackground(canvas, "Requiems", new Color(0.08f, 0.08f, 0.12f));
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

            // Back button - use LayerLab style
            var backButton = CreateBackButton(header, "BackButton");
            var backRect = backButton.GetComponent<RectTransform>();
            backRect.anchorMin = new Vector2(0, 0.5f);
            backRect.anchorMax = new Vector2(0, 0.5f);
            backRect.pivot = new Vector2(0, 0.5f);

            var title = CreateSimpleTextObject("Title", header.transform, "Requiems");
            var titleRect = title.GetComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0, 0.5f);
            titleRect.anchorMax = new Vector2(0, 0.5f);
            titleRect.pivot = new Vector2(0, 0.5f);
            titleRect.sizeDelta = new Vector2(200, 50);
            titleRect.anchoredPosition = new Vector2(100, 0);
            title.GetComponent<TMP_Text>().fontSize = 32;

            // Settings button - use LayerLab style
            var reqLayerLabConfig = LoadLayerLabConfig();
            var settingsButton = CreateLayerLabIconButton(header, "SettingsButton", reqLayerLabConfig?.IconSettings);
            var settingsRect = settingsButton.GetComponent<RectTransform>();
            settingsRect.anchorMin = new Vector2(1, 0.5f);
            settingsRect.anchorMax = new Vector2(1, 0.5f);
            settingsRect.pivot = new Vector2(1, 0.5f);
            settingsRect.sizeDelta = new Vector2(60, 60);

            // Portrait grid container - adjusted for larger 322x480 cells
            var gridContainer = CreateSimpleUIObject("PortraitGrid", screenObj.transform);
            var gridRect = gridContainer.GetComponent<RectTransform>();
            gridRect.anchorMin = new Vector2(0.15f, 0.05f);
            gridRect.anchorMax = new Vector2(0.85f, 0.88f);
            gridRect.offsetMin = Vector2.zero;
            gridRect.offsetMax = Vector2.zero;

            var grid = gridContainer.AddComponent<GridLayoutGroup>();
            // Match RequiemPortraitButton.prefab designed size (322x480 LayerLab ListFrame style)
            grid.cellSize = new Vector2(322, 480);
            grid.spacing = new Vector2(20, 20);
            grid.startCorner = GridLayoutGroup.Corner.UpperLeft;
            grid.startAxis = GridLayoutGroup.Axis.Horizontal;
            grid.childAlignment = TextAnchor.MiddleCenter;
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = 2;

            // Load RequiemPortraitButton prefab from Runtime prefabs
            var portraitButtonPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Project/Prefabs/UI/Runtime/RequiemPortraitButton.prefab");
            if (portraitButtonPrefab == null)
            {
                Debug.LogWarning("[ProductionSceneSetupGenerator] RequiemPortraitButton prefab not found at Assets/_Project/Prefabs/UI/Runtime/RequiemPortraitButton.prefab");
            }

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
            so.FindProperty("_portraitButtonPrefab").objectReferenceValue = portraitButtonPrefab;

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

        /// <summary>
        /// Creates a Requiem portrait button template using LayerLab StageFrame styling.
        /// </summary>
        /// <summary>
        /// Creates a Requiem portrait button template using profile-style frame (not zone-selection StageFrame).
        /// </summary>
        private static GameObject CreateRequiemPortraitTemplate(Transform parent)
        {
            var layerLabConfig = LoadLayerLabConfig();

            GameObject templateObj = new GameObject("PortraitButtonTemplate");
            templateObj.transform.SetParent(parent, false);

            RectTransform rect = templateObj.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(200, 280);

            // Background - use ProfileFrame if available
            Image bgImage = templateObj.AddComponent<Image>();
            bool hasProfileFrame = layerLabConfig?.ProfileFrameBg != null;

            if (hasProfileFrame)
            {
                bgImage.sprite = layerLabConfig.ProfileFrameBg;
                bgImage.type = Image.Type.Sliced;
                bgImage.color = Color.white;
            }
            else
            {
                bgImage.color = new Color(0.12f, 0.10f, 0.18f, 0.95f);
            }

            // Add button component
            Button button = templateObj.AddComponent<Button>();
            button.targetGraphic = bgImage;

            // Border layer - use ProfileFrameBorder if available
            if (hasProfileFrame && layerLabConfig.ProfileFrameBorder != null)
            {
                GameObject borderObj = new GameObject("Border");
                borderObj.transform.SetParent(templateObj.transform, false);
                RectTransform borderRect = borderObj.AddComponent<RectTransform>();
                borderRect.anchorMin = Vector2.zero;
                borderRect.anchorMax = Vector2.one;
                borderRect.sizeDelta = Vector2.zero;

                Image borderImg = borderObj.AddComponent<Image>();
                borderImg.sprite = layerLabConfig.ProfileFrameBorder;
                borderImg.type = Image.Type.Sliced;
                borderImg.color = Color.white;
                borderImg.raycastTarget = false;
            }

            // Purple border decoration layer
            if (hasProfileFrame && layerLabConfig.ProfileFrameBorderDecoPurple != null)
            {
                GameObject decoObj = new GameObject("BorderDeco");
                decoObj.transform.SetParent(templateObj.transform, false);
                RectTransform decoRect = decoObj.AddComponent<RectTransform>();
                decoRect.anchorMin = Vector2.zero;
                decoRect.anchorMax = Vector2.one;
                decoRect.sizeDelta = Vector2.zero;

                Image decoImg = decoObj.AddComponent<Image>();
                decoImg.sprite = layerLabConfig.ProfileFrameBorderDecoPurple;
                decoImg.type = Image.Type.Sliced;
                decoImg.color = Color.white;
                decoImg.raycastTarget = false;
            }

            // Portrait image area (fills 80% of frame, centered)
            GameObject portraitArea = new GameObject("Portrait");
            portraitArea.transform.SetParent(templateObj.transform, false);
            RectTransform portraitRect = portraitArea.AddComponent<RectTransform>();
            portraitRect.anchorMin = new Vector2(0.1f, 0.2f);   // Portrait fills 80% of width, 75% of height
            portraitRect.anchorMax = new Vector2(0.9f, 0.95f);
            portraitRect.sizeDelta = Vector2.zero;

            Image portraitImage = portraitArea.AddComponent<Image>();
            portraitImage.color = Color.white;
            portraitImage.preserveAspect = true;
            portraitImage.raycastTarget = false;

            // Name text at bottom (15% height area) with soul gold color
            GameObject nameObj = CreateText(templateObj, "Title", "Requiem Name", 18);
            RectTransform nameRect = nameObj.GetComponent<RectTransform>();
            nameRect.anchorMin = new Vector2(0, 0);
            nameRect.anchorMax = new Vector2(1, 0.18f);
            nameRect.offsetMin = new Vector2(8, 5);
            nameRect.offsetMax = new Vector2(-8, -2);
            var nameTmp = nameObj.GetComponent<TMP_Text>();
            nameTmp.color = new Color(0.83f, 0.69f, 0.22f); // Soul gold
            nameTmp.fontStyle = FontStyles.Bold;
            nameTmp.alignment = TextAlignmentOptions.Center;

            // Focus/selection highlight layer
            GameObject focusObj = new GameObject("Focus");
            focusObj.transform.SetParent(templateObj.transform, false);
            RectTransform focusRect = focusObj.AddComponent<RectTransform>();
            focusRect.anchorMin = new Vector2(-0.05f, -0.02f);
            focusRect.anchorMax = new Vector2(1.05f, 1.02f);
            focusRect.sizeDelta = Vector2.zero;

            Image focusImg = focusObj.AddComponent<Image>();
            focusImg.color = new Color(0.56f, 0.27f, 0.68f, 0.6f); // Purple glow
            focusImg.raycastTarget = false;
            focusObj.SetActive(false); // Hidden by default, shown on selection

            // Add RequiemPortraitButton component
            var portraitButton = templateObj.AddComponent<RequiemPortraitButton>();

            // Wire references
            SerializedObject portraitSo = new SerializedObject(portraitButton);
            portraitSo.FindProperty("_button").objectReferenceValue = button;
            portraitSo.FindProperty("_portraitImage").objectReferenceValue = portraitImage;
            portraitSo.FindProperty("_nameText").objectReferenceValue = nameTmp;
            portraitSo.FindProperty("_frameImage").objectReferenceValue = bgImage;
            portraitSo.FindProperty("_glowImage").objectReferenceValue = focusImg;
            portraitSo.FindProperty("_selectionHighlight").objectReferenceValue = focusImg;
            portraitSo.ApplyModifiedPropertiesWithoutUndo();

            return templateObj;
        }

        private static GameObject CreateRequiemDetailPanelUI(Transform parent)
        {
            var panelObj = CreateSimpleUIObject("RequiemDetailPanel", parent);
            SetFullStretchRect(panelObj);
            var detailPanel = panelObj.AddComponent<RequiemDetailPanel>();
            var layerLabConfig = LoadLayerLabConfig();

            var canvasGroup = panelObj.AddComponent<CanvasGroup>();
            canvasGroup.alpha = 0;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;

            var bgImage = panelObj.AddComponent<Image>();
            // NavyBackground (#1F3A5F) for dark theme - RGB(31, 58, 95)
            bgImage.color = new Color(31f / 255f, 58f / 255f, 95f / 255f, 1f);

            var panelContent = CreateSimpleUIObject("PanelContent", panelObj.transform);
            var contentRect = panelContent.GetComponent<RectTransform>();
            contentRect.anchorMin = Vector2.zero;
            contentRect.anchorMax = Vector2.one;
            contentRect.offsetMin = Vector2.zero;
            contentRect.offsetMax = Vector2.zero;

            // Header (top bar) - apply BaseFrame_Border_Rectangle_H60 if available
            var header = CreateSimpleUIObject("Header", panelContent.transform);
            var headerRect = header.GetComponent<RectTransform>();
            headerRect.anchorMin = new Vector2(0, 0.92f);
            headerRect.anchorMax = new Vector2(1, 1);
            headerRect.offsetMin = Vector2.zero;
            headerRect.offsetMax = Vector2.zero;

            var headerBg = header.AddComponent<Image>();
            if (layerLabConfig?.BaseFrameBorderRectH60 != null)
            {
                headerBg.sprite = layerLabConfig.BaseFrameBorderRectH60;
                headerBg.type = Image.Type.Sliced;
                headerBg.color = new Color(0.2f, 0.15f, 0.3f, 0.95f); // Purple tint for theme
            }
            else
            {
                // Dark theme fallback
                headerBg.color = new Color(0.2f, 0.15f, 0.3f, 0.95f);
            }

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
                closeBtnText.color = Color.white;
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
            titleText.color = Color.white;

            // ==============================
            // Left Sidebar - Portrait List (apply ListFrame_01 if available)
            // ==============================
            var leftSidebar = CreateSimpleUIObject("LeftSidebar", panelContent.transform);
            var leftRect = leftSidebar.GetComponent<RectTransform>();
            leftRect.anchorMin = new Vector2(0, 0);
            leftRect.anchorMax = new Vector2(0.08f, 0.92f);
            leftRect.offsetMin = Vector2.zero;
            leftRect.offsetMax = Vector2.zero;

            var leftBg = leftSidebar.AddComponent<Image>();
            if (layerLabConfig?.ListFrameBg != null)
            {
                leftBg.sprite = layerLabConfig.ListFrameBg;
                leftBg.type = Image.Type.Sliced;
                leftBg.color = new Color(0.15f, 0.12f, 0.2f, 0.95f); // Dark purple
            }
            else
            {
                // Dark theme fallback
                leftBg.color = new Color(0.15f, 0.12f, 0.2f, 0.95f);
            }

            // Add ListFrame border if available
            if (layerLabConfig?.ListFrameBorder != null)
            {
                var leftBorderObj = CreateSimpleUIObject("Border", leftSidebar.transform);
                SetFullStretchRect(leftBorderObj);
                var leftBorderImg = leftBorderObj.AddComponent<Image>();
                leftBorderImg.sprite = layerLabConfig.ListFrameBorder;
                leftBorderImg.type = Image.Type.Sliced;
                leftBorderImg.color = new Color(0.5f, 0.4f, 0.7f, 0.8f);
                leftBorderImg.raycastTarget = false;
            }

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
            // Menu Sidebar - Stats/Cards Tabs (Vertical) - apply TabMenu styling
            // ==============================
            var menuSidebar = CreateSimpleUIObject("MenuSidebar", panelContent.transform);
            var menuRect = menuSidebar.GetComponent<RectTransform>();
            menuRect.anchorMin = new Vector2(0.08f, 0);
            menuRect.anchorMax = new Vector2(0.22f, 0.92f);
            menuRect.offsetMin = Vector2.zero;
            menuRect.offsetMax = Vector2.zero;

            var menuBg = menuSidebar.AddComponent<Image>();
            if (layerLabConfig?.TabMenuBg != null)
            {
                menuBg.sprite = layerLabConfig.TabMenuBg;
                menuBg.type = Image.Type.Sliced;
                menuBg.color = new Color(0.18f, 0.15f, 0.25f, 0.95f);
            }
            else
            {
                // Dark theme fallback
                menuBg.color = new Color(0.18f, 0.15f, 0.25f, 0.95f);
            }

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
            // DeepViolet (#4B2D6E) for dark theme character area - RGB(75, 45, 110)
            characterBg.color = new Color(75f / 255f, 45f / 255f, 110f / 255f, 0.6f);

            var artContainer = CreateSimpleUIObject("CharacterArt", characterArea.transform);
            var artRect = artContainer.GetComponent<RectTransform>();
            artRect.anchorMin = new Vector2(0.1f, 0.05f);
            artRect.anchorMax = new Vector2(0.9f, 0.95f);
            artRect.offsetMin = Vector2.zero;
            artRect.offsetMax = Vector2.zero;

            var artImage = artContainer.AddComponent<Image>();
            artImage.color = Color.white; // Full alpha for proper portrait display
            artImage.preserveAspect = true;

            // Stats Panel (right side of stats view) - apply PanelFrame_BottomDeco_01 if available
            var statsPanel = CreateSimpleUIObject("StatsPanel", statsContent.transform);
            var statsPanelRect = statsPanel.GetComponent<RectTransform>();
            statsPanelRect.anchorMin = new Vector2(0.65f, 0);
            statsPanelRect.anchorMax = new Vector2(1, 1);
            statsPanelRect.offsetMin = Vector2.zero;
            statsPanelRect.offsetMax = Vector2.zero;

            var statsPanelBg = statsPanel.AddComponent<Image>();
            if (layerLabConfig?.PanelFrameBg != null)
            {
                statsPanelBg.sprite = layerLabConfig.PanelFrameBg;
                statsPanelBg.type = Image.Type.Sliced;
                statsPanelBg.color = new Color(0.12f, 0.10f, 0.18f, 0.95f); // Dark purple
            }
            else
            {
                // Dark theme fallback
                statsPanelBg.color = new Color(0.12f, 0.10f, 0.18f, 0.95f);
            }

            // Add PanelFrame border and deco if available
            if (layerLabConfig?.PanelFrameBorder != null)
            {
                var panelBorderObj = CreateSimpleUIObject("Border", statsPanel.transform);
                SetFullStretchRect(panelBorderObj);
                var panelBorderImg = panelBorderObj.AddComponent<Image>();
                panelBorderImg.sprite = layerLabConfig.PanelFrameBorder;
                panelBorderImg.type = Image.Type.Sliced;
                panelBorderImg.color = new Color(0.5f, 0.4f, 0.7f, 0.8f);
                panelBorderImg.raycastTarget = false;
            }

            if (layerLabConfig?.PanelFrameDeco != null)
            {
                var panelDecoObj = CreateSimpleUIObject("Deco", statsPanel.transform);
                SetFullStretchRect(panelDecoObj);
                var panelDecoImg = panelDecoObj.AddComponent<Image>();
                panelDecoImg.sprite = layerLabConfig.PanelFrameDeco;
                panelDecoImg.type = Image.Type.Sliced;
                panelDecoImg.color = new Color(0.7f, 0.5f, 0.9f, 0.6f);
                panelDecoImg.raycastTarget = false;
            }

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
            // Use soul gold for name text against dark background
            nameText.color = new Color(0.83f, 0.69f, 0.22f);
            nameText.alignment = TextAlignmentOptions.Left;

            var classObj = CreateSimpleTextObject("CharacterClass", statsPanel.transform, "Class | Aspect");
            var classRect = classObj.GetComponent<RectTransform>();
            classRect.anchorMin = new Vector2(0, 0.82f);
            classRect.anchorMax = new Vector2(1, 0.88f);
            classRect.offsetMin = new Vector2(15, 0);
            classRect.offsetMax = new Vector2(-15, 0);
            var classText = classObj.GetComponent<TMP_Text>();
            classText.fontSize = 14;
            // Light gray for secondary text against dark background
            classText.color = new Color(0.75f, 0.75f, 0.8f);
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
            // Medium gray for header against dark background
            statsHeaderText.color = new Color(0.65f, 0.65f, 0.7f);
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
            // Dark theme for cards content - match NavyBackground
            cardsContentBg.color = new Color(31f / 255f, 58f / 255f, 95f / 255f, 1f);

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
            // Soul gold color for header on dark background
            startingHeaderText.color = new Color(0.83f, 0.69f, 0.22f, 1f);
            startingHeaderText.alignment = TextAlignmentOptions.Left;

            // ScrollView for cards (matching Sanctuary screen pattern)
            var cardsScrollView = CreateSimpleUIObject("CardsScrollView", startingCardsSection.transform);
            var scrollViewRect = cardsScrollView.GetComponent<RectTransform>();
            scrollViewRect.anchorMin = new Vector2(0, 0);
            scrollViewRect.anchorMax = new Vector2(1, 0.92f);
            scrollViewRect.offsetMin = Vector2.zero;
            scrollViewRect.offsetMax = Vector2.zero;

            // Add background to scroll view - dark theme
            var scrollBg = cardsScrollView.AddComponent<Image>();
            scrollBg.color = new Color(75f / 255f, 45f / 255f, 110f / 255f, 0.3f); // DeepViolet semi-transparent

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

            // Wire sidebar portrait prefab from RuntimeUIPrefabConfig
            var runtimePrefabConfig = LoadRuntimePrefabConfig();
            if (runtimePrefabConfig?.SidebarPortraitPrefab != null)
            {
                so.FindProperty("_sidebarPortraitPrefab").objectReferenceValue = runtimePrefabConfig.SidebarPortraitPrefab;
                Debug.Log("[ProductionSceneSetupGenerator] Wired SidebarPortraitPrefab to RequiemDetailPanel");
            }
            else
            {
                Debug.LogWarning("[ProductionSceneSetupGenerator] SidebarPortraitPrefab not found in RuntimeUIPrefabConfig");
            }

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
        /// <summary>
        /// Creates a stat row for the redesigned detail panel with LayerLab styling.
        /// Uses BaseFrame_Border_Rectangle_H40 if available, with themed text colors.
        /// </summary>
        private static GameObject CreateStatRowForDetailRedesigned(Transform parent, string label, string value)
        {
            var layerLabConfig = LoadLayerLabConfig();

            var row = CreateSimpleUIObject(label + "Row", parent);
            var layoutElement = row.AddComponent<LayoutElement>();
            layoutElement.preferredHeight = 35;

            // Add background with LayerLab BaseFrame styling
            var rowBg = row.AddComponent<Image>();
            if (layerLabConfig?.BaseFrameBorderRectH40 != null)
            {
                rowBg.sprite = layerLabConfig.BaseFrameBorderRectH40;
                rowBg.type = Image.Type.Sliced;
                rowBg.color = new Color(0.18f, 0.15f, 0.25f, 0.8f); // Semi-transparent purple
            }
            else
            {
                rowBg.color = new Color(0.2f, 0.18f, 0.25f, 0.6f);
            }
            rowBg.raycastTarget = false;

            var labelObj = CreateSimpleTextObject("Label", row.transform, label);
            var labelRect = labelObj.GetComponent<RectTransform>();
            labelRect.anchorMin = new Vector2(0, 0);
            labelRect.anchorMax = new Vector2(0.6f, 1);
            labelRect.offsetMin = new Vector2(10, 0);
            labelRect.offsetMax = Vector2.zero;
            var labelText = labelObj.GetComponent<TMP_Text>();
            labelText.fontSize = 16;
            // Light color for label against dark background
            labelText.color = new Color(0.8f, 0.8f, 0.85f);
            labelText.alignment = TextAlignmentOptions.Left;

            var valueObj = CreateSimpleTextObject("Value", row.transform, value);
            var valueRect = valueObj.GetComponent<RectTransform>();
            valueRect.anchorMin = new Vector2(0.6f, 0);
            valueRect.anchorMax = new Vector2(1, 1);
            valueRect.offsetMin = Vector2.zero;
            valueRect.offsetMax = new Vector2(-10, 0);
            var valueText = valueObj.GetComponent<TMP_Text>();
            valueText.fontSize = 18;
            valueText.fontStyle = FontStyles.Bold;
            // White for value text against dark background
            valueText.color = Color.white;
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

            // Load LayerLab config for settings category icons
            var layerLabConfig = LoadLayerLabConfig();

            // Category buttons - use sprites if available, fallback to emoji text
            CreateSettingsCategoryButton(leftSidebar.transform, "DisplayBtn", layerLabConfig?.SettingsIconDisplay, "\U0001F5A5", false);
            CreateSettingsCategoryButton(leftSidebar.transform, "AudioBtn", layerLabConfig?.SettingsIconAudio, "\U0001F3A7", true);   // Selected
            CreateSettingsCategoryButton(leftSidebar.transform, "GameBtn", layerLabConfig?.SettingsIconGame, "\u2699", false);
            CreateSettingsCategoryButton(leftSidebar.transform, "NetworkBtn", layerLabConfig?.SettingsIconNetwork, "\U0001F310", false);
            CreateSettingsCategoryButton(leftSidebar.transform, "AccountBtn", layerLabConfig?.SettingsIconAccount, "\U0001F464", false);

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
            vlg.childControlWidth = true;   // Enable width control
            vlg.childControlHeight = true;  // Enable height control
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
            // Wire volume labels (Percent text in each row)
            so.FindProperty("_masterVolumeLabel").objectReferenceValue = masterSlider.transform.Find("Percent")?.GetComponent<TMP_Text>();
            so.FindProperty("_musicVolumeLabel").objectReferenceValue = musicSlider.transform.Find("Percent")?.GetComponent<TMP_Text>();
            so.FindProperty("_sfxVolumeLabel").objectReferenceValue = sfxSlider.transform.Find("Percent")?.GetComponent<TMP_Text>();
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
            layoutElement.minHeight = 30;
            layoutElement.flexibleWidth = 1; // Allow expansion

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
            layoutElement.preferredHeight = 50;
            layoutElement.minHeight = 50;
            layoutElement.flexibleWidth = 1; // Allow expansion to fill parent width

            // Row background (light)
            var rowBg = row.AddComponent<Image>();
            rowBg.color = new Color(0.95f, 0.95f, 0.97f, 1f);

            // Label (left side, 20% width)
            var label = CreateSimpleTextObject("Label", row.transform, labelText);
            var labelRect = label.GetComponent<RectTransform>();
            labelRect.anchorMin = new Vector2(0, 0);
            labelRect.anchorMax = new Vector2(0.20f, 1);
            labelRect.offsetMin = new Vector2(15, 0);
            labelRect.offsetMax = new Vector2(0, 0);
            var labelTmp = label.GetComponent<TMP_Text>();
            labelTmp.fontSize = 16;
            labelTmp.color = Color.black;
            labelTmp.alignment = TextAlignmentOptions.MidlineLeft;

            // Slider container - positioned between label and percentage
            var sliderObj = CreateSimpleUIObject("Slider", row.transform);
            var sliderRect = sliderObj.GetComponent<RectTransform>();
            sliderRect.anchorMin = new Vector2(0.22f, 0.3f);
            sliderRect.anchorMax = new Vector2(0.72f, 0.7f);
            sliderRect.offsetMin = Vector2.zero;
            sliderRect.offsetMax = Vector2.zero;

            var slider = sliderObj.AddComponent<Slider>();
            slider.minValue = 0;
            slider.maxValue = 1;
            slider.value = 1;
            slider.direction = Slider.Direction.LeftToRight;
            slider.wholeNumbers = false;

            // Slider background track (gray bar)
            var sliderBgObj = CreateSimpleUIObject("Background", sliderObj.transform);
            var sliderBgRect = sliderBgObj.GetComponent<RectTransform>();
            sliderBgRect.anchorMin = Vector2.zero;
            sliderBgRect.anchorMax = Vector2.one;
            sliderBgRect.offsetMin = Vector2.zero;
            sliderBgRect.offsetMax = Vector2.zero;
            var sliderBgImage = sliderBgObj.AddComponent<Image>();
            sliderBgImage.color = new Color(0.7f, 0.7f, 0.73f, 1f);

            // Fill area - contains the fill image
            var fillArea = CreateSimpleUIObject("Fill Area", sliderObj.transform);
            var fillAreaRect = fillArea.GetComponent<RectTransform>();
            fillAreaRect.anchorMin = Vector2.zero;
            fillAreaRect.anchorMax = Vector2.one;
            fillAreaRect.offsetMin = Vector2.zero;
            fillAreaRect.offsetMax = Vector2.zero;

            // Fill image (orange progress bar)
            var fill = CreateSimpleUIObject("Fill", fillArea.transform);
            var fillRect = fill.GetComponent<RectTransform>();
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = Vector2.one;
            fillRect.offsetMin = Vector2.zero;
            fillRect.offsetMax = Vector2.zero;
            var fillImage = fill.AddComponent<Image>();
            fillImage.color = new Color(0.9f, 0.6f, 0.1f, 1f);  // Orange fill
            slider.fillRect = fillRect;

            // Handle slide area - defines the area where handle can move
            var handleArea = CreateSimpleUIObject("Handle Slide Area", sliderObj.transform);
            var handleAreaRect = handleArea.GetComponent<RectTransform>();
            handleAreaRect.anchorMin = Vector2.zero;
            handleAreaRect.anchorMax = Vector2.one;
            handleAreaRect.offsetMin = new Vector2(10, 0);
            handleAreaRect.offsetMax = new Vector2(-10, 0);

            // Handle (draggable knob)
            var handle = CreateSimpleUIObject("Handle", handleArea.transform);
            var handleRect = handle.GetComponent<RectTransform>();
            handleRect.sizeDelta = new Vector2(20, 20);
            var handleImage = handle.AddComponent<Image>();
            handleImage.color = new Color(0.95f, 0.65f, 0.15f, 1f);  // Brighter orange handle
            slider.handleRect = handleRect;
            slider.targetGraphic = handleImage;

            // Percentage text (shows current value like "100%")
            var percentText = CreateSimpleTextObject("Percent", row.transform, "100%");
            var percentRect = percentText.GetComponent<RectTransform>();
            percentRect.anchorMin = new Vector2(0.74f, 0);
            percentRect.anchorMax = new Vector2(0.84f, 1);
            percentRect.offsetMin = Vector2.zero;
            percentRect.offsetMax = Vector2.zero;
            var percentTmp = percentText.GetComponent<TMP_Text>();
            percentTmp.fontSize = 16;
            percentTmp.color = Color.black;
            percentTmp.alignment = TextAlignmentOptions.Center;

            // Mute label
            var muteLabel = CreateSimpleTextObject("MuteLabel", row.transform, "Mute");
            var muteLabelRect = muteLabel.GetComponent<RectTransform>();
            muteLabelRect.anchorMin = new Vector2(0.85f, 0);
            muteLabelRect.anchorMax = new Vector2(0.93f, 1);
            muteLabelRect.offsetMin = Vector2.zero;
            muteLabelRect.offsetMax = Vector2.zero;
            var muteLabelTmp = muteLabel.GetComponent<TMP_Text>();
            muteLabelTmp.fontSize = 14;
            muteLabelTmp.color = new Color(0.4f, 0.4f, 0.45f, 1f);
            muteLabelTmp.alignment = TextAlignmentOptions.Center;

            // Mute checkbox placeholder
            var muteBox = CreateSimpleUIObject("MuteCheckbox", row.transform);
            var muteBoxRect = muteBox.GetComponent<RectTransform>();
            muteBoxRect.anchorMin = new Vector2(0.94f, 0.5f);
            muteBoxRect.anchorMax = new Vector2(0.94f, 0.5f);
            muteBoxRect.pivot = new Vector2(0.5f, 0.5f);
            muteBoxRect.sizeDelta = new Vector2(24, 24);
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
            var layerLabConfig = LoadLayerLabConfig();

            // Check if this looks like a back button
            bool isBackButton = name.ToLower().Contains("back") || text == "<" || text == "\u2190";

            if (layerLabConfig != null && layerLabConfig.HasAllConvexButtonSprites() && isBackButton)
            {
                // Use LayerLab left flush button for back buttons
                var buttonObj = LayerLabButtonBuilder.CreateConvexLeftFlushButton(parent.gameObject, name, layerLabConfig.IconBack);
                RectTransform rect = buttonObj.GetComponent<RectTransform>();
                rect.sizeDelta = new Vector2(70, 60);
                return buttonObj;
            }

            // Fallback to simple button
            var fallbackObj = CreateSimpleUIObject(name, parent);
            var image = fallbackObj.AddComponent<Image>();
            image.color = new Color(0.25f, 0.25f, 0.3f, 0.9f);

            var button = fallbackObj.AddComponent<Button>();
            button.targetGraphic = image;

            var textObj = CreateSimpleTextObject("Text", fallbackObj.transform, text);
            SetFullStretchRect(textObj);
            var tmpText = textObj.GetComponent<TMP_Text>();
            tmpText.alignment = TextAlignmentOptions.Center;
            tmpText.fontSize = 24;

            return fallbackObj;
        }

        /// <summary>
        /// Creates a button with an icon sprite (or fallback text).
        /// Uses LayerLab Convex Rectangle style for settings-like buttons when available.
        /// </summary>
        private static GameObject CreateIconButton(string name, Transform parent, Sprite iconSprite, string fallbackText, Vector2 size)
        {
            var layerLabConfig = LoadLayerLabConfig();

            // Check if this looks like a settings button
            bool isSettingsButton = name.ToLower().Contains("settings") || fallbackText == "\u2699";

            if (layerLabConfig != null && layerLabConfig.HasAllConvexButtonSprites() && isSettingsButton)
            {
                // Use LayerLab convex rectangle button for settings
                var buttonObj = LayerLabButtonBuilder.CreateConvexRectangleButton(parent.gameObject, name, layerLabConfig.IconSettings);
                RectTransform rect = buttonObj.GetComponent<RectTransform>();
                rect.sizeDelta = size;
                return buttonObj;
            }

            // Fallback to simple icon button
            var fallbackObj = CreateSimpleUIObject(name, parent);
            var rect2 = fallbackObj.GetComponent<RectTransform>();
            rect2.sizeDelta = size;

            var bgImage = fallbackObj.AddComponent<Image>();
            bgImage.color = new Color(0.25f, 0.25f, 0.3f, 0.9f);

            var button = fallbackObj.AddComponent<Button>();
            button.targetGraphic = bgImage;

            // Use sprite if available, otherwise use text fallback
            Color cyanColor = new Color(0f, 0.83f, 0.89f); // Soul cyan
            var iconSize = new Vector2(size.x * 0.6f, size.y * 0.6f);
            GameObject iconObj = CreateIconImage(fallbackObj, "Icon", iconSprite, iconSize, cyanColor, fallbackText, (int)(size.y * 0.4f));
            var iconRect = iconObj.GetComponent<RectTransform>();
            iconRect.anchorMin = Vector2.zero;
            iconRect.anchorMax = Vector2.one;
            iconRect.sizeDelta = Vector2.zero;

            return fallbackObj;
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

            // Apply LayerLab font if available
            var layerLabConfig = LoadLayerLabConfig();
            if (layerLabConfig != null && layerLabConfig.FontAfacadFlux != null)
            {
                tmpText.font = layerLabConfig.FontAfacadFlux;
            }

            return textObj;
        }
    }
}
