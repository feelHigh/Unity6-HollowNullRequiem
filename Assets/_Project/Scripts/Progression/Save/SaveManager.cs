// ============================================
// SaveManager.cs
// Persistence manager using Easy Save 3
// ============================================

using System;
using UnityEngine;
using HNR.Core;
using HNR.Core.Interfaces;

namespace HNR.Progression
{
    /// <summary>
    /// Manages all save/load operations using Easy Save 3.
    /// Handles run saves, settings, and meta-progression data.
    /// </summary>
    public class SaveManager : MonoBehaviour, ISaveManager
    {
        // ============================================
        // Constants
        // ============================================

        private const string SAVE_FILE = "HNR_Save.es3";
        private const string RUN_SAVE_KEY = "CurrentRun";
        private const string SETTINGS_KEY = "Settings";
        private const string META_KEY = "MetaProgression";
        private const string BATTLE_MISSION_KEY = "BattleMissionProgress";

        // ============================================
        // Properties
        // ============================================

        /// <summary>Whether a saved run exists.</summary>
        public bool HasSavedRun
        {
            get
            {
                try
                {
                    return ES3.KeyExists(RUN_SAVE_KEY, SAVE_FILE);
                }
                catch (Exception e)
                {
                    Debug.LogError($"[SaveManager] Error checking save: {e.Message}");
                    return false;
                }
            }
        }

        /// <summary>Path to the save file.</summary>
        public string SaveFilePath => System.IO.Path.Combine(Application.persistentDataPath, SAVE_FILE);

        // ============================================
        // Unity Lifecycle
        // ============================================

        private void Awake()
        {
            ServiceLocator.Register<ISaveManager>(this);
            DontDestroyOnLoad(gameObject);
            Debug.Log($"[SaveManager] Initialized. Save path: {SaveFilePath}");
        }

        private void OnDestroy()
        {
            if (ServiceLocator.Has<ISaveManager>())
            {
                ServiceLocator.Unregister<ISaveManager>();
            }
        }

        // ============================================
        // Run Save/Load
        // ============================================

        /// <summary>
        /// Save the current run state.
        /// </summary>
        /// <param name="data">Run data to save.</param>
        public void SaveRun(RunSaveData data)
        {
            if (data == null)
            {
                Debug.LogWarning("[SaveManager] Cannot save null run data");
                return;
            }

            try
            {
                // Add timestamp
                data.SaveTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                data.GameVersion = Application.version;

                ES3.Save(RUN_SAVE_KEY, data, SAVE_FILE);
                Debug.Log($"[SaveManager] Run saved successfully");
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveManager] Failed to save run: {e.Message}");
            }
        }

