// ============================================
// CombatTestManager.cs
// Test harness for combat system
// ============================================

using System.Collections.Generic;
using UnityEngine;
using HNR.Core;
using HNR.Core.Events;
using HNR.Combat;
using HNR.Cards;

// Resolve ambiguity: use real types from proper namespaces
using RequiemDataSO = HNR.Characters.RequiemDataSO;
using EnemyInstance = HNR.Combat.EnemyInstance;

namespace HNR.Testing
{
    /// <summary>
    /// Test harness for combat system.
    /// Press T to start test combat, Space to end turn.
    /// </summary>
    public class CombatTestManager : MonoBehaviour
    {
        [Header("Test Data")]
        [SerializeField, Tooltip("Requiem data for test team")]
        private RequiemDataSO _testRequiem;

        [SerializeField, Tooltip("Enemy data for test combat")]
        private EnemyDataSO _testEnemy;

        [SerializeField, Tooltip("Number of test Requiems")]
        private int _teamSize = 1;

        [SerializeField, Tooltip("Number of test enemies")]
        private int _enemyCount = 1;

        [Header("Debug")]
        [SerializeField] private bool _logEvents = true;

        private void Start()
        {
            if (_logEvents)
            {
                SubscribeToEvents();
            }

            Debug.Log("[CombatTestManager] Press T to start test combat, Space to end turn");
        }

        private void OnDestroy()
        {
            if (_logEvents)
            {
                UnsubscribeFromEvents();
            }
        }

        private void Update()
        {
            // T = Start test combat
            if (Input.GetKeyDown(KeyCode.T))
            {
                StartTestCombat();
            }

            // Space = End turn
            if (Input.GetKeyDown(KeyCode.Space))
            {
                EndTurn();
            }

            // D = Deal damage to team (debug)
            if (Input.GetKeyDown(KeyCode.D))
            {
                DealTestDamage();
            }

            // B = Add block (debug)
            if (Input.GetKeyDown(KeyCode.B))
            {
                AddTestBlock();
            }

            // K = Kill first enemy (debug)
            if (Input.GetKeyDown(KeyCode.K))
            {
                KillFirstEnemy();
            }
        }

        private void StartTestCombat()
        {
            Debug.Log("[CombatTestManager] Starting test combat...");

            // Create test team
            var team = new List<RequiemInstance>();
            for (int i = 0; i < _teamSize; i++)
            {
                var requiem = new RequiemInstance
                {
                    Data = _testRequiem,
                    MaxHP = _testRequiem != null ? _testRequiem.BaseHP : 100,
                    CurrentHP = _testRequiem != null ? _testRequiem.BaseHP : 100
                };
                team.Add(requiem);
            }

            // Create test enemies (EnemyInstance is a MonoBehaviour)
            var enemies = new List<EnemyInstance>();
            for (int i = 0; i < _enemyCount; i++)
            {
                var enemyGO = new GameObject($"TestEnemy_{i}");
                var enemy = enemyGO.AddComponent<EnemyInstance>();
                if (_testEnemy != null)
                {
                    enemy.Initialize(_testEnemy, 1);
                }
                enemies.Add(enemy);
            }

            // Start combat
            if (ServiceLocator.TryGet<TurnManager>(out var turnManager))
            {
                turnManager.StartCombat(team, enemies);
            }
            else
            {
                Debug.LogError("[CombatTestManager] TurnManager not found!");
            }
        }

        private void EndTurn()
        {
            if (ServiceLocator.TryGet<TurnManager>(out var turnManager))
            {
                if (turnManager.IsPlayerTurn)
                {
                    turnManager.EndPlayerTurn();
                }
                else
                {
                    Debug.Log("[CombatTestManager] Not player's turn");
                }
            }
        }

        private void DealTestDamage()
        {
            if (ServiceLocator.TryGet<TurnManager>(out var turnManager))
            {
                turnManager.DamageTeam(10);
                Debug.Log("[CombatTestManager] Dealt 10 damage to team");
            }
        }

        private void AddTestBlock()
        {
            if (ServiceLocator.TryGet<TurnManager>(out var turnManager))
            {
                turnManager.AddTeamBlock(5);
                Debug.Log("[CombatTestManager] Added 5 block to team");
            }
        }

        private void KillFirstEnemy()
        {
            if (ServiceLocator.TryGet<TurnManager>(out var turnManager))
            {
                var context = turnManager.Context;
                if (context.Enemies.Count > 0)
                {
                    var enemy = context.Enemies[0];
                    // Deal massive damage to kill instantly
                    enemy.TakeDamage(9999);
                    Debug.Log("[CombatTestManager] Killed first enemy");
                }
            }
        }

        // ============================================
        // Event Logging
        // ============================================

        private void SubscribeToEvents()
        {
            EventBus.Subscribe<CombatStartedEvent>(e => Debug.Log($"[Event] Combat Started: {e.Enemies?.Count ?? 0} enemies"));
            EventBus.Subscribe<CombatEndedEvent>(e => Debug.Log($"[Event] Combat Ended: {(e.Victory ? "Victory" : "Defeat")}"));
            EventBus.Subscribe<TurnStartedEvent>(e => Debug.Log($"[Event] Turn {e.TurnNumber} Started: {(e.IsPlayerTurn ? "Player" : "Enemy")}"));
            EventBus.Subscribe<TurnEndedEvent>(e => Debug.Log($"[Event] Turn Ended: {(e.WasPlayerTurn ? "Player" : "Enemy")}"));
            EventBus.Subscribe<APChangedEvent>(e => Debug.Log($"[Event] AP: {e.CurrentAP}/{e.MaxAP}"));
            EventBus.Subscribe<TeamHPChangedEvent>(e => Debug.Log($"[Event] Team HP: {e.CurrentHP}/{e.MaxHP} (delta: {e.Delta})"));
            EventBus.Subscribe<BlockChangedEvent>(e => Debug.Log($"[Event] Block: {e.Block}"));
            EventBus.Subscribe<CardDrawnEvent>(e => Debug.Log($"[Event] Card Drawn: {e.Card?.Data?.CardName ?? "Unknown"}"));
            EventBus.Subscribe<CombatPhaseChangedEvent>(e => Debug.Log($"[Event] Phase: {e.PreviousPhase} -> {e.NewPhase}"));
        }

        private void UnsubscribeFromEvents()
        {
            // Note: Anonymous lambdas can't be unsubscribed
            // In production, use named methods
        }
    }
}
