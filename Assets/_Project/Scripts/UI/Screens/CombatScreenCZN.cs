// ============================================
// CombatScreenCZN.cs
// Combat screen with CZN layout integrating all combat UI components
// ============================================

using UnityEngine;
using HNR.Core;
using HNR.Core.Events;
using HNR.Core.Interfaces;
using HNR.Combat;
using HNR.Characters;
using HNR.UI.Combat;

namespace HNR.UI.Screens
{
    /// <summary>
    /// Combat screen with CZN layout integrating all combat UI components.
    /// Features: top HUD, left sidebar, bottom command center, world-space UI.
    /// </summary>
    public class CombatScreenCZN : ScreenBase
    {
        // ============================================
        // Top HUD
        // ============================================

        [Header("Top HUD")]
        [SerializeField, Tooltip("Wide HP bar with embedded portraits")]
        private SharedVitalityBarCZN _vitalityBar;

        [SerializeField, Tooltip("Speed, auto-battle, settings buttons")]
        private SystemMenuBar _systemMenu;

        // ============================================
        // Left Sidebar
        // ============================================

        [Header("Left Sidebar")]
        [SerializeField, Tooltip("Party member slots with EP gauges")]
        private PartyStatusSidebar _partySidebar;

        // ============================================
        // Bottom Command Center
        // ============================================

        [Header("Bottom Command Center")]
        [SerializeField, Tooltip("Curved card fan layout")]
        private CardFanLayout _cardFanLayout;

        [SerializeField, Tooltip("Large AP number display")]
        private APCounterDisplay _apCounter;

        [SerializeField, Tooltip("End turn button")]
        private ExecutionButton _executionButton;

        // ============================================
        // World Space UI
        // ============================================

        [Header("World Space")]
        [SerializeField, Tooltip("Container for enemy floating UIs")]
        private Transform _enemyUIContainer;

        [SerializeField, Tooltip("Container for ally indicators")]
        private Transform _allyIndicatorContainer;

        [SerializeField, Tooltip("Prefab for enemy floating UI")]
        private EnemyFloatingUI _enemyUIPrefab;

        [SerializeField, Tooltip("Prefab for ally indicator")]
        private AllyIndicator _allyIndicatorPrefab;

        // ============================================
        // Runtime State
        // ============================================

        private CombatContext _context;
        private int _cardDrawIndex;

        // ============================================
        // Unity Lifecycle
        // ============================================

        protected virtual void Awake()
        {
            _showGlobalHeader = false;
            _showGlobalNav = false;
        }

        // ============================================
        // ScreenBase Overrides
        // ============================================

        public override void OnShow()
        {
            base.OnShow();

            // Get combat context
            if (ServiceLocator.TryGet<TurnManager>(out var turnManager))
            {
                _context = turnManager.Context;
            }

            if (_context == null)
            {
                Debug.LogWarning("[CombatScreenCZN] No combat context available");
                return;
            }

            _cardDrawIndex = 0;
            InitializeUI();
            SubscribeToEvents();
        }

        public override void OnHide()
        {
            base.OnHide();
            UnsubscribeFromEvents();

            if (_systemMenu != null)
            {
                _systemMenu.ResetOnCombatEnd();
            }

            ClearWorldSpaceUI();
        }

        // ============================================
        // Initialization
        // ============================================

        private void InitializeUI()
        {
            // Top HUD
            if (_vitalityBar != null)
            {
                _vitalityBar.SetPartyPortraits(_context.Team.ToArray());
                _vitalityBar.Initialize(_context.TeamHP, _context.TeamMaxHP, _context.TeamBlock);
            }

            // Left Sidebar
            if (_partySidebar != null)
            {
                _partySidebar.Initialize(_context.Team.ToArray());
            }

            // Bottom Command Center
            if (_apCounter != null)
            {
                _apCounter.SetAP(_context.CurrentAP, _context.MaxAP);
            }

            // World Space
            SpawnEnemyUIs();
            SpawnAllyIndicators();
        }

        // ============================================
        // World Space UI Management
        // ============================================

        private void SpawnEnemyUIs()
        {
            if (_enemyUIContainer == null || _enemyUIPrefab == null) return;

            // Clear existing
            foreach (Transform child in _enemyUIContainer)
            {
                Destroy(child.gameObject);
            }

            // Spawn for each enemy
            foreach (var enemy in _context.Enemies)
            {
                if (enemy == null || enemy.IsDead) continue;

                var ui = Instantiate(_enemyUIPrefab, _enemyUIContainer);
                ui.Initialize(enemy);
            }
        }

