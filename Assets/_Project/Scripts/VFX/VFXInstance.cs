// ============================================
// VFXInstance.cs
// Component for pooled VFX particle effects
// ============================================

using UnityEngine;
using HNR.Core.Interfaces;

namespace HNR.VFX
{
    /// <summary>
    /// Component attached to VFX prefabs for pool management.
    /// Handles particle system control, auto-return, and target following.
    /// </summary>
    [RequireComponent(typeof(ParticleSystem))]
    public class VFXInstance : MonoBehaviour, IPoolable
    {
        // ============================================
        // Serialized Fields
        // ============================================

        [Header("Configuration")]
        [SerializeField, Tooltip("Auto-return to pool when particles finish")]
        private bool _autoReturn = true;

        [SerializeField, Tooltip("Override duration (0 = use particle system duration)")]
        private float _overrideDuration = 0f;

        [SerializeField, Tooltip("Scale effect with target")]
        private bool _scaleWithTarget = false;

        // ============================================
        // Properties
        // ============================================

        /// <summary>Effect identifier for pool management.</summary>
        public string EffectId { get; private set; }

        /// <summary>Whether this instance is currently active/playing.</summary>
        public bool IsPlaying => _particleSystem != null && _particleSystem.isPlaying;

        // ============================================
        // Private Fields
        // ============================================

        private ParticleSystem _particleSystem;
        private VFXPoolManager _poolManager;
        private Transform _followTarget;
        private Vector3 _followOffset;
        private float _returnTimer;
        private bool _isActive;

        // ============================================
        // Unity Lifecycle
        // ============================================

        private void Awake()
        {
            _particleSystem = GetComponent<ParticleSystem>();

            // Ensure particle system doesn't play on awake (pool handles activation)
            var main = _particleSystem.main;
            main.playOnAwake = false;
        }

        private void Update()
        {
            // Handle target following
            if (_followTarget != null && _isActive)
            {
                transform.position = _followTarget.position + _followOffset;

                if (_scaleWithTarget)
                {
                    transform.localScale = _followTarget.lossyScale;
                }
            }

            // Handle auto-return timer
            if (_autoReturn && _isActive && _returnTimer > 0)
            {
                _returnTimer -= Time.deltaTime;
                if (_returnTimer <= 0)
                {
                    ReturnToPool();
                }
            }
        }

        // ============================================
        // Initialization
        // ============================================

        /// <summary>
        /// Initialize the instance with pool reference.
        /// Called by VFXPoolManager when creating instances.
        /// </summary>
        public void Initialize(string effectId, VFXPoolManager poolManager)
        {
            EffectId = effectId;
            _poolManager = poolManager;
        }

        // ============================================
        // IPoolable Implementation
        // ============================================

        /// <summary>
        /// Called when spawned from pool.
        /// </summary>
        public void OnSpawnFromPool()
        {
            gameObject.SetActive(true);
            _isActive = true;
            _followTarget = null;
            _followOffset = Vector3.zero;

            // Calculate return timer
            if (_overrideDuration > 0)
            {
                _returnTimer = _overrideDuration;
            }
            else if (_particleSystem != null)
            {
                var main = _particleSystem.main;
                _returnTimer = main.duration + main.startLifetime.constantMax;
            }
            else
            {
                _returnTimer = 2f; // Fallback
            }

            // Play particle system
            if (_particleSystem != null)
            {
                _particleSystem.Clear();
                _particleSystem.Play();
            }
        }

        /// <summary>
        /// Called when returned to pool.
        /// </summary>
        public void OnReturnToPool()
        {
            _isActive = false;
            _followTarget = null;
            _followOffset = Vector3.zero;

            // Stop and clear particles
            if (_particleSystem != null)
            {
                _particleSystem.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            }

            gameObject.SetActive(false);
        }

        // ============================================
        // Public Methods
        // ============================================

        /// <summary>
        /// Set a target transform to follow.
        /// </summary>
        /// <param name="target">Transform to follow</param>
        /// <param name="offset">Offset from target position</param>
        public void SetFollowTarget(Transform target, Vector3 offset = default)
        {
            _followTarget = target;
            _followOffset = offset;
        }

        /// <summary>
        /// Stop the particle effect immediately.
        /// Does not return to pool automatically.
        /// </summary>
        public void Stop()
        {
            if (_particleSystem != null)
            {
                _particleSystem.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            }
        }

        /// <summary>
        /// Stop and immediately return to pool.
        /// </summary>
        public void StopAndReturn()
        {
            Stop();
            ReturnToPool();
        }

        /// <summary>
        /// Return this instance to the pool.
        /// </summary>
        public void ReturnToPool()
        {
            if (!_isActive) return;

            if (_poolManager != null)
            {
                _poolManager.Return(this);
            }
            else
            {
                // Fallback: just disable if no pool reference
                OnReturnToPool();
            }
        }

        /// <summary>
        /// Extend the active duration.
        /// </summary>
        /// <param name="additionalTime">Time to add in seconds</param>
        public void ExtendDuration(float additionalTime)
        {
            _returnTimer += additionalTime;
        }

        /// <summary>
        /// Set the emission rate multiplier.
        /// </summary>
        public void SetEmissionMultiplier(float multiplier)
        {
            if (_particleSystem == null) return;

            var emission = _particleSystem.emission;
            emission.rateOverTimeMultiplier = multiplier;
        }

        /// <summary>
        /// Set the particle start color.
        /// </summary>
        public void SetColor(Color color)
        {
            if (_particleSystem == null) return;

            var main = _particleSystem.main;
            main.startColor = color;
        }

        /// <summary>
        /// Set the particle system scale.
        /// </summary>
        public void SetScale(float scale)
        {
            transform.localScale = Vector3.one * scale;
        }
    }
}
