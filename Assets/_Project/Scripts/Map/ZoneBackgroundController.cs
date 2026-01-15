// ============================================
// ZoneBackgroundController.cs
// Updates zone background based on current zone
// ============================================

using UnityEngine;
using UnityEngine.UI;
using HNR.Core;
using HNR.Core.Interfaces;
using HNR.UI.Config;

namespace HNR.Map
{
    /// <summary>
    /// Controls the NullRift scene background based on current zone.
    /// Reads from BackgroundConfigSO and sets the appropriate zone sprite.
    /// Updates automatically when the scene loads or when explicitly refreshed.
    /// </summary>
    public class ZoneBackgroundController : MonoBehaviour
    {
        // ============================================
        // Serialized Fields
        // ============================================

        [Header("Background Rendering")]
        [SerializeField, Tooltip("UI Image for the zone background")]
        private Image _backgroundImage;

        [SerializeField, Tooltip("Background config asset")]
        private BackgroundConfigSO _backgroundConfig;

        [Header("Fallback")]
        [SerializeField, Tooltip("Fallback color if no sprite available")]
        private Color _fallbackColor = new Color(0.03f, 0.01f, 0.08f);

        // ============================================
        // Private Fields
        // ============================================

        private int _lastSetZone = -1;

        // ============================================
        // Unity Lifecycle
        // ============================================

        private void Start()
        {
            // Reset cache on Start to ensure fresh zone detection
            _lastSetZone = -1;
            Debug.Log("[ZoneBackgroundController] Start() - forcing background update");
            UpdateBackgroundForCurrentZone();
        }

        private void OnEnable()
        {
            // Reset cache and update when re-enabled (e.g., returning from combat or new run)
            _lastSetZone = -1;
            Debug.Log("[ZoneBackgroundController] OnEnable() - forcing background update");
            UpdateBackgroundForCurrentZone();
        }

        // ============================================
        // Public Methods
        // ============================================

        /// <summary>
        /// Updates the background based on the current zone from RunManager.
        /// </summary>
        public void UpdateBackgroundForCurrentZone()
        {
            Debug.Log($"[ZoneBackgroundController] UpdateBackgroundForCurrentZone called. " +
                      $"BackgroundImage assigned: {_backgroundImage != null}, BackgroundConfig assigned: {_backgroundConfig != null}");

            if (_backgroundImage == null)
            {
                Debug.LogWarning("[ZoneBackgroundController] No background image assigned");
                return;
            }

            // Get current zone from RunManager
            int zone = GetCurrentZone();

            Debug.Log($"[ZoneBackgroundController] Zone={zone}, LastSetZone={_lastSetZone}");

            // Skip update if zone hasn't changed
            if (zone == _lastSetZone)
            {
                Debug.Log($"[ZoneBackgroundController] Zone unchanged, skipping update");
                return;
            }

            _lastSetZone = zone;

            // Try to get sprite from config
            Sprite backgroundSprite = null;
            if (_backgroundConfig != null)
            {
                backgroundSprite = _backgroundConfig.GetZoneBackground(zone);
                Debug.Log($"[ZoneBackgroundController] Got sprite from config: {(backgroundSprite != null ? backgroundSprite.name : "NULL")}");
            }
            else
            {
                Debug.LogWarning("[ZoneBackgroundController] BackgroundConfig is NULL - cannot get zone sprite");
            }

            if (backgroundSprite != null)
            {
                _backgroundImage.sprite = backgroundSprite;
                _backgroundImage.color = Color.white;
                Debug.Log($"[ZoneBackgroundController] Set Zone {zone} background: {backgroundSprite.name}");
            }
            else
            {
                // Use fallback color
                _backgroundImage.sprite = null;
                _backgroundImage.color = _fallbackColor;
                Debug.Log($"[ZoneBackgroundController] Using fallback color for Zone {zone} (no sprite assigned)");
            }
        }

        /// <summary>
        /// Sets a specific zone background.
        /// </summary>
        /// <param name="zone">Zone number (1-3)</param>
        public void SetZoneBackground(int zone)
        {
            if (_backgroundImage == null || _backgroundConfig == null)
            {
                return;
            }

            _lastSetZone = zone;
            var sprite = _backgroundConfig.GetZoneBackground(zone);

            if (sprite != null)
            {
                _backgroundImage.sprite = sprite;
                _backgroundImage.color = Color.white;
                Debug.Log($"[ZoneBackgroundController] Manually set Zone {zone} background");
            }
            else
            {
                _backgroundImage.sprite = null;
                _backgroundImage.color = _fallbackColor;
            }
        }

        /// <summary>
        /// Forces a refresh of the background (useful when zone changes).
        /// </summary>
        public void ForceRefresh()
        {
            _lastSetZone = -1;
            UpdateBackgroundForCurrentZone();
        }

        // ============================================
        // Private Methods
        // ============================================

        private int GetCurrentZone()
        {
            // Try to get zone from RunManager - CurrentZone is always set correctly
            // whether it's a Battle Mission or story run
            if (ServiceLocator.TryGet<IRunManager>(out var runManager))
            {
                int zone = runManager.CurrentZone;
                Debug.Log($"[ZoneBackgroundController] GetCurrentZone: IsBattleMissionRun={runManager.IsBattleMissionRun}, " +
                          $"BattleMissionZone={runManager.BattleMissionZone}, CurrentZone={zone}, IsRunActive={runManager.IsRunActive}");

                if (zone > 0)
                {
                    return zone;
                }
            }
            else
            {
                Debug.LogWarning("[ZoneBackgroundController] RunManager not found in ServiceLocator");
            }

            // Default to zone 1
            Debug.Log("[ZoneBackgroundController] Defaulting to zone 1");
            return 1;
        }
    }
}
