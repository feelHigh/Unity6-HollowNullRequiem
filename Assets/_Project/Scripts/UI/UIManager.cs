// ============================================
// UIManager.cs
// Central UI management service
// ============================================

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using HNR.Core;
using HNR.Core.Interfaces;

namespace HNR.UI
{
    /// <summary>
    /// Central UI management service. Handles screen transitions and overlays.
    /// Persists across scene loads via DontDestroyOnLoad.
    /// </summary>
    /// <remarks>
    /// Requires DOTween for fade transitions.
    /// If DOTween is not available, transitions will be instant.
    /// </remarks>
    public class UIManager : MonoBehaviour, IUIManager
    {
        // ============================================
        // Serialized Fields
        // ============================================

        [Header("Canvas References")]
        [SerializeField, Tooltip("Container for main screens")]
        private Transform _screenContainer;

        [SerializeField, Tooltip("Container for overlay screens")]
        private Transform _overlayContainer;

        [Header("Global UI")]
        [SerializeField, Tooltip("Global header bar (team status, resources)")]
        private GameObject _globalHeader;

        [SerializeField, Tooltip("Global navigation dock (bottom nav)")]
        private GameObject _globalNavDock;

        [Header("Transition")]
        [SerializeField, Tooltip("Duration of fade transitions")]
        private float _transitionDuration = 0.3f;

        [SerializeField, Tooltip("Canvas group for fade overlay")]
        private CanvasGroup _fadeOverlay;

        // ============================================
        // Private Fields
        // ============================================

        private Dictionary<Type, ScreenBase> _screens = new();
        private Stack<ScreenBase> _overlayStack = new();
        private ScreenBase _currentScreen;
        private bool _isTransitioning;
        private bool _isRegisteredInstance;

        // ============================================
        // Properties
        // ============================================

        /// <summary>Currently active main screen.</summary>
        public ScreenBase CurrentScreen => _currentScreen;

        /// <summary>Whether a screen transition is in progress.</summary>
        public bool IsTransitioning => _isTransitioning;

        /// <summary>Number of active overlays.</summary>
        public int OverlayCount => _overlayStack.Count;

        // ============================================
        // Unity Lifecycle
        // ============================================

        private void Awake()
        {
            // Prevent duplicates - destroy self if UIManager already exists
            if (ServiceLocator.Has<IUIManager>())
            {
                Debug.Log("[UIManager] UIManager already exists. Destroying duplicate.");
                _isRegisteredInstance = false;
                Destroy(gameObject);
                return;
            }

            // Persist across scene loads
            DontDestroyOnLoad(gameObject);

            // Register with ServiceLocator
            if (!ServiceLocator.IsInitialized)
            {
                ServiceLocator.Initialize();
            }
            ServiceLocator.Register<IUIManager>(this);
            _isRegisteredInstance = true;

            // Subscribe to scene loaded events to re-cache screens
            SceneManager.sceneLoaded += OnSceneLoaded;

            Debug.Log("[UIManager] Initialized.");
        }

        private void OnDestroy()
        {
            // Only unsubscribe and unregister if this was the registered instance
            if (_isRegisteredInstance)
            {
                SceneManager.sceneLoaded -= OnSceneLoaded;

                if (ServiceLocator.Has<IUIManager>())
                {
                    ServiceLocator.Unregister<IUIManager>();
                }
            }
        }

        /// <summary>
        /// Called when a new scene is loaded. Re-caches screens from the new scene.
        /// </summary>
        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            Debug.Log($"[UIManager] Scene loaded: {scene.name}. Re-caching screens...");

            // Clear previous screen cache
            _screens.Clear();
            _currentScreen = null;
            _overlayStack.Clear();

            // Re-cache screens from new scene
            CacheScreens();
        }

