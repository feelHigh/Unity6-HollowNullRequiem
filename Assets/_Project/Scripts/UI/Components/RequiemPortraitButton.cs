// ============================================
// RequiemPortraitButton.cs
// Clickable Requiem portrait for roster grid
// ============================================

using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using DG.Tweening;
using HNR.Characters;

namespace HNR.UI.Components
{
    /// <summary>
    /// Button component displaying a Requiem portrait.
    /// Used in the Requiems roster screen for character selection.
    /// </summary>
    public class RequiemPortraitButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        // ============================================
        // Events
        // ============================================

        /// <summary>Invoked when portrait is clicked.</summary>
        public event Action<RequiemDataSO> OnPortraitClicked;

        /// <summary>Invoked when pointer enters portrait.</summary>
        public event Action<RequiemDataSO> OnPortraitHovered;

        /// <summary>Invoked when pointer exits portrait.</summary>
        public event Action OnPortraitUnhovered;

        // ============================================
        // Serialized Fields
        // ============================================

        [Header("Data")]
        [SerializeField, Tooltip("Requiem data reference")]
        private RequiemDataSO _requiemData;

        [Header("UI References")]
        [SerializeField, Tooltip("Main button component")]
        private Button _button;

        [SerializeField, Tooltip("Portrait image")]
        private Image _portraitImage;

        [SerializeField, Tooltip("Requiem name text")]
        private TMP_Text _nameText;

        [SerializeField, Tooltip("Level text")]
        private TMP_Text _levelText;

        [SerializeField, Tooltip("Class/Aspect icon")]
        private Image _classIcon;

        [SerializeField, Tooltip("Frame/border image")]
        private Image _frameImage;

        [SerializeField, Tooltip("Glow effect for hover")]
        private Image _glowImage;

        [SerializeField, Tooltip("Selection highlight")]
        private Image _selectionHighlight;

        [Header("Colors")]
        [SerializeField] private Color _normalFrameColor = Color.white;
        [SerializeField] private Color _hoverFrameColor = new Color(1f, 0.9f, 0.5f, 1f);
        [SerializeField] private Color _selectedFrameColor = new Color(0.9f, 0.7f, 0.2f, 1f);

        [Header("Animation")]
        [SerializeField] private float _hoverScale = 1.05f;
        [SerializeField] private float _hoverDuration = 0.15f;
        [SerializeField] private float _clickPunchScale = 0.1f;

        // ============================================
        // State
        // ============================================

        private bool _isSelected;
        private bool _isHovered;
        private Tween _currentTween;
        private Vector3 _originalScale;

        // ============================================
        // Properties
        // ============================================

        /// <summary>The Requiem data assigned to this portrait.</summary>
        public RequiemDataSO RequiemData => _requiemData;

        /// <summary>Whether this portrait is currently selected.</summary>
        public bool IsSelected => _isSelected;

        // ============================================
        // Unity Lifecycle
        // ============================================

        private void Awake()
        {
            _originalScale = transform.localScale;

            if (_button != null)
            {
                _button.onClick.AddListener(OnButtonClicked);
            }

            // Hide glow initially
            if (_glowImage != null)
            {
                _glowImage.gameObject.SetActive(false);
            }

            // Hide selection highlight initially
            if (_selectionHighlight != null)
            {
                _selectionHighlight.gameObject.SetActive(false);
            }
        }

        private void OnDestroy()
        {
            _currentTween?.Kill();

            if (_button != null)
            {
                _button.onClick.RemoveListener(OnButtonClicked);
            }
        }

        private void Start()
        {
            // Initialize display if data is already set
            if (_requiemData != null)
            {
                UpdateDisplay();
            }
        }

        // ============================================
        // Public API
        // ============================================

        /// <summary>
        /// Sets the Requiem data and updates the display.
        /// </summary>
        public void SetRequiemData(RequiemDataSO data)
        {
            _requiemData = data;
            UpdateDisplay();
        }

        /// <summary>
        /// Sets the portrait sprite directly (for manual assignment).
        /// </summary>
        public void SetPortraitSprite(Sprite sprite)
        {
            if (_portraitImage != null)
            {
                _portraitImage.sprite = sprite;
            }
        }

