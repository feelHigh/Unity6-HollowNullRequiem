// ============================================
// ScreenBase.cs
// Base class for all UI screens
// ============================================

using UnityEngine;

namespace HNR.UI
{
    /// <summary>
    /// Abstract base class for all UI screens.
    /// Derive from this for MainMenuScreen, CombatScreen, etc.
    /// </summary>
    public abstract class ScreenBase : MonoBehaviour
    {
        /// <summary>
        /// Whether this screen is currently visible.
        /// </summary>
        public bool IsVisible { get; protected set; }

        /// <summary>
        /// Called when the screen is shown.
        /// Override to implement show animations and initialization.
        /// </summary>
        public virtual void Show()
        {
            IsVisible = true;
            gameObject.SetActive(true);
        }

        /// <summary>
        /// Called when the screen is hidden.
        /// Override to implement hide animations and cleanup.
        /// </summary>
        public virtual void Hide()
        {
            IsVisible = false;
            gameObject.SetActive(false);
        }

        /// <summary>
        /// Called when the screen becomes the top screen in the stack.
        /// </summary>
        public virtual void OnFocus() { }

        /// <summary>
        /// Called when another screen is pushed on top of this one.
        /// </summary>
        public virtual void OnLostFocus() { }
    }
}
