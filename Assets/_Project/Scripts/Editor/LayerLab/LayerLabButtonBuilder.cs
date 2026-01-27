// ============================================
// LayerLabButtonBuilder.cs
// Builder helper for creating LayerLab-styled buttons
// ============================================

using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;
using HNR.UI.Config;

namespace HNR.Editor.LayerLab
{
    /// <summary>
    /// Builder helper for creating buttons using LayerLab GUI Pro-FantasyHero sprites.
    /// Used by ProductionSceneSetupGenerator for consistent button styling.
    /// </summary>
    public static class LayerLabButtonBuilder
    {
        private const string ConfigPath = "Assets/_Project/Data/Config/LayerLabSpriteConfig.asset";

        // Default sizes based on LayerLab prefab specifications
        public static readonly Vector2 Button01SmallSize = new Vector2(230, 104);
        public static readonly Vector2 ConvexRectangleSize = new Vector2(124, 122);
        public static readonly Vector2 ConvexLeftFlushSize = new Vector2(171, 138);

        // Fallback colors matching theme
        private static readonly Color FallbackPurple = new Color(0.25f, 0.15f, 0.35f);
        private static readonly Color FallbackGreen = new Color(0.15f, 0.35f, 0.2f);
        private static readonly Color FallbackRed = new Color(0.35f, 0.15f, 0.15f);
        private static readonly Color FallbackGray = new Color(0.25f, 0.25f, 0.28f);

        // Fallback text colors (used when config not available)
        private static readonly Color FallbackTextColorPurple = new Color(0.322f, 0.106f, 0.612f, 1f);
        private static readonly Color FallbackTextColorGreen = new Color(0.145f, 0.451f, 0.345f, 1f);
        private static readonly Color FallbackTextColorRed = new Color(0.525f, 0.161f, 0.255f, 1f);
        private static readonly Color FallbackTextColorGray = new Color(0.314f, 0.298f, 0.353f, 1f);

        /// <summary>
        /// Creates a Button_01 small style button (230x104).
        /// Standard menu button with background layer.
        /// </summary>
        /// <param name="parent">Parent GameObject</param>
        /// <param name="name">Button name</param>
        /// <param name="text">Button text</param>
        /// <param name="color">Button color: "purple", "green", "red", "gray"</param>
        /// <returns>The button GameObject</returns>
        public static GameObject CreateButton01Small(GameObject parent, string name, string text, string color = "purple")
        {
            var config = LoadConfig();

            GameObject buttonObj = new GameObject(name);
            buttonObj.transform.SetParent(parent.transform, false);

            RectTransform rect = buttonObj.AddComponent<RectTransform>();
            rect.sizeDelta = Button01SmallSize;

            // Get sprites for this color
            (Sprite bgSprite, Sprite decoSprite) = config != null
                ? config.GetButton01SmallSprites(color)
                : (null, null);

            // Background Image
            Image bgImage = buttonObj.AddComponent<Image>();
            if (bgSprite != null)
            {
                bgImage.sprite = bgSprite;
                bgImage.type = Image.Type.Sliced;
                bgImage.color = Color.white;
            }
            else
            {
                bgImage.color = GetFallbackColor(color);
            }

            // Button component
            Button button = buttonObj.AddComponent<Button>();
            button.targetGraphic = bgImage;
            button.transition = Selectable.Transition.ColorTint;

            ColorBlock colors = button.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(1.1f, 1.1f, 1.1f);
            colors.pressedColor = new Color(0.85f, 0.85f, 0.85f);
            colors.selectedColor = new Color(1.05f, 1.05f, 1.05f);
            colors.disabledColor = new Color(0.5f, 0.5f, 0.5f);
            button.colors = colors;

            // Text
            GameObject textObj = CreateButtonText(buttonObj, text, 42, config);
            RectTransform textRect = textObj.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.anchoredPosition = new Vector2(0, 6); // 6 units above center
            textRect.sizeDelta = new Vector2(-44, -28); // 22 each side, 14 top/bottom

            // Set text color based on button color (from config if available)
            var tmp = textObj.GetComponent<TextMeshProUGUI>();
            if (tmp != null)
            {
                tmp.color = GetTextColor(color, config);
            }

            return buttonObj;
        }

