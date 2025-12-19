// ============================================
// EditorMenuOrganizer.cs
// Organized HNR menu structure for editor tools
// ============================================

using UnityEditor;
using UnityEngine;
using HNR.Combat;

namespace HNR.Editor
{
    /// <summary>
    /// Provides an organized menu structure for all HNR editor tools.
    /// This class is the single source of truth for HNR menu items.
    ///
    /// Menu Structure:
    /// HNR/
    /// ├── 1. Data Assets/     (priority 10-29) - Content generation
    /// ├── 2. Prefabs/         (priority 30-49) - UI and visual prefabs
    /// ├── 3. Production/      (priority 50-69) - Production setup
    /// ├── 4. Audio & VFX/     (priority 70-79) - Audio/VFX configuration
    /// ├── 5. Scenes/          (priority 100-139) - Production scenes
    /// └── 6. Utilities/       (priority 200+) - Verification and utilities
    /// </summary>
    public static class EditorMenuOrganizer
    {
        // ============================================
        // 1. Data Assets (priority 10-29)
        // ============================================

        [MenuItem("HNR/1. Data Assets/Generate All Placeholder Assets", priority = 10)]
        public static void GenerateAllPlaceholderAssets()
        {
            PlaceholderAssetGenerator.GenerateAll();
        }

        [MenuItem("HNR/1. Data Assets/Requiems/Generate All Requiems", priority = 11)]
        public static void GenerateAllRequiems()
        {
            PlaceholderAssetGenerator.GenerateRequiemsOnly();
        }

        [MenuItem("HNR/1. Data Assets/Requiems/Kira", priority = 12)]
        public static void GenerateKiraAssets()
        {
            KiraAssetGenerator.GenerateKiraAssets();
        }

        [MenuItem("HNR/1. Data Assets/Requiems/Mordren", priority = 13)]
        public static void GenerateMordrenAssets()
        {
            MordrenAssetGenerator.GenerateMordrenAssets();
        }

        [MenuItem("HNR/1. Data Assets/Requiems/Elara", priority = 14)]
        public static void GenerateElaraAssets()
        {
            ElaraAssetGenerator.GenerateElaraAssets();
        }

        [MenuItem("HNR/1. Data Assets/Requiems/Thornwick", priority = 15)]
        public static void GenerateThornwickAssets()
        {
            ThornwickAssetGenerator.GenerateThornwickAssets();
        }

        [MenuItem("HNR/1. Data Assets/Requiems/Requiem Art (All)", priority = 16)]
        public static void GenerateRequiemArtAssets()
        {
            RequiemArtAssetGenerator.GenerateAllRequiemArts();
        }

        [MenuItem("HNR/1. Data Assets/Cards/Shared Cards", priority = 17)]
        public static void GenerateSharedCards()
        {
            SharedCardsAssetGenerator.GenerateSharedCards();
        }

        [MenuItem("HNR/1. Data Assets/Cards/Upgraded Cards", priority = 18)]
        public static void GenerateUpgradedCards()
        {
            UpgradedCardsAssetGenerator.GenerateUpgradedCards();
        }

        [MenuItem("HNR/1. Data Assets/Enemies & Encounters/Generate All", priority = 19)]
        public static void GenerateAllEncounterAssets()
        {
            EncounterAssetGenerator.GenerateAllAssets();
        }

        [MenuItem("HNR/1. Data Assets/Enemies & Encounters/Enemies Only", priority = 20)]
        public static void GenerateEnemiesOnly()
        {
            PlaceholderAssetGenerator.GenerateEnemiesOnly();
        }

        [MenuItem("HNR/1. Data Assets/Enemies & Encounters/Encounters Only", priority = 21)]
        public static void GenerateEncountersOnly()
        {
            EncounterAssetGenerator.GenerateEncountersMenuItem();
        }

        [MenuItem("HNR/1. Data Assets/Events/Echo Events", priority = 22)]
        public static void GenerateEchoEvents()
        {
            EchoEventGenerator.GenerateTestEchoEvents();
        }

