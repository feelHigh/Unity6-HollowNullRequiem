// ============================================
// MapScreen.cs
// Displays the Null Rift map with nodes and paths
// ============================================

using System.Collections.Generic;
using UnityEngine;
using HNR.Core;
using HNR.Core.Events;
using HNR.Core.Interfaces;
using HNR.UI;

namespace HNR.Map
{
    /// <summary>
    /// UI screen for displaying and navigating the Null Rift map.
    /// Renders nodes, paths, and handles player navigation.
    /// </summary>
    public class MapScreen : ScreenBase
    {
        // ============================================
        // References
        // ============================================

        [Header("Map References")]
        [SerializeField, Tooltip("Container for node UI instances")]
        private Transform _nodeContainer;

        [SerializeField, Tooltip("Prefab for map nodes")]
        private MapNodeUI _nodePrefab;

        [SerializeField, Tooltip("Renderer for connection paths")]
        private MapPathRenderer _pathRenderer;

        [Header("Map Display")]
        [SerializeField, Tooltip("Scroll rect for map panning")]
        private RectTransform _mapContent;

        // ============================================
        // Runtime State
        // ============================================

        private readonly Dictionary<string, MapNodeUI> _nodeUIs = new();
        private MapManager _mapManager;

        // ============================================
        // Screen Lifecycle
        // ============================================

        public override void OnShow()
        {
            base.OnShow();

            // Get MapManager reference
            _mapManager = ServiceLocator.Get<MapManager>();

            // Subscribe to map events
            EventBus.Subscribe<MapGeneratedEvent>(OnMapGenerated);
            EventBus.Subscribe<PlayerMovedToNodeEvent>(OnPlayerMoved);
            EventBus.Subscribe<NodeCompletedEvent>(OnNodeCompleted);

            // Render existing map if available
            if (_mapManager?.CurrentMap != null)
            {
                RenderMap(_mapManager.CurrentMap);
            }
        }

        public override void OnHide()
        {
            base.OnHide();

            // Unsubscribe from events
            EventBus.Unsubscribe<MapGeneratedEvent>(OnMapGenerated);
            EventBus.Unsubscribe<PlayerMovedToNodeEvent>(OnPlayerMoved);
            EventBus.Unsubscribe<NodeCompletedEvent>(OnNodeCompleted);
        }

        // ============================================
        // Event Handlers
        // ============================================

        private void OnMapGenerated(MapGeneratedEvent evt)
        {
            RenderMap(evt.Map);
        }

        private void OnPlayerMoved(PlayerMovedToNodeEvent evt)
        {
            UpdateNodeStates();
            HandleNodeAction(evt.Node);
        }

        private void OnNodeCompleted(NodeCompletedEvent evt)
        {
            UpdateNodeStates();

            // Check for zone completion
            if (evt.Node.Type == NodeType.Boss)
            {
                Debug.Log("[MapScreen] Zone complete - boss defeated");
            }
        }

        // ============================================
        // Map Rendering
        // ============================================

        private void RenderMap(MapData mapData)
        {
            ClearMap();

            if (mapData == null)
            {
                Debug.LogWarning("[MapScreen] Cannot render null map data");
                return;
            }

            // Create node UIs
            foreach (var nodeData in mapData.Nodes)
            {
                CreateNodeUI(nodeData);
            }

            // Render connection paths
            if (_pathRenderer != null)
            {
                _pathRenderer.RenderPaths(mapData, _nodeUIs);
            }

            // Center map on current node
            CenterOnCurrentNode();

            Debug.Log($"[MapScreen] Rendered {_nodeUIs.Count} nodes");
        }

        private void CreateNodeUI(MapNodeData nodeData)
        {
            if (_nodePrefab == null)
            {
                Debug.LogWarning("[MapScreen] Node prefab is null");
                return;
            }

            var container = _nodeContainer != null ? _nodeContainer : transform;
            var nodeUI = Instantiate(_nodePrefab, container);
            nodeUI.Initialize(nodeData, OnNodeClicked);
            _nodeUIs[nodeData.NodeId] = nodeUI;
        }

        private void ClearMap()
        {
            foreach (var nodeUI in _nodeUIs.Values)
            {
                if (nodeUI != null)
                    Destroy(nodeUI.gameObject);
            }
            _nodeUIs.Clear();

            if (_pathRenderer != null)
            {
                _pathRenderer.ClearPaths();
            }
        }

