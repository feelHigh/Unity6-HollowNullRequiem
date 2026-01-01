// ============================================
// SaveData.cs
// Save/Load data structures for Easy Save 3
// ============================================

using System;
using System.Collections.Generic;

namespace HNR.Progression
{
    // ============================================
    // RUN SAVE DATA
    // ============================================

    /// <summary>
    /// Complete save state for a run in progress.
    /// Serialized via Easy Save 3 for persistence.
    /// </summary>
    [Serializable]
    public class RunSaveData
    {
        /// <summary>Random seed used for run generation (deterministic replay).</summary>
        public int RunSeed;

        /// <summary>Team state including HP and corruption.</summary>
        public TeamSaveData Team = new();

        /// <summary>Deck state including cards and upgrades.</summary>
        public DeckSaveData Deck = new();

        /// <summary>Progression state including zone, shards, and relics.</summary>
        public ProgressionSaveData Progression = new();

        /// <summary>Map state including nodes and current position.</summary>
        public MapSaveData Map = new();

        /// <summary>Run statistics for results screen.</summary>
        public StatsSaveData Stats = new();

        /// <summary>Timestamp when this save was created.</summary>
        public long SaveTimestamp;

        /// <summary>Game version when save was created.</summary>
        public string GameVersion = "1.0.0";
    }

    // ============================================
    // TEAM SAVE DATA
    // ============================================

    /// <summary>
    /// Saved state for the player's team of Requiems.
    /// </summary>
    [Serializable]
    public class TeamSaveData
    {
        /// <summary>IDs of selected Requiems (order preserved).</summary>
        public List<string> RequiemIds = new();

        /// <summary>Current HP for each Requiem (parallel to RequiemIds).</summary>
        public List<int> CurrentHP = new();

        /// <summary>Max HP for each Requiem (parallel to RequiemIds).</summary>
        public List<int> MaxHP = new();

        /// <summary>Corruption value for each Requiem (parallel to RequiemIds).</summary>
        public List<int> Corruption = new();

        /// <summary>Soul Essence for each Requiem (parallel to RequiemIds).</summary>
        public List<int> SoulEssence = new();

        /// <summary>Team-wide shared HP pool.</summary>
        public int TeamCurrentHP;

        /// <summary>Team-wide max HP.</summary>
        public int TeamMaxHP;
    }

    // ============================================
    // DECK SAVE DATA
    // ============================================

    /// <summary>
    /// Saved state for the player's deck.
    /// </summary>
    [Serializable]
    public class DeckSaveData
    {
        /// <summary>IDs of all cards in the deck.</summary>
        public List<string> CardIds = new();

        /// <summary>IDs of cards that have been upgraded.</summary>
        public List<string> UpgradedCardIds = new();

        /// <summary>Number of times each card has been played (for stats).</summary>
        public Dictionary<string, int> CardPlayCounts = new();
    }

    // ============================================
    // PROGRESSION SAVE DATA
    // ============================================

    /// <summary>
    /// Saved state for run progression (zone, currency, relics).
    /// </summary>
    [Serializable]
    public class ProgressionSaveData
    {
        /// <summary>Current zone number (1-3).</summary>
        public int CurrentZone = 1;

        /// <summary>Current Void Shards balance.</summary>
        public int VoidShards;

        /// <summary>IDs of all owned relics.</summary>
        public List<string> RelicIds = new();

        /// <summary>IDs of Echo events already seen this run.</summary>
        public List<string> SeenEventIds = new();

        /// <summary>Number of combats completed.</summary>
        public int CombatsCompleted;

        /// <summary>Number of shops visited.</summary>
        public int ShopsVisited;

        /// <summary>Number of events completed.</summary>
        public int EventsCompleted;
    }

    // ============================================
    // MAP SAVE DATA
    // ============================================

    /// <summary>
    /// Saved state for the Null Rift map.
    /// </summary>
    [Serializable]
    public class MapSaveData
    {
        /// <summary>Current zone number.</summary>
        public int Zone = 1;

        /// <summary>Seed used to generate this map.</summary>
        public int Seed;

        /// <summary>ID of the current node.</summary>
        public string CurrentNodeId;

        /// <summary>List of visited nodes with completion state.</summary>
        public List<VisitedNode> VisitedNodes = new();

        /// <summary>IDs of nodes that are currently accessible.</summary>
        public List<string> AccessibleNodeIds = new();
    }

    /// <summary>
    /// Record of a visited map node.
    /// </summary>
    [Serializable]
    public class VisitedNode
    {
        /// <summary>Node identifier.</summary>
        public string NodeId;

        /// <summary>Whether the node encounter was completed.</summary>
        public bool Completed;

        /// <summary>Type of node (combat, shop, event, etc.).</summary>
        public string NodeType;
    }

    // ============================================
    // STATS SAVE DATA
    // ============================================

    /// <summary>
    /// Run statistics for results screen and achievements.
    /// </summary>
    [Serializable]
    public class StatsSaveData
    {
        /// <summary>Total enemies defeated this run.</summary>
        public int EnemiesDefeated;

        /// <summary>Total cards played this run.</summary>
        public int CardsPlayed;

        /// <summary>Total time played in seconds.</summary>
        public float PlayTime;

        /// <summary>Total damage dealt to enemies.</summary>
        public int DamageDealt;

        /// <summary>Total damage taken by team.</summary>
        public int DamageTaken;

        /// <summary>Total healing received.</summary>
        public int HealingReceived;

