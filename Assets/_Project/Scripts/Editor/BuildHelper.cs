// ============================================
// BuildHelper.cs
// Build automation for Android development and release
// ============================================

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
using System;
using System.IO;

namespace HNR.Editor
{
    /// <summary>
    /// Build automation helper for Android APK and AAB builds.
    /// Provides menu items for development, release, and store builds.
    /// </summary>
    /// <remarks>
    /// Build configurations (TDD 11):
    /// - Development: Debug symbols, allow debugging
    /// - Release: Optimized, stripped, symbols for crash reporting
    /// - AAB: App Bundle for Play Store distribution
    ///
    /// Menu: Build → Android → ...
    /// </remarks>
    public static class BuildHelper
    {
        // ============================================
        // Configuration
        // ============================================

        private static readonly string[] SCENE_PATHS =
        {
            "Assets/_Project/Scenes/Boot.unity",
            "Assets/_Project/Scenes/MainMenu.unity",
            "Assets/_Project/Scenes/Bastion.unity",
            "Assets/_Project/Scenes/Combat.unity",
            "Assets/_Project/Scenes/NullRift.unity"
        };

        private const string BUILD_FOLDER = "Builds";
        private const string DEV_APK_NAME = "HNR_Dev.apk";
        private const string RELEASE_APK_NAME = "HNR_Release.apk";
        private const string RELEASE_AAB_NAME = "HNR_Release.aab";

        // ============================================
        // Build Menu Items
        // ============================================

        [MenuItem("Build/Android/Development Build", priority = 100)]
        public static void BuildDevelopment()
        {
            EnsureBuildFolder();

            var options = new BuildPlayerOptions
            {
                scenes = SCENE_PATHS,
                locationPathName = Path.Combine(BUILD_FOLDER, DEV_APK_NAME),
                target = BuildTarget.Android,
                options = BuildOptions.Development | BuildOptions.AllowDebugging
            };

            // Development build settings
            PlayerSettings.Android.minifyDebug = false;
            EditorUserBuildSettings.buildAppBundle = false;

            ExecuteBuild(options, "Development");
        }

        [MenuItem("Build/Android/Release Build", priority = 101)]
        public static void BuildRelease()
        {
            EnsureBuildFolder();

            var options = new BuildPlayerOptions
            {
                scenes = SCENE_PATHS,
                locationPathName = Path.Combine(BUILD_FOLDER, RELEASE_APK_NAME),
                target = BuildTarget.Android,
                options = BuildOptions.None
            };

            // Release build settings (TDD 11)
            PlayerSettings.SetManagedStrippingLevel(NamedBuildTarget.Android, ManagedStrippingLevel.High);
            PlayerSettings.Android.minifyRelease = true;
#pragma warning disable CS0618 // Suppress obsolete warning for androidCreateSymbols
            EditorUserBuildSettings.androidCreateSymbols = AndroidCreateSymbols.Debugging;
#pragma warning restore CS0618
            EditorUserBuildSettings.buildAppBundle = false;

            ExecuteBuild(options, "Release");
        }

        [MenuItem("Build/Android/Release AAB (Store)", priority = 102)]
        public static void BuildAAB()
        {
            EnsureBuildFolder();

            var options = new BuildPlayerOptions
            {
                scenes = SCENE_PATHS,
                locationPathName = Path.Combine(BUILD_FOLDER, RELEASE_AAB_NAME),
                target = BuildTarget.Android,
                options = BuildOptions.None
            };

            // AAB settings for Play Store
            PlayerSettings.SetManagedStrippingLevel(NamedBuildTarget.Android, ManagedStrippingLevel.High);
            PlayerSettings.Android.minifyRelease = true;
#pragma warning disable CS0618 // Suppress obsolete warning for androidCreateSymbols
            EditorUserBuildSettings.androidCreateSymbols = AndroidCreateSymbols.Debugging;
#pragma warning restore CS0618
            EditorUserBuildSettings.buildAppBundle = true;

            ExecuteBuild(options, "AAB (Store)");
        }

        // ============================================
        // Version Management
        // ============================================

        [MenuItem("Build/Version/Increment Build Number", priority = 200)]
        public static void IncrementBuildNumber()
        {
            PlayerSettings.Android.bundleVersionCode++;
            Debug.Log($"[BuildHelper] Bundle version code incremented to: {PlayerSettings.Android.bundleVersionCode}");
        }

        [MenuItem("Build/Version/Show Current Version", priority = 201)]
        public static void ShowCurrentVersion()
        {
            Debug.Log($"[BuildHelper] Version: {PlayerSettings.bundleVersion} (Build {PlayerSettings.Android.bundleVersionCode})");
        }

        // ============================================
        // Utilities
        // ============================================

        [MenuItem("Build/Open Builds Folder", priority = 300)]
        public static void OpenBuildsFolder()
        {
            EnsureBuildFolder();
            EditorUtility.RevealInFinder(BUILD_FOLDER);
        }

