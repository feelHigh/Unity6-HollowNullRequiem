// ============================================
// CombatBootstrap.cs
// Initializes combat when the Combat scene loads
// ============================================

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HNR.Core;
using HNR.Core.Events;
using HNR.Core.Interfaces;
using HNR.Characters;
using HNR.Map;
using HNR.UI;
using HNR.UI.Screens;

namespace HNR.Combat
{
    /// <summary>
    /// Bootstraps combat when the Combat scene loads.
    /// Reads pending encounter data and initializes all combat systems.
    /// </summary>
    public class CombatBootstrap : MonoBehaviour
    {
        // ============================================
        // Static Pending Combat Data
        // ============================================

        private static EncounterDataSO _pendingEncounter;
        private static int _pendingZone = 1;
        private static NodeType _pendingNodeType = NodeType.Combat;
        private static bool _isPreBossEncounter;
        private static bool _isFinalNode;
        private static bool _hasPendingCombat;

        /// <summary>
        /// Gets the node type of the pending/current combat encounter.
        /// </summary>
        public static NodeType PendingNodeType => _pendingNodeType;

        /// <summary>
        /// Gets the zone of the pending/current combat encounter.
        /// </summary>
        public static int PendingZone => _pendingZone;

        /// <summary>
        /// Whether this encounter is in the pre-boss column (last combat before boss).
        /// Used for playing elite themes regardless of actual node type.
        /// </summary>
        public static bool IsPreBossEncounter => _isPreBossEncounter;

        /// <summary>
        /// Whether this encounter is the final node of the zone (last column).
        /// Used for playing zone finale themes (elite/boss themes).
        /// </summary>
        public static bool IsFinalNode => _isFinalNode;

        /// <summary>
        /// Sets the pending combat encounter data before loading Combat scene.
        /// </summary>
        public static void SetPendingCombat(EncounterDataSO encounter, int zone, NodeType nodeType = NodeType.Combat, bool isPreBoss = false, bool isFinalNode = false)
        {
            _pendingEncounter = encounter;
            _pendingZone = zone;
            _pendingNodeType = nodeType;
            _isPreBossEncounter = isPreBoss;
            _isFinalNode = isFinalNode;
            _hasPendingCombat = true;
            Debug.Log($"[CombatBootstrap] Pending combat set: {encounter?.EncounterName ?? "null"}, Zone {zone}, NodeType {nodeType}, PreBoss {isPreBoss}, FinalNode {isFinalNode}");
        }

        /// <summary>
        /// Clears any pending combat data.
        /// </summary>
        public static void ClearPendingCombat()
        {
            _pendingEncounter = null;
            _pendingZone = 1;
            _pendingNodeType = NodeType.Combat;
            _isPreBossEncounter = false;
            _isFinalNode = false;
            _hasPendingCombat = false;
        }

        // ============================================
        // Inspector Configuration
        // ============================================

        [Header("UI References")]
        [SerializeField, Tooltip("Combat screen to show")]
        private CombatScreen _combatScreen;

        [Header("Fallback Configuration")]
        [SerializeField, Tooltip("Default encounter if none pending")]
        private EncounterDataSO _fallbackEncounter;

        // ============================================
        // Unity Lifecycle
        // ============================================

        private void Start()
        {
            // Delay initialization to ensure all managers are ready
            Invoke(nameof(InitializeCombat), 0.1f);
        }

        // ============================================
        // Combat Initialization
        // ============================================

