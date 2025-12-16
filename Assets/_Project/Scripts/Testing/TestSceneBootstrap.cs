// ============================================
// TestSceneBootstrap.cs
// Bootstrap for standalone scene testing
// ============================================

using UnityEngine;
using HNR.Core;
using HNR.Core.Interfaces;
using HNR.Audio;
using HNR.Progression;
using HNR.UI;
using HNR.Map;
using HNR.Combat;
using HNR.Characters;
using HNR.VFX;

namespace HNR.Testing
{
    /// <summary>
    /// Bootstrap component for testing scenes without going through Boot scene.
    /// Initializes all required services for standalone scene testing.
    /// Add this component BEFORE Week12FinalVerification in execution order,
    /// or set Script Execution Order to run first (-100).
    /// </summary>
    [DefaultExecutionOrder(-100)]
    public class TestSceneBootstrap : MonoBehaviour
    {
        // ============================================
        // Serialized Fields
        // ============================================

        [Header("Auto Initialize")]
        [Tooltip("Initialize services on Awake")]
        [SerializeField] private bool _initializeOnAwake = true;

        [Header("Optional Prefabs")]
        [SerializeField] private GameObject _gameManagerPrefab;
        [SerializeField] private GameObject _audioManagerPrefab;
        [SerializeField] private GameObject _uiManagerPrefab;

        // ============================================
        // Unity Lifecycle
        // ============================================

        private void Awake()
        {
            if (_initializeOnAwake)
            {
                InitializeAllServices();
            }
        }

        // ============================================
        // Public Methods
        // ============================================

        /// <summary>
        /// Initialize all game services for testing.
        /// </summary>
        public void InitializeAllServices()
        {
            Debug.Log("[TestSceneBootstrap] Initializing test services...");

            // Initialize ServiceLocator
            if (!ServiceLocator.IsInitialized)
            {
                ServiceLocator.Initialize();
                Debug.Log("[TestSceneBootstrap] ServiceLocator initialized");
            }

            // Core managers
            InitializeGameManager();
            InitializeSaveManager();
            InitializeAudioManager();
            InitializeUIManager();
            InitializePoolManager();

            // Progression managers
            InitializeShopManager();
            InitializeRelicManager();
            InitializeMapManager();

            // Quality settings
            InitializeQualitySettingsManager();

            // Combat managers (find existing in scene)
            FindAndRegisterCombatManagers();

            Debug.Log("[TestSceneBootstrap] All test services initialized");
        }

        // ============================================
        // Initialization Methods
        // ============================================

        private void InitializeGameManager()
        {
            if (ServiceLocator.Has<IGameManager>())
            {
                Debug.Log("[TestSceneBootstrap] GameManager already registered");
                return;
            }

            var existing = FindAnyObjectByType<GameManager>();
            if (existing == null)
            {
                if (_gameManagerPrefab != null)
                {
                    var instance = Instantiate(_gameManagerPrefab);
                    instance.name = "[GameManager]";
                    DontDestroyOnLoad(instance);
                }
                else
                {
                    var go = new GameObject("[GameManager]");
                    go.AddComponent<GameManager>();
                    DontDestroyOnLoad(go);
                }
            }
            Debug.Log("[TestSceneBootstrap] GameManager initialized");
        }

        private void InitializeSaveManager()
        {
            if (ServiceLocator.Has<ISaveManager>())
            {
                Debug.Log("[TestSceneBootstrap] SaveManager already registered");
                return;
            }

            var existing = FindAnyObjectByType<SaveManager>();
            if (existing == null)
            {
                var go = new GameObject("[SaveManager]");
                go.AddComponent<SaveManager>();
                DontDestroyOnLoad(go);
            }
            Debug.Log("[TestSceneBootstrap] SaveManager initialized");
        }

        private void InitializeAudioManager()
        {
            if (ServiceLocator.Has<IAudioManager>())
            {
                Debug.Log("[TestSceneBootstrap] AudioManager already registered");
                return;
            }

            var existing = FindAnyObjectByType<AudioManager>();
            if (existing == null)
            {
                if (_audioManagerPrefab != null)
                {
                    var instance = Instantiate(_audioManagerPrefab);
                    instance.name = "[AudioManager]";
                    DontDestroyOnLoad(instance);
                }
                else
                {
                    var go = new GameObject("[AudioManager]");
                    go.AddComponent<AudioManager>();
                    DontDestroyOnLoad(go);
                }
            }
            Debug.Log("[TestSceneBootstrap] AudioManager initialized");
        }

