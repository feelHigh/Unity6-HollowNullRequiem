// ============================================
// Phase2SceneCompleter.cs
// Completes Phase 2 scene UI wiring
// ============================================

#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using TMPro;
using HNR.Map;
using HNR.Combat;
using HNR.UI;
using HNR.UI.Screens;
using HNR.UI.Combat;

namespace HNR.Editor
{
    /// <summary>
    /// Completes Phase 2 scene UI wiring for production scenes.
    /// Adds missing components and overlay screens.
    /// </summary>
    public static class Phase2SceneCompleter
    {
        private const string ScenesPath = "Assets/_Project/Scenes/";

        // ============================================
        // Public API
        // ============================================

        /// <summary>
        /// Completes all Phase 2 scene wiring.
        /// </summary>
        public static void CompleteAllScenes()
        {
            var currentScene = SceneManager.GetActiveScene().path;

            bool nullRiftFixed = CompleteNullRiftScene();
            bool combatFixed = CompleteCombatScene();

            if (!string.IsNullOrEmpty(currentScene))
            {
                EditorSceneManager.OpenScene(currentScene);
            }

            string message = "Phase 2 Scene Completion:\n\n";
            message += $"NullRift: {(nullRiftFixed ? "Fixed" : "No changes needed")}\n";
            message += $"Combat: {(combatFixed ? "Fixed" : "No changes needed")}";

            Debug.Log($"[Phase2SceneCompleter] {message}");
            EditorUtility.DisplayDialog("Phase 2 Complete", message, "OK");
        }

        /// <summary>
        /// Completes NullRift scene - adds SanctuaryScreen overlay.
        /// </summary>
        public static bool CompleteNullRiftScene()
        {
            string scenePath = ScenesPath + "NullRift.unity";
            if (!System.IO.File.Exists(scenePath))
            {
                Debug.LogError("[Phase2SceneCompleter] NullRift scene not found");
                return false;
            }

            var scene = EditorSceneManager.OpenScene(scenePath);
            bool modified = false;

            // Fix MapPathRenderer (delegate to SceneWiringFixer)
            modified |= SceneWiringFixer.FixNullRiftScene();

            // Add SanctuaryScreen if missing
            modified |= EnsureSanctuaryScreen();

            if (modified)
            {
                EditorSceneManager.MarkSceneDirty(scene);
                EditorSceneManager.SaveScene(scene);
                Debug.Log("[Phase2SceneCompleter] NullRift scene completed");
            }

            return modified;
        }

        /// <summary>
        /// Completes Combat scene - adds UI components and overlay screens.
        /// </summary>
        public static bool CompleteCombatScene()
        {
            string scenePath = ScenesPath + "Combat.unity";
            if (!System.IO.File.Exists(scenePath))
            {
                Debug.LogError("[Phase2SceneCompleter] Combat scene not found");
                return false;
            }

            var scene = EditorSceneManager.OpenScene(scenePath);
            bool modified = false;

            // Fix basic wiring (delegate to SceneWiringFixer)
            modified |= SceneWiringFixer.FixCombatScene();

            // Add combat UI components
            modified |= EnsureCardFanLayout();
            modified |= EnsurePartyStatusSidebar();
            modified |= EnsureAPCounterDisplay();

            // Add overlay screens
            modified |= EnsureNullStateModal();
            modified |= EnsureResultsScreen();

            // Wire CombatScreen references
            modified |= WireCombatScreenReferences();

            if (modified)
            {
                EditorSceneManager.MarkSceneDirty(scene);
                EditorSceneManager.SaveScene(scene);
                Debug.Log("[Phase2SceneCompleter] Combat scene completed");
            }

            return modified;
        }

        // ============================================
        // NullRift Components
        // ============================================

