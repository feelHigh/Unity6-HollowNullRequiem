// ============================================
// RequiemSelectionSceneGenerator.cs
// Editor tool to generate Requiem selection test scene
// ============================================

using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using TMPro;
using HNR.Characters;
using System.Collections.Generic;

namespace HNR.Editor
{
    public static class RequiemSelectionSceneGenerator
    {
        [MenuItem("HNR/Generate Requiem Selection Scene")]
        public static void GenerateRequiemSelectionScene()
        {
            // Create new scene
            var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

            // === Create UI Canvas ===
            GameObject canvasObj = new GameObject("SelectionCanvas");
            Canvas canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            canvasObj.AddComponent<GraphicRaycaster>();

            // === Background ===
            GameObject bgObj = new GameObject("Background");
            bgObj.transform.SetParent(canvasObj.transform, false);
            RectTransform bgRect = bgObj.AddComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.sizeDelta = Vector2.zero;
            Image bgImage = bgObj.AddComponent<Image>();
            bgImage.color = new Color(0.05f, 0.05f, 0.1f);

            // === Title ===
            GameObject titleObj = CreateText(canvasObj, "TitleText", "SELECT YOUR TEAM", 48);
            RectTransform titleRect = titleObj.GetComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0.5f, 1);
            titleRect.anchorMax = new Vector2(0.5f, 1);
            titleRect.anchoredPosition = new Vector2(0, -60);
            titleRect.sizeDelta = new Vector2(600, 80);
            var titleTMP = titleObj.GetComponent<TextMeshProUGUI>();
            titleTMP.fontStyle = FontStyles.Bold;

            // === Subtitle ===
            GameObject subtitleObj = CreateText(canvasObj, "SubtitleText", "Choose 3 Requiems to form your team", 24);
            RectTransform subtitleRect = subtitleObj.GetComponent<RectTransform>();
            subtitleRect.anchorMin = new Vector2(0.5f, 1);
            subtitleRect.anchorMax = new Vector2(0.5f, 1);
            subtitleRect.anchoredPosition = new Vector2(0, -110);
            subtitleRect.sizeDelta = new Vector2(600, 40);
            var subtitleTMP = subtitleObj.GetComponent<TextMeshProUGUI>();
            subtitleTMP.color = new Color(0.7f, 0.7f, 0.7f);

            // === Slot Container ===
            GameObject slotContainer = new GameObject("SlotContainer");
            slotContainer.transform.SetParent(canvasObj.transform, false);
            RectTransform slotContainerRect = slotContainer.AddComponent<RectTransform>();
            slotContainerRect.anchorMin = new Vector2(0.5f, 0.5f);
            slotContainerRect.anchorMax = new Vector2(0.5f, 0.5f);
            slotContainerRect.anchoredPosition = new Vector2(0, 50);
            slotContainerRect.sizeDelta = new Vector2(1200, 400);

            // Add HorizontalLayoutGroup
            var hlg = slotContainer.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 30;
            hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.childControlWidth = false;
            hlg.childControlHeight = false;
            hlg.childForceExpandWidth = false;
            hlg.childForceExpandHeight = false;

            // === Create Slot Prefab ===
            GameObject slotPrefab = CreateSlotPrefab();

            // === Preview Panel ===
            GameObject previewPanel = CreatePreviewPanel(canvasObj);

            // === Bottom Bar ===
            GameObject bottomBar = new GameObject("BottomBar");
            bottomBar.transform.SetParent(canvasObj.transform, false);
            RectTransform bottomBarRect = bottomBar.AddComponent<RectTransform>();
            bottomBarRect.anchorMin = new Vector2(0, 0);
            bottomBarRect.anchorMax = new Vector2(1, 0);
            bottomBarRect.anchoredPosition = new Vector2(0, 80);
            bottomBarRect.sizeDelta = new Vector2(0, 160);
            Image bottomBarImg = bottomBar.AddComponent<Image>();
            bottomBarImg.color = new Color(0.1f, 0.1f, 0.15f, 0.9f);

            // Selected Count Text
            GameObject selectedCountObj = CreateText(bottomBar, "SelectedCountText", "0/3", 36);
            RectTransform selectedCountRect = selectedCountObj.GetComponent<RectTransform>();
            selectedCountRect.anchorMin = new Vector2(0, 0.5f);
            selectedCountRect.anchorMax = new Vector2(0, 0.5f);
            selectedCountRect.anchoredPosition = new Vector2(150, 0);
            selectedCountRect.sizeDelta = new Vector2(100, 50);

