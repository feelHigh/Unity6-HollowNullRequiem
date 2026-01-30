// ============================================
// RelicDisplayBar.cs
// Displays owned relics in combat UI sidebar
// ============================================

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using HNR.Core;
using HNR.Core.Events;
using HNR.Core.Interfaces;
using HNR.Progression;
using HNR.UI.Config;

namespace HNR.UI.Combat
{
    /// <summary>
    /// Displays owned relics in the combat UI.
    /// Shows small icons with tooltips for each relic.
    /// </summary>
    public class RelicDisplayBar : MonoBehaviour
    {
        // ============================================
        // Configuration
        // ============================================

        [Header("Layout")]
        [SerializeField, Tooltip("Container for relic icons")]
        private Transform _relicContainer;

        [SerializeField, Tooltip("Prefab for individual relic icons")]
        private GameObject _relicIconPrefab;

        [SerializeField, Tooltip("Maximum relics to display")]
        private int _maxDisplay = 8;

        [Header("Tooltip")]
        [SerializeField, Tooltip("Tooltip panel for relic details")]
        private GameObject _tooltipPanel;

        [SerializeField, Tooltip("Tooltip relic name text")]
        private TMP_Text _tooltipName;

        [SerializeField, Tooltip("Tooltip description text")]
        private TMP_Text _tooltipDescription;

        [SerializeField, Tooltip("Tooltip rarity text")]
        private TMP_Text _tooltipRarity;

        [Header("Animation")]
        [SerializeField] private float _iconSize = 32f;
        [SerializeField] private float _iconSpacing = 4f;
        [SerializeField] private float _pulseOnTrigger = 0.2f;

        // ============================================
        // Private State
        // ============================================

        private List<RelicIconSlot> _iconSlots = new();
        private IRelicManager _relicManager;

        // ============================================
        // Unity Lifecycle
        // ============================================

        private void Awake()
        {
            // Hide tooltip initially
            if (_tooltipPanel != null)
            {
                _tooltipPanel.SetActive(false);
            }
        }

        private void OnEnable()
        {
            EventBus.Subscribe<RelicAcquiredEvent>(OnRelicAcquired);
            EventBus.Subscribe<RelicTriggeredEvent>(OnRelicTriggered);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<RelicAcquiredEvent>(OnRelicAcquired);
            EventBus.Unsubscribe<RelicTriggeredEvent>(OnRelicTriggered);
        }

        // ============================================
        // Initialization
        // ============================================

        /// <summary>
        /// Initialize the display with current owned relics.
        /// </summary>
        public void Initialize()
        {
            if (!ServiceLocator.TryGet<IRelicManager>(out _relicManager))
            {
                Debug.LogWarning("[RelicDisplayBar] RelicManager not found");
                return;
            }

            RefreshDisplay();
        }

        /// <summary>
        /// Refreshes the relic display from RelicManager.
        /// </summary>
        public void RefreshDisplay()
        {
            if (_relicManager == null) return;
            if (_relicContainer == null)
            {
                Debug.LogWarning("[RelicDisplayBar] Relic container not assigned");
                return;
            }

            // Clear existing icons
            ClearIcons();

            // Create icons for owned relics
            var ownedRelics = _relicManager.OwnedRelics;
            int displayCount = Mathf.Min(ownedRelics.Count, _maxDisplay);

            for (int i = 0; i < displayCount; i++)
            {
                CreateRelicIcon(ownedRelics[i], i);
            }

            Debug.Log($"[RelicDisplayBar] Displaying {displayCount} relics");
        }

        // ============================================
        // Icon Management
        // ============================================

        private void CreateRelicIcon(RelicDataSO relic, int index)
        {
            // Use local prefab or fall back to config
            var prefab = _relicIconPrefab ?? RuntimeUIPrefabConfigSO.Instance?.RelicDisplayIconPrefab;

            if (prefab == null)
            {
                Debug.LogError($"[RelicDisplayBar] RelicIcon prefab not assigned. Check RuntimeUIPrefabConfig.");
                return;
            }

            var iconGO = Instantiate(prefab, _relicContainer);
            iconGO.name = $"Relic_{relic.RelicId}";

            // Set up icon slot
            var slot = iconGO.GetComponent<RelicIconSlot>();
            if (slot == null)
            {
                slot = iconGO.AddComponent<RelicIconSlot>();
            }

            slot.Initialize(relic, this);
            _iconSlots.Add(slot);
        }

        private void ClearIcons()
        {
            foreach (var slot in _iconSlots)
            {
                if (slot != null)
                {
                    Destroy(slot.gameObject);
                }
            }
            _iconSlots.Clear();
        }

        // ============================================
        // Event Handlers
        // ============================================

        private void OnRelicAcquired(RelicAcquiredEvent evt)
        {
            // Refresh display to include new relic
            RefreshDisplay();
        }

