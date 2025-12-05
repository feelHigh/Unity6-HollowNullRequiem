// ============================================
// QualityTier.cs
// Device-based quality tier enumeration
// ============================================

namespace HNR.Core
{
    /// <summary>
    /// Quality tier for device-based settings.
    /// Determines target frame rate and visual fidelity.
    /// </summary>
    /// <remarks>
    /// Device tiers (TDD 10):
    /// - Low: 3GB RAM, SD 660 → 30fps (e.g., Galaxy A50)
    /// - Mid: 4GB RAM, SD 730 → 45fps (e.g., Pixel 4a)
    /// - High: 6GB+ RAM, SD 855+ → 60fps (e.g., Galaxy S20)
    /// </remarks>
    public enum QualityTier
    {
        /// <summary>
        /// Low-end devices: 3GB RAM, Snapdragon 660 class.
        /// Target: 30fps. Example: Galaxy A50.
        /// </summary>
        Low = 0,

        /// <summary>
        /// Mid-range devices: 4GB RAM, Snapdragon 730 class.
        /// Target: 45fps. Example: Pixel 4a.
        /// </summary>
        Mid = 1,

        /// <summary>
        /// High-end devices: 6GB+ RAM, Snapdragon 855+ class.
        /// Target: 60fps. Example: Galaxy S20.
        /// </summary>
        High = 2
    }
}
