// ============================================
// GlobalNavDock.cs
// Bottom navigation dock with nav buttons
// ============================================

using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using HNR.Core;
using HNR.Core.Events;

namespace HNR.UI
{
    /// <summary>
    /// Global navigation dock anchored at bottom of screen.
    /// Contains navigation buttons for main game sections.
    /// </summary>
    public class GlobalNavDock : MonoBehaviour
    {
        // ============================================
        // Configuration
        // ============================================

        [Header("Navigation Buttons")]
        [SerializeField] private NavDockButton[] _buttons;
        [SerializeField] private int _selectedIndex = 0;

        [Header("Styling")]
        [SerializeField] private Color _normalColor = new Color(0.5f, 0.5f, 0.5f, 1f);
        [SerializeField] private Color _selectedColor;
        [SerializeField] private float _glowIntensity = 1.5f;
        [SerializeField] private float _transitionDuration = 0.2f;

        // ============================================
        // Events
        // ============================================

        /// <summary>
        /// Fired when a navigation button is clicked.
        /// </summary>
        public event Action<NavDestination> OnNavigationRequested;

        // ============================================
        // Unity Lifecycle
        // ============================================

        private void Awake()
        {
            _selectedColor = UIColors.SoulCyan;
        }

        private void Start()
        {
            InitializeButtons();
        }

        // ============================================
        // Initialization
        // ============================================

        private void InitializeButtons()
        {
            for (int i = 0; i < _buttons.Length; i++)
            {
                int index = i; // Capture for closure
                var button = _buttons[i];

                if (button != null)
                {
                    button.Initialize(_normalColor, _selectedColor, _glowIntensity, _transitionDuration);
                    button.OnClicked += () => OnButtonClicked(index);
                    button.SetSelected(i == _selectedIndex);
                }
            }
        }

        // ============================================
        // Button Handling
        // ============================================

        private void OnButtonClicked(int index)
        {
            if (index == _selectedIndex) return;

            SelectButton(index);

            // Fire navigation event
            if (index >= 0 && index < _buttons.Length)
            {
                var destination = _buttons[index].Destination;
                OnNavigationRequested?.Invoke(destination);
                EventBus.Publish(new NavDockNavigationEvent(destination));
            }
        }

        /// <summary>
        /// Select a button by index.
        /// </summary>
        /// <param name="index">Button index to select.</param>
        public void SelectButton(int index)
        {
            if (index < 0 || index >= _buttons.Length) return;

            _selectedIndex = index;

            for (int i = 0; i < _buttons.Length; i++)
            {
                _buttons[i]?.SetSelected(i == index);
            }
        }

        /// <summary>
        /// Select a button by destination.
        /// </summary>
        /// <param name="destination">Destination to select.</param>
        public void SelectDestination(NavDestination destination)
        {
            for (int i = 0; i < _buttons.Length; i++)
            {
                if (_buttons[i] != null && _buttons[i].Destination == destination)
                {
                    SelectButton(i);
                    return;
                }
            }
        }

        /// <summary>
        /// Set notification badge on a button.
        /// </summary>
        /// <param name="destination">Button destination.</param>
        /// <param name="count">Badge count (0 to hide).</param>
        public void SetNotificationBadge(NavDestination destination, int count)
        {
            foreach (var button in _buttons)
            {
                if (button != null && button.Destination == destination)
                {
                    button.SetNotificationBadge(count);
                    return;
                }
            }
        }

        /// <summary>
        /// Gets the currently selected index.
        /// </summary>
        public int SelectedIndex => _selectedIndex;
    }

    /// <summary>
    /// Navigation destinations available from the dock.
    /// </summary>
    public enum NavDestination
    {
        Bastion,
        Requiems,
        Inventory,
        Settings
    }

    /// <summary>
    /// Event fired when nav dock navigation is requested.
    /// </summary>
    public class NavDockNavigationEvent : GameEvent
    {
        public NavDestination Destination { get; }

        public NavDockNavigationEvent(NavDestination destination)
        {
            Destination = destination;
        }
    }

    /// <summary>
    /// Individual navigation dock button component.
    /// </summary>
    [Serializable]
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
                _icon.DOColor(targetColor, _transitionDuration);
            }

            if (_glowRing != null)
            {
                _glowRing.DOFade(targetGlow > 0 ? 0.6f : 0f, _transitionDuration);
                _glowRing.color = _selectedColor;
            }

            if (_labelText != null)
            {
                _labelText.DOColor(targetColor, _transitionDuration);
            }

            // Scale animation
            if (selected)
            {
                transform.DOScale(1.1f, _transitionDuration).SetEase(Ease.OutBack);
            }
            else
            {
                transform.DOScale(1f, _transitionDuration);
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
