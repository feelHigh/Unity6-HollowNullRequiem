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
        /// <summary>Background music tracks (menu, bastion, combat, boss, zones).</summary>
        Music,

        /// <summary>Combat feedback sounds (damage_hit, critical_hit, heal, enemy_attack, enemy_defeated).</summary>
        Combat,

        /// <summary>Status effect sounds (burn, poison, weakness, vulnerability, strength, regen, tick).</summary>
        Status,

        /// <summary>Requiem Art sounds (art_activate, art_kira, art_mordren, art_elara, art_thornwick).</summary>
        Arts,

        /// <summary>Element/Aspect attack sounds (flame, shadow, light, nature).</summary>
        Elements,

        /// <summary>Shop and reward sounds (purchase, relic_acquire, card_acquire, void_shards_gain).</summary>
        Shop,

        /// <summary>Stingers and alerts (victory, defeat, low_health).</summary>
        Stingers
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

        /// <summary>
        /// Create an AudioEntry with default settings.
        /// </summary>
        public AudioEntry() { }

        /// <summary>
        /// Create an AudioEntry with specified values.
        /// </summary>
        public AudioEntry(string id, AudioClip clip, AudioCategory category, float volume = 1f, bool loop = false)
        {
            Id = id;
            Clip = clip;
            Category = category;
            Volume = volume;
            Pitch = 1f;
            Loop = loop;
        }
    }

    /// <summary>
    /// Centralized audio configuration with categorized clips and lazy-loaded lookup.
    /// </summary>
    /// <remarks>
    /// Audio categories:
    /// - Music: menu, bastion, combat, boss, zones, shop, sanctuary
    /// - Combat: damage_hit, critical_hit, heal, enemy_attack, enemy_defeated
    /// - Status: burn, poison, weakness, vulnerability, strength, regen, tick
    /// - Arts: art_activate, art_kira, art_mordren, art_elara, art_thornwick
    /// - Elements: flame_attack, shadow_attack, light_attack, nature_attack
    /// - Shop: purchase, relic_acquire, card_acquire, void_shards_gain
    /// - Stingers: victory, defeat, low_health
    /// </remarks>
    [CreateAssetMenu(fileName = "AudioConfig", menuName = "HNR/Config/Audio Config")]
    public class AudioConfigSO : ScriptableObject
    {
        // ============================================
        // Serialized Fields
        // ============================================

        [Header("Music (9 tracks)")]
        [SerializeField, Tooltip("Background music tracks")]
        private List<AudioEntry> _musicEntries = new();

        [Header("Combat SFX (5 sounds)")]
        [SerializeField, Tooltip("Combat feedback sounds")]
        private List<AudioEntry> _combatEntries = new();

        [Header("Status Effect SFX (7 sounds)")]
        [SerializeField, Tooltip("Status effect application sounds")]
        private List<AudioEntry> _statusEntries = new();

        [Header("Requiem Arts SFX (5 sounds)")]
        [SerializeField, Tooltip("Ultimate ability sounds")]
        private List<AudioEntry> _artsEntries = new();

        [Header("Element Attack SFX (4 sounds)")]
        [SerializeField, Tooltip("Elemental aspect attack sounds")]
        private List<AudioEntry> _elementsEntries = new();

        [Header("Shop & Reward SFX (4 sounds)")]
        [SerializeField, Tooltip("Shop and reward sounds")]
        private List<AudioEntry> _shopEntries = new();

        [Header("Stingers & Alerts (3 sounds)")]
        [SerializeField, Tooltip("Victory, defeat, and alert stingers")]
        private List<AudioEntry> _stingersEntries = new();

        // ============================================
        // Private Fields
        // ============================================

        private Dictionary<string, AudioEntry> _lookup;

        // ============================================
        // Public Properties
        // ============================================

        /// <summary>All music entries.</summary>
        public IReadOnlyList<AudioEntry> MusicEntries => _musicEntries;

        /// <summary>All combat sound entries.</summary>
        public IReadOnlyList<AudioEntry> CombatEntries => _combatEntries;

        /// <summary>All status effect sound entries.</summary>
        public IReadOnlyList<AudioEntry> StatusEntries => _statusEntries;

        /// <summary>All Requiem Art sound entries.</summary>
        public IReadOnlyList<AudioEntry> ArtsEntries => _artsEntries;

        /// <summary>All element attack sound entries.</summary>
        public IReadOnlyList<AudioEntry> ElementsEntries => _elementsEntries;

        /// <summary>All shop and reward sound entries.</summary>
        public IReadOnlyList<AudioEntry> ShopEntries => _shopEntries;

        /// <summary>All stinger and alert sound entries.</summary>
        public IReadOnlyList<AudioEntry> StingersEntries => _stingersEntries;

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
                AudioCategory.Combat => _combatEntries,
                AudioCategory.Status => _statusEntries,
                AudioCategory.Arts => _artsEntries,
                AudioCategory.Elements => _elementsEntries,
                AudioCategory.Shop => _shopEntries,
                AudioCategory.Stingers => _stingersEntries,
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
            _combatEntries.Count +
            _statusEntries.Count +
            _artsEntries.Count +
            _elementsEntries.Count +
            _shopEntries.Count +
            _stingersEntries.Count;

        // ============================================
        // Editor Support Methods
        // ============================================

#if UNITY_EDITOR
        /// <summary>
        /// Clear all entries (editor only).
        /// </summary>
        public void ClearAllEntries()
        {
            _musicEntries.Clear();
            _combatEntries.Clear();
            _statusEntries.Clear();
            _artsEntries.Clear();
            _elementsEntries.Clear();
            _shopEntries.Clear();
            _stingersEntries.Clear();
            _lookup = null;
        }

        /// <summary>
        /// Add an entry to the appropriate category list (editor only).
        /// </summary>
        public void AddEntry(AudioEntry entry)
        {
            var list = GetEditableList(entry.Category);
            list?.Add(entry);
            _lookup = null;
        }

        /// <summary>
        /// Get the editable list for a category (editor only).
        /// </summary>
        public List<AudioEntry> GetEditableList(AudioCategory category)
        {
            return category switch
            {
                AudioCategory.Music => _musicEntries,
                AudioCategory.Combat => _combatEntries,
                AudioCategory.Status => _statusEntries,
                AudioCategory.Arts => _artsEntries,
                AudioCategory.Elements => _elementsEntries,
                AudioCategory.Shop => _shopEntries,
                AudioCategory.Stingers => _stingersEntries,
                _ => null
            };
        }
#endif

        // ============================================
        // Private Methods
        // ============================================

        private void BuildLookupIfNeeded()
        {
            if (_lookup != null) return;

            _lookup = new Dictionary<string, AudioEntry>();

            AddToLookup(_musicEntries);
            AddToLookup(_combatEntries);
            AddToLookup(_statusEntries);
            AddToLookup(_artsEntries);
            AddToLookup(_elementsEntries);
            AddToLookup(_shopEntries);
            AddToLookup(_stingersEntries);
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
        // Unity Callbacks
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