        [MenuItem("HNR/1. Data Assets/Events/Zone Config", priority = 23)]
        public static void GenerateZoneConfig()
        {
            EchoEventGenerator.GenerateTestZoneConfig();
        }

        [MenuItem("HNR/1. Data Assets/Relics", priority = 24)]
        public static void GenerateRelicAssets()
        {
            RelicAssetGenerator.GenerateRelicAssets();
        }

        [MenuItem("HNR/1. Data Assets/Production/Generate All Zone Data", priority = 25)]
        public static void GenerateAllZoneData()
        {
            ProductionDataGenerator.GenerateAllZoneData();
        }

        [MenuItem("HNR/1. Data Assets/Production/Zone Configs Only", priority = 26)]
        public static void GenerateZoneConfigsOnly()
        {
            ProductionDataGenerator.GenerateZoneConfigsOnly();
        }

        [MenuItem("HNR/1. Data Assets/Production/Zone 2 & 3 Encounters", priority = 27)]
        public static void GenerateZone2And3Encounters()
        {
            ProductionDataGenerator.GenerateZone2And3Encounters();
        }

        // ============================================
        // 2. Prefabs (priority 30-49)
        // ============================================

        [MenuItem("HNR/2. Prefabs/UI/Card Prefab", priority = 30)]
        public static void GenerateCardPrefab()
        {
            CardPrefabGenerator.GenerateCardPrefab();
        }

        [MenuItem("HNR/2. Prefabs/UI/CombatCard Prefab (CZN)", priority = 30)]
        public static void GenerateCombatCardPrefab()
        {
            CombatCardPrefabGenerator.GenerateCombatCardPrefab();
        }

        [MenuItem("HNR/2. Prefabs/UI/DamageNumber Prefab", priority = 31)]
        public static void GenerateDamageNumberPrefab()
        {
            DamageNumberPrefabGenerator.GeneratePrefab();
        }

        [MenuItem("HNR/2. Prefabs/UI/EnemySlotUI Prefab", priority = 32)]
        public static void GenerateEnemySlotUIPrefab()
        {
            EnemySlotUIPrefabGenerator.GeneratePrefab();
        }

        [MenuItem("HNR/2. Prefabs/UI/Meta-Game UI (All)", priority = 33)]
        public static void GenerateAllMetaGameUI()
        {
            MetaGameUIPrefabGenerator.GenerateAllPrefabs();
        }

        [MenuItem("HNR/2. Prefabs/UI/Meta-Game UI/Toast Only", priority = 34)]
        public static void GenerateToastPrefab()
        {
            MetaGameUIPrefabGenerator.GenerateToastPrefab();
        }

        [MenuItem("HNR/2. Prefabs/UI/Meta-Game UI/GlobalHeader Only", priority = 35)]
        public static void GenerateGlobalHeaderPrefab()
        {
            MetaGameUIPrefabGenerator.GenerateGlobalHeaderPrefab();
        }

        [MenuItem("HNR/2. Prefabs/UI/Meta-Game UI/GlobalNavDock Only", priority = 36)]
        public static void GenerateGlobalNavDockPrefab()
        {
            MetaGameUIPrefabGenerator.GenerateGlobalNavDockPrefab();
        }

        [MenuItem("HNR/2. Prefabs/UI/Meta-Game UI/CurrencyTicker Only", priority = 37)]
        public static void GenerateCurrencyTickerPrefab()
        {
            MetaGameUIPrefabGenerator.GenerateCurrencyTickerPrefab();
        }

        [MenuItem("HNR/2. Prefabs/UI/Combat UI (All)", priority = 38)]
        public static void GenerateAllCombatUIPrefabs()
        {
            CombatUIPrefabGenerator.GenerateAll();
        }

        [MenuItem("HNR/2. Prefabs/UI/Combat UI/EnemyFloatingUI Only", priority = 39)]
        public static void GenerateEnemyFloatingUIPrefab()
        {
            CombatUIPrefabGenerator.GenerateEnemyFloatingUI();
        }

        [MenuItem("HNR/2. Prefabs/UI/Combat UI/AllyIndicator Only", priority = 39)]
        public static void GenerateAllyIndicatorPrefab()
        {
            CombatUIPrefabGenerator.GenerateAllyIndicator();
        }

