// ============================================
// HandManager.cs
// Displays and manages cards in hand
// ============================================

using System.Collections.Generic;
using UnityEngine;
using HNR.Core;
using HNR.Core.Events;
using HNR.Core.Interfaces;
using HNR.Cards;
using DG.Tweening;

namespace HNR.Combat
{
    /// <summary>
    /// Displays cards in hand with arc layout.
    /// Manages card selection and hover states.
    /// </summary>
    public class HandManager : MonoBehaviour
    {
        // ============================================
        // Layout Settings
        // ============================================

        [Header("Layout")]
        [SerializeField, Tooltip("Parent transform for card instances")]
        private Transform _handContainer;

        [SerializeField, Tooltip("Base spacing between cards")]
        private float _cardSpacing = 120f;

        [SerializeField, Tooltip("Height of arc curve")]
        private float _arcHeight = 40f;

        [SerializeField, Tooltip("Rotation angle at edges")]
        private float _arcAngle = 5f;

        [SerializeField, Tooltip("Maximum width before cards compress")]
        private float _maxHandWidth = 800f;

        // ============================================
        // Animation Settings
        // ============================================

        [Header("Animation")]
        [SerializeField, Tooltip("Duration of draw animation")]
        private float _drawDuration = 0.3f;

        [SerializeField, Tooltip("Duration of reposition animation")]
        private float _repositionDuration = 0.2f;

        [SerializeField, Tooltip("Position where drawn cards originate")]
        private Transform _drawSource;

        // ============================================
        // Selection Settings
        // ============================================

        [Header("Selection")]
        [SerializeField, Tooltip("Y offset when card is selected")]
        private float _selectedYOffset = 60f;

        [SerializeField, Tooltip("Scale multiplier on hover")]
        private float _hoverScale = 1.15f;

        // ============================================
        // References
        // ============================================

        [Header("References")]
        [SerializeField, Tooltip("Card prefab for instantiation")]
        private Card _cardPrefab;

        // ============================================
        // Runtime State
        // ============================================

        private List<Card> _hand = new();
        private Card _selectedCard;
        private Card _hoveredCard;

        // ============================================
        // Properties
        // ============================================

        /// <summary>Number of cards currently in hand.</summary>
        public int CardCount => _hand.Count;

        /// <summary>Currently selected card (null if none).</summary>
        public Card SelectedCard => _selectedCard;

        /// <summary>Read-only access to cards in hand.</summary>
        public IReadOnlyList<Card> Hand => _hand;

        // ============================================
        // Unity Lifecycle
        // ============================================

        private void Awake()
        {
            ServiceLocator.Register(this);
        }

        private void OnDestroy()
        {
            ServiceLocator.Unregister<HandManager>();
        }

        // ============================================
        // Card Management
        // ============================================

        /// <summary>
        /// Add a card to the hand with draw animation.
        /// </summary>
        /// <param name="cardInstance">Card instance to display</param>
        public void AddCard(CardInstance cardInstance)
        {
            var poolManager = ServiceLocator.TryGet<IPoolManager>(out var pm) ? pm : null;
            var card = poolManager?.Get<Card>() ?? Instantiate(_cardPrefab);

            if (_handContainer != null)
            {
                card.transform.SetParent(_handContainer, false);
            }

            card.Initialize(cardInstance);
            card.OnCardClicked += HandleCardClicked;
            card.OnCardHoverEnter += HandleCardHoverEnter;
            card.OnCardHoverExit += HandleCardHoverExit;

            _hand.Add(card);
            AnimateDrawCard(card);
        }

        /// <summary>
        /// Remove a card from the hand.
        /// </summary>
        /// <param name="card">Card to remove</param>
        public void RemoveCard(Card card)
        {
            if (card == null) return;

            if (_selectedCard == card) _selectedCard = null;
            if (_hoveredCard == card) _hoveredCard = null;

            card.OnCardClicked -= HandleCardClicked;
            card.OnCardHoverEnter -= HandleCardHoverEnter;
            card.OnCardHoverExit -= HandleCardHoverExit;
            _hand.Remove(card);

            if (ServiceLocator.TryGet<IPoolManager>(out var poolManager))
            {
                poolManager.Return(card);
            }
            else
            {
                Destroy(card.gameObject);
            }

            RepositionCards();
        }

        /// <summary>
        /// Remove all cards from hand.
        /// </summary>
        public void ClearHand()
        {
            foreach (var card in _hand.ToArray())
            {
                RemoveCard(card);
            }
        }

        /// <summary>
        /// Get all card instances currently in hand.
        /// </summary>
        public List<CardInstance> GetHandInstances()
        {
            var instances = new List<CardInstance>();
            foreach (var card in _hand)
            {
                instances.Add(card.CardInstance);
            }
            return instances;
        }

        // ============================================
        // Animation
        // ============================================

        private void AnimateDrawCard(Card card)
        {
            if (_drawSource != null)
            {
                card.transform.position = _drawSource.position;
            }

            card.transform.localScale = Vector3.one * 0.5f;
            card.transform.DOScale(Vector3.one, _drawDuration).SetEase(Ease.OutBack);
            RepositionCards();
        }

