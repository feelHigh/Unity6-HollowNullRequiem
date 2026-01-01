// ============================================
// MapGenerator.cs
// Procedural map generation for Null Rifts
// ============================================

using System.Collections.Generic;
using UnityEngine;

namespace HNR.Map
{
    /// <summary>
    /// Generates procedural Null Rift maps with branching paths.
    /// Uses seeded random for deterministic output.
    /// </summary>
    public class MapGenerator
    {
        private ZoneConfigSO _config;
        private System.Random _rng;

        // ============================================
        // Public API
        // ============================================

        /// <summary>
        /// Generates a complete zone map from configuration.
        /// </summary>
        /// <param name="config">Zone configuration asset.</param>
        /// <param name="seed">Random seed for deterministic generation.</param>
        /// <returns>Complete map data ready for navigation.</returns>
        public MapData Generate(ZoneConfigSO config, int seed)
        {
            _config = config;
            _rng = new System.Random(seed);

            var mapData = new MapData
            {
                Zone = config.ZoneNumber,
                Seed = seed
            };

            // Phase 1: Generate nodes column by column (horizontal steps)
            GenerateNodes(mapData);

            // Phase 2: Create connections between columns
            GenerateConnections(mapData);

            // Phase 3: Ensure all nodes are reachable
            EnsureAllNodesReachable(mapData);

            // Phase 4: Ensure guaranteed node types exist
            EnsureGuaranteedNodes(mapData);

            // Phase 5: Calculate visual positions
            CalculatePositions(mapData);

            // Phase 6: Assign encounters to nodes
            AssignEncounters(mapData);

            // Phase 7: Initialize starting state
            InitializeStartState(mapData);

            Debug.Log($"[MapGenerator] Generated Zone {config.ZoneNumber}: {mapData.Nodes.Count} nodes, seed {seed}");
            return mapData;
        }

        // ============================================
        // Node Generation
        // ============================================

        private void GenerateNodes(MapData mapData)
        {
            for (int col = 0; col < _config.ColumnCount; col++)
            {
                int nodeCount = GetNodeCountForColumn(col);

                for (int row = 0; row < nodeCount; row++)
                {
                    var nodeType = DetermineNodeType(col);
                    var node = MapNodeData.Create($"node_{col}_{row}", nodeType, col, row);
                    mapData.Nodes.Add(node);
                }
            }
        }

        private int GetNodeCountForColumn(int col)
        {
            // Start and Boss columns have exactly 1 node
            if (col == 0 || col == _config.ColumnCount - 1)
                return 1;

            // Middle columns have variable node count (vertical spread)
            return _rng.Next(_config.MinNodesPerColumn, _config.MaxNodesPerColumn + 1);
        }

        private NodeType DetermineNodeType(int col)
        {
            // Fixed types for first and last columns
            if (col == 0) return NodeType.Start;

            // Last column: Boss if configured, otherwise Elite (serves as zone exit)
            if (col == _config.ColumnCount - 1)
            {
                bool hasBoss = _config.BossEncounter != null;
                var nodeType = hasBoss ? NodeType.Boss : NodeType.Elite;
                Debug.Log($"[MapGenerator] Final column {col}: BossEncounter={_config.BossEncounter?.name ?? "null"}, NodeType={nodeType}");
                return nodeType;
            }

            // Build weighted selection list
            var weights = BuildWeightList(col);
            int totalWeight = 0;
            foreach (var w in weights)
                totalWeight += w.Weight;

            if (totalWeight <= 0)
                return NodeType.Combat;

            // Weighted random selection
            int roll = _rng.Next(totalWeight);
            int cumulative = 0;

            foreach (var w in weights)
            {
                cumulative += w.Weight;
                if (roll < cumulative)
                    return w.Type;
            }

            return NodeType.Combat;
        }

        private List<NodeTypeWeight> BuildWeightList(int col)
        {
            var weights = new List<NodeTypeWeight>();
            bool canBeElite = col >= _config.EliteMinColumn;

            // Add weights for each valid node type
            weights.Add(new NodeTypeWeight { Type = NodeType.Combat, Weight = _config.CombatWeight });
            weights.Add(new NodeTypeWeight { Type = NodeType.Echo, Weight = _config.EchoWeight });
            weights.Add(new NodeTypeWeight { Type = NodeType.Shop, Weight = _config.ShopWeight });
            weights.Add(new NodeTypeWeight { Type = NodeType.Sanctuary, Weight = _config.SanctuaryWeight });
            weights.Add(new NodeTypeWeight { Type = NodeType.Treasure, Weight = _config.TreasureWeight });

            // Elite only in later rows
            if (canBeElite)
                weights.Add(new NodeTypeWeight { Type = NodeType.Elite, Weight = _config.EliteWeight });

            return weights;
        }

        // ============================================
        // Connection Generation
        // ============================================

        private void GenerateConnections(MapData mapData)
        {
            for (int col = 0; col < _config.ColumnCount - 1; col++)
            {
                var currentCol = mapData.GetColumn(col);
                var nextCol = mapData.GetColumn(col + 1);

                foreach (var node in currentCol)
                {
                    // Each node connects to 1-2 nodes in next column
                    int connectionCount = nextCol.Count == 1 ? 1 : _rng.Next(1, 3);
                    var shuffledNext = ShuffleList(new List<MapNodeData>(nextCol));

                    for (int i = 0; i < Mathf.Min(connectionCount, shuffledNext.Count); i++)
                    {
                        var targetNode = shuffledNext[i];
                        if (!node.ConnectedNodeIds.Contains(targetNode.NodeId))
                        {
                            node.ConnectedNodeIds.Add(targetNode.NodeId);
                            targetNode.ConnectionsFrom.Add(node.NodeId);
                        }
                    }
                }
            }
        }