            // Team Stats
            GameObject teamHPObj = CreateText(bottomBar, "TeamHPText", "Team HP: 0", 20);
            RectTransform teamHPRect = teamHPObj.GetComponent<RectTransform>();
            teamHPRect.anchorMin = new Vector2(0, 0.5f);
            teamHPRect.anchorMax = new Vector2(0, 0.5f);
            teamHPRect.anchoredPosition = new Vector2(350, 20);

            GameObject teamATKObj = CreateText(bottomBar, "TeamATKText", "Team ATK: 0", 20);
            RectTransform teamATKRect = teamATKObj.GetComponent<RectTransform>();
            teamATKRect.anchorMin = new Vector2(0, 0.5f);
            teamATKRect.anchorMax = new Vector2(0, 0.5f);
            teamATKRect.anchoredPosition = new Vector2(350, -10);

            GameObject teamDEFObj = CreateText(bottomBar, "TeamDEFText", "Team DEF: 0", 20);
            RectTransform teamDEFRect = teamDEFObj.GetComponent<RectTransform>();
            teamDEFRect.anchorMin = new Vector2(0, 0.5f);
            teamDEFRect.anchorMax = new Vector2(0, 0.5f);
            teamDEFRect.anchoredPosition = new Vector2(350, -40);

            // Start Run Button
            GameObject startButtonObj = CreateButton(bottomBar, "StartRunButton", "START RUN");
            RectTransform startButtonRect = startButtonObj.GetComponent<RectTransform>();
            startButtonRect.anchorMin = new Vector2(1, 0.5f);
            startButtonRect.anchorMax = new Vector2(1, 0.5f);
            startButtonRect.anchoredPosition = new Vector2(-150, 0);
            startButtonRect.sizeDelta = new Vector2(200, 60);
            var startButton = startButtonObj.GetComponent<Button>();
            startButton.interactable = false;

            // Back Button
            GameObject backButtonObj = CreateButton(bottomBar, "BackButton", "BACK");
            RectTransform backButtonRect = backButtonObj.GetComponent<RectTransform>();
            backButtonRect.anchorMin = new Vector2(1, 0.5f);
            backButtonRect.anchorMax = new Vector2(1, 0.5f);
            backButtonRect.anchoredPosition = new Vector2(-350, 0);
            backButtonRect.sizeDelta = new Vector2(120, 50);
            var backButtonImg = backButtonObj.GetComponent<Image>();
            backButtonImg.color = new Color(0.3f, 0.3f, 0.3f);

            // === RequiemSelectionScreen Component ===
            GameObject screenObj = new GameObject("RequiemSelectionScreen");
            screenObj.transform.SetParent(canvasObj.transform, false);
            var selectionScreen = screenObj.AddComponent<UI.RequiemSelectionScreen>();

