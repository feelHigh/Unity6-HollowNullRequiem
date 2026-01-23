// ============================================
// BannerConfigSO.cs
// ScriptableObject configuration for event banner carousel
// ============================================

using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace HNR.UI.Config
{
    /// <summary>
    /// ScriptableObject that configures the event banner carousel.
    /// Stores banner slides and carousel behavior settings.
    /// </summary>
    [CreateAssetMenu(fileName = "BannerConfig", menuName = "HNR/Config/Banner Config")]
    public class BannerConfigSO : ScriptableObject
    {
        // ============================================
        // Banner Slides
        // ============================================

        [Header("Banner Slides")]
        [SerializeField, Tooltip("List of banner slides to display")]
        private BannerSlide[] _banners;

        // ============================================
        // Carousel Behavior
        // ============================================

        [Header("Carousel Behavior")]
        [SerializeField, Tooltip("Seconds between auto-advance (0 to disable)")]
        [Range(0f, 10f)]
        private float _autoAdvanceInterval = 2f;

        [SerializeField, Tooltip("Duration of slide transition animation")]
        [Range(0.1f, 1f)]
        private float _transitionDuration = 0.3f;

        [SerializeField, Tooltip("Whether to loop from last slide to first")]
        private bool _enableLoop = true;

        [SerializeField, Tooltip("Pause auto-advance while user is interacting")]
        private bool _pauseOnInteraction = true;

        [SerializeField, Tooltip("Seconds to wait before resuming auto-advance after interaction")]
        [Range(0f, 5f)]
        private float _resumeDelay = 1.5f;

        // ============================================
        // Visual Settings
        // ============================================

        [Header("Visual Settings")]
        [SerializeField, Tooltip("Active page indicator color")]
        private Color _activeIndicatorColor = new Color(1f, 1f, 1f, 1f);

        [SerializeField, Tooltip("Inactive page indicator color")]
        private Color _inactiveIndicatorColor = new Color(1f, 1f, 1f, 0.4f);

        [SerializeField, Tooltip("Size of page indicator dots")]
        [Range(8f, 24f)]
        private float _indicatorSize = 12f;

        [SerializeField, Tooltip("Spacing between page indicators")]
        [Range(4f, 20f)]
        private float _indicatorSpacing = 8f;

        // ============================================
        // Public Accessors - Slides
        // ============================================

        /// <summary>All configured banner slides</summary>
        public BannerSlide[] Banners => _banners;

        /// <summary>Number of configured banners</summary>
        public int BannerCount => _banners?.Length ?? 0;

        // ============================================
        // Public Accessors - Behavior
        // ============================================

        /// <summary>Auto-advance interval in seconds (0 = disabled)</summary>
        public float AutoAdvanceInterval => _autoAdvanceInterval;

        /// <summary>Transition animation duration in seconds</summary>
        public float TransitionDuration => _transitionDuration;

        /// <summary>Whether carousel loops from last to first slide</summary>
        public bool EnableLoop => _enableLoop;

        /// <summary>Whether to pause auto-advance during user interaction</summary>
        public bool PauseOnInteraction => _pauseOnInteraction;

        /// <summary>Delay before resuming auto-advance after interaction</summary>
        public float ResumeDelay => _resumeDelay;

        // ============================================
        // Public Accessors - Visual
        // ============================================

        /// <summary>Active page indicator color</summary>
        public Color ActiveIndicatorColor => _activeIndicatorColor;

        /// <summary>Inactive page indicator color</summary>
        public Color InactiveIndicatorColor => _inactiveIndicatorColor;

        /// <summary>Page indicator dot size</summary>
        public float IndicatorSize => _indicatorSize;

        /// <summary>Spacing between page indicators</summary>
        public float IndicatorSpacing => _indicatorSpacing;

        // ============================================
        // Public API
        // ============================================

        /// <summary>
        /// Gets a banner slide by index.
        /// </summary>
        /// <param name="index">Banner index</param>
        /// <returns>Banner slide or null if out of range</returns>
        public BannerSlide GetBanner(int index)
        {
            if (_banners == null || index < 0 || index >= _banners.Length)
                return null;
            return _banners[index];
        }

        /// <summary>
        /// Checks if any banners are configured.
        /// </summary>
        /// <returns>True if at least one banner exists</returns>
        public bool HasBanners()
        {
            return _banners != null && _banners.Length > 0;
        }

        /// <summary>
        /// Checks if the configuration is valid for display.
        /// </summary>
        /// <returns>True if config has at least one active banner</returns>
        public bool IsValid()
        {
            return HasBanners() && GetActiveCount() > 0;
        }

        /// <summary>
        /// Gets the count of active banners.
        /// </summary>
        /// <returns>Number of active banner slides</returns>
        public int GetActiveCount()
        {
            if (_banners == null) return 0;
            return _banners.Count(b => b != null && b.IsActive);
        }

        /// <summary>
        /// Gets all active banners, sorted by priority.
        /// </summary>
        /// <returns>Active banners in priority order</returns>
        public IEnumerable<BannerSlide> GetActiveBanners()
        {
            if (_banners == null) return Enumerable.Empty<BannerSlide>();

            return _banners
                .Where(b => b != null && b.IsActive && b.IsWithinSchedule())
                .OrderBy(b => b.Priority);
        }

        /// <summary>
        /// Gets all active banners as an array.
        /// </summary>
        /// <returns>Array of active banners in priority order</returns>
        public BannerSlide[] GetActiveBannersArray()
        {
            return GetActiveBanners().ToArray();
        }

        /// <summary>
        /// Checks if auto-advance is enabled.
        /// </summary>
        /// <returns>True if auto-advance interval is greater than 0</returns>
        public bool IsAutoAdvanceEnabled()
        {
            return _autoAdvanceInterval > 0f;
        }

        /// <summary>
        /// Gets the count of banners with assigned images.
        /// </summary>
        /// <returns>Number of banners with images</returns>
        public int GetBannersWithImageCount()
        {
            if (_banners == null) return 0;
            return _banners.Count(b => b != null && b.HasImage);
        }

        /// <summary>
        /// Validates the configuration and logs any issues.
        /// </summary>
        /// <returns>True if no validation errors</returns>
        public bool Validate()
        {
            bool isValid = true;

            if (!HasBanners())
            {
                Debug.LogWarning("[BannerConfigSO] No banners configured");
                return false;
            }

            int activeCount = GetActiveCount();
            if (activeCount == 0)
            {
                Debug.LogWarning("[BannerConfigSO] No active banners - all banners are disabled or outside schedule");
                isValid = false;
            }

            int imageCount = GetBannersWithImageCount();
            if (imageCount == 0)
            {
                Debug.LogWarning("[BannerConfigSO] No banners have images assigned - will show text placeholders");
            }
            else if (imageCount < activeCount)
            {
                Debug.LogWarning($"[BannerConfigSO] Only {imageCount} of {activeCount} active banners have images");
            }

            return isValid;
        }
    }
}
