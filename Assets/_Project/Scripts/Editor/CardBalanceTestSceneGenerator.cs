// ============================================
// CardBalanceTestSceneGenerator.cs
// Editor tool to generate card balance test scene
// ============================================

using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using TMPro;
using HNR.Characters;
using HNR.Cards;
using System.Collections.Generic;

namespace HNR.Editor
{
    public static class CardBalanceTestSceneGenerator
    {
        public static void GenerateCardBalanceTestScene()
        {
            // Create new scene
            var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

            // === Create UI Canvas ===
            GameObject canvasObj = new GameObject("TestCanvas");
            Canvas canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            canvasObj.AddComponent<GraphicRaycaster>();

            // === Instructions Panel ===
            GameObject panel = new GameObject("InstructionsPanel");
            panel.transform.SetParent(canvasObj.transform, false);
            RectTransform panelRect = panel.AddComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0, 1);
            panelRect.anchorMax = new Vector2(0, 1);
            panelRect.pivot = new Vector2(0, 1);
            panelRect.anchoredPosition = new Vector2(20, -20);
            panelRect.sizeDelta = new Vector2(500, 200);
            Image panelImg = panel.AddComponent<Image>();
            panelImg.color = new Color(0, 0, 0, 0.7f);

            // Instructions Text
            GameObject instructionsObj = new GameObject("InstructionsText");
            instructionsObj.transform.SetParent(panel.transform, false);
            RectTransform instructionsRect = instructionsObj.AddComponent<RectTransform>();
            instructionsRect.anchorMin = Vector2.zero;
            instructionsRect.anchorMax = Vector2.one;
            instructionsRect.offsetMin = new Vector2(10, 10);
            instructionsRect.offsetMax = new Vector2(-10, -10);
            TextMeshProUGUI instructionsTMP = instructionsObj.AddComponent<TextMeshProUGUI>();
            instructionsTMP.text = "<b>Card Balance Test Scene</b>\n\n" +
                "Press <color=yellow>B</color> to run balance tests\n\n" +
                "Tests:\n" +
                "- Damage output per Requiem\n" +
                "- Block/Defense values\n" +
                "- Healing output\n" +
                "- Utility (Draw, AP, SE)\n" +
                "- Role validation\n" +
                "- Team composition";
            instructionsTMP.fontSize = 18;
            instructionsTMP.alignment = TextAlignmentOptions.TopLeft;
            instructionsTMP.color = Color.white;

            // === Title ===
            GameObject titleObj = new GameObject("TitleText");
            titleObj.transform.SetParent(canvasObj.transform, false);
            RectTransform titleRect = titleObj.AddComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0.5f, 1);
            titleRect.anchorMax = new Vector2(0.5f, 1);
            titleRect.anchoredPosition = new Vector2(0, -50);
            titleRect.sizeDelta = new Vector2(600, 60);
            TextMeshProUGUI titleTMP = titleObj.AddComponent<TextMeshProUGUI>();
            titleTMP.text = "Card Balance Test";
            titleTMP.fontSize = 48;
            titleTMP.alignment = TextAlignmentOptions.Center;
            titleTMP.color = Color.white;
            titleTMP.fontStyle = FontStyles.Bold;

            // === Status Panel ===
            GameObject statusPanel = new GameObject("StatusPanel");
            statusPanel.transform.SetParent(canvasObj.transform, false);
            RectTransform statusRect = statusPanel.AddComponent<RectTransform>();
            statusRect.anchorMin = new Vector2(0.5f, 0.5f);
            statusRect.anchorMax = new Vector2(0.5f, 0.5f);
            statusRect.anchoredPosition = Vector2.zero;
            statusRect.sizeDelta = new Vector2(700, 400);
            Image statusImg = statusPanel.AddComponent<Image>();
            statusImg.color = new Color(0.1f, 0.1f, 0.2f, 0.9f);

