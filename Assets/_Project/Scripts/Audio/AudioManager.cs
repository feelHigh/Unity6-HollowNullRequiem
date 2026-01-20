// ============================================
// AudioManager.cs
// Full audio management with AudioMixer integration
// ============================================

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using HNR.Core;
using HNR.Core.Interfaces;

namespace HNR.Audio
{
    /// <summary>
    /// Full audio manager implementation with AudioMixer integration.
    /// Features music crossfade, PlayOneShot SFX, and ambient sound management.
    /// </summary>
    public class AudioManager : MonoBehaviour, IAudioManager
    {
        // ============================================
        // Serialized Fields
        // ============================================

        [Header("Configuration")]
        [SerializeField, Tooltip("Audio configuration asset")]
        private AudioConfigSO _audioConfig;

        [SerializeField, Tooltip("Master audio mixer")]
        private AudioMixer _masterMixer;

        [Header("Sources")]
        [SerializeField, Tooltip("Primary music source")]
        private AudioSource _musicSourceA;

        [SerializeField, Tooltip("Secondary music source for crossfade")]
        private AudioSource _musicSourceB;

        [Header("Default Volumes")]
        [SerializeField, Range(0f, 1f)]
        private float _defaultMasterVolume = 1f;

        [SerializeField, Range(0f, 1f)]
        private float _defaultMusicVolume = 0.8f;

        [SerializeField, Range(0f, 1f)]
        private float _defaultSFXVolume = 1f;

        // ============================================
        // Constants
        // ============================================

        private const string MASTER_VOLUME_PARAM = "MasterVolume";
        private const string MUSIC_VOLUME_PARAM = "MusicVolume";
        private const string SFX_VOLUME_PARAM = "SFXVolume";
        private const float MIN_DECIBELS = -80f;

        // ============================================
        // Private Fields
        // ============================================

        // Volume state
        private float _masterVolume;
        private float _musicVolume;
        private float _sfxVolume;

        private bool _isMasterMuted;
        private bool _isMusicMuted;
        private bool _isSFXMuted;

        // Music crossfade
        private AudioSource _currentMusicSource;
        private Coroutine _crossfadeCoroutine;

        // SFX sources (PlayOneShot approach - no pooling needed)
        private AudioSource _sfxSource2D;  // For non-positional SFX

        // Ambient tracking
        private Dictionary<string, AudioSource> _ambientSources;

        // ============================================
        // Properties
        // ============================================

        public float MasterVolume
        {
            get => _masterVolume;
            set
            {
                _masterVolume = Mathf.Clamp01(value);
                UpdateMixerVolume(MASTER_VOLUME_PARAM, _isMasterMuted ? 0f : _masterVolume);
            }
        }

        public float MusicVolume
        {
            get => _musicVolume;
            set
            {
                _musicVolume = Mathf.Clamp01(value);
                UpdateMixerVolume(MUSIC_VOLUME_PARAM, _isMusicMuted ? 0f : _musicVolume);
            }
        }

