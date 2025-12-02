// ============================================
// IPoolManager.cs
// Object pooling system interface
// ============================================

using UnityEngine;

namespace HNR.Core.Interfaces
{
    /// <summary>
    /// Object pooling system service.
    /// Manages reusable object instances to reduce garbage collection.
    /// </summary>
    /// <remarks>
    /// Register with ServiceLocator at startup.
    /// Implementation: PoolManager (MonoBehaviour)
    /// Pool prefabs for: cards, VFX, damage numbers, enemies.
    /// </remarks>
    public interface IPoolManager
    {
        /// <summary>
        /// Get an object from the pool.
        /// Creates a new instance if the pool is empty.
        /// </summary>
        /// <typeparam name="T">Type of component (must be Component and IPoolable)</typeparam>
        /// <returns>A pooled or new instance</returns>
        T Get<T>() where T : Component, IPoolable;

        /// <summary>
        /// Return an object to the pool for reuse.
        /// </summary>
        /// <typeparam name="T">Type of component (must be Component and IPoolable)</typeparam>
        /// <param name="obj">The object to return</param>
        void Return<T>(T obj) where T : Component, IPoolable;

        /// <summary>
        /// Pre-instantiate objects to avoid runtime allocation.
        /// Call during loading screens or initialization.
        /// </summary>
        /// <typeparam name="T">Type of component (must be Component and IPoolable)</typeparam>
        /// <param name="count">Number of instances to pre-create</param>
        void PreWarm<T>(int count) where T : Component, IPoolable;
    }
}
