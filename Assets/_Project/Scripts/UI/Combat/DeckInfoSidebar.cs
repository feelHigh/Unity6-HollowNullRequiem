// ============================================
// DeckInfoSidebar.cs
// Displays draw pile and discard pile counts during combat
// ============================================

using UnityEngine;
using TMPro;
using HNR.Core;
using HNR.Core.Events;
using HNR.Combat;

namespace HNR.UI.Combat
{
    /// <summary>
    /// Displays the current draw pile and discard pile counts.
    /// Updates in real-time as cards are drawn, played, and discarded.
    /// </summary>
    public class DeckInfoSidebar : MonoBehaviour
    {
        // ============================================
        // UI References
        // ============================================

        [Header("Draw Pile")]
        [SerializeField, Tooltip("Draw pile count text")]
        private TMP_Text _drawPileText;

        [SerializeField, Tooltip("Draw pile icon/label")]
        private TMP_Text _drawPileLabel;

        [Header("Discard Pile")]
        [SerializeField, Tooltip("Discard pile count text")]
        private TMP_Text _discardPileText;

        [SerializeField, Tooltip("Discard pile icon/label")]
        private TMP_Text _discardPileLabel;

        // ============================================
        // State
        // ============================================

        private CombatManager _combatManager;
        private bool _isSubscribed;

        // ============================================
        // Unity Lifecycle
        // ============================================

        private void OnEnable()
        {
            SubscribeToEvents();
            UpdateDisplay();
        }

        private void OnDisable()
        {
            UnsubscribeFromEvents();
        }

        private void Start()
        {
            // Try to get CombatManager reference
            _combatManager = CombatManager.Instance;
            UpdateDisplay();
        }

        // ============================================
        // Event Subscriptions
        // ============================================

        private void SubscribeToEvents()
        {
            if (_isSubscribed) return;

            EventBus.Subscribe<CardDrawnEvent>(OnDeckChanged);
            EventBus.Subscribe<CardDiscardedEvent>(OnDeckChanged);
            EventBus.Subscribe<CardPlayedEvent>(OnDeckChanged);
            EventBus.Subscribe<TurnStartedEvent>(OnTurnStarted);
            EventBus.Subscribe<CombatStartedEvent>(OnCombatStarted);

            _isSubscribed = true;
        }

        private void UnsubscribeFromEvents()
        {
            if (!_isSubscribed) return;

            EventBus.Unsubscribe<CardDrawnEvent>(OnDeckChanged);
            EventBus.Unsubscribe<CardDiscardedEvent>(OnDeckChanged);
            EventBus.Unsubscribe<CardPlayedEvent>(OnDeckChanged);
            EventBus.Unsubscribe<TurnStartedEvent>(OnTurnStarted);
            EventBus.Unsubscribe<CombatStartedEvent>(OnCombatStarted);

            _isSubscribed = false;
        }

        // ============================================
        // Event Handlers
        // ============================================

        private void OnDeckChanged(GameEvent evt)
        {
            UpdateDisplay();
        }

        private void OnTurnStarted(TurnStartedEvent evt)
        {
            UpdateDisplay();
        }

        private void OnCombatStarted(CombatStartedEvent evt)
        {
            // Refresh CombatManager reference on combat start
            _combatManager = CombatManager.Instance;
            UpdateDisplay();
        }

        // ============================================
        // Display Update
        // ============================================

        /// <summary>
        /// Updates the draw and discard pile count displays.
        /// </summary>
        public void UpdateDisplay()
        {
            // Try to get CombatManager if not already available
            if (_combatManager == null)
            {
                _combatManager = CombatManager.Instance;
            }

            if (_combatManager == null)
            {
                // Combat not active - show zeros or hide
                SetDrawPileCount(0);
                SetDiscardPileCount(0);
                return;
            }

            SetDrawPileCount(_combatManager.DrawPileCount);
            SetDiscardPileCount(_combatManager.DiscardPileCount);
        }

        private void SetDrawPileCount(int count)
        {
            if (_drawPileText != null)
            {
                _drawPileText.text = count.ToString();
            }
        }

        private void SetDiscardPileCount(int count)
        {
            if (_discardPileText != null)
            {
                _discardPileText.text = count.ToString();
            }
        }

        // ============================================
        // Public API
        // ============================================

        /// <summary>
        /// Forces a refresh of the display.
        /// </summary>
        public void Refresh()
        {
            UpdateDisplay();
        }

        /// <summary>
        /// Sets the label text for the draw pile.
        /// </summary>
        public void SetDrawPileLabel(string label)
        {
            if (_drawPileLabel != null)
            {
                _drawPileLabel.text = label;
            }
        }

        /// <summary>
        /// Sets the label text for the discard pile.
        /// </summary>
        public void SetDiscardPileLabel(string label)
        {
            if (_discardPileLabel != null)
            {
                _discardPileLabel.text = label;
            }
        }
    }
}
