// ============================================
// VFXPoolManager.cs
// Specialized pooling for particle effects with pre-warm and max limits
// ============================================

using System;
using System.Collections.Generic;
using UnityEngine;
using HNR.Core;

namespace HNR.VFX
{
    /// <summary>
    /// Configuration for a VFX pool entry.
    /// </summary>
    [Serializable]
    public class VFXPoolConfig
    {
        [Tooltip("Unique identifier for this effect (e.g., hit_flame, vfx_heal)")]
        public string EffectId;

        [Tooltip("Prefab to instantiate for this effect")]
        public GameObject Prefab;

        [Tooltip("Number of instances to create on initialization")]
        public int PreWarmCount = 3;

        [Tooltip("Maximum simultaneous active instances")]
        public int MaxActive = 10;
    }

    /// <summary>
    /// Manages VFX particle effect pooling with pre-warming and max active limits.
    /// Specialized for named effect types rather than generic type-based pooling.
    /// </summary>
    /// <remarks>
    /// Pool configuration per TDD 10:
    /// - hit_flame/shadow/nature/arcane/light: 5 pre-warm, 10 max
    /// - vfx_slash: 3 pre-warm, 5 max
    /// - vfx_shield/heal: 2 pre-warm, 3 max
    /// - vfx_corruption: 3 pre-warm, 5 max
    /// - vfx_null_burst: 1 pre-warm, 2 max
    /// </remarks>
    public class VFXPoolManager : MonoBehaviour
    {
        // ============================================
        // Serialized Fields
        // ============================================

        [Header("Pool Configuration")]
        [SerializeField, Tooltip("VFX pool configurations")]
        private List<VFXPoolConfig> _poolConfigs = new();

        [SerializeField, Tooltip("Root transform for pooled objects")]
        private Transform _poolRoot;

        // ============================================
        // Private Fields
        // ============================================

        private Dictionary<string, Queue<VFXInstance>> _pools = new();
        private Dictionary<string, VFXPoolConfig> _configLookup = new();
        private Dictionary<string, int> _activeCount = new();

        // ============================================
        // Unity Lifecycle
        // ============================================

        private void Awake()
        {
            ServiceLocator.Register(this);

            // Create pool root if not assigned
            if (_poolRoot == null)
            {
                var root = new GameObject("[VFX Pool]");
                root.transform.SetParent(transform);
                _poolRoot = root.transform;
            }

            Initialize();
            Debug.Log("[VFXPoolManager] Initialized.");
        }

        private void OnDestroy()
        {
            ServiceLocator.Unregister<VFXPoolManager>();
        }

        // ============================================
        // Initialization
        // ============================================

        /// <summary>
        /// Initialize all configured pools and pre-warm instances.
        /// </summary>
        public void Initialize()
        {
            foreach (var config in _poolConfigs)
            {
                if (string.IsNullOrEmpty(config.EffectId))
                {
                    Debug.LogWarning("[VFXPoolManager] Skipping config with empty EffectId");
                    continue;
                }

                if (config.Prefab == null)
                {
                    Debug.LogWarning($"[VFXPoolManager] Skipping '{config.EffectId}' - no prefab assigned");
                    continue;
                }

                // Initialize pool structures
                _pools[config.EffectId] = new Queue<VFXInstance>();
                _configLookup[config.EffectId] = config;
                _activeCount[config.EffectId] = 0;

                // Pre-warm pool
                for (int i = 0; i < config.PreWarmCount; i++)
                {
                    var instance = CreateInstance(config);
                    instance.OnReturnToPool();
                    _pools[config.EffectId].Enqueue(instance);
                }

                Debug.Log($"[VFXPoolManager] Initialized '{config.EffectId}' with {config.PreWarmCount} instances (max: {config.MaxActive})");
            }
        }

        // ============================================
        // Spawn Methods
        // ============================================

        /// <summary>
        /// Spawn VFX at world position.
        /// </summary>
        /// <param name="effectId">Effect identifier from pool config</param>
        /// <param name="position">World position to spawn at</param>
        /// <param name="rotation">Rotation for the effect</param>
        /// <returns>VFXInstance if successful, null if pool unavailable or at max</returns>
        public VFXInstance Spawn(string effectId, Vector3 position, Quaternion rotation)
        {
            if (!_pools.ContainsKey(effectId))
            {
                Debug.LogWarning($"[VFXPoolManager] Effect not registered: {effectId}");
                return null;
            }

            var config = _configLookup[effectId];
            VFXInstance instance;

            // Try to get from pool
            if (_pools[effectId].Count > 0)
            {
                instance = _pools[effectId].Dequeue();
            }
            // Create new if under max active
            else if (_activeCount[effectId] < config.MaxActive)
            {
                instance = CreateInstance(config);
                Debug.Log($"[VFXPoolManager] Pool empty, created new '{effectId}'");
            }
            // At max capacity - refuse spawn
            else
            {
                Debug.LogWarning($"[VFXPoolManager] Max active reached for '{effectId}' ({config.MaxActive})");
                return null;
            }

            // Position and activate
            instance.transform.SetPositionAndRotation(position, rotation);
            instance.transform.SetParent(null);
            instance.OnSpawnFromPool();
            _activeCount[effectId]++;

            return instance;
        }

        /// <summary>
        /// Spawn VFX at world position with default rotation.
        /// </summary>
        public VFXInstance Spawn(string effectId, Vector3 position)
        {
            return Spawn(effectId, position, Quaternion.identity);
        }

