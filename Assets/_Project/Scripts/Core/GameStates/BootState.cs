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
        /// Verify the SaveManager service is registered.
        /// SaveManager is a MonoBehaviour that self-registers in Awake().
        /// </summary>
        private void InitializeSaveManager()
        {
            if (ServiceLocator.Has<ISaveManager>())
            {
                var saveManager = ServiceLocator.Get<ISaveManager>();
                Debug.Log($"[BootState] SaveManager ready. Has saved run: {saveManager.HasSavedRun}");
            }
            else
            {
                Debug.LogWarning("[BootState] SaveManager not registered - ensure it exists in Boot scene");
            }
        }

        /// <summary>
        /// Verify the AudioManager service is registered.
        /// AudioManager is a MonoBehaviour that self-registers in Awake().
        /// </summary>
        private void InitializeAudioManager()
        {
            if (ServiceLocator.Has<IAudioManager>())
            {
                Debug.Log("[BootState] AudioManager ready");
            }
            else
            {
                Debug.LogWarning("[BootState] AudioManager not registered - ensure it exists in Boot scene");
            }
        }

        /// <summary>
        /// Verify the PoolManager service is registered.
        /// PoolManager is a MonoBehaviour that self-registers in Awake().
        /// </summary>
        private void InitializePoolManager()
        {
            if (ServiceLocator.Has<IPoolManager>())
            {
                Debug.Log("[BootState] PoolManager ready");
            }
            else
            {
                Debug.LogWarning("[BootState] PoolManager not registered - ensure it exists in Boot scene");
            }
        }

        /// <summary>
        /// Verify the UIManager service is registered.
        /// UIManager is a MonoBehaviour that self-registers in Awake().
        /// </summary>
        private void InitializeUIManager()
        {
            if (ServiceLocator.Has<IUIManager>())
            {
                Debug.Log("[BootState] UIManager ready");
            }
            else
            {
                Debug.LogWarning("[BootState] UIManager not registered - ensure it exists in Boot scene");
            }
        }
    }
}
