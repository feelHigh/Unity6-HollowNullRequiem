// ============================================
// HandLayoutManager.cs
// Arc-based card hand layout system
// ============================================

using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using HNR.Cards;

namespace HNR.UI
{
    /// <summary>
    /// Manages arc/fan layout for cards in hand.
    /// Do NOT use Unity's HorizontalLayoutGroup - this provides custom arc positioning.
    /// </summary>
    public class HandLayoutManager : MonoBehaviour
    {
        // ============================================
        // Arc Layout Settings
        // ============================================

        [Header("Arc Configuration")]
        [SerializeField, Tooltip("Container for card transforms")]
        private RectTransform _handContainer;

        [SerializeField, Tooltip("Radius of the arc curve")]
        private float _arcRadius = 800f;

        [SerializeField, Tooltip("Y offset for arc center (below screen)")]
        private float _arcCenterYOffset = -200f;

        [SerializeField, Tooltip("Maximum rotation for edge cards")]
        private float _maxRotation = 15f;

        [SerializeField, Tooltip("Maximum arc spread angle")]
        private float _maxArcAngle = 60f;

        // ============================================
        // Animation Settings
        // ============================================

        [Header("Animation")]
        [SerializeField, Tooltip("Duration for layout reorganization")]
        private float _reorganizeDuration = 0.2f;

        [SerializeField, Tooltip("Easing for reorganization")]
        private Ease _reorganizeEase = Ease.OutQuad;

        // ============================================
        // Scale Settings
        // ============================================

        [Header("Scale by Card Count")]
        [SerializeField, Tooltip("Scale for 1-5 cards")]
        private float _scaleSmall = 1.0f;

        [SerializeField, Tooltip("Scale for 6-8 cards")]
        private float _scaleMedium = 0.9f;

        [SerializeField, Tooltip("Scale for 9-10 cards")]
        private float _scaleLarge = 0.8f;

        [SerializeField, Tooltip("Maximum cards in hand")]
        private int _maxCards = 10;

        // ============================================
        // Hover Settings
        // ============================================

        [Header("Hover Spread")]
        [SerializeField, Tooltip("Extra space around hovered card")]
        private float _hoverSpreadOffset = 30f;

        // ============================================
        // Runtime State
        // ============================================

        private List<Card> _cards = new();
        private Card _hoveredCard;

        // ============================================
        // Properties
        // ============================================

        /// <summary>Number of cards currently in hand.</summary>
        public int CardCount => _cards.Count;

        /// <summary>Whether hand is at maximum capacity.</summary>
        public bool IsFull => _cards.Count >= _maxCards;

        /// <summary>Read-only access to cards in hand.</summary>
        public IReadOnlyList<Card> Cards => _cards;

        // ============================================
        // Public Methods - Card Management
        // ============================================

        /// <summary>
        /// Add a card to the hand.
        /// </summary>
        /// <param name="card">Card to add.</param>
        /// <returns>True if added, false if hand is full.</returns>
        public bool AddCard(Card card)
        {
            if (IsFull)
            {
                Debug.LogWarning("[HandLayoutManager] Hand is full, cannot add card");
                return false;
            }

            _cards.Add(card);
            card.transform.SetParent(_handContainer, false);
            RefreshLayout(true);
            return true;
        }

        /// <summary>
        /// Remove a card from the hand.
        /// </summary>
        /// <param name="card">Card to remove.</param>
        public void RemoveCard(Card card)
        {
            if (_cards.Remove(card))
            {
                if (_hoveredCard == card)
                    _hoveredCard = null;

                RefreshLayout(true);
            }
        }

        /// <summary>
        /// Remove all cards from hand.
        /// </summary>
        public void ClearHand()
        {
            _cards.Clear();
            _hoveredCard = null;
        }

        /// <summary>
        /// Set the currently hovered card for spread effect.
        /// </summary>
        /// <param name="card">Hovered card, or null if none.</param>
        public void SetHoveredCard(Card card)
        {
            if (_hoveredCard == card) return;

            _hoveredCard = card;
            RefreshLayout(true);
        }

        // ============================================
        // Layout Calculation
        // ============================================

