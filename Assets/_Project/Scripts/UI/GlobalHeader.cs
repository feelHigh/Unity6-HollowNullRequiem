// ============================================
// GlobalHeader.cs
// Top bar with player profile and currency tickers
// ============================================

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using HNR.Core.Events;
using HNR.UI.Components;

namespace HNR.UI
{
    /// <summary>
    /// Global header bar anchored at top of screen.
    /// Shows player profile, currency tickers, and event banners.
    /// </summary>
    public class GlobalHeader : MonoBehaviour
    {
        // ============================================
        // Player Profile
        // ============================================

        [Header("Player Profile")]
        [SerializeField] private Image _avatarFrame;
        [SerializeField] private Image _avatarImage;
        [SerializeField] private TMP_Text _playerName;
        [SerializeField] private TMP_Text _playerLevel;
        [SerializeField] private Image _expBar;

        // ============================================
        // Currency Tickers
        // ============================================

        [Header("Currency Tickers")]
        [SerializeField] private CurrencyTicker _soulCrystals;
        [SerializeField] private CurrencyTicker _voidDust;
        [SerializeField] private CurrencyTicker _aetherStamina;

        // ============================================
        // Event Banner
        // ============================================

        [Header("Event Banner")]
        [SerializeField] private GameObject _eventBanner;
        [SerializeField] private TMP_Text _eventText;

        // ============================================
        // Unity Lifecycle
        // ============================================

        private void OnEnable()
        {
            EventBus.Subscribe<CurrencyChangedEvent>(OnCurrencyChanged);
            EventBus.Subscribe<PlayerLevelChangedEvent>(OnPlayerLevelChanged);
            EventBus.Subscribe<PlayerExpChangedEvent>(OnPlayerExpChanged);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<CurrencyChangedEvent>(OnCurrencyChanged);
            EventBus.Unsubscribe<PlayerLevelChangedEvent>(OnPlayerLevelChanged);
            EventBus.Unsubscribe<PlayerExpChangedEvent>(OnPlayerExpChanged);
        }

        // ============================================
        // Event Handlers
        // ============================================

        private void OnCurrencyChanged(CurrencyChangedEvent evt)
        {
            var ticker = GetTickerForCurrency(evt.CurrencyType);
            ticker?.AnimateToValue(evt.NewValue);
        }

        private void OnPlayerLevelChanged(PlayerLevelChangedEvent evt)
        {
            if (_playerLevel != null)
            {
                _playerLevel.text = $"Lv.{evt.NewLevel}";
            }
        }

        private void OnPlayerExpChanged(PlayerExpChangedEvent evt)
        {
            if (_expBar != null)
            {
                _expBar.fillAmount = evt.MaxExp > 0 ? (float)evt.CurrentExp / evt.MaxExp : 0f;
            }
        }

        // ============================================
        // Currency Ticker Mapping
        // ============================================

        private CurrencyTicker GetTickerForCurrency(CurrencyType type)
        {
            return type switch
            {
                CurrencyType.SoulCrystals => _soulCrystals,
                CurrencyType.VoidDust => _voidDust,
                CurrencyType.AetherStamina => _aetherStamina,
                _ => null
            };
        }

        // ============================================
        // Public API
        // ============================================

        /// <summary>
        /// Initialize header with player data.
        /// </summary>
        public void Initialize(string playerName, int level, int exp, int maxExp,
            int soulCrystals, int voidDust, int aetherStamina, Sprite avatar = null)
        {
            SetPlayerInfo(playerName, level, exp, maxExp, avatar);
            SetCurrencies(soulCrystals, voidDust, aetherStamina);
        }

        /// <summary>
        /// Set player profile information.
        /// </summary>
        public void SetPlayerInfo(string name, int level, int exp, int maxExp, Sprite avatar = null)
        {
            if (_playerName != null)
            {
                _playerName.text = name;
            }

            if (_playerLevel != null)
            {
                _playerLevel.text = $"Lv.{level}";
            }

            if (_expBar != null)
            {
                _expBar.fillAmount = maxExp > 0 ? (float)exp / maxExp : 0f;
            }

            if (_avatarImage != null && avatar != null)
            {
                _avatarImage.sprite = avatar;
            }
        }

        /// <summary>
        /// Set all currency values immediately (no animation).
        /// </summary>
        public void SetCurrencies(int soulCrystals, int voidDust, int aetherStamina)
        {
            _soulCrystals?.SetValueImmediate(soulCrystals);
            _voidDust?.SetValueImmediate(voidDust);
            _aetherStamina?.SetValueImmediate(aetherStamina);
        }

        /// <summary>
        /// Show or hide the event banner.
        /// </summary>
        public void SetEventBanner(bool visible, string text = null)
        {
            if (_eventBanner != null)
            {
                _eventBanner.SetActive(visible);
            }

            if (_eventText != null && text != null)
            {
                _eventText.text = text;
            }
        }
    }

    // ============================================
    // Supporting Enums and Events
    // ============================================

    /// <summary>
    /// Types of currency in the game.
    /// </summary>
    public enum CurrencyType
    {
        /// <summary>Premium currency for special purchases.</summary>
        SoulCrystals,
        /// <summary>Standard upgrade currency.</summary>
        VoidDust,
        /// <summary>Stamina for starting runs.</summary>
        AetherStamina,
        /// <summary>Run-only currency (Void Shards handled separately).</summary>
        VoidShards
    }

    /// <summary>
    /// Event fired when a currency value changes.
    /// </summary>
    public class CurrencyChangedEvent : GameEvent
    {
        public CurrencyType CurrencyType { get; }
        public int OldValue { get; }
        public int NewValue { get; }

        public CurrencyChangedEvent(CurrencyType type, int oldValue, int newValue)
        {
            CurrencyType = type;
            OldValue = oldValue;
            NewValue = newValue;
        }
    }

    /// <summary>
    /// Event fired when player level changes.
    /// </summary>
    public class PlayerLevelChangedEvent : GameEvent
    {
        public int OldLevel { get; }
        public int NewLevel { get; }

        public PlayerLevelChangedEvent(int oldLevel, int newLevel)
        {
            OldLevel = oldLevel;
            NewLevel = newLevel;
        }
    }

    /// <summary>
    /// Event fired when player experience changes.
    /// </summary>
    public class PlayerExpChangedEvent : GameEvent
    {
        public int CurrentExp { get; }
        public int MaxExp { get; }

        public PlayerExpChangedEvent(int currentExp, int maxExp)
        {
            CurrentExp = currentExp;
            MaxExp = maxExp;
        }
    }
}
