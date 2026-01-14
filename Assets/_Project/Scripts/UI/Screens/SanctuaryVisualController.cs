// ============================================
// SanctuaryVisualController.cs
// Manages world-space Requiem visuals for Sanctuary screen
// Uses same approach as Combat scene - world-space positioning
// ============================================

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using HNR.Core;
using HNR.Core.Interfaces;
using HNR.Characters;

namespace HNR.UI
{
    /// <summary>
    /// Controls the positioning and visibility of Requiem visuals in the Sanctuary screen.
    /// Uses world-space positioning similar to Combat scene approach.
    /// The UI canvas is transparent, allowing world-space objects to show through.
    /// </summary>
    public class SanctuaryVisualController : MonoBehaviour
    {
        // ============================================
        // Serialized Fields
        // ============================================

        [Header("Zone Background (to hide when Sanctuary shows)")]
        [SerializeField, Tooltip("Reference to the ZoneBackground Image - hidden when Sanctuary shows")]
        private Image _zoneBackground;

        [Header("World-Space Background")]
        [SerializeField, Tooltip("World-space background SpriteRenderer for Sanctuary")]
        private SpriteRenderer _worldBackground;

        [Header("Visual Slots (World-Space Positions)")]
        [SerializeField, Tooltip("Left position slot for Requiem visual")]
        private Transform _leftSlot;

        [SerializeField, Tooltip("Center position slot for Requiem visual")]
        private Transform _centerSlot;

        [SerializeField, Tooltip("Right position slot for Requiem visual")]
        private Transform _rightSlot;

        [Header("Settings")]
        [SerializeField, Tooltip("Scale for Requiem visuals in Sanctuary")]
        private float _visualScale = 2.0f;

        [SerializeField, Tooltip("Sorting order for Requiem sprites (higher = in front)")]
        private int _requiemSortingOrder = 10;

        [SerializeField, Tooltip("Y-axis rotation for right Requiem (to face toward center/behind)")]
        private float _rightRequiemYRotation = 180f;

        // ============================================
        // Private Fields
        // ============================================

        private List<RequiemInstance> _activeVisuals = new();
        private Dictionary<RequiemInstance, Vector3> _originalPositions = new();
        private Dictionary<RequiemInstance, Vector3> _originalScales = new();
        private Dictionary<RequiemInstance, Quaternion> _originalRotations = new();
        private Dictionary<RequiemInstance, int> _originalSortingOrders = new();
        private bool _isShowing;

        // ============================================
        // Properties
        // ============================================

        /// <summary>Whether visuals are currently being displayed.</summary>
        public bool IsShowing => _isShowing;

        // ============================================
        // Unity Lifecycle
        // ============================================

        private void Awake()
        {
            // Initially hide world background
            if (_worldBackground != null)
            {
                _worldBackground.gameObject.SetActive(false);
            }
        }

        private void OnDisable()
        {
            // Ensure visuals are hidden if controller is disabled
            if (_isShowing)
            {
                HideVisuals();
            }
        }

        // ============================================
        // Public Methods
        // ============================================

