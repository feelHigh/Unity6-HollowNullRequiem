// ============================================
// PartyMemberSlot.cs
// Individual party member display with portrait and status icons
// Clickable to activate Requiem Art
// ============================================

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using HNR.Core;
using HNR.Core.Events;
using HNR.Combat;
using HNR.Characters;

namespace HNR.UI.Combat
{
    /// <summary>
    /// Individual party member display with portrait and status icons.
    /// Clickable to activate the corresponding Requiem's Art ability.
    /// SE is handled by PartyStatusSidebar as a shared gauge.
    /// </summary>
    public class PartyMemberSlot : MonoBehaviour
    {
        [Header("Identity")]
        [SerializeField] private Image _portrait;
        [SerializeField] private Image _portraitFrame;
        [SerializeField] private TMP_Text _nameLabel;

        [Header("Status Effects")]
        [SerializeField] private Transform _statusContainer;
        [SerializeField] private StatusIconUI _statusPrefab;
        [SerializeField] private int _maxVisibleStatuses = 4;

        [Header("Highlight")]
        [SerializeField] private Image _activeGlow;

        [Header("Interaction")]
        [SerializeField] private Button _slotButton;

        private RequiemInstance _requiem;
        private int _slotIndex;
        private Dictionary<StatusType, StatusIconUI> _activeStatuses = new();
        private Sequence _glowSequence;

        /// <summary>
        /// Gets the Requiem associated with this slot.
        /// </summary>
        public RequiemInstance Requiem => _requiem;

        /// <summary>
        /// Gets the slot index (0-2).
        /// </summary>
        public int SlotIndex => _slotIndex;

        private void Awake()
        {
            // Auto-wire button if not set in Inspector
            if (_slotButton == null)
            {
                _slotButton = GetComponent<Button>();
            }

            // Wire click handler
            if (_slotButton != null)
            {
                _slotButton.onClick.AddListener(OnSlotClicked);
            }
        }

        /// <summary>
        /// Initializes the slot with a Requiem instance.
        /// </summary>
        /// <param name="requiem">The Requiem to display.</param>
        /// <param name="index">The slot index (0-2).</param>
        public void Initialize(RequiemInstance requiem, int index = 0)
        {
            // Unsubscribe first to prevent duplicate subscriptions on re-initialization
            EventBus.Unsubscribe<StatusAppliedEvent>(OnStatusApplied);
            EventBus.Unsubscribe<StatusRemovedEvent>(OnStatusRemoved);
            EventBus.Unsubscribe<SoulEssenceChangedEvent>(OnSEChanged);

            _requiem = requiem;
            _slotIndex = index;

            if (_portrait != null && requiem.Data?.Portrait != null)
            {
                _portrait.sprite = requiem.Data.Portrait;
            }

            if (_nameLabel != null && requiem.Data != null)
            {
                _nameLabel.text = requiem.Data.RequiemName;
            }

            if (_portraitFrame != null && requiem.Data != null)
            {
                _portraitFrame.color = UIColors.GetAspectColor(requiem.Data.SoulAspect);
            }

            // Reset Art-ready glow at combat start (SE starts at 0, so no Art available)
            SetActive(false);

            // Clear any leftover status icons from previous combat
            ClearStatuses();

            // Subscribe to events
            EventBus.Subscribe<StatusAppliedEvent>(OnStatusApplied);
            EventBus.Subscribe<StatusRemovedEvent>(OnStatusRemoved);
            EventBus.Subscribe<SoulEssenceChangedEvent>(OnSEChanged);
        }

        /// <summary>
        /// Called when this slot is clicked. Attempts to activate Requiem Art.
        /// </summary>
        private void OnSlotClicked()
        {
            if (_requiem == null)
            {
                Debug.LogWarning("[PartyMemberSlot] No Requiem assigned to slot");
                return;
            }

            Debug.Log($"[PartyMemberSlot] Slot clicked for {_requiem.Name}");

            // Try to activate Requiem Art via CombatManager
            var combatManager = CombatManager.Instance;
            if (combatManager != null)
            {
                bool success = combatManager.TryActivateArt(_requiem);
                if (success)
                {
                    Debug.Log($"[PartyMemberSlot] Requiem Art activated for {_requiem.Name}!");
                    // Visual feedback for successful activation
                    TriggerArtActivationFeedback();
                }
                else
                {
                    Debug.Log($"[PartyMemberSlot] Cannot activate Art - insufficient SE or already used");
                    // Visual feedback for failed activation
                    TriggerArtFailedFeedback();
                }
            }
            else
            {
                Debug.LogWarning("[PartyMemberSlot] CombatManager not available");
            }
        }

        private void OnSEChanged(SoulEssenceChangedEvent evt)
        {
            // Update glow state when SE changes (show glow when Art is ready)
            UpdateArtReadyGlow();
        }

