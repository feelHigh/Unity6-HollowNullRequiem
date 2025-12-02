// ============================================
// IAudioManager.cs
// Audio playback and settings interface
// ============================================

namespace HNR.Core.Interfaces
{
    /// <summary>
    /// Audio playback and settings service.
    /// Handles music, sound effects, and volume control.
    /// </summary>
    /// <remarks>
    /// Register with ServiceLocator at startup.
    /// Implementation: AudioManager (MonoBehaviour)
    /// Full implementation in Week 9.
    /// </remarks>
    public interface IAudioManager
    {
        /// <summary>
        /// Gets or sets the music volume (0.0 - 1.0).
        /// </summary>
        float MusicVolume { get; set; }

        /// <summary>
        /// Gets or sets the sound effects volume (0.0 - 1.0).
        /// </summary>
        float SFXVolume { get; set; }

        /// <summary>
        /// Play a music track by name.
        /// Crossfades from current track if one is playing.
        /// </summary>
        /// <param name="trackName">Name of the music track asset</param>
        void PlayMusic(string trackName);

        /// <summary>
        /// Play a sound effect by name.
        /// </summary>
        /// <param name="clipName">Name of the audio clip asset</param>
        void PlaySFX(string clipName);

        /// <summary>
        /// Stop the currently playing music track.
        /// </summary>
        void StopMusic();

        /// <summary>
        /// Set music volume with clamping.
        /// </summary>
        /// <param name="volume">Volume level (0.0 - 1.0)</param>
        void SetMusicVolume(float volume);

        /// <summary>
        /// Set SFX volume with clamping.
        /// </summary>
        /// <param name="volume">Volume level (0.0 - 1.0)</param>
        void SetSFXVolume(float volume);
    }
}