        /// <summary>
        /// Shows the team's Requiem visuals at the sanctuary slots.
        /// </summary>
        public void ShowVisuals()
        {
            if (_isShowing) return;

            var runManager = ServiceLocator.Get<IRunManager>();
            if (runManager == null || !runManager.IsRunActive)
            {
                Debug.LogWarning("[SanctuaryVisualController] No active run, cannot show visuals");
                return;
            }

            var team = runManager.Team;
            if (team == null || team.Count == 0)
            {
                Debug.LogWarning("[SanctuaryVisualController] Team is empty");
                return;
            }

            // Hide zone background so Sanctuary background can show
            if (_zoneBackground != null)
            {
                _zoneBackground.gameObject.SetActive(false);
            }

            // Show world background
            if (_worldBackground != null)
            {
                _worldBackground.gameObject.SetActive(true);
            }

            // Get slot positions
            Transform[] slots = GetSlotArray();

            // Store original states and position visuals
            _activeVisuals.Clear();
            _originalPositions.Clear();
            _originalScales.Clear();
            _originalRotations.Clear();
            _originalSortingOrders.Clear();

            for (int i = 0; i < team.Count && i < slots.Length; i++)
            {
                var requiem = team[i];
                if (requiem == null || requiem.gameObject == null) continue;

                // Store original state
                _originalPositions[requiem] = requiem.transform.position;
                _originalScales[requiem] = requiem.transform.localScale;
                _originalRotations[requiem] = requiem.transform.rotation;

                // Store and update sprite sorting order
                var spriteRenderer = requiem.GetComponentInChildren<SpriteRenderer>();
                if (spriteRenderer != null)
                {
                    _originalSortingOrders[requiem] = spriteRenderer.sortingOrder;
                    spriteRenderer.sortingOrder = _requiemSortingOrder;
                }

                // Position at slot
                if (slots[i] != null)
                {
                    requiem.transform.position = slots[i].position;
                }

                // Apply scale
                requiem.transform.localScale = Vector3.one * _visualScale;

                // Apply rotation for right-most Requiem (index 2) to face toward center
                bool isRightSlot = (i == slots.Length - 1 && slots.Length >= 3);
                if (isRightSlot)
                {
                    requiem.transform.rotation = Quaternion.Euler(0f, _rightRequiemYRotation, 0f);
                }

                // Enable the visual GameObject
                requiem.gameObject.SetActive(true);

                _activeVisuals.Add(requiem);

                Debug.Log($"[SanctuaryVisualController] Positioned {requiem.Name} at slot {i} ({slots[i]?.position}){(isRightSlot ? " [rotated]" : "")}");
            }

            _isShowing = true;
            Debug.Log($"[SanctuaryVisualController] Showing {_activeVisuals.Count} Requiem visuals (world-space)");
        }

        /// <summary>
        /// Hides the team's Requiem visuals and restores original positions.
        /// </summary>
        public void HideVisuals()
        {
            if (!_isShowing) return;

            // Hide world background
            if (_worldBackground != null)
            {
                _worldBackground.gameObject.SetActive(false);
            }

            // Restore zone background
            if (_zoneBackground != null)
            {
                _zoneBackground.gameObject.SetActive(true);
            }

            foreach (var requiem in _activeVisuals)
            {
                if (requiem == null || requiem.gameObject == null) continue;

                // Restore original position
                if (_originalPositions.TryGetValue(requiem, out var originalPos))
                {
                    requiem.transform.position = originalPos;
                }

                // Restore original scale
                if (_originalScales.TryGetValue(requiem, out var originalScale))
                {
                    requiem.transform.localScale = originalScale;
                }

                // Restore original rotation
                if (_originalRotations.TryGetValue(requiem, out var originalRotation))
                {
                    requiem.transform.rotation = originalRotation;
                }

                // Restore sprite sorting order
                var spriteRenderer = requiem.GetComponentInChildren<SpriteRenderer>();
                if (spriteRenderer != null && _originalSortingOrders.TryGetValue(requiem, out var originalOrder))
                {
                    spriteRenderer.sortingOrder = originalOrder;
                }
            }

            _activeVisuals.Clear();
            _originalPositions.Clear();
            _originalScales.Clear();
            _originalRotations.Clear();
            _originalSortingOrders.Clear();
            _isShowing = false;

            Debug.Log("[SanctuaryVisualController] Requiem visuals hidden");
        }

        // ============================================
        // Private Methods
        // ============================================

        /// <summary>
        /// Gets the slot transforms as an array, handling null slots.
        /// </summary>
        private Transform[] GetSlotArray()
        {
            var slots = new List<Transform>();

            if (_leftSlot != null) slots.Add(_leftSlot);
            if (_centerSlot != null) slots.Add(_centerSlot);
            if (_rightSlot != null) slots.Add(_rightSlot);

            if (slots.Count == 0)
            {
                Debug.LogWarning("[SanctuaryVisualController] No slots configured");
            }

            return slots.ToArray();
        }
    }
}
