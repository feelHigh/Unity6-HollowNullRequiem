// ============================================
// TargetingSystem.cs
// Handles card target selection during combat
// ============================================

using System.Collections.Generic;
using UnityEngine;
using HNR.Core;
using HNR.Core.Events;
using HNR.Cards;

namespace HNR.Combat
{
    /// <summary>
    /// Manages target selection for cards during combat.
    /// Handles mouse-based targeting with visual feedback.
    /// </summary>
    public class TargetingSystem : MonoBehaviour
    {
        // ============================================
        // Configuration
        // ============================================

        [Header("Visual Feedback")]
        [SerializeField, Tooltip("Line renderer for targeting arrow")]
        private LineRenderer _targetingLine;

        [SerializeField, Tooltip("Color when hovering valid target")]
        private Color _validTargetColor = Color.green;

        [SerializeField, Tooltip("Color when no valid target")]
        private Color _invalidTargetColor = Color.red;

        [Header("Targeting Settings")]
        [SerializeField, Tooltip("Layer mask for target detection")]
        private LayerMask _targetLayerMask = -1;

        [SerializeField, Tooltip("Origin point for targeting line")]
        private Transform _targetingOrigin;

        // ============================================
        // Runtime State
        // ============================================

        private CardInstance _sourceCard;
        private List<ICombatTarget> _validTargets = new();
        private ICombatTarget _hoveredTarget;
        private bool _isTargeting;

        // ============================================
        // Properties
        // ============================================

        /// <summary>True when actively selecting a target.</summary>
        public bool IsTargeting => _isTargeting;

        /// <summary>Currently hovered valid target (null if none).</summary>
        public ICombatTarget HoveredTarget => _hoveredTarget;

        /// <summary>The card being targeted.</summary>
        public CardInstance SourceCard => _sourceCard;

        /// <summary>List of valid targets for current card.</summary>
        public IReadOnlyList<ICombatTarget> ValidTargets => _validTargets;

        // ============================================
        // Unity Lifecycle
        // ============================================

        private void Awake()
        {
            ServiceLocator.Register(this);

            if (_targetingOrigin == null)
            {
                _targetingOrigin = transform;
            }
        }

        private void OnDestroy()
        {
            ServiceLocator.Unregister<TargetingSystem>();
        }

        private void Update()
        {
            if (!_isTargeting) return;

            UpdateTargetingLine();
            CheckHoveredTarget();
            CheckForTargetConfirmation();
        }

        /// <summary>
        /// Check for mouse click to confirm target selection.
        /// </summary>
        private void CheckForTargetConfirmation()
        {
            // Left mouse button click confirms target
            if (Input.GetMouseButtonDown(0) && _hoveredTarget != null)
            {
                Debug.Log($"[TargetingSystem] Click detected on {_hoveredTarget.Name}, confirming target");
                ConfirmTarget();
            }
            // Right mouse button cancels targeting
            else if (Input.GetMouseButtonDown(1))
            {
                Debug.Log("[TargetingSystem] Right-click detected, cancelling targeting");
                CancelTargeting();
            }
        }

        // ============================================
        // Public API
        // ============================================

        /// <summary>
        /// Begin targeting mode for a card.
        /// </summary>
        /// <param name="card">Card requiring target selection</param>
        public void BeginTargeting(CardInstance card)
        {
            if (card == null)
            {
                Debug.LogWarning("[TargetingSystem] Cannot begin targeting with null card");
                return;
            }

            _sourceCard = card;
            _isTargeting = true;
            _validTargets = GetValidTargets(card.Data.TargetType);

            // Highlight all valid targets
            Debug.Log($"[TargetingSystem] Highlighting {_validTargets.Count} valid targets");
            foreach (var target in _validTargets)
            {
                Debug.Log($"[TargetingSystem] Calling ShowTargetHighlight(true) on {target.Name} (type: {target.GetType().Name})");
                target.ShowTargetHighlight(true);
            }

            // Enable targeting line
            if (_targetingLine != null)
            {
                _targetingLine.enabled = true;
                _targetingLine.startColor = _invalidTargetColor;
                _targetingLine.endColor = _invalidTargetColor;
            }

            Debug.Log($"[TargetingSystem] Begin targeting for: {card.Data.CardName} ({_validTargets.Count} valid targets)");
        }

        /// <summary>
        /// Cancel current targeting and clear state.
        /// </summary>
        /// <param name="publishEvent">Whether to publish the cancelled event</param>
        public void CancelTargeting(bool publishEvent = true)
        {
            // Remove highlights from all targets
            foreach (var target in _validTargets)
            {
                target.ShowTargetHighlight(false);
            }

            // Clear state
            _isTargeting = false;
            _sourceCard = null;
            _validTargets.Clear();
            _hoveredTarget = null;

            // Disable targeting line
            if (_targetingLine != null)
            {
                _targetingLine.enabled = false;
            }

            // Publish cancel event
            if (publishEvent)
            {
                EventBus.Publish(new CardTargetCancelledEvent());
            }

            Debug.Log("[TargetingSystem] Targeting cancelled");
        }

