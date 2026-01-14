// ============================================
// BackgroundConfigSO.cs
// Configuration for scene and screen background sprites
// ============================================

using UnityEngine;

namespace HNR.UI.Config
{
    /// <summary>
    /// ScriptableObject that stores background sprites for all scenes and screens.
    /// Used by ProductionSceneSetupGenerator and runtime background managers.
    /// </summary>
    [CreateAssetMenu(fileName = "BackgroundConfig", menuName = "HNR/Config/Background Config")]
    public class BackgroundConfigSO : ScriptableObject
    {
        // ============================================
        // Scene Backgrounds
        // ============================================

        [Header("Scene Backgrounds")]
        [SerializeField, Tooltip("Main menu background (void gateway)")]
        private Sprite _mainMenuBackground;

        [SerializeField, Tooltip("Bastion hub background (safe haven)")]
        private Sprite _bastionBackground;

        [SerializeField, Tooltip("Missions screen background (strategic command)")]
        private Sprite _missionsBackground;

        [SerializeField, Tooltip("Battle mission selection background (zone vista)")]
        private Sprite _battleMissionBackground;

        [SerializeField, Tooltip("Requiems character viewer background (soul chamber)")]
        private Sprite _requiemsBackground;

        // ============================================
        // NullRift Zone Backgrounds
        // ============================================

        [Header("NullRift Zone Backgrounds")]
        [SerializeField, Tooltip("Zone 1 - The Outer Reaches (corrupted wilderness)")]
        private Sprite _nullRiftZone1Background;

        [SerializeField, Tooltip("Zone 2 - The Hollow Depths (underground caverns)")]
        private Sprite _nullRiftZone2Background;

        [SerializeField, Tooltip("Zone 3 - The Null Core (heart of corruption)")]
        private Sprite _nullRiftZone3Background;

        // ============================================
        // Combat Backgrounds (by encounter type)
        // ============================================

        [Header("Combat Backgrounds")]
        [SerializeField, Tooltip("Normal combat encounter background")]
        private Sprite _combatNormalBackground;

        [SerializeField, Tooltip("Elite combat encounter background")]
        private Sprite _combatEliteBackground;

        [SerializeField, Tooltip("Boss combat encounter background")]
        private Sprite _combatBossBackground;

        // ============================================
        // Screen Backgrounds (Overlays)
        // ============================================

        [Header("Screen Backgrounds")]
        [SerializeField, Tooltip("Sanctuary rest stop background (campfire scene)")]
        private Sprite _sanctuaryBackground;

        [SerializeField, Tooltip("Shop screen background (void merchant bazaar)")]
        private Sprite _shopBackground;

        // ============================================
        // Sanctuary Special Elements
        // ============================================

        [Header("Sanctuary Elements")]
        [SerializeField, Tooltip("Campfire sprite for Sanctuary screen")]
        private Sprite _campfireSprite;

        // ============================================
        // Fallback Colors (used when sprites not assigned)
        // ============================================

        [Header("Fallback Colors")]
        [SerializeField, Tooltip("MainMenu fallback color")]
        private Color _mainMenuFallbackColor = new Color(0.05f, 0.02f, 0.1f);

        [SerializeField, Tooltip("Bastion fallback color")]
        private Color _bastionFallbackColor = new Color(0.08f, 0.05f, 0.12f);

        [SerializeField, Tooltip("Missions fallback color")]
        private Color _missionsFallbackColor = new Color(0.08f, 0.08f, 0.12f);

        [SerializeField, Tooltip("BattleMission fallback color")]
        private Color _battleMissionFallbackColor = new Color(0.08f, 0.08f, 0.12f);

        [SerializeField, Tooltip("Requiems fallback color")]
        private Color _requiemsFallbackColor = new Color(0.08f, 0.08f, 0.12f);

        [SerializeField, Tooltip("NullRift fallback color")]
        private Color _nullRiftFallbackColor = new Color(0.03f, 0.01f, 0.08f);

        [SerializeField, Tooltip("Sanctuary fallback color")]
        private Color _sanctuaryFallbackColor = new Color(0.15f, 0.08f, 0.05f);

        [SerializeField, Tooltip("Shop fallback color")]
        private Color _shopFallbackColor = new Color(0.08f, 0.08f, 0.1f);

        [SerializeField, Tooltip("Combat fallback color")]
        private Color _combatFallbackColor = new Color(0.02f, 0.01f, 0.05f);

        // ============================================
        // Scene Background Accessors
        // ============================================

        public Sprite MainMenuBackground => _mainMenuBackground;
        public Sprite BastionBackground => _bastionBackground;
        public Sprite MissionsBackground => _missionsBackground;
        public Sprite BattleMissionBackground => _battleMissionBackground;
        public Sprite RequiemsBackground => _requiemsBackground;

        // ============================================
        // NullRift Zone Accessors
        // ============================================

        public Sprite NullRiftZone1Background => _nullRiftZone1Background;
        public Sprite NullRiftZone2Background => _nullRiftZone2Background;
        public Sprite NullRiftZone3Background => _nullRiftZone3Background;

        // ============================================
        // Combat Background Accessors
        // ============================================

        public Sprite CombatNormalBackground => _combatNormalBackground;
        public Sprite CombatEliteBackground => _combatEliteBackground;
        public Sprite CombatBossBackground => _combatBossBackground;

        // ============================================
        // Screen Background Accessors
        // ============================================

        public Sprite SanctuaryBackground => _sanctuaryBackground;
        public Sprite ShopBackground => _shopBackground;
        public Sprite CampfireSprite => _campfireSprite;