        private void UpdateNodeStates()
        {
            foreach (var kvp in _nodeUIs)
            {
                kvp.Value.UpdateVisuals();
            }

            if (_pathRenderer != null && _mapManager?.CurrentMap != null)
            {
                _pathRenderer.UpdatePathColors(_mapManager.CurrentMap);
            }
        }

        private void CenterOnCurrentNode()
        {
            if (_mapContent == null || _mapManager?.CurrentNode == null) return;

            var currentNode = _mapManager.CurrentNode;
            if (_nodeUIs.TryGetValue(currentNode.NodeId, out var nodeUI))
            {
                // Center scroll view on current node position
                _mapContent.anchoredPosition = -nodeUI.transform.localPosition;
            }
        }

        // ============================================
        // Node Interaction
        // ============================================

        private void OnNodeClicked(MapNodeData nodeData)
        {
            if (_mapManager == null)
            {
                Debug.LogWarning("[MapScreen] MapManager not available");
                return;
            }

            bool success = _mapManager.TryMoveToNode(nodeData.NodeId);
            if (!success)
            {
                Debug.Log($"[MapScreen] Cannot move to node {nodeData.NodeId}");
            }
        }

        // ============================================
        // Node Actions
        // ============================================

        private void HandleNodeAction(MapNodeData node)
        {
            Debug.Log($"[MapScreen] Handling node action: {node.Type}");

            switch (node.Type)
            {
                case NodeType.Start:
                    // No action for start node
                    break;

                case NodeType.Combat:
                case NodeType.Elite:
                case NodeType.Boss:
                    StartCombat(node);
                    break;

                case NodeType.Echo:
                    ShowEchoEvent(node);
                    break;

                case NodeType.Shop:
                    ShowShop();
                    break;

                case NodeType.Sanctuary:
                    ShowSanctuary();
                    break;

                case NodeType.Treasure:
                    ShowTreasure();
                    break;
            }
        }

        private void StartCombat(MapNodeData node)
        {
            if (node.Encounter == null)
            {
                Debug.LogWarning($"[MapScreen] No encounter data for combat node {node.NodeId}");
                _mapManager?.CompleteCurrentNode();
                return;
            }

            // Transition to combat state
            if (ServiceLocator.TryGet<IGameManager>(out var gameManager))
            {
                Debug.Log($"[MapScreen] Starting combat: {node.Encounter.EncounterName}");
                gameManager.ChangeState(GameState.Combat);
            }
            else
            {
                Debug.LogWarning("[MapScreen] GameManager not available for combat transition");
            }
        }

        private void ShowEchoEvent(MapNodeData node)
        {
            if (node.EchoEvent == null)
            {
                Debug.LogWarning($"[MapScreen] No echo event data for node {node.NodeId}");
                _mapManager?.CompleteCurrentNode();
                return;
            }

            // TODO: Implement EchoEventManager integration
            Debug.Log($"[MapScreen] Echo event: {node.EchoEvent.EventTitle}");

            // For now, auto-complete the node
            _mapManager?.CompleteCurrentNode();
        }

        private void ShowShop()
        {
            // TODO: Implement shop screen transition
            Debug.Log("[MapScreen] Shop not implemented yet");
            _mapManager?.CompleteCurrentNode();
        }

        private void ShowSanctuary()
        {
            // TODO: Implement sanctuary screen transition
            Debug.Log("[MapScreen] Sanctuary not implemented yet");
            _mapManager?.CompleteCurrentNode();
        }

        private void ShowTreasure()
        {
            // TODO: Implement treasure reward popup
            Debug.Log("[MapScreen] Treasure not implemented yet");
            _mapManager?.CompleteCurrentNode();
        }

        // ============================================
        // Public Methods
        // ============================================

        /// <summary>
        /// Refreshes the map display.
        /// </summary>
        public void RefreshMap()
        {
            if (_mapManager?.CurrentMap != null)
            {
                RenderMap(_mapManager.CurrentMap);
            }
        }

        /// <summary>
        /// Scrolls the map to show the current node.
        /// </summary>
        public void FocusCurrentNode()
        {
            CenterOnCurrentNode();
        }
    }
}
