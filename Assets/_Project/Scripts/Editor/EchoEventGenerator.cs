// ============================================
// EchoEventGenerator.cs
// Editor utility to generate test Echo events
// ============================================

#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using HNR.Map;

namespace HNR.Editor
{
    /// <summary>
    /// Editor utility to generate placeholder Echo events for testing.
    /// </summary>
    public static class EchoEventGenerator
    {
        private const string EventsPath = "Assets/_Project/Resources/Data/Events";
        private const string ZoneConfigPath = "Assets/_Project/Resources/Data/Zones";

        public static void GenerateTestEchoEvents()
        {
            // Ensure directories exist
            EnsureDirectoryExists(EventsPath);

            // Generate 6 test events
            CreateAbandonedCacheEvent();
            CreateWoundedTravelerEvent();
            CreateVoidRiftEvent();
            CreateAncientShrineEvent();
            CreateShadowMerchantEvent();
            CreateMemoryFragmentEvent();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("[EchoEventGenerator] Generated 6 test Echo events");
        }

        public static void GenerateTestZoneConfig()
        {
            EnsureDirectoryExists(ZoneConfigPath);

            var config = ScriptableObject.CreateInstance<ZoneConfigSO>();

            // Use reflection or create a minimal config
            // Since fields are private with SerializeField, we use SerializedObject
            string path = $"{ZoneConfigPath}/Zone1_TestConfig.asset";
            AssetDatabase.CreateAsset(config, path);

            // Configure via SerializedObject
            var serializedObj = new SerializedObject(config);
            serializedObj.FindProperty("_zoneNumber").intValue = 1;
            serializedObj.FindProperty("_zoneName").stringValue = "Test Zone 1";
            serializedObj.FindProperty("_rowCount").intValue = 5;
            serializedObj.FindProperty("_minNodesPerRow").intValue = 2;
            serializedObj.FindProperty("_maxNodesPerRow").intValue = 3;
            serializedObj.FindProperty("_combatWeight").intValue = 50;
            serializedObj.FindProperty("_eliteWeight").intValue = 10;
            serializedObj.FindProperty("_shopWeight").intValue = 10;
            serializedObj.FindProperty("_echoWeight").intValue = 15;
            serializedObj.FindProperty("_sanctuaryWeight").intValue = 10;
            serializedObj.FindProperty("_treasureWeight").intValue = 5;
            serializedObj.FindProperty("_eliteMinRow").intValue = 3;
            serializedObj.FindProperty("_guaranteedShop").boolValue = true;
            serializedObj.FindProperty("_guaranteedSanctuary").boolValue = true;
            serializedObj.FindProperty("_horizontalSpacing").floatValue = 200f;
            serializedObj.FindProperty("_verticalSpacing").floatValue = 150f;
            serializedObj.FindProperty("_nodeJitter").floatValue = 20f;

            // Add echo events to the config
            var echoEventsProp = serializedObj.FindProperty("_echoEvents");
            var eventGuids = AssetDatabase.FindAssets("t:EchoEventDataSO", new[] { EventsPath });
            echoEventsProp.arraySize = eventGuids.Length;
            for (int i = 0; i < eventGuids.Length; i++)
            {
                var eventPath = AssetDatabase.GUIDToAssetPath(eventGuids[i]);
                var eventAsset = AssetDatabase.LoadAssetAtPath<EchoEventDataSO>(eventPath);
                echoEventsProp.GetArrayElementAtIndex(i).objectReferenceValue = eventAsset;
            }

            serializedObj.ApplyModifiedProperties();

            AssetDatabase.SaveAssets();
            Debug.Log($"[EchoEventGenerator] Generated Zone 1 test config at {path}");
        }

        // ============================================
        // Event Creation Methods
        // ============================================

