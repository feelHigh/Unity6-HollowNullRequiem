// ============================================
// EncounterManager.cs
// Manages enemy spawning and encounter setup
// ============================================

using System.Collections.Generic;
using UnityEngine;
using HNR.Core;
using HNR.Core.Events;
using HNR.Characters;

namespace HNR.Combat
{
    /// <summary>
    /// Spawns and manages enemy formations for combat encounters.
    /// Handles zone scaling, positioning, and cleanup.
    /// </summary>
    public class EncounterManager : MonoBehaviour
    {
        // ============================================
        // Configuration
        // ============================================

        [Header("Spawn Configuration")]
        [SerializeField, Tooltip("Transform positions for enemy spawning (up to 3)")]
        private Transform[] _enemySlots;

        [SerializeField, Tooltip("Enemy prefab to instantiate")]
        private EnemyInstance _enemyPrefab;

        [Header("Encounter Pools")]
        [SerializeField, Tooltip("Available encounters for Zone 1")]
        private List<EncounterDataSO> _zone1Encounters = new();

        [SerializeField, Tooltip("Available encounters for Zone 2")]
        private List<EncounterDataSO> _zone2Encounters = new();

        [SerializeField, Tooltip("Available encounters for Zone 3")]
        private List<EncounterDataSO> _zone3Encounters = new();

        [SerializeField, Tooltip("Elite encounters")]
        private List<EncounterDataSO> _eliteEncounters = new();

        [SerializeField, Tooltip("Boss encounters")]
        private List<EncounterDataSO> _bossEncounters = new();

        // ============================================
        // State
        // ============================================

        private List<EnemyInstance> _spawnedEnemies = new();
        private EncounterDataSO _currentEncounter;
        private int _currentZone = 1;

        // ============================================
        // Properties
        // ============================================

        /// <summary>Currently spawned enemies.</summary>
        public IReadOnlyList<EnemyInstance> SpawnedEnemies => _spawnedEnemies;

        /// <summary>Current encounter data.</summary>
        public EncounterDataSO CurrentEncounter => _currentEncounter;

        /// <summary>Number of living enemies.</summary>
        public int LivingEnemyCount
        {
            get
            {
                int count = 0;
                foreach (var enemy in _spawnedEnemies)
                {
                    if (enemy != null && !enemy.IsDead)
                        count++;
                }
                return count;
            }
        }

        // ============================================
        // Lifecycle
        // ============================================

        private void Awake()
        {
            ServiceLocator.Register(this);
        }

        private void OnDestroy()
        {
            ServiceLocator.Unregister<EncounterManager>();
        }

        // ============================================
        // Encounter Spawning
        // ============================================

        /// <summary>
        /// Spawn enemies for a specific encounter.
        /// </summary>
        /// <param name="encounter">Encounter configuration</param>
        /// <param name="zone">Current zone for scaling</param>
        /// <returns>List of spawned enemies</returns>
        public List<EnemyInstance> SpawnEncounter(EncounterDataSO encounter, int zone)
        {
            if (encounter == null)
            {
                Debug.LogError("[EncounterManager] Cannot spawn null encounter");
                return new List<EnemyInstance>();
            }

            ClearEnemies();
            _currentEncounter = encounter;
            _currentZone = zone;

            // Determine which enemies to spawn
            List<EnemyDataSO> enemiesToSpawn;

            if (encounter.HasFixedFormation)
            {
                // Use fixed formation for bosses/special encounters
                enemiesToSpawn = new List<EnemyDataSO>(encounter.FixedFormation);
            }
            else
            {
                // Random selection from pool
                enemiesToSpawn = SelectRandomEnemies(encounter);
            }

            // Spawn each enemy
            for (int i = 0; i < enemiesToSpawn.Count && i < _enemySlots.Length; i++)
            {
                var enemy = SpawnEnemy(enemiesToSpawn[i], zone, i);
                if (enemy != null)
                {
                    _spawnedEnemies.Add(enemy);
                }
            }

            // Set up arena effects
            if (encounter.ArenaCorruptionPerTurn > 0)
            {
                SetupArenaCorruption(encounter.ArenaCorruptionPerTurn);
            }

            Debug.Log($"[EncounterManager] Spawned {_spawnedEnemies.Count} enemies for '{encounter.EncounterId}' in Zone {zone}");

            // Publish event
            EventBus.Publish(new EncounterStartedEvent(encounter, _spawnedEnemies));

            return new List<EnemyInstance>(_spawnedEnemies);
        }

        /// <summary>
        /// Spawn a random encounter for the current zone.
        /// </summary>
        /// <param name="zone">Zone number (1-3)</param>
        /// <param name="isElite">Whether to spawn an elite encounter</param>
        /// <returns>List of spawned enemies</returns>
        public List<EnemyInstance> SpawnRandomEncounter(int zone, bool isElite = false)
        {
            var pool = GetEncounterPool(zone, isElite);

            if (pool.Count == 0)
            {
                Debug.LogWarning($"[EncounterManager] No encounters available for Zone {zone}, Elite: {isElite}");
                return new List<EnemyInstance>();
            }

            var encounter = pool[Random.Range(0, pool.Count)];
            return SpawnEncounter(encounter, zone);
        }

