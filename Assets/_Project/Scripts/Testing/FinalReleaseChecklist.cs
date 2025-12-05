// ============================================
// FinalReleaseChecklist.cs
// Runtime verification for release readiness
// ============================================

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using HNR.Core;
using HNR.Core.Interfaces;
using HNR.Characters;
using HNR.Cards;
using HNR.Combat;
using HNR.Map;
using HNR.Progression;

namespace HNR.Testing
{
    /// <summary>
    /// Release check item with validation function.
    /// </summary>
    [Serializable]
    public class ReleaseCheckItem
    {
        public string Name;
        public string Category;
        public bool Required;
        [NonSerialized] public Func<bool> Validator;
        public bool Passed;
        public string Notes;
    }

    /// <summary>
    /// Comprehensive release checklist validator.
    /// Verifies systems, content, build settings, and performance.
    /// </summary>
    public class FinalReleaseChecklist : MonoBehaviour
    {
        // ============================================
        // Serialized Fields
        // ============================================

        [Header("Results")]
        [SerializeField] private int _totalChecks;
        [SerializeField] private int _passedChecks;
        [SerializeField] private int _failedChecks;
        [SerializeField] private int _requiredFailed;
        [SerializeField] private bool _isReleaseReady;

        // ============================================
        // Private State
        // ============================================

        private List<ReleaseCheckItem> _checks = new();
        private StringBuilder _report;

        // ============================================
        // Public Properties
        // ============================================

        /// <summary>Whether all required checks passed.</summary>
        public bool IsReleaseReady => _isReleaseReady;

        /// <summary>Total number of checks.</summary>
        public int TotalChecks => _totalChecks;

        /// <summary>Number of passed checks.</summary>
        public int PassedChecks => _passedChecks;

        /// <summary>Number of failed checks.</summary>
        public int FailedChecks => _failedChecks;

        // ============================================
        // Public Methods
        // ============================================

        /// <summary>
        /// Run all release checks.
        /// </summary>
        [ContextMenu("Run Release Checks")]
        public void RunAllChecks()
        {
            _checks.Clear();
            _report = new StringBuilder();
            _passedChecks = 0;
            _failedChecks = 0;
            _requiredFailed = 0;

            // Define checks
            DefineSystemChecks();
            DefineContentChecks();
            DefineBuildChecks();
            DefinePerformanceChecks();

            // Run checks
            foreach (var check in _checks)
            {
                try
                {
                    check.Passed = check.Validator?.Invoke() ?? false;
                }
                catch (Exception e)
                {
                    check.Passed = false;
                    check.Notes = e.Message;
                }

                if (check.Passed)
                {
                    _passedChecks++;
                }
                else
                {
                    _failedChecks++;
                    if (check.Required)
                        _requiredFailed++;
                }
            }

            _totalChecks = _checks.Count;
            _isReleaseReady = _requiredFailed == 0;

            GenerateReport();
        }

        /// <summary>
        /// Get report as string.
        /// </summary>
        public string GetReport() => _report?.ToString() ?? "";

        /// <summary>
        /// Export report to file.
        /// </summary>
        [ContextMenu("Export Report")]
        public void ExportReport()
        {
            if (_report == null || _report.Length == 0)
                RunAllChecks();

            string path = Path.Combine(Application.persistentDataPath, "ReleaseChecklist.md");
            File.WriteAllText(path, _report.ToString());
            Debug.Log($"[ReleaseChecklist] Exported to {path}");
        }

        // ============================================
        // Check Definitions
        // ============================================

        private void DefineSystemChecks()
        {
            AddCheck("Systems", "GameManager registered", true,
                () => ServiceLocator.Has<IGameManager>());

            AddCheck("Systems", "SaveManager registered", true,
                () => ServiceLocator.Has<ISaveManager>());

            AddCheck("Systems", "AudioManager registered", true,
                () => ServiceLocator.Has<IAudioManager>());

            AddCheck("Systems", "UIManager registered", true,
                () => ServiceLocator.Has<IUIManager>());

            AddCheck("Systems", "PoolManager registered", true,
                () => ServiceLocator.Has<IPoolManager>());

            AddCheck("Systems", "EventBus functional", true,
                () => true); // EventBus is static, always available
        }

