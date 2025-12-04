// ============================================
// SFXTrigger.cs
// MonoBehaviour for easy sound attachment to GameObjects
// ============================================

using UnityEngine;
using UnityEngine.Events;
using HNR.Core;
using HNR.Core.Interfaces;

namespace HNR.Audio
{
    /// <summary>
    /// Trigger types for automatic SFX playback.
    /// </summary>
    public enum SFXTriggerType
    {
        /// <summary>Play when GameObject is enabled.</summary>
        OnEnable,

        /// <summary>Play when GameObject is disabled.</summary>
        OnDisable,

        /// <summary>Play when GameObject is destroyed.</summary>
        OnDestroy,

        /// <summary>Play on UI click (call OnPointerClick).</summary>
        OnClick,

        /// <summary>Play on UI hover (call OnPointerEnter).</summary>
        OnHover,

        /// <summary>Play only via script (call Play()).</summary>
        Manual
    }

    /// <summary>
    /// Component for triggering sound effects on various events.
    /// Attach to any GameObject to easily add audio feedback.
    /// </summary>
    /// <remarks>
    /// Usage:
    /// - Assign SFX ID from AudioConfigSO
    /// - Select trigger type or call Play() manually
    /// - Enable positional audio for 3D sounds
    /// - Add delay if needed
    /// </remarks>
    public class SFXTrigger : MonoBehaviour
    {
        // ============================================
        // Serialized Fields
        // ============================================

        [Header("Sound")]
        [SerializeField, Tooltip("Audio entry ID from AudioConfigSO")]
        private string _sfxId;

        [SerializeField, Tooltip("When to automatically play the sound")]
        private SFXTriggerType _triggerType = SFXTriggerType.Manual;

        [Header("Options")]
        [SerializeField, Tooltip("Play at GameObject's world position (3D spatial audio)")]
        private bool _playAtPosition;

        [SerializeField, Tooltip("Delay before playing (seconds)")]
        private float _delay;

        [SerializeField, Tooltip("Prevent multiple plays within this cooldown (seconds)")]
        private float _cooldown;

        [Header("Events")]
        [SerializeField, Tooltip("Called when sound starts playing")]
        private UnityEvent _onPlayStarted;

        // ============================================
        // Private Fields
        // ============================================

        private IAudioManager _audioManager;
        private float _lastPlayTime = -999f;

        // ============================================
        // Properties
        // ============================================

        /// <summary>Get or set the SFX ID to play.</summary>
        public string SfxId
        {
            get => _sfxId;
            set => _sfxId = value;
        }

        // ============================================
        // Unity Lifecycle
        // ============================================

        private void Awake()
        {
            ServiceLocator.TryGet(out _audioManager);
        }

        private void OnEnable()
        {
            if (_triggerType == SFXTriggerType.OnEnable)
            {
                PlayDelayed();
            }
        }

        private void OnDisable()
        {
            if (_triggerType == SFXTriggerType.OnDisable)
            {
                Play();
            }
        }

        private void OnDestroy()
        {
            if (_triggerType == SFXTriggerType.OnDestroy)
            {
                Play();
            }
        }

        // ============================================
        // Public Methods
        // ============================================

        /// <summary>
        /// Play the assigned sound effect immediately.
        /// </summary>
        public void Play()
        {
            if (string.IsNullOrEmpty(_sfxId))
            {
                Debug.LogWarning($"[SFXTrigger] No SFX ID assigned on {gameObject.name}");
                return;
            }

            // Check cooldown
            if (_cooldown > 0f && Time.time - _lastPlayTime < _cooldown)
            {
                return;
            }

            _lastPlayTime = Time.time;

            if (_playAtPosition)
            {
                _audioManager?.PlaySFXAtPosition(_sfxId, transform.position);
            }
            else
            {
                _audioManager?.PlaySFX(_sfxId);
            }

            _onPlayStarted?.Invoke();
        }

        /// <summary>
        /// Play the sound after the configured delay.
        /// </summary>
        public void PlayDelayed()
        {
            if (_delay > 0f)
            {
                Invoke(nameof(Play), _delay);
            }
            else
            {
                Play();
            }
        }

        /// <summary>
        /// Play a specific sound ID (ignores configured _sfxId).
        /// </summary>
        /// <param name="id">Audio entry ID to play</param>
        public void PlayById(string id)
        {
            if (string.IsNullOrEmpty(id)) return;

            if (_playAtPosition)
            {
                _audioManager?.PlaySFXAtPosition(id, transform.position);
            }
            else
            {
                _audioManager?.PlaySFX(id);
            }
        }

        /// <summary>
        /// Cancel any pending delayed play.
        /// </summary>
        public void CancelDelayed()
        {
            CancelInvoke(nameof(Play));
        }

        // ============================================
        // UI Event Handlers
        // ============================================

        /// <summary>
        /// Call from UI Button OnClick or EventTrigger PointerClick.
        /// </summary>
        public void OnPointerClick()
        {
            if (_triggerType == SFXTriggerType.OnClick || _triggerType == SFXTriggerType.Manual)
            {
                Play();
            }
        }

        /// <summary>
        /// Call from EventTrigger PointerEnter for hover sounds.
        /// </summary>
        public void OnPointerEnter()
        {
            if (_triggerType == SFXTriggerType.OnHover)
            {
                Play();
            }
        }

        /// <summary>
        /// Call from EventTrigger PointerExit if needed.
        /// </summary>
        public void OnPointerExit()
        {
            // Optional: could play a different sound on exit
        }
    }
}
