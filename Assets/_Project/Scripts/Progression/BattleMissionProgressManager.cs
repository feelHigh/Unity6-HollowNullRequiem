// ============================================
// BattleMissionProgressManager.cs
// Manages Battle Mission zone and difficulty progression
// ============================================

using UnityEngine;
using HNR.Core;
using HNR.Core.Interfaces;

namespace HNR.Progression
{
    /// <summary>
    /// Manages Battle Mission progression including zone unlocks and difficulty levels.
    /// Persists progress via SaveManager using Easy Save 3.
    /// </summary>
    public class BattleMissionProgressManager : MonoBehaviour
    {
        // ============================================
        // Constants
        // ============================================

        private const int TOTAL_ZONES = 3;

        // ============================================
        // Singleton
        // ============================================

        private static BattleMissionProgressManager _instance;
        public static BattleMissionProgressManager Instance => _instance;

        // ============================================
        // State
        // ============================================

        private BattleMissionSaveData _progressData;
        private ISaveManager _saveManager;

        /// <summary>Current selected difficulty level.</summary>
        public DifficultyLevel CurrentDifficulty
        {
            get => _progressData?.CurrentDifficulty ?? DifficultyLevel.Easy;
            set
            {
                if (_progressData != null && IsDifficultyUnlocked(value))
                {
                    _progressData.CurrentDifficulty = value;
                    SaveProgress();
                }
            }
        }

        // ============================================
        // Unity Lifecycle
        // ============================================

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject);