            // Load Requiem assets
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
                    Debug.Log($"[RequiemSelectionSceneGenerator] Loaded: {requiem.RequiemName}");
                }
            }

            // Wire references
            SerializedObject screenSO = new SerializedObject(selectionScreen);
            screenSO.FindProperty("_slotContainer").objectReferenceValue = slotContainer.transform;
            screenSO.FindProperty("_slotPrefab").objectReferenceValue = slotPrefab.GetComponent<UI.RequiemSlotUI>();
            screenSO.FindProperty("_startRunButton").objectReferenceValue = startButton;
            screenSO.FindProperty("_selectedCountText").objectReferenceValue = selectedCountObj.GetComponent<TextMeshProUGUI>();
            screenSO.FindProperty("_backButton").objectReferenceValue = backButtonObj.GetComponent<Button>();
            screenSO.FindProperty("_previewPanel").objectReferenceValue = previewPanel;
            screenSO.FindProperty("_teamHPText").objectReferenceValue = teamHPObj.GetComponent<TextMeshProUGUI>();
            screenSO.FindProperty("_teamATKText").objectReferenceValue = teamATKObj.GetComponent<TextMeshProUGUI>();
            screenSO.FindProperty("_teamDEFText").objectReferenceValue = teamDEFObj.GetComponent<TextMeshProUGUI>();

            // Wire preview panel children
            var previewNameText = previewPanel.transform.Find("PreviewNameText");
            var previewTitleText = previewPanel.transform.Find("PreviewTitleText");
            var previewClassText = previewPanel.transform.Find("PreviewClassText");
            var previewStatsText = previewPanel.transform.Find("PreviewStatsText");
            var previewBackstoryText = previewPanel.transform.Find("PreviewBackstoryText");

            if (previewNameText != null)
                screenSO.FindProperty("_previewNameText").objectReferenceValue = previewNameText.GetComponent<TextMeshProUGUI>();
            if (previewTitleText != null)
                screenSO.FindProperty("_previewTitleText").objectReferenceValue = previewTitleText.GetComponent<TextMeshProUGUI>();
            if (previewClassText != null)
                screenSO.FindProperty("_previewClassText").objectReferenceValue = previewClassText.GetComponent<TextMeshProUGUI>();
            if (previewStatsText != null)
                screenSO.FindProperty("_previewStatsText").objectReferenceValue = previewStatsText.GetComponent<TextMeshProUGUI>();
            if (previewBackstoryText != null)
                screenSO.FindProperty("_previewBackstoryText").objectReferenceValue = previewBackstoryText.GetComponent<TextMeshProUGUI>();

            // Set Requiems array
            var requiemsProp = screenSO.FindProperty("_availableRequiems");
            requiemsProp.ClearArray();
            for (int i = 0; i < requiems.Count; i++)
            {
                requiemsProp.InsertArrayElementAtIndex(i);
                requiemsProp.GetArrayElementAtIndex(i).objectReferenceValue = requiems[i];
            }

            screenSO.ApplyModifiedPropertiesWithoutUndo();

            // Save slot prefab
            string prefabDir = "Assets/_Project/Prefabs/UI";
            if (!AssetDatabase.IsValidFolder(prefabDir))
            {
                AssetDatabase.CreateFolder("Assets/_Project/Prefabs", "UI");
            }
            string slotPrefabPath = $"{prefabDir}/RequiemSlotUI.prefab";
            PrefabUtility.SaveAsPrefabAsset(slotPrefab, slotPrefabPath);
            Object.DestroyImmediate(slotPrefab);

            // Re-assign prefab reference
            var savedPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(slotPrefabPath);
            screenSO = new SerializedObject(selectionScreen);
            screenSO.FindProperty("_slotPrefab").objectReferenceValue = savedPrefab.GetComponent<UI.RequiemSlotUI>();
            screenSO.ApplyModifiedPropertiesWithoutUndo();

            // Save scene
            string scenePath = "Assets/_Project/Scenes/RequiemSelectionTest.unity";
            if (!AssetDatabase.IsValidFolder("Assets/_Project/Scenes"))
            {
                AssetDatabase.CreateFolder("Assets/_Project", "Scenes");
            }
            EditorSceneManager.SaveScene(scene, scenePath);

            Debug.Log($"[RequiemSelectionSceneGenerator] Created scene at {scenePath}");
            Debug.Log($"[RequiemSelectionSceneGenerator] Created prefab at {slotPrefabPath}");
            Debug.Log($"  - {requiems.Count} Requiems loaded");

            Selection.activeGameObject = screenObj;
        }

        private static GameObject CreateSlotPrefab()
        {
            GameObject slot = new GameObject("RequiemSlotUI");
            RectTransform slotRect = slot.AddComponent<RectTransform>();
            slotRect.sizeDelta = new Vector2(250, 350);

            // Background
            GameObject bg = new GameObject("Background");
            bg.transform.SetParent(slot.transform, false);
            RectTransform bgRect = bg.AddComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.sizeDelta = Vector2.zero;
            Image bgImg = bg.AddComponent<Image>();
            bgImg.color = new Color(0.15f, 0.15f, 0.2f);

            // Portrait
            GameObject portrait = new GameObject("Portrait");
            portrait.transform.SetParent(slot.transform, false);
            RectTransform portraitRect = portrait.AddComponent<RectTransform>();
            portraitRect.anchorMin = new Vector2(0.5f, 1);
            portraitRect.anchorMax = new Vector2(0.5f, 1);
            portraitRect.anchoredPosition = new Vector2(0, -100);
            portraitRect.sizeDelta = new Vector2(150, 150);
            Image portraitImg = portrait.AddComponent<Image>();
            portraitImg.color = new Color(0.3f, 0.3f, 0.4f);

            // Name
            GameObject nameObj = CreateText(slot, "NameText", "Requiem Name", 22);
            RectTransform nameRect = nameObj.GetComponent<RectTransform>();
            nameRect.anchorMin = new Vector2(0.5f, 1);
            nameRect.anchorMax = new Vector2(0.5f, 1);
            nameRect.anchoredPosition = new Vector2(0, -200);
            nameRect.sizeDelta = new Vector2(230, 30);
            var nameTMP = nameObj.GetComponent<TextMeshProUGUI>();
            nameTMP.fontStyle = FontStyles.Bold;

            // Class
            GameObject classObj = CreateText(slot, "ClassText", "Class", 16);
            RectTransform classRect = classObj.GetComponent<RectTransform>();
            classRect.anchorMin = new Vector2(0.5f, 1);
            classRect.anchorMax = new Vector2(0.5f, 1);
            classRect.anchoredPosition = new Vector2(0, -230);
            classRect.sizeDelta = new Vector2(230, 25);
            var classTMP = classObj.GetComponent<TextMeshProUGUI>();
            classTMP.color = new Color(0.7f, 0.7f, 0.7f);

            // Stats
            GameObject statsObj = CreateText(slot, "StatsText", "HP:0  ATK:0  DEF:0", 14);
            RectTransform statsRect = statsObj.GetComponent<RectTransform>();
            statsRect.anchorMin = new Vector2(0.5f, 0);
            statsRect.anchorMax = new Vector2(0.5f, 0);
            statsRect.anchoredPosition = new Vector2(0, 50);
            statsRect.sizeDelta = new Vector2(230, 25);
            var statsTMP = statsObj.GetComponent<TextMeshProUGUI>();
            statsTMP.color = new Color(0.8f, 0.8f, 0.8f);

            // Selected Overlay
            GameObject selectedOverlay = new GameObject("SelectedOverlay");
            selectedOverlay.transform.SetParent(slot.transform, false);
            RectTransform overlayRect = selectedOverlay.AddComponent<RectTransform>();
            overlayRect.anchorMin = Vector2.zero;
            overlayRect.anchorMax = Vector2.one;
            overlayRect.sizeDelta = Vector2.zero;
            Image overlayImg = selectedOverlay.AddComponent<Image>();
            overlayImg.color = new Color(0f, 0.8f, 0.4f, 0.3f);
            selectedOverlay.SetActive(false);

            // Selected Checkmark
            GameObject checkmark = CreateText(slot, "SelectedCheckmark", "✓", 48);
            RectTransform checkRect = checkmark.GetComponent<RectTransform>();
            checkRect.anchorMin = new Vector2(1, 1);
            checkRect.anchorMax = new Vector2(1, 1);
            checkRect.anchoredPosition = new Vector2(-25, -25);
            checkRect.sizeDelta = new Vector2(50, 50);
            var checkTMP = checkmark.GetComponent<TextMeshProUGUI>();
            checkTMP.color = new Color(0.3f, 1f, 0.3f);
            checkmark.SetActive(false);

            // Add RequiemSlotUI component
            var slotUI = slot.AddComponent<UI.RequiemSlotUI>();

            // Wire references
            SerializedObject slotSO = new SerializedObject(slotUI);
            slotSO.FindProperty("_portrait").objectReferenceValue = portraitImg;
            slotSO.FindProperty("_background").objectReferenceValue = bgImg;
            slotSO.FindProperty("_nameText").objectReferenceValue = nameObj.GetComponent<TextMeshProUGUI>();
            slotSO.FindProperty("_classText").objectReferenceValue = classObj.GetComponent<TextMeshProUGUI>();
            slotSO.FindProperty("_statsText").objectReferenceValue = statsObj.GetComponent<TextMeshProUGUI>();
            slotSO.FindProperty("_selectedOverlay").objectReferenceValue = selectedOverlay;
            slotSO.FindProperty("_selectedCheckmark").objectReferenceValue = checkmark;
            slotSO.ApplyModifiedPropertiesWithoutUndo();

            return slot;
        }

        private static GameObject CreatePreviewPanel(GameObject parent)
        {
            GameObject panel = new GameObject("PreviewPanel");
            panel.transform.SetParent(parent.transform, false);
            RectTransform panelRect = panel.AddComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(1, 0.5f);
            panelRect.anchorMax = new Vector2(1, 0.5f);
            panelRect.anchoredPosition = new Vector2(-200, 50);
            panelRect.sizeDelta = new Vector2(350, 500);
            Image panelImg = panel.AddComponent<Image>();
            panelImg.color = new Color(0.1f, 0.1f, 0.15f, 0.95f);

            // Preview Name
            GameObject nameObj = CreateText(panel, "PreviewNameText", "Character Name", 28);
            RectTransform nameRect = nameObj.GetComponent<RectTransform>();
            nameRect.anchorMin = new Vector2(0.5f, 1);
            nameRect.anchorMax = new Vector2(0.5f, 1);
            nameRect.anchoredPosition = new Vector2(0, -30);
            nameRect.sizeDelta = new Vector2(320, 40);
            var nameTMP = nameObj.GetComponent<TextMeshProUGUI>();
            nameTMP.fontStyle = FontStyles.Bold;

            // Preview Title
            GameObject titleObj = CreateText(panel, "PreviewTitleText", "Title", 18);
            RectTransform titleRect = titleObj.GetComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0.5f, 1);
            titleRect.anchorMax = new Vector2(0.5f, 1);
            titleRect.anchoredPosition = new Vector2(0, -60);
            titleRect.sizeDelta = new Vector2(320, 25);
            var titleTMP = titleObj.GetComponent<TextMeshProUGUI>();
            titleTMP.color = new Color(0.8f, 0.7f, 0.3f);

            // Preview Class
            GameObject classObj = CreateText(panel, "PreviewClassText", "Class - Aspect", 16);
            RectTransform classRect = classObj.GetComponent<RectTransform>();
            classRect.anchorMin = new Vector2(0.5f, 1);
            classRect.anchorMax = new Vector2(0.5f, 1);
            classRect.anchoredPosition = new Vector2(0, -90);
            classRect.sizeDelta = new Vector2(320, 25);

            // Preview Stats
            GameObject statsObj = CreateText(panel, "PreviewStatsText", "HP: 0\nATK: 0\nDEF: 0\nSE Rate: 1.0x", 16);
            RectTransform statsRect = statsObj.GetComponent<RectTransform>();
            statsRect.anchorMin = new Vector2(0.5f, 1);
            statsRect.anchorMax = new Vector2(0.5f, 1);
            statsRect.anchoredPosition = new Vector2(0, -160);
            statsRect.sizeDelta = new Vector2(320, 100);
            var statsTMP = statsObj.GetComponent<TextMeshProUGUI>();
            statsTMP.alignment = TextAlignmentOptions.TopLeft;

            // Preview Backstory
            GameObject backstoryObj = CreateText(panel, "PreviewBackstoryText", "Backstory text...", 14);
            RectTransform backstoryRect = backstoryObj.GetComponent<RectTransform>();
            backstoryRect.anchorMin = new Vector2(0.5f, 0);
            backstoryRect.anchorMax = new Vector2(0.5f, 0);
            backstoryRect.anchoredPosition = new Vector2(0, 120);
            backstoryRect.sizeDelta = new Vector2(320, 150);
            var backstoryTMP = backstoryObj.GetComponent<TextMeshProUGUI>();
            backstoryTMP.alignment = TextAlignmentOptions.TopLeft;
            backstoryTMP.color = new Color(0.7f, 0.7f, 0.7f);
            backstoryTMP.fontStyle = FontStyles.Italic;

            panel.SetActive(false);
            return panel;
        }

        private static GameObject CreateText(GameObject parent, string name, string text, int fontSize)
        {
            GameObject obj = new GameObject(name);
            obj.transform.SetParent(parent.transform, false);
            obj.AddComponent<RectTransform>().sizeDelta = new Vector2(200, 50);
            TextMeshProUGUI tmp = obj.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = Color.white;
            return obj;
        }

        private static GameObject CreateButton(GameObject parent, string name, string text)
        {
            GameObject obj = new GameObject(name);
            obj.transform.SetParent(parent.transform, false);
            obj.AddComponent<RectTransform>();
            Image img = obj.AddComponent<Image>();
            img.color = new Color(0.2f, 0.5f, 0.3f);
            Button btn = obj.AddComponent<Button>();
            btn.targetGraphic = img;

            GameObject textObj = CreateText(obj, "Text", text, 20);
            RectTransform textRect = textObj.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.sizeDelta = Vector2.zero;
            var textTMP = textObj.GetComponent<TextMeshProUGUI>();
            textTMP.fontStyle = FontStyles.Bold;

            return obj;
        }
    }
}