        /// <summary>
        /// Load the saved run state.
        /// </summary>
        /// <returns>Loaded run data, or null if no save exists or load fails.</returns>
        public RunSaveData LoadRun()
        {
            try
            {
                if (!HasSavedRun)
                {
                    Debug.Log("[SaveManager] No saved run found");
                    return null;
                }

                var data = ES3.Load<RunSaveData>(RUN_SAVE_KEY, SAVE_FILE);
                Debug.Log($"[SaveManager] Run loaded successfully (saved at {data.SaveTimestamp})");
                return data;
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveManager] Failed to load run: {e.Message}");
                return null;
            }
        }

        /// <summary>
        /// Delete the saved run.
        /// </summary>
        public void DeleteRun()
        {
            try
            {
                if (HasSavedRun)
                {
                    ES3.DeleteKey(RUN_SAVE_KEY, SAVE_FILE);
                    Debug.Log("[SaveManager] Run deleted");
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveManager] Failed to delete run: {e.Message}");
            }
        }

        // ============================================
        // Settings Save/Load
        // ============================================

        /// <summary>
        /// Save player settings.
        /// </summary>
        /// <param name="data">Settings to save.</param>
        public void SaveSettings(SettingsData data)
        {
            if (data == null)
            {
                Debug.LogWarning("[SaveManager] Cannot save null settings");
                return;
            }

            try
            {
                ES3.Save(SETTINGS_KEY, data, SAVE_FILE);
                Debug.Log("[SaveManager] Settings saved");
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveManager] Failed to save settings: {e.Message}");
            }
        }

        /// <summary>
        /// Load player settings.
        /// </summary>
        /// <returns>Loaded settings, or defaults if none exist.</returns>
        public SettingsData LoadSettings()
        {
            try
            {
                if (ES3.KeyExists(SETTINGS_KEY, SAVE_FILE))
                {
                    var data = ES3.Load<SettingsData>(SETTINGS_KEY, SAVE_FILE);
                    Debug.Log("[SaveManager] Settings loaded");
                    return data;
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveManager] Failed to load settings: {e.Message}");
            }

            Debug.Log("[SaveManager] Using default settings");
            return new SettingsData();
        }

        // ============================================
        // Meta Progression Save/Load
        // ============================================

        /// <summary>
        /// Save meta-progression data.
        /// </summary>
        /// <param name="data">Meta data to save.</param>
        public void SaveMeta(MetaSaveData data)
        {
            if (data == null)
            {
                Debug.LogWarning("[SaveManager] Cannot save null meta data");
                return;
            }

            try
            {
                ES3.Save(META_KEY, data, SAVE_FILE);
                Debug.Log("[SaveManager] Meta data saved");
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveManager] Failed to save meta: {e.Message}");
            }
        }

        /// <summary>
        /// Load meta-progression data.
        /// </summary>
        /// <returns>Loaded meta data, or defaults if none exist.</returns>
        public MetaSaveData LoadMeta()
        {
            try
            {
                if (ES3.KeyExists(META_KEY, SAVE_FILE))
                {
                    var data = ES3.Load<MetaSaveData>(META_KEY, SAVE_FILE);
                    Debug.Log("[SaveManager] Meta data loaded");
                    return data;
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveManager] Failed to load meta: {e.Message}");
            }

            Debug.Log("[SaveManager] Using default meta data");
            return new MetaSaveData();
        }

        // ============================================
        // Battle Mission Progress Save/Load
        // ============================================

        /// <summary>
        /// Save Battle Mission progress (zone clears, difficulty unlocks).
        /// </summary>
        /// <param name="data">Battle mission progress data to save.</param>
        public void SaveBattleMissionProgress(BattleMissionSaveData data)
        {
            if (data == null)
            {
                Debug.LogWarning("[SaveManager] Cannot save null battle mission data");
                return;
            }

            try
            {
                ES3.Save(BATTLE_MISSION_KEY, data, SAVE_FILE);
                Debug.Log("[SaveManager] Battle mission progress saved");
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveManager] Failed to save battle mission progress: {e.Message}");
            }
        }

        /// <summary>
        /// Load Battle Mission progress.
        /// </summary>
        /// <returns>Loaded battle mission data, or defaults if none exist.</returns>
        public BattleMissionSaveData LoadBattleMissionProgress()
        {
            try
            {
                if (ES3.KeyExists(BATTLE_MISSION_KEY, SAVE_FILE))
                {
                    var data = ES3.Load<BattleMissionSaveData>(BATTLE_MISSION_KEY, SAVE_FILE);
                    Debug.Log("[SaveManager] Battle mission progress loaded");
                    return data;
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveManager] Failed to load battle mission progress: {e.Message}");
            }

            Debug.Log("[SaveManager] Using default battle mission data");
            return new BattleMissionSaveData();
        }

        /// <summary>
        /// Check if Battle Mission progress exists.
        /// </summary>
        public bool HasBattleMissionProgress
        {
            get
            {
                try
                {
                    return ES3.KeyExists(BATTLE_MISSION_KEY, SAVE_FILE);
                }
                catch
                {
                    return false;
                }
            }
        }

        // ============================================
        // Utility Methods
        // ============================================

        /// <summary>
        /// Delete all save data (run, settings, meta).
        /// Use with caution - this is irreversible.
        /// </summary>
        public void DeleteAllData()
        {
            try
            {
                if (ES3.FileExists(SAVE_FILE))
                {
                    ES3.DeleteFile(SAVE_FILE);
                    Debug.Log("[SaveManager] All save data deleted");
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveManager] Failed to delete all data: {e.Message}");
            }
        }

        /// <summary>
        /// Check if the save file exists.
        /// </summary>
        public bool SaveFileExists()
        {
            try
            {
                return ES3.FileExists(SAVE_FILE);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Get the size of the save file in bytes.
        /// </summary>
        public long GetSaveFileSize()
        {
            try
            {
                if (ES3.FileExists(SAVE_FILE))
                {
                    var fileInfo = new System.IO.FileInfo(SaveFilePath);
                    return fileInfo.Length;
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveManager] Failed to get file size: {e.Message}");
            }
            return 0;
        }

        /// <summary>
        /// Create a backup of the current save file.
        /// </summary>
        public void BackupSave()
        {
            try
            {
                if (ES3.FileExists(SAVE_FILE))
                {
                    string backupFile = $"HNR_Backup_{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}.es3";
                    ES3.CopyFile(SAVE_FILE, backupFile);
                    Debug.Log($"[SaveManager] Backup created: {backupFile}");
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveManager] Failed to create backup: {e.Message}");
            }
        }
    }
}
