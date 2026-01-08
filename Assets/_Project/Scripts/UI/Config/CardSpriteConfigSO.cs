// ============================================
// CardSpriteConfigSO.cs
// Configuration for card visual sprites by CardType and CardRarity
// ============================================

using System.Collections.Generic;
using UnityEngine;
using HNR.Cards;

namespace HNR.UI.Config
{
    /// <summary>
    /// Configuration for card frame sprites based on CardType and CardRarity.
    /// Pre-colored frames from GUI Pro-FantasyHero eliminate runtime tinting.
    /// </summary>
    [CreateAssetMenu(fileName = "CardSpriteConfig", menuName = "HNR/Config/Card Sprite Config")]
    public class CardSpriteConfigSO : ScriptableObject
    {
        // ============================================
        // Frame Sprite Sets (by CardType)
        // ============================================

        [Header("Strike Card Sprites (Red)")]
        [SerializeField, Tooltip("Frame sprites for Strike cards")]
        private CardFrameSet _strikeFrames;

        [Header("Guard Card Sprites (Blue)")]
        [SerializeField, Tooltip("Frame sprites for Guard cards")]
        private CardFrameSet _guardFrames;

        [Header("Skill Card Sprites (Green)")]
        [SerializeField, Tooltip("Frame sprites for Skill cards")]
        private CardFrameSet _skillFrames;

        [Header("Power Card Sprites (Purple)")]
        [SerializeField, Tooltip("Frame sprites for Power cards")]
        private CardFrameSet _powerFrames;

        [Header("Special Card Sprites (Yellow - Reserved)")]
        [SerializeField, Tooltip("Frame sprites for special/upgraded cards")]
        private CardFrameSet _specialFrames;

        // ============================================
        // Cost Frame Components
        // ============================================

        [Header("Cost Frame Sprites (Label_Flag_01)")]
        [SerializeField, Tooltip("Cost frame background sprite (white, tintable)")]
        private Sprite _costFrameBg;

        [SerializeField, Tooltip("Cost frame border sprite (white, tintable)")]
        private Sprite _costFrameBorder;

        [SerializeField, Tooltip("Cost frame gradient sprite (white, tintable)")]
        private Sprite _costFrameGradient;

        [SerializeField, Tooltip("Cost frame inner border sprite (white, tintable)")]
        private Sprite _costFrameInnerBorder;

        // ============================================
        // Lookup Cache
        // ============================================

        private Dictionary<CardType, CardFrameSet> _frameSetLookup;

        // ============================================
        // Public API - Frame Sets
        // ============================================

        /// <summary>
        /// Gets the frame sprite set for a CardType.
        /// </summary>
        public CardFrameSet GetFrameSet(CardType type)
        {
            BuildLookupIfNeeded();
            return _frameSetLookup.TryGetValue(type, out var set) ? set : _skillFrames;
        }

        /// <summary>
        /// Gets the appropriate border sprite based on CardType and CardRarity.
        /// Common/Uncommon use Border, Rare/Legendary use BorderGem.
        /// </summary>
        public Sprite GetBorderSprite(CardType type, CardRarity rarity)
        {
            var frameSet = GetFrameSet(type);
            return rarity >= CardRarity.Rare ? frameSet.BorderGem : frameSet.Border;
        }

        /// <summary>
        /// Gets the background sprite for a CardType.
        /// </summary>
        public Sprite GetBackgroundSprite(CardType type)
        {
            return GetFrameSet(type).Background;
        }

        /// <summary>
        /// Gets the tint color for cost frame elements.
        /// </summary>
        public Color GetCostFrameTint(CardType type)
        {
            return GetFrameSet(type).TintColor;
        }

        // ============================================
        // Public API - Cost Frame Sprites
        // ============================================

        /// <summary>Cost frame background sprite.</summary>
        public Sprite CostFrameBg => _costFrameBg;

        /// <summary>Cost frame border sprite.</summary>
        public Sprite CostFrameBorder => _costFrameBorder;

        /// <summary>Cost frame gradient sprite.</summary>
        public Sprite CostFrameGradient => _costFrameGradient;

        /// <summary>Cost frame inner border sprite.</summary>
        public Sprite CostFrameInnerBorder => _costFrameInnerBorder;

        // ============================================
        // Public API - Direct Accessors
        // ============================================

        public CardFrameSet StrikeFrames => _strikeFrames;
        public CardFrameSet GuardFrames => _guardFrames;
        public CardFrameSet SkillFrames => _skillFrames;
        public CardFrameSet PowerFrames => _powerFrames;
        public CardFrameSet SpecialFrames => _specialFrames;

        // ============================================
        // Private Methods
        // ============================================

        private void BuildLookupIfNeeded()
        {
            if (_frameSetLookup != null) return;

            _frameSetLookup = new Dictionary<CardType, CardFrameSet>
            {
                { CardType.Strike, _strikeFrames },
                { CardType.Guard, _guardFrames },
                { CardType.Skill, _skillFrames },
                { CardType.Power, _powerFrames }
            };
        }

        private void OnValidate()
        {
            _frameSetLookup = null; // Force rebuild on editor change
        }

        private void OnEnable()
        {
            _frameSetLookup = null; // Ensure fresh on load
        }

        // ============================================
        // Validation
        // ============================================

        /// <summary>
        /// Validates that all required sprites are assigned.
        /// </summary>
        /// <returns>List of validation issues (empty if valid).</returns>
        public List<string> Validate()
        {
            var issues = new List<string>();

            ValidateFrameSet("Strike", _strikeFrames, issues);
            ValidateFrameSet("Guard", _guardFrames, issues);
            ValidateFrameSet("Skill", _skillFrames, issues);
            ValidateFrameSet("Power", _powerFrames, issues);
            ValidateFrameSet("Special", _specialFrames, issues);

            if (_costFrameBg == null) issues.Add("Cost frame background not assigned");
            if (_costFrameBorder == null) issues.Add("Cost frame border not assigned");
            if (_costFrameGradient == null) issues.Add("Cost frame gradient not assigned");
            if (_costFrameInnerBorder == null) issues.Add("Cost frame inner border not assigned");

            return issues;
        }

        private void ValidateFrameSet(string name, CardFrameSet set, List<string> issues)
        {
            if (set.Background == null) issues.Add($"{name} background sprite not assigned");
            if (set.Border == null) issues.Add($"{name} border sprite not assigned");
            if (set.BorderGem == null) issues.Add($"{name} border gem sprite not assigned");
        }

        /// <summary>
        /// Checks if all required sprites are assigned.
        /// </summary>
        public bool IsValid()
        {
            return Validate().Count == 0;
        }
    }
}
