// ============================================
// RunState.cs
// Run state - active run navigation
// ============================================

using UnityEngine;
using UnityEngine.SceneManagement;
using DG.Tweening;
using HNR.Core.Interfaces;
using HNR.Map;
using HNR.UI;
using HNR.Progression;

namespace HNR.Core.GameStates
{
    /// <summary>
    /// Run state handles active run navigation through the Null Rift map.
    /// Players choose paths, encounter events, and enter combat nodes.
    /// </summary>
    public class RunState : IGameState
    {
        private readonly GameManager _manager;

        /// <summary>
        /// Creates a new RunState.
        /// </summary>
        /// <param name="manager">Reference to the GameManager</param>
        public RunState(GameManager manager)
        {
            _manager = manager;
        }

        /// <summary>
        /// Load NullRift scene, show map UI, and play music.
        /// </summary>
        public void Enter()
        {
            Debug.Log("[RunState] Starting run navigation...");

            // Kill all DOTween animations to prevent null reference errors from previous scene
            DOTween.KillAll();

            // Subscribe to scene loaded event before loading
            SceneManager.sceneLoaded += OnNullRiftSceneLoaded;

            // Load NullRift scene
            if (SceneManager.GetActiveScene().name != "NullRift")
            {
                SceneManager.LoadScene("NullRift", LoadSceneMode.Single);
            }
            else
            {
                // Already in NullRift, show UI directly
                OnNullRiftSceneLoaded(SceneManager.GetActiveScene(), LoadSceneMode.Single);
            }
        }

        private void OnNullRiftSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (scene.name != "NullRift") return;

            // Unsubscribe immediately
            SceneManager.sceneLoaded -= OnNullRiftSceneLoaded;

            Debug.Log("[RunState] NullRift scene loaded, initializing map...");

            // Get MapManager and RunManager
            var mapManager = ServiceLocator.Get<MapManager>();
            var runManagerInterface = ServiceLocator.Get<IRunManager>();
            var runManager = runManagerInterface as HNR.Progression.RunManager;

            Debug.Log($"[RunState] MapManager found: {mapManager != null}, RunManager found: {runManager != null}");

            if (mapManager != null)
            {
                Debug.Log($"[RunState] MapManager.HasActiveMap: {mapManager.HasActiveMap}");

                if (!mapManager.HasActiveMap)
                {
                    // Check for cached map state from RunManager (returning from combat)
                    var cachedMapData = runManager?.GetCachedMapData();
                    Debug.Log($"[RunState] Cached map data: {(cachedMapData != null ? $"Zone={cachedMapData.Zone}, CurrentNode={cachedMapData.CurrentNodeId}, Visited={cachedMapData.VisitedNodes.Count}, Accessible={cachedMapData.AccessibleNodeIds.Count}" : "NULL")}");

                    if (cachedMapData != null && !string.IsNullOrEmpty(cachedMapData.CurrentNodeId))
                    {
                        // Restore map state from cached data
                        mapManager.RestoreMapState(cachedMapData);
                        Debug.Log($"[RunState] Restored map from cached state: node {cachedMapData.CurrentNodeId}");
                    }
                    else
                    {
                        // Generate new map for fresh run
                        int zone = runManager?.CurrentZone ?? 1;
                        int seed = runManager?.RunSeed ?? -1;

                        mapManager.GenerateMap(zone, seed);
                        Debug.Log($"[RunState] Generated new map for zone {zone}, seed {seed}");
                    }
                }
                else
                {
                    Debug.Log($"[RunState] Map already active, CurrentNode: {mapManager.CurrentNode?.NodeId}");
                }
            }
            else
            {
                Debug.LogWarning("[RunState] MapManager not found in scene");
            }

            // Show MapScreen
            ShowMapScreen();

            // Play zone-appropriate music
            PlayZoneMusic(runManager?.CurrentZone ?? 1);
        }

        private void PlayZoneMusic(int zone)
        {
            if (ServiceLocator.TryGet<IAudioManager>(out var audioManager))
            {
                string musicId = zone switch
                {
                    1 => "music_zone1",
                    2 => "music_zone2",
                    3 => "music_zone3",
                    _ => "music_zone1"
                };

                audioManager.PlayMusic(musicId);
                Debug.Log($"[RunState] Playing zone {zone} music: {musicId}");
            }
        }

        private void ShowMapScreen()
        {
            if (ServiceLocator.TryGet<IUIManager>(out var uiManager))
            {
                uiManager.ShowScreen<MapScreen>();
                Debug.Log("[RunState] MapScreen shown");
            }
            else
            {
                Debug.LogWarning("[RunState] UIManager not available - cannot show MapScreen");
            }
        }

        /// <summary>
        /// Per-frame update for run state.
        /// </summary>
        public void Update()
        {
            // Map interactions handled by UI events
        }

        /// <summary>
        /// Cleanup when leaving run navigation.
        /// </summary>
        public void Exit()
        {
            Debug.Log("[RunState] Exiting run navigation...");

            // Ensure we're unsubscribed
            SceneManager.sceneLoaded -= OnNullRiftSceneLoaded;

            // Skip saving during application quit (editor play mode exit or app shutdown)
            // Services may already be destroyed at this point
            if (Application.isPlaying && !IsApplicationQuitting())
            {
                // Save run state
                if (ServiceLocator.TryGet<IRunManager>(out var runManager) && runManager.IsRunActive)
                {
                    runManager.SaveRun();
                }
            }
        }

        /// <summary>
        /// Check if the application is in the process of quitting.
        /// </summary>
        private static bool IsApplicationQuitting()
        {
#if UNITY_EDITOR
            return !UnityEditor.EditorApplication.isPlayingOrWillChangePlaymode;
#else
            return false;
#endif
        }
    }
}
