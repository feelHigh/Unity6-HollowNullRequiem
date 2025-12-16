// ============================================
// CharacterVisualPrefabGenerator.cs
// Editor tool for creating character visual prefabs
// ============================================

#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;
using HNR.Characters.Visuals;

namespace HNR.Editor
{
    /// <summary>
    /// Editor tool to generate character visual prefabs from HeroEditor templates.
    /// Creates prefab variants with AnimatedCharacterVisual component attached.
    /// </summary>
    public static class CharacterVisualPrefabGenerator
    {
        private const string REQUIEM_PREFAB_PATH = "Assets/_Project/Prefabs/Characters/Requiems";
        private const string ENEMY_PREFAB_PATH = "Assets/_Project/Prefabs/Characters/Enemies";

        // HeroEditor template paths (in order of preference)
        private static readonly string[] HEROEDITOR_TEMPLATE_PATHS = new[]
        {
            "Assets/_Project/Art/Characters/Requiems/ExampleCharacter.prefab",
            "Assets/ThirdParty/HeroEditor/FantasyHeroes/Prefabs/Human.prefab",
            "Assets/ThirdParty/HeroEditor/UndeadHeroes/Prefabs/Undead.prefab"
        };

        // ============================================
        // Public Methods
        // ============================================

        public static void GenerateAllPrefabs()
        {
            EnsureDirectoriesExist();

            int created = 0;

            // Generate Requiem prefabs
            created += GenerateRequiemPrefab("Kira", AttackType.Slash);
            created += GenerateRequiemPrefab("Mordren", AttackType.Jab);
            created += GenerateRequiemPrefab("Elara", AttackType.Shoot);
            created += GenerateRequiemPrefab("Thornwick", AttackType.Slash);

            // Generate basic enemy prefabs
            created += GenerateEnemyPrefab("HollowThrall", AttackType.Slash);
            created += GenerateEnemyPrefab("CorruptionSprite", AttackType.Jab);
            created += GenerateEnemyPrefab("VoidHound", AttackType.Slash);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog(
                "Character Visual Prefabs",
                $"Generated {created} prefab(s).\n\nNote: Configure appearance in Unity using HeroEditor CharacterBuilder.",
                "OK"
            );
        }

        /// <summary>
        /// Find the first available HeroEditor template.
        /// </summary>
        private static string FindTemplatePath()
        {
            foreach (var path in HEROEDITOR_TEMPLATE_PATHS)
            {
                if (File.Exists(path))
                {
                    return path;
                }
            }
            return null;
        }

        public static void CreateEmptyVisualPrefab()
        {
            string path = EditorUtility.SaveFilePanelInProject(
                "Create Visual Prefab",
                "NewCharacter_Visual",
                "prefab",
                "Create a new character visual prefab",
                REQUIEM_PREFAB_PATH
            );

            if (string.IsNullOrEmpty(path)) return;

            CreateVisualPrefabFromTemplate(path);

            EditorUtility.DisplayDialog(
                "Visual Prefab Created",
                $"Created prefab at:\n{path}\n\nUse HeroEditor CharacterBuilder to customize appearance.",
                "OK"
            );
        }

        // ============================================
        // Prefab Generation
        // ============================================

        private static int GenerateRequiemPrefab(string name, AttackType attackType)
        {
            string prefabPath = $"{REQUIEM_PREFAB_PATH}/{name}_Visual.prefab";

            if (File.Exists(prefabPath))
            {
                Debug.Log($"[CharacterVisualPrefabGenerator] Skipping {name} - prefab already exists");
                return 0;
            }

            return CreateVisualPrefabFromTemplate(prefabPath) ? 1 : 0;
        }

        private static int GenerateEnemyPrefab(string name, AttackType attackType)
        {
            string prefabPath = $"{ENEMY_PREFAB_PATH}/{name}_Visual.prefab";

            if (File.Exists(prefabPath))
            {
                Debug.Log($"[CharacterVisualPrefabGenerator] Skipping {name} - prefab already exists");
                return 0;
            }

            return CreateVisualPrefabFromTemplate(prefabPath) ? 1 : 0;
        }

