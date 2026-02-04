// ============================================
// SystemMenuBar.cs
// Top-right combat menu bar with speed, auto-battle, settings
// ============================================

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using HNR.Core.Events;
using HNR.Combat;
using HNR.UI.Screens;

namespace HNR.UI.Combat
{
    /// <summary>
    /// Top-right menu bar with speed toggle, auto-battle, and settings.
    /// </summary>
    public class SystemMenuBar : MonoBehaviour
    {
        [Header("Speed Toggle")]
        [SerializeField] private Button _speedToggle;
        [SerializeField] private Image _speedIcon;
        [SerializeField] private Sprite _speed1xSprite;
        [SerializeField] private Sprite _speed2xSprite;

        [Header("Auto-Battle")]
        [SerializeField] private Button _autoBattleToggle;
        [SerializeField] private Image _autoBattleIcon;

        [Header("Menu Buttons")]
        [SerializeField] private Button _settingsButton;
        [SerializeField] private Button _menuButton;

        private readonly float[] _speedOptions = { 1f, 2f };
        private int _currentSpeedIndex = 0;
        private bool _autoBattleEnabled = false;
        private float _previousTimeScale = 1f;

        private void OnEnable()
        {
            EventBus.Subscribe<SettingsClosedEvent>(OnSettingsClosed);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<SettingsClosedEvent>(OnSettingsClosed);
        }

        private void OnSettingsClosed(SettingsClosedEvent evt)
        {
            // Restore time scale respecting current speed setting
            Time.timeScale = _speedOptions[_currentSpeedIndex];
        }

        private void Start()
        {
            if (_speedToggle != null)
                _speedToggle.onClick.AddListener(CycleSpeed);

            if (_autoBattleToggle != null)
                _autoBattleToggle.onClick.AddListener(ToggleAutoBattle);

            if (_settingsButton != null)
                _settingsButton.onClick.AddListener(OpenSettings);

            if (_menuButton != null)
                _menuButton.onClick.AddListener(OpenMenu);

            UpdateSpeedDisplay();
            UpdateAutoBattleDisplay();
        }

        private void OnDestroy()
        {
            if (_speedToggle != null)
                _speedToggle.onClick.RemoveListener(CycleSpeed);

            if (_autoBattleToggle != null)
                _autoBattleToggle.onClick.RemoveListener(ToggleAutoBattle);

            if (_settingsButton != null)
                _settingsButton.onClick.RemoveListener(OpenSettings);

            if (_menuButton != null)
                _menuButton.onClick.RemoveListener(OpenMenu);
        }

        /// <summary>
        /// Cycles through speed options: 1x → 1.5x → 2x → 1x.
        /// </summary>
        private void CycleSpeed()
        {
            _currentSpeedIndex = (_currentSpeedIndex + 1) % _speedOptions.Length;
            Time.timeScale = _speedOptions[_currentSpeedIndex];
            UpdateSpeedDisplay();
            EventBus.Publish(new GameSpeedChangedEvent(_speedOptions[_currentSpeedIndex]));
        }

        private void UpdateSpeedDisplay()
        {
            if (_speedIcon == null) return;

            float speed = _speedOptions[_currentSpeedIndex];
            _speedIcon.sprite = speed == 1f ? _speed1xSprite : _speed2xSprite;
        }

        /// <summary>
        /// Toggles auto-battle mode on/off.
        /// </summary>
        private void ToggleAutoBattle()
        {
            _autoBattleEnabled = !_autoBattleEnabled;
            UpdateAutoBattleDisplay();
            EventBus.Publish(new AutoBattleToggledEvent(_autoBattleEnabled));
        }

        private void UpdateAutoBattleDisplay()
        {
            if (_autoBattleIcon == null) return;

            _autoBattleIcon.color = _autoBattleEnabled ? UIColors.White : UIColors.PanelGray;
        }

        private void OpenSettings()
        {
            // Store current time scale and pause
            _previousTimeScale = Time.timeScale;
            Time.timeScale = 0f;

            // Show SettingsOverlay
            if (SettingsOverlay.Instance != null)
            {
                SettingsOverlay.Instance.Show();
            }
            else
            {
                SettingsOverlay.ShowSettings();
            }
        }

        private void OpenMenu()
        {
            EventBus.Publish(new OpenPauseMenuRequestEvent());
        }

        /// <summary>
        /// Resets all settings to defaults when combat ends.
        /// </summary>
        public void ResetOnCombatEnd()
        {
            _currentSpeedIndex = 0;
            Time.timeScale = 1f;
            UpdateSpeedDisplay();

            _autoBattleEnabled = false;
            UpdateAutoBattleDisplay();
        }

        /// <summary>
        /// Gets the current game speed multiplier.
        /// </summary>
        public float CurrentSpeed => _speedOptions[_currentSpeedIndex];

        /// <summary>
        /// Gets whether auto-battle is currently enabled.
        /// </summary>
        public bool IsAutoBattleEnabled => _autoBattleEnabled;

        /// <summary>
        /// Sets the speed index directly (for loading saved state).
        /// </summary>
        /// <param name="index">Speed index (0=1x, 1=1.5x, 2=2x).</param>
        public void SetSpeedIndex(int index)
        {
            if (index < 0 || index >= _speedOptions.Length) return;

            _currentSpeedIndex = index;
            Time.timeScale = _speedOptions[_currentSpeedIndex];
            UpdateSpeedDisplay();
        }

        /// <summary>
        /// Sets auto-battle state directly (for loading saved state).
        /// </summary>
        /// <param name="enabled">Whether auto-battle should be enabled.</param>
        public void SetAutoBattle(bool enabled)
        {
            _autoBattleEnabled = enabled;
            UpdateAutoBattleDisplay();
        }
    }
}
