// ============================================
// MapScreen.cs
// Displays the Null Rift map with nodes and paths
// ============================================

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using HNR.Core;
using HNR.Core.Events;
using HNR.Core.Interfaces;
using HNR.Combat;
using HNR.UI;
using HNR.UI.Screens;
using HNR.UI.Components;

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

        [Header("Zone Header")]
        [SerializeField, Tooltip("Zone title text (NULL RIFT)")]
        private TMP_Text _zoneTitle;

        [SerializeField, Tooltip("Zone subtitle (Zone X • Zone Name)")]
        private TMP_Text _zoneSubtitle;

        [SerializeField, Tooltip("Team HP display text")]
        private TMP_Text _hpText;

        [SerializeField, Tooltip("HP icon")]
        private Image _hpIcon;

        [SerializeField, Tooltip("Currency display text")]
        private TMP_Text _currencyText;

        [SerializeField, Tooltip("Currency icon")]
        private Image _currencyIcon;

        [Header("Navigation")]
        [SerializeField, Tooltip("Back button to abandon run")]
        private Button _backButton;

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
            EventBus.Subscribe<TeamHPChangedEvent>(OnTeamHPChanged);
            EventBus.Subscribe<VoidShardsChangedEvent>(OnVoidShardsChanged);

            // Setup back button
            if (_backButton != null)
            {
                _backButton.onClick.AddListener(OnBackButtonClicked);
            }

            // Update zone header
            UpdateZoneHeader();

            // Render existing map if available
            if (_mapManager?.CurrentMap != null)
            {
                RenderMap(_mapManager.CurrentMap);
            }
        }

        public override void OnHide()
        {
            base.OnHide();
            UnsubscribeFromEvents();

            // Remove back button listener
            if (_backButton != null)
            {
                _backButton.onClick.RemoveListener(OnBackButtonClicked);
            }
        }

        private void OnDestroy()
        {
            // Ensure we unsubscribe when destroyed (scene change)
            UnsubscribeFromEvents();

            if (_backButton != null)
            {
                _backButton.onClick.RemoveListener(OnBackButtonClicked);
            }
        }

        private void UnsubscribeFromEvents()
        {
            EventBus.Unsubscribe<MapGeneratedEvent>(OnMapGenerated);
            EventBus.Unsubscribe<PlayerMovedToNodeEvent>(OnPlayerMoved);
            EventBus.Unsubscribe<NodeCompletedEvent>(OnNodeCompleted);
            EventBus.Unsubscribe<TeamHPChangedEvent>(OnTeamHPChanged);
            EventBus.Unsubscribe<VoidShardsChangedEvent>(OnVoidShardsChanged);
        }

        private void OnBackButtonClicked()
        {
            ConfirmationDialog.Show(
                "Abandon Run?",
                "Are you sure you want to abandon the current run? All progress will be lost.",
                onConfirm: () =>
                {
                    // End run without victory
                    if (ServiceLocator.TryGet<IRunManager>(out var runManager))
                    {
                        runManager.EndRun(false);
                    }

                    // Delete saved run
                    if (ServiceLocator.TryGet<ISaveManager>(out var saveManager))
                    {
                        saveManager.DeleteRun();
                    }

                    // Navigate to Bastion
                    if (ServiceLocator.TryGet<IGameManager>(out var gameManager))
                    {
                        gameManager.ChangeState(GameState.Bastion);
                    }
                },
                onCancel: null,
                confirmText: "Abandon",
                cancelText: "Continue Run"
            );
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

        private void OnTeamHPChanged(TeamHPChangedEvent evt)
        {
            UpdateHPDisplay(evt.CurrentHP, evt.MaxHP);
        }

        private void OnVoidShardsChanged(VoidShardsChangedEvent evt)
        {
            UpdateCurrencyDisplay(evt.NewValue);
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

            // Map is already centered by MapGenerator - no additional offset needed

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

            // Cache map state before transitioning to combat
            // Note: RunManager is registered as IRunManager, so get by interface and cast
            if (ServiceLocator.TryGet<IRunManager>(out var runManagerInterface))
            {
                var runManager = runManagerInterface as HNR.Progression.RunManager;
                if (runManager != null)
                {
                    runManager.CacheMapState();
                    Debug.Log($"[MapScreen] Map state cached before combat. CurrentNode: {_mapManager?.CurrentNode?.NodeId}");
                }
                else
                {
                    Debug.LogWarning("[MapScreen] RunManager cast failed - map state not cached!");
                }
            }
            else
            {
                Debug.LogWarning("[MapScreen] RunManager not found - map state not cached!");
            }

            // Set pending combat data for CombatBootstrap
            int zone = _mapManager?.CurrentZone ?? 1;
            CombatBootstrap.SetPendingCombat(node.Encounter, zone);

            // Transition to combat state
            if (ServiceLocator.TryGet<IGameManager>(out var gameManager))
            {
                Debug.Log($"[MapScreen] Starting combat: {node.Encounter.EncounterName}");
                gameManager.ChangeState(GameState.Combat);
            }
            else
            {
                Debug.LogWarning("[MapScreen] GameManager not available for combat transition");
                CombatBootstrap.ClearPendingCombat();
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

            // Start the event via EchoEventManager
            var echoManager = ServiceLocator.Get<EchoEventManager>();
            if (echoManager != null)
            {
                echoManager.StartEvent(node.EchoEvent);
            }

            // Navigate to EchoEventScreen
            var uiManager = ServiceLocator.Get<IUIManager>();
            if (uiManager != null)
            {
                Debug.Log($"[MapScreen] Showing Echo event: {node.EchoEvent.EventTitle}");
                uiManager.ShowScreen<EchoEventScreen>();
            }
            else
            {
                Debug.LogWarning("[MapScreen] UIManager not available for echo event transition");
                _mapManager?.CompleteCurrentNode();
            }
        }

        private void ShowShop()
        {
            // Open shop via ShopManager
            var shopManager = ServiceLocator.Get<IShopManager>();
            if (shopManager != null)
            {
                int zone = _mapManager?.CurrentZone ?? 1;
                shopManager.OpenShop(zone);
            }

            // Navigate to shop screen
            var uiManager = ServiceLocator.Get<IUIManager>();
            if (uiManager != null)
            {
                uiManager.ShowScreen<ShopScreen>();
            }
            else
            {
                Debug.LogWarning("[MapScreen] UIManager not available for shop transition");
                _mapManager?.CompleteCurrentNode();
            }
        }

        private void ShowSanctuary()
        {
            // Navigate to SanctuaryScreen
            var uiManager = ServiceLocator.Get<IUIManager>();
            if (uiManager != null)
            {
                Debug.Log("[MapScreen] Showing Sanctuary screen");
                uiManager.ShowScreen<SanctuaryScreen>();
            }
            else
            {
                Debug.LogWarning("[MapScreen] UIManager not available for sanctuary transition");
                _mapManager?.CompleteCurrentNode();
            }
        }

        private void ShowTreasure()
        {
            // Navigate to TreasureScreen for reward selection
            var uiManager = ServiceLocator.Get<IUIManager>();
            if (uiManager != null)
            {
                Debug.Log("[MapScreen] Showing Treasure screen");
                uiManager.ShowScreen<TreasureScreen>();
            }
            else
            {
                Debug.LogWarning("[MapScreen] UIManager not available for treasure transition");
                _mapManager?.CompleteCurrentNode();
            }
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

        // ============================================
        // Zone Header
        // ============================================

        private void UpdateZoneHeader()
        {
            // Update zone title
            if (_zoneTitle != null)
            {
                _zoneTitle.text = "NULL RIFT";
            }

            // Update zone subtitle
            if (_zoneSubtitle != null)
            {
                int zone = _mapManager?.CurrentZone ?? 1;
                string zoneName = GetZoneName(zone);
                _zoneSubtitle.text = $"Zone {zone} • {zoneName}";
            }

            // Get HP from RunManager
            if (ServiceLocator.TryGet<IRunManager>(out var runManager))
            {
                UpdateHPDisplay(runManager.TeamCurrentHP, runManager.TeamMaxHP);
            }

            // Get currency from ShopManager
            if (ServiceLocator.TryGet<IShopManager>(out var shopManager))
            {
                UpdateCurrencyDisplay(shopManager.VoidShards);
            }
        }

        private void UpdateHPDisplay(int current, int max)
        {
            if (_hpText != null)
            {
                _hpText.text = $"{current}/{max}";
            }
        }

        private void UpdateCurrencyDisplay(int amount)
        {
            if (_currencyText != null)
            {
                _currencyText.text = amount.ToString();
            }
        }

        private string GetZoneName(int zone)
        {
            return zone switch
            {
                1 => "The Outer Reaches",
                2 => "The Hollow Depths",
                3 => "The Null Core",
                _ => "Unknown Zone"
            };
        }
    }
}
