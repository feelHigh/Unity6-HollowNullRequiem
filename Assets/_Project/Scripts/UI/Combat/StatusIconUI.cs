// ============================================
// StatusIconUI.cs
// Status effect icon display for party sidebar
// ============================================

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using HNR.Characters;
using HNR.UI.Config;

namespace HNR.UI.Combat
{
    /// <summary>
    /// Displays a single status effect icon with stack count.
    /// Uses StatusIconConfigSO for sprite and color configuration.
    /// </summary>
    public class StatusIconUI : MonoBehaviour
    {
        [Header("Display")]
        [SerializeField] private Image _icon;
        [SerializeField] private TMP_Text _stackText;
        [SerializeField] private Image _background;

        private StatusType _statusType;
        private int _stacks;

        /// <summary>
        /// The status type this icon represents.
        /// </summary>
        public StatusType StatusType => _statusType;

        /// <summary>
        /// Initializes the icon with status type and stack count.
        /// </summary>
        /// <param name="statusType">The status effect type.</param>
        /// <param name="stacks">Number of stacks.</param>
        public void Initialize(StatusType statusType, int stacks)
        {
            _statusType = statusType;
            _stacks = stacks;

            UpdateDisplay();
        }

        /// <summary>
        /// Updates the stack count.
        /// </summary>
        /// <param name="stacks">New stack count.</param>
        public void UpdateStacks(int stacks)
        {
            _stacks = stacks;
            UpdateDisplay();
        }

        private void UpdateDisplay()
        {
            // Update stack text
            if (_stackText != null)
            {
                _stackText.text = _stacks > 1 ? _stacks.ToString() : "";
            }

            // Get config for sprite and color
            var iconConfig = StatusIconConfigSO.Instance;

            // Update icon sprite
            if (_icon != null && iconConfig != null)
            {
                var sprite = iconConfig.GetIcon(_statusType);
                if (sprite != null)
                {
                    _icon.sprite = sprite;
                }
                _icon.color = iconConfig.GetTintColor(_statusType);
            }

            // Update background color
            if (_background != null)
            {
                if (iconConfig != null)
                {
                    // Use darker version of tint for background
                    var tint = iconConfig.GetTintColor(_statusType);
                    _background.color = new Color(tint.r * 0.3f, tint.g * 0.3f, tint.b * 0.3f, 0.8f);
                }
                else
                {
                    _background.color = GetStatusColorFallback(_statusType);
                }
            }
        }

        private Color GetStatusColorFallback(StatusType type)
        {
            return type switch
            {
                StatusType.Burn => new Color(1f, 0.4f, 0.2f),
                StatusType.Poison => new Color(0.4f, 0.8f, 0.2f),
                StatusType.Weakness => new Color(0.6f, 0.4f, 0.6f),
                StatusType.Strength => new Color(1f, 0.6f, 0.2f),
                StatusType.Vulnerability => new Color(0.8f, 0.2f, 0.2f),
                StatusType.Dexterity => UIColors.SoulCyan,
                StatusType.Regeneration => UIColors.NatureAspect,
                StatusType.Shielded => UIColors.SoulGold,
                _ => UIColors.PanelGray
            };
        }
    }
}
