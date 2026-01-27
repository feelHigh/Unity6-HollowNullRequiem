// ============================================
// LayerLabStageFrameBuilder.cs
// Builder helper for creating LayerLab-styled stage frames
// ============================================

using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;
using HNR.UI.Config;
using HNR.UI.Components;

namespace HNR.Editor.LayerLab
{
    /// <summary>
    /// Builder helper for creating stage frame UI using LayerLab GUI Pro-FantasyHero sprites.
    /// Stage frames are used for zone selection in BattleMission scene.
    /// </summary>
    public static class LayerLabStageFrameBuilder
    {
        private const string ConfigPath = "Assets/_Project/Data/Config/LayerLabSpriteConfig.asset";

        // Default size for stage frames
        public static readonly Vector2 StageFrameSize = new Vector2(300, 340);

        // Fallback colors
        private static readonly Color FallbackBackBg = new Color(0.11f, 0.1f, 0.18f);
        private static readonly Color FallbackBg = new Color(0.15f, 0.12f, 0.22f);
        private static readonly Color FallbackBorder = new Color(0.42f, 0.25f, 0.63f);
        private static readonly Color FallbackFocus = new Color(0.55f, 0.35f, 0.75f, 0.8f);

        // Purple tint for styled frames (with 50% transparency on bg)
        private static readonly Color FrameBgTint = new Color(0.6f, 0.45f, 0.85f, 0.5f);
        private static readonly Color FrameBorderTint = new Color(0.7f, 0.55f, 0.9f, 1f);

        /// <summary>
        /// Creates a stage frame for zone selection.
        /// Includes back bg, main bg, border, deco, focus, and content area.
        /// </summary>
        /// <param name="parent">Parent GameObject</param>
        /// <param name="name">Frame name</param>
        /// <param name="title">Zone title (e.g., "ZONE 1")</param>
        /// <param name="subtitle">Zone subtitle (e.g., "The Outer Reaches")</param>
        /// <param name="isSelectable">Whether the frame should be interactive</param>
        /// <returns>The stage frame GameObject</returns>
        public static GameObject CreateStageFrame(GameObject parent, string name, string title, string subtitle, bool isSelectable = true)
        {
            // Use the styled variant (no content area, with color tint)
            return CreateStyledStageFrame(parent, name, title, subtitle, isSelectable);
        }

