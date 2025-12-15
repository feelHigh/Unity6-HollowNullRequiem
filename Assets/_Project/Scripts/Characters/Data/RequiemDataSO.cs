// ============================================
// RequiemDataSO.cs
// ScriptableObject defining Requiem character data
// ============================================

using System.Collections.Generic;
using UnityEngine;
using HNR.Cards;
using HNR.Characters.Visuals;

namespace HNR.Characters
{
    /// <summary>
    /// Defines a playable Requiem character's static data.
    /// Create one asset per character (Kira, Mordren, Elara, Thornwick).
    /// </summary>
    [CreateAssetMenu(fileName = "New Requiem", menuName = "HNR/Requiem Data")]
    public class RequiemDataSO : ScriptableObject
    {
        // ============================================
        // Identity
        // ============================================

        [Header("Identity")]
        [SerializeField, Tooltip("Unique identifier for save/load")]
        private string _requiemId;

        [SerializeField, Tooltip("Character name")]
        private string _requiemName;

        [SerializeField, Tooltip("Title/epithet shown under name")]
        private string _title;

        [SerializeField, TextArea(3, 6), Tooltip("Character backstory for lore screen")]
        private string _backstory;

        // ============================================
        // Classification
        // ============================================

        [Header("Classification")]
        [SerializeField, Tooltip("Combat role")]
        private RequiemClass _class;

        [SerializeField, Tooltip("Elemental affinity")]
        private SoulAspect _soulAspect;

        // ============================================
        // Base Stats
        // ============================================

        [Header("Base Stats")]
        [SerializeField, Tooltip("Base HP contribution to team. Kira:70, Mordren:60, Elara:80, Thornwick:100")]
        private int _baseHP = 70;

        [SerializeField, Tooltip("Base ATK modifier. Kira:12, Mordren:8, Elara:6, Thornwick:8")]
        private int _baseATK = 8;

        [SerializeField, Tooltip("Base DEF modifier. Kira:4, Mordren:6, Elara:8, Thornwick:10")]
        private int _baseDEF = 6;

        [SerializeField, Range(0.5f, 3f), Tooltip("Soul Essence gain rate multiplier. Kira:1.5x, Mordren:2.0x, Elara:1.0x, Thornwick:1.0x")]
        private float _seRate = 1f;

        // ============================================
        // Cards
        // ============================================

        [Header("Cards")]
        [SerializeField, Tooltip("10 cards added to shared deck at run start")]
        private List<CardDataSO> _startingCards = new();

        [SerializeField, Tooltip("Cards that can be unlocked/found during runs")]
        private List<CardDataSO> _unlockableCards = new();

        // ============================================
        // Requiem Art
        // ============================================

        [Header("Requiem Art")]
        [SerializeField, Tooltip("Ultimate ability data (requires Null State or SE threshold)")]
        private RequiemArtDataSO _requiemArt;

        // ============================================
        // Null State
        // ============================================

        [Header("Null State")]
        [SerializeField, Tooltip("Special effect when entering Null State (100 Corruption)")]
        private NullStateEffect _nullStateEffect;

        [SerializeField, TextArea(2, 4), Tooltip("Description of Null State effect for UI")]
        private string _nullStateDescription;

        // ============================================
        // Visuals
        // ============================================

        [Header("Visuals")]
        [SerializeField, Tooltip("Portrait for selection and combat UI (256x256)")]
        private Sprite _portrait;

        [SerializeField, Tooltip("Full body sprite for character screens (512x1024)")]
        private Sprite _fullBodySprite;

        [SerializeField, Tooltip("Animator controller for combat animations")]
        private RuntimeAnimatorController _animator;

        [SerializeField, Tooltip("Color associated with Soul Aspect for UI theming")]
        private Color _aspectColor = Color.white;

        [Header("Character Visual")]
        [SerializeField, Tooltip("HeroEditor visual prefab for animated combat display")]
        private GameObject _visualPrefab;

        [SerializeField, Tooltip("Preferred attack animation type")]
        private AttackType _preferredAttackType = AttackType.Slash;

        // ============================================
        // Public Accessors
        // ============================================

        /// <summary>Unique identifier for save/load.</summary>
        public string RequiemId => _requiemId;

        /// <summary>Character display name.</summary>
        public string RequiemName => _requiemName;

        /// <summary>Title/epithet (e.g., "Ember Blade").</summary>
        public string Title => _title;

        /// <summary>Character backstory for lore screen.</summary>
        public string Backstory => _backstory;

        /// <summary>Combat role (Striker, Controller, Support, Tank).</summary>
        public RequiemClass Class => _class;

        /// <summary>Elemental affinity.</summary>
        public SoulAspect SoulAspect => _soulAspect;

        /// <summary>Base HP contribution to team total.</summary>
        public int BaseHP => _baseHP;

        /// <summary>Base ATK modifier for damage calculations.</summary>
        public int BaseATK => _baseATK;

        /// <summary>Base DEF modifier for block calculations.</summary>
        public int BaseDEF => _baseDEF;

        /// <summary>Soul Essence gain rate multiplier.</summary>
        public float SERate => _seRate;

        /// <summary>Cards added to shared deck at run start.</summary>
        public IReadOnlyList<CardDataSO> StartingCards => _startingCards;

        /// <summary>Cards available to unlock during runs.</summary>
        public IReadOnlyList<CardDataSO> UnlockableCards => _unlockableCards;

        /// <summary>Ultimate ability data.</summary>
        public RequiemArtDataSO RequiemArt => _requiemArt;

        /// <summary>Effect triggered at 100 Corruption.</summary>
        public NullStateEffect NullStateEffect => _nullStateEffect;

        /// <summary>UI description of Null State effect.</summary>
        public string NullStateDescription => _nullStateDescription;

        /// <summary>Portrait sprite (256x256).</summary>
        public Sprite Portrait => _portrait;

        /// <summary>Full body sprite (512x1024).</summary>
        public Sprite FullBodySprite => _fullBodySprite;

        /// <summary>Combat animation controller.</summary>
        public RuntimeAnimatorController Animator => _animator;

        /// <summary>Soul Aspect theme color.</summary>
        public Color AspectColor => _aspectColor;

        /// <summary>HeroEditor visual prefab for animated combat display.</summary>
        public GameObject VisualPrefab => _visualPrefab;

        /// <summary>Preferred attack animation type.</summary>
        public AttackType PreferredAttackType => _preferredAttackType;

        // ============================================
        // Utility Methods
        // ============================================

        /// <summary>
        /// Get full display name with title.
        /// </summary>
        /// <returns>Format: "Name, Title"</returns>
        public string GetFullName()
        {
            return string.IsNullOrEmpty(_title) ? _requiemName : $"{_requiemName}, {_title}";
        }

        /// <summary>
        /// Get starting card count.
        /// </summary>
        public int StartingCardCount => _startingCards?.Count ?? 0;

        // ============================================
        // Editor Helpers
        // ============================================

#if UNITY_EDITOR
        private void OnValidate()
        {
            // Auto-generate ID from asset name if empty
            if (string.IsNullOrEmpty(_requiemId))
            {
                _requiemId = name.ToLower().Replace(" ", "_");
            }
        }
#endif
    }
}
