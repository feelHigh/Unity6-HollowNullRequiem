// ============================================
// AudioManager.cs
// Full audio management implementation
// ============================================

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HNR.Core;
using HNR.Core.Interfaces;

namespace HNR.Audio
{
    /// <summary>
    /// Full audio manager implementation with music, SFX, and ambient sound support.
    /// Uses AudioConfigSO for clip management.
    /// </summary>
    public class AudioManager : MonoBehaviour, IAudioManager
    {
        // ============================================
        // Serialized Fields
        // ============================================

        [Header("Configuration")]
        [SerializeField, Tooltip("Audio configuration asset")]
        private AudioConfigSO _audioConfig;

        [Header("Default Volumes")]
        [SerializeField, Range(0f, 1f)]
        private float _defaultMasterVolume = 1.0f;

        [SerializeField, Range(0f, 1f)]
        private float _defaultMusicVolume = 0.8f;

        [SerializeField, Range(0f, 1f)]
        private float _defaultSFXVolume = 1.0f;

        [Header("Audio Sources")]
        [SerializeField, Tooltip("Number of pooled SFX sources")]
        private int _sfxPoolSize = 8;

        // ============================================
        // Private Fields
        // ============================================

        private AudioSource _musicSource;
        private AudioSource _musicSourceFade;
        private List<AudioSource> _sfxPool;
        private Dictionary<string, AudioSource> _ambientSources;

        private float _masterVolume;
        private float _musicVolume;
        private float _sfxVolume;

        private bool _isMasterMuted;
        private bool _isMusicMuted;
        private bool _isSFXMuted;

        private Coroutine _fadeCoroutine;

        // ============================================
        // Properties
        // ============================================

        public float MasterVolume
        {
            get => _masterVolume;
            set
            {
                _masterVolume = Mathf.Clamp01(value);
                UpdateAllVolumes();
            }
        }

        public float MusicVolume
        {
            get => _musicVolume;
            set
            {
                _musicVolume = Mathf.Clamp01(value);
                UpdateMusicVolume();
            }
        }

        public float SFXVolume
        {
            get => _sfxVolume;
            set
            {
                _sfxVolume = Mathf.Clamp01(value);
            }
        }

        public bool IsMasterMuted => _isMasterMuted;
        public bool IsMusicMuted => _isMusicMuted;
        public bool IsSFXMuted => _isSFXMuted;

        // ============================================
        // Unity Lifecycle
        // ============================================

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);

            InitializeVolumes();
            InitializeAudioSources();

            if (!ServiceLocator.IsInitialized)
            {
                ServiceLocator.Initialize();
            }
            ServiceLocator.Register<IAudioManager>(this);