        public float SFXVolume
        {
            get => _sfxVolume;
            set
            {
                _sfxVolume = Mathf.Clamp01(value);
                UpdateMixerVolume(SFX_VOLUME_PARAM, _isSFXMuted ? 0f : _sfxVolume);
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

            if (!ServiceLocator.IsInitialized)
            {
                ServiceLocator.Initialize();
            }
            ServiceLocator.Register<IAudioManager>(this);

            Initialize();
            Debug.Log("[AudioManager] Initialized with AudioMixer integration.");
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

        private void Initialize()
        {
            // Create music sources if not assigned
            if (_musicSourceA == null)
            {
                _musicSourceA = CreateAudioSource("MusicSourceA");
                _musicSourceA.loop = true;
            }

            if (_musicSourceB == null)
            {
                _musicSourceB = CreateAudioSource("MusicSourceB");
                _musicSourceB.loop = true;
            }

            _currentMusicSource = _musicSourceA;

            // Create single 2D SFX source (uses PlayOneShot for unlimited concurrent sounds)
            _sfxSource2D = CreateAudioSource("SFXSource2D");

            // Initialize ambient tracking
            _ambientSources = new Dictionary<string, AudioSource>();

            // Apply initial volumes
            _masterVolume = _defaultMasterVolume;
            _musicVolume = _defaultMusicVolume;
            _sfxVolume = _defaultSFXVolume;

            MasterVolume = _masterVolume;
            MusicVolume = _musicVolume;
            SFXVolume = _sfxVolume;
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
            UpdateMixerVolume(MASTER_VOLUME_PARAM, muted ? 0f : _masterVolume);
            Debug.Log($"[AudioManager] Master muted: {muted}");
        }

        public void MuteMusic(bool muted)
        {
            _isMusicMuted = muted;
            UpdateMixerVolume(MUSIC_VOLUME_PARAM, muted ? 0f : _musicVolume);
            Debug.Log($"[AudioManager] Music muted: {muted}");
        }

        public void MuteSFX(bool muted)
        {
            _isSFXMuted = muted;
            UpdateMixerVolume(SFX_VOLUME_PARAM, muted ? 0f : _sfxVolume);
            Debug.Log($"[AudioManager] SFX muted: {muted}");
        }

        // ============================================
        // Music Playback
        // ============================================

        public void PlayMusic(string id, float fadeTime = 1f)
        {
            var entry = _audioConfig?.GetEntry(id);
            if (entry == null || entry.Clip == null)
            {
                Debug.LogWarning($"[AudioManager] Music not found: {id}");
                return;
            }

            if (_crossfadeCoroutine != null)
            {
                StopCoroutine(_crossfadeCoroutine);
            }

            _crossfadeCoroutine = StartCoroutine(CrossfadeMusic(entry, fadeTime));
        }

        public void StopMusic(float fadeTime = 1f)
        {
            if (_crossfadeCoroutine != null)
            {
                StopCoroutine(_crossfadeCoroutine);
            }

            _crossfadeCoroutine = StartCoroutine(FadeOutMusic(fadeTime));
        }

        public void PauseMusic()
        {
            _currentMusicSource?.Pause();
            Debug.Log("[AudioManager] Music paused");
        }

        public void ResumeMusic()
        {
            _currentMusicSource?.UnPause();
            Debug.Log("[AudioManager] Music resumed");
        }

        private IEnumerator CrossfadeMusic(AudioEntry entry, float fadeTime)
        {
            // Swap to alternate source
            var newSource = _currentMusicSource == _musicSourceA ? _musicSourceB : _musicSourceA;
            var oldSource = _currentMusicSource;

            // Setup new track
            newSource.clip = entry.Clip;
            newSource.volume = 0f;
            newSource.pitch = entry.Pitch;
            newSource.loop = entry.Loop;
            newSource.Play();

            // Crossfade
            float elapsed = 0f;
            float oldStartVolume = oldSource.volume;

            while (elapsed < fadeTime)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = elapsed / fadeTime;

                newSource.volume = Mathf.Lerp(0f, entry.Volume, t);
                oldSource.volume = Mathf.Lerp(oldStartVolume, 0f, t);

                yield return null;
            }

            // Finalize
            oldSource.Stop();
            oldSource.clip = null;
            newSource.volume = entry.Volume;

            _currentMusicSource = newSource;
            Debug.Log($"[AudioManager] Now playing music: {entry.Id}");
        }

        private IEnumerator FadeOutMusic(float fadeTime)
        {
            float startVolume = _currentMusicSource.volume;
            float elapsed = 0f;

            while (elapsed < fadeTime)
            {
                elapsed += Time.unscaledDeltaTime;
                _currentMusicSource.volume = Mathf.Lerp(startVolume, 0f, elapsed / fadeTime);
                yield return null;
            }

            _currentMusicSource.Stop();
            _currentMusicSource.clip = null;
            Debug.Log("[AudioManager] Music stopped");
        }

        // ============================================
        // SFX Playback
        // ============================================

        public void PlaySFX(string id)
        {
            if (_isMasterMuted || _isSFXMuted) return;

            var entry = _audioConfig?.GetEntry(id);
            if (entry == null || entry.Clip == null)
            {
                Debug.LogWarning($"[AudioManager] SFX not found: {id}");
                return;
            }

            // PlayOneShot allows unlimited concurrent sounds on a single source
            _sfxSource2D.pitch = entry.Pitch;
            _sfxSource2D.PlayOneShot(entry.Clip, entry.Volume);
        }

        public void PlaySFXAtPosition(string id, Vector3 position)
        {
            if (_isMasterMuted || _isSFXMuted) return;

            var entry = _audioConfig?.GetEntry(id);
            if (entry == null || entry.Clip == null)
            {
                Debug.LogWarning($"[AudioManager] SFX not found: {id}");
                return;
            }

            // PlayClipAtPoint creates a temporary GameObject that auto-destroys when done
            // Volume is passed directly; pitch requires a custom approach if needed
            AudioSource.PlayClipAtPoint(entry.Clip, position, entry.Volume);
        }

        public void StopAllSFX()
        {
            // Stop any sounds currently playing on the 2D source
            _sfxSource2D?.Stop();
            Debug.Log("[AudioManager] All SFX stopped");
        }

        // ============================================
        // Ambient Sounds
        // ============================================

        public void PlayAmbient(string id)
        {
            if (_ambientSources.ContainsKey(id)) return;

            var entry = _audioConfig?.GetEntry(id);
            if (entry == null || entry.Clip == null)
            {
                Debug.LogWarning($"[AudioManager] Ambient not found: {id}");
                return;
            }

            var source = CreateAudioSource($"Ambient_{id}");
            source.clip = entry.Clip;
            source.volume = entry.Volume;
            source.pitch = entry.Pitch;
            source.loop = true;
            source.Play();

            _ambientSources[id] = source;
            Debug.Log($"[AudioManager] Playing ambient: {id}");
        }

        public void StopAmbient(string id)
        {
            if (_ambientSources.TryGetValue(id, out var source))
            {
                source.Stop();
                Destroy(source.gameObject);
                _ambientSources.Remove(id);
                Debug.Log($"[AudioManager] Stopped ambient: {id}");
            }
        }

        public void StopAllAmbient()
        {
            foreach (var source in _ambientSources.Values)
            {
                if (source != null)
                {
                    source.Stop();
                    Destroy(source.gameObject);
                }
            }
            _ambientSources.Clear();
            Debug.Log("[AudioManager] All ambient sounds stopped");
        }

        // ============================================
        // AudioMixer Integration
        // ============================================

        private void UpdateMixerVolume(string parameter, float linearVolume)
        {
            if (_masterMixer == null) return;

            // Convert linear (0-1) to decibels (-80 to 0)
            float db = linearVolume > 0.0001f
                ? 20f * Mathf.Log10(linearVolume)
                : MIN_DECIBELS;

            _masterMixer.SetFloat(parameter, db);
        }

        /// <summary>
        /// Get current mixer volume for a parameter.
        /// </summary>
        public float GetMixerVolume(string parameter)
        {
            if (_masterMixer == null) return 1f;

            if (_masterMixer.GetFloat(parameter, out float db))
            {
                // Convert decibels back to linear
                return Mathf.Pow(10f, db / 20f);
            }
            return 1f;
        }

    }
}