        /// <summary>
        /// Sets the selected state.
        /// </summary>
        public void SetSelected(bool selected)
        {
            _isSelected = selected;
            UpdateVisuals();

            if (selected)
            {
                PlaySelectionAnimation();
            }
        }

        /// <summary>
        /// Updates the display from the assigned RequiemDataSO.
        /// </summary>
        public void UpdateDisplay()
        {
            if (_requiemData == null)
            {
                // Clear display
                if (_portraitImage != null)
                    _portraitImage.sprite = null;
                if (_nameText != null)
                    _nameText.text = "";
                if (_levelText != null)
                    _levelText.text = "";
                return;
            }

            // Update portrait
            if (_portraitImage != null && _requiemData.Portrait != null)
            {
                _portraitImage.sprite = _requiemData.Portrait;
            }

            // Update name
            if (_nameText != null)
            {
                _nameText.text = _requiemData.RequiemName;
            }

            // Update level (placeholder - would come from save data)
            if (_levelText != null)
            {
                _levelText.text = "Lv. 1";
            }

            // Update class icon based on aspect
            if (_classIcon != null)
            {
                // Would load appropriate icon based on _requiemData.Aspect
                // For now, just show/hide
                _classIcon.gameObject.SetActive(true);
            }

            UpdateVisuals();
        }

        // ============================================
        // Visual Updates
        // ============================================

        private void UpdateVisuals()
        {
            // Update frame color
            if (_frameImage != null)
            {
                Color targetColor;
                if (_isSelected)
                    targetColor = _selectedFrameColor;
                else if (_isHovered)
                    targetColor = _hoverFrameColor;
                else
                    targetColor = _normalFrameColor;

                _frameImage.color = targetColor;
            }

            // Update selection highlight
            if (_selectionHighlight != null)
            {
                _selectionHighlight.gameObject.SetActive(_isSelected);
            }
        }

        // ============================================
        // Pointer Events
        // ============================================

        public void OnPointerEnter(PointerEventData eventData)
        {
            _isHovered = true;
            UpdateVisuals();

            // Show glow
            if (_glowImage != null)
            {
                _glowImage.gameObject.SetActive(true);
                _glowImage.DOFade(1f, _hoverDuration).SetLink(gameObject);
            }

            // Scale up
            _currentTween?.Kill();
            _currentTween = transform.DOScale(_originalScale * _hoverScale, _hoverDuration)
                .SetEase(Ease.OutQuad)
                .SetLink(gameObject);

            // Invoke hover event
            OnPortraitHovered?.Invoke(_requiemData);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            _isHovered = false;
            UpdateVisuals();

            // Hide glow
            if (_glowImage != null)
            {
                _glowImage.DOFade(0f, _hoverDuration)
                    .OnComplete(() => _glowImage.gameObject.SetActive(false))
                    .SetLink(gameObject);
            }

            // Scale back
            _currentTween?.Kill();
            _currentTween = transform.DOScale(_originalScale, _hoverDuration)
                .SetEase(Ease.OutQuad)
                .SetLink(gameObject);

            // Invoke unhover event
            OnPortraitUnhovered?.Invoke();
        }

        // ============================================
        // Button Handler
        // ============================================

        private void OnButtonClicked()
        {
            // Play click animation
            _currentTween?.Kill();
            _currentTween = transform.DOPunchScale(Vector3.one * _clickPunchScale, 0.2f, 5)
                .SetEase(Ease.OutBack)
                .SetLink(gameObject);

            // Invoke click event
            OnPortraitClicked?.Invoke(_requiemData);
        }

        // ============================================
        // Animations
        // ============================================

        private void PlaySelectionAnimation()
        {
            if (_selectionHighlight != null)
            {
                _selectionHighlight.transform.localScale = Vector3.one * 1.2f;
                _selectionHighlight.transform.DOScale(1f, 0.3f)
                    .SetEase(Ease.OutBack)
                    .SetLink(gameObject);
            }
        }

        /// <summary>
        /// Plays an entrance animation.
        /// </summary>
        public void PlayEntranceAnimation(float delay = 0f)
        {
            transform.localScale = Vector3.zero;

            DOTween.Sequence()
                .AppendInterval(delay)
                .Append(transform.DOScale(_originalScale, 0.3f).SetEase(Ease.OutBack))
                .SetLink(gameObject);
        }
    }
}
