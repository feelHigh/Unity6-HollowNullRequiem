// ============================================
// PartyStatusSidebar.cs
// Left sidebar showing party members with EP gauges
// ============================================

using UnityEngine;
using HNR.Characters;

namespace HNR.UI.Combat
{
    /// <summary>
    /// Left sidebar showing party members with EP gauges and status icons.
    /// Per CZN layout specification.
    /// </summary>
    public class PartyStatusSidebar : MonoBehaviour
    {
        [Header("Layout")]
        [SerializeField] private float _slotSpacing = 8f;

        [Header("Slots")]
        [SerializeField] private PartyMemberSlot[] _memberSlots;

        private int _activeSlotIndex = -1;

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
                    _memberSlots[i].Initialize(team[i]);
                    _memberSlots[i].gameObject.SetActive(true);
                }
                else
                {
                    _memberSlots[i].gameObject.SetActive(false);
                }
            }

            // Default first slot as active
            if (team.Length > 0)
            {
                SetActiveSlot(0);
            }
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
