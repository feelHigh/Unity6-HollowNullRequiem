// ============================================
// MapManager.cs
// Manages map state, navigation, and node encounters
// ============================================

using UnityEngine;
using HNR.Core;
using HNR.Core.Events;
using HNR.Core.Interfaces;

namespace HNR.Map
{
    /// <summary>
    /// Manages the current map state and player navigation.
    /// Registers with ServiceLocator for global access.
    /// </summary>
    public class MapManager : MonoBehaviour
    {
        // ============================================
        // Configuration
        // ============================================

        [Header("Zone Configurations")]
        [SerializeField, Tooltip("Zone configs in order (Zone 1, Zone 2, Zone 3)")]
        private ZoneConfigSO[] _zoneConfigs;

        // ============================================
        // Runtime State
        // ============================================

        private MapGenerator _generator;
        private MapData _currentMap;

        // ============================================
        // Public Properties
        // ============================================

        /// <summary>Current map data.</summary>
        public MapData CurrentMap => _currentMap;

        /// <summary>Current zone number.</summary>
        public int CurrentZone => _currentMap?.Zone ?? 0;

        /// <summary>Current node the player is at.</summary>
        public MapNodeData CurrentNode => _currentMap?.GetNode(_currentMap.CurrentNodeId);

        /// <summary>Whether a map is currently active.</summary>
        public bool HasActiveMap => _currentMap != null;

        /// <summary>Whether current zone is complete (boss defeated).</summary>
        public bool IsZoneComplete => _currentMap?.IsZoneComplete() ?? false;

        // ============================================
        // Unity Lifecycle
        // ============================================

        private void Awake()
        {
            _generator = new MapGenerator();
            ServiceLocator.Register(this);
        }

        private void OnDestroy()
        {
            ServiceLocator.Unregister<MapManager>();
        }

        // ============================================
        // Map Generation
        // ============================================

        /// <summary>
        /// Generates a new map for the specified zone.
        /// </summary>
        /// <param name="zone">Zone number (1-3).</param>
        /// <param name="seed">Random seed (-1 for random).</param>
        public void GenerateMap(int zone, int seed = -1)
        {
            if (zone < 1 || zone > _zoneConfigs.Length)
            {
                Debug.LogError($"[MapManager] Invalid zone: {zone}. Available: 1-{_zoneConfigs.Length}");
                return;
            }

            var config = _zoneConfigs[zone - 1];
            if (config == null)
            {
                Debug.LogError($"[MapManager] Zone config {zone} is null");
                return;
            }

            // Generate seed if not provided
            if (seed < 0)
                seed = Random.Range(0, int.MaxValue);

            // Generate map
            _currentMap = _generator.Generate(config, seed);

            // Publish event for UI updates
            EventBus.Publish(new MapGeneratedEvent(_currentMap));

            Debug.Log($"[MapManager] Generated Zone {zone} map: {_currentMap.Nodes.Count} nodes, seed {seed}");
        }

        // ============================================
        // Navigation
        // ============================================

        /// <summary>
        /// Attempts to move the player to a node.
        /// </summary>
        /// <param name="nodeId">Target node ID.</param>
        /// <returns>True if movement succeeded.</returns>
        public bool TryMoveToNode(string nodeId)
        {
            if (_currentMap == null)
            {
                Debug.LogWarning("[MapManager] No active map");
                return false;
            }

            var targetNode = _currentMap.GetNode(nodeId);
            if (targetNode == null)
            {
                Debug.LogWarning($"[MapManager] Node not found: {nodeId}");
                return false;
            }

            if (targetNode.State != NodeState.Available)
            {
                Debug.Log($"[MapManager] Node {nodeId} not available (state: {targetNode.State})");
                return false;
            }

            // Mark current node as visited
            var currentNode = CurrentNode;
            if (currentNode != null)
                currentNode.State = NodeState.Visited;

            // Move to target node
            targetNode.State = NodeState.Current;
            _currentMap.CurrentNodeId = nodeId;

            // Unlock connected nodes
            UnlockConnectedNodes(targetNode);

            // Publish movement event
            EventBus.Publish(new PlayerMovedToNodeEvent(targetNode));

            Debug.Log($"[MapManager] Moved to node {nodeId} ({targetNode.Type})");
            return true;
        }

        /// <summary>
        /// Gets nodes available for travel from current position.
        /// </summary>
        public System.Collections.Generic.List<MapNodeData> GetAvailableNodes()
        {
            return _currentMap?.GetAvailableNodes() ?? new System.Collections.Generic.List<MapNodeData>();
        }