        private void InitializeCombat()
        {
            Debug.Log("[CombatBootstrap] Initializing combat...");

            // Get encounter data
            EncounterDataSO encounter = _hasPendingCombat ? _pendingEncounter : _fallbackEncounter;
            int zone = _hasPendingCombat ? _pendingZone : 1;

            if (encounter == null)
            {
                Debug.LogError("[CombatBootstrap] No encounter data available!");
                ShowErrorAndReturn();
                return;
            }

            // Get team from RunManager
            var runManager = ServiceLocator.Get<IRunManager>();
            if (runManager == null)
            {
                Debug.LogError("[CombatBootstrap] RunManager not found in ServiceLocator!");
                Debug.LogError($"[CombatBootstrap] ServiceLocator has IRunManager: {ServiceLocator.Has<IRunManager>()}");
                ShowErrorAndReturn();
                return;
            }

            Debug.Log($"[CombatBootstrap] RunManager found. IsRunActive: {runManager.IsRunActive}, Team count: {runManager.Team?.Count ?? -1}");

            if (runManager.Team == null || runManager.Team.Count == 0)
            {
                Debug.LogError("[CombatBootstrap] RunManager has no team! IsRunActive: " + runManager.IsRunActive);
                ShowErrorAndReturn();
                return;
            }

            // Spawn enemies
            var encounterManager = ServiceLocator.Get<EncounterManager>();
            if (encounterManager == null)
            {
                Debug.LogError("[CombatBootstrap] EncounterManager not found!");
                ShowErrorAndReturn();
                return;
            }

            List<EnemyInstance> enemies = encounterManager.SpawnEncounter(encounter, zone);
            if (enemies.Count == 0)
            {
                Debug.LogWarning("[CombatBootstrap] No enemies spawned - encounter may have empty enemy pool");
                // Continue anyway to allow testing
            }

            // Get TurnManager and start combat
            var turnManager = ServiceLocator.Get<TurnManager>();
            if (turnManager == null)
            {
                Debug.LogError("[CombatBootstrap] TurnManager not found!");
                ShowErrorAndReturn();
                return;
            }

            // Convert IReadOnlyList to List for TurnManager
            var teamList = new List<RequiemInstance>(runManager.Team);

            // Start combat initialization coroutine to ensure proper timing
            StartCoroutine(StartCombatAfterUIReady(turnManager, teamList, enemies, encounter));
        }

        /// <summary>
        /// Coroutine that waits for UI to be ready before starting combat.
        /// This ensures CombatScreen subscribes to events before cards are drawn.
        /// </summary>
        private IEnumerator StartCombatAfterUIReady(TurnManager turnManager, List<RequiemInstance> teamList, List<EnemyInstance> enemies, EncounterDataSO encounter)
        {
            // Show combat UI first
            ShowCombatScreen();

            // Wait for UIManager transition to complete (OnShow gets called during transition)
            // UIManager uses FadeOut -> OnShow -> FadeIn, so we wait until transition is done
            if (ServiceLocator.TryGet<IUIManager>(out var uiManager) && uiManager is UIManager uiMgr)
            {
                while (uiMgr.IsTransitioning)
                {
                    yield return null;
                }
            }
            else
            {
                // Fallback: wait a frame to ensure OnShow has been called
                yield return null;
            }

            Debug.Log("[CombatBootstrap] UI ready, starting combat...");

            // Now start combat - DrawPhase will publish CardDrawnEvent which CombatScreen will receive
            turnManager.StartCombat(teamList, enemies);

            // Clear pending data
            ClearPendingCombat();

            Debug.Log($"[CombatBootstrap] Combat started: {encounter.EncounterName} with {enemies.Count} enemies vs {teamList.Count} Requiems");
        }

        private void ShowCombatScreen()
        {
            // Try to show via UIManager first
            if (ServiceLocator.TryGet<IUIManager>(out var uiManager))
            {
                uiManager.ShowScreen<CombatScreen>();
                return;
            }

            // Fallback: activate direct reference
            if (_combatScreen != null)
            {
                _combatScreen.gameObject.SetActive(true);
                _combatScreen.OnShow();
            }
            else
            {
                Debug.LogWarning("[CombatBootstrap] No CombatScreen available");
            }
        }

        private void ShowErrorAndReturn()
        {
            Debug.LogError("[CombatBootstrap] Combat initialization failed - returning to map");

            // Return to Run state
            if (ServiceLocator.TryGet<IGameManager>(out var gameManager))
            {
                gameManager.ChangeState(GameState.Run);
            }
        }
    }
}
