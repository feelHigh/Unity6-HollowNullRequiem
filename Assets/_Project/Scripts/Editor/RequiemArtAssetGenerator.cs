// ============================================
// RequiemArtAssetGenerator.cs
// Editor tool to generate RequiemArtDataSO assets
// ============================================

#if UNITY_EDITOR
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEditor;
using HNR.Characters;
using HNR.Cards;

namespace HNR.Editor
{
    /// <summary>
    /// Generates RequiemArtDataSO assets for the 4 playable Requiems.
    /// Called from EditorMenuOrganizer.
    /// </summary>
    public static class RequiemArtAssetGenerator
    {
        private const string ASSET_PATH = "Assets/_Project/Data/Characters/RequiemArts";

        public static void GenerateAllRequiemArts()
        {
            EnsureDirectoryExists();

            GenerateKiraArt();
            GenerateMordrenArt();
            GenerateElaraArt();
            GenerateThornwickArt();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("[RequiemArtAssetGenerator] All Requiem Art assets generated successfully!");
        }

        // ============================================
        // Kira - Inferno's Wrath
        // ============================================

        public static void GenerateKiraArt()
        {
            EnsureDirectoryExists();

            var art = ScriptableObject.CreateInstance<RequiemArtDataSO>();

            // Identity
            SetField(art, "_artName", "Inferno's Wrath");
            SetField(art, "_description", "Unleash a devastating flame that engulfs all enemies, dealing massive damage and leaving them burning.");
            SetField(art, "_flavorText", "\"Let the flames consume all who stand against us!\"");

            // Cost & Activation
            SetField(art, "_seCost", 40);
            SetField(art, "_oncePerCombat", true);

            // Targeting
            SetField(art, "_targetType", TargetType.AllEnemies);

            // Effects
            var effects = new List<CardEffectData>
            {
                new CardEffectData(EffectType.Damage, 25),           // 25 damage to all enemies
                new CardEffectData(EffectType.ApplyBurn, 8, 3),      // 8 Burn for 3 turns
                new CardEffectData(EffectType.ApplyVulnerability, 1, 2) // Vulnerable for 2 turns
            };
            SetField(art, "_effects", effects);

            // Visuals
            SetField(art, "_flashColor", new Color(1f, 0.4f, 0.1f, 0.8f)); // Orange flash
            SetField(art, "_effectDuration", 2f);

            SaveAsset(art, "Kira_InfernosWrath");
        }

        // ============================================
        // Mordren - Soul Harvest
        // ============================================

        public static void GenerateMordrenArt()
        {
            EnsureDirectoryExists();

            var art = ScriptableObject.CreateInstance<RequiemArtDataSO>();

            // Identity
            SetField(art, "_artName", "Soul Harvest");
            SetField(art, "_description", "Drain the life essence from all enemies, dealing shadow damage and healing the team for a portion of damage dealt.");
            SetField(art, "_flavorText", "\"Your souls will sustain us.\"");

            // Cost & Activation
            SetField(art, "_seCost", 35);
            SetField(art, "_oncePerCombat", true);

            // Targeting
            SetField(art, "_targetType", TargetType.AllEnemies);

            // Effects
            var effects = new List<CardEffectData>
            {
                new CardEffectData(EffectType.Damage, 18),           // 18 damage to all enemies
                new CardEffectData(EffectType.Heal, 12),             // Heal team 12 HP
                new CardEffectData(EffectType.ApplyWeakness, 1, 2),  // Weakness for 2 turns
                new CardEffectData(EffectType.CorruptionGain, 10)    // Gain 10 Corruption (risk/reward)
            };
            SetField(art, "_effects", effects);

            // Visuals
            SetField(art, "_flashColor", new Color(0.5f, 0.2f, 0.8f, 0.8f)); // Purple flash
            SetField(art, "_effectDuration", 1.8f);

            SaveAsset(art, "Mordren_SoulHarvest");
        }

        // ============================================
        // Elara - Divine Aegis
        // ============================================

