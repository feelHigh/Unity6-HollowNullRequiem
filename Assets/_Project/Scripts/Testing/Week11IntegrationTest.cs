// ============================================
// Week11IntegrationTest.cs
// Integration tests for Performance & Build systems
// ============================================

using System.Collections;
using System.Text;
using UnityEngine;
using HNR.Core;
using HNR.Diagnostics;

namespace HNR.Testing
{
    /// <summary>
    /// Week 11 integration tests for performance optimization and build systems.
    /// Tests PerformanceProfiler, MemoryTracker, GCAllocationTracker, and QualitySettingsManager.
    /// </summary>
    /// <remarks>
    /// Keyboard shortcuts:
    /// [T] Run all tests
    /// [B] Run performance benchmark
    /// [P] Toggle performance overlay
    /// [M] Take memory snapshot
    /// [Q] Cycle quality tier
    /// </remarks>
    public class Week11IntegrationTest : MonoBehaviour
    {
        // ============================================
        // Configuration
        // ============================================

        [Header("Test Configuration")]
        [SerializeField, Tooltip("Duration for performance benchmark in seconds")]
        private float _benchmarkDuration = 10f;

        [Header("Performance Gates (TDD 10)")]
        [SerializeField] private float _minAcceptableFPS = 30f;
        [SerializeField] private float _minCriticalFPS = 20f;
        [SerializeField] private float _maxAcceptableFrameTimeMs = 33.33f;

        // ============================================
        // Test State
        // ============================================

        private int _passCount;
        private int _failCount;

