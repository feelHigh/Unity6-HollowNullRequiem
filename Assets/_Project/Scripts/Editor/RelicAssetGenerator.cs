// ============================================
// RelicAssetGenerator.cs
// Editor tool to generate relic ScriptableObject assets
// ============================================

#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using HNR.Progression;

namespace HNR.Editor
{
    /// <summary>
    /// Editor tool to generate relic assets per GDD specifications.
    /// </summary>
    public static class RelicAssetGenerator
    {
        private const string RelicPath = "Assets/_Project/Data/Relics";
        private const string ResourcesPath = "Assets/Resources/Data/Relics";

        [MenuItem("HNR/Generate Relic Assets")]
        public static void GenerateRelicAssets()
        {
            // Ensure directories exist
            EnsureDirectoryExists(RelicPath);
            EnsureDirectoryExists(ResourcesPath);

            int created = 0;

            // 1. Void Heart (Common)
            created += CreateRelic(
                relicId: "relic_void_heart",
                relicName: "Void Heart",
                description: "At combat start, gain {value} Soul Essence.",
                flavorText: "A fragment of the void, pulsing with dark energy.",
                rarity: RelicRarity.Common,
                trigger: RelicTrigger.OnCombatStart,
                effectType: RelicEffectType.GainSoulEssence,
                effectValue: 1
            );

            // 2. Corruption Siphon (Uncommon)
            created += CreateRelic(
                relicId: "relic_corruption_siphon",
                relicName: "Corruption Siphon",
                description: "When you kill an enemy, reduce Corruption by {value}.",
                flavorText: "Feeds on the death of foes, purifying its bearer.",
                rarity: RelicRarity.Uncommon,
                trigger: RelicTrigger.OnKill,
                effectType: RelicEffectType.ReduceCorruption,
                effectValue: 5
            );

            // 3. Hollow Bulwark (Common)
            created += CreateRelic(
                relicId: "relic_hollow_bulwark",
                relicName: "Hollow Bulwark",
                description: "Increase max HP by {value}.",
                flavorText: "A shield forged in the depths of the null.",
                rarity: RelicRarity.Common,
                trigger: RelicTrigger.Passive,
                effectType: RelicEffectType.ModifyMaxHP,
                effectValue: 10
            );

            // 4. Soul Collector (Uncommon)
            created += CreateRelic(
                relicId: "relic_soul_collector",
                relicName: "Soul Collector",
                description: "Gain {value} Void Shards when you kill an enemy.",
                flavorText: "Harvests the essence of fallen foes.",
                rarity: RelicRarity.Uncommon,
                trigger: RelicTrigger.OnKill,
                effectType: RelicEffectType.GainVoidShards,
                effectValue: 5
            );

            // 5. Requiem's Blessing (Rare)
            created += CreateRelic(
                relicId: "relic_requiems_blessing",
                relicName: "Requiem's Blessing",
                description: "Draw {value} extra card at turn start.",
                flavorText: "The spirits of fallen Requiems guide your hand.",
                rarity: RelicRarity.Rare,
                trigger: RelicTrigger.OnTurnStart,
                effectType: RelicEffectType.DrawCard,
                effectValue: 1
            );

            // 6. Null Fragment (Uncommon)
            created += CreateRelic(
                relicId: "relic_null_fragment",
                relicName: "Null Fragment",
                description: "Deal +{value} damage while in Null State.",
                flavorText: "Embrace the void, and it will empower you.",
                rarity: RelicRarity.Uncommon,
                trigger: RelicTrigger.OnNullStateEntered,
                effectType: RelicEffectType.ModifyDamage,
                effectValue: 5
            );

            // 7. Healing Echo (Common)
            created += CreateRelic(
                relicId: "relic_healing_echo",
                relicName: "Healing Echo",
                description: "Heal {value} HP after combat.",
                flavorText: "Whispers of restoration linger after battle.",
                rarity: RelicRarity.Common,
                trigger: RelicTrigger.OnCombatEnd,
                effectType: RelicEffectType.Healing,
                effectValue: 5
            );

            // 8. Ancient Aegis (Uncommon)
            created += CreateRelic(
                relicId: "relic_ancient_aegis",
                relicName: "Ancient Aegis",
                description: "Gain {value} Block at combat start.",
                flavorText: "An ancient ward, still holding strong.",
                rarity: RelicRarity.Uncommon,
                trigger: RelicTrigger.OnCombatStart,
                effectType: RelicEffectType.ModifyBlock,
                effectValue: 5
            );

            // 9. Void Merchant's Coin (Common)
            created += CreateRelic(
                relicId: "relic_merchants_coin",
                relicName: "Void Merchant's Coin",
                description: "Gain {value} Void Shards after Echo events.",
                flavorText: "A lucky coin from a wandering merchant of the void.",
                rarity: RelicRarity.Common,
                trigger: RelicTrigger.OnEventComplete,
                effectType: RelicEffectType.GainVoidShards,
                effectValue: 10
            );

            // 10. Corrupted Core (Rare)
            created += CreateRelic(
                relicId: "relic_corrupted_core",
                relicName: "Corrupted Core",
                description: "All attacks deal +{value} damage.",
                flavorText: "Power at a price. Start with +15 Corruption.",
                rarity: RelicRarity.Rare,
                trigger: RelicTrigger.Passive,
                effectType: RelicEffectType.ModifyDamage,
                effectValue: 3
            );

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"[RelicAssetGenerator] Created {created} relic assets");
            EditorUtility.DisplayDialog("Relic Generation Complete",
                $"Created {created} relic assets in:\n{RelicPath}\n{ResourcesPath}", "OK");
        }

