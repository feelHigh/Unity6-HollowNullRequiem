// ============================================
// CombatState.cs
// Combat state - active card combat encounter
// ============================================

using UnityEngine;
using UnityEngine.SceneManagement;
using HNR.Core.Interfaces;
using HNR.Combat;
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

            // Load Combat scene
            if (SceneManager.GetActiveScene().name != "Combat")
            {
                SceneManager.LoadScene("Combat", LoadSceneMode.Single);
            }

            // TurnManager and CombatManager will initialize when scene loads
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