        [MenuItem("HNR/2. Prefabs/Characters/Generate Visual Prefabs", priority = 40)]
        public static void GenerateCharacterVisualPrefabs()
        {
            CharacterVisualPrefabGenerator.GenerateAllPrefabs();
        }

        [MenuItem("HNR/2. Prefabs/Characters/Create Empty Visual", priority = 41)]
        public static void CreateEmptyVisualPrefab()
        {
            CharacterVisualPrefabGenerator.CreateEmptyVisualPrefab();
        }

        [MenuItem("HNR/2. Prefabs/Characters/Link to Data Assets", priority = 42)]
        public static void AssignVisualPrefabsToData()
        {
            CharacterVisualPrefabGenerator.AssignPrefabsToDataAssets();
        }

        // ============================================
        // 3. Production Setup (priority 50-69)
        // ============================================

        [MenuItem("HNR/3. Production/Complete Production Finalization", priority = 49)]
        public static void CompleteProductionFinalization()
        {
            if (!EditorUtility.DisplayDialog("Complete Production Finalization",
                "This will run ALL production setup steps:\n\n" +
                "1. Create all prefabs (Card, MapNode, Enemy, Character)\n" +
                "2. Link visual prefabs to data assets\n" +
                "3. Generate Audio & VFX configs\n" +
                "4. Setup all 5 production scenes\n" +
                "5. Configure build settings\n\n" +
                "This may take a few minutes. Continue?",
                "Yes, Run All", "Cancel"))
            {
                return;
            }

            Debug.Log("[Production Finalization] Starting complete production setup...");

            // Step 1: Create prefabs
            EditorUtility.DisplayProgressBar("Production Finalization", "Creating prefabs...", 0.1f);
            ProductionSetupTool.CreateAllPrefabs();

            // Step 2: Link visual prefabs to data
            EditorUtility.DisplayProgressBar("Production Finalization", "Linking visual prefabs...", 0.25f);
            ProductionSetupTool.LinkAllVisualPrefabs();

            // Step 2b: Wire enemy visuals with AnimatedCharacterVisual
            EditorUtility.DisplayProgressBar("Production Finalization", "Wiring enemy visuals...", 0.35f);
            EnemyVisualWiringTool.WireAllEnemyVisuals();

            // Step 3: Generate Audio & VFX configs
            EditorUtility.DisplayProgressBar("Production Finalization", "Generating Audio & VFX configs...", 0.5f);
            AudioVFXConfigGenerator.GenerateAllConfigs();

            // Step 4: Setup all scenes
            EditorUtility.DisplayProgressBar("Production Finalization", "Setting up scenes...", 0.7f);
            ProductionSceneSetupGenerator.SetupAllScenes();

            // Step 5: Configure build settings
            EditorUtility.DisplayProgressBar("Production Finalization", "Configuring build settings...", 0.9f);
            ProductionSceneSetupGenerator.ConfigureBuildSettings();

            EditorUtility.ClearProgressBar();

            Debug.Log("[Production Finalization] Complete!");
            EditorUtility.DisplayDialog("Production Finalization Complete",
                "All production setup steps completed:\n\n" +
                "- Prefabs created\n" +
                "- Visual prefabs linked to data\n" +
                "- Audio & VFX configs generated\n" +
                "- All 5 scenes setup\n" +
                "- Build settings configured\n\n" +
                "Run integration tests to verify everything works.",
                "OK");
        }

        [MenuItem("HNR/3. Production/Run Full Setup", priority = 50)]
        public static void RunFullProductionSetup()
        {
            ProductionSetupTool.RunFullSetup();
        }

        [MenuItem("HNR/3. Production/Create All Prefabs", priority = 51)]
        public static void CreateAllProductionPrefabs()
        {
            ProductionSetupTool.CreateAllPrefabs();
        }

        [MenuItem("HNR/3. Production/Link Visual Prefabs to Data", priority = 52)]
        public static void LinkVisualPrefabsToData()
        {
            ProductionSetupTool.LinkAllVisualPrefabs();
        }

