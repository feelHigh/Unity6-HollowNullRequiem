// ============================================
// EventBus.cs
// Centralized event system for decoupled communication
// ============================================

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace HNR.Core.Events
{
    /// <summary>
    /// Centralized event system for decoupled communication between systems.
    /// Uses the publish-subscribe pattern.
    /// </summary>
    /// <remarks>
    /// Thread-safe for Unity's main thread. Do not use from background threads.
    ///
    /// Best practices:
    /// - Subscribe in OnEnable, Unsubscribe in OnDisable
    /// - Keep event handlers lightweight
    /// - Don't publish events from within event handlers if possible
    /// </remarks>
    /// <example>
    /// // Subscribe to events
    /// EventBus.Subscribe&lt;DamageDealtEvent&gt;(OnDamageDealt);
    ///
    /// // Publish events
    /// EventBus.Publish(new DamageDealtEvent(source, target, 10));
    ///
    /// // Unsubscribe when done
    /// EventBus.Unsubscribe&lt;DamageDealtEvent&gt;(OnDamageDealt);
    /// </example>
    public static class EventBus
    {
        private static Dictionary<Type, List<Delegate>> _subscribers = new();

        // Reset static state when entering play mode (Editor only)
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStaticState()
        {
            _subscribers = new Dictionary<Type, List<Delegate>>();
        }

        /// <summary>
        /// Subscribe to an event type.
        /// </summary>
        /// <typeparam name="T">Event type (must derive from GameEvent)</typeparam>
        /// <param name="handler">Handler method to call when event is published</param>
        public static void Subscribe<T>(Action<T> handler) where T : GameEvent
        {
            if (handler == null)
            {
                Debug.LogError("[EventBus] Cannot subscribe null handler");
                return;
            }

            var type = typeof(T);

            if (!_subscribers.ContainsKey(type))
            {
                _subscribers[type] = new List<Delegate>();
            }

            if (_subscribers[type].Contains(handler))
            {
                Debug.LogWarning($"[EventBus] Handler already subscribed to {type.Name}");
                return;
            }

            _subscribers[type].Add(handler);
        }

        /// <summary>
        /// Unsubscribe from an event type.
        /// </summary>
        /// <typeparam name="T">Event type (must derive from GameEvent)</typeparam>
        /// <param name="handler">Handler method to remove</param>
        public static void Unsubscribe<T>(Action<T> handler) where T : GameEvent
        {
            if (handler == null)
            {
                Debug.LogError("[EventBus] Cannot unsubscribe null handler");
                return;
            }

            var type = typeof(T);

            if (_subscribers.ContainsKey(type))
            {
                _subscribers[type].Remove(handler);
            }
        }

        /// <summary>
        /// Publish an event to all subscribers.
        /// Handlers are invoked synchronously in subscription order.
        /// </summary>
        /// <typeparam name="T">Event type (must derive from GameEvent)</typeparam>
        /// <param name="evt">Event instance to publish</param>
        public static void Publish<T>(T evt) where T : GameEvent
        {
            if (evt == null)
            {
                Debug.LogError("[EventBus] Cannot publish null event");
                return;
            }

            var type = typeof(T);

            if (!_subscribers.ContainsKey(type))
            {
                return;
            }

            // ToArray() creates a copy to handle modifications during iteration
            // (e.g., if a handler unsubscribes itself or subscribes new handlers)
            foreach (var subscriber in _subscribers[type].ToArray())
            {
                try
                {
                    (subscriber as Action<T>)?.Invoke(evt);
                }
                catch (Exception e)
                {
                    Debug.LogError($"[EventBus] Error in handler for {type.Name}: {e}");
                }
            }
        }

        /// <summary>
        /// Check if there are any subscribers for an event type.
        /// </summary>
        /// <typeparam name="T">Event type to check</typeparam>
        /// <returns>True if at least one handler is subscribed</returns>
        public static bool HasSubscribers<T>() where T : GameEvent
        {
            var type = typeof(T);
            return _subscribers.ContainsKey(type) && _subscribers[type].Count > 0;
        }

        /// <summary>
        /// Get the number of subscribers for an event type.
        /// Useful for debugging.
        /// </summary>
        /// <typeparam name="T">Event type to check</typeparam>
        /// <returns>Number of subscribed handlers</returns>
        public static int GetSubscriberCount<T>() where T : GameEvent
        {
            var type = typeof(T);
            return _subscribers.ContainsKey(type) ? _subscribers[type].Count : 0;
        }

        /// <summary>
        /// Clear all subscribers for a specific event type.
        /// </summary>
        /// <typeparam name="T">Event type to clear</typeparam>
        public static void ClearSubscribers<T>() where T : GameEvent
        {
            var type = typeof(T);
            if (_subscribers.ContainsKey(type))
            {
                _subscribers[type].Clear();
                Debug.Log($"[EventBus] Cleared subscribers for {type.Name}");
            }
        }

        /// <summary>
        /// Clear all subscribers for all event types.
        /// Used for testing and scene transitions.
        /// </summary>
        public static void Clear()
        {
            _subscribers.Clear();
            Debug.Log("[EventBus] Cleared all subscribers");
        }

        /// <summary>
        /// Get total number of registered event types.
        /// Useful for debugging.
        /// </summary>
        public static int EventTypeCount => _subscribers.Count;

        /// <summary>
        /// Get total number of all subscribers across all event types.
        /// Useful for debugging.
        /// </summary>
        public static int TotalSubscriberCount => _subscribers.Values.Sum(list => list.Count);
    }
}
