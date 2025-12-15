// ============================================
// StatusIconUI.cs
// Status effect icon display for party sidebar
// ============================================

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using HNR.Combat;

namespace HNR.UI.Combat
{
    /// <summary>
    /// Displays a single status effect icon with stack count.
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
            if (_stackText != null)
            {
                _stackText.text = _stacks > 1 ? _stacks.ToString() : "";
            }

            if (_background != null)
            {
                _background.color = GetStatusColor(_statusType);
            }
        }

        private Color GetStatusColor(StatusType type)
        {
            return type switch
            {
                StatusType.Burn => new Color(1f, 0.4f, 0.2f),
                StatusType.Poison => new Color(0.4f, 0.8f, 0.2f),
                StatusType.Weakness => new Color(0.6f, 0.4f, 0.6f),
                StatusType.Strength => new Color(1f, 0.6f, 0.2f),
                StatusType.Vulnerable => new Color(0.8f, 0.2f, 0.2f),
                StatusType.Block => UIColors.SoulCyan,
                _ => UIColors.PanelGray
            };
        }
    }
}
