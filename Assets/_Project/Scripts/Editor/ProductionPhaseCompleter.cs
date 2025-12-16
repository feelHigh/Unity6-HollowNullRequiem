// ============================================
// ProductionPhaseCompleter.cs
// Master script to complete all production phases
// ============================================

#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

namespace HNR.Editor
{
    /// <summary>
    /// Master script to complete all production phases.
    /// Provides menu items for easy access to phase completion tools.
    /// </summary>
    public static class ProductionPhaseCompleter
    {
        // ============================================
        // Complete All Phases
        // ============================================

        /// <summary>
        /// Completes all remaining production phases (2 and 3).
        /// </summary>
        public static void CompleteAllPhases()
        {
            Debug.Log("[ProductionPhaseCompleter] Starting complete production setup...");

            // Phase 2: Scene UI Wiring
            Debug.Log("[ProductionPhaseCompleter] Running Phase 2: Scene UI Wiring...");
            Phase2SceneCompleter.CompleteAllScenes();

            // Phase 3: Global UI Setup
            Debug.Log("[ProductionPhaseCompleter] Running Phase 3: Global UI Setup...");
            Phase3GlobalUISetup.CompletePhase3();

            Debug.Log("[ProductionPhaseCompleter] All phases completed!");

            EditorUtility.DisplayDialog("Production Setup Complete",
                "All production phases have been completed!\n\n" +
                "Phase 2: Scene UI Wiring - Done\n" +
                "Phase 3: Global UI Setup - Done\n\n" +
                "Next: Run game from Boot scene to test.",
                "OK");
        }

        // ============================================
        // Phase 2 Individual Methods
        // ============================================

        /// <summary>
        /// Completes Phase 2 - Scene UI Wiring.
        /// </summary>
        public static void CompletePhase2()
        {
            Phase2SceneCompleter.CompleteAllScenes();
        }

        /// <summary>
        /// Fixes NullRift scene only.
        /// </summary>
        public static void FixNullRiftScene()
        {
            Phase2SceneCompleter.CompleteNullRiftScene();
        }

        /// <summary>
        /// Fixes Combat scene only.
        /// </summary>
        public static void FixCombatScene()
        {
            Phase2SceneCompleter.CompleteCombatScene();
        }

        // ============================================
        // Phase 3 Individual Methods
        // ============================================

        /// <summary>
        /// Completes Phase 3 - Global UI Setup.
        /// </summary>
        public static void CompletePhase3()
        {
            Phase3GlobalUISetup.CompletePhase3();
        }

        /// <summary>
        /// Creates UIManager prefab only.
        /// </summary>
        public static void CreateUIManagerPrefab()
        {
            Phase3GlobalUISetup.CreateUIManagerPrefab();
        }

        /// <summary>
        /// Creates ToastManager prefab only.
        /// </summary>
        public static void CreateToastManagerPrefab()
        {
            Phase3GlobalUISetup.CreateToastManagerPrefab();
        }

        /// <summary>
        /// Adds LoadingScreen to scenes.
        /// </summary>
        public static void AddLoadingScreenToScenes()
        {
            Phase3GlobalUISetup.AddLoadingScreenToScenes();
        }

        // ============================================
        // Verification
        // ============================================

        /// <summary>
        /// Verifies the current state of all phases.
        /// </summary>
        public static void VerifyAllPhases()
        {
            var report = new System.Text.StringBuilder();
            report.AppendLine("=== Production Phase Verification ===\n");

            // Check Phase 2 scenes
            report.AppendLine("Phase 2 - Scene UI Wiring:");
            report.AppendLine(VerifyPhase2());
            report.AppendLine();

            // Check Phase 3 global UI
            report.AppendLine("Phase 3 - Global UI Setup:");
            report.AppendLine(VerifyPhase3());

            Debug.Log(report.ToString());
            EditorUtility.DisplayDialog("Phase Verification", report.ToString(), "OK");
        }

        private static string VerifyPhase2()
        {
            var sb = new System.Text.StringBuilder();

            // Check for key components in scenes (simplified check via asset existence)
            string scenesPath = "Assets/_Project/Scenes/";

            sb.AppendLine($"  Boot.unity: {(System.IO.File.Exists(scenesPath + "Boot.unity") ? "Exists" : "Missing")}");
            sb.AppendLine($"  MainMenu.unity: {(System.IO.File.Exists(scenesPath + "MainMenu.unity") ? "Exists" : "Missing")}");
            sb.AppendLine($"  Bastion.unity: {(System.IO.File.Exists(scenesPath + "Bastion.unity") ? "Exists" : "Missing")}");
            sb.AppendLine($"  NullRift.unity: {(System.IO.File.Exists(scenesPath + "NullRift.unity") ? "Exists" : "Missing")}");
            sb.AppendLine($"  Combat.unity: {(System.IO.File.Exists(scenesPath + "Combat.unity") ? "Exists" : "Missing")}");

            return sb.ToString();
        }

        private static string VerifyPhase3()
        {
            var sb = new System.Text.StringBuilder();
            string prefabsPath = "Assets/_Project/Prefabs/UI/";

            var uiManager = AssetDatabase.LoadAssetAtPath<GameObject>(prefabsPath + "UIManager.prefab");
            sb.AppendLine($"  UIManager.prefab: {(uiManager != null ? "Exists" : "Missing")}");

            var toastManager = AssetDatabase.LoadAssetAtPath<GameObject>(prefabsPath + "ToastManager.prefab");
            sb.AppendLine($"  ToastManager.prefab: {(toastManager != null ? "Exists" : "Missing")}");

            var toast = AssetDatabase.LoadAssetAtPath<GameObject>(prefabsPath + "Toast.prefab");
            sb.AppendLine($"  Toast.prefab: {(toast != null ? "Exists" : "Missing")}");

            var globalHeader = AssetDatabase.LoadAssetAtPath<GameObject>(prefabsPath + "GlobalHeader.prefab");
            sb.AppendLine($"  GlobalHeader.prefab: {(globalHeader != null ? "Exists" : "Missing")}");

            var globalNavDock = AssetDatabase.LoadAssetAtPath<GameObject>(prefabsPath + "GlobalNavDock.prefab");
            sb.AppendLine($"  GlobalNavDock.prefab: {(globalNavDock != null ? "Exists" : "Missing")}");

            return sb.ToString();
        }
    }
}
#endif