        private static bool CreateVisualPrefabFromTemplate(string outputPath)
        {
            // Find available HeroEditor template
            string templatePath = FindTemplatePath();
            GameObject template = null;

            if (templatePath != null)
            {
                template = AssetDatabase.LoadAssetAtPath<GameObject>(templatePath);
                Debug.Log($"[CharacterVisualPrefabGenerator] Using template: {templatePath}");
            }

            if (template == null)
            {
                // Create a simple placeholder if HeroEditor not available
                Debug.LogWarning("[CharacterVisualPrefabGenerator] HeroEditor template not found, creating placeholder");
                return CreatePlaceholderPrefab(outputPath);
            }

            // Instantiate the template
            GameObject instance = Object.Instantiate(template);
            instance.name = Path.GetFileNameWithoutExtension(outputPath);

            // Add AnimatedCharacterVisual component if not present
            if (instance.GetComponent<AnimatedCharacterVisual>() == null)
            {
                var visual = instance.AddComponent<AnimatedCharacterVisual>();

                // Try to find and assign the Character component
                var character = instance.GetComponentInChildren<Assets.HeroEditor.Common.Scripts.CharacterScripts.Character>();
                if (character != null)
                {
                    // Use reflection to set the private field (or make it public/add Initialize method)
                    var field = typeof(AnimatedCharacterVisual).GetField("_character",
                        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    field?.SetValue(visual, character);
                }
            }

            // Ensure output directory exists
            string directory = Path.GetDirectoryName(outputPath);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            // Create prefab
            bool success = false;
            PrefabUtility.SaveAsPrefabAsset(instance, outputPath, out success);

            // Clean up instance
            Object.DestroyImmediate(instance);

            if (success)
            {
                Debug.Log($"[CharacterVisualPrefabGenerator] Created prefab: {outputPath}");
            }
            else
            {
                Debug.LogError($"[CharacterVisualPrefabGenerator] Failed to create prefab: {outputPath}");
            }

            return success;
        }

        private static bool CreatePlaceholderPrefab(string outputPath)
        {
            // Create a simple placeholder GameObject
            GameObject placeholder = new GameObject(Path.GetFileNameWithoutExtension(outputPath));

            // Add SpriteRenderer as fallback visual
            var spriteRenderer = placeholder.AddComponent<SpriteRenderer>();
            spriteRenderer.color = Color.gray;

            // Add SimpleCharacterVisual instead
            placeholder.AddComponent<SimpleCharacterVisual>();

            // Ensure output directory exists
            string directory = Path.GetDirectoryName(outputPath);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            // Create prefab
            bool success = false;
            PrefabUtility.SaveAsPrefabAsset(placeholder, outputPath, out success);

            // Clean up
            Object.DestroyImmediate(placeholder);

            return success;
        }

        // ============================================
        // Utility
        // ============================================

        private static void EnsureDirectoriesExist()
        {
            if (!Directory.Exists(REQUIEM_PREFAB_PATH))
            {
                Directory.CreateDirectory(REQUIEM_PREFAB_PATH);
                AssetDatabase.Refresh();
            }

            if (!Directory.Exists(ENEMY_PREFAB_PATH))
            {
                Directory.CreateDirectory(ENEMY_PREFAB_PATH);
                AssetDatabase.Refresh();
            }
        }

        // ============================================
        // Prefab Assignment Helper
        // ============================================

        public static void AssignPrefabsToDataAssets()
        {
            int assigned = 0;

            // Find all RequiemDataSO assets
            string[] requiemGuids = AssetDatabase.FindAssets("t:RequiemDataSO");
            foreach (string guid in requiemGuids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var requiemData = AssetDatabase.LoadAssetAtPath<HNR.Characters.RequiemDataSO>(path);

                if (requiemData == null) continue;

                // Try to find matching visual prefab
                string visualPath = $"{REQUIEM_PREFAB_PATH}/{requiemData.RequiemName}_Visual.prefab";
                GameObject visualPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(visualPath);

                if (visualPrefab != null)
                {
                    // Use SerializedObject to set the prefab reference
                    SerializedObject so = new SerializedObject(requiemData);
                    SerializedProperty visualProp = so.FindProperty("_visualPrefab");

                    if (visualProp != null && visualProp.objectReferenceValue == null)
                    {
                        visualProp.objectReferenceValue = visualPrefab;
                        so.ApplyModifiedProperties();
                        assigned++;
                        Debug.Log($"[CharacterVisualPrefabGenerator] Assigned {visualPrefab.name} to {requiemData.RequiemName}");
                    }
                }
            }

            // Find all EnemyDataSO assets
            string[] enemyGuids = AssetDatabase.FindAssets("t:EnemyDataSO");
            foreach (string guid in enemyGuids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var enemyData = AssetDatabase.LoadAssetAtPath<HNR.Combat.EnemyDataSO>(path);

                if (enemyData == null) continue;

                // Try to find matching visual prefab (sanitize name for path)
                string sanitizedName = enemyData.EnemyName.Replace(" ", "");
                string visualPath = $"{ENEMY_PREFAB_PATH}/{sanitizedName}_Visual.prefab";
                GameObject visualPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(visualPath);

                if (visualPrefab != null)
                {
                    SerializedObject so = new SerializedObject(enemyData);
                    SerializedProperty visualProp = so.FindProperty("_visualPrefab");

                    if (visualProp != null && visualProp.objectReferenceValue == null)
                    {
                        visualProp.objectReferenceValue = visualPrefab;
                        so.ApplyModifiedProperties();
                        assigned++;
                        Debug.Log($"[CharacterVisualPrefabGenerator] Assigned {visualPrefab.name} to {enemyData.EnemyName}");
                    }
                }
            }

            AssetDatabase.SaveAssets();

            EditorUtility.DisplayDialog(
                "Visual Prefab Assignment",
                $"Assigned {assigned} visual prefab(s) to data assets.",
                "OK"
            );
        }
    }
}
#endif
