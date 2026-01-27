// ============================================
// ReadmeGenerator.cs
// Portfolio README.md generator
// ============================================

using System;
using System.IO;
using System.Text;
using UnityEngine;

namespace HNR.Diagnostics
{
    /// <summary>
    /// Generates portfolio-ready README.md file.
    /// </summary>
    public class ReadmeGenerator : MonoBehaviour
    {
        // ============================================
        // Serialized Fields
        // ============================================

        [Header("Project Info")]
        [SerializeField] private string _projectName = "Hollow Null Requiem";
        [SerializeField] private string _authorName = "[Your Name]";
        [SerializeField] private string _repositoryUrl = "https://github.com/[username]/HollowNullRequiem";

        [Header("Output")]
        [SerializeField] private string _outputPath = "README.md";

        // ============================================
        // Public Methods
        // ============================================

        /// <summary>
        /// Generate README.md file.
        /// </summary>
        [ContextMenu("Generate README")]
        public void GenerateReadme()
        {
            var sb = new StringBuilder();

            GenerateHeader(sb);
            GenerateOverview(sb);
            GenerateFeatures(sb);
            GenerateScreenshots(sb);
            GenerateTechnicalStack(sb);
            GenerateArchitecture(sb);
            GenerateDocumentation(sb);
            GenerateBuilding(sb);
            GenerateAuthor(sb);
            GenerateLicense(sb);
            GenerateFooter(sb);

            // Write file
            string fullPath = Path.Combine(Application.dataPath, "..", _outputPath);
            File.WriteAllText(fullPath, sb.ToString());

            Debug.Log($"[ReadmeGenerator] Generated {fullPath}");

#if UNITY_EDITOR
            UnityEditor.AssetDatabase.Refresh();
#endif
        }

        /// <summary>
        /// Get README content as string.
        /// </summary>
        public string GetReadmeContent()
        {
            string path = Path.Combine(Application.dataPath, "..", _outputPath);
            if (File.Exists(path))
                return File.ReadAllText(path);

            GenerateReadme();
            return File.ReadAllText(path);
        }

        /// <summary>
        /// Set project info programmatically.
        /// </summary>
        public void SetProjectInfo(string name, string author, string repoUrl)
        {
            _projectName = name;
            _authorName = author;
            _repositoryUrl = repoUrl;
        }

        // ============================================
        // Generation Methods
        // ============================================

        private void GenerateHeader(StringBuilder sb)
        {
            sb.AppendLine($"# {_projectName}");
            sb.AppendLine();
            sb.AppendLine("![Unity](https://img.shields.io/badge/Unity-6000.0-black?logo=unity)");
            sb.AppendLine("![Platform](https://img.shields.io/badge/Platform-Android-green)");
            sb.AppendLine("![License](https://img.shields.io/badge/License-Portfolio-blue)");
            sb.AppendLine();
            sb.AppendLine("![Game Banner](Docs/banner.png)");
            sb.AppendLine();
        }

        private void GenerateOverview(StringBuilder sb)
        {
            sb.AppendLine("## Overview");
            sb.AppendLine();
            sb.AppendLine("**Hollow Null Requiem** is a fantasy roguelike deckbuilder where players command a team of three Requiems—fallen heroes resurrected to fight the corrupting Hollow—through procedurally generated Null Rifts.");
            sb.AppendLine();
            sb.AppendLine("This project demonstrates professional game development competency including:");
            sb.AppendLine("- Clean architecture with Service Locator and Event Bus patterns");
            sb.AppendLine("- Data-driven design using ScriptableObjects");
            sb.AppendLine("- Mobile-optimized performance (60fps target)");
            sb.AppendLine("- Comprehensive documentation (GDD + TDD)");
            sb.AppendLine("- AI-assisted development workflow");
            sb.AppendLine();
        }

        private void GenerateFeatures(StringBuilder sb)
        {
            sb.AppendLine("## Key Features");
            sb.AppendLine();
            sb.AppendLine("- **Team-Based Deckbuilding**: Three-character parties with shared decks");
            sb.AppendLine("- **Hollow Corruption System**: Risk-reward mechanics with Null State transformation");
            sb.AppendLine("- **Five Soul Aspects**: Elemental interactions (Flame, Shadow, Nature, Arcane, Light)");
            sb.AppendLine("- **Procedural Maps**: Each run offers unique paths and encounters");
            sb.AppendLine("- **40+ Cards**: Diverse card pool with upgrade paths");
            sb.AppendLine("- **10 Relics**: Passive abilities that modify gameplay");
            sb.AppendLine();
        }