        private static bool EnsureSanctuaryScreen()
        {
            if (Object.FindAnyObjectByType<SanctuaryScreen>() != null)
                return false;

            var overlayContainer = GameObject.Find("OverlayContainer");
            if (overlayContainer == null)
            {
                Debug.LogWarning("[Phase2SceneCompleter] OverlayContainer not found in NullRift");
                return false;
            }

            var sanctuaryGO = CreateOverlayScreen("SanctuaryScreen", overlayContainer.transform);
            sanctuaryGO.AddComponent<SanctuaryScreen>();

            // Add basic UI structure
            CreateScreenTitle(sanctuaryGO.transform, "Sanctuary");
            CreateScreenDescription(sanctuaryGO.transform, "Rest and recover your strength...");

            var buttonContainer = CreateButtonContainer(sanctuaryGO.transform);
            CreateButton(buttonContainer.transform, "RestButton", "Rest (Heal 30%)");
            CreateButton(buttonContainer.transform, "UpgradeCardButton", "Upgrade Card");
            CreateButton(buttonContainer.transform, "LeaveButton", "Leave");

            sanctuaryGO.SetActive(false);
            Debug.Log("[Phase2SceneCompleter] Added SanctuaryScreen");
            return true;
        }

        // ============================================
        // Combat UI Components
        // ============================================

        private static bool EnsureCardFanLayout()
        {
            if (Object.FindAnyObjectByType<CardFanLayout>() != null)
                return false;

            var handContainer = GameObject.Find("HandContainer");
            if (handContainer == null)
            {
                Debug.LogWarning("[Phase2SceneCompleter] HandContainer not found");
                return false;
            }

            handContainer.AddComponent<CardFanLayout>();
            Debug.Log("[Phase2SceneCompleter] Added CardFanLayout to HandContainer");
            return true;
        }

        private static bool EnsurePartyStatusSidebar()
        {
            if (Object.FindAnyObjectByType<PartyStatusSidebar>() != null)
                return false;

            var partySidebar = GameObject.Find("PartySidebar");
            if (partySidebar == null)
            {
                Debug.LogWarning("[Phase2SceneCompleter] PartySidebar not found");
                return false;
            }

            var sidebar = partySidebar.AddComponent<PartyStatusSidebar>();

            // Wire party slots
            var serialized = new SerializedObject(sidebar);
            var slotsProperty = serialized.FindProperty("_partySlots");

            if (slotsProperty != null)
            {
                var slot0 = GameObject.Find("PartySlot_0");
                var slot1 = GameObject.Find("PartySlot_1");
                var slot2 = GameObject.Find("PartySlot_2");

                slotsProperty.arraySize = 3;
                if (slot0 != null) slotsProperty.GetArrayElementAtIndex(0).objectReferenceValue = slot0.GetComponent<RectTransform>();
                if (slot1 != null) slotsProperty.GetArrayElementAtIndex(1).objectReferenceValue = slot1.GetComponent<RectTransform>();
                if (slot2 != null) slotsProperty.GetArrayElementAtIndex(2).objectReferenceValue = slot2.GetComponent<RectTransform>();

                serialized.ApplyModifiedProperties();
            }

            Debug.Log("[Phase2SceneCompleter] Added PartyStatusSidebar");
            return true;
        }

        private static bool EnsureAPCounterDisplay()
        {
            if (Object.FindAnyObjectByType<APCounterDisplay>() != null)
                return false;

            var apCounter = GameObject.Find("APCounter");
            if (apCounter == null)
            {
                Debug.LogWarning("[Phase2SceneCompleter] APCounter not found");
                return false;
            }

            var display = apCounter.AddComponent<APCounterDisplay>();

            // Wire references
            var serialized = new SerializedObject(display);

            var apNumber = GameObject.Find("APNumber");
            if (apNumber != null)
            {
                var tmp = apNumber.GetComponent<TextMeshProUGUI>();
                var apTextProp = serialized.FindProperty("_apText");
                if (apTextProp != null && tmp != null)
                    apTextProp.objectReferenceValue = tmp;
            }

            var apLabel = GameObject.Find("APLabel");
            if (apLabel != null)
            {
                var tmp = apLabel.GetComponent<TextMeshProUGUI>();
                var labelProp = serialized.FindProperty("_apLabel");
                if (labelProp != null && tmp != null)
                    labelProp.objectReferenceValue = tmp;
            }

            serialized.ApplyModifiedProperties();
            Debug.Log("[Phase2SceneCompleter] Added APCounterDisplay");
            return true;
        }

        // ============================================
        // Combat Overlay Screens
        // ============================================