        private void SpawnAllyIndicators()
        {
            if (_allyIndicatorContainer == null || _allyIndicatorPrefab == null) return;

            // Clear existing
            foreach (Transform child in _allyIndicatorContainer)
            {
                Destroy(child.gameObject);
            }

            // Spawn for each team member
            foreach (var requiem in _context.Team)
            {
                if (requiem == null) continue;

                var indicator = Instantiate(_allyIndicatorPrefab, _allyIndicatorContainer);
                indicator.Initialize(requiem, requiem.transform);
            }
        }

        private void ClearWorldSpaceUI()
        {
            if (_enemyUIContainer != null)
            {
                foreach (Transform child in _enemyUIContainer)
                {
                    Destroy(child.gameObject);
                }
            }

            if (_allyIndicatorContainer != null)
            {
                foreach (Transform child in _allyIndicatorContainer)
                {
                    Destroy(child.gameObject);
                }
            }
        }

        // ============================================
        // Event Management
        // ============================================

        private void SubscribeToEvents()
        {
            EventBus.Subscribe<CardDrawnEvent>(OnCardDrawn);
            EventBus.Subscribe<CardPlayedEvent>(OnCardPlayed);
            EventBus.Subscribe<CardDiscardedEvent>(OnCardDiscarded);
            EventBus.Subscribe<CombatEndedEvent>(OnCombatEnded);
            EventBus.Subscribe<EnemyDefeatedEvent>(OnEnemyDefeated);
        }

        private void UnsubscribeFromEvents()
        {
            EventBus.Unsubscribe<CardDrawnEvent>(OnCardDrawn);
            EventBus.Unsubscribe<CardPlayedEvent>(OnCardPlayed);
            EventBus.Unsubscribe<CardDiscardedEvent>(OnCardDiscarded);
            EventBus.Unsubscribe<CombatEndedEvent>(OnCombatEnded);
            EventBus.Unsubscribe<EnemyDefeatedEvent>(OnEnemyDefeated);
        }

        // ============================================
        // Event Handlers
        // ============================================

        private void OnCardDrawn(CardDrawnEvent evt)
        {
            if (_cardFanLayout == null || evt.Card == null) return;

            // Get card from pool
            if (ServiceLocator.TryGet<IPoolManager>(out var poolManager))
            {
                var card = poolManager.Get<CombatCard>();
                if (card != null)
                {
                    card.Initialize(evt.Card);
                    _cardFanLayout.AddCard(card, _cardDrawIndex * 0.1f);
                    _cardDrawIndex++;
                }
            }
        }

        private void OnCardPlayed(CardPlayedEvent evt)
        {
            // Card removal handled by CardFanLayout internally
            _cardDrawIndex = Mathf.Max(0, _cardDrawIndex - 1);
        }

        private void OnCardDiscarded(CardDiscardedEvent evt)
        {
            _cardDrawIndex = Mathf.Max(0, _cardDrawIndex - 1);
        }

        private void OnEnemyDefeated(EnemyDefeatedEvent evt)
        {
            // Enemy UI handles its own cleanup via event subscription
        }

        private void OnCombatEnded(CombatEndedEvent evt)
        {
            // Combat result handling
            // Victory/Defeat screens would be shown via UIManager
            if (ServiceLocator.TryGet<IUIManager>(out var uiManager))
            {
                // Screen navigation handled by game state system
                Debug.Log($"[CombatScreenCZN] Combat ended - Victory: {evt.Victory}");
            }
        }

        // ============================================
        // Public API
        // ============================================

        /// <summary>
        /// Sets the active party member highlight in the sidebar.
        /// </summary>
        /// <param name="index">Index of active member (0-2).</param>
        public void SetActivePartyMember(int index)
        {
            if (_partySidebar != null)
            {
                _partySidebar.SetActiveSlot(index);
            }
        }

        /// <summary>
        /// Refreshes all UI components from current context state.
        /// </summary>
        public void RefreshUI()
        {
            if (_context == null) return;

            if (_vitalityBar != null)
            {
                _vitalityBar.UpdateHealth(_context.TeamHP, _context.TeamMaxHP);
                _vitalityBar.UpdateBlock(_context.TeamBlock);
            }

            if (_partySidebar != null)
            {
                _partySidebar.RefreshAll();
            }

            if (_apCounter != null)
            {
                _apCounter.SetAP(_context.CurrentAP, _context.MaxAP);
            }
        }
    }
}
