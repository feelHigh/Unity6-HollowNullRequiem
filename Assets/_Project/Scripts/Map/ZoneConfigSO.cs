// ============================================
// ZoneConfigSO.cs
// ScriptableObject defining zone configuration
// ============================================

using System;
using System.Collections.Generic;
using UnityEngine;
using HNR.Combat;

namespace HNR.Map
{
    /// <summary>
    /// Configuration for a single zone in the Null Rift.
    /// Create one asset per zone with encounters and node distribution.
    /// </summary>
    [CreateAssetMenu(fileName = "New Zone Config", menuName = "HNR/Zone Config")]
    public class ZoneConfigSO : ScriptableObject
    {
        // ============================================
        // Zone Info
        // ============================================

        [Header("Zone Info")]
        [SerializeField, Tooltip("Zone number (1-3)")]
        private int _zoneNumber = 1;

        [SerializeField, Tooltip("Display name for the zone")]
        private string _zoneName = "Null Rift - Zone 1";

        [SerializeField, TextArea(2, 4), Tooltip("Zone description")]
        private string _zoneDescription;

        // ============================================
        // Layout Configuration
        // ============================================

        [Header("Layout")]
        [SerializeField, Range(3, 8), Tooltip("Number of rows (including start and boss)")]
        private int _rowCount = 5;

        [SerializeField, Range(1, 4), Tooltip("Minimum nodes per row")]
        private int _minNodesPerRow = 2;

        [SerializeField, Range(2, 5), Tooltip("Maximum nodes per row")]
        private int _maxNodesPerRow = 4;

        // ============================================
        // Node Distribution Weights
        // ============================================

        [Header("Node Distribution (weights out of 100)")]
        [SerializeField, Range(0, 100), Tooltip("Combat encounter weight")]
        private int _combatWeight = 50;

        [SerializeField, Range(0, 100), Tooltip("Elite encounter weight")]
        private int _eliteWeight = 10;

        [SerializeField, Range(0, 100), Tooltip("Shop node weight")]
        private int _shopWeight = 10;

        [SerializeField, Range(0, 100), Tooltip("Echo event weight")]
        private int _echoWeight = 15;

        [SerializeField, Range(0, 100), Tooltip("Sanctuary weight")]
        private int _sanctuaryWeight = 10;

        [SerializeField, Range(0, 100), Tooltip("Treasure weight")]
        private int _treasureWeight = 5;

        // ============================================
        // Special Rules
        // ============================================

        [Header("Special Rules")]
        [SerializeField, Range(1, 7), Tooltip("Minimum row for Elite nodes")]
        private int _eliteMinRow = 3;

        [SerializeField, Tooltip("Guarantee at least one shop per zone")]
        private bool _guaranteedShop = true;

        [SerializeField, Tooltip("Guarantee at least one sanctuary per zone")]
        private bool _guaranteedSanctuary = true;

        // ============================================
        // Encounters
        // ============================================

        [Header("Combat Encounters")]
        [SerializeField, Tooltip("Pool of standard combat encounters")]
        private List<EncounterDataSO> _combatEncounters = new();

        [Header("Elite Encounters")]
        [SerializeField, Tooltip("Pool of elite encounters")]
        private List<EncounterDataSO> _eliteEncounters = new();

        [Header("Boss Encounter")]
        [SerializeField, Tooltip("Boss encounter for this zone")]
        private EncounterDataSO _bossEncounter;

        // ============================================
        // Echo Events
        // ============================================

        [Header("Echo Events")]
        [SerializeField, Tooltip("Pool of echo events for this zone")]
        private List<EchoEventDataSO> _echoEvents = new();

        // ============================================
        // Visual Configuration
        // ============================================

        [Header("Visual Settings")]
        [SerializeField, Tooltip("Horizontal spacing between nodes")]
        private float _horizontalSpacing = 200f;

        [SerializeField, Tooltip("Vertical spacing between rows")]
        private float _verticalSpacing = 150f;

        [SerializeField, Tooltip("Random position jitter for visual variety")]
        private float _nodeJitter = 20f;

        // ============================================
        // Public Accessors
        // ============================================

        /// <summary>Zone number (1-3).</summary>
        public int ZoneNumber => _zoneNumber;

        /// <summary>Zone display name.</summary>
        public string ZoneName => _zoneName;

        /// <summary>Zone description text.</summary>
        public string ZoneDescription => _zoneDescription;

        /// <summary>Number of rows in the zone.</summary>
        public int RowCount => _rowCount;

        /// <summary>Minimum nodes per row.</summary>
        public int MinNodesPerRow => _minNodesPerRow;