        /// <summary>
        /// Creates a styled stage frame button with purple tint and transparency.
        /// No content area - just frame, title, and subtitle. Used for zone selection and mission buttons.
        /// </summary>
        /// <param name="parent">Parent GameObject</param>
        /// <param name="name">Frame name</param>
        /// <param name="title">Frame title (e.g., "ZONE 1")</param>
        /// <param name="subtitle">Frame subtitle (e.g., "The Outer Reaches")</param>
        /// <param name="isSelectable">Whether the frame should be interactive</param>
        /// <returns>The stage frame GameObject</returns>
        public static GameObject CreateStyledStageFrame(GameObject parent, string name, string title, string subtitle, bool isSelectable = true)
        {
            var config = LoadConfig();

            GameObject frameObj = new GameObject(name);
            frameObj.transform.SetParent(parent.transform, false);

            RectTransform rect = frameObj.AddComponent<RectTransform>();
            rect.sizeDelta = StageFrameSize;

            // Get sprites
            (Sprite backBg, Sprite bg, Sprite border, Sprite deco, Sprite focus) = config != null
                ? config.GetStageFrameSprites()
                : (null, null, null, null, null);

            // Back background layer (outermost) with purple tint and transparency
            Image backBgImage = frameObj.AddComponent<Image>();
            if (backBg != null)
            {
                backBgImage.sprite = backBg;
                backBgImage.type = Image.Type.Sliced;
                backBgImage.color = FrameBgTint;
            }
            else
            {
                backBgImage.color = FallbackBackBg;
            }

            // Main background layer with tint
            if (bg != null)
            {
                CreateLayer(frameObj, "Bg", bg, FrameBgTint, Vector2.zero, Vector2.one, new Vector2(8, 8), new Vector2(-8, -8));
            }
            else
            {
                CreateColorLayer(frameObj, "Bg", FallbackBg, new Vector2(8, 8), new Vector2(-8, -8));
            }

            // Border layer with tint
            if (border != null)
            {
                CreateLayer(frameObj, "Border", border, FrameBorderTint, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            }
            else
            {
                // Create simple border effect with outline
                CreateColorLayer(frameObj, "Border", FallbackBorder, new Vector2(-2, -2), new Vector2(2, 2));
            }

            // Deco layer (full coverage for depth effect)
            if (deco != null)
            {
                CreateLayer(frameObj, "Deco", deco, new Color(1, 1, 1, 0.8f), Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            }

            // Focus layer (shown when selected/hovered)
            GameObject focusObj = null;
            if (focus != null)
            {
                focusObj = CreateLayer(frameObj, "Focus", focus, Color.white, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
                focusObj.SetActive(false);
            }
            else
            {
                focusObj = CreateColorLayer(frameObj, "Focus", FallbackFocus, new Vector2(-4, -4), new Vector2(4, 4));
                focusObj.SetActive(false);
            }

            // Title text (centered vertically upper area)
            GameObject titleObj = CreateFrameText(frameObj, "Title", title, 28, FontStyles.Bold);
            RectTransform titleRect = titleObj.GetComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0, 0.55f);
            titleRect.anchorMax = new Vector2(1, 0.75f);
            titleRect.sizeDelta = Vector2.zero;
            titleObj.GetComponent<TextMeshProUGUI>().color = new Color(0.83f, 0.69f, 0.22f); // Soul gold

            // Subtitle text (centered vertically lower area)
            GameObject subtitleObj = CreateFrameText(frameObj, "Subtitle", subtitle, 16, FontStyles.Normal);
            RectTransform subtitleRect = subtitleObj.GetComponent<RectTransform>();
            subtitleRect.anchorMin = new Vector2(0, 0.3f);
            subtitleRect.anchorMax = new Vector2(1, 0.5f);
            subtitleRect.sizeDelta = Vector2.zero;
            subtitleObj.GetComponent<TextMeshProUGUI>().color = new Color(0.75f, 0.75f, 0.8f);

            // Button component (if selectable)
            if (isSelectable)
            {
                Button button = frameObj.AddComponent<Button>();
                button.targetGraphic = backBgImage;
                button.transition = Selectable.Transition.ColorTint;

                ColorBlock colors = button.colors;
                colors.normalColor = Color.white;
                colors.highlightedColor = new Color(1.2f, 1.2f, 1.3f);
                colors.pressedColor = new Color(0.85f, 0.85f, 0.9f);
                colors.selectedColor = new Color(1.1f, 1.1f, 1.15f);
                colors.disabledColor = new Color(0.5f, 0.5f, 0.5f, 0.8f);
                button.colors = colors;

                // Add StageFrameVisuals component for focus state management (no content area references)
                var visuals = frameObj.AddComponent<StageFrameVisuals>();
                WireStageFrameVisuals(visuals, focusObj, null, null);
            }

            return frameObj;
        }

        /// <summary>
        /// Creates a simpler zone button without full stage frame decoration.
        /// Used when LayerLab sprites are not available.
        /// </summary>
        public static GameObject CreateSimpleZoneButton(GameObject parent, string name, string title, string subtitle)
        {
            GameObject buttonObj = new GameObject(name);
            buttonObj.transform.SetParent(parent.transform, false);

            RectTransform rect = buttonObj.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(280, 320);

            // Background
            Image bgImage = buttonObj.AddComponent<Image>();
            bgImage.color = FallbackBackBg;

            // Button
            Button button = buttonObj.AddComponent<Button>();
            button.targetGraphic = bgImage;

            // Content area
            GameObject contentArea = new GameObject("ContentArea");
            contentArea.transform.SetParent(buttonObj.transform, false);

            RectTransform contentRect = contentArea.AddComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0.08f, 0.22f);
            contentRect.anchorMax = new Vector2(0.92f, 0.82f);
            contentRect.sizeDelta = Vector2.zero;

            Image contentImage = contentArea.AddComponent<Image>();
            contentImage.color = new Color(0.1f, 0.08f, 0.15f, 0.9f);

            // Title
            GameObject titleObj = CreateFrameText(buttonObj, "Title", title, 26, FontStyles.Bold);
            RectTransform titleRect = titleObj.GetComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0, 0.85f);
            titleRect.anchorMax = new Vector2(1, 0.98f);
            titleRect.sizeDelta = Vector2.zero;
            titleObj.GetComponent<TextMeshProUGUI>().color = new Color(0.83f, 0.69f, 0.22f);

            // Subtitle
            GameObject subtitleObj = CreateFrameText(buttonObj, "Subtitle", subtitle, 14, FontStyles.Normal);
            RectTransform subtitleRect = subtitleObj.GetComponent<RectTransform>();
            subtitleRect.anchorMin = new Vector2(0, 0.04f);
            subtitleRect.anchorMax = new Vector2(1, 0.16f);
            subtitleRect.sizeDelta = Vector2.zero;
            subtitleObj.GetComponent<TextMeshProUGUI>().color = new Color(0.7f, 0.7f, 0.75f);

            return buttonObj;
        }

