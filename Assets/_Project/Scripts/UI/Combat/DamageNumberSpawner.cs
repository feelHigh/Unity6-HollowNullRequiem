// ============================================
// DamageNumberSpawner.cs
// Service for spawning pooled damage numbers
// ============================================

using System.Collections.Generic;
using UnityEngine;
using HNR.Core;
using HNR.Core.Events;

namespace HNR.UI
{
    /// <summary>
    /// Manages spawning and pooling of damage number UI elements.
    /// Subscribes to combat events and spawns numbers at target positions.
    /// </summary>
    public class DamageNumberSpawner : MonoBehaviour
    {
        // ============================================
        // Pool Settings
        // ============================================

        [Header("Pool Settings")]
        [SerializeField, Tooltip("Prefab for damage number UI")]
        private DamageNumberUI _prefab;

        [SerializeField, Tooltip("Container for spawned numbers")]
        private RectTransform _container;

        [SerializeField, Tooltip("Initial pool size")]
        private int _poolSize = 10;

        // ============================================
        // Position Settings
        // ============================================

        [Header("Position Offset")]
        [SerializeField, Tooltip("Random X/Y offset range for variety")]
        private Vector2 _randomOffset = new(20f, 10f);

        [SerializeField, Tooltip("Base Y offset above target")]
        private float _baseYOffset = 50f;

        // ============================================
        // Runtime State
        // ============================================

        private Queue<DamageNumberUI> _pool = new();
        private Camera _mainCamera;

        // ============================================
        // Unity Lifecycle
        // ============================================

        private void Awake()
        {
            _mainCamera = Camera.main;
            InitializePool();
        }

        private void OnEnable()
        {
            EventBus.Subscribe<DamageDealtEvent>(OnDamageDealt);
            EventBus.Subscribe<HealingReceivedEvent>(OnHealingReceived);
            EventBus.Subscribe<BlockGainedEvent>(OnBlockGained);
            EventBus.Subscribe<CorruptionChangedEvent>(OnCorruptionChanged);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<DamageDealtEvent>(OnDamageDealt);
            EventBus.Unsubscribe<HealingReceivedEvent>(OnHealingReceived);
            EventBus.Unsubscribe<BlockGainedEvent>(OnBlockGained);
            EventBus.Unsubscribe<CorruptionChangedEvent>(OnCorruptionChanged);
        }

        // ============================================
        // Pool Management
        // ============================================

        private void InitializePool()
        {
            for (int i = 0; i < _poolSize; i++)
            {
                CreatePooledInstance();
            }
        }

        private DamageNumberUI CreatePooledInstance()
        {
            var instance = Instantiate(_prefab, _container);
            instance.gameObject.SetActive(false);
            _pool.Enqueue(instance);
            return instance;
        }

        private DamageNumberUI GetFromPool()
        {
            // Expand pool if empty
            if (_pool.Count == 0)
            {
                Debug.Log("[DamageNumberSpawner] Pool empty, creating new instance");
                CreatePooledInstance();
            }

            var instance = _pool.Dequeue();
            instance.gameObject.SetActive(true);
            instance.OnSpawnFromPool();
            return instance;
        }

        private void ReturnToPool(DamageNumberUI instance)
        {
            instance.OnReturnToPool();
            instance.gameObject.SetActive(false);
            _pool.Enqueue(instance);
        }

        // ============================================
        // Event Handlers
        // ============================================

        private void OnDamageDealt(DamageDealtEvent evt)
        {
            if (evt.Target == null || evt.Amount <= 0) return;
            SpawnNumber(evt.Target.Position, evt.Amount, DamageNumberType.Damage, evt.IsCritical);
        }

        private void OnHealingReceived(HealingReceivedEvent evt)
        {
            if (evt.Target == null || evt.Amount <= 0) return;
            SpawnNumber(evt.Target.Position, evt.Amount, DamageNumberType.Heal, false);
        }

        private void OnBlockGained(BlockGainedEvent evt)
        {
            if (evt.Target == null || evt.Amount <= 0) return;
            SpawnNumber(evt.Target.Position, evt.Amount, DamageNumberType.Block, false);
        }

        private void OnCorruptionChanged(CorruptionChangedEvent evt)
        {
            // Only show for corruption gained (positive delta)
            if (evt.Requiem == null || evt.Delta <= 0) return;
            SpawnNumber(evt.Requiem.Position, evt.Delta, DamageNumberType.Corruption, false);
        }

        // ============================================
        // Spawn Methods
        // ============================================

        /// <summary>
        /// Spawn a damage number at a world position.
        /// </summary>
        /// <param name="worldPos">World position of the target.</param>
        /// <param name="value">Numeric value to display.</param>
        /// <param name="type">Type of number.</param>
        /// <param name="isCritical">Whether this is a critical hit.</param>
        public void SpawnNumber(Vector3 worldPos, int value, DamageNumberType type, bool isCritical)
        {
            var instance = GetFromPool();

            // Convert world position to canvas position
            var screenPos = WorldToCanvasPosition(worldPos);

            // Add random offset for visual variety
            screenPos += new Vector2(
                Random.Range(-_randomOffset.x, _randomOffset.x),
                Random.Range(-_randomOffset.y, _randomOffset.y) + _baseYOffset
            );

            instance.GetComponent<RectTransform>().anchoredPosition = screenPos;
            instance.Show(value, type, isCritical, ReturnToPool);
        }

        /// <summary>
        /// Spawn a damage number directly at a screen position.
        /// </summary>
        /// <param name="screenPos">Screen position.</param>
        /// <param name="value">Numeric value to display.</param>
        /// <param name="type">Type of number.</param>
        /// <param name="isCritical">Whether this is a critical hit.</param>
        public void SpawnNumberAtScreenPosition(Vector2 screenPos, int value, DamageNumberType type, bool isCritical)
        {
            var instance = GetFromPool();

            // Add random offset
            screenPos += new Vector2(
                Random.Range(-_randomOffset.x, _randomOffset.x),
                Random.Range(-_randomOffset.y, _randomOffset.y) + _baseYOffset
            );

            instance.GetComponent<RectTransform>().anchoredPosition = screenPos;
            instance.Show(value, type, isCritical, ReturnToPool);
        }

        // ============================================
        // Utility Methods
        // ============================================

        /// <summary>
        /// Convert world position to canvas local position.
        /// </summary>
        private Vector2 WorldToCanvasPosition(Vector3 worldPos)
        {
            if (_mainCamera == null)
                _mainCamera = Camera.main;

            var screenPos = _mainCamera.WorldToScreenPoint(worldPos);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                _container, screenPos, null, out var localPos);
            return localPos;
        }

        /// <summary>
        /// Pre-warm the pool with additional instances.
        /// </summary>
        /// <param name="count">Number of instances to add.</param>
        public void PreWarmPool(int count)
        {
            for (int i = 0; i < count; i++)
            {
                CreatePooledInstance();
            }
        }
    }
}
