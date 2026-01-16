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

        // Cached distribution for current generation
        private int[] _nodeDistribution;

        private void GenerateNodes(MapData mapData)
        {
            // Get node distribution from pattern
            _nodeDistribution = _config.GetNodeDistribution();
            int maxSlots = _config.MaxVerticalSlots;

            for (int col = 0; col < _config.ColumnCount; col++)
            {
                int nodeCount = _nodeDistribution[col];

                // Calculate which vertical slots this column's nodes occupy
                int[] slots = CalculateSlotPositions(nodeCount, maxSlots, col);

                for (int row = 0; row < nodeCount; row++)
                {
                    var nodeType = DetermineNodeType(col);
                    var node = MapNodeData.Create($"node_{col}_{row}", nodeType, col, row);
                    node.VerticalSlot = slots[row];
                    mapData.Nodes.Add(node);
                }
            }
        }

        /// <summary>
        /// Calculates vertical slot positions for nodes in a column.
        /// Distributes nodes evenly across available slots, centered.
        /// </summary>
        private int[] CalculateSlotPositions(int nodeCount, int maxSlots, int column)
        {
            var slots = new int[nodeCount];

            if (nodeCount == 1)
            {
                // Single node centers in middle slot
                slots[0] = maxSlots / 2;
                return slots;
            }

            if (nodeCount >= maxSlots)
            {
                // Fill all slots
                for (int i = 0; i < nodeCount; i++)
                {
                    slots[i] = i;
                }
                return slots;
            }

            // Spread nodes evenly across available slots, centered
            // For 3 nodes in 5 slots: positions [1, 2, 3] (centered)
            float spacing = (float)(maxSlots - 1) / (nodeCount - 1);

            for (int i = 0; i < nodeCount; i++)
            {
                slots[i] = Mathf.RoundToInt(i * spacing);
            }

            return slots;
        }

        private int GetNodeCountForColumn(int col)
        {
            // Use distribution array if available
            if (_nodeDistribution != null && col < _nodeDistribution.Length)
                return _nodeDistribution[col];

            // Fallback: Start and Boss columns have exactly 1 node
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
        // Connection Generation (Adjacent-Slot Algorithm)
        // ============================================

        private void GenerateConnections(MapData mapData)
        {
            int maxRowDistance = _config.MaxRowDistance;

            for (int col = 0; col < _config.ColumnCount - 1; col++)
            {
                var currentCol = mapData.GetColumn(col);
                var nextCol = mapData.GetColumn(col + 1);

                bool isMergePoint = _config.IsMergeColumn(col + 1);
                bool isDivergePoint = _config.IsDivergeColumn(col);

                // Track incoming connections for load balancing
                var incomingCounts = new Dictionary<string, int>();
                foreach (var node in nextCol)
                    incomingCounts[node.NodeId] = 0;

                // Pass 1: Each node connects to at least one valid target
                foreach (var node in currentCol)
                {
                    var validTargets = GetValidTargets(node, nextCol, maxRowDistance);

                    if (validTargets.Count == 0)
                    {
                        // Fallback: Connect to closest node by slot
                        var closest = FindClosestBySlot(node, nextCol);
                        if (closest != null)
                        {
                            CreateConnection(node, closest, incomingCounts);
                        }
                    }
                    else
                    {
                        // Connect to best adjacent target
                        var bestTarget = SelectBestTarget(node, validTargets, incomingCounts, isDivergePoint);
                        CreateConnection(node, bestTarget, incomingCounts);
                    }
                }

                // Pass 2: Ensure all next-column nodes have at least one incoming connection
                foreach (var nextNode in nextCol)
                {
                    if (incomingCounts[nextNode.NodeId] == 0)
                    {
                        var validSources = GetValidSources(nextNode, currentCol, maxRowDistance);
                        if (validSources.Count > 0)
                        {
                            var source = validSources[_rng.Next(validSources.Count)];
                            CreateConnection(source, nextNode, incomingCounts);
                        }
                        else
                        {
                            // Fallback: Use closest source
                            var closest = FindClosestBySlot(nextNode, currentCol);
                            if (closest != null)
                            {
                                CreateConnection(closest, nextNode, incomingCounts);
                            }
                        }
                    }
                }

                // Pass 3: Add secondary connections for branching (if appropriate)
                if (!_config.PreferSeparatePaths && !isMergePoint)
                {
                    AddSecondaryConnections(currentCol, nextCol, maxRowDistance, incomingCounts);
                }
            }
        }

        /// <summary>
        /// Gets valid target nodes based on vertical slot adjacency.
        /// </summary>
        private List<MapNodeData> GetValidTargets(MapNodeData source, List<MapNodeData> nextCol, int maxDistance)
        {
            var valid = new List<MapNodeData>();
            foreach (var target in nextCol)
            {
                int slotDistance = Mathf.Abs(source.VerticalSlot - target.VerticalSlot);
                if (slotDistance <= maxDistance)
                {
                    valid.Add(target);
                }
            }
            return valid;
        }

        /// <summary>
        /// Gets valid source nodes that can connect to this target.
        /// </summary>
        private List<MapNodeData> GetValidSources(MapNodeData target, List<MapNodeData> prevCol, int maxDistance)
        {
            var valid = new List<MapNodeData>();
            foreach (var source in prevCol)
            {
                int slotDistance = Mathf.Abs(source.VerticalSlot - target.VerticalSlot);
                if (slotDistance <= maxDistance)
                {
                    valid.Add(source);
                }
            }
            return valid;
        }

        /// <summary>
        /// Selects the best target considering connection distribution.
        /// Prefers targets with fewer incoming connections for balanced paths.
        /// </summary>
        private MapNodeData SelectBestTarget(MapNodeData source, List<MapNodeData> targets,
            Dictionary<string, int> incomingCounts, bool isDivergePoint)
        {
            if (targets.Count == 0) return null;
            if (targets.Count == 1) return targets[0];

            // Sort by incoming count (prefer less connected)
            targets.Sort((a, b) => incomingCounts[a.NodeId].CompareTo(incomingCounts[b.NodeId]));

            // If diverge point, strongly prefer unconnected targets
            if (isDivergePoint)
            {
                var unconnected = targets.FindAll(t => incomingCounts[t.NodeId] == 0);
                if (unconnected.Count > 0)
                    return unconnected[_rng.Next(unconnected.Count)];
            }

            // Prefer separating paths if configured
            if (_config.PreferSeparatePaths)
            {
                var lowConnection = targets.FindAll(t => incomingCounts[t.NodeId] <= 1);
                if (lowConnection.Count > 0)
                    return lowConnection[_rng.Next(lowConnection.Count)];
            }

            // Return one of the least-connected targets
            int minCount = incomingCounts[targets[0].NodeId];
            var minTargets = targets.FindAll(t => incomingCounts[t.NodeId] == minCount);
            return minTargets[_rng.Next(minTargets.Count)];
        }

        /// <summary>
        /// Finds the node closest to source by vertical slot.
        /// </summary>
        private MapNodeData FindClosestBySlot(MapNodeData source, List<MapNodeData> targetCol)
        {
            if (targetCol.Count == 0) return null;

            MapNodeData closest = null;
            int minDistance = int.MaxValue;

            foreach (var target in targetCol)
            {
                int distance = Mathf.Abs(source.VerticalSlot - target.VerticalSlot);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    closest = target;
                }
            }

            return closest;
        }

        /// <summary>
        /// Creates a connection between nodes and tracks incoming counts.
        /// </summary>
        private void CreateConnection(MapNodeData from, MapNodeData to, Dictionary<string, int> incomingCounts)
        {
            if (from == null || to == null) return;
            if (from.ConnectedNodeIds.Contains(to.NodeId)) return;

            from.ConnectedNodeIds.Add(to.NodeId);
            to.ConnectionsFrom.Add(from.NodeId);
            incomingCounts[to.NodeId]++;
        }

        /// <summary>
        /// Adds secondary connections for branching diversity (30% chance per node).
        /// </summary>
        private void AddSecondaryConnections(List<MapNodeData> currentCol, List<MapNodeData> nextCol,
            int maxRowDistance, Dictionary<string, int> incomingCounts)
        {
            foreach (var node in currentCol)
            {
                // 30% chance for additional connection
                if (_rng.NextDouble() > 0.3) continue;
                if (node.ConnectedNodeIds.Count >= 2) continue;

                var validTargets = GetValidTargets(node, nextCol, maxRowDistance);
                var unconnected = validTargets.FindAll(t => !node.ConnectedNodeIds.Contains(t.NodeId));

                if (unconnected.Count > 0)
                {
                    var target = unconnected[_rng.Next(unconnected.Count)];
                    CreateConnection(node, target, incomingCounts);
                }
            }
        }

        private void EnsureAllNodesReachable(MapData mapData)
        {
            // Verify and repair any orphan nodes (should be rare with new algorithm)
            for (int col = 1; col < _config.ColumnCount; col++)
            {
                var currentCol = mapData.GetColumn(col);
                var prevCol = mapData.GetColumn(col - 1);

                foreach (var node in currentCol)
                {
                    if (node.ConnectionsFrom.Count == 0 && prevCol.Count > 0)
                    {
                        // Connect from closest node by slot
                        var closest = FindClosestBySlot(node, prevCol);
                        if (closest != null)
                        {
                            closest.ConnectedNodeIds.Add(node.NodeId);
                            node.ConnectionsFrom.Add(closest.NodeId);
                            Debug.LogWarning($"[MapGenerator] Repaired orphan node {node.NodeId} by connecting from {closest.NodeId}");
                        }
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
        // Position Calculation (Slot-Based Layout)
        // ============================================

        private void CalculatePositions(MapData mapData)
        {
            // Horizontal progression: Start on left, Boss on right
            // Vertical positioning based on VerticalSlot for consistent spacing
            // Center the entire map so middle is at origin

            float totalWidth = (_config.ColumnCount - 1) * _config.HorizontalSpacing;
            float offsetX = -totalWidth / 2f;

            // Calculate total height based on max slots (not node count)
            int maxSlots = _config.MaxVerticalSlots;
            float slotHeight = _config.VerticalSpacing;
            float totalHeight = (maxSlots - 1) * slotHeight;
            float topY = totalHeight / 2f;

            for (int col = 0; col < _config.ColumnCount; col++)
            {
                var colNodes = mapData.GetColumn(col);

                // X = horizontal progression (left to right), centered
                float baseX = offsetX + (col * _config.HorizontalSpacing);

                foreach (var node in colNodes)
                {
                    // Y position based on vertical slot (consistent across all columns)
                    float y = topY - (node.VerticalSlot * slotHeight);

                    // Add jitter for middle columns (visual variety)
                    float x = baseX;
                    if (col > 0 && col < _config.ColumnCount - 1)
                    {
                        x += RandomJitter() * 0.3f; // Reduced horizontal jitter
                        y += RandomJitter();         // Normal vertical jitter
                    }

                    node.Position = new Vector2(x, y);
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
