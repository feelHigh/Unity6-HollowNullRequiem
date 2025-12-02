// ============================================
// GameEvent.cs
// Base class for all game events
// ============================================

namespace HNR.Core.Events
{
    /// <summary>
    /// Abstract base class for all game events.
    /// Derive from this class to create specific event types.
    /// </summary>
    /// <remarks>
    /// Events are published via EventBus.Publish() and received
    /// by subscribers registered with EventBus.Subscribe().
    ///
    /// Events should be immutable - set all properties in constructor.
    /// </remarks>
    /// <example>
    /// public class DamageDealtEvent : GameEvent
    /// {
    ///     public int Amount { get; }
    ///     public DamageDealtEvent(int amount) => Amount = amount;
    /// }
    /// </example>
    public abstract class GameEvent
    {
        /// <summary>
        /// Timestamp when the event was created.
        /// Useful for debugging and event ordering.
        /// </summary>
        public float Timestamp { get; }

        /// <summary>
        /// Creates a new GameEvent with current timestamp.
        /// </summary>
        protected GameEvent()
        {
            Timestamp = UnityEngine.Time.time;
        }
    }
}
