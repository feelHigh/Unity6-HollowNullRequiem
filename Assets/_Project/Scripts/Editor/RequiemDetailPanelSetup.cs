// ============================================
// RequiemDetailPanelSetup.cs
// Editor tool to set up RequiemDetailPanel with LayerLab UI styling
// ============================================

using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using TMPro;
using HNR.UI.Components;
using HNR.UI.Config;

namespace HNR.Editor
{
    /// <summary>
    /// Editor tool for setting up RequiemDetailPanel with dark theme styling.
    /// Key approach: Modify EXISTING Image components directly rather than adding child backgrounds.
    /// </summary>
    public static class RequiemDetailPanelSetup
    {
        private const string RequiemsScenePath = "Assets/_Project/Scenes/Requiems.unity";
        private const string ConfigPath = "Assets/_Project/Data/Config/LayerLabSpriteConfig.asset";

        // Tab button sprites
        private const string TabBgPath = "Assets/ThirdParty/Layer Lab/GUI Pro-FantasyHero/ResourcesData/Sptites/Components/Button/Tab_Middle_02_Bg.png";
        private const string TabGlowPath = "Assets/ThirdParty/Layer Lab/GUI Pro-FantasyHero/ResourcesData/Sptites/Components/Button/Tab_Middle_02_Glow.png";
        private const string TabBorderPath = "Assets/ThirdParty/Layer Lab/GUI Pro-FantasyHero/ResourcesData/Sptites/Components/Button/Tab_Middle_02_Border.png";
        private const string TabFocusGlowPath = "Assets/ThirdParty/Layer Lab/GUI Pro-FantasyHero/ResourcesData/Sptites/Components/Button/Tab_Middle_02_FocusGlow.png";
        private const string TabFocusPath = "Assets/ThirdParty/Layer Lab/GUI Pro-FantasyHero/ResourcesData/Sptites/Components/Button/Tab_Middle_02_Focus.png";

        // ============================================
        // Colors (dark purple theme)
        // ============================================

        // Main panel background
        private static readonly Color PanelBackgroundColor = new Color(0.08f, 0.05f, 0.12f, 1f); // Very dark purple

        // Dimmed overlay
        private static readonly Color DimmedOverlayColor = new Color(0.04f, 0.02f, 0.08f, 0.90f);

        // Sidebar background
        private static readonly Color SidebarBgColor = new Color(0.10f, 0.07f, 0.15f, 1f);

        // Stats panel - solid dark color, NO sprite
        private static readonly Color StatsPanelColor = new Color(0.12f, 0.08f, 0.18f, 1f);

        // Character art area - slightly different dark
        private static readonly Color CharacterAreaColor = new Color(0.06f, 0.04f, 0.10f, 1f);

        // Cards tab content - dark to match panel
        private static readonly Color CardsContentColor = new Color(0.08f, 0.05f, 0.12f, 1f);

        // Tab button colors
        private static readonly Color TabNormalTint = new Color(0.45f, 0.35f, 0.65f, 1f);
        private static readonly Color TabFocusGlowColor = new Color(0.55f, 0.35f, 0.75f, 0.6f);

        // Text colors
        private static readonly Color TextPrimaryColor = Color.white;
        private static readonly Color TextSecondaryColor = new Color(0.7f, 0.6f, 0.85f, 1f);

        // ============================================
        // Menu Items
        // ============================================

        [MenuItem("HNR/4. Scenes/Requiems/Setup RequiemDetailPanel LayerLab", priority = 118)]
        public static void SetupRequiemDetailPanel()
        {
            RunSetup(cleanFirst: false);
        }

        [MenuItem("HNR/4. Scenes/Requiems/Redesign RequiemDetailPanel (Complete)", priority = 119)]
        public static void RedesignRequiemDetailPanelComplete()
        {
            RunSetup(cleanFirst: true);
        }

        [MenuItem("HNR/4. Scenes/Requiems/Clean RequiemDetailPanel (Remove Added Layers)", priority = 120)]
        public static void CleanRequiemDetailPanel()
        {
            if (!OpenRequiemsScene()) return;

            var detailPanel = FindRequiemDetailPanel();
            if (detailPanel == null)
            {
                EditorUtility.DisplayDialog("Error", "Could not find RequiemDetailPanel in scene.", "OK");
                return;
            }

            int removed = RemoveAddedLayers(detailPanel);
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            EditorUtility.DisplayDialog("Clean Complete", $"Removed {removed} added layers.\n\nRemember to save the scene!", "OK");
        }

