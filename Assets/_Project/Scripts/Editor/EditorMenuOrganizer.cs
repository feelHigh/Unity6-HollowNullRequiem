// ============================================
// EditorMenuOrganizer.cs
// Organized HNR menu structure for editor tools
// ============================================

using UnityEditor;
using UnityEngine;

namespace HNR.Editor
{
    /// <summary>
    /// Provides an organized menu structure for all HNR editor tools.
    /// This class creates a clean, hierarchical menu that calls existing generators.
    ///
    /// Menu Structure:
    /// HNR/
    /// ├── 1. Data Assets/     (priority 10-29) - Content generation
    /// ├── 2. Prefabs/         (priority 30-49) - UI and visual prefabs
    /// ├── 3. Production/      (priority 50-69) - Production setup
    /// ├── 4. Audio & VFX/     (priority 70-79) - Audio/VFX configuration
    /// ├── 5. Scenes/          (priority 80-139) - Test and production scenes
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
        // 5. Scenes (priority 80-139)
        // ============================================

        // Test Scenes (80-99)
        [MenuItem("HNR/5. Scenes/Test/Combat Test Scene", priority = 80)]
        public static void GenerateCombatTestScene()
        {
            CombatTestSceneGenerator.GenerateCombatTestScene();
        }

        [MenuItem("HNR/5. Scenes/Test/Card Balance Test Scene", priority = 81)]
        public static void GenerateCardBalanceTestScene()
        {
            CardBalanceTestSceneGenerator.GenerateCardBalanceTestScene();
        }

        [MenuItem("HNR/5. Scenes/Test/Requiem Selection Scene", priority = 82)]
        public static void GenerateRequiemSelectionScene()
        {
            RequiemSelectionSceneGenerator.GenerateRequiemSelectionScene();
        }

        // Production Scenes (100-139)
        [MenuItem("HNR/5. Scenes/Production/Setup All Scenes", priority = 100)]
        public static void SetupAllProductionScenes()
        {
            ProductionSceneSetupGenerator.SetupAllScenes();
        }

        [MenuItem("HNR/5. Scenes/Production/1. Setup Boot Scene", priority = 110)]
        public static void SetupBootScene()
        {
            ProductionSceneSetupGenerator.SetupBootScene();
        }

        [MenuItem("HNR/5. Scenes/Production/2. Setup MainMenu Scene", priority = 111)]
        public static void SetupMainMenuScene()
        {
            ProductionSceneSetupGenerator.SetupMainMenuScene();
        }

        [MenuItem("HNR/5. Scenes/Production/3. Setup Bastion Scene", priority = 112)]
        public static void SetupBastionScene()
        {
            ProductionSceneSetupGenerator.SetupBastionScene();
        }

        [MenuItem("HNR/5. Scenes/Production/4. Setup NullRift Scene", priority = 113)]
        public static void SetupNullRiftScene()
        {
            ProductionSceneSetupGenerator.SetupNullRiftScene();
        }

        [MenuItem("HNR/5. Scenes/Production/5. Setup Combat Scene", priority = 114)]
        public static void SetupCombatScene()
        {
            ProductionSceneSetupGenerator.SetupCombatScene();
        }

        [MenuItem("HNR/5. Scenes/Production/Configure Build Settings", priority = 130)]
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

        [MenuItem("HNR/6. Utilities/Show Menu Organization", priority = 201)]
        public static void ShowMenuOrganization()
        {
            Debug.Log(@"
[EditorMenuOrganizer] HNR Menu Structure:

HNR/
├── 1. Data Assets/         (10-29)  - Content generation
│   ├── Generate All Placeholder Assets
│   ├── Requiems/ (Kira, Mordren, Elara, Thornwick, Art)
│   ├── Cards/ (Shared, Upgraded)
│   ├── Enemies & Encounters/
│   ├── Events/ (Echo Events, Zone Config)
│   └── Relics
│
├── 2. Prefabs/             (30-49)  - UI and visual prefabs
│   ├── UI/ (Card, DamageNumber, EnemySlotUI, Meta-Game)
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
├── 5. Scenes/              (80-139) - Test and production scenes
│   ├── Test/ (Combat, Card Balance, Requiem Selection)
│   └── Production/ (Boot, MainMenu, Bastion, NullRift, Combat)
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
