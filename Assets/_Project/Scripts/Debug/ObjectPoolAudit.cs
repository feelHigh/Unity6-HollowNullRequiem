// ============================================
// ObjectPoolAudit.cs
// Verifies poolable objects are properly pooled
// ============================================

using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using HNR.Core;
using HNR.Core.Interfaces;
using HNR.VFX;
using HNR.UI;

namespace HNR.Diagnostics
{
    /// <summary>
    /// Audit entry for a single pool type.
    /// </summary>
    [Serializable]
    public class PoolAuditEntry
    {
        public string TypeName;
        public int ActiveCount;
        public int PooledCount;
        public int TotalInstances;
        public int MaxRecommended;
        public bool IsHealthy;
        public string Notes;
    }

    /// <summary>
    /// Audits object pools to verify proper pooling of frequently instantiated objects.
    /// Only active in Editor and Development builds.
    /// </summary>
    /// <remarks>
    /// Pool requirements (TDD 10):
    /// - CardVisual: PoolManager
    /// - DamageNumber: PoolManager (10 pre-warm, 20 max)
    /// - VFX Hit: VFXPoolManager (5 pre-warm, 10 max)
    /// - VFX Heal: VFXPoolManager (3 pre-warm, 5 max)
    /// - StatusIcon: PoolManager
    /// - EnemySlot: PoolManager (4 max)
    ///
    /// Press [F7] to run audit.
    /// </remarks>
    public class ObjectPoolAudit : MonoBehaviour
    {
        // ============================================
        // Configuration
        // ============================================

        [Header("Settings")]
        [SerializeField, Tooltip("Key to trigger pool audit")]
        private KeyCode _auditKey = KeyCode.F7;

        [SerializeField, Tooltip("Run audit on scene load")]
        private bool _auditOnSceneLoad;

        [Header("Thresholds (TDD 10)")]
        [SerializeField] private int _vfxMaxActive = 10;
        [SerializeField] private int _damageNumberMaxActive = 20;
        [SerializeField] private int _generalMaxActive = 20;

        [Header("Audit Results")]
        [SerializeField] private List<PoolAuditEntry> _lastAuditResults = new();
        [SerializeField] private bool _lastAuditPassed;

        // ============================================
        // Properties
        // ============================================

        /// <summary>Read-only access to last audit results.</summary>
        public IReadOnlyList<PoolAuditEntry> Results => _lastAuditResults.AsReadOnly();

        /// <summary>True if last audit found no issues.</summary>
        public bool LastAuditPassed => _lastAuditPassed;

        // ============================================
        // Conditional Compilation - Dev Builds Only
        // ============================================

#if UNITY_EDITOR || DEVELOPMENT_BUILD

        private void OnEnable()
        {
            if (_auditOnSceneLoad)
            {
                UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
            }
        }

        private void OnDisable()
        {
            UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode mode)
        {
            // Delay audit to let scene settle
            Invoke(nameof(RunAudit), 0.5f);
        }

        private void Update()
        {
            if (Input.GetKeyDown(_auditKey))
            {
                RunAudit();
            }
        }

        /// <summary>
        /// Run full pool audit and log results.
        /// </summary>
        public void RunAudit()
        {
            _lastAuditResults.Clear();
            var sb = new StringBuilder();
            sb.AppendLine("=== OBJECT POOL AUDIT ===");
            sb.AppendLine();

            // Audit VFX Pool
            AuditVFXPool(sb);

            // Audit general pool
            AuditGeneralPool(sb);

            // Audit specific types that should be pooled
            AuditDamageNumbers(sb);

            // Summary
            int healthy = 0;
            int unhealthy = 0;
            foreach (var entry in _lastAuditResults)
            {
                if (entry.IsHealthy) healthy++;
                else unhealthy++;
            }

            _lastAuditPassed = unhealthy == 0;

            sb.AppendLine();
            sb.AppendLine($"=== SUMMARY: {healthy} healthy, {unhealthy} issues ===");

            if (_lastAuditPassed)
            {
                UnityEngine.Debug.Log(sb.ToString());
            }
            else
            {
                UnityEngine.Debug.LogWarning(sb.ToString());
            }
        }

