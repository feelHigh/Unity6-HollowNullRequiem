// ============================================
// TabButtonVisuals.cs
// Runtime component for managing tab button visual states
// ============================================

using UnityEngine;

namespace HNR.UI.Components
{
    /// <summary>
    /// Manages the visual state (normal/focused) of LayerLab-styled tab buttons.
    /// Toggles visibility of glow and focus highlight layers based on selection state.
    /// </summary>
    public class TabButtonVisuals : MonoBehaviour
    {
        [Header("Visual Layers")]
        [SerializeField, Tooltip("Normal state glow (subtle, visible when not focused)")]
        private GameObject _normalGlow;

        [SerializeField, Tooltip("Focus state glow (prominent, visible when focused)")]
        private GameObject _focusGlow;

        [SerializeField, Tooltip("Focus highlight layer (visible when focused)")]
        private GameObject _focusHighlight;

        [Header("State")]
        [SerializeField, Tooltip("Current focus state")]
        private bool _isFocused;

        /// <summary>
        /// Gets or sets whether this tab button is in focused state.
        /// </summary>
        public bool IsFocused
        {
            get => _isFocused;
            set => SetFocused(value);
        }

        private void Start()
        {
            // Apply initial state
            UpdateVisuals();
        }

        /// <summary>
        /// Sets the focused state and updates visuals.
        /// </summary>
        /// <param name="focused">True if button should show focused state</param>
        public void SetFocused(bool focused)
        {
            if (_isFocused == focused) return;

            _isFocused = focused;
            UpdateVisuals();
        }

        /// <summary>
        /// Toggles focus state.
        /// </summary>
        public void ToggleFocus()
        {
            SetFocused(!_isFocused);
        }

        /// <summary>
        /// Updates visual layers based on current state.
        /// </summary>
        private void UpdateVisuals()
        {
            if (_normalGlow != null)
            {
                _normalGlow.SetActive(!_isFocused);
            }

            if (_focusGlow != null)
            {
                _focusGlow.SetActive(_isFocused);
            }

            if (_focusHighlight != null)
            {
                _focusHighlight.SetActive(_isFocused);
            }
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            // Update visuals when inspector values change
            UpdateVisuals();
        }
#endif
    }
}