        private void UnlockConnectedNodes(MapNodeData node)
        {
            foreach (var connectedId in node.ConnectedNodeIds)
            {
                var connected = _currentMap.GetNode(connectedId);
                if (connected != null && connected.State == NodeState.Locked)
                    connected.State = NodeState.Available;
            }
        }

        /// <summary>
        /// Checks if the given row is the last row of the map.
        /// </summary>
        private bool IsLastRow(int row)
        {
            if (_currentMap == null) return false;

            // Find the maximum row number in the map
            int maxRow = 0;
            foreach (var node in _currentMap.Nodes)
            {
                if (node.Row > maxRow) maxRow = node.Row;
            }

            return row == maxRow;
        }

        /// <summary>
        /// Locks sibling nodes (same row, not visited) when committing to a path.
        /// This prevents backtracking to other nodes in the same row after completing a node.
        /// </summary>
        private void LockSiblingNodes(MapNodeData completedNode)
        {
            if (_currentMap == null) return;

            int currentRow = completedNode.Row;

            foreach (var node in _currentMap.Nodes)
            {
                // Lock sibling nodes in the same row that are still Available (not visited)
                if (node.Row == currentRow &&
                    node.NodeId != completedNode.NodeId &&
                    node.State == NodeState.Available)
                {
                    node.State = NodeState.Locked;
                    Debug.Log($"[MapManager] Locked sibling node {node.NodeId} (row {currentRow})");
                }
            }
        }

        // ============================================
        // Node Completion
        // ============================================

        /// <summary>
        /// Marks the current node as completed.
        /// Called after combat victory, event resolution, etc.
        /// </summary>
        public void CompleteCurrentNode()
        {
            var current = CurrentNode;
            if (current == null)
            {
                Debug.LogWarning("[MapManager] No current node to complete");
                return;
            }

            current.State = NodeState.Visited;

            // Lock sibling nodes (same row) that weren't visited - player committed to this path
            LockSiblingNodes(current);

            EventBus.Publish(new NodeCompletedEvent(current));

            Debug.Log($"[MapManager] Completed node {current.NodeId} ({current.Type})");

            // Immediate save after node completion to prevent data loss
            if (ServiceLocator.TryGet<IRunManager>(out var runManager) && runManager.IsRunActive)
            {
                runManager.SaveRun();
                Debug.Log("[MapManager] Run saved after node completion");
            }

            // Check for zone completion
            // Zone is complete when the boss is defeated, OR when completing
            // the last row's Elite node (for zones without bosses)
            bool isBossNode = current.Type == NodeType.Boss;
            bool isLastRowElite = current.Type == NodeType.Elite && IsLastRow(current.Row);

            if (isBossNode || isLastRowElite)
            {
                EventBus.Publish(new ZoneCompletedEvent(_currentMap.Zone));
                Debug.Log($"[MapManager] Zone {_currentMap.Zone} completed!");
            }
        }

        // ============================================
        // Save/Load
        // ============================================

        /// <summary>
        /// Loads map state from saved data.
        /// </summary>
        public void LoadMapState(MapData savedMap)
        {
            if (savedMap == null)
            {
                Debug.LogWarning("[MapManager] Cannot load null map data");
                return;
            }

            _currentMap = savedMap;
            EventBus.Publish(new MapGeneratedEvent(_currentMap));

            Debug.Log($"[MapManager] Loaded Zone {savedMap.Zone} map state");
        }

