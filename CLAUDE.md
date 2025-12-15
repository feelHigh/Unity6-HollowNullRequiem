# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

Hollow Null Requiem (HNR) is a roguelike deckbuilder game built with Unity 6 (6000.0.63f1) targeting Android (API 24+, ARM64). It uses 2D URP rendering pipeline.

**Technical Design Documents** are located at `Docs/TDD/` - reference these for implementation specifications. The TDD index is at `Docs/TDD/00_TDD_Overview.md`.

**UI Mockup** at `Docs/HollowNullRequiem_Mockup.jsx` (v4.0) - Interactive React mockup showing all screens with CZN layout. Reference for visual implementation of Combat Screen, Card Fan, and all UI components.

## Development Environment

This project uses AI-assisted development with MCP (Model Context Protocol):
1. Open Unity project
2. Window > MCP for Unity > Start Local HTTP Server (verify green "Session Active" indicator)
3. Use Claude Code with Synaptic AI Pro MCP tools for direct Unity Editor control

See `Docs/Guides/Workflow_Transition_Guide.md` for detailed setup instructions.

## Build & Test

Unity project - use Unity Editor (no CLI build):
- **Build:** File > Build Settings > Build (Android target)
- **Play Mode:** Press Play or Ctrl+P
- **Run Tests:** Window > General > Test Runner

## Dependencies

- **DOTween** - UI animations (HandManager card layout, screen transitions)
- **Easy Save 3** - Persistence system (SaveManager)
- **TextMeshPro** - UI text rendering

## Editor Tools (HNR Menu)

| Menu Item | Script | Purpose |
|-----------|--------|---------|
| Generate Placeholder Assets | `PlaceholderAssetGenerator.cs` | Test Requiems, Cards, Enemies |
| Generate Combat Test Scene | `CombatTestSceneGenerator.cs` | Combat test environment |
| Card Balance Test Scene | `CardBalanceTestSceneGenerator.cs` | Card effectiveness testing |
| Requiem Selection Scene | `RequiemSelectionSceneGenerator.cs` | Team selection UI test |

## Architecture Patterns

### Service Locator (`HNR.Core.ServiceLocator`)
Global service access without tight coupling. Services are registered by interface type.
```csharp
ServiceLocator.Register<IGameManager>(this);
var gm = ServiceLocator.Get<IGameManager>();
ServiceLocator.TryGet<IGameManager>(out var gm); // Non-throwing variant
ServiceLocator.Has<IGameManager>(); // Check if registered
```

### Event Bus (`HNR.Core.Events.EventBus`)
Pub-sub pattern for decoupled system communication. All events derive from `GameEvent`.
```csharp
EventBus.Subscribe<GameStateChangedEvent>(OnStateChanged);
EventBus.Publish(new GameStateChangedEvent(prev, next));
EventBus.Unsubscribe<GameStateChangedEvent>(OnStateChanged);
```
Best practice: Subscribe in `OnEnable`, unsubscribe in `OnDisable`.

### State Machine (`HNR.Core.GameStates`)
Game flow: `Boot -> MainMenu -> Bastion -> Run <-> Combat -> Results`

Each state implements `IGameState` with `Enter()`, `Update()`, `Exit()` methods. GameManager orchestrates transitions and publishes `GameStateChangedEvent`.

### Object Pooling (`HNR.Core.PoolManager`)
Generic pooling for frequently instantiated objects:
```csharp
poolManager.RegisterPrefab<T>(prefab);  // Register at startup
poolManager.PreWarm<T>(count);          // Optional pre-allocation
var obj = poolManager.Get<T>();         // Get from pool
poolManager.Return<T>(obj);             // Return to pool
```
Pooled objects must implement `IPoolable` with `OnSpawnFromPool()` and `OnReturnToPool()`.