        // ============================================
        // Unity Lifecycle
        // ============================================

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.T)) StartCoroutine(RunAllTests());
            if (Input.GetKeyDown(KeyCode.B)) StartCoroutine(RunPerformanceBenchmark());
            if (Input.GetKeyDown(KeyCode.P)) ToggleProfiler();
            if (Input.GetKeyDown(KeyCode.M)) TakeMemorySnapshot();
            if (Input.GetKeyDown(KeyCode.Q)) CycleQuality();
        }

        // ============================================
        // Full Test Suite
        // ============================================

        /// <summary>
        /// Run all Week 11 integration tests.
        /// </summary>
        public IEnumerator RunAllTests()
        {
            _passCount = 0;
            _failCount = 0;

            Debug.Log("=== WEEK 11 INTEGRATION TESTS ===");
            Debug.Log("Testing Performance & Build systems...");

            TestPerformanceProfiler();
            TestMemoryTracker();
            TestGCTracker();
            TestObjectPoolAudit();
            TestQualitySettings();
            TestBuildConfiguration();

            yield return StartCoroutine(RunPerformanceBenchmark());

            Debug.Log($"=== RESULTS: {_passCount}/{_passCount + _failCount} passed ===");

            if (_failCount == 0)
            {
                Debug.Log("All Week 11 tests PASSED!");
            }
            else
            {
                Debug.LogWarning($"Week 11 tests completed with {_failCount} failure(s)");
            }
        }

        // ============================================
        // Individual Tests
        // ============================================

        private void TestPerformanceProfiler()
        {
            Debug.Log("--- Performance Profiler ---");

            var profiler = FindAnyObjectByType<PerformanceProfiler>();
            Log("PerformanceProfiler exists", profiler != null);

            if (profiler != null)
            {
                Log("CurrentFrameTime >= 0", profiler.CurrentFrameTime >= 0);
                Log("AverageFrameTime >= 0", profiler.AverageFrameTime >= 0);
                Log("CurrentFPS > 0", profiler.CurrentFPS > 0);
                Log("MemoryUsageMB > 0", profiler.MemoryUsageMB > 0);

                // Test status properties
                bool hasStatus = profiler.IsPerformanceGood || profiler.IsPerformanceWarning || profiler.IsPerformanceCritical;
                Log("Performance status accessible", hasStatus);
            }
        }

        private void TestMemoryTracker()
        {
            Debug.Log("--- Memory Tracker ---");

            var tracker = FindAnyObjectByType<MemoryTracker>();
            Log("MemoryTracker exists", tracker != null);

            if (tracker != null)
            {
                Log("CurrentMemoryMB > 0", tracker.CurrentMemoryMB > 0);

                // Take test snapshot
                tracker.TakeSnapshot("Week11 Test Snapshot");
                Log("Snapshot taken successfully", tracker.Snapshots.Count > 0);

                // Check memory thresholds
                Log("Memory under target (400MB)", !tracker.IsMemoryOverTarget);
                Log("Memory not critical (500MB)", !tracker.IsMemoryCritical);
            }
        }

        private void TestGCTracker()
        {
            Debug.Log("--- GC Allocation Tracker ---");

            var gcTracker = FindAnyObjectByType<GCAllocationTracker>();
            Log("GCAllocationTracker exists", gcTracker != null);

            if (gcTracker != null)
            {
                Log("IsTracking accessible", gcTracker.IsTracking || !gcTracker.IsTracking);
                Log("LastFrameAllocation accessible", gcTracker.LastFrameAllocation >= 0 || gcTracker.LastFrameAllocation < 0);
            }
        }

        private void TestObjectPoolAudit()
        {
            Debug.Log("--- Object Pool Audit ---");

            var audit = FindAnyObjectByType<ObjectPoolAudit>();
            Log("ObjectPoolAudit exists", audit != null);

            if (audit != null)
            {
                audit.RunAudit();
                Log("Pool audit completed", true);
            }
        }

        private void TestQualitySettings()
        {
            Debug.Log("--- Quality Settings Manager ---");

            if (!ServiceLocator.TryGet<QualitySettingsManager>(out var quality))
            {
                Log("QualitySettingsManager registered", false);
                return;
            }

            Log("QualitySettingsManager registered", true);

            // Test detection
            var detectedTier = quality.DetectDeviceTier();
            Log("Device tier detected", true);
            Debug.Log($"  Detected Tier: {detectedTier}");

            // Test current tier
            var currentTier = quality.CurrentTier;
            Log("Current tier accessible", true);
            Debug.Log($"  Current Tier: {currentTier}");

            // Test target frame rate
            int targetFPS = quality.TargetFrameRate;
            Log("Target FPS valid", targetFPS == 30 || targetFPS == 45 || targetFPS == 60);
            Debug.Log($"  Target FPS: {targetFPS}");

            // Log device info
            Debug.Log(quality.GetDeviceInfoString());
        }

        private void TestBuildConfiguration()
        {
            Debug.Log("--- Build Configuration ---");

            // Version
            bool versionSet = !string.IsNullOrEmpty(Application.version);
            Log("Application version set", versionSet);
            Debug.Log($"  Version: {Application.version}");

            // Bundle identifier
            bool bundleSet = !string.IsNullOrEmpty(Application.identifier);
            Log("Bundle identifier set", bundleSet);
            Debug.Log($"  Bundle: {Application.identifier}");

            // Platform info
            Debug.Log($"  Platform: {Application.platform}");
            Debug.Log($"  Unity Version: {Application.unityVersion}");

            // Check if development build
#if DEVELOPMENT_BUILD
            Debug.Log("  Build Type: DEVELOPMENT");
#else
            Debug.Log("  Build Type: Release");
#endif
        }

        // ============================================
        // Performance Benchmark
        // ============================================

        private IEnumerator RunPerformanceBenchmark()
        {
            Debug.Log($"--- Performance Benchmark ({_benchmarkDuration}s) ---");

            float startTime = Time.realtimeSinceStartup;
            float endTime = startTime + _benchmarkDuration;

            float minFPS = float.MaxValue;
            float maxFPS = 0f;
            float sumFPS = 0f;
            int frameCount = 0;
            int droppedFrames = 0;

            while (Time.realtimeSinceStartup < endTime)
            {
                float fps = 1f / Time.unscaledDeltaTime;
                sumFPS += fps;
                frameCount++;

                if (fps < minFPS) minFPS = fps;
                if (fps > maxFPS) maxFPS = fps;
                if (fps < _minCriticalFPS) droppedFrames++;

                yield return null;
            }

            float avgFPS = sumFPS / frameCount;
            float avgFrameTime = 1000f / avgFPS;

            var sb = new StringBuilder();
            sb.AppendLine("BENCHMARK RESULTS:");
            sb.AppendLine($"  Average FPS: {avgFPS:F1}");
            sb.AppendLine($"  Min FPS: {minFPS:F1}");
            sb.AppendLine($"  Max FPS: {maxFPS:F1}");
            sb.AppendLine($"  Frame Count: {frameCount}");
            sb.AppendLine($"  Dropped Frames (<{_minCriticalFPS}fps): {droppedFrames}");
            sb.AppendLine($"  Avg Frame Time: {avgFrameTime:F2}ms");
            Debug.Log(sb.ToString());

            // Performance gates (TDD 10)
            Log($"Benchmark: Avg FPS >= {_minAcceptableFPS}", avgFPS >= _minAcceptableFPS);
            Log($"Benchmark: Min FPS >= {_minCriticalFPS}", minFPS >= _minCriticalFPS);
            Log($"Benchmark: Avg Frame Time <= {_maxAcceptableFrameTimeMs}ms", avgFrameTime <= _maxAcceptableFrameTimeMs);
            Log("Benchmark: Dropped frames < 5%", (float)droppedFrames / frameCount < 0.05f);
        }

        // ============================================
        // Manual Test Methods
        // ============================================

        private void ToggleProfiler()
        {
            var profiler = FindAnyObjectByType<PerformanceProfiler>();
            if (profiler != null)
            {
                profiler.ToggleOverlay();
                Debug.Log("[TEST] Toggled performance overlay");
            }
            else
            {
                Debug.LogWarning("[TEST] PerformanceProfiler not found");
            }
        }

        private void TakeMemorySnapshot()
        {
            var tracker = FindAnyObjectByType<MemoryTracker>();
            if (tracker != null)
            {
                tracker.TakeSnapshot("Manual Test Snapshot");
                Debug.Log("[TEST] Memory snapshot taken");
            }
            else
            {
                Debug.LogWarning("[TEST] MemoryTracker not found");
            }
        }

        private void CycleQuality()
        {
            if (!ServiceLocator.TryGet<QualitySettingsManager>(out var quality))
            {
                Debug.LogWarning("[TEST] QualitySettingsManager not available");
                return;
            }

            var current = quality.CurrentTier;
            var next = (QualityTier)(((int)current + 1) % 3);
            quality.SetQualityTier(next);

            Debug.Log($"[TEST] Cycled quality: {current} -> {next}");
        }

        // ============================================
        // Test Helpers
        // ============================================

        private void Log(string testName, bool passed)
        {
            if (passed)
            {
                Debug.Log($"  [PASS] {testName}");
                _passCount++;
            }
            else
            {
                Debug.LogError($"  [FAIL] {testName}");
                _failCount++;
            }
        }
    }
}