            LoadProgress();
            Debug.Log("[BattleMissionProgressManager] Initialized");
        }

        private void Start()
        {
            _saveManager = ServiceLocator.TryGet<ISaveManager>(out var sm) ? sm : null;
        }

        private void OnDestroy()
        {
            if (_instance == this)
            {
                _instance = null;
            }
        }

        // ============================================
        // Zone Unlock Logic
        // ============================================

        /// <summary>
        /// Checks if a zone is unlocked for the given difficulty.
        /// Zone 1 is always unlocked on Easy.
        /// Zone N unlocks when Zone N-1 is cleared on the same difficulty.
        /// </summary>
        public bool IsZoneUnlocked(int zone, DifficultyLevel difficulty)
        {
            // Validate zone
            if (zone < 1 || zone > TOTAL_ZONES)
            {
                Debug.LogWarning($"[BattleMissionProgressManager] Invalid zone: {zone}");
                return false;
            }

            // Check difficulty is unlocked first
            if (!IsDifficultyUnlocked(difficulty))
            {
                return false;
            }

            // Zone 1 is always unlocked for unlocked difficulties
            if (zone == 1)
            {
                return true;
            }

            // Zone N unlocks when Zone N-1 is cleared on the same difficulty
            return IsZoneCleared(zone - 1, difficulty);
        }

        /// <summary>
        /// Checks if a zone has been cleared on a given difficulty.
        /// </summary>
        public bool IsZoneCleared(int zone, DifficultyLevel difficulty)
        {
            if (_progressData == null) return false;
            return _progressData.IsZoneCleared(zone, difficulty);
        }

        /// <summary>
        /// Marks a zone as cleared on a given difficulty.
        /// Also checks for difficulty unlocks.
        /// </summary>
        public void MarkZoneCleared(int zone, DifficultyLevel difficulty)
        {
            if (_progressData == null)
            {
                Debug.LogWarning("[BattleMissionProgressManager] Progress data not loaded");
                return;
            }

            if (zone < 1 || zone > TOTAL_ZONES)
            {
                Debug.LogWarning($"[BattleMissionProgressManager] Invalid zone: {zone}");
                return;
            }

            _progressData.SetZoneCleared(zone, difficulty, true);
            Debug.Log($"[BattleMissionProgressManager] Zone {zone} cleared on {difficulty}");

            // Check for difficulty unlocks
            CheckDifficultyUnlocks();

            // Save immediately
            SaveProgress();
        }

        // ============================================
        // Difficulty Unlock Logic
        // ============================================

        /// <summary>
        /// Checks if a difficulty level is unlocked.
        /// Easy is always unlocked.
        /// Normal unlocks when all zones are cleared on Easy.
        /// Hard unlocks when all zones are cleared on Normal.
        /// </summary>
        public bool IsDifficultyUnlocked(DifficultyLevel difficulty)
        {
            if (_progressData == null) return difficulty == DifficultyLevel.Easy;

            return difficulty switch
            {
                DifficultyLevel.Easy => true,
                DifficultyLevel.Normal => _progressData.NormalUnlocked,
                DifficultyLevel.Hard => _progressData.HardUnlocked,
                _ => false
            };
        }

        /// <summary>
        /// Checks and updates difficulty unlock status based on zone clears.
        /// </summary>
        public void CheckDifficultyUnlocks()
        {
            if (_progressData == null) return;

            // Check Normal unlock (all zones cleared on Easy)
            if (!_progressData.NormalUnlocked)
            {
                bool allEasyCleared = true;
                for (int zone = 1; zone <= TOTAL_ZONES; zone++)
                {
                    if (!IsZoneCleared(zone, DifficultyLevel.Easy))
                    {
                        allEasyCleared = false;
                        break;
                    }
                }

                if (allEasyCleared)
                {
                    _progressData.NormalUnlocked = true;
                    Debug.Log("[BattleMissionProgressManager] Normal difficulty unlocked!");
                }
            }

            // Check Hard unlock (all zones cleared on Normal)
            if (!_progressData.HardUnlocked && _progressData.NormalUnlocked)
            {
                bool allNormalCleared = true;
                for (int zone = 1; zone <= TOTAL_ZONES; zone++)
                {
                    if (!IsZoneCleared(zone, DifficultyLevel.Normal))
                    {
                        allNormalCleared = false;
                        break;
                    }
                }

                if (allNormalCleared)
                {
                    _progressData.HardUnlocked = true;
                    Debug.Log("[BattleMissionProgressManager] Hard difficulty unlocked!");
                }
            }
        }

        // ============================================
        // Progress Queries
        // ============================================

        /// <summary>
        /// Gets the number of zones cleared on a difficulty.
        /// </summary>
        public int GetZonesClearedCount(DifficultyLevel difficulty)
        {
            if (_progressData == null) return 0;

            int count = 0;
            for (int zone = 1; zone <= TOTAL_ZONES; zone++)
            {
                if (IsZoneCleared(zone, difficulty))
                {
                    count++;
                }
            }
            return count;
        }

        /// <summary>
        /// Gets whether all zones are cleared on a difficulty.
        /// </summary>
        public bool AreAllZonesCleared(DifficultyLevel difficulty)
        {
            return GetZonesClearedCount(difficulty) >= TOTAL_ZONES;
        }

        /// <summary>
        /// Gets the highest unlocked difficulty.
        /// </summary>
        public DifficultyLevel GetHighestUnlockedDifficulty()
        {
            if (_progressData == null) return DifficultyLevel.Easy;

            if (_progressData.HardUnlocked) return DifficultyLevel.Hard;
            if (_progressData.NormalUnlocked) return DifficultyLevel.Normal;
            return DifficultyLevel.Easy;
        }

        // ============================================
        // Save/Load
        // ============================================

        /// <summary>
        /// Loads progress from SaveManager.
        /// </summary>
        public void LoadProgress()
        {
            if (_saveManager == null)
            {
                ServiceLocator.TryGet<ISaveManager>(out var sm);
                _saveManager = sm;
            }

            if (_saveManager != null)
            {
                _progressData = (_saveManager as SaveManager)?.LoadBattleMissionProgress();
            }

            if (_progressData == null)
            {
                _progressData = new BattleMissionSaveData();
            }

            Debug.Log($"[BattleMissionProgressManager] Progress loaded. " +
                      $"Normal unlocked: {_progressData.NormalUnlocked}, " +
                      $"Hard unlocked: {_progressData.HardUnlocked}");
        }

        /// <summary>
        /// Saves progress to SaveManager.
        /// </summary>
        public void SaveProgress()
        {
            if (_progressData == null) return;

            if (_saveManager == null)
            {
                ServiceLocator.TryGet<ISaveManager>(out var sm);
                _saveManager = sm;
            }

            if (_saveManager != null)
            {
                (_saveManager as SaveManager)?.SaveBattleMissionProgress(_progressData);
            }
        }

        /// <summary>
        /// Resets all progress (for testing or new game plus).
        /// </summary>
        public void ResetAllProgress()
        {
            _progressData = new BattleMissionSaveData();
            SaveProgress();
            Debug.Log("[BattleMissionProgressManager] All progress reset");
        }

        // ============================================
        // Debug
        // ============================================

        /// <summary>
        /// Debug method to unlock all zones on a difficulty.
        /// </summary>
        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        public void DebugUnlockAllZones(DifficultyLevel difficulty)
        {
            for (int zone = 1; zone <= TOTAL_ZONES; zone++)
            {
                _progressData.SetZoneCleared(zone, difficulty, true);
            }
            CheckDifficultyUnlocks();
            SaveProgress();
            Debug.Log($"[BattleMissionProgressManager] DEBUG: All zones unlocked on {difficulty}");
        }
    }
}
