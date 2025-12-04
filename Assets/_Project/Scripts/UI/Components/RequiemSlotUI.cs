// ============================================
// RequiemSlotUI.cs
// Individual Requiem slot for team selection
// ============================================

using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using HNR.Characters;

namespace HNR.UI
{
    /// <summary>
    /// UI component for displaying a single Requiem in the selection screen.
    /// Handles click selection and hover preview.
    /// </summary>
    public class RequiemSlotUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
    {
        // ============================================
        // References
        // ============================================

        [Header("Visual Elements")]
        [SerializeField, Tooltip("Portrait image")]
        private Image _portrait;

        [SerializeField, Tooltip("Background/frame image")]
        private Image _background;

        [SerializeField, Tooltip("Character name text")]
        private TextMeshProUGUI _nameText;

        [SerializeField, Tooltip("Class/role text")]
        private TextMeshProUGUI _classText;

        [SerializeField, Tooltip("Stats summary text")]
        private TextMeshProUGUI _statsText;

        [SerializeField, Tooltip("Soul Aspect indicator")]
        private Image _aspectIndicator;

        [Header("Selection Visual")]
        [SerializeField, Tooltip("Selection overlay/highlight")]
        private GameObject _selectedOverlay;

        [SerializeField, Tooltip("Selection checkmark icon")]
        private GameObject _selectedCheckmark;

        [SerializeField, Tooltip("Selection number text (1, 2, 3)")]
        private TextMeshProUGUI _selectionOrderText;

        [Header("Hover Visual")]
        [SerializeField, Tooltip("Hover highlight effect")]
        private GameObject _hoverHighlight;

        [SerializeField, Tooltip("Glow effect on hover")]
        private Image _hoverGlow;

        [Header("Interaction")]
        [SerializeField, Tooltip("Main clickable button")]
        private Button _button;

        [Header("Colors")]
        [SerializeField] private Color _normalBackgroundColor = new Color(0.15f, 0.15f, 0.2f);
        [SerializeField] private Color _selectedBackgroundColor = new Color(0.2f, 0.3f, 0.4f);
        [SerializeField] private Color _hoverBackgroundColor = new Color(0.2f, 0.2f, 0.3f);

        // ============================================
        // State
        // ============================================

        private RequiemDataSO _requiem;
        private Action<RequiemDataSO> _onClicked;
        private Action<RequiemDataSO> _onHovered;
        private bool _isSelected;
        private bool _isHovered;
        private int _selectionOrder;

        // ============================================
        // Properties
        // ============================================

        /// <summary>The Requiem data this slot represents.</summary>
        public RequiemDataSO Requiem => _requiem;

        /// <summary>Whether this slot is currently selected.</summary>
        public bool IsSelected => _isSelected;

        // ============================================
        // Initialization
        // ============================================

        /// <summary>
        /// Initialize the slot with Requiem data and callbacks.
        /// </summary>
        /// <param name="requiem">The Requiem to display</param>
        /// <param name="onClicked">Callback when slot is clicked</param>
        /// <param name="onHovered">Optional callback when slot is hovered</param>
        public void Initialize(RequiemDataSO requiem, Action<RequiemDataSO> onClicked, Action<RequiemDataSO> onHovered = null)
        {
            _requiem = requiem;
            _onClicked = onClicked;
            _onHovered = onHovered;

            UpdateVisuals();
            SetSelected(false);
            SetHovered(false);

            // Wire button if exists
            if (_button != null)
            {
                _button.onClick.RemoveAllListeners();
                _button.onClick.AddListener(OnClick);
            }
        }

        // ============================================
        // Visual Updates
        // ============================================

