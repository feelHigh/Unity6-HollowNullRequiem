// ============================================
// CorruptionManager.cs
// Centralized corruption system management
// ============================================

using UnityEngine;
using System.Collections.Generic;
using HNR.Core;
using HNR.Core.Events;

namespace HNR.Characters
{
    /// <summary>
    /// Manages the Hollow Corruption system for the team.
    /// Coordinates corruption across all Requiems and handles team-wide effects.
    /// </summary>
    /// <remarks>
    /// Corruption thresholds per GDD:
    /// - 0-24: Safe (no penalty)
    /// - 25-49: Uneasy (healing -10%)
    /// - 50-74: Strained (healing -20%)
    /// - 75-99: Critical (healing -30%)
    /// - 100: Null State (transformation)
    /// </remarks>
    public class CorruptionManager : MonoBehaviour
    {
        // ============================================
        // Constants
        // ============================================

        public const int MAX_CORRUPTION = 100;
        public const int NULL_STATE_THRESHOLD = 100;
        public const int NULL_STATE_DRAIN_PER_TURN = 5;

        // Corruption thresholds
        public const int UNEASY_THRESHOLD = 25;
        public const int STRAINED_THRESHOLD = 50;
        public const int CRITICAL_THRESHOLD = 75;

        // ============================================
        // State
        // ============================================

        private List<RequiemInstance> _team = new();
        private int _arenaCorruptionPerTurn = 0;

        // ============================================
        // Lifecycle
        // ============================================

        private void Awake()
        {
            ServiceLocator.Register(this);
        }

        private void OnDestroy()
        {
            ServiceLocator.Unregister<CorruptionManager>();
        }

        // ============================================
        // Initialization
        // ============================================

        /// <summary>
        /// Initialize the corruption manager with the team.
        /// </summary>
        /// <param name="team">List of RequiemInstances for this combat</param>
        public void Initialize(List<RequiemInstance> team)
        {
            _team.Clear();
            _team.AddRange(team);
            _arenaCorruptionPerTurn = 0;

            Debug.Log($"[CorruptionManager] Initialized with {_team.Count} Requiems");
        }

        /// <summary>
        /// Set arena corruption effect (e.g., boss fight modifier).
        /// </summary>
        /// <param name="corruptionPerTurn">Corruption added to each Requiem per turn</param>
        public void SetArenaCorruption(int corruptionPerTurn)
        {
            _arenaCorruptionPerTurn = corruptionPerTurn;
            Debug.Log($"[CorruptionManager] Arena corruption set to {corruptionPerTurn}/turn");
        }

        // ============================================
        // Corruption Operations
        // ============================================

        /// <summary>
        /// Get corruption value for a specific Requiem.
        /// </summary>
        public int GetCorruption(RequiemInstance requiem)
        {
            return requiem?.Corruption ?? 0;
        }

        /// <summary>
        /// Get the corruption state tier for a Requiem.
        /// </summary>
        public CorruptionState GetCorruptionState(RequiemInstance requiem)
        {
            int corruption = GetCorruption(requiem);

            if (corruption >= NULL_STATE_THRESHOLD)
                return CorruptionState.NullState;
            if (corruption >= CRITICAL_THRESHOLD)
                return CorruptionState.Critical;
            if (corruption >= STRAINED_THRESHOLD)
                return CorruptionState.Strained;
            if (corruption >= UNEASY_THRESHOLD)
                return CorruptionState.Uneasy;

            return CorruptionState.Safe;
        }

        /// <summary>
        /// Add corruption to a specific Requiem.
        /// </summary>
        public void AddCorruption(RequiemInstance requiem, int amount)
        {
            if (requiem == null || amount <= 0) return;

            requiem.AddCorruption(amount);
            Debug.Log($"[CorruptionManager] {requiem.Name} gained {amount} corruption. Total: {requiem.Corruption}");
        }

        /// <summary>
        /// Remove corruption from a specific Requiem.
        /// </summary>
        public void RemoveCorruption(RequiemInstance requiem, int amount)
        {
            if (requiem == null || amount <= 0) return;

            requiem.RemoveCorruption(amount);
            Debug.Log($"[CorruptionManager] {requiem.Name} purified {amount} corruption. Total: {requiem.Corruption}");
        }

        /// <summary>
        /// Add corruption to all Requiems in the team.
        /// </summary>
        public void AddCorruptionToTeam(int amount)
        {
            if (amount <= 0) return;

            foreach (var requiem in _team)
            {
                if (requiem != null && !requiem.IsDead)
                {
                    requiem.AddCorruption(amount);
                }
            }

            Debug.Log($"[CorruptionManager] Team gained {amount} corruption");
        }

