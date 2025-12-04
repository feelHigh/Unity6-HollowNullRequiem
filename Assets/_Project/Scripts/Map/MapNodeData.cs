// ============================================
// MapNodeData.cs
// Serializable data structures for map nodes
// ============================================

using System;
using System.Collections.Generic;
using UnityEngine;
using HNR.Combat;

namespace HNR.Map
{
    /// <summary>
    /// Data for a single map node. Serializable for save/load.
    /// </summary>
    [Serializable]
    public class MapNodeData
    {
        // ============================================
        // Identity
        // ============================================

        /// <summary>Unique identifier for this node.</summary>
        public string NodeId;

        /// <summary>Type of encounter at this node.</summary>
        public NodeType Type;

        // ============================================
        // Position
        // ============================================

        /// <summary>Row index (0 = start, last = boss).</summary>
        public int Row;

        /// <summary>Column index within the row.</summary>
        public int Column;

        /// <summary>Visual position for UI rendering.</summary>
        public Vector2 Position;

        // ============================================
        // Connections
        // ============================================

        /// <summary>Node IDs this node connects to (forward).</summary>
        public List<string> ConnectedNodeIds = new();

        /// <summary>Node IDs that connect to this node (backward).</summary>
        public List<string> ConnectionsFrom = new();

        // ============================================
        // State
        // ============================================

        /// <summary>Current accessibility state.</summary>
        public NodeState State = NodeState.Locked;

        // ============================================
        // Encounter Data (runtime only, not serialized)
        // ============================================

        /// <summary>Combat encounter configuration (for Combat/Elite/Boss nodes).</summary>
        [NonSerialized] public EncounterDataSO Encounter;

        /// <summary>Event data (for Echo nodes).</summary>
        [NonSerialized] public EchoEventDataSO EchoEvent;

        // ============================================
        // Computed Properties
        // ============================================

        /// <summary>Whether the player can travel to this node.</summary>
        public bool IsAccessible => State == NodeState.Available || State == NodeState.Current;

        /// <summary>Whether this node has been completed.</summary>
        public bool IsCompleted => State == NodeState.Visited;

        /// <summary>Whether this is the current node.</summary>
        public bool IsCurrent => State == NodeState.Current;

        // ============================================
        // Factory
        // ============================================

        /// <summary>
        /// Creates a new node with the specified parameters.
        /// </summary>
        public static MapNodeData Create(string nodeId, NodeType type, int row, int column)
        {
            return new MapNodeData
            {
                NodeId = nodeId,
                Type = type,
                Row = row,
                Column = column,
                State = row == 0 ? NodeState.Current : NodeState.Locked
            };
        }
    }

    /// <summary>
    /// Complete map data for a zone. Serializable for save/load.
    /// </summary>
    [Serializable]
    public class MapData
    {
        // ============================================
        // Zone Info
        // ============================================

        /// <summary>Current zone number (1-3).</summary>
        public int Zone;

        /// <summary>Seed used for map generation (for reproducibility).</summary>
        public int Seed;

        // ============================================
        // Nodes
        // ============================================

        /// <summary>All nodes in this map.</summary>
        public List<MapNodeData> Nodes = new();

        /// <summary>ID of the node the player is currently at.</summary>
        public string CurrentNodeId;

        // ============================================
        // Query Methods
        // ============================================

        /// <summary>
        /// Gets a node by its ID.
        /// </summary>
        public MapNodeData GetNode(string nodeId)
        {
            return Nodes.Find(n => n.NodeId == nodeId);
        }

        /// <summary>
        /// Gets all nodes in a specific row.
        /// </summary>
        public List<MapNodeData> GetRow(int row)
        {
            return Nodes.FindAll(n => n.Row == row);
        }

        /// <summary>
        /// Gets the current node.
        /// </summary>
        public MapNodeData GetCurrentNode()
        {
            return GetNode(CurrentNodeId);
        }

        /// <summary>
        /// Gets nodes connected to the current node.
        /// </summary>
        public List<MapNodeData> GetAvailableNodes()
        {
            var current = GetCurrentNode();
            if (current == null) return new List<MapNodeData>();

            var available = new List<MapNodeData>();
            foreach (var nodeId in current.ConnectedNodeIds)
            {
                var node = GetNode(nodeId);
                if (node != null && node.State != NodeState.Visited)
                {
                    available.Add(node);
                }
            }
            return available;
        }

        /// <summary>
        /// Gets the start node.
        /// </summary>
        public MapNodeData GetStartNode()
        {
            return Nodes.Find(n => n.Type == NodeType.Start);
        }

        /// <summary>
        /// Gets the boss node.
        /// </summary>
        public MapNodeData GetBossNode()
        {
            return Nodes.Find(n => n.Type == NodeType.Boss);
        }

        /// <summary>
        /// Checks if the zone is complete (boss defeated).
        /// </summary>
        public bool IsZoneComplete()
        {
            var boss = GetBossNode();
            return boss?.State == NodeState.Visited;
        }

        /// <summary>
        /// Gets count of nodes by type.
        /// </summary>
        public int CountNodesByType(NodeType type)
        {
            int count = 0;
            foreach (var node in Nodes)
            {
                if (node.Type == type) count++;
            }
            return count;
        }
    }
}
