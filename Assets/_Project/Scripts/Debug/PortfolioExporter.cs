// ============================================
// PortfolioExporter.cs
// Export clean project package for portfolio
// ============================================

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace HNR.Diagnostics
{
    /// <summary>
    /// Exports a clean portfolio package with scripts, docs, and code samples.
    /// </summary>
    public class PortfolioExporter : MonoBehaviour
    {
        // ============================================
        // Serialized Fields
        // ============================================

        [Header("Export Settings")]
        [SerializeField] private string _exportFolderName = "HNR_Portfolio";

        [Header("Include")]
        [SerializeField] private bool _includeScripts = true;
        [SerializeField] private bool _includeDocumentation = true;
        [SerializeField] private bool _includeScreenshots = true;

        [Header("Code Samples")]
        [SerializeField] private List<string> _highlightedScripts = new()
        {
            "Core/GameManager.cs",
            "Core/ServiceLocator.cs",
            "Core/Events/EventBus.cs",
            "Combat/TurnManager.cs",
            "Cards/Effects/CardExecutor.cs",
            "Characters/CorruptionManager.cs",
            "Map/MapGenerator.cs",
            "VFX/VFXPoolManager.cs"
        };

        // ============================================
        // Public Methods
        // ============================================

        /// <summary>
        /// Export portfolio package.
        /// </summary>
        [ContextMenu("Export Portfolio")]
        public void ExportPortfolio()
        {
            string exportPath = Path.Combine(Application.dataPath, "..", _exportFolderName);

            // Create export directory
            if (Directory.Exists(exportPath))
                Directory.Delete(exportPath, true);
            Directory.CreateDirectory(exportPath);

            Debug.Log($"[PortfolioExporter] Exporting to {exportPath}");

            // Generate components
            if (_includeScripts)
                ExportScripts(exportPath);

            if (_includeDocumentation)
                ExportDocumentation(exportPath);

            if (_includeScreenshots)
                ExportScreenshots(exportPath);

            ExportCodeSamples(exportPath);
            GenerateProjectOverview(exportPath);
            CopyReadme(exportPath);

            Debug.Log("[PortfolioExporter] Export complete!");

#if UNITY_EDITOR
            UnityEditor.EditorUtility.RevealInFinder(exportPath);
#endif
        }

        /// <summary>
        /// Get export path.
        /// </summary>
        public string GetExportPath()
        {
            return Path.Combine(Application.dataPath, "..", _exportFolderName);
        }

        // ============================================
        // Export Methods
        // ============================================

        private void ExportScripts(string exportPath)
        {
            string scriptsPath = Path.Combine(exportPath, "Scripts");
            Directory.CreateDirectory(scriptsPath);

            string sourcePath = Path.Combine(Application.dataPath, "_Project", "Scripts");

            // Copy scripts preserving structure
            CopyDirectory(sourcePath, scriptsPath, "*.cs");

            Debug.Log("[PortfolioExporter] Exported scripts");
        }

        private void ExportDocumentation(string exportPath)
        {
            string docsPath = Path.Combine(exportPath, "Docs");
            Directory.CreateDirectory(docsPath);

            string projectRoot = Path.Combine(Application.dataPath, "..");

            // Copy GDD
            string gddSource = Path.Combine(projectRoot, "Docs", "HNR_GameDesignDocument.md");
            if (File.Exists(gddSource))
                File.Copy(gddSource, Path.Combine(docsPath, "HNR_GameDesignDocument.md"));

            // Copy TDDs
            string tddPath = Path.Combine(docsPath, "TDD");
            Directory.CreateDirectory(tddPath);

            string tddSource = Path.Combine(projectRoot, "Docs", "TDD");
            if (Directory.Exists(tddSource))
                CopyDirectory(tddSource, tddPath, "*.md");

            Debug.Log("[PortfolioExporter] Exported documentation");
        }

        private void ExportScreenshots(string exportPath)
        {
            string screenshotsPath = Path.Combine(exportPath, "Screenshots");
            Directory.CreateDirectory(screenshotsPath);

            string sourcePath = Path.Combine(Application.dataPath, "..", "Docs", "Screenshots");
            if (Directory.Exists(sourcePath))
            {
                CopyDirectory(sourcePath, screenshotsPath, "*.png");
                CopyDirectory(sourcePath, screenshotsPath, "*.jpg");
            }

            Debug.Log("[PortfolioExporter] Exported screenshots");
        }

        private void ExportCodeSamples(string exportPath)
        {
            string samplesPath = Path.Combine(exportPath, "CodeSamples");
            Directory.CreateDirectory(samplesPath);

            var sb = new StringBuilder();
            sb.AppendLine("# Code Samples");
            sb.AppendLine();
            sb.AppendLine("Highlighted code demonstrating key systems and patterns.");
            sb.AppendLine();

            sb.AppendLine("## Systems Overview");
            sb.AppendLine();
            sb.AppendLine("| File | Description |");
            sb.AppendLine("|------|-------------|");

            foreach (var scriptPath in _highlightedScripts)
            {
                string fullPath = Path.Combine(Application.dataPath, "_Project", "Scripts", scriptPath);
                if (File.Exists(fullPath))
                {
                    string fileName = Path.GetFileName(scriptPath);
                    string destPath = Path.Combine(samplesPath, fileName);
                    File.Copy(fullPath, destPath);

                    string description = GetScriptDescription(fileName);
                    sb.AppendLine($"| [{fileName}]({fileName}) | {description} |");
                }
            }

            sb.AppendLine();
            sb.AppendLine("## Architecture Patterns");
            sb.AppendLine();
            sb.AppendLine("- **Service Locator** - Global service access without tight coupling");
            sb.AppendLine("- **Event Bus** - Decoupled pub-sub communication");
            sb.AppendLine("- **State Machine** - Game and combat state management");
            sb.AppendLine("- **Command Pattern** - Card effect execution");
            sb.AppendLine("- **Object Pooling** - Performance optimization");

            // Write index
            File.WriteAllText(Path.Combine(samplesPath, "README.md"), sb.ToString());

            Debug.Log("[PortfolioExporter] Exported code samples");
        }

        private void GenerateProjectOverview(string exportPath)
        {
            var sb = new StringBuilder();

            sb.AppendLine("# Hollow Null Requiem - Portfolio Package");
            sb.AppendLine();
            sb.AppendLine($"**Exported:** {DateTime.Now:yyyy-MM-dd HH:mm}");
            sb.AppendLine($"**Version:** {Application.version}");
            sb.AppendLine($"**Unity:** {Application.unityVersion}");
            sb.AppendLine();

            sb.AppendLine("## Contents");
            sb.AppendLine();
            sb.AppendLine("```");
            sb.AppendLine($"{_exportFolderName}/");
            sb.AppendLine("├── Scripts/           # Full source code");
            sb.AppendLine("├── Docs/              # GDD and TDD documentation");
            sb.AppendLine("├── CodeSamples/       # Highlighted code examples");
            sb.AppendLine("├── Screenshots/       # Game screenshots");
            sb.AppendLine("├── README.md          # Project overview");
            sb.AppendLine("└── ProjectOverview.md # This file");
            sb.AppendLine("```");
            sb.AppendLine();

            sb.AppendLine("## Project Statistics");
            sb.AppendLine();
            sb.AppendLine("| Metric | Value |");
            sb.AppendLine("|--------|-------|");
            sb.AppendLine($"| Lines of Code | ~{CountLinesOfCode():N0} |");
            sb.AppendLine($"| Script Files | {CountScriptFiles()} |");
            sb.AppendLine($"| Documentation Pages | {CountDocFiles()} |");
            sb.AppendLine($"| Development Time | 12 weeks |");
            sb.AppendLine();

            sb.AppendLine("## Key Systems");
            sb.AppendLine();
            sb.AppendLine("| System | Description | Location |");
            sb.AppendLine("|--------|-------------|----------|");
            sb.AppendLine("| Service Locator | Global service access | `Core/ServiceLocator.cs` |");
            sb.AppendLine("| Event Bus | Decoupled communication | `Core/Events/EventBus.cs` |");
            sb.AppendLine("| Game Manager | State machine orchestration | `Core/GameManager.cs` |");
            sb.AppendLine("| Combat System | Turn-based battle | `Combat/TurnManager.cs` |");
            sb.AppendLine("| Card Executor | Card effect resolution | `Cards/Effects/CardExecutor.cs` |");
            sb.AppendLine("| Corruption | Risk-reward mechanic | `Characters/CorruptionManager.cs` |");
            sb.AppendLine("| Map Generator | Procedural generation | `Map/MapGenerator.cs` |");
            sb.AppendLine("| VFX Pool | Particle effect pooling | `VFX/VFXPoolManager.cs` |");
            sb.AppendLine();

            sb.AppendLine("## Technical Highlights");
            sb.AppendLine();
            sb.AppendLine("- Clean architecture with dependency injection via Service Locator");
            sb.AppendLine("- Event-driven design for loose coupling between systems");
            sb.AppendLine("- Data-driven content using ScriptableObjects");
            sb.AppendLine("- Mobile-optimized with 60fps target and object pooling");
            sb.AppendLine("- Comprehensive documentation (GDD + 12 TDD modules)");
            sb.AppendLine();

            File.WriteAllText(Path.Combine(exportPath, "ProjectOverview.md"), sb.ToString());
        }

        private void CopyReadme(string exportPath)
        {
            string readmePath = Path.Combine(Application.dataPath, "..", "README.md");
            if (File.Exists(readmePath))
            {
                File.Copy(readmePath, Path.Combine(exportPath, "README.md"));
            }
        }

        // ============================================
        // Helper Methods
        // ============================================

        private void CopyDirectory(string source, string dest, string pattern)
        {
            if (!Directory.Exists(source)) return;

            foreach (var file in Directory.GetFiles(source, pattern, SearchOption.AllDirectories))
            {
                string relativePath = file.Substring(source.Length + 1);
                string destFile = Path.Combine(dest, relativePath);

                string destDir = Path.GetDirectoryName(destFile);
                if (!string.IsNullOrEmpty(destDir))
                    Directory.CreateDirectory(destDir);

                File.Copy(file, destFile, true);
            }
        }

        private int CountLinesOfCode()
        {
            string scriptsPath = Path.Combine(Application.dataPath, "_Project", "Scripts");
            if (!Directory.Exists(scriptsPath)) return 0;

            int lines = 0;
            foreach (var file in Directory.GetFiles(scriptsPath, "*.cs", SearchOption.AllDirectories))
            {
                lines += File.ReadAllLines(file).Length;
            }
            return lines;
        }

        private int CountScriptFiles()
        {
            string scriptsPath = Path.Combine(Application.dataPath, "_Project", "Scripts");
            if (!Directory.Exists(scriptsPath)) return 0;
            return Directory.GetFiles(scriptsPath, "*.cs", SearchOption.AllDirectories).Length;
        }

        private int CountDocFiles()
        {
            string docsPath = Path.Combine(Application.dataPath, "..", "Docs");
            if (!Directory.Exists(docsPath)) return 0;
            return Directory.GetFiles(docsPath, "*.md", SearchOption.AllDirectories).Length;
        }

        private string GetScriptDescription(string fileName)
        {
            return fileName switch
            {
                "GameManager.cs" => "Master state machine and game flow",
                "ServiceLocator.cs" => "Global service registry pattern",
                "EventBus.cs" => "Pub-sub event system",
                "TurnManager.cs" => "Combat phase orchestration",
                "CardExecutor.cs" => "Card effect execution hub",
                "CorruptionManager.cs" => "Team corruption tracking",
                "MapGenerator.cs" => "Procedural map generation",
                "VFXPoolManager.cs" => "Named VFX pooling system",
                _ => "Game system component"
            };
        }
    }
}