        private static void CreateAbandonedCacheEvent()
        {
            var evt = CreateEvent("Echo_AbandonedCache", "Abandoned Cache",
                "You discover a rusted lockbox half-buried in the corrupted soil. Strange whispers emanate from within, promising treasure... or something else.");

            AddChoice(evt, "Force it open",
                "The lock shatters. Gold coins spill out, glinting with an otherworldly sheen.",
                new[] { (EchoOutcomeType.GainGold, 50, "", "") });

            AddChoice(evt, "Listen to the whispers",
                "The voices guide you to a hidden card, pulsing with power.",
                new[] { (EchoOutcomeType.GainRandomCard, 0, "", "") });

            AddChoice(evt, "Leave it be",
                "Some things are best left undisturbed. You walk away unscathed.",
                new[] { (EchoOutcomeType.None, 0, "", "") });

            SaveEvent(evt, "Echo_AbandonedCache");
        }

        private static void CreateWoundedTravelerEvent()
        {
            var evt = CreateEvent("Echo_WoundedTraveler", "Wounded Traveler",
                "A fellow wanderer lies wounded by the path. Their eyes plead for help, but the corruption spreading through their veins tells another story.");

            AddChoice(evt, "Heal them (Lose 15 HP)",
                "Your sacrifice purifies their wounds. They gift you an ancient remedy before departing.",
                new[] {
                    (EchoOutcomeType.LoseHP, 15, "", ""),
                    (EchoOutcomeType.LoseCorruption, 10, "", "")
                });

            AddChoice(evt, "End their suffering",
                "A mercy killing. The corruption dissipates, leaving behind void shards.",
                new[] {
                    (EchoOutcomeType.GainGold, 30, "", ""),
                    (EchoOutcomeType.GainCorruption, 5, "", "")
                });

            AddChoice(evt, "Walk away",
                "Their fate is not your burden. You continue on.",
                new[] { (EchoOutcomeType.None, 0, "", "") });

            SaveEvent(evt, "Echo_WoundedTraveler");
        }

        private static void CreateVoidRiftEvent()
        {
            var evt = CreateEvent("Echo_VoidRift", "Void Rift",
                "A tear in reality crackles before you, raw void energy seeping through. Those who touch it are forever changed.");

            AddChoice(evt, "Embrace the void",
                "Power surges through you, but at a cost. Your maximum potential expands.",
                new[] {
                    (EchoOutcomeType.GainMaxHP, 10, "", ""),
                    (EchoOutcomeType.GainCorruption, 15, "", "")
                });

            AddChoice(evt, "Reach through carefully",
                "You pull something through... a relic from beyond.",
                new[] {
                    (EchoOutcomeType.GainRandomRelic, 0, "", ""),
                    (EchoOutcomeType.GainCorruption, 8, "", "")
                });

            AddChoice(evt, "Seal the rift",
                "You channel your will to close the tear. The corruption recedes.",
                new[] { (EchoOutcomeType.LoseCorruption, 5, "", "") });

            SaveEvent(evt, "Echo_VoidRift");
        }

        private static void CreateAncientShrineEvent()
        {
            var evt = CreateEvent("Echo_AncientShrine", "Ancient Shrine",
                "A forgotten shrine stands untouched by corruption. The altar glows with residual divine energy, offering blessings to those who would pray.");

            AddChoice(evt, "Pray for healing",
                "Warm light envelops you, mending wounds and soothing pain.",
                new[] { (EchoOutcomeType.GainHP, 25, "", "") });

            AddChoice(evt, "Pray for strength",
                "The shrine grants you an upgrade to one of your cards.",
                new[] { (EchoOutcomeType.UpgradeCard, 0, "", "") });

            AddChoice(evt, "Pray for guidance",
                "Visions show you the path ahead and hidden treasures.",
                new[] { (EchoOutcomeType.GainGold, 40, "", "") });

            SaveEvent(evt, "Echo_AncientShrine");
        }

        private static void CreateShadowMerchantEvent()
        {
            var evt = CreateEvent("Echo_ShadowMerchant", "Shadow Merchant",
                "A cloaked figure materializes from the shadows. 'I deal in trades most peculiar,' they whisper. 'What would you give... to gain?'");

            AddChoice(evt, "Trade HP for gold (Lose 20 HP)",
                "Blood is the currency of the desperate. The merchant seems pleased.",
                new[] {
                    (EchoOutcomeType.LoseHP, 20, "", ""),
                    (EchoOutcomeType.GainGold, 75, "", "")
                });

            AddChoice(evt, "Trade gold for power (Lose 50 gold)",
                "The merchant produces a card unlike any you've seen.",
                new[] {
                    (EchoOutcomeType.LoseGold, 50, "", ""),
                    (EchoOutcomeType.GainRandomCard, 0, "", "")
                });

            AddChoice(evt, "Decline",
                "The merchant shrugs and fades into shadow. 'Perhaps next time...'",
                new[] { (EchoOutcomeType.None, 0, "", "") });

            SaveEvent(evt, "Echo_ShadowMerchant");
        }

