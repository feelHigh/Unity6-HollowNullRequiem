// ============================================
// SpriteAtlasConfigSO.cs
// Centralized sprite atlas references for batching optimization
// ============================================

using UnityEngine;
using UnityEngine.U2D;

namespace HNR.Core
{
    /// <summary>
    /// Configuration asset for all sprite atlas references.
    /// Centralizes atlas access for draw call optimization.
    /// </summary>
    /// <remarks>
    /// Atlas configuration (TDD 10):
    /// - UI Atlas: 2048×2048, all UI sprites
    /// - Character Atlas: 1024×1024, per character
    /// - Card Atlas: 2048×2048, card frames and icons
    /// - VFX Atlas: 1024×1024, particle sprites
    /// </remarks>
    [CreateAssetMenu(fileName = "AtlasConfig", menuName = "HNR/Config/Atlas Config")]
    public class SpriteAtlasConfigSO : ScriptableObject
    {
        // ============================================
        // UI Atlases
        // ============================================

        [Header("UI Atlases")]
        [SerializeField, Tooltip("Main UI elements atlas (2048x2048)")]
        private SpriteAtlas _uiAtlas;

        [SerializeField, Tooltip("Icons and small UI elements")]
        private SpriteAtlas _iconsAtlas;

        // ============================================
        // Game Atlases
        // ============================================

        [Header("Game Atlases")]
        [SerializeField, Tooltip("Card frames and card icons (2048x2048)")]
        private SpriteAtlas _cardAtlas;

        [SerializeField, Tooltip("Requiem character sprites (1024x1024 per character)")]
        private SpriteAtlas _characterAtlas;

        [SerializeField, Tooltip("Enemy sprites")]
        private SpriteAtlas _enemyAtlas;

        // ============================================
        // Effects Atlases
        // ============================================

        [Header("Effects Atlases")]
        [SerializeField, Tooltip("VFX particle sprites (1024x1024)")]
        private SpriteAtlas _vfxAtlas;

        [SerializeField, Tooltip("Status effect icons")]
        private SpriteAtlas _statusEffectsAtlas;

        // ============================================
        // Properties
        // ============================================

        /// <summary>Main UI elements atlas.</summary>
        public SpriteAtlas UIAtlas => _uiAtlas;

        /// <summary>Icons and small UI elements atlas.</summary>
        public SpriteAtlas IconsAtlas => _iconsAtlas;

        /// <summary>Card frames and icons atlas.</summary>
        public SpriteAtlas CardAtlas => _cardAtlas;

        /// <summary>Requiem character sprites atlas.</summary>
        public SpriteAtlas CharacterAtlas => _characterAtlas;

        /// <summary>Enemy sprites atlas.</summary>
        public SpriteAtlas EnemyAtlas => _enemyAtlas;

        /// <summary>VFX particle sprites atlas.</summary>
        public SpriteAtlas VFXAtlas => _vfxAtlas;

        /// <summary>Status effect icons atlas.</summary>
        public SpriteAtlas StatusEffectsAtlas => _statusEffectsAtlas;

        // ============================================
        // Sprite Retrieval
        // ============================================

        /// <summary>
        /// Get sprite from UI atlas by name.
        /// </summary>
        /// <param name="name">Sprite name in atlas</param>
        /// <returns>Sprite if found, null otherwise</returns>
        public Sprite GetUISprite(string name)
        {
            return _uiAtlas?.GetSprite(name);
        }

        /// <summary>
        /// Get sprite from icons atlas by name.
        /// </summary>
        public Sprite GetIconSprite(string name)
        {
            return _iconsAtlas?.GetSprite(name);
        }

        /// <summary>
        /// Get sprite from card atlas by name.
        /// </summary>
        public Sprite GetCardSprite(string name)
        {
            return _cardAtlas?.GetSprite(name);
        }

        /// <summary>
        /// Get sprite from character atlas by name.
        /// </summary>
        public Sprite GetCharacterSprite(string name)
        {
            return _characterAtlas?.GetSprite(name);
        }

        /// <summary>
        /// Get sprite from enemy atlas by name.
        /// </summary>
        public Sprite GetEnemySprite(string name)
        {
            return _enemyAtlas?.GetSprite(name);
        }

        /// <summary>
        /// Get sprite from VFX atlas by name.
        /// </summary>
        public Sprite GetVFXSprite(string name)
        {
            return _vfxAtlas?.GetSprite(name);
        }

        /// <summary>
        /// Get sprite from status effects atlas by name.
        /// </summary>
        public Sprite GetStatusEffectSprite(string name)
        {
            return _statusEffectsAtlas?.GetSprite(name);
        }

        // ============================================
        // Validation
        // ============================================

        /// <summary>
        /// Validate all required atlases are assigned.
        /// </summary>
        /// <returns>True if all required atlases are assigned</returns>
        public bool ValidateAtlases()
        {
            bool valid = true;

            if (_uiAtlas == null)
            {
                Debug.LogWarning("[AtlasConfig] UI Atlas not assigned");
                valid = false;
            }

            if (_cardAtlas == null)
            {
                Debug.LogWarning("[AtlasConfig] Card Atlas not assigned");
                valid = false;
            }

            if (_characterAtlas == null)
            {
                Debug.LogWarning("[AtlasConfig] Character Atlas not assigned");
                valid = false;
            }

            if (_vfxAtlas == null)
            {
                Debug.LogWarning("[AtlasConfig] VFX Atlas not assigned");
                valid = false;
            }

            if (valid)
            {
                Debug.Log("[AtlasConfig] All required atlases validated successfully");
            }

            return valid;
        }

        /// <summary>
        /// Get debug info about atlas assignments.
        /// </summary>
        public string GetDebugInfo()
        {
            return $"[AtlasConfig] Status:\n" +
                   $"  UI Atlas: {(_uiAtlas != null ? "Assigned" : "MISSING")}\n" +
                   $"  Icons Atlas: {(_iconsAtlas != null ? "Assigned" : "MISSING")}\n" +
                   $"  Card Atlas: {(_cardAtlas != null ? "Assigned" : "MISSING")}\n" +
                   $"  Character Atlas: {(_characterAtlas != null ? "Assigned" : "MISSING")}\n" +
                   $"  Enemy Atlas: {(_enemyAtlas != null ? "Assigned" : "MISSING")}\n" +
                   $"  VFX Atlas: {(_vfxAtlas != null ? "Assigned" : "MISSING")}\n" +
                   $"  Status Effects Atlas: {(_statusEffectsAtlas != null ? "Assigned" : "MISSING")}";
        }
    }
}
