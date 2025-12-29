// ============================================
// AspectIconConfigSO.cs
// Configuration for Soul Aspect icon sprites
// ============================================

using UnityEngine;
using HNR.Cards;

namespace HNR.UI.Config
{
    /// <summary>
    /// ScriptableObject that maps Soul Aspects to their icon sprites.
    /// Used for displaying aspect badges in UI elements.
    /// </summary>
    [CreateAssetMenu(fileName = "AspectIconConfig", menuName = "HNR/Config/Aspect Icon Config")]
    public class AspectIconConfigSO : ScriptableObject
    {
        // ============================================
        // Serialized Fields
        // ============================================

        [Header("Aspect Icons")]
        [SerializeField, Tooltip("Icon for Flame aspect (Kira)")]
        private Sprite _flameIcon;

        [SerializeField, Tooltip("Icon for Shadow aspect (Mordren)")]
        private Sprite _shadowIcon;

        [SerializeField, Tooltip("Icon for Light aspect (Elara)")]
        private Sprite _lightIcon;

        [SerializeField, Tooltip("Icon for Nature aspect (Thornwick)")]
        private Sprite _natureIcon;

        [SerializeField, Tooltip("Icon for Arcane aspect")]
        private Sprite _arcaneIcon;

        [SerializeField, Tooltip("Icon for None/Neutral aspect")]
        private Sprite _noneIcon;

        // ============================================
        // Public API
        // ============================================

        /// <summary>
        /// Gets the icon sprite for the specified Soul Aspect.
        /// </summary>
        /// <param name="aspect">The Soul Aspect to get an icon for.</param>
        /// <returns>The corresponding sprite, or null if not assigned.</returns>
        public Sprite GetIcon(SoulAspect aspect)
        {
            return aspect switch
            {
                SoulAspect.Flame => _flameIcon,
                SoulAspect.Shadow => _shadowIcon,
                SoulAspect.Light => _lightIcon,
                SoulAspect.Nature => _natureIcon,
                SoulAspect.Arcane => _arcaneIcon,
                SoulAspect.None => _noneIcon,
                _ => _noneIcon
            };
        }

        /// <summary>
        /// Checks if all aspect icons are assigned.
        /// </summary>
        /// <returns>True if all icons are assigned.</returns>
        public bool HasAllIcons()
        {
            return _flameIcon != null &&
                   _shadowIcon != null &&
                   _lightIcon != null &&
                   _natureIcon != null &&
                   _arcaneIcon != null &&
                   _noneIcon != null;
        }

        // ============================================
        // Direct Accessors
        // ============================================

        public Sprite FlameIcon => _flameIcon;
        public Sprite ShadowIcon => _shadowIcon;
        public Sprite LightIcon => _lightIcon;
        public Sprite NatureIcon => _natureIcon;
        public Sprite ArcaneIcon => _arcaneIcon;
        public Sprite NoneIcon => _noneIcon;
    }
}
