// ============================================
// MapPathRenderer.cs
// Renders connection paths between map nodes
// ============================================

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace HNR.Map
{
    /// <summary>
    /// Renders visual connections between map nodes.
    /// Uses UI Images stretched and rotated to form paths.
    /// </summary>
    public class MapPathRenderer : MonoBehaviour
    {
        // ============================================
        // Configuration
        // ============================================

        [Header("Prefab & Container")]
        [SerializeField, Tooltip("Image prefab for path segments")]
        private Image _pathPrefab;

        [SerializeField, Tooltip("Container transform for path instances")]
        private Transform _pathContainer;

        [Header("Path Styling")]
        [SerializeField, Tooltip("Width of path lines")]
        private float _pathWidth = 4f;

        [SerializeField, Tooltip("Color for available/current paths")]
        private Color _availableColor = Color.white;

        [SerializeField, Tooltip("Color for already traversed paths")]
        private Color _visitedColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);

        [SerializeField, Tooltip("Color for locked/inaccessible paths")]
        private Color _lockedColor = new Color(0.3f, 0.3f, 0.3f, 0.3f);

        // ============================================
        // Runtime State
        // ============================================

        private readonly List<PathConnection> _paths = new();
        private string _currentNodeId;

        // ============================================
        // Public API
        // ============================================

        /// <summary>
        /// Renders all paths between connected nodes.
        /// </summary>
        /// <param name="mapData">Map data with node connections.</param>
        /// <param name="nodeUIs">Dictionary of node UI components by ID.</param>
        public void RenderPaths(MapData mapData, Dictionary<string, MapNodeUI> nodeUIs)
        {
            ClearPaths();

            if (mapData == null || nodeUIs == null) return;

            _currentNodeId = mapData.CurrentNodeId;

            foreach (var node in mapData.Nodes)
            {
                foreach (var connectedId in node.ConnectedNodeIds)
                {
                    if (nodeUIs.TryGetValue(node.NodeId, out var fromUI) &&
                        nodeUIs.TryGetValue(connectedId, out var toUI))
                    {
                        var toNode = mapData.GetNode(connectedId);
                        CreatePath(fromUI, toUI, node, toNode);
                    }
                }
            }
        }

        /// <summary>
        /// Updates path colors based on current node states.
        /// More efficient than full re-render when only states change.
        /// </summary>
        public void UpdatePathColors(MapData mapData)
        {
            if (mapData == null) return;

            _currentNodeId = mapData.CurrentNodeId;

            foreach (var pathConnection in _paths)
            {
                var fromNode = mapData.GetNode(pathConnection.FromNodeId);
                var toNode = mapData.GetNode(pathConnection.ToNodeId);

                if (fromNode != null && toNode != null && pathConnection.PathImage != null)
                {
                    pathConnection.PathImage.color = GetPathColor(fromNode, toNode);
                }
            }
        }

        /// <summary>
        /// Clears all rendered paths.
        /// </summary>
        public void ClearPaths()
        {
            foreach (var pathConnection in _paths)
            {
                if (pathConnection.PathImage != null)
                    Destroy(pathConnection.PathImage.gameObject);
            }
            _paths.Clear();
        }

        // ============================================
        // Path Creation
        // ============================================

        private void CreatePath(MapNodeUI fromUI, MapNodeUI toUI, MapNodeData fromNode, MapNodeData toNode)
        {
            if (_pathPrefab == null)
            {
                Debug.LogWarning("[MapPathRenderer] Path prefab is null");
                return;
            }

            var container = _pathContainer != null ? _pathContainer : transform;
            var pathImage = Instantiate(_pathPrefab, container);

            // Calculate positions
            Vector2 from = fromUI.transform.localPosition;
            Vector2 to = toUI.transform.localPosition;
            Vector2 direction = to - from;
            Vector2 midpoint = (from + to) / 2f;

            // Position at midpoint
            pathImage.rectTransform.localPosition = new Vector3(midpoint.x, midpoint.y, 0f);

            // Rotate to face target
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            pathImage.rectTransform.localRotation = Quaternion.Euler(0f, 0f, angle);

            // Set length (x = distance, y = width)
            float distance = direction.magnitude;
            pathImage.rectTransform.sizeDelta = new Vector2(distance, _pathWidth);

            // Set pivot to center for proper rotation
            pathImage.rectTransform.pivot = new Vector2(0.5f, 0.5f);

            // Set color based on node states
            pathImage.color = GetPathColor(fromNode, toNode);

            // Store reference for later updates
            _paths.Add(new PathConnection
            {
                PathImage = pathImage,
                FromNodeId = fromNode.NodeId,
                ToNodeId = toNode.NodeId
            });
        }

        // ============================================
        // Color Logic
        // ============================================

        private Color GetPathColor(MapNodeData from, MapNodeData to)
        {
            // Check if 'from' is the player's current position (by ID, not just state)
            // This handles the case where player completed a node (state=Visited) but is still there
            bool fromIsCurrentPosition = from.NodeId == _currentNodeId;

            // Both visited = traveled path (player took this route)
            if (from.State == NodeState.Visited && to.State == NodeState.Visited)
                return _visitedColor;

            // Path to current position from visited = traveled path
            if (from.State == NodeState.Visited && to.State == NodeState.Current)
                return _visitedColor;

            // Path from current position (state=Current) to available = active path
            if (from.State == NodeState.Current && to.State == NodeState.Available)
                return _availableColor;

            // Path from current position (state=Visited but player is here) to available = active path
            // This handles the case after clearing a node - player is still there but state is Visited
            if (fromIsCurrentPosition && to.State == NodeState.Available)
                return _availableColor;

            // Default = locked (includes paths from other nodes to available nodes)
            return _lockedColor;
        }

        // ============================================
        // Helper Types
        // ============================================

        /// <summary>
        /// Stores path instance with node references for updates.
        /// </summary>
        private class PathConnection
        {
            public Image PathImage;
            public string FromNodeId;
            public string ToNodeId;
        }
    }
}