        /// <summary>Total Void Shards earned this run.</summary>
        public int VoidShardsEarned;

        /// <summary>Total Void Shards spent this run.</summary>
        public int VoidShardsSpent;

        /// <summary>Highest single hit damage.</summary>
        public int MaxSingleHitDamage;

        /// <summary>Number of times a Requiem entered Null State.</summary>
        public int NullStateEntered;

        /// <summary>Floors cleared in this run.</summary>
        public int FloorsCleared;
    }

    // ============================================
    // SETTINGS DATA
    // ============================================

    /// <summary>
    /// Player settings persisted between sessions.
    /// Not tied to a specific run.
    /// </summary>
    [Serializable]
    public class SettingsData
    {
        /// <summary>Music volume (0-1).</summary>
        public float MusicVolume = 0.8f;

        /// <summary>Sound effects volume (0-1).</summary>
        public float SFXVolume = 1.0f;

        /// <summary>Whether haptic feedback is enabled.</summary>
        public bool HapticsEnabled = true;

        /// <summary>Whether screen shake effects are enabled.</summary>
        public bool ScreenShakeEnabled = true;

        /// <summary>Target frame rate (30, 60, or -1 for unlimited).</summary>
        public int TargetFrameRate = 60;

        /// <summary>Whether to show damage numbers.</summary>
        public bool ShowDamageNumbers = true;

        /// <summary>Whether to show card tooltips on hover.</summary>
        public bool ShowCardTooltips = true;

        /// <summary>Animation speed multiplier (0.5 to 2.0).</summary>
        public float AnimationSpeed = 1.0f;

        /// <summary>Whether tutorials have been completed.</summary>
        public bool TutorialCompleted;

        /// <summary>Language code (e.g., "en", "ko", "ja").</summary>
        public string Language = "en";
    }

    // ============================================
    // META SAVE DATA (Unlocks/Achievements)
    // ============================================

    /// <summary>
    /// Persistent meta-progression data across all runs.
    /// </summary>
    [Serializable]
    public class MetaSaveData
    {
        /// <summary>Current player level.</summary>
        public int PlayerLevel = 1;

        /// <summary>Current XP within level (progress toward next level).</summary>
        public int CurrentXP;

        /// <summary>Total XP earned across all runs.</summary>
        public int TotalXP;

        /// <summary>Total runs started.</summary>
        public int TotalRunsStarted;

        /// <summary>Total runs completed (victories).</summary>
        public int TotalRunsCompleted;

        /// <summary>IDs of unlocked Requiems.</summary>
        public List<string> UnlockedRequiemIds = new();

        /// <summary>IDs of unlocked cards.</summary>
        public List<string> UnlockedCardIds = new();

        /// <summary>IDs of unlocked relics.</summary>
        public List<string> UnlockedRelicIds = new();

        /// <summary>IDs of completed achievements.</summary>
        public List<string> CompletedAchievementIds = new();

        /// <summary>Highest zone reached across all runs.</summary>
        public int HighestZoneReached;

        /// <summary>Best run time in seconds.</summary>
        public float BestRunTime;

        /// <summary>Total play time in seconds.</summary>
        public float TotalPlayTime;

        /// <summary>Total enemies defeated across all runs.</summary>
        public int TotalEnemiesDefeated;
    }

    // ============================================
    // DIFFICULTY LEVEL ENUM
    // ============================================

    /// <summary>
    /// Difficulty levels for Battle Mission mode.
    /// </summary>
    [Serializable]
    public enum DifficultyLevel
    {
        Easy = 0,
        Normal = 1,
        Hard = 2
    }

    // ============================================
    // BATTLE MISSION SAVE DATA
    // ============================================

    /// <summary>
    /// Persistent save data for Battle Mission progression.
    /// Tracks zone clears and difficulty unlocks across sessions.
    /// </summary>
    [Serializable]
    public class BattleMissionSaveData
    {
        /// <summary>
        /// Zone clear status per difficulty level.
        /// Key format: "Zone{1-3}_{Easy|Normal|Hard}" (e.g., "Zone1_Easy", "Zone2_Normal")
        /// </summary>
        public Dictionary<string, bool> ZoneClearStatus = new();

        /// <summary>Whether Normal difficulty is unlocked (all zones cleared on Easy).</summary>
        public bool NormalUnlocked;

        /// <summary>Whether Hard difficulty is unlocked (all zones cleared on Normal).</summary>
        public bool HardUnlocked;

        /// <summary>Currently selected difficulty level.</summary>
        public DifficultyLevel CurrentDifficulty = DifficultyLevel.Easy;

        /// <summary>
        /// Gets the save key for a zone and difficulty combination.
        /// </summary>
        public static string GetZoneKey(int zone, DifficultyLevel difficulty)
        {
            return $"Zone{zone}_{difficulty}";
        }

        /// <summary>
        /// Checks if a specific zone is cleared on a given difficulty.
        /// </summary>
        public bool IsZoneCleared(int zone, DifficultyLevel difficulty)
        {
            string key = GetZoneKey(zone, difficulty);
            return ZoneClearStatus.TryGetValue(key, out bool cleared) && cleared;
        }

        /// <summary>
        /// Marks a zone as cleared on a given difficulty.
        /// </summary>
        public void SetZoneCleared(int zone, DifficultyLevel difficulty, bool cleared = true)
        {
            string key = GetZoneKey(zone, difficulty);
            ZoneClearStatus[key] = cleared;
        }
    }
}
