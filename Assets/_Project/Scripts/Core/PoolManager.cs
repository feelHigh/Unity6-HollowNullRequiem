// ============================================
// PoolManager.cs
// Object pooling system implementation
// ============================================

using System;
using System.Collections.Generic;
using UnityEngine;
using HNR.Core.Interfaces;

namespace HNR.Core
{
    /// <summary>
    /// Manages object pools for frequently instantiated objects.
    /// Reduces garbage collection by reusing objects.
    /// </summary>
    /// <remarks>
    /// Usage:
    /// 1. RegisterPrefab<T>(prefab) - Register prefab on startup
    /// 2. PreWarm<T>(count) - Optional pre-allocation
    /// 3. Get<T>() - Get object from pool
    /// 4. Return<T>(obj) - Return object to pool
    /// </remarks>
    public class PoolManager : MonoBehaviour, IPoolManager
    {
        // ============================================
        // Private Fields
        // ============================================

        private Dictionary<Type, Queue<Component>> _pools = new();
        private Dictionary<Type, Component> _prefabs = new();
        private Dictionary<Type, Transform> _containers = new();

        // ============================================
        // Unity Lifecycle
        // ============================================

        private void Awake()
        {
            // Persist across scene loads
            DontDestroyOnLoad(gameObject);

            // Register with ServiceLocator
            if (!ServiceLocator.IsInitialized)
            {
                ServiceLocator.Initialize();
            }
            ServiceLocator.Register<IPoolManager>(this);

            Debug.Log("[PoolManager] Initialized.");
        }

        private void OnDestroy()
        {
            if (ServiceLocator.Has<IPoolManager>())
            {
                ServiceLocator.Unregister<IPoolManager>();
            }
        }

        // ============================================
        // Registration
        // ============================================

        /// <summary>
        /// Register a prefab for pooling.
        /// Creates a container GameObject for this pool type.
        /// </summary>
        public void RegisterPrefab<T>(T prefab) where T : Component, IPoolable
        {
            if (prefab == null)
            {
                Debug.LogError("[PoolManager] Cannot register null prefab.");
                return;
            }

            var type = typeof(T);

            if (_prefabs.ContainsKey(type))
            {
                Debug.LogWarning($"[PoolManager] Prefab already registered: {type.Name}");
                return;
            }

            // Store prefab reference
            _prefabs[type] = prefab;

            // Create pool queue
            _pools[type] = new Queue<Component>();

            // Create container for organized hierarchy
            var container = new GameObject($"Pool_{type.Name}");
            container.transform.SetParent(transform);
            _containers[type] = container.transform;

            Debug.Log($"[PoolManager] Registered prefab: {type.Name}");
        }

        // ============================================
        // Pre-warming
        // ============================================

        /// <summary>
        /// Pre-instantiate objects to avoid runtime allocation.
        /// </summary>
        public void PreWarm<T>(int count) where T : Component, IPoolable
        {
            var type = typeof(T);

            if (!_prefabs.TryGetValue(type, out var prefab))
            {
                Debug.LogError($"[PoolManager] No prefab registered for: {type.Name}. Call RegisterPrefab first.");
                return;
            }

            if (count <= 0)
            {
                Debug.LogWarning($"[PoolManager] PreWarm count must be positive: {count}");
                return;
            }

            for (int i = 0; i < count; i++)
            {
                var obj = Instantiate(prefab, _containers[type]);
                obj.gameObject.SetActive(false);
                _pools[type].Enqueue(obj);
            }

            Debug.Log($"[PoolManager] Pre-warmed {count} instances of {type.Name}");
        }

        // ============================================
        // Get / Return
        // ============================================

        /// <summary>
        /// Get an object from the pool, or create a new one if empty.
        /// </summary>
        public T Get<T>() where T : Component, IPoolable
        {
            var type = typeof(T);

            if (!_pools.TryGetValue(type, out var pool))
            {
                Debug.LogError($"[PoolManager] No pool for: {type.Name}. Call RegisterPrefab first.");
                return null;
            }

            Component obj;

            if (pool.Count > 0)
            {
                // Get from pool
                obj = pool.Dequeue();
            }
            else
            {
                // Create new instance
                obj = Instantiate(_prefabs[type], _containers[type]);
                Debug.Log($"[PoolManager] Pool empty, created new {type.Name}");
            }

            // Activate and notify
            obj.gameObject.SetActive(true);
            (obj as IPoolable)?.OnSpawnFromPool();

            return obj as T;
        }

        /// <summary>
        /// Return an object to the pool for reuse.
        /// </summary>
        public void Return<T>(T obj) where T : Component, IPoolable
        {
            if (obj == null)
            {
                Debug.LogWarning("[PoolManager] Cannot return null object.");
                return;
            }

            var type = typeof(T);

            if (!_pools.ContainsKey(type))
            {
                Debug.LogWarning($"[PoolManager] No pool for {type.Name}, destroying object.");
                Destroy(obj.gameObject);
                return;
            }

            // Notify, deactivate, and re-parent
            obj.OnReturnToPool();
            obj.gameObject.SetActive(false);
            obj.transform.SetParent(_containers[type]);

            // Add back to pool
            _pools[type].Enqueue(obj);
        }

        // ============================================
        // Pool Management
        // ============================================

        /// <summary>
        /// Get the current available count in a pool.
        /// </summary>
        public int GetPoolSize<T>() where T : Component, IPoolable
        {
            var type = typeof(T);
            return _pools.TryGetValue(type, out var pool) ? pool.Count : 0;
        }

        /// <summary>
        /// Clear all pools and destroy all pooled objects.
        /// </summary>
        public void ClearAllPools()
        {
            foreach (var container in _containers.Values)
            {
                if (container != null)
                {
                    Destroy(container.gameObject);
                }
            }

            _pools.Clear();
            _prefabs.Clear();
            _containers.Clear();

            Debug.Log("[PoolManager] Cleared all pools.");
        }

        /// <summary>
        /// Clear a specific pool type.
        /// </summary>
        public void ClearPool<T>() where T : Component, IPoolable
        {
            var type = typeof(T);

            if (_containers.TryGetValue(type, out var container))
            {
                // Destroy all children
                foreach (Transform child in container)
                {
                    Destroy(child.gameObject);
                }

                // Clear the queue
                if (_pools.TryGetValue(type, out var pool))
                {
                    pool.Clear();
                }

                Debug.Log($"[PoolManager] Cleared pool: {type.Name}");
            }
        }

        // ============================================
        // Debug Info
        // ============================================

        /// <summary>
        /// Get debug info about all pools.
        /// </summary>
        public string GetDebugInfo()
        {
            var info = "[PoolManager] Pool Status:\n";
            foreach (var kvp in _pools)
            {
                info += $"  {kvp.Key.Name}: {kvp.Value.Count} available\n";
            }
            return info;
        }
    }
}