        /// <summary>
        /// Confirm the currently hovered target.
        /// </summary>
        /// <returns>Selected target, or null if no valid target hovered</returns>
        public ICombatTarget ConfirmTarget()
        {
            if (!_isTargeting)
            {
                Debug.LogWarning("[TargetingSystem] Cannot confirm - not in targeting mode");
                return null;
            }

            if (_hoveredTarget == null)
            {
                Debug.LogWarning("[TargetingSystem] Cannot confirm - no target selected");
                return null;
            }

            var selected = _hoveredTarget;

            // Clear targeting state without publishing cancel event
            CancelTargeting(publishEvent: false);

            // Publish confirmed event
            EventBus.Publish(new CardTargetConfirmedEvent(selected));

            Debug.Log($"[TargetingSystem] Target confirmed: {selected.Name}");
            return selected;
        }

        /// <summary>
        /// Check if a target type requires manual selection.
        /// </summary>
        /// <param name="targetType">The target type to check</param>
        /// <returns>True if player must select a target</returns>
        public bool RequiresTargeting(TargetType targetType)
        {
            return targetType == TargetType.SingleEnemy ||
                   targetType == TargetType.SingleAlly;
        }

        /// <summary>
        /// Get all targets for auto-target cards (AllEnemies, AllAllies).
        /// </summary>
        /// <param name="targetType">The target type</param>
        /// <returns>List of all valid targets</returns>
        public List<ICombatTarget> GetAllTargets(TargetType targetType)
        {
            return GetValidTargets(targetType);
        }

        // ============================================
        // Private Methods
        // ============================================

        private void UpdateTargetingLine()
        {
            if (_targetingLine == null || Camera.main == null) return;

            // Get mouse position in world space
            Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mouseWorldPos.z = 0;

            // Set line positions
            Vector3 origin = _targetingOrigin != null ? _targetingOrigin.position : transform.position;
            origin.z = 0;

            _targetingLine.SetPosition(0, origin);
            _targetingLine.SetPosition(1, mouseWorldPos);

            // Update color based on hover state
            Color lineColor = _hoveredTarget != null ? _validTargetColor : _invalidTargetColor;
            _targetingLine.startColor = lineColor;
            _targetingLine.endColor = lineColor;
        }

        private void CheckHoveredTarget()
        {
            if (Camera.main == null) return;

            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

            // Raycast to find targets
            RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero, 0f, _targetLayerMask);

            if (hit.collider != null)
            {
                // Try to get ICombatTarget from hit object
                var target = hit.collider.GetComponent<ICombatTarget>();

                if (target != null && _validTargets.Contains(target))
                {
                    if (_hoveredTarget != target)
                    {
                        _hoveredTarget = target;
                        Debug.Log($"[TargetingSystem] Hovering: {target.Name}");
                    }
                    return;
                }
            }

            // No valid target under cursor
            _hoveredTarget = null;
        }

        private List<ICombatTarget> GetValidTargets(TargetType targetType)
        {
            var targets = new List<ICombatTarget>();

            if (!ServiceLocator.TryGet<TurnManager>(out var turnManager))
            {
                Debug.LogWarning("[TargetingSystem] TurnManager not available");
                return targets;
            }

            var context = turnManager.Context;
            if (context == null)
            {
                Debug.LogWarning("[TargetingSystem] No combat context");
                return targets;
            }

            switch (targetType)
            {
                case TargetType.None:
                    // No targets needed
                    break;

                case TargetType.SingleEnemy:
                case TargetType.AllEnemies:
                    // Get all living enemies
                    foreach (var enemy in context.Enemies)
                    {
                        if (!enemy.IsDead)
                        {
                            // Note: EnemyInstance needs to implement ICombatTarget
                            // For now, check if it's a MonoBehaviour with the interface
                            if (enemy is ICombatTarget combatTarget)
                            {
                                targets.Add(combatTarget);
                            }
                        }
                    }
                    break;

                case TargetType.SingleAlly:
                case TargetType.AllAllies:
                    // Get all living allies
                    foreach (var requiem in context.Team)
                    {
                        if (!requiem.IsDead)
                        {
                            if (requiem is ICombatTarget combatTarget)
                            {
                                targets.Add(combatTarget);
                            }
                        }
                    }
                    break;

                case TargetType.Self:
                    // TODO: Get the casting Requiem
                    break;

                case TargetType.Random:
                    // Get all valid targets, selection happens at play time
                    foreach (var enemy in context.Enemies)
                    {
                        if (!enemy.IsDead && enemy is ICombatTarget combatTarget)
                        {
                            targets.Add(combatTarget);
                        }
                    }
                    break;
            }

            return targets;
        }

        // ============================================
        // Debug
        // ============================================

        /// <summary>
        /// Get debug info for UI display.
        /// </summary>
        public string GetDebugInfo()
        {
            if (!_isTargeting)
            {
                return "Not targeting";
            }

            return $"Targeting: {_sourceCard?.Data?.CardName ?? "Unknown"}\n" +
                   $"Valid targets: {_validTargets.Count}\n" +
                   $"Hovered: {_hoveredTarget?.Name ?? "None"}";
        }
    }
}