            Debug.Log("[AudioManager] Initialized.");
        }

        private void OnDestroy()
        {
            if (ServiceLocator.Has<IAudioManager>())
            {
                ServiceLocator.Unregister<IAudioManager>();
            }
        }

        // ============================================
        // Initialization
        // ============================================

        private void InitializeVolumes()
        {
            _masterVolume = _defaultMasterVolume;
            _musicVolume = _defaultMusicVolume;
            _sfxVolume = _defaultSFXVolume;
        }

        private void InitializeAudioSources()
        {
            // Music sources (two for crossfade)
            _musicSource = CreateAudioSource("Music");
            _musicSourceFade = CreateAudioSource("MusicFade");

            // SFX pool
            _sfxPool = new List<AudioSource>();
            for (int i = 0; i < _sfxPoolSize; i++)
            {
                _sfxPool.Add(CreateAudioSource($"SFX_{i}"));
            }

            // Ambient sources dictionary
            _ambientSources = new Dictionary<string, AudioSource>();
        }

        private AudioSource CreateAudioSource(string name)
        {
            var go = new GameObject(name);
            go.transform.SetParent(transform);
            var source = go.AddComponent<AudioSource>();
            source.playOnAwake = false;
            return source;
        }

        // ============================================
        // Mute Controls
        // ============================================

        public void MuteMaster(bool muted)
        {
            _isMasterMuted = muted;
            UpdateAllVolumes();
            Debug.Log($"[AudioManager] Master muted: {muted}");
        }

        public void MuteMusic(bool muted)
        {
            _isMusicMuted = muted;
            UpdateMusicVolume();
            Debug.Log($"[AudioManager] Music muted: {muted}");
        }

        public void MuteSFX(bool muted)
        {
            _isSFXMuted = muted;
            Debug.Log($"[AudioManager] SFX muted: {muted}");
        }

        // ============================================
        // Music Playback
        // ============================================

        public void PlayMusic(string id, float fadeTime = 1f)
        {
            var entry = _audioConfig?.GetEntry(id);
            if (entry?.Clip == null)
            {
                Debug.LogWarning($"[AudioManager] Music not found: {id}");
                return;
            }

            if (_fadeCoroutine != null)
            {
                StopCoroutine(_fadeCoroutine);
            }

            _fadeCoroutine = StartCoroutine(CrossfadeMusic(entry, fadeTime));
        }

        public void StopMusic(float fadeTime = 1f)
        {
            if (_fadeCoroutine != null)
            {
                StopCoroutine(_fadeCoroutine);
            }

            _fadeCoroutine = StartCoroutine(FadeOutMusic(fadeTime));
        }

        public void PauseMusic()
        {
            _musicSource.Pause();
            Debug.Log("[AudioManager] Music paused");
        }

        public void ResumeMusic()
        {
            _musicSource.UnPause();
            Debug.Log("[AudioManager] Music resumed");
        }

        private IEnumerator CrossfadeMusic(AudioEntry entry, float fadeTime)
        {
            // Swap sources
            (_musicSource, _musicSourceFade) = (_musicSourceFade, _musicSource);

            // Setup new track
            _musicSource.clip = entry.Clip;
            _musicSource.loop = entry.Loop;
            _musicSource.pitch = entry.Pitch;
            _musicSource.volume = 0f;
            _musicSource.Play();

            float elapsed = 0f;
            float startVolume = _musicSourceFade.volume;
            float targetVolume = GetEffectiveMusicVolume() * entry.Volume;

            while (elapsed < fadeTime)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = elapsed / fadeTime;

                _musicSource.volume = Mathf.Lerp(0f, targetVolume, t);
                _musicSourceFade.volume = Mathf.Lerp(startVolume, 0f, t);

                yield return null;
            }

            _musicSource.volume = targetVolume;
            _musicSourceFade.Stop();
            _musicSourceFade.clip = null;

            Debug.Log($"[AudioManager] Playing music: {entry.Id}");
        }

        private IEnumerator FadeOutMusic(float fadeTime)
        {
            float elapsed = 0f;
            float startVolume = _musicSource.volume;

            while (elapsed < fadeTime)
            {
                elapsed += Time.unscaledDeltaTime;
                _musicSource.volume = Mathf.Lerp(startVolume, 0f, elapsed / fadeTime);
                yield return null;
            }

            _musicSource.Stop();
            _musicSource.clip = null;
            Debug.Log("[AudioManager] Music stopped");
        }

        // ============================================
        // SFX Playback
        // ============================================

        public void PlaySFX(string id)
        {
            if (_isMasterMuted || _isSFXMuted) return;

            var entry = _audioConfig?.GetEntry(id);
            if (entry?.Clip == null)
            {
                Debug.LogWarning($"[AudioManager] SFX not found: {id}");
                return;
            }

            var source = GetAvailableSFXSource();
            if (source == null) return;

            source.clip = entry.Clip;
            source.volume = GetEffectiveSFXVolume() * entry.Volume;
            source.pitch = entry.Pitch;
            source.spatialBlend = 0f;
            source.Play();
        }

        public void PlaySFXAtPosition(string id, Vector3 position)
        {
            if (_isMasterMuted || _isSFXMuted) return;

            var entry = _audioConfig?.GetEntry(id);
            if (entry?.Clip == null)
            {
                Debug.LogWarning($"[AudioManager] SFX not found: {id}");
                return;
            }

            var source = GetAvailableSFXSource();
            if (source == null) return;

            source.transform.position = position;
            source.clip = entry.Clip;
            source.volume = GetEffectiveSFXVolume() * entry.Volume;
            source.pitch = entry.Pitch;
            source.spatialBlend = 1f;
            source.Play();
        }

        public void StopAllSFX()
        {
            foreach (var source in _sfxPool)
            {
                source.Stop();
            }
            Debug.Log("[AudioManager] All SFX stopped");
        }

        private AudioSource GetAvailableSFXSource()
        {
            foreach (var source in _sfxPool)
            {
                if (!source.isPlaying)
                {
                    return source;
                }
            }

            // All sources busy - use oldest
            Debug.LogWarning("[AudioManager] SFX pool exhausted, reusing source");
            return _sfxPool[0];
        }

        // ============================================
        // Ambient Sounds
        // ============================================

        public void PlayAmbient(string id)
        {
            if (_ambientSources.ContainsKey(id)) return;

            var entry = _audioConfig?.GetEntry(id);
            if (entry?.Clip == null)
            {
                Debug.LogWarning($"[AudioManager] Ambient not found: {id}");
                return;
            }

            var source = CreateAudioSource($"Ambient_{id}");
            source.clip = entry.Clip;
            source.volume = GetEffectiveSFXVolume() * entry.Volume;
            source.pitch = entry.Pitch;
            source.loop = true;
            source.Play();

            _ambientSources[id] = source;
            Debug.Log($"[AudioManager] Playing ambient: {id}");
        }

        public void StopAmbient(string id)
        {
            if (!_ambientSources.TryGetValue(id, out var source)) return;

            source.Stop();
            Destroy(source.gameObject);
            _ambientSources.Remove(id);
            Debug.Log($"[AudioManager] Stopped ambient: {id}");
        }

        public void StopAllAmbient()
        {
            foreach (var kvp in _ambientSources)
            {
                kvp.Value.Stop();
                Destroy(kvp.Value.gameObject);
            }
            _ambientSources.Clear();
            Debug.Log("[AudioManager] All ambient sounds stopped");
        }

        // ============================================
        // Volume Helpers
        // ============================================

        private float GetEffectiveMusicVolume()
        {
            if (_isMasterMuted || _isMusicMuted) return 0f;
            return _masterVolume * _musicVolume;
        }

        private float GetEffectiveSFXVolume()
        {
            if (_isMasterMuted || _isSFXMuted) return 0f;
            return _masterVolume * _sfxVolume;
        }

        private void UpdateAllVolumes()
        {
            UpdateMusicVolume();
            UpdateAmbientVolumes();
        }

        private void UpdateMusicVolume()
        {
            if (_musicSource != null && _musicSource.isPlaying)
            {
                _musicSource.volume = GetEffectiveMusicVolume();
            }
        }

        private void UpdateAmbientVolumes()
        {
            float volume = GetEffectiveSFXVolume();
            foreach (var source in _ambientSources.Values)
            {
                source.volume = volume;
            }
        }

        // ============================================
        // Legacy Support
        // ============================================

        [System.Obsolete("Use MusicVolume property instead")]
        public void SetMusicVolume(float volume)
        {
            MusicVolume = volume;
        }

        [System.Obsolete("Use SFXVolume property instead")]
        public void SetSFXVolume(float volume)
        {
            SFXVolume = volume;
        }
    }
}
