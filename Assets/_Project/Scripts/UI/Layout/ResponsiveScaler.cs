// ============================================
// ResponsiveScaler.cs
// Adapts UI scaling for different aspect ratios
// ============================================

using UnityEngine;
using UnityEngine.UI;

namespace HNR.UI
{
    /// <summary>
    /// Aspect ratio category for layout decisions.
    /// </summary>
    public enum AspectCategory
    {
        Tablet,      // 4:3 (1.33)
        Standard,    // 16:9 (1.78)
        Wide,        // 18:9 (2.0)
        UltraWide    // 21:9 (2.33)
    }

    /// <summary>
    /// Dynamically adjusts CanvasScaler match factor based on device aspect ratio.
    /// Ensures UI looks correct on all device types from tablets to tall phones.
    /// </summary>
    public class ResponsiveScaler : MonoBehaviour
    {
        // ============================================
        // References
        // ============================================

        [Header("References")]
        [SerializeField, Tooltip("Target CanvasScaler to adjust")]
        private CanvasScaler _canvasScaler;

        // ============================================
        // Base Resolution
        // ============================================

        [Header("Base Resolution")]
        [SerializeField, Tooltip("Reference resolution (16:9)")]
        private Vector2 _baseResolution = new(1920, 1080);

        // ============================================
        // Aspect Ratio Thresholds
        // ============================================

        [Header("Aspect Ratio Thresholds")]
        [SerializeField, Tooltip("Tablet aspect threshold (4:3 = 1.33)")]
        private float _tabletAspect = 1.5f;

        [SerializeField, Tooltip("Wide aspect threshold (18:9 = 2.0)")]
        private float _wideAspect = 2f;

        [SerializeField, Tooltip("Ultra-wide aspect threshold (21:9 = 2.33)")]
        private float _ultraWideAspect = 2.1f;

        // ============================================
        // Scale Adjustments
        // ============================================

        [Header("Match Width/Height Factors")]
        [SerializeField, Tooltip("Match factor for tablets (favor height)")]
        private float _tabletMatchFactor = 1f;

        [SerializeField, Tooltip("Match factor for standard 16:9")]
        private float _standardMatchFactor = 0.7f;

        [SerializeField, Tooltip("Match factor for wide screens")]
        private float _wideMatchFactor = 0.5f;

        [SerializeField, Tooltip("Match factor for ultra-wide screens")]
        private float _ultraWideMatchFactor = 0.3f;

        // ============================================
        // Debug
        // ============================================

        [Header("Debug")]
        [SerializeField, Tooltip("Log aspect ratio changes")]
        private bool _debugLog;

        // ============================================
        // Runtime State
        // ============================================

        private float _lastAspect;
        private AspectCategory _currentCategory;

        // ============================================
        // Properties
        // ============================================

        /// <summary>
        /// Current device aspect ratio.
        /// </summary>
        public float AspectRatio => (float)Screen.width / Screen.height;

        /// <summary>
        /// Current aspect ratio category.
        /// </summary>
        public AspectCategory CurrentCategory => _currentCategory;

        /// <summary>
        /// Whether device is ultra-wide (21:9 or wider).
        /// </summary>
        public bool IsUltraWide => AspectRatio >= _ultraWideAspect;

        /// <summary>
        /// Whether device is wide (18:9 or wider).
        /// </summary>
        public bool IsWide => AspectRatio >= _wideAspect;

        /// <summary>
        /// Whether device is tablet aspect (4:3 range).
        /// </summary>
        public bool IsTablet => AspectRatio < _tabletAspect;

        // ============================================
        // Unity Lifecycle
        // ============================================

        private void Awake()
        {
            if (_canvasScaler == null)
                _canvasScaler = GetComponent<CanvasScaler>();

            if (_canvasScaler != null)
            {
                _canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                _canvasScaler.referenceResolution = _baseResolution;
                _canvasScaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            }
        }

        private void Start()
        {
            UpdateScaling();
        }

        private void Update()
        {
            float currentAspect = AspectRatio;
            if (!Mathf.Approximately(currentAspect, _lastAspect))
            {
                UpdateScaling();
            }
        }

        // ============================================
        // Scaling Logic
        // ============================================

        private void UpdateScaling()
        {
            if (_canvasScaler == null) return;

            float aspect = AspectRatio;
            _lastAspect = aspect;

            // Determine category and match factor
            float match;
            if (aspect >= _ultraWideAspect)
            {
                _currentCategory = AspectCategory.UltraWide;
                match = _ultraWideMatchFactor;
            }
            else if (aspect >= _wideAspect)
            {
                _currentCategory = AspectCategory.Wide;
                match = _wideMatchFactor;
            }
            else if (aspect < _tabletAspect)
            {
                _currentCategory = AspectCategory.Tablet;
                match = _tabletMatchFactor;
            }
            else
            {
                _currentCategory = AspectCategory.Standard;
                match = _standardMatchFactor;
            }

            _canvasScaler.matchWidthOrHeight = match;

            if (_debugLog)
            {
                Debug.Log($"[ResponsiveScaler] Aspect: {aspect:F2}, Category: {_currentCategory}, Match: {match:F2}");
            }
        }

        // ============================================
        // Public Methods
        // ============================================

        /// <summary>
        /// Force refresh scaling calculation.
        /// </summary>
        public void Refresh()
        {
            _lastAspect = 0; // Force update
            UpdateScaling();
        }

        /// <summary>
        /// Get a scale multiplier for UI elements that need aspect-specific sizing.
        /// </summary>
        /// <returns>Scale multiplier (1.0 for standard, adjusted for other aspects).</returns>
        public float GetUIScaleMultiplier()
        {
            return _currentCategory switch
            {
                AspectCategory.Tablet => 0.9f,
                AspectCategory.UltraWide => 1.1f,
                AspectCategory.Wide => 1.05f,
                _ => 1f
            };
        }

        /// <summary>
        /// Get recommended card hand width percentage based on aspect ratio.
        /// Wider screens can fit more cards horizontally.
        /// </summary>
        public float GetHandWidthPercent()
        {
            return _currentCategory switch
            {
                AspectCategory.Tablet => 0.95f,
                AspectCategory.UltraWide => 0.7f,
                AspectCategory.Wide => 0.75f,
                _ => 0.85f
            };
        }

        /// <summary>
        /// Get recommended vertical offset for hand position.
        /// Taller screens need the hand higher to stay visible.
        /// </summary>
        public float GetHandVerticalOffset()
        {
            return _currentCategory switch
            {
                AspectCategory.Tablet => 0f,
                AspectCategory.UltraWide => 50f,
                AspectCategory.Wide => 30f,
                _ => 0f
            };
        }

        // ============================================
        // Editor
        // ============================================

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (_canvasScaler != null && Application.isPlaying)
            {
                UpdateScaling();
            }
        }
#endif
    }
}
