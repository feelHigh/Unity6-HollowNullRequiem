// ============================================
// GameBootstrap.cs
// Bootstrap component for initializing core systems
// ============================================

using UnityEngine;
using HNR.Core.Interfaces;

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
    /// 3. Destroys itself if systems already exist
    ///
    /// Boot scene setup:
    /// 1. Create empty GameObject named "Bootstrap"
    /// 2. Attach this component
    /// 3. Assign GameManager prefab reference
    /// 4. Assign UIManager prefab reference (when implemented)
    /// </remarks>
    public class GameBootstrap : MonoBehaviour
    {
        // ============================================
        // Serialized Fields
        // ============================================

        [Header("Core System Prefabs")]
        [Tooltip("Prefab containing the GameManager component")]
        [SerializeField] private GameManager _gameManagerPrefab;

        // TODO: Uncomment when UIManager is implemented
        // [Tooltip("Prefab containing the UIManager component")]
        // [SerializeField] private UIManager _uiManagerPrefab;

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

            // Instantiate GameManager
            InstantiateGameManager();

            // Instantiate UIManager
            InstantiateUIManager();

            Debug.Log("[GameBootstrap] Core systems initialized.");
        }

        // ============================================
        // Initialization Methods
        // ============================================

        /// <summary>
        /// Instantiate the GameManager prefab.
        /// </summary>
        private void InstantiateGameManager()
        {
            if (_gameManagerPrefab == null)
            {
                Debug.LogError("[GameBootstrap] GameManager prefab not assigned!");

                // Create GameManager dynamically as fallback
                var go = new GameObject("[GameManager]");
                go.AddComponent<GameManager>();
                Debug.LogWarning("[GameBootstrap] Created GameManager dynamically. Assign prefab for proper setup.");
                return;
            }

            var gameManager = Instantiate(_gameManagerPrefab);
            gameManager.name = "[GameManager]";

            Debug.Log("[GameBootstrap] GameManager instantiated.");
        }

        /// <summary>
        /// Instantiate the UIManager prefab.
        /// </summary>
        private void InstantiateUIManager()
        {
            // TODO: Implement when UIManager is created
            // if (_uiManagerPrefab == null)
            // {
            //     Debug.LogWarning("[GameBootstrap] UIManager prefab not assigned!");
            //     return;
            // }
            //
            // var uiManager = Instantiate(_uiManagerPrefab);
            // uiManager.name = "[UIManager]";
            // DontDestroyOnLoad(uiManager.gameObject);
            //
            // Debug.Log("[GameBootstrap] UIManager instantiated.");

            Debug.Log("[GameBootstrap] UIManager initialization skipped (not yet implemented).");
        }
    }
}
