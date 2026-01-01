// ============================================
// PlayerProgressionManager.cs
// Manages player XP and leveling system
// ============================================

using UnityEngine;
using HNR.Core;
using HNR.Core.Events;
using HNR.Core.Interfaces;
using HNR.UI;

namespace HNR.Progression
{
    /// <summary>
    /// Manages player XP and level progression across runs.
    /// XP is earned by completing zones and persists permanently.
    /// </summary>
    /// <remarks>
    /// Level curve: XP required = 100 * 1.2^level (exponential)
    /// Zone XP: Base (zone * 100) * difficulty multiplier
    /// Difficulty: Easy 1x, Normal 1.5x, Hard 2x
    /// </remarks>
    public class PlayerProgressionManager : MonoBehaviour
    {
        // ============================================
        // Singleton
        // ============================================

        public static PlayerProgressionManager Instance { get; private set; }

        // ============================================
        // State
        // ============================================

        private int _playerLevel = 1;
        private int _currentXP;
        private int _totalXP;

        // ============================================
        // Public Properties
        // ============================================

        /// <summary>Current player level.</summary>
        public int PlayerLevel => _playerLevel;

        /// <summary>Current XP within level (progress toward next level).</summary>
        public int CurrentXP => _currentXP;

        /// <summary>Total XP earned across all runs.</summary>
        public int TotalXP => _totalXP;

        /// <summary>XP required to reach next level.</summary>
        public int XPForNextLevel => GetXPRequiredForLevel(_playerLevel);

        // ============================================
        // Unity Lifecycle
        // ============================================

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            ServiceLocator.Register(this);
            LoadProgress();

