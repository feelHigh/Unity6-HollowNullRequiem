// ============================================
// CardFanLayout.cs
// Curved card fan layout
// ============================================

using System;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using HNR.Core;
using HNR.Core.Interfaces;
using HNR.Cards;
using HNR.Combat;

#pragma warning disable CS0414 // Field is assigned but never used (reserved for future Inspector configuration)

namespace HNR.UI.Combat
{
    /// <summary>
    /// Displays cards in a curved fan layout emanating from bottom center.
    /// Overlapping card fan with hover lift and deal animations.
    /// </summary>
    /// <remarks>
    /// Works with CombatCard components for drag-to-play targeting.
    /// CardFanLayout handles visual positioning and animations.
    /// </remarks>
    public class CardFanLayout : MonoBehaviour
    {
        // ============================================
        // Fan Configuration
        // ============================================

        [Header("Fan Configuration")]
        [SerializeField, Tooltip("Total angle span of the fan (degrees)")]
        private float _fanAngle = 30f;

        [SerializeField, Tooltip("Radius of the arc curve (pixels)")]
        private float _fanRadius = 800f;

        [SerializeField, Tooltip("Overlap factor between cards (0-1)")]
        private float _cardOverlap = 0.6f;

        [SerializeField, Tooltip("Center point of the fan arc")]
        private Vector2 _fanCenter = new(0, -600f);

        // ============================================
        // Card Sizing
        // ============================================

        [Header("Card Sizing")]
        [SerializeField, Tooltip("Base card dimensions")]
        private Vector2 _cardSize = new(140, 190);

        [SerializeField, Tooltip("Y lift on hover (pixels)")]
        private float _hoverLiftY = 50f;

        [SerializeField, Tooltip("Scale multiplier on hover")]
        private float _hoverScale = 1.2f;

        // ============================================
        // Animation Settings
        // ============================================

        [Header("Animation")]
        [SerializeField, Tooltip("Duration of card deal animation")]
        private float _dealDuration = 0.3f;

        [SerializeField, Tooltip("Duration of card reposition animation")]
        private float _repositionDuration = 0.15f;

        [SerializeField, Tooltip("Duration of hover animation")]
        private float _hoverDuration = 0.1f;

        [SerializeField, Tooltip("Easing for deal animation")]
        private Ease _dealEase = Ease.OutBack;

        [SerializeField, Tooltip("Stagger delay between dealing multiple cards")]
        private float _dealStaggerDelay = 0.1f;

        // ============================================
        // References
        // ============================================

        [Header("References")]
        [SerializeField, Tooltip("Position where drawn cards originate")]
        private Transform _drawPilePosition;

        [SerializeField, Tooltip("Position where discarded cards go")]
        private Transform _discardPilePosition;

        // ============================================
        // Runtime State
        // ============================================

        private readonly List<CombatCard> _cards = new();
        private readonly Dictionary<CombatCard, Vector2> _cardBasePositions = new();
        private CombatCard _hoveredCard;
        private CombatCard _selectedCard;

        // ============================================
        // Properties
        // ============================================

        /// <summary>Number of cards currently displayed.</summary>
        public int CardCount => _cards.Count;

        /// <summary>Read-only access to displayed cards.</summary>
        public IReadOnlyList<CombatCard> Cards => _cards;

        /// <summary>Currently hovered card (null if none).</summary>
        public CombatCard HoveredCard => _hoveredCard;

        /// <summary>Currently selected card (null if none).</summary>
        public CombatCard SelectedCard => _selectedCard;

        // ============================================
        // Events
        // ============================================

        /// <summary>Fired when a card is played from the fan.</summary>
        public event Action<CombatCard> OnCardPlayed;

        /// <summary>Fired when a card is removed from the fan.</summary>
        public event Action<CombatCard> OnCardRemoved;

        // ============================================
        // Unity Lifecycle
        // ============================================

        private void Awake()
        {
            ServiceLocator.Register(this);
        }