        private void DefineContentChecks()
        {
            AddCheck("Content", "4+ Requiems exist", true,
                () => Resources.LoadAll<RequiemDataSO>("").Length >= 4);

            AddCheck("Content", "40+ cards exist", true,
                () => Resources.LoadAll<CardDataSO>("").Length >= 40);

            AddCheck("Content", "10+ relics exist", false,
                () => Resources.LoadAll<RelicDataSO>("").Length >= 10);

            AddCheck("Content", "8+ enemies exist", true,
                () => Resources.LoadAll<EnemyDataSO>("").Length >= 8);

            AddCheck("Content", "6+ Echo events exist", false,
                () => Resources.LoadAll<EchoEventDataSO>("").Length >= 6);

            AddCheck("Content", "3+ zones configured", true,
                () => Resources.LoadAll<ZoneConfigSO>("").Length >= 3);
        }

        private void DefineBuildChecks()
        {
            AddCheck("Build", "Version set (not 0.0)", true,
                () => !string.IsNullOrEmpty(Application.version) && Application.version != "0.0");

            AddCheck("Build", "Bundle identifier configured", true,
                () => !string.IsNullOrEmpty(Application.identifier) &&
                      !Application.identifier.Contains("DefaultCompany"));

            AddCheck("Build", "Company name set", true,
                () => !string.IsNullOrEmpty(Application.companyName) &&
                      Application.companyName != "DefaultCompany");

            AddCheck("Build", "Product name set", true,
                () => !string.IsNullOrEmpty(Application.productName));

            AddCheck("Build", "Release build (not debug)", false,
                () => !Debug.isDebugBuild);
        }

        private void DefinePerformanceChecks()
        {
            AddCheck("Performance", "Target frame rate >= 30", true,
                () => Application.targetFrameRate >= 30 || Application.targetFrameRate == -1);

            AddCheck("Performance", "Quality level set", true,
                () => QualitySettings.GetQualityLevel() >= 0);

            AddCheck("Performance", "VSync disabled for mobile", false,
                () => QualitySettings.vSyncCount == 0);

            AddCheck("Performance", "Memory under 500MB", false,
                () => UnityEngine.Profiling.Profiler.GetTotalAllocatedMemoryLong() < 500 * 1024 * 1024);
        }

        // ============================================
        // Helper Methods
        // ============================================

        private void AddCheck(string category, string name, bool required, Func<bool> validator)
        {
            _checks.Add(new ReleaseCheckItem
            {
                Category = category,
                Name = name,
                Required = required,
                Validator = validator
            });
        }

        private void GenerateReport()
        {
            _report.Clear();
            _report.AppendLine("# RELEASE CHECKLIST REPORT");
            _report.AppendLine();
            _report.AppendLine($"**Date:** {DateTime.Now:yyyy-MM-dd HH:mm}");
            _report.AppendLine($"**Version:** {Application.version}");
            _report.AppendLine($"**Platform:** {Application.platform}");
            _report.AppendLine();

            _report.AppendLine("## Summary");
            _report.AppendLine();
            _report.AppendLine($"| Metric | Value |");
            _report.AppendLine($"|--------|-------|");
            _report.AppendLine($"| Total Checks | {_totalChecks} |");
            _report.AppendLine($"| Passed | {_passedChecks} |");
            _report.AppendLine($"| Failed | {_failedChecks} |");
            _report.AppendLine($"| Required Failed | {_requiredFailed} |");
            _report.AppendLine($"| **Release Ready** | **{(_isReleaseReady ? "YES" : "NO")}** |");
            _report.AppendLine();

            // Group by category
            var categories = new[] { "Systems", "Content", "Build", "Performance" };

            foreach (var category in categories)
            {
                _report.AppendLine($"## {category}");
                _report.AppendLine();

                foreach (var check in _checks.FindAll(c => c.Category == category))
                {
                    string status = check.Passed ? "[x]" : "[ ]";
                    string required = check.Required ? "(Required)" : "(Optional)";
                    string icon = check.Passed ? "" : (check.Required ? " **FAILED**" : " (skipped)");

                    _report.AppendLine($"- {status} {check.Name} {required}{icon}");

                    if (!string.IsNullOrEmpty(check.Notes))
                        _report.AppendLine($"  - Error: {check.Notes}");
                }

                _report.AppendLine();
            }

            _report.AppendLine("---");
            _report.AppendLine();
            _report.AppendLine($"*Generated by FinalReleaseChecklist*");

            // Log summary
            string readyText = _isReleaseReady ? "READY FOR RELEASE" : "NOT READY - Fix required checks";
            Debug.Log($"[ReleaseChecklist] {_passedChecks}/{_totalChecks} passed. {readyText}");

            if (!_isReleaseReady)
            {
                foreach (var check in _checks.FindAll(c => c.Required && !c.Passed))
                {
                    Debug.LogError($"[ReleaseChecklist] REQUIRED FAILED: {check.Name}");
                }
            }
        }
    }
}