### Save System (`HNR.Core.SaveManager`)
Uses Easy Save 3 (ES3) for persistence:
```csharp
saveManager.SaveRun(runData);     // Save current run
saveManager.LoadRun();            // Load saved run
saveManager.HasSavedRun;          // Check for save
saveManager.SaveSettings(data);   // Save player settings
```

### Combat System (`HNR.Combat`)
Phase-based state machine for turn-based combat:
```csharp
// Combat flow: Setup -> DrawPhase -> PlayerPhase -> EndPhase -> EnemyPhase -> DrawPhase...
turnManager.StartCombat(team, enemies);
turnManager.TransitionToPhase(CombatPhase.PlayerPhase);
turnManager.TryPlayCard(card, target);
turnManager.EndPlayerTurn();
```

Each phase implements `ICombatPhase` with `Enter()`, `Update()`, `Exit()`, `GetNextPhase()`. `CombatContext` holds shared state (team HP, AP, enemies, deck/hand managers).

Key managers:
- `TurnManager` - Orchestrates phase transitions, card playing, damage/healing
- `DeckManager` - Draw pile, discard pile, exhaust pile operations
- `HandManager` - Card state management (draw, discard, selection)
- `CardFanLayout` - CZN curved 30° fan visual layout with hover lift, DOTween animations
- `CombatManager` - High-level facade simplifying common combat operations
- `RequiemArtExecutor` - Executes Requiem Art effects using card effect system

### CombatManager Facade (`HNR.Combat.CombatManager`)
High-level API for combat operations, simplifying common tasks:
```csharp
var cm = CombatManager.Instance;
cm.StartCombat(team, enemies);       // Start combat with Team wrapper
cm.TryPlayCard(card, target);        // Play a card
cm.DealDamageToAllEnemies(10);       // AoE damage
cm.AddBlock(5);                      // Add team block
cm.TryActivateArt(requiem);          // Activate Requiem Art
cm.EndPlayerTurn();                  // End turn
```

### Team Wrapper (`HNR.Characters.Team`)
Wrapper class for party of 3 Requiems with utility methods:
```csharp
var team = new Team(requiemList);
team.Add(requiem);                   // Add member (max 3)
team.GetByAspect(SoulAspect.Flame);  // Query by aspect
team.GetHighestCorruption();         // Get most corrupted
team.AnyInNullState;                 // Check Null State
team.CollectStartingCards();         // Get all starting cards
team.PostCombatCleanup();            // Reset after combat
// Implicit conversion to List<RequiemInstance> for compatibility
```

### StatBlock System (`HNR.Characters.StatBlock`)
Stat calculations with modifier support for buff/debuff systems:
```csharp
var stats = new StatBlock(requiemData);
stats.AddModifier(StatModifier.Flat(StatType.ATK, 5, source));
stats.AddModifier(StatModifier.PercentAdd(StatType.DEF, 0.2f, source));
int finalATK = stats.ATK;            // Calculated with modifiers
stats.RemoveModifiersFromSource(source);
stats.ClearAllModifiers();
```
Modifier order: Base + Flat → × (1 + PercentAdd) → × PercentMult

### Requiem Art Execution (`HNR.Combat.RequiemArtExecutor`)
Executes ultimate abilities using the card effect system:
```csharp
var executor = RequiemArtExecutor.Instance;
executor.ExecuteArt(requiem, artData, target);
// Handles: targeting, VFX, audio, effect execution
// +50% damage bonus when Requiem is in Null State
```

### Card Effect System (`HNR.Cards.Effects`)
Effects are registered handlers executed by `CardExecutor`:
```csharp
// ICardEffect interface implementation
public class DamageEffect : ICardEffect
{
    public void Execute(CardEffectData effectData, EffectContext context) { }
}
// Effects auto-registered by EffectType enum in CardExecutor.RegisterEffects()
```

