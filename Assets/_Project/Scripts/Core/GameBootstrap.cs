// ============================================
// GameBootstrap.cs
// Bootstrap component for initializing core systems
// ============================================

using UnityEngine;
using HNR.Core.Interfaces;
using HNR.UI;
using HNR.Audio;
using HNR.Progression;

namespace HNR.Core
{
    /// <summary>
    /// Bootstrap component that initializes the game on startup.
    /// Attach to a GameObject in the Boot scene.
    /// </summary>
    /// <remarks>
    /// This component:
    /// 1. Checks if GameManager already exists (scene reload case)
    /// 2. Instantiates core system prefabs if needed
    /// 3. Initializes non-MonoBehaviour services (SaveManager)
    /// 4. Destroys itself if systems already exist
    ///
    /// Boot scene setup:
    /// 1. Create empty GameObject named "Bootstrap"
    /// 2. Attach this component
    /// 3. Assign GameManager prefab reference
    /// 4. Optionally place UIManager, PoolManager, AudioManager in scene
    /// </remarks>
    public class GameBootstrap : MonoBehaviour
    {
        // ============================================
        // Serialized Fields
        // ============================================

        [Header("Core System Prefabs")]
        [Tooltip("Prefab containing the GameManager component")]
        [SerializeField] private GameManager _gameManagerPrefab;

        // ============================================
        // Unity Lifecycle
        // ============================================

        private void Awake()
        {
            // Check if GameManager already exists (scene was reloaded)
            if (ServiceLocator.Has<IGameManager>())
            {
                Debug.Log("[GameBootstrap] GameManager already exists. Destroying bootstrap.");
                Destroy(gameObject);
                return;
            }

            Debug.Log("[GameBootstrap] Initializing core systems...");

            // Initialize ServiceLocator first
            if (!ServiceLocator.IsInitialized)
            {
                ServiceLocator.Initialize();
            }

            // Initialize non-MonoBehaviour services
            InitializeSaveManager();

            // Initialize player progression (depends on SaveManager)
            InitializePlayerProgressionManager();

            // Instantiate MonoBehaviour managers
            InstantiateGameManager();
            InitializeUIManager();
            InitializePoolManager();
            InitializeAudioManager();
            InitializeRunManager();
            InitializeShopManager();
            InitializeRelicManager();

            Debug.Log("[GameBootstrap] Core systems initialized.");
        }

        // ============================================
        // Initialization Methods
        // ============================================

        /// <summary>
        /// Initialize the SaveManager (MonoBehaviour).
        /// </summary>
        private void InitializeSaveManager()
        {
            if (ServiceLocator.Has<ISaveManager>())
            {
                Debug.Log("[GameBootstrap] SaveManager already registered.");
                return;
            }

            var saveManager = FindAnyObjectByType<SaveManager>();
            if (saveManager == null)
            {
                var go = new GameObject("[SaveManager]");
                go.AddComponent<SaveManager>();
                Debug.Log("[GameBootstrap] SaveManager created dynamically.");
            }
            else
            {
                Debug.Log("[GameBootstrap] SaveManager found in scene.");
            }
        }

        /// <summary>
        /// Find or create PlayerProgressionManager.
        /// Depends on SaveManager for persistence.
        /// </summary>
        private void InitializePlayerProgressionManager()
        {
            if (ServiceLocator.Has<PlayerProgressionManager>())
            {
                Debug.Log("[GameBootstrap] PlayerProgressionManager already registered.");
                return;
            }

            var progressionManager = FindAnyObjectByType<PlayerProgressionManager>();
            if (progressionManager == null)
            {
                var go = new GameObject("[PlayerProgressionManager]");
                go.AddComponent<PlayerProgressionManager>();
                Debug.Log("[GameBootstrap] PlayerProgressionManager created dynamically.");
            }
            else
            {
                Debug.Log("[GameBootstrap] PlayerProgressionManager found in scene.");
            }
        }

