// ============================================
// LayerLabSpriteConfigSO.cs
// Configuration for LayerLab GUI Pro-FantasyHero sprites
// ============================================

using UnityEngine;
using TMPro;

namespace HNR.UI.Config
{
    /// <summary>
    /// ScriptableObject that stores LayerLab GUI Pro-FantasyHero sprite references.
    /// Used by ProductionSceneSetupGenerator and LayerLab builder helpers.
    /// </summary>
    [CreateAssetMenu(fileName = "LayerLabSpriteConfig", menuName = "HNR/Config/LayerLab Sprite Config")]
    public class LayerLabSpriteConfigSO : ScriptableObject
    {
        // ============================================
        // Button_01 Small (230x104) - Standard Menu Buttons
        // ============================================

        [Header("Button_01 Small (230x104)")]
        [SerializeField, Tooltip("Purple button background sprite")]
        private Sprite _button01SmallPurpleBg;

        [SerializeField, Tooltip("Purple button deco sprite")]
        private Sprite _button01SmallPurpleDeco;

        [SerializeField, Tooltip("Green button background sprite")]
        private Sprite _button01SmallGreenBg;

        [SerializeField, Tooltip("Green button deco sprite")]
        private Sprite _button01SmallGreenDeco;

        [SerializeField, Tooltip("Red button background sprite")]
        private Sprite _button01SmallRedBg;

        [SerializeField, Tooltip("Red button deco sprite")]
        private Sprite _button01SmallRedDeco;

        [SerializeField, Tooltip("Gray button background sprite")]
        private Sprite _button01SmallGrayBg;

        [SerializeField, Tooltip("Gray button deco sprite")]
        private Sprite _button01SmallGrayDeco;

        // ============================================
        // Button Convex Rectangle (124x122) - Icon Buttons
        // ============================================

        [Header("Button Convex Rectangle (124x122)")]
        [SerializeField, Tooltip("Convex rectangle gray background sprite")]
        private Sprite _buttonConvexRectangleGrayBg;

        [SerializeField, Tooltip("Convex rectangle gray border sprite")]
        private Sprite _buttonConvexRectangleGrayBorder;

        // ============================================
        // Button Convex LeftFlush (171x138) - Back Buttons
        // ============================================

        [Header("Button Convex LeftFlush (171x138)")]
        [SerializeField, Tooltip("Convex left flush gray background sprite")]
        private Sprite _buttonConvexLeftFlushGrayBg;

        [SerializeField, Tooltip("Convex left flush gray border sprite")]
        private Sprite _buttonConvexLeftFlushGrayBorder;

        // ============================================
        // TabMenu (863x110) - Navigation Tabs
        // ============================================

        [Header("TabMenu Middle_02")]
        [SerializeField, Tooltip("Tab menu background sprite")]
        private Sprite _tabMenuBg;

        [SerializeField, Tooltip("Tab menu glow sprite")]
        private Sprite _tabMenuGlow;

        [SerializeField, Tooltip("Tab menu border sprite")]
        private Sprite _tabMenuBorder;

        [SerializeField, Tooltip("Tab focus glow sprite")]
        private Sprite _tabFocusGlow;

        [SerializeField, Tooltip("Tab focus sprite")]
        private Sprite _tabFocus;

        // ============================================
        // StageFrame (Zone Selection Nodes)
        // ============================================

        [Header("StageFrame_02 Focus Purple")]
        [SerializeField, Tooltip("Stage frame back background sprite")]
        private Sprite _stageFrameBackBg;

        [SerializeField, Tooltip("Stage frame background sprite")]
        private Sprite _stageFrameBg;

        [SerializeField, Tooltip("Stage frame border sprite")]
        private Sprite _stageFrameBorder;

        [SerializeField, Tooltip("Stage frame deco sprite")]
        private Sprite _stageFrameDeco;

        [SerializeField, Tooltip("Stage frame focus sprite")]
        private Sprite _stageFrameFocus;

        // ============================================
        // Common UI Elements
        // ============================================

        [Header("Common Elements")]
        [SerializeField, Tooltip("Default icon for buttons (settings gear, etc.)")]
        private Sprite _iconSettings;

        [SerializeField, Tooltip("Back arrow icon")]
        private Sprite _iconBack;

        // ============================================
        // Fonts
        // ============================================

        [Header("Fonts")]
        [SerializeField, Tooltip("AfacadFlux-ExtraBold SDF font asset")]
        private TMP_FontAsset _fontAfacadFlux;

        // ============================================
        // Button Text Colors
        // ============================================

        [Header("Button Text Colors")]
        [SerializeField, Tooltip("Text color for purple buttons")]
        private Color _textColorPurple = new Color(0.322f, 0.106f, 0.612f, 1f);

        [SerializeField, Tooltip("Text color for green buttons")]
        private Color _textColorGreen = new Color(0.145f, 0.451f, 0.345f, 1f);