        private static bool OpenRequiemsScene()
        {
            var scene = EditorSceneManager.GetActiveScene();
            if (scene.path != RequiemsScenePath)
            {
                if (EditorUtility.DisplayDialog("Open Requiems Scene",
                    "This tool needs to open the Requiems scene. Continue?", "Yes", "Cancel"))
                {
                    EditorSceneManager.OpenScene(RequiemsScenePath);
                    return true;
                }
                return false;
            }
            return true;
        }

        private static void RunSetup(bool cleanFirst)
        {
            if (!OpenRequiemsScene()) return;

            var detailPanel = FindRequiemDetailPanel();
            if (detailPanel == null)
            {
                EditorUtility.DisplayDialog("Error", "Could not find RequiemDetailPanel in scene.", "OK");
                return;
            }

            int changesCount = 0;

            if (cleanFirst)
            {
                EditorUtility.DisplayProgressBar("Redesigning RequiemDetailPanel", "Cleaning old layers...", 0.05f);
                changesCount += RemoveAddedLayers(detailPanel);
            }

            // Setup dimmed overlay
            EditorUtility.DisplayProgressBar("Redesigning RequiemDetailPanel", "Setting up dimmed overlay...", 0.15f);
            changesCount += SetupDimmedOverlay(detailPanel);

            // Modify existing Image components directly
            EditorUtility.DisplayProgressBar("Redesigning RequiemDetailPanel", "Setting panel colors...", 0.30f);
            changesCount += SetupPanelContent(detailPanel);

            EditorUtility.DisplayProgressBar("Redesigning RequiemDetailPanel", "Setting sidebar colors...", 0.40f);
            changesCount += SetupSidebar(detailPanel);

            EditorUtility.DisplayProgressBar("Redesigning RequiemDetailPanel", "Setting character area...", 0.50f);
            changesCount += SetupCharacterArea(detailPanel);

            EditorUtility.DisplayProgressBar("Redesigning RequiemDetailPanel", "Setting stats panel...", 0.60f);
            changesCount += SetupStatsPanel(detailPanel);

            EditorUtility.DisplayProgressBar("Redesigning RequiemDetailPanel", "Setting cards content...", 0.70f);
            changesCount += SetupCardsContent(detailPanel);

            EditorUtility.DisplayProgressBar("Redesigning RequiemDetailPanel", "Setting up tabs...", 0.85f);
            changesCount += SetupTabs(detailPanel);

            EditorUtility.ClearProgressBar();

            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            EditorUtility.DisplayDialog("Setup Complete",
                $"RequiemDetailPanel redesign complete.\n{changesCount} changes made.\n\nRemember to save the scene!", "OK");
            Debug.Log($"[RequiemDetailPanelSetup] Setup complete with {changesCount} changes.");
        }

        // ============================================
        // Remove Added Layers
        // ============================================

        private static int RemoveAddedLayers(GameObject detailPanel)
        {
            int removed = 0;

            string[] layersToRemove = {
                // All previously added layers
                "DimmedOverlay",
                "PanelBg", "PanelBorder", "PanelInnerBorder", "PanelTopDeco",
                "TopGradient", "BottomGradient", "PanelInnerGlow",
                "SidebarBg", "SidebarBorder",
                "CharacterAreaBg", "CharacterBackDeco",
                "StatsPanelBg", "StatsPanelBorder",
                "ProfileFrameBg", "ProfileFrameBorder", "ProfileFrameDeco",
                "CardFrameBg", "CardFrameBorder",
                "ListFrameBg", "ListFrameBorder",
                "CardsTabBg"
            };

            foreach (string name in layersToRemove)
            {
                var layer = FindChildRecursiveIncludeInactive(detailPanel.transform, name);
                if (layer != null)
                {
                    Undo.DestroyObjectImmediate(layer.gameObject);
                    removed++;
                    Debug.Log($"[RequiemDetailPanelSetup] Removed {name}");
                }
            }

            return removed;
        }

        // ============================================
        // Setup Methods - Modify EXISTING components
        // ============================================

