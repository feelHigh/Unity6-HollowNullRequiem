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

        [Header("Layout (Horizontal Map: Start on left → Boss on right)")]
        [SerializeField, Range(3, 8), Tooltip("Number of columns/steps (including start and boss)")]
        private int _columnCount = 5;

        [SerializeField, Range(1, 4), Tooltip("Minimum nodes per column (vertical spread)")]
        private int _minNodesPerColumn = 2;

        [SerializeField, Range(2, 5), Tooltip("Maximum nodes per column (vertical spread)")]
        private int _maxNodesPerColumn = 4;

        // ============================================
        // Map Shape Pattern
        // ============================================

        [Header("Map Shape Pattern")]
        [SerializeField, Tooltip("Pattern for node distribution across columns (Diamond creates organic branching)")]
        private MapShapePattern _mapShapePattern = MapShapePattern.Diamond;

        [SerializeField, Tooltip("Custom node counts per column (only used with Custom pattern). Length must match ColumnCount.")]
        private int[] _customNodeDistribution;

        [SerializeField, Range(3, 7), Tooltip("Maximum vertical slots available for node positioning")]
        private int _maxVerticalSlots = 5;

        // ============================================
        // Connection Rules
        // ============================================

        [Header("Connection Rules")]
        [SerializeField, Range(1, 3), Tooltip("Maximum slot distance for connections (1 = adjacent only, creates organic paths)")]
        private int _maxRowDistance = 1;

        [SerializeField, Tooltip("Prefer keeping paths separate (reduces merge frequency for more distinct routes)")]
        private bool _preferSeparatePaths = false;

        [SerializeField, Tooltip("Force merge points at specific columns (0-indexed, excluding start/end)")]
        private int[] _forcedMergeColumns;

        [SerializeField, Tooltip("Force diverge points at specific columns")]
        private int[] _forcedDivergeColumns;

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
        [SerializeField, Range(1, 7), Tooltip("Minimum column (step) for Elite nodes")]
        private int _eliteMinColumn = 3;

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

        /// <summary>Number of columns/steps in the zone (horizontal progression).</summary>
        public int ColumnCount => _columnCount;

        /// <summary>Minimum nodes per column (vertical spread).</summary>
        public int MinNodesPerColumn => _minNodesPerColumn;

        /// <summary>Maximum nodes per column (vertical spread).</summary>
        public int MaxNodesPerColumn => _maxNodesPerColumn;

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

        /// <summary>Minimum column (step) for elite encounters.</summary>
        public int EliteMinColumn => _eliteMinColumn;

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

        /// <summary>Map shape pattern for node distribution.</summary>
        public MapShapePattern MapShapePattern => _mapShapePattern;

        /// <summary>Custom node distribution array (for Custom pattern).</summary>
        public int[] CustomNodeDistribution => _customNodeDistribution;

        /// <summary>Maximum vertical slots for node positioning.</summary>
        public int MaxVerticalSlots => _maxVerticalSlots;

        /// <summary>Maximum slot distance for connections (1 = adjacent only).</summary>
        public int MaxRowDistance => _maxRowDistance;

        /// <summary>Whether to prefer keeping paths separate.</summary>
        public bool PreferSeparatePaths => _preferSeparatePaths;

        /// <summary>Columns that force path convergence.</summary>
        public int[] ForcedMergeColumns => _forcedMergeColumns ?? System.Array.Empty<int>();

        /// <summary>Columns that force path divergence.</summary>
        public int[] ForcedDivergeColumns => _forcedDivergeColumns ?? System.Array.Empty<int>();

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
        /// Gets the node distribution array based on the selected pattern.
        /// Returns an array where each index is a column and value is node count.
        /// </summary>
        public int[] GetNodeDistribution()
        {
            return _mapShapePattern switch
            {
                MapShapePattern.Diamond => GenerateDiamondDistribution(),
                MapShapePattern.Hourglass => GenerateHourglassDistribution(),
                MapShapePattern.WideDiamond => GenerateWideDiamondDistribution(),
                MapShapePattern.Custom => GetCustomDistribution(),
                _ => GenerateDiamondDistribution()
            };
        }

        /// <summary>
        /// Checks if a column is a forced merge point.
        /// </summary>
        public bool IsMergeColumn(int column)
        {
            if (_forcedMergeColumns == null) return false;
            return System.Array.IndexOf(_forcedMergeColumns, column) >= 0;
        }

        /// <summary>
        /// Checks if a column is a forced diverge point.
        /// </summary>
        public bool IsDivergeColumn(int column)
        {
            if (_forcedDivergeColumns == null) return false;
            return System.Array.IndexOf(_forcedDivergeColumns, column) >= 0;
        }

        // ============================================
        // Pattern Generators
        // ============================================

        /// <summary>
        /// Diamond pattern: 1 → expand → peak → contract → 1
        /// Creates classic roguelike branching with smooth expansion/contraction.
        /// </summary>
        private int[] GenerateDiamondDistribution()
        {
            var dist = new int[_columnCount];
            dist[0] = 1; // Start node
            dist[_columnCount - 1] = 1; // End node

            if (_columnCount <= 2) return dist;

            int midPoint = _columnCount / 2;
            int maxNodes = Mathf.Min(_maxNodesPerColumn, _maxVerticalSlots);

            for (int i = 1; i < _columnCount - 1; i++)
            {
                if (i <= midPoint)
                {
                    // Expansion phase: linearly increase toward midpoint
                    float progress = (float)i / midPoint;
                    dist[i] = Mathf.RoundToInt(Mathf.Lerp(2, maxNodes, progress));
                }
                else
                {
                    // Contraction phase: linearly decrease toward end
                    float progress = (float)(i - midPoint) / (_columnCount - 1 - midPoint);
                    dist[i] = Mathf.RoundToInt(Mathf.Lerp(maxNodes, 2, progress));
                }
                dist[i] = Mathf.Clamp(dist[i], _minNodesPerColumn, maxNodes);
            }

            return dist;
        }

        /// <summary>
        /// Hourglass pattern: 1 → rapid expand → contract → 1
        /// More aggressive branching early, faster convergence.
        /// </summary>
        private int[] GenerateHourglassDistribution()
        {
            var dist = new int[_columnCount];
            dist[0] = 1;
            dist[_columnCount - 1] = 1;

            if (_columnCount <= 2) return dist;

            int maxNodes = Mathf.Min(_maxNodesPerColumn, _maxVerticalSlots);

            // Rapid expansion in first third, then gradual contraction
            int expansionEnd = Mathf.Max(1, _columnCount / 3);

            for (int i = 1; i < _columnCount - 1; i++)
            {
                if (i <= expansionEnd)
                {
                    // Rapid expansion to max
                    float progress = (float)i / expansionEnd;
                    dist[i] = Mathf.RoundToInt(Mathf.Lerp(2, maxNodes, progress));
                }
                else
                {
                    // Gradual contraction
                    float progress = (float)(i - expansionEnd) / (_columnCount - 1 - expansionEnd);
                    dist[i] = Mathf.RoundToInt(Mathf.Lerp(maxNodes, 2, progress));
                }
                dist[i] = Mathf.Clamp(dist[i], _minNodesPerColumn, maxNodes);
            }

            return dist;
        }

        /// <summary>
        /// Wide diamond pattern: 1 → expand → wide plateau → contract → 1
        /// Maintains max width for longer, more branching opportunities.
        /// </summary>
        private int[] GenerateWideDiamondDistribution()
        {
            var dist = new int[_columnCount];
            dist[0] = 1;
            dist[_columnCount - 1] = 1;

            if (_columnCount <= 2) return dist;

            int maxNodes = Mathf.Min(_maxNodesPerColumn, _maxVerticalSlots);

            // Expand in first 25%, plateau for 50%, contract in last 25%
            int expansionEnd = Mathf.Max(1, _columnCount / 4);
            int contractionStart = _columnCount - 1 - expansionEnd;

            for (int i = 1; i < _columnCount - 1; i++)
            {
                if (i <= expansionEnd)
                {
                    // Expansion phase
                    float progress = (float)i / expansionEnd;
                    dist[i] = Mathf.RoundToInt(Mathf.Lerp(2, maxNodes, progress));
                }
                else if (i >= contractionStart)
                {
                    // Contraction phase
                    float progress = (float)(i - contractionStart) / (_columnCount - 1 - contractionStart);
                    dist[i] = Mathf.RoundToInt(Mathf.Lerp(maxNodes, 2, progress));
                }
                else
                {
                    // Plateau phase - maintain max
                    dist[i] = maxNodes;
                }
                dist[i] = Mathf.Clamp(dist[i], _minNodesPerColumn, maxNodes);
            }

            return dist;
        }

        /// <summary>
        /// Gets the custom distribution, validating and adjusting as needed.
        /// </summary>
        private int[] GetCustomDistribution()
        {
            if (_customNodeDistribution == null || _customNodeDistribution.Length == 0)
            {
                Debug.LogWarning($"[ZoneConfigSO] Custom pattern selected but no distribution defined. Falling back to Diamond.");
                return GenerateDiamondDistribution();
            }

            // If length doesn't match column count, pad or truncate
            if (_customNodeDistribution.Length != _columnCount)
            {
                Debug.LogWarning($"[ZoneConfigSO] Custom distribution length ({_customNodeDistribution.Length}) doesn't match column count ({_columnCount}). Adjusting...");

                var adjusted = new int[_columnCount];
                adjusted[0] = 1; // Force start to 1
                adjusted[_columnCount - 1] = 1; // Force end to 1

                for (int i = 1; i < _columnCount - 1; i++)
                {
                    if (i < _customNodeDistribution.Length)
                        adjusted[i] = Mathf.Clamp(_customNodeDistribution[i], 1, _maxVerticalSlots);
                    else
                        adjusted[i] = _minNodesPerColumn;
                }
                return adjusted;
            }

            // Ensure start and end are 1
            var dist = (int[])_customNodeDistribution.Clone();
            dist[0] = 1;
            dist[_columnCount - 1] = 1;

            // Clamp middle values
            for (int i = 1; i < _columnCount - 1; i++)
            {
                dist[i] = Mathf.Clamp(dist[i], 1, _maxVerticalSlots);
            }

            return dist;
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
            _minNodesPerColumn = Mathf.Min(_minNodesPerColumn, _maxNodesPerColumn);

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