### VFX Pool System (`HNR.VFX`)
Named effect pooling for particle effects:
```csharp
var pool = ServiceLocator.Get<VFXPoolManager>();
var instance = pool.Spawn("hit_flame", position, rotation);  // Returns VFXInstance
instance.SetColor(Color.red);   // Tint particles
instance.SetScale(1.5f);        // Scale effect
pool.Return(instance);          // Return to pool
pool.SpawnAttached("effect", transform);  // Follow target
```
Effects configured via `VFXPoolConfig` with pre-warm count and max active limits. `VFXInstance` wraps ParticleSystem with auto-return on completion.

### Feedback Systems (`HNR.Audio`, `HNR.Core`)
Combat feedback is coordinated across multiple systems:
- **CombatAudioController** - Subscribes to combat events, plays context-appropriate SFX
- **CombatVFXController** - Spawns VFX on damage/status events based on SoulAspect
- **HapticController** - Mobile vibration with Light/Medium/Heavy intensities
- **ScreenShakeController** - Camera shake via DOTween
- **CombatFeedbackIntegrator** - Coordinates all above for unified feedback triggers

Each controller can work independently (via EventBus) or be orchestrated by CombatFeedbackIntegrator for complex combined feedback.

### Initialization Order
1. `ServiceLocator.Initialize()`
2. `SaveManager.Initialize()` - non-MonoBehaviour service
3. `GameManager.Awake()` - registers self, initializes states
4. Other managers (UIManager, PoolManager, AudioManager) - self-register in Awake
5. `GameManager.Start()` - enters Boot state

GameBootstrap handles instantiation and wiring of all managers.

## Namespace Structure

- `HNR.Core` - ServiceLocator, GameManager, PoolManager, GameState enum
- `HNR.Core.Events` - EventBus, GameEvent, all event classes
- `HNR.Core.GameStates` - IGameState and state implementations
- `HNR.Core.Interfaces` - Service interfaces (IGameManager, IAudioManager, IPoolManager, ISaveManager)
- `HNR.Core.Save` - SaveManager (non-MonoBehaviour service)
- `HNR.Cards` - Card system (CardDataSO, CardInstance, CardEnums)
- `HNR.Cards.Effects` - ICardEffect and effect implementations (DamageEffect, BlockEffect, etc.)
- `HNR.Characters` - Character system (RequiemDataSO, RequiemArtDataSO, RequiemInstance, CorruptionManager, NullStateHandler) - Note: files in `Characters/Data/` folder also use this namespace
- `HNR.Combat` - Combat system (TurnManager, DeckManager, HandManager, EncounterManager, CombatContext)
- `HNR.Combat.Events` - Combat events (APChangedEvent, TeamHPChangedEvent, CorruptionChangedEvent, etc.)
- `HNR.Combat.Phases` - ICombatPhase implementations (SetupPhase, DrawPhase, PlayerPhase, etc.)
- `HNR.Map` - Map system (MapManager, MapGenerator, EchoEventManager, MapNodeData, ZoneConfigSO)
- `HNR.UI` - UI base classes (UIManager, ScreenBase)
- `HNR.UI.Screens` - Screen implementations (CombatScreen, RequiemSelectionScreen, MapScreen, EchoEventScreen, ShopScreen, MainMenuScreen)
- `HNR.UI.Combat` - Combat-specific UI (CorruptionBarUI, EnemySlotUI, SharedVitalityBarCZN, PartyStatusSidebar, APCounterDisplay, ExecutionButton, CardFanLayout)
- `HNR.UI.Map` - Map UI components (MapNodeUI, MapPathRenderer)
- `HNR.UI.Components` - Reusable UI components (RequiemSlotUI, CurrencyTicker)
- `HNR.UI.Toast` - Toast notification system (ToastManager, ToastController)
- `HNR.UI.Utilities` - UI utilities (UIColors)
- `HNR.UI.Effects` - Visual effect components (DamageNumber, DamageNumberSpawner)
- `HNR.Audio` - Audio system (AudioManager, CombatAudioController, HapticController, AudioConfigSO)
- `HNR.VFX` - Visual effects system (VFXPoolManager, VFXInstance, CombatVFXController)
- `HNR.Progression` - Shop system (ShopManager, ShopGenerator, ShopConfigSO), Save data (RunSaveData, SettingsData)
- `HNR.Editor` - Editor tools (PlaceholderAssetGenerator, scene generators)
- `HNR.Diagnostics` - Performance monitoring and QA tools (PerformanceProfiler, MemoryTracker, GCAllocationTracker, ObjectPoolAudit, BugTracker, ReadmeGenerator, PortfolioExporter)
- `HNR.Testing` - Integration test components (Week1-12 tests, QAChecklistSO, FinalReleaseChecklist)

