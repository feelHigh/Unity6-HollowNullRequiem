// ============================================
// PartyMemberSlot.cs
// Individual party member display with EP gauge
// ============================================

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using HNR.Core.Events;
using HNR.Combat;
using HNR.Characters;

namespace HNR.UI.Combat
{
    /// <summary>
    /// Individual party member display with portrait, EP gauge, and status icons.
    /// </summary>
    public class PartyMemberSlot : MonoBehaviour
    {
        [Header("Identity")]
        [SerializeField] private Image _portrait;
        [SerializeField] private Image _portraitFrame;
        [SerializeField] private TMP_Text _nameLabel;

        [Header("EP Gauge")]
        [SerializeField] private Slider _epBar;
        [SerializeField] private TMP_Text _epText;
        [SerializeField] private Image _epFill;
        [SerializeField] private int _maxEP = 100;

        [Header("Status Effects")]
        [SerializeField] private Transform _statusContainer;
        [SerializeField] private StatusIconUI _statusPrefab;
        [SerializeField] private int _maxVisibleStatuses = 4;

        [Header("Highlight")]
        [SerializeField] private Image _activeGlow;

        private RequiemInstance _requiem;
        private Dictionary<StatusType, StatusIconUI> _activeStatuses = new();
        private Sequence _glowSequence;

        /// <summary>
        /// Initializes the slot with a Requiem instance.
        /// </summary>
        /// <param name="requiem">The Requiem to display.</param>
        public void Initialize(RequiemInstance requiem)
        {
            _requiem = requiem;

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

            UpdateEP(requiem.SoulEssence, _maxEP);

            EventBus.Subscribe<SoulEssenceChangedEvent>(OnSEChanged);
            EventBus.Subscribe<StatusAppliedEvent>(OnStatusApplied);
            EventBus.Subscribe<StatusRemovedEvent>(OnStatusRemoved);
        }

        private void OnDestroy()
        {
            EventBus.Unsubscribe<SoulEssenceChangedEvent>(OnSEChanged);
            EventBus.Unsubscribe<StatusAppliedEvent>(OnStatusApplied);
            EventBus.Unsubscribe<StatusRemovedEvent>(OnStatusRemoved);
            _glowSequence?.Kill();
        }

        private void OnSEChanged(SoulEssenceChangedEvent evt)
        {
            // SE is team-wide, update all slots
            if (_requiem != null)
            {
                UpdateEP(evt.Current, _maxEP);
            }
        }

        private void UpdateEP(int current, int max)
        {
            if (_epBar != null)
            {
                _epBar.value = max > 0 ? (float)current / max : 0;
            }

            if (_epText != null)
            {
                _epText.text = current.ToString();
            }

            if (_epFill != null)
            {
                bool isFull = max > 0 && (float)current / max >= 1f;
                _epFill.color = isFull ? UIColors.SoulGold : UIColors.SoulCyan;
            }
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
            if (_requiem != null)
            {
                UpdateEP(_requiem.SoulEssence, _maxEP);
            }
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