        /// <summary>
        /// Spawn VFX attached to a transform (follows target).
        /// </summary>
        /// <param name="effectId">Effect identifier from pool config</param>
        /// <param name="parent">Transform to attach to</param>
        /// <returns>VFXInstance if successful, null otherwise</returns>
        public VFXInstance SpawnAttached(string effectId, Transform parent)
        {
            if (parent == null)
            {
                Debug.LogWarning("[VFXPoolManager] Cannot spawn attached to null parent");
                return null;
            }

            var instance = Spawn(effectId, parent.position, parent.rotation);
            if (instance != null)
            {
                instance.SetTarget(parent);
            }
            return instance;
        }

        // ============================================
        // Return Methods
        // ============================================

        /// <summary>
        /// Return VFX instance to its pool.
        /// </summary>
        /// <param name="instance">Instance to return</param>
        public void Return(VFXInstance instance)
        {
            if (instance == null) return;

            var effectId = instance.EffectId;
            if (string.IsNullOrEmpty(effectId) || !_pools.ContainsKey(effectId))
            {
                Debug.LogWarning($"[VFXPoolManager] Cannot return unknown effect: {effectId}");
                Destroy(instance.gameObject);
                return;
            }

            // Deactivate and re-parent
            instance.OnReturnToPool();
            instance.transform.SetParent(_poolRoot);

            // Add back to pool
            _pools[effectId].Enqueue(instance);
            _activeCount[effectId] = Mathf.Max(0, _activeCount[effectId] - 1);
        }

        // ============================================
        // Pool Management
        // ============================================

        /// <summary>
        /// Stop all active VFX immediately.
        /// </summary>
        public void StopAll()
        {
            var activeInstances = FindObjectsOfType<VFXInstance>();
            foreach (var instance in activeInstances)
            {
                if (instance.gameObject.activeInHierarchy)
                {
                    instance.Stop();
                }
            }
        }

        /// <summary>
        /// Return all active VFX to pools.
        /// </summary>
        public void ReturnAll()
        {
            var activeInstances = FindObjectsOfType<VFXInstance>();
            foreach (var instance in activeInstances)
            {
                if (instance.gameObject.activeInHierarchy)
                {
                    Return(instance);
                }
            }
        }

        /// <summary>
        /// Clear all pools and destroy all instances.
        /// </summary>
        public void ClearAll()
        {
            // Destroy pooled instances
            foreach (var kvp in _pools)
            {
                while (kvp.Value.Count > 0)
                {
                    var instance = kvp.Value.Dequeue();
                    if (instance != null)
                    {
                        Destroy(instance.gameObject);
                    }
                }
            }

            // Reset tracking
            _pools.Clear();
            _configLookup.Clear();
            _activeCount.Clear();

            Debug.Log("[VFXPoolManager] Cleared all pools");
        }

        /// <summary>
        /// Get current active count for an effect type.
        /// </summary>
        public int GetActiveCount(string effectId)
        {
            return _activeCount.TryGetValue(effectId, out var count) ? count : 0;
        }

        /// <summary>
        /// Get available pool count for an effect type.
        /// </summary>
        public int GetAvailableCount(string effectId)
        {
            return _pools.TryGetValue(effectId, out var pool) ? pool.Count : 0;
        }

        /// <summary>
        /// Check if an effect type is registered.
        /// </summary>
        public bool HasEffect(string effectId)
        {
            return _pools.ContainsKey(effectId);
        }

        // ============================================
        // Runtime Registration
        // ============================================

        /// <summary>
        /// Register a new effect type at runtime.
        /// </summary>
        public void RegisterEffect(string effectId, GameObject prefab, int preWarmCount = 3, int maxActive = 10)
        {
            if (_pools.ContainsKey(effectId))
            {
                Debug.LogWarning($"[VFXPoolManager] Effect already registered: {effectId}");
                return;
            }

            var config = new VFXPoolConfig
            {
                EffectId = effectId,
                Prefab = prefab,
                PreWarmCount = preWarmCount,
                MaxActive = maxActive
            };

            _pools[effectId] = new Queue<VFXInstance>();
            _configLookup[effectId] = config;
            _activeCount[effectId] = 0;

            // Pre-warm
            for (int i = 0; i < preWarmCount; i++)
            {
                var instance = CreateInstance(config);
                instance.OnReturnToPool();
                _pools[effectId].Enqueue(instance);
            }

            Debug.Log($"[VFXPoolManager] Registered '{effectId}' at runtime ({preWarmCount} pre-warm, {maxActive} max)");
        }

        // ============================================
        // Helper Methods
        // ============================================

        private VFXInstance CreateInstance(VFXPoolConfig config)
        {
            var go = Instantiate(config.Prefab, _poolRoot);
            var instance = go.GetComponent<VFXInstance>();

            if (instance == null)
            {
                instance = go.AddComponent<VFXInstance>();
            }

            instance.EffectId = config.EffectId;
            return instance;
        }

        // ============================================
        // Debug
        // ============================================

        /// <summary>
        /// Get debug info about all pools.
        /// </summary>
        public string GetDebugInfo()
        {
            var info = "[VFXPoolManager] Pool Status:\n";
            foreach (var effectId in _pools.Keys)
            {
                var config = _configLookup[effectId];
                info += $"  {effectId}: {_pools[effectId].Count} available, {_activeCount[effectId]}/{config.MaxActive} active\n";
            }
            return info;
        }
    }
}
