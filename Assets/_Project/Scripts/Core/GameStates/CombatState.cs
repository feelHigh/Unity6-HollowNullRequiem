// ============================================
// CombatState.cs
// Combat state - active card combat encounter
// ============================================

using UnityEngine;
using UnityEngine.SceneManagement;
using DG.Tweening;
using HNR.Core.Events;
using HNR.Core.Interfaces;
using HNR.Combat;
using HNR.Progression;
using HNR.VFX;

namespace HNR.Core.GameStates
{
    /// <summary>
    /// Combat state handles active card combat encounters.
    /// Turn-based combat with the player's Requiem team against enemies.
    /// </summary>
    public class CombatState : IGameState
    {
        private readonly GameManager _manager;

        /// <summary>
        /// Creates a new CombatState.
        /// </summary>
        /// <param name="manager">Reference to the GameManager</param>
        public CombatState(GameManager manager)
        {
            _manager = manager;
        }

        /// <summary>
        /// Load Combat scene, initialize combat systems, and play music.
        /// </summary>
        public void Enter()
        {
            Debug.Log("[CombatState] Entering combat...");

            // Kill all DOTween animations to prevent null reference errors from previous scene
            DOTween.KillAll();

            // Subscribe to combat end event
            EventBus.Subscribe<CombatEndedEvent>(OnCombatEnded);

            // Load Combat scene
            if (SceneManager.GetActiveScene().name != "Combat")
            {
                SceneManager.LoadScene("Combat", LoadSceneMode.Single);
            }

            // TurnManager and CombatManager will initialize when scene loads
        }

        /// <summary>
        /// Handle combat end - mark node complete but let ResultsScreen handle navigation.
        /// </summary>
        private void OnCombatEnded(CombatEndedEvent evt)
        {
            Debug.Log($"[CombatState] Combat ended with victory={evt.Victory}");

            if (evt.Victory)
            {
                // Mark the current map node as completed in cached map data
                // (MapManager in NullRift scene was destroyed, so we update the cached data)
                if (ServiceLocator.TryGet<IRunManager>(out var runManagerInterface))
                {
                    var runManager = runManagerInterface as HNR.Progression.RunManager;
                    var cachedMapData = runManager?.GetCachedMapData();
                    if (cachedMapData != null && !string.IsNullOrEmpty(cachedMapData.CurrentNodeId))
                    {
                        // Find the current node in visited nodes and mark as completed
                        var currentNodeEntry = cachedMapData.VisitedNodes.Find(v => v.NodeId == cachedMapData.CurrentNodeId);
                        if (currentNodeEntry != null)
                        {
                            currentNodeEntry.Completed = true;
                        }
                        else
                        {
                            // Add current node to visited list as completed
                            cachedMapData.VisitedNodes.Add(new HNR.Progression.VisitedNode
                            {
                                NodeId = cachedMapData.CurrentNodeId,
                                Completed = true,
                                NodeType = "Combat"
                            });
                        }
                        Debug.Log($"[CombatState] Marked node {cachedMapData.CurrentNodeId} as completed in cached data");
                    }
                }

                // NOTE: Don't transition here - let ResultsScreen handle navigation
                // after user selects reward and clicks Continue.
                // CombatScreenCZN.OnCombatEnded() shows ResultsScreen which calls
                // GameManager.ChangeState(GameState.Run) when user clicks Continue.
            }
            else
            {
                // Go to results/game over immediately for defeat
                _manager.ChangeState(GameState.Results);
            }
        }

        /// <summary>
        /// Per-frame update for combat state.
        /// </summary>
        public void Update()
        {
            // Combat logic handled by CombatManager
        }

        /// <summary>
        /// Cleanup when leaving combat.
        /// </summary>
        public void Exit()
        {
            Debug.Log("[CombatState] Exiting combat...");

            // Kill all DOTween animations to prevent null reference errors after scene unload
            DOTween.KillAll();

            // Unsubscribe from combat end event
            EventBus.Unsubscribe<CombatEndedEvent>(OnCombatEnded);

            // Cleanup status effects
            if (ServiceLocator.TryGet<StatusEffectManager>(out var statusMgr))
            {
                statusMgr.ClearAllEffects();
            }

            // Return all VFX to pools
            if (ServiceLocator.TryGet<VFXPoolManager>(out var vfxPool))
            {
                vfxPool.ReturnAll();
            }

            // Combat results are applied by CombatManager before state transition
            // RunManager subscribes to CombatEndedEvent for reward/progress application
        }
    }
}