## Data Architecture

ScriptableObject-based data definitions:
- `CardDataSO` - Card definitions with effects array
- `RequiemDataSO` - Playable character data (class, aspect, stats, null state)
- `EnemyDataSO` - Enemy definitions with intent patterns
- `EncounterDataSO` - Combat encounter configurations for enemy spawning
- `EchoEventDataSO` - Narrative event definitions with multi-outcome choices
- `ZoneConfigSO` - Map zone generation parameters
- `RequiemArtDataSO` - Character art configuration

Data asset locations:
- `Assets/_Project/Data/Cards/` - Card assets (organized by Requiem: Kira, Mordren, Elara, Thornwick)
- `Assets/_Project/Data/Characters/Requiems/` - Requiem character assets
- `Assets/_Project/Data/Enemies/` - Enemy assets (Zone 1-3, Elite, Boss)
- `Assets/_Project/Data/Encounters/` - Encounter definitions for enemy spawning
- `Assets/_Project/Data/Events/` - Echo event narrative content
- `Assets/_Project/Data/Config/` - Zone and map configuration

## Scene Architecture

| Scene | Purpose |
|-------|---------|
| `Boot` | GameBootstrap initialization, ServiceLocator setup |
| `MainMenu` | Main menu UI |
| `Bastion` | Hub/progression area between runs |
| `NullRift` | Dungeon/map exploration |
| `Combat` | Combat encounters |
| `CombatTest` | Combat testing with CombatTestManager |

## Testing

Integration tests in `Assets/_Project/Scripts/Testing/`:
- `Week1IntegrationTest.cs` - Core systems (ServiceLocator, EventBus, GameManager)
- `Week4IntegrationTest.cs` - Combat and card systems ([T] tests, [Y] card play)
- `Week7IntegrationTest.cs` - Map and Echo event systems ([M] map tests)
- `Week8IntegrationTest.cs` - Shop and Save systems
- `Week9IntegrationTest.cs` - UI polish and animations ([D] damage numbers, [S] shake, [C] corruption, [F] flash)
- `Week10IntegrationTest.cs` - VFX and Audio systems ([V] VFX, [M] music, [S] SFX, [H] haptic, [1-5] aspect VFX)
- `Week11IntegrationTest.cs` - Performance and Build systems ([B] benchmark, [P] profiler, [M] memory, [Q] quality)
- `Week12FinalVerification.cs` - Final release verification ([T] run all)
- `FinalReleaseChecklist.cs` - Release readiness checks (context menu)
- `QAChecklistSO.cs` - ScriptableObject for QA progress tracking

To run integration tests:
- Attach test component to GameObject in scene
- Press [T] at runtime to run all tests (common to all test files)

Combat test harness in `Assets/_Project/Scripts/Testing/CombatTestManager.cs`:
- Press `T` to start test combat
- Press `Space` to end turn
- Press `D` to deal damage to team
- Press `B` to add block
- Press `K` to kill first enemy

For unit tests, use Unity Test Framework with assembly `HNR.Tests.asmdef`.

## Key Files

All paths relative to `Assets/_Project/Scripts/`:

