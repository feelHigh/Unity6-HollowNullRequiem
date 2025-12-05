// ============================================
// MemoryTracker.cs
// Memory usage monitoring and leak detection
// ============================================

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.SceneManagement;

namespace HNR.Diagnostics
{
    /// <summary>
    /// Memory snapshot data captured at a point in time.
    /// </summary>
    [Serializable]
    public class MemorySnapshot
    {
        public long TotalMemory;
        public long MonoHeap;
        public long GfxMemory;
        public float Timestamp;
        public string Context;
        public long DeltaFromPrevious;

        public float TotalMB => TotalMemory / (1024f * 1024f);
        public float MonoHeapMB => MonoHeap / (1024f * 1024f);
        public float GfxMemoryMB => GfxMemory / (1024f * 1024f);
        public float DeltaMB => DeltaFromPrevious / (1024f * 1024f);
    }

    /// <summary>
    /// Tracks memory usage over time and detects potential leaks.
    /// Only active in Editor and Development builds.
    /// </summary>
    /// <remarks>
    /// Snapshot triggers:
    /// - Scene load complete
    /// - Every 60 seconds during gameplay
    /// - Manual trigger via [F5]
    /// - Combat start/end (via public API)
    ///
    /// Leak detection:
    /// - Alerts if memory grows consistently over 5+ snapshots
    /// - Threshold: >10MB growth without plateau
    ///
    /// Press [F5] to take snapshot, [F6] to log report.
    /// </remarks>
    public class MemoryTracker : MonoBehaviour
    {
        // ============================================
        // Configuration
        // ============================================

        [Header("Snapshot Settings")]
        [SerializeField, Tooltip("Interval between automatic snapshots (seconds)")]
        private float _snapshotInterval = 60f;

        [SerializeField, Tooltip("Maximum snapshots to retain")]
        private int _maxSnapshots = 30;

        [SerializeField, Tooltip("Key to take manual snapshot")]
        private KeyCode _snapshotKey = KeyCode.F5;

        [SerializeField, Tooltip("Key to log memory report")]
        private KeyCode _reportKey = KeyCode.F6;

        [Header("Leak Detection (TDD 10)")]
        [SerializeField, Tooltip("Memory growth threshold to trigger leak warning (bytes)")]
        private long _leakThresholdBytes = 10 * 1024 * 1024; // 10MB

        [SerializeField, Tooltip("Consecutive growth snapshots required to detect leak")]
        private int _consecutiveGrowthForLeak = 5;

        [Header("Memory Targets (TDD 10)")]
        [SerializeField] private long _targetMemoryMB = 400;
        [SerializeField] private long _maxMemoryMB = 500;

        // ============================================
        // Runtime State
        // ============================================

        private List<MemorySnapshot> _snapshots = new();
        private Coroutine _periodicSnapshot;
        private bool _leakWarningIssued;

        // ============================================
        // Properties
        // ============================================

        /// <summary>Read-only access to all snapshots.</summary>
        public IReadOnlyList<MemorySnapshot> Snapshots => _snapshots.AsReadOnly();

        /// <summary>Current total allocated memory in bytes.</summary>
        public long CurrentMemory => Profiler.GetTotalAllocatedMemoryLong();

        /// <summary>Current total allocated memory in megabytes.</summary>
        public float CurrentMemoryMB => CurrentMemory / (1024f * 1024f);

        /// <summary>True if current memory exceeds target threshold.</summary>
        public bool IsMemoryOverTarget => CurrentMemoryMB > _targetMemoryMB;

        /// <summary>True if current memory exceeds maximum threshold.</summary>
        public bool IsMemoryCritical => CurrentMemoryMB > _maxMemoryMB;

        /// <summary>True if a potential leak has been detected.</summary>
        public bool LeakDetected => _leakWarningIssued;

        // ============================================
        // Conditional Compilation - Dev Builds Only
        // ============================================

#if UNITY_EDITOR || DEVELOPMENT_BUILD

        private void OnEnable()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
            StartPeriodicSnapshots();
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            StopPeriodicSnapshots();
        }

