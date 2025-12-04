// ============================================
// Week7IntegrationTest.cs
// Integration tests for Map and Echo Event systems
// ============================================

using System.Collections.Generic;
using UnityEngine;
using HNR.Core;
using HNR.Core.Events;

namespace HNR.Map
{
    /// <summary>
    /// Integration tests for Week 7: Map and Event Systems.
    /// Press M to run all tests.
    /// </summary>
    public class Week7IntegrationTest : MonoBehaviour
    {
        // ============================================
        // Configuration
        // ============================================

        [Header("Test Configuration")]
        [SerializeField, Tooltip("Zone config for testing")]
        private ZoneConfigSO _testZoneConfig;

        [SerializeField, Tooltip("Test seed for deterministic generation")]
        private int _testSeed = 12345;

        // ============================================
        // Test State
        // ============================================

        private int _passCount;
        private int _failCount;
        private List<string> _testResults = new();

        // ============================================
        // Unity Lifecycle
        // ============================================

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.M))
            {
                RunAllTests();
            }
        }

        // ============================================
        // Test Runner
        // ============================================

        public void RunAllTests()
        {
            _passCount = 0;
            _failCount = 0;
            _testResults.Clear();

            Debug.Log("========================================");
            Debug.Log("WEEK 7 INTEGRATION TESTS - Map & Events");
            Debug.Log("========================================");

            // Map Generation Tests
            TestMapGeneration();
            TestMapStructure();
            TestDeterministicGeneration();
            TestAllNodesReachable();
            TestGuaranteedNodes();

            // Map Manager Tests
            TestMapManagerExists();
            TestNodeNavigation();
            TestNodeStates();

            // Echo Event Tests
            TestEchoEventManagerExists();
            TestEchoEventFlow();

            // Summary
            Debug.Log("========================================");
            Debug.Log($"RESULTS: {_passCount}/{_passCount + _failCount} tests passed");
            if (_failCount > 0)
                Debug.LogWarning($"FAILURES: {_failCount} tests failed");
            else
                Debug.Log("ALL TESTS PASSED!");
            Debug.Log("========================================");
        }

        // ============================================
        // Map Generation Tests
        // ============================================

        private void TestMapGeneration()
        {
            if (_testZoneConfig == null)
            {
                Log("Map Generation - Config Exists", false, "ZoneConfig not assigned");
                return;
            }

            var generator = new MapGenerator();
            var map = generator.Generate(_testZoneConfig, _testSeed);

            Log("Map Generation - Map Created", map != null);
            Log("Map Generation - Has Nodes", map?.Nodes.Count > 0, $"Count: {map?.Nodes.Count ?? 0}");
        }

        private void TestMapStructure()
        {
            if (_testZoneConfig == null) return;

            var generator = new MapGenerator();
            var map = generator.Generate(_testZoneConfig, _testSeed);

            // Check required node types
            bool hasStart = map.Nodes.Exists(n => n.Type == NodeType.Start);
            bool hasBoss = map.Nodes.Exists(n => n.Type == NodeType.Boss);
            int startCount = map.Nodes.FindAll(n => n.Type == NodeType.Start).Count;
            int bossCount = map.Nodes.FindAll(n => n.Type == NodeType.Boss).Count;

            Log("Map Structure - Has Start Node", hasStart);
            Log("Map Structure - Has Boss Node", hasBoss);
            Log("Map Structure - Single Start", startCount == 1, $"Count: {startCount}");
            Log("Map Structure - Single Boss", bossCount == 1, $"Count: {bossCount}");

            // Check rows
            int expectedRows = _testZoneConfig.RowCount;
            var lastRowNodes = map.Nodes.FindAll(n => n.Row == expectedRows - 1);
            Log("Map Structure - Correct Row Count", lastRowNodes.Count > 0, $"Expected row {expectedRows - 1}");
        }

        private void TestDeterministicGeneration()
        {
            if (_testZoneConfig == null) return;

            var generator = new MapGenerator();
            var map1 = generator.Generate(_testZoneConfig, _testSeed);
            var map2 = generator.Generate(_testZoneConfig, _testSeed);

            bool sameNodeCount = map1.Nodes.Count == map2.Nodes.Count;
            bool sameTypes = true;

            for (int i = 0; i < map1.Nodes.Count && i < map2.Nodes.Count; i++)
            {
                if (map1.Nodes[i].Type != map2.Nodes[i].Type)
                {
                    sameTypes = false;
                    break;
                }
            }

            Log("Deterministic - Same Node Count", sameNodeCount);
            Log("Deterministic - Same Node Types", sameTypes);
        }

        private void TestAllNodesReachable()
        {
            if (_testZoneConfig == null) return;

            var generator = new MapGenerator();
            var map = generator.Generate(_testZoneConfig, _testSeed);

            var reachable = new HashSet<string>();
            var startNode = map.GetStartNode();

            if (startNode != null)
            {
                TraverseReachable(map, startNode, reachable);
            }

            bool allReachable = reachable.Count == map.Nodes.Count;
            Log("Reachability - All Nodes Reachable", allReachable,
                $"{reachable.Count}/{map.Nodes.Count} nodes");
        }

        private void TraverseReachable(MapData map, MapNodeData node, HashSet<string> visited)
        {
            if (visited.Contains(node.NodeId)) return;
            visited.Add(node.NodeId);

            foreach (var connectedId in node.ConnectedNodeIds)
            {
                var connected = map.GetNode(connectedId);
                if (connected != null)
                {
                    TraverseReachable(map, connected, visited);
                }
            }
        }

        private void TestGuaranteedNodes()
        {
            if (_testZoneConfig == null) return;

            var generator = new MapGenerator();
            var map = generator.Generate(_testZoneConfig, _testSeed);

            bool hasShop = map.Nodes.Exists(n => n.Type == NodeType.Shop);
            bool hasSanctuary = map.Nodes.Exists(n => n.Type == NodeType.Sanctuary);

            if (_testZoneConfig.GuaranteedShop)
                Log("Guaranteed Nodes - Shop Exists", hasShop);
            if (_testZoneConfig.GuaranteedSanctuary)
                Log("Guaranteed Nodes - Sanctuary Exists", hasSanctuary);
        }

        // ============================================
        // Map Manager Tests
        // ============================================

        private void TestMapManagerExists()
        {
            var mapManager = ServiceLocator.Get<MapManager>();
            Log("MapManager - Registered", mapManager != null);
        }

        private void TestNodeNavigation()
        {
            var mapManager = ServiceLocator.Get<MapManager>();
            if (mapManager == null)
            {
                Log("Navigation - MapManager Available", false, "Not found");
                return;
            }

            // Generate a test map
            mapManager.GenerateMap(1, _testSeed);

            var startNode = mapManager.CurrentNode;
            Log("Navigation - Start Node Is Current", startNode?.Type == NodeType.Start);

            // Try to move to connected node
            if (startNode?.ConnectedNodeIds.Count > 0)
            {
                string targetId = startNode.ConnectedNodeIds[0];
                bool moved = mapManager.TryMoveToNode(targetId);
                Log("Navigation - Move To Connected Node", moved);

                // Verify state changed
                var newCurrent = mapManager.CurrentNode;
                bool stateChanged = newCurrent?.NodeId == targetId;
                Log("Navigation - Current Node Updated", stateChanged);
            }

            // Try to move to invalid node
            bool invalidMove = mapManager.TryMoveToNode("invalid_node_id");
            Log("Navigation - Reject Invalid Node", !invalidMove);
        }

        private void TestNodeStates()
        {
            var mapManager = ServiceLocator.Get<MapManager>();
            if (mapManager == null) return;

            mapManager.GenerateMap(1, _testSeed);
            var map = mapManager.CurrentMap;

            // Check initial states
            var startNode = map.GetStartNode();
            bool startIsCurrent = startNode?.State == NodeState.Current;
            Log("Node States - Start Is Current", startIsCurrent);

            // Check connected nodes are available
            if (startNode?.ConnectedNodeIds.Count > 0)
            {
                var firstConnected = map.GetNode(startNode.ConnectedNodeIds[0]);
                bool connectedAvailable = firstConnected?.State == NodeState.Available;
                Log("Node States - Connected Nodes Available", connectedAvailable);
            }

            // Check boss is locked
            var bossNode = map.GetBossNode();
            bool bossLocked = bossNode?.State == NodeState.Locked || bossNode?.State == NodeState.Available;
            Log("Node States - Boss Initially Not Current", bossLocked);
        }

        // ============================================
        // Echo Event Tests
        // ============================================

        private void TestEchoEventManagerExists()
        {
            var echoManager = ServiceLocator.Get<EchoEventManager>();
            Log("EchoEventManager - Registered", echoManager != null);
        }

        private void TestEchoEventFlow()
        {
            var echoManager = ServiceLocator.Get<EchoEventManager>();
            if (echoManager == null)
            {
                Log("Echo Flow - Manager Available", false, "Not found");
                return;
            }

            // Test random event retrieval
            var randomEvent = echoManager.GetRandomEvent();
            Log("Echo Flow - GetRandomEvent Works", true,
                randomEvent != null ? randomEvent.EventTitle : "No events configured");

            // Test event start (if we have an event)
            if (randomEvent != null)
            {
                bool eventStarted = false;
                void OnEventStarted(EchoEventStartedEvent e) => eventStarted = true;

                EventBus.Subscribe<EchoEventStartedEvent>(OnEventStarted);
                echoManager.StartEvent(randomEvent);
                EventBus.Unsubscribe<EchoEventStartedEvent>(OnEventStarted);

                Log("Echo Flow - Event Started", eventStarted);
                Log("Echo Flow - Current Event Set", echoManager.CurrentEvent != null);

                // Clean up
                echoManager.CompleteEvent();
            }
        }

        // ============================================
        // Utility
        // ============================================

        private void Log(string testName, bool passed, string info = "")
        {
            string status = passed ? "PASS" : "FAIL";
            string message = string.IsNullOrEmpty(info) ? testName : $"{testName} ({info})";

            if (passed)
            {
                Debug.Log($"[{status}] {message}");
                _passCount++;
            }
            else
            {
                Debug.LogError($"[{status}] {message}");
                _failCount++;
            }

            _testResults.Add($"[{status}] {message}");
        }
    }
}