        /// <summary>
        /// Instantiate the GameManager prefab.
        /// </summary>
        private void InstantiateGameManager()
        {
            if (_gameManagerPrefab == null)
            {
                // Create GameManager dynamically as fallback (this is fine for development)
                var go = new GameObject("[GameManager]");
                go.AddComponent<GameManager>();
                Debug.Log("[GameBootstrap] GameManager created dynamically.");
                return;
            }

            var gameManager = Instantiate(_gameManagerPrefab);
            gameManager.name = "[GameManager]";

            Debug.Log("[GameBootstrap] GameManager instantiated.");
        }

        /// <summary>
        /// Find or create UIManager.
        /// UIManager self-registers in Awake, so we just ensure it exists.
        /// </summary>
        private void InitializeUIManager()
        {
            var uiManager = FindAnyObjectByType<UIManager>();
            if (uiManager == null)
            {
                var go = new GameObject("[UIManager]");
                go.AddComponent<UIManager>();
                Debug.Log("[GameBootstrap] UIManager created dynamically.");
            }
            else
            {
                Debug.Log("[GameBootstrap] UIManager found in scene.");
            }
        }

        /// <summary>
        /// Find or create PoolManager.
        /// PoolManager self-registers in Awake.
        /// </summary>
        private void InitializePoolManager()
        {
            var poolManager = FindAnyObjectByType<PoolManager>();
            if (poolManager == null)
            {
                var go = new GameObject("[PoolManager]");
                go.AddComponent<PoolManager>();
                Debug.Log("[GameBootstrap] PoolManager created dynamically.");
            }
            else
            {
                Debug.Log("[GameBootstrap] PoolManager found in scene.");
            }
        }

        /// <summary>
        /// Find or create AudioManager.
        /// AudioManager self-registers in Awake.
        /// </summary>
        private void InitializeAudioManager()
        {
            var audioManager = FindAnyObjectByType<AudioManager>();
            if (audioManager == null)
            {
                var go = new GameObject("[AudioManager]");
                go.AddComponent<AudioManager>();
                Debug.Log("[GameBootstrap] AudioManager created dynamically.");
            }
            else
            {
                Debug.Log("[GameBootstrap] AudioManager found in scene.");
            }
        }

        /// <summary>
        /// Find or create RunManager.
        /// RunManager self-registers in Awake.
        /// </summary>
        private void InitializeRunManager()
        {
            if (ServiceLocator.Has<IRunManager>())
            {
                Debug.Log("[GameBootstrap] RunManager already registered.");
                return;
            }

            var runManager = FindAnyObjectByType<RunManager>();
            if (runManager == null)
            {
                var go = new GameObject("[RunManager]");
                go.AddComponent<RunManager>();
                Debug.Log("[GameBootstrap] RunManager created dynamically.");
            }
            else
            {
                Debug.Log("[GameBootstrap] RunManager found in scene.");
            }
        }

        /// <summary>
        /// Find or create ShopManager.
        /// ShopManager self-registers in Awake.
        /// </summary>
        private void InitializeShopManager()
        {
            if (ServiceLocator.Has<IShopManager>())
            {
                Debug.Log("[GameBootstrap] ShopManager already registered.");
                return;
            }

            var shopManager = FindAnyObjectByType<ShopManager>();
            if (shopManager == null)
            {
                var go = new GameObject("[ShopManager]");
                go.AddComponent<ShopManager>();
                Debug.Log("[GameBootstrap] ShopManager created dynamically.");
            }
            else
            {
                Debug.Log("[GameBootstrap] ShopManager found in scene.");
            }
        }

        /// <summary>
        /// Find or create RelicManager.
        /// RelicManager self-registers in Awake.
        /// </summary>
        private void InitializeRelicManager()
        {
            if (ServiceLocator.Has<IRelicManager>())
            {
                Debug.Log("[GameBootstrap] RelicManager already registered.");
                return;
            }

            var relicManager = FindAnyObjectByType<RelicManager>();
            if (relicManager == null)
            {
                var go = new GameObject("[RelicManager]");
                go.AddComponent<RelicManager>();
                Debug.Log("[GameBootstrap] RelicManager created dynamically.");
            }
            else
            {
                Debug.Log("[GameBootstrap] RelicManager found in scene.");
            }
        }
    }
}
