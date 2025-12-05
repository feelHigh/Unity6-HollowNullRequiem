// ============================================
// QAChecklistSO.cs
// ScriptableObject for QA progress tracking
// ============================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace HNR.Testing
{
    /// <summary>
    /// Individual QA checklist item with completion tracking.
    /// </summary>
    [Serializable]
    public class QAChecklistItem
    {
        public string Name;
        public bool Completed;
        public string TestedBy;
        public string TestDate;
        [TextArea(1, 3)] public string Notes;
    }

    /// <summary>
    /// Category of related QA checklist items.
    /// </summary>
    [Serializable]
    public class QAChecklistCategory
    {
        public string CategoryName;
        public List<QAChecklistItem> Items = new();

        /// <summary>Number of completed items in this category.</summary>
        public int CompletedCount => Items.Count(i => i.Completed);

        /// <summary>Total number of items in this category.</summary>
        public int TotalCount => Items.Count;

        /// <summary>Completion percentage (0-100).</summary>
        public float CompletionPercent => TotalCount > 0 ? (float)CompletedCount / TotalCount * 100f : 0f;

        /// <summary>True if all items are completed.</summary>
        public bool IsComplete => CompletedCount == TotalCount && TotalCount > 0;
    }

    /// <summary>
    /// QA Checklist ScriptableObject for tracking testing progress.
    /// Use the context menu "Initialize Default Checklist" to populate with HNR items.
    /// </summary>
    [CreateAssetMenu(fileName = "QAChecklist", menuName = "HNR/Testing/QA Checklist")]
    public class QAChecklistSO : ScriptableObject
    {
        // ============================================
        // Serialized Fields
        // ============================================

        [Header("Metadata")]
        [SerializeField, Tooltip("Project version being tested")]
        private string _projectVersion;

        [SerializeField, Tooltip("Last update timestamp")]
        private string _lastUpdated;

        [Header("Categories")]
        [SerializeField]
        private List<QAChecklistCategory> _categories = new();

        // ============================================
        // Public Properties
        // ============================================

        /// <summary>All checklist categories.</summary>
        public IReadOnlyList<QAChecklistCategory> Categories => _categories.AsReadOnly();

        /// <summary>Project version being tested.</summary>
        public string ProjectVersion => _projectVersion;

        /// <summary>Last update timestamp.</summary>
        public string LastUpdated => _lastUpdated;

        /// <summary>Overall completion percentage across all categories.</summary>
        public float OverallProgress
        {
            get
            {
                int total = _categories.Sum(c => c.TotalCount);
                int completed = _categories.Sum(c => c.CompletedCount);
                return total > 0 ? (float)completed / total * 100f : 0f;
            }
        }

        /// <summary>True if all items in all categories are complete.</summary>
        public bool IsFullyComplete => _categories.Count > 0 && _categories.All(c => c.IsComplete);

        /// <summary>Total number of items across all categories.</summary>
        public int TotalItems => _categories.Sum(c => c.TotalCount);

        /// <summary>Total completed items across all categories.</summary>
        public int CompletedItems => _categories.Sum(c => c.CompletedCount);

        // ============================================
        // Public Methods
        // ============================================

        /// <summary>
        /// Mark an item as complete.
        /// </summary>
        /// <param name="category">Category name.</param>
        /// <param name="item">Item name.</param>
        /// <param name="tester">Tester name (optional).</param>
        public void MarkComplete(string category, string item, string tester = "")
        {
            var cat = _categories.Find(c => c.CategoryName == category);
            var itm = cat?.Items.Find(i => i.Name == item);
            if (itm != null)
            {
                itm.Completed = true;
                itm.TestedBy = tester;
                itm.TestDate = DateTime.Now.ToString("yyyy-MM-dd");
                _lastUpdated = DateTime.Now.ToString("yyyy-MM-dd HH:mm");
                MarkDirty();
            }
        }

        /// <summary>
        /// Mark an item as incomplete.
        /// </summary>
        /// <param name="category">Category name.</param>
        /// <param name="item">Item name.</param>
        public void MarkIncomplete(string category, string item)
        {
            var cat = _categories.Find(c => c.CategoryName == category);
            var itm = cat?.Items.Find(i => i.Name == item);
            if (itm != null)
            {
                itm.Completed = false;
                _lastUpdated = DateTime.Now.ToString("yyyy-MM-dd HH:mm");
                MarkDirty();
            }
        }

        /// <summary>
        /// Add a note to an item.
        /// </summary>
        /// <param name="category">Category name.</param>
        /// <param name="item">Item name.</param>
        /// <param name="note">Note text.</param>
        public void AddNote(string category, string item, string note)
        {
            var cat = _categories.Find(c => c.CategoryName == category);
            var itm = cat?.Items.Find(i => i.Name == item);
            if (itm != null)
            {
                itm.Notes = note;
                MarkDirty();
            }
        }

        /// <summary>
        /// Reset all items to incomplete.
        /// </summary>
        public void ResetAll()
        {
            foreach (var category in _categories)
            {
                foreach (var item in category.Items)
                {
                    item.Completed = false;
                    item.TestedBy = "";
                    item.TestDate = "";
                    item.Notes = "";
                }
            }
            _lastUpdated = DateTime.Now.ToString("yyyy-MM-dd HH:mm");
            MarkDirty();
        }

        /// <summary>
        /// Export checklist to Markdown format.
        /// </summary>
        /// <returns>Markdown string.</returns>
        public string ExportToMarkdown()
        {
            var sb = new StringBuilder();
            sb.AppendLine("# QA Checklist Report");
            sb.AppendLine();
            sb.AppendLine($"**Project Version:** {_projectVersion}");
            sb.AppendLine($"**Last Updated:** {_lastUpdated}");
            sb.AppendLine($"**Overall Progress:** {OverallProgress:F0}% ({CompletedItems}/{TotalItems})");
            sb.AppendLine($"**Status:** {(IsFullyComplete ? "COMPLETE" : "IN PROGRESS")}");
            sb.AppendLine();

            foreach (var category in _categories)
            {
                string catStatus = category.IsComplete ? "[DONE]" : "[...]";
                sb.AppendLine($"## {catStatus} {category.CategoryName} ({category.CompletionPercent:F0}%)");
                sb.AppendLine();

                foreach (var item in category.Items)
                {
                    string check = item.Completed ? "x" : " ";
                    sb.AppendLine($"- [{check}] {item.Name}");

                    if (item.Completed && !string.IsNullOrEmpty(item.TestedBy))
                    {
                        sb.AppendLine($"  - Tested by: {item.TestedBy} on {item.TestDate}");
                    }

                    if (!string.IsNullOrEmpty(item.Notes))
                    {
                        sb.AppendLine($"  - Note: {item.Notes}");
                    }
                }
                sb.AppendLine();
            }

            return sb.ToString();
        }

        /// <summary>
        /// Get a brief summary string.
        /// </summary>
        /// <returns>Summary text.</returns>
        public string GetSummary()
        {
            return $"QA Progress: {CompletedItems}/{TotalItems} ({OverallProgress:F0}%)";
        }

        /// <summary>
        /// Get incomplete items grouped by category.
        /// </summary>
        /// <returns>Dictionary of category name to incomplete item names.</returns>
        public Dictionary<string, List<string>> GetIncompleteItems()
        {
            var result = new Dictionary<string, List<string>>();

            foreach (var category in _categories)
            {
                var incomplete = category.Items
                    .Where(i => !i.Completed)
                    .Select(i => i.Name)
                    .ToList();

                if (incomplete.Count > 0)
                {
                    result[category.CategoryName] = incomplete;
                }
            }

            return result;
        }

        /// <summary>
        /// Log current progress to console.
        /// </summary>
        public void LogProgress()
        {
            Debug.Log($"[QAChecklist] {GetSummary()}");

            foreach (var category in _categories)
            {
                string status = category.IsComplete ? "DONE" : $"{category.CompletionPercent:F0}%";
                Debug.Log($"  {category.CategoryName}: {category.CompletedCount}/{category.TotalCount} ({status})");
            }
        }

        // ============================================
        // Context Menu Actions
        // ============================================

        /// <summary>
        /// Initialize with default HNR QA checklist items.
        /// </summary>
        [ContextMenu("Initialize Default Checklist")]
        public void InitializeDefaultChecklist()
        {
            _categories.Clear();
            _projectVersion = Application.version;

            // Core Gameplay
            var coreGameplay = new QAChecklistCategory { CategoryName = "Core Gameplay" };
            coreGameplay.Items.Add(new QAChecklistItem { Name = "New run starts correctly" });
            coreGameplay.Items.Add(new QAChecklistItem { Name = "Team selection (3 Requiems)" });
            coreGameplay.Items.Add(new QAChecklistItem { Name = "Combat initializes with correct HP/AP" });
            coreGameplay.Items.Add(new QAChecklistItem { Name = "Cards draw correctly (5 per turn)" });
            coreGameplay.Items.Add(new QAChecklistItem { Name = "Cards play and resolve effects" });
            coreGameplay.Items.Add(new QAChecklistItem { Name = "Damage calculation accurate" });
            coreGameplay.Items.Add(new QAChecklistItem { Name = "Status effects apply and tick" });
            coreGameplay.Items.Add(new QAChecklistItem { Name = "Corruption accumulates correctly" });
            coreGameplay.Items.Add(new QAChecklistItem { Name = "Null State triggers at 100" });
            coreGameplay.Items.Add(new QAChecklistItem { Name = "Victory when all enemies dead" });
            coreGameplay.Items.Add(new QAChecklistItem { Name = "Defeat at 0 HP" });
            _categories.Add(coreGameplay);

            // Progression
            var progression = new QAChecklistCategory { CategoryName = "Progression" };
            progression.Items.Add(new QAChecklistItem { Name = "Map generates correctly" });
            progression.Items.Add(new QAChecklistItem { Name = "All node types appear" });
            progression.Items.Add(new QAChecklistItem { Name = "Navigation between nodes" });
            progression.Items.Add(new QAChecklistItem { Name = "Shop displays items" });
            progression.Items.Add(new QAChecklistItem { Name = "Shop purchases work" });
            progression.Items.Add(new QAChecklistItem { Name = "Void Shards economy works" });
            progression.Items.Add(new QAChecklistItem { Name = "Echo events display" });
            progression.Items.Add(new QAChecklistItem { Name = "Echo choices resolve" });
            progression.Items.Add(new QAChecklistItem { Name = "Relics apply effects" });
            _categories.Add(progression);

            // Save System
            var saveSystem = new QAChecklistCategory { CategoryName = "Save System" };
            saveSystem.Items.Add(new QAChecklistItem { Name = "Run saves automatically" });
            saveSystem.Items.Add(new QAChecklistItem { Name = "Run loads on continue" });
            saveSystem.Items.Add(new QAChecklistItem { Name = "All run state preserved" });
            saveSystem.Items.Add(new QAChecklistItem { Name = "Settings save correctly" });
            saveSystem.Items.Add(new QAChecklistItem { Name = "Delete save works" });
            _categories.Add(saveSystem);

            // Edge Cases
            var edgeCases = new QAChecklistCategory { CategoryName = "Edge Cases" };
            edgeCases.Items.Add(new QAChecklistItem { Name = "0 HP triggers defeat" });
            edgeCases.Items.Add(new QAChecklistItem { Name = "100 corruption triggers Null State" });
            edgeCases.Items.Add(new QAChecklistItem { Name = "Empty draw pile reshuffles" });
            edgeCases.Items.Add(new QAChecklistItem { Name = "Max hand size respected" });
            edgeCases.Items.Add(new QAChecklistItem { Name = "0 AP prevents card play" });
            edgeCases.Items.Add(new QAChecklistItem { Name = "All Requiems in Null State" });
            _categories.Add(edgeCases);

            // Performance
            var performance = new QAChecklistCategory { CategoryName = "Performance" };
            performance.Items.Add(new QAChecklistItem { Name = "Stable 60fps on high-end" });
            performance.Items.Add(new QAChecklistItem { Name = "Stable 30fps on low-end" });
            performance.Items.Add(new QAChecklistItem { Name = "No crashes during play" });
            performance.Items.Add(new QAChecklistItem { Name = "Memory stable (no leaks)" });
            performance.Items.Add(new QAChecklistItem { Name = "Load times < 3 seconds" });
            performance.Items.Add(new QAChecklistItem { Name = "Battery usage acceptable" });
            _categories.Add(performance);

            // UI/UX
            var uiux = new QAChecklistCategory { CategoryName = "UI/UX" };
            uiux.Items.Add(new QAChecklistItem { Name = "All screens display correctly" });
            uiux.Items.Add(new QAChecklistItem { Name = "Buttons respond to touch" });
            uiux.Items.Add(new QAChecklistItem { Name = "Touch targets >= 44dp" });
            uiux.Items.Add(new QAChecklistItem { Name = "Safe areas respected" });
            uiux.Items.Add(new QAChecklistItem { Name = "Landscape orientation locked" });
            uiux.Items.Add(new QAChecklistItem { Name = "Text readable on all devices" });
            uiux.Items.Add(new QAChecklistItem { Name = "Transitions smooth" });
            _categories.Add(uiux);

            _lastUpdated = DateTime.Now.ToString("yyyy-MM-dd HH:mm");

            MarkDirty();
            Debug.Log($"[QAChecklist] Initialized with {TotalItems} items across {_categories.Count} categories");
        }

        /// <summary>
        /// Export to markdown and log path.
        /// </summary>
        [ContextMenu("Export to Markdown")]
        public void ExportAndLog()
        {
            string markdown = ExportToMarkdown();
            string path = System.IO.Path.Combine(Application.persistentDataPath, "QAChecklist.md");
            System.IO.File.WriteAllText(path, markdown);
            Debug.Log($"[QAChecklist] Exported to {path}");
        }

        /// <summary>
        /// Log current progress.
        /// </summary>
        [ContextMenu("Log Progress")]
        public void LogProgressMenu()
        {
            LogProgress();
        }

        // ============================================
        // Private Methods
        // ============================================

        private void MarkDirty()
        {
#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
#endif
        }
    }
}