        /// <summary>Maximum nodes per row.</summary>
        public int MaxNodesPerRow => _maxNodesPerRow;

        /// <summary>Combat node weight.</summary>
        public int CombatWeight => _combatWeight;

        /// <summary>Elite node weight.</summary>
        public int EliteWeight => _eliteWeight;

        /// <summary>Shop node weight.</summary>
        public int ShopWeight => _shopWeight;

        /// <summary>Echo event weight.</summary>
        public int EchoWeight => _echoWeight;

        /// <summary>Sanctuary weight.</summary>
        public int SanctuaryWeight => _sanctuaryWeight;

        /// <summary>Treasure weight.</summary>
        public int TreasureWeight => _treasureWeight;

        /// <summary>Minimum row for elite encounters.</summary>
        public int EliteMinRow => _eliteMinRow;

        /// <summary>Whether to guarantee at least one shop.</summary>
        public bool GuaranteedShop => _guaranteedShop;

        /// <summary>Whether to guarantee at least one sanctuary.</summary>
        public bool GuaranteedSanctuary => _guaranteedSanctuary;

        /// <summary>Combat encounter pool.</summary>
        public IReadOnlyList<EncounterDataSO> CombatEncounters => _combatEncounters;

        /// <summary>Elite encounter pool.</summary>
        public IReadOnlyList<EncounterDataSO> EliteEncounters => _eliteEncounters;

        /// <summary>Boss encounter.</summary>
        public EncounterDataSO BossEncounter => _bossEncounter;

        /// <summary>Echo event pool.</summary>
        public IReadOnlyList<EchoEventDataSO> EchoEvents => _echoEvents;

        /// <summary>Horizontal spacing for layout.</summary>
        public float HorizontalSpacing => _horizontalSpacing;

        /// <summary>Vertical spacing for layout.</summary>
        public float VerticalSpacing => _verticalSpacing;

        /// <summary>Position jitter amount.</summary>
        public float NodeJitter => _nodeJitter;

        // ============================================
        // Helper Methods
        // ============================================

        /// <summary>
        /// Gets a random combat encounter from the pool.
        /// </summary>
        public EncounterDataSO GetRandomCombatEncounter(System.Random rng)
        {
            if (_combatEncounters.Count == 0) return null;
            return _combatEncounters[rng.Next(_combatEncounters.Count)];
        }

        /// <summary>
        /// Gets a random elite encounter from the pool.
        /// </summary>
        public EncounterDataSO GetRandomEliteEncounter(System.Random rng)
        {
            if (_eliteEncounters.Count == 0) return null;
            return _eliteEncounters[rng.Next(_eliteEncounters.Count)];
        }

        /// <summary>
        /// Gets a random echo event from the pool.
        /// </summary>
        public EchoEventDataSO GetRandomEchoEvent(System.Random rng)
        {
            if (_echoEvents.Count == 0) return null;
            return _echoEvents[rng.Next(_echoEvents.Count)];
        }

        /// <summary>
        /// Gets the total weight for node distribution.
        /// </summary>
        public int GetTotalWeight()
        {
            return _combatWeight + _eliteWeight + _shopWeight +
                   _echoWeight + _sanctuaryWeight + _treasureWeight;
        }

        /// <summary>
        /// Gets all node type weights as an array.
        /// </summary>
        public NodeTypeWeight[] GetNodeWeights()
        {
            return new[]
            {
                new NodeTypeWeight { Type = NodeType.Combat, Weight = _combatWeight },
                new NodeTypeWeight { Type = NodeType.Elite, Weight = _eliteWeight },
                new NodeTypeWeight { Type = NodeType.Shop, Weight = _shopWeight },
                new NodeTypeWeight { Type = NodeType.Echo, Weight = _echoWeight },
                new NodeTypeWeight { Type = NodeType.Sanctuary, Weight = _sanctuaryWeight },
                new NodeTypeWeight { Type = NodeType.Treasure, Weight = _treasureWeight }
            };
        }

        // ============================================
        // Editor Validation
        // ============================================

#if UNITY_EDITOR
        private void OnValidate()
        {
            _zoneNumber = Mathf.Clamp(_zoneNumber, 1, 3);
            _minNodesPerRow = Mathf.Min(_minNodesPerRow, _maxNodesPerRow);

            // Ensure string name matches zone number
            if (string.IsNullOrEmpty(_zoneName))
            {
                _zoneName = $"Null Rift - Zone {_zoneNumber}";
            }
        }
#endif
    }

    /// <summary>
    /// Weight configuration for a node type.
    /// </summary>
    [Serializable]
    public class NodeTypeWeight
    {
        public NodeType Type;
        [Range(0, 100)] public int Weight;
    }
}
