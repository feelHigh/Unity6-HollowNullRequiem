// ============================================
// GCAllocationTracker.cs
// Per-frame garbage allocation monitoring for hot paths
// ============================================

using UnityEngine;
using UnityEngine.Profiling;

namespace HNR.Diagnostics
{
    /// <summary>
    /// Tracks per-frame garbage allocations to detect hot path violations.
    /// Only active in Editor and Development builds.
    /// </summary>
    /// <remarks>
    /// Hot path rules (TDD 10):
    /// - ZERO allocations allowed in Update loops
    /// - Pre-allocate all lists and arrays
    /// - Use StringBuilder for string concatenation
    /// - Cache component references
    /// - Avoid LINQ in Update loops
    ///
    /// Press [G] to toggle tracking on/off.
    /// </remarks>
    public class GCAllocationTracker : MonoBehaviour
    {
        // ============================================
        // Configuration
        // ============================================

        [Header("Settings")]
        [SerializeField, Tooltip("Enable allocation tracking")]
        private bool _trackEnabled = true;

        [SerializeField, Tooltip("Key to toggle tracking")]
        private KeyCode _toggleKey = KeyCode.G;

        [SerializeField, Tooltip("Allocation threshold for warning (bytes)")]
        private long _warningThresholdBytes = 1024; // 1KB per frame

        [SerializeField, Tooltip("Maximum warnings to log before suppressing")]
        private int _maxWarningsToLog = 10;

        [Header("Current Frame Stats")]
        [SerializeField] private long _lastFrameAllocation;
        [SerializeField] private long _peakAllocation;
        [SerializeField] private int _framesWithAllocations;

        // ============================================
        // Private State
        // ============================================

        private long _previousTotalMemory;
        private int _warningCount;
        private int _totalFramesTracked;

        // ============================================
        // Properties
        // ============================================

        /// <summary>Bytes allocated in the last frame.</summary>
        public long LastFrameAllocation => _lastFrameAllocation;

        /// <summary>Peak single-frame allocation since last reset.</summary>
        public long PeakAllocation => _peakAllocation;

        /// <summary>Number of frames that exceeded warning threshold.</summary>
        public int WarningCount => _warningCount;

        /// <summary>Number of frames with any allocation.</summary>
        public int FramesWithAllocations => _framesWithAllocations;

        /// <summary>Total frames tracked since last reset.</summary>
        public int TotalFramesTracked => _totalFramesTracked;

        /// <summary>Percentage of frames that had allocations.</summary>
        public float AllocationFrequency => _totalFramesTracked > 0
            ? (float)_framesWithAllocations / _totalFramesTracked * 100f
            : 0f;

        /// <summary>Whether tracking is currently enabled.</summary>
        public bool IsTracking => _trackEnabled;

        // ============================================
        // Conditional Compilation - Dev Builds Only
        // ============================================

#if UNITY_EDITOR || DEVELOPMENT_BUILD

        private void Awake()
        {
            CalibrateBaseline();
        }

        private void Update()
        {
            if (Input.GetKeyDown(_toggleKey))
            {
                ToggleTracking();
            }

            if (!_trackEnabled) return;

            TrackAllocations();
        }

        private void TrackAllocations()
        {
            long currentTotal = Profiler.GetTotalAllocatedMemoryLong();

            // Calculate this frame's allocation
            _lastFrameAllocation = currentTotal - _previousTotalMemory;
            _totalFramesTracked++;

            // Track frames with any allocation (ignore negative from GC)
            if (_lastFrameAllocation > 0)
            {
                _framesWithAllocations++;
            }

            // Track peak (ignore negative values from GC)
            if (_lastFrameAllocation > _peakAllocation)
            {
                _peakAllocation = _lastFrameAllocation;
            }

            // Warn if exceeding threshold
            if (_lastFrameAllocation > _warningThresholdBytes)
            {
                _warningCount++;
                if (_warningCount <= _maxWarningsToLog)
                {
                    float kb = _lastFrameAllocation / 1024f;
                    UnityEngine.Debug.LogWarning(
                        $"[GCTracker] High allocation: {kb:F1}KB this frame (threshold: {_warningThresholdBytes / 1024f:F1}KB)");
                }
                else if (_warningCount == _maxWarningsToLog + 1)
                {
                    UnityEngine.Debug.LogWarning(
                        $"[GCTracker] Suppressing further warnings. Use ResetStats() to resume logging.");
                }
            }

            _previousTotalMemory = currentTotal;
        }

        /// <summary>
        /// Toggle allocation tracking on/off.
        /// </summary>
        public void ToggleTracking()
        {
            _trackEnabled = !_trackEnabled;
            UnityEngine.Debug.Log($"[GCTracker] Tracking {(_trackEnabled ? "ENABLED" : "DISABLED")}");

            if (_trackEnabled)
            {
                CalibrateBaseline();
            }
        }

        /// <summary>
        /// Enable tracking.
        /// </summary>
        public void EnableTracking()
        {
            if (!_trackEnabled)
            {
                _trackEnabled = true;
                CalibrateBaseline();
                UnityEngine.Debug.Log("[GCTracker] Tracking ENABLED");
            }
        }

        /// <summary>
        /// Disable tracking.
        /// </summary>
        public void DisableTracking()
        {
            if (_trackEnabled)
            {
                _trackEnabled = false;
                UnityEngine.Debug.Log("[GCTracker] Tracking DISABLED");
            }
        }

        /// <summary>
        /// Reset all statistics and peak tracking.
        /// </summary>
        public void ResetStats()
        {
            _peakAllocation = 0;
            _warningCount = 0;
            _framesWithAllocations = 0;
            _totalFramesTracked = 0;
            CalibrateBaseline();
            UnityEngine.Debug.Log("[GCTracker] Stats reset");
        }

        /// <summary>
        /// Start tracking from current memory state.
        /// Call after loading or other expected allocation spikes.
        /// </summary>
        public void CalibrateBaseline()
        {
            _previousTotalMemory = Profiler.GetTotalAllocatedMemoryLong();
            _lastFrameAllocation = 0;
        }

        /// <summary>
        /// Log current statistics to console.
        /// </summary>
        public void LogStats()
        {
            float peakKB = _peakAllocation / 1024f;
            UnityEngine.Debug.Log(
                $"[GCTracker] Stats:\n" +
                $"  Peak Allocation: {peakKB:F1}KB\n" +
                $"  Warning Count: {_warningCount}\n" +
                $"  Frames with Allocations: {_framesWithAllocations}/{_totalFramesTracked} ({AllocationFrequency:F1}%)");
        }

        /// <summary>
        /// Get summary string for display.
        /// </summary>
        public string GetSummary()
        {
            float lastKB = _lastFrameAllocation / 1024f;
            float peakKB = _peakAllocation / 1024f;
            return $"GC: {lastKB:F1}KB (Peak: {peakKB:F1}KB)";
        }

#else
        // Release build stubs - no overhead

        public void ToggleTracking() { }
        public void EnableTracking() { }
        public void DisableTracking() { }
        public void ResetStats() { }
        public void CalibrateBaseline() { }
        public void LogStats() { }
        public string GetSummary() => string.Empty;

#endif
    }
}