        private void InitializeUIManager()
        {
            // Check if already registered OR exists in scene
            var existing = FindAnyObjectByType<UIManager>();
            if (existing != null || ServiceLocator.Has<IUIManager>())
            {
                Debug.Log("[TestSceneBootstrap] UIManager already exists, skipping");
                return;
            }

            if (_uiManagerPrefab != null)
            {
                var instance = Instantiate(_uiManagerPrefab);
                instance.name = "[UIManager]";
                DontDestroyOnLoad(instance);
            }
            else
            {
                var go = new GameObject("[UIManager]");
                var canvas = go.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                go.AddComponent<UnityEngine.UI.CanvasScaler>();
                go.AddComponent<UnityEngine.UI.GraphicRaycaster>();
                go.AddComponent<UIManager>();
                DontDestroyOnLoad(go);
            }
            Debug.Log("[TestSceneBootstrap] UIManager initialized");
        }

        private void InitializePoolManager()
        {
            if (ServiceLocator.Has<IPoolManager>())
            {
                Debug.Log("[TestSceneBootstrap] PoolManager already registered");
                return;
            }

            var existing = FindAnyObjectByType<PoolManager>();
            if (existing == null)
            {
                var go = new GameObject("[PoolManager]");
                go.AddComponent<PoolManager>();
                DontDestroyOnLoad(go);
            }
            Debug.Log("[TestSceneBootstrap] PoolManager initialized");
        }

        private void InitializeShopManager()
        {
            if (ServiceLocator.Has<IShopManager>())
            {
                Debug.Log("[TestSceneBootstrap] ShopManager already registered");
                return;
            }

            var existing = FindAnyObjectByType<ShopManager>();
            if (existing == null)
            {
                var go = new GameObject("[ShopManager]");
                go.AddComponent<ShopManager>();
                DontDestroyOnLoad(go);
            }
            Debug.Log("[TestSceneBootstrap] ShopManager initialized");
        }

        private void InitializeRelicManager()
        {
            if (ServiceLocator.Has<IRelicManager>())
            {
                Debug.Log("[TestSceneBootstrap] RelicManager already registered");
                return;
            }

            var existing = FindAnyObjectByType<RelicManager>();
            if (existing == null)
            {
                var go = new GameObject("[RelicManager]");
                go.AddComponent<RelicManager>();
                DontDestroyOnLoad(go);
            }
            Debug.Log("[TestSceneBootstrap] RelicManager initialized");
        }

        private void InitializeMapManager()
        {
            var existing = FindAnyObjectByType<MapManager>();
            if (existing == null)
            {
                var go = new GameObject("[MapManager]");
                go.AddComponent<MapManager>();
                // Don't persist - map is scene-specific
            }
            Debug.Log("[TestSceneBootstrap] MapManager initialized");
        }

        private void InitializeQualitySettingsManager()
        {
            var existing = FindAnyObjectByType<QualitySettingsManager>();
            if (existing == null)
            {
                var go = new GameObject("[QualitySettingsManager]");
                go.AddComponent<QualitySettingsManager>();
                DontDestroyOnLoad(go);
            }
            Debug.Log("[TestSceneBootstrap] QualitySettingsManager initialized");
        }

        private void FindAndRegisterCombatManagers()
        {
            // These should already exist in Combat scene
            var turnManager = FindAnyObjectByType<TurnManager>();
            var deckManager = FindAnyObjectByType<DeckManager>();
            var corruptionManager = FindAnyObjectByType<CorruptionManager>();

            if (turnManager != null)
                Debug.Log("[TestSceneBootstrap] TurnManager found");
            if (deckManager != null)
                Debug.Log("[TestSceneBootstrap] DeckManager found");

            // Create CorruptionManager if missing
            if (corruptionManager == null)
            {
                var go = new GameObject("[CorruptionManager]");
                go.AddComponent<CorruptionManager>();
                Debug.Log("[TestSceneBootstrap] CorruptionManager created");
            }
        }

        // ============================================
        // Editor Helper
        // ============================================

#if UNITY_EDITOR
        [ContextMenu("Initialize Services Now")]
        private void InitializeServicesNow()
        {
            InitializeAllServices();
        }
#endif
    }
}