        private void OnDestroy()
        {
            // Clean up any running tweens
            foreach (var card in _cards)
            {
                if (card != null)
                {
                    DOTween.Kill(card.transform);
                }
            }

            ServiceLocator.Unregister<CardFanLayout>();
        }

        // ============================================
        // Card Management
        // ============================================

        /// <summary>
        /// Add a card to the fan with deal animation.
        /// </summary>
        /// <param name="card">CombatCard to add</param>
        /// <param name="delay">Optional delay before dealing</param>
        public void AddCard(CombatCard card, float delay = 0f)
        {
            if (card == null || _cards.Contains(card)) return;

            _cards.Add(card);
            card.transform.SetParent(transform, false);

            // Ensure RectTransform is properly set after parenting
            var rect = card.RectTransform;
            if (rect == null)
            {
                rect = card.GetComponent<RectTransform>();
            }

            // Ensure proper anchoring for centered positioning after parenting
            if (rect != null)
            {
                rect.anchorMin = new Vector2(0.5f, 0.5f);
                rect.anchorMax = new Vector2(0.5f, 0.5f);
                rect.pivot = new Vector2(0.5f, 0.5f);
            }

            // Subscribe to events
            card.OnHoverEnter += OnCardHoverEnter;
            card.OnHoverExit += OnCardHoverExit;
            card.OnSelected += OnCardSelected;
            card.OnDragComplete += OnCardDragComplete;

            // Set initial position at draw pile or off-screen
            if (_drawPilePosition != null)
            {
                card.transform.position = _drawPilePosition.position;
            }
            else if (rect != null)
            {
                // Fallback: start off-screen at bottom
                rect.anchoredPosition = new Vector2(0, -500f);
            }
            card.transform.localScale = Vector3.zero;

            // Animate deal after delay
            if (delay > 0f)
            {
                DOVirtual.DelayedCall(delay, () =>
                {
                    if (card != null && _cards.Contains(card))
                    {
                        // Skip the new card in LayoutCards - AnimateCardDeal will handle it
                        LayoutCards(card);
                        AnimateCardDeal(card);
                    }
                }).SetLink(gameObject);
            }
            else
            {
                // Skip the new card in LayoutCards - AnimateCardDeal will handle it
                LayoutCards(card);
                AnimateCardDeal(card);
            }

            Debug.Log($"[CardFanLayout] Added card, count: {_cards.Count}");
        }

        /// <summary>
        /// Add multiple cards with staggered deal animation.
        /// </summary>
        /// <param name="cards">CombatCards to add</param>
        public void AddCards(IEnumerable<CombatCard> cards)
        {
            float delay = 0f;
            foreach (var card in cards)
            {
                AddCard(card, delay);
                delay += _dealStaggerDelay;
            }
        }

        /// <summary>
        /// Remove a card from the fan with optional discard animation.
        /// </summary>
        /// <param name="card">CombatCard to remove</param>
        /// <param name="toDiscard">Whether to animate to discard pile</param>
        public void RemoveCard(CombatCard card, bool toDiscard = true)
        {
            if (card == null || !_cards.Contains(card)) return;

            // Unsubscribe from events
            card.OnHoverEnter -= OnCardHoverEnter;
            card.OnHoverExit -= OnCardHoverExit;
            card.OnSelected -= OnCardSelected;
            card.OnDragComplete -= OnCardDragComplete;

            _cards.Remove(card);
            _cardBasePositions.Remove(card);

            if (_hoveredCard == card) _hoveredCard = null;
            if (_selectedCard == card) _selectedCard = null;

            if (toDiscard && _discardPilePosition != null)
            {
                // Animate to discard pile
                var seq = DOTween.Sequence();
                seq.Append(card.transform.DOMove(_discardPilePosition.position, 0.25f).SetEase(Ease.InQuad));
                seq.Join(card.transform.DOScale(0.5f, 0.25f));
                seq.OnComplete(() =>
                {
                    ReturnCardToPool(card);
                    OnCardRemoved?.Invoke(card);
                });
                seq.SetLink(card.gameObject);
            }
            else
            {
                ReturnCardToPool(card);
                OnCardRemoved?.Invoke(card);
            }

            // Reposition remaining cards
            LayoutCards();

            Debug.Log($"[CardFanLayout] Removed card, count: {_cards.Count}");
        }

