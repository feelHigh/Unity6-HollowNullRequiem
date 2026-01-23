// ============================================
// BannerDragHandler.cs
// Touch/drag interaction handler for event banner carousel
// ============================================

using UnityEngine;
using UnityEngine.EventSystems;
using System;

namespace HNR.UI.Components
{
    /// <summary>
    /// Handles drag interactions for the event banner carousel.
    /// Notifies the carousel when drag starts/ends for auto-advance pause/resume.
    /// Also detects taps for banner action triggering.
    /// </summary>
    public class BannerDragHandler : MonoBehaviour,
        IBeginDragHandler, IDragHandler, IEndDragHandler,
        IPointerDownHandler, IPointerUpHandler
    {
        // ============================================
        // Events
        // ============================================

        /// <summary>Fired when drag begins</summary>
        public event Action OnDragBegin;

        /// <summary>Fired when drag ends</summary>
        public event Action OnDragEnd;

        /// <summary>Fired when banner is tapped (not dragged)</summary>
        public event Action OnTap;

        // ============================================
        // Configuration
        // ============================================

        [Header("Tap Detection")]
        [SerializeField, Tooltip("Maximum movement distance for tap detection")]
        private float _tapThreshold = 10f;

        [SerializeField, Tooltip("Maximum duration for tap detection (seconds)")]
        private float _tapMaxDuration = 0.3f;

        // ============================================
        // State
        // ============================================

        private Vector2 _pointerDownPosition;
        private float _pointerDownTime;
        private bool _isDragging;
        private bool _wasPointerDown;

        // ============================================
        // Public State
        // ============================================

        /// <summary>Whether a drag is currently in progress</summary>
        public bool IsDragging => _isDragging;

        // ============================================
        // Drag Handlers
        // ============================================

        public void OnBeginDrag(PointerEventData eventData)
        {
            _isDragging = true;
            OnDragBegin?.Invoke();
        }

        public void OnDrag(PointerEventData eventData)
        {
            // Drag is handled by ScrollRect
            // This handler just tracks state
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            _isDragging = false;
            OnDragEnd?.Invoke();
        }

        // ============================================
        // Pointer Handlers (for tap detection)
        // ============================================

        public void OnPointerDown(PointerEventData eventData)
        {
            _pointerDownPosition = eventData.position;
            _pointerDownTime = Time.unscaledTime;
            _wasPointerDown = true;

            // Also notify drag begin for auto-advance pause
            OnDragBegin?.Invoke();
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (!_wasPointerDown) return;
            _wasPointerDown = false;

            // If we're dragging, let OnEndDrag handle it
            if (_isDragging) return;

            // Check for tap
            float distance = Vector2.Distance(_pointerDownPosition, eventData.position);
            float duration = Time.unscaledTime - _pointerDownTime;

            if (distance <= _tapThreshold && duration <= _tapMaxDuration)
            {
                OnTap?.Invoke();
            }

            // Notify drag end for auto-advance resume
            OnDragEnd?.Invoke();
        }

        // ============================================
        // Manual Control
        // ============================================

        /// <summary>
        /// Manually triggers drag begin notification.
        /// </summary>
        public void NotifyDragBegin()
        {
            OnDragBegin?.Invoke();
        }

        /// <summary>
        /// Manually triggers drag end notification.
        /// </summary>
        public void NotifyDragEnd()
        {
            OnDragEnd?.Invoke();
        }

        /// <summary>
        /// Clears all event subscriptions.
        /// </summary>
        public void ClearEvents()
        {
            OnDragBegin = null;
            OnDragEnd = null;
            OnTap = null;
        }
    }
}
