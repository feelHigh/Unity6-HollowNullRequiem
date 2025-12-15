// ============================================
// UIColors.cs
// Central UI color palette for consistent theming
// ============================================

using UnityEngine;
using HNR.Cards;
using HNR.Characters;

namespace HNR.UI
{
    /// <summary>
    /// Central color palette for UI consistency.
    /// Based on Hollow Null Requiem visual identity.
    /// </summary>
    public static class UIColors
    {
        // ============================================
        // Primary Palette (Soul Aspects)
        // ============================================

        /// <summary>Cyan - Primary accent color (Soul Aspect: Void)</summary>
        public static readonly Color SoulCyan = new Color(0.2f, 0.85f, 0.95f, 1f);

        /// <summary>Purple - Secondary accent (Soul Aspect: Corruption)</summary>
        public static readonly Color SoulPurple = new Color(0.6f, 0.3f, 0.9f, 1f);

        /// <summary>Orange - Tertiary accent (Soul Aspect: Flame)</summary>
        public static readonly Color SoulOrange = new Color(1f, 0.5f, 0.2f, 1f);

        /// <summary>Green - Quaternary accent (Soul Aspect: Nature)</summary>
        public static readonly Color SoulGreen = new Color(0.4f, 0.9f, 0.4f, 1f);

        /// <summary>Blue - Quinary accent (Soul Aspect: Frost)</summary>
        public static readonly Color SoulBlue = new Color(0.3f, 0.5f, 1f, 1f);

        // ============================================
        // UI State Colors
        // ============================================

        /// <summary>Normal/Default state</summary>
        public static readonly Color Normal = new Color(0.8f, 0.8f, 0.8f, 1f);

        /// <summary>Highlighted/Hover state</summary>
        public static readonly Color Highlighted = new Color(1f, 1f, 1f, 1f);

        /// <summary>Pressed/Active state</summary>
        public static readonly Color Pressed = new Color(0.6f, 0.6f, 0.6f, 1f);

        /// <summary>Disabled state</summary>
        public static readonly Color Disabled = new Color(0.4f, 0.4f, 0.4f, 0.5f);

        /// <summary>Selected state</summary>
        public static readonly Color Selected = SoulCyan;

        // ============================================
        // Feedback Colors
        // ============================================

        /// <summary>Positive feedback (heal, success)</summary>
        public static readonly Color Positive = new Color(0.2f, 0.9f, 0.4f, 1f);

        /// <summary>Negative feedback (damage, error)</summary>
        public static readonly Color Negative = new Color(0.9f, 0.2f, 0.2f, 1f);

        /// <summary>Warning feedback</summary>
        public static readonly Color Warning = new Color(1f, 0.7f, 0.2f, 1f);

        /// <summary>Info feedback</summary>
        public static readonly Color Info = new Color(0.3f, 0.7f, 1f, 1f);

        // ============================================
        // Combat Colors
        // ============================================

        /// <summary>HP bar fill</summary>
        public static readonly Color HPBar = new Color(0.8f, 0.2f, 0.2f, 1f);

        /// <summary>Block/Shield display</summary>
        public static readonly Color Block = new Color(0.3f, 0.6f, 0.9f, 1f);

        /// <summary>Corruption bar fill</summary>
        public static readonly Color Corruption = SoulPurple;

        /// <summary>AP available</summary>
        public static readonly Color APFull = SoulCyan;

        /// <summary>AP spent</summary>
        public static readonly Color APEmpty = new Color(0.2f, 0.2f, 0.2f, 0.5f);

        /// <summary>Soul Essence gauge</summary>
        public static readonly Color SoulEssence = new Color(1f, 0.8f, 0.2f, 1f);

        // ============================================
        // Background Colors
        // ============================================

        /// <summary>Dark panel background</summary>
        public static readonly Color PanelDark = new Color(0.1f, 0.1f, 0.12f, 0.95f);

        /// <summary>Medium panel background</summary>
        public static readonly Color PanelMedium = new Color(0.15f, 0.15f, 0.18f, 0.95f);

        /// <summary>Light panel background</summary>
        public static readonly Color PanelLight = new Color(0.2f, 0.2f, 0.25f, 0.9f);

        /// <summary>Overlay/Modal background</summary>
        public static readonly Color Overlay = new Color(0f, 0f, 0f, 0.75f);

        // ============================================
        // Card Rarity Colors
        // ============================================

        /// <summary>Common card border</summary>
        public static readonly Color RarityCommon = new Color(0.6f, 0.6f, 0.6f, 1f);

        /// <summary>Uncommon card border</summary>
        public static readonly Color RarityUncommon = new Color(0.2f, 0.8f, 0.4f, 1f);

        /// <summary>Rare card border</summary>
        public static readonly Color RarityRare = new Color(0.3f, 0.5f, 1f, 1f);

        /// <summary>Epic card border</summary>
        public static readonly Color RarityEpic = new Color(0.7f, 0.3f, 0.9f, 1f);

        /// <summary>Legendary card border</summary>
        public static readonly Color RarityLegendary = new Color(1f, 0.7f, 0.2f, 1f);

        // ============================================
        // Utility Methods
        // ============================================

        /// <summary>
        /// Get color for a Soul Aspect.
        /// </summary>
        public static Color GetAspectColor(SoulAspect aspect)
        {
            return aspect switch
            {
                SoulAspect.Void => SoulCyan,
                SoulAspect.Flame => SoulOrange,
                SoulAspect.Frost => SoulBlue,
                SoulAspect.Nature => SoulGreen,
                SoulAspect.Shadow => SoulPurple,
                _ => Normal
            };
        }

        /// <summary>
        /// Get color for card rarity.
        /// </summary>
        public static Color GetRarityColor(CardRarity rarity)
        {
            return rarity switch
            {
                CardRarity.Common => RarityCommon,
                CardRarity.Uncommon => RarityUncommon,
                CardRarity.Rare => RarityRare,
                CardRarity.Epic => RarityEpic,
                CardRarity.Legendary => RarityLegendary,
                _ => RarityCommon
            };
        }

        /// <summary>
        /// Create a faded version of a color.
        /// </summary>
        public static Color WithAlpha(Color color, float alpha)
        {
            return new Color(color.r, color.g, color.b, alpha);
        }

        /// <summary>
        /// Brighten a color.
        /// </summary>
        public static Color Brighten(Color color, float amount = 0.2f)
        {
            return new Color(
                Mathf.Min(1f, color.r + amount),
                Mathf.Min(1f, color.g + amount),
                Mathf.Min(1f, color.b + amount),
                color.a
            );
        }

        /// <summary>
        /// Darken a color.
        /// </summary>
        public static Color Darken(Color color, float amount = 0.2f)
        {
            return new Color(
                Mathf.Max(0f, color.r - amount),
                Mathf.Max(0f, color.g - amount),
                Mathf.Max(0f, color.b - amount),
                color.a
            );
        }
    }
}
