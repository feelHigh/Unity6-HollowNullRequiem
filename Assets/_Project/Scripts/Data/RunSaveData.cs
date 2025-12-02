// ============================================
// RunSaveData.cs
// Save data structure for active runs
// ============================================

using System;
using System.Collections.Generic;

namespace HNR.Data
{
    /// <summary>
    /// Contains all data needed to save and restore an active run.
    /// Serialized by Easy Save 3 for persistence.
    /// </summary>
    [Serializable]
    public class RunSaveData
    {
        /// <summary>
        /// Current floor/level in the Null Rift.
        /// </summary>
        public int CurrentFloor;

        /// <summary>
        /// Current zone index (0 = Outer Reaches, 1 = Hollow's Maw, etc.)
        /// </summary>
        public int CurrentZone;

        /// <summary>
        /// Current Void Shards (currency).
        /// </summary>
        public int VoidShards;

        /// <summary>
        /// IDs of acquired relics.
        /// </summary>
        public List<string> RelicIds = new();

        /// <summary>
        /// Current deck card IDs.
        /// </summary>
        public List<string> DeckCardIds = new();

        /// <summary>
        /// Team Requiem data.
        /// </summary>
        public List<RequiemSaveData> Team = new();

        /// <summary>
        /// Map node states for current floor.
        /// </summary>
        public MapSaveData MapState;

        /// <summary>
        /// Total enemies defeated this run.
        /// </summary>
        public int EnemiesDefeated;

        /// <summary>
        /// Total damage dealt this run.
        /// </summary>
        public int TotalDamageDealt;

        /// <summary>
        /// Run start timestamp.
        /// </summary>
        public long StartTimestamp;
    }

    /// <summary>
    /// Save data for a single Requiem character.
    /// </summary>
    [Serializable]
    public class RequiemSaveData
    {
        /// <summary>
        /// Requiem data asset ID.
        /// </summary>
        public string RequiemId;

        /// <summary>
        /// Current HP.
        /// </summary>
        public int CurrentHP;

        /// <summary>
        /// Maximum HP (may be modified by relics).
        /// </summary>
        public int MaxHP;

        /// <summary>
        /// Current corruption level (0-100).
        /// </summary>
        public int Corruption;
    }

    /// <summary>
    /// Save data for the map state.
    /// </summary>
    [Serializable]
    public class MapSaveData
    {
        /// <summary>
        /// IDs of visited node positions.
        /// </summary>
        public List<int> VisitedNodes = new();

        /// <summary>
        /// Current node position.
        /// </summary>
        public int CurrentNodeIndex;

        /// <summary>
        /// Seed used for map generation (for deterministic regeneration).
        /// </summary>
        public int MapSeed;
    }
}