        /// <summary>
        /// Spawn the boss encounter for a zone.
        /// </summary>
        /// <param name="zone">Zone number</param>
        /// <returns>List of spawned enemies (usually 1 boss)</returns>
        public List<EnemyInstance> SpawnBossEncounter(int zone)
        {
            var bossEncounter = _bossEncounters.Find(e => e.Zone == zone);

            if (bossEncounter == null)
            {
                Debug.LogWarning($"[EncounterManager] No boss encounter for Zone {zone}");
                return new List<EnemyInstance>();
            }

            return SpawnEncounter(bossEncounter, zone);
        }

        // ============================================
        // Enemy Selection
        // ============================================

        private List<EnemyDataSO> SelectRandomEnemies(EncounterDataSO encounter)
        {
            var result = new List<EnemyDataSO>();

            if (encounter.EnemyPool.Count == 0)
            {
                Debug.LogWarning($"[EncounterManager] Encounter '{encounter.EncounterId}' has empty enemy pool");
                return result;
            }

            int count = Random.Range(encounter.MinEnemies, encounter.MaxEnemies + 1);
            count = Mathf.Min(count, _enemySlots.Length);

            for (int i = 0; i < count; i++)
            {
                var enemyData = encounter.EnemyPool[Random.Range(0, encounter.EnemyPool.Count)];
                result.Add(enemyData);
            }

            return result;
        }

        private List<EncounterDataSO> GetEncounterPool(int zone, bool isElite)
        {
            if (isElite)
            {
                return _eliteEncounters.FindAll(e => e.Zone == zone || e.Zone == 0);
            }

            return zone switch
            {
                1 => _zone1Encounters,
                2 => _zone2Encounters,
                3 => _zone3Encounters,
                _ => _zone1Encounters
            };
        }

        // ============================================
        // Enemy Spawning
        // ============================================

        private EnemyInstance SpawnEnemy(EnemyDataSO data, int zone, int slotIndex)
        {
            if (data == null || _enemyPrefab == null)
            {
                Debug.LogError("[EncounterManager] Missing enemy data or prefab");
                return null;
            }

            if (slotIndex >= _enemySlots.Length)
            {
                Debug.LogWarning($"[EncounterManager] Slot index {slotIndex} out of range");
                return null;
            }

            var spawnPosition = _enemySlots[slotIndex].position;
            var enemy = Instantiate(_enemyPrefab, spawnPosition, Quaternion.identity, _enemySlots[slotIndex]);
            enemy.Initialize(data, zone);

            Debug.Log($"[EncounterManager] Spawned {data.EnemyName} at slot {slotIndex}");

            return enemy;
        }

        // ============================================
        // Arena Effects
        // ============================================

        private void SetupArenaCorruption(int corruptionPerTurn)
        {
            if (ServiceLocator.TryGet<CorruptionManager>(out var corruptionManager))
            {
                corruptionManager.SetArenaCorruption(corruptionPerTurn);
                Debug.Log($"[EncounterManager] Arena corruption set to {corruptionPerTurn}/turn");
            }
        }

        // ============================================
        // Cleanup
        // ============================================

        /// <summary>
        /// Remove all spawned enemies.
        /// </summary>
        public void ClearEnemies()
        {
            foreach (var enemy in _spawnedEnemies)
            {
                if (enemy != null)
                {
                    Destroy(enemy.gameObject);
                }
            }

            _spawnedEnemies.Clear();
            _currentEncounter = null;

            // Reset arena effects
            if (ServiceLocator.TryGet<CorruptionManager>(out var corruptionManager))
            {
                corruptionManager.SetArenaCorruption(0);
            }
        }

        // ============================================
        // Queries
        // ============================================

        /// <summary>
        /// Get all living enemies.
        /// </summary>
        public List<EnemyInstance> GetLivingEnemies()
        {
            var result = new List<EnemyInstance>();
            foreach (var enemy in _spawnedEnemies)
            {
                if (enemy != null && !enemy.IsDead)
                {
                    result.Add(enemy);
                }
            }
            return result;
        }

        /// <summary>
        /// Check if all enemies are defeated.
        /// </summary>
        public bool AllEnemiesDefeated()
        {
            return LivingEnemyCount == 0;
        }

        /// <summary>
        /// Get a random living enemy.
        /// </summary>
        public EnemyInstance GetRandomLivingEnemy()
        {
            var living = GetLivingEnemies();
            if (living.Count == 0) return null;
            return living[Random.Range(0, living.Count)];
        }
    }

    // ============================================
    // Events
    // ============================================

    /// <summary>
    /// Published when an encounter starts.
    /// </summary>
    public class EncounterStartedEvent : GameEvent
    {
        public EncounterDataSO Encounter { get; }
        public IReadOnlyList<EnemyInstance> Enemies { get; }

        public EncounterStartedEvent(EncounterDataSO encounter, List<EnemyInstance> enemies)
        {
            Encounter = encounter;
            Enemies = enemies.AsReadOnly();
        }
    }
}
