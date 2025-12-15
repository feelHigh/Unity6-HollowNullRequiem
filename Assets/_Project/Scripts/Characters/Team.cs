// ============================================
// Team.cs
// Wrapper class for party of 3 Requiems
// ============================================

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using HNR.Cards;

namespace HNR.Characters
{
    /// <summary>
    /// Wrapper class for a team of Requiems.
    /// Provides utility methods for team-wide operations.
    /// </summary>
    [Serializable]
    public class Team : IEnumerable<RequiemInstance>
    {
        // ============================================
        // Constants
        // ============================================

        /// <summary>Maximum team size.</summary>
        public const int MaxSize = 3;

        // ============================================
        // Data
        // ============================================

        private readonly List<RequiemInstance> _members = new();

        // ============================================
        // Properties
        // ============================================

        /// <summary>Number of members in the team.</summary>
        public int Count => _members.Count;

        /// <summary>Whether the team is full (3 members).</summary>
        public bool IsFull => _members.Count >= MaxSize;

        /// <summary>Whether the team is empty.</summary>
        public bool IsEmpty => _members.Count == 0;

        /// <summary>
        /// Total maximum HP contribution from all team members.
        /// </summary>
        public int TotalMaxHP
        {
            get
            {
                int total = 0;
                foreach (var member in _members)
                {
                    if (member?.Data != null)
                    {
                        total += member.Data.BaseHP;
                    }
                }
                return total;
            }
        }

        /// <summary>
        /// Total base ATK from all team members.
        /// </summary>
        public int TotalBaseATK
        {
            get
            {
                int total = 0;
                foreach (var member in _members)
                {
                    if (member?.Data != null)
                    {
                        total += member.Data.BaseATK;
                    }
                }
                return total;
            }
        }

        /// <summary>
        /// Total base DEF from all team members.
        /// </summary>
        public int TotalBaseDEF
        {
            get
            {
                int total = 0;
                foreach (var member in _members)
                {
                    if (member?.Data != null)
                    {
                        total += member.Data.BaseDEF;
                    }
                }
                return total;
            }
        }

        /// <summary>
        /// Whether any team member is in Null State.
        /// </summary>
        public bool AnyInNullState
        {
            get
            {
                foreach (var member in _members)
                {
                    if (member != null && member.InNullState)
                    {
                        return true;
                    }
                }
                return false;
            }
        }

        /// <summary>
        /// Count of team members currently in Null State.
        /// </summary>
        public int NullStateCount
        {
            get
            {
                int count = 0;
                foreach (var member in _members)
                {
                    if (member != null && member.InNullState)
                    {
                        count++;
                    }
                }
                return count;
            }
        }

        /// <summary>
        /// Average corruption level across the team.
        /// </summary>
        public float AverageCorruption
        {
            get
            {
                if (_members.Count == 0) return 0f;
                int total = 0;
                foreach (var member in _members)
                {
                    if (member != null)
                    {
                        total += member.Corruption;
                    }
                }
                return (float)total / _members.Count;
            }
        }

        // ============================================
        // Indexer
        // ============================================

        /// <summary>
        /// Access team member by index.
        /// </summary>
        public RequiemInstance this[int index]
        {
            get => index >= 0 && index < _members.Count ? _members[index] : null;
        }

        // ============================================
        // Constructors
        // ============================================

        /// <summary>
        /// Create an empty team.
        /// </summary>
        public Team() { }

        /// <summary>
        /// Create a team from existing members.
        /// </summary>
        /// <param name="members">Initial members.</param>
        public Team(IEnumerable<RequiemInstance> members)
        {
            if (members != null)
            {
                foreach (var member in members)
                {
                    if (member != null && _members.Count < MaxSize)
                    {
                        _members.Add(member);
                    }
                }
            }
        }

        /// <summary>
        /// Create a team from a list.
        /// </summary>
        /// <param name="list">List of RequiemInstance.</param>
        public Team(List<RequiemInstance> list) : this((IEnumerable<RequiemInstance>)list) { }

        // ============================================
        // Member Management
        // ============================================

        /// <summary>
        /// Add a member to the team.
        /// </summary>
        /// <param name="member">Member to add.</param>
        /// <returns>True if added successfully.</returns>
        public bool Add(RequiemInstance member)
        {
            if (member == null || IsFull)
            {
                return false;
            }

            if (_members.Contains(member))
            {
                return false;
            }

            _members.Add(member);
            return true;
        }

        /// <summary>
        /// Remove a member from the team.
        /// </summary>
        /// <param name="member">Member to remove.</param>
        /// <returns>True if removed successfully.</returns>
        public bool Remove(RequiemInstance member)
        {
            return _members.Remove(member);
        }

        /// <summary>
        /// Clear all members from the team.
        /// </summary>
        public void Clear()
        {
            _members.Clear();
        }

        /// <summary>
        /// Check if team contains a specific member.
        /// </summary>
        /// <param name="member">Member to check.</param>
        /// <returns>True if member is in team.</returns>
        public bool Contains(RequiemInstance member)
        {
            return _members.Contains(member);
        }