        [MenuItem("HNR/3. Production/Setup Requiem Visuals (Add Component + Link)", priority = 53)]
        public static void SetupRequiemVisuals()
        {
            ProductionSetupTool.SetupAndLinkRequiemVisuals();
        }

        [MenuItem("HNR/3. Production/Setup Enemy Visuals (HeroEditor)", priority = 54)]
        public static void SetupEnemyVisuals()
        {
            ProductionSetupTool.SetupAndLinkEnemyVisuals();
        }

        [MenuItem("HNR/3. Production/Verify Enemy Visuals", priority = 55)]
        public static void VerifyEnemyVisuals()
        {
            ProductionSetupTool.VerifyEnemyVisuals();
        }

        // ============================================
        // 4. Audio & VFX (priority 70-79)
        // ============================================

        [MenuItem("HNR/4. Audio & VFX/Generate All Configs", priority = 70)]
        public static void GenerateAllAudioVFXConfigs()
        {
            AudioVFXConfigGenerator.GenerateAllConfigs();
        }

        [MenuItem("HNR/4. Audio & VFX/Generate Audio Config", priority = 71)]
        public static void GenerateAudioConfig()
        {
            AudioVFXConfigGenerator.GenerateAudioConfig();
        }

        [MenuItem("HNR/4. Audio & VFX/Generate VFX Prefabs", priority = 72)]
        public static void GenerateVFXPrefabs()
        {
            AudioVFXConfigGenerator.GenerateVFXPrefabs();
        }

        [MenuItem("HNR/4. Audio & VFX/Create VFXPoolManager Config", priority = 73)]
        public static void CreateVFXPoolManagerConfig()
        {
            AudioVFXConfigGenerator.CreateVFXPoolManagerConfig();
        }

        // ============================================
        // 5. Scenes (priority 100-139)
        // ============================================

        [MenuItem("HNR/5. Scenes/Setup All Scenes", priority = 100)]
        public static void SetupAllProductionScenes()
        {
            ProductionSceneSetupGenerator.SetupAllScenes();
        }

        [MenuItem("HNR/5. Scenes/1. Setup Boot Scene", priority = 110)]
        public static void SetupBootScene()
        {
            ProductionSceneSetupGenerator.SetupBootScene();
        }

        [MenuItem("HNR/5. Scenes/2. Setup MainMenu Scene", priority = 111)]
        public static void SetupMainMenuScene()
        {
            ProductionSceneSetupGenerator.SetupMainMenuScene();
        }

        [MenuItem("HNR/5. Scenes/3. Setup Bastion Scene", priority = 112)]
        public static void SetupBastionScene()
        {
            ProductionSceneSetupGenerator.SetupBastionScene();
        }

        [MenuItem("HNR/5. Scenes/4. Setup NullRift Scene", priority = 113)]
        public static void SetupNullRiftScene()
        {
            ProductionSceneSetupGenerator.SetupNullRiftScene();
        }

        [MenuItem("HNR/5. Scenes/5. Setup Combat Scene", priority = 114)]
        public static void SetupCombatScene()
        {
            ProductionSceneSetupGenerator.SetupCombatScene();
        }

        [MenuItem("HNR/5. Scenes/Configure Build Settings", priority = 130)]
        public static void ConfigureBuildSettings()
        {
            ProductionSceneSetupGenerator.ConfigureBuildSettings();
        }

        // ============================================
        // 6. Utilities (priority 200+)
        // ============================================

        [MenuItem("HNR/6. Utilities/Verify Relic Assets", priority = 200)]
        public static void VerifyRelicAssets()
        {
            RelicAssetGenerator.VerifyRelicAssets();
        }

