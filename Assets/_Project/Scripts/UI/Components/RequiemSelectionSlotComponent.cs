// ============================================
// RequiemSelectionSlotComponent.cs
// Component for RequiemSelectionSlot prefab
// ============================================

using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using HNR.Cards;
using HNR.Characters;
using HNR.UI.Config;

namespace HNR.UI.Components
{
    /// <summary>
    /// Component for RequiemSelectionSlot prefab to receive data and manage state.
    /// Handles visual display and selection state for team selection screen.
    /// </summary>
    public class RequiemSelectionSlotComponent : MonoBehaviour
    {
        // ============================================
        // Visual Elements
        // ============================================

        [Header("Visual Elements")]
        [SerializeField, Tooltip("Portrait image (full body or portrait)")]
        private Image _portrait;

        [SerializeField, Tooltip("Background image of the slot")]
        private Image _background;

        [SerializeField, Tooltip("Requiem name text")]
        private TextMeshProUGUI _nameText;

        [SerializeField, Tooltip("Class and aspect text")]
        private TextMeshProUGUI _classText;

        [SerializeField, Tooltip("HP display text")]
        private TextMeshProUGUI _hpText;

        [SerializeField, Tooltip("Aspect icon badge")]
        private Image _aspectBadge;

        [SerializeField, Tooltip("Selection border container")]
        private GameObject _selectionBorder;

        [SerializeField, Tooltip("Button component")]
        private Button _button;

        // ============================================
        // Events
        // ============================================

        /// <summary>
        /// Fired when this slot is clicked.
        /// </summary>
        public event Action<RequiemDataSO> OnClicked;

        // ============================================
        // State
        // ============================================

        /// <summary>
        /// The Requiem data assigned to this slot.
        /// </summary>
        public RequiemDataSO RequiemData { get; private set; }

        /// <summary>
        /// Whether this slot is currently selected.
        /// </summary>
        public bool IsSelected { get; private set; }

        // ============================================
        // Initialization
        // ============================================

        private void Awake()
        {
            AutoWireReferences();

            if (_button != null)
            {
                _button.onClick.AddListener(OnButtonClicked);
            }
        }

        /// <summary>
        /// Auto-wire references if not set in Inspector.
        /// </summary>
        private void AutoWireReferences()
        {
            // Try to find components if not assigned
            if (_button == null)
                _button = GetComponent<Button>();

            if (_background == null)
                _background = GetComponent<Image>();

            if (_portrait == null)
            {
                var portraitTransform = transform.Find("Portrait");
                if (portraitTransform != null)
                    _portrait = portraitTransform.GetComponent<Image>();
            }

            if (_selectionBorder == null)
            {
                var borderTransform = transform.Find("SelectionBorder");
                if (borderTransform != null)
                    _selectionBorder = borderTransform.gameObject;
            }

            // Find InfoPanel children
            var infoPanel = transform.Find("InfoPanel");
            if (infoPanel != null)
            {
                if (_nameText == null)
                {
                    var nameTransform = infoPanel.Find("Name");
                    if (nameTransform != null)
                        _nameText = nameTransform.GetComponent<TextMeshProUGUI>();
                }

                if (_classText == null)
                {
                    var classTransform = infoPanel.Find("Class");
                    if (classTransform != null)
                        _classText = classTransform.GetComponent<TextMeshProUGUI>();
                }

                if (_hpText == null)
                {
                    var hpTransform = infoPanel.Find("HP");
                    if (hpTransform != null)
                        _hpText = hpTransform.GetComponent<TextMeshProUGUI>();
                }
            }

            if (_aspectBadge == null)
            {
                var badgeTransform = transform.Find("AspectBadge");
                if (badgeTransform != null)
                    _aspectBadge = badgeTransform.GetComponent<Image>();
            }
        }

        /// <summary>
        /// Initializes the slot with Requiem data and aspect icon config.
        /// </summary>
        /// <param name="requiem">The Requiem data to display.</param>
        /// <param name="aspectConfig">Optional aspect icon configuration.</param>
        public void Initialize(RequiemDataSO requiem, AspectIconConfigSO aspectConfig = null)
        {
            RequiemData = requiem;
            IsSelected = false;

            if (requiem == null)
            {
                gameObject.SetActive(false);
                return;
            }

            gameObject.SetActive(true);
            gameObject.name = $"Slot_{requiem.RequiemName}";

            // Set portrait - prefer full body, fallback to portrait
            if (_portrait != null)
            {
                if (requiem.FullBodySprite != null)
                {
                    _portrait.sprite = requiem.FullBodySprite;
                    _portrait.color = Color.white;
                }
                else if (requiem.Portrait != null)
                {
                    _portrait.sprite = requiem.Portrait;
                    _portrait.color = Color.white;
                }
                else
                {
                    _portrait.sprite = null;
                    _portrait.color = GetAspectColor(requiem.SoulAspect);
                }
            }

            // Set name
            if (_nameText != null)
            {
                _nameText.text = requiem.RequiemName;
            }

            // Set class and aspect
            if (_classText != null)
            {
                _classText.text = $"{requiem.Class} | {requiem.SoulAspect}";
            }

            // Set HP
            if (_hpText != null)
            {
                _hpText.text = $"HP {requiem.BaseHP}";
            }

            // Set aspect badge
            if (_aspectBadge != null)
            {
                var aspectIcon = aspectConfig != null ? aspectConfig.GetIcon(requiem.SoulAspect) : null;
                if (aspectIcon != null)
                {
                    _aspectBadge.sprite = aspectIcon;
                    _aspectBadge.color = Color.white;
                }
                else
                {
                    _aspectBadge.sprite = null;
                    _aspectBadge.color = GetAspectColor(requiem.SoulAspect);
                }
            }

            // Hide selection border initially
            if (_selectionBorder != null)
            {
                _selectionBorder.SetActive(false);
            }
        }

        /// <summary>
        /// Sets the selected state of this slot.
        /// </summary>
        /// <param name="selected">Whether the slot should be selected.</param>
        public void SetSelected(bool selected)
        {
            IsSelected = selected;

            if (_selectionBorder != null)
            {
                _selectionBorder.SetActive(selected);
            }
        }

        // ============================================
        // Button Handler
        // ============================================

        private void OnButtonClicked()
        {
            if (RequiemData != null)
            {
                OnClicked?.Invoke(RequiemData);
            }
        }

        // ============================================
        // Helper Methods
        // ============================================

        private Color GetAspectColor(SoulAspect aspect)
        {
            return aspect switch
            {
                SoulAspect.Flame => new Color(0.9f, 0.3f, 0.2f, 0.8f),
                SoulAspect.Shadow => new Color(0.3f, 0.2f, 0.4f, 0.8f),
                SoulAspect.Light => new Color(0.9f, 0.9f, 0.5f, 0.8f),
                SoulAspect.Nature => new Color(0.3f, 0.7f, 0.3f, 0.8f),
                _ => new Color(0.5f, 0.5f, 0.5f, 0.8f)
            };
        }

        // ============================================
        // Cleanup
        // ============================================

        private void OnDestroy()
        {
            if (_button != null)
            {
                _button.onClick.RemoveListener(OnButtonClicked);
            }
        }
    }
}
