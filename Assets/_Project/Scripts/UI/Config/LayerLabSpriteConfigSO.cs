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
        // ProfileFrame_01 (Character Profile Frames)
        // ============================================

        [Header("ProfileFrame_01")]
        [SerializeField, Tooltip("Profile frame background sprite")]
        private Sprite _profileFrameBg;

        [SerializeField, Tooltip("Profile frame border sprite")]
        private Sprite _profileFrameBorder;

        [SerializeField, Tooltip("Profile frame purple border decoration")]
        private Sprite _profileFrameBorderDecoPurple;

        // ============================================
        // CardFrame (Large Card-Style Buttons)
        // ============================================

        [Header("CardFrame_Rectangle")]
        [SerializeField, Tooltip("Card frame rectangle purple background")]
        private Sprite _cardFrameRectanglePurpleBg;

        [SerializeField, Tooltip("Card frame rectangle green background")]
        private Sprite _cardFrameRectangleGreenBg;

        // ============================================
        // ListFrame_01 (Sidebar/List Backgrounds)
        // ============================================

        [Header("ListFrame_01")]
        [SerializeField, Tooltip("List frame background sprite")]
        private Sprite _listFrameBg;

        [SerializeField, Tooltip("List frame border sprite")]
        private Sprite _listFrameBorder;

        // ============================================
        // PanelFrame (Panel Backgrounds with Decorations)
        // ============================================

        [Header("PanelFrame_BottomDeco_01")]
        [SerializeField, Tooltip("Panel frame background sprite")]
        private Sprite _panelFrameBg;

        [SerializeField, Tooltip("Panel frame border sprite")]
        private Sprite _panelFrameBorder;

        [SerializeField, Tooltip("Panel frame bottom decoration sprite")]
        private Sprite _panelFrameDeco;

        // ============================================
        // ItemFrame (Item/Relic Slots)
        // ============================================

        [Header("ItemFrame")]
        [SerializeField, Tooltip("Item frame square purple sprite")]
        private Sprite _itemFrameSquarePurple;

        // ============================================
        // BaseFrame (Basic Rectangles with Border)
        // ============================================

        [Header("BaseFrame")]
        [SerializeField, Tooltip("Base frame border rectangle H40")]
        private Sprite _baseFrameBorderRectH40;

        [SerializeField, Tooltip("Base frame border rectangle H50")]
        private Sprite _baseFrameBorderRectH50;

        [SerializeField, Tooltip("Base frame border rectangle H60")]
        private Sprite _baseFrameBorderRectH60;

        // ============================================
        // LineFrame (Decorative Dividers)
        // ============================================

        [Header("LineFrame")]
        [SerializeField, Tooltip("Decorative line divider")]
        private Sprite _lineFrameDecoLine01;

        // ============================================
        // Slider Sprites
        // ============================================

        [Header("Slider")]
        [SerializeField, Tooltip("Slider tapered background")]
        private Sprite _sliderBorderTaperedBg;

        [SerializeField, Tooltip("Slider tapered fill")]
        private Sprite _sliderBorderTaperedFill;

        [SerializeField, Tooltip("Slider tapered border")]
        private Sprite _sliderBorderTaperedBorder;

        // ============================================
        // PictoIcons (UI Action Icons)
        // ============================================

        [Header("PictoIcons")]
        [SerializeField, Tooltip("Timer/speed icon")]
        private Sprite _pictoIconTimer;

        [SerializeField, Tooltip("Attack/auto-battle icon")]
        private Sprite _pictoIconAttack;

        [SerializeField, Tooltip("Menu icon")]
        private Sprite _pictoIconMenu;

        [SerializeField, Tooltip("Book icon (for Story mode)")]
        private Sprite _pictoIconBook;

        [SerializeField, Tooltip("Battle/sword icon")]
        private Sprite _pictoIconBattle;

        [SerializeField, Tooltip("Lock icon")]
        private Sprite _pictoIconLock;

        // ============================================
        // ItemIcons (Resource/Status Icons)
        // ============================================

        [Header("ItemIcons")]
        [SerializeField, Tooltip("Heart/HP icon")]
        private Sprite _itemIconHeart;

        [SerializeField, Tooltip("Energy/SE purple icon")]
        private Sprite _itemIconEnergyPurple;

        [SerializeField, Tooltip("Shield/block icon")]
        private Sprite _itemIconShield;

        [SerializeField, Tooltip("Currency/coin icon")]
        private Sprite _itemIconCurrency;

        // ============================================
        // Map Legend Icons (Node Types)
        // ============================================

        [Header("Map Legend Icons")]
        [SerializeField, Tooltip("Combat node icon (crossed swords)")]
        private Sprite _mapIconCombat;

        [SerializeField, Tooltip("Elite enemy node icon (skull)")]
        private Sprite _mapIconElite;

        [SerializeField, Tooltip("Shop node icon (cart/bag)")]
        private Sprite _mapIconShop;

        [SerializeField, Tooltip("Echo event node icon (question mark)")]
        private Sprite _mapIconEcho;

        [SerializeField, Tooltip("Sanctuary node icon (candle/rest)")]
        private Sprite _mapIconSanctuary;

        [SerializeField, Tooltip("Treasure node icon (gem/chest)")]
        private Sprite _mapIconTreasure;

        [SerializeField, Tooltip("Boss node icon (demon/boss)")]
        private Sprite _mapIconBoss;

        // ============================================
        // Deck Display Icons
        // ============================================

        [Header("Deck Display Icons")]
        [SerializeField, Tooltip("Draw pile icon (deck/book)")]
        private Sprite _deckIconDraw;

        [SerializeField, Tooltip("Discard pile icon (refresh/cycle)")]
        private Sprite _deckIconDiscard;

        // ============================================
        // Speed/System Icons
        // ============================================

        [Header("Speed/System Icons")]
        [SerializeField, Tooltip("Speed 1x icon (normal speed)")]
        private Sprite _speedIcon1x;

        [SerializeField, Tooltip("Speed 2x icon (fast speed)")]
        private Sprite _speedIcon2x;

        [SerializeField, Tooltip("Checkmark/confirm icon")]
        private Sprite _iconCheckmark;

        // ============================================
        // Settings Category Icons
        // ============================================

        [Header("Settings Category Icons")]
        [SerializeField, Tooltip("Display/monitor settings icon")]
        private Sprite _settingsIconDisplay;

        [SerializeField, Tooltip("Audio/sound settings icon")]
        private Sprite _settingsIconAudio;

        [SerializeField, Tooltip("Game/gameplay settings icon")]
        private Sprite _settingsIconGame;

        [SerializeField, Tooltip("Network/online settings icon")]
        private Sprite _settingsIconNetwork;

        [SerializeField, Tooltip("Account/profile settings icon")]
        private Sprite _settingsIconAccount;

        // ============================================
        // Alert Icons
        // ============================================

        [Header("Alert Icons")]
        [SerializeField, Tooltip("Diamond alert background (white)")]
        private Sprite _alertDiamondWhiteBg;

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
        // Vertical Slider (StageVertical_02 for SE Gauge)
        // ============================================

        [Header("Vertical Slider (SE Gauge)")]
        [SerializeField, Tooltip("Vertical slider background")]
        private Sprite _verticalSliderBg;

        [SerializeField, Tooltip("Vertical slider background left edge")]
        private Sprite _verticalSliderBgLeft;

        [SerializeField, Tooltip("Vertical slider background right edge")]
        private Sprite _verticalSliderBgRight;

        [SerializeField, Tooltip("Vertical slider fill border")]
        private Sprite _verticalSliderFillBorder;

        // ============================================
        // Party Slot Styling
        // ============================================

        [Header("Party Slot (Manual Assignment)")]
        [SerializeField, Tooltip("Frame for party member portrait (user assigns in Inspector)")]
        private Sprite _partySlotFrame;

        [SerializeField, Tooltip("Glow when Art is ready (user assigns in Inspector)")]
        private Sprite _partySlotActiveGlow;

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
        // ProfileFrame Accessors
        // ============================================

        public Sprite ProfileFrameBg => _profileFrameBg;
        public Sprite ProfileFrameBorder => _profileFrameBorder;
        public Sprite ProfileFrameBorderDecoPurple => _profileFrameBorderDecoPurple;

        // ============================================
        // CardFrame Accessors
        // ============================================

        public Sprite CardFrameRectanglePurpleBg => _cardFrameRectanglePurpleBg;
        public Sprite CardFrameRectangleGreenBg => _cardFrameRectangleGreenBg;

        // ============================================
        // ListFrame Accessors
        // ============================================

        public Sprite ListFrameBg => _listFrameBg;
        public Sprite ListFrameBorder => _listFrameBorder;

        // ============================================
        // PanelFrame Accessors
        // ============================================

        public Sprite PanelFrameBg => _panelFrameBg;
        public Sprite PanelFrameBorder => _panelFrameBorder;
        public Sprite PanelFrameDeco => _panelFrameDeco;

        // ============================================
        // ItemFrame Accessors
        // ============================================

        public Sprite ItemFrameSquarePurple => _itemFrameSquarePurple;

        // ============================================
        // BaseFrame Accessors
        // ============================================

        public Sprite BaseFrameBorderRectH40 => _baseFrameBorderRectH40;
        public Sprite BaseFrameBorderRectH50 => _baseFrameBorderRectH50;
        public Sprite BaseFrameBorderRectH60 => _baseFrameBorderRectH60;

        // ============================================
        // LineFrame Accessors
        // ============================================

        public Sprite LineFrameDecoLine01 => _lineFrameDecoLine01;

        // ============================================
        // Slider Accessors
        // ============================================

        public Sprite SliderBorderTaperedBg => _sliderBorderTaperedBg;
        public Sprite SliderBorderTaperedFill => _sliderBorderTaperedFill;
        public Sprite SliderBorderTaperedBorder => _sliderBorderTaperedBorder;

        // ============================================
        // Vertical Slider Accessors
        // ============================================

        public Sprite VerticalSliderBg => _verticalSliderBg;
        public Sprite VerticalSliderBgLeft => _verticalSliderBgLeft;
        public Sprite VerticalSliderBgRight => _verticalSliderBgRight;
        public Sprite VerticalSliderFillBorder => _verticalSliderFillBorder;

        // ============================================
        // Party Slot Accessors
        // ============================================

        public Sprite PartySlotFrame => _partySlotFrame;
        public Sprite PartySlotActiveGlow => _partySlotActiveGlow;

        // ============================================
        // PictoIcon Accessors
        // ============================================

        public Sprite PictoIconTimer => _pictoIconTimer;
        public Sprite PictoIconAttack => _pictoIconAttack;
        public Sprite PictoIconMenu => _pictoIconMenu;
        public Sprite PictoIconBook => _pictoIconBook;
        public Sprite PictoIconBattle => _pictoIconBattle;
        public Sprite PictoIconLock => _pictoIconLock;

        // ============================================
        // ItemIcon Accessors
        // ============================================

        public Sprite ItemIconHeart => _itemIconHeart;
        public Sprite ItemIconEnergyPurple => _itemIconEnergyPurple;
        public Sprite ItemIconShield => _itemIconShield;
        public Sprite ItemIconCurrency => _itemIconCurrency;

        // ============================================
        // Map Legend Icon Accessors
        // ============================================

        public Sprite MapIconCombat => _mapIconCombat;
        public Sprite MapIconElite => _mapIconElite;
        public Sprite MapIconShop => _mapIconShop;
        public Sprite MapIconEcho => _mapIconEcho;
        public Sprite MapIconSanctuary => _mapIconSanctuary;
        public Sprite MapIconTreasure => _mapIconTreasure;
        public Sprite MapIconBoss => _mapIconBoss;

        // ============================================
        // Deck Display Icon Accessors
        // ============================================

        public Sprite DeckIconDraw => _deckIconDraw;
        public Sprite DeckIconDiscard => _deckIconDiscard;

        // ============================================
        // Speed/System Icon Accessors
        // ============================================

        public Sprite SpeedIcon1x => _speedIcon1x;
        public Sprite SpeedIcon2x => _speedIcon2x;
        public Sprite IconCheckmark => _iconCheckmark;

        // ============================================
        // Settings Category Icon Accessors
        // ============================================

        public Sprite SettingsIconDisplay => _settingsIconDisplay;
        public Sprite SettingsIconAudio => _settingsIconAudio;
        public Sprite SettingsIconGame => _settingsIconGame;
        public Sprite SettingsIconNetwork => _settingsIconNetwork;
        public Sprite SettingsIconAccount => _settingsIconAccount;

        // ============================================
        // Alert Icon Accessors
        // ============================================

        public Sprite AlertDiamondWhiteBg => _alertDiamondWhiteBg;

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
        /// Gets profile frame sprites.
        /// </summary>
        /// <returns>Tuple of (bg, border, borderDecoPurple) sprites</returns>
        public (Sprite bg, Sprite border, Sprite borderDecoPurple) GetProfileFrameSprites()
        {
            return (_profileFrameBg, _profileFrameBorder, _profileFrameBorderDecoPurple);
        }

        /// <summary>
        /// Gets card frame rectangle sprites for a specific color.
        /// </summary>
        /// <param name="color">"purple" or "green"</param>
        /// <returns>The card frame sprite for that color</returns>
        public Sprite GetCardFrameRectangleSprite(string color)
        {
            return color?.ToLower() switch
            {
                "purple" => _cardFrameRectanglePurpleBg,
                "green" => _cardFrameRectangleGreenBg,
                _ => _cardFrameRectanglePurpleBg
            };
        }

        /// <summary>
        /// Gets list frame sprites.
        /// </summary>
        /// <returns>Tuple of (bg, border) sprites</returns>
        public (Sprite bg, Sprite border) GetListFrameSprites()
        {
            return (_listFrameBg, _listFrameBorder);
        }

        /// <summary>
        /// Gets panel frame sprites with bottom decoration.
        /// </summary>
        /// <returns>Tuple of (bg, border, deco) sprites</returns>
        public (Sprite bg, Sprite border, Sprite deco) GetPanelFrameSprites()
        {
            return (_panelFrameBg, _panelFrameBorder, _panelFrameDeco);
        }

        /// <summary>
        /// Gets slider sprites.
        /// </summary>
        /// <returns>Tuple of (bg, fill, border) sprites</returns>
        public (Sprite bg, Sprite fill, Sprite border) GetSliderSprites()
        {
            return (_sliderBorderTaperedBg, _sliderBorderTaperedFill, _sliderBorderTaperedBorder);
        }

        /// <summary>
        /// Gets vertical slider sprites for SE gauge.
        /// </summary>
        /// <returns>Tuple of (bg, bgLeft, bgRight, fillBorder) sprites</returns>
        public (Sprite bg, Sprite bgLeft, Sprite bgRight, Sprite fillBorder) GetVerticalSliderSprites()
        {
            return (_verticalSliderBg, _verticalSliderBgLeft, _verticalSliderBgRight, _verticalSliderFillBorder);
        }

        /// <summary>
        /// Gets party slot sprites.
        /// </summary>
        /// <returns>Tuple of (frame, activeGlow) sprites</returns>
        public (Sprite frame, Sprite activeGlow) GetPartySlotSprites()
        {
            return (_partySlotFrame, _partySlotActiveGlow);
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

            // ProfileFrame
            if (_profileFrameBg != null) count++;
            if (_profileFrameBorder != null) count++;
            if (_profileFrameBorderDecoPurple != null) count++;

            // CardFrame
            if (_cardFrameRectanglePurpleBg != null) count++;
            if (_cardFrameRectangleGreenBg != null) count++;

            // ListFrame
            if (_listFrameBg != null) count++;
            if (_listFrameBorder != null) count++;

            // PanelFrame
            if (_panelFrameBg != null) count++;
            if (_panelFrameBorder != null) count++;
            if (_panelFrameDeco != null) count++;

            // ItemFrame
            if (_itemFrameSquarePurple != null) count++;

            // BaseFrame
            if (_baseFrameBorderRectH40 != null) count++;
            if (_baseFrameBorderRectH50 != null) count++;
            if (_baseFrameBorderRectH60 != null) count++;

            // LineFrame
            if (_lineFrameDecoLine01 != null) count++;

            // Slider
            if (_sliderBorderTaperedBg != null) count++;
            if (_sliderBorderTaperedFill != null) count++;
            if (_sliderBorderTaperedBorder != null) count++;

            // PictoIcons
            if (_pictoIconTimer != null) count++;
            if (_pictoIconAttack != null) count++;
            if (_pictoIconMenu != null) count++;
            if (_pictoIconBook != null) count++;
            if (_pictoIconBattle != null) count++;
            if (_pictoIconLock != null) count++;

            // ItemIcons
            if (_itemIconHeart != null) count++;
            if (_itemIconEnergyPurple != null) count++;
            if (_itemIconShield != null) count++;
            if (_itemIconCurrency != null) count++;

            // Map Legend Icons
            if (_mapIconCombat != null) count++;
            if (_mapIconElite != null) count++;
            if (_mapIconShop != null) count++;
            if (_mapIconEcho != null) count++;
            if (_mapIconSanctuary != null) count++;
            if (_mapIconTreasure != null) count++;
            if (_mapIconBoss != null) count++;

            // Deck Display Icons
            if (_deckIconDraw != null) count++;
            if (_deckIconDiscard != null) count++;

            // Speed/System Icons
            if (_speedIcon1x != null) count++;
            if (_speedIcon2x != null) count++;
            if (_iconCheckmark != null) count++;

            // Settings Category Icons
            if (_settingsIconDisplay != null) count++;
            if (_settingsIconAudio != null) count++;
            if (_settingsIconGame != null) count++;
            if (_settingsIconNetwork != null) count++;
            if (_settingsIconAccount != null) count++;

            // Icons
            if (_iconSettings != null) count++;
            if (_iconBack != null) count++;

            // Font
            if (_fontAfacadFlux != null) count++;

            // Vertical Slider
            if (_verticalSliderBg != null) count++;
            if (_verticalSliderBgLeft != null) count++;
            if (_verticalSliderBgRight != null) count++;
            if (_verticalSliderFillBorder != null) count++;

            // Party Slot
            if (_partySlotFrame != null) count++;
            if (_partySlotActiveGlow != null) count++;

            // Alert Icons
            if (_alertDiamondWhiteBg != null) count++;

            return count;
        }

        /// <summary>
        /// Gets the total number of sprite slots.
        /// </summary>
        public int GetTotalSlots() => 78; // Previous 77 + 1 alert icon

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