        [SerializeField, Tooltip("Text color for red buttons")]
        private Color _textColorRed = new Color(0.525f, 0.161f, 0.255f, 1f);

        [SerializeField, Tooltip("Text color for gray buttons")]
        private Color _textColorGray = new Color(0.314f, 0.298f, 0.353f, 1f);

        // ============================================
        // Button_01 Small Accessors
        // ============================================

        public Sprite Button01SmallPurpleBg => _button01SmallPurpleBg;
        public Sprite Button01SmallPurpleDeco => _button01SmallPurpleDeco;
        public Sprite Button01SmallGreenBg => _button01SmallGreenBg;
        public Sprite Button01SmallGreenDeco => _button01SmallGreenDeco;
        public Sprite Button01SmallRedBg => _button01SmallRedBg;
        public Sprite Button01SmallRedDeco => _button01SmallRedDeco;
        public Sprite Button01SmallGrayBg => _button01SmallGrayBg;
        public Sprite Button01SmallGrayDeco => _button01SmallGrayDeco;

        // ============================================
        // Convex Button Accessors
        // ============================================

        public Sprite ButtonConvexRectangleGrayBg => _buttonConvexRectangleGrayBg;
        public Sprite ButtonConvexRectangleGrayBorder => _buttonConvexRectangleGrayBorder;
        public Sprite ButtonConvexLeftFlushGrayBg => _buttonConvexLeftFlushGrayBg;
        public Sprite ButtonConvexLeftFlushGrayBorder => _buttonConvexLeftFlushGrayBorder;

        // ============================================
        // TabMenu Accessors
        // ============================================

        public Sprite TabMenuBg => _tabMenuBg;
        public Sprite TabMenuGlow => _tabMenuGlow;
        public Sprite TabMenuBorder => _tabMenuBorder;
        public Sprite TabFocusGlow => _tabFocusGlow;
        public Sprite TabFocus => _tabFocus;

        // ============================================
        // StageFrame Accessors
        // ============================================

        public Sprite StageFrameBackBg => _stageFrameBackBg;
        public Sprite StageFrameBg => _stageFrameBg;
        public Sprite StageFrameBorder => _stageFrameBorder;
        public Sprite StageFrameDeco => _stageFrameDeco;
        public Sprite StageFrameFocus => _stageFrameFocus;

        // ============================================
        // Icon Accessors
        // ============================================

        public Sprite IconSettings => _iconSettings;
        public Sprite IconBack => _iconBack;

        // ============================================
        // Font Accessors
        // ============================================

        public TMP_FontAsset FontAfacadFlux => _fontAfacadFlux;

        // ============================================
        // Text Color Accessors
        // ============================================

        public Color TextColorPurple => _textColorPurple;
        public Color TextColorGreen => _textColorGreen;
        public Color TextColorRed => _textColorRed;
        public Color TextColorGray => _textColorGray;

        // ============================================
        // Public API
        // ============================================

        /// <summary>
        /// Gets Button_01 small sprites for a specific color.
        /// </summary>
        /// <param name="color">Button color: "purple", "green", "red", "gray"</param>
        /// <returns>Tuple of (background, deco) sprites</returns>
        public (Sprite bg, Sprite deco) GetButton01SmallSprites(string color)
        {
            return color?.ToLower() switch
            {
                "purple" => (_button01SmallPurpleBg, _button01SmallPurpleDeco),
                "green" => (_button01SmallGreenBg, _button01SmallGreenDeco),
                "red" => (_button01SmallRedBg, _button01SmallRedDeco),
                "gray" => (_button01SmallGrayBg, _button01SmallGrayDeco),
                _ => (_button01SmallPurpleBg, _button01SmallPurpleDeco) // Default to purple
            };
        }

        /// <summary>
        /// Gets convex rectangle button sprites.
        /// </summary>
        /// <returns>Tuple of (background, border) sprites</returns>
        public (Sprite bg, Sprite border) GetConvexRectangleSprites()
        {
            return (_buttonConvexRectangleGrayBg, _buttonConvexRectangleGrayBorder);
        }

        /// <summary>
        /// Gets convex left flush button sprites.
        /// </summary>
        /// <returns>Tuple of (background, border) sprites</returns>
        public (Sprite bg, Sprite border) GetConvexLeftFlushSprites()
        {
            return (_buttonConvexLeftFlushGrayBg, _buttonConvexLeftFlushGrayBorder);
        }

        /// <summary>
        /// Gets tab menu sprites for normal state.
        /// </summary>
        /// <returns>Tuple of (bg, glow, border) sprites</returns>
        public (Sprite bg, Sprite glow, Sprite border) GetTabMenuNormalSprites()
        {
            return (_tabMenuBg, _tabMenuGlow, _tabMenuBorder);
        }

