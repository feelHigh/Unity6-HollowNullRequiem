// ============================================
// CombatBackgroundController.cs
// Sets combat background based on encounter type
// ============================================

using UnityEngine;
using HNR.Core;
using HNR.Core.Events;
using HNR.Map;
using HNR.UI.Config;

namespace HNR.Combat
{
    /// <summary>
    /// Controls the combat scene background based on encounter type (normal, elite, boss).
    /// Reads from BackgroundConfigSO and sets the appropriate background sprite.
    /// </summary>
    public class CombatBackgroundController : MonoBehaviour
    {
        // ============================================
        // Serialized Fields
        // ============================================

        [Header("Background Rendering")]
        [SerializeField, Tooltip("SpriteRenderer for the background")]
        private SpriteRenderer _backgroundRenderer;

        [SerializeField, Tooltip("Background config asset")]
        private BackgroundConfigSO _backgroundConfig;

        [Header("Fallback")]
        [SerializeField, Tooltip("Fallback color if no sprite available")]
        private Color _fallbackColor = new Color(0.02f, 0.01f, 0.05f);

        // ============================================
        // Unity Lifecycle
        // ============================================

        private void Start()
        {
            SetBackgroundForCurrentEncounter();
        }

        // ============================================
        // Public Methods
        // ============================================

        /// <summary>
        /// Sets the background based on the current pending encounter type.
        /// </summary>
        public void SetBackgroundForCurrentEncounter()
        {
            if (_backgroundRenderer == null)
            {
                Debug.LogWarning("[CombatBackgroundController] No background renderer assigned");
                return;
            }

            // Determine encounter type from CombatBootstrap
            NodeType nodeType = CombatBootstrap.PendingNodeType;
            string encounterType = GetEncounterTypeString(nodeType);

            // Try to get sprite from config
            Sprite backgroundSprite = null;
            if (_backgroundConfig != null)
            {
                backgroundSprite = _backgroundConfig.GetCombatBackground(encounterType);
            }

            if (backgroundSprite != null)
            {
                _backgroundRenderer.sprite = backgroundSprite;
                _backgroundRenderer.color = Color.white;
                Debug.Log($"[CombatBackgroundController] Set {encounterType} combat background");
            }
            else
            {
                // Use fallback color
                _backgroundRenderer.sprite = null;
                _backgroundRenderer.color = _fallbackColor;
                Debug.Log($"[CombatBackgroundController] Using fallback color for {encounterType} combat (no sprite assigned)");
            }
        }

        /// <summary>
        /// Sets a specific background by encounter type.
        /// </summary>
        /// <param name="encounterType">Encounter type: "normal", "elite", or "boss"</param>
        public void SetBackground(string encounterType)
        {
            if (_backgroundRenderer == null || _backgroundConfig == null)
            {
                return;
            }

            var sprite = _backgroundConfig.GetCombatBackground(encounterType);
            if (sprite != null)
            {
                _backgroundRenderer.sprite = sprite;
                _backgroundRenderer.color = Color.white;
            }
        }

        // ============================================
        // Private Methods
        // ============================================

        private string GetEncounterTypeString(NodeType nodeType)
        {
            return nodeType switch
            {
                NodeType.Elite => "elite",
                NodeType.Boss => "boss",
                _ => "normal" // Combat, MiniBoss, etc. default to normal
            };
        }
    }
}