        /// <summary>
        /// Updates the active glow to show when Requiem Art is ready to use.
        /// </summary>
        private void UpdateArtReadyGlow()
        {
            if (_requiem == null || _activeGlow == null) return;

            // Check if Art can be activated (enough SE or in Null State)
            var combatManager = CombatManager.Instance;
            if (combatManager != null)
            {
                bool canActivate = combatManager.CanActivateArt(_requiem);
                bool notUsedYet = !_requiem.HasUsedArtThisCombat;

                if (canActivate && notUsedYet)
                {
                    SetActive(true);
                }
                else
                {
                    SetActive(false);
                }
            }
            else
            {
                SetActive(false);
            }
        }

        private void TriggerArtActivationFeedback()
        {
            // Flash gold and pulse
            if (_portraitFrame != null)
            {
                var originalColor = _portraitFrame.color;
                var seq = DOTween.Sequence();
                seq.Append(_portraitFrame.DOColor(UIColors.SoulGold, 0.1f));
                seq.Append(_portraitFrame.DOColor(originalColor, 0.3f));
                seq.SetLink(gameObject);
            }

            // Scale pulse
            transform.DOPunchScale(Vector3.one * 0.1f, 0.3f, 2).SetLink(gameObject);
        }

        private void TriggerArtFailedFeedback()
        {
            // Shake to indicate failure
            transform.DOShakePosition(0.2f, 3f, 10).SetLink(gameObject);

            // Brief red flash on frame
            if (_portraitFrame != null)
            {
                var originalColor = _portraitFrame.color;
                var seq = DOTween.Sequence();
                seq.Append(_portraitFrame.DOColor(UIColors.CorruptionGlow, 0.1f));
                seq.Append(_portraitFrame.DOColor(originalColor, 0.2f));
                seq.SetLink(gameObject);
            }
        }

        private void OnDestroy()
        {
            EventBus.Unsubscribe<StatusAppliedEvent>(OnStatusApplied);
            EventBus.Unsubscribe<StatusRemovedEvent>(OnStatusRemoved);
            EventBus.Unsubscribe<SoulEssenceChangedEvent>(OnSEChanged);

            if (_slotButton != null)
            {
                _slotButton.onClick.RemoveListener(OnSlotClicked);
            }

            _glowSequence?.Kill();
        }

        /// <summary>
        /// Sets whether this slot shows the active highlight.
        /// </summary>
        /// <param name="active">True to show active glow.</param>
        public void SetActive(bool active)
        {
            _glowSequence?.Kill();

            if (_activeGlow != null)
            {
                _activeGlow.gameObject.SetActive(active);

                if (active)
                {
                    _activeGlow.color = new Color(
                        _activeGlow.color.r,
                        _activeGlow.color.g,
                        _activeGlow.color.b,
                        0.3f);

                    _glowSequence = DOTween.Sequence();
                    _glowSequence.Append(_activeGlow.DOFade(0.6f, 0.5f));
                    _glowSequence.Append(_activeGlow.DOFade(0.3f, 0.5f));
                    _glowSequence.SetLoops(-1);
                }
            }
        }

        private void OnStatusApplied(StatusAppliedEvent evt)
        {
            if (_requiem == null || evt.Target != (ICombatTarget)_requiem) return;

            if (_activeStatuses.ContainsKey(evt.StatusType))
            {
                // Update existing status stacks
                _activeStatuses[evt.StatusType].UpdateStacks(evt.Stacks);
            }
            else if (_activeStatuses.Count < _maxVisibleStatuses && _statusPrefab != null && _statusContainer != null)
            {
                // Add new status icon
                var icon = Instantiate(_statusPrefab, _statusContainer);
                icon.Initialize(evt.StatusType, evt.Stacks);
                _activeStatuses[evt.StatusType] = icon;
            }
        }

        private void OnStatusRemoved(StatusRemovedEvent evt)
        {
            if (_requiem == null || evt.Target != (ICombatTarget)_requiem) return;

            if (_activeStatuses.TryGetValue(evt.StatusType, out var icon))
            {
                _activeStatuses.Remove(evt.StatusType);
                if (icon != null)
                {
                    Destroy(icon.gameObject);
                }
            }
        }

        /// <summary>
        /// Refreshes the display from current Requiem state.
        /// </summary>
        public void Refresh()
        {
            // Portrait and frame color are set during Initialize
            // SE is now handled by PartyStatusSidebar as a shared gauge
            // Status effects update via event subscriptions
        }

        /// <summary>
        /// Clears all status icons.
        /// </summary>
        public void ClearStatuses()
        {
            foreach (var kvp in _activeStatuses)
            {
                if (kvp.Value != null)
                {
                    Destroy(kvp.Value.gameObject);
                }
            }
            _activeStatuses.Clear();
        }
    }
}
