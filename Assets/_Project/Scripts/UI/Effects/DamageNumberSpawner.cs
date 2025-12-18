// ============================================
// DamageNumberSpawner.cs
// Spawns pooled damage numbers in response to events
// ============================================

using UnityEngine;
using HNR.Core;
using HNR.Core.Events;
using HNR.Core.Interfaces;
using HNR.Combat;

namespace HNR.UI
{
    /// <summary>
    /// Listens to combat events and spawns damage numbers.
    /// Pre-warms pool on start for performance.
    /// </summary>
    public class DamageNumberSpawner : MonoBehaviour
    {
        // ============================================
        // Configuration
        // ============================================

        [Header("Prefab")]
        [SerializeField, Tooltip("DamageNumber prefab to spawn")]
        private DamageNumber _prefab;

        [Header("Pool Settings")]
        [SerializeField, Tooltip("Number of instances to pre-warm")]
        private int _preWarmCount = 10;

        [Header("Positioning")]
        [SerializeField, Tooltip("Offset above target position")]
        private Vector3 _spawnOffset = new Vector3(0, 0.5f, 0);

        [SerializeField, Tooltip("Camera for world-to-screen conversion (optional)")]
        private Camera _worldCamera;

        [Header("Parent Canvas")]
        [SerializeField, Tooltip("Canvas to parent spawned numbers to")]
        private Canvas _canvas;

        // ============================================
        // Unity Lifecycle
        // ============================================

        private void Awake()
        {
            // Try to load prefab if not assigned
            if (_prefab == null)
            {
                TryLoadPrefabFallback();
            }

            // Register prefab with pool manager early (before any events fire)
            if (_prefab != null && ServiceLocator.TryGet<IPoolManager>(out var poolManager))
            {
                poolManager.RegisterPrefab(_prefab);
                poolManager.PreWarm<DamageNumber>(_preWarmCount);
                Debug.Log($"[DamageNumberSpawner] Registered and pre-warmed {_preWarmCount} damage numbers");
            }
            else if (_prefab == null)
            {
                Debug.LogWarning("[DamageNumberSpawner] _prefab is not assigned and fallback failed! Damage numbers will not spawn.");
            }
        }

        /// <summary>
        /// Attempt to load the DamageNumber prefab via fallback methods.
        /// </summary>
        private void TryLoadPrefabFallback()
        {
            // Try Resources.Load first
            _prefab = Resources.Load<DamageNumber>("Prefabs/UI/Effects/DamageNumber");
            if (_prefab != null)
            {
                Debug.Log("[DamageNumberSpawner] Loaded prefab from Resources folder");
                return;
            }

            // Try finding in scene (maybe it exists but just not referenced)
            _prefab = FindAnyObjectByType<DamageNumber>(FindObjectsInactive.Include);
            if (_prefab != null)
            {
                Debug.Log("[DamageNumberSpawner] Found existing DamageNumber in scene, using as template");
                return;
            }

#if UNITY_EDITOR
            // Editor-only fallback: try to load via AssetDatabase
            var guids = UnityEditor.AssetDatabase.FindAssets("t:Prefab DamageNumber");
            foreach (var guid in guids)
            {
                var path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
                if (path.Contains("DamageNumber.prefab"))
                {
                    var prefabGO = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(path);
                    _prefab = prefabGO?.GetComponent<DamageNumber>();
                    if (_prefab != null)
                    {
                        Debug.Log($"[DamageNumberSpawner] Loaded prefab from AssetDatabase: {path}");
                        return;
                    }
                }
            }
#endif
        }

        private void Start()
        {
            // Get camera reference if not set
            if (_worldCamera == null)
            {
                _worldCamera = Camera.main;
            }

            // Find canvas if not assigned
            if (_canvas == null)
            {
                _canvas = GetComponentInParent<Canvas>();
                if (_canvas == null)
                {
                    _canvas = FindAnyObjectByType<Canvas>();
                }
                if (_canvas != null)
                {
                    Debug.Log($"[DamageNumberSpawner] Found canvas at runtime: {_canvas.name}");
                }
                else
                {
                    Debug.LogWarning("[DamageNumberSpawner] No canvas found! Damage numbers will not be visible.");
                }
            }

            // Subscribe to events
            SubscribeToEvents();
            Debug.Log("[DamageNumberSpawner] Subscribed to combat events");
        }

        private void OnDestroy()
        {
            UnsubscribeFromEvents();
        }

        // ============================================
        // Event Handling
        // ============================================

        private void SubscribeToEvents()
        {
            EventBus.Subscribe<EnemyDamagedEvent>(OnEnemyDamaged);
            EventBus.Subscribe<TeamHPChangedEvent>(OnTeamHPChanged);
            EventBus.Subscribe<BlockChangedEvent>(OnBlockChanged);
        }

        private void UnsubscribeFromEvents()
        {
            EventBus.Unsubscribe<EnemyDamagedEvent>(OnEnemyDamaged);
            EventBus.Unsubscribe<TeamHPChangedEvent>(OnTeamHPChanged);
            EventBus.Unsubscribe<BlockChangedEvent>(OnBlockChanged);
        }