        [MenuItem("HNR/6. Utilities/Fix Enemy Visual Prefabs", priority = 202)]
        public static void FixEnemyVisualPrefabs()
        {
            string prefabsPath = "Assets/_Project/Prefabs/Characters/Enemies";
            string[] guids = AssetDatabase.FindAssets("t:Prefab", new[] { prefabsPath });

            Sprite placeholderSprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");

            int fixedCount = 0;
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var prefabContents = PrefabUtility.LoadPrefabContents(path);

                var spriteRenderer = prefabContents.GetComponent<SpriteRenderer>();
                if (spriteRenderer != null)
                {
                    // Assign placeholder sprite if missing
                    if (spriteRenderer.sprite == null)
                    {
                        spriteRenderer.sprite = placeholderSprite;
                    }
                    fixedCount++;
                }

                // Scale up the prefab to be visible (0.16 units -> ~2 units)
                prefabContents.transform.localScale = new Vector3(12f, 12f, 1f);

                // Wire SimpleCharacterVisual._renderer if present
                var simpleVisual = prefabContents.GetComponent<HNR.Characters.Visuals.SimpleCharacterVisual>();
                if (simpleVisual != null)
                {
                    var so = new SerializedObject(simpleVisual);
                    if (so.FindProperty("_renderer").objectReferenceValue == null && spriteRenderer != null)
                    {
                        so.FindProperty("_renderer").objectReferenceValue = spriteRenderer;
                        so.ApplyModifiedPropertiesWithoutUndo();
                    }
                }

                PrefabUtility.SaveAsPrefabAsset(prefabContents, path);
                PrefabUtility.UnloadPrefabContents(prefabContents);
            }

            Debug.Log($"[EditorMenuOrganizer] Fixed {fixedCount} enemy visual prefabs (sprite + scale)");

            // Also fix EnemyDataSO sprite scales
            FixEnemyDataSpriteScales();
        }

        private static void FixEnemyDataSpriteScales()
        {
            string[] enemyPaths = new[]
            {
                "Assets/_Project/Data/Enemies/Zone1",
                "Assets/_Project/Data/Enemies/Zone2",
                "Assets/_Project/Data/Enemies/Zone3",
                "Assets/_Project/Data/Enemies/Elites",
                "Assets/_Project/Data/Enemies/Bosses",
                "Assets/_Project/Data/Enemies"
            };

            int fixedCount = 0;
            foreach (string folder in enemyPaths)
            {
                string[] guids = AssetDatabase.FindAssets("t:EnemyDataSO", new[] { folder });
                foreach (string guid in guids)
                {
                    string path = AssetDatabase.GUIDToAssetPath(guid);
                    var enemyData = AssetDatabase.LoadAssetAtPath<EnemyDataSO>(path);
                    if (enemyData != null)
                    {
                        var so = new SerializedObject(enemyData);
                        var scaleProperty = so.FindProperty("_spriteScale");
                        if (scaleProperty != null && scaleProperty.floatValue < 10f)
                        {
                            scaleProperty.floatValue = 12f;
                            so.ApplyModifiedPropertiesWithoutUndo();
                            EditorUtility.SetDirty(enemyData);
                            fixedCount++;
                        }
                    }
                }
            }

            AssetDatabase.SaveAssets();
            Debug.Log($"[EditorMenuOrganizer] Fixed {fixedCount} EnemyDataSO sprite scales to 12x");
        }

        [MenuItem("HNR/6. Utilities/Fix Combat Scene Background", priority = 204)]
        public static void FixCombatSceneBackground()
        {
            // Find Background object in current scene and disable it
            var background = GameObject.Find("Background");
            if (background != null)
            {
                var image = background.GetComponent<UnityEngine.UI.Image>();
                if (image != null)
                {
                    // Make it transparent instead of deleting (preserve structure)
                    image.color = new Color(0, 0, 0, 0);
                    EditorUtility.SetDirty(background);
                    Debug.Log("[EditorMenuOrganizer] Combat scene Background made transparent");
                }
            }
            else
            {
                Debug.Log("[EditorMenuOrganizer] No Background object found in current scene");
            }

            // Mark scene dirty
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
                UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
        }

        [MenuItem("HNR/6. Utilities/Fix EnemyInstance Prefab Wiring", priority = 205)]
        public static void FixEnemyInstancePrefab()
        {
            string prefabPath = "Assets/_Project/Prefabs/Combat/EnemyInstance.prefab";
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);