        /// <summary>
        /// Gets tab menu sprites for focused state.
        /// </summary>
        /// <returns>Tuple of (bg, focusGlow, focus, border) sprites</returns>
        public (Sprite bg, Sprite focusGlow, Sprite focus, Sprite border) GetTabMenuFocusSprites()
        {
            return (_tabMenuBg, _tabFocusGlow, _tabFocus, _tabMenuBorder);
        }

        /// <summary>
        /// Gets stage frame sprites.
        /// </summary>
        /// <returns>Tuple of (backBg, bg, border, deco, focus) sprites</returns>
        public (Sprite backBg, Sprite bg, Sprite border, Sprite deco, Sprite focus) GetStageFrameSprites()
        {
            return (_stageFrameBackBg, _stageFrameBg, _stageFrameBorder, _stageFrameDeco, _stageFrameFocus);
        }

        /// <summary>
        /// Gets text color for a specific button color.
        /// </summary>
        /// <param name="color">Button color: "purple", "green", "red", "gray"</param>
        /// <returns>The text color for that button style</returns>
        public Color GetTextColor(string color)
        {
            return color?.ToLower() switch
            {
                "purple" => _textColorPurple,
                "green" => _textColorGreen,
                "red" => _textColorRed,
                "gray" => _textColorGray,
                _ => _textColorPurple // Default to purple
            };
        }

        // ============================================
        // Validation
        // ============================================

        /// <summary>
        /// Checks if all Button_01 small sprites are assigned.
        /// </summary>
        public bool HasAllButton01SmallSprites()
        {
            return _button01SmallPurpleBg != null && _button01SmallPurpleDeco != null &&
                   _button01SmallGreenBg != null && _button01SmallGreenDeco != null &&
                   _button01SmallRedBg != null && _button01SmallRedDeco != null;
        }

        /// <summary>
        /// Checks if all convex button sprites are assigned.
        /// </summary>
        public bool HasAllConvexButtonSprites()
        {
            return _buttonConvexRectangleGrayBg != null && _buttonConvexRectangleGrayBorder != null &&
                   _buttonConvexLeftFlushGrayBg != null && _buttonConvexLeftFlushGrayBorder != null;
        }

        /// <summary>
        /// Checks if all tab menu sprites are assigned.
        /// </summary>
        public bool HasAllTabMenuSprites()
        {
            return _tabMenuBg != null && _tabMenuGlow != null && _tabMenuBorder != null &&
                   _tabFocusGlow != null && _tabFocus != null;
        }

        /// <summary>
        /// Checks if all stage frame sprites are assigned.
        /// </summary>
        public bool HasAllStageFrameSprites()
        {
            return _stageFrameBackBg != null && _stageFrameBg != null &&
                   _stageFrameBorder != null && _stageFrameDeco != null &&
                   _stageFrameFocus != null;
        }

        /// <summary>
        /// Gets the count of assigned sprites.
        /// </summary>
        public int GetAssignedCount()
        {
            int count = 0;

            // Button_01 Small
            if (_button01SmallPurpleBg != null) count++;
            if (_button01SmallPurpleDeco != null) count++;
            if (_button01SmallGreenBg != null) count++;
            if (_button01SmallGreenDeco != null) count++;
            if (_button01SmallRedBg != null) count++;
            if (_button01SmallRedDeco != null) count++;
            if (_button01SmallGrayBg != null) count++;
            if (_button01SmallGrayDeco != null) count++;

            // Convex buttons
            if (_buttonConvexRectangleGrayBg != null) count++;
            if (_buttonConvexRectangleGrayBorder != null) count++;
            if (_buttonConvexLeftFlushGrayBg != null) count++;
            if (_buttonConvexLeftFlushGrayBorder != null) count++;

            // Tab menu
            if (_tabMenuBg != null) count++;
            if (_tabMenuGlow != null) count++;
            if (_tabMenuBorder != null) count++;
            if (_tabFocusGlow != null) count++;
            if (_tabFocus != null) count++;

            // Stage frame
            if (_stageFrameBackBg != null) count++;
            if (_stageFrameBg != null) count++;
            if (_stageFrameBorder != null) count++;
            if (_stageFrameDeco != null) count++;
            if (_stageFrameFocus != null) count++;

            // Icons
            if (_iconSettings != null) count++;
            if (_iconBack != null) count++;

            // Font
            if (_fontAfacadFlux != null) count++;

            return count;
        }

        /// <summary>
        /// Gets the total number of sprite slots.
        /// </summary>
        public int GetTotalSlots() => 25; // 8 + 4 + 5 + 5 + 2 + 1 (font)

        /// <summary>
        /// Checks if the config is valid (has minimum required sprites).
        /// </summary>
        public bool IsValid()
        {
            // At minimum, we need purple button sprites for menu buttons
            return _button01SmallPurpleBg != null;
        }
    }
}