        [MenuItem("Build/Clean Builds Folder", priority = 301)]
        public static void CleanBuildsFolder()
        {
            if (Directory.Exists(BUILD_FOLDER))
            {
                if (EditorUtility.DisplayDialog("Clean Builds",
                    "Delete all files in Builds folder?", "Delete", "Cancel"))
                {
                    Directory.Delete(BUILD_FOLDER, true);
                    Directory.CreateDirectory(BUILD_FOLDER);
                    Debug.Log("[BuildHelper] Builds folder cleaned");
                }
            }
        }

        [MenuItem("Build/Validate Build Settings", priority = 302)]
        public static void ValidateBuildSettings()
        {
            Debug.Log("=== BUILD SETTINGS VALIDATION ===");
            bool allValid = true;

            // Check scenes exist
            bool scenesValid = true;
            foreach (var scene in SCENE_PATHS)
            {
                if (!File.Exists(scene))
                {
                    Debug.LogError($"[BuildHelper] Missing scene: {scene}");
                    scenesValid = false;
                }
            }
            Debug.Log($"Scenes: {(scenesValid ? "OK" : "MISSING")}");
            allValid &= scenesValid;

            // Check keystore configuration
            bool keystoreValid = !string.IsNullOrEmpty(PlayerSettings.Android.keystoreName);
            Debug.Log($"Keystore: {(keystoreValid ? "Configured" : "NOT CONFIGURED (required for release)")}");

            // Package name
            string packageName = PlayerSettings.applicationIdentifier;
            bool packageValid = !string.IsNullOrEmpty(packageName) && packageName.Contains(".");
            Debug.Log($"Package: {packageName} {(packageValid ? "OK" : "INVALID")}");
            allValid &= packageValid;

            // Version info
            Debug.Log($"Version: {PlayerSettings.bundleVersion} (Build {PlayerSettings.Android.bundleVersionCode})");

            // Scripting backend
            var backend = PlayerSettings.GetScriptingBackend(NamedBuildTarget.Android);
            bool backendValid = backend == ScriptingImplementation.IL2CPP;
            Debug.Log($"Scripting Backend: {backend} {(backendValid ? "OK" : "(IL2CPP recommended)")}");

            // Target architecture
            var targetArch = PlayerSettings.Android.targetArchitectures;
            bool archValid = (targetArch & AndroidArchitecture.ARM64) != 0;
            Debug.Log($"Architecture: {targetArch} {(archValid ? "OK" : "(ARM64 required)")}");
            allValid &= archValid;

            // Min API level
            var minApi = PlayerSettings.Android.minSdkVersion;
            Debug.Log($"Min API Level: {minApi} (Target: API 24+)");

            // Graphics APIs
            var graphicsAPIs = PlayerSettings.GetGraphicsAPIs(BuildTarget.Android);
            Debug.Log($"Graphics APIs: {string.Join(", ", graphicsAPIs)}");

            Debug.Log("=================================");
            Debug.Log(allValid ? "[BuildHelper] All critical settings valid" : "[BuildHelper] Some settings need attention");
        }

        // ============================================
        // Build Execution
        // ============================================

        private static void ExecuteBuild(BuildPlayerOptions options, string buildType)
        {
            Debug.Log($"[BuildHelper] Starting {buildType} build...");
            Debug.Log($"[BuildHelper] Output: {options.locationPathName}");

            var startTime = DateTime.Now;
            var report = BuildPipeline.BuildPlayer(options);
            var summary = report.summary;

            if (summary.result == BuildResult.Succeeded)
            {
                float sizeMB = summary.totalSize / (1024f * 1024f);
                var duration = DateTime.Now - startTime;

                Debug.Log($"[BuildHelper] {buildType} build SUCCEEDED!");
                Debug.Log($"[BuildHelper] Size: {sizeMB:F1}MB");
                Debug.Log($"[BuildHelper] Time: {duration.TotalSeconds:F1}s");
                Debug.Log($"[BuildHelper] Output: {options.locationPathName}");

                // Check size against TDD target (<150MB)
                if (sizeMB > 150)
                {
                    Debug.LogWarning($"[BuildHelper] APK size ({sizeMB:F1}MB) exceeds target of 150MB");
                }

                // Show notification
                EditorUtility.DisplayDialog("Build Complete",
                    $"{buildType} build succeeded!\n\nSize: {sizeMB:F1}MB\nTime: {duration.TotalSeconds:F1}s",
                    "OK");
            }
            else
            {
                Debug.LogError($"[BuildHelper] {buildType} build FAILED!");
                Debug.LogError($"[BuildHelper] Errors: {summary.totalErrors}, Warnings: {summary.totalWarnings}");

                EditorUtility.DisplayDialog("Build Failed",
                    $"{buildType} build failed!\n\nErrors: {summary.totalErrors}\nCheck console for details.",
                    "OK");
            }
        }

        private static void EnsureBuildFolder()
        {
            if (!Directory.Exists(BUILD_FOLDER))
            {
                Directory.CreateDirectory(BUILD_FOLDER);
                Debug.Log($"[BuildHelper] Created build folder: {BUILD_FOLDER}");
            }
        }
    }
}
#endif
