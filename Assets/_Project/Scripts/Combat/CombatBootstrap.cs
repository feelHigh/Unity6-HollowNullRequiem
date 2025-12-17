// ============================================
// CombatBootstrap.cs
// Initializes combat when the Combat scene loads
// ============================================

using System.Collections.Generic;
using UnityEngine;
using HNR.Core;
using HNR.Core.Events;
using HNR.Core.Interfaces;
using HNR.Characters;
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
        private static bool _hasPendingCombat;

        /// <summary>
        /// Sets the pending combat encounter data before loading Combat scene.
        /// </summary>
        public static void SetPendingCombat(EncounterDataSO encounter, int zone)
        {
            _pendingEncounter = encounter;
            _pendingZone = zone;
            _hasPendingCombat = true;
            Debug.Log($"[CombatBootstrap] Pending combat set: {encounter?.EncounterName ?? "null"}, Zone {zone}");
        }

        /// <summary>
        /// Clears any pending combat data.
        /// </summary>
        public static void ClearPendingCombat()
        {
            _pendingEncounter = null;
            _pendingZone = 1;
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
            turnManager.StartCombat(teamList, enemies);

            // Show combat UI
            ShowCombatScreen();

            // Clear pending data
            ClearPendingCombat();

            Debug.Log($"[CombatBootstrap] Combat started: {encounter.EncounterName} with {enemies.Count} enemies vs {teamList.Count} Requiems");
        }

        private void ShowCombatScreen()
        {
            // Try to show via UIManager first
            if (ServiceLocator.TryGet<IUIManager>(out var uiManager))
            {
                uiManager.ShowScreen<CombatScreenCZN>();
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