        private void Update()
        {
            if (Input.GetKeyDown(_snapshotKey))
            {
                TakeSnapshot("Manual Snapshot");
            }

            if (Input.GetKeyDown(_reportKey))
            {
                LogReport();
            }
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            // Wait a frame for scene to settle
            StartCoroutine(DelayedSceneSnapshot(scene.name));
        }

        private IEnumerator DelayedSceneSnapshot(string sceneName)
        {
            yield return null;
            TakeSnapshot($"Scene Loaded: {sceneName}");
        }

        /// <summary>
        /// Start periodic automatic snapshots.
        /// </summary>
        public void StartPeriodicSnapshots()
        {
            if (_periodicSnapshot != null)
            {
                StopCoroutine(_periodicSnapshot);
            }
            _periodicSnapshot = StartCoroutine(PeriodicSnapshotRoutine());
        }

        /// <summary>
        /// Stop periodic automatic snapshots.
        /// </summary>
        public void StopPeriodicSnapshots()
        {
            if (_periodicSnapshot != null)
            {
                StopCoroutine(_periodicSnapshot);
                _periodicSnapshot = null;
            }
        }

        private IEnumerator PeriodicSnapshotRoutine()
        {
            while (true)
            {
                yield return new WaitForSeconds(_snapshotInterval);
                TakeSnapshot("Periodic");
            }
        }

        /// <summary>
        /// Take a memory snapshot with optional context description.
        /// </summary>
        /// <param name="context">Description of when/why snapshot was taken</param>
        public void TakeSnapshot(string context = "")
        {
            long currentTotal = Profiler.GetTotalAllocatedMemoryLong();
            long monoHeap = Profiler.GetMonoHeapSizeLong();
            long gfxMemory = Profiler.GetAllocatedMemoryForGraphicsDriver();

            long delta = 0;
            if (_snapshots.Count > 0)
            {
                delta = currentTotal - _snapshots[_snapshots.Count - 1].TotalMemory;
            }

            var snapshot = new MemorySnapshot
            {
                TotalMemory = currentTotal,
                MonoHeap = monoHeap,
                GfxMemory = gfxMemory,
                Timestamp = Time.realtimeSinceStartup,
                Context = context,
                DeltaFromPrevious = delta
            };

            _snapshots.Add(snapshot);

            // Limit snapshot count
            while (_snapshots.Count > _maxSnapshots)
            {
                _snapshots.RemoveAt(0);
            }

            // Log snapshot
            string deltaStr = delta >= 0 ? $"+{snapshot.DeltaMB:F1}" : $"{snapshot.DeltaMB:F1}";
            UnityEngine.Debug.Log($"[MemoryTracker] {snapshot.TotalMB:F1}MB ({deltaStr}MB) - {context}");

            // Check for leaks
            AnalyzeForLeaks();
        }

        /// <summary>
        /// Take snapshot for combat start event.
        /// </summary>
        public void OnCombatStart()
        {
            TakeSnapshot("Combat Start");
        }

        /// <summary>
        /// Take snapshot for combat end event.
        /// </summary>
        public void OnCombatEnd()
        {
            TakeSnapshot("Combat End");
        }

        /// <summary>
        /// Analyze recent snapshots for memory leak patterns.
        /// Alerts if memory grows consistently over configured threshold.
        /// </summary>
        public void AnalyzeForLeaks()
        {
            if (_snapshots.Count < _consecutiveGrowthForLeak) return;

            int consecutiveGrowth = 0;
            long totalGrowth = 0;

            // Check from most recent backwards
            for (int i = _snapshots.Count - 1; i > 0 && consecutiveGrowth < _consecutiveGrowthForLeak; i--)
            {
                if (_snapshots[i].DeltaFromPrevious > 0)
                {
                    consecutiveGrowth++;
                    totalGrowth += _snapshots[i].DeltaFromPrevious;
                }
                else
                {
                    break; // Growth pattern broken
                }
            }

            // Issue warning if leak pattern detected
            if (consecutiveGrowth >= _consecutiveGrowthForLeak &&
                totalGrowth > _leakThresholdBytes &&
                !_leakWarningIssued)
            {
                float growthMB = totalGrowth / (1024f * 1024f);
                UnityEngine.Debug.LogWarning(
                    $"[MemoryTracker] POTENTIAL MEMORY LEAK DETECTED!\n" +
                    $"Memory grew for {consecutiveGrowth} consecutive snapshots.\n" +
                    $"Total growth: +{growthMB:F1}MB");
                _leakWarningIssued = true;
            }
            else if (consecutiveGrowth < _consecutiveGrowthForLeak)
            {
                _leakWarningIssued = false; // Reset warning if pattern breaks
            }
        }