        // ============================================
        // Query Methods
        // ============================================

        /// <summary>
        /// Get team member by Soul Aspect.
        /// </summary>
        /// <param name="aspect">Soul Aspect to find.</param>
        /// <returns>First member with matching aspect, or null.</returns>
        public RequiemInstance GetByAspect(SoulAspect aspect)
        {
            foreach (var member in _members)
            {
                if (member?.Data != null && member.Data.SoulAspect == aspect)
                {
                    return member;
                }
            }
            return null;
        }

        /// <summary>
        /// Get team member by Requiem class.
        /// </summary>
        /// <param name="requiemClass">Class to find.</param>
        /// <returns>First member with matching class, or null.</returns>
        public RequiemInstance GetByClass(RequiemClass requiemClass)
        {
            foreach (var member in _members)
            {
                if (member?.Data != null && member.Data.Class == requiemClass)
                {
                    return member;
                }
            }
            return null;
        }

        /// <summary>
        /// Get team member by ID.
        /// </summary>
        /// <param name="requiemId">Requiem ID to find.</param>
        /// <returns>Member with matching ID, or null.</returns>
        public RequiemInstance GetById(string requiemId)
        {
            foreach (var member in _members)
            {
                if (member?.Data != null && member.Data.RequiemId == requiemId)
                {
                    return member;
                }
            }
            return null;
        }

        /// <summary>
        /// Get all members in Null State.
        /// </summary>
        /// <returns>List of members in Null State.</returns>
        public List<RequiemInstance> GetMembersInNullState()
        {
            var result = new List<RequiemInstance>();
            foreach (var member in _members)
            {
                if (member != null && member.InNullState)
                {
                    result.Add(member);
                }
            }
            return result;
        }

        /// <summary>
        /// Get member with highest corruption.
        /// </summary>
        /// <returns>Member with highest corruption, or null if empty.</returns>
        public RequiemInstance GetHighestCorruption()
        {
            RequiemInstance highest = null;
            int maxCorruption = -1;

            foreach (var member in _members)
            {
                if (member != null && member.Corruption > maxCorruption)
                {
                    maxCorruption = member.Corruption;
                    highest = member;
                }
            }
            return highest;
        }

        /// <summary>
        /// Get member with lowest corruption.
        /// </summary>
        /// <returns>Member with lowest corruption, or null if empty.</returns>
        public RequiemInstance GetLowestCorruption()
        {
            RequiemInstance lowest = null;
            int minCorruption = int.MaxValue;

            foreach (var member in _members)
            {
                if (member != null && member.Corruption < minCorruption)
                {
                    minCorruption = member.Corruption;
                    lowest = member;
                }
            }
            return lowest;
        }

        // ============================================
        // Combat Lifecycle
        // ============================================

        /// <summary>
        /// Reset combat-specific flags for all members.
        /// Call this when combat ends.
        /// </summary>
        public void PostCombatCleanup()
        {
            foreach (var member in _members)
            {
                if (member == null) continue;

                // Reset Art usage
                member.HasUsedArtThisCombat = false;

                // Handle Null State exit (reset to 50 corruption per TDD)
                if (member.InNullState)
                {
                    // SetCorruption handles the _inNullState flag
                    member.SetCorruption(50);
                }
            }
        }

        /// <summary>
        /// Collect all starting cards from team members.
        /// </summary>
        /// <returns>Combined list of all starting cards.</returns>
        public List<CardDataSO> CollectStartingCards()
        {
            var cards = new List<CardDataSO>();
            foreach (var member in _members)
            {
                if (member?.Data?.StartingCards != null)
                {
                    foreach (var card in member.Data.StartingCards)
                    {
                        if (card != null)
                        {
                            cards.Add(card);
                        }
                    }
                }
            }
            return cards;
        }

        // ============================================
        // Conversion
        // ============================================

        /// <summary>
        /// Convert to List for compatibility with existing systems.
        /// </summary>
        /// <returns>List of team members.</returns>
        public List<RequiemInstance> ToList()
        {
            return new List<RequiemInstance>(_members);
        }

        /// <summary>
        /// Convert to array.
        /// </summary>
        /// <returns>Array of team members.</returns>
        public RequiemInstance[] ToArray()
        {
            return _members.ToArray();
        }

        /// <summary>
        /// Create Team from existing list.
        /// </summary>
        /// <param name="list">Source list.</param>
        /// <returns>New Team instance.</returns>
        public static Team FromList(List<RequiemInstance> list)
        {
            return new Team(list);
        }

        // ============================================
        // IEnumerable Implementation
        // ============================================

        public IEnumerator<RequiemInstance> GetEnumerator()
        {
            return _members.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        // ============================================
        // Implicit Conversion
        // ============================================

        /// <summary>
        /// Implicit conversion to List for backward compatibility.
        /// </summary>
        public static implicit operator List<RequiemInstance>(Team team)
        {
            return team?.ToList() ?? new List<RequiemInstance>();
        }
    }
}