        /// <summary>
        /// Clear all cards from the hand with discard animation.
        /// </summary>
        public void ClearHand()
        {
            foreach (var card in _cards.ToArray())
            {
                RemoveCard(card, true);
            }
        }

        /// <summary>
        /// Clear all cards immediately without animation.
        /// </summary>
        public void ClearHandImmediate()
        {
            foreach (var card in _cards.ToArray())
            {
                card.OnHoverEnter -= OnCardHoverEnter;
                card.OnHoverExit -= OnCardHoverExit;
                card.OnSelected -= OnCardSelected;
                card.OnDragComplete -= OnCardDragComplete;
                DOTween.Kill(card.transform);
                ReturnCardToPool(card);
            }

            _cards.Clear();
            _cardBasePositions.Clear();
            _hoveredCard = null;
            _selectedCard = null;
        }

        // ============================================
        // Layout Calculation
        // ============================================

        /// <summary>
        /// Recalculate and animate all card positions.
        /// </summary>
        /// <param name="skipCard">Optional card to skip (e.g., card being dealt that AnimateCardDeal will handle)</param>
        public void LayoutCards(CombatCard skipCard = null)
        {
            int count = _cards.Count;
            if (count == 0) return;

            // Calculate angle step based on card count
            float angleStep = count > 1 ? _fanAngle / (count - 1) : 0;
            float startAngle = -_fanAngle / 2;

            for (int i = 0; i < count; i++)
            {
                var card = _cards[i];
                if (card == null) continue;

                // Skip hovered/selected/dragging cards (they have special positioning)
                // Also skip the card being dealt (AnimateCardDeal handles it)
                if (card == _hoveredCard || card == _selectedCard || card.IsDragging || card == skipCard) continue;

                float angle = startAngle + (angleStep * i);
                var (position, rotation) = CalculateCardTransform(angle);

                // Store base position for hover calculations
                _cardBasePositions[card] = position;

                // Animate to position
                var rect = card.RectTransform;
                if (rect != null)
                {
                    // Kill existing tweens to prevent conflicts
                    DOTween.Kill(rect);
                    DOTween.Kill(card.transform);

                    rect.DOAnchorPos(position, _repositionDuration).SetEase(Ease.OutQuad).SetLink(card.gameObject);
                    rect.DOLocalRotate(new Vector3(0, 0, rotation), _repositionDuration).SetEase(Ease.OutQuad).SetLink(card.gameObject);
                    card.transform.DOScale(1f, _repositionDuration).SetLink(card.gameObject);
                }

                // Set sibling index for proper layering (left cards behind right)
                card.transform.SetSiblingIndex(i);
            }
        }

        /// <summary>
        /// Calculate card position and rotation for a given angle.
        /// </summary>
        /// <param name="angle">Angle in degrees from center</param>
        /// <returns>Tuple of (position, rotation)</returns>
        private (Vector2 position, float rotation) CalculateCardTransform(float angle)
        {
            // Convert angle to radians
            float radians = angle * Mathf.Deg2Rad;

            // Calculate position on arc
            float x = Mathf.Sin(radians) * _fanRadius;
            float y = (Mathf.Cos(radians) * _fanRadius) - _fanRadius + _fanCenter.y;

            // Rotation is proportional to angle (cards tilt toward center)
            float rotation = -angle * 0.5f;

            return (new Vector2(x + _fanCenter.x, y), rotation);
        }

        // ============================================
        // Animation
        // ============================================