        private static int SetupDimmedOverlay(GameObject detailPanel)
        {
            if (FindChildRecursiveIncludeInactive(detailPanel.transform, "DimmedOverlay") != null)
                return 0;

            var overlay = new GameObject("DimmedOverlay");
            Undo.RegisterCreatedObjectUndo(overlay, "Create DimmedOverlay");
            overlay.transform.SetParent(detailPanel.transform, false);
            overlay.transform.SetAsFirstSibling();

            var rect = overlay.AddComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            var image = overlay.AddComponent<Image>();
            image.color = DimmedOverlayColor;
            image.raycastTarget = true;

            Debug.Log("[RequiemDetailPanelSetup] Added DimmedOverlay");
            return 1;
        }

        private static int SetupPanelContent(GameObject detailPanel)
        {
            var panelContent = FindChildRecursiveIncludeInactive(detailPanel.transform, "PanelContent");
            if (panelContent == null) return 0;

            var image = panelContent.GetComponent<Image>();
            if (image == null) return 0;

            Undo.RecordObject(image, "Update PanelContent");
            image.color = PanelBackgroundColor;
            image.sprite = null; // Remove any sprite, use solid color

            Debug.Log("[RequiemDetailPanelSetup] Updated PanelContent color");
            return 1;
        }

        private static int SetupSidebar(GameObject detailPanel)
        {
            int changes = 0;

            // Find LeftSidebar
            var leftSidebar = FindChildRecursiveIncludeInactive(detailPanel.transform, "LeftSidebar");
            if (leftSidebar != null)
            {
                var image = leftSidebar.GetComponent<Image>();
                if (image != null)
                {
                    Undo.RecordObject(image, "Update LeftSidebar");
                    image.color = SidebarBgColor;
                    image.sprite = null;
                    changes++;
                    Debug.Log("[RequiemDetailPanelSetup] Updated LeftSidebar color");
                }
            }

            // Find PortraitList (may have its own background)
            var portraitList = FindChildRecursiveIncludeInactive(detailPanel.transform, "PortraitList");
            if (portraitList != null)
            {
                var image = portraitList.GetComponent<Image>();
                if (image != null)
                {
                    Undo.RecordObject(image, "Update PortraitList");
                    image.color = new Color(0, 0, 0, 0); // Transparent
                    image.sprite = null;
                    changes++;
                }
            }

            return changes;
        }

        private static int SetupCharacterArea(GameObject detailPanel)
        {
            var charArt = FindChildRecursiveIncludeInactive(detailPanel.transform, "CharacterArt");
            if (charArt == null) return 0;

            var image = charArt.GetComponent<Image>();
            if (image == null) return 0;

            Undo.RecordObject(image, "Update CharacterArt");
            image.color = CharacterAreaColor;
            image.sprite = null;

            Debug.Log("[RequiemDetailPanelSetup] Updated CharacterArt color");
            return 1;
        }

        private static int SetupStatsPanel(GameObject detailPanel)
        {
            var statsPanel = FindChildRecursiveIncludeInactive(detailPanel.transform, "StatsPanel");
            if (statsPanel == null) return 0;

            var image = statsPanel.GetComponent<Image>();
            if (image == null) return 0;

            Undo.RecordObject(image, "Update StatsPanel");
            image.color = StatsPanelColor;
            image.sprite = null; // CRITICAL: Remove the PanelFrame_BottomDeco_01 sprite!

            // Update text colors
            UpdateTextColors(statsPanel.gameObject);

            Debug.Log("[RequiemDetailPanelSetup] Updated StatsPanel - removed sprite, set dark color");
            return 1;
        }