        /// <summary>
        /// Remove corruption from all Requiems in the team.
        /// </summary>
        public void RemoveCorruptionFromTeam(int amount)
        {
            if (amount <= 0) return;

            foreach (var requiem in _team)
            {
                if (requiem != null && !requiem.IsDead)
                {
                    requiem.RemoveCorruption(amount);
                }
            }

            Debug.Log($"[CorruptionManager] Team purified {amount} corruption");
        }

        // ============================================
        // Null State Queries
        // ============================================

        /// <summary>
        /// Check if a Requiem is in Null State.
        /// </summary>
        public bool IsInNullState(RequiemInstance requiem)
        {
            return requiem?.InNullState ?? false;
        }

        /// <summary>
        /// Get all Requiems currently in Null State.
        /// </summary>
        public List<RequiemInstance> GetNullStateRequiems()
        {
            var result = new List<RequiemInstance>();
            foreach (var requiem in _team)
            {
                if (requiem != null && requiem.InNullState)
                {
                    result.Add(requiem);
                }
            }
            return result;
        }

        /// <summary>
        /// Get the total team corruption (sum of all Requiems).
        /// </summary>
        public int GetTotalTeamCorruption()
        {
            int total = 0;
            foreach (var requiem in _team)
            {
                if (requiem != null)
                {
                    total += requiem.Corruption;
                }
            }
            return total;
        }

        /// <summary>
        /// Get the average team corruption.
        /// </summary>
        public float GetAverageTeamCorruption()
        {
            if (_team.Count == 0) return 0f;
            return (float)GetTotalTeamCorruption() / _team.Count;
        }

        // ============================================
        // Turn Processing
        // ============================================

        /// <summary>
        /// Process end of player turn effects.
        /// - Apply Null State HP drain
        /// - Apply arena corruption
        /// </summary>
        public void ProcessTurnEnd()
        {
            // Apply arena corruption to all living Requiems
            if (_arenaCorruptionPerTurn > 0)
            {
                AddCorruptionToTeam(_arenaCorruptionPerTurn);
            }

            // Apply Null State drain
            foreach (var requiem in _team)
            {
                if (requiem != null && !requiem.IsDead && requiem.InNullState)
                {
                    requiem.TakeDamage(NULL_STATE_DRAIN_PER_TURN);
                    Debug.Log($"[CorruptionManager] {requiem.Name} suffers {NULL_STATE_DRAIN_PER_TURN} Null State drain");
                }
            }
        }

        /// <summary>
        /// Process corruption at combat end.
        /// Requiems in Null State have corruption reset to 50.
        /// </summary>
        public void ProcessCombatEnd()
        {
            foreach (var requiem in _team)
            {
                if (requiem != null && requiem.InNullState)
                {
                    // Reset to 50 corruption after combat
                    int currentCorruption = requiem.Corruption;
                    if (currentCorruption > 50)
                    {
                        requiem.RemoveCorruption(currentCorruption - 50);
                        Debug.Log($"[CorruptionManager] {requiem.Name} Null State reset to 50 corruption");
                    }
                }
            }
        }

        // ============================================
        // Healing Modifier
        // ============================================

        /// <summary>
        /// Get the healing multiplier based on corruption level.
        /// </summary>
        /// <param name="requiem">The Requiem to check</param>
        /// <returns>Multiplier (1.0 = full healing, 0.7 = 70% healing)</returns>
        public float GetHealingMultiplier(RequiemInstance requiem)
        {
            var state = GetCorruptionState(requiem);

            return state switch
            {
                CorruptionState.Safe => 1.0f,
                CorruptionState.Uneasy => 0.9f,
                CorruptionState.Strained => 0.8f,
                CorruptionState.Critical => 0.7f,
                CorruptionState.NullState => 0.5f,
                _ => 1.0f
            };
        }

        // ============================================
        // Damage Modifier (Null State)
        // ============================================

        /// <summary>
        /// Get the damage multiplier for cards from a Requiem.
        /// </summary>
        /// <param name="requiem">The card's owner</param>
        /// <returns>Multiplier (1.0 = normal, 1.5 = +50% in Null State)</returns>
        public float GetDamageMultiplier(RequiemInstance requiem)
        {
            if (requiem != null && requiem.InNullState)
            {
                return 1.5f; // +50% damage in Null State
            }
            return 1.0f;
        }

        /// <summary>
        /// Get the AP cost reduction for cards from a Requiem in Null State.
        /// </summary>
        /// <param name="requiem">The card's owner</param>
        /// <returns>AP reduction (0 = none, 1 = -1 AP in Null State)</returns>
        public int GetAPReduction(RequiemInstance requiem)
        {
            if (requiem != null && requiem.InNullState)
            {
                return 1; // -1 AP cost in Null State (minimum 1)
            }
            return 0;
        }
    }

    /// <summary>
    /// Corruption state tiers per GDD thresholds.
    /// </summary>
    public enum CorruptionState
    {
        Safe,       // 0-24
        Uneasy,     // 25-49
        Strained,   // 50-74
        Critical,   // 75-99
        NullState   // 100
    }
}
