// ============================================
// StageFrameVisuals.cs
// Runtime component for managing stage frame visual states
// ============================================

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace HNR.UI.Components
{
    /// <summary>
    /// Manages the visual state of LayerLab-styled stage frames.
    /// Handles hover/selection states for zone selection in BattleMission.
    /// </summary>
    public class StageFrameVisuals : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler
    {
        [Header("Visual Layers")]
        [SerializeField, Tooltip("Focus highlight layer (shown on hover/select)")]
        private GameObject _focusLayer;

        [SerializeField, Tooltip("Content area image (for tinting on state change)")]
        private Image _contentImage;

        [SerializeField, Tooltip("Zone icon image")]
        private Image _iconImage;

        [Header("State Colors")]
        [SerializeField]
        private Color _normalContentColor = new Color(0.08f, 0.06f, 0.12f, 0.8f);

        [SerializeField]
        private Color _hoverContentColor = new Color(0.12f, 0.1f, 0.18f, 0.9f);

        [SerializeField]
        private Color _selectedContentColor = new Color(0.15f, 0.12f, 0.22f, 0.95f);

        [Header("State")]
        [SerializeField]
        private bool _isSelected;

        [SerializeField]
        private bool _isHovered;

        [SerializeField]
        private bool _isLocked;

        /// <summary>
        /// Gets or sets whether this frame is selected.
        /// </summary>
        public bool IsSelected
        {
            get => _isSelected;
            set => SetSelected(value);
        }

        /// <summary>
        /// Gets or sets whether this frame is locked (unavailable).
        /// </summary>
        public bool IsLocked
        {
            get => _isLocked;
            set => SetLocked(value);
        }

        /// <summary>
        /// Sets the zone icon sprite.
        /// </summary>
        public void SetIcon(Sprite icon)
        {
            if (_iconImage != null && icon != null)
            {
                _iconImage.sprite = icon;
                _iconImage.preserveAspect = true;
            }
        }

        /// <summary>
        /// Sets the zone preview image.
        /// </summary>
        public void SetPreviewImage(Sprite preview)
        {
            if (_contentImage != null && preview != null)
            {
                _contentImage.sprite = preview;
                _contentImage.type = Image.Type.Simple;
                _contentImage.preserveAspect = true;
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (_isLocked) return;

            _isHovered = true;
            UpdateVisuals();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            _isHovered = false;
            UpdateVisuals();
        }

        public void OnSelect(BaseEventData eventData)
        {
            if (_isLocked) return;

            _isSelected = true;
            UpdateVisuals();
        }

        public void OnDeselect(BaseEventData eventData)
        {
            _isSelected = false;
            UpdateVisuals();
        }

        /// <summary>
        /// Sets the selected state.
        /// </summary>
        public void SetSelected(bool selected)
        {
            if (_isLocked && selected) return;

            _isSelected = selected;
            UpdateVisuals();
        }

        /// <summary>
        /// Sets the locked state.
        /// </summary>
        public void SetLocked(bool locked)
        {
            _isLocked = locked;

            if (locked)
            {
                _isSelected = false;
                _isHovered = false;
            }

            UpdateVisuals();

            // Disable button if locked
            var button = GetComponent<Button>();
            if (button != null)
            {
                button.interactable = !locked;
            }
        }

        /// <summary>
        /// Updates visual layers based on current state.
        /// </summary>
        private void UpdateVisuals()
        {
            // Focus layer visibility
            if (_focusLayer != null)
            {
                _focusLayer.SetActive((_isSelected || _isHovered) && !_isLocked);
            }

            // Content area color
            if (_contentImage != null)
            {
                if (_isLocked)
                {
                    _contentImage.color = new Color(_normalContentColor.r * 0.5f,
                        _normalContentColor.g * 0.5f,
                        _normalContentColor.b * 0.5f,
                        _normalContentColor.a);
                }
                else if (_isSelected)
                {
                    _contentImage.color = _selectedContentColor;
                }
                else if (_isHovered)
                {
                    _contentImage.color = _hoverContentColor;
                }
                else
                {
                    _contentImage.color = _normalContentColor;
                }
            }

            // Icon tint for locked state
            if (_iconImage != null)
            {
                _iconImage.color = _isLocked
                    ? new Color(0.3f, 0.3f, 0.35f, 0.5f)
                    : new Color(0.42f, 0.25f, 0.63f, 0.6f);
            }
        }

        private void Start()
        {
            UpdateVisuals();
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            UpdateVisuals();
        }
#endif
    }
}