        private static bool EnsureNullStateModal()
        {
            if (Object.FindAnyObjectByType<NullStateModal>() != null)
                return false;

            var overlayContainer = GameObject.Find("ScreenContainer");
            if (overlayContainer == null)
            {
                overlayContainer = GameObject.Find("CombatCanvas");
            }
            if (overlayContainer == null)
            {
                Debug.LogWarning("[Phase2SceneCompleter] No container found for NullStateModal");
                return false;
            }

            var modalGO = CreateOverlayScreen("NullStateModal", overlayContainer.transform);
            modalGO.AddComponent<NullStateModal>();

            // Add modal UI structure
            var panel = CreatePanel(modalGO.transform, "ModalPanel", new Color(0.1f, 0.05f, 0.15f, 0.95f));
            var panelRect = panel.GetComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.15f, 0.2f);
            panelRect.anchorMax = new Vector2(0.85f, 0.8f);
            panelRect.offsetMin = Vector2.zero;
            panelRect.offsetMax = Vector2.zero;

            CreateScreenTitle(panel.transform, "NULL STATE");
            CreateScreenDescription(panel.transform, "A Requiem has entered the Null State!\nChoose how to channel this power...");

            var buttonContainer = CreateButtonContainer(panel.transform);
            CreateButton(buttonContainer.transform, "OverdriveButton", "Overdrive (2x Damage)");
            CreateButton(buttonContainer.transform, "SacrificeButton", "Sacrifice (Heal Team)");

            modalGO.SetActive(false);
            Debug.Log("[Phase2SceneCompleter] Added NullStateModal");
            return true;
        }

        private static bool EnsureResultsScreen()
        {
            if (Object.FindAnyObjectByType<ResultsScreen>() != null)
                return false;

            var overlayContainer = GameObject.Find("ScreenContainer");
            if (overlayContainer == null)
            {
                overlayContainer = GameObject.Find("CombatCanvas");
            }
            if (overlayContainer == null)
            {
                Debug.LogWarning("[Phase2SceneCompleter] No container found for ResultsScreen");
                return false;
            }

            var resultsGO = CreateOverlayScreen("ResultsScreen", overlayContainer.transform);
            resultsGO.AddComponent<ResultsScreen>();

            // Add results UI structure
            CreateScreenTitle(resultsGO.transform, "VICTORY");

            var rewardsContainer = new GameObject("RewardsContainer");
            rewardsContainer.transform.SetParent(resultsGO.transform, false);
            var rewardsRect = rewardsContainer.AddComponent<RectTransform>();
            rewardsRect.anchorMin = new Vector2(0.2f, 0.3f);
            rewardsRect.anchorMax = new Vector2(0.8f, 0.7f);
            rewardsRect.offsetMin = Vector2.zero;
            rewardsRect.offsetMax = Vector2.zero;

            var buttonContainer = CreateButtonContainer(resultsGO.transform);
            CreateButton(buttonContainer.transform, "ContinueButton", "Continue");

            resultsGO.SetActive(false);
            Debug.Log("[Phase2SceneCompleter] Added ResultsScreen");
            return true;
        }

        // ============================================
        // Wire CombatScreen
        // ============================================

        private static bool WireCombatScreenReferences()
        {
            var combatScreen = Object.FindAnyObjectByType<CombatScreen>();
            if (combatScreen == null)
                return false;

            var serialized = new SerializedObject(combatScreen);
            bool modified = false;

            // Wire _partySidebar
            var sidebarProp = serialized.FindProperty("_partySidebar");
            if (sidebarProp != null && sidebarProp.objectReferenceValue == null)
            {
                var sidebar = Object.FindAnyObjectByType<PartyStatusSidebar>();
                if (sidebar != null)
                {
                    sidebarProp.objectReferenceValue = sidebar;
                    modified = true;
                }
            }

            // Wire _cardFanLayout
            var fanProp = serialized.FindProperty("_cardFanLayout");
            if (fanProp != null && fanProp.objectReferenceValue == null)
            {
                var fan = Object.FindAnyObjectByType<CardFanLayout>();
                if (fan != null)
                {
                    fanProp.objectReferenceValue = fan;
                    modified = true;
                }
            }

            // Wire _apCounter
            var apProp = serialized.FindProperty("_apCounter");
            if (apProp != null && apProp.objectReferenceValue == null)
            {
                var ap = Object.FindAnyObjectByType<APCounterDisplay>();
                if (ap != null)
                {
                    apProp.objectReferenceValue = ap;
                    modified = true;
                }
            }

            // Wire _executionButton
            var execProp = serialized.FindProperty("_executionButton");
            if (execProp != null && execProp.objectReferenceValue == null)
            {
                var exec = Object.FindAnyObjectByType<ExecutionButton>();
                if (exec != null)
                {
                    execProp.objectReferenceValue = exec;
                    modified = true;
                }
            }

            if (modified)
            {
                serialized.ApplyModifiedProperties();
                Debug.Log("[Phase2SceneCompleter] Wired CombatScreen references");
            }

            return modified;
        }