        private void GenerateScreenshots(StringBuilder sb)
        {
            sb.AppendLine("## Screenshots");
            sb.AppendLine();
            sb.AppendLine("| Combat | Map | Shop |");
            sb.AppendLine("|--------|-----|------|");
            sb.AppendLine("| ![Combat](Docs/screenshot_combat.png) | ![Map](Docs/screenshot_map.png) | ![Shop](Docs/screenshot_shop.png) |");
            sb.AppendLine();
        }

        private void GenerateTechnicalStack(StringBuilder sb)
        {
            sb.AppendLine("## Technical Stack");
            sb.AppendLine();
            sb.AppendLine("| Category | Technology |");
            sb.AppendLine("|----------|------------|");
            sb.AppendLine("| Engine | Unity 6 (6000.0.63f1) |");
            sb.AppendLine("| Platform | Android (API 24+) |");
            sb.AppendLine("| Scripting | C# with IL2CPP |");
            sb.AppendLine("| Animation | DOTween Pro |");
            sb.AppendLine("| Persistence | Easy Save 3 |");
            sb.AppendLine("| VFX | Cartoon FX Remaster |");
            sb.AppendLine();
        }

        private void GenerateArchitecture(StringBuilder sb)
        {
            sb.AppendLine("## Architecture");
            sb.AppendLine();
            sb.AppendLine("```");
            sb.AppendLine("┌─────────────────────────────────────────┐");
            sb.AppendLine("│           SERVICE LOCATOR               │");
            sb.AppendLine("│  GameManager | AudioManager | SaveManager│");
            sb.AppendLine("└───────────────────┬─────────────────────┘");
            sb.AppendLine("                    │");
            sb.AppendLine("         ┌──────────┼──────────┐");
            sb.AppendLine("         ▼          ▼          ▼");
            sb.AppendLine("    ┌─────────┐ ┌─────────┐ ┌─────────┐");
            sb.AppendLine("    │ Combat  │ │   UI    │ │  Data   │");
            sb.AppendLine("    │ System  │ │ System  │ │  Layer  │");
            sb.AppendLine("    └─────────┘ └─────────┘ └─────────┘");
            sb.AppendLine("```");
            sb.AppendLine();
        }

        private void GenerateDocumentation(StringBuilder sb)
        {
            sb.AppendLine("## Documentation");
            sb.AppendLine();
            sb.AppendLine("- [Game Design Document](Docs/HNR_GameDesignDocument.md)");
            sb.AppendLine("- [Technical Design Documents](Docs/TDD/)");
            sb.AppendLine("- [Architecture Overview](Docs/TDD/00_TDD_Overview.md)");
            sb.AppendLine();
        }

        private void GenerateBuilding(StringBuilder sb)
        {
            sb.AppendLine("## Building");
            sb.AppendLine();
            sb.AppendLine("```bash");
            sb.AppendLine("# Clone repository");
            sb.AppendLine($"git clone {_repositoryUrl}");
            sb.AppendLine();
            sb.AppendLine("# Open in Unity 6 (6000.0.63f1+)");
            sb.AppendLine("# Build > Android > Development Build");
            sb.AppendLine("```");
            sb.AppendLine();
        }

        private void GenerateAuthor(StringBuilder sb)
        {
            sb.AppendLine("## Author");
            sb.AppendLine();
            sb.AppendLine($"**{_authorName}**");
            sb.AppendLine();
            sb.AppendLine("- Portfolio: [yourportfolio.com]");
            sb.AppendLine("- LinkedIn: [linkedin.com/in/yourname]");
            sb.AppendLine("- Email: [your@email.com]");
            sb.AppendLine();
        }

        private void GenerateLicense(StringBuilder sb)
        {
            sb.AppendLine("## License");
            sb.AppendLine();
            sb.AppendLine("This project is a portfolio piece. All rights reserved.");
            sb.AppendLine("Third-party assets are used under their respective licenses.");
            sb.AppendLine();
        }

        private void GenerateFooter(StringBuilder sb)
        {
            sb.AppendLine("---");
            sb.AppendLine();
            sb.AppendLine($"*Generated: {DateTime.Now:yyyy-MM-dd}*");
        }
    }
}
