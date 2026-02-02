// ============================================
// UIColors.cs
// Centralized color palette for HNR UI
// ============================================

using UnityEngine;
using HNR.Cards;

namespace HNR.UI
{
    /// <summary>
    /// Centralized color palette for HNR UI.
    /// Based on TDD 08 "Dark Fantasy" aesthetic.
    /// </summary>
    public static class UIColors
    {
        // ============================================
        // Base Colors
        // ============================================

        /// <summary>Primary background. Hex: #0A0A0A</summary>
        public static readonly Color32 VoidBlack = new(10, 10, 10, 255);

        /// <summary>Secondary background. Hex: #111122</summary>
        public static readonly Color32 AbyssBlue = new(17, 17, 34, 255);

        /// <summary>Panel background. Hex: #1A1A2E</summary>
        public static readonly Color32 PanelGray = new(26, 26, 46, 255);

        /// <summary>Dark background for panels. Hex: #1F3A5F</summary>
        public static readonly Color32 NavyBackground = new(31, 58, 95, 255);

        /// <summary>Overlay background. Hex: #4B2D6E</summary>
        public static readonly Color32 DeepViolet = new(75, 45, 110, 255);

        // ============================================
        // Interactive Accents
        // ============================================

        /// <summary>Safe interactions. Hex: #00D4E4</summary>
        public static readonly Color32 SoulCyan = new(0, 212, 228, 255);

        /// <summary>Danger/Corruption base. Hex: #8B0000</summary>
        public static readonly Color32 CorruptionRed = new(139, 0, 0, 255);

        /// <summary>Active corruption glow. Hex: #FF4444</summary>
        public static readonly Color32 CorruptionGlow = new(255, 68, 68, 255);

        /// <summary>Rarity/Premium accents. Hex: #D4AF37</summary>
        public static readonly Color32 SoulGold = new(212, 175, 55, 255);

        /// <summary>Hollow Corruption gauge. Hex: #6B3FA0</summary>
        public static readonly Color32 HollowViolet = new(107, 63, 160, 255);

        // ============================================
        // Soul Aspects
        // ============================================

        /// <summary>Flame aspect color. Hex: #FF6B35</summary>
        public static readonly Color32 FlameAspect = new(255, 107, 53, 255);

        /// <summary>Shadow aspect color. Hex: #4A0E4E</summary>
        public static readonly Color32 ShadowAspect = new(74, 14, 78, 255);

        /// <summary>Nature aspect color. Hex: #2D5A27</summary>
        public static readonly Color32 NatureAspect = new(45, 90, 39, 255);

        /// <summary>Arcane aspect color. Hex: #5B2C6F</summary>
        public static readonly Color32 ArcaneAspect = new(91, 44, 111, 255);

        /// <summary>Light aspect color. Hex: #F4D03F</summary>
        public static readonly Color32 LightAspect = new(244, 208, 63, 255);

        // ============================================
        // Rarity Colors
        // ============================================

        /// <summary>Common rarity. Gray.</summary>
        public static readonly Color32 CommonRarity = new(150, 150, 150, 255);

        /// <summary>Uncommon rarity. Green.</summary>
        public static readonly Color32 UncommonRarity = new(0, 180, 80, 255);

        /// <summary>Rare rarity. Blue.</summary>
        public static readonly Color32 RareRarity = new(0, 120, 255, 255);

        /// <summary>Legendary rarity. Gold.</summary>
        public static readonly Color32 LegendaryRarity = new(255, 180, 0, 255);

        // ============================================
        // Helper Methods
        // ============================================

        /// <summary>
        /// Get the color associated with a Soul Aspect.
        /// </summary>
        /// <param name="aspect">The soul aspect.</param>
        /// <returns>Color for the aspect.</returns>
        public static Color GetAspectColor(SoulAspect aspect) => aspect switch
        {
            SoulAspect.Flame => FlameAspect,
            SoulAspect.Shadow => ShadowAspect,
            SoulAspect.Nature => NatureAspect,
            SoulAspect.Arcane => ArcaneAspect,
            SoulAspect.Light => LightAspect,
            _ => SoulCyan
        };

        /// <summary>
        /// Get the color associated with a Card Rarity.
        /// </summary>
        /// <param name="rarity">The card rarity.</param>
        /// <returns>Color for the rarity.</returns>
        public static Color GetRarityColor(CardRarity rarity) => rarity switch
        {
            CardRarity.Common => CommonRarity,
            CardRarity.Uncommon => UncommonRarity,
            CardRarity.Rare => RareRarity,
            CardRarity.Legendary => LegendaryRarity,
            _ => CommonRarity
        };
    }
}