        private static int SetupCardsContent(GameObject detailPanel)
        {
            int changes = 0;

            // CardsTabContent - the main container for cards tab
            var cardsTabContent = FindChildRecursiveIncludeInactive(detailPanel.transform, "CardsTabContent");
            if (cardsTabContent != null)
            {
                var image = cardsTabContent.GetComponent<Image>();
                if (image != null)
                {
                    Undo.RecordObject(image, "Update CardsTabContent");
                    image.color = CardsContentColor;
                    image.sprite = null;
                    changes++;
                    Debug.Log("[RequiemDetailPanelSetup] Updated CardsTabContent color");
                }
            }

            // CardsScrollView may have its own background
            var cardsScrollView = FindChildRecursiveIncludeInactive(detailPanel.transform, "CardsScrollView");
            if (cardsScrollView != null)
            {
                var image = cardsScrollView.GetComponent<Image>();
                if (image != null)
                {
                    Undo.RecordObject(image, "Update CardsScrollView");
                    image.color = new Color(0, 0, 0, 0); // Transparent
                    changes++;
                }
            }

            // StartingCardsSection may have background
            var startingCards = FindChildRecursiveIncludeInactive(detailPanel.transform, "StartingCardsSection");
            if (startingCards != null)
            {
                var image = startingCards.GetComponent<Image>();
                if (image != null)
                {
                    Undo.RecordObject(image, "Update StartingCardsSection");
                    image.color = new Color(0, 0, 0, 0); // Transparent
                    changes++;
                }
            }

            // CardContainer may have background
            var cardContainer = FindChildRecursiveIncludeInactive(detailPanel.transform, "CardContainer");
            if (cardContainer != null)
            {
                var image = cardContainer.GetComponent<Image>();
                if (image != null)
                {
                    Undo.RecordObject(image, "Update CardContainer");
                    image.color = new Color(0, 0, 0, 0); // Transparent
                    changes++;
                }
            }

            return changes;
        }

        private static int SetupTabs(GameObject detailPanel)
        {
            int changes = 0;

            var statsTab = FindChildRecursiveIncludeInactive(detailPanel.transform, "StatsTab");
            var cardsTab = FindChildRecursiveIncludeInactive(detailPanel.transform, "CardsTab");

            // Remove Icon children from tabs
            if (statsTab != null)
            {
                var icon = FindChildRecursiveIncludeInactive(statsTab, "Icon");
                if (icon != null)
                {
                    Undo.DestroyObjectImmediate(icon.gameObject);
                    changes++;
                }
            }

            if (cardsTab != null)
            {
                var icon = FindChildRecursiveIncludeInactive(cardsTab, "Icon");
                if (icon != null)
                {
                    Undo.DestroyObjectImmediate(icon.gameObject);
                    changes++;
                }
            }

            // Load tab sprites
            var bgSprite = AssetDatabase.LoadAssetAtPath<Sprite>(TabBgPath);
            var glowSprite = AssetDatabase.LoadAssetAtPath<Sprite>(TabGlowPath);
            var borderSprite = AssetDatabase.LoadAssetAtPath<Sprite>(TabBorderPath);
            var focusGlowSprite = AssetDatabase.LoadAssetAtPath<Sprite>(TabFocusGlowPath);
            var focusSprite = AssetDatabase.LoadAssetAtPath<Sprite>(TabFocusPath);

            if (bgSprite == null)
            {
                Debug.LogWarning("[RequiemDetailPanelSetup] Tab sprites not found");
                return changes;
            }

            if (statsTab != null)
                changes += SetupTabButton(statsTab.gameObject, "Stats", bgSprite, glowSprite, borderSprite, focusGlowSprite, focusSprite, true);

            if (cardsTab != null)
                changes += SetupTabButton(cardsTab.gameObject, "Cards", bgSprite, glowSprite, borderSprite, focusGlowSprite, focusSprite, false);

            return changes;
        }