        private static void CreateMemoryFragmentEvent()
        {
            var evt = CreateEvent("Echo_MemoryFragment", "Memory Fragment",
                "A crystallized memory floats before you - an echo of someone's final moments before the Hollow consumed them. Do you dare witness their fate?");

            AddChoice(evt, "Absorb the memory",
                "Pain. Fear. Then... understanding. You learn from their mistakes.",
                new[] {
                    (EchoOutcomeType.GainCorruption, 3, "", ""),
                    (EchoOutcomeType.GainRandomCard, 0, "", "")
                });

            AddChoice(evt, "Shatter the crystal",
                "The memory disperses, leaving behind concentrated void essence.",
                new[] { (EchoOutcomeType.GainGold, 25, "", "") });

            AddChoice(evt, "Leave it intact",
                "Some memories deserve to persist. You feel strangely at peace.",
                new[] { (EchoOutcomeType.LoseCorruption, 3, "", "") });

            SaveEvent(evt, "Echo_MemoryFragment");
        }

        // ============================================
        // Helper Methods
        // ============================================

        private static EchoEventDataSO CreateEvent(string id, string title, string narrative)
        {
            var evt = ScriptableObject.CreateInstance<EchoEventDataSO>();

            var serializedObj = new SerializedObject(evt);
            serializedObj.FindProperty("_eventId").stringValue = id;
            serializedObj.FindProperty("_eventTitle").stringValue = title;
            serializedObj.FindProperty("_narrative").stringValue = narrative;
            serializedObj.FindProperty("_uniquePerRun").boolValue = true;
            serializedObj.ApplyModifiedProperties();

            return evt;
        }

        private static void AddChoice(EchoEventDataSO evt, string choiceText, string outcomeText,
            (EchoOutcomeType type, int value, string cardId, string relicId)[] outcomes)
        {
            var serializedObj = new SerializedObject(evt);
            var choicesProp = serializedObj.FindProperty("_choices");

            int index = choicesProp.arraySize;
            choicesProp.InsertArrayElementAtIndex(index);

            var choiceProp = choicesProp.GetArrayElementAtIndex(index);
            choiceProp.FindPropertyRelative("_choiceText").stringValue = choiceText;
            choiceProp.FindPropertyRelative("_outcomeText").stringValue = outcomeText;

            var outcomesProp = choiceProp.FindPropertyRelative("_outcomes");
            outcomesProp.arraySize = outcomes.Length;

            for (int i = 0; i < outcomes.Length; i++)
            {
                var outcomeProp = outcomesProp.GetArrayElementAtIndex(i);
                outcomeProp.FindPropertyRelative("_type").enumValueIndex = (int)outcomes[i].type;
                outcomeProp.FindPropertyRelative("_value").intValue = outcomes[i].value;
                outcomeProp.FindPropertyRelative("_cardId").stringValue = outcomes[i].cardId;
                outcomeProp.FindPropertyRelative("_relicId").stringValue = outcomes[i].relicId;
            }

            serializedObj.ApplyModifiedProperties();
        }

        private static void SaveEvent(EchoEventDataSO evt, string filename)
        {
            string path = $"{EventsPath}/{filename}.asset";
            AssetDatabase.CreateAsset(evt, path);
        }

        private static void EnsureDirectoryExists(string path)
        {
            if (!AssetDatabase.IsValidFolder(path))
            {
                string[] parts = path.Split('/');
                string currentPath = parts[0];

                for (int i = 1; i < parts.Length; i++)
                {
                    string newPath = $"{currentPath}/{parts[i]}";
                    if (!AssetDatabase.IsValidFolder(newPath))
                    {
                        AssetDatabase.CreateFolder(currentPath, parts[i]);
                    }
                    currentPath = newPath;
                }
            }
        }
    }
}
#endif
