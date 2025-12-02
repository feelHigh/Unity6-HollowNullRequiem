// ============================================
// SaveManager.cs
// Save/load operations using Easy Save 3
// ============================================

using UnityEngine;
using HNR.Core.Interfaces;
using HNR.Data;

namespace HNR.Core
{
    /// <summary>
    /// Manages save/load operations using Easy Save 3.
    /// Registered with ServiceLocator during Boot state.
    /// </summary>
    public class SaveManager : ISaveManager
    {
        private const string RUN_SAVE_KEY = "CurrentRun";
        private const string SETTINGS_KEY = "Settings";
        private const string SAVE_FILE = "HNR_Save.es3";

        /// <summary>
        /// Gets whether there is a saved run that can be continued.
        /// </summary>
        public bool HasSavedRun => ES3.KeyExists(RUN_SAVE_KEY, SAVE_FILE);

        /// <summary>
        /// Initialize the save manager.
        /// </summary>
        public void Initialize()
        {
            Debug.Log("[SaveManager] Initialized with Easy Save 3");
        }

        /// <summary>
        /// Save the current run data.
        /// </summary>
        public void SaveRun(RunSaveData data)
        {
            ES3.Save(RUN_SAVE_KEY, data, SAVE_FILE);
            Debug.Log("[SaveManager] Run saved");
        }

        /// <summary>
        /// Load the saved run data.
        /// </summary>
        public RunSaveData LoadRun()
        {
            if (!HasSavedRun)
            {
                Debug.Log("[SaveManager] No saved run found");
                return null;
            }

            var data = ES3.Load<RunSaveData>(RUN_SAVE_KEY, SAVE_FILE);
            Debug.Log("[SaveManager] Run loaded");
            return data;
        }

        /// <summary>
        /// Delete the saved run data.
        /// </summary>
        public void DeleteRun()
        {
            if (HasSavedRun)
            {
                ES3.DeleteKey(RUN_SAVE_KEY, SAVE_FILE);
                Debug.Log("[SaveManager] Run deleted");
            }
        }

        /// <summary>
        /// Save player settings.
        /// </summary>
        public void SaveSettings(SettingsData data)
        {
            ES3.Save(SETTINGS_KEY, data, SAVE_FILE);
            Debug.Log("[SaveManager] Settings saved");
        }

        /// <summary>
        /// Load player settings.
        /// </summary>
        public SettingsData LoadSettings()
        {
            if (!ES3.KeyExists(SETTINGS_KEY, SAVE_FILE))
            {
                Debug.Log("[SaveManager] No settings found, using defaults");
                return SettingsData.CreateDefault();
            }

            return ES3.Load<SettingsData>(SETTINGS_KEY, SAVE_FILE);
        }
    }
}
