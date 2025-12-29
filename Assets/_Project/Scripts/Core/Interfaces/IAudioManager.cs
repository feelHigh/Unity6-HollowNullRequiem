// ============================================
// IAudioManager.cs
// Audio playback and settings interface
// ============================================

using UnityEngine;

namespace HNR.Core.Interfaces
{
    /// <summary>
    /// Audio playback and settings service.
    /// Handles music, sound effects, ambient sounds, and volume control.
    /// </summary>
    /// <remarks>
    /// Register with ServiceLocator at startup.
    /// Implementation: AudioManager (MonoBehaviour)
    /// </remarks>
    public interface IAudioManager
    {
        // ============================================
        // Volume Controls
        // ============================================

        /// <summary>Master volume multiplier (0.0 - 1.0).</summary>
        float MasterVolume { get; set; }

        /// <summary>Music volume multiplier (0.0 - 1.0).</summary>
        float MusicVolume { get; set; }

        /// <summary>Sound effects volume multiplier (0.0 - 1.0).</summary>
        float SFXVolume { get; set; }

        // ============================================
        // Mute Controls
        // ============================================

        /// <summary>Whether master audio is muted.</summary>
        bool IsMasterMuted { get; }

        /// <summary>Whether music is muted.</summary>
        bool IsMusicMuted { get; }

        /// <summary>Whether SFX is muted.</summary>
        bool IsSFXMuted { get; }

        /// <summary>
        /// Mute or unmute master audio.
        /// </summary>
        /// <param name="muted">True to mute</param>
        void MuteMaster(bool muted);

        /// <summary>
        /// Mute or unmute music.
        /// </summary>
        /// <param name="muted">True to mute</param>
        void MuteMusic(bool muted);

        /// <summary>
        /// Mute or unmute sound effects.
        /// </summary>
        /// <param name="muted">True to mute</param>
        void MuteSFX(bool muted);

        // ============================================
        // Music Playback
        // ============================================

        /// <summary>
        /// Play a music track by ID with optional crossfade.
        /// </summary>
        /// <param name="id">Audio entry ID from AudioConfigSO</param>
        /// <param name="fadeTime">Crossfade duration in seconds (default 1s)</param>
        void PlayMusic(string id, float fadeTime = 1f);

        /// <summary>
        /// Stop the currently playing music with optional fade out.
        /// </summary>
        /// <param name="fadeTime">Fade out duration in seconds (default 1s)</param>
        void StopMusic(float fadeTime = 1f);

        /// <summary>
        /// Pause the currently playing music.
        /// </summary>
        void PauseMusic();

        /// <summary>
        /// Resume paused music playback.
        /// </summary>
        void ResumeMusic();

        // ============================================
        // SFX Playback
        // ============================================

        /// <summary>
        /// Play a sound effect by ID.
        /// </summary>
        /// <param name="id">Audio entry ID from AudioConfigSO</param>
        void PlaySFX(string id);

        /// <summary>
        /// Play a sound effect at a world position (3D spatial audio).
        /// </summary>
        /// <param name="id">Audio entry ID from AudioConfigSO</param>
        /// <param name="position">World position for spatial audio</param>
        void PlaySFXAtPosition(string id, Vector3 position);

        /// <summary>
        /// Stop all currently playing sound effects.
        /// </summary>
        void StopAllSFX();

        // ============================================
        // Ambient Sounds
        // ============================================

        /// <summary>
        /// Play an ambient sound loop by ID.
        /// </summary>
        /// <param name="id">Audio entry ID from AudioConfigSO</param>
        void PlayAmbient(string id);

        /// <summary>
        /// Stop a specific ambient sound by ID.
        /// </summary>
        /// <param name="id">Audio entry ID to stop</param>
        void StopAmbient(string id);

        /// <summary>
        /// Stop all ambient sounds.
        /// </summary>
        void StopAllAmbient();

    }
}