        private void Update()
        {
            // Handle back button (Android/Escape key)
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                HandleBackPressed();
            }
        }

        // ============================================
        // Initialization
        // ============================================

        /// <summary>
        /// Cache all screens from containers for fast lookup.
        /// If no containers are assigned, searches the entire scene.
        /// </summary>
        private void CacheScreens()
        {
            // Cache screens from main container
            if (_screenContainer != null)
            {
                foreach (var screen in _screenContainer.GetComponentsInChildren<ScreenBase>(true))
                {
                    _screens[screen.GetType()] = screen;
                    screen.gameObject.SetActive(false);
                }
            }

            // Cache screens from overlay container
            if (_overlayContainer != null)
            {
                foreach (var screen in _overlayContainer.GetComponentsInChildren<ScreenBase>(true))
                {
                    _screens[screen.GetType()] = screen;
                    screen.gameObject.SetActive(false);
                }
            }

            // If no containers set, search entire scene for screens
            if (_screenContainer == null && _overlayContainer == null)
            {
                var allScreens = FindObjectsByType<ScreenBase>(FindObjectsInactive.Include, FindObjectsSortMode.None);
                foreach (var screen in allScreens)
                {
                    _screens[screen.GetType()] = screen;
                    screen.gameObject.SetActive(false);
                }
            }

            Debug.Log($"[UIManager] Cached {_screens.Count} screens.");
        }

        // ============================================
        // Screen Management
        // ============================================

        /// <summary>
        /// Transition to a new screen with fade effect.
        /// </summary>
        public void ShowScreen<T>(bool showGlobalUI = true) where T : ScreenBase
        {
            if (_isTransitioning)
            {
                Debug.LogWarning("[UIManager] Transition already in progress.");
                return;
            }

            StartCoroutine(TransitionToScreen<T>(showGlobalUI));
        }

        private IEnumerator TransitionToScreen<T>(bool showGlobalUI) where T : ScreenBase
        {
            if (!_screens.TryGetValue(typeof(T), out var newScreen))
            {
                Debug.LogError($"[UIManager] Screen not found: {typeof(T).Name}");
                yield break;
            }

            _isTransitioning = true;

            // Clear any overlays
            ClearOverlays();

            // Fade out
            yield return FadeOut();

            // Hide current screen
            if (_currentScreen != null)
            {
                _currentScreen.OnHide();
                _currentScreen.gameObject.SetActive(false);
            }

            // Toggle global UI based on screen settings
            if (_globalHeader != null)
            {
                _globalHeader.SetActive(showGlobalUI && newScreen.ShowGlobalHeader);
            }
            if (_globalNavDock != null)
            {
                _globalNavDock.SetActive(showGlobalUI && newScreen.ShowGlobalNav);
            }

            // Show new screen
            _currentScreen = newScreen;
            _currentScreen.gameObject.SetActive(true);
            _currentScreen.OnShow();

            // Fade in
            yield return FadeIn();

            _isTransitioning = false;

            Debug.Log($"[UIManager] Transitioned to {typeof(T).Name}");
        }

        // ============================================
        // Overlay Management
        // ============================================

        /// <summary>
        /// Push an overlay on top of the current screen.
        /// </summary>
        public void PushOverlay<T>() where T : ScreenBase
        {
            if (!_screens.TryGetValue(typeof(T), out var overlay))
            {
                Debug.LogError($"[UIManager] Overlay not found: {typeof(T).Name}");
                return;
            }

            // Pause current screen or top overlay
            if (_overlayStack.Count > 0)
            {
                _overlayStack.Peek().OnPause();
            }
            else if (_currentScreen != null)
            {
                _currentScreen.OnPause();
            }

            // Push and show overlay
            _overlayStack.Push(overlay);
            overlay.gameObject.SetActive(true);
            overlay.OnShow();

            Debug.Log($"[UIManager] Pushed overlay: {typeof(T).Name}");
        }

        /// <summary>
        /// Pop the topmost overlay from the stack.
        /// </summary>
        public void PopOverlay()
        {
            if (_overlayStack.Count == 0)
            {
                Debug.LogWarning("[UIManager] No overlay to pop.");
                return;
            }

            // Hide and pop current overlay
            var overlay = _overlayStack.Pop();
            overlay.OnHide();
            overlay.gameObject.SetActive(false);

            // Resume underlying screen/overlay
            if (_overlayStack.Count > 0)
            {
                _overlayStack.Peek().OnResume();
            }
            else if (_currentScreen != null)
            {
                _currentScreen.OnResume();
            }

            Debug.Log($"[UIManager] Popped overlay: {overlay.GetType().Name}");
        }

        /// <summary>
        /// Clear all overlays from the stack.
        /// </summary>
        public void ClearOverlays()
        {
            while (_overlayStack.Count > 0)
            {
                var overlay = _overlayStack.Pop();
                overlay.OnHide();
                overlay.gameObject.SetActive(false);
            }

            // Resume current screen if any
            if (_currentScreen != null && _currentScreen.IsVisible)
            {
                _currentScreen.OnResume();
            }
        }

        // ============================================
        // Screen Queries
        // ============================================

        /// <summary>
        /// Get a screen instance by type.
        /// </summary>
        public T GetScreen<T>() where T : ScreenBase
        {
            return _screens.TryGetValue(typeof(T), out var screen) ? screen as T : null;
        }

        /// <summary>
        /// Check if a specific screen is currently active.
        /// </summary>
        public bool IsScreenActive<T>() where T : ScreenBase
        {
            return _currentScreen != null && _currentScreen.GetType() == typeof(T);
        }

        // ============================================
        // Back Button Handling
        // ============================================

        private void HandleBackPressed()
        {
            // Try overlay first
            if (_overlayStack.Count > 0)
            {
                var topOverlay = _overlayStack.Peek();
                if (!topOverlay.OnBackPressed())
                {
                    PopOverlay();
                }
                return;
            }

            // Then current screen
            if (_currentScreen != null)
            {
                _currentScreen.OnBackPressed();
            }
        }

        // ============================================
        // Fade Transitions
        // ============================================

        private IEnumerator FadeOut()
        {
            if (_fadeOverlay == null) yield break;

            _fadeOverlay.gameObject.SetActive(true);
            _fadeOverlay.alpha = 0f;

            float elapsed = 0f;
            float duration = _transitionDuration * 0.5f;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                _fadeOverlay.alpha = Mathf.Clamp01(elapsed / duration);
                yield return null;
            }

            _fadeOverlay.alpha = 1f;
        }

        private IEnumerator FadeIn()
        {
            if (_fadeOverlay == null) yield break;

            _fadeOverlay.alpha = 1f;

            float elapsed = 0f;
            float duration = _transitionDuration * 0.5f;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                _fadeOverlay.alpha = 1f - Mathf.Clamp01(elapsed / duration);
                yield return null;
            }

            _fadeOverlay.alpha = 0f;
            _fadeOverlay.gameObject.SetActive(false);
        }
    }
}
