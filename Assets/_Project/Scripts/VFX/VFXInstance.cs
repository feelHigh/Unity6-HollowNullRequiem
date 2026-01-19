// ============================================
// VFXInstance.cs
// Component for pooled VFX particle effects
// ============================================

using System.Collections;
using UnityEngine;
using HNR.Core;
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
        [SerializeField, Tooltip("Default lifetime in seconds")]
        private float _lifetime = 2f;

        [SerializeField, Tooltip("Auto-return to pool when particles finish")]
        private bool _autoReturn = true;

        [SerializeField, Tooltip("Use particle system duration instead of _lifetime")]
        private bool _useParticleLifetime = true;

        // ============================================
        // Properties
        // ============================================

        /// <summary>Effect identifier for pool management.</summary>
        public string EffectId { get; set; }

        /// <summary>Whether this instance is currently active/playing.</summary>
        public bool IsPlaying => _particleSystem != null && _particleSystem.isPlaying;

        // ============================================
        // Private Fields
        // ============================================

        private ParticleSystem _particleSystem;
        private Transform _followTarget;
        private Vector3 _followOffset;
        private Coroutine _lifetimeCoroutine;

        // ============================================
        // Unity Lifecycle
        // ============================================

        private void Awake()
        {
            _particleSystem = GetComponent<ParticleSystem>();

            // Calculate lifetime from particle system if enabled
            if (_useParticleLifetime && _particleSystem != null)
            {
                var main = _particleSystem.main;
                _lifetime = main.duration + main.startLifetime.constantMax;
            }
        }

        private void LateUpdate()
        {
            // Handle target following in LateUpdate for smoother results
            if (_followTarget != null)
            {
                transform.position = _followTarget.position + _followOffset;
            }
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
            _followTarget = null;
            _followOffset = Vector3.zero;

            // Play particle system
            if (_particleSystem != null)
            {
                _particleSystem.Clear();
                _particleSystem.Play();
            }

            // Start auto-return coroutine
            if (_autoReturn)
            {
                _lifetimeCoroutine = StartCoroutine(AutoReturnRoutine());
            }
        }

        /// <summary>
        /// Called when returned to pool.
        /// </summary>
        public void OnReturnToPool()
        {
            // Stop coroutine if running
            if (_lifetimeCoroutine != null)
            {
                StopCoroutine(_lifetimeCoroutine);
                _lifetimeCoroutine = null;
            }

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
        /// Set target transform to follow.
        /// </summary>
        /// <param name="target">Transform to follow</param>
        /// <param name="offset">Offset from target position</param>
        public void SetTarget(Transform target, Vector3 offset = default)
        {
            _followTarget = target;
            _followOffset = offset;
        }

        /// <summary>
        /// Stop particle emission immediately.
        /// </summary>
        public void Stop()
        {
            if (_particleSystem != null)
            {
                _particleSystem.Stop();
            }
        }

        /// <summary>
        /// Play the effect.
        /// </summary>
        public void Play()
        {
            if (_particleSystem != null)
            {
                _particleSystem.Play();
            }
        }

        /// <summary>
        /// Set particle color.
        /// </summary>
        public void SetColor(Color color)
        {
            if (_particleSystem != null)
            {
                var main = _particleSystem.main;
                main.startColor = color;
            }
        }

        /// <summary>
        /// Set particle scale.
        /// </summary>
        public void SetScale(float scale)
        {
            transform.localScale = Vector3.one * scale;
        }

        /// <summary>
        /// Set whether this VFX should persist (not auto-return).
        /// Use for looping effects that need manual cleanup.
        /// </summary>
        /// <param name="persistent">True to disable auto-return</param>
        public void SetPersistent(bool persistent)
        {
            _autoReturn = !persistent;

            // Stop auto-return coroutine if making persistent
            if (persistent && _lifetimeCoroutine != null)
            {
                StopCoroutine(_lifetimeCoroutine);
                _lifetimeCoroutine = null;
            }
        }

        // ============================================
        // Private Methods
        // ============================================

        private IEnumerator AutoReturnRoutine()
        {
            float elapsed = 0f;

            while (elapsed < _lifetime)
            {
                // Check if this instance was destroyed during lifetime
                if (this == null || gameObject == null)
                    yield break;

                // Check if particle system finished early
                if (_particleSystem != null && !_particleSystem.IsAlive(true))
                    break;

                elapsed += Time.deltaTime;
                yield return null;
            }

            // Final check before returning to pool
            if (this == null || gameObject == null)
                yield break;

            // Return to pool via ServiceLocator
            if (ServiceLocator.TryGet<VFXPoolManager>(out var pool))
            {
                pool.Return(this);
            }
        }
    }
}