        // ============================================
        // Fallback Color Accessors
        // ============================================

        public Color MainMenuFallbackColor => _mainMenuFallbackColor;
        public Color BastionFallbackColor => _bastionFallbackColor;
        public Color MissionsFallbackColor => _missionsFallbackColor;
        public Color BattleMissionFallbackColor => _battleMissionFallbackColor;
        public Color RequiemsFallbackColor => _requiemsFallbackColor;
        public Color NullRiftFallbackColor => _nullRiftFallbackColor;
        public Color SanctuaryFallbackColor => _sanctuaryFallbackColor;
        public Color ShopFallbackColor => _shopFallbackColor;
        public Color CombatFallbackColor => _combatFallbackColor;

        // ============================================
        // Public API
        // ============================================

        /// <summary>
        /// Gets the background sprite for a specific NullRift zone.
        /// </summary>
        /// <param name="zone">Zone number (1-3)</param>
        /// <returns>The zone background sprite, or null if not assigned.</returns>
        public Sprite GetZoneBackground(int zone)
        {
            return zone switch
            {
                1 => _nullRiftZone1Background,
                2 => _nullRiftZone2Background,
                3 => _nullRiftZone3Background,
                _ => _nullRiftZone1Background
            };
        }

        /// <summary>
        /// Gets the combat background sprite based on encounter type.
        /// </summary>
        /// <param name="encounterType">Encounter type: "normal", "elite", or "boss"</param>
        /// <returns>The appropriate combat background sprite, or null if not assigned.</returns>
        public Sprite GetCombatBackground(string encounterType)
        {
            return encounterType?.ToLower() switch
            {
                "elite" => _combatEliteBackground,
                "boss" => _combatBossBackground,
                _ => _combatNormalBackground // Default to normal for "normal" or unknown types
            };
        }

        /// <summary>
        /// Checks if all combat backgrounds are assigned.
        /// </summary>
        /// <returns>True if all combat backgrounds are assigned.</returns>
        public bool HasAllCombatBackgrounds()
        {
            return _combatNormalBackground != null &&
                   _combatEliteBackground != null &&
                   _combatBossBackground != null;
        }

        /// <summary>
        /// Gets the background sprite for a scene by name.
        /// </summary>
        /// <param name="sceneName">Scene name (MainMenu, Bastion, etc.)</param>
        /// <returns>The background sprite, or null if not found.</returns>
        public Sprite GetSceneBackground(string sceneName)
        {
            return sceneName switch
            {
                "MainMenu" => _mainMenuBackground,
                "Bastion" => _bastionBackground,
                "Missions" => _missionsBackground,
                "BattleMission" => _battleMissionBackground,
                "Requiems" => _requiemsBackground,
                "NullRift" => _nullRiftZone1Background, // Default to Zone 1
                _ => null
            };
        }

        /// <summary>
        /// Gets the fallback color for a scene by name.
        /// </summary>
        /// <param name="sceneName">Scene name</param>
        /// <returns>The fallback color.</returns>
        public Color GetSceneFallbackColor(string sceneName)
        {
            return sceneName switch
            {
                "MainMenu" => _mainMenuFallbackColor,
                "Bastion" => _bastionFallbackColor,
                "Missions" => _missionsFallbackColor,
                "BattleMission" => _battleMissionFallbackColor,
                "Requiems" => _requiemsFallbackColor,
                "NullRift" => _nullRiftFallbackColor,
                "Sanctuary" => _sanctuaryFallbackColor,
                "Shop" => _shopFallbackColor,
                _ => new Color(0.05f, 0.02f, 0.1f)
            };
        }

        /// <summary>
        /// Checks if all scene backgrounds are assigned.
        /// </summary>
        /// <returns>True if all required backgrounds are assigned.</returns>
        public bool HasAllSceneBackgrounds()
        {
            return _mainMenuBackground != null &&
                   _bastionBackground != null &&
                   _missionsBackground != null &&
                   _battleMissionBackground != null &&
                   _requiemsBackground != null;
        }

        /// <summary>
        /// Checks if all zone backgrounds are assigned.
        /// </summary>
        /// <returns>True if all zone backgrounds are assigned.</returns>
        public bool HasAllZoneBackgrounds()
        {
            return _nullRiftZone1Background != null &&
                   _nullRiftZone2Background != null &&
                   _nullRiftZone3Background != null;
        }

        /// <summary>
        /// Checks if all screen backgrounds are assigned.
        /// </summary>
        /// <returns>True if all screen backgrounds are assigned.</returns>
        public bool HasAllScreenBackgrounds()
        {
            return _sanctuaryBackground != null &&
                   _shopBackground != null;
        }

        /// <summary>
        /// Gets the count of assigned backgrounds.
        /// </summary>
        /// <returns>Number of assigned background sprites.</returns>
        public int GetAssignedCount()
        {
            int count = 0;
            if (_mainMenuBackground != null) count++;
            if (_bastionBackground != null) count++;
            if (_missionsBackground != null) count++;
            if (_battleMissionBackground != null) count++;
            if (_requiemsBackground != null) count++;
            if (_nullRiftZone1Background != null) count++;
            if (_nullRiftZone2Background != null) count++;
            if (_nullRiftZone3Background != null) count++;
            if (_combatNormalBackground != null) count++;
            if (_combatEliteBackground != null) count++;
            if (_combatBossBackground != null) count++;
            if (_sanctuaryBackground != null) count++;
            if (_shopBackground != null) count++;
            return count;
        }
    }
}
