// ============================================
// IPoolable.cs
// Interface for objects managed by the pool system
// ============================================

namespace HNR.Core.Interfaces
{
    /// <summary>
    /// Interface for objects that can be pooled by IPoolManager.
    /// Implement this on MonoBehaviours that need object pooling.
    /// </summary>
    /// <remarks>
    /// Common poolable objects:
    /// - Card visuals
    /// - VFX particles
    /// - Damage numbers
    /// - Enemy instances
    /// - Projectiles
    /// </remarks>
    public interface IPoolable
    {
        /// <summary>
        /// Called when the object is retrieved from the pool.
        /// Use this to reset/initialize the object state.
        /// </summary>
        void OnSpawnFromPool();

        /// <summary>
        /// Called when the object is returned to the pool.
        /// Use this to clean up, stop effects, and prepare for reuse.
        /// </summary>
        void OnReturnToPool();
    }
}
