// ============================================
// IUIManager.cs
// UI screen management interface
// ============================================

using HNR.UI;

namespace HNR.Core.Interfaces
{
    /// <summary>
    /// UI screen management service.
    /// Controls screen stack, overlays, and screen transitions.
    /// </summary>
    /// <remarks>
    /// Register with ServiceLocator at startup.
    /// Implementation: UIManager (MonoBehaviour)
    /// Uses GUI Pro - Fantasy Hero assets for UI styling.
    /// </remarks>
    public interface IUIManager
    {
        /// <summary>
        /// Show a screen by type, adding it to the screen stack.
        /// Hides the previous top screen.
        /// </summary>
        /// <typeparam name="T">Type of screen to show (must derive from ScreenBase)</typeparam>
        void ShowScreen<T>() where T : ScreenBase;

        /// <summary>
        /// Hide a screen by type, removing it from the screen stack.
        /// Shows the next screen in the stack if any.
        /// </summary>
        /// <typeparam name="T">Type of screen to hide (must derive from ScreenBase)</typeparam>
        void HideScreen<T>() where T : ScreenBase;

        /// <summary>
        /// Show an overlay screen on top of the current screen.
        /// Does not hide the underlying screen.
        /// </summary>
        /// <typeparam name="T">Type of overlay to show (must derive from ScreenBase)</typeparam>
        void ShowOverlay<T>() where T : ScreenBase;

        /// <summary>
        /// Get a reference to a screen instance by type.
        /// </summary>
        /// <typeparam name="T">Type of screen to retrieve (must derive from ScreenBase)</typeparam>
        /// <returns>The screen instance, or null if not found</returns>
        T GetScreen<T>() where T : ScreenBase;
    }
}