        /// <summary>
        /// Recalculate and apply card positions.
        /// </summary>
        /// <param name="animate">Whether to animate the transition.</param>
        public void RefreshLayout(bool animate = true)
        {
            int count = _cards.Count;
            if (count == 0) return;

            float scale = CalculateCardScale(count);

            for (int i = 0; i < count; i++)
            {
                var card = _cards[i];
                var animator = card.GetComponent<CardAnimator>();

                var position = CalculateCardPosition(i, count);
                var rotation = CalculateCardRotation(i, count);

                // Apply spread offset if there's a hovered card
                if (_hoveredCard != null && card != _hoveredCard)
                {
                    int hoveredIndex = _cards.IndexOf(_hoveredCard);
                    if (hoveredIndex >= 0)
                    {
                        if (i < hoveredIndex)
                            position.x -= _hoverSpreadOffset;
                        else if (i > hoveredIndex)
                            position.x += _hoverSpreadOffset;
                    }
                }

                if (animate)
                {
                    card.transform.DOScale(scale, _reorganizeDuration).SetEase(_reorganizeEase);
                    ((RectTransform)card.transform).DOAnchorPos(position, _reorganizeDuration).SetEase(_reorganizeEase);
                    card.transform.DOLocalRotate(new Vector3(0, 0, rotation), _reorganizeDuration).SetEase(_reorganizeEase);
                }
                else
                {
                    card.transform.localScale = Vector3.one * scale;
                    ((RectTransform)card.transform).anchoredPosition = position;
                    card.transform.localRotation = Quaternion.Euler(0, 0, rotation);
                }

                // Update sorting order and animator's original position
                if (animator != null)
                {
                    animator.SetOriginalSortingOrder(i);
                    animator.UpdateOriginalPosition(position);
                }

                // Sibling index for proper layering
                card.transform.SetSiblingIndex(i);
            }

            // Ensure hovered card is on top
            if (_hoveredCard != null)
            {
                _hoveredCard.transform.SetAsLastSibling();
            }
        }

        /// <summary>
        /// Calculate card position on the arc.
        /// </summary>
        private Vector3 CalculateCardPosition(int index, int totalCards)
        {
            if (totalCards == 1)
                return new Vector3(0, _arcCenterYOffset, 0);

            // Normalize position along arc (0 to 1)
            float t = (float)index / (totalCards - 1);

            // Map to angle range (-maxArcAngle/2 to +maxArcAngle/2)
            float angle = Mathf.Lerp(-_maxArcAngle / 2f, _maxArcAngle / 2f, t);
            float radians = angle * Mathf.Deg2Rad;

            // Calculate position on arc
            // Arc center is at (0, -arcRadius + arcCenterYOffset)
            float x = Mathf.Sin(radians) * _arcRadius;
            float y = (Mathf.Cos(radians) - 1f) * _arcRadius + _arcCenterYOffset;

            return new Vector3(x, y, 0);
        }

        /// <summary>
        /// Calculate card rotation based on arc position.
        /// </summary>
        private float CalculateCardRotation(int index, int totalCards)
        {
            if (totalCards == 1) return 0f;

            // Normalize position (0 to 1)
            float t = (float)index / (totalCards - 1);

            // Left cards rotate positive, right cards rotate negative
            return Mathf.Lerp(_maxRotation, -_maxRotation, t);
        }

        /// <summary>
        /// Calculate card scale based on hand size.
        /// </summary>
        private float CalculateCardScale(int totalCards)
        {
            if (totalCards <= 5) return _scaleSmall;
            if (totalCards <= 8) return _scaleMedium;
            return _scaleLarge;
        }

        // ============================================
        // Utility Methods
        // ============================================

        /// <summary>
        /// Get card at specific index.
        /// </summary>
        /// <param name="index">Index in hand.</param>
        /// <returns>Card at index, or null if invalid.</returns>
        public Card GetCardAtIndex(int index)
        {
            return index >= 0 && index < _cards.Count ? _cards[index] : null;
        }

        /// <summary>
        /// Get index of a card in hand.
        /// </summary>
        /// <param name="card">Card to find.</param>
        /// <returns>Index, or -1 if not found.</returns>
        public int GetCardIndex(Card card)
        {
            return _cards.IndexOf(card);
        }

        /// <summary>
        /// Get the draw position (deck location) for draw animations.
        /// </summary>
        public Vector3 GetDrawPilePosition()
        {
            // Bottom right corner, off-screen
            return new Vector3(500f, -400f, 0f);
        }

        /// <summary>
        /// Get the discard position for discard animations.
        /// </summary>
        public Vector3 GetDiscardPilePosition()
        {
            // Bottom left corner, off-screen
            return new Vector3(-500f, -400f, 0f);
        }
    }
}