        private void RepositionCards()
        {
            int count = _hand.Count;
            if (count == 0) return;

            float actualSpacing = Mathf.Min(_cardSpacing, _maxHandWidth / count);
            float totalWidth = actualSpacing * (count - 1);
            float startX = -totalWidth / 2f;

            for (int i = 0; i < count; i++)
            {
                float t = count > 1 ? (float)i / (count - 1) : 0.5f;
                float x = startX + (actualSpacing * i);
                float y = Mathf.Sin(t * Mathf.PI) * _arcHeight;
                float rotation = Mathf.Lerp(_arcAngle, -_arcAngle, t);

                var card = _hand[i];
                var targetPos = new Vector3(x, y, 0);
                var targetRot = Quaternion.Euler(0, 0, rotation);

                card.transform.DOLocalMove(targetPos, _repositionDuration).SetEase(Ease.OutQuad);
                card.transform.DOLocalRotateQuaternion(targetRot, _repositionDuration);
                card.SetSortOrder(i);
            }
        }

        // ============================================
        // Card Play Flow
        // ============================================

        /// <summary>
        /// Handle a card play attempt. Routes to targeting or immediate play.
        /// </summary>
        /// <param name="card">Card attempting to be played</param>
        public void HandleCardPlayAttempt(Card card)
        {
            if (card?.CardInstance == null) return;

            // Check if we're in player phase
            if (!ServiceLocator.TryGet<TurnManager>(out var turnManager)) return;
            if (turnManager.CurrentPhase != CombatPhase.PlayerPhase)
            {
                Debug.Log("[HandManager] Cannot play card - not in player phase");
                return;
            }

            // Check if card is playable
            var instance = card.CardInstance;
            if (!instance.CanPlay(turnManager.Context.CurrentAP))
            {
                Debug.Log($"[HandManager] Cannot play card - not enough AP ({turnManager.Context.CurrentAP}/{instance.CurrentCost})");
                return;
            }

            var targetType = instance.Data.TargetType;

            // Cards that don't require targeting
            if (targetType == TargetType.None ||
                targetType == TargetType.Self ||
                targetType == TargetType.AllEnemies ||
                targetType == TargetType.AllAllies ||
                targetType == TargetType.Random)
            {
                // Play immediately
                turnManager.TryPlayCard(card, null);
                return;
            }

            // Cards that require targeting (SingleEnemy, SingleAlly)
            if (ServiceLocator.TryGet<TargetingSystem>(out var targetingSystem))
            {
                targetingSystem.BeginTargeting(instance);
                _pendingCard = card;
                SubscribeToTargeting();
                Debug.Log($"[HandManager] Started targeting for {instance.Data.CardName}");
            }
        }

        private Card _pendingCard;

        private void SubscribeToTargeting()
        {
            EventBus.Subscribe<CardTargetConfirmedEvent>(OnTargetConfirmed);
            EventBus.Subscribe<CardTargetCancelledEvent>(OnTargetCancelled);
        }

        private void UnsubscribeFromTargeting()
        {
            EventBus.Unsubscribe<CardTargetConfirmedEvent>(OnTargetConfirmed);
            EventBus.Unsubscribe<CardTargetCancelledEvent>(OnTargetCancelled);
        }

        private void OnTargetConfirmed(CardTargetConfirmedEvent evt)
        {
            UnsubscribeFromTargeting();

            if (_pendingCard != null && ServiceLocator.TryGet<TurnManager>(out var turnManager))
            {
                turnManager.TryPlayCard(_pendingCard, evt.Target);
            }

            _pendingCard = null;
        }

        private void OnTargetCancelled(CardTargetCancelledEvent evt)
        {
            UnsubscribeFromTargeting();
            _pendingCard = null;
            Debug.Log("[HandManager] Targeting cancelled");
        }

        // ============================================
        // Selection
        // ============================================

        private void HandleCardClicked(Card card)
        {
            // Try to play the card
            HandleCardPlayAttempt(card);
        }

        private void SelectCard(Card card)
        {
            if (_selectedCard != null) DeselectCard();

            _selectedCard = card;
            card.SetSelected(true);
            card.transform.DOLocalMoveY(card.transform.localPosition.y + _selectedYOffset, 0.15f);
        }

        /// <summary>
        /// Deselect the currently selected card.
        /// </summary>
        public void DeselectCard()
        {
            if (_selectedCard == null) return;

            _selectedCard.SetSelected(false);
            _selectedCard = null;
            RepositionCards();
        }

        // ============================================
        // Hover
        // ============================================

        private void HandleCardHoverEnter(Card card)
        {
            if (_hoveredCard != null) return;

            _hoveredCard = card;
            card.transform.DOScale(Vector3.one * _hoverScale, 0.1f);
            card.SetSortOrder(100);
        }

        private void HandleCardHoverExit(Card card)
        {
            if (_hoveredCard != card) return;

            _hoveredCard = null;
            card.transform.DOScale(Vector3.one, 0.1f);
            RepositionCards();
        }
    }
}
