// ============================================
// IUIManager.cs
// UI screen management interface
// ============================================

using HNR.UI;

namespace HNR.Core.Interfaces
{
    /// <summary>
    /// UI screen management service.
    /// Controls screen transitions, overlays, and global UI elements.
    /// </summary>
    /// <remarks>
    /// Register with ServiceLocator at startup.
    /// Implementation: UIManager (MonoBehaviour with DontDestroyOnLoad)
    /// </remarks>
    public interface IUIManager
    {
        /// <summary>
        /// Transition to a screen, hiding the current one.
        /// </summary>
        /// <typeparam name="T">Screen type to show</typeparam>
        /// <param name="showGlobalUI">Whether to show global header/nav</param>
        void ShowScreen<T>(bool showGlobalUI = true) where T : ScreenBase;

        /// <summary>
        /// Push an overlay screen on top of current screen.
        /// Does not hide the underlying screen.
        /// </summary>
        /// <typeparam name="T">Overlay type to push</typeparam>
        void PushOverlay<T>() where T : ScreenBase;

        /// <summary>
        /// Pop the topmost overlay from the stack.
        /// </summary>
        void PopOverlay();

        /// <summary>
        /// Clear all overlays from the stack.
        /// </summary>
        void ClearOverlays();

        /// <summary>
        /// Get a reference to a screen instance by type.
        /// </summary>
        /// <typeparam name="T">Screen type to retrieve</typeparam>
        /// <returns>Screen instance or null if not found</returns>
        T GetScreen<T>() where T : ScreenBase;

        /// <summary>
        /// Check if a specific screen is currently active.
        /// </summary>
        /// <typeparam name="T">Screen type to check</typeparam>
        /// <returns>True if screen is the current active screen</returns>
        bool IsScreenActive<T>() where T : ScreenBase;

        /// <summary>
        /// Get the currently active screen.
        /// </summary>
        ScreenBase CurrentScreen { get; }

        /// <summary>
        /// Check if a transition is in progress.
        /// </summary>
        bool IsTransitioning { get; }
    }
}