        private void OnRelicTriggered(RelicTriggeredEvent evt)
        {
            // Find and pulse the triggered relic icon
            foreach (var slot in _iconSlots)
            {
                if (slot.Relic == evt.Relic)
                {
                    slot.PlayTriggerAnimation();
                    break;
                }
            }
        }

        // ============================================
        // Tooltip
        // ============================================

        /// <summary>
        /// Show tooltip for a relic.
        /// </summary>
        public void ShowTooltip(RelicDataSO relic, Vector3 position)
        {
            if (_tooltipPanel == null || relic == null) return;

            _tooltipPanel.SetActive(true);
            _tooltipPanel.transform.position = position + new Vector3(0, 50f, 0);

            if (_tooltipName != null)
            {
                _tooltipName.text = relic.RelicName;
                _tooltipName.color = GetRarityColor(relic.Rarity);
            }

            if (_tooltipDescription != null)
            {
                _tooltipDescription.text = relic.Description;
            }

            if (_tooltipRarity != null)
            {
                _tooltipRarity.text = relic.Rarity.ToString();
                _tooltipRarity.color = GetRarityColor(relic.Rarity);
            }
        }

        /// <summary>
        /// Hide the tooltip.
        /// </summary>
        public void HideTooltip()
        {
            if (_tooltipPanel != null)
            {
                _tooltipPanel.SetActive(false);
            }
        }

        // ============================================
        // Helpers
        // ============================================

        private Color GetRarityColor(RelicRarity rarity)
        {
            return rarity switch
            {
                RelicRarity.Common => UIColors.PanelGray,
                RelicRarity.Uncommon => new Color(0.2f, 0.8f, 0.2f),  // Green
                RelicRarity.Rare => new Color(0.3f, 0.5f, 1f),        // Blue
                RelicRarity.Boss => new Color(1f, 0.5f, 0f),          // Orange
                _ => Color.white
            };
        }
    }

    /// <summary>
    /// Individual relic icon slot with interaction.
    /// </summary>
    public class RelicIconSlot : MonoBehaviour
    {
        private RelicDataSO _relic;
        private RelicDisplayBar _displayBar;
        private Image _icon;
        private Image _frame;

        public RelicDataSO Relic => _relic;

        public void Initialize(RelicDataSO relic, RelicDisplayBar displayBar)
        {
            _relic = relic;
            _displayBar = displayBar;

            // Set icon sprite
            _icon = GetComponent<Image>();
            if (_icon != null && relic.Icon != null)
            {
                _icon.sprite = relic.Icon;
            }

            // Add frame if child exists
            if (transform.childCount > 0)
            {
                _frame = transform.GetChild(0).GetComponent<Image>();
            }

            // Set up button interaction
            var button = GetComponent<Button>();
            if (button != null)
            {
                button.onClick.AddListener(OnClick);
            }

            // Set up hover events via EventTrigger
            var eventTrigger = GetComponent<UnityEngine.EventSystems.EventTrigger>();
            if (eventTrigger == null)
            {
                eventTrigger = gameObject.AddComponent<UnityEngine.EventSystems.EventTrigger>();
            }

            // Pointer Enter
            var enterEntry = new UnityEngine.EventSystems.EventTrigger.Entry
            {
                eventID = UnityEngine.EventSystems.EventTriggerType.PointerEnter
            };
            enterEntry.callback.AddListener((data) => OnPointerEnter());
            eventTrigger.triggers.Add(enterEntry);

            // Pointer Exit
            var exitEntry = new UnityEngine.EventSystems.EventTrigger.Entry
            {
                eventID = UnityEngine.EventSystems.EventTriggerType.PointerExit
            };
            exitEntry.callback.AddListener((data) => OnPointerExit());
            eventTrigger.triggers.Add(exitEntry);
        }

        private void OnClick()
        {
            // Toggle tooltip on click for mobile
            _displayBar?.ShowTooltip(_relic, transform.position);
        }

        private void OnPointerEnter()
        {
            _displayBar?.ShowTooltip(_relic, transform.position);

            // Slight scale up
            transform.DOScale(1.15f, 0.1f).SetLink(gameObject);
        }

        private void OnPointerExit()
        {
            _displayBar?.HideTooltip();

            // Scale back
            transform.DOScale(1f, 0.1f).SetLink(gameObject);
        }

        /// <summary>
        /// Play animation when relic triggers.
        /// </summary>
        public void PlayTriggerAnimation()
        {
            // Pulse animation
            var seq = DOTween.Sequence();
            seq.Append(transform.DOScale(1.3f, 0.1f));
            seq.Append(transform.DOScale(1f, 0.15f));
            seq.SetLink(gameObject);

            // Flash white
            if (_icon != null)
            {
                var originalColor = _icon.color;
                _icon.DOColor(Color.white, 0.1f)
                    .OnComplete(() => _icon.DOColor(originalColor, 0.2f).SetLink(gameObject))
                    .SetLink(gameObject);
            }
        }
    }
}