        private void OnEnemyDamaged(EnemyDamagedEvent evt)
        {
            if (evt.Enemy == null) return;

            Debug.Log($"[DamageNumberSpawner] OnEnemyDamaged: {evt.Enemy.Name} took {evt.Damage} damage (blocked: {evt.Blocked})");

            // Spawn damage number at enemy position
            Vector3 worldPos = evt.Enemy.Position + _spawnOffset;
            SpawnNumber(evt.Damage, DamageNumberType.Damage, worldPos);

            // Also show blocked amount if any
            if (evt.Blocked > 0)
            {
                Vector3 blockPos = worldPos + new Vector3(0.3f, 0.2f, 0);
                SpawnNumber(evt.Blocked, DamageNumberType.Block, blockPos);
            }
        }

        private void OnTeamHPChanged(TeamHPChangedEvent evt)
        {
            if (evt.Delta == 0) return;

            // Get a position for team damage/heal numbers
            // Use center-bottom of screen for team
            Vector3 screenPos = new Vector3(Screen.width * 0.5f, Screen.height * 0.3f, 0);

            if (evt.Delta > 0)
            {
                // Healing
                SpawnNumberAtScreen(evt.Delta, DamageNumberType.Heal, screenPos);
            }
            else
            {
                // Damage
                SpawnNumberAtScreen(-evt.Delta, DamageNumberType.Damage, screenPos);
            }
        }

        private void OnBlockChanged(BlockChangedEvent evt)
        {
            int delta = evt.Block - evt.PreviousBlock;
            if (delta <= 0) return;

            // Block gained - show at bottom center
            Vector3 screenPos = new Vector3(Screen.width * 0.5f, Screen.height * 0.25f, 0);
            SpawnNumberAtScreen(delta, DamageNumberType.Block, screenPos);
        }

        // ============================================
        // Spawning
        // ============================================

        /// <summary>
        /// Spawn a damage number at a world position.
        /// </summary>
        public void SpawnNumber(int value, DamageNumberType type, Vector3 worldPosition, bool isCritical = false)
        {
            if (value <= 0) return;

            Debug.Log($"[DamageNumberSpawner] SpawnNumber: value={value}, type={type}, worldPos={worldPosition}");

            var number = GetNumber();
            if (number == null)
            {
                Debug.LogWarning("[DamageNumberSpawner] GetNumber returned null!");
                return;
            }

            // Convert world position to screen position
            if (_worldCamera != null)
            {
                Vector3 screenPos = _worldCamera.WorldToScreenPoint(worldPosition);
                number.transform.position = screenPos;
                Debug.Log($"[DamageNumberSpawner] Converted to screen pos: {screenPos}");
            }
            else
            {
                number.transform.position = worldPosition;
                Debug.LogWarning("[DamageNumberSpawner] No camera - using world position directly");
            }

            number.Show(value, type, number.transform.position, isCritical);
            Debug.Log($"[DamageNumberSpawner] DamageNumber shown at {number.transform.position}, parent={number.transform.parent?.name ?? "null"}");
        }

        /// <summary>
        /// Spawn a damage number at a screen position.
        /// </summary>
        public void SpawnNumberAtScreen(int value, DamageNumberType type, Vector3 screenPosition, bool isCritical = false)
        {
            if (value <= 0) return;

            var number = GetNumber();
            if (number == null) return;

            number.ShowAtScreenPosition(value, type, screenPosition, isCritical);
        }

        private DamageNumber GetNumber()
        {
            DamageNumber number = null;

            if (ServiceLocator.TryGet<IPoolManager>(out var poolManager))
            {
                number = poolManager.Get<DamageNumber>();
                if (number != null)
                {
                    Debug.Log($"[DamageNumberSpawner] Got DamageNumber from pool");
                }
            }

            // Fallback to instantiate
            if (number == null && _prefab != null)
            {
                number = Instantiate(_prefab);
                Debug.Log($"[DamageNumberSpawner] Instantiated new DamageNumber (pool returned null)");
            }

            if (number == null)
            {
                Debug.LogWarning($"[DamageNumberSpawner] Failed to get DamageNumber instance. Prefab={_prefab != null}, PoolManager={ServiceLocator.Has<IPoolManager>()}");
                return null;
            }

            // Parent to canvas
            if (_canvas != null)
            {
                number.transform.SetParent(_canvas.transform, false);
                Debug.Log($"[DamageNumberSpawner] Parented to canvas: {_canvas.name}");
            }
            else
            {
                Debug.LogWarning("[DamageNumberSpawner] Canvas is null! DamageNumber will not be visible.");
            }

            return number;
        }

        // ============================================
        // Public API for Manual Spawning
        // ============================================

        /// <summary>
        /// Manually spawn a damage number (for external systems).
        /// </summary>
        public void SpawnDamage(int amount, Vector3 worldPosition, bool isCritical = false)
        {
            SpawnNumber(amount, DamageNumberType.Damage, worldPosition, isCritical);
        }

        /// <summary>
        /// Manually spawn a heal number.
        /// </summary>
        public void SpawnHeal(int amount, Vector3 worldPosition)
        {
            SpawnNumber(amount, DamageNumberType.Heal, worldPosition, false);
        }

        /// <summary>
        /// Manually spawn a block number.
        /// </summary>
        public void SpawnBlock(int amount, Vector3 worldPosition)
        {
            SpawnNumber(amount, DamageNumberType.Block, worldPosition, false);
        }
    }
}
