// ============================================
// PerformanceProfiler.cs
// Runtime performance monitoring with visual overlay
// ============================================

using UnityEngine;
using UnityEngine.Profiling;
using System.Collections.Generic;

namespace HNR.Diagnostics
{
    /// <summary>
    /// Runtime performance profiler with visual overlay.
    /// Only active in Editor and Development builds.
    /// </summary>
    /// <remarks>
    /// Performance targets (from TDD 10):
    /// - Frame Rate: 60fps (16.67ms), minimum 30fps
    /// - Draw Calls: &lt;50 target, &lt;100 minimum
    /// - Triangles: &lt;50K target, &lt;100K minimum
    /// - RAM: &lt;400MB target, &lt;500MB minimum
    ///
    /// Press [P] to toggle overlay display.
    /// </remarks>
    public class PerformanceProfiler : MonoBehaviour
    {
        // ============================================
        // Configuration
        // ============================================

        [Header("Display Settings")]
        [SerializeField, Tooltip("Show performance overlay on screen")]
        private bool _showOverlay;

        [SerializeField, Tooltip("Key to toggle overlay visibility")]
        private KeyCode _toggleKey = KeyCode.P;

        [Header("Performance Targets (TDD 10)")]
        [SerializeField] private int _targetFPS = 60;
        [SerializeField] private float _goodFrameTimeMs = 16.67f;
        [SerializeField] private float _warningFrameTimeMs = 25f;
        [SerializeField] private float _criticalFrameTimeMs = 33.33f;

        [Header("Draw Call Thresholds")]
        [SerializeField] private int _targetDrawCalls = 50;
        [SerializeField] private int _maxDrawCalls = 100;

        [Header("Memory Thresholds (MB)")]
        [SerializeField] private int _targetMemoryMB = 400;
        [SerializeField] private int _maxMemoryMB = 500;

        [Header("Sampling")]
        [SerializeField, Tooltip("Number of frames for rolling average")]
        private int _sampleCount = 60;

        // ============================================
        // Metrics
        // ============================================

        private Queue<float> _frameTimes = new();
        private float _currentFrameTime;
        private float _averageFrameTime;
        private float _worstFrameTime;
        private long _memoryUsage;
        private int _drawCallCount;
        private int _triangleCount;

        // GC tracking
        private int _lastGCCount;
        private int _gcThisFrame;

        // ============================================
        // Properties
        // ============================================

        /// <summary>Current frame time in milliseconds.</summary>
        public float CurrentFrameTime => _currentFrameTime;

        /// <summary>Rolling average frame time in milliseconds.</summary>
        public float AverageFrameTime => _averageFrameTime;

        /// <summary>Worst frame time in sample window.</summary>
        public float WorstFrameTime => _worstFrameTime;

        /// <summary>Current memory usage in megabytes.</summary>
        public long MemoryUsageMB => _memoryUsage / (1024 * 1024);

        /// <summary>Current draw call count (Editor only).</summary>
        public int DrawCallCount => _drawCallCount;

        /// <summary>Current triangle count (Editor only).</summary>
        public int TriangleCount => _triangleCount;

        /// <summary>Current FPS based on current frame time.</summary>
        public float CurrentFPS => _currentFrameTime > 0 ? 1000f / _currentFrameTime : 0;

        /// <summary>Average FPS based on rolling average.</summary>
        public float AverageFPS => _averageFrameTime > 0 ? 1000f / _averageFrameTime : 0;

        /// <summary>True if performance is within target (60fps).</summary>
        public bool IsPerformanceGood => _averageFrameTime <= _goodFrameTimeMs;

        /// <summary>True if performance is degraded but playable.</summary>
        public bool IsPerformanceWarning => _averageFrameTime > _goodFrameTimeMs && _averageFrameTime <= _warningFrameTimeMs;

        /// <summary>True if performance is critically low.</summary>
        public bool IsPerformanceCritical => _averageFrameTime > _criticalFrameTimeMs;

        /// <summary>True if draw calls exceed maximum threshold.</summary>
        public bool IsDrawCallsCritical => _drawCallCount > _maxDrawCalls;

        /// <summary>True if memory usage exceeds maximum threshold.</summary>
        public bool IsMemoryCritical => MemoryUsageMB > _maxMemoryMB;

        // ============================================
        // Conditional Compilation - Dev Builds Only
        // ============================================

#if UNITY_EDITOR || DEVELOPMENT_BUILD

        private void Awake()
        {
            // Apply target frame rate from configuration
            Application.targetFrameRate = _targetFPS;
            UnityEngine.Debug.Log($"[PerformanceProfiler] Target FPS set to {_targetFPS}");
        }

        private void Update()
        {
            if (Input.GetKeyDown(_toggleKey))
            {
                ToggleOverlay();
            }

            UpdateMetrics();
        }