        private void AnimateCardDeal(CombatCard card)
        {
            int index = _cards.IndexOf(card);
            if (index < 0) return;

            int count = _cards.Count;

            float angleStep = count > 1 ? _fanAngle / (count - 1) : 0;
            float angle = -_fanAngle / 2 + (angleStep * index);

            var (position, rotation) = CalculateCardTransform(angle);
            _cardBasePositions[card] = position;

            var rect = card.RectTransform;
            if (rect == null)
            {
                rect = card.GetComponent<RectTransform>();
            }

            if (rect != null)
            {
                // Kill any existing tweens on this card
                DOTween.Kill(rect);
                DOTween.Kill(card.transform);

                // Use magnitude check for more robust scale detection (handles floating-point issues)
                bool isNewCard = card.transform.localScale.magnitude < 0.1f;

                if (isNewCard)
                {
                    // Bring newly dealt card to front during deal animation so it's always visible
                    card.transform.SetAsLastSibling();

                    // Starting fresh from pool - animate position, rotation, and scale
                    rect.DOAnchorPos(position, _dealDuration).SetEase(_dealEase).SetLink(card.gameObject);
                    rect.DOLocalRotate(new Vector3(0, 0, rotation), _dealDuration).SetEase(_dealEase).SetLink(card.gameObject);
                    card.transform.DOScale(1f, _dealDuration).SetEase(_dealEase)
                        .OnComplete(() =>
                        {
                            // After deal animation completes, set correct sibling index for fan layering
                            int finalIndex = _cards.IndexOf(card);
                            if (finalIndex >= 0)
                            {
                                card.transform.SetSiblingIndex(finalIndex);
                            }
                        })
                        .SetLink(card.gameObject);
                }
                else
                {
                    // Already visible, just reposition - but ensure scale is correct
                    rect.DOAnchorPos(position, _repositionDuration).SetEase(Ease.OutQuad).SetLink(card.gameObject);
                    rect.DOLocalRotate(new Vector3(0, 0, rotation), _repositionDuration).SetEase(Ease.OutQuad).SetLink(card.gameObject);
                    // Ensure scale is 1 in case of any issues
                    if (card.transform.localScale.magnitude < 0.9f)
                    {
                        card.transform.DOScale(1f, _repositionDuration).SetEase(Ease.OutQuad).SetLink(card.gameObject);
                    }
                    card.transform.SetSiblingIndex(index);
                }
            }
            else
            {
                // Fallback: set position directly if RectTransform unavailable
                card.transform.localPosition = new Vector3(position.x, position.y, 0);
                card.transform.localScale = Vector3.one;
                card.transform.SetSiblingIndex(index);
                Debug.LogWarning($"[CardFanLayout] RectTransform null for card at index {index}, setting position directly");
            }
        }

        // ============================================
        // Hover & Selection
        // ============================================

        private void OnCardHoverEnter(CombatCard card)
        {
            if (_selectedCard != null) return;
            if (_hoveredCard != null) return;

            _hoveredCard = card;

            // Lift and scale the hovered card
            var rect = card.RectTransform;
            if (rect != null && _cardBasePositions.TryGetValue(card, out var basePos))
            {
                var hoverPos = new Vector2(basePos.x, basePos.y + _hoverLiftY);
                rect.DOAnchorPos(hoverPos, _hoverDuration).SetLink(card.gameObject);
                rect.DOLocalRotate(Vector3.zero, _hoverDuration).SetLink(card.gameObject);
                card.transform.DOScale(_hoverScale, _hoverDuration).SetLink(card.gameObject);
            }

            // Bring to front
            card.transform.SetAsLastSibling();

            // Push other cards aside slightly
            LayoutCards();
        }

        private void OnCardHoverExit(CombatCard card)
        {
            if (_selectedCard != null) return;
            if (_hoveredCard != card) return;

            _hoveredCard = null;

            // Return to base position via layout
            LayoutCards();
        }

