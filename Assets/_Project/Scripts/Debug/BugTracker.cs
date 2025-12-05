// ============================================
// BugTracker.cs
// Bug reporting system with screenshot capture and export
// ============================================

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace HNR.Diagnostics
{
    /// <summary>
    /// Bug severity levels for triage prioritization.
    /// </summary>
    public enum BugSeverity
    {
        Critical,   // Game-breaking, crashes, data loss
        High,       // Major feature broken, no workaround
        Medium,     // Feature impaired, workaround exists
        Low         // Minor issue, cosmetic
    }

    /// <summary>
    /// Bug category for classification.
    /// </summary>
    public enum BugCategory
    {
        Gameplay,
        UI,
        Performance,
        Audio,
        Visual,
        Crash,
        Other
    }

    /// <summary>
    /// Individual bug report data.
    /// </summary>
    [Serializable]
    public class BugReport
    {
        public string Id;
        public BugSeverity Severity;
        public BugCategory Category;
        public string Description;
        public string StepsToReproduce;
        public string ScreenshotPath;
        public string Timestamp;
        public string BuildVersion;
        public string DeviceInfo;
        public bool IsResolved;
        public string Resolution;
    }

    /// <summary>
    /// Container for serializing bug report list.
    /// </summary>
    [Serializable]
    public class BugReportList
    {
        public List<BugReport> Reports = new();
    }

    /// <summary>
    /// Bug tracking system for QA and testing.
    /// Captures screenshots, logs device info, and exports reports.
    /// </summary>
    /// <remarks>
    /// Press [B] to toggle bug reporter UI.
    /// Auto-captures screenshot when UI opens.
    /// Exports to JSON or Markdown for review.
    /// </remarks>
    public class BugTracker : MonoBehaviour
    {
        // ============================================
        // Configuration
        // ============================================

        [Header("Settings")]
        [SerializeField, Tooltip("Key to toggle bug reporter UI")]
        private KeyCode _toggleKey = KeyCode.B;

        [SerializeField, Tooltip("Show the bug reporter UI")]
        private bool _showReporterUI;

        [Header("Current Session Stats")]
        [SerializeField] private int _bugsReported;
        [SerializeField] private int _criticalBugs;

        // ============================================
        // Private State
        // ============================================

        private List<BugReport> _reports = new();
        private string _pendingScreenshotPath;
        private string _currentDescription = "";
        private string _currentSteps = "";
        private BugSeverity _currentSeverity = BugSeverity.Medium;
        private BugCategory _currentCategory = BugCategory.Gameplay;

        private const string SAVE_FILENAME = "BugReports.json";
        private const string SCREENSHOT_FOLDER = "Screenshots";

        // ============================================
        // Public Properties
        // ============================================

        /// <summary>All bug reports.</summary>
        public IReadOnlyList<BugReport> Reports => _reports.AsReadOnly();

        /// <summary>Total number of bugs reported.</summary>
        public int TotalBugs => _reports.Count;

        /// <summary>Number of unresolved bugs.</summary>
        public int UnresolvedBugs => _reports.FindAll(b => !b.IsResolved).Count;

        /// <summary>Number of critical bugs.</summary>
        public int CriticalBugs => _reports.FindAll(b => b.Severity == BugSeverity.Critical && !b.IsResolved).Count;

        // ============================================
        // Unity Lifecycle
        // ============================================

        private void Awake()
        {
            LoadReports();
        }

        private void Update()
        {
            if (Input.GetKeyDown(_toggleKey))
            {
                ToggleReporter();
            }
        }

        // ============================================
        // Public Methods
        // ============================================

        /// <summary>
        /// Toggle bug reporter UI visibility.
        /// </summary>
        public void ToggleReporter()
        {
            _showReporterUI = !_showReporterUI;

            if (_showReporterUI)
            {
                CaptureScreenshot();
            }
        }

        /// <summary>
        /// Report a bug programmatically.
        /// </summary>
        /// <param name="severity">Bug severity level.</param>
        /// <param name="category">Bug category.</param>
        /// <param name="description">Bug description.</param>
        /// <param name="steps">Steps to reproduce (optional).</param>
        /// <returns>The created bug report.</returns>
        public BugReport ReportBug(BugSeverity severity, BugCategory category, string description, string steps = "")
        {
            var report = new BugReport
            {
                Id = GenerateId(),
                Severity = severity,
                Category = category,
                Description = description,
                StepsToReproduce = steps,
                ScreenshotPath = _pendingScreenshotPath,
                Timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                BuildVersion = Application.version,
                DeviceInfo = GetDeviceInfo(),
                IsResolved = false
            };

            _reports.Add(report);
            _bugsReported++;

            if (severity == BugSeverity.Critical)
            {
                _criticalBugs++;
            }

            SaveReports();

            Debug.Log($"[BugTracker] Bug #{report.Id} reported: [{severity}] {description}");
            _pendingScreenshotPath = null;

            return report;
        }

        /// <summary>
        /// Mark a bug as resolved.
        /// </summary>
        /// <param name="bugId">Bug ID to resolve.</param>
        /// <param name="resolution">Resolution description.</param>
        public void ResolveBug(string bugId, string resolution = "")
        {
            var bug = _reports.Find(b => b.Id == bugId);
            if (bug != null)
            {
                bug.IsResolved = true;
                bug.Resolution = resolution;
                SaveReports();
                Debug.Log($"[BugTracker] Bug #{bugId} resolved: {resolution}");
            }
            else
            {
                Debug.LogWarning($"[BugTracker] Bug #{bugId} not found");
            }
        }

        /// <summary>
        /// Capture screenshot for bug report attachment.
        /// </summary>
        public void CaptureScreenshot()
        {
            string folder = Path.Combine(Application.persistentDataPath, SCREENSHOT_FOLDER);
            Directory.CreateDirectory(folder);

            string filename = $"Bug_{DateTime.Now:yyyyMMdd_HHmmss}.png";
            string fullPath = Path.Combine(folder, filename);

            ScreenCapture.CaptureScreenshot(fullPath);
            _pendingScreenshotPath = fullPath;

            Debug.Log($"[BugTracker] Screenshot captured: {filename}");
        }

        /// <summary>
        /// Export all bugs to JSON file.
        /// </summary>
        /// <returns>Path to exported file.</returns>
        public string ExportToJson()
        {
            string path = Path.Combine(Application.persistentDataPath, "BugExport.json");
            var list = new BugReportList { Reports = _reports };
            string json = JsonUtility.ToJson(list, true);
            File.WriteAllText(path, json);

            Debug.Log($"[BugTracker] Exported {_reports.Count} bugs to {path}");
            return path;
        }

        /// <summary>
        /// Export bugs to Markdown for documentation.
        /// </summary>
        /// <returns>Path to exported file.</returns>
        public string ExportToMarkdown()
        {
            var sb = new StringBuilder();
            sb.AppendLine("# Bug Report Summary");
            sb.AppendLine();
            sb.AppendLine($"**Generated:** {DateTime.Now:yyyy-MM-dd HH:mm}");
            sb.AppendLine($"**Build:** {Application.version}");
            sb.AppendLine($"**Total Bugs:** {_reports.Count}");
            sb.AppendLine($"**Unresolved:** {UnresolvedBugs}");
            sb.AppendLine($"**Critical:** {CriticalBugs}");
            sb.AppendLine();

            // Group by severity
            foreach (BugSeverity severity in Enum.GetValues(typeof(BugSeverity)))
            {
                var bugs = _reports.FindAll(b => b.Severity == severity);
                if (bugs.Count == 0) continue;

                sb.AppendLine($"## {severity} ({bugs.Count})");
                sb.AppendLine();

                foreach (var bug in bugs)
                {
                    string status = bug.IsResolved ? "[x]" : "[ ]";
                    sb.AppendLine($"- {status} **{bug.Id}** ({bug.Category}): {bug.Description}");

                    if (!string.IsNullOrEmpty(bug.StepsToReproduce))
                    {
                        sb.AppendLine($"  - Steps: {bug.StepsToReproduce}");
                    }

                    if (!string.IsNullOrEmpty(bug.ScreenshotPath))
                    {
                        sb.AppendLine($"  - Screenshot: {Path.GetFileName(bug.ScreenshotPath)}");
                    }

                    sb.AppendLine($"  - Reported: {bug.Timestamp}");
                    sb.AppendLine($"  - Device: {bug.DeviceInfo}");

                    if (bug.IsResolved && !string.IsNullOrEmpty(bug.Resolution))
                    {
                        sb.AppendLine($"  - Resolution: {bug.Resolution}");
                    }
                }

                sb.AppendLine();
            }

            string path = Path.Combine(Application.persistentDataPath, "BugReport.md");
            File.WriteAllText(path, sb.ToString());

            Debug.Log($"[BugTracker] Exported markdown to {path}");
            return path;
        }

        /// <summary>
        /// Get summary statistics.
        /// </summary>
        public string GetSummary()
        {
            var sb = new StringBuilder();
            sb.AppendLine("=== BUG TRACKER SUMMARY ===");
            sb.AppendLine($"Total: {TotalBugs}");
            sb.AppendLine($"Unresolved: {UnresolvedBugs}");
            sb.AppendLine($"Critical: {CriticalBugs}");

            foreach (BugSeverity severity in Enum.GetValues(typeof(BugSeverity)))
            {
                int count = _reports.FindAll(b => b.Severity == severity && !b.IsResolved).Count;
                if (count > 0)
                {
                    sb.AppendLine($"  {severity}: {count}");
                }
            }

            return sb.ToString();
        }

        /// <summary>
        /// Clear all resolved bugs from the list.
        /// </summary>
        public void ClearResolved()
        {
            int removed = _reports.RemoveAll(b => b.IsResolved);
            SaveReports();
            Debug.Log($"[BugTracker] Cleared {removed} resolved bugs");
        }

        /// <summary>
        /// Clear all bugs (use with caution).
        /// </summary>
        public void ClearAll()
        {
            _reports.Clear();
            SaveReports();
            Debug.Log("[BugTracker] All bugs cleared");
        }

        // ============================================
        // Private Methods
        // ============================================

        private void SaveReports()
        {
            string path = Path.Combine(Application.persistentDataPath, SAVE_FILENAME);
            var list = new BugReportList { Reports = _reports };
            string json = JsonUtility.ToJson(list, true);
            File.WriteAllText(path, json);
        }

        private void LoadReports()
        {
            string path = Path.Combine(Application.persistentDataPath, SAVE_FILENAME);
            if (File.Exists(path))
            {
                try
                {
                    string json = File.ReadAllText(path);
                    var list = JsonUtility.FromJson<BugReportList>(json);
                    _reports = list?.Reports ?? new List<BugReport>();
                    Debug.Log($"[BugTracker] Loaded {_reports.Count} existing bug reports");
                }
                catch (Exception e)
                {
                    Debug.LogError($"[BugTracker] Failed to load reports: {e.Message}");
                    _reports = new List<BugReport>();
                }
            }
        }

        private string GenerateId()
        {
            return Guid.NewGuid().ToString().Substring(0, 8).ToUpper();
        }

        private string GetDeviceInfo()
        {
            return $"{SystemInfo.deviceModel} | " +
                   $"{SystemInfo.operatingSystem} | " +
                   $"RAM: {SystemInfo.systemMemorySize}MB | " +
                   $"GPU: {SystemInfo.graphicsDeviceName}";
        }

        // ============================================
        // OnGUI - Debug UI
        // ============================================

        private void OnGUI()
        {
            if (!_showReporterUI) return;

            float width = 450;
            float height = 420;
            float x = (Screen.width - width) / 2;
            float y = (Screen.height - height) / 2;

            GUI.Box(new Rect(x, y, width, height), "Bug Reporter [B]");

            float yOffset = y + 25;
            float padding = 10;

            // Description label
            GUI.Label(new Rect(x + padding, yOffset, 100, 20), "Description:");
            yOffset += 20;

            // Description text area
            _currentDescription = GUI.TextArea(
                new Rect(x + padding, yOffset, width - padding * 2, 60),
                _currentDescription);
            yOffset += 70;

            // Steps to reproduce
            GUI.Label(new Rect(x + padding, yOffset, 120, 20), "Steps to Reproduce:");
            yOffset += 20;

            _currentSteps = GUI.TextArea(
                new Rect(x + padding, yOffset, width - padding * 2, 50),
                _currentSteps);
            yOffset += 60;

            // Severity
            GUI.Label(new Rect(x + padding, yOffset, 80, 25), "Severity:");
            if (GUI.Button(new Rect(x + 100, yOffset, 100, 25), _currentSeverity.ToString()))
            {
                _currentSeverity = (BugSeverity)(((int)_currentSeverity + 1) % 4);
            }
            yOffset += 30;

            // Category
            GUI.Label(new Rect(x + padding, yOffset, 80, 25), "Category:");
            if (GUI.Button(new Rect(x + 100, yOffset, 100, 25), _currentCategory.ToString()))
            {
                _currentCategory = (BugCategory)(((int)_currentCategory + 1) % 7);
            }
            yOffset += 35;

            // Screenshot status
            string screenshotStatus = _pendingScreenshotPath != null
                ? $"Screenshot: {Path.GetFileName(_pendingScreenshotPath)}"
                : "Screenshot: (none)";
            GUI.Label(new Rect(x + padding, yOffset, width - padding * 2, 20), screenshotStatus);
            yOffset += 25;

            // Buttons row 1
            if (GUI.Button(new Rect(x + padding, yOffset, 130, 30), "Submit Bug"))
            {
                if (!string.IsNullOrEmpty(_currentDescription))
                {
                    ReportBug(_currentSeverity, _currentCategory, _currentDescription, _currentSteps);
                    _currentDescription = "";
                    _currentSteps = "";
                    _showReporterUI = false;
                }
            }

            if (GUI.Button(new Rect(x + 150, yOffset, 100, 30), "New Screenshot"))
            {
                CaptureScreenshot();
            }

            if (GUI.Button(new Rect(x + 260, yOffset, 80, 30), "Cancel"))
            {
                _showReporterUI = false;
                _pendingScreenshotPath = null;
            }
            yOffset += 40;

            // Stats
            GUI.Label(new Rect(x + padding, yOffset, width - padding * 2, 20),
                $"Total: {TotalBugs} | Unresolved: {UnresolvedBugs} | Critical: {CriticalBugs}");
            yOffset += 25;

            // Export buttons
            if (GUI.Button(new Rect(x + padding, yOffset, 100, 25), "Export JSON"))
            {
                ExportToJson();
            }

            if (GUI.Button(new Rect(x + 120, yOffset, 100, 25), "Export MD"))
            {
                ExportToMarkdown();
            }

            if (GUI.Button(new Rect(x + 230, yOffset, 100, 25), "Log Summary"))
            {
                Debug.Log(GetSummary());
            }
        }
    }
}