        /// <summary>
        /// Creates a sprite layer with specified anchors and offsets.
        /// </summary>
        private static GameObject CreateLayer(GameObject parent, string name, Sprite sprite, Color color,
            Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)
        {
            GameObject layerObj = new GameObject(name);
            layerObj.transform.SetParent(parent.transform, false);

            RectTransform rect = layerObj.AddComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = offsetMin;
            rect.offsetMax = offsetMax;

            Image image = layerObj.AddComponent<Image>();
            image.sprite = sprite;
            image.type = Image.Type.Sliced;
            image.color = color;
            image.raycastTarget = false;

            return layerObj;
        }

        /// <summary>
        /// Creates a color-only layer (fallback when sprites not available).
        /// </summary>
        private static GameObject CreateColorLayer(GameObject parent, string name, Color color,
            Vector2 offsetMin, Vector2 offsetMax)
        {
            GameObject layerObj = new GameObject(name);
            layerObj.transform.SetParent(parent.transform, false);

            RectTransform rect = layerObj.AddComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = offsetMin;
            rect.offsetMax = offsetMax;

            Image image = layerObj.AddComponent<Image>();
            image.color = color;
            image.raycastTarget = false;

            return layerObj;
        }

        /// <summary>
        /// Creates a TextMeshPro text object for stage frames.
        /// </summary>
        private static GameObject CreateFrameText(GameObject parent, string name, string text, int fontSize, FontStyles style)
        {
            GameObject textObj = new GameObject(name);
            textObj.transform.SetParent(parent.transform, false);

            RectTransform rect = textObj.AddComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.sizeDelta = Vector2.zero;

            TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.fontStyle = style;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = Color.white;
            tmp.raycastTarget = false;

            // Apply LayerLab font if available
            var config = LoadConfig();
            if (config != null && config.FontAfacadFlux != null)
            {
                tmp.font = config.FontAfacadFlux;
            }

            return textObj;
        }

        /// <summary>
        /// Loads the LayerLab sprite config.
        /// </summary>
        private static LayerLabSpriteConfigSO LoadConfig()
        {
            return AssetDatabase.LoadAssetAtPath<LayerLabSpriteConfigSO>(ConfigPath);
        }

        /// <summary>
        /// Wires the StageFrameVisuals component with references.
        /// </summary>
        private static void WireStageFrameVisuals(StageFrameVisuals visuals, GameObject focus, Image contentImage, Image iconImage)
        {
            SerializedObject so = new SerializedObject(visuals);
            so.FindProperty("_focusLayer").objectReferenceValue = focus;
            so.FindProperty("_contentImage").objectReferenceValue = contentImage;
            so.FindProperty("_iconImage").objectReferenceValue = iconImage;
            so.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