        /// <summary>
        /// Creates a convex rectangle button (124x122).
        /// Used for icon-only buttons like Settings.
        /// </summary>
        /// <param name="parent">Parent GameObject</param>
        /// <param name="name">Button name</param>
        /// <param name="iconSprite">Icon sprite to display (optional)</param>
        /// <returns>The button GameObject</returns>
        public static GameObject CreateConvexRectangleButton(GameObject parent, string name, Sprite iconSprite = null)
        {
            var config = LoadConfig();

            GameObject buttonObj = new GameObject(name);
            buttonObj.transform.SetParent(parent.transform, false);

            RectTransform rect = buttonObj.AddComponent<RectTransform>();
            rect.sizeDelta = ConvexRectangleSize;

            // Get sprites
            (Sprite bgSprite, Sprite borderSprite) = config != null
                ? config.GetConvexRectangleSprites()
                : (null, null);

            // Background Image
            Image bgImage = buttonObj.AddComponent<Image>();
            if (bgSprite != null)
            {
                bgImage.sprite = bgSprite;
                bgImage.type = Image.Type.Sliced;
                bgImage.color = Color.white;
            }
            else
            {
                bgImage.color = FallbackGray;
            }

            // Button component
            Button button = buttonObj.AddComponent<Button>();
            button.targetGraphic = bgImage;
            button.transition = Selectable.Transition.ColorTint;

            ColorBlock colors = button.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(1.1f, 1.1f, 1.1f);
            colors.pressedColor = new Color(0.85f, 0.85f, 0.85f);
            colors.selectedColor = new Color(1.05f, 1.05f, 1.05f);
            colors.disabledColor = new Color(0.5f, 0.5f, 0.5f);
            button.colors = colors;

            // Border layer
            if (borderSprite != null)
            {
                GameObject borderObj = new GameObject("Border");
                borderObj.transform.SetParent(buttonObj.transform, false);

                RectTransform borderRect = borderObj.AddComponent<RectTransform>();
                borderRect.anchorMin = Vector2.zero;
                borderRect.anchorMax = Vector2.one;
                borderRect.sizeDelta = Vector2.zero;

                Image borderImage = borderObj.AddComponent<Image>();
                borderImage.sprite = borderSprite;
                borderImage.type = Image.Type.Sliced;
                borderImage.raycastTarget = false;
            }

            // Icon
            GameObject iconObj = new GameObject("Icon");
            iconObj.transform.SetParent(buttonObj.transform, false);

            RectTransform iconRect = iconObj.AddComponent<RectTransform>();
            iconRect.anchorMin = new Vector2(0.2f, 0.2f);
            iconRect.anchorMax = new Vector2(0.8f, 0.8f);
            iconRect.sizeDelta = Vector2.zero;

            Image iconImage = iconObj.AddComponent<Image>();
            iconImage.raycastTarget = false;

            if (iconSprite != null)
            {
                iconImage.sprite = iconSprite;
                iconImage.preserveAspect = true;
            }
            else if (config?.IconSettings != null)
            {
                // Default to settings icon if no icon specified
                iconImage.sprite = config.IconSettings;
                iconImage.preserveAspect = true;
            }
            else
            {
                // Fallback: text icon
                iconImage.color = new Color(0.6f, 0.6f, 0.7f, 0.8f);
            }

            return buttonObj;
        }

        /// <summary>
        /// Creates a convex left flush button (171x138).
        /// Used for back buttons with directional appearance.
        /// </summary>
        /// <param name="parent">Parent GameObject</param>
        /// <param name="name">Button name</param>
        /// <param name="iconSprite">Icon sprite to display (optional)</param>
        /// <returns>The button GameObject</returns>
        public static GameObject CreateConvexLeftFlushButton(GameObject parent, string name, Sprite iconSprite = null)
        {
            var config = LoadConfig();

            GameObject buttonObj = new GameObject(name);
            buttonObj.transform.SetParent(parent.transform, false);

            RectTransform rect = buttonObj.AddComponent<RectTransform>();
            rect.sizeDelta = ConvexLeftFlushSize;

            // Get sprites
            (Sprite bgSprite, Sprite borderSprite) = config != null
                ? config.GetConvexLeftFlushSprites()
                : (null, null);

            // Background Image
            Image bgImage = buttonObj.AddComponent<Image>();
            if (bgSprite != null)
            {
                bgImage.sprite = bgSprite;
                bgImage.type = Image.Type.Sliced;
                bgImage.color = Color.white;
            }
            else
            {
                bgImage.color = FallbackGray;
            }

            // Button component
            Button button = buttonObj.AddComponent<Button>();
            button.targetGraphic = bgImage;
            button.transition = Selectable.Transition.ColorTint;

            ColorBlock colors = button.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(1.1f, 1.1f, 1.1f);
            colors.pressedColor = new Color(0.85f, 0.85f, 0.85f);
            colors.selectedColor = new Color(1.05f, 1.05f, 1.05f);
            colors.disabledColor = new Color(0.5f, 0.5f, 0.5f);
            button.colors = colors;

            // Border layer
            if (borderSprite != null)
            {
                GameObject borderObj = new GameObject("Border");
                borderObj.transform.SetParent(buttonObj.transform, false);

                RectTransform borderRect = borderObj.AddComponent<RectTransform>();
                borderRect.anchorMin = Vector2.zero;
                borderRect.anchorMax = Vector2.one;
                borderRect.sizeDelta = Vector2.zero;

                Image borderImage = borderObj.AddComponent<Image>();
                borderImage.sprite = borderSprite;
                borderImage.type = Image.Type.Sliced;
                borderImage.raycastTarget = false;
            }

            // Icon
            GameObject iconObj = new GameObject("Icon");
            iconObj.transform.SetParent(buttonObj.transform, false);

            RectTransform iconRect = iconObj.AddComponent<RectTransform>();
            iconRect.anchorMin = new Vector2(0.15f, 0.2f);
            iconRect.anchorMax = new Vector2(0.7f, 0.8f);
            iconRect.sizeDelta = Vector2.zero;

            Image iconImage = iconObj.AddComponent<Image>();
            iconImage.raycastTarget = false;

            if (iconSprite != null)
            {
                iconImage.sprite = iconSprite;
                iconImage.preserveAspect = true;
            }
            else if (config?.IconBack != null)
            {
                // Default to back arrow icon if no icon specified
                iconImage.sprite = config.IconBack;
                iconImage.preserveAspect = true;
            }
            else
            {
                // Fallback: simple arrow
                iconImage.color = new Color(0.6f, 0.6f, 0.7f, 0.8f);
            }

            return buttonObj;
        }

