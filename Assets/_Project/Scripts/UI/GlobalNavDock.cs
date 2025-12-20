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

    // NavDockButton is now in its own file: UI/Components/NavDockButton.cs
}
