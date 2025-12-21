// ============================================
// ToastManager.cs
// System feedback notification manager
// ============================================

using System.Collections.Generic;
using UnityEngine;
using HNR.Core;

#pragma warning disable CS0414 // Field is assigned but never used (reserved for future Inspector configuration)

namespace HNR.UI.Toast
{
    /// <summary>
    /// Manages toast notifications for system feedback.
    /// Singleton accessible via ToastManager.Instance.
    /// </summary>
    public class ToastManager : MonoBehaviour
    {
        // ============================================
        // Singleton
        // ============================================

        private static ToastManager _instance;
        public static ToastManager Instance => _instance;

        // ============================================
        // Configuration
        // ============================================

        [Header("Prefab")]
        [SerializeField] private GameObject _toastPrefab;
        [SerializeField] private Transform _toastContainer;

        [Header("Timing")]
        [SerializeField] private float _displayDuration = 2f;
        [SerializeField] private float _fadeInDuration = 0.2f;
        [SerializeField] private float _fadeOutDuration = 0.3f;

        [Header("Layout")]
        [SerializeField] private int _maxVisibleToasts = 3;
        [SerializeField] private float _toastSpacing = 10f;
        [SerializeField] private ToastPosition _position = ToastPosition.TopCenter;

        // ============================================
        // Runtime State
        // ============================================

        private Queue<ToastData> _pendingToasts = new();
        private List<ToastController> _activeToasts = new();

        // ============================================
        // Unity Lifecycle
        // ============================================

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            ServiceLocator.Register(this);
        }

        private void OnDestroy()
        {
            if (_instance == this)
            {
                ServiceLocator.Unregister<ToastManager>();
                _instance = null;
            }
        }

        private void Update()
        {
            // Process pending toasts if space available
            while (_pendingToasts.Count > 0 && _activeToasts.Count < _maxVisibleToasts)
            {
                var data = _pendingToasts.Dequeue();
                SpawnToast(data);
            }

            // Clean up dismissed toasts
            _activeToasts.RemoveAll(t => t == null || t.IsDismissed);
        }

        // ============================================
        // Public API
        // ============================================

        /// <summary>
        /// Show a toast notification.
        /// </summary>
        /// <param name="message">Message to display.</param>
        /// <param name="type">Toast type (affects styling).</param>
        public void ShowToast(string message, ToastType type = ToastType.Info)
        {
            ShowToast(message, type, _displayDuration);
        }

        /// <summary>
        /// Show a toast notification with custom duration.
        /// </summary>
        /// <param name="message">Message to display.</param>
        /// <param name="type">Toast type (affects styling).</param>
        /// <param name="duration">Display duration in seconds.</param>
        public void ShowToast(string message, ToastType type, float duration)
        {
            var data = new ToastData
            {
                Message = message,
                Type = type,
                Duration = duration
            };

            _pendingToasts.Enqueue(data);
        }

        /// <summary>
        /// Show an info toast.
        /// </summary>
        public void ShowInfo(string message) => ShowToast(message, ToastType.Info);

        /// <summary>
        /// Show a success toast.
        /// </summary>
        public void ShowSuccess(string message) => ShowToast(message, ToastType.Success);

        /// <summary>
        /// Show a warning toast.
        /// </summary>
        public void ShowWarning(string message) => ShowToast(message, ToastType.Warning);

        /// <summary>
        /// Show an error toast.
        /// </summary>
        public void ShowError(string message) => ShowToast(message, ToastType.Error);

        /// <summary>
        /// Dismiss all active toasts.
        /// </summary>
        public void DismissAll()
        {
            foreach (var toast in _activeToasts)
            {
                if (toast != null)
                {
                    toast.Dismiss();
                }
            }
            _activeToasts.Clear();
            _pendingToasts.Clear();
        }

        // ============================================
        // Toast Spawning
        // ============================================

        private void SpawnToast(ToastData data)
        {
            if (_toastPrefab == null || _toastContainer == null)
            {
                Debug.LogWarning("[ToastManager] Toast prefab or container not set");
                return;
            }

            var toastObj = Instantiate(_toastPrefab, _toastContainer);
            var controller = toastObj.GetComponent<ToastController>();

            if (controller != null)
            {
                controller.Initialize(data.Message, data.Type, data.Duration, _fadeInDuration, _fadeOutDuration);
                controller.OnDismissed += () => OnToastDismissed(controller);
                _activeToasts.Add(controller);
            }
            else
            {
                Debug.LogWarning("[ToastManager] Toast prefab missing ToastController component");
                Destroy(toastObj);
            }
        }

        private void OnToastDismissed(ToastController toast)
        {
            _activeToasts.Remove(toast);
        }

        // ============================================
        // Supporting Types
        // ============================================

        private struct ToastData
        {
            public string Message;
            public ToastType Type;
            public float Duration;
        }
    }

    /// <summary>
    /// Types of toast notifications.
    /// </summary>
    public enum ToastType
    {
        /// <summary>Neutral information.</summary>
        Info,
        /// <summary>Positive confirmation.</summary>
        Success,
        /// <summary>Cautionary notice.</summary>
        Warning,
        /// <summary>Error or failure.</summary>
        Error
    }

    /// <summary>
    /// Position for toast display.
    /// </summary>
    public enum ToastPosition
    {
        TopCenter,
        TopLeft,
        TopRight,
        BottomCenter,
        BottomLeft,
        BottomRight
    }
}
