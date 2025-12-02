// ============================================
// AudioManager.cs
// Stub audio manager - full implementation Week 9
// ============================================

using UnityEngine;
using HNR.Core;
using HNR.Core.Interfaces;

namespace HNR.Audio
{
    /// <summary>
    /// Stub audio manager for service registration.
    /// Full implementation with actual audio playback in Week 9.
    /// </summary>
    /// <remarks>
    /// TODO Week 9:
    /// - Integrate Feel/MMSoundManager or custom audio system
    /// - Add AudioClip caching/pooling
    /// - Implement crossfade for music transitions
    /// - Add 3D spatial audio support
    /// - Volume persistence via SaveManager
    /// </remarks>
    public class AudioManager : MonoBehaviour, IAudioManager
    {
        // ============================================
        // Volume Properties
        // ============================================

        [Header("Default Volumes")]
        [SerializeField, Range(0f, 1f)]
        private float _defaultMusicVolume = 0.8f;

        [SerializeField, Range(0f, 1f)]
        private float _defaultSFXVolume = 1.0f;

        private float _musicVolume;
        private float _sfxVolume;

        /// <summary>Music volume (0.0 - 1.0).</summary>
        public float MusicVolume
        {
            get => _musicVolume;
            set => _musicVolume = Mathf.Clamp01(value);
        }

        /// <summary>SFX volume (0.0 - 1.0).</summary>
        public float SFXVolume
        {
            get => _sfxVolume;
            set => _sfxVolume = Mathf.Clamp01(value);
        }

        // ============================================
        // Unity Lifecycle
        // ============================================

        private void Awake()
        {
            // Persist across scene loads
            DontDestroyOnLoad(gameObject);

            // Initialize volumes
            _musicVolume = _defaultMusicVolume;
            _sfxVolume = _defaultSFXVolume;

            // Register with ServiceLocator
            if (!ServiceLocator.IsInitialized)
            {
                ServiceLocator.Initialize();
            }
            ServiceLocator.Register<IAudioManager>(this);

            Debug.Log("[AudioManager] Stub initialized.");
        }

        private void OnDestroy()
        {
            if (ServiceLocator.Has<IAudioManager>())
            {
                ServiceLocator.Unregister<IAudioManager>();
            }
        }

        // ============================================
        // Music Methods (Stub)
        // ============================================

        /// <summary>
        /// Play a music track by name.
        /// </summary>
        public void PlayMusic(string trackName)
        {
            Debug.Log($"[AudioManager] PlayMusic: {trackName} (stub)");
            // TODO Week 9: Implement actual music playback
        }

        /// <summary>
        /// Stop the currently playing music.
        /// </summary>
        public void StopMusic()
        {
            Debug.Log("[AudioManager] StopMusic (stub)");
            // TODO Week 9: Implement music stop with fade out
        }

        /// <summary>
        /// Set music volume.
        /// </summary>
        public void SetMusicVolume(float volume)
        {
            MusicVolume = volume;
            Debug.Log($"[AudioManager] Music volume set to: {MusicVolume:F2} (stub)");
            // TODO Week 9: Apply to actual audio mixer
        }

        // ============================================
        // SFX Methods (Stub)
        // ============================================

        /// <summary>
        /// Play a sound effect by name.
        /// </summary>
        public void PlaySFX(string clipName)
        {
            Debug.Log($"[AudioManager] PlaySFX: {clipName} (stub)");
            // TODO Week 9: Implement actual SFX playback
        }

        /// <summary>
        /// Set SFX volume.
        /// </summary>
        public void SetSFXVolume(float volume)
        {
            SFXVolume = volume;
            Debug.Log($"[AudioManager] SFX volume set to: {SFXVolume:F2} (stub)");
            // TODO Week 9: Apply to actual audio mixer
        }
    }
}