| File | Purpose |
|------|---------|
| `Core/ServiceLocator.cs` | Global service registry |
| `Core/Events/EventBus.cs` | Pub-sub event system |
| `Core/Events/GameEvents.cs` | All event definitions |
| `Core/GameManager.cs` | Master state machine |
| `Core/GameBootstrap.cs` | Scene bootstrap - initializes all managers |
| `Combat/TurnManager.cs` | Combat orchestrator and phase transitions |
| `Combat/CombatContext.cs` | Shared combat state |
| `Combat/DeckManager.cs` | Draw/discard/exhaust pile management |
| `Combat/HandManager.cs` | Card display with arc layout (DOTween) |
| `Combat/EncounterManager.cs` | Enemy spawning from encounter data |
| `Combat/StatusEffectManager.cs` | Status effect application and ticking |
| `Combat/SoulEssenceManager.cs` | SE accumulation and Requiem Art activation |
| `Cards/Effects/CardExecutor.cs` | Card effect execution hub |
| `Cards/CardInstance.cs` | Runtime card with upgrade support (ApplyUpgrade, IsUpgraded) |
| `Characters/RequiemInstance.cs` | Runtime Requiem (ICombatTarget) |
| `Characters/CorruptionManager.cs` | Team-wide corruption tracking |
| `Characters/NullStateHandler.cs` | Requiem-specific null state effects |
| `Combat/EnemyInstance.cs` | Runtime enemy (ICombatTarget) |
| `Map/MapManager.cs` | Run state and map navigation |
| `Map/MapGenerator.cs` | Procedural map generation |
| `Map/EchoEventManager.cs` | Narrative event handling |
| `Progression/Shop/ShopManager.cs` | Void Market shop system |
| `VFX/VFXPoolManager.cs` | Named effect pooling with pre-warm and max limits |
| `VFX/VFXInstance.cs` | Poolable particle effect wrapper |
| `VFX/CombatVFXController.cs` | Event-driven combat VFX spawning |
| `Audio/AudioManager.cs` | Central audio with AudioMixer integration |
| `Audio/CombatAudioController.cs` | Event-driven combat sound effects |
| `Audio/HapticController.cs` | Mobile haptic feedback (Android) |
| `Core/CombatFeedbackIntegrator.cs` | Coordinates VFX, audio, haptics, screen shake |
| `Core/QualitySettingsManager.cs` | Device-adaptive quality tiers (Low/Mid/High) |
| `Debug/PerformanceProfiler.cs` | Real-time FPS and frame time monitoring |
| `Debug/MemoryTracker.cs` | Memory snapshots and leak detection |
| `Debug/GCAllocationTracker.cs` | Per-frame GC allocation monitoring |
| `Debug/ObjectPoolAudit.cs` | Pool utilization verification |
| `Debug/BugTracker.cs` | QA bug reporting with screenshot capture |
| `Debug/ReadmeGenerator.cs` | Portfolio README.md generation |
| `Debug/PortfolioExporter.cs` | Clean project export for portfolio |
| `UI/Screens/LoadingScreen.cs` | Loading screen with tips |
| `UI/Screens/SettingsScreen.cs` | Player settings and options |
| `UI/Screens/CreditsScreen.cs` | Scrolling credits display |
| `UI/Tutorial/TutorialTooltipManager.cs` | First-time player guidance |
| `UI/Components/VersionDisplay.cs` | Build version display |
| `UI/Combat/SharedVitalityBarCZN.cs` | Wide team HP bar with embedded portraits (CZN) |
| `UI/Combat/PartyStatusSidebar.cs` | Left panel with EP gauges per Requiem (CZN) |
| `UI/Combat/APCounterDisplay.cs` | Large AP number display (replaces orbs) |
| `UI/Combat/ExecutionButton.cs` | Circular end turn button with glow |
| `UI/Combat/CardFanLayout.cs` | Curved 30° card fan layout (CZN) |
| `UI/GlobalHeader.cs` | Top bar with player profile, currency tickers |
| `UI/GlobalNavDock.cs` | Bottom navigation dock with nav buttons |
| `UI/Toast/ToastManager.cs` | System feedback notifications (Info/Success/Warning/Error) |
| `UI/Toast/ToastController.cs` | Individual toast notification behavior |
| `UI/Components/CurrencyTicker.cs` | Animated currency display with number lerp |
| `UI/Utilities/UIColors.cs` | Central UI color palette for theming |
| `Combat/CombatManager.cs` | High-level combat facade |
| `Combat/RequiemArtExecutor.cs` | Requiem Art execution with card effects |
| `Characters/Team.cs` | Party wrapper with utility methods |
| `Characters/StatBlock.cs` | Stat calculations with modifier support |