            if (prefab == null)
            {
                Debug.LogError("[EditorMenuOrganizer] EnemyInstance.prefab not found. Run 'Create All Prefabs' first.");
                return;
            }

            // Load prefab for editing
            var prefabContents = PrefabUtility.LoadPrefabContents(prefabPath);
            var enemy = prefabContents.GetComponent<EnemyInstance>();

            if (enemy == null)
            {
                Debug.LogError("[EditorMenuOrganizer] EnemyInstance component not found on prefab");
                PrefabUtility.UnloadPrefabContents(prefabContents);
                return;
            }

            // Add BoxCollider2D if missing (required for targeting raycast)
            var collider = prefabContents.GetComponent<BoxCollider2D>();
            if (collider == null)
            {
                collider = prefabContents.AddComponent<BoxCollider2D>();
                collider.size = new Vector2(2f, 2f);
                collider.isTrigger = true;
                Debug.Log("[EditorMenuOrganizer] Added BoxCollider2D for targeting");
            }

            // Find existing sprite renderer
            var spriteRenderer = prefabContents.GetComponentInChildren<SpriteRenderer>();

            // Find or create highlight ring
            var highlightRing = prefabContents.transform.Find("HighlightRing");
            if (highlightRing == null)
            {
                var ringObj = new GameObject("HighlightRing");
                ringObj.transform.SetParent(prefabContents.transform, false);
                var ringSprite = ringObj.AddComponent<SpriteRenderer>();
                ringSprite.color = new Color(1f, 1f, 0f, 0.5f);
                ringSprite.sortingOrder = -1;
                ringObj.SetActive(false);
                highlightRing = ringObj.transform;
            }

            // Wire references
            var so = new SerializedObject(enemy);
            so.FindProperty("_sprite").objectReferenceValue = spriteRenderer;
            so.FindProperty("_highlightRing").objectReferenceValue = highlightRing.gameObject;
            so.ApplyModifiedPropertiesWithoutUndo();

            // Save prefab
            PrefabUtility.SaveAsPrefabAsset(prefabContents, prefabPath);
            PrefabUtility.UnloadPrefabContents(prefabContents);

            Debug.Log("[EditorMenuOrganizer] EnemyInstance prefab wiring fixed successfully!");
        }

        [MenuItem("HNR/6. Utilities/Show Menu Organization", priority = 201)]
        public static void ShowMenuOrganization()
        {
            Debug.Log(@"
[EditorMenuOrganizer] HNR Production Menu Structure:

HNR/
├── 1. Data Assets/         (10-29)  - Content generation
│   ├── Generate All Placeholder Assets
│   ├── Requiems/ (Kira, Mordren, Elara, Thornwick, Art)
│   ├── Cards/ (Shared, Upgraded)
│   ├── Enemies & Encounters/
│   ├── Events/ (Echo Events, Zone Config)
│   ├── Relics
│   └── Production/ (Zone Data, Zone Configs, Zone 2&3 Encounters)
│
├── 2. Prefabs/             (30-49)  - UI and visual prefabs
│   ├── UI/ (Card, DamageNumber, EnemySlotUI, Meta-Game, Combat UI)
│   └── Characters/ (Visual Prefabs, Create Empty, Link to Data)
│
├── 3. Production/          (50-69)  - Production setup
│   ├── Run Full Setup
│   ├── Create All Prefabs
│   └── Link Visual Prefabs to Data
│
├── 4. Audio & VFX/         (70-79)  - Audio/VFX configuration
│   ├── Generate All Configs
│   ├── Generate Audio Config
│   ├── Generate VFX Prefabs
│   └── Create VFXPoolManager Config
│
├── 5. Scenes/              (100-139) - Production scenes
│   ├── Setup All Scenes
│   ├── 1-5. Setup Individual Scenes
│   └── Configure Build Settings
│
└── 6. Utilities/           (200+)   - Verification tools
    ├── Verify Relic Assets
    └── Show Menu Organization

Build/ (separate menu)
├── Android/ (Development, Release, AAB)
├── Version/ (Increment, Show Current)
└── Builds Folder/ (Open, Clean, Validate)
");
        }
    }
}