        /// <summary>
        /// Creates a Button_01 small with scaled dimensions.
        /// Maintains aspect ratio while fitting specified size.
        /// </summary>
        public static GameObject CreateButton01SmallScaled(GameObject parent, string name, string text, string color, Vector2 targetSize)
        {
            var buttonObj = CreateButton01Small(parent, name, text, color);

            RectTransform rect = buttonObj.GetComponent<RectTransform>();
            rect.sizeDelta = targetSize;

            // Adjust text and deco size proportionally
            float scale = targetSize.y / Button01SmallSize.y;

            var textObj = buttonObj.transform.Find("Text")?.GetComponent<TextMeshProUGUI>();
            if (textObj != null)
            {
                textObj.fontSizeMax = Mathf.RoundToInt(42 * scale);
                textObj.fontSizeMin = Mathf.RoundToInt(18 * scale);
            }

            return buttonObj;
        }

        /// <summary>
        /// Loads the LayerLab sprite config.
        /// </summary>
        private static LayerLabSpriteConfigSO LoadConfig()
        {
            return AssetDatabase.LoadAssetAtPath<LayerLabSpriteConfigSO>(ConfigPath);
        }

        /// <summary>
        /// Gets fallback color for button when sprites are not available.
        /// </summary>
        private static Color GetFallbackColor(string color)
        {
            return color?.ToLower() switch
            {
                "purple" => FallbackPurple,
                "green" => FallbackGreen,
                "red" => FallbackRed,
                "gray" => FallbackGray,
                _ => FallbackPurple
            };
        }

        /// <summary>
        /// Gets text color for button from config, with fallback to hardcoded values.
        /// </summary>
        private static Color GetTextColor(string color, LayerLabSpriteConfigSO config)
        {
            // Use config colors if available
            if (config != null)
            {
                return config.GetTextColor(color);
            }

            // Fallback to hardcoded colors
            return color?.ToLower() switch
            {
                "purple" => FallbackTextColorPurple,
                "green" => FallbackTextColorGreen,
                "red" => FallbackTextColorRed,
                "gray" => FallbackTextColorGray,
                _ => FallbackTextColorPurple
            };
        }

        /// <summary>
        /// Creates a TextMeshPro text object for buttons.
        /// </summary>
        private static GameObject CreateButtonText(GameObject parent, string text, int fontSize, LayerLabSpriteConfigSO config = null)
        {
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(parent.transform, false);

            RectTransform rect = textObj.AddComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.sizeDelta = Vector2.zero;

            TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.fontStyle = FontStyles.Normal; // LayerLab uses normal style with bold font
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = Color.white;
            tmp.raycastTarget = false;

            // Use LayerLab font if available
            if (config?.FontAfacadFlux != null)
            {
                tmp.font = config.FontAfacadFlux;
            }

            // Enable auto-sizing (from LayerLab prefab: min 18, max 42)
            tmp.enableAutoSizing = true;
            tmp.fontSizeMin = 18;
            tmp.fontSizeMax = fontSize;

            return textObj;
        }
    }
}
