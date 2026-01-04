// ============================================
// VFXConfigSO.cs
// ScriptableObject for centralized VFX prefab management
// ============================================

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace HNR.VFX
{
    /// <summary>
    /// VFX categories for organization and pool management.
    /// </summary>
    public enum VFXCategory
    {
        /// <summary>Hit effects by Soul Aspect (hit_flame, hit_shadow, hit_nature, hit_arcane, hit_light).</summary>
        Hit,

        /// <summary>Combat action effects (vfx_slash, vfx_shield, vfx_heal).</summary>
        Combat,

        /// <summary>Status effect visuals (vfx_buff, vfx_debuff, vfx_corruption).</summary>
        Status,

        /// <summary>Special/rare effects (vfx_null_burst, vfx_requiem_art).</summary>
        Special,

        /// <summary>Card-related effects (vfx_card_draw, vfx_card_play).</summary>
        Card,

        /// <summary>UI feedback effects.</summary>
        UI
    }

    /// <summary>
    /// Individual VFX entry with prefab reference and pool settings.
    /// </summary>
    [Serializable]
    public class VFXEntry
    {
        [Tooltip("Unique identifier for this effect (e.g., hit_flame, vfx_heal)")]
        public string EffectId;

        [Tooltip("Prefab to instantiate for this effect")]
        public GameObject Prefab;

        [Tooltip("Category for organization")]
        public VFXCategory Category;

        [Tooltip("Number of instances to pre-warm on initialization")]
        [Range(0, 20)]
        public int PreWarmCount = 3;

        [Tooltip("Maximum simultaneous active instances")]
        [Range(1, 50)]
        public int MaxActive = 10;

        [Tooltip("Default color tint for this effect")]
        public Color DefaultColor = Color.white;

        [Tooltip("Default scale multiplier")]
        [Range(0.1f, 5f)]
        public float DefaultScale = 1f;
    }

    /// <summary>
    /// Centralized VFX configuration with categorized prefabs and lazy-loaded lookup.
    /// Used by VFXPoolManager for pool initialization and effect spawning.
    /// </summary>
    /// <remarks>
    /// VFX categories:
    /// - Hit: Aspect-based damage effects (hit_flame, hit_shadow, etc.)
    /// - Combat: Attack and defense visuals (vfx_slash, vfx_shield, vfx_heal)
    /// - Status: Buff/debuff/corruption effects (vfx_buff, vfx_debuff, vfx_corruption)
    /// - Special: Rare/powerful effects (vfx_null_burst, vfx_requiem_art)
    /// - Card: Card manipulation effects (vfx_card_draw, vfx_card_play)
    /// - UI: UI feedback effects
    ///
    /// Pool sizing per TDD 10:
    /// - hit_*: 5 pre-warm, 10 max
    /// - vfx_slash: 3 pre-warm, 5 max
    /// - vfx_shield/heal: 2 pre-warm, 3 max
    /// - vfx_corruption: 3 pre-warm, 5 max
    /// - vfx_null_burst: 1 pre-warm, 2 max
    /// </remarks>
    [CreateAssetMenu(fileName = "VFXConfig", menuName = "HNR/Config/VFX Config")]
    public class VFXConfigSO : ScriptableObject
    {
        // ============================================
        // Serialized Fields
        // ============================================

        [Header("Hit Effects")]
        [SerializeField, Tooltip("Aspect-based hit effects (hit_flame, hit_shadow, etc.)")]
        private List<VFXEntry> _hitEntries = new();

        [Header("Combat Effects")]
        [SerializeField, Tooltip("Combat action effects (vfx_slash, vfx_shield, vfx_heal)")]
        private List<VFXEntry> _combatEntries = new();

        [Header("Status Effects")]
        [SerializeField, Tooltip("Status effect visuals (vfx_buff, vfx_debuff, vfx_corruption)")]
        private List<VFXEntry> _statusEntries = new();

        [Header("Special Effects")]
        [SerializeField, Tooltip("Special/rare effects (vfx_null_burst, vfx_requiem_art)")]
        private List<VFXEntry> _specialEntries = new();

        [Header("Card Effects")]
        [SerializeField, Tooltip("Card-related effects (vfx_card_draw, vfx_card_play)")]
        private List<VFXEntry> _cardEntries = new();

        [Header("UI Effects")]
        [SerializeField, Tooltip("UI feedback effects")]
        private List<VFXEntry> _uiEntries = new();

        // ============================================
        // Private Fields
        // ============================================

        private Dictionary<string, VFXEntry> _lookup;

        // ============================================
        // Public Properties
        // ============================================

        /// <summary>All hit effect entries.</summary>
        public IReadOnlyList<VFXEntry> HitEntries => _hitEntries;

        /// <summary>All combat effect entries.</summary>
        public IReadOnlyList<VFXEntry> CombatEntries => _combatEntries;

        /// <summary>All status effect entries.</summary>
        public IReadOnlyList<VFXEntry> StatusEntries => _statusEntries;

        /// <summary>All special effect entries.</summary>
        public IReadOnlyList<VFXEntry> SpecialEntries => _specialEntries;

        /// <summary>All card effect entries.</summary>
        public IReadOnlyList<VFXEntry> CardEntries => _cardEntries;

        /// <summary>All UI effect entries.</summary>
        public IReadOnlyList<VFXEntry> UIEntries => _uiEntries;

        /// <summary>All entries across all categories.</summary>
        public IEnumerable<VFXEntry> AllEntries
        {
            get
            {
                foreach (var e in _hitEntries) yield return e;
                foreach (var e in _combatEntries) yield return e;
                foreach (var e in _statusEntries) yield return e;
                foreach (var e in _specialEntries) yield return e;
                foreach (var e in _cardEntries) yield return e;
                foreach (var e in _uiEntries) yield return e;
            }
        }

        /// <summary>Get total entry count across all categories.</summary>
        public int TotalEntryCount =>
            _hitEntries.Count +
            _combatEntries.Count +
            _statusEntries.Count +
            _specialEntries.Count +
            _cardEntries.Count +
            _uiEntries.Count;

        // ============================================
        // Lookup Methods
        // ============================================

        /// <summary>
        /// Get VFX entry by effect ID.
        /// </summary>
        /// <param name="effectId">Effect ID (e.g., "hit_flame", "vfx_heal")</param>
        /// <returns>VFXEntry if found, null otherwise</returns>
        public VFXEntry GetEntry(string effectId)
        {
            BuildLookupIfNeeded();
            return _lookup.TryGetValue(effectId, out var entry) ? entry : null;
        }

        /// <summary>
        /// Try to get VFX entry by effect ID.
        /// </summary>
        /// <param name="effectId">Effect ID</param>
        /// <param name="entry">Output entry if found</param>
        /// <returns>True if entry was found</returns>
        public bool TryGetEntry(string effectId, out VFXEntry entry)
        {
            BuildLookupIfNeeded();
            return _lookup.TryGetValue(effectId, out entry);
        }

        /// <summary>
        /// Get prefab by effect ID (convenience method).
        /// </summary>
        /// <param name="effectId">Effect ID</param>
        /// <returns>GameObject prefab if found, null otherwise</returns>
        public GameObject GetPrefab(string effectId)
        {
            return GetEntry(effectId)?.Prefab;
        }

        /// <summary>
        /// Get all entries for a category.
        /// </summary>
        /// <param name="category">VFX category</param>
        /// <returns>Enumerable of VFXEntry for the category</returns>
        public IEnumerable<VFXEntry> GetByCategory(VFXCategory category)
        {
            return category switch
            {
                VFXCategory.Hit => _hitEntries,
                VFXCategory.Combat => _combatEntries,
                VFXCategory.Status => _statusEntries,
                VFXCategory.Special => _specialEntries,
                VFXCategory.Card => _cardEntries,
                VFXCategory.UI => _uiEntries,
                _ => Enumerable.Empty<VFXEntry>()
            };
        }

        /// <summary>
        /// Check if effect ID exists.
        /// </summary>
        /// <param name="effectId">Effect ID</param>
        /// <returns>True if entry exists</returns>
        public bool HasEntry(string effectId)
        {
            BuildLookupIfNeeded();
            return _lookup.ContainsKey(effectId);
        }

        /// <summary>
        /// Get all effect IDs.
        /// </summary>
        /// <returns>Enumerable of all effect IDs</returns>
        public IEnumerable<string> GetAllEffectIds()
        {
            BuildLookupIfNeeded();
            return _lookup.Keys;
        }

        // ============================================
        // Pool Configuration Export
        // ============================================

        /// <summary>
        /// Convert all entries to VFXPoolConfig list for VFXPoolManager.
        /// </summary>
        /// <returns>List of VFXPoolConfig for pool initialization</returns>
        public List<VFXPoolConfig> ToPoolConfigs()
        {
            var configs = new List<VFXPoolConfig>();

            foreach (var entry in AllEntries)
            {
                if (string.IsNullOrEmpty(entry.EffectId) || entry.Prefab == null)
                    continue;

                configs.Add(new VFXPoolConfig
                {
                    EffectId = entry.EffectId,
                    Prefab = entry.Prefab,
                    PreWarmCount = entry.PreWarmCount,
                    MaxActive = entry.MaxActive
                });
            }

            return configs;
        }

        // ============================================
        // Private Methods
        // ============================================

        private void BuildLookupIfNeeded()
        {
            if (_lookup != null) return;

            _lookup = new Dictionary<string, VFXEntry>();

            AddToLookup(_hitEntries);
            AddToLookup(_combatEntries);
            AddToLookup(_statusEntries);
            AddToLookup(_specialEntries);
            AddToLookup(_cardEntries);
            AddToLookup(_uiEntries);
        }

        private void AddToLookup(List<VFXEntry> entries)
        {
            foreach (var entry in entries)
            {
                if (string.IsNullOrEmpty(entry.EffectId))
                {
                    Debug.LogWarning($"[VFXConfigSO] Entry with null/empty EffectId found in {name}");
                    continue;
                }

                if (_lookup.ContainsKey(entry.EffectId))
                {
                    Debug.LogWarning($"[VFXConfigSO] Duplicate effect ID '{entry.EffectId}' in {name}");
                    continue;
                }

                _lookup[entry.EffectId] = entry;
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

        // ============================================
        // Validation
        // ============================================

        /// <summary>
        /// Validate all entries and return list of issues.
        /// </summary>
        /// <returns>List of validation error messages</returns>
        public List<string> Validate()
        {
            var issues = new List<string>();
            var seenIds = new HashSet<string>();

            foreach (var entry in AllEntries)
            {
                if (string.IsNullOrEmpty(entry.EffectId))
                {
                    issues.Add("Entry has empty EffectId");
                    continue;
                }

                if (seenIds.Contains(entry.EffectId))
                {
                    issues.Add($"Duplicate EffectId: {entry.EffectId}");
                }
                else
                {
                    seenIds.Add(entry.EffectId);
                }

                if (entry.Prefab == null)
                {
                    issues.Add($"Entry '{entry.EffectId}' has no prefab assigned");
                }

                if (entry.PreWarmCount > entry.MaxActive)
                {
                    issues.Add($"Entry '{entry.EffectId}' has PreWarmCount ({entry.PreWarmCount}) > MaxActive ({entry.MaxActive})");
                }
            }

            return issues;
        }
    }
}
