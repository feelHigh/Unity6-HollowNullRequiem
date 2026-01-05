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
    /// Pool configuration can be provided via:
    /// 1. VFXConfigSO asset (recommended for centralized management)
    /// 2. Direct _poolConfigs list (legacy/override)
    ///
    /// Pool sizing per TDD 10:
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

        [Header("Configuration Source")]
        [SerializeField, Tooltip("Centralized VFX configuration (recommended). If set, overrides _poolConfigs.")]
        private VFXConfigSO _vfxConfig;

        [Header("Pool Configuration (Legacy/Override)")]
        [SerializeField, Tooltip("Direct pool configurations. Used if _vfxConfig is not set.")]
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
        /// Loads from VFXConfigSO if set, otherwise uses direct _poolConfigs.
        /// </summary>
        public void Initialize()
        {
            // Get configurations from VFXConfigSO or fall back to direct list
            var configs = GetEffectiveConfigs();

            foreach (var config in configs)
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
            VFXInstance instance = null;

            // Try to get valid instance from pool (skip destroyed objects)
            while (_pools[effectId].Count > 0)
            {
                var candidate = _pools[effectId].Dequeue();

                // Check if the pooled object was destroyed (e.g., scene reload)
                if (candidate == null || candidate.gameObject == null)
                {
                    Debug.LogWarning($"[VFXPoolManager] Removed destroyed instance from '{effectId}' pool");
                    continue;
                }

                instance = candidate;
                break;
            }

            // If no valid instance from pool, create new if under max active
            if (instance == null)
            {
                if (_activeCount[effectId] < config.MaxActive)
                {
                    instance = CreateInstance(config);
                    Debug.Log($"[VFXPoolManager] Pool empty, created new '{effectId}'");
                }
                else
                {
                    Debug.LogWarning($"[VFXPoolManager] Max active reached for '{effectId}' ({config.MaxActive})");
                    return null;
                }
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
            // Null check for destroyed objects
            if (instance == null || instance.gameObject == null) return;

            var effectId = instance.EffectId;
            if (string.IsNullOrEmpty(effectId) || !_pools.ContainsKey(effectId))
            {
                Debug.LogWarning($"[VFXPoolManager] Cannot return unknown effect: {effectId}");
                if (instance != null && instance.gameObject != null)
                {
                    Destroy(instance.gameObject);
                }
                return;
            }

            // Check if pool root still exists (may be destroyed on scene unload)
            if (_poolRoot == null)
            {
                Debug.LogWarning($"[VFXPoolManager] Pool root destroyed, cannot return '{effectId}'");
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
            var activeInstances = FindObjectsByType<VFXInstance>(FindObjectsSortMode.None);
            foreach (var instance in activeInstances)
            {
                // Check for destroyed objects before accessing properties
                if (instance != null && instance.gameObject != null && instance.gameObject.activeInHierarchy)
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
            var activeInstances = FindObjectsByType<VFXInstance>(FindObjectsSortMode.None);
            foreach (var instance in activeInstances)
            {
                // Check for destroyed objects before accessing properties
                if (instance != null && instance.gameObject != null && instance.gameObject.activeInHierarchy)
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
                    // Check for both null reference and destroyed Unity object
                    if (instance != null && instance.gameObject != null)
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
        /// Purge destroyed instances from all pools.
        /// Call this after scene transitions or when pool corruption is suspected.
        /// </summary>
        public void PurgeDestroyedInstances()
        {
            int purgedCount = 0;

            foreach (var kvp in _pools)
            {
                var effectId = kvp.Key;
                var pool = kvp.Value;
                var validInstances = new Queue<VFXInstance>();

                while (pool.Count > 0)
                {
                    var instance = pool.Dequeue();
                    if (instance != null && instance.gameObject != null)
                    {
                        validInstances.Enqueue(instance);
                    }
                    else
                    {
                        purgedCount++;
                    }
                }

                // Replace pool with cleaned version
                _pools[effectId] = validInstances;
            }

            if (purgedCount > 0)
            {
                Debug.Log($"[VFXPoolManager] Purged {purgedCount} destroyed instances from pools");
            }
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

        /// <summary>
        /// Get effective pool configurations from VFXConfigSO or direct list.
        /// </summary>
        private List<VFXPoolConfig> GetEffectiveConfigs()
        {
            // Priority: VFXConfigSO > direct _poolConfigs
            if (_vfxConfig != null)
            {
                Debug.Log($"[VFXPoolManager] Loading {_vfxConfig.TotalEntryCount} effects from VFXConfig asset");
                return _vfxConfig.ToPoolConfigs();
            }

            // Fall back to direct configuration
            if (_poolConfigs.Count > 0)
            {
                Debug.Log($"[VFXPoolManager] Using {_poolConfigs.Count} direct pool configurations");
                return _poolConfigs;
            }

            Debug.LogWarning("[VFXPoolManager] No VFX configurations found. Assign VFXConfigSO or add pool configs.");
            return new List<VFXPoolConfig>();
        }

        // ============================================
        // Configuration Management
        // ============================================

        /// <summary>
        /// Get the current VFXConfigSO reference.
        /// </summary>
        public VFXConfigSO VFXConfig => _vfxConfig;

        /// <summary>
        /// Set VFXConfigSO at runtime and reinitialize pools.
        /// </summary>
        /// <param name="config">VFXConfigSO to use</param>
        /// <param name="reinitialize">Whether to clear and reinitialize pools</param>
        public void SetVFXConfig(VFXConfigSO config, bool reinitialize = true)
        {
            _vfxConfig = config;

            if (reinitialize)
            {
                ClearAll();
                Initialize();
            }
        }

        /// <summary>
        /// Load VFXConfigSO from Resources path.
        /// </summary>
        /// <param name="resourcePath">Path relative to Resources folder (e.g., "Data/Config/VFXConfig")</param>
        /// <returns>True if loaded successfully</returns>
        public bool LoadVFXConfigFromResources(string resourcePath = "Data/Config/VFXConfig")
        {
            var config = Resources.Load<VFXConfigSO>(resourcePath);
            if (config != null)
            {
                SetVFXConfig(config);
                Debug.Log($"[VFXPoolManager] Loaded VFXConfig from Resources: {resourcePath}");
                return true;
            }

            Debug.LogWarning($"[VFXPoolManager] Failed to load VFXConfig from Resources: {resourcePath}");
            return false;
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
