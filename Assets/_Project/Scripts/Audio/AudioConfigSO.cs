// ============================================
// AudioConfigSO.cs
// ScriptableObject for centralized audio clip management
// ============================================

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace HNR.Audio
{
    /// <summary>
    /// Audio clip categories for organization and playback control.
    /// </summary>
    public enum AudioCategory
    {
        /// <summary>Background music tracks (menu_theme, combat_theme, boss_theme).</summary>
        Music,

        /// <summary>UI interaction sounds (click, hover, confirm, cancel, error).</summary>
        UI,

        /// <summary>Combat feedback sounds (card_draw, card_play, damage_hit, block, heal).</summary>
        Combat,

        /// <summary>Ambient/atmosphere sounds (corruption_pulse, null_state_trigger).</summary>
        Ambient,

        /// <summary>Character voice lines.</summary>
        Voice
    }

    /// <summary>
    /// Individual audio entry with clip reference and playback settings.
    /// </summary>
    [Serializable]
    public class AudioEntry
    {
        [Tooltip("Unique identifier for this audio clip")]
        public string Id;

        [Tooltip("The audio clip to play")]
        public AudioClip Clip;

        [Tooltip("Category for organization and volume control")]
        public AudioCategory Category;

        [Range(0f, 1f), Tooltip("Volume multiplier (0-1)")]
        public float Volume = 1f;

        [Range(0.5f, 2f), Tooltip("Pitch multiplier (0.5-2)")]
        public float Pitch = 1f;

        [Tooltip("Whether this clip should loop")]
        public bool Loop;
    }

    /// <summary>
    /// Centralized audio configuration with categorized clips and lazy-loaded lookup.
    /// </summary>
    /// <remarks>
    /// Audio categories:
    /// - Music: menu_theme, combat_theme, boss_theme
    /// - UI: click, hover, confirm, cancel, error
    /// - Combat: card_draw, card_play, damage_hit, block, heal
    /// - Ambient: corruption_pulse, null_state_trigger
    /// - Voice: Character voice lines
    /// </remarks>
    [CreateAssetMenu(fileName = "AudioConfig", menuName = "HNR/Config/Audio Config")]
    public class AudioConfigSO : ScriptableObject
    {
        // ============================================
        // Serialized Fields
        // ============================================

        [Header("Music")]
        [SerializeField, Tooltip("Background music tracks")]
        private List<AudioEntry> _musicEntries = new();

        [Header("UI Sounds")]
        [SerializeField, Tooltip("UI interaction sounds")]
        private List<AudioEntry> _uiEntries = new();

        [Header("Combat Sounds")]
        [SerializeField, Tooltip("Combat feedback sounds")]
        private List<AudioEntry> _combatEntries = new();

        [Header("Ambient Sounds")]
        [SerializeField, Tooltip("Ambient/atmosphere sounds")]
        private List<AudioEntry> _ambientEntries = new();

        [Header("Voice Lines")]
        [SerializeField, Tooltip("Character voice lines")]
        private List<AudioEntry> _voiceEntries = new();

        // ============================================
        // Private Fields
        // ============================================

        private Dictionary<string, AudioEntry> _lookup;

        // ============================================
        // Public Properties
        // ============================================

        /// <summary>All music entries.</summary>
        public IReadOnlyList<AudioEntry> MusicEntries => _musicEntries;

        /// <summary>All UI sound entries.</summary>
        public IReadOnlyList<AudioEntry> UIEntries => _uiEntries;

        /// <summary>All combat sound entries.</summary>
        public IReadOnlyList<AudioEntry> CombatEntries => _combatEntries;

        /// <summary>All ambient sound entries.</summary>
        public IReadOnlyList<AudioEntry> AmbientEntries => _ambientEntries;

        /// <summary>All voice line entries.</summary>
        public IReadOnlyList<AudioEntry> VoiceEntries => _voiceEntries;

        // ============================================
        // Lookup Methods
        // ============================================

        /// <summary>
        /// Get audio entry by ID.
        /// </summary>
        /// <param name="id">Audio entry ID</param>
        /// <returns>AudioEntry if found, null otherwise</returns>
        public AudioEntry GetEntry(string id)
        {
            BuildLookupIfNeeded();
            return _lookup.TryGetValue(id, out var entry) ? entry : null;
        }

        /// <summary>
        /// Get audio clip by ID (convenience method).
        /// </summary>
        /// <param name="id">Audio entry ID</param>
        /// <returns>AudioClip if found, null otherwise</returns>
        public AudioClip GetClip(string id)
        {
            return GetEntry(id)?.Clip;
        }

        /// <summary>
        /// Get all entries for a category.
        /// </summary>
        /// <param name="category">Audio category</param>
        /// <returns>Enumerable of AudioEntry for the category</returns>
        public IEnumerable<AudioEntry> GetByCategory(AudioCategory category)
        {
            return category switch
            {
                AudioCategory.Music => _musicEntries,
                AudioCategory.UI => _uiEntries,
                AudioCategory.Combat => _combatEntries,
                AudioCategory.Ambient => _ambientEntries,
                AudioCategory.Voice => _voiceEntries,
                _ => Enumerable.Empty<AudioEntry>()
            };
        }

        /// <summary>
        /// Check if audio ID exists.
        /// </summary>
        /// <param name="id">Audio entry ID</param>
        /// <returns>True if entry exists</returns>
        public bool HasEntry(string id)
        {
            BuildLookupIfNeeded();
            return _lookup.ContainsKey(id);
        }

        /// <summary>
        /// Get total entry count across all categories.
        /// </summary>
        public int TotalEntryCount =>
            _musicEntries.Count +
            _uiEntries.Count +
            _combatEntries.Count +
            _ambientEntries.Count +
            _voiceEntries.Count;

        // ============================================
        // Private Methods
        // ============================================

        private void BuildLookupIfNeeded()
        {
            if (_lookup != null) return;

            _lookup = new Dictionary<string, AudioEntry>();

            AddToLookup(_musicEntries);
            AddToLookup(_uiEntries);
            AddToLookup(_combatEntries);
            AddToLookup(_ambientEntries);
            AddToLookup(_voiceEntries);
        }

        private void AddToLookup(List<AudioEntry> entries)
        {
            foreach (var entry in entries)
            {
                if (string.IsNullOrEmpty(entry.Id))
                {
                    Debug.LogWarning($"[AudioConfigSO] Entry with null/empty ID found in {name}");
                    continue;
                }

                if (_lookup.ContainsKey(entry.Id))
                {
                    Debug.LogWarning($"[AudioConfigSO] Duplicate audio ID '{entry.Id}' in {name}");
                    continue;
                }

                _lookup[entry.Id] = entry;
            }
        }

        // ============================================
        // Editor Support
        // ============================================

        private void OnValidate()
        {
            // Force rebuild lookup when modified in editor
            _lookup = null;
        }

        private void OnEnable()
        {
            // Ensure lookup is rebuilt when asset is loaded
            _lookup = null;
        }
    }
}
