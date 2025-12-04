// ============================================
// SafeAreaHandler.cs
// Adapts UI to device safe area (notch/cutout)
// ============================================

using UnityEngine;

namespace HNR.UI
{
    /// <summary>
    /// Adjusts RectTransform anchors to fit within device safe area.
    /// Handles notches, cutouts, and rounded corners on modern devices.
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public class SafeAreaHandler : MonoBehaviour
    {
        // ============================================
        // Edge Application Settings
        // ============================================

        [Header("Edge Application")]
        [SerializeField, Tooltip("Apply safe area to top edge (notch area)")]
        private bool _applyTop = true;

        [SerializeField, Tooltip("Apply safe area to bottom edge (home indicator)")]
        private bool _applyBottom = true;

        [SerializeField, Tooltip("Apply safe area to left edge")]
        private bool _applyLeft = true;

        [SerializeField, Tooltip("Apply safe area to right edge")]
        private bool _applyRight = true;

        // ============================================
        // Debug Settings
        // ============================================

        [Header("Debug (Editor Only)")]
        [SerializeField, Tooltip("Simulate a device notch in editor")]
        private bool _simulateNotch;

        [SerializeField, Tooltip("Simulated notch height in pixels")]
        private float _simulatedNotchHeight = 80f;

        [SerializeField, Tooltip("Simulated home indicator height")]
        private float _simulatedBottomInset = 34f;

        [SerializeField, Tooltip("Log safe area changes")]
        private bool _debugLog;

        // ============================================
        // Runtime State
        // ============================================

        private RectTransform _rectTransform;
        private Rect _lastSafeArea;
        private Vector2Int _lastScreenSize;

        // ============================================
        // Properties
        // ============================================

        /// <summary>
        /// Current safe area being applied.
        /// </summary>
        public Rect CurrentSafeArea => _lastSafeArea;

        /// <summary>
        /// Whether safe area differs from full screen.
        /// </summary>
        public bool HasSafeAreaInsets => _lastSafeArea != new Rect(0, 0, Screen.width, Screen.height);

        // ============================================
        // Unity Lifecycle
        // ============================================

        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
        }

        private void Start()
        {
            ApplySafeArea();
        }

        private void Update()
        {
            // Check for safe area or screen size changes (orientation change)
            if (Screen.safeArea != _lastSafeArea ||
                Screen.width != _lastScreenSize.x ||
                Screen.height != _lastScreenSize.y)
            {
                ApplySafeArea();
            }
        }

        // ============================================
        // Public Methods
        // ============================================

        /// <summary>
        /// Force refresh of safe area application.
        /// </summary>
        public void Refresh()
        {
            ApplySafeArea();
        }

        /// <summary>
        /// Set which edges to apply safe area to.
        /// </summary>
        public void SetEdges(bool top, bool bottom, bool left, bool right)
        {
            _applyTop = top;
            _applyBottom = bottom;
            _applyLeft = left;
            _applyRight = right;
            ApplySafeArea();
        }

        // ============================================
        // Safe Area Application
        // ============================================

        private void ApplySafeArea()
        {
            var safeArea = GetSafeArea();
            _lastSafeArea = safeArea;
            _lastScreenSize = new Vector2Int(Screen.width, Screen.height);

            if (_debugLog)
            {
                Debug.Log($"[SafeAreaHandler] Screen: {Screen.width}x{Screen.height}, " +
                         $"SafeArea: {safeArea}");
            }

            // Convert safe area to anchor values (0-1 range)
            var anchorMin = safeArea.position;
            var anchorMax = safeArea.position + safeArea.size;

            anchorMin.x /= Screen.width;
            anchorMin.y /= Screen.height;
            anchorMax.x /= Screen.width;
            anchorMax.y /= Screen.height;

            // Apply selective edges
            if (!_applyLeft) anchorMin.x = 0f;
            if (!_applyBottom) anchorMin.y = 0f;
            if (!_applyRight) anchorMax.x = 1f;
            if (!_applyTop) anchorMax.y = 1f;

            // Clamp values
            anchorMin.x = Mathf.Clamp01(anchorMin.x);
            anchorMin.y = Mathf.Clamp01(anchorMin.y);
            anchorMax.x = Mathf.Clamp01(anchorMax.x);
            anchorMax.y = Mathf.Clamp01(anchorMax.y);

            // Apply to RectTransform
            _rectTransform.anchorMin = anchorMin;
            _rectTransform.anchorMax = anchorMax;

            // Reset offset to ensure anchors fully control positioning
            _rectTransform.offsetMin = Vector2.zero;
            _rectTransform.offsetMax = Vector2.zero;

            if (_debugLog)
            {
                Debug.Log($"[SafeAreaHandler] Applied anchors: min={anchorMin}, max={anchorMax}");
            }
        }

        private Rect GetSafeArea()
        {
#if UNITY_EDITOR
            if (_simulateNotch)
            {
                // Simulate iPhone-style notch (top) and home indicator (bottom)
                return new Rect(
                    0,
                    _simulatedBottomInset,
                    Screen.width,
                    Screen.height - _simulatedNotchHeight - _simulatedBottomInset
                );
            }
#endif

            return Screen.safeArea;
        }

        // ============================================
        // Editor Validation
        // ============================================

#if UNITY_EDITOR
        private void OnValidate()
        {
            // Reapply when settings change in editor
            if (_rectTransform != null && Application.isPlaying)
            {
                ApplySafeArea();
            }
        }

        private void OnDrawGizmosSelected()
        {
            if (!Application.isPlaying) return;

            // Visualize safe area in scene view
            var canvas = GetComponentInParent<Canvas>();
            if (canvas == null) return;

            Gizmos.color = new Color(0, 1, 0, 0.3f);
            var corners = new Vector3[4];
            _rectTransform.GetWorldCorners(corners);

            Gizmos.DrawLine(corners[0], corners[1]);
            Gizmos.DrawLine(corners[1], corners[2]);
            Gizmos.DrawLine(corners[2], corners[3]);
            Gizmos.DrawLine(corners[3], corners[0]);
        }
#endif
    }
}
