// ============================================
// ISaveManager.cs
// Persistence and save data interface
// ============================================

using HNR.Data;

namespace HNR.Core.Interfaces
{
    /// <summary>
    /// Persistence and save data service.
    /// Handles saving/loading of run data and player settings.
    /// </summary>
    /// <remarks>
    /// Register with ServiceLocator at startup.
    /// Implementation: SaveManager (non-MonoBehaviour)
    /// Uses Easy Save 3 for serialization.
    /// </remarks>
    public interface ISaveManager
    {
        /// <summary>
        /// Gets whether there is a saved run that can be continued.
        /// </summary>
        bool HasSavedRun { get; }

        /// <summary>
        /// Load the saved run data.
        /// </summary>
        /// <returns>The loaded run data, or null if no save exists</returns>
        RunSaveData LoadRun();

        /// <summary>
        /// Save the current run data.
        /// Called automatically at checkpoints and manually on exit.
        /// </summary>
        /// <param name="data">The run data to save</param>
        void SaveRun(RunSaveData data);

        /// <summary>
        /// Delete the saved run data.
        /// Called when a run ends (victory or defeat).
        /// </summary>
        void DeleteRun();

        /// <summary>
        /// Load player settings.
        /// Returns default settings if no save exists.
        /// </summary>
        /// <returns>The loaded settings data</returns>
        SettingsData LoadSettings();

        /// <summary>
        /// Save player settings.
        /// </summary>
        /// <param name="data">The settings data to save</param>
        void SaveSettings(SettingsData data);
    }
}
