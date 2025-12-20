using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace HNR.Editor
{
    /// <summary>
    /// Editor utility to find and remove missing script references from prefabs.
    /// </summary>
    public static class MissingScriptCleaner
    {
        [MenuItem("HNR/6. Utilities/Find Missing Scripts in HeroEditor", priority = 210)]
        public static void FindMissingScriptsInHeroEditor()
        {
            FindMissingScriptsInFolder("Assets/ThirdParty/HeroEditor");
        }

        [MenuItem("HNR/6. Utilities/Find Missing Scripts in Project", priority = 211)]
        public static void FindMissingScriptsInProject()
        {
            FindMissingScriptsInFolder("Assets/_Project");
        }

        [MenuItem("HNR/6. Utilities/Find Missing Scripts (All)", priority = 212)]
        public static void FindMissingScriptsAll()
        {
            FindMissingScriptsInFolder("Assets");
        }

        [MenuItem("HNR/6. Utilities/Remove Missing Scripts from HeroEditor", priority = 213)]
        public static void RemoveMissingScriptsFromHeroEditor()
        {
            RemoveMissingScriptsFromFolder("Assets/ThirdParty/HeroEditor");
        }

        [MenuItem("HNR/6. Utilities/Remove Missing Scripts from Project", priority = 214)]
        public static void RemoveMissingScriptsFromProject()
        {
            RemoveMissingScriptsFromFolder("Assets/_Project");
        }

        private static void FindMissingScriptsInFolder(string folderPath)
        {
            string[] prefabGuids = AssetDatabase.FindAssets("t:Prefab", new[] { folderPath });
            int totalMissing = 0;
            var prefabsWithMissing = new List<string>();

            Debug.Log($"[MissingScriptCleaner] Scanning {prefabGuids.Length} prefabs in {folderPath}...");

            foreach (string guid in prefabGuids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);

                if (prefab == null) continue;

                int missingCount = CountMissingScripts(prefab);
                if (missingCount > 0)
                {
                    totalMissing += missingCount;
                    prefabsWithMissing.Add($"  - {path}: {missingCount} missing script(s)");
                    Debug.LogWarning($"[MissingScriptCleaner] Found {missingCount} missing script(s) in: {path}");
                }
            }

            if (totalMissing > 0)
            {
                Debug.LogWarning($"[MissingScriptCleaner] Found {totalMissing} total missing script(s) in {prefabsWithMissing.Count} prefab(s):");
                foreach (var line in prefabsWithMissing)
                {
                    Debug.LogWarning(line);
                }
            }
            else
            {
                Debug.Log($"[MissingScriptCleaner] No missing scripts found in {folderPath}");
            }

            // Also check ScriptableObjects
            string[] assetGuids = AssetDatabase.FindAssets("t:ScriptableObject", new[] { folderPath });
            int missingInAssets = 0;

            foreach (string guid in assetGuids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var asset = AssetDatabase.LoadAssetAtPath<ScriptableObject>(path);

                if (asset == null)
                {
                    missingInAssets++;
                    Debug.LogWarning($"[MissingScriptCleaner] ScriptableObject with missing script: {path}");
                }
            }

            if (missingInAssets > 0)
            {
                Debug.LogWarning($"[MissingScriptCleaner] Found {missingInAssets} ScriptableObject(s) with missing scripts");
            }
        }

        private static int CountMissingScripts(GameObject go)
        {
            int count = 0;

            // Check all components on this GameObject
            Component[] components = go.GetComponents<Component>();
            foreach (var component in components)
            {
                if (component == null)
                {
                    count++;
                }
            }

            // Recursively check children
            foreach (Transform child in go.transform)
            {
                count += CountMissingScripts(child.gameObject);
            }

            return count;
        }

        private static void RemoveMissingScriptsFromFolder(string folderPath)
        {
            string[] prefabGuids = AssetDatabase.FindAssets("t:Prefab", new[] { folderPath });
            int totalRemoved = 0;
            var modifiedPrefabs = new List<string>();

            Debug.Log($"[MissingScriptCleaner] Scanning and cleaning {prefabGuids.Length} prefabs in {folderPath}...");

            foreach (string guid in prefabGuids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);

                // Load the prefab contents
                using (var editScope = new PrefabUtility.EditPrefabContentsScope(path))
                {
                    GameObject prefabRoot = editScope.prefabContentsRoot;
                    int removed = RemoveMissingScriptsRecursive(prefabRoot);

                    if (removed > 0)
                    {
                        totalRemoved += removed;
                        modifiedPrefabs.Add($"  - {path}: removed {removed} missing script(s)");
                        Debug.Log($"[MissingScriptCleaner] Removed {removed} missing script(s) from: {path}");
                    }
                }
            }

            if (totalRemoved > 0)
            {
                AssetDatabase.SaveAssets();
                Debug.Log($"[MissingScriptCleaner] Removed {totalRemoved} total missing script(s) from {modifiedPrefabs.Count} prefab(s):");
                foreach (var line in modifiedPrefabs)
                {
                    Debug.Log(line);
                }
            }
            else
            {
                Debug.Log($"[MissingScriptCleaner] No missing scripts to remove in {folderPath}");
            }
        }

        private static int RemoveMissingScriptsRecursive(GameObject go)
        {
            int removed = GameObjectUtility.RemoveMonoBehavioursWithMissingScript(go);

            // Process children
            foreach (Transform child in go.transform)
            {
                removed += RemoveMissingScriptsRecursive(child.gameObject);
            }

            return removed;
        }
    }
}