        private void OnCardSelected(CombatCard card)
        {
            if (_selectedCard == card)
            {
                // Deselect on second click
                DeselectCard();
            }
            else
            {
                SelectCard(card);
            }
        }

        private void OnCardDragComplete(CombatCard card, ICombatTarget target)
        {
            if (target != null)
            {
                // Card was played successfully
                OnCardPlayed?.Invoke(card);
            }
            else
            {
                // Card returned to hand
                DeselectCard();
            }
        }

        /// <summary>
        /// Select a card for playing.
        /// </summary>
        /// <param name="card">CombatCard to select</param>
        public void SelectCard(CombatCard card)
        {
            if (card == null || !_cards.Contains(card)) return;

            // Deselect previous
            if (_selectedCard != null)
            {
                _selectedCard.SetSelected(false);
            }

            _selectedCard = card;
            card.SetSelected(true);

            Debug.Log($"[CardFanLayout] Selected card: {card.CardData?.Data?.CardName}");
        }

        /// <summary>
        /// Deselect the currently selected card.
        /// </summary>
        public void DeselectCard()
        {
            if (_selectedCard == null) return;

            _selectedCard.SetSelected(false);
            _selectedCard = null;
            _hoveredCard = null;

            LayoutCards();

            Debug.Log("[CardFanLayout] Deselected card");
        }

        // ============================================
        // Card Play
        // ============================================

        /// <summary>
        /// Signal that a card play has completed.
        /// Called by external systems after card effect executes.
        /// </summary>
        /// <param name="card">CombatCard that was played</param>
        public void OnCardPlayComplete(CombatCard card)
        {
            _selectedCard = null;
            _hoveredCard = null;
            OnCardPlayed?.Invoke(card);
        }

        // ============================================
        // Pooling
        // ============================================

        private void ReturnCardToPool(CombatCard card)
        {
            if (card == null) return;

            DOTween.Kill(card.transform);

            if (ServiceLocator.TryGet<IPoolManager>(out var poolManager))
            {
                poolManager.Return(card);
            }
            else
            {
                Destroy(card.gameObject);
            }
        }

        // ============================================
        // Utility
        // ============================================

        /// <summary>
        /// Get card at a specific index.
        /// </summary>
        /// <param name="index">Index in the fan</param>
        /// <returns>CombatCard at index or null</returns>
        public CombatCard GetCardAt(int index)
        {
            if (index < 0 || index >= _cards.Count) return null;
            return _cards[index];
        }

        /// <summary>
        /// Get index of a card in the fan.
        /// </summary>
        /// <param name="card">CombatCard to find</param>
        /// <returns>Index or -1 if not found</returns>
        public int GetCardIndex(CombatCard card)
        {
            return _cards.IndexOf(card);
        }

        /// <summary>
        /// Force immediate layout without animation.
        /// </summary>
        public void LayoutImmediate()
        {
            int count = _cards.Count;
            if (count == 0) return;

            float angleStep = count > 1 ? _fanAngle / (count - 1) : 0;
            float startAngle = -_fanAngle / 2;

            for (int i = 0; i < count; i++)
            {
                var card = _cards[i];
                float angle = startAngle + (angleStep * i);
                var (position, rotation) = CalculateCardTransform(angle);

                _cardBasePositions[card] = position;

                var rect = card.RectTransform;
                if (rect != null)
                {
                    rect.anchoredPosition = position;
                    rect.localRotation = Quaternion.Euler(0, 0, rotation);
                }

                card.transform.localScale = Vector3.one;
                card.transform.SetSiblingIndex(i);
            }
        }

        /// <summary>
        /// Update playability state for all cards based on available AP.
        /// </summary>
        /// <param name="availableAP">Current available Action Points</param>
        public void UpdateAllPlayability(int availableAP)
        {
            foreach (var card in _cards)
            {
                card.UpdatePlayability(availableAP);
            }
        }
    }
}