        private void AuditVFXPool(StringBuilder sb)
        {
            sb.AppendLine("[VFX Pool]");

            if (!ServiceLocator.TryGet<VFXPoolManager>(out var vfxPool))
            {
                sb.AppendLine("  Status: NOT FOUND (VFXPoolManager not registered)");
                _lastAuditResults.Add(new PoolAuditEntry
                {
                    TypeName = "VFXPoolManager",
                    IsHealthy = false,
                    Notes = "Service not registered"
                });
                sb.AppendLine();
                return;
            }

            // Get debug info from VFXPoolManager
            sb.AppendLine(vfxPool.GetDebugInfo());

            // Count active VFX instances by type
            var activeVFX = FindObjectsByType<VFXInstance>(FindObjectsSortMode.None);
            var grouped = new Dictionary<string, (int active, int total)>();

            foreach (var vfx in activeVFX)
            {
                string id = string.IsNullOrEmpty(vfx.EffectId) ? "unknown" : vfx.EffectId;
                if (!grouped.ContainsKey(id))
                {
                    grouped[id] = (0, 0);
                }

                var current = grouped[id];
                current.total++;
                if (vfx.gameObject.activeInHierarchy)
                {
                    current.active++;
                }
                grouped[id] = current;
            }

            foreach (var kvp in grouped)
            {
                bool isHealthy = kvp.Value.active <= _vfxMaxActive;
                string status = isHealthy ? "OK" : "HIGH";

                sb.AppendLine($"  {kvp.Key}: {kvp.Value.active} active / {kvp.Value.total} total [{status}]");

                _lastAuditResults.Add(new PoolAuditEntry
                {
                    TypeName = $"VFX_{kvp.Key}",
                    ActiveCount = kvp.Value.active,
                    TotalInstances = kvp.Value.total,
                    PooledCount = kvp.Value.total - kvp.Value.active,
                    MaxRecommended = _vfxMaxActive,
                    IsHealthy = isHealthy,
                    Notes = isHealthy ? "" : "Exceeds max active threshold"
                });
            }

            sb.AppendLine();
        }

        private void AuditGeneralPool(StringBuilder sb)
        {
            sb.AppendLine("[General Pool]");

            if (!ServiceLocator.TryGet<IPoolManager>(out var pool))
            {
                sb.AppendLine("  Status: NOT FOUND (IPoolManager not registered)");
                _lastAuditResults.Add(new PoolAuditEntry
                {
                    TypeName = "PoolManager",
                    IsHealthy = false,
                    Notes = "Service not registered"
                });
                sb.AppendLine();
                return;
            }

            // Get debug info from PoolManager
            if (pool is PoolManager pm)
            {
                sb.AppendLine(pm.GetDebugInfo());
            }
            else
            {
                sb.AppendLine("  Pool manager registered");
            }

            sb.AppendLine($"  General max active threshold: {_generalMaxActive}");
            sb.AppendLine();
        }

        private void AuditDamageNumbers(StringBuilder sb)
        {
            sb.AppendLine("[DamageNumber Audit]");

            var damageNumbers = FindObjectsByType<DamageNumber>(FindObjectsSortMode.None);
            int activeCount = 0;
            int totalCount = damageNumbers.Length;

            foreach (var dn in damageNumbers)
            {
                if (dn.gameObject.activeInHierarchy)
                {
                    activeCount++;
                }
            }

            bool isHealthy = activeCount <= _damageNumberMaxActive;
            string status = isHealthy ? "OK" : "HIGH";

            sb.AppendLine($"  DamageNumber: {activeCount} active / {totalCount} total [{status}]");

            _lastAuditResults.Add(new PoolAuditEntry
            {
                TypeName = "DamageNumber",
                ActiveCount = activeCount,
                TotalInstances = totalCount,
                PooledCount = totalCount - activeCount,
                MaxRecommended = _damageNumberMaxActive,
                IsHealthy = isHealthy,
                Notes = isHealthy ? "" : "Exceeds max active threshold"
            });

            sb.AppendLine();
        }

        /// <summary>
        /// Get a summary string of the last audit.
        /// </summary>
        public string GetSummary()
        {
            int issues = 0;
            foreach (var entry in _lastAuditResults)
            {
                if (!entry.IsHealthy) issues++;
            }
            return $"Pool Audit: {_lastAuditResults.Count} types, {issues} issues";
        }

        /// <summary>
        /// Check if a specific pool type is healthy.
        /// </summary>
        public bool IsPoolHealthy(string typeName)
        {
            foreach (var entry in _lastAuditResults)
            {
                if (entry.TypeName == typeName)
                {
                    return entry.IsHealthy;
                }
            }
            return true; // Not tracked = assume healthy
        }

#else
        // Release build stubs - no overhead

        public void RunAudit() { }
        public string GetSummary() => string.Empty;
        public bool IsPoolHealthy(string typeName) => true;

#endif
    }
}