        private static int CreateRelic(
            string relicId,
            string relicName,
            string description,
            string flavorText,
            RelicRarity rarity,
            RelicTrigger trigger,
            RelicEffectType effectType,
            int effectValue)
        {
            string assetName = $"Relic_{relicName.Replace(" ", "").Replace("'", "")}";
            string dataPath = $"{RelicPath}/{assetName}.asset";
            string resourcesDataPath = $"{ResourcesPath}/{assetName}.asset";

            // Check if already exists
            if (AssetDatabase.LoadAssetAtPath<RelicDataSO>(dataPath) != null)
            {
                Debug.Log($"[RelicAssetGenerator] Relic already exists: {relicName}");
                return 0;
            }

            // Create the asset
            var relic = ScriptableObject.CreateInstance<RelicDataSO>();

            // Use SerializedObject to set private fields
            var serializedObject = new SerializedObject(relic);
            serializedObject.FindProperty("_relicId").stringValue = relicId;
            serializedObject.FindProperty("_relicName").stringValue = relicName;
            serializedObject.FindProperty("_description").stringValue = description;
            serializedObject.FindProperty("_flavorText").stringValue = flavorText;
            serializedObject.FindProperty("_rarity").enumValueIndex = (int)rarity;
            serializedObject.FindProperty("_trigger").enumValueIndex = (int)trigger;
            serializedObject.FindProperty("_effectType").enumValueIndex = (int)effectType;
            serializedObject.FindProperty("_effectValue").intValue = effectValue;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();

            // Save to Data folder
            AssetDatabase.CreateAsset(relic, dataPath);

            // Copy to Resources folder for runtime loading
            AssetDatabase.CopyAsset(dataPath, resourcesDataPath);

            Debug.Log($"[RelicAssetGenerator] Created: {relicName} ({rarity})");
            return 1;
        }

        private static void EnsureDirectoryExists(string path)
        {
            if (!AssetDatabase.IsValidFolder(path))
            {
                string[] folders = path.Split('/');
                string currentPath = folders[0];

                for (int i = 1; i < folders.Length; i++)
                {
                    string newPath = $"{currentPath}/{folders[i]}";
                    if (!AssetDatabase.IsValidFolder(newPath))
                    {
                        AssetDatabase.CreateFolder(currentPath, folders[i]);
                    }
                    currentPath = newPath;
                }
            }
        }

        [MenuItem("HNR/Verify Relic Assets")]
        public static void VerifyRelicAssets()
        {
            var relics = Resources.LoadAll<RelicDataSO>("Data/Relics");

            Debug.Log($"[RelicAssetGenerator] Found {relics.Length} relics in Resources:");
            foreach (var relic in relics)
            {
                Debug.Log($"  - {relic.RelicName} ({relic.Rarity}): {relic.GetFormattedDescription()}");
            }

            EditorUtility.DisplayDialog("Relic Verification",
                $"Found {relics.Length} relics in Resources/Data/Relics", "OK");
        }
    }
}
#endif
