// ============================================
// IRelicManager.cs
// Relic service interface
// ============================================

using System.Collections.Generic;
using HNR.Progression;

namespace HNR.Core.Interfaces
{
    /// <summary>
    /// Relic service for managing owned relics and their effects.
    /// </summary>
    /// <remarks>
    /// Register with ServiceLocator at startup.
    /// Implementation: RelicManager (MonoBehaviour)
    /// </remarks>
    public interface IRelicManager
    {
        /// <summary>
        /// List of all owned relics.
        /// </summary>
        IReadOnlyList<RelicDataSO> OwnedRelics { get; }

        /// <summary>
        /// Number of owned relics.
        /// </summary>
        int RelicCount { get; }

        /// <summary>
        /// Add a relic to the collection.
        /// Publishes RelicAcquiredEvent and applies passive effects immediately.
        /// </summary>
        /// <param name="relic">Relic to add.</param>
        void AddRelic(RelicDataSO relic);

        /// <summary>
        /// Check if a relic is owned by ID.
        /// </summary>
        /// <param name="relicId">Relic ID to check.</param>
        /// <returns>True if relic is owned.</returns>
        bool HasRelic(string relicId);

        /// <summary>
        /// Get an owned relic by ID.
        /// </summary>
        /// <param name="relicId">Relic ID to find.</param>
        /// <returns>Relic data or null if not owned.</returns>
        RelicDataSO GetRelic(string relicId);

        /// <summary>
        /// Trigger all relics matching the specified trigger type.
        /// </summary>
        /// <param name="trigger">Trigger type to match.</param>
        /// <param name="context">Optional context object for the trigger.</param>
        void TriggerRelics(RelicTrigger trigger, object context = null);

        /// <summary>
        /// Load relics from saved IDs.
        /// Used for save/load functionality.
        /// </summary>
        /// <param name="relicIds">List of relic IDs to load.</param>
        void LoadRelics(List<string> relicIds);

        /// <summary>
        /// Get list of owned relic IDs for saving.
        /// </summary>
        /// <returns>List of relic IDs.</returns>
        List<string> GetRelicIds();

        /// <summary>
        /// Clear all owned relics.
        /// Used when starting a new run.
        /// </summary>
        void ClearRelics();
    }
}