        private void UpdateMetrics()
        {
            // Frame time (in milliseconds)
            _currentFrameTime = Time.unscaledDeltaTime * 1000f;

            // Rolling average
            _frameTimes.Enqueue(_currentFrameTime);
            if (_frameTimes.Count > _sampleCount)
            {
                _frameTimes.Dequeue();
            }

            // Calculate average and worst
            float sum = 0f;
            _worstFrameTime = 0f;
            foreach (var ft in _frameTimes)
            {
                sum += ft;
                if (ft > _worstFrameTime)
                {
                    _worstFrameTime = ft;
                }
            }
            _averageFrameTime = sum / _frameTimes.Count;

            // Memory usage
            _memoryUsage = Profiler.GetTotalAllocatedMemoryLong();

            // GC tracking
            int currentGCCount = System.GC.CollectionCount(0);
            _gcThisFrame = currentGCCount - _lastGCCount;
            _lastGCCount = currentGCCount;

            // Draw calls (Editor only)
#if UNITY_EDITOR
            _drawCallCount = UnityEditor.UnityStats.batches;
            _triangleCount = UnityEditor.UnityStats.triangles;
#endif
        }

        /// <summary>
        /// Toggle overlay visibility.
        /// </summary>
        public void ToggleOverlay()
        {
            _showOverlay = !_showOverlay;
            UnityEngine.Debug.Log($"[PerformanceProfiler] Overlay {(_showOverlay ? "enabled" : "disabled")}");
        }

        /// <summary>
        /// Reset all statistics.
        /// </summary>
        public void ResetStats()
        {
            _frameTimes.Clear();
            _worstFrameTime = 0f;
            _lastGCCount = System.GC.CollectionCount(0);
        }

        /// <summary>
        /// Get performance report as formatted string.
        /// </summary>
        public string GetReport()
        {
            string status = IsPerformanceGood ? "GOOD" : IsPerformanceWarning ? "WARNING" : "CRITICAL";
            return $"[{status}] Performance Report\n" +
                   $"FPS: {AverageFPS:F0} (Current: {CurrentFPS:F0})\n" +
                   $"Frame Time: {_averageFrameTime:F1}ms (Worst: {_worstFrameTime:F1}ms)\n" +
                   $"Memory: {MemoryUsageMB}MB / {_targetMemoryMB}MB target\n" +
                   $"Draw Calls: {_drawCallCount} / {_targetDrawCalls} target\n" +
                   $"Triangles: {_triangleCount}";
        }

        /// <summary>
        /// Log current performance to console.
        /// </summary>
        public void LogPerformance()
        {
            UnityEngine.Debug.Log($"[PerformanceProfiler] {GetReport()}");
        }

        private void OnGUI()
        {
            if (!_showOverlay) return;

            // Determine color based on performance
            Color bgColor;
            if (IsPerformanceGood)
            {
                bgColor = new Color(0f, 0.5f, 0f, 0.8f); // Green
            }
            else if (IsPerformanceWarning)
            {
                bgColor = new Color(0.5f, 0.5f, 0f, 0.8f); // Yellow
            }
            else
            {
                bgColor = new Color(0.5f, 0f, 0f, 0.8f); // Red
            }

            // Create background style
            var boxStyle = new GUIStyle(GUI.skin.box);
            var bgTex = new Texture2D(1, 1);
            bgTex.SetPixel(0, 0, bgColor);
            bgTex.Apply();
            boxStyle.normal.background = bgTex;
            boxStyle.normal.textColor = Color.white;
            boxStyle.fontSize = 18;
            boxStyle.padding = new RectOffset(10, 10, 10, 10);
            boxStyle.alignment = TextAnchor.UpperLeft;

            // Build display text
            string status = IsPerformanceGood ? "GOOD" : IsPerformanceWarning ? "WARNING" : "CRITICAL";
            string memStatus = MemoryUsageMB > _targetMemoryMB ? "!" : "";
            string drawStatus = _drawCallCount > _targetDrawCalls ? "!" : "";

            string info = $"[{status}] FPS: {CurrentFPS:F0} (Avg: {AverageFPS:F0})\n" +
                         $"Frame: {_currentFrameTime:F1}ms (Avg: {_averageFrameTime:F1}ms)\n" +
                         $"Worst: {_worstFrameTime:F1}ms\n" +
                         $"Memory: {MemoryUsageMB}MB{memStatus}\n" +
                         $"Draw Calls: {_drawCallCount}{drawStatus}\n" +
                         $"GC: {_gcThisFrame}";

            // Draw box in top-left corner
            Rect boxRect = new Rect(10, 10, 300, 155);
            GUI.Box(boxRect, info, boxStyle);

            // Cleanup texture to avoid memory leak
            Destroy(bgTex);
        }

#else
        // Release build stubs - no overhead

        public void ToggleOverlay() { }
        public void ResetStats() { }
        public string GetReport() => string.Empty;
        public void LogPerformance() { }

#endif
    }
}
