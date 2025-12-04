// ============================================
// CombatScreen.cs
// Combat UI screen with resource displays
// ============================================

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using HNR.Core;
using HNR.Core.Events;
using HNR.Combat;

namespace HNR.UI
{
    /// <summary>
    /// Combat UI screen displaying resources, deck info, and turn controls.
    /// Subscribes to combat events for real-time updates.
    /// </summary>
    public class CombatScreen : ScreenBase
    {
        // ============================================
        // Resource Displays
        // ============================================

        [Header("Resource Displays")]
        [SerializeField, Tooltip("Action Points display")]
        private TextMeshProUGUI _apText;

        [SerializeField, Tooltip("Team HP slider")]
        private Slider _hpSlider;

        [SerializeField, Tooltip("Team HP text display")]
        private TextMeshProUGUI _hpText;

        [SerializeField, Tooltip("Block amount display")]
        private TextMeshProUGUI _blockText;

        // ============================================
        // Deck Info
        // ============================================

        [Header("Deck Info")]
        [SerializeField, Tooltip("Draw pile count")]
        private TextMeshProUGUI _drawPileText;

        [SerializeField, Tooltip("Discard pile count")]
        private TextMeshProUGUI _discardPileText;

        // ============================================
        // Controls
        // ============================================

        [Header("Controls")]
        [SerializeField, Tooltip("End turn button")]
        private Button _endTurnButton;

        // ============================================
        // Turn Indicator
        // ============================================

        [Header("Turn Indicator")]
        [SerializeField, Tooltip("Current turn display")]
        private TextMeshProUGUI _turnText;

        [SerializeField, Tooltip("Phase display")]
        private TextMeshProUGUI _phaseText;

        // ============================================
        // Unity Lifecycle
        // ============================================

        private void Start()
        {
            if (_endTurnButton != null)
            {
                _endTurnButton.onClick.AddListener(OnEndTurnClicked);
            }

            SubscribeToEvents();
        }

        private void OnDestroy()
        {
            if (_endTurnButton != null)
            {
                _endTurnButton.onClick.RemoveListener(OnEndTurnClicked);
            }

            UnsubscribeFromEvents();
        }

        // ============================================
        // Event Subscriptions
        // ============================================

        private void SubscribeToEvents()
        {
            EventBus.Subscribe<APChangedEvent>(OnAPChanged);
            EventBus.Subscribe<TeamHPChangedEvent>(OnTeamHPChanged);
            EventBus.Subscribe<BlockChangedEvent>(OnBlockChanged);
            EventBus.Subscribe<TurnStartedEvent>(OnTurnStarted);
            EventBus.Subscribe<CardDrawnEvent>(OnCardDrawn);
            EventBus.Subscribe<CardDiscardedEvent>(OnCardDiscarded);
            EventBus.Subscribe<CombatPhaseChangedEvent>(OnPhaseChanged);
        }

        private void UnsubscribeFromEvents()
        {
            EventBus.Unsubscribe<APChangedEvent>(OnAPChanged);
            EventBus.Unsubscribe<TeamHPChangedEvent>(OnTeamHPChanged);
            EventBus.Unsubscribe<BlockChangedEvent>(OnBlockChanged);
            EventBus.Unsubscribe<TurnStartedEvent>(OnTurnStarted);
            EventBus.Unsubscribe<CardDrawnEvent>(OnCardDrawn);
            EventBus.Unsubscribe<CardDiscardedEvent>(OnCardDiscarded);
            EventBus.Unsubscribe<CombatPhaseChangedEvent>(OnPhaseChanged);
        }

        // ============================================
        // Event Handlers
        // ============================================

        private void OnAPChanged(APChangedEvent evt)
        {
            if (_apText != null)
            {
                _apText.text = $"{evt.CurrentAP}/{evt.MaxAP}";
            }
        }

        private void OnTeamHPChanged(TeamHPChangedEvent evt)
        {
            if (_hpSlider != null)
            {
                _hpSlider.value = evt.MaxHP > 0 ? (float)evt.CurrentHP / evt.MaxHP : 0f;
            }

            if (_hpText != null)
            {
                _hpText.text = $"{evt.CurrentHP}/{evt.MaxHP}";
            }
        }

        private void OnBlockChanged(BlockChangedEvent evt)
        {
            if (_blockText != null)
            {
                _blockText.text = evt.Block > 0 ? evt.Block.ToString() : "";
                _blockText.gameObject.SetActive(evt.Block > 0);
            }
        }

        private void OnTurnStarted(TurnStartedEvent evt)
        {
            if (_turnText != null)
            {
                _turnText.text = evt.IsPlayerTurn ? $"Turn {evt.TurnNumber}" : "Enemy Turn";
            }

            if (_endTurnButton != null)
            {
                _endTurnButton.interactable = evt.IsPlayerTurn;
            }
        }

        private void OnPhaseChanged(CombatPhaseChangedEvent evt)
        {
            if (_phaseText != null)
            {
                _phaseText.text = evt.NewPhase.ToString();
            }
        }

        private void OnCardDrawn(CardDrawnEvent evt)
        {
            UpdateDeckCounts();
        }

        private void OnCardDiscarded(CardDiscardedEvent evt)
        {
            UpdateDeckCounts();
        }

        // ============================================
        // UI Updates
        // ============================================

        private void UpdateDeckCounts()
        {
            if (ServiceLocator.TryGet<DeckManager>(out var deckManager))
            {
                if (_drawPileText != null)
                {
                    _drawPileText.text = deckManager.DrawPileCount.ToString();
                }

                if (_discardPileText != null)
                {
                    _discardPileText.text = deckManager.DiscardPileCount.ToString();
                }
            }
        }

        /// <summary>
        /// Initialize display with current combat context.
        /// </summary>
        public void InitializeFromContext(CombatContext context)
        {
            if (context == null) return;

            OnAPChanged(new APChangedEvent(context.CurrentAP, context.MaxAP));
            OnTeamHPChanged(new TeamHPChangedEvent(context.TeamHP, context.TeamMaxHP));
            OnBlockChanged(new BlockChangedEvent(context.TeamBlock, 0));
            UpdateDeckCounts();
        }

        // ============================================
        // Button Handlers
        // ============================================

        private void OnEndTurnClicked()
        {
            if (ServiceLocator.TryGet<TurnManager>(out var turnManager))
            {
                turnManager.EndPlayerTurn();
            }
        }

        // ============================================
        // ScreenBase Overrides
        // ============================================

        public override void OnShow()
        {
            base.OnShow();
            UpdateDeckCounts();

            // Initialize from current context if available
            if (ServiceLocator.TryGet<TurnManager>(out var turnManager))
            {
                InitializeFromContext(turnManager.Context);
            }
        }
    }
}
