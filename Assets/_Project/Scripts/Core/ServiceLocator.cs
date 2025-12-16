// ============================================
// ServiceLocator.cs
// Core service access pattern for Hollow Null Requiem
// ============================================

using System;
using System.Collections.Generic;
using UnityEngine;

namespace HNR.Core
{
    /// <summary>
    /// Provides global access to registered services without tight coupling.
    /// Use interfaces for service types to maintain testability.
    /// </summary>
    /// <example>
    /// // Registration (at startup)
    /// ServiceLocator.Register&lt;IGameManager&gt;(gameManager);
    ///
    /// // Retrieval (anywhere in code)
    /// var gm = ServiceLocator.Get&lt;IGameManager&gt;();
    /// </example>
    public static class ServiceLocator
    {
        private static Dictionary<Type, object> _services = new();
        private static bool _isInitialized;

        // Reset static state when entering play mode (Editor only)
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStaticState()
        {
            _services = new Dictionary<Type, object>();
            _isInitialized = false;
        }

        /// <summary>
        /// Initialize the Service Locator. Call once at startup.
        /// Clears any existing services and sets initialized flag.
        /// </summary>
        public static void Initialize()
        {
            if (_isInitialized)
            {
                Debug.Log("[ServiceLocator] Already initialized, skipping.");
                return;
            }

            _services.Clear();
            _isInitialized = true;
            Debug.Log("[ServiceLocator] Initialized.");
        }

        /// <summary>
        /// Register a service implementation.
        /// </summary>
        /// <typeparam name="T">Service interface type (should be an interface)</typeparam>
        /// <param name="service">Service implementation instance</param>
        /// <exception cref="ArgumentNullException">Thrown if service is null</exception>
        public static void Register<T>(T service) where T : class
        {
            if (service == null)
            {
                Debug.LogError($"[ServiceLocator] Cannot register null service for type: {typeof(T).Name}");
                return;
            }

            var type = typeof(T);

            if (_services.ContainsKey(type))
            {
                Debug.LogWarning($"[ServiceLocator] Overwriting existing service: {type.Name}");
            }

            _services[type] = service;
            Debug.Log($"[ServiceLocator] Registered service: {type.Name}");
        }

        /// <summary>
        /// Retrieve a registered service.
        /// </summary>
        /// <typeparam name="T">Service interface type</typeparam>
        /// <returns>Service implementation or null if not found</returns>
        public static T Get<T>() where T : class
        {
            var type = typeof(T);

            if (_services.TryGetValue(type, out var service))
            {
                return service as T;
            }

            Debug.LogError($"[ServiceLocator] Service not found: {type.Name}");
            return null;
        }

        /// <summary>
        /// Try to retrieve a registered service without logging errors.
        /// </summary>
        /// <typeparam name="T">Service interface type</typeparam>
        /// <param name="service">Output service if found</param>
        /// <returns>True if service was found, false otherwise</returns>
        public static bool TryGet<T>(out T service) where T : class
        {
            var type = typeof(T);

            if (_services.TryGetValue(type, out var obj))
            {
                service = obj as T;
                return service != null;
            }

            service = null;
            return false;
        }

        /// <summary>
        /// Check if a service is registered.
        /// </summary>
        /// <typeparam name="T">Service interface type</typeparam>
        /// <returns>True if service is registered</returns>
        public static bool Has<T>() where T : class
        {
            return _services.ContainsKey(typeof(T));
        }

        /// <summary>
        /// Unregister a service. Primarily used for testing.
        /// </summary>
        /// <typeparam name="T">Service interface type</typeparam>
        public static void Unregister<T>() where T : class
        {
            var type = typeof(T);

            if (_services.Remove(type))
            {
                Debug.Log($"[ServiceLocator] Unregistered service: {type.Name}");
            }
        }

        /// <summary>
        /// Clear all registered services and reset initialization flag.
        /// Used for testing and scene transitions.
        /// </summary>
        public static void Clear()
        {
            _services.Clear();
            _isInitialized = false;
            Debug.Log("[ServiceLocator] Cleared all services.");
        }

        /// <summary>
        /// Get the count of registered services.
        /// Useful for debugging.
        /// </summary>
        public static int ServiceCount => _services.Count;

        /// <summary>
        /// Check if the ServiceLocator has been initialized.
        /// </summary>
        public static bool IsInitialized => _isInitialized;
    }
}