        /// <summary>
        /// Restores map state from save data by regenerating map with same seed
        /// and applying saved node states.
        /// </summary>
        /// <param name="mapSaveData">Saved map state to restore.</param>
        public void RestoreMapState(HNR.Progression.MapSaveData mapSaveData)
        {
            if (mapSaveData == null)
            {
                Debug.LogWarning("[MapManager] Cannot restore null map save data");
                return;
            }

            Debug.Log($"[MapManager] RestoreMapState called: Zone={mapSaveData.Zone}, Seed={mapSaveData.Seed}, CurrentNode={mapSaveData.CurrentNodeId}");

            // Regenerate map with same seed
            GenerateMap(mapSaveData.Zone, mapSaveData.Seed);

            if (_currentMap == null)
            {
                Debug.LogError("[MapManager] Failed to regenerate map for restoration");
                return;
            }

            Debug.Log($"[MapManager] Regenerated map with {_currentMap.Nodes.Count} nodes");

            // Reset ALL nodes to Locked first (we'll restore proper states next)
            foreach (var node in _currentMap.Nodes)
            {
                node.State = NodeState.Locked;
            }

            // Restore current node ID
            if (!string.IsNullOrEmpty(mapSaveData.CurrentNodeId))
            {
                _currentMap.CurrentNodeId = mapSaveData.CurrentNodeId;
                Debug.Log($"[MapManager] Set CurrentNodeId to: {mapSaveData.CurrentNodeId}");
            }

            // Restore visited node states
            foreach (var visitedNode in mapSaveData.VisitedNodes)
            {
                var node = _currentMap.GetNode(visitedNode.NodeId);
                if (node != null)
                {
                    node.State = visitedNode.Completed ? NodeState.Visited : NodeState.Current;
                    Debug.Log($"[MapManager] Restored node {visitedNode.NodeId}: Completed={visitedNode.Completed}, State={node.State}");
                }
                else
                {
                    Debug.LogWarning($"[MapManager] Node not found for restoration: {visitedNode.NodeId}");
                }
            }

            // Get current node to determine which row the player is at
            var currentNode = _currentMap.GetNode(_currentMap.CurrentNodeId);
            int currentRow = currentNode?.Row ?? 0;

            Debug.Log($"[MapManager] Current node after restore: {currentNode?.NodeId}, Row={currentRow}, State={currentNode?.State}, Connections={currentNode?.ConnectedNodeIds.Count ?? 0}");

            // In a roguelike map, only nodes in rows AFTER the current row can be available
            // Nodes in previous rows or the same row (but not visited) are permanently locked
            foreach (var accessibleId in mapSaveData.AccessibleNodeIds)
            {
                var node = _currentMap.GetNode(accessibleId);
                if (node != null && node.State == NodeState.Locked)
                {
                    // Only restore as Available if the node is in a row ahead of current position
                    if (node.Row > currentRow)
                    {
                        node.State = NodeState.Available;
                        Debug.Log($"[MapManager] Restored accessible node: {accessibleId} (row {node.Row})");
                    }
                    else
                    {
                        Debug.Log($"[MapManager] Skipped accessible node {accessibleId} (row {node.Row} <= current row {currentRow})");
                    }
                }
            }

            // Ensure forward nodes from current position are available (nodes connected from current node)
            if (currentNode != null)
            {
                foreach (var connectedId in currentNode.ConnectedNodeIds)
                {
                    var connectedNode = _currentMap.GetNode(connectedId);
                    if (connectedNode != null && connectedNode.State == NodeState.Locked && connectedNode.Row > currentRow)
                    {
                        connectedNode.State = NodeState.Available;
                        Debug.Log($"[MapManager] Unlocked forward node from current: {connectedId}");
                    }
                }
            }

            // Re-publish event for UI update
            EventBus.Publish(new MapGeneratedEvent(_currentMap));

            Debug.Log($"[MapManager] Restored map state complete: Zone={mapSaveData.Zone}, CurrentNode={mapSaveData.CurrentNodeId}, Visited={mapSaveData.VisitedNodes.Count}");
        }

        /// <summary>
        /// Gets current map data for saving.
        /// </summary>
        public MapData GetMapStateForSave()
        {
            return _currentMap;
        }

        // ============================================
        // Zone Progression
        // ============================================

        /// <summary>
        /// Advances to the next zone.
        /// </summary>
        /// <param name="seed">Optional seed for next zone.</param>
        public void AdvanceToNextZone(int seed = -1)
        {
            int nextZone = CurrentZone + 1;
            if (nextZone > _zoneConfigs.Length)
            {
                Debug.Log("[MapManager] No more zones available");
                return;
            }

            GenerateMap(nextZone, seed);
        }

        /// <summary>
        /// Clears the current map.
        /// </summary>
        public void ClearMap()
        {
            _currentMap = null;
            Debug.Log("[MapManager] Map cleared");
        }
    }

    // ============================================
    // Map Events
    // ============================================

    /// <summary>
    /// Published when a new map is generated or loaded.
    /// </summary>
    public class MapGeneratedEvent : GameEvent
    {
        public MapData Map { get; }
        public MapGeneratedEvent(MapData map) => Map = map;
    }

    /// <summary>
    /// Published when player moves to a new node.
    /// </summary>
    public class PlayerMovedToNodeEvent : GameEvent
    {
        public MapNodeData Node { get; }
        public PlayerMovedToNodeEvent(MapNodeData node) => Node = node;
    }

    /// <summary>
    /// Published when a node encounter is completed.
    /// </summary>
    public class NodeCompletedEvent : GameEvent
    {
        public MapNodeData Node { get; }
        public NodeCompletedEvent(MapNodeData node) => Node = node;
    }

    /// <summary>
    /// Published when the zone boss is defeated.
    /// </summary>
    public class ZoneCompletedEvent : GameEvent
    {
        public int Zone { get; }
        public ZoneCompletedEvent(int zone) => Zone = zone;
    }
}