            Debug.Log($"[PlayerProgressionManager] Initialized. Level: {_playerLevel}, XP: {_currentXP}/{XPForNextLevel}");
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
                ServiceLocator.Unregister<PlayerProgressionManager>();
            }
        }

        // ============================================
        // XP Calculation
        // ============================================

        /// <summary>
        /// Calculate XP reward for completing a zone.
        /// </summary>
        /// <param name="zone">Zone number (1-3)</param>
        /// <param name="difficulty">Difficulty level</param>
        /// <returns>XP amount to award</returns>
        public int CalculateZoneXP(int zone, DifficultyLevel difficulty)
        {
            // Base XP: Zone 1 = 100, Zone 2 = 200, Zone 3 = 300
            int baseXP = zone * 100;

            // Difficulty multiplier: Easy 1x, Normal 1.5x, Hard 2x
            float multiplier = difficulty switch
            {
                DifficultyLevel.Easy => 1.0f,
                DifficultyLevel.Normal => 1.5f,
                DifficultyLevel.Hard => 2.0f,
                _ => 1.0f
            };

            return Mathf.RoundToInt(baseXP * multiplier);
        }

        /// <summary>
        /// Calculate XP required to reach a given level.
        /// Uses exponential curve: 100 * 1.2^level
        /// </summary>
        /// <param name="level">Target level</param>
        /// <returns>XP required for that level</returns>
        public int GetXPRequiredForLevel(int level)
        {
            return Mathf.RoundToInt(100f * Mathf.Pow(1.2f, level));
        }

        // ============================================
        // XP Award Methods
        // ============================================

        /// <summary>
        /// Award XP for completing a zone.
        /// Calculates XP based on zone and difficulty, then adds it.
        /// </summary>
        /// <param name="zone">Zone number (1-3)</param>
        /// <param name="difficulty">Difficulty level</param>
        public void AwardZoneCompletion(int zone, DifficultyLevel difficulty)
        {
            int xpAmount = CalculateZoneXP(zone, difficulty);
            Debug.Log($"[PlayerProgressionManager] Awarding {xpAmount} XP for Zone {zone} on {difficulty}");
            AddXP(xpAmount);
        }

        /// <summary>
        /// Add XP to the player and check for level up.
        /// Saves progress and publishes events.
        /// </summary>
        /// <param name="amount">XP amount to add</param>
        public void AddXP(int amount)
        {
            if (amount <= 0) return;

            int oldLevel = _playerLevel;
            _currentXP += amount;
            _totalXP += amount;

            // Check for level up(s)
            while (_currentXP >= XPForNextLevel)
            {
                _currentXP -= XPForNextLevel;
                _playerLevel++;
                Debug.Log($"[PlayerProgressionManager] Level up! Now level {_playerLevel}");
            }

            // Save progress
            SaveProgress();

            // Publish events
            EventBus.Publish(new PlayerExpChangedEvent(_currentXP, XPForNextLevel));

            if (_playerLevel > oldLevel)
            {
                EventBus.Publish(new PlayerLevelChangedEvent(oldLevel, _playerLevel));
            }
        }

        // ============================================
        // UI Helper Methods
        // ============================================

        /// <summary>
        /// Get current level.
        /// </summary>
        public int GetLevel()
        {
            return _playerLevel;
        }

        /// <summary>
        /// Get XP progress as 0-1 float for UI bars.
        /// </summary>
        public float GetXPProgress()
        {
            int required = XPForNextLevel;
            return required > 0 ? (float)_currentXP / required : 0f;
        }

        /// <summary>
        /// Get XP required to reach next level.
        /// </summary>
        public int GetXPForNextLevel()
        {
            return XPForNextLevel;
        }

        /// <summary>
        /// Reset all player progression to default values.
        /// Called when starting a New Run to clear all progress.
        /// </summary>
        public void ResetProgression()
        {
            int oldLevel = _playerLevel;

            _playerLevel = 1;
            _currentXP = 0;
            _totalXP = 0;

            SaveProgress();

            // Publish events for UI updates
            EventBus.Publish(new PlayerExpChangedEvent(0, XPForNextLevel));
            if (oldLevel != 1)
            {
                EventBus.Publish(new PlayerLevelChangedEvent(oldLevel, 1));
            }

            Debug.Log("[PlayerProgressionManager] Progression reset to Level 1");
        }

        // ============================================
        // Persistence
        // ============================================

        /// <summary>
        /// Load player progression from save data.
        /// </summary>
        private void LoadProgress()
        {
            if (!ServiceLocator.TryGet<ISaveManager>(out var saveManagerInterface))
            {
                Debug.LogWarning("[PlayerProgressionManager] SaveManager not found. Using defaults.");
                return;
            }

            var saveManager = saveManagerInterface as SaveManager;
            if (saveManager == null)
            {
                Debug.LogWarning("[PlayerProgressionManager] Could not cast to SaveManager. Using defaults.");
                return;
            }

            var metaData = saveManager.LoadMeta();
            if (metaData != null)
            {
                _playerLevel = Mathf.Max(1, metaData.PlayerLevel);
                _currentXP = metaData.CurrentXP;
                _totalXP = metaData.TotalXP;
            }
        }

        /// <summary>
        /// Save player progression to meta save data.
        /// </summary>
        private void SaveProgress()
        {
            if (!ServiceLocator.TryGet<ISaveManager>(out var saveManagerInterface))
            {
                Debug.LogWarning("[PlayerProgressionManager] SaveManager not found. Cannot save progress.");
                return;
            }

            var saveManager = saveManagerInterface as SaveManager;
            if (saveManager == null)
            {
                Debug.LogWarning("[PlayerProgressionManager] Could not cast to SaveManager. Cannot save progress.");
                return;
            }

            var metaData = saveManager.LoadMeta() ?? new MetaSaveData();
            metaData.PlayerLevel = _playerLevel;
            metaData.CurrentXP = _currentXP;
            metaData.TotalXP = _totalXP;

            saveManager.SaveMeta(metaData);
            Debug.Log($"[PlayerProgressionManager] Progress saved. Level: {_playerLevel}, XP: {_currentXP}/{XPForNextLevel}");
        }
    }
}
