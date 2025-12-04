// ============================================
// MapNodeUI.cs
// Visual representation of a map node
// ============================================

using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

namespace HNR.Map
{
    /// <summary>
    /// UI component for displaying and interacting with map nodes.
    /// Handles icon display, state coloring, and click/hover events.
    /// </summary>
    public class MapNodeUI : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
    {
        // ============================================
        // References
        // ============================================

        [Header("UI References")]
        [SerializeField, Tooltip("Icon displaying node type")]
        private Image _nodeIcon;

        [SerializeField, Tooltip("Background/frame image")]
        private Image _background;

        [SerializeField, Tooltip("Highlight ring for hover state")]
        private GameObject _highlightRing;

        [SerializeField, Tooltip("Indicator for current node")]
        private GameObject _currentIndicator;

        [SerializeField, Tooltip("Optional type label")]
        private TextMeshProUGUI _typeLabel;

        // ============================================
        // Node Type Icons
        // ============================================

        [Header("Node Type Icons")]
        [SerializeField] private Sprite _startIcon;
        [SerializeField] private Sprite _combatIcon;
        [SerializeField] private Sprite _eliteIcon;
        [SerializeField] private Sprite _shopIcon;
        [SerializeField] private Sprite _echoIcon;
        [SerializeField] private Sprite _sanctuaryIcon;
        [SerializeField] private Sprite _treasureIcon;
        [SerializeField] private Sprite _bossIcon;

        // ============================================
        // State Colors
        // ============================================

        [Header("State Colors")]
        [SerializeField, Tooltip("Color when node is locked")]
        private Color _lockedColor = new Color(0.3f, 0.3f, 0.3f, 1f);

        [SerializeField, Tooltip("Color when node is available")]
        private Color _availableColor = Color.white;

        [SerializeField, Tooltip("Color when node is current")]
        private Color _currentColor = new Color(1f, 0.84f, 0f, 1f); // Gold

        [SerializeField, Tooltip("Color when node is visited")]
        private Color _visitedColor = new Color(0.5f, 0.5f, 0.5f, 1f);

        // ============================================
        // Runtime State
        // ============================================

        private MapNodeData _nodeData;
        private Action<MapNodeData> _onClick;
        private bool _isInteractable;

        // ============================================
        // Public Properties
        // ============================================

        /// <summary>The node data this UI represents.</summary>
        public MapNodeData NodeData => _nodeData;

        /// <summary>Whether this node can be clicked.</summary>
        public bool IsInteractable => _isInteractable;

        // ============================================
        // Initialization
        // ============================================

        /// <summary>
        /// Initializes the node UI with data and click callback.
        /// </summary>
        /// <param name="nodeData">Node data to display.</param>
        /// <param name="onClick">Callback when node is clicked.</param>
        public void Initialize(MapNodeData nodeData, Action<MapNodeData> onClick)
        {
            _nodeData = nodeData;
            _onClick = onClick;

            // Set position
            transform.localPosition = new Vector3(nodeData.Position.x, nodeData.Position.y, 0f);

            // Update visuals
            UpdateVisuals();
        }

        // ============================================
        // Visual Updates
        // ============================================

        /// <summary>
        /// Updates all visual elements based on current node state.
        /// </summary>
        public void UpdateVisuals()
        {
            if (_nodeData == null) return;

            // Update icon
            UpdateIcon();

            // Update background color
            UpdateStateColor();

            // Update indicators
            UpdateIndicators();

            // Update interactability
            _isInteractable = _nodeData.State == NodeState.Available;
        }

        private void UpdateIcon()
        {
            if (_nodeIcon == null) return;

            _nodeIcon.sprite = GetIconForType(_nodeData.Type);

            // Dim icon for locked/visited nodes
            var iconColor = _nodeIcon.color;
            iconColor.a = (_nodeData.State == NodeState.Locked) ? 0.5f : 1f;
            _nodeIcon.color = iconColor;
        }

        private void UpdateStateColor()
        {
            if (_background == null) return;

            _background.color = GetColorForState(_nodeData.State);
        }

        private void UpdateIndicators()
        {
            // Current node indicator
            if (_currentIndicator != null)
                _currentIndicator.SetActive(_nodeData.State == NodeState.Current);

            // Hide highlight by default
            if (_highlightRing != null)
                _highlightRing.SetActive(false);

            // Type label
            if (_typeLabel != null)
                _typeLabel.text = GetNodeTypeLabel(_nodeData.Type);
        }

        // ============================================
        // Icon/Color Mapping
        // ============================================

        private Sprite GetIconForType(NodeType type)
        {
            return type switch
            {
                NodeType.Start => _startIcon,
                NodeType.Combat => _combatIcon,
                NodeType.Elite => _eliteIcon,
                NodeType.Shop => _shopIcon,
                NodeType.Echo => _echoIcon,
                NodeType.Sanctuary => _sanctuaryIcon,
                NodeType.Treasure => _treasureIcon,
                NodeType.Boss => _bossIcon,
                _ => _combatIcon
            };
        }

        private Color GetColorForState(NodeState state)
        {
            return state switch
            {
                NodeState.Locked => _lockedColor,
                NodeState.Available => _availableColor,
                NodeState.Current => _currentColor,
                NodeState.Visited => _visitedColor,
                _ => _lockedColor
            };
        }

        private string GetNodeTypeLabel(NodeType type)
        {
            return type switch
            {
                NodeType.Start => "Start",
                NodeType.Combat => "Combat",
                NodeType.Elite => "Elite",
                NodeType.Shop => "Shop",
                NodeType.Echo => "Echo",
                NodeType.Sanctuary => "Rest",
                NodeType.Treasure => "Treasure",
                NodeType.Boss => "Boss",
                _ => ""
            };
        }

        // ============================================
        // Pointer Events
        // ============================================

        public void OnPointerClick(PointerEventData eventData)
        {
            if (!_isInteractable) return;

            _onClick?.Invoke(_nodeData);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (!_isInteractable) return;

            // Show highlight
            if (_highlightRing != null)
                _highlightRing.SetActive(true);

            // Scale up slightly
            transform.localScale = Vector3.one * 1.1f;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            // Hide highlight
            if (_highlightRing != null)
                _highlightRing.SetActive(false);

            // Reset scale
            transform.localScale = Vector3.one;
        }

        // ============================================
        // Public Methods
        // ============================================

        /// <summary>
        /// Sets the node as the current node (player location).
        /// </summary>
        public void SetAsCurrent()
        {
            if (_nodeData != null)
                _nodeData.State = NodeState.Current;

            UpdateVisuals();
        }

        /// <summary>
        /// Sets the node as visited.
        /// </summary>
        public void SetAsVisited()
        {
            if (_nodeData != null)
                _nodeData.State = NodeState.Visited;

            UpdateVisuals();
        }

        /// <summary>
        /// Sets the node as available for travel.
        /// </summary>
        public void SetAsAvailable()
        {
            if (_nodeData != null)
                _nodeData.State = NodeState.Available;

            UpdateVisuals();
        }
    }
}
