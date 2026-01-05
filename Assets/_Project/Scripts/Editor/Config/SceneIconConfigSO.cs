// ============================================
// SceneIconConfigSO.cs
// Centralized sprite references for scene generation
// ============================================

using UnityEngine;

namespace HNR.Editor
{
    /// <summary>
    /// Configuration asset for scene icon sprites used by ProductionSceneSetupGenerator.
    /// Allows replacing emoji text placeholders with proper sprite-based icons.
    /// </summary>
    [CreateAssetMenu(fileName = "SceneIconConfig", menuName = "HNR/Config/Scene Icon Config")]
    public class SceneIconConfigSO : ScriptableObject
    {
        // ============================================
        // Deck Display Icons
        // ============================================

        [Header("Deck Display Icons")]
        [SerializeField, Tooltip("Draw pile icon (replaces book emoji)")]
        private Sprite _drawPileIcon;

        [SerializeField, Tooltip("Discard pile icon (replaces refresh emoji)")]
        private Sprite _discardPileIcon;

        // ============================================
        // System Menu Icons
        // ============================================

        [Header("System Menu Icons")]
        [SerializeField, Tooltip("Settings button icon (replaces gear emoji)")]
        private Sprite _settingsIcon;

        [SerializeField, Tooltip("Auto-battle button icon (replaces play emoji)")]
        private Sprite _autoBattleIcon;

        [SerializeField, Tooltip("Speed toggle button icon (optional, uses text by default)")]
        private Sprite _speedIcon;

        // ============================================
        // Map Legend Icons
        // ============================================

        [Header("Map Legend Icons")]
        [SerializeField, Tooltip("Combat node icon (replaces crossed swords emoji)")]
        private Sprite _combatNodeIcon;

        [SerializeField, Tooltip("Elite enemy node icon (replaces skull emoji)")]
        private Sprite _eliteNodeIcon;

        [SerializeField, Tooltip("Shop node icon (replaces shopping cart emoji)")]
        private Sprite _shopNodeIcon;

        [SerializeField, Tooltip("Echo event node icon (replaces question mark emoji)")]
        private Sprite _echoEventIcon;

        [SerializeField, Tooltip("Sanctuary node icon (replaces candle emoji)")]
        private Sprite _sanctuaryIcon;

        [SerializeField, Tooltip("Treasure node icon (replaces diamond emoji)")]
        private Sprite _treasureIcon;

        [SerializeField, Tooltip("Boss node icon (replaces demon emoji)")]
        private Sprite _bossNodeIcon;

        // ============================================
        // Combat UI Icons
        // ============================================

        [Header("Combat UI Icons")]
        [SerializeField, Tooltip("Checkmark icon for execution button")]
        private Sprite _checkmarkIcon;

        // ============================================
        // Settings Category Icons
        // ============================================

        [Header("Settings Category Icons")]
        [SerializeField, Tooltip("Display settings icon (replaces monitor emoji)")]
        private Sprite _displaySettingsIcon;

        [SerializeField, Tooltip("Audio settings icon (replaces headphones emoji)")]
        private Sprite _audioSettingsIcon;

        [SerializeField, Tooltip("Game settings icon (replaces gear emoji)")]
        private Sprite _gameSettingsIcon;

        [SerializeField, Tooltip("Network settings icon (replaces globe emoji)")]
        private Sprite _networkSettingsIcon;

        [SerializeField, Tooltip("Account settings icon (replaces person emoji)")]
        private Sprite _accountSettingsIcon;

        // ============================================
        // Properties - Deck Display
        // ============================================

        /// <summary>Draw pile icon sprite.</summary>
        public Sprite DrawPileIcon => _drawPileIcon;

        /// <summary>Discard pile icon sprite.</summary>
        public Sprite DiscardPileIcon => _discardPileIcon;

        // ============================================
        // Properties - System Menu
        // ============================================

        /// <summary>Settings button icon sprite.</summary>
        public Sprite SettingsIcon => _settingsIcon;

        /// <summary>Auto-battle button icon sprite.</summary>
        public Sprite AutoBattleIcon => _autoBattleIcon;

        /// <summary>Speed toggle button icon sprite (optional).</summary>
        public Sprite SpeedIcon => _speedIcon;

        // ============================================
        // Properties - Map Legend
        // ============================================

        /// <summary>Combat node icon sprite.</summary>
        public Sprite CombatNodeIcon => _combatNodeIcon;

        /// <summary>Elite enemy node icon sprite.</summary>
        public Sprite EliteNodeIcon => _eliteNodeIcon;

        /// <summary>Shop node icon sprite.</summary>
        public Sprite ShopNodeIcon => _shopNodeIcon;

        /// <summary>Echo event node icon sprite.</summary>
        public Sprite EchoEventIcon => _echoEventIcon;

        /// <summary>Sanctuary node icon sprite.</summary>
        public Sprite SanctuaryIcon => _sanctuaryIcon;

        /// <summary>Treasure node icon sprite.</summary>
        public Sprite TreasureIcon => _treasureIcon;

        /// <summary>Boss node icon sprite.</summary>
        public Sprite BossNodeIcon => _bossNodeIcon;

        // ============================================
        // Properties - Combat UI
        // ============================================

        /// <summary>Checkmark icon sprite.</summary>
        public Sprite CheckmarkIcon => _checkmarkIcon;

        // ============================================
        // Properties - Settings Categories
        // ============================================

        /// <summary>Display settings icon sprite.</summary>
        public Sprite DisplaySettingsIcon => _displaySettingsIcon;

        /// <summary>Audio settings icon sprite.</summary>
        public Sprite AudioSettingsIcon => _audioSettingsIcon;

