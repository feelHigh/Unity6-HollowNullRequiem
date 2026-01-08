// ============================================
// CardFrameSet.cs
// Serializable structure for card frame sprite variants
// ============================================

using System;
using UnityEngine;

namespace HNR.UI.Config
{
    /// <summary>
    /// Sprite set for a single card type with all frame variants.
    /// Pre-colored sprites from GUI Pro-FantasyHero assets.
    /// </summary>
    [Serializable]
    public struct CardFrameSet
    {
        [Tooltip("Background fill sprite (CardFrame_Rectangle_01_[Color]_Bg)")]
        public Sprite Background;

        [Tooltip("Standard border for Common/Uncommon (CardFrame_Rectangle_01_[Color]_Border)")]
        public Sprite Border;

        [Tooltip("Gem border for Rare/Legendary (CardFrame_Rectangle_01_[Color]_BorderGem)")]
        public Sprite BorderGem;

        [Tooltip("Tint color for cost frame and other tintable elements")]
        public Color TintColor;

        /// <summary>
        /// Creates a CardFrameSet with specified tint color.
        /// </summary>
        public static CardFrameSet Create(Color tintColor)
        {
            return new CardFrameSet
            {
                Background = null,
                Border = null,
                BorderGem = null,
                TintColor = tintColor
            };
        }

        /// <summary>
        /// Checks if all required sprites are assigned.
        /// </summary>
        public bool IsValid()
        {
            return Background != null && Border != null && BorderGem != null;
        }
    }
}
