// ============================================
// PartyStatusSidebar.cs
// Left sidebar showing party members with shared SE gauge
// Uses Slider component for reliable gauge updates
// ============================================

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using HNR.Core;
using HNR.Core.Events;
using HNR.Combat;
using HNR.Characters;

#pragma warning disable CS0414 // Field is assigned but never used (reserved for future Inspector configuration)

namespace HNR.UI.Combat
{
    /// <summary>
    /// Left sidebar showing party members with shared Soul Essence gauge.
    /// SE is a team-wide resource displayed as a single vertical gauge.
    /// Uses Slider component for reliable visual updates.
    /// </summary>
    public class PartyStatusSidebar : MonoBehaviour
    {
        [Header("Shared Soul Essence Slider")]
        [SerializeField] private Slider _sharedSESlider;
        [SerializeField] private TMP_Text _sharedSEText;
        [SerializeField] private Image _sharedSEFillImage;
        [SerializeField] private int _maxSE = 100;

        [Header("Layout")]
        [SerializeField] private float _slotSpacing = 8f;

        [Header("Slots")]
        [SerializeField] private PartyMemberSlot[] _memberSlots;

        private int _activeSlotIndex = -1;
        private bool _isArtReady;

        private void Awake()
        {
            // Configure slider for SE display
            if (_sharedSESlider != null)
            {
                _sharedSESlider.minValue = 0f;
                _sharedSESlider.maxValue = 1f;
                _sharedSESlider.value = 0f;
                _sharedSESlider.interactable = false; // Display only, not interactive
                Debug.Log("[PartyStatusSidebar] SE slider configured: min=0, max=1, value=0");
            }
        }

        private void OnEnable()
        {
            EventBus.Subscribe<SoulEssenceChangedEvent>(OnSEChanged);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<SoulEssenceChangedEvent>(OnSEChanged);
        }

        private void OnSEChanged(SoulEssenceChangedEvent evt)
        {
            UpdateSharedSE(evt.Current);
        }

        /// <summary>
        /// Updates the shared Soul Essence gauge display.
        /// </summary>
        /// <param name="current">Current SE value.</param>
        private void UpdateSharedSE(int current)
        {
            float normalized = _maxSE > 0 ? (float)current / _maxSE : 0f;
            bool isFull = normalized >= 1f;

            // Update Slider value
            if (_sharedSESlider != null)
            {
                _sharedSESlider.value = normalized;
            }

            // Update SE text display
            if (_sharedSEText != null)
            {
                _sharedSEText.text = current.ToString();
            }

            // Update fill image color
            if (_sharedSEFillImage != null)
            {
                // Gold when full (Art ready), Cyan otherwise
                Color targetColor = isFull ? UIColors.SoulGold : UIColors.SoulCyan;
                _sharedSEFillImage.color = targetColor;
            }

            // Trigger pulse when Art becomes ready
            if (isFull && !_isArtReady)
            {
                TriggerArtReadyFeedback();
            }
            _isArtReady = isFull;
        }

        /// <summary>
        /// Visual feedback when SE is full and Requiem Art is ready.
        /// </summary>
        private void TriggerArtReadyFeedback()
        {
            if (_sharedSESlider != null)
            {
                var seq = DOTween.Sequence();
                seq.Append(_sharedSESlider.transform.DOScale(1.1f, 0.15f));
                seq.Append(_sharedSESlider.transform.DOScale(1f, 0.15f));
                seq.SetLink(gameObject);
            }

            Debug.Log("[PartyStatusSidebar] Soul Essence full - Requiem Art ready!");
        }

        /// <summary>
        /// Initializes the sidebar with the party team.
        /// </summary>
        /// <param name="team">Array of RequiemInstance for the party.</param>
        public void Initialize(RequiemInstance[] team)
        {
            if (_memberSlots == null || team == null) return;

            for (int i = 0; i < _memberSlots.Length; i++)
            {
                if (i < team.Length && team[i] != null)
                {
                    _memberSlots[i].Initialize(team[i], i); // Pass slot index for click handling
                    _memberSlots[i].gameObject.SetActive(true);
                }
                else
                {
                    _memberSlots[i].gameObject.SetActive(false);
                }
            }

            // Note: Art glow is managed by each PartyMemberSlot based on SE
            // Do NOT call SetActiveSlot here - it would override the correct glow state

            // Initialize SE gauge with persisted value from TurnManager context
            // SE now persists across combats during a run
            int initialSE = 0;
            if (ServiceLocator.TryGet<TurnManager>(out var turnManager) && turnManager.Context != null)
            {
                initialSE = turnManager.Context.SoulEssence;
            }
            UpdateSharedSE(initialSE);
            _isArtReady = initialSE >= _maxSE;
            Debug.Log($"[PartyStatusSidebar] Initialized - SE gauge set to {initialSE}");
        }

        /// <summary>
        /// Sets which party member slot shows the active highlight.
        /// </summary>
        /// <param name="index">Index of the active member (0-2).</param>
        public void SetActiveSlot(int index)
        {
            _activeSlotIndex = index;

            for (int i = 0; i < _memberSlots.Length; i++)
            {
                if (_memberSlots[i] != null && _memberSlots[i].gameObject.activeSelf)
                {
                    _memberSlots[i].SetActive(i == index);
                }
            }
        }

        /// <summary>
        /// Refreshes all visible slot displays.
        /// </summary>
        public void RefreshAll()
        {
            if (_memberSlots == null) return;

            foreach (var slot in _memberSlots)
            {
                if (slot != null && slot.gameObject.activeSelf)
                {
                    slot.Refresh();
                }
            }
        }

        /// <summary>
        /// Gets the currently active slot index.
        /// </summary>
        public int ActiveSlotIndex => _activeSlotIndex;

        /// <summary>
        /// Gets the number of active slots.
        /// </summary>
        public int ActiveSlotCount
        {
            get
            {
                int count = 0;
                if (_memberSlots != null)
                {
                    foreach (var slot in _memberSlots)
                    {
                        if (slot != null && slot.gameObject.activeSelf)
                        {
                            count++;
                        }
                    }
                }
                return count;
            }
        }

        /// <summary>
        /// Clears all status effects from all slots.
        /// </summary>
        public void ClearAllStatuses()
        {
            if (_memberSlots == null) return;

            foreach (var slot in _memberSlots)
            {
                if (slot != null)
                {
                    slot.ClearStatuses();
                }
            }
        }
    }
}