        private void EnsureAllNodesReachable(MapData mapData)
        {
            // Ensure every node (except start) has at least one incoming connection
            for (int col = 1; col < _config.ColumnCount; col++)
            {
                var currentCol = mapData.GetColumn(col);
                var prevCol = mapData.GetColumn(col - 1);

                foreach (var node in currentCol)
                {
                    if (node.ConnectionsFrom.Count == 0 && prevCol.Count > 0)
                    {
                        // Connect from a random previous column node
                        var randomPrev = prevCol[_rng.Next(prevCol.Count)];
                        randomPrev.ConnectedNodeIds.Add(node.NodeId);
                        node.ConnectionsFrom.Add(randomPrev.NodeId);
                    }
                }
            }
        }

        // ============================================
        // Guaranteed Nodes
        // ============================================

        private void EnsureGuaranteedNodes(MapData mapData)
        {
            var middleNodes = GetMiddleNodes(mapData);

            // Ensure at least one Shop
            if (_config.GuaranteedShop && !HasNodeType(mapData, NodeType.Shop))
            {
                ConvertRandomCombatNode(middleNodes, NodeType.Shop);
            }

            // Ensure at least one Sanctuary
            if (_config.GuaranteedSanctuary && !HasNodeType(mapData, NodeType.Sanctuary))
            {
                ConvertRandomCombatNode(middleNodes, NodeType.Sanctuary);
            }
        }

        private List<MapNodeData> GetMiddleNodes(MapData mapData)
        {
            return mapData.Nodes.FindAll(n =>
                n.Column > 0 &&
                n.Column < _config.ColumnCount - 1);
        }

        private bool HasNodeType(MapData mapData, NodeType type)
        {
            return mapData.Nodes.Exists(n => n.Type == type);
        }

        private void ConvertRandomCombatNode(List<MapNodeData> nodes, NodeType targetType)
        {
            var combatNodes = nodes.FindAll(n => n.Type == NodeType.Combat);
            if (combatNodes.Count > 0)
            {
                combatNodes[_rng.Next(combatNodes.Count)].Type = targetType;
            }
        }

        // ============================================
        // Position Calculation
        // ============================================

        private void CalculatePositions(MapData mapData)
        {
            // Horizontal progression: Start on left, Boss on right
            // Nodes within each column are stacked vertically
            // Center the entire map so middle is at origin

            float totalWidth = (_config.ColumnCount - 1) * _config.HorizontalSpacing;
            float offsetX = -totalWidth / 2f;

            for (int col = 0; col < _config.ColumnCount; col++)
            {
                var colNodes = mapData.GetColumn(col);
                int count = colNodes.Count;

                // X = horizontal progression (left to right), centered
                // All nodes in the same column share the same X position (no jitter)
                float x = offsetX + (col * _config.HorizontalSpacing);

                // Y = fixed spacing between adjacent nodes, centered around y=0
                // This ensures consistent gap between nodes regardless of node count
                float columnHeight = (count - 1) * _config.VerticalSpacing;
                float topY = columnHeight / 2f;

                for (int i = 0; i < count; i++)
                {
                    // Position from top to bottom with fixed spacing
                    float y = topY - (i * _config.VerticalSpacing);

                    // Add vertical jitter only for visual variety (not on start/boss)
                    if (col > 0 && col < _config.ColumnCount - 1)
                    {
                        y += RandomJitter();
                    }

                    colNodes[i].Position = new Vector2(x, y);
                }
            }
        }

        private float RandomJitter()
        {
            return ((float)_rng.NextDouble() - 0.5f) * 2f * _config.NodeJitter;
        }

        // ============================================
        // Encounter Assignment
        // ============================================

        private void AssignEncounters(MapData mapData)
        {
            foreach (var node in mapData.Nodes)
            {
                switch (node.Type)
                {
                    case NodeType.Combat:
                        node.Encounter = _config.GetRandomCombatEncounter(_rng);
                        break;

                    case NodeType.Elite:
                        node.Encounter = _config.GetRandomEliteEncounter(_rng);
                        Debug.Log($"[MapGenerator] Assigned Elite encounter: {node.Encounter?.EncounterName ?? "null"} to node {node.NodeId}");
                        break;

                    case NodeType.Boss:
                        node.Encounter = _config.BossEncounter;
                        Debug.Log($"[MapGenerator] Assigned Boss encounter: {node.Encounter?.EncounterName ?? "null"} (IsBoss={node.Encounter?.IsBoss}) to node {node.NodeId}");
                        break;

                    case NodeType.Echo:
                        node.EchoEvent = _config.GetRandomEchoEvent(_rng);
                        break;
                }
            }
        }

        // ============================================
        // State Initialization
        // ============================================

        private void InitializeStartState(MapData mapData)
        {
            var startNode = mapData.GetStartNode();
            if (startNode == null) return;

            // Mark start as current
            startNode.State = NodeState.Current;
            mapData.CurrentNodeId = startNode.NodeId;

            // Unlock nodes connected to start
            foreach (var connectedId in startNode.ConnectedNodeIds)
            {
                var connected = mapData.GetNode(connectedId);
                if (connected != null)
                    connected.State = NodeState.Available;
            }
        }

        // ============================================
        // Utility
        // ============================================

        private List<T> ShuffleList<T>(List<T> list)
        {
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = _rng.Next(n + 1);
                (list[k], list[n]) = (list[n], list[k]);
            }
            return list;
        }
    }
}
