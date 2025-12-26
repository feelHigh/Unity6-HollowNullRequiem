// ============================================
// ScreenBase.cs
// Base class for all UI screens
// ============================================

using UnityEngine;

namespace HNR.UI
{
    /// <summary>
    /// Abstract base class for all UI screens.
    /// Extend this for MainMenuScreen, CombatScreen, BastionScreen, etc.
    /// </summary>
    public abstract class ScreenBase : MonoBehaviour
    {
        // ============================================
        // Configuration
        // ============================================

        [Header("Screen Configuration")]
        [SerializeField, Tooltip("Pause game time while this screen is active")]
        protected bool _pausesGame = false;

        [SerializeField, Tooltip("Show global header bar on this screen")]
        protected bool _showGlobalHeader = true;

        [SerializeField, Tooltip("Show global navigation dock on this screen")]
        protected bool _showGlobalNav = true;

        // ============================================
        // Properties
        // ============================================

        /// <summary>Whether to show global header on this screen.</summary>
        public bool ShowGlobalHeader => _showGlobalHeader;

        /// <summary>Whether to show global nav dock on this screen.</summary>
        public bool ShowGlobalNav => _showGlobalNav;

        /// <summary>Whether this screen pauses the game.</summary>
        public bool PausesGame => _pausesGame;

        /// <summary>Whether this screen is currently visible.</summary>
        public bool IsVisible { get; protected set; }

        // ============================================
        // Unity Lifecycle
        // ============================================

        /// <summary>
        /// Unity Awake. Override to configure screen settings.
        /// Always call base.Awake() first.
        /// </summary>
        protected virtual void Awake()
        {
            // Base implementation - derived classes can configure _showGlobalHeader, _showGlobalNav, etc.
        }

        // ============================================
        // Screen Lifecycle Methods
        // ============================================

        /// <summary>
        /// Called when the screen is shown.
        /// Override to implement show animations and initialization.
        /// </summary>
        public virtual void OnShow()
        {
            IsVisible = true;

            if (_pausesGame)
            {
                Time.timeScale = 0f;
            }

            Debug.Log($"[{GetType().Name}] OnShow");
        }

        /// <summary>
        /// Called when the screen is hidden.
        /// Override to implement hide animations and cleanup.
        /// </summary>
        public virtual void OnHide()
        {
            IsVisible = false;

            if (_pausesGame)
            {
                Time.timeScale = 1f;
            }

            Debug.Log($"[{GetType().Name}] OnHide");
        }

        /// <summary>
        /// Called when an overlay above this screen is closed.
        /// Screen regains focus.
        /// </summary>
        public virtual void OnResume()
        {
            Debug.Log($"[{GetType().Name}] OnResume");
        }

        /// <summary>
        /// Called when an overlay is pushed on top of this screen.
        /// Screen loses focus but remains visible.
        /// </summary>
        public virtual void OnPause()
        {
            Debug.Log($"[{GetType().Name}] OnPause");
        }

        /// <summary>
        /// Called when back/escape is pressed.
        /// </summary>
        /// <returns>True if handled, false to allow default behavior</returns>
        public virtual bool OnBackPressed()
        {
            return false;
        }

        // ============================================
        // Utility Methods
        // ============================================

        /// <summary>
        /// Animate screen elements on show.
        /// Override for custom entrance animations.
        /// </summary>
        protected virtual void PlayShowAnimation()
        {
            // Override in derived classes for DOTween animations
        }

        /// <summary>
        /// Animate screen elements on hide.
        /// Override for custom exit animations.
        /// </summary>
        protected virtual void PlayHideAnimation()
        {
            // Override in derived classes for DOTween animations
        }
    }
}