        /// <summary>
        /// Force garbage collection and take snapshot.
        /// Useful for determining true memory usage vs cached objects.
        /// </summary>
        public void ForceGCAndSnapshot()
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            TakeSnapshot("After GC.Collect()");
        }

        /// <summary>
        /// Log full memory report to console.
        /// </summary>
        public void LogReport()
        {
            var sb = new StringBuilder();
            sb.AppendLine("=== MEMORY REPORT ===");
            sb.AppendLine($"Current: {CurrentMemoryMB:F1}MB (Target: {_targetMemoryMB}MB, Max: {_maxMemoryMB}MB)");
            sb.AppendLine($"Mono Heap: {Profiler.GetMonoHeapSizeLong() / (1024f * 1024f):F1}MB");
            sb.AppendLine($"GFX Memory: {Profiler.GetAllocatedMemoryForGraphicsDriver() / (1024f * 1024f):F1}MB");
            sb.AppendLine($"Snapshots: {_snapshots.Count}");
            sb.AppendLine($"Leak Warning: {(_leakWarningIssued ? "YES" : "No")}");
            sb.AppendLine();

            if (_snapshots.Count > 0)
            {
                sb.AppendLine("Recent Snapshots:");
                int startIdx = Mathf.Max(0, _snapshots.Count - 10);
                for (int i = startIdx; i < _snapshots.Count; i++)
                {
                    var s = _snapshots[i];
                    string deltaStr = s.DeltaFromPrevious >= 0
                        ? $"+{s.DeltaMB:F1}"
                        : $"{s.DeltaMB:F1}";
                    sb.AppendLine($"  [{i}] {s.TotalMB:F1}MB ({deltaStr}MB) @ {s.Timestamp:F0}s - {s.Context}");
                }

                // Calculate total growth
                if (_snapshots.Count >= 2)
                {
                    var first = _snapshots[0];
                    var last = _snapshots[_snapshots.Count - 1];
                    float totalGrowth = last.TotalMB - first.TotalMB;
                    float duration = last.Timestamp - first.Timestamp;
                    sb.AppendLine();
                    sb.AppendLine($"Total Growth: {totalGrowth:+0.0;-0.0}MB over {duration:F0}s");
                }
            }

            UnityEngine.Debug.Log(sb.ToString());
        }

        /// <summary>
        /// Get summary string for display.
        /// </summary>
        public string GetSummary()
        {
            string status = IsMemoryCritical ? "CRITICAL" : IsMemoryOverTarget ? "HIGH" : "OK";
            return $"[{status}] Memory: {CurrentMemoryMB:F0}MB";
        }

        /// <summary>
        /// Clear all snapshots and reset leak detection.
        /// </summary>
        public void ClearSnapshots()
        {
            _snapshots.Clear();
            _leakWarningIssued = false;
            UnityEngine.Debug.Log("[MemoryTracker] Snapshots cleared");
        }

#else
        // Release build stubs - no overhead

        public void StartPeriodicSnapshots() { }
        public void StopPeriodicSnapshots() { }
        public void TakeSnapshot(string context = "") { }
        public void OnCombatStart() { }
        public void OnCombatEnd() { }
        public void AnalyzeForLeaks() { }
        public void ForceGCAndSnapshot() { }
        public void LogReport() { }
        public string GetSummary() => string.Empty;
        public void ClearSnapshots() { }

#endif
    }
}
