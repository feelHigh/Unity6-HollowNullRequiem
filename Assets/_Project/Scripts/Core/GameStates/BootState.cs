// ============================================
// BootState.cs
// Initial boot state - initializes core systems
// ============================================

using UnityEngine;
using HNR.Core.Events;
using HNR.Core.Interfaces;

namespace HNR.Core.GameStates
{
    /// <summary>
    /// Initial boot state that initializes all core systems.
    /// Automatically transitions to MainMenu after initialization.
    /// </summary>
    /// <remarks>
    /// Initialization order is critical:
    /// 1. ServiceLocator (already done by GameManager)
    /// 2. EventBus (clear any stale subscribers)
    /// 3. SaveManager (loads settings, checks for saved run)
    /// 4. AudioManager (uses settings from SaveManager)
    /// 5. PoolManager (pre-warms object pools)
    /// 6. UIManager (shows appropriate screen)
    /// </remarks>
    public class BootState : IGameState
    {
        private readonly GameManager _manager;

        /// <summary>
        /// Creates a new BootState.
        /// </summary>
        /// <param name="manager">Reference to the GameManager</param>
        public BootState(GameManager manager)
        {
            _manager = manager;
        }

        /// <summary>
        /// Initialize all core systems and transition to MainMenu.
        /// </summary>
        public void Enter()
        {
            Debug.Log("[BootState] Initializing core systems...");

            // ServiceLocator is already initialized by GameManager.Awake()
            // Note: EventBus is reset via RuntimeInitializeOnLoadMethod - no manual Clear() needed
            // (Manual Clear() here would wipe GameManager's subscriptions)

            // Initialize core services in dependency order
            InitializeSaveManager();
            InitializeAudioManager();
            InitializePoolManager();
            InitializeUIManager();

            Debug.Log("[BootState] Core systems initialized. Transitioning to MainMenu...");

            // Transition to main menu
            _manager.ChangeState(GameState.MainMenu);
        }

        /// <summary>
        /// Per-frame update (not used in Boot state).
        /// </summary>
        public void Update()
        {
            // Boot state transitions immediately, no update logic needed
        }

        /// <summary>
        /// Cleanup when leaving Boot state (not used).
        /// </summary>
        public void Exit()
        {
            // No cleanup needed for Boot state
        }

        // ============================================
        // Service Initialization Methods
        // ============================================

        /// <summary>
        /// Initialize the SaveManager service.
        /// </summary>
        private void InitializeSaveManager()
        {
            Debug.Log("[BootState] Initializing SaveManager...");

            // TODO: Implement SaveManager
            // var saveManager = new SaveManager();
            // ServiceLocator.Register<ISaveManager>(saveManager);
            // saveManager.Initialize();

            // Check for existing saved run
            // if (saveManager.HasSavedRun)
            // {
            //     Debug.Log("[BootState] Found saved run data.");
            // }
        }

        /// <summary>
        /// Initialize the AudioManager service.
        /// </summary>
        private void InitializeAudioManager()
        {
            Debug.Log("[BootState] Initializing AudioManager...");

            // TODO: Implement AudioManager
            // var audioGO = new GameObject("[AudioManager]");
            // Object.DontDestroyOnLoad(audioGO);
            // var audioManager = audioGO.AddComponent<AudioManager>();
            // ServiceLocator.Register<IAudioManager>(audioManager);

            // Load volume settings from SaveManager
            // var settings = ServiceLocator.Get<ISaveManager>().LoadSettings();
            // audioManager.MusicVolume = settings.MusicVolume;
            // audioManager.SFXVolume = settings.SFXVolume;
        }

        /// <summary>
        /// Initialize the PoolManager service.
        /// </summary>
        private void InitializePoolManager()
        {
            Debug.Log("[BootState] Initializing PoolManager...");

            // TODO: Implement PoolManager
            // var poolGO = new GameObject("[PoolManager]");
            // Object.DontDestroyOnLoad(poolGO);
            // var poolManager = poolGO.AddComponent<PoolManager>();
            // ServiceLocator.Register<IPoolManager>(poolManager);

            // Pre-warm commonly used pools
            // poolManager.PreWarm<CardVisual>(20);
            // poolManager.PreWarm<DamageNumber>(10);
        }

        /// <summary>
        /// Initialize the UIManager service.
        /// </summary>
        private void InitializeUIManager()
        {
            Debug.Log("[BootState] Initializing UIManager...");

            // TODO: Implement UIManager
            // UIManager may already exist in scene, find it
            // var uiManager = Object.FindAnyObjectByType<UIManager>();
            // if (uiManager != null)
            // {
            //     ServiceLocator.Register<IUIManager>(uiManager);
            // }
            // else
            // {
            //     Debug.LogWarning("[BootState] UIManager not found in scene!");
            // }
        }
    }
}