        /// <summary>Game settings icon sprite.</summary>
        public Sprite GameSettingsIcon => _gameSettingsIcon;

        /// <summary>Network settings icon sprite.</summary>
        public Sprite NetworkSettingsIcon => _networkSettingsIcon;

        /// <summary>Account settings icon sprite.</summary>
        public Sprite AccountSettingsIcon => _accountSettingsIcon;

        // ============================================
        // Helper Methods
        // ============================================

        /// <summary>
        /// Get map node icon by node type name.
        /// </summary>
        /// <param name="nodeType">Node type: Combat, Elite, Shop, Echo, Sanctuary, Treasure, Boss</param>
        /// <returns>Corresponding sprite or null if not found</returns>
        public Sprite GetMapNodeIcon(string nodeType)
        {
            return nodeType switch
            {
                "Combat" => _combatNodeIcon,
                "Elite" => _eliteNodeIcon,
                "Shop" => _shopNodeIcon,
                "Echo" => _echoEventIcon,
                "Sanctuary" => _sanctuaryIcon,
                "Treasure" => _treasureIcon,
                "Boss" => _bossNodeIcon,
                _ => null
            };
        }

        /// <summary>
        /// Get settings category icon by category name.
        /// </summary>
        /// <param name="category">Category: Display, Audio, Game, Network, Account</param>
        /// <returns>Corresponding sprite or null if not found</returns>
        public Sprite GetSettingsCategoryIcon(string category)
        {
            return category switch
            {
                "Display" => _displaySettingsIcon,
                "Audio" => _audioSettingsIcon,
                "Game" => _gameSettingsIcon,
                "Network" => _networkSettingsIcon,
                "Account" => _accountSettingsIcon,
                _ => null
            };
        }

        // ============================================
        // Validation
        // ============================================

        /// <summary>
        /// Validate all icons are assigned.
        /// </summary>
        /// <returns>True if all icons are assigned</returns>
        public bool ValidateAllIcons()
        {
            bool valid = true;

            // Deck icons
            if (_drawPileIcon == null) { Debug.LogWarning("[SceneIconConfig] Draw Pile Icon not assigned"); valid = false; }
            if (_discardPileIcon == null) { Debug.LogWarning("[SceneIconConfig] Discard Pile Icon not assigned"); valid = false; }

            // System menu
            if (_settingsIcon == null) { Debug.LogWarning("[SceneIconConfig] Settings Icon not assigned"); valid = false; }
            if (_autoBattleIcon == null) { Debug.LogWarning("[SceneIconConfig] Auto-Battle Icon not assigned"); valid = false; }
            // Note: _speedIcon is optional, not validated

            // Map legend
            if (_combatNodeIcon == null) { Debug.LogWarning("[SceneIconConfig] Combat Node Icon not assigned"); valid = false; }
            if (_eliteNodeIcon == null) { Debug.LogWarning("[SceneIconConfig] Elite Node Icon not assigned"); valid = false; }
            if (_shopNodeIcon == null) { Debug.LogWarning("[SceneIconConfig] Shop Node Icon not assigned"); valid = false; }
            if (_echoEventIcon == null) { Debug.LogWarning("[SceneIconConfig] Echo Event Icon not assigned"); valid = false; }
            if (_sanctuaryIcon == null) { Debug.LogWarning("[SceneIconConfig] Sanctuary Icon not assigned"); valid = false; }
            if (_treasureIcon == null) { Debug.LogWarning("[SceneIconConfig] Treasure Icon not assigned"); valid = false; }
            if (_bossNodeIcon == null) { Debug.LogWarning("[SceneIconConfig] Boss Node Icon not assigned"); valid = false; }

            // Combat UI
            if (_checkmarkIcon == null) { Debug.LogWarning("[SceneIconConfig] Checkmark Icon not assigned"); valid = false; }

            // Settings categories
            if (_displaySettingsIcon == null) { Debug.LogWarning("[SceneIconConfig] Display Settings Icon not assigned"); valid = false; }
            if (_audioSettingsIcon == null) { Debug.LogWarning("[SceneIconConfig] Audio Settings Icon not assigned"); valid = false; }
            if (_gameSettingsIcon == null) { Debug.LogWarning("[SceneIconConfig] Game Settings Icon not assigned"); valid = false; }
            if (_networkSettingsIcon == null) { Debug.LogWarning("[SceneIconConfig] Network Settings Icon not assigned"); valid = false; }
            if (_accountSettingsIcon == null) { Debug.LogWarning("[SceneIconConfig] Account Settings Icon not assigned"); valid = false; }

            if (valid)
            {
                Debug.Log("[SceneIconConfig] All 18 required icons validated successfully");
            }

            return valid;
        }

        /// <summary>
        /// Get count of assigned icons.
        /// </summary>
        public int GetAssignedCount()
        {
            int count = 0;
            if (_drawPileIcon != null) count++;
            if (_discardPileIcon != null) count++;
            if (_settingsIcon != null) count++;
            if (_autoBattleIcon != null) count++;
            if (_speedIcon != null) count++;
            if (_combatNodeIcon != null) count++;
            if (_eliteNodeIcon != null) count++;
            if (_shopNodeIcon != null) count++;
            if (_echoEventIcon != null) count++;
            if (_sanctuaryIcon != null) count++;
            if (_treasureIcon != null) count++;
            if (_bossNodeIcon != null) count++;
            if (_checkmarkIcon != null) count++;
            if (_displaySettingsIcon != null) count++;
            if (_audioSettingsIcon != null) count++;
            if (_gameSettingsIcon != null) count++;
            if (_networkSettingsIcon != null) count++;
            if (_accountSettingsIcon != null) count++;
            return count;
        }
    }
}
