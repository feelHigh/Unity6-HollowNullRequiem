// ============================================
// LayerLabTabMenuBuilder.cs
// Builder helper for creating LayerLab-styled tab menus
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
    /// Builder helper for creating tab menu buttons using LayerLab GUI Pro-FantasyHero sprites.
    /// Tab menus have normal and focused states with glow effects.
    /// </summary>
    public static class LayerLabTabMenuBuilder
    {
        private const string ConfigPath = "Assets/_Project/Data/Config/LayerLabSpriteConfig.asset";

        // Default size based on LayerLab TabMenu_Middle_02 prefab (scaled down for nav buttons)
        public static readonly Vector2 TabButtonSize = new Vector2(256, 70);

        // Fallback colors
        private static readonly Color FallbackNormal = new Color(0.15f, 0.15f, 0.2f, 0.85f);
        private static readonly Color FallbackFocused = new Color(0.25f, 0.2f, 0.35f, 0.95f);
        private static readonly Color FallbackGlow = new Color(0.42f, 0.25f, 0.63f, 0.5f);

        // Purple tint for nav buttons (with 50% transparency on bg)
        private static readonly Color NavButtonBgTint = new Color(0.6f, 0.45f, 0.85f, 0.5f);
        private static readonly Color NavButtonBorderTint = new Color(0.7f, 0.55f, 0.9f, 1f);

        /// <summary>
        /// Creates a tab-style navigation button with focus/normal visual states.
        /// </summary>
        /// <param name="parent">Parent GameObject</param>
        /// <param name="name">Button name</param>
        /// <param name="text">Button text</param>
        /// <param name="icon">Optional icon sprite</param>
        /// <returns>The tab button GameObject with TabButtonVisuals component for state switching</returns>
        public static GameObject CreateTabButton(GameObject parent, string name, string text, Sprite icon = null)
        {
            var config = LoadConfig();

            GameObject buttonObj = new GameObject(name);
            buttonObj.transform.SetParent(parent.transform, false);

            RectTransform rect = buttonObj.AddComponent<RectTransform>();
            rect.sizeDelta = TabButtonSize;

            // Get sprites
            (Sprite bg, Sprite glow, Sprite border) = config != null
                ? config.GetTabMenuNormalSprites()
                : (null, null, null);
            (_, Sprite focusGlow, Sprite focus, _) = config != null
                ? config.GetTabMenuFocusSprites()
                : (null, null, null, null);

            // Background layer
            Image bgImage = buttonObj.AddComponent<Image>();
            if (bg != null)
            {
                bgImage.sprite = bg;
                bgImage.type = Image.Type.Sliced;
                bgImage.color = Color.white;
            }
            else
            {
                bgImage.color = FallbackNormal;
            }

            // Normal glow layer (subtle, always visible in normal state)
            GameObject normalGlowObj = null;
            if (glow != null)
            {
                normalGlowObj = CreateLayer(buttonObj, "NormalGlow", glow, new Color(1, 1, 1, 0.3f));
            }

            // Focus glow layer (visible when focused)
            GameObject focusGlowObj = null;
            if (focusGlow != null)
            {
                focusGlowObj = CreateLayer(buttonObj, "FocusGlow", focusGlow, FallbackGlow);
                focusGlowObj.SetActive(false);
            }

            // Focus highlight layer (visible when focused)
            GameObject focusObj = null;
            if (focus != null)
            {
                focusObj = CreateLayer(buttonObj, "Focus", focus, Color.white);
                focusObj.SetActive(false);
            }

            // Border layer
            if (border != null)
            {
                CreateLayer(buttonObj, "Border", border, Color.white);
            }

            // Icon (left side)
            GameObject iconObj = null;
            if (icon != null)
            {
                iconObj = new GameObject("Icon");
                iconObj.transform.SetParent(buttonObj.transform, false);

                RectTransform iconRect = iconObj.AddComponent<RectTransform>();
                iconRect.anchorMin = new Vector2(0, 0.15f);
                iconRect.anchorMax = new Vector2(0, 0.85f);
                iconRect.pivot = new Vector2(0, 0.5f);
                iconRect.anchoredPosition = new Vector2(15, 0);
                iconRect.sizeDelta = new Vector2(40, 0);

                Image iconImage = iconObj.AddComponent<Image>();
                iconImage.sprite = icon;
                iconImage.preserveAspect = true;
                iconImage.raycastTarget = false;
            }

            // Text
            GameObject textObj = CreateTabText(buttonObj, text);
            RectTransform textRect = textObj.GetComponent<RectTransform>();
            textRect.anchorMin = new Vector2(0, 0);
            textRect.anchorMax = new Vector2(0.85f, 1);
            textRect.offsetMin = new Vector2(icon != null ? 60 : 20, 0);
            textRect.offsetMax = new Vector2(-10, 0);

            // Right arrow/chevron placeholder
            GameObject arrowObj = new GameObject("Arrow");
            arrowObj.transform.SetParent(buttonObj.transform, false);

            RectTransform arrowRect = arrowObj.AddComponent<RectTransform>();
            arrowRect.anchorMin = new Vector2(0.85f, 0.3f);
            arrowRect.anchorMax = new Vector2(0.95f, 0.7f);
            arrowRect.sizeDelta = Vector2.zero;

            Image arrowImage = arrowObj.AddComponent<Image>();
            arrowImage.color = new Color(0.7f, 0.7f, 0.8f, 0.6f);
            arrowImage.raycastTarget = false;

            // Button component
            Button button = buttonObj.AddComponent<Button>();
            button.targetGraphic = bgImage;
            button.transition = Selectable.Transition.ColorTint;

            ColorBlock colors = button.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(1.05f, 1.05f, 1.1f);
            colors.pressedColor = new Color(0.9f, 0.9f, 0.95f);
            colors.selectedColor = new Color(1.02f, 1.02f, 1.05f);
            colors.disabledColor = new Color(0.5f, 0.5f, 0.5f);
            button.colors = colors;

            // Add TabButtonVisuals component for runtime state management
            var visuals = buttonObj.AddComponent<TabButtonVisuals>();
            WireTabButtonVisuals(visuals, normalGlowObj, focusGlowObj, focusObj);

            return buttonObj;
        }

        /// <summary>
        /// Creates a simpler wide navigation button without complex tab visuals.
        /// Used for navigation buttons that don't need focus states.
        /// Applies purple tint with 50% transparency to match HNR theme.
        /// </summary>
        /// <param name="parent">Parent GameObject</param>
        /// <param name="name">Button name</param>
        /// <param name="text">Button text</param>
        /// <returns>The button GameObject</returns>
        public static GameObject CreateWideNavButton(GameObject parent, string name, string text)
        {
            var config = LoadConfig();

            GameObject buttonObj = new GameObject(name);
            buttonObj.transform.SetParent(parent.transform, false);
            buttonObj.AddComponent<RectTransform>();

            // Get tab background sprite for consistent styling
            (Sprite bg, _, Sprite border) = config != null
                ? config.GetTabMenuNormalSprites()
                : (null, null, null);

            // Background with purple tint and 50% transparency
            Image image = buttonObj.AddComponent<Image>();
            if (bg != null)
            {
                image.sprite = bg;
                image.type = Image.Type.Sliced;
                image.color = NavButtonBgTint;
            }
            else
            {
                image.color = FallbackNormal;
            }

            // Button component
            Button button = buttonObj.AddComponent<Button>();
            button.targetGraphic = image;

            // Border layer with purple tint
            if (border != null)
            {
                CreateLayer(buttonObj, "Border", border, NavButtonBorderTint);
            }

            // Button text (centered across full width)
            GameObject textObj = CreateTabText(buttonObj, text);
            RectTransform textRect = textObj.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = new Vector2(20, 0);
            textRect.offsetMax = new Vector2(-20, 0);
            textObj.name = "ButtonText";

            TextMeshProUGUI buttonText = textObj.GetComponent<TextMeshProUGUI>();
            buttonText.alignment = TextAlignmentOptions.Center;

            return buttonObj;
        }

        /// <summary>
        /// Creates a sprite layer as a child of the button.
        /// </summary>
        private static GameObject CreateLayer(GameObject parent, string name, Sprite sprite, Color color)
        {
            GameObject layerObj = new GameObject(name);
            layerObj.transform.SetParent(parent.transform, false);

            RectTransform rect = layerObj.AddComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.sizeDelta = Vector2.zero;

            Image image = layerObj.AddComponent<Image>();
            image.sprite = sprite;
            image.type = Image.Type.Sliced;
            image.color = color;
            image.raycastTarget = false;

            return layerObj;
        }

        /// <summary>
        /// Creates a TextMeshPro text object for tab buttons.
        /// </summary>
        private static GameObject CreateTabText(GameObject parent, string text)
        {
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(parent.transform, false);

            RectTransform rect = textObj.AddComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.sizeDelta = Vector2.zero;

            TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = 22;
            tmp.fontStyle = FontStyles.Normal;
            tmp.alignment = TextAlignmentOptions.Left;
            tmp.color = Color.white;
            tmp.raycastTarget = false;

            // Enable auto-sizing
            tmp.enableAutoSizing = true;
            tmp.fontSizeMin = 14;
            tmp.fontSizeMax = 22;

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
        /// Wires the TabButtonVisuals component with layer references.
        /// </summary>
        private static void WireTabButtonVisuals(TabButtonVisuals visuals, GameObject normalGlow, GameObject focusGlow, GameObject focus)
        {
            SerializedObject so = new SerializedObject(visuals);
            so.FindProperty("_normalGlow").objectReferenceValue = normalGlow;
            so.FindProperty("_focusGlow").objectReferenceValue = focusGlow;
            so.FindProperty("_focusHighlight").objectReferenceValue = focus;
            so.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