            // Status Text
            GameObject statusTextObj = new GameObject("StatusText");
            statusTextObj.transform.SetParent(statusPanel.transform, false);
            RectTransform statusTextRect = statusTextObj.AddComponent<RectTransform>();
            statusTextRect.anchorMin = Vector2.zero;
            statusTextRect.anchorMax = Vector2.one;
            statusTextRect.offsetMin = new Vector2(20, 20);
            statusTextRect.offsetMax = new Vector2(-20, -20);
            TextMeshProUGUI statusTMP = statusTextObj.AddComponent<TextMeshProUGUI>();
            statusTMP.text = "<b>Requiems Loaded:</b>\n" +
                "- Kira (Striker/Flame)\n" +
                "- Mordren (Controller/Shadow)\n" +
                "- Elara (Support/Light)\n" +
                "- Thornwick (Tank/Nature)\n\n" +
                "<b>Shared Cards:</b> 8 neutral cards\n\n" +
                "<color=green>Ready to test. Press B to run.</color>";
            statusTMP.fontSize = 20;
            statusTMP.alignment = TextAlignmentOptions.TopLeft;
            statusTMP.color = Color.white;

            // === CardBalanceTest Component ===
            GameObject testObj = new GameObject("CardBalanceTest");
            var balanceTest = testObj.AddComponent<Testing.CardBalanceTest>();

            // Load and assign Requiem assets
            var requiems = new List<RequiemDataSO>();
            string[] requiemPaths = new[]
            {
                "Assets/_Project/Data/Characters/Requiems/Kira_Data.asset",
                "Assets/_Project/Data/Characters/Requiems/Mordren_Data.asset",
                "Assets/_Project/Data/Characters/Requiems/Elara_Data.asset",
                "Assets/_Project/Data/Characters/Requiems/Thornwick_Data.asset"
            };

            foreach (var path in requiemPaths)
            {
                var requiem = AssetDatabase.LoadAssetAtPath<RequiemDataSO>(path);
                if (requiem != null)
                {
                    requiems.Add(requiem);
                    Debug.Log($"[CardBalanceTestSceneGenerator] Loaded: {requiem.RequiemName}");
                }
                else
                {
                    Debug.LogWarning($"[CardBalanceTestSceneGenerator] Requiem not found at: {path}");
                }
            }

            // Load shared cards
            var sharedCards = new List<CardDataSO>();
            string sharedCardsPath = "Assets/_Project/Data/Cards/Shared";
            string[] sharedCardGuids = AssetDatabase.FindAssets("t:CardDataSO", new[] { sharedCardsPath });

            foreach (var guid in sharedCardGuids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var card = AssetDatabase.LoadAssetAtPath<CardDataSO>(path);
                if (card != null)
                {
                    sharedCards.Add(card);
                }
            }
            Debug.Log($"[CardBalanceTestSceneGenerator] Loaded {sharedCards.Count} shared cards");

            // Wire references via SerializedObject
            SerializedObject testSO = new SerializedObject(balanceTest);

            // Set Requiems array
            var requiemsProp = testSO.FindProperty("_requiems");
            requiemsProp.ClearArray();
            for (int i = 0; i < requiems.Count; i++)
            {
                requiemsProp.InsertArrayElementAtIndex(i);
                requiemsProp.GetArrayElementAtIndex(i).objectReferenceValue = requiems[i];
            }

            // Set Shared Cards array
            var sharedProp = testSO.FindProperty("_sharedCards");
            sharedProp.ClearArray();
            for (int i = 0; i < sharedCards.Count; i++)
            {
                sharedProp.InsertArrayElementAtIndex(i);
                sharedProp.GetArrayElementAtIndex(i).objectReferenceValue = sharedCards[i];
            }

            testSO.ApplyModifiedPropertiesWithoutUndo();

            // Save scene
            string scenePath = "Assets/_Project/Scenes/CardBalanceTest.unity";

            // Ensure Scenes directory exists
            if (!AssetDatabase.IsValidFolder("Assets/_Project/Scenes"))
            {
                AssetDatabase.CreateFolder("Assets/_Project", "Scenes");
            }

            EditorSceneManager.SaveScene(scene, scenePath);

            Debug.Log($"[CardBalanceTestSceneGenerator] Created card balance test scene at {scenePath}");
            Debug.Log($"  - {requiems.Count} Requiems loaded");
            Debug.Log($"  - {sharedCards.Count} Shared Cards loaded");
            Debug.Log("Press B to run balance tests");

            // Select the test object
            Selection.activeGameObject = testObj;
        }
    }
}