        private void UpdateVisuals()
        {
            if (_requiem == null) return;

            // Portrait
            if (_portrait != null)
            {
                if (_requiem.Portrait != null)
                {
                    _portrait.sprite = _requiem.Portrait;
                    _portrait.color = Color.white;
                }
                else
                {
                    // Placeholder color based on aspect
                    _portrait.sprite = null;
                    _portrait.color = _requiem.AspectColor;
                }
            }

            // Name
            if (_nameText != null)
                _nameText.text = _requiem.RequiemName;

            // Class/Role
            if (_classText != null)
                _classText.text = $"{_requiem.Class}";

            // Stats summary
            if (_statsText != null)
            {
                _statsText.text = $"HP:{_requiem.BaseHP}  ATK:{_requiem.BaseATK}  DEF:{_requiem.BaseDEF}";
            }

            // Aspect indicator
            if (_aspectIndicator != null)
            {
                _aspectIndicator.color = _requiem.AspectColor;
            }
        }

        /// <summary>
        /// Set the selection state of this slot.
        /// </summary>
        /// <param name="selected">Whether the slot is selected</param>
        /// <param name="order">Selection order (1-3) if selected</param>
        public void SetSelected(bool selected, int order = 0)
        {
            _isSelected = selected;
            _selectionOrder = order;

            // Selection overlay
            if (_selectedOverlay != null)
                _selectedOverlay.SetActive(selected);

            // Checkmark
            if (_selectedCheckmark != null)
                _selectedCheckmark.SetActive(selected);

            // Selection order number
            if (_selectionOrderText != null)
            {
                _selectionOrderText.gameObject.SetActive(selected && order > 0);
                if (order > 0)
                    _selectionOrderText.text = order.ToString();
            }

            // Background color
            UpdateBackgroundColor();
        }

        private void SetHovered(bool hovered)
        {
            _isHovered = hovered;

            if (_hoverHighlight != null)
                _hoverHighlight.SetActive(hovered);

            if (_hoverGlow != null)
            {
                var color = _hoverGlow.color;
                color.a = hovered ? 0.5f : 0f;
                _hoverGlow.color = color;
            }

            UpdateBackgroundColor();
        }

        private void UpdateBackgroundColor()
        {
            if (_background == null) return;

            if (_isSelected)
                _background.color = _selectedBackgroundColor;
            else if (_isHovered)
                _background.color = _hoverBackgroundColor;
            else
                _background.color = _normalBackgroundColor;
        }

        // ============================================
        // Interaction Handlers
        // ============================================

        private void OnClick()
        {
            _onClicked?.Invoke(_requiem);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            SetHovered(true);
            _onHovered?.Invoke(_requiem);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            SetHovered(false);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            // Only trigger if button component doesn't exist
            // (to avoid double-triggering)
            if (_button == null)
            {
                OnClick();
            }
        }

        // ============================================
        // Animation Helpers
        // ============================================

        private Coroutine _scaleCoroutine;

        /// <summary>
        /// Play selection animation.
        /// </summary>
        public void PlaySelectAnimation()
        {
            if (_scaleCoroutine != null)
                StopCoroutine(_scaleCoroutine);

            _scaleCoroutine = StartCoroutine(ScalePunchRoutine(1.1f));
        }

        /// <summary>
        /// Play deselection animation.
        /// </summary>
        public void PlayDeselectAnimation()
        {
            if (_scaleCoroutine != null)
                StopCoroutine(_scaleCoroutine);

            _scaleCoroutine = StartCoroutine(ScalePunchRoutine(0.95f));
        }

        private System.Collections.IEnumerator ScalePunchRoutine(float targetScale)
        {
            float duration = 0.1f;
            float elapsed = 0f;
            Vector3 startScale = transform.localScale;
            Vector3 target = Vector3.one * targetScale;

            // Scale out
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                transform.localScale = Vector3.Lerp(startScale, target, t);
                yield return null;
            }

            // Scale back
            elapsed = 0f;
            startScale = target;
            target = Vector3.one;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                transform.localScale = Vector3.Lerp(startScale, target, t);
                yield return null;
            }

            transform.localScale = Vector3.one;
            _scaleCoroutine = null;
        }
    }
}
