// ============================================
// BannerSlide.cs
// Serializable banner data structure for event carousel
// ============================================

using UnityEngine;

namespace HNR.UI.Config
{
    /// <summary>
    /// Defines the action type that can be triggered when a banner is tapped.
    /// </summary>
    public enum BannerActionType
    {
        /// <summary>No action - banner is purely visual</summary>
        None,
        /// <summary>Open an external URL in browser</summary>
        OpenURL,
        /// <summary>Navigate to a different game scene/state</summary>
        ChangeScene,
        /// <summary>Show a modal dialog or overlay</summary>
        ShowModal,
        /// <summary>Trigger a custom event for game logic</summary>
        TriggerEvent
    }

    /// <summary>
    /// Serializable data structure for a single event banner slide.
    /// Used by BannerConfigSO to configure the event banner carousel.
    /// </summary>
    [System.Serializable]
    public class BannerSlide
    {
        // ============================================
        // Visual Configuration
        // ============================================

        [Header("Visual")]
        [SerializeField, Tooltip("Banner image sprite (recommended: 1920x540 or 960x270)")]
        private Sprite _image;

        [SerializeField, Tooltip("Banner title text (displayed over image if no image assigned)")]
        private string _title = "Event Banner";

        [SerializeField, Tooltip("Banner description text (displayed over image if no image assigned)")]
        private string _description = "Tap for details";

        // ============================================
        // State Configuration
        // ============================================

        [Header("State")]
        [SerializeField, Tooltip("Whether this banner should be shown")]
        private bool _isActive = true;

        [SerializeField, Tooltip("Priority order (lower numbers shown first)")]
        private int _priority = 0;

        // ============================================
        // Action Configuration (Future Use)
        // ============================================

        [Header("Action (Future)")]
        [SerializeField, Tooltip("Action to perform when banner is tapped")]
        private BannerActionType _actionType = BannerActionType.None;

        [SerializeField, Tooltip("Parameter for the action (URL, scene name, modal ID, event name)")]
        private string _actionParameter = "";

        // ============================================
        // Scheduling (Future Use)
        // ============================================

        [Header("Scheduling (Future)")]
        [SerializeField, Tooltip("Banner start date (ISO format: YYYY-MM-DD)")]
        private string _startDate = "";

        [SerializeField, Tooltip("Banner end date (ISO format: YYYY-MM-DD)")]
        private string _endDate = "";

        // ============================================
        // Public Accessors
        // ============================================

        /// <summary>Banner image sprite</summary>
        public Sprite Image => _image;

        /// <summary>Banner title text</summary>
        public string Title => _title;

        /// <summary>Banner description text</summary>
        public string Description => _description;

        /// <summary>Whether this banner is active and should be shown</summary>
        public bool IsActive => _isActive;

        /// <summary>Display priority (lower = higher priority)</summary>
        public int Priority => _priority;

        /// <summary>Action type to perform on tap</summary>
        public BannerActionType ActionType => _actionType;

        /// <summary>Parameter for the action</summary>
        public string ActionParameter => _actionParameter;

        /// <summary>Start date string (ISO format)</summary>
        public string StartDate => _startDate;

        /// <summary>End date string (ISO format)</summary>
        public string EndDate => _endDate;

        // ============================================
        // Helper Methods
        // ============================================

        /// <summary>
        /// Checks if the banner has a valid image assigned.
        /// </summary>
        public bool HasImage => _image != null;

        /// <summary>
        /// Checks if the banner has an action configured.
        /// </summary>
        public bool HasAction => _actionType != BannerActionType.None && !string.IsNullOrEmpty(_actionParameter);

        /// <summary>
        /// Checks if the banner has scheduling configured.
        /// </summary>
        public bool HasScheduling => !string.IsNullOrEmpty(_startDate) || !string.IsNullOrEmpty(_endDate);

        /// <summary>
        /// Checks if the banner is currently within its scheduled date range.
        /// Returns true if no scheduling is configured or if current date is within range.
        /// </summary>
        public bool IsWithinSchedule()
        {
            // If no scheduling configured, always valid
            if (!HasScheduling) return true;

            var now = System.DateTime.Now;

            // Check start date
            if (!string.IsNullOrEmpty(_startDate))
            {
                if (System.DateTime.TryParse(_startDate, out var start))
                {
                    if (now < start) return false;
                }
            }

            // Check end date
            if (!string.IsNullOrEmpty(_endDate))
            {
                if (System.DateTime.TryParse(_endDate, out var end))
                {
                    if (now > end) return false;
                }
            }

            return true;
        }
    }
}