## Conventions

- All MonoBehaviour singletons persist via `DontDestroyOnLoad` and register with ServiceLocator
- Non-MonoBehaviour services (SaveManager) are created and registered by GameBootstrap
- Service types use interfaces (e.g., `IGameManager`) for testability
- Events include all relevant data in constructors (immutable properties)
- Debug logs use `[ClassName]` prefix format
- GameState enum uses explicit values for serialization stability
- ScriptableObject fields use `_privateField` naming convention with `[SerializeField]`
- Diagnostics tools use `HNR.Diagnostics` namespace (not `HNR.Debug` to avoid conflict with `UnityEngine.Debug`)

## Runtime Combat Types

Combat entity types are implemented as MonoBehaviours with `ICombatTarget` interface:
- `RequiemInstance` (`HNR.Characters`) - Runtime playable character with HP, Block, Corruption, Soul Essence
- `EnemyInstance` (`HNR.Combat`) - Runtime enemy with HP, Block, Intent pattern, Status effects
- `ICombatTarget` (`HNR.Combat`) - Common targeting interface (Name, Position, IsDead, TakeDamage, Heal, ShowTargetHighlight)

## Status Effect System

`StatusEffectManager` (`HNR.Combat`) manages combat status effects:
- Effects tick at turn start (player turn)
- Effects auto-clear on combat end
- Supports stacking and duration tracking
- Common status types: Burn, Poison, Weakness, Strength, etc.

## Meta-Game UI (`HNR.UI`)

Global UI components for meta-game navigation:

### GlobalHeader
Top bar anchored at screen top with:
- Player avatar, name, level, XP bar
- Currency tickers (Soul Crystals, Void Dust, Aether Stamina)
- Event banner display
- Subscribes to `CurrencyChangedEvent`, `PlayerLevelChangedEvent`, `PlayerExpChangedEvent`

### GlobalNavDock
Bottom navigation dock with:
- Navigation buttons (Bastion, Requiems, Inventory, Settings)
- DOTween selection animations with glow ring
- Notification badges per button
- Publishes `NavDockNavigationEvent`

### ToastManager
System feedback notifications:
```csharp
ToastManager.Instance.ShowInfo("Card added to deck");
ToastManager.Instance.ShowSuccess("Run saved!");
ToastManager.Instance.ShowWarning("Low health!");
ToastManager.Instance.ShowError("Connection failed");
```
Features: Queue-based, max 3 visible, configurable duration, fade animations.

### CurrencyTicker
Animated currency display with:
- Number lerping between values
- Color flash (green increase, red decrease)
- Punch scale on change
- K/M suffix formatting for large numbers

## UI Screens

New screens added in Week 12:
- `LoadingScreen` - Progress bar with rotating tips, spinner animation
- `SettingsScreen` - Audio/gameplay/graphics options, data management
- `CreditsScreen` - Auto-scrolling credits with skip button
- `TutorialTooltipManager` - First-time tooltips with persistent shown state (pauses game)

## Release Tools

Context menu tools for release preparation:
- `ReadmeGenerator` > "Generate README" - Creates portfolio README.md with badges
- `PortfolioExporter` > "Export Portfolio" - Exports scripts, docs, code samples
- `FinalReleaseChecklist` > "Run Release Checks" - Verifies systems, content, build
- `QAChecklistSO` > "Initialize Default Checklist" - Creates 44-item QA checklist