        public static void GenerateElaraArt()
        {
            EnsureDirectoryExists();

            var art = ScriptableObject.CreateInstance<RequiemArtDataSO>();

            // Identity
            SetField(art, "_artName", "Divine Aegis");
            SetField(art, "_description", "Invoke sacred protection, healing the entire team significantly and granting a powerful shield that absorbs incoming damage.");
            SetField(art, "_flavorText", "\"By the light, we are protected!\"");

            // Cost & Activation
            SetField(art, "_seCost", 45);
            SetField(art, "_oncePerCombat", true);

            // Targeting
            SetField(art, "_targetType", TargetType.AllAllies);

            // Effects
            var effects = new List<CardEffectData>
            {
                new CardEffectData(EffectType.Heal, 20),             // Heal team 20 HP
                new CardEffectData(EffectType.Block, 30),            // Grant 30 Block
                new CardEffectData(EffectType.CorruptionReduce, 15)  // Reduce 15 Corruption
            };
            SetField(art, "_effects", effects);

            // Visuals
            SetField(art, "_flashColor", new Color(0.9f, 0.95f, 0.5f, 0.8f)); // Golden flash
            SetField(art, "_effectDuration", 2.2f);

            SaveAsset(art, "Elara_DivineAegis");
        }

        // ============================================
        // Thornwick - Earthen Prison
        // ============================================

        public static void GenerateThornwickArt()
        {
            EnsureDirectoryExists();

            var art = ScriptableObject.CreateInstance<RequiemArtDataSO>();

            // Identity
            SetField(art, "_artName", "Earthen Prison");
            SetField(art, "_description", "Command the earth to rise and trap all enemies, stunning them and making them vulnerable to follow-up attacks.");
            SetField(art, "_flavorText", "\"The earth itself rejects you!\"");

            // Cost & Activation
            SetField(art, "_seCost", 30);
            SetField(art, "_oncePerCombat", true);

            // Targeting
            SetField(art, "_targetType", TargetType.AllEnemies);

            // Effects
            var effects = new List<CardEffectData>
            {
                new CardEffectData(EffectType.Damage, 12),           // 12 damage to all enemies
                new CardEffectData(EffectType.ApplyStun, 1, 1),      // Stun for 1 turn
                new CardEffectData(EffectType.ApplyVulnerability, 1, 2), // Vulnerable for 2 turns
                new CardEffectData(EffectType.Block, 15)             // Grant 15 Block to team
            };
            SetField(art, "_effects", effects);

            // Visuals
            SetField(art, "_flashColor", new Color(0.5f, 0.35f, 0.2f, 0.8f)); // Brown/earth flash
            SetField(art, "_effectDuration", 1.5f);

            SaveAsset(art, "Thornwick_EarthenPrison");
        }

        // ============================================
        // Helper Methods
        // ============================================

        private static void EnsureDirectoryExists()
        {
            if (!AssetDatabase.IsValidFolder(ASSET_PATH))
            {
                string[] parts = ASSET_PATH.Split('/');
                string currentPath = parts[0];
                for (int i = 1; i < parts.Length; i++)
                {
                    string nextPath = currentPath + "/" + parts[i];
                    if (!AssetDatabase.IsValidFolder(nextPath))
                    {
                        AssetDatabase.CreateFolder(currentPath, parts[i]);
                    }
                    currentPath = nextPath;
                }
            }
        }

        private static void SetField(object target, string fieldName, object value)
        {
            var field = target.GetType().GetField(fieldName,
                BindingFlags.NonPublic | BindingFlags.Instance);
            if (field != null)
            {
                field.SetValue(target, value);
            }
            else
            {
                Debug.LogWarning($"[RequiemArtAssetGenerator] Field not found: {fieldName}");
            }
        }

        private static void SaveAsset(RequiemArtDataSO art, string fileName)
        {
            string path = $"{ASSET_PATH}/{fileName}.asset";

            // Check if asset already exists
            var existing = AssetDatabase.LoadAssetAtPath<RequiemArtDataSO>(path);
            if (existing != null)
            {
                EditorUtility.CopySerialized(art, existing);
                EditorUtility.SetDirty(existing);
                Object.DestroyImmediate(art);
                Debug.Log($"[RequiemArtAssetGenerator] Updated {path}");
            }
            else
            {
                AssetDatabase.CreateAsset(art, path);
                Debug.Log($"[RequiemArtAssetGenerator] Created {path}");
            }
        }
    }
}
#endif