        private static int SetupTabButton(GameObject tabButton, string labelText,
            Sprite bgSprite, Sprite glowSprite, Sprite borderSprite,
            Sprite focusGlowSprite, Sprite focusSprite, bool isFocused)
        {
            int changes = 0;

            // Check if already setup
            if (FindChildRecursiveIncludeInactive(tabButton.transform, "NormalState") != null)
            {
                var existingVisuals = tabButton.GetComponent<TabButtonVisuals>();
                if (existingVisuals != null)
                {
                    var so = new SerializedObject(existingVisuals);
                    so.FindProperty("_isFocused").boolValue = isFocused;
                    so.ApplyModifiedProperties();
                }
                return 1;
            }

            // Setup main button image
            var mainImage = tabButton.GetComponent<Image>();
            if (mainImage != null)
            {
                Undo.RecordObject(mainImage, "Update tab button");
                mainImage.sprite = bgSprite;
                mainImage.type = Image.Type.Sliced;
                mainImage.color = TabNormalTint;
                changes++;
            }

            // Create NormalState
            var normalState = CreateUIChild(tabButton, "NormalState");
            var normalGlow = CreateSpriteChild(normalState, "Glow", glowSprite, new Color(1, 1, 1, 0.25f));
            CreateSpriteChild(normalState, "Border", borderSprite, Color.white);
            changes += 2;

            // Create FocusState
            var focusState = CreateUIChild(tabButton, "FocusState");
            focusState.SetActive(isFocused);
            var focusGlow = CreateSpriteChild(focusState, "FocusGlow", focusGlowSprite, TabFocusGlowColor);
            var focusHighlight = CreateSpriteChild(focusState, "Focus", focusSprite, Color.white);
            changes += 2;

            // Fix Label
            var label = FindChildRecursiveIncludeInactive(tabButton.transform, "Label");
            if (label != null)
            {
                label.SetAsLastSibling();

                var tmpText = label.GetComponent<TextMeshProUGUI>();
                if (tmpText != null)
                {
                    Undo.RecordObject(tmpText, "Update label");
                    tmpText.text = labelText;
                    tmpText.alignment = TextAlignmentOptions.Center;
                    tmpText.color = Color.white;

                    var rect = label.GetComponent<RectTransform>();
                    if (rect != null)
                    {
                        rect.anchorMin = Vector2.zero;
                        rect.anchorMax = Vector2.one;
                        rect.offsetMin = Vector2.zero;
                        rect.offsetMax = Vector2.zero;
                    }

                    var config = AssetDatabase.LoadAssetAtPath<LayerLabSpriteConfigSO>(ConfigPath);
                    if (config != null && config.FontAfacadFlux != null)
                        tmpText.font = config.FontAfacadFlux;
                }
            }

            // Add TabButtonVisuals
            var visuals = tabButton.GetComponent<TabButtonVisuals>();
            if (visuals == null)
            {
                visuals = Undo.AddComponent<TabButtonVisuals>(tabButton);
                changes++;
            }

            var serialized = new SerializedObject(visuals);
            serialized.FindProperty("_normalGlow").objectReferenceValue = normalGlow;
            serialized.FindProperty("_focusGlow").objectReferenceValue = focusGlow;
            serialized.FindProperty("_focusHighlight").objectReferenceValue = focusHighlight;
            serialized.FindProperty("_isFocused").boolValue = isFocused;
            serialized.ApplyModifiedProperties();

            return changes;
        }

        // ============================================
        // Helper Methods
        // ============================================

        private static void UpdateTextColors(GameObject parent)
        {
            var texts = parent.GetComponentsInChildren<TMP_Text>(true);
            foreach (var text in texts)
            {
                Undo.RecordObject(text, "Update text color");
                string name = text.gameObject.name.ToLower();
                text.color = (name.Contains("label") || name.Contains("header")) ? TextSecondaryColor : TextPrimaryColor;
            }
        }

        private static GameObject FindRequiemDetailPanel()
        {
            var allObjects = Resources.FindObjectsOfTypeAll<Transform>();
            foreach (var t in allObjects)
            {
                if (t.hideFlags != HideFlags.None) continue;
                if (!string.IsNullOrEmpty(AssetDatabase.GetAssetPath(t.gameObject))) continue;
                if (t.name == "RequiemDetailPanel") return t.gameObject;
            }
            return null;
        }

        private static Transform FindChildRecursiveIncludeInactive(Transform parent, string name)
        {
            for (int i = 0; i < parent.childCount; i++)
            {
                var child = parent.GetChild(i);
                if (child.name == name) return child;
                var found = FindChildRecursiveIncludeInactive(child, name);
                if (found != null) return found;
            }
            return null;
        }

        private static GameObject CreateUIChild(GameObject parent, string name)
        {
            var existing = FindChildRecursiveIncludeInactive(parent.transform, name);
            if (existing != null) return existing.gameObject;

            var child = new GameObject(name);
            Undo.RegisterCreatedObjectUndo(child, $"Create {name}");
            child.transform.SetParent(parent.transform, false);

            var rect = child.AddComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            return child;
        }

        private static GameObject CreateSpriteChild(GameObject parent, string name, Sprite sprite, Color color)
        {
            var child = CreateUIChild(parent, name);

            var image = child.GetComponent<Image>();
            if (image == null) image = child.AddComponent<Image>();

            image.sprite = sprite;
            image.type = Image.Type.Sliced;
            image.color = color;
            image.raycastTarget = false;

            return child;
        }
    }
}
