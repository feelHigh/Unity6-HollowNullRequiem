// ============================================
// NavDockButton.cs
// Individual navigation dock button component
// ============================================

using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

namespace HNR.UI
{
    /// <summary>
    /// Individual navigation dock button component.
    /// </summary>
    public class NavDockButton : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private NavDestination _destination;
        [SerializeField] private string _label;

        [Header("Visual References")]
        [SerializeField] private Button _button;
        [SerializeField] private Image _icon;
        [SerializeField] private Image _glowRing;
        [SerializeField] private TMP_Text _labelText;
        [SerializeField] private GameObject _notificationBadge;
        [SerializeField] private TMP_Text _notificationCount;

        private Color _normalColor;
        private Color _selectedColor;
        private float _glowIntensity;
        private float _transitionDuration;
        private bool _isSelected;

        /// <summary>
        /// Fired when this button is clicked.
        /// </summary>
        public event Action OnClicked;

        /// <summary>
        /// Gets the destination this button navigates to.
        /// </summary>
        public NavDestination Destination => _destination;

        /// <summary>
        /// Initialize the button with styling.
        /// </summary>
        public void Initialize(Color normalColor, Color selectedColor, float glowIntensity, float transitionDuration)
        {
            _normalColor = normalColor;
            _selectedColor = selectedColor;
            _glowIntensity = glowIntensity;
            _transitionDuration = transitionDuration;

            if (_button != null)
            {
                _button.onClick.AddListener(() => OnClicked?.Invoke());
            }

            if (_labelText != null)
            {
                _labelText.text = _label;
            }

            // Hide notification badge initially
            if (_notificationBadge != null)
            {
                _notificationBadge.SetActive(false);
            }
        }

        /// <summary>
        /// Set the selected state of this button.
        /// </summary>
        public void SetSelected(bool selected)
        {
            _isSelected = selected;

            Color targetColor = selected ? _selectedColor : _normalColor;
            float targetGlow = selected ? _glowIntensity : 0f;

            if (_icon != null)
            {
                _icon.DOColor(targetColor, _transitionDuration).SetLink(gameObject);
            }

            if (_glowRing != null)
            {
                _glowRing.DOFade(targetGlow > 0 ? 0.6f : 0f, _transitionDuration).SetLink(gameObject);
                _glowRing.color = _selectedColor;
            }

            if (_labelText != null)
            {
                _labelText.DOColor(targetColor, _transitionDuration).SetLink(gameObject);
            }

            // Scale animation
            if (selected)
            {
                transform.DOScale(1.1f, _transitionDuration).SetEase(Ease.OutBack).SetLink(gameObject);
            }
            else
            {
                transform.DOScale(1f, _transitionDuration).SetLink(gameObject);
            }
        }

        /// <summary>
        /// Set the notification badge count.
        /// </summary>
        /// <param name="count">Badge count (0 to hide).</param>
        public void SetNotificationBadge(int count)
        {
            if (_notificationBadge != null)
            {
                _notificationBadge.SetActive(count > 0);
            }

            if (_notificationCount != null)
            {
                _notificationCount.text = count > 99 ? "99+" : count.ToString();
            }
        }

        /// <summary>
        /// Gets whether this button is currently selected.
        /// </summary>
        public bool IsSelected => _isSelected;
    }
}