        // ============================================
        // UI Creation Helpers
        // ============================================

        private static GameObject CreateOverlayScreen(string name, Transform parent)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);

            var rect = go.AddComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            var image = go.AddComponent<Image>();
            image.color = new Color(0.02f, 0.01f, 0.05f, 0.9f);

            var canvas = go.AddComponent<Canvas>();
            canvas.overrideSorting = true;
            canvas.sortingOrder = 100;

            go.AddComponent<GraphicRaycaster>();

            return go;
        }

        private static GameObject CreatePanel(Transform parent, string name, Color color)
        {
            var panel = new GameObject(name);
            panel.transform.SetParent(parent, false);

            var rect = panel.AddComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            var image = panel.AddComponent<Image>();
            image.color = color;

            return panel;
        }

        private static void CreateScreenTitle(Transform parent, string titleText)
        {
            var titleGO = new GameObject("Title");
            titleGO.transform.SetParent(parent, false);

            var rect = titleGO.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.1f, 0.75f);
            rect.anchorMax = new Vector2(0.9f, 0.9f);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            var tmp = titleGO.AddComponent<TextMeshProUGUI>();
            tmp.text = titleText;
            tmp.fontSize = 48;
            tmp.fontStyle = FontStyles.Bold;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = new Color(0.9f, 0.8f, 1f);
        }

        private static void CreateScreenDescription(Transform parent, string descText)
        {
            var descGO = new GameObject("Description");
            descGO.transform.SetParent(parent, false);

            var rect = descGO.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.1f, 0.5f);
            rect.anchorMax = new Vector2(0.9f, 0.7f);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            var tmp = descGO.AddComponent<TextMeshProUGUI>();
            tmp.text = descText;
            tmp.fontSize = 24;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = new Color(0.7f, 0.7f, 0.8f);
        }

        private static GameObject CreateButtonContainer(Transform parent)
        {
            var container = new GameObject("ButtonContainer");
            container.transform.SetParent(parent, false);

            var rect = container.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.2f, 0.1f);
            rect.anchorMax = new Vector2(0.8f, 0.35f);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            var layout = container.AddComponent<VerticalLayoutGroup>();
            layout.spacing = 15;
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childControlHeight = true;
            layout.childControlWidth = true;
            layout.childForceExpandHeight = false;
            layout.childForceExpandWidth = true;

            return container;
        }

        private static GameObject CreateButton(Transform parent, string name, string text)
        {
            var buttonGO = new GameObject(name);
            buttonGO.transform.SetParent(parent, false);

            var rect = buttonGO.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(300, 50);

            var image = buttonGO.AddComponent<Image>();
            image.color = new Color(0.3f, 0.2f, 0.4f);

            var button = buttonGO.AddComponent<Button>();
            var colors = button.colors;
            colors.highlightedColor = new Color(0.5f, 0.3f, 0.6f);
            colors.pressedColor = new Color(0.2f, 0.1f, 0.3f);
            button.colors = colors;

            var textGO = new GameObject("Text");
            textGO.transform.SetParent(buttonGO.transform, false);

            var textRect = textGO.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;

            var tmp = textGO.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = 20;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = Color.white;

            var layoutElement = buttonGO.AddComponent<LayoutElement>();
            layoutElement.preferredHeight = 50;

            return buttonGO;
        }
    }
}
#endif
