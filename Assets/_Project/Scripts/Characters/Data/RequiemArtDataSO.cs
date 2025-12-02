// ============================================
// RequiemArtDataSO.cs
// ScriptableObject defining Requiem ultimate abilities
// ============================================

using System.Collections.Generic;
using UnityEngine;
using HNR.Cards;

namespace HNR.Characters
{
    /// <summary>
    /// Defines a Requiem's ultimate ability (Requiem Art).
    /// Can be used when in Null State (free) or by spending Soul Essence.
    /// </summary>
    /// <remarks>
    /// Default SE costs per character:
    /// - Kira (Inferno's Wrath): 40 SE
    /// - Mordren (Soul Harvest): 35 SE
    /// - Elara (Divine Aegis): 45 SE
    /// - Thornwick (Earthen Prison): 30 SE
    /// </remarks>
    [CreateAssetMenu(fileName = "New Requiem Art", menuName = "HNR/Requiem Art")]
    public class RequiemArtDataSO : ScriptableObject
    {
        // ============================================
        // Identity
        // ============================================

        [Header("Identity")]
        [SerializeField, Tooltip("Name of the Requiem Art")]
        private string _artName;

        [SerializeField, TextArea(2, 4), Tooltip("Effect description for UI")]
        private string _description;

        [SerializeField, TextArea(1, 2), Tooltip("Flavor text/quote")]
        private string _flavorText;

        // ============================================
        // Cost & Activation
        // ============================================

        [Header("Cost & Activation")]
        [SerializeField, Tooltip("Soul Essence cost. Kira:40, Mordren:35, Elara:45, Thornwick:30")]
        private int _seCost = 40;

        [SerializeField, Tooltip("If true, can only be used once per combat")]
        private bool _oncePerCombat = true;

        // ============================================
        // Targeting
        // ============================================

        [Header("Targeting")]
        [SerializeField, Tooltip("Target type for the art")]
        private TargetType _targetType = TargetType.AllEnemies;

        // ============================================
        // Effects
        // ============================================

        [Header("Effects")]
        [SerializeField, Tooltip("Effects applied when art is activated")]
        private List<CardEffectData> _effects = new();

        // ============================================
        // Visuals
        // ============================================

        [Header("Visuals")]
        [SerializeField, Tooltip("Icon displayed on SE bar (64x64)")]
        private Sprite _icon;

        [SerializeField, Tooltip("VFX prefab spawned on activation")]
        private GameObject _vfxPrefab;

        [SerializeField, Tooltip("Screen flash color on activation")]
        private Color _flashColor = Color.white;

        [SerializeField, Tooltip("Duration of screen effect")]
        private float _effectDuration = 1.5f;

        // ============================================
        // Audio
        // ============================================

        [Header("Audio")]
        [SerializeField, Tooltip("Sound effect on activation")]
        private AudioClip _activationSound;

        [SerializeField, Tooltip("Voice line on activation")]
        private AudioClip _voiceLine;

        // ============================================
        // Public Accessors
        // ============================================

        /// <summary>Name of the Requiem Art.</summary>
        public string ArtName => _artName;

        /// <summary>Effect description for UI.</summary>
        public string Description => _description;

        /// <summary>Flavor text/quote.</summary>
        public string FlavorText => _flavorText;

        /// <summary>Soul Essence cost to activate.</summary>
        public int SECost => _seCost;

        /// <summary>Whether this can only be used once per combat.</summary>
        public bool OncePerCombat => _oncePerCombat;

        /// <summary>Target type for the art.</summary>
        public TargetType TargetType => _targetType;

        /// <summary>Effects applied on activation.</summary>
        public IReadOnlyList<CardEffectData> Effects => _effects;

        /// <summary>Icon for SE bar display.</summary>
        public Sprite Icon => _icon;

        /// <summary>VFX prefab to spawn.</summary>
        public GameObject VFXPrefab => _vfxPrefab;

        /// <summary>Screen flash color.</summary>
        public Color FlashColor => _flashColor;

        /// <summary>Duration of visual effects.</summary>
        public float EffectDuration => _effectDuration;

        /// <summary>Activation sound effect.</summary>
        public AudioClip ActivationSound => _activationSound;

        /// <summary>Voice line on activation.</summary>
        public AudioClip VoiceLine => _voiceLine;

        // ============================================
        // Utility Methods
        // ============================================

        /// <summary>
        /// Check if the art can be activated with current SE.
        /// </summary>
        /// <param name="currentSE">Current Soul Essence</param>
        /// <param name="isInNullState">Whether owner is in Null State</param>
        /// <returns>True if art can be activated</returns>
        public bool CanActivate(int currentSE, bool isInNullState)
        {
            // Free activation in Null State
            if (isInNullState) return true;

            // Otherwise requires SE threshold
            return currentSE >= _seCost;
        }

        /// <summary>
        /// Get the actual SE cost considering Null State.
        /// </summary>
        /// <param name="isInNullState">Whether owner is in Null State</param>
        /// <returns>0 if in Null State, otherwise SECost</returns>
        public int GetEffectiveCost(bool isInNullState)
        {
            return isInNullState ? 0 : _seCost;
        }
    }
}
